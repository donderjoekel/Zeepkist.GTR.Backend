using Docker.DotNet;
using Docker.DotNet.Models;
using FluentResults;
using Microsoft.Extensions.Options;

namespace TNRD.Zeepkist.GTR.Backend.Workshop;

public interface IWorkshopService
{
    Task<Result<WorkshopDownloads>> DownloadWorkshopItems(IEnumerable<ulong> publishedFileIds);
    void RemoveDownloads(string id);
    void RemoveDownloads(WorkshopDownloads downloads);
    void RemoveAllDownloads();
}

public class WorkshopService : IWorkshopService
{
    private const ulong AppId = 1440670;

    private readonly IDockerClient _docker;
    private readonly WorkshopOptions _options;
    private readonly ILogger<WorkshopService> _logger;

    public WorkshopService(IDockerClient docker, IOptions<WorkshopOptions> options, ILogger<WorkshopService> logger)
    {
        _docker = docker;
        _logger = logger;
        _options = options.Value;
    }

    public async Task<Result<WorkshopDownloads>> DownloadWorkshopItems(IEnumerable<ulong> publishedFileIds)
    {
        string guid = Guid.NewGuid().ToString();
        bool success = await RunDockerProcess(guid, CreateParameters(publishedFileIds));

        if (!success)
        {
            return Result.Fail("Failed to download workshop items");
        }

        List<WorkshopItem> workshopItems = publishedFileIds.Select(x => GetWorkshopItem(guid, x))
            .Where(x => x != null)
            .ToList();

        return workshopItems.Count != 0
            ? Result.Ok(new WorkshopDownloads(guid, workshopItems))
            : Result.Fail("Unable to parse workshop items");
    }

    private WorkshopItem? GetWorkshopItem(string guid, ulong publishedFileId)
    {
        string path = Path.Combine(
            _options.MountPath,
            guid,
            "steamapps",
            "workshop",
            "content",
            AppId.ToString(),
            publishedFileId.ToString());

        if (!Directory.Exists(path))
        {
            _logger.LogWarning("Workshop item not found: {PublishedFileId}", publishedFileId);
            return null;
        }

        List<WorkshopLevel> levels = new();

        foreach (string directory in Directory.EnumerateDirectories(path))
        {
            string[] zeeplevels = Directory.GetFiles(directory, "*.zeeplevel");
            if (zeeplevels.Length != 1)
            {
                _logger.LogWarning(
                    "Found more than one zeeplevel for {PublishedFileId} in {Path}",
                    publishedFileId,
                    directory);

                continue;
            }

            string zeeplevelPath = zeeplevels.First();
            string[] thumbnails = Directory.GetFiles(
                directory,
                Path.GetFileNameWithoutExtension(zeeplevelPath) + "_Thumbnail.*");

            if (thumbnails.Length != 1)
            {
                _logger.LogWarning(
                    "Found more than one thumbnail for {PublishedFileId} in {Path}",
                    publishedFileId,
                    directory);

                continue;
            }

            levels.Add(new WorkshopLevel(zeeplevelPath, thumbnails.First()));
        }

        return new WorkshopItem(publishedFileId, levels);
    }

    public void RemoveDownloads(string id)
    {
        string path = Path.Combine(_options.MountPath, id);
        if (Path.Exists(path))
        {
            Directory.Delete(path, true);
        }
    }

    public void RemoveDownloads(WorkshopDownloads downloads)
    {
        RemoveDownloads(downloads.Id);
    }

    public void RemoveAllDownloads()
    {
        throw new NotImplementedException();
    }

    private async Task<bool> RunDockerProcess(string guid, IEnumerable<string> parameters)
    {
        CreateContainerResponse createResponse = await _docker.Containers.CreateContainerAsync(
            new CreateContainerParameters()
            {
                Image = "steamcmd/steamcmd:latest",
                Cmd = parameters.ToArray(),
                HostConfig = new HostConfig
                {
                    Binds = new List<string>
                    {
                        Path.Combine(_options.MountPath, guid) + ":/data"
                    }
                }
            });

        try
        {
            bool started = await _docker.Containers.StartContainerAsync(
                createResponse.ID,
                new ContainerStartParameters());

            if (!started)
            {
                throw new Exception("Failed to start container");
            }

            ContainerWaitResponse waitResponse = await _docker.Containers.WaitContainerAsync(createResponse.ID);
            if (waitResponse.StatusCode == 0)
            {
                return true;
            }
            else
            {
                _logger.LogError("Failed to download workshop item: {Message}", waitResponse.Error.Message);
                return false;
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to download workshop item");
            return false;
        }
        finally
        {
            await _docker.Containers.RemoveContainerAsync(
                createResponse.ID,
                new ContainerRemoveParameters()
                {
                    RemoveVolumes = true
                });
        }
    }

    private static IEnumerable<string> CreateParameters(ulong publishedFileId)
    {
        return CreateParameters(new[] { publishedFileId });
    }

    private static IEnumerable<string> CreateParameters(IEnumerable<ulong> publishedFileIds)
    {
        yield return "+force_install_dir /data";
        yield return "+login anonymous";

        foreach (ulong publishedFileId in publishedFileIds)
        {
            yield return $"+workshop_download_item {AppId} {publishedFileId}";
        }

        yield return "+quit";
    }
}
