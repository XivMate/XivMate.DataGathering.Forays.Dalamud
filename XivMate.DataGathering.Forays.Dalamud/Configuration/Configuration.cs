using System;
using Dalamud.Configuration;

namespace XivMate.DataGathering.Forays.Dalamud;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public bool IsConfigWindowMovable { get; set; } = true;
    public bool SomePropertyToBeSavedAndWithADefault { get; set; } = true;

    public SystemConfiguration SystemConfiguration { get; set; } = new();
    public FateConfiguration FateConfiguration { get; set; } = new();

    // the below exist just to make saving less cumbersome
    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }
}

public class FateConfiguration
{
    public bool Enabled { get; set; } = false;
}
