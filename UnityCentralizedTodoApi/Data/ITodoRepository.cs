using UnityCentralizedTodoApi.Domain;

namespace UnityCentralizedTodoApi.Data;

public interface ITodoRepository
{
    IReadOnlyList<ProjectSummary> GetProjects();
    IReadOnlyList<Todo> Query(string projectKey, TodoQuery query);
    Todo? GetById(string projectKey, int id);
    Todo Create(string projectKey, string name, TodoSeverity severity, DateTime dueDate, bool isComplete);
    bool Delete(string projectKey, int id);
    UpdateResult TryUpdate(string projectKey, int id, string name, TodoSeverity severity, DateTime dueDate, bool isComplete);
}