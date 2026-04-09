using System.Web;

namespace MetaCompta.BackOffice.Services;

/// <summary>
/// Fluent query string builder with proper URL encoding.
/// Provides a clean, type-safe way to construct API URLs with parameters.
/// </summary>
public sealed class QueryBuilder
{
    private readonly string _baseUrl;
    private readonly List<KeyValuePair<string, string>> _params = new();

    private QueryBuilder(string baseUrl) => _baseUrl = baseUrl;

    /// <summary>Create a new QueryBuilder for the given API endpoint.</summary>
    public static QueryBuilder For(string baseUrl) => new(baseUrl);

    /// <summary>Add a parameter if the value is not null or empty.</summary>
    public QueryBuilder Add(string key, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
            _params.Add(new(key, value));
        return this;
    }

    /// <summary>Add a parameter if the value is not null.</summary>
    public QueryBuilder Add(string key, int? value)
    {
        if (value.HasValue)
            _params.Add(new(key, value.Value.ToString()));
        return this;
    }

    /// <summary>Add a parameter with a fixed int value.</summary>
    public QueryBuilder Add(string key, int value)
    {
        _params.Add(new(key, value.ToString()));
        return this;
    }

    /// <summary>Add a parameter if the boolean is true.</summary>
    public QueryBuilder Add(string key, bool value)
    {
        if (value)
            _params.Add(new(key, "true"));
        return this;
    }

    /// <summary>Build the full URL with properly encoded query string.</summary>
    public string Build()
    {
        if (_params.Count == 0)
            return _baseUrl;

        var queryString = string.Join("&",
            _params.Select(p => $"{HttpUtility.UrlEncode(p.Key)}={HttpUtility.UrlEncode(p.Value)}"));

        return $"{_baseUrl}?{queryString}";
    }

    public override string ToString() => Build();

    /// <summary>Implicit conversion to string for easy use.</summary>
    public static implicit operator string(QueryBuilder builder) => builder.Build();
}
