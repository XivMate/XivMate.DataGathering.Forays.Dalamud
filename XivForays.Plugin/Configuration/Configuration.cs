using System;
using Dalamud.Configuration;

namespace XivMate.DataGathering.Forays.Dalamud.Configuration;

/// <summary>
/// Main plugin configuration
/// </summary>
[Serializable]
public class Configuration : IPluginConfiguration
{
    /// <summary>
    /// Configuration version
    /// </summary>
    public int Version { get; set; } = 0;

    /// <summary>
    /// System-related configuration
    /// </summary>
    public SystemConfiguration SystemConfiguration { get; set; } = new();

    /// <summary>
    /// FATE tracking configuration
    /// </summary>
    public FateConfiguration FateConfiguration { get; set; } = new();

    /// <summary>
    /// Saves the configuration
    /// </summary>
    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }
}

/// <summary>
/// Configuration for FATE tracking module
/// </summary>
[Serializable]
public class FateConfiguration
{
    /// <summary>
    /// Whether FATE tracking is enabled
    /// </summary>
    public bool Enabled { get; set; } = false;
}
