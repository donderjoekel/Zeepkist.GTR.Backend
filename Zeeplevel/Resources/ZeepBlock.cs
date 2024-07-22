using System.Numerics;

namespace TNRD.Zeepkist.GTR.Backend.Zeeplevel.Resources;

public class ZeepBlock
{
    public ZeepBlock()
    {
        Paints = new List<int>();
        Options = new List<float>();
    }

    public int Id { get; set; }
    public Vector3 Position { get; set; }
    public Vector3 Euler { get; set; }
    public Vector3 Scale { get; set; }
    public List<int> Paints { get; private set; }
    public List<float> Options { get; private set; }

    public override string ToString()
    {
        return
            $"Id: {Id}, Position: {Position}, Euler: {Euler}, Scale: {Scale}, Paints: {string.Join(", ", Paints)}, Options: {string.Join(", ", Options)}";
    }
}
