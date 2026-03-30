using UnityCentralizedTodoApi.Contracts;
using UnityCentralizedTodoApi.Domain;

namespace UnityCentralizedTodoApi.Validation;

public sealed class TodoSeverityValidFilter<TRequest> : IEndpointFilter where TRequest : class, ITodoWriteRequest
{
    public ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var request = context.Arguments.OfType<TRequest>().FirstOrDefault();

        if (request is null)
        {
            return next(context);
        }

        if (!TodoParsing.TryParseSeverity(request.Severity, out _))
        {
            return ValueTask.FromResult<object?>(TypedResults.BadRequest(ApiErrors.Validation(
                context.HttpContext,
                code: "validation.severity_invalid",
                message: "Severity must be one of: low, medium, high, critical.",
                field: "Severity")));
        }

        return next(context);
    }
}