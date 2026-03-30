using Microsoft.AspNetCore.Http.HttpResults;
using MyNewApp.Contracts;
using MyNewApp.Data;
using MyNewApp.Domain;
using MyNewApp.Validation;

namespace MyNewApp.Endpoints;

public static class TodoEndpoints
{
    public static RouteGroupBuilder MapTodoEndpoints(this IEndpointRouteBuilder app)
    {
        var routes = app.MapGroup("/todos");

        routes.MapPost("", Results<Created<Todo>, BadRequest<ApiError>> (CreateTodoRequest request, TodoStore store) =>
        {
            var createdTodo = store.Create(
                name: request.Name.Trim(),
                dueDate: request.DueDate,
                isComplete: request.IsComplete);

            return TypedResults.Created($"/todos/{createdTodo.Id}", createdTodo);
        })
        .AddEndpointFilter<TodoNameRequiredFilter<CreateTodoRequest>>()
        .AddEndpointFilter<TodoDueDateNotPastFilter<CreateTodoRequest>>();

        routes.MapGet("", (TodoStore store) =>
        {
            return TypedResults.Ok(store.GetAll());
        });

        routes.MapGet("/{id:int}", Results<Ok<Todo>, NotFound<ApiError>> (int id, TodoStore store, HttpContext httpContext) =>
        {
            return store.GetById(id) is { } todo
                ? TypedResults.Ok(todo)
                : TypedResults.NotFound(ApiErrors.NotFound(
                    httpContext,
                    code: "resource.todo_not_found",
                    message: $"No todo was found with id '{id}'.",
                    field: "Id"));
        })
        .AddEndpointFilter<PositiveIdFilter>();

        routes.MapPut("/{id:int}", Results<Ok<Todo>, NotFound<ApiError>, BadRequest<ApiError>, Conflict<ApiError>> (int id, UpdateTodoRequest request, TodoStore store, HttpContext httpContext) =>
        {
            var updateResult = store.TryUpdate(
                id: id,
                name: request.Name.Trim(),
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
        .AddEndpointFilter<TodoDueDateNotPastFilter<UpdateTodoRequest>>();

        routes.MapDelete("/{id:int}", Results<NoContent, NotFound<ApiError>> (int id, TodoStore store, HttpContext httpContext) =>
        {
            if (!store.Delete(id))
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

        return routes;
    }
}
