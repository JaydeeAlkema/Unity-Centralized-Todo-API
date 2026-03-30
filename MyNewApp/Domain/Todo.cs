namespace MyNewApp.Domain;

public record Todo(int Id, string Name, DateTime DueDate, bool IsComplete);