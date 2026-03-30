namespace UnityCentralizedTodoApi.Domain;

public record Todo(
    int Id,
    string ProjectKey,
    string Name,
    TodoSeverity Severity,
    DateTime DueDate,
    bool IsComplete,
    DateTime CreatedAt,
    DateTime UpdatedAt);