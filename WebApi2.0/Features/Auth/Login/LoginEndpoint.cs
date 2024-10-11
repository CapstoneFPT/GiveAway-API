using System.Security.Claims;
using System.Security.Cryptography;
using FastEndpoints;
using FastEndpoints.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using WebApi2._0.Domain.Entities;
using WebApi2._0.Domain.Enums;
using WebApi2._0.Infrastructure.Persistence;
namespace WebApi2._0.Features.Auth.Login;

[HttpPost("api/auth/login")]
[AllowAnonymous]
public class LoginEndpoint : Endpoint<LoginRequest, LoginResponse>
{
    private readonly GiveAwayDbContext _dbContext;
    private readonly IConfiguration _configuration;

    public LoginEndpoint(GiveAwayDbContext dbContext, IConfiguration configuration)
    {
        _dbContext = dbContext;
        _configuration = configuration;
    }

    public override async Task HandleAsync(LoginRequest req, CancellationToken ct)
    {
        var account = await _dbContext.Accounts.Include(staff => ((Staff)staff).Shop)
            .FirstOrDefaultAsync(x => x.Email == req.Email, cancellationToken: ct);

        if (account == null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        var isPasswordVerified = VerifyPasswordHash(new VerifyPasswordHashParams()
        {
            Password = req.Password,
            PasswordHash = account.PasswordHash,
            PasswordSalt = account.PasswordSalt
        });

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
                o.User.Roles.Add(account.Role.ToString());
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

    private static bool VerifyPasswordHash(VerifyPasswordHashParams verifyPasswordHashParams)
    {
        using var hmac = new HMACSHA512(verifyPasswordHashParams.PasswordSalt);
        var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(verifyPasswordHashParams.Password));
        return computedHash.SequenceEqual(verifyPasswordHashParams.PasswordHash);
    }
}

public record VerifyPasswordHashParams
{
    public string Password { get; set; }
    public byte[] PasswordHash { get; set; }
    public byte[] PasswordSalt { get; set; }
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