using System;

namespace XivMate.DataGathering.Forays.Dalamud.Gathering;

public interface IModule : IDisposable
{
    void Enable();
}
