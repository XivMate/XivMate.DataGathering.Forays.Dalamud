using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Dalamud.Game.ClientState.Fates;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using SamplePlugin.Extensions;
using SamplePlugin.Services;

namespace SamplePlugin.Gathering.Fate;

public class FateModule : IModule
{
    private readonly IClientState clientState;
    private readonly IFateTable fateTable;
    private readonly IFramework framework;
    private readonly SchedulerService schedulerService;
    private readonly TerritoryService territoryService;
    private readonly IPluginLog log;
    private static ReaderWriterLock? RwLock = null!;
    private bool isInRecordableTerritory = false;

    private Dictionary<uint, Models.Fate> fates = new();

    public FateModule(
        IClientState clientState, IFateTable fateTable,
        IFramework framework, SchedulerService schedulerService,
        TerritoryService territoryService, IPluginLog log)
    {
        this.clientState = clientState;
        this.fateTable = fateTable;
        this.framework = framework;
        this.schedulerService = schedulerService;
        this.territoryService = territoryService;
        this.log = log;

        RwLock ??= new ReaderWriterLock();
    }


    public void Enable()
    {
        clientState.TerritoryChanged += OnTerritoryChanged;
        schedulerService.ScheduleOnFrameworkThread(FateTick, 5000);

        OnTerritoryChanged(clientState.TerritoryType);
    }

    private void FateTick()
    {
        if (clientState.LocalPlayer == null || !isInRecordableTerritory)
        {
            return;
        }


        foreach (var fate in fateTable)
        {
            Models.Fate? modelFate;
            if (fates.TryGetValue(fate.FateId, out modelFate))
            {
                // Update existing fate
                if (fate.State is FateState.Ended or FateState.Failed)
                {
                    fates.Remove(modelFate.Id);
                    modelFate.EndedAt = DateTime.UtcNow.ToUnixTime();
                    log.Info($"Fate ended: {fate.Name}, at: {modelFate.EndedAt}");
                }
            }
            else
            {
                // Add new fate
                log.Info($"New fate found: {fate.Name}, started at: {fate.StartTimeEpoch}");
                modelFate = new Models.Fate()
                {
                    Name = fate.Name.ToString(),
                    Id = fate.FateId,
                    Position = fate.Position,
                    StartedAt = fate.StartTimeEpoch,
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

        }
        // Alert removed any fates not in table
        //log.Error("Fate found in game FateTable but not local variable", fates);
    }

    public void Dispose()
    {
        clientState.TerritoryChanged -= OnTerritoryChanged;
        // Release any acquired reader locks
        try
        {
            if (RwLock is { IsReaderLockHeld: true })
            {
                RwLock.ReleaseLock();
            }
        }
        catch (ApplicationException)
        {
            // Handle case where the lock might be in an inconsistent state
        }

        // Release any acquired writer locks
        try
        {
            if (RwLock is { IsWriterLockHeld: true })
            {
                RwLock.ReleaseWriterLock();
            }
        }
        catch (ApplicationException)
        {
            // Handle case where the lock might be in an inconsistent state
        }

        // Allow the lock to be garbage collected
        RwLock = null;

        GC.SuppressFinalize(this);
    }


    private void OnTerritoryChanged(ushort obj)
    {
        var territory = territoryService.GetTerritoryForId(obj);
        if (territory?.PlaceName.Value.Name.ToString()?.Contains("Eureka") ?? false)
        {
            log.Info($"Eureka territory: {territory?.PlaceName.Value.Name}");
            isInRecordableTerritory = true;
        }
        else
        {
            if (territory != null) log.Info($"Not Eureka territory: {territory?.PlaceName.Value.Name}");
            isInRecordableTerritory = false;
        }
    }
}
