using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Kaffeine.Api.Infrastructure.SecretProtection;

internal sealed class SecretProtectionCertificate : IDisposable
{
    public X509Certificate2 Value { get; }

    public SecretProtectionCertificate(string certificatePath, string privateKeyPath)
    {
        Value = X509Certificate2.CreateFromPemFile(certificatePath, privateKeyPath);
        using var rsa = Value.GetRSAPrivateKey();
        if (rsa is null || rsa.KeySize < 2048)
        {
            Value.Dispose();
            throw new CryptographicException(
                "Secret protection requires an RSA certificate with a private key of at least 2048 bits."
            );
        }
    }

    public void Dispose() => Value.Dispose();
}
