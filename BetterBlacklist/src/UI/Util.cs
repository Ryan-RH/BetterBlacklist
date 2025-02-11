using Dalamud.Interface.Utility.Raii;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FFXIVClientStructs.Havok.Common.Base.System.IO.IStream.hkIstream.Delegates;

namespace BetterBlacklist.UI;
internal static class Util
{
    public static bool DrawButtonIcon(FontAwesomeIcon icon, Vector2? size = null, string? id = null)
    {
        using var font = ImRaii.PushFont(UiBuilder.IconFont);
        using ImRaii.Style style = new();
        if (size != null)
            style.Push(ImGuiStyleVar.FramePadding, size.Value);

        ImGui.AlignTextToFramePadding();

        var ret = false;
        if (id == null)
            ret = ImGui.Button(icon.ToIconString());
        else
            ret = ImGui.Button($"{icon.ToIconString()}##{id}");


        if (size != null)
            style.Pop();

        font.Pop();

        return ret;
    }

    public static void SetHoverTooltip(string tooltip, bool allowWhenDisabled = false)
    {
        if (ImGui.IsItemHovered(allowWhenDisabled ? ImGuiHoveredFlags.AllowWhenDisabled : ImGuiHoveredFlags.None))
        {
            ImGui.BeginTooltip();
            ImGui.TextUnformatted(tooltip);
            ImGui.EndTooltip();
        }
    }

    public static Vector4 GetColourRange(float percent)
    {
        float red = MathF.Min(2 * (1.0f - percent), 1.0f);
        float green = MathF.Min(2 * percent, 1.0f);

        return new Vector4(red, green, 0.0f, 1.0f);
    }

    public static void OpenLink(string link)
    {
        Dalamud.Utility.Util.OpenLink(link);
    }

    public static Vector4 GetColourState(Game.State state)
    {
        switch (state)
        {
            case Game.State.Friend:
                return new Vector4(1.00f, 0.00f, 0.84f, 1.0f);
            case Game.State.Good:
                return new Vector4(0.00f, 1.00f, 0.00f, 1.0f);
            case Game.State.Familiar:
                return new Vector4(0.00f, 1.00f, 1.00f, 1.0f);
            case Game.State.New:
                return new Vector4(0.85f, 0.85f, 0.85f, 1.0f);
            case Game.State.Poor:
                return new Vector4(1.00f, 1.00f, 0.00f, 1.0f);
            case Game.State.Bad:
                return new Vector4(1.00f, 0.50f, 0.00f, 1.0f);
            case Game.State.Avoid:
                return new Vector4(1.00f, 0.00f, 0.00f, 1.0f);
            default:
                return new Vector4(0, 0, 0, 0);
        }
    }
    public static Vector4 DecideColour(string progPoint, int ultimate)
    {
        string[] parts = progPoint.Split(':');
        string numberPart = parts[0].Substring(1);
        int phase = int.Parse(numberPart);

        int divider = 0;

        switch (ultimate)
        {
            case 0:
            case 1:
            case 5:
                divider = 7;
                break;
            case 2:
                divider = 6;
                break;
            case 3:
                divider = 9;
                break;
            case 4:
                divider = 8;
                break;
        }

        var progress = phase / (float)divider;

        return GetColourRange(progress);
    }

    public static void MakeIconAndPlayerSelectable(Game.Player player)
    {
        var jobIcon = Game.Util.GetJobIcon(player.JobId);
        float totalWidth = ImGui.CalcTextSize(player.Name).X + 30;

        var cursorPos = ImGui.GetCursorPos();
        var copyPos = new Vector2(cursorPos.X + 3, cursorPos.Y);
        ImGui.SetCursorPos(copyPos);
        var popupName = $"##US-{player.Name!.Replace(" ", "+")}_{player.HomeWorld}";

        Popup.Draw(player);
        if (ImGui.Selectable(popupName, false, ImGuiSelectableFlags.None, new Vector2(totalWidth, 20)))
        {
            ImGui.OpenPopup(popupName);
        }
        ImGui.SetCursorPos(cursorPos);

        var iconSize = new Vector2(20, 20);
        if (jobIcon != null)
        {
            ImGui.Image(jobIcon.ImGuiHandle, iconSize);
        }
        else
        {
            ImGui.Image(Game.Util.GetJobIcon(45)!.ImGuiHandle, iconSize);
        }
        ImGui.SameLine();

        using var colour = (ImRaii.PushColor(ImGuiCol.Text, Util.GetColourState(player.State)));
        ImGui.Text(player.Name);
        colour.Pop();
    }
}
