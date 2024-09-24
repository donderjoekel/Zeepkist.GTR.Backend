namespace TNRD.Zeepkist.GTR.Backend.Numerics;

public struct Vector3
{
    public readonly decimal X;
    public readonly decimal Y;
    public readonly decimal Z;

    public Vector3(decimal x, decimal y, decimal z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public string ToString(string format, IFormatProvider formatProvider)
    {
        return
            $"<{X.ToString(format, formatProvider)},{Y.ToString(format, formatProvider)},{Z.ToString(format, formatProvider)}>";
    }
}
