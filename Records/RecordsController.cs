using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TNRD.Zeepkist.GTR.Backend.Jwt;
using TNRD.Zeepkist.GTR.Backend.Records.Resources;

namespace TNRD.Zeepkist.GTR.Backend.Records;

[ApiController]
[Route("[controller]")]
public class RecordsController : ControllerBase
{
    private readonly IRecordsService _service;

    public RecordsController(IRecordsService service)
    {
        _service = service;
    }

    [HttpPost]
    public IActionResult Submit([FromBody] RecordResource record)
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

        _service.Submit(steamId, record);
        return Ok();
    }
}
