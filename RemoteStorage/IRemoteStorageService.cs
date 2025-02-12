using FluentResults;

namespace TNRD.Zeepkist.GTR.Backend.RemoteStorage;

public interface IRemoteStorageService
{
    Task<Result> Delete(string path);

    Task<Result> Delete(string[] paths);
    
    Task<Result<string>> Upload(
        string b64,
        string folder,
        string name,
        string extension,
        string contentType,
        bool withEncoding);

    Task<Result<string>> Upload(
        byte[] buffer,
        string folder,
        string name,
        string extension,
        string contentType,
        bool withEncoding);

    Task<Result<string>> UploadImage(
        string b64,
        string folder,
        string name,
        string extension);

    Task<Result<string>> UploadImage(
        byte[] buffer,
        string folder,
        string name,
        string extension);
}
