using FastEndpoints;
using FluentResults;
using TNRD.Zeepkist.GTR.Backend.Directus;
using TNRD.Zeepkist.GTR.Backend.Directus.Api;
using TNRD.Zeepkist.GTR.DTOs.Internal.Models;
using TNRD.Zeepkist.GTR.DTOs.RequestDTOs;
using TNRD.Zeepkist.GTR.DTOs.ResponseDTOs;
using TNRD.Zeepkist.GTR.DTOs.ResponseModels;

namespace TNRD.Zeepkist.GTR.Backend.Features.Users.Get.All;

internal class Endpoint : Endpoint<GenericGetRequestDTO, UsersGetAllResponseDTO>
{
    private readonly IDirectusClient client;

    public Endpoint(IDirectusClient client)
    {
        this.client = client;
    }

    /// <inheritdoc />
    public override void Configure()
    {
        AllowAnonymous();
        Get("users");
    }

    /// <inheritdoc />
    public override async Task HandleAsync(GenericGetRequestDTO req, CancellationToken ct)
    {
        UsersApi usersApi = new UsersApi(client);
        Result<DirectusGetMultipleResponse<UserModel>> getResult = await usersApi.Get(builder =>
            {
                if (req.Limit.HasValue)
                    builder.WithLimit(req.Limit.Value);
                if (req.Offset.HasValue)
                    builder.WithOffset(req.Offset.Value);
            },
            ct);

        if (getResult.IsFailed)
        {
            Logger.LogCritical("Unable to check if user exists: {Result}", getResult.ToString());
            await SendAsync(null!, 500, ct);
            return;
        }

        List<UserResponseModel> users = new();

        foreach (UserModel item in getResult.Value.Data)
        {
            users.Add(item);
        }

        UsersGetAllResponseDTO usersGetAllResponseDTO = new UsersGetAllResponseDTO()
        {
            TotalAmount = getResult.Value.Metadata!.FilterCount!.Value,
            Users = users
        };

        await SendAsync(usersGetAllResponseDTO, cancellation: ct);
    }
}
