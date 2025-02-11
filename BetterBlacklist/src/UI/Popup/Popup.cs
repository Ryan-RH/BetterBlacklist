using Dalamud.Interface.Components;
using Lumina.Excel.Sheets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterBlacklist.UI;

public static class Popup
{
    public static void Draw(Game.Player player)
    {
        if (player == null || player.Name == null || player.HomeWorld == null) return;

        string popupName = $"##US-{player.Name.Replace(" ", "+")}_{player.HomeWorld}";

        if (!ImGui.BeginPopup(popupName, ImGuiWindowFlags.NoMove | ImGuiWindowFlags.AlwaysUseWindowPadding)) return;

        if (ImGui.Selectable("Open FF Logs"))
        {
            FFLogsLink(player);
        }
        ImGui.Dummy(new Vector2(0, 5));
        if (ImGui.Selectable("Open TomeStone"))
        {
            Util.OpenLink($"https://tomestone.gg/character-name/{player.HomeWorld}/{player.Name}");
        }
        ImGuiComponents.HelpMarker("Tomestone is a website developed by the creator of FF Logs.\nIt can be used to see the current prog point/activity of a player based on logs.");

        ImGui.EndPopup();
    }

    private static void FFLogsLink(Game.Player player)
    {
        var world = Svc.Data.GetExcelSheet<World>().First(world => world.InternalName.ExtractText() == player.HomeWorld);
        string homeRegion;

        switch (world.DataCenter.Value.PvPRegion)
        {
            case 1:
                homeRegion = "JP";
                break;
            case 2:
                homeRegion = "NA";
                break;
            case 3:
                homeRegion = "EU";
                break;
            default:
                homeRegion = "OCE";
                break;
        }

        Util.OpenLink($"https://fflogs.com/character/{homeRegion}/{player.HomeWorld}/{player.Name}");
    }
}
