using ECommons.ImGuiMethods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterBlacklist.UI;

internal static class Customise
{
    internal static void DisplayDraw()
    {
        ImGui.Text("Display Business");
    }

    internal static void ColoursDraw()
    {
        ImGui.Text("Colours Business");
    }

    internal static void Draw()
    {
        ImGuiEx.EzTabBar("##playersbar",
                ("Display", DisplayDraw, null, true),
                ("Colours", ColoursDraw, null, true)
            );
    }
}
