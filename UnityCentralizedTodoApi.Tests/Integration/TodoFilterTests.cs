using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using UnityCentralizedTodoApi.Domain;
using UnityCentralizedTodoApi.Tests.Helpers;

namespace UnityCentralizedTodoApi.Tests.Integration;

public class TodoFilterTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private const string Project = "filter-tests";
    private const string Base = $"/projects/{Project}/todos";

    public TodoFilterTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Get_FilterBySeverity_ReturnsOnlyMatchingTodos()
    {
        await CreateTodoAsync("High todo", "high");
        await CreateTodoAsync("Low todo", "low");

        var response = await _client.GetAsync($"{Base}?severity=high");
        var todos = await response.Content.ReadFromJsonAsync<List<Todo>>(TestJsonOptions.Api);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.All(todos!, t => Assert.Equal(TodoSeverity.High, t.Severity));
    }

    [Fact]
    public async Task Get_FilterByIsComplete_ReturnsOnlyMatchingTodos()
    {
        await CreateTodoAsync("Done todo", "medium", isComplete: true);
        await CreateTodoAsync("Pending todo", "medium", isComplete: false);

        var complete = await (await _client.GetAsync($"{Base}?isComplete=true")).Content.ReadFromJsonAsync<List<Todo>>(TestJsonOptions.Api);
        var incomplete = await (await _client.GetAsync($"{Base}?isComplete=false")).Content.ReadFromJsonAsync<List<Todo>>(TestJsonOptions.Api);

        Assert.All(complete!, t => Assert.True(t.IsComplete));
        Assert.All(incomplete!, t => Assert.False(t.IsComplete));
    }

    [Fact]
    public async Task Get_FilterBySearch_ReturnsCaseInsensitiveMatches()
    {
        await CreateTodoAsync("Fix Shader Warnings", "medium");
        await CreateTodoAsync("Write unit tests", "medium");

        var todos = await (await _client.GetAsync($"{Base}?search=shader")).Content.ReadFromJsonAsync<List<Todo>>(TestJsonOptions.Api);

        Assert.Single(todos!);
        Assert.Contains("Shader", todos![0].Name);
    }

    [Fact]
    public async Task Get_FilterByDueRange_ExcludesOutOfRangeTodos()
    {
        // create one todo far in the future so it is excluded by the range filter
        await CreateTodoAsync("Far future", "medium", dueDate: "2030-01-01T00:00:00Z");
        await CreateTodoAsync("Near future", "medium", dueDate: "2026-06-01T00:00:00Z");

        var todos = await (await _client.GetAsync(
            $"{Base}?dueFrom=2026-01-01T00:00:00Z&dueTo=2027-01-01T00:00:00Z")).Content.ReadFromJsonAsync<List<Todo>>(TestJsonOptions.Api);

        Assert.All(todos!, t =>
        {
            Assert.True(t.DueDate >= new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
            Assert.True(t.DueDate <= new DateTime(2027, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        });
    }

    [Fact]
    public async Task Get_NoFilters_ReturnsAllProjectTodos()
    {
        await CreateTodoAsync("Todo 1", "low");
        await CreateTodoAsync("Todo 2", "high");

        var todos = await (await _client.GetAsync(Base)).Content.ReadFromJsonAsync<List<Todo>>(TestJsonOptions.Api);

        Assert.NotNull(todos);
        Assert.True(todos!.Count >= 2);
    }

    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    private async Task<Todo> CreateTodoAsync(
        string name,
        string severity,
        bool isComplete = false,
        string dueDate = "2026-09-01T00:00:00Z")
    {
        var response = await _client.PostAsJsonAsync(Base, new { name, severity, dueDate, isComplete });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<Todo>(TestJsonOptions.Api))!;
    }
}
