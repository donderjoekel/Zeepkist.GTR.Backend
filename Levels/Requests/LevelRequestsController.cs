using Microsoft.AspNetCore.Mvc;
using TNRD.Zeepkist.GTR.Backend.Levels.Requests.Resources;

namespace TNRD.Zeepkist.GTR.Backend.Levels.Requests;

[ApiController]
[Route("level/requests/")]
public class LevelRequestsController : ControllerBase
{
    private readonly ILevelRequestsService _service;

    public LevelRequestsController(ILevelRequestsService service)
    {
        _service = service;
    }

    [HttpPost("submit")]
    public IActionResult Submit([FromBody] LevelRequestResource resource)
    {
        _service.Add(resource.WorkshopId);
        return Ok();
    }
}
