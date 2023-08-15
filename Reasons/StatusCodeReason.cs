﻿using System.Net;
using FluentResults;

namespace TNRD.Zeepkist.GTR.Backend.Reasons;

internal class StatusCodeReason : IReason
{
    /// <inheritdoc />
    string? IReason.Message { get; }

    /// <inheritdoc />
    Dictionary<string, object>? IReason.Metadata { get; }
    
    public HttpStatusCode StatusCode { get; }

    public StatusCodeReason(HttpStatusCode statusCode)
    {
        StatusCode = statusCode;
    }
}
