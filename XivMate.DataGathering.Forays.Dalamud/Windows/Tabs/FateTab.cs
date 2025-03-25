using System.Linq;
using ImGuiNET;
using XivMate.DataGathering.Forays.Dalamud.Gathering.Fate;

namespace XivMate.DataGathering.Forays.Dalamud.Windows.Tabs;

/// <summary>
/// Tab for FATE tracking configuration and status display
/// </summary>
public class FateTab(FateModule fateModule) : ITab
{
    /// <inheritdoc />
    public int Index => 1;
    
    /// <inheritdoc />
    public string TabTitle => "Fates";

    /// <inheritdoc />
    public void Draw(Configuration configuration)
    {
        ImGui.Text("Check fates");
        ImGui.SameLine();
        
        bool fateConfigurationEnabled = configuration.FateConfiguration.Enabled;
        if (ImGui.Checkbox("##Track Fates", ref fateConfigurationEnabled))
        {
            configuration.FateConfiguration.Enabled = fateConfigurationEnabled;
            configuration.Save();
            fateModule.LoadConfig(configuration);
        }

        if (!configuration.FateConfiguration.Enabled)
        {
            return;
        }

        int fateCount = fateModule.ActiveFates.Count();
        ImGui.Text($"Fates on map: {fateCount}##{fateCount}");
    }
}
