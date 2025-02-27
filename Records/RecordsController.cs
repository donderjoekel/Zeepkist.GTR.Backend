﻿using System.Security.Claims;
using FluentResults;
using Microsoft.AspNetCore.Mvc;
using TNRD.Zeepkist.GTR.Backend.Jwt;
using TNRD.Zeepkist.GTR.Backend.Records.Resources;

namespace TNRD.Zeepkist.GTR.Backend.Records;

[ApiController]
[Route("records")]
public class RecordsController : ControllerBase
{
    private readonly IRecordsService _service;

    public RecordsController(IRecordsService service)
    {
        _service = service;
    }

    [HttpPost("submit")]
    public async Task<IActionResult> Submit([FromBody] RecordResource record)
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

        Result result = await _service.Submit(steamId, record);
        if (result.IsSuccess)
        {
            return Ok();
        }

        return BadRequest(result.ToString());
    }
}
