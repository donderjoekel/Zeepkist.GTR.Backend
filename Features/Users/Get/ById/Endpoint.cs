using FastEndpoints;
using FluentResults;
using TNRD.Zeepkist.GTR.Backend.Directus;
using TNRD.Zeepkist.GTR.Backend.Directus.Api;
using TNRD.Zeepkist.GTR.DTOs.Internal.Models;
using TNRD.Zeepkist.GTR.DTOs.RequestDTOs;
using TNRD.Zeepkist.GTR.DTOs.ResponseModels;

namespace TNRD.Zeepkist.GTR.Backend.Features.Users.Get.ById;

internal class Endpoint : Endpoint<GenericIdRequestDTO, UserResponseModel>
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
        Get("users/{Id}");
    }

    /// <inheritdoc />
    public override async Task HandleAsync(GenericIdRequestDTO req, CancellationToken ct)
    {
        UsersApi usersApi = new UsersApi(client);

        Result<UserModel?> getResult = await usersApi.GetById(req.Id, ct);

        if (getResult.IsFailed)
        {
            Logger.LogCritical("Unable to check if user exists: {Result}", getResult.ToString());
            ThrowError("Unable to check if user exists");
        }

        if (getResult.Value != null)
        {
            await SendOkAsync(getResult.Value, ct);
        }
        else
        {
            await SendNotFoundAsync(ct);
        }
    }
}
