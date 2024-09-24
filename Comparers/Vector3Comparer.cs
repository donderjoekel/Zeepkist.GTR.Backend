using TNRD.Zeepkist.GTR.Backend.Numerics;

namespace TNRD.Zeepkist.GTR.Backend.Comparers;

public class Vector3Comparer : IComparer<Vector3>
{
    public int Compare(Vector3 v1, Vector3 v2)
    {
        int xComparison = v1.X.CompareTo(v2.X);
        if (xComparison != 0)
            return xComparison;

        int yComparison = v1.Y.CompareTo(v2.Y);
        if (yComparison != 0)
            return yComparison;

        return v1.Z.CompareTo(v2.Z);
    }
}
