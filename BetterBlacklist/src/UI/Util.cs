using Dalamud.Interface.Utility.Raii;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    public static void AddTextVertical(string text, uint textColor, float scale)
    {

        var drawList = ImGui.GetWindowDrawList();
        var pos = ImGui.GetCursorScreenPos();
        var font = ImGui.GetFont();
        var size = ImGui.CalcTextSize(text);
        pos.X = (float)Math.Round(pos.X);
        pos.Y = (float)Math.Round(pos.Y) + (float)Math.Round(size.X * scale);

        foreach (var c in text)
        {
            var glyph = font.FindGlyph(c);

            drawList.PrimReserve(6, 4);

            drawList.PrimQuadUV(
                pos + new Vector2(glyph.Y0 * scale, -glyph.X0 * scale),
                pos + new Vector2(glyph.Y0 * scale, -glyph.X1 * scale),
                pos + new Vector2(glyph.Y1 * scale, -glyph.X1 * scale),
                pos + new Vector2(glyph.Y1 * scale, -glyph.X0 * scale),

                new Vector2(glyph.U0, glyph.V0),
                new Vector2(glyph.U1, glyph.V0),
                new Vector2(glyph.U1, glyph.V1),
                new Vector2(glyph.U0, glyph.V1),
                textColor);
            pos.Y -= glyph.AdvanceX * scale;
        }

        ImGui.Dummy(new Vector2(size.Y, size.X));

    }

    public static void OpenLink(string link)
    {
        Dalamud.Utility.Util.OpenLink(link);
    }
}
