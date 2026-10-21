using Microsoft.AspNetCore.DataProtection;

namespace Kaffeine.Api.Infrastructure.SecretProtection;

/// <summary>Configures certificate-protected, persistent encryption keys for stored secrets.</summary>
public static class SecretProtectionRegistration
{
    /// <summary>
    /// Registers secret protection using external PEM certificate/private-key files and a key directory.
    /// Paths resolve against the content root. Absent configuration permits shell startup but rejects
    /// secret-protector resolution; partial configuration fails immediately. Configured hosts check
    /// protection on startup. The key directory must already exist with restricted permissions.
    /// </summary>
    public static IServiceCollection AddSecretProtection(
        this IServiceCollection services,
        IConfiguration configuration,
        string contentRootPath
    )
    {
        var section = configuration.GetSection("SecretProtection");
        if (!section.Exists())
        {
            services.AddSingleton<ISecretProtector>(_ =>
                throw new InvalidOperationException(
                    "Secret protection is not configured. Set SecretProtection:KeyRingPath, CertificatePath, and PrivateKeyPath before storing secrets."
                )
            );
            return services;
        }

        string RequiredPath(string key)
        {
            var value = section[key];
            if (string.IsNullOrWhiteSpace(value))
                throw new InvalidOperationException($"SecretProtection:{key} is required.");
            return Path.GetFullPath(value, contentRootPath);
        }

        var keyRingPath = RequiredPath("KeyRingPath");
        var certificatePath = RequiredPath("CertificatePath");
        var privateKeyPath = RequiredPath("PrivateKeyPath");
        if (!Directory.Exists(keyRingPath))
            throw new InvalidOperationException(
                "Provision SecretProtection:KeyRingPath as a private persistent directory before startup."
            );

        var certificate = new SecretProtectionCertificate(certificatePath, privateKeyPath);
        // The factory registration transfers disposal to the host when protection is resolved.
        services.AddSingleton(_ => certificate);
        services
            .AddDataProtection()
            .SetApplicationName("Kaffeine.StoredSecrets")
            .PersistKeysToFileSystem(new DirectoryInfo(keyRingPath))
            .ProtectKeysWithCertificate(certificate.Value);
        services.AddSingleton<ISecretProtector>(provider =>
        {
            _ = provider.GetRequiredService<SecretProtectionCertificate>();
            return new DataProtectionSecretProtector(
                provider.GetRequiredService<IDataProtectionProvider>()
            );
        });
        services.AddHostedService<SecretProtectionStartupCheck>();
        return services;
    }
}
