using Dalamud.Interface.Utility;
using ECommons.SimpleGui;
using System.Collections.Generic;

namespace BetterBlacklist.UI;

public unsafe class MainWindow : ConfigWindow
{
    public MainWindow() : base() { }

    public override void Draw()
    {
        ImGui.Text("Content");
    }
}
