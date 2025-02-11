using BetterBlacklist.Game;
using BetterBlacklist.UI;
using Newtonsoft.Json;
using System.Data.SQLite;

namespace BetterBlacklist.Database;

public static class Query
{
    public static async Task<Game.Player?> ExtractPlayer(string playerName, string homeWorld)
    {

        string query = @"
                SELECT State
                FROM Players 
                WHERE PlayerName = @PlayerName AND HomeWorld = @HomeWorld
                LIMIT 1;";

        using (var command = new SQLiteCommand(query, Connect.Connection))
        {
            command.Parameters.AddWithValue("@PlayerName", playerName);
            command.Parameters.AddWithValue("@HomeWorld", homeWorld);

            using (var reader = await command.ExecuteReaderAsync())
            {
                if (reader.Read())
                {
                    return new Game.Player(playerName, homeWorld, null, (State)reader.GetInt32(0));
                }
                else
                {
                    return null;
                }
            }
        }
    }

    public static async Task<List<Game.Duty>> ExtractDutyHistory()
    {
        var duties = new List<Game.Duty>();

        string query = "SELECT Duty, UnixTimestamp, Players FROM Duties ORDER BY UnixTimestamp DESC;";

        using (var command = new SQLiteCommand(query, Connect.Connection))
        {
            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    string dutyName = reader.GetString(0);
                    uint unixTimestamp = (uint)reader.GetInt64(1);
                    string playersJson = reader.GetString(2);

                    Player[] players = JsonConvert.DeserializeObject<Player[]>(playersJson)!;

                    var duty = new Game.Duty
                    {
                        Name = dutyName,
                        UnixTimestamp = unixTimestamp,
                        Players = players
                    };

                    duties.Add(duty);
                }
            }
        }

        return duties;
    }
}
