using FluentResults;
using TNRD.Zeepkist.GTR.Backend.Steam.Resources;
using TNRD.Zeepkist.GTR.Backend.Workshop;

namespace TNRD.Zeepkist.GTR.Backend.Levels.Jobs;

public record DownloadResult(IEnumerable<PublishedFileDetails> PublishedFileDetails, Result<WorkshopDownloads> Result);