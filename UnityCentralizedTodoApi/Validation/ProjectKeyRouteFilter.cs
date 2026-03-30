using UnityCentralizedTodoApi.Contracts;
using UnityCentralizedTodoApi.Domain;

namespace UnityCentralizedTodoApi.Validation;

public sealed class ProjectKeyRouteFilter : IEndpointFilter
{
    public ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var routeProjectKey = context.HttpContext.Request.RouteValues["projectKey"]?.ToString();

        if (ProjectKey.TryNormalize(routeProjectKey, out _))
        {
            return next(context);
        }

        return ValueTask.FromResult<object?>(TypedResults.BadRequest(ApiErrors.Validation(
            context.HttpContext,
            code: "validation.project_key_invalid",
            message: "The projectKey route value is required and may only contain letters, numbers, hyphens, or underscores.",
            field: "ProjectKey")));
    }
}