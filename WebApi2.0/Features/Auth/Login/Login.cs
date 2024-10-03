using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using FastEndpoints;
using FastEndpoints.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualBasic.CompilerServices;
using WebApi2._0.Domain.Entities;
using WebApi2._0.Domain.Enums;
using WebApi2._0.Infrastructure.Persistence;

namespace WebApi2._0.Features.Auth.Login;

[HttpPost("api/auth/login")]
[AllowAnonymous]
public class Login : Endpoint<LoginRequest, LoginResponse>
{
    private readonly GiveAwayDbContext _dbContext;
    private readonly IConfiguration _configuration;

    public Login(GiveAwayDbContext dbContext, IConfiguration configuration)
    {
        _dbContext = dbContext;
        _configuration = configuration;
    }


    public override async Task HandleAsync(LoginRequest req, CancellationToken ct)
    {
        var account = await _dbContext.Accounts.FirstOrDefaultAsync(x => x.Email == req.Email, cancellationToken: ct);

        if (account == null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        var isPasswordVerified = VerifyPasswordHash(req.Password, account.PasswordHash, account.PasswordSalt);

        if (!isPasswordVerified)
        {
            await SendUnauthorizedAsync(ct);
            return;
        }

        if (account.Status == AccountStatus.NotVerified)
        {
            await SendUnauthorizedAsync(ct);
            return;
        }

        if (account.Status == AccountStatus.Inactive)
        {
            await SendUnauthorizedAsync(ct);
            return;
        }

        var claims = new List<Claim>()
        {
            new Claim("AccountId", account.AccountId.ToString()),
            new(ClaimTypes.Name, account.Email),
            new(ClaimTypes.Role, account.Role.ToString())
        };

        if (account.Role == Domain.Enums.Roles.Staff && (account as Staff).Shop != null)
        {
            var shopId = await _dbContext.Shops.Where(x => x.StaffId == account.AccountId).Select(x => x.ShopId)
                .FirstOrDefaultAsync(ct);
            claims.Add(new Claim("ShopId", shopId.ToString()));
        }

        var accessToken = JwtBearer.CreateToken(
            o =>
            {
                o.SigningKey = _configuration.GetSection(JwtConstants.JwtKey).Value!;
                o.Issuer = _configuration.GetSection(JwtConstants.JwtIssuer).Value!;
                o.Audience = _configuration.GetSection(JwtConstants.JwtAudience).Value!;
                o.User.Claims.AddRange(claims);
            }
        );

        var response = new LoginResponse
        {
            AccessToken = accessToken,
            Role = account.Role,
            Id = account.AccountId,
            Email = account.Email
        };

        await SendOkAsync(response, ct);
    }

    private static bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
    {
        using var hmac = new HMACSHA512(passwordSalt);
        var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
        return computedHash.SequenceEqual(passwordHash);
    }

    private string GenerateAccessToken(List<Claim> claims)
    {
        var securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration[JwtConstants.JwtKey]!)
        );
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            _configuration[JwtConstants.JwtIssuer],
            _configuration[JwtConstants.JwtAudience],
            claims,
            expires: DateTime.Now.AddDays(1),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public static class JwtConstants
{
    public const string JwtIssuer = "Jwt:JWT_ISSUER";
    public const string JwtKey = "Jwt:JWT_SECRET_KEY";
    public const string JwtAudience = "Jwt:JWT_AUDIENCE";
}

public record LoginRequest
{
    public string Email { get; set; }
    public string Password { get; set; }
}

public class LoginResponse
{
    public string AccessToken { get; set; }
    public Roles Role { get; set; }
    public Guid Id { get; set; }
    public string Email { get; set; }
    public Guid? ShopId { get; set; }
}