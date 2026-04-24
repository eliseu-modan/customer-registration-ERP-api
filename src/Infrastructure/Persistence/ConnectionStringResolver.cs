using Microsoft.Extensions.Configuration;
using Npgsql;

namespace ERP.Infrastructure.Persistence;

public static class ConnectionStringResolver
{
    public static string Resolve(IConfiguration configuration)
    {
        var rawConnectionString =
            configuration.GetConnectionString("DefaultConnection") ??
            configuration["DATABASE_URL"] ??
            throw new InvalidOperationException("Connection string 'DefaultConnection' não foi configurada.");

        if (rawConnectionString.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) ||
            rawConnectionString.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
        {
            return ConvertFromUrl(rawConnectionString);
        }

        return rawConnectionString;
    }

    private static string ConvertFromUrl(string connectionUrl)
    {
        var uri = new Uri(connectionUrl);
        var userInfo = uri.UserInfo.Split(':', 2);
        var database = uri.AbsolutePath.Trim('/');
        var query = ParseQuery(uri.Query);

        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.Port > 0 ? uri.Port : 5432,
            Username = Uri.UnescapeDataString(userInfo[0]),
            Password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : string.Empty,
            Database = database
        };

        if (query.TryGetValue("sslmode", out var sslMode) &&
            Enum.TryParse<SslMode>(sslMode, true, out var parsedSslMode))
        {
            builder.SslMode = parsedSslMode;
        }

        return builder.ConnectionString;
    }

    private static Dictionary<string, string> ParseQuery(string queryString)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(queryString))
        {
            return result;
        }

        var query = queryString.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries);
        foreach (var part in query)
        {
            var pieces = part.Split('=', 2);
            var key = Uri.UnescapeDataString(pieces[0]);
            var value = pieces.Length > 1 ? Uri.UnescapeDataString(pieces[1]) : string.Empty;
            result[key] = value;
        }

        return result;
    }
}
