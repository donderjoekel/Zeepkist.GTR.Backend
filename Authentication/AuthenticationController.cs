using FluentResults;
using Microsoft.AspNetCore.Mvc;
using TNRD.Zeepkist.GTR.Backend.Authentication.Resources;
using TNRD.Zeepkist.GTR.Backend.Versioning;

namespace TNRD.Zeepkist.GTR.Backend.Authentication;

[ApiController]
[Route("[controller]")]
public class AuthenticationController : ControllerBase
{
    private readonly IAuthenticationService _authenticationService;
    private readonly IVersionService _versionService;
    private readonly ILogger<AuthenticationController> _logger;

    public AuthenticationController(
        IAuthenticationService authenticationService,
        IVersionService versionService,
        ILogger<AuthenticationController> logger)
    {
        _authenticationService = authenticationService;
        _versionService = versionService;
        _logger = logger;
    }

    [HttpPost]
    [Route("login")]
    public async Task<IActionResult> Login([FromBody] LoginResource resource)
    {
        if (_versionService.NeedsUpdate(resource.ModVersion))
        {
            return BadRequest("Update required");
        }

        Result<AuthenticationData> result = await _authenticationService.Login(
            resource.SteamId,
            resource.AuthenticationTicket);

        if (result.IsFailed)
        {
            _logger.LogError("Failed to authenticate: {Result}", result);
            return Problem("Failed to authenticate", statusCode: 500);
        }

        return Ok(result.Value);
    }

    [HttpPost]
    [HttpPost("refresh")]
    public IActionResult Refresh([FromBody] RefreshResource resource)
    {
        if (_versionService.NeedsUpdate(resource.ModVersion))
        {
            return BadRequest("Update required");
        }

        Result<AuthenticationData> result = _authenticationService.Refresh(
            resource.SteamId,
            resource.LoginToken,
            resource.RefreshToken);

        if (result.IsFailed)
        {
            _logger.LogError("Failed to refresh token: {Result}", result);
            return Problem("Failed to refresh token", statusCode: 500);
        }

        return Ok(result.Value);
    }
}
