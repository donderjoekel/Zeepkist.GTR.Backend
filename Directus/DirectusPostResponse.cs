namespace TNRD.Zeepkist.GTR.Backend.Directus;

internal class DirectusPostResponse<T>
{
    public T Data { get; set; } = default!;
}
