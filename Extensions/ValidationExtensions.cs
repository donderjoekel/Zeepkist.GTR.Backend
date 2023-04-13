using FluentValidation.Results;

namespace TNRD.Zeepkist.GTR.Backend.Extensions;

internal class ValidationResponse
{
    public string Property { get; init; } = default!;
    public string Message { get; init; } = default!;
}

internal static class ValidationExtensions
{
    public static List<ValidationResponse> ToResponse(this IEnumerable<ValidationFailure> errors)
    {
        var list = new List<ValidationResponse>();

        foreach (var error in errors)
        {
            list.Add(new ValidationResponse
            {
                Property = error.PropertyName,
                Message = error.ErrorMessage
            });
        }

        return list;
    }
}
