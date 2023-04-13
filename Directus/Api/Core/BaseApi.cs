using System.Net;
using FluentResults;
using TNRD.Zeepkist.GTR.Backend.Directus.Factories.Core;
using TNRD.Zeepkist.GTR.Backend.Directus.FilterBuilders.Core;
using TNRD.Zeepkist.GTR.Backend.Extensions;
using TNRD.Zeepkist.GTR.DTOs.Internal.Models;

namespace TNRD.Zeepkist.GTR.Backend.Directus.Api.Core;

internal abstract class BaseApi<TBuilder, TFactory, TModel>
    where TBuilder : BaseFilterBuilder<TBuilder>, new()
    where TFactory : BaseFactory<TFactory>, new()
    where TModel : BaseDirectusModel
{
    private readonly IDirectusClient client;

    protected abstract string Collection { get; }

    protected virtual int DefaultDepth { get; } = 1;

    protected BaseApi(IDirectusClient client)
    {
        this.client = client;
    }

    public async Task<Result<TModel?>> GetById(int id, CancellationToken ct = default)
    {
        Result<DirectusGetSingleResponse<TModel>> result =
            await client.Get<DirectusGetSingleResponse<TModel>>($"items/{Collection}/{id}{GetQuery()}", ct);

        if (result.IsSuccess)
            return result.Value.Data;

        if (result.TryGetReason(out StatusCodeReason reason))
        {
            if (reason.StatusCode == HttpStatusCode.Forbidden)
                return Result.Ok<TModel?>(null);
        }

        return result.ToResult();
    }

    public async Task<Result<DirectusGetMultipleResponse<TModel>>> Get(
        Action<TBuilder>? filter,
        CancellationToken ct = default
    )
    {
        string query = GetQuery(filter);
        return await Get(query, ct);
    }

    private async Task<Result<DirectusGetMultipleResponse<TModel>>> Get(string query, CancellationToken ct = default)
    {
        Result<DirectusGetMultipleResponse<TModel>> result =
            await client.Get<DirectusGetMultipleResponse<TModel>>($"items/{Collection}" + query, ct);

        return result.IsSuccess
            ? result.Value
            : result.ToResult();
    }

    public async Task<Result<IEnumerable<TModel>>> GetAll(
        Action<TBuilder>? filter = null,
        CancellationToken ct = default
    )
    {
        Result<int> countResult = await Count(filter, ct);
        if (countResult.IsFailed)
            return countResult.ToResult();

        TBuilder filterBuilder = new();
        filter?.Invoke(filterBuilder);
        filterBuilder.WithDepth(3);
        filterBuilder.WithLimit(100);

        List<TModel> levels = new();
        int pages = (int)Math.Ceiling(countResult.Value / 100d);
        for (int i = 0; i < pages; i++)
        {
            filterBuilder.WithOffset(i * 100);
            string query = filterBuilder.Build();
            Result<DirectusGetMultipleResponse<TModel>> getResult = await Get(query, ct);
            if (getResult.IsSuccess)
                levels.AddRange(getResult.Value.Data);
            else
                return getResult.ToResult();
        }

        return levels;
    }

    public async Task<Result<int>> Post(Action<TFactory> builder, CancellationToken ct = default)
    {
        TFactory factory = new();
        builder?.Invoke(factory);

        Result<DirectusPostResponse<BaseDirectusModel>> postResult =
            await client.Post<DirectusPostResponse<BaseDirectusModel>>($"items/{Collection}?fields=id",
                factory.Build(),
                ct);

        return postResult.IsSuccess
            ? postResult.Value.Data.Id
            : postResult.ToResult();
    }

    public async Task<Result<TModel>> Patch(int id, Action<TFactory> builder, CancellationToken ct = default)
    {
        TFactory factory = new();
        builder?.Invoke(factory);

        Result<DirectusPostResponse<BaseDirectusModel>> patchResult =
            await client.Patch<DirectusPostResponse<BaseDirectusModel>>($"items/{Collection}/" + id,
                factory.Build(),
                ct);

        if (patchResult.IsFailed)
            return patchResult.ToResult();

        Result<TModel?> getResult = await GetById(patchResult.Value.Data.Id, ct);
        return getResult.IsSuccess
            ? getResult.Value!
            : getResult.ToResult();
    }

    public async Task<Result> Delete(int id, CancellationToken ct = default)
    {
        return await client.Delete($"items/{Collection}/{id}", ct);
    }

    public async Task<Result<int>> Count(Action<TBuilder>? filter, CancellationToken ct = default)
    {
        string query = GetQuery(filter);

        Result<DirectusGetMultipleResponse<BaseDirectusModel>> result =
            await client.Get<DirectusGetMultipleResponse<BaseDirectusModel>>($"items/{Collection}" + query, ct);

        return result.IsSuccess ? result.Value.Metadata!.FilterCount! : result.ToResult();
    }

    protected string GetQuery(Action<TBuilder>? filter = null)
    {
        TBuilder filterBuilder = new();
        filterBuilder.WithDepth(DefaultDepth);
        filter?.Invoke(filterBuilder);
        return filterBuilder.Build();
    }
}
