using BetterBlacklist.Game;
using BetterBlacklist.UI;
using Lumina.Excel.Sheets;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterBlacklist.Database;

public static class Modify
{
    public static async Task AddPlayer(Game.Player player)
    {
        var oldPlayer = await Query.ExtractPlayer(player.Name!, player.HomeWorld!);

        if (oldPlayer != null)
        {
            string deleteQuery = @"
                        DELETE FROM Players 
                        WHERE PlayerName = @PlayerName AND HomeWorld = @HomeWorld;";
            using (var deleteCommand = new SQLiteCommand(deleteQuery, Connect.Connection))
            {
                deleteCommand.Parameters.AddWithValue("@PlayerName", player.Name);
                deleteCommand.Parameters.AddWithValue("@HomeWorld", player.HomeWorld);
                await deleteCommand.ExecuteNonQueryAsync();
            }

            player.State = oldPlayer.State;
        }

        string insertQuery = @"
        INSERT INTO Players (PlayerName, HomeWorld, State)
        VALUES (@PlayerName, @HomeWorld, @State);";

        using (var insertCommand = new SQLiteCommand(insertQuery, Connect.Connection))
        {
            insertCommand.Parameters.AddWithValue("@PlayerName", player.Name);
            insertCommand.Parameters.AddWithValue("@HomeWorld", player.HomeWorld);
            insertCommand.Parameters.AddWithValue("@State", (int)player.State);

            await insertCommand.ExecuteNonQueryAsync();
        }
    }


    public static async Task AddDuty(Game.Duty duty)
    {
        string playersJson = JsonConvert.SerializeObject(duty.Players.Where(player => player != null).ToArray());
        string insertPlayers = @"
        INSERT INTO Duties (Duty, UnixTimestamp, Players)
        VALUES (@Duty, @UnixTimestamp, @Players);
        ";

        using (var command = new SQLiteCommand(insertPlayers, Connect.Connection))
        {
            command.Parameters.AddWithValue("@Duty", duty.Name);
            command.Parameters.AddWithValue("@UnixTimestamp", duty.UnixTimestamp);
            command.Parameters.AddWithValue("@Players", playersJson);
            await command.ExecuteNonQueryAsync();
        }
        HistoryList.Update();
    }

    public static async Task Rating(Game.Player player)
    {
        string updateQuery = @"
            UPDATE Players
            SET State = @State
            WHERE PlayerName = @PlayerName AND HomeWorld = @HomeWorld;";

        using (var command = new SQLiteCommand(updateQuery, Connect.Connection))
        {
            command.Parameters.AddWithValue("@PlayerName", player.Name);
            command.Parameters.AddWithValue("@HomeWorld", player.HomeWorld);
            command.Parameters.AddWithValue("@State", (int)player.State);

            int rowsAffected = await command.ExecuteNonQueryAsync();

            if (rowsAffected == 0)
            {
                await AddPlayer(player);
            }
        }
    }
}
