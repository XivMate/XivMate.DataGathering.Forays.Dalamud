using System.Numerics;

namespace SamplePlugin.Models;

public class Fate
{
    public uint Id { get; set; }
    public string Name { get; set; }
    public Vector3 Position { get; set; }
    public int StartedAt { get; set; }
    public long EndedAt { get; set; }
}
