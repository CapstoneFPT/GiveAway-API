using System.Security.Claims;
using System.Security.Cryptography;
using AutoMapper;
using BusinessObjects.Dtos.Account.Request;
using BusinessObjects.Dtos.Account.Response;
using BusinessObjects.Dtos.Auth;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.Email;
using BusinessObjects.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Org.BouncyCastle.Asn1.Ocsp;
using Repositories.Accounts;
using Repositories.Shops;
using Services.Emails;

namespace Services.Auth;

public class AuthService : IAuthService
{
    private readonly IAccountRepository _accountRepository;
    private readonly ITokenService _tokenService;
    private readonly IEmailService _emailService;
    private readonly IMemoryCache _cache;
    private readonly IMapper _mapper;
    private readonly IShopRepository _shopRepository;
    private readonly IConfiguration _configuration;
    private readonly string tempdata = "tempdatakey";
    private readonly string newpass = "newpasskey";
    private readonly IWebHostEnvironment _webHostEnvironment;

    public AuthService(IAccountRepository accountRepository, ITokenService tokenService, IEmailService emailService,
        IMemoryCache memoryCache, IMapper mapper, IShopRepository shopRepository,
        IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
    {
        _accountRepository = accountRepository;
        _tokenService = tokenService;
        _emailService = emailService;
        _cache = memoryCache;
        _mapper = mapper;
        _shopRepository = shopRepository;
        _configuration = configuration;
        _webHostEnvironment = webHostEnvironment;
    }

    public async Task<Result<AccountResponse>> ChangeToNewPassword(string confirmtoken)
    {
        var user = await _accountRepository.FindUserByPasswordResetToken(confirmtoken);
        if (user == null || user.ResetTokenExpires < DateTime.UtcNow)
        {
            return new Result<AccountResponse>()
            {
                Messages = ["Invalid token or token is expired"],
                ResultStatus = ResultStatus.Error,
            };
        }
        else
        {
            CreatePasswordHash(_cache.Get<string>(newpass), out byte[] passwordHash, out byte[] passwordSalt);
            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;
            user.ResetTokenExpires = null;
            user.PasswordResetToken = null;
            await _accountRepository.UpdateAccount(user);
            return new Result<AccountResponse>()
            {
                Data = _mapper.Map<AccountResponse>(user),
                Messages = ["Change password successfully"],
                ResultStatus = ResultStatus.Success
            };
        }
    }

    public async Task<Result<string>> CheckPassword(string email, string newPassword)
    {
        var response = new Result<string>();
        var account = await _accountRepository.FindUserByEmail(email);
        if (account is null)
        {
            response.ResultStatus = ResultStatus.NotFound;
            response.Messages = new[] { "User not found" };
            return response;
        }
        else if (VerifyPasswordHash(newPassword, account.PasswordHash, account.PasswordSalt))
        {
            response.ResultStatus = ResultStatus.Duplicated;
            response.Messages = new[] { "This password is duplicated with the old password" };
            return response;
        }
        else
        {
            var cacheEntryOption = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromSeconds(180))
                .SetPriority(CacheItemPriority.Normal);
            _cache.Set(this.newpass, newPassword, cacheEntryOption);
            response = await _emailService.SendMailForgetPassword(email);
            return response;
        }
    }

    public async Task<Result<AccountResponse>> CheckPasswordToChange(Guid accountId, ChangePasswordRequest request)
    {
        var response = new Result<AccountResponse>();
        var account = await _accountRepository.FindOne(c => c.AccountId == accountId);
        if (account is null)
        {
            response.ResultStatus = ResultStatus.NotFound;
            response.Messages = new[] { "User not found" };
            return response;
        }
        else if (!VerifyPasswordHash(request.CurrentPassword, account.PasswordHash, account.PasswordSalt))
        {
            response.ResultStatus = ResultStatus.Error;
            response.Messages = new[] { "Your current is incorrecr" };
            return response;
        }
        else if (VerifyPasswordHash(request.NewPassword, account.PasswordHash, account.PasswordSalt))
        {
            response.ResultStatus = ResultStatus.Error;
            response.Messages = new[] { "This new password is same as the current password. Please enter the new one" };
            return response;
        }

        CreatePasswordHash(request.NewPassword, out byte[] passwordHash, out byte[] passwordSalt);
        account.PasswordHash = passwordHash;
        account.PasswordSalt = passwordSalt;
        await _accountRepository.UpdateAccount(account);
        response.Data = _mapper.Map<AccountResponse>(account);
        response.ResultStatus = ResultStatus.Success;
        response.Messages = new[] { "Change password successfully" };
        return response;
    }

