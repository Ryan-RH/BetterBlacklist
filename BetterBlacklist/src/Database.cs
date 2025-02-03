using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game;
using Lumina.Excel.Sheets;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using static Lumina.Data.Parsing.Layer.LayerCommon;

namespace BetterBlacklist;

public static class Database
{
    public static string DbDirectory = Svc.PluginInterface.ConfigDirectory.FullName + "\\BblDatabase.db";

    public class DBPlayerData
    {
        public string? Name;
        public string? HomeWorld;
        public string? DutySeen;
        public int? UnixTimestamp;
        public string? JobPlayed;
        public float? Rating;
        public string? Note;
    }

    public static void Init()
    {
        if (File.Exists(DbDirectory))
            Svc.Log.Information("Database Exists.");
        else
            CreateDB();
    }

    private static bool DBOpened = false;
    private static bool InfoCollected = false;

    public unsafe static void Update(object framework)
    {
        var SRGroup = Svc.Party;
        var currentContentId = GameMain.Instance()->CurrentContentFinderConditionId;
        var player = Svc.ClientState.LocalPlayer;

        using (var connection = new SQLiteConnection("Data Source=" + Database.DbDirectory + ";Version=3;"))
        {
            if (currentContentId != 0)
            {
                if (!DBOpened)
                {
                    connection.Open();
                    DBOpened = true;
                }
            }
            else
            {
                InfoCollected = false; 
                DBOpened = false;
            }

            if (player != null  // player exists
                && player.TargetObject != null // player has target
                && player.TargetObject.ObjectKind == Dalamud.Game.ClientState.Objects.Enums.ObjectKind.BattleNpc // Target is enemy
                && player.TargetObject.TargetObject != null // Enemy has target
                && !InfoCollected
                && DBOpened) // Hasn't collected yet
            {
                Svc.Log.Information("Storing Party Information");
                Svc.Data.GetExcelSheet<ContentFinderCondition>().TryGetRow(currentContentId, out var content);
                if (SRGroup.Count > 0)
                {
                    foreach (var member in SRGroup)
                    {
                        if (member.Name != player.Name && member.World.Value.InternalName.ExtractText() != player.HomeWorld.Value.InternalName.ExtractText())
                        {
                            var worldSheet = Svc.Data.GetExcelSheet<World>();
                            var homeWorld = worldSheet.First(world => world.RowId == member.World.RowId);
                            var (Rating, Note) = SearchPlayerRating(member.Name.TextValue, homeWorld.InternalName.ExtractText());

                            AddPlayer(member.Name.TextValue, homeWorld.InternalName.ExtractText(), content.Name.ExtractText(), member.ClassJob.Value.Abbreviation.ExtractText(), Rating, Note);
                        }
                    }
                }
                else
                {
                    // To be removed
                    var (Rating, Note) = SearchPlayerRating(player.Name.TextValue, player.HomeWorld.Value.InternalName.ExtractText());
                    AddPlayer(player.Name.TextValue, player.HomeWorld.Value.InternalName.ExtractText(), content.Name.ExtractText(), player.ClassJob.Value.Abbreviation.ExtractText(), Rating, Note);
                }
                InfoCollected = true;
                connection.Close();

            }
        }
    }

