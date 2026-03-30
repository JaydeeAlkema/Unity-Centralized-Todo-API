using UnityCentralizedTodoApi.Contracts;
using UnityCentralizedTodoApi.Domain;

namespace UnityCentralizedTodoApi.Validation;

public sealed class TodoQueryValidationFilter : IEndpointFilter
{
    public ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var query = context.Arguments.OfType<TodoQueryRequest>().FirstOrDefault();

        if (query is null)
        {
            return next(context);
        }

        if (!string.IsNullOrWhiteSpace(query.Severity) && !TodoParsing.TryParseSeverity(query.Severity, out _))
        {
            return ValueTask.FromResult<object?>(TypedResults.BadRequest(ApiErrors.Validation(
                context.HttpContext,
                code: "validation.severity_invalid",
                message: "Severity must be one of: low, medium, high, critical.",
                field: "Severity")));
        }

        if (!string.IsNullOrWhiteSpace(query.SortBy) && !TodoParsing.TryParseSortBy(query.SortBy, out _))
        {
            return ValueTask.FromResult<object?>(TypedResults.BadRequest(ApiErrors.Validation(
                context.HttpContext,
                code: "validation.sort_by_invalid",
                message: "SortBy must be one of: dueDate, severity, name, createdAt, updatedAt.",
                field: "SortBy")));
        }

        if (query.DueFrom is { } dueFrom && query.DueTo is { } dueTo && dueFrom.ToUniversalTime() > dueTo.ToUniversalTime())
        {
            return ValueTask.FromResult<object?>(TypedResults.BadRequest(ApiErrors.Validation(
                context.HttpContext,
                code: "validation.due_range_invalid",
                message: "DueFrom cannot be later than DueTo.",
                field: "DueFrom")));
        }

        return next(context);
    }
}