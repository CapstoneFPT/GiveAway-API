using System.Security.Claims;
using BusinessObjects.Dtos.Auth;
using BusinessObjects.Dtos.Commons;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using BusinessObjects.Dtos.Email;
using Microsoft.AspNetCore.Mvc;
using Services.Auth;
using Services.Emails;

namespace WebApi.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IEmailService _emailService;

    public AuthController(IAuthService authService, IEmailService emailService)
    {
        _authService = authService;
        _emailService = emailService;
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

    [HttpGet("forgot-password")]
    public async Task<Result<string>> ForgotPassword(string email, string newpass)
    {
        var user = await _authService.CheckPassword(email, newpass);
        return user;
    }
    [HttpPut("reset-password")]
    public async Task<IActionResult> ResetPassword(string confirmtoken)
    {
        var user = await _authService.ChangeToNewPassword(confirmtoken);
        if (user == null)
        {
            return BadRequest("Invalid token");
        }else
        return Ok(user);
    }
    [HttpPost("register")]
    public async Task<Result<string>> Register(RegisterRequest registerRequest)
    {
        return await _authService.Register(registerRequest);
    }
   /* [HttpGet("verify-email")]
    public async Task<Result<string>> VerifyEmail(string email)
    {

    }*/
}
