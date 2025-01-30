using Dalamud.Interface.Utility.Raii;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterBlacklist.UI;

internal static class Util
{
    public static bool DrawButtonIcon(FontAwesomeIcon icon, Vector2? size = null)
    {
        using var font = ImRaii.PushFont(UiBuilder.IconFont);
        using ImRaii.Style style = new();
        if (size != null)
        {
            style.Push(ImGuiStyleVar.FramePadding, size.Value);
        }

        ImGui.AlignTextToFramePadding();
        var ret = ImGui.Button(icon.ToIconString());

        if (size != null)
        {
            style.Pop();
        }

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
}
