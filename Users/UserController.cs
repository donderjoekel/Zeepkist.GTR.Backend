using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using TNRD.Zeepkist.GTR.Backend.Jwt;
using TNRD.Zeepkist.GTR.Backend.Users.Resources;

namespace TNRD.Zeepkist.GTR.Backend.Users;

[ApiController]
[Route("user")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost("update/name")]
    public IActionResult UpdateName([FromBody] UpdateNameResource resource)
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

        _userService.UpdateName(steamId, resource.Name);
        return Ok();
    }

    [HttpPost("update/discord")]
    public IActionResult UpdateDiscord([FromBody] UpdateDiscordResource resource)
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

        _userService.UpdateDiscord(steamId, resource.Id);
        return Ok();
    }
}