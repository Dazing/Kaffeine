using System.Security.Cryptography;
using Microsoft.AspNetCore.DataProtection;

namespace Kaffeine.Api.Infrastructure.SecretProtection;

internal sealed class DataProtectionSecretProtector(IDataProtectionProvider provider)
    : ISecretProtector
{
    private const string Prefix = "v1:";

    public string Protect(string plaintext, SecretProtectionContext context)
    {
        ArgumentNullException.ThrowIfNull(plaintext);
        return Prefix + CreateProtector(context).Protect(plaintext);
    }

    public string Unprotect(string protectedValue, SecretProtectionContext context)
    {
        ArgumentNullException.ThrowIfNull(protectedValue);
        var protector = CreateProtector(context);
        if (!protectedValue.StartsWith(Prefix, StringComparison.Ordinal))
        {
            throw new CryptographicException("The protected secret format is unsupported.");
        }

        return protector.Unprotect(protectedValue[Prefix.Length..]);
    }

    private IDataProtector CreateProtector(SecretProtectionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentException.ThrowIfNullOrWhiteSpace(context.OwnerId);
        ArgumentException.ThrowIfNullOrWhiteSpace(context.ResourceId);
        ArgumentException.ThrowIfNullOrWhiteSpace(context.SecretName);

        // Separate purpose segments prevent ambiguous concatenations of ownership identifiers.
        return provider.CreateProtector(
            "Kaffeine.StoredSecrets",
            "v1",
            context.OwnerId,
            context.ResourceId,
            context.SecretName
        );
    }
}
