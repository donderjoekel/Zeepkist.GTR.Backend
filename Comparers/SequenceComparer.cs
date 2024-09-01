using System.Numerics;

namespace TNRD.Zeepkist.GTR.Backend.Comparers;

public class SequenceComparer<T> : IComparer<List<T>>
    where T : INumber<T>
{
    public int Compare(List<T>? x, List<T>? y)
    {
        if (x == null && y == null)
            return 0;

        if (x == null)
            return -1;

        if (y == null)
            return 1;

        int countComparison = x.Count.CompareTo(y.Count);
        if (countComparison != 0)
            return countComparison;

        for (int i = 0; i < x.Count; i++)
        {
            int elementComparison = x[i].CompareTo(y[i]);
            if (elementComparison != 0)
                return elementComparison;
        }

        return 0;
    }
}
