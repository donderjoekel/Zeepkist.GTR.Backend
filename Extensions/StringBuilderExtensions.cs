using System.Text;

namespace TNRD.Zeepkist.GTR.Backend.Extensions;

public static class StringBuilderExtensions
{
    public static StringBuilder AppendCLRF(this StringBuilder builder, string value)
    {
        builder.Append(value);
        builder.Append("\r\n");
        return builder;
    }
}