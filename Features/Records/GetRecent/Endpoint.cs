using FastEndpoints;
using FluentResults;
using TNRD.Zeepkist.GTR.Backend.Directus;
using TNRD.Zeepkist.GTR.Backend.Directus.Api;
using TNRD.Zeepkist.GTR.DTOs.Internal.Models;
using TNRD.Zeepkist.GTR.DTOs.RequestDTOs;
using TNRD.Zeepkist.GTR.DTOs.ResponseDTOs;
using TNRD.Zeepkist.GTR.DTOs.ResponseModels;

namespace TNRD.Zeepkist.GTR.Backend.Features.Records.GetRecent;

internal class Endpoint : Endpoint<GenericGetRequestDTO, RecordsGetRecentResponseDTO>
{
    private readonly IDirectusClient client;

    /// <inheritdoc />
    public Endpoint(IDirectusClient client)
    {
        this.client = client;
    }

    /// <inheritdoc />
    public override void Configure()
    {
        AllowAnonymous();
        Get("records/recent");
        Description(b => { b.Produces<RecordsGetRecentResponseDTO>(); });
    }

    /// <inheritdoc />
    public override async Task HandleAsync(GenericGetRequestDTO req, CancellationToken ct)
    {
        RecordsApi api = new RecordsApi(client);

        Result<DirectusGetMultipleResponse<RecordModel>> getResult = await api.Get(
            filter =>
            {
                filter
                    .WithLimit(req.Limit)
                    .WithOffset(req.Offset)
                    .WithSort("-date_created");
            },
            ct);

        if (getResult.IsFailed)
        {
            Logger.LogCritical("Unable to get records: {Result}", getResult.ToString());
            ThrowError("Unable to get records");
        }

        List<RecordResponseModel> records = new();

        foreach (RecordModel model in getResult.Value.Data)
        {
            records.Add(model);
        }

        await SendOkAsync(new RecordsGetRecentResponseDTO()
            {
                Records = records,
            },
            ct);
    }
}
