using System.Security.Claims;

namespace Services.Auth;

public interface ITokenService
{
    public string GenerateAccessToken(List<Claim> claims);
}
