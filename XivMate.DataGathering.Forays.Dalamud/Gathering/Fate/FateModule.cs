using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Game.ClientState.Fates;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Newtonsoft.Json;
using XivMate.DataGathering.Forays.Dalamud.Extensions;
using XivMate.DataGathering.Forays.Dalamud.Services;

namespace XivMate.DataGathering.Forays.Dalamud.Gathering.Fate;

public class FateModule : IModule
{
    private readonly IClientState clientState;
    private readonly IFateTable fateTable;
    private readonly IFramework framework;
    private readonly SchedulerService schedulerService;
    private readonly TerritoryService territoryService;
    private readonly IPluginLog log;
    private readonly ApiService apiService;
    private bool isInRecordableTerritory = false;
    private Guid instanceGuid = Guid.NewGuid();
    private Dictionary<uint, Models.Fate> fates = new();
    private ConcurrentQueue<Models.Fate> fateQueue = new();

    public FateModule(
        IClientState clientState, IFateTable fateTable,
        IFramework framework, SchedulerService schedulerService,
        TerritoryService territoryService, IPluginLog log,
        ApiService apiService)
    {
        this.clientState = clientState;
        this.fateTable = fateTable;
        this.framework = framework;
        this.schedulerService = schedulerService;
        this.territoryService = territoryService;
        this.log = log;
        this.apiService = apiService;
    }

    private void FateTick()
    {
        if (clientState.LocalPlayer == null || !isInRecordableTerritory)
        {
            return;
        }


        try
        {
            foreach (var fate in fateTable)
            {
                if (fates.TryGetValue(fate.FateId, out var modelFate))
                {
                    // Update existing fate
                    if (fate.State is FateState.Ended or FateState.Failed)
                    {
                        fates.Remove(modelFate.Id);
                        modelFate.EndedAt = DateTime.UtcNow.ToUnixTime();
                        fateQueue.Enqueue(modelFate);
                        log.Info($"Fate ended: {fate.Name}, at: {modelFate.EndedAt}");
                        log.Info(JsonConvert.SerializeObject(modelFate));
                    }
                }
                else if (fate.State is FateState.Running)
                {
                    // Add new fate
                    log.Info(
                        $"New fate found: {fate.Name}, started at: {fate.StartTimeEpoch}, position: {fate.Position}, status: {fate.State.ToString()}");
                    modelFate = new Models.Fate()
                    {
                        Name = fate.Name.ToString(),
                        Id = fate.FateId,
                        Position = fate.Position,
                        StartedAt = fate.StartTimeEpoch,
                        InstanceId = instanceGuid,
                    };

                    fates.Add(fate.FateId, modelFate);
                }
            }

            var removedFates = fates.Where(recordedFate =>
                                               fateTable.All(existingFate => existingFate.FateId != recordedFate.Key));
            foreach (var removedFate in removedFates)
            {
                fates.Remove(removedFate.Value.Id);
                removedFate.Value.EndedAt = DateTime.UtcNow.ToUnixTime();
                log.Info($"Fate removed: {removedFate.Value.Name}, at: {removedFate.Value.EndedAt}");
                log.Info(JsonConvert.SerializeObject(removedFate.Value));
                fateQueue.Enqueue(removedFate.Value);
            }
        }
        catch (Exception)
        {
            throw;
        }
    }

    public void Dispose()
    {
        clientState.TerritoryChanged -= OnTerritoryChanged;
        schedulerService.CancelScheduledTask(FateTick);
        schedulerService.CancelScheduledTask(FateUpload);
        GC.SuppressFinalize(this);
    }


    private void OnTerritoryChanged(ushort obj)
    {
        var territory = territoryService.GetTerritoryForId(obj);
        if (territory.PlaceName.Value.Name.ToString().Contains("Eureka") ||
            territory.PlaceName.Value.Name.ToString().Contains("Zadnor") ||
            territory.PlaceName.Value.Name.ToString().Contains("Bozjan Southern Front"))
        {
            isInRecordableTerritory = true;
            instanceGuid = Guid.NewGuid();
            log.Info($"Foray territory: {territory.PlaceName.Value.Name}, local guid: {instanceGuid}");
        }
        else
        {
            log.Info($"Not Foray territory: {territory.PlaceName.Value.Name}");
            isInRecordableTerritory = false;
        }
    }

    public bool Enabled { get; private set; } = false;
    public IEnumerable<Models.Fate> ActiveFates => fates.Values.ToList();

    public void LoadConfig(Configuration configuration)
    {
        if (configuration.FateConfiguration.Enabled && !Enabled)
        {
            clientState.TerritoryChanged += OnTerritoryChanged;
            schedulerService.ScheduleOnFrameworkThread(FateTick, 500);
            schedulerService.ScheduleOnNewThread(FateUpload, 2500);
            OnTerritoryChanged(clientState.TerritoryType);
            Enabled = true;
            fates.Clear();
        }
        else if (Enabled && !configuration.FateConfiguration.Enabled)
        {
            clientState.TerritoryChanged -= OnTerritoryChanged;
            schedulerService.CancelScheduledTask(FateTick);
            schedulerService.CancelScheduledTask(FateUpload);

            Enabled = false;
        }
    }

    private void FateUpload()
    {
        if (fateQueue.TryDequeue(out var fate))
        {
            try
            {
                apiService.UploadFate(fate).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                fateQueue.Enqueue(fate);
                log.Warning($"Error uploading {fate.Name}: {ex.Message}");
            }
        }
    }
}
