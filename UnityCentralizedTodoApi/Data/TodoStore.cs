using System.Collections.Concurrent;
using System.Threading;
using UnityCentralizedTodoApi.Domain;

namespace UnityCentralizedTodoApi.Data;

public sealed class TodoStore : ITodoRepository
{
    private readonly ConcurrentDictionary<string, ProjectBucket> _projects = new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyList<ProjectSummary> GetProjects()
    {
        return _projects
            .OrderBy(project => project.Key, StringComparer.OrdinalIgnoreCase)
            .Select(project => new ProjectSummary(
                ProjectKey: project.Key,
                TodoCount: project.Value.Todos.Count,
                CompletedTodoCount: project.Value.Todos.Values.Count(todo => todo.IsComplete)))
            .Where(project => project.TodoCount > 0)
            .ToList();
    }

    public IReadOnlyList<Todo> Query(string projectKey, TodoQuery query)
    {
        if (!TryGetProject(projectKey, out var bucket))
        {
            return Array.Empty<Todo>();
        }

        IEnumerable<Todo> todos = bucket.Todos.Values;

        if (query.Severity is { } severity)
        {
            todos = todos.Where(todo => todo.Severity == severity);
        }

        if (query.IsComplete is { } isComplete)
        {
            todos = todos.Where(todo => todo.IsComplete == isComplete);
        }

        if (query.DueFrom is { } dueFrom)
        {
            todos = todos.Where(todo => todo.DueDate >= dueFrom);
        }

        if (query.DueTo is { } dueTo)
        {
            todos = todos.Where(todo => todo.DueDate <= dueTo);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            todos = todos.Where(todo => todo.Name.Contains(query.Search, StringComparison.OrdinalIgnoreCase));
        }

        todos = ApplySort(todos, query.SortBy, query.Descending);

        return todos.ToList();
    }

    public Todo? GetById(string projectKey, int id)
    {
        if (!TryGetProject(projectKey, out var bucket))
        {
            return null;
        }

        return bucket.Todos.TryGetValue(id, out var todo) ? todo : null;
    }

    public Todo Create(string projectKey, string name, TodoSeverity severity, DateTime dueDate, bool isComplete)
    {
        var normalizedProjectKey = ProjectKey.Normalize(projectKey);
        var bucket = _projects.GetOrAdd(normalizedProjectKey, _ => new ProjectBucket());
        var id = bucket.NextTodoId();
        var timestamp = DateTime.UtcNow;
        var createdTodo = new Todo(
            Id: id,
            ProjectKey: normalizedProjectKey,
            Name: name,
            Severity: severity,
            DueDate: dueDate.ToUniversalTime(),
            IsComplete: isComplete,
            CreatedAt: timestamp,
            UpdatedAt: timestamp);

        bucket.Todos[id] = createdTodo;
        return createdTodo;
    }

    public bool Delete(string projectKey, int id)
    {
        return TryGetProject(projectKey, out var bucket) && bucket.Todos.TryRemove(id, out _);
    }

    public UpdateResult TryUpdate(string projectKey, int id, string name, TodoSeverity severity, DateTime dueDate, bool isComplete)
    {
        if (!TryGetProject(projectKey, out var bucket))
        {
            return UpdateResult.NotFound;
        }

        for (var attempt = 0; attempt < 5; attempt++)
        {
            if (!bucket.Todos.TryGetValue(id, out var currentTodo))
            {
                return UpdateResult.NotFound;
            }

            var updatedTodo = currentTodo with
            {
                Name = name,
                Severity = severity,
                DueDate = dueDate.ToUniversalTime(),
                IsComplete = isComplete,
                UpdatedAt = DateTime.UtcNow
            };

            if (bucket.Todos.TryUpdate(id, updatedTodo, currentTodo))
            {
                return UpdateResult.Success(updatedTodo);
            }
        }

        return UpdateResult.Conflict;
    }

    private static IOrderedEnumerable<Todo> ApplySort(IEnumerable<Todo> todos, TodoSortBy sortBy, bool descending)
    {
        return (sortBy, descending) switch
        {
            (TodoSortBy.Name, true) => todos.OrderByDescending(todo => todo.Name, StringComparer.OrdinalIgnoreCase).ThenBy(todo => todo.Id),
            (TodoSortBy.Name, false) => todos.OrderBy(todo => todo.Name, StringComparer.OrdinalIgnoreCase).ThenBy(todo => todo.Id),
            (TodoSortBy.Severity, true) => todos.OrderByDescending(todo => todo.Severity).ThenBy(todo => todo.DueDate).ThenBy(todo => todo.Id),
            (TodoSortBy.Severity, false) => todos.OrderBy(todo => todo.Severity).ThenBy(todo => todo.DueDate).ThenBy(todo => todo.Id),
            (TodoSortBy.CreatedAt, true) => todos.OrderByDescending(todo => todo.CreatedAt).ThenBy(todo => todo.Id),
            (TodoSortBy.CreatedAt, false) => todos.OrderBy(todo => todo.CreatedAt).ThenBy(todo => todo.Id),
            (TodoSortBy.UpdatedAt, true) => todos.OrderByDescending(todo => todo.UpdatedAt).ThenBy(todo => todo.Id),
            (TodoSortBy.UpdatedAt, false) => todos.OrderBy(todo => todo.UpdatedAt).ThenBy(todo => todo.Id),
            (TodoSortBy.DueDate, true) => todos.OrderByDescending(todo => todo.DueDate).ThenBy(todo => todo.Id),
            _ => todos.OrderBy(todo => todo.DueDate).ThenBy(todo => todo.Id)
        };
    }

    private bool TryGetProject(string projectKey, out ProjectBucket bucket)
    {
        return _projects.TryGetValue(ProjectKey.Normalize(projectKey), out bucket!);
    }

    private sealed class ProjectBucket
    {
        private int _nextTodoId;

        public ConcurrentDictionary<int, Todo> Todos { get; } = new();

        public int NextTodoId() => Interlocked.Increment(ref _nextTodoId);
    }
}

public readonly record struct UpdateResult(UpdateResultStatus Status, Todo? Todo)
{
    public static UpdateResult NotFound => new(UpdateResultStatus.NotFound, null);
    public static UpdateResult Conflict => new(UpdateResultStatus.Conflict, null);
    public static UpdateResult Success(Todo todo) => new(UpdateResultStatus.Success, todo);
}

public enum UpdateResultStatus
{
    Success,
    NotFound,
    Conflict
}
