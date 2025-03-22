using System.Linq;
using ImGuiNET;
using XivMate.DataGathering.Forays.Dalamud.Gathering.Fate;

namespace XivMate.DataGathering.Forays.Dalamud.Windows.Tabs;

public class FateTab(FateModule fateModule) : ITab
{
    public int Index { get; } = 1;
    public string TabTitle => "Fates";

    public void Draw(Configuration configuration)
    {
        ImGui.Text("Check fates");
        ImGui.SameLine();
        var fateConfigurationEnabled = configuration.FateConfiguration.Enabled;
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

        ImGui.Text($"Fates on map: {fateModule.ActiveFates.Count()}##{fateModule.ActiveFates.Count()}");
    }
}
