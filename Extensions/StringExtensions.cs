using NJsonSchema.Annotations;

namespace TNRD.Zeepkist.GTR.Backend.Extensions;

internal static class StringExtensions
{
    public static bool HasValue(this string? str)
    {
        return !string.IsNullOrEmpty(str);
    }
}
