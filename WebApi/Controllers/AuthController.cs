using BusinessObjects.Dtos.Auth;
using BusinessObjects.Dtos.Commons;
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
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(string email)
    {
        var user = await _authService.FindUserByEmail(email);
        if (user == null)
        {
            return BadRequest("User not found");
        }
        else
        {
            await _authService.ResetPasswordToken(user);
            /*var mail = new SendEmailRequest();
            mail.To = "alejandrin.hane@ethereal.email";
            mail.Subject = "Reset Password";
            mail.Body = user.PasswordResetToken.ToString();
            _emailService.SendEmail(mail);*/
        }
        
        return Ok(user);
    }
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest request)
    {
        var user = await _authService.ChangeToNewPassword(request);
        if (user == null)
        {
            return BadRequest("Invalid token");
        }else
        return Ok(user);
    }
}
