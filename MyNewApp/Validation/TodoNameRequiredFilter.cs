using MyNewApp.Contracts;

namespace MyNewApp.Validation;

public sealed class TodoNameRequiredFilter<TRequest> : IEndpointFilter where TRequest : class, ITodoWriteRequest
{
    public ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var request = context.Arguments.OfType<TRequest>().FirstOrDefault();

        if (request is null)
        {
            return next(context);
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return ValueTask.FromResult<object?>(TypedResults.BadRequest(ApiErrors.Validation(
                context.HttpContext,
                code: "validation.name_required",
                message: "The Name field is required and cannot be empty or whitespace.",
                field: "Name")));
        }

        return next(context);
    }
}