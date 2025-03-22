using System.IO;
using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Microsoft.Extensions.DependencyInjection;
using XivMate.DataGathering.Forays.Dalamud.Gathering.Fate;
using XivMate.DataGathering.Forays.Dalamud.Services;
using XivMate.DataGathering.Forays.Dalamud.Windows;

namespace XivMate.DataGathering.Forays.Dalamud;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ITextureProvider TextureProvider { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static IClientState ClientState { get; private set; } = null!;
    [PluginService] internal static IDataManager DataManager { get; private set; } = null!;
    [PluginService] internal static IPluginLog Log { get; private set; } = null!;
    [PluginService] internal static IFateTable FateTable { get; private set; } = null!;
    [PluginService] internal static IFramework Framework { get; private set; } = null!;

    private const string CommandName = "/xivmate";

    public Configuration Configuration { get; init; }

    public readonly WindowSystem WindowSystem = new("XivMate");
    private ServiceProvider provider;
    private ConfigWindow ConfigWindow { get; init; }
    private MainWindow MainWindow { get; init; }

    public Plugin()
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        // you might normally want to embed resources and load them from the manifest stream
        var goatImagePath = Path.Combine(PluginInterface.AssemblyLocation.Directory?.FullName!, "goat.png");

        ConfigWindow = new ConfigWindow(this);
        MainWindow = new MainWindow(this, goatImagePath);

        WindowSystem.AddWindow(ConfigWindow);
        WindowSystem.AddWindow(MainWindow);

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Open XivMate main window"
        });

        PluginInterface.UiBuilder.Draw += DrawUI;

        // This adds a button to the plugin installer entry of this plugin which allows
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUI;

        // Adds another button that is doing the same but for the main ui of the plugin
        PluginInterface.UiBuilder.OpenMainUi += ToggleMainUI;

        GenerateDependencyInjection();

    }

    private void GenerateDependencyInjection()
    {
        var services = new ServiceCollection();
        services.AddSingleton(this);
        services.AddSingleton(TextureProvider);
        services.AddSingleton(PluginInterface);
        services.AddSingleton(CommandManager);
        services.AddSingleton(ClientState);
        services.AddSingleton(DataManager);
        services.AddSingleton(Framework);
        services.AddSingleton(FateTable);
        services.AddSingleton(Log);
        services.AddSingleton<FateModule>();
        services.AddSingleton<SchedulerService>();
        services.AddSingleton<TerritoryService>();
        provider = services.BuildServiceProvider();
        var fateModule = provider.GetRequiredService<FateModule>();
        fateModule.Enable();
        
    }


    public void Dispose()
    {
        WindowSystem.RemoveAllWindows();

        ConfigWindow.Dispose();
        MainWindow.Dispose();

        CommandManager.RemoveHandler(CommandName);
        provider.Dispose();
        
    }

    private void OnCommand(string command, string args)
    {
        // in response to the slash command, just toggle the display status of our main ui
        ToggleMainUI();
    }

    private void DrawUI() => WindowSystem.Draw();

    public void ToggleConfigUI() => ConfigWindow.Toggle();
    public void ToggleMainUI() => MainWindow.Toggle();
}
