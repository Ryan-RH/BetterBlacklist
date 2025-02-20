using Dalamud.Configuration;

namespace BetterBlacklist;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;
    public bool DataCollection = false;
    public bool DutyWindow = false;

    public void Save()
    {
        Svc.PluginInterface.SavePluginConfig(this);
    }
}
