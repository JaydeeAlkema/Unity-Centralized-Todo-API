namespace UnityCentralizedTodoApi.Contracts;

public sealed record ApiError(
    string Code,
    string Message,
    string? Field,
    int Status,
    string TraceId);

public static class ApiErrors
{
    public static ApiError Validation(HttpContext context, string code, string message, string field)
    {
        return new ApiError(
            Code: code,
            Message: message,
            Field: field,
            Status: StatusCodes.Status400BadRequest,
            TraceId: context.TraceIdentifier);
    }

    public static ApiError NotFound(HttpContext context, string code, string message, string? field = null)
    {
        return new ApiError(
            Code: code,
            Message: message,
            Field: field,
            Status: StatusCodes.Status404NotFound,
            TraceId: context.TraceIdentifier);
    }

    public static ApiError Conflict(HttpContext context, string code, string message, string? field = null)
    {
        return new ApiError(
            Code: code,
            Message: message,
            Field: field,
            Status: StatusCodes.Status409Conflict,
            TraceId: context.TraceIdentifier);
    }
}