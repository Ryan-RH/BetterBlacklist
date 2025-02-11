using BetterBlacklist.Game;
using BetterBlacklist.UI;
using FFXIVClientStructs.FFXIV.Client.Game;
using Lumina.Excel.Sheets;

namespace BetterBlacklist.Database;

public static class Collection
{
    private static bool InfoCollected = false;
    public static Game.Duty currentDuty = new Game.Duty();
    public static DateTime timeOfExit = DateTime.Now;

    public unsafe static void Update(object framework)
    {
        var currentContentId = GameMain.Instance()->CurrentContentFinderConditionId;
        var localPlayer = Svc.ClientState.LocalPlayer;

        if (currentContentId != 0)
        {
            if (localPlayer != null
                && localPlayer.TargetObject != null
                && localPlayer.TargetObject.ObjectKind == Dalamud.Game.ClientState.Objects.Enums.ObjectKind.BattleNpc
                && localPlayer.TargetObject.TargetObject != null
                && !InfoCollected)
            {
                Svc.Log.Information("Storing Party Information...");
                Svc.Data.GetExcelSheet<ContentFinderCondition>().TryGetRow(currentContentId, out var content);

                currentDuty.Name = content.Name.ExtractText();
                currentDuty.UnixTimestamp = (uint)DateTimeOffset.Now.ToUnixTimeSeconds();
                FindParty();
                InfoCollected = true;
            }
        }
        else
        {
            if (InfoCollected)
            {
                InfoCollected = false;
                timeOfExit = DateTime.Now;
                Command.AddDuty(currentDuty);
                P.ToggleDutyUI();
            }
        }
    }

    public static void FindParty()
    {
        Task.Run(async () =>
        {
            try
            {
                await FindPartyCondition();
            }
            catch (Exception ex)
            {
                Svc.Log.Error($"Shits fucked: {ex.Message}");
            }

        });
    }

    public static async Task FindPartyCondition()
    {
        var partyState = await Tasks.Party.FetchPartyState(Party.Collect());
        currentDuty.Players = partyState.Members.ToArray();
        foreach (var member in currentDuty.Players)
        {
            if (member.State == State.New)
                member.State = State.Familiar;
        }
    }

}
