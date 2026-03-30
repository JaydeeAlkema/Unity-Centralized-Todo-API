using MyNewApp.Contracts;

namespace MyNewApp.Validation;

public sealed class TodoDueDateNotPastFilter<TRequest> : IEndpointFilter where TRequest : class, ITodoWriteRequest
{
    public ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var request = context.Arguments.OfType<TRequest>().FirstOrDefault();

        if (request is null)
        {
            return next(context);
        }

        if (request.DueDate.ToUniversalTime() < DateTime.UtcNow)
        {
            var apiError = ApiErrors.Validation(
                context.HttpContext,
                code: "validation.due_date_past",
                message: "The DueDate cannot be in the past.",
                field: "DueDate");
            var result = TypedResults.BadRequest(apiError);
            
            return ValueTask.FromResult<object?>(result);
        }

        return next(context);
    }
}