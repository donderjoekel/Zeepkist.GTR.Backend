using System.Text;
using FluentResults;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Upload;
using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Options;
using Object = Google.Apis.Storage.v1.Data.Object;

namespace TNRD.Zeepkist.GTR.Backend.RemoteStorage;

public class GoogleCloudStorageService //: IRemoteStorageService
{
    private readonly GoogleCloudStorageOptions _options;

    private StorageClient? _storageClient;

    public GoogleCloudStorageService(IOptions<GoogleCloudStorageOptions> options)
    {
        _options = options.Value;
    }

    private StorageClient GetOrCreateStorageClient()
    {
        if (_storageClient != null)
            return _storageClient;

        byte[] bytes = Convert.FromBase64String(_options.Credentials);
        string json = Encoding.UTF8.GetString(bytes);

        GoogleCredential credential = GoogleCredential.FromJson(json);
        if (credential.IsCreateScopedRequired)
            credential = credential.CreateScoped("https://www.googleapis.com/auth/devstorage.read_write");

        _storageClient = StorageClient.Create(credential);
        return _storageClient;
    }

    public Task<Result<string>> Upload(
        string b64,
        string folder,
        string name,
        string extension,
        string contentType,
        bool withEncoding)
    {
        return Upload(Convert.FromBase64String(b64), folder, name, extension, contentType, withEncoding);
    }

    public async Task<Result<string>> Upload(
        byte[] buffer,
        string folder,
        string name,
        string extension,
        string contentType,
        bool withEncoding)
    {
        StorageClient storageClient = GetOrCreateStorageClient();

        bool hasCompleted = false;
        bool hasFailed = false;
        Exception? exception = null;
        Object? uploadedObject;

        using (MemoryStream stream = new(buffer))
        {
            Progress<IUploadProgress> progress = new();
            progress.ProgressChanged += (sender, uploadProgress) =>
            {
                if (uploadProgress.Status == UploadStatus.Completed)
                {
                    hasCompleted = true;
                }
                else if (uploadProgress.Status == UploadStatus.Failed)
                {
                    hasCompleted = true;
                    hasFailed = true;
                    exception = uploadProgress.Exception;
                }
            };

            Object objectToUpload = new()
            {
                Bucket = _options.Bucket,
                Name = CreatePath(folder, name, extension),
                ContentType = contentType,
                ContentEncoding = withEncoding ? "lzma" : null
            };

            uploadedObject = await storageClient.UploadObjectAsync(objectToUpload, stream, progress: progress);
        }

        while (!hasCompleted)
        {
            await Task.Delay(100);
        }

        return hasFailed
            ? Result.Fail(new ExceptionalError(exception ?? new Exception("Unable to upload data")))
            : Result.Ok(uploadedObject.MediaLink);
    }

    private static string CreatePath(string folder, string name, string extension)
    {
        return extension.StartsWith('.') ? $"{folder}/{name}{extension}" : $"{folder}/{name}.{extension}";
    }
}
