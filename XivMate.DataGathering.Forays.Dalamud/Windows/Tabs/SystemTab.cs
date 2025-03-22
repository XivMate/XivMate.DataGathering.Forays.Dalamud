using Dalamud.Interface.Utility;
using ImGuiNET;
using XivMate.DataGathering.Forays.Dalamud.Extensions;


namespace XivMate.DataGathering.Forays.Dalamud.Windows.Tabs;

public class SystemTab : ITab
{
    // public int Index { get; set; } = 999;
    // public string TabTitle { get; set; } = "System";

    public int Index => 999;
    public string TabTitle => "System";

    void ITab.Draw(Configuration configuration)
    {
        var sysConfig = configuration.SystemConfiguration;

        var apiUrl = sysConfig.ApiUrl;
        if (ImGuiHelper.InputText("Api URL:", ref apiUrl))
        {
            sysConfig.ApiUrl = apiUrl;
            configuration.Save();
        }

        var apiKey = sysConfig.ApiKey;
        if (ImGuiHelper.InputText("Api Key:", ref apiKey))
        {
            sysConfig.ApiKey = apiKey;
            configuration.Save();
        }
    }
}
