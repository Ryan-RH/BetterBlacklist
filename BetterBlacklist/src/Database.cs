using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Data.SQLite;

namespace BetterBlacklist;

internal static class Database
{
    private static string DbDirectory = Svc.PluginInterface.ConfigDirectory.FullName + "\\BblDatabase.db";

    public static void Init()
    {
        if (CheckExist())
        {
            PluginLog.Information("Databases Exist.");
        }
        else
        {
            CreateDBs();
        }
    }

    private static bool CheckExist()
    {
        if (File.Exists(DbDirectory))
            return true;
        return false;
    }

    private static void CreateDBs()
    {
        SQLiteConnection.CreateFile(DbDirectory);

        using (var connection = new SQLiteConnection("Data Source=" + DbDirectory + ";Version=3;"))
        {
            connection.Open();

            // Create History table
            string createPlayerHistoryTableQuery = @"
            CREATE TABLE IF NOT EXISTS History (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                PlayerName TEXT NOT NULL,
                DutyFirstMet TEXT NOT NULL,
                UnixTimestamp INTEGER NOT NULL,
                JobPlayed TEXT NOT NULL,
                Note TEXT
            );
        ";

            using (var command = new SQLiteCommand(createPlayerHistoryTableQuery, connection))
            {
                command.ExecuteNonQuery();
            }

            // Create Whitelist table
            string createWhitelistTableQuery = @"
            CREATE TABLE IF NOT EXISTS Whitelist (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                PlayerName TEXT NOT NULL,
                DutyFirstMet TEXT NOT NULL,
                UnixTimestamp INTEGER NOT NULL,
                JobPlayed TEXT NOT NULL,
                Note TEXT
            );
        ";

            using (var command = new SQLiteCommand(createWhitelistTableQuery, connection))
            {
                command.ExecuteNonQuery();
            }

            // Create Blacklist table
            string createBlacklistTableQuery = @"
            CREATE TABLE IF NOT EXISTS Blacklist (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                PlayerName TEXT NOT NULL,
                DutyFirstMet TEXT NOT NULL,
                UnixTimestamp INTEGER NOT NULL,
                JobPlayed TEXT NOT NULL,
                Note TEXT
            );
        ";

            using (var command = new SQLiteCommand(createBlacklistTableQuery, connection))
            {
                command.ExecuteNonQuery();
            }

            PluginLog.Information("Tables created successfully.");
        }
    }
}
