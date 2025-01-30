using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using ECommons.ImGuiMethods;
using ECommons.SimpleGui;
using System.Collections.Generic;

namespace BetterBlacklist.UI;

public unsafe class MainWindow : ConfigWindow
{
    private static bool PartyView = true;


    public MainWindow() : base()
    {
        //Size = new(430, 350);
        Size = new(600, 350);
        Flags = ImGuiWindowFlags.NoResize | ImGuiWindowFlags.MenuBar;
    }

    public override void Draw()
    {
        MenuBar.Draw();
        /*
        ImGuiEx.EzTabBar("##tabbar",
            ("Players", Players.Draw, null, true),
            ("Customise", Customise.Draw, null, true)
        );*/
        if (PartyView)
            Players.PartyDraw();
    }
}
