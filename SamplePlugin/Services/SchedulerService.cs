using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Plugin.Services;

namespace SamplePlugin.Services;

public class SchedulerService(IFramework framework, IPluginLog log) : IDisposable
{
    private readonly Dictionary<Action, ThreadLoopData> threads = new();

    public void ScheduleOnFrameworkThread(Action action, int intervalMs)
    {
        if (threads.ContainsKey(action)) throw new Exception("Thread already exists");

        var threadLoop = new ThreadLoop();
        threadLoop.Start(() => { framework.RunOnFrameworkThread(action); }, intervalMs);
        log.Info("Started threadloop for " + action.Method.Name);
        threads.Add(action, new ThreadLoopData()
        {
            ThreadLoop = threadLoop
        });
    }

    public void Dispose()
    {
        // TODO release managed resources here
        foreach (var threadsValue in threads.Values)
        {
            threadsValue?.ThreadLoop?.Stop();
        }

        threads.Clear();
        GC.SuppressFinalize(this);
    }
}

internal class ThreadLoopData
{
    public required ThreadLoop ThreadLoop { get; init; }
}

internal class ThreadLoop
{
    private Task task = null!;
    private readonly CancellationTokenSource cancellationTokenSource = new();

    public void Start(Action action, int interval)
    {
        task = Task.Factory.StartNew(action: () =>
        {
            while (!cancellationTokenSource.IsCancellationRequested)
            {
                try
                {
                    if (cancellationTokenSource.IsCancellationRequested)
                        break;
                    action.Invoke();
                    cancellationTokenSource.Token.WaitHandle.WaitOne(interval);
                }
                catch
                {
                    cancellationTokenSource.Token.WaitHandle.WaitOne(interval);
                }
            }
        }, cancellationTokenSource.Token);
    }


    public void Stop()
    {
        cancellationTokenSource.Cancel();
    }
}
