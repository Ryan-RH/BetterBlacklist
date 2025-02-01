using Dalamud.Game.Text.SeStringHandling.Payloads;
using BetterBlacklist.UI;
using FFXIVClientStructs.FFXIV.Client.Game;
using Dalamud.Interface.Windowing;
using Dalamud.Game.Command;
using BetterBlacklist.Services;

namespace BetterBlacklist;

public class BetterBlacklist : IDalamudPlugin
{
    internal static BetterBlacklist P;

    public Configuration Configuration { get; init; }

    public readonly WindowSystem WindowSystem = new("BetterBlacklist");
    private ConfigWindow ConfigWindow { get; init; }
    private MainWindow MainWindow { get; init; }

    public BetterBlacklist(IDalamudPluginInterface pi)
    {
        P = this;

        Configuration = Svc.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        ConfigWindow = new ConfigWindow(this);
        MainWindow = new MainWindow(this);

        WindowSystem.AddWindow(ConfigWindow);
        WindowSystem.AddWindow(MainWindow);

        Svc.PluginInterface.UiBuilder.Draw += DrawUI;
        Svc.PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUI;
        Svc.PluginInterface.UiBuilder.OpenMainUi += ToggleMainUI;


        Svc.Commands.AddHandler("/bbl", new CommandInfo(OnChatCommand));

        Database.Init();

        Svc.Framework.Update += DataCollection.Update;

        //Task.Run(() => { Network.NetStoneFind.Start(); });

    }

    public void Dispose()
    {
        Svc.Framework.Update -= DataCollection.Update;

        WindowSystem.RemoveAllWindows();

        ConfigWindow.Dispose();
        MainWindow.Dispose();
        P = null;
    }

    private void DrawUI() => WindowSystem.Draw();
    public void ToggleConfigUI() => ConfigWindow.Toggle();
    public void ToggleMainUI() => MainWindow.Toggle();

    private void OnChatCommand(string command, string arguments)
    {
        arguments = arguments.Trim();

        if (arguments == string.Empty)
        {
            ToggleMainUI();
        }
    }
}
