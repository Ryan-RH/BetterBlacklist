using BetterBlacklist.Database;
using BetterBlacklist.Game;
using BetterBlacklist.Services;
using BetterBlacklist.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Info;
using Lumina.Excel.Sheets;

namespace BetterBlacklist.Tasks;

public static class Party
{
    public static async Task Refresh()
    {
        Game.Party party = Game.Party.Collect();
        for (int i = 0; i < party.Size; i++)
        {
            var member = party.Members[i];
            if (Game.Util.IsFriend(member))
            {
                member.State = State.Friend;
            }
            else
            {
                var memberData = await Query.ExtractPlayer(member.Name!, member.HomeWorld!);
                if (memberData != null)
                {
                    member.State = memberData.State;
                }
            }
        }
        PartyList.party = party;
        await Tomestone.FetchPartyProg();
        MenuBar.Refreshing = false;
    }

    public static async Task DWAddPlayers()
    {
        foreach (var player in Collection.currentDuty.Players)
        {
            await Modify.Rating(player);
        }
        Collection.currentDuty.Clear();
    }

}
