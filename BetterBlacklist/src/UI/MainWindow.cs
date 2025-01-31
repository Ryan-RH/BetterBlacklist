using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using ECommons.ImGuiMethods;
using ECommons.SimpleGui;
using System.Collections.Generic;

namespace BetterBlacklist.UI;

public unsafe class MainWindow : Window, IDisposable
{
    public static bool PartyView = true;
    private BetterBlacklist Plugin;

    public MainWindow(BetterBlacklist plugin) : base("BetterBlacklist##mainWin")
    {
        Size = new(600, 350);
        Flags = ImGuiWindowFlags.NoResize | ImGuiWindowFlags.MenuBar | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoDocking;
        Plugin = plugin;
    }


    public void Dispose() { }


    public override void Draw()
    {
        MenuBar.Draw();

        if (PartyView)
            Players.PartyDraw();
    }
}
