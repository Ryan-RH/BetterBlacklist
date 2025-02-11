using System.Data.SQLite;
using System.IO;

namespace BetterBlacklist.Database;

public static class Setup
{
    private static string DbDirectory = Path.Combine(Svc.PluginInterface.ConfigDirectory.FullName, "BblDatabase.db");

    public static void Init()
    {
        if (File.Exists(DbDirectory))
        {
            Svc.Log.Information("Database Exists");
        }
        else
        {
            SQLiteConnection.CreateFile(DbDirectory);
        }

        Task.Run(async () =>
        {
            try
            {
                await Configure().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Svc.Log.Information($"Failed to get DB connection: {ex}");
            }
        });
    }

    private static async Task Configure()
    {
        Connect.Connection = new SQLiteConnection("Data Source=" + DbDirectory + ";Version=3;");
        await Connect.Connection.OpenAsync();

        string players = @"
            CREATE TABLE IF NOT EXISTS Players (
                PlayerName TEXT NOT NULL,
                HomeWorld TEXT NOT NULL,
                State INTEGER NOT NULL,
                PRIMARY KEY (PlayerName, HomeWorld)
            );";

        using (var command = new SQLiteCommand(players, Connect.Connection))
        {
            await command.ExecuteNonQueryAsync();
        }

        string Duties = @"
            CREATE TABLE IF NOT EXISTS Duties (
                Duty TEXT NOT NULL,
                UnixTimestamp INTEGER NOT NULL,
                Players JSON NOT NULL
            );";

        using (var command = new SQLiteCommand(Duties, Connect.Connection))
        {
            await command.ExecuteNonQueryAsync();
        }
    }
}
