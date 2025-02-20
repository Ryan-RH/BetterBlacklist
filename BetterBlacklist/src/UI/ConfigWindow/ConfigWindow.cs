using FFXIVClientStructs.FFXIV.Common.Lua;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterBlacklist.UI;

public class ConfigWindow : Window, IDisposable
{
    private Configuration Configuration;

    public ConfigWindow(BetterBlacklist plugin) : base("Customise##configWin")
    {
        Flags = ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar |
                ImGuiWindowFlags.NoScrollWithMouse;

        Size = new Vector2(232, 90);
        SizeCondition = ImGuiCond.Always;

        Configuration = plugin.Configuration;
    }

    public void Dispose() { }

    public override void Draw()
    {
        ImGui.Checkbox("Enable DB Collection", ref Configuration.DataCollection);
        ImGui.Checkbox("Enable Duty End Window", ref Configuration.DutyWindow);
    }
}
