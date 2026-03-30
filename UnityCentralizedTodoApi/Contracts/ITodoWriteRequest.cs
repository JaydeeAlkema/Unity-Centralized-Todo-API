namespace UnityCentralizedTodoApi.Contracts;

public interface ITodoWriteRequest
{
    string Name { get; }
    string Severity { get; }
    DateTime DueDate { get; }
}