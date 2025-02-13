using Dalamud.Configuration;

namespace BetterBlacklist;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public void Save()
    {
        Svc.PluginInterface.SavePluginConfig(this);
    }
}
