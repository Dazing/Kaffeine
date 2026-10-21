namespace Kaffeine.Api.Infrastructure.SecretProtection;

/// <summary>
/// Protects recoverable secrets independently of persistence. Callers must authorize access,
/// supply trusted stable ownership identifiers, and never log plaintext or return it in read APIs.
/// </summary>
public interface ISecretProtector
{
    /// <summary>Returns versioned authenticated ciphertext bound to the supplied context.</summary>
    string Protect(string plaintext, SecretProtectionContext context);

    /// <summary>
    /// Recovers plaintext only for the original context and available, non-revoked keys.
    /// Invalid, tampered, or undecryptable ciphertext throws CryptographicException; no fallback is used.
    /// </summary>
    string Unprotect(string protectedValue, SecretProtectionContext context);
}

/// <summary>
/// Stable, non-secret identifiers, not display names. OwnerId is a user or workspace ID;
/// ResourceId identifies the owning record; SecretName distinguishes fields on that record.
/// Changing any component requires explicitly re-protecting the secret.
/// </summary>
public sealed record SecretProtectionContext(string OwnerId, string ResourceId, string SecretName);
