using System.Text;
using FluentResults;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Upload;
using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Options;
using TNRD.Zeepkist.GTR.Backend.Reasons;
using Object = Google.Apis.Storage.v1.Data.Object;

namespace TNRD.Zeepkist.GTR.Backend.Google;

internal class CloudStorageUploadService : IGoogleUploadService
{
    private readonly GoogleOptions googleOptions;
    private StorageClient? cachedStorageClient;

    public CloudStorageUploadService(IOptions<GoogleOptions> googleOptions)
    {
        this.googleOptions = googleOptions.Value;
    }

    private StorageClient GetOrCreateStorageClient()
    {
        if (cachedStorageClient != null)
            return cachedStorageClient;

        byte[] bytes = Convert.FromBase64String(googleOptions.Credentials);
        string json = Encoding.UTF8.GetString(bytes);

        GoogleCredential credential = GoogleCredential.FromJson(json);
        if (credential.IsCreateScopedRequired)
            credential = credential.CreateScoped("https://www.googleapis.com/auth/devstorage.read_write");

        cachedStorageClient = StorageClient.Create(credential);
        return cachedStorageClient;
    }

    /// <inheritdoc />
    public async Task<Result<Object>> GetObject(string identifier, CancellationToken ct = default)
    {
        StorageClient client = GetOrCreateStorageClient();
        try
        {
            Object objectAsync = await client.GetObjectAsync(googleOptions.DriveId, identifier, cancellationToken: ct);
            return objectAsync;
        }
        catch (Exception e)
        {
            return Result.Fail(new ExceptionalError(e));
        }
    }

    /// <inheritdoc />
    public async Task<Result<string>> UploadGhost(
        string identifier,
        string ghostData,
        CancellationToken ct = default
    )
    {
        Result<Object> uploadResult =
            await Upload(ghostData, "ghosts", $"{identifier}.bin", "application/octet-stream", ct);

        if (uploadResult.IsFailed)
            return uploadResult.ToResult();

        try
        {
            uploadResult.Value.ContentEncoding = "gzip";
            StorageClient client = GetOrCreateStorageClient();
            await client.UpdateObjectAsync(uploadResult.Value, cancellationToken: ct);
        }
        catch (Exception e)
        {
            return Result
                .Ok(CreatePublicUrl(uploadResult.Value))
                .WithReason(new InfoReason(e.Message));
        }

        return CreatePublicUrl(uploadResult.Value);
    }

    private async Task<Result<Object>> Upload(
        string b64,
        string folder,
        string name,
        string contentType,
        CancellationToken ct = default
    )
    {
        byte[] bytes = Convert.FromBase64String(b64);
        StorageClient client = GetOrCreateStorageClient();

        bool hasCompleted = false;
        bool hasFailed = false;
        Exception? exception = null;
        Object? uploadedObject;

        using (MemoryStream stream = new(bytes))
        {
            Progress<IUploadProgress> progress = new();
            progress.ProgressChanged += (sender, p) =>
            {
                if (p.Status == UploadStatus.Completed)
                {
                    hasCompleted = true;
                }
                else if (p.Status == UploadStatus.Failed)
                {
                    hasCompleted = true;
                    hasFailed = true;
                    exception = p.Exception;
                }
            };

            uploadedObject = await client.UploadObjectAsync(googleOptions.DriveId,
                $"{folder}/{name}",
                contentType,
                stream,
                progress: progress,
                cancellationToken: ct);
        }

        while (!hasCompleted)
        {
            await Task.Yield();
        }

        return hasFailed
            ? Result.Fail(new ExceptionalError(exception ?? new Exception("Unable to upload data")))
            : uploadedObject;
    }

    /// <inheritdoc />
    public async Task<Result<string>> UploadScreenshot(
        string identifier,
        string screenshotData,
        CancellationToken ct = default
    )
    {
        Result<Object> uploadResult =
            await Upload(screenshotData, "screenshots", $"{identifier}.jpeg", "image/jpeg", ct);
        if (uploadResult.IsFailed)
            return uploadResult.ToResult();

        return CreatePublicUrl(uploadResult.Value);
    }

    /// <inheritdoc />
    public async Task<Result<string>> UploadThumbnail(
        string uid,
        string thumbnailData,
        CancellationToken ct = default
    )
    {
        Result<Object> uploadResult =
            await Upload(thumbnailData, "thumbnails", $"{uid}.jpeg", "image/jpeg", ct);
        if (uploadResult.IsFailed)
            return uploadResult.ToResult();

        return CreatePublicUrl(uploadResult.Value);
    }

    private string CreatePublicUrl(Object uploadedObject)
    {
        return uploadedObject.MediaLink;
        // Outdated
        // string name = uploadedObject.Name;
        // string folder = name[..name.IndexOf('/')];
        // string encodedName = Uri.EscapeDataString(name[(name.IndexOf('/') + 1)..]);
        // return $"http://storage.googleapis.com/{uploadedObject.Bucket}/{folder}/{encodedName}";
    }
}
