using FluentResults;
using JetBrains.Annotations;
using TNRD.Zeepkist.GTR.Backend.Jobs;

namespace TNRD.Zeepkist.GTR.Backend.Media.Jobs;

public class UploadMediaJob
{
    private readonly IMediaService _mediaService;

    public UploadMediaJob(IMediaService mediaService)
    {
        _mediaService = mediaService;
    }

    [UsedImplicitly]
    public async Task ExecuteAsync(int recordId, string identifier, string ghostData, string screenshotData)
    {
        if (!_mediaService.HasGhost(recordId))
        {
            Result<string> ghostResult = await _mediaService.UploadGhost(recordId, ghostData, identifier);
            if (ghostResult.IsFailed)
            {
                throw new Exception("Failed to upload ghost:\n" + ghostResult);
            }
        }

        if (!_mediaService.HasScreenshot(recordId))
        {
            Result<string> screenshotResult = await _mediaService.UploadScreenshot(
                recordId,
                screenshotData,
                identifier);

            if (screenshotResult.IsFailed)
            {
                throw new Exception("Failed to upload screenshot:\n" + screenshotResult);
            }
        }
    }

    public static void Schedule(IJobScheduler jobScheduler,
        int userId,
        int recordId,
        string ghostData,
        string screenshotData)
    {
        jobScheduler.Enqueue<UploadMediaJob>(
            recordId,
            $"{userId}-{recordId}-{Guid.NewGuid()}",
            ghostData,
            screenshotData);
    }
}
