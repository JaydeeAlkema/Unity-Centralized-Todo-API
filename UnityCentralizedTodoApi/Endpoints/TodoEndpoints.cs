using Microsoft.AspNetCore.Http.HttpResults;
using UnityCentralizedTodoApi.Contracts;
using UnityCentralizedTodoApi.Data;
using UnityCentralizedTodoApi.Domain;
using UnityCentralizedTodoApi.Validation;

namespace UnityCentralizedTodoApi.Endpoints;

public static class TodoEndpoints
{
    public static RouteGroupBuilder MapTodoEndpoints(this IEndpointRouteBuilder app)
    {
        var projectRoutes = app.MapGroup("/projects");

        projectRoutes.MapGet("", (ITodoRepository repository) =>
        {
            return TypedResults.Ok(repository.GetProjects());
        });

        var todoRoutes = projectRoutes
            .MapGroup("/{projectKey}/todos")
            .AddEndpointFilter<ProjectKeyRouteFilter>();

        todoRoutes.MapPost("", Results<Created<Todo>, BadRequest<ApiError>> (string projectKey, CreateTodoRequest request, ITodoRepository repository) =>
        {
            var createdTodo = repository.Create(
                projectKey: projectKey,
                name: request.Name.Trim(),
                severity: ParseSeverity(request.Severity),
                dueDate: request.DueDate,
                isComplete: request.IsComplete);

            return TypedResults.Created($"/projects/{createdTodo.ProjectKey}/todos/{createdTodo.Id}", createdTodo);
        })
        .AddEndpointFilter<TodoNameRequiredFilter<CreateTodoRequest>>()
        .AddEndpointFilter<TodoSeverityValidFilter<CreateTodoRequest>>()
        .AddEndpointFilter<TodoDueDateNotPastFilter<CreateTodoRequest>>();

        todoRoutes.MapGet("", Results<Ok<IReadOnlyList<Todo>>, BadRequest<ApiError>> ([AsParameters] TodoQueryRequest query, string projectKey, ITodoRepository repository) =>
        {
            var todoQuery = new TodoQuery(
                Severity: ParseOptionalSeverity(query.Severity),
                IsComplete: query.IsComplete,
                DueFrom: query.DueFrom?.ToUniversalTime(),
                DueTo: query.DueTo?.ToUniversalTime(),
                Search: string.IsNullOrWhiteSpace(query.Search) ? null : query.Search.Trim(),
                SortBy: TodoParsing.ParseSortByOrDefault(query.SortBy),
                Descending: query.Descending ?? false);

            return TypedResults.Ok(repository.Query(projectKey, todoQuery));
        })
        .AddEndpointFilter<TodoQueryValidationFilter>();

        todoRoutes.MapGet("/{id:int}", Results<Ok<Todo>, NotFound<ApiError>> (string projectKey, int id, ITodoRepository repository, HttpContext httpContext) =>
        {
            return repository.GetById(projectKey, id) is { } todo
                ? TypedResults.Ok(todo)
                : TypedResults.NotFound(ApiErrors.NotFound(
                    httpContext,
                    code: "resource.todo_not_found",
                    message: $"No todo was found with id '{id}'.",
                    field: "Id"));
        })
        .AddEndpointFilter<PositiveIdFilter>();

        todoRoutes.MapPut("/{id:int}", Results<Ok<Todo>, NotFound<ApiError>, BadRequest<ApiError>, Conflict<ApiError>> (string projectKey, int id, UpdateTodoRequest request, ITodoRepository repository, HttpContext httpContext) =>
        {
            var updateResult = repository.TryUpdate(
                projectKey: projectKey,
                id: id,
                name: request.Name.Trim(),
                severity: ParseSeverity(request.Severity),
                dueDate: request.DueDate,
                isComplete: request.IsComplete);

            return updateResult.Status switch
            {
                UpdateResultStatus.NotFound => TypedResults.NotFound(ApiErrors.NotFound(
                    httpContext,
                    code: "resource.todo_not_found",
                    message: $"No todo was found with id '{id}'.",
                    field: "Id")),
                UpdateResultStatus.Conflict => TypedResults.Conflict(ApiErrors.Conflict(
                    httpContext,
                    code: "conflict.concurrent_update",
                    message: "The todo was changed by another request. Please retry.",
                    field: "Id")),
                _ => TypedResults.Ok(updateResult.Todo!)
            };
        })
        .AddEndpointFilter<PositiveIdFilter>()
        .AddEndpointFilter<TodoNameRequiredFilter<UpdateTodoRequest>>()
        .AddEndpointFilter<TodoSeverityValidFilter<UpdateTodoRequest>>()
        .AddEndpointFilter<TodoDueDateNotPastFilter<UpdateTodoRequest>>();

        todoRoutes.MapDelete("/{id:int}", Results<NoContent, NotFound<ApiError>> (string projectKey, int id, ITodoRepository repository, HttpContext httpContext) =>
        {
            if (!repository.Delete(projectKey, id))
            {
                return TypedResults.NotFound(ApiErrors.NotFound(
                    httpContext,
                    code: "resource.todo_not_found",
                    message: $"No todo was found with id '{id}'.",
                    field: "Id"));
            }

            return TypedResults.NoContent();
        })
        .AddEndpointFilter<PositiveIdFilter>();

        return projectRoutes;
    }

    private static TodoSeverity ParseSeverity(string severity)
    {
        return TodoParsing.TryParseSeverity(severity, out var parsedSeverity)
            ? parsedSeverity
            : throw new InvalidOperationException("Severity should have been validated before parsing.");
    }

    private static TodoSeverity? ParseOptionalSeverity(string? severity)
    {
        return TodoParsing.TryParseSeverity(severity, out var parsedSeverity)
            ? parsedSeverity
            : null;
    }
}
