using MyNewApp.Contracts;

namespace MyNewApp.Validation;

public sealed class PositiveIdFilter : IEndpointFilter
{
    public ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var hasId = context.Arguments.OfType<int>().FirstOrDefault();

        if (hasId <= 0)
        {
            return ValueTask.FromResult<object?>(TypedResults.BadRequest(ApiErrors.Validation(
                context.HttpContext,
                code: "validation.id_non_positive",
                message: "The id must be greater than 0.",
                field: "Id")));
        }

        return next(context);
    }
}