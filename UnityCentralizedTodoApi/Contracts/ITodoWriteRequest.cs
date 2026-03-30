namespace UnityCentralizedTodoApi.Contracts;

public interface ITodoWriteRequest
{
    string Name { get; }
    DateTime DueDate { get; }
}