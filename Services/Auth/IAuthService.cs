using BusinessObjects;
using BusinessObjects.Dtos.Auth;
using BusinessObjects.Dtos.Commons;

namespace Services.Auth;

public interface IAuthService
{
    Task<Result<LoginResponse>> Login(string email, string password);
    Task<User> FindUserByEmail(string email);
    Task<User> ResetPasswordToken(User user);
    Task<User> ChangeToNewPassword(ResetPasswordRequest request);
}
