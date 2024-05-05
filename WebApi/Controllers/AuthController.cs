using System.Security.Claims;
using BusinessObjects.Dtos.Auth;
using BusinessObjects.Dtos.Commons;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using Services.Auth;

namespace WebApi.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<Result<LoginResponse>>> Login(
        [FromBody] LoginRequest loginRequest
    )
    {
        var result = await _authService.Login(loginRequest.Email, loginRequest.Password);
        return Ok(result);
    }

    [HttpGet("login-google")]
    public IActionResult GoogleLogin()
    {
        var props = new AuthenticationProperties()
        {
            RedirectUri = "api/auth/signin-google"
        };
        return Challenge(props, GoogleDefaults.AuthenticationScheme);
    }

    [HttpGet("signin-google")]
    public async Task<IActionResult> GoogleSignin()
    {
        var response = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        if (response.Principal is null)
        {
            return BadRequest();
        }

        var name = response.Principal.FindFirstValue(ClaimTypes.Name);
        var givenName = response.Principal.FindFirstValue(ClaimTypes.GivenName);
        var email = response.Principal.FindFirstValue(ClaimTypes.Email);

        return Ok(new
        {
            name, givenName, email
        });
    }
}