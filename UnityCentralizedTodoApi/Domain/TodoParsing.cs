namespace UnityCentralizedTodoApi.Domain;

public static class TodoParsing
{
    public static bool TryParseSeverity(string? value, out TodoSeverity severity)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            severity = default;
            return false;
        }

        return Enum.TryParse(value, ignoreCase: true, out severity) && Enum.IsDefined(severity);
    }

    public static bool TryParseSortBy(string? value, out TodoSortBy sortBy)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            sortBy = TodoSortBy.DueDate;
            return false;
        }

        return Enum.TryParse(value, ignoreCase: true, out sortBy) && Enum.IsDefined(sortBy);
    }

    public static TodoSortBy ParseSortByOrDefault(string? value)
    {
        return TryParseSortBy(value, out var sortBy)
            ? sortBy
            : TodoSortBy.DueDate;
    }
}