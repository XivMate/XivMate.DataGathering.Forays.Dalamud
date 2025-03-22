using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Game.ClientState.Fates;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Newtonsoft.Json;
using XivMate.DataGathering.Forays.Dalamud.Extensions;
using XivMate.DataGathering.Forays.Dalamud.Services;

namespace XivMate.DataGathering.Forays.Dalamud.Gathering.Fate;

public class FateModule(
    IClientState clientState,
    IFateTable fateTable,
    IFramework framework,
    SchedulerService schedulerService,
    TerritoryService territoryService,
    IPluginLog log,
    ApiService apiService)
    : IModule
{
    private readonly IFramework framework = framework;
    private bool isInRecordableTerritory = false;
    private Guid instanceGuid = Guid.NewGuid();
    private Dictionary<uint, Models.Fate> fates = new();
    private ConcurrentQueue<Models.Fate> fateQueue = new();

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
                    if (fate.State is FateState.Ended or FateState.Failed)
                    {
                        RemoveFate(modelFate);
                    }
                }
                else if (fate.State is FateState.Running && fate.StartTimeEpoch > 0 && fate.Position != Vector3.Zero)
                {
                    // Add new fate
                    log.Info(
                        $"New fate found: {fate.Name}, started at: {fate.StartTimeEpoch}, position: {fate.Position}, status: {fate.State.ToString()}");
                    modelFate = new Models.Fate()
                    {
                        Name = fate.Name.ToString(),
                        FateId = fate.FateId,
                        X = fate.Position.X,
                        Y = fate.Position.X,
                        Z = fate.Position.X,
                        Radius = fate.Radius,
                        StartedAt = fate.StartTimeEpoch,
                        InstanceId = instanceGuid,
                        TerritoryType = clientState.TerritoryType
                    };

                    fates.Add(fate.FateId, modelFate);
                }
            }

            var removedFates = fates.Where(recordedFate =>
                                               fateTable.All(existingFate => existingFate.FateId != recordedFate.Key));
            foreach (var removedFate in removedFates)
            {
                RemoveFate(removedFate.Value);
            }
        }
        catch (Exception)
        {
            throw;
        }
    }

    private void RemoveFate(Models.Fate modelFate)
    {
        fates.Remove(modelFate.FateId);
        modelFate.EndedAt = DateTime.UtcNow.ToUnixTime();
        if (modelFate.TerritoryType == clientState.TerritoryType)
            fateQueue.Enqueue(modelFate);
        log.Info($"Fate ended: {modelFate.Name}, at: {modelFate.EndedAt}");
        log.Info(
            $"#1 - Fate ended territoryid {modelFate.TerritoryType}, client territory: {clientState.TerritoryType}, client map: {clientState.MapId}");
        log.Info(JsonConvert.SerializeObject(modelFate));
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
        if (territory.HasValue && (territory.Value.PlaceName.Value.Name.ToString().Contains("Eureka") ||
                                   territory.Value.PlaceName.Value.Name.ToString().Contains("Zadnor") ||
                                   territory.Value.PlaceName.Value.Name.ToString().Contains("Bozjan Southern Front")))
        {
            isInRecordableTerritory = true;
            instanceGuid = Guid.NewGuid();
            log.Info($"Foray territory: {territory.Value.PlaceName.Value.Name}, local guid: {instanceGuid}, local territory {clientState.TerritoryType}, map {clientState.MapId}");
        }
        else
        {
            if (territory.HasValue)
                log.Info($"Not Foray territory: {territory.Value.PlaceName.Value.Name}");
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
