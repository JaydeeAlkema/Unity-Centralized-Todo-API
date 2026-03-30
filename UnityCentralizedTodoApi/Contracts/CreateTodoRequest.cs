namespace UnityCentralizedTodoApi.Contracts;

public record CreateTodoRequest(string Name, DateTime DueDate, bool IsComplete) : ITodoWriteRequest;
