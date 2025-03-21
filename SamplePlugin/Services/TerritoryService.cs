using System.Linq;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Lumina.Excel.Sheets;

namespace SamplePlugin.Services;

public class TerritoryService
{
    private readonly IDataManager dataManager;

    public TerritoryService(IDalamudPluginInterface pluginInterface, IDataManager dataManager)
    {
        this.dataManager = dataManager;
    }

    public TerritoryType? GetTerritoryForId(uint territoryId)
    {
        return dataManager.GameData.GetExcelSheet<TerritoryType>()
                          ?.FirstOrDefault((territory) => territory.RowId == territoryId);
    }
}
