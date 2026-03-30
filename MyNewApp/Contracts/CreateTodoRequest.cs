namespace MyNewApp.Contracts;

public record CreateTodoRequest(string Name, DateTime DueDate, bool IsComplete) : ITodoWriteRequest;
