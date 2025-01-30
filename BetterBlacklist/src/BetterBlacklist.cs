using Dalamud.Game.Text.SeStringHandling.Payloads;
using ECommons.Configuration;
using ECommons.EzIpcManager;
using ECommons.SimpleGui;
using ECommons.Singletons;
using BetterBlacklist.Services;
using BetterBlacklist.UI;
using FFXIVClientStructs.FFXIV.Client.Game;

namespace BetterBlacklist;

public unsafe class PluginName : IDalamudPlugin
{
    internal static PluginName P;
    internal Configuration Config;
    public PluginName(IDalamudPluginInterface pi)
    {
        // Plugin Initialisation
        P = this;
        ECommonsMain.Init(pi, this, Module.DalamudReflector);


        // Config Window
        EzConfig.Migrate<Configuration>();
        Config = EzConfig.Init<Configuration>();
        EzConfigGui.Init(new MainWindow());

        // Command + IPC
        EzCmd.Add("/bbl", OnChatCommand, "Toggles plugin interface");;
        SingletonServiceManager.Initialize(typeof(ServiceManager));
        Database.Init();

        Svc.Framework.Update += FWM.FrameworkManager.Framework_Update;
    }

    public void Dispose()
    {
        Svc.Framework.Update -= FWM.FrameworkManager.Framework_Update;
        ECommonsMain.Dispose();
        P = null;
    }

    private void OnChatCommand(string command, string arguments)
    {
        arguments = arguments.Trim();

        if (arguments == string.Empty)
        {
            EzConfigGui.Window.Toggle();
        }
        else if (arguments == "debug")
        {
            Config.Debug = !Config.Debug;
            PluginLog.Information($"Debug: {Config.Debug}");
        }
    }
}
