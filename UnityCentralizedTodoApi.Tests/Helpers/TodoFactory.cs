using UnityCentralizedTodoApi.Data;
using UnityCentralizedTodoApi.Domain;

namespace UnityCentralizedTodoApi.Tests.Helpers;

/// <summary>
/// Convenience methods for creating test data without repeating boilerplate.
/// </summary>
internal static class TodoFactory
{
    internal static readonly DateTime FutureDueDate = DateTime.UtcNow.AddDays(30);

    internal static Todo Create(
        TodoStore store,
        string projectKey = "test-project",
        string name = "Test todo",
        TodoSeverity severity = TodoSeverity.Medium,
        DateTime? dueDate = null,
        bool isComplete = false)
    {
        return store.Create(
            projectKey: projectKey,
            name: name,
            severity: severity,
            dueDate: dueDate ?? FutureDueDate,
            isComplete: isComplete);
    }

    internal static TodoQuery EmptyQuery(TodoSortBy sortBy = TodoSortBy.DueDate) =>
        new(null, null, null, null, null, sortBy, false);
}
