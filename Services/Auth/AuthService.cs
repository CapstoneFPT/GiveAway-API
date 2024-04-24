using System.Security.Claims;
using BusinessObjects.Dtos.Auth;
using BusinessObjects.Dtos.Commons;
using Repositories.User;

namespace Services.Auth;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;

    public AuthService(IUserRepository userRepository,ITokenService tokenService)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
    }


    public async Task<Result<LoginResponse>> Login(string email, string password)
    {
        try
        {
            var user = await _userRepository.FindOne(x => x.Email.Equals(email) && x.Password.Equals(password));

            if (user is null)
            {
                return new Result<LoginResponse>()
                {
                    ResultStatus = ResultStatus.NotFound,
                    Messages = ["Member Not Found"]
                };
            }

            var claims = new List<Claim>()
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.Email),
                new(ClaimTypes.Role, user.Role.ToString())
            };

            var accessToken = _tokenService.GenerateAccessToken(claims);

            var data = new LoginResponse()
            {
                AccessToken = accessToken
            };

            return new Result<LoginResponse>()
            {
                Data = data,
                Messages = ["Login successfully"],
                ResultStatus = ResultStatus.Success
            };
        }
        catch (Exception e)
        {
            return new Result<LoginResponse>()
            {
                Messages = new[] { e.Message },
                ResultStatus = ResultStatus.Error
            };
        }
    }
}