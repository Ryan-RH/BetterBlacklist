namespace BetterBlacklist.UI;

public unsafe class MainWindow : Window, IDisposable
{
    public static bool PartyView = true;

    public MainWindow(BetterBlacklist plugin) : base("BetterBlacklist##mainWin")
    {
        Size = new(600, 350);
        Flags = ImGuiWindowFlags.NoResize | ImGuiWindowFlags.MenuBar | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoDocking;
    }

    public void Dispose() { }

    public override void Draw()
    {
        MenuBar.Draw();

        if (PartyView)
            PartyList.Draw();
        else
            HistoryList.Draw();
    }
}
