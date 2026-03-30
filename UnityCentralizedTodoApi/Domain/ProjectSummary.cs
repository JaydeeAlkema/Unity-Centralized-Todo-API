namespace UnityCentralizedTodoApi.Domain;

public sealed record ProjectSummary(string ProjectKey, int TodoCount, int CompletedTodoCount);