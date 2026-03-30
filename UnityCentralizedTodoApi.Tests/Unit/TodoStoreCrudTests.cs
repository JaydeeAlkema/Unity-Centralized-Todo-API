using UnityCentralizedTodoApi.Data;
using UnityCentralizedTodoApi.Domain;
using UnityCentralizedTodoApi.Tests.Helpers;

namespace UnityCentralizedTodoApi.Tests.Unit;

public class TodoStoreCrudTests
{
    private readonly TodoStore _store = new();
    private const string Project = "test-project";

    [Fact]
    public void Create_ReturnsPersistedTodo()
    {
        var dueDate = TodoFactory.FutureDueDate;

        var todo = _store.Create(Project, "Buy groceries", TodoSeverity.High, dueDate, false);

        Assert.Equal(1, todo.Id);
        Assert.Equal("test-project", todo.ProjectKey);
        Assert.Equal("Buy groceries", todo.Name);
        Assert.Equal(TodoSeverity.High, todo.Severity);
        Assert.False(todo.IsComplete);
    }

    [Fact]
    public void Create_AssignsIncrementingIds()
    {
        var a = TodoFactory.Create(_store, projectKey: Project);
        var b = TodoFactory.Create(_store, projectKey: Project);
        var c = TodoFactory.Create(_store, projectKey: Project);

        Assert.Equal(1, a.Id);
        Assert.Equal(2, b.Id);
        Assert.Equal(3, c.Id);
    }

    [Fact]
    public void Create_IdCountersAreIndependentPerProject()
    {
        var a = TodoFactory.Create(_store, projectKey: "proj-a");
        var b = TodoFactory.Create(_store, projectKey: "proj-b");

        // Both start at 1 because each project has its own counter
        Assert.Equal(1, a.Id);
        Assert.Equal(1, b.Id);
    }

    [Fact]
    public void Create_NormalizesDueDateToUtc()
    {
        var localDue = new DateTime(2026, 6, 1, 12, 0, 0, DateTimeKind.Local);

        var todo = _store.Create(Project, "Test", TodoSeverity.Low, localDue, false);

        Assert.Equal(DateTimeKind.Utc, todo.DueDate.Kind);
    }

    [Fact]
    public void GetById_ReturnsCreatedTodo()
    {
        var todo = TodoFactory.Create(_store, projectKey: Project);

        var result = _store.GetById(Project, todo.Id);

        Assert.NotNull(result);
        Assert.Equal(todo.Id, result.Id);
    }

    [Fact]
    public void GetById_ReturnsNullForMissingId()
    {
        var result = _store.GetById(Project, 999);

        Assert.Null(result);
    }

    [Fact]
    public void TryUpdate_PersistsChanges()
    {
        var todo = TodoFactory.Create(_store, projectKey: Project, name: "Original");

        var result = _store.TryUpdate(
            projectKey: Project,
            id: todo.Id,
            name: "Updated",
            severity: TodoSeverity.Critical,
            dueDate: TodoFactory.FutureDueDate,
            isComplete: true);

        Assert.Equal(UpdateResultStatus.Success, result.Status);
        Assert.NotNull(result.Todo);
        Assert.Equal("Updated", result.Todo!.Name);
        Assert.Equal(TodoSeverity.Critical, result.Todo.Severity);
        Assert.True(result.Todo.IsComplete);
    }

    [Fact]
    public void TryUpdate_UpdatedAtIsNewerThanCreatedAt()
    {
        var todo = TodoFactory.Create(_store, projectKey: Project);

        var result = _store.TryUpdate(
            projectKey: Project,
            id: todo.Id,
            name: "Updated",
            severity: TodoSeverity.Low,
            dueDate: TodoFactory.FutureDueDate,
            isComplete: false);

        Assert.True(result.Todo!.UpdatedAt >= result.Todo.CreatedAt);
    }

    [Fact]
    public void TryUpdate_ReturnsNotFoundForMissingId()
    {
        var result = _store.TryUpdate(Project, 999, "x", TodoSeverity.Low, TodoFactory.FutureDueDate, false);

        Assert.Equal(UpdateResultStatus.NotFound, result.Status);
        Assert.Null(result.Todo);
    }

    [Fact]
    public void Delete_RemovesTodo()
    {
        var todo = TodoFactory.Create(_store, projectKey: Project);

        var deleted = _store.Delete(Project, todo.Id);
        var found = _store.GetById(Project, todo.Id);

        Assert.True(deleted);
        Assert.Null(found);
    }

    [Fact]
    public void Delete_ReturnsFalseForMissingId()
    {
        var deleted = _store.Delete(Project, 999);

        Assert.False(deleted);
    }
}
