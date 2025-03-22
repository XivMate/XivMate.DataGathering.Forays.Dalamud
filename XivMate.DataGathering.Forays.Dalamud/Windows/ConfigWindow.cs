using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using ImGuiNET;
using XivMate.DataGathering.Forays.Dalamud.Windows.Tabs;

namespace XivMate.DataGathering.Forays.Dalamud.Windows;

public class ConfigWindow : Window, IDisposable
{
    private readonly IEnumerable<ITab> tabs;
    private Configuration Configuration;

    // We give this window a constant ID using ###
    // This allows for labels being dynamic, like "{FPS Counter}fps###XYZ counter window",
    // and the window ID will always be "###XYZ counter window" for ImGui
    public ConfigWindow(Plugin plugin, IEnumerable<ITab> tabs, IPluginLog log) : base("XivMate Settings")
    {
        this.tabs = tabs;
        log.Info($"Config Window has {tabs?.Count()} tabs");
        Flags = ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar |
                ImGuiWindowFlags.NoScrollWithMouse;

        Size = new Vector2(600, 250);
        SizeCondition = ImGuiCond.Always;

        Configuration = plugin.Configuration;
    }

    public void Dispose() { }

    public override void PreDraw()
    {
        // Flags must be added or removed before Draw() is being called, or they won't apply
        if (Configuration.IsConfigWindowMovable)
        {
            Flags &= ~ImGuiWindowFlags.NoMove;
        }
        else
        {
            Flags |= ImGuiWindowFlags.NoMove;
        }
    }

    public override void Draw()
    {
        ImGui.BeginTabBar("#tabs");
        foreach(var tab in tabs)
            tab.DrawTab(Configuration);
        ImGui.EndTabBar();
        // // can't ref a property, so use a local copy
        // var configValue = Configuration.SomePropertyToBeSavedAndWithADefault;
        // if (ImGui.Checkbox("Random Config Bool", ref configValue))
        // {
        //     Configuration.SomePropertyToBeSavedAndWithADefault = configValue;
        //     // can save immediately on change, if you don't want to provide a "Save and Close" button
        //     Configuration.Save();
        // }
        //
        // var movable = Configuration.IsConfigWindowMovable;
        // if (ImGui.Checkbox("Movable Config Window", ref movable))
        // {
        //     Configuration.IsConfigWindowMovable = movable;
        //     Configuration.Save();
        // }
    }
}
