using Amazon.S3;
using Amazon.S3.Model;
using FluentResults;
using FluentStorage;
using FluentStorage.AWS.Blobs;
using FluentStorage.Blobs;
using Microsoft.Extensions.Options;

namespace TNRD.Zeepkist.GTR.Backend.RemoteStorage;

public class WasabiStorageService : IRemoteStorageService
{
    private readonly WasabiStorageOptions _options;
    private readonly IBlobStorage _blobStorage;
    private readonly IBlobStorage _imageBlobStorage;
    private readonly ILogger<WasabiStorageService> _logger;

    public WasabiStorageService(IOptions<WasabiStorageOptions> options, ILogger<WasabiStorageService> logger)
    {
        _logger = logger;
        _options = options.Value;
        _blobStorage = StorageFactory.Blobs.Wasabi(
            _options.AccessKeyId,
            _options.SecretAccessKey,
            _options.Bucket,
            _options.ServiceUrl);
        _imageBlobStorage = _blobStorage.WithGzipCompression();
    }

    public Task<Result<string>> Upload(
        string b64,
        string folder,
        string name,
        string extension,
        string contentType,
        bool withEncoding)
    {
        byte[] buffer = Convert.FromBase64String(b64);
        return Upload(buffer, folder, name, extension, contentType, withEncoding);
    }

    public async Task<Result<string>> Upload(
        byte[] buffer,
        string folder,
        string name,
        string extension,
        string contentType,
        bool withEncoding)
    {
        string path = StoragePath.Combine(folder, name + extension);

        try
        {
            await _blobStorage.WriteAsync(path, buffer);
            return Result.Ok("https://cdn.zeepkist-gtr.com" + path);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to upload to remote storage");
            return Result.Fail("Failed to upload to remote storage");
        }
    }

    public Task<Result<string>> UploadImage(string b64, string folder, string name, string extension)
    {
        return UploadImage(Convert.FromBase64String(b64), folder, name, extension);
    }

    public async Task<Result<string>> UploadImage(byte[] buffer, string folder, string name, string extension)
    {
        string path = StoragePath.Combine(folder, name + extension);

        try
        {
            await _imageBlobStorage.WriteAsync(path, buffer);

            Result gzipResult = await SetGzipHeader(path);
            if (gzipResult.IsFailed)
            {
                _logger.LogError("Failed to set gzip header");
            }

            return Result.Ok("https://cdn.zeepkist-gtr.com" + path);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to upload to remote storage");
            return Result.Fail("Failed to upload to remote storage");
        }
    }

    private async Task<Result> SetGzipHeader(string path)
    {
        IAwsS3BlobStorage awsStorage = (IAwsS3BlobStorage)_blobStorage;
        CopyObjectRequest request = new()
        {
            SourceBucket = _options.Bucket,
            DestinationBucket = _options.Bucket,
            SourceKey = path,
            DestinationKey = path,
            MetadataDirective = S3MetadataDirective.REPLACE
        };
        request.Headers.ContentEncoding = "gzip";

        CopyObjectResponse response = await awsStorage.NativeBlobClient.CopyObjectAsync(request);
        if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
        {
            _logger.LogError("Failed to copy object; Status Code: {StatusCode}", response.HttpStatusCode);
            return Result.Fail("Failed to copy object");
        }

        return Result.Ok();
    }
}
