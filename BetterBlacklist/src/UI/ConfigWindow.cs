using FFXIVClientStructs.FFXIV.Common.Lua;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace BetterBlacklist.UI;

public class ConfigWindow : Window, IDisposable
{
    private Configuration Configuration;
    private float value = 0f;

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
        ImGui.Text("Testing");
        ImGui.SliderFloat("Slider", ref value, 0, 10);
    }
}
