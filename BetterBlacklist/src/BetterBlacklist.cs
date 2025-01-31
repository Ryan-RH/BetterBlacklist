using Dalamud.Game.Text.SeStringHandling.Payloads;
using ECommons.Configuration;
using ECommons.EzIpcManager;
using ECommons.Singletons;
using BetterBlacklist.Services;
using BetterBlacklist.UI;
using FFXIVClientStructs.FFXIV.Client.Game;
using Dalamud.Interface.Windowing;

namespace BetterBlacklist;

public class BetterBlacklist : IDalamudPlugin
{
    internal static BetterBlacklist P;
    internal Configuration Config;


    public Configuration Configuration { get; init; }
    public readonly WindowSystem WindowSystem = new("BetterBlacklist");
    private ConfigWindow ConfigWindow { get; init; }
    private MainWindow MainWindow { get; init; }

    public BetterBlacklist(IDalamudPluginInterface pi)
    {
        // Plugin Initialisation
        P = this;
        ECommonsMain.Init(pi, this, Module.DalamudReflector);

        Configuration = Svc.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        ConfigWindow = new ConfigWindow(this);
        MainWindow = new MainWindow(this);

        WindowSystem.AddWindow(ConfigWindow);
        WindowSystem.AddWindow(MainWindow);

        Svc.PluginInterface.UiBuilder.Draw += DrawUI;
        Svc.PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUI;
        Svc.PluginInterface.UiBuilder.OpenMainUi += ToggleMainUI;

        // Config Window
        /*
        EzConfig.Migrate<Configuration>();
        Config = EzConfig.Init<Configuration>();
        EzConfigGui.Init(new MainWindow());
        EzConfigGui.WindowSystem.AddWindow(new Popup());*/



        // Command + IPC
        EzCmd.Add("/bbl", OnChatCommand, "Toggles plugin interface");;
        SingletonServiceManager.Initialize(typeof(ServiceManager));
        Database.Init();

        Svc.Framework.Update += FWM.FrameworkManager.Framework_Update;

        Task.Run(() => { Network.NetStoneFind.Start(); });

    }

    public void Dispose()
    {
        Svc.Framework.Update -= FWM.FrameworkManager.Framework_Update;
        ECommonsMain.Dispose();

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
        else if (arguments == "debug")
        {
            Config.Debug = !Config.Debug;
            PluginLog.Information($"Debug: {Config.Debug}");
        }
    }
}
