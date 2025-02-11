using BetterBlacklist.Game;
using BetterBlacklist.Tasks;
using BetterBlacklist.UI;
using Newtonsoft.Json;
using System.Data.SQLite;
using static BetterBlacklist.Services.Tomestone;

namespace BetterBlacklist.Database;

public static class Command
{
    public static void AddPlayer(Game.Player player)
    {
        Task.Run(async () =>
        {
            try
            {
                //await Modify.AddPlayer(player).ConfigureAwait(false);
                //UpdatePartyRating();
                await Modify.AddPlayer(player).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Svc.Log.Information($"Failed to add {player.Name}: {ex.Message}");
            }
        });

    }
    public static void ModifyRating(Game.Player player)
    {
        Task.Run(async () =>
        {
            try
            {
                await Modify.Rating(player);
            }
            catch (Exception ex)
            {
                Svc.Log.Information($"Failed to modify rating: {ex.Message}");
            }
        });
    }

    public static void AddDuty(Game.Duty duty)
    {
        Task.Run(async () =>
        {
            try
            {
                await Modify.AddDuty(duty).ConfigureAwait(false);   
            }
            catch (Exception ex)
            {
                Svc.Log.Information($"Failed to add duty: {ex.Message}");
            }
        });
    }

    public static async Task FindPartyState()
    {
        var tasks = new List<Task<Player?>>();
        
        var playerStates = await Task.WhenAll(tasks);

    }

    public static async Task FindDutyState(int index)
    {
        var dutyContent = UI.HistoryList.DutyHistory[index];
        var dutyParty = dutyContent.duty.Players;
        if (!dutyContent.visible)
        {
            for (int i = 0; i < dutyParty.Length; i++)
            {
                var player = dutyParty[i];
                if (Game.Util.IsFriend(player))
                {
                    UI.HistoryList.DutyHistory[index].duty.Players[i].State = State.Friend;
                }
                else
                {
                    var playerData = await Query.ExtractPlayer(player.Name!, player.HomeWorld!);
                    if (playerData != null)
                    {
                        UI.HistoryList.DutyHistory[index].duty.Players[i].State = playerData.State;
                    }
                }
            }
        }
        UI.HistoryList.DutyHistory[index].visible = !UI.HistoryList.DutyHistory[index].visible;
    }
}
