using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Kaffeine.Api.Infrastructure.SecretProtection;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Kaffeine.Api.Tests.Infrastructure.SecretProtection;

public sealed class SecretProtectionTests : IDisposable
{
    private readonly string root = Path.Combine(
        Path.GetTempPath(),
        "kaffeine-secrets-" + Guid.NewGuid()
    );

    public SecretProtectionTests()
    {
        Directory.CreateDirectory(root);
        Directory.CreateDirectory(Path.Combine(root, "keys"));
        using var rsa = RSA.Create(2048);
        var request = new CertificateRequest(
            "CN=Kaffeine tests",
            rsa,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1
        );
        using var certificate = request.CreateSelfSigned(
            DateTimeOffset.UtcNow.AddDays(-1),
            DateTimeOffset.UtcNow.AddDays(1)
        );
        File.WriteAllText(
            Path.Combine(root, "certificate.pem"),
            certificate.ExportCertificatePem()
        );
        File.WriteAllText(Path.Combine(root, "private-key.pem"), rsa.ExportPkcs8PrivateKeyPem());
    }

    [Fact]
    public void CiphertextCannotBeMovedToAnotherOwnerResourceOrSecret()
    {
        using var services = CreateServices();
        var protector = services.GetRequiredService<ISecretProtector>();
        var context = new SecretProtectionContext("workspace-a", "connection-a", "sasl-password");
        var ciphertext = protector.Protect("sensitive-password", context);

        Assert.Equal("sensitive-password", protector.Unprotect(ciphertext, context));
        Assert.Throws<CryptographicException>(() =>
            protector.Unprotect(ciphertext, context with { OwnerId = "workspace-b" })
        );
        Assert.Throws<CryptographicException>(() =>
            protector.Unprotect(ciphertext, context with { ResourceId = "connection-b" })
        );
        Assert.Throws<CryptographicException>(() =>
            protector.Unprotect(ciphertext, context with { SecretName = "api-key" })
        );
    }

    [Fact]
    public void SecretsSurviveProviderRestartAndKeyRotation()
    {
        var context = new SecretProtectionContext("workspace", "connection", "password");
        string ciphertext;
        using (var services = CreateServices())
        {
            ciphertext = services
                .GetRequiredService<ISecretProtector>()
                .Protect("persisted secret ü", context);
            services
                .GetRequiredService<IKeyManager>()
                .CreateNewKey(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddDays(90));
        }

        using var restarted = CreateServices();
        var protector = restarted.GetRequiredService<ISecretProtector>();
        Assert.Equal("persisted secret ü", protector.Unprotect(ciphertext, context));
        var replacement = protector.Protect("replacement", context);
        Assert.Equal("replacement", protector.Unprotect(replacement, context));
    }

    [Fact]
    public void ModifiedTruncatedAndUnknownFormatValuesFailClosed()
    {
        using var services = CreateServices();
        var protector = services.GetRequiredService<ISecretProtector>();
        var context = new SecretProtectionContext("workspace", "connection", "password");
        var ciphertext = protector.Protect("secret", context);
        var bytes = WebEncoders.Base64UrlDecode(ciphertext[3..]);
        bytes[^1] ^= 1;
        Assert.Throws<CryptographicException>(() =>
            protector.Unprotect("v1:" + WebEncoders.Base64UrlEncode(bytes), context)
        );
        Assert.Throws<CryptographicException>(() =>
            protector.Unprotect(ciphertext[..^12], context)
        );
        Assert.Throws<CryptographicException>(() =>
            protector.Unprotect("v2:" + ciphertext[3..], context)
        );
        Assert.Throws<CryptographicException>(() => protector.Unprotect("plaintext", context));
    }

    [Fact]
    public void KeyRingWithoutPrivateCertificateCannotRecoverSecrets()
    {
        var context = new SecretProtectionContext("workspace", "connection", "password");
        string ciphertext;
        using (var services = CreateServices())
            ciphertext = services.GetRequiredService<ISecretProtector>().Protect("secret", context);

        // Simulate possession of the ciphertext and encrypted key-ring files, but no private key.
        var stolenServices = new ServiceCollection();
        stolenServices.AddLogging();
        stolenServices
            .AddDataProtection()
            .SetApplicationName("Kaffeine.StoredSecrets")
            .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(root, "keys")))
            .DisableAutomaticKeyGeneration();
        using var stolen = stolenServices.BuildServiceProvider();
        var protector = stolen
            .GetRequiredService<IDataProtectionProvider>()
            .CreateProtector(
                "Kaffeine.StoredSecrets",
                "v1",
                context.OwnerId,
                context.ResourceId,
                context.SecretName
            );
        Assert.Throws<CryptographicException>(() => protector.Unprotect(ciphertext[3..]));
    }

    [Fact]
    public void MissingOriginalKeyRingCannotRecoverSecrets()
    {
        var context = new SecretProtectionContext("workspace", "connection", "password");
        string ciphertext;
        using (var services = CreateServices())
            ciphertext = services.GetRequiredService<ISecretProtector>().Protect("secret", context);
        Directory.CreateDirectory(Path.Combine(root, "empty-keys"));
        using var replacement = CreateServices(
            new() { ["SecretProtection:KeyRingPath"] = "empty-keys" }
        );
        Assert.Throws<CryptographicException>(() =>
            replacement.GetRequiredService<ISecretProtector>().Unprotect(ciphertext, context)
        );
    }

    [Fact]
    public void MissingConfigurationCannotFallBackToDefaultProtection()
    {
        using var services = new ServiceCollection()
            .AddSecretProtection(new ConfigurationBuilder().Build(), root)
            .BuildServiceProvider();
        Assert.Throws<InvalidOperationException>(() =>
            services.GetRequiredService<ISecretProtector>()
        );
    }

    [Theory]
    [InlineData("SecretProtection:KeyRingPath")]
    [InlineData("SecretProtection:CertificatePath")]
    [InlineData("SecretProtection:PrivateKeyPath")]
    public void PartialConfigurationIsRejected(string key)
    {
        Assert.Throws<InvalidOperationException>(() => CreateServices(new() { [key] = "" }));
    }

    private ServiceProvider CreateServices(Dictionary<string, string?>? overrides = null)
    {
        var values = new Dictionary<string, string?>
        {
            ["SecretProtection:KeyRingPath"] = "keys",
            ["SecretProtection:CertificatePath"] = "certificate.pem",
            ["SecretProtection:PrivateKeyPath"] = "private-key.pem",
        };
        if (overrides is not null)
            foreach (var pair in overrides)
                values[pair.Key] = pair.Value;
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(values).Build();
        return new ServiceCollection()
            .AddLogging()
            .AddSecretProtection(configuration, root)
            .BuildServiceProvider();
    }

    public void Dispose() => Directory.Delete(root, recursive: true);
}
