using System.Text.RegularExpressions;

namespace UnityCentralizedTodoApi.Domain;

public static partial class ProjectKey
{
    [GeneratedRegex("^[a-z0-9]+(?:[-_][a-z0-9]+)*$", RegexOptions.IgnoreCase)]
    private static partial Regex ProjectKeyPattern();

    public static bool TryNormalize(string? value, out string normalized)
    {
        normalized = string.Empty;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var candidate = value.Trim().ToLowerInvariant();

        if (!ProjectKeyPattern().IsMatch(candidate))
        {
            return false;
        }

        normalized = candidate;
        return true;
    }

    public static string Normalize(string value)
    {
        if (!TryNormalize(value, out var normalized))
        {
            throw new ArgumentException("Project key is invalid.", nameof(value));
        }

        return normalized;
    }
}