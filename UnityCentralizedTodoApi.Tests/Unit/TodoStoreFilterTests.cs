using UnityCentralizedTodoApi.Data;
using UnityCentralizedTodoApi.Domain;
using UnityCentralizedTodoApi.Tests.Helpers;

namespace UnityCentralizedTodoApi.Tests.Unit;

public class TodoStoreFilterTests
{
    private readonly TodoStore _store = new();
    private const string Project = "test-project";

    [Fact]
    public void Query_FilterBySeverity_ReturnsOnlyMatchingSeverity()
    {
        TodoFactory.Create(_store, projectKey: Project, severity: TodoSeverity.High);
        TodoFactory.Create(_store, projectKey: Project, severity: TodoSeverity.Low);

        var results = _store.Query(Project, new TodoQuery(TodoSeverity.High, null, null, null, null, TodoSortBy.DueDate, false));

        Assert.Single(results);
        Assert.Equal(TodoSeverity.High, results[0].Severity);
    }

    [Fact]
    public void Query_FilterByIsComplete_ReturnsOnlyMatchingState()
    {
        TodoFactory.Create(_store, projectKey: Project, isComplete: true);
        TodoFactory.Create(_store, projectKey: Project, isComplete: false);

        var complete = _store.Query(Project, new TodoQuery(null, true, null, null, null, TodoSortBy.DueDate, false));
        var incomplete = _store.Query(Project, new TodoQuery(null, false, null, null, null, TodoSortBy.DueDate, false));

        Assert.Single(complete);
        Assert.True(complete[0].IsComplete);
        Assert.Single(incomplete);
        Assert.False(incomplete[0].IsComplete);
    }

    [Fact]
    public void Query_FilterByDueFrom_ExcludesEarlierTodos()
    {
        var cutoff = DateTime.UtcNow.AddDays(10);
        TodoFactory.Create(_store, projectKey: Project, dueDate: DateTime.UtcNow.AddDays(5));
        TodoFactory.Create(_store, projectKey: Project, dueDate: DateTime.UtcNow.AddDays(15));

        var results = _store.Query(Project, new TodoQuery(null, null, cutoff, null, null, TodoSortBy.DueDate, false));

        Assert.Single(results);
        Assert.True(results[0].DueDate >= cutoff);
    }

    [Fact]
    public void Query_FilterByDueTo_ExcludesLaterTodos()
    {
        var cutoff = DateTime.UtcNow.AddDays(10);
        TodoFactory.Create(_store, projectKey: Project, dueDate: DateTime.UtcNow.AddDays(5));
        TodoFactory.Create(_store, projectKey: Project, dueDate: DateTime.UtcNow.AddDays(15));

        var results = _store.Query(Project, new TodoQuery(null, null, null, cutoff, null, TodoSortBy.DueDate, false));

        Assert.Single(results);
        Assert.True(results[0].DueDate <= cutoff);
    }

    [Fact]
    public void Query_FilterBySearch_IsCaseInsensitive()
    {
        TodoFactory.Create(_store, projectKey: Project, name: "Fix Shader Warnings");
        TodoFactory.Create(_store, projectKey: Project, name: "Write unit tests");

        var results = _store.Query(Project, new TodoQuery(null, null, null, null, "shader", TodoSortBy.DueDate, false));

        Assert.Single(results);
        Assert.Contains("Shader", results[0].Name);
    }

    [Fact]
    public void Query_FilterBySearch_ReturnsEmptyWhenNoMatch()
    {
        TodoFactory.Create(_store, projectKey: Project, name: "Buy groceries");

        var results = _store.Query(Project, new TodoQuery(null, null, null, null, "xxxxxxx", TodoSortBy.DueDate, false));

        Assert.Empty(results);
    }

    [Fact]
    public void Query_CombinedFilters_ApplyAllConditions()
    {
        TodoFactory.Create(_store, projectKey: Project, name: "High incomplete", severity: TodoSeverity.High, isComplete: false);
        TodoFactory.Create(_store, projectKey: Project, name: "High complete", severity: TodoSeverity.High, isComplete: true);
        TodoFactory.Create(_store, projectKey: Project, name: "Low incomplete", severity: TodoSeverity.Low, isComplete: false);

        var results = _store.Query(Project, new TodoQuery(TodoSeverity.High, false, null, null, null, TodoSortBy.DueDate, false));

        Assert.Single(results);
        Assert.Equal("High incomplete", results[0].Name);
    }

    [Fact]
    public void Query_UnknownProject_ReturnsEmpty()
    {
        var results = _store.Query("no-such-project", TodoFactory.EmptyQuery());

        Assert.Empty(results);
    }

    [Theory]
    [InlineData(TodoSortBy.Name, false, new[] { "Alpha", "Beta", "Gamma" })]
    [InlineData(TodoSortBy.Name, true, new[] { "Gamma", "Beta", "Alpha" })]
    public void Query_SortByName_ReturnsCorrectOrder(TodoSortBy sortBy, bool descending, string[] expectedNames)
    {
        TodoFactory.Create(_store, projectKey: Project, name: "Gamma");
        TodoFactory.Create(_store, projectKey: Project, name: "Alpha");
        TodoFactory.Create(_store, projectKey: Project, name: "Beta");

        var results = _store.Query(Project, new TodoQuery(null, null, null, null, null, sortBy, descending));

        Assert.Equal(expectedNames, results.Select(t => t.Name).ToArray());
    }

    [Theory]
    [InlineData(TodoSortBy.Severity, false, new[] { TodoSeverity.Low, TodoSeverity.High, TodoSeverity.Critical })]
    [InlineData(TodoSortBy.Severity, true, new[] { TodoSeverity.Critical, TodoSeverity.High, TodoSeverity.Low })]
    public void Query_SortBySeverity_ReturnsCorrectOrder(TodoSortBy sortBy, bool descending, TodoSeverity[] expectedSeverities)
    {
        TodoFactory.Create(_store, projectKey: Project, severity: TodoSeverity.Critical);
        TodoFactory.Create(_store, projectKey: Project, severity: TodoSeverity.Low);
        TodoFactory.Create(_store, projectKey: Project, severity: TodoSeverity.High);

        var results = _store.Query(Project, new TodoQuery(null, null, null, null, null, sortBy, descending));

        Assert.Equal(expectedSeverities, results.Select(t => t.Severity).ToArray());
    }
}
