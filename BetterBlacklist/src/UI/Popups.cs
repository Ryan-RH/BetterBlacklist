using Dalamud.Interface.Utility.Raii;
using Lumina.Excel.Sheets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static FFXIVClientStructs.FFXIV.Client.UI.Info.InfoProxy24.Delegates;

namespace BetterBlacklist.UI;

public class Popups
{
    private static float Rating = 5f;

    private static bool Current = false;
    private static string CurrentUser = "";

    public static void DrawUserRating(PlayerData member)
    {
        string popupName = member.Name.Replace(" ", "+") + "-" + member.HomeWorld;
        string popupNameFull = $"User##UserSettings_{popupName}";

        if (ImGui.IsPopupOpen(popupNameFull) && !Current)
        {
            Current = true;
            Rating = member.Rating ?? 5f;
            CurrentUser = popupNameFull;
        }
       
        if (!ImGui.IsPopupOpen(CurrentUser))
        {
            Rating = 5f;
            Current = false;
        }

        if (!ImGui.BeginPopup(popupNameFull, ImGuiWindowFlags.NoMove | ImGuiWindowFlags.AlwaysUseWindowPadding))
        {
            return;
        }

        ImGui.PushItemWidth(195);
        using (ImRaii.PushColor(ImGuiCol.SliderGrab, Util.GetColourRange(Rating / 10)))
        {
            using (ImRaii.PushColor(ImGuiCol.SliderGrabActive, Util.GetColourRange(Rating / 10)))
            {
                ImGui.SliderFloat("", ref Rating, 0, 10, "%.1f");
            }
        }
        ImGui.PopItemWidth();
        ImGui.Dummy(new Vector2(5, 5));

        ImGui.Dummy(new Vector2(2, 5));
        ImGui.SameLine();

        if (Util.DrawButtonIcon(FontAwesomeIcon.ArrowRight, new Vector2(5, 5)))
        {
            Database.UpdatePlayerRatingAndNote(member, Rating);
        }
        Util.SetHoverTooltip("None");
        ImGui.SameLine();
        if (Util.DrawButtonIcon(FontAwesomeIcon.Running, new Vector2(5, 5)))
        {
            Database.UpdatePlayerRatingAndNote(member, Rating, "Prog Skipper");
        }
        Util.SetHoverTooltip("Prog Skipper");
        ImGui.SameLine();
        if (Util.DrawButtonIcon(FontAwesomeIcon.TrashAlt, new Vector2(5, 5)))
        {
            Database.UpdatePlayerRatingAndNote(member, Rating, "Terrible");
        }
        Util.SetHoverTooltip("Terrible");
        ImGui.SameLine();
        if (Util.DrawButtonIcon(FontAwesomeIcon.Radiation, new Vector2(5, 5))) // PersonHarassing also good
        {
            Database.UpdatePlayerRatingAndNote(member, Rating, "Big Mouth");
        }
        Util.SetHoverTooltip("Big Mouth");
        ImGui.SameLine();
        if (Util.DrawButtonIcon(FontAwesomeIcon.Crown, new Vector2(5, 5)))
        {
            Database.UpdatePlayerRatingAndNote(member, Rating, "Goat");
        }
        Util.SetHoverTooltip("Goat");
        ImGui.EndPopup();
    }

    public static void DrawLinks(string tag, string playerName, string homeWorld)
    {
        string popupName = $"User##UserOptions_{tag}";

        if (!ImGui.BeginPopupContextItem(popupName))
        {
            return;
        }

        if (ImGui.Selectable("Open Tomestone"))
        {
            Util.OpenLink($"https://tomestone.gg/character-name/{homeWorld}/{playerName}");
        }
        ImGui.SameLine();

        var test = UiBuilder.IconFont;
        using var font = ImRaii.PushFont(test);
        ImGui.Text(FontAwesomeIcon.ExternalLinkAlt.ToIconString());
        font.Pop();
        var world = Svc.Data.GetExcelSheet<World>().First(world => world.Name == homeWorld);
        string homeRegion = "NA";
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
            case 4:
                homeRegion = "OCE";
                break;
        }


        if (ImGui.Selectable("Open FFLogs"))
        {
            Util.OpenLink($"https://fflogs.com/character/{homeRegion}/{homeWorld}/{playerName}");
        }

        ImGui.EndPopup();
    }
}
