namespace TNRD.Zeepkist.GTR.Backend.Directus.FilterBuilders.Core;

internal class Filter
{
    private readonly string key;
    private readonly string value;
    private readonly FilterMode filterMode;

    public Filter(string key, string value, FilterMode filterMode)
    {
        this.key = key;
        this.value = value;
        this.filterMode = filterMode;
    }

    public string Build()
    {
        switch (filterMode)
        {
            case FilterMode.Equals:
                return $"filter{key}[_eq]={value}";
            case FilterMode.NotEquals:
                return $"filter{key}[_neq]={value}";
            case FilterMode.LessThan:
                return $"filter{key}[_lt]={value}";
            case FilterMode.LessThanEquals:
                return $"filter{key}[_lte]={value}";
            case FilterMode.GreaterThan:
                return $"filter{key}[_gt]={value}";
            case FilterMode.GreaterThanEquals:
                return $"filter{key}[_gte]={value}";
            case FilterMode.Contains:
                return $"filter{key}[_contains]={value}";
            case FilterMode.NotContains:
                return $"filter{key}[_ncontains]={value}";
            case FilterMode.StartsWith:
                return $"filter{key}[_starts_with]={value}";
            case FilterMode.NotStartsWith:
                return $"filter{key}[_nstarts_with]={value}";
            case FilterMode.EndsWith:
                return $"filter{key}[_ends_with]={value}";
            case FilterMode.NotEndsWith:
                return $"filter{key}[_nends_with]={value}";
            case FilterMode.OneOf:
                return $"filter{key}[_in]={value}";
            case FilterMode.Between:
                return $"filter{key}[_between]={value}";
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
