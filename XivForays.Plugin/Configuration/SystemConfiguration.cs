using System;

namespace XivMate.DataGathering.Forays.Dalamud.Configuration;

/// <summary>
/// Configuration for system-wide settings
/// </summary>
[Serializable]
public class SystemConfiguration
{
    /// <summary>
    /// URL for the API endpoint
    /// </summary>
    public string ApiUrl { get; set; } = "https://localhost:7222/api/forays/";

    /// <summary>
    /// API authentication key
    /// </summary>
    public string ApiKey { get; set; } = "";
}
