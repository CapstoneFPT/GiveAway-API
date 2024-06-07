using BusinessObjects;
using BusinessObjects.Dtos.Account.Response;
using BusinessObjects.Dtos.Auth;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Entities;

namespace Services.Auth;

public interface IAuthService
{
    Task<Result<LoginResponse>> Login(string email, string password);
    Task<Account> FindUserByEmail(string email);
    Task<Result<string>> SendMail(string email);
    Task<Result<string>> CheckPassword(string email, string newpass);
    Task<Account> ChangeToNewPassword(string confirmtoken);
    Task<Result<AccountResponse>> Register(RegisterRequest request);
    Task<Result<string>> VerifyEmail(string email);
    Task<Result<AccountResponse>> CreateStaffAccount(CreateStaffAccountRequest request);
}
