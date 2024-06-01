using BusinessObjects;
using BusinessObjects.Dtos.Auth;
using BusinessObjects.Dtos.Commons;

namespace Services.Auth;

public interface IAuthService
{
    Task<Result<LoginResponse>> Login(string email, string password);
    Task<User> FindUserByEmail(string email);
    Task<Result<string>> SendMail(string email);
    Task<Result<string>> CheckPassword(string email, string newpass);
    Task<User> ChangeToNewPassword(string confirmtoken);
    Task<Result<string>> Register(RegisterRequest request);
    Task<Result<string>> VerifyEmail(string email);
}
