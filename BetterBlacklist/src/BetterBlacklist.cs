using Dalamud.Game.Text.SeStringHandling.Payloads;
using BetterBlacklist.UI;
using FFXIVClientStructs.FFXIV.Client.Game;
using Dalamud.Interface.Windowing;
using Dalamud.Game.Command;
using BetterBlacklist.Services;
using System.Net;
using BetterBlacklist.GUI;
using BetterBlacklist.Game;
using BetterBlacklist.Database;

namespace BetterBlacklist;

public class BetterBlacklist : IDalamudPlugin
{
    internal static BetterBlacklist P;

    public Configuration Configuration { get; init; }

    public readonly WindowSystem WindowSystem = new("BetterBlacklist");
    private ConfigWindow ConfigWindow { get; init; }
    private MainWindow MainWindow { get; init; }
    private DutyWindow DutyWindow { get; init; }

    public BetterBlacklist(IDalamudPluginInterface pi)
    {
        P = this;
        Svc.Init(pi);

        Configuration = Svc.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        ConfigWindow = new ConfigWindow(this);
        MainWindow = new MainWindow(this);
        DutyWindow = new DutyWindow(this);

        WindowSystem.AddWindow(ConfigWindow);
        WindowSystem.AddWindow(MainWindow);
        WindowSystem.AddWindow(DutyWindow);

        Svc.PluginInterface.UiBuilder.Draw += DrawUI;
        Svc.PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUI;
        Svc.PluginInterface.UiBuilder.OpenMainUi += ToggleMainUI;


        Svc.Commands.AddHandler("/bbl", new CommandInfo(OnChatCommand));

        //Database.Init();
        Database.Setup.Init();


        Svc.Framework.Update += Database.Collection.Update;
        Svc.PartyFinder.ReceiveListing += PartyFinder.Update;

        Task.Run(() => { Tomestone.Start(); });
    }

    public void Dispose()
    {
        Svc.Framework.Update -= Database.Collection.Update;
        Svc.PartyFinder.ReceiveListing -= PartyFinder.Update;

        WindowSystem.RemoveAllWindows();
        Database.Connect.CloseConnection();
        ConfigWindow.Dispose();
        MainWindow.Dispose();
        P = null;
    }

    private void DrawUI() => WindowSystem.Draw();
    public void ToggleConfigUI() => ConfigWindow.Toggle();
    public void ToggleMainUI() => MainWindow.Toggle();
    public void ToggleDutyUI() => DutyWindow.Toggle();

    private void OnChatCommand(string command, string arguments)
    {
        arguments = arguments.Trim();

        if (arguments == string.Empty)
            ToggleMainUI();
    }
}
