namespace TNRD.Zeepkist.GTR.Backend.Numerics;

public readonly struct Vector2
{
    public readonly decimal X;
    public readonly decimal Y;

    public Vector2(decimal x, decimal y)
    {
        X = x;
        Y = y;
    }

    public string ToString(string format, IFormatProvider formatProvider)
    {
        return
            $"<{X.ToString(format, formatProvider)},{Y.ToString(format, formatProvider)}>";
    }
}
