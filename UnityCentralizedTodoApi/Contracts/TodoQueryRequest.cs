namespace UnityCentralizedTodoApi.Contracts;

public sealed class TodoQueryRequest
{
    public string? Severity { get; init; }
    public bool? IsComplete { get; init; }
    public DateTime? DueFrom { get; init; }
    public DateTime? DueTo { get; init; }
    public string? Search { get; init; }
    public string? SortBy { get; init; }
    public bool? Descending { get; init; }
}