using System;
using System.Numerics;

namespace XivMate.DataGathering.Forays.Dalamud.Models;

public class Fate
{
    public uint Id { get; set; }
    public string Name { get; set; }
    public Vector3 Position { get; set; }
    public int StartedAt { get; set; }
    public long EndedAt { get; set; }
    public Guid InstanceId { get; set; }
}
