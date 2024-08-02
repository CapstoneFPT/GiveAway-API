using BusinessObjects;
using BusinessObjects.Dtos.Account.Request;
using BusinessObjects.Dtos.Account.Response;
using BusinessObjects.Dtos.Auth;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Entities;
using Microsoft.AspNetCore.Identity;

namespace Services.Auth;

public interface IAuthService
{
    Task<Result<LoginResponse>> Login(string email, string password);
    
    Task<Result<string>> CheckPassword(string email, string newPassword);
    Task<Result<AccountResponse>> ChangeToNewPassword(string confirmtoken);
    Task<Result<AccountResponse>> Register(RegisterRequest request);
    Task<Result<string>> VerifyEmail(Guid id, string token);
    Task<Result<AccountResponse>> CreateStaffAccount(CreateStaffAccountRequest request);
    Task<Result<string>> ResendVerifyEmail(string email);
    Task<Result<AccountResponse>> CheckPasswordToChange(Guid accountId, ChangePasswordRequest request);
}
