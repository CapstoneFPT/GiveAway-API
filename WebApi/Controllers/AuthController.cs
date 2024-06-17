﻿using System.Security.Claims;
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
        try
        {
            var props = new AuthenticationProperties() { RedirectUri = Url.Action(nameof(GoogleSignin)) };
            return Challenge(props, GoogleDefaults.AuthenticationScheme);
        }
        catch (Exception e)
        {
            throw new Exception("Something went wrong with google sign in",e.InnerException);
        }
    }

    [HttpGet("signin-google")]
    public async Task<IActionResult> GoogleSignin()
    {
        try
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
                    email
                }
            );
        }
        catch (Exception e)
        {
            throw new Exception(e.Message, e.InnerException);
        }
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
        }
        else
            return Ok(user);
    }

    [HttpPost("register")]
    public async Task<Result<AccountResponse>> Register(RegisterRequest registerRequest)
    {
        return await _authService.Register(registerRequest);
    }

    [HttpPost("create-staff-account")]
    public async Task<ActionResult<Result<AccountResponse>>> CreateStaffAccount(
        CreateStaffAccountRequest registerRequest)
    {
        return await _authService.CreateStaffAccount(registerRequest);
    }

    [HttpGet("confirm-email")]
    public async Task<Result<string>> VerifyEmail(Guid id, string token)
    {
        return await _authService.VerifyEmail(id, token);
    }

    [HttpGet("resend-verify-email")]
    public async Task<Result<string>> ResendVerifyEmail(string email)
    {
        return await _authService.ResendVerifyEmail(email);
    }
}