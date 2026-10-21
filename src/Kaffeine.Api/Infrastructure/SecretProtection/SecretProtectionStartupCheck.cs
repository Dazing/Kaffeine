namespace Kaffeine.Api.Infrastructure.SecretProtection;

internal sealed class SecretProtectionStartupCheck(ISecretProtector protector) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var context = new SecretProtectionContext("system", "startup", "key-ring-check");
        const string probe = "Kaffeine key ring check";
        var protectedValue = protector.Protect(probe, context);
        if (protector.Unprotect(protectedValue, context) != probe)
            throw new InvalidOperationException("Secret protection startup check failed.");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
