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
                currentDuty.Players = Party.Collect().Members.ToArray();
                foreach (var player in currentDuty.Players)
                {
                    player.State = State.Familiar;
                }
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
                //ToggleDutyUI();
                P.ToggleDutyUI();
            }
        }
    }
}
