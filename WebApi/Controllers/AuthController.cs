using System.Net;
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
using BusinessObjects.Dtos.Account.Response;
using Microsoft.AspNetCore.Identity;
using BusinessObjects.Dtos.Account.Request;
using DotNext;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;

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
    public async Task<ActionResult<BusinessObjects.Dtos.Commons.Result<LoginResponse>>> Login(
        [FromBody] LoginRequest loginRequest
    )
    {
        var result = await _authService.Login(loginRequest.Email, loginRequest.Password);

        if (result.ResultStatus != ResultStatus.Success)
        {
            return StatusCode((int)HttpStatusCode.InternalServerError, result);
        }

        return Ok(result);
    }

    /*[HttpGet("login-google")]
    public IActionResult GoogleLogin()
    {
        var props = new AuthenticationProperties() { RedirectUri = Url.Action(nameof(GoogleSignin)) };
        return Challenge(props, GoogleDefaults.AuthenticationScheme);
    }

    [HttpGet("signin-google")]
    public async Task<IActionResult> GoogleSignin()
    {
        var response = await HttpContext.AuthenticateAsync(
            CookieAuthenticationDefaults.AuthenticationScheme
        );
        if (response.Principal is null)
        {
            return BadRequest("Principle is null here");
        }

        var name = response.Principal.FindFirstValue(ClaimTypes.Name);
        var givenName = response.Principal.FindFirstValue(ClaimTypes.GivenName);
        var email = response.Principal.FindFirstValue(ClaimTypes.Email);

        return Ok(
            new
            {
                name,
                givenName,
                email,
                response.Succeeded,
                response.Properties.Items
            }
        );
    }*/

    [HttpGet("forgot-password")]
    public async Task<ActionResult<BusinessObjects.Dtos.Commons.Result<string>>> ForgotPassword([FromQuery] ForgetPasswordRequest request)
    {
        var user = await _authService.CheckPassword(request.Email, request.Password);

        if (user.ResultStatus != ResultStatus.Success)
        {
            return StatusCode((int)HttpStatusCode.InternalServerError, user);
        }

        return Ok(user);
    }

    [HttpPut("reset-password")]
    public async Task<ActionResult<BusinessObjects.Dtos.Commons.Result<AccountResponse>>> ResetPassword(string confirmtoken)
    {
        var result = await _authService.ChangeToNewPassword(confirmtoken);

        if (result.ResultStatus != ResultStatus.Success)
        {
            return StatusCode((int)HttpStatusCode.InternalServerError, result);
        }

        return Ok(result);
    }

    [HttpPost("register")]
    public async Task<ActionResult<BusinessObjects.Dtos.Commons.Result<AccountResponse>>> Register(RegisterRequest registerRequest)
    {
        var result = await _authService.Register(registerRequest);

        if (result.ResultStatus != ResultStatus.Success)
        {
            return StatusCode((int)HttpStatusCode.InternalServerError, result);
        }

        return result;
    }
    [Authorize(Roles = "Admin")]
    [HttpPost("create-staff-account")]
    public async Task<ActionResult<BusinessObjects.Dtos.Commons.Result<AccountResponse>>> CreateStaffAccount(
        [FromBody] CreateStaffAccountRequest registerRequest)
    {
        var result = await _authService.CreateStaffAccount(registerRequest);

        if (result.ResultStatus != ResultStatus.Success)
        {
            return StatusCode((int)HttpStatusCode.InternalServerError, result);
        }

        return result;
    }

    [HttpGet("confirm-email")]
    public async Task<IActionResult> VerifyEmail(Guid id, string token)
    {
        var result = await _authService.VerifyEmail(id, token);
        if (result.ResultStatus == ResultStatus.Success)
            return Redirect($"https://giveawayproject.jettonetto.org/verify-email?verificationstatus=success");

        return Redirect($"https://giveawayproject.jettonetto.org/verify-email?verificationstatus=failed");
    }

    [HttpGet("resend-verify-email")]
    public async Task<ActionResult<BusinessObjects.Dtos.Commons.Result<string>>> ResendVerifyEmail(string email)
    {
        var result = await _authService.ResendVerifyEmail(email);

        if (result.ResultStatus != ResultStatus.Success)
        {
            return StatusCode((int)HttpStatusCode.InternalServerError, result);
        }

        return Ok(result);
    }
    [Authorize]
    [HttpPut("{accountId}/change-password")]
    public async Task<ActionResult<BusinessObjects.Dtos.Commons.Result<AccountResponse>>> ChangePassword([FromRoute] Guid accountId,
        [FromBody] ChangePasswordRequest request)
    {
        var result = await _authService.CheckPasswordToChange(accountId, request);

        if (result.ResultStatus != ResultStatus.Success)
        {
            return StatusCode((int)HttpStatusCode.InternalServerError, result);
        }

        return Ok(result);
    }
}