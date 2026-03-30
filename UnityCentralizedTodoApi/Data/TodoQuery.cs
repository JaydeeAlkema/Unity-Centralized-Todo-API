using UnityCentralizedTodoApi.Domain;

namespace UnityCentralizedTodoApi.Data;

public sealed record TodoQuery(
    TodoSeverity? Severity,
    bool? IsComplete,
    DateTime? DueFrom,
    DateTime? DueTo,
    string? Search,
    TodoSortBy SortBy,
    bool Descending);