    public static (float, string) SearchPlayerRating(string playerName, string homeWorld)
    {
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
                    if (reader.Read()) 
                    {
                        var rating = reader.GetFloat(0); 
                        var note = reader.IsDBNull(1) ? string.Empty : reader.GetString(1); 
                        connection.Close();
                        return (rating, note);
                    }
                    else
                    {   
                        connection.Close();
                        return (-1f, "NULL");
                    }
                }
            }
        }
    }

    public static void AddPlayer(string playerName, string homeWorld, string dutyName, string jobPlayed, float rating, string? note = null)
    {
        using (var connection = new SQLiteConnection("Data Source=" + DbDirectory + ";Version=3;"))
        {
            connection.Open();

            string deleteQuery = @"
                            DELETE FROM History 
                            WHERE PlayerName = @PlayerName AND HomeWorld = @HomeWorld;";

            using(var deleteCommand = new SQLiteCommand(deleteQuery, connection))
                            {
                deleteCommand.Parameters.AddWithValue("@PlayerName", playerName);
                deleteCommand.Parameters.AddWithValue("@HomeWorld", homeWorld);
                deleteCommand.ExecuteNonQuery();
            }

            string insertQuery = @"
            INSERT INTO History (PlayerName, HomeWorld, DutySeen, UnixTimestamp, JobPlayed, Rating, Note)
            VALUES (@PlayerName, @HomeWorld, @DutyName, @UnixTimestamp, @JobPlayed, @Rating, @Note);";

            using (var command = new SQLiteCommand(insertQuery, connection))
            {
                command.Parameters.AddWithValue("@PlayerName", playerName);
                command.Parameters.AddWithValue("@HomeWorld", homeWorld);
                command.Parameters.AddWithValue("@DutyName", dutyName);

                command.Parameters.AddWithValue("@UnixTimestamp", DateTimeOffset.Now.ToUnixTimeSeconds());

                command.Parameters.AddWithValue("@JobPlayed", jobPlayed);
                command.Parameters.AddWithValue("@Rating", rating);
                command.Parameters.AddWithValue("@Note", string.IsNullOrEmpty(note) ? DBNull.Value : (object)note);

                command.ExecuteNonQuery();
            }
            connection.Close();
        }
    }

    public static void UpdatePlayerRatingAndNote(PlayerData player, float newRating, string? newNote = null)
    {
        ImGui.CloseCurrentPopup();
        using (var connection = new SQLiteConnection("Data Source=" + DbDirectory + ";Version=3;"))
        {
            connection.Open();

            string updateQuery = @"
            UPDATE History
            SET Rating = @Rating, Note = @Note
            WHERE PlayerName = @PlayerName AND HomeWorld = @HomeWorld;";

            using (var command = new SQLiteCommand(updateQuery, connection))
            {
                // Use parameters to prevent SQL injection
                command.Parameters.AddWithValue("@PlayerName", player.Name);
                command.Parameters.AddWithValue("@HomeWorld", player.HomeWorld);
                command.Parameters.AddWithValue("@Rating", newRating);
                command.Parameters.AddWithValue("@Note", string.IsNullOrEmpty(newNote) ? DBNull.Value : (object)newNote);

                int rowsAffected = command.ExecuteNonQuery();


                if (rowsAffected == 0)
                {
                    var job = Svc.Data.GetExcelSheet<ClassJob>().First(job => player.Job == job.RowId);
                    AddPlayer(player.Name!, player.HomeWorld!, "Added from Party", job.Abbreviation.ExtractText(), newRating, newNote);
                }
            }

            connection.Close();
        }
    }

    public static List<DBPlayerData> ExtractHistory()
    {
        var historyList = new List<DBPlayerData>();
        using (var connection = new SQLiteConnection("Data Source=" + DbDirectory + ";Version=3;"))
        {
            connection.Open();

            // Define the query to extract all rows from the History table
            string query = @"
                SELECT PlayerName, HomeWorld, DutySeen, UnixTimestamp, JobPlayed, Rating, Note 
                FROM History
                ORDER BY UnixTimestamp DESC;";
            using (var command = new SQLiteCommand(query, connection))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var playerName = reader.GetString(0);
                    var homeWorld = reader.GetString(1);
                    var dutySeen = reader.GetString(2);
                    var unixTimestamp = reader.GetInt32(3);//reader.GetInt32(1); // If you need to convert it back to DateTime, you can use DateTimeOffset.FromUnixTimeSeconds(unixTimestamp)
                    var jobPlayed = reader.GetString(4);
                    var rating = reader.GetFloat(5);
                    var note = reader.IsDBNull(4) ? string.Empty : reader.GetString(4); //

                    var player = new DBPlayerData
                    {
                        Name = playerName,
                        HomeWorld = homeWorld,
                        DutySeen = dutySeen,
                        UnixTimestamp = unixTimestamp,
                        JobPlayed = jobPlayed,
                        Rating = rating,
                        Note = note
                    };

                    historyList.Add(player);
                }
            }
            connection.Close();
        }
        return historyList;
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
                PlayerName TEXT NOT NULL,
                HomeWorld TEXT NOT NULL,
                DutySeen TEXT NOT NULL,
                UnixTimestamp INTEGER NOT NULL,
                JobPlayed TEXT NOT NULL,
                Rating FLOAT NOT NULL,
                Note TEXT,
                PRIMARY KEY (PlayerName, HomeWorld)
            );
            ";

            using (var command = new SQLiteCommand(createPlayerHistoryTableQuery, connection))
            {
                command.ExecuteNonQuery();
            }
            connection.Close();
            Svc.Log.Information("Database created successfully.");
        }
    }
}
