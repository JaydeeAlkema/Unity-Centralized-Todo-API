using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using UnityCentralizedTodoApi.Domain;
using UnityCentralizedTodoApi.Tests.Helpers;

namespace UnityCentralizedTodoApi.Tests.Integration;

public class TodoCrudTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private const string Project = "crud-tests";
    private const string Base = $"/projects/{Project}/todos";

    public TodoCrudTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    // -----------------------------------------------------------------------
    // POST (Create)
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Post_ValidTodo_Returns201WithLocation()
    {
        var response = await _client.PostAsJsonAsync(Base, new
        {
            name = "Write unit tests",
            severity = "high",
            dueDate = "2026-06-01T00:00:00Z",
            isComplete = false
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        Assert.Contains("/todos/", response.Headers.Location!.ToString());
    }

    [Fact]
    public async Task Post_ValidTodo_ResponseBodyContainsAllFields()
    {
        var response = await _client.PostAsJsonAsync(Base, new
        {
            name = "Check response",
            severity = "medium",
            dueDate = "2026-07-01T00:00:00Z",
            isComplete = false
        });

        var todo = await response.Content.ReadFromJsonAsync<Todo>(TestJsonOptions.Api);
        Assert.NotNull(todo);
        Assert.True(todo!.Id > 0);
        Assert.Equal(Project, todo.ProjectKey);
        Assert.Equal("Check response", todo.Name);
        Assert.Equal(TodoSeverity.Medium, todo.Severity);
        Assert.False(todo.IsComplete);
        Assert.NotEqual(default, todo.CreatedAt);
        Assert.NotEqual(default, todo.UpdatedAt);
    }

    // -----------------------------------------------------------------------
    // GET (Read)
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetById_ExistingTodo_Returns200()
    {
        var created = await CreateTodoAsync("Get by id test");

        var response = await _client.GetAsync($"{Base}/{created.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var todo = await response.Content.ReadFromJsonAsync<Todo>(TestJsonOptions.Api);
        Assert.Equal(created.Id, todo!.Id);
    }

    [Fact]
    public async Task GetById_MissingTodo_Returns404()
    {
        var response = await _client.GetAsync($"{Base}/99999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_ReturnsList()
    {
        await CreateTodoAsync("List item A");
        await CreateTodoAsync("List item B");

        var response = await _client.GetAsync(Base);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var todos = await response.Content.ReadFromJsonAsync<List<Todo>>(TestJsonOptions.Api);
        Assert.NotNull(todos);
        Assert.NotEmpty(todos);
    }

    // -----------------------------------------------------------------------
    // PUT (Update)
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Put_ExistingTodo_Returns200WithUpdatedValues()
    {
        var created = await CreateTodoAsync("Before update");

        var response = await _client.PutAsJsonAsync($"{Base}/{created.Id}", new
        {
            name = "After update",
            severity = "critical",
            dueDate = "2026-08-01T00:00:00Z",
            isComplete = true
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await response.Content.ReadFromJsonAsync<Todo>(TestJsonOptions.Api);
        Assert.Equal("After update", updated!.Name);
        Assert.Equal(TodoSeverity.Critical, updated.Severity);
        Assert.True(updated.IsComplete);
    }

    [Fact]
    public async Task Put_MissingTodo_Returns404()
    {
        var response = await _client.PutAsJsonAsync($"{Base}/99999", new
        {
            name = "No such todo",
            severity = "low",
            dueDate = "2026-08-01T00:00:00Z",
            isComplete = false
        });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // -----------------------------------------------------------------------
    // DELETE
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Delete_ExistingTodo_Returns204()
    {
        var created = await CreateTodoAsync("To be deleted");

        var response = await _client.DeleteAsync($"{Base}/{created.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Delete_Then_GetById_Returns404()
    {
        var created = await CreateTodoAsync("Delete then check");

        await _client.DeleteAsync($"{Base}/{created.Id}");
        var response = await _client.GetAsync($"{Base}/{created.Id}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_MissingTodo_Returns404()
    {
        var response = await _client.DeleteAsync($"{Base}/99999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    private async Task<Todo> CreateTodoAsync(string name, string severity = "medium")
    {
        var response = await _client.PostAsJsonAsync(Base, new
        {
            name,
            severity,
            dueDate = "2026-09-01T00:00:00Z",
            isComplete = false
        });

        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<Todo>(TestJsonOptions.Api))!;
    }
}
