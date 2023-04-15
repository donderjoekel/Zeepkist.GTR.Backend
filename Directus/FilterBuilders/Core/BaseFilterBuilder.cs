namespace TNRD.Zeepkist.GTR.Backend.Directus.FilterBuilders.Core;

public abstract class BaseFilterBuilder<T> where T : BaseFilterBuilder<T>
{
    private readonly Dictionary<string, string> specialFilters = new();
    private readonly Dictionary<string, Filter> filters = new();

    public bool HasFilters => filters.Count > 0 || specialFilters.Count > 0;

    protected BaseFilterBuilder()
    {
        specialFilters["meta"] = "*";
        WithDepth(1);
    }

    protected T AddFilter(string key, object? value, FilterMode mode = FilterMode.Equals)
    {
        if (!key.StartsWith("[") && !key.EndsWith("]"))
            key = $"[{key}]";
        
        if (value != null)
            filters[key] = new Filter(key, value.ToString()!, mode);
        return (T)this;
    }

    public T WithFields(string fields)
    {
        specialFilters["fields"] = fields;
        return (T)this;
    }

    public T WithDepth(int depth)
    {
        string fields = string.Empty;

        for (int i = 0; i < depth; i++)
        {
            fields += "*";
            if (i < depth - 1)
                fields += ".";
        }

        return WithFields(fields);
    }

    public T WithId(int id)
    {
        return AddFilter("id", id);
    }

    public T WithIds(IEnumerable<int> ids)
    {
        return AddFilter("id", string.Join(',', ids), FilterMode.OneOf);
    }

    public T WithDateCreated(DateTime dateCreated, FilterMode filterMode = FilterMode.Equals)
    {
        return AddFilter("[date_created]", dateCreated.ToString("yyyy-MM-dd HH:mm:ss"), filterMode);
    }

    public T WithDateCreatedBetween(DateTime start, DateTime end)
    {
        return AddFilter("[date_created]",
            start.ToString("yyyy-MM-dd") + "," + end.ToString("yyyy-MM-dd"),
            FilterMode.Between);
    }

    public T WithDateUpdated(DateTime dateUpdated, FilterMode filterMode = FilterMode.Equals)
    {
        return AddFilter("[date_updated]", dateUpdated.ToString("yyyy-MM-dd HH:mm:ss"), filterMode);
    }

    public T WithLimit(int limit)
    {
        specialFilters["limit"] = Math.Min(limit, 100).ToString();
        return (T)this;
    }

    public T WithLimit(int? limit)
    {
        return limit.HasValue ? WithLimit(limit.Value) : (T)this;
    }

    public T WithOffset(int offset)
    {
        specialFilters["offset"] = offset.ToString();
        return (T)this;
    }

    public T WithOffset(int? offset)
    {
        return offset.HasValue ? WithOffset(offset.Value) : (T)this;
    }

    public T WithSort(string sort)
    {
        if (sort.Contains(','))
        {
            string[] splits = sort.Split(',');
            specialFilters["sort[]"] = string.Join("&sort[]=", splits);
        }
        else
        {
            specialFilters["sort[]"] = sort;
        }

        return (T)this;
    }

    public string Build()
    {
        if (!HasFilters)
            return string.Empty;

        if (filters.Count > 0 && specialFilters.Count > 0)
        {
            return "?" +
                   string.Join('&',
                       string.Join('&', filters.Select(x => x.Value.Build())),
                       string.Join('&', specialFilters.Select(x => $"{x.Key}={x.Value}"))
                   );
        }

        if (filters.Count > 0)
        {
            return "?" + string.Join('&', filters.Select(x => x.Value.Build()));
        }

        if (specialFilters.Count > 0)
        {
            return "?" + string.Join('&', specialFilters.Select(x => $"{x.Key}={x.Value}"));
        }

        throw new NotSupportedException();
    }

    protected static string CreateMultiKey(params string[] keys)
    {
        return string.Join("", keys.Select(x => "[" + x + "]"));
    }
}
