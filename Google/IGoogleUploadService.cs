using FluentResults;
using Object = Google.Apis.Storage.v1.Data.Object;

namespace TNRD.Zeepkist.GTR.Backend.Google;

public interface IGoogleUploadService
{
    Task<Result<Object>> GetObject(string identifier, CancellationToken ct = default);
    Task<Result<string>> UploadGhost(string identifier, string ghostData, CancellationToken ct = default);
    Task<Result<string>> UploadScreenshot(string identifier, string screenshotData, CancellationToken ct = default);
    Task<Result<string>> UploadThumbnail(string uid, string thumbnailData, CancellationToken ct = default);
}
