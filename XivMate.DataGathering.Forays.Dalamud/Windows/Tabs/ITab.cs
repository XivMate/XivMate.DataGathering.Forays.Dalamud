using ImGuiNET;

namespace XivMate.DataGathering.Forays.Dalamud.Windows.Tabs;

public interface ITab
{
    public int Index { get; }
    public string TabTitle { get; }

    public void DrawTab(Configuration configuration)
    {
        if (ImGui.BeginTabItem(TabTitle))
        {
            Draw(configuration);
            ImGui.EndTabItem();
        }
    }

    protected void Draw(Configuration configuration);
}
