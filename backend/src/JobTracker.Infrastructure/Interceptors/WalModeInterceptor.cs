using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace JobTracker.Infrastructure.Interceptors;

internal sealed class WalModeInterceptor : DbConnectionInterceptor
{
    public override void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
        => SetWalMode(connection);

    public override Task ConnectionOpenedAsync(DbConnection connection, ConnectionEndEventData eventData, CancellationToken cancellationToken = default)
    {
        SetWalMode(connection);
        return Task.CompletedTask;
    }

    private static void SetWalMode(DbConnection connection)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "PRAGMA journal_mode=WAL;";
        cmd.ExecuteNonQuery();
    }
}
