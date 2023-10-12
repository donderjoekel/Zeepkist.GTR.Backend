using FastEndpoints;
using FluentValidation.Results;
using TNRD.Zeepkist.GTR.DTOs.RequestDTOs;

namespace TNRD.Zeepkist.GTR.Backend.PreProcessors;

public class GenericGetRequestPreProcessor : IGlobalPreProcessor
{
    public Task PreProcessAsync(object req, HttpContext ctx, List<ValidationFailure> failures, CancellationToken ct)
    {
        if (req is GenericGetRequestDTO genericGetRequest)
        {
            if (genericGetRequest.Limit.HasValue && genericGetRequest.Limit > 100)
            {
                genericGetRequest.Limit = 100;
            }
            else if (!genericGetRequest.Limit.HasValue)
            {
                genericGetRequest.Limit = 100;
            }

            if (genericGetRequest.Offset.HasValue && genericGetRequest.Offset < 0)
            {
                genericGetRequest.Offset = 0;
            }
            else if (!genericGetRequest.Offset.HasValue)
            {
                genericGetRequest.Offset = 0;
            }
        }

        return Task.CompletedTask;
    }
}
