using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Kaffeine.Api.Persistence;

/// <summary>Configures application persistence without creating or modifying its schema.</summary>
public static class PersistenceRegistration
{
    /// <summary>
    /// Registers a scoped context. Relative SQLite paths resolve against the host content root.
    /// Invalid or non-durable database configuration is rejected during registration.
    /// </summary>
    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration,
        string contentRootPath
    )
    {
        if (
            !string.Equals(
                configuration["Persistence:Provider"],
                "Sqlite",
                StringComparison.OrdinalIgnoreCase
            )
        )
        {
            throw new InvalidOperationException(
                "Persistence:Provider must be Sqlite. No other database provider is supported."
            );
        }

        var connectionString = configuration.GetConnectionString("Kaffeine");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "ConnectionStrings:Kaffeine must specify a persistent SQLite database."
            );
        }

        SqliteConnectionStringBuilder sqlite;
        try
        {
            sqlite = new SqliteConnectionStringBuilder(connectionString);
        }
        catch (ArgumentException)
        {
            // Do not expose connection-string values through startup errors.
            throw new InvalidOperationException(
                "ConnectionStrings:Kaffeine is not a valid SQLite connection string."
            );
        }

        if (
            string.IsNullOrWhiteSpace(sqlite.DataSource)
            || sqlite.DataSource == ":memory:"
            || sqlite.Mode == SqliteOpenMode.Memory
            || sqlite.DataSource.StartsWith("file:", StringComparison.OrdinalIgnoreCase)
        )
        {
            throw new InvalidOperationException(
                "ConnectionStrings:Kaffeine must use a file path, not a temporary, in-memory, or URI database."
            );
        }

        sqlite.DataSource = Path.GetFullPath(sqlite.DataSource, contentRootPath);
        services.AddDbContext<KaffeineDbContext>(options =>
            options.UseSqlite(sqlite.ConnectionString)
        );
        return services;
    }
}
