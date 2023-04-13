using FluentResults;

namespace TNRD.Zeepkist.GTR.Backend.Extensions;

public static class ResultExtensions
{
    public static bool TryGetReason<T>(this IResultBase resultBase, out T reason)
        where T : IReason
    {
        reason = default!;

        foreach (IReason r in resultBase.Reasons)
        {
            if (r is T converted)
            {
                reason = converted;
                return true;
            }
        }

        return false;
    }
}
