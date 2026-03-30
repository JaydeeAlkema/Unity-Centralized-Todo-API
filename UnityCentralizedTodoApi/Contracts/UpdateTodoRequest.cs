namespace UnityCentralizedTodoApi.Contracts;

public record UpdateTodoRequest(string Name, DateTime DueDate, bool IsComplete) : ITodoWriteRequest;
