namespace MyNewApp.Contracts;

public record UpdateTodoRequest(string Name, DateTime DueDate, bool IsComplete) : ITodoWriteRequest;
