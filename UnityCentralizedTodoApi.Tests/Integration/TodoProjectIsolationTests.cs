using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using UnityCentralizedTodoApi.Domain;
using UnityCentralizedTodoApi.Tests.Helpers;

namespace UnityCentralizedTodoApi.Tests.Integration;

public class TodoProjectIsolationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private const string ProjectA = "isolation-proj-a";
    private const string ProjectB = "isolation-proj-b";

    public TodoProjectIsolationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Get_ProjectA_DoesNotReturnProjectBTodos()
    {
        await CreateTodoAsync(ProjectA, "Task for A");
        await CreateTodoAsync(ProjectB, "Task for B");

        var listA = await (await _client.GetAsync($"/projects/{ProjectA}/todos")).Content.ReadFromJsonAsync<List<Todo>>(TestJsonOptions.Api);
        var listB = await (await _client.GetAsync($"/projects/{ProjectB}/todos")).Content.ReadFromJsonAsync<List<Todo>>(TestJsonOptions.Api);

        Assert.NotNull(listA);
        Assert.NotNull(listB);
        Assert.DoesNotContain(listA!, t => t.Name == "Task for B");
        Assert.DoesNotContain(listB!, t => t.Name == "Task for A");
    }

    [Fact]
    public async Task GetById_UsesProjectScopedId()
    {
        // Both projects get id=1 because counters are per-project
        var todoA = await CreateTodoAsync(ProjectA, "A's first todo");
        var todoB = await CreateTodoAsync(ProjectB, "B's first todo");

        Assert.Equal(todoA.Id, todoB.Id); // both should be 1

        var fetchA = await (await _client.GetAsync($"/projects/{ProjectA}/todos/{todoA.Id}")).Content.ReadFromJsonAsync<Todo>(TestJsonOptions.Api);
        var fetchB = await (await _client.GetAsync($"/projects/{ProjectB}/todos/{todoB.Id}")).Content.ReadFromJsonAsync<Todo>(TestJsonOptions.Api);

        Assert.Equal("A's first todo", fetchA!.Name);
        Assert.Equal("B's first todo", fetchB!.Name);
    }

    [Fact]
    public async Task Delete_InProjectA_DoesNotAffectProjectB()
    {
        var todoA = await CreateTodoAsync(ProjectA, "Delete from A");
        var todoB = await CreateTodoAsync(ProjectB, "Should survive");

        await _client.DeleteAsync($"/projects/{ProjectA}/todos/{todoA.Id}");

        var responseFetchB = await _client.GetAsync($"/projects/{ProjectB}/todos/{todoB.Id}");
        Assert.Equal(HttpStatusCode.OK, responseFetchB.StatusCode);
    }

    [Fact]
    public async Task GetProjects_ListsAllProjectsWithTodos()
    {
        await CreateTodoAsync(ProjectA, "Any todo in A");
        await CreateTodoAsync(ProjectB, "Any todo in B");

        var projects = await (await _client.GetAsync("/projects")).Content.ReadFromJsonAsync<List<ProjectSummary>>(TestJsonOptions.Api);

        Assert.NotNull(projects);
        Assert.Contains(projects!, p => p.ProjectKey == ProjectA);
        Assert.Contains(projects!, p => p.ProjectKey == ProjectB);
    }

    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    private async Task<Todo> CreateTodoAsync(string projectKey, string name)
    {
        var response = await _client.PostAsJsonAsync($"/projects/{projectKey}/todos", new
        {
            name,
            severity = "medium",
            dueDate = "2026-09-01T00:00:00Z",
            isComplete = false
        });

        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<Todo>(TestJsonOptions.Api))!;
    }
}
