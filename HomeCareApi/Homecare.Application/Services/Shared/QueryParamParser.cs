public static class QueryParamParser
{
    public static string? GetString(Dictionary<string, string> p, string key)
        => p.TryGetValue(key, out var v) && !string.IsNullOrWhiteSpace(v) ? v : null;

    public static decimal? GetDecimal(Dictionary<string, string> p, string key)
        => p.TryGetValue(key, out var v) && decimal.TryParse(v, out var result) ? result : null;

    public static int GetInt(Dictionary<string, string> p, string key, int fallback = 0)
        => p.TryGetValue(key, out var v) && int.TryParse(v, out var result) ? result : fallback;

    public static bool GetBool(Dictionary<string, string> p, string key)
        => p.TryGetValue(key, out var v) && bool.TryParse(v, out var result) && result;
}