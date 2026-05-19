using System;

namespace Homecare.Application.Constants.Offers;

public static class InputTypeOperators
{
    public static readonly IReadOnlyDictionary<string, IReadOnlyList<string>> Allowed =
        new Dictionary<string, IReadOnlyList<string>>
        {
            { "number", new[] { "gte", "lte", "eq", "between" } },
            { "date",   new[] { "eq", "between" } },
            { "time",   new[] { "between" } },
            { "days",   new[] { "in" } },
            { "text",   new[] { "eq", "in" } },
            { "subcategory", new[] { "in" } },
        };
}
