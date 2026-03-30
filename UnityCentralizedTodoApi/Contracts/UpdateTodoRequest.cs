namespace UnityCentralizedTodoApi.Contracts;

public record UpdateTodoRequest(string Name, string Severity, DateTime DueDate, bool IsComplete) : ITodoWriteRequest;
