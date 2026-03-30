using UnityCentralizedTodoApi.Data;
using UnityCentralizedTodoApi.Domain;
using UnityCentralizedTodoApi.Tests.Helpers;

namespace UnityCentralizedTodoApi.Tests.Unit;

public class TodoStoreProjectIsolationTests
{
    private readonly TodoStore _store = new();

    [Fact]
    public void Create_TodoAppearsOnlyInItsOwnProject()
    {
        TodoFactory.Create(_store, projectKey: "proj-a", name: "Task A");
        TodoFactory.Create(_store, projectKey: "proj-b", name: "Task B");

        var projA = _store.Query("proj-a", TodoFactory.EmptyQuery());
        var projB = _store.Query("proj-b", TodoFactory.EmptyQuery());

        Assert.Single(projA);
        Assert.Equal("Task A", projA[0].Name);
        Assert.Single(projB);
        Assert.Equal("Task B", projB[0].Name);
    }

    [Fact]
    public void GetById_ReturnsNullForDifferentProject()
    {
        var todo = TodoFactory.Create(_store, projectKey: "proj-a");

        var result = _store.GetById("proj-b", todo.Id);

        Assert.Null(result);
    }

    [Fact]
    public void Delete_OnlyRemovesFromTargetProject()
    {
        var todoA = TodoFactory.Create(_store, projectKey: "proj-a");
        TodoFactory.Create(_store, projectKey: "proj-b");

        var deleted = _store.Delete("proj-a", todoA.Id);
        var projA = _store.Query("proj-a", TodoFactory.EmptyQuery());
        var projB = _store.Query("proj-b", TodoFactory.EmptyQuery());

        Assert.True(deleted);
        Assert.Empty(projA);
        Assert.Single(projB);
    }

    [Fact]
    public void Delete_ReturnsFalseForDifferentProject()
    {
        var todo = TodoFactory.Create(_store, projectKey: "proj-a");

        var deleted = _store.Delete("proj-b", todo.Id);

        Assert.False(deleted);
    }

    [Fact]
    public void TryUpdate_ReturnsNotFoundForDifferentProject()
    {
        var todo = TodoFactory.Create(_store, projectKey: "proj-a");

        var result = _store.TryUpdate(
            projectKey: "proj-b",
            id: todo.Id,
            name: "Updated",
            severity: TodoSeverity.Low,
            dueDate: TodoFactory.FutureDueDate,
            isComplete: false);

        Assert.Equal(UpdateResultStatus.NotFound, result.Status);
    }

    [Fact]
    public void GetProjects_ListsOnlyProjectsWithTodos()
    {
        TodoFactory.Create(_store, projectKey: "proj-a");
        TodoFactory.Create(_store, projectKey: "proj-b");
        TodoFactory.Create(_store, projectKey: "proj-b");

        var projects = _store.GetProjects();

        Assert.Equal(2, projects.Count);
        Assert.Contains(projects, p => p.ProjectKey == "proj-a" && p.TodoCount == 1);
        Assert.Contains(projects, p => p.ProjectKey == "proj-b" && p.TodoCount == 2);
    }

    [Fact]
    public void GetProjects_NormalizesProjectKeyToCamelCase()
    {
        TodoFactory.Create(_store, projectKey: "My-Project");

        var projects = _store.GetProjects();

        Assert.Single(projects);
        Assert.Equal("my-project", projects[0].ProjectKey);
    }

    [Fact]
    public void GetProjects_TracksCompletedCount()
    {
        TodoFactory.Create(_store, projectKey: "proj-a", isComplete: true);
        TodoFactory.Create(_store, projectKey: "proj-a", isComplete: false);

        var projects = _store.GetProjects();
        var summary = projects.Single(p => p.ProjectKey == "proj-a");

        Assert.Equal(2, summary.TodoCount);
        Assert.Equal(1, summary.CompletedTodoCount);
    }
}
