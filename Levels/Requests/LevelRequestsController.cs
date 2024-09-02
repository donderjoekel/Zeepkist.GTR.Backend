using FluentResults;
using Microsoft.AspNetCore.Mvc;
using TNRD.Zeepkist.GTR.Backend.Hashing;
using TNRD.Zeepkist.GTR.Backend.Levels.Jobs;
using TNRD.Zeepkist.GTR.Backend.Levels.Requests.Resources;
using TNRD.Zeepkist.GTR.Backend.Steam.Resources;
using TNRD.Zeepkist.GTR.Backend.Workshop;
using TNRD.Zeepkist.GTR.Backend.Zeeplevel;
using TNRD.Zeepkist.GTR.Backend.Zeeplevel.Resources;

namespace TNRD.Zeepkist.GTR.Backend.Levels.Requests;

[ApiController]
[Route("level/requests/")]
public class LevelRequestsController : ControllerBase
{
    private readonly ILevelRequestsService _service;
    private readonly WorkshopLister _workshopLister;
    private readonly WorkshopDownloader _workshopDownloader;
    private readonly IZeeplevelService _zeeplevelService;
    private readonly IHashService _hashService;

    public LevelRequestsController(ILevelRequestsService service,
        WorkshopLister workshopLister,
        WorkshopDownloader workshopDownloader,
        IZeeplevelService zeeplevelService,
        IHashService hashService)
    {
        _service = service;
        _workshopLister = workshopLister;
        _workshopDownloader = workshopDownloader;
        _zeeplevelService = zeeplevelService;
        _hashService = hashService;
    }

    [HttpPost("submit")]
    public IActionResult Submit([FromBody] LevelRequestResource resource)
    {
        _service.Add(resource.WorkshopId);
        return Ok();
    }

    [HttpGet("test")]
    public async Task<IActionResult> Test()
    {
        Result<List<PublishedFileDetails>> result = await _workshopLister.List(3104710625);

        if (result.IsFailed)
        {
            return base.Problem(result.ToString());
        }

        DownloadResult[] downloadResults = await _workshopDownloader.Download(result.Value);
        DownloadResult downloadResult = downloadResults.First();
        WorkshopItem workshopItem = downloadResult.Result.Value.Items.First();
        WorkshopLevel workshopLevel = workshopItem.Levels.First();
        ZeepLevel zeepLevel = _zeeplevelService.Parse(workshopLevel.ZeeplevelPath)!;
        string hash = _hashService.Hash(zeepLevel);
        return Ok(hash);
    }
}
