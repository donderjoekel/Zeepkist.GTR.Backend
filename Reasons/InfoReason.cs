using FluentResults;

namespace TNRD.Zeepkist.GTR.Backend.Reasons;

internal class InfoReason : IReason
{
    public InfoReason(string message)
    {
        Message = message;
        Metadata = new Dictionary<string, object>();
    }

    /// <inheritdoc />
    public string Message { get; }

    /// <inheritdoc />
    public Dictionary<string, object> Metadata { get; }
}
