using FastEndpoints;
using TNRD.Zeepkist.GTR.DTOs.ResponseDTOs;

namespace TNRD.Zeepkist.GTR.Backend.Features.Votes.Categories;

internal class Endpoint : EndpointWithoutRequest<VotesGetCategoriesResponseDTO>
{
    /// <inheritdoc />
    public override void Configure()
    {
        AllowAnonymous();
        Get("votes/categories");
    }

    /// <inheritdoc />
    public override async Task HandleAsync(CancellationToken ct)
    {
        VotesGetCategoriesResponseDTO responseModel = new()
        {
            Categories = new List<VotesGetCategoriesResponseDTO.Category>()
            {
                new()
                {
                    DisplayName = "General",
                    Value = 0
                },
                new()
                {
                    DisplayName = "Flow",
                    Value = 1
                },
                new()
                {
                    DisplayName = "Difficulty",
                    Value = 2
                },
                new()
                {
                    DisplayName = "Scenery",
                    Value = 3
                }
            }
        };

        await SendAsync(responseModel, cancellation: ct);
    }
}
