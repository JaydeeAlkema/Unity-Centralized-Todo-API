using System.Collections.Concurrent;
using System.Threading;
using UnityCentralizedTodoApi.Domain;

namespace UnityCentralizedTodoApi.Data;

public sealed class TodoStore
{
    private readonly ConcurrentDictionary<int, Todo> _todos = new();
    private int _nextTodoId;

    public IReadOnlyList<Todo> GetAll()
    {
        return _todos.Values.OrderBy(t => t.Id).ToList();
    }

    public Todo? GetById(int id)
    {
        return _todos.TryGetValue(id, out var todo) ? todo : null;
    }

    public Todo Create(string name, DateTime dueDate, bool isComplete)
    {
        var id = Interlocked.Increment(ref _nextTodoId);
        var createdTodo = new Todo(
            Id: id,
            Name: name,
            DueDate: dueDate,
            IsComplete: isComplete);

        _todos[id] = createdTodo;
        return createdTodo;
    }

    public bool Delete(int id)
    {
        return _todos.TryRemove(id, out _);
    }

    public UpdateResult TryUpdate(int id, string name, DateTime dueDate, bool isComplete)
    {
        var updatedTodo = new Todo(
            Id: id,
            Name: name,
            DueDate: dueDate,
            IsComplete: isComplete);

        for (var attempt = 0; attempt < 5; attempt++)
        {
            if (!_todos.TryGetValue(id, out var currentTodo))
            {
                return UpdateResult.NotFound;
            }

            if (_todos.TryUpdate(id, updatedTodo, currentTodo))
            {
                return UpdateResult.Success(updatedTodo);
            }
        }

        return UpdateResult.Conflict;
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
