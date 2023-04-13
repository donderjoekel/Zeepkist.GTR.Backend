using FastEndpoints;
using FluentResults;
using TNRD.Zeepkist.GTR.Backend.Directus;
using TNRD.Zeepkist.GTR.Backend.Directus.Api;
using TNRD.Zeepkist.GTR.DTOs.Internal.Models;
using TNRD.Zeepkist.GTR.DTOs.RequestDTOs;
using TNRD.Zeepkist.GTR.DTOs.ResponseDTOs;
using TNRD.Zeepkist.GTR.DTOs.ResponseModels;

namespace TNRD.Zeepkist.GTR.Backend.Features.Votes.Get.All;

internal class Endpoint : Endpoint<VotesGetRequestDTO, VotesGetResponseDTO>
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
        Get("votes");
    }

    /// <inheritdoc />
    public override async Task HandleAsync(VotesGetRequestDTO req, CancellationToken ct)
    {
        VotesApi api = new VotesApi(client);

        Result<DirectusGetMultipleResponse<VoteModel>> getResult = await api.Get(filter =>
            {
                filter.WithUser(req.UserId)
                    .WithSteamId(req.UserSteamId)
                    .WithLevel(req.LevelId)
                    .WithUid(req.LevelUid)
                    .WithWorkshopId(req.LevelWorkshopId)
                    .WithOffset(req.Offset)
                    .WithLimit(req.Limit);
            },
            ct);

        if (getResult.IsFailed)
        {
            Logger.LogCritical("Unable to get votes: {Result}", getResult.ToString());
            ThrowError("Unable to get votes");
        }

        List<VoteResponseModel> votes = new();

        foreach (VoteModel model in getResult.Value.Data)
        {
            votes.Add(model);
        }

        VotesGetResponseDTO responseModel = new VotesGetResponseDTO()
        {
            Votes = votes,
            TotalAmount = getResult.Value.Metadata!.FilterCount!.Value
        };

        await SendAsync(responseModel, cancellation: ct);
    }
}
