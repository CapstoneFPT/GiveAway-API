using BusinessObjects.Dtos.Auth;
using BusinessObjects.Dtos.Commons;

namespace Services.Auth;

public interface IAuthService
{
    Task<Result<LoginResponse>> Login(string email, string password);
}
