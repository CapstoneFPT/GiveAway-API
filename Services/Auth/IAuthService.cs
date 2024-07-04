using BusinessObjects;
using BusinessObjects.Dtos.Account.Response;
using BusinessObjects.Dtos.Auth;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Entities;
using Microsoft.AspNetCore.Identity;

namespace Services.Auth;

public interface IAuthService
{
    Task<Result<LoginResponse>> Login(string email, string password);
    //Task<Account> FindUserByEmail(string email);
    Task<Result<string>> SendMail(string email);
    Task<Result<string>> CheckPassword(string email, string newpass);
    Task<Result<AccountResponse>> ChangeToNewPassword(string confirmtoken);
    Task<Result<AccountResponse>> Register(RegisterRequest request);
    Task<Result<string>> VerifyEmail(Guid id, string token);
    Task<Result<AccountResponse>> CreateStaffAccount(CreateStaffAccountRequest request);
    Task<Result<string>> ResendVerifyEmail(string email);
}
