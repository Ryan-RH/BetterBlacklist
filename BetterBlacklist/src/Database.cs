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
        if (File.Exists(DbDirectory))
            PluginLog.Information("Database Exists.");
        else
            CreateDB();
    }

    public static (float, string) SearchPlayer(string playerName, string homeWorld, bool destroy)
    {
        // return rating and note

        using (var connection = new SQLiteConnection("Data Source=" + DbDirectory + ";Version=3;"))
        {

            connection.Open();

            string query = @"
                SELECT Rating, Note 
                FROM History 
                WHERE PlayerName = @PlayerName AND HomeWorld = @HomeWorld
                LIMIT 1;";
            using (var command = new SQLiteCommand(query, connection))
            {
                // Use parameters to avoid SQL injection
                command.Parameters.AddWithValue("@PlayerName", playerName);
                command.Parameters.AddWithValue("@HomeWorld", homeWorld);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read()) // If we found a result
                    {
                        // Extract the Rating and Note
                        float rating = reader.GetFloat(6); // Assuming Rating is in the first column
                        string note = reader.IsDBNull(7) ? "" : reader.GetString(7); // Handle nulls for notes

                        if (destroy) // If we need to delete the row
                        {
                            // Delete the row from the database
                            string deleteQuery = @"
                            DELETE FROM History 
                            WHERE PlayerName = @PlayerName AND HomeWorld = @HomeWorld;";

                            using (var deleteCommand = new SQLiteCommand(deleteQuery, connection))
                            {
                                deleteCommand.Parameters.AddWithValue("@PlayerName", playerName);
                                deleteCommand.Parameters.AddWithValue("@HomeWorld", homeWorld);
                                deleteCommand.ExecuteNonQuery(); // Execute the DELETE query
                            }
                        }

                        return (rating, note);
                    }
                    else
                    {
                        return (-1f, "NULL");
                    }
                }
            }

        }
    }

    public static void AddPlayer(string playerName, string homeWorld, string dutyName, string jobPlayed, float rating, string note)
    {
        using (var connection = new SQLiteConnection("Data Source=" + DbDirectory + ";Version=3;"))
        {
            connection.Open();

            // Insert query to add a new player to the History table
            string insertQuery = @"
            INSERT INTO History (PlayerName, HomeWorld, DutySeen, UnixTimestamp, JobPlayed, Rating, Note)
            VALUES (@PlayerName, @HomeWorld, @DutyName, @UnixTimestamp, @JobPlayed, @Rating, @Note);";

            using (var command = new SQLiteCommand(insertQuery, connection))
            {
                // Use parameters to avoid SQL injection
                command.Parameters.AddWithValue("@PlayerName", playerName);
                command.Parameters.AddWithValue("@HomeWorld", homeWorld);
                command.Parameters.AddWithValue("@DutyName", dutyName);

                // Use current Unix timestamp for the UnixTimestamp column
                command.Parameters.AddWithValue("@UnixTimestamp", DateTimeOffset.Now.ToUnixTimeSeconds());

                command.Parameters.AddWithValue("@JobPlayed", jobPlayed);
                command.Parameters.AddWithValue("@Rating", rating);
                command.Parameters.AddWithValue("@Note", note); // Handle null values for Note

                // Execute the insert command
                command.ExecuteNonQuery();
            }
        }
    }

    private static void CreateDB()
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
                HomeWorld TEXT NOT NULL,
                DutySeen TEXT NOT NULL,
                UnixTimestamp INTEGER NOT NULL,
                JobPlayed TEXT NOT NULL,
                Rating FLOAT NOT NULL,
                Note TEXT
            );
        ";

            using (var command = new SQLiteCommand(createPlayerHistoryTableQuery, connection))
            {
                command.ExecuteNonQuery();
            }

            PluginLog.Information("Database created successfully.");
        }
    }
}
