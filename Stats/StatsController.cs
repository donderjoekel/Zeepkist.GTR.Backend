using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using TNRD.Zeepkist.GTR.Backend.Jwt;
using TNRD.Zeepkist.GTR.Backend.Stats.Resources;

namespace TNRD.Zeepkist.GTR.Backend.Stats;

[ApiController]
[Route("[controller]")]
public class StatsController : ControllerBase
{
    private readonly IStatsService _service;

    public StatsController(IStatsService service)
    {
        _service = service;
    }

    [HttpPost]
    public IActionResult Submit([FromBody] StatsResource stats)
    {
        string? value = User.FindFirstValue(IJwtService.SteamIdClaimName);
        if (string.IsNullOrEmpty(value))
        {
            return Unauthorized();
        }

        if (!ulong.TryParse(value, out ulong steamId))
        {
            return Unauthorized();
        }

        _service.Submit(steamId, stats);
        return Ok();
    }
}
