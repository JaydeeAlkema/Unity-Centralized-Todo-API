namespace UnityCentralizedTodoApi.Contracts;

public record CreateTodoRequest(string Name, string Severity, DateTime DueDate, bool IsComplete) : ITodoWriteRequest;
