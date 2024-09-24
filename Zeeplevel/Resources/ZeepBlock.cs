using System.Globalization;
using TNRD.Zeepkist.GTR.Backend.Numerics;

namespace TNRD.Zeepkist.GTR.Backend.Zeeplevel.Resources;

public class ZeepBlock
{
    private static readonly CultureInfo Culture = new CultureInfo("en-US");

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
            $"Id: {Id.ToString(Culture)}, Position: {Position.ToString(string.Empty, Culture)}, Euler: {Euler.ToString(string.Empty, Culture)}, Scale: {Scale.ToString(string.Empty, Culture)}, Paints: {string.Join(", ", Paints.Select(x => x.ToString(Culture)))}, Options: {string.Join(", ", Options.Select(x => x.ToString(Culture)))}";
    }
}