    public async Task<Result<AccountResponse>> CreateStaffAccount(CreateStaffAccountRequest request)
    {
        var isused = await _accountRepository.FindUserByEmail(request.Email);
        var response = new Result<AccountResponse>();
        if (isused != null)
        {
            response.Messages = new[] { "This mail is already used" };
            response.ResultStatus = ResultStatus.Duplicated;
            return response;
        }
        else
        {
            CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);
            Account? account = new Account();
            account.Email = request.Email;
            /*account.AccountId = new Guid();*/
            account.PasswordHash = passwordHash;
            account.PasswordSalt = passwordSalt;
            account.Fullname = request.Fullname;
            account.Phone = request.Phone;
            account.Role = Roles.Staff;
            account.Status = AccountStatus.Active;
            account.CreatedDate = DateTime.UtcNow;
            account.VerifiedAt = DateTime.UtcNow;

            var user = await _accountRepository.Register(account);

            

            response.ResultStatus = ResultStatus.Success;
            response.Messages = ["Create staff successfully"];
            response.Data = _mapper.Map<AccountResponse>(user);
            return response;
            /*var cacheEntryOption = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromSeconds(60))
                .SetPriority(CacheItemPriority.Normal);
            _cache.Set(newuser, account, cacheEntryOption);
            return await SendMailRegister(request.Email);*/
        }
    }

    

    public async Task<Result<LoginResponse>> Login(string email, string password)
    {
        try
        {
            var account = await _accountRepository.FindOne(x =>
                x.Email.Equals(email)
            );
            var admin = _accountRepository.GetAdminAccount(email, password);
            if (account is null && admin is null)
            {
                return new Result<LoginResponse>()
                {
                    ResultStatus = ResultStatus.NotFound,
                    Messages = ["Account is Not Found"]
                };
            }
            else if (admin != null)
            {
                var claimsadmin = new List<Claim>()
                {
                    new(ClaimTypes.Name, admin),
                    new(ClaimTypes.Role, Roles.Admin.ToString())
                };
                var accessTokenadmin = _tokenService.GenerateAccessToken(claimsadmin);

                var dataadmin = new LoginResponse()
                {
                    AccessToken = accessTokenadmin,
                    Email = admin,
                    Role = Roles.Admin,
                };
                return new Result<LoginResponse>()
                {
                    Data = dataadmin,
                    Messages = ["Login successfully. Welcome Admin"],
                    ResultStatus = ResultStatus.Success
                };
            }

            if (!VerifyPasswordHash(password, account.PasswordHash, account.PasswordSalt))
            {
                return new Result<LoginResponse>()
                {
                    ResultStatus = ResultStatus.Error,
                    Messages = ["Password is not correct"]
                };
            }

            if (account.Status is AccountStatus.NotVerified)
            {
                return new Result<LoginResponse>()
                {
                    ResultStatus = ResultStatus.Error,
                    Messages = ["Account is Not Verified! Please verify your account"]
                };
            }
            if (account.Status is AccountStatus.Inactive)
            {
                return new Result<LoginResponse>()
                {
                    ResultStatus = ResultStatus.Error,
                    Messages = ["Your account is banned"]
                };
            }
            var claims = new List<Claim>()
            {
                new Claim("AccountId", account.AccountId.ToString()),
                new(ClaimTypes.Name, account.Email),
                new(ClaimTypes.Role, account.Role.ToString())
            };
            var shop = await _shopRepository.GetQueryable()
                .Select(x=> new
                {
                    StaffId = x.StaffId,
                    ShopId = x.ShopId
                })
                .FirstOrDefaultAsync(x=>x.StaffId == account.AccountId);
            if (account.Role.Equals(Roles.Staff))
            {
                
                if (shop == null)
                {
                    return new Result<LoginResponse>()
                    {
                        Messages = ["Your shop account is not created!"],
                        ResultStatus = ResultStatus.Error
                    };
                }
                claims = new List<Claim>()
                {
                    new Claim("AccountId", account.AccountId.ToString()),
                    new(ClaimTypes.Name, account.Email),
                    new(ClaimTypes.Role, account.Role.ToString()),
                    new Claim("ShopId", shop.ShopId.ToString())
                };
            }
            var accessToken = _tokenService.GenerateAccessToken(claims);

            var data = new LoginResponse()
            {
                AccessToken = accessToken,
                Email = account.Email,
                Role = account.Role,
                Id = account.AccountId,
            };
            if (account.Role.Equals(Roles.Staff))
                data.ShopId = shop?.ShopId;

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

    public async Task<Result<AccountResponse>> Register(RegisterRequest request)
    {
        var isMailUsed = await _accountRepository.FindUserByEmail(request.Email);
        var isPhoneUsed = await _accountRepository.FindUserByPhone(request.Phone);
        var response = new Result<AccountResponse>();
        if (isMailUsed != null)
        {
            response.Messages = new[] { "This mail is already used" };
            response.ResultStatus = ResultStatus.Duplicated;
            return response;
        }

        if (isPhoneUsed != null)
        {
            response.Messages = new[] { "This phone number is already used" };
            response.ResultStatus = ResultStatus.Duplicated;
            return response;
        }

        CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);
        Member? member = new Member
        {
            Email = request.Email,
            PasswordHash = passwordHash,
            PasswordSalt = passwordSalt,
            Fullname = request.Fullname,
            Phone = request.Phone,
            Role = Roles.Member,
            Status = AccountStatus.NotVerified,
            CreatedDate = DateTime.UtcNow
        };

        var user = await _accountRepository.Register(member);

        var token = _accountRepository.CreateRandomToken();
        var cacheEntryOption = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromMinutes(10))
            .SetPriority(CacheItemPriority.Normal);
        _cache.Set(tempdata, token, cacheEntryOption);
        var mail = await _emailService.SendMailRegister(member.Email, token);

        response.ResultStatus = ResultStatus.Success;
        response.Messages = new []{"Register successfully! Please check your email for verification in 10 minutes"};
        response.Data = _mapper.Map<AccountResponse>(user);
        return response;
        
    }

    public async Task<Result<string>> ResendVerifyEmail(string email)
    {
        var response = new Result<string>();
        var user = await _accountRepository.FindUserByEmail(email);
        if (user.VerifiedAt != null)
        {
            response.Messages = ["This account is already verified"];
            response.ResultStatus = ResultStatus.Error;
            return response;
        }

        string appDomain = _configuration.GetSection("MailSettings:AppDomain").Value;
        string confirmationLink = _configuration.GetSection("MailSettings:EmailConfirmation").Value;

        _cache.Remove(tempdata);
        var token = _accountRepository.CreateRandomToken();
        var cacheEntryOption = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromSeconds(190))
            .SetPriority(CacheItemPriority.Normal);
        _cache.Set(tempdata, token, cacheEntryOption);
        /*string formattedLink = string.Format(appDomain + confirmationLink, user.AccountId, token);

        var PathToFile = _webHostEnvironment.WebRootPath + Path.DirectorySeparatorChar.ToString()
                                                         + "MailTemplate" + Path.DirectorySeparatorChar.ToString() +
                                                         "VerifyAccountMail.html";
        *//*var subject = "Confirm Account Registration";*//*
        string HtmlBody = "";
        using (StreamReader streamReader = System.IO.File.OpenText(PathToFile))
        {
            HtmlBody = streamReader.ReadToEnd();
        }

        string Message = $@"<a href=""{formattedLink}"">Click here to verify your email</a>";
        string messageBody = string.Format(HtmlBody, Message);

        SendEmailRequest content = new SendEmailRequest
        {
            To = email,
            Subject = "[GIVEAWAY] Verify Account",
            Body = messageBody,
        };*/
        await _emailService.SendMailRegister(email, token);
        
        response.Messages =
            ["Resend verification email successfully! Please check your email for verification in 3 minutes"];
        response.ResultStatus = ResultStatus.Success;
        return response;
    }

    

    

    public async Task<Result<string>> VerifyEmail(Guid id, string token)
    {
        var response = new Result<string>();
        if (string.IsNullOrEmpty(token))
        {
            response.Messages = ["Token is null"];
            response.ResultStatus = ResultStatus.Error;
            return response;
        }

        if (token.Equals(_cache.Get<string>(tempdata)))
        {
            var user = await _accountRepository.FindOne(c => c.AccountId == id);
            user.VerifiedAt = DateTime.UtcNow;
            user.Status = AccountStatus.Active;
            await _accountRepository.UpdateAccount(user);
            response.Messages = ["Verify successfully at" + DateTime.UtcNow.ToString()];
            response.ResultStatus = ResultStatus.Success;
            return response;
        }

        response.Messages = ["Token is not correct or expired"];
        response.ResultStatus = ResultStatus.Error;
        return response;
    }

    private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
    {
        using (var hmac = new HMACSHA512())
        {
            passwordSalt = hmac.Key;
            passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
        }
    }

    private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
    {
        using (var hmac = new HMACSHA512(passwordSalt))
        {
            var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            return computedHash.SequenceEqual(passwordHash);
        }
    }
}