using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using TNRD.Zeepkist.GTR.Backend.Jwt;
using TNRD.Zeepkist.GTR.Backend.Voting.Resources;

namespace TNRD.Zeepkist.GTR.Backend.Voting;

[ApiController]
[Route("voting")]
public class VotingController : ControllerBase
{
    private readonly IVotingService _service;

    public VotingController(IVotingService service)
    {
        _service = service;
    }

    [HttpPost("upvote")]
    public IActionResult Upvote([FromBody] VoteResource resource)
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

        if (_service.Upvote(steamId, resource.Level).IsSuccess)
        {
            return Ok();
        }

        return Problem();
    }

    [HttpPost("downvote")]
    public IActionResult Downvote([FromBody] VoteResource resource)
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

        if (_service.Downvote(steamId, resource.Level).IsSuccess)
        {
            return Ok();
        }

        return Problem();
    }

    [HttpPost("dupvote")]
    public IActionResult DoubleUpvote([FromBody] VoteResource resource)
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

        if (_service.DoubleUpvote(steamId, resource.Level).IsSuccess)
        {
            return Ok();
        }

        return Problem();
    }

    [HttpPost("ddownvote")]
    public IActionResult DoubleDownvote([FromBody] VoteResource resource)
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

        if (_service.DoubleDownvote(steamId, resource.Level).IsSuccess)
        {
            return Ok();
        }

        return Problem();
    }
}
