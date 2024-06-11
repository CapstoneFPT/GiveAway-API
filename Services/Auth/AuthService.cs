using System.Security.Claims;
using System.Security.Cryptography;
using AutoMapper;
using BusinessObjects;
using BusinessObjects.Dtos.Account.Response;
using BusinessObjects.Dtos.Auth;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.Email;
using BusinessObjects.Entities;
using Microsoft.Extensions.Caching.Memory;
using Repositories.Accounts;
using Repositories.Shops;
using Repositories.User;
using Repositories.Wallets;
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
	private readonly IWalletRepository _walletRepository;
	private readonly string newpassword = "newpasscachekey";
	private readonly User newuser = new User();
    public AuthService(IAccountRepository accountRepository, ITokenService tokenService, IEmailService emailService, IMemoryCache memoryCache, IMapper mapper, IShopRepository shopRepository, IWalletRepository walletRepository)
    {
        _accountRepository = accountRepository;
        _tokenService = tokenService;
		_emailService = emailService;
		_cache = memoryCache;
		_mapper = mapper;
		_shopRepository = shopRepository;
		_walletRepository = walletRepository;
    }

    public async Task<Account> ChangeToNewPassword(string confirmtoken)
    {
        var user = await _accountRepository.FindUserByPasswordResetToken(confirmtoken);
        if (user == null || user.ResetTokenExpires < DateTime.Now)
        {
            return null;
        }
        else
        {
            /*user.Password = _cache.Get<string>(newpassword);
            user.ResetTokenExpires = null;
            user.PasswordResetToken = null;*/
            return user;
        }
    }

    public async Task<Result<string>> CheckPassword(string email, string newpass)
    {
        var response = new Result<string>();
        var account = await _accountRepository.FindUserByEmail(email);
        if (account is null)
        {
            response.ResultStatus = ResultStatus.NotFound;
            response.Messages = new[] { "User not found" };
            return response;
        }
		else if (VerifyPasswordHash(newpass, account.PasswordHash, account.PasswordSalt))
		{
			response.ResultStatus = ResultStatus.Duplicated;
			response.Messages = new[] { "This password is duplicated with the old password" };
			return response;
		}
		else
		{
			var cacheEntryOption = new MemoryCacheEntryOptions()
				.SetSlidingExpiration(TimeSpan.FromSeconds(60))
				.SetPriority(CacheItemPriority.Normal);
			_cache.Set(newpassword,newpass,cacheEntryOption);
			response = await SendMail(email);
			return response;
		}
    }

    public async Task<Result<AccountResponse>> CreateStaffAccount(CreateStaffAccountRequest request)
    {
        var isused = await FindUserByEmail(request.Email);
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
            Account account = new Account();
            account.Email = request.Email;
            /*account.AccountId = new Guid();*/
            account.PasswordHash = passwordHash;
            account.PasswordSalt = passwordSalt;
            account.Fullname = request.Fullname;
            account.Phone = request.Phone;
            account.Role = Roles.Staff.ToString();
            account.Status = AccountStatus.Active.ToString();

            var user = await _accountRepository.Register(account);

			Shop shop = new Shop();
			shop.ShopId = new Guid();
			shop.Address = request.Address;
			shop.StaffId = account.AccountId;
			await _shopRepository.CreateShop(shop);

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

    public async Task<Account> FindUserByEmail(string email)
    {
        var result = await _accountRepository.FindUserByEmail(email);
        return result;
    }

    public async Task<Result<LoginResponse>> Login(string email, string password)
    {
        try
        {
            var user = await _accountRepository.FindOne(x =>
                x.Email.Equals(email)
            );

            if (user is null)
            {
                return new Result<LoginResponse>()
                {
                    ResultStatus = ResultStatus.NotFound,
                    Messages = ["Member Not Found"]
                };
            }
            if (!VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
            {
                return new Result<LoginResponse>()
                {
                    ResultStatus = ResultStatus.Error,
                    Messages = ["Password is not correct"]
                };
            }
            if (user.VerifiedAt == null)
			{
                return new Result<LoginResponse>()
                {
                    ResultStatus = ResultStatus.Error,
                    Messages = ["Not Verified"]
                };
            }
			
            var claims = new List<Claim>()
            {
                new(ClaimTypes.NameIdentifier, user.AccountId.ToString()),
                new(ClaimTypes.Name, user.Email),
                new(ClaimTypes.Role, user.Role.ToString())
            };

            var accessToken = _tokenService.GenerateAccessToken(claims);

            var data = new LoginResponse() { AccessToken = accessToken };

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
        var isused = await FindUserByEmail(request.Email);
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
			Account account = new Account();
			account.Email = request.Email;
			/*account.AccountId = new Guid();*/
			account.PasswordHash = passwordHash;
			account.PasswordSalt = passwordSalt;
			account.Fullname = request.Fullname;
			account.Phone = request.Phone;
			account.Role = Roles.Member.ToString();
			account.Status = AccountStatus.Active.ToString();

			var user = await _accountRepository.Register(account);

			Wallet wallet = new Wallet();
			wallet.Balance = 0;
			wallet.MemberId = account.AccountId;
			await _walletRepository.CreateWallet(wallet);
			
			response.ResultStatus = ResultStatus.Success;
			response.Messages = ["Register successfully"];
			response.Data = _mapper.Map<AccountResponse>(user);
			return response;
            /*var cacheEntryOption = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromSeconds(60))
                .SetPriority(CacheItemPriority.Normal);
            _cache.Set(newuser, account, cacheEntryOption);
            return await SendMailRegister(request.Email);*/
		}
    }

    public async Task<Result<string>> SendMail(string email)
    {
        var response = new Result<string>();
        var account = await _accountRepository.FindUserByEmail(email);
        var user = await _accountRepository.ResetPasswordToken(account);
        response.Data = user.PasswordResetToken;
        SendEmailRequest content = new SendEmailRequest
        {
            To = $"{account.Email}",
            Subject = "[GIVEAWAY] Reset Password",
            Body = $@"
<html xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:o=""urn:schemas-microsoft-com:office:office"" lang=""en"">
<head>
	<title></title>
	<meta http-equiv=""Content-Type"" content=""text/html; charset=utf-8"">
	<meta name=""viewport"" content=""width=device-width, initial-scale=1.0""><!--[if mso]><xml><o:OfficeDocumentSettings><o:PixelsPerInch>96</o:PixelsPerInch><o:AllowPNG/></o:OfficeDocumentSettings></xml><![endif]-->
	<style>
		* {{
			box-sizing: border-box;
		}}

		body {{
			margin: 0;
			padding: 0;
		}}

		a[x-apple-data-detectors] {{
			color: inherit !important;
			text-decoration: inherit !important;
		}}

		#MessageViewBody a {{
			color: inherit;
			text-decoration: none;
		}}

		p {{
			line-height: inherit
		}}

		.desktop_hide,
		.desktop_hide table {{
			mso-hide: all;
			display: none;
			max-height: 0px;
			overflow: hidden;
		}}

		.image_block img+div {{
			display: none;
		}}

		@media (max-width:520px) {{
			.desktop_hide table.icons-inner {{
				display: inline-block !important;
			}}

			.icons-inner {{
				text-align: center;
			}}

			.icons-inner td {{
				margin: 0 auto;
			}}

			.image_block img.fullWidth {{
				max-width: 100% !important;
			}}

			.social_block.desktop_hide .social-table {{
				display: inline-block !important;
			}}

			.row-content {{
				width: 100% !important;
			}}

			.stack .column {{
				width: 100%;
				display: block;
			}}

			.mobile_hide {{
				max-width: 0;
				min-height: 0;
				max-height: 0;
				font-size: 0;
				display: none;
				overflow: hidden;
			}}

			.desktop_hide,
			.desktop_hide table {{
				max-height: none !important;
				display: table !important;
			}}
		}}
	</style>
</head>

<body style=""text-size-adjust: none; background-color: #fff; margin: 0; padding: 0;"">
	<table class=""nl-container"" width=""100%"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt; background-color: #fff;"">
		<tbody>
			<tr>
				<td>
					<table class=""row row-1"" align=""center"" width=""100%"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt; background-color: #f5f5f5;"">
						<tbody>
							<tr>
								<td>
									<table class=""row-content stack"" align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt; color: #000; width: 500px; margin: 0 auto;"" width=""500"">
										<tbody>
											<tr>
												<td class=""column column-1"" width=""100%"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt; text-align: left; font-weight: 400; padding-bottom: 5px; vertical-align: top; border-top: 0px; border-right: 0px; border-bottom: 0px; border-left: 0px;"">
													<table class=""image_block block-1"" width=""100%"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt;"">
														<tr>
															<td class=""pad"" style=""padding-bottom:10px;width:100%;padding-right:0px;padding-left:0px;"">
																<div class=""alignment"" align=""center"" style=""line-height:10px""><img src=""https://d1oco4z2z1fhwp.cloudfront.net/templates/default/2966/LOGO.png"" style=""height: auto; display: block; border: 0; max-width: 125px; width: 100%;"" width=""125"" alt=""your-logo"" title=""your-logo""></div>
															</td>
														</tr>
													</table>
												</td>
											</tr>
										</tbody>
									</table>
								</td>
							</tr>
						</tbody>
					</table>
					<table class=""row row-2"" align=""center"" width=""100%"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt; background-color: #f5f5f5;"">
						<tbody>
							<tr>
								<td>
									<table class=""row-content stack"" align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt; color: #000; background-color: #fff; width: 500px; margin: 0 auto;"" width=""500"">
										<tbody>
											<tr>
												<td class=""column column-1"" width=""100%"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt; text-align: left; font-weight: 400; padding-bottom: 20px; padding-top: 15px; vertical-align: top; border-top: 0px; border-right: 0px; border-bottom: 0px; border-left: 0px;"">
													<table class=""image_block block-1"" width=""100%"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt;"">
														<tr>
															<td class=""pad"" style=""padding-bottom:5px;padding-left:5px;padding-right:5px;width:100%;"">
																<div class=""alignment"" align=""center"" style=""line-height:10px""><img class=""fullWidth"" src=""https://d1oco4z2z1fhwp.cloudfront.net/templates/default/2966/gif-resetpass.gif"" style=""height: auto; display: block; border: 0; max-width: 350px; width: 100%;"" width=""350"" alt=""reset-password"" title=""reset-password""></div>
															</td>
														</tr>
													</table>
													<table class=""heading_block block-2"" width=""100%"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt;"">
														<tr>
															<td class=""pad"" style=""text-align:center;width:100%;"">
																<h1 style=""margin: 0; color: #393d47; direction: ltr; font-family: Tahoma, Verdana, Segoe, sans-serif; font-size: 25px; font-weight: normal; letter-spacing: normal; line-height: 120%; text-align: center; margin-top: 0; margin-bottom: 0;""><strong>Forgot your password?</strong></h1>
															</td>
														</tr>
													</table>
													<table class=""text_block block-3"" width=""100%"" border=""0"" cellpadding=""10"" cellspacing=""0"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt; word-break: break-word;"">
														<tr>
															<td class=""pad"">
																<div style=""font-family: Tahoma, Verdana, sans-serif"">
																	<div class style=""font-size: 12px; font-family: Tahoma, Verdana, Segoe, sans-serif; mso-line-height-alt: 18px; color: #393d47; line-height: 1.5;"">
																		<p style=""margin: 0; font-size: 14px; text-align: center; mso-line-height-alt: 21px;""><span style=""font-size:14px;""><span style>User <strong>{account.Email}</strong><br> </span><span style>Not to worry, we got you! </span><span style>Let’s get you a new password.</span></span></p>
																	</div>
																</div>
															</td>
														</tr>
													</table>
													<table class=""button_block block-4"" width=""100%"" border=""0"" cellpadding=""15"" cellspacing=""0"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt;"">
														<tr>
															<td class=""pad"">
																<div class=""alignment"" align=""center""><!--[if mso]><v:roundrect xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:w=""urn:schemas-microsoft-com:office:word"" style=""height:58px;width:272px;v-text-anchor:middle;"" arcsize=""35%"" strokeweight=""0.75pt"" strokecolor=""#FFC727"" fillcolor=""#ffc727""><w:anchorlock/><v:textbox inset=""0px,0px,0px,0px""><center style=""color:#000000; font-family:Tahoma, Verdana, sans-serif; font-size:20px""><![endif]--><a target=""_blank"" style=""text-decoration:none;display:inline-block;color:#393d47;background-color:#ffc727;border-radius:20px;width:auto;border-top:1px solid #FFC727;font-weight:undefined;border-right:1px solid #FFC727;border-bottom:1px solid #FFC727;border-left:1px solid #FFC727;padding-top:10px;padding-bottom:10px;font-family:Tahoma, Verdana, Segoe, sans-serif;font-size:18px;text-align:center;mso-border-alt:none;word-break:keep-all;""><span style=""padding-left:50px;padding-right:50px;font-size:18px;display:inline-block;letter-spacing:normal;""><span style=""word-break:break-word;""><span style=""line-height: 36px; color: #000000"" data-mce-style><strong>{response.Data}</strong></span></span></span></a><!--[if mso]></center></v:textbox></v:roundrect><![endif]--></div>
															</td>
														</tr>
													</table>
													<table class=""text_block block-5"" width=""100%"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt; word-break: break-word;"">
														<tr>
															<td class=""pad"" style=""padding-bottom:5px;padding-left:10px;padding-right:10px;padding-top:10px;"">
																<div style=""font-family: Tahoma, Verdana, sans-serif"">
																	<div class style=""font-size: 12px; font-family: Tahoma, Verdana, Segoe, sans-serif; text-align: center; mso-line-height-alt: 18px; color: #393d47; line-height: 1.5;"">
																		<p style=""margin: 0; mso-line-height-alt: 19.5px;""><span style=""font-size:13px;"">If you didn’t request to change your password, simply ignore this email.</span></p>
                                                                        
																	</div>
																</div>
															</td>
														</tr>
													</table>
												</td>
											</tr>
										</tbody>
									</table>
								</td>
							</tr>
						</tbody>
					</table>
					<table class=""row row-3"" align=""center"" width=""100%"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt; background-color: #f5f5f5;"">
						<tbody>
							<tr>
								<td>
									<table class=""row-content stack"" align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt; color: #000; width: 500px; margin: 0 auto;"" width=""500"">
										<tbody>
											<tr>
												<td class=""column column-1"" width=""100%"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt; text-align: left; font-weight: 400; padding-bottom: 5px; padding-top: 5px; vertical-align: top; border-top: 0px; border-right: 0px; border-bottom: 0px; border-left: 0px;"">
													<table class=""text_block block-1"" width=""100%"" border=""0"" cellpadding=""15"" cellspacing=""0"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt; word-break: break-word;"">
														<tr>
															<td class=""pad"">
																<div style=""font-family: Tahoma, Verdana, sans-serif"">
																	<div class style=""font-size: 12px; font-family: Tahoma, Verdana, Segoe, sans-serif; mso-line-height-alt: 14.399999999999999px; color: #393d47; line-height: 1.2;"">
																		<p style=""margin: 0; font-size: 14px; text-align: center; mso-line-height-alt: 16.8px;""><span style=""font-size:10px;"">This link will&nbsp;expire in 24 hours.&nbsp;If you continue to have problems</span><br><span style=""font-size:10px;"">please feel free to contact us at <a href=""mailto:support@youremail.com"" target=""_blank"" title=""support@youremail.com"" style=""text-decoration: underline; color: #393d47;"" rel=""noopener"">support@youremail.com</a>. <a href=""Example.com"" target=""_blank"" style=""text-decoration: underline; color: #393d47;"" rel=""noopener"">UNSUBSCRIBE</a></span></p>
																	</div>
																</div>
															</td>
														</tr>
													</table>
												</td>
											</tr>
										</tbody>
									</table>
								</td>
							</tr>
						</tbody>
					</table>
					<table class=""row row-4"" align=""center"" width=""100%"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt; background-color: #fff;"">
						<tbody>
							<tr>
								<td>
									<table class=""row-content stack"" align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt; color: #000; width: 500px; margin: 0 auto;"" width=""500"">
										<tbody>
											<tr>
												<td class=""column column-1"" width=""100%"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt; text-align: left; font-weight: 400; padding-bottom: 5px; padding-top: 5px; vertical-align: top; border-top: 0px; border-right: 0px; border-bottom: 0px; border-left: 0px;"">
													<table class=""html_block block-1"" width=""100%"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt;"">
														<tr>
															<td class=""pad"">
																<div style=""font-family:Arial, Helvetica Neue, Helvetica, sans-serif;text-align:center;"" align=""center""><div style=""height:30px;"">&nbsp;</div></div>
															</td>
														</tr>
													</table>
													<table class=""social_block block-2"" width=""100%"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt;"">
														<tr>
															<td class=""pad"" style=""text-align:center;padding-right:0px;padding-left:0px;"">
																<div class=""alignment"" align=""center"">
																	<table class=""social-table"" width=""168px"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt; display: inline-block;"">
																		<tr>
																			<td style=""padding:0 5px 0 5px;""><a href=""https://www.facebook.com"" target=""_blank""><img src=""https://app-rsrc.getbee.io/public/resources/social-networks-icon-sets/t-outline-circle-default-gray/facebook@2x.png"" width=""32"" height=""32"" alt=""Facebook"" title=""Facebook"" style=""height: auto; display: block; border: 0;""></a></td>
																			<td style=""padding:0 5px 0 5px;""><a href=""https://www.twitter.com"" target=""_blank""><img src=""https://app-rsrc.getbee.io/public/resources/social-networks-icon-sets/t-outline-circle-default-gray/twitter@2x.png"" width=""32"" height=""32"" alt=""Twitter"" title=""Twitter"" style=""height: auto; display: block; border: 0;""></a></td>
																			<td style=""padding:0 5px 0 5px;""><a href=""https://www.instagram.com"" target=""_blank""><img src=""https://app-rsrc.getbee.io/public/resources/social-networks-icon-sets/t-outline-circle-default-gray/instagram@2x.png"" width=""32"" height=""32"" alt=""Instagram"" title=""Instagram"" style=""height: auto; display: block; border: 0;""></a></td>
																			<td style=""padding:0 5px 0 5px;""><a href=""https://www.linkedin.com"" target=""_blank""><img src=""https://app-rsrc.getbee.io/public/resources/social-networks-icon-sets/t-outline-circle-default-gray/linkedin@2x.png"" width=""32"" height=""32"" alt=""LinkedIn"" title=""LinkedIn"" style=""height: auto; display: block; border: 0;""></a></td>
																		</tr>
																	</table>
																</div>
															</td>
														</tr>
													</table>
													<table class=""html_block block-3"" width=""100%"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt;"">
														<tr>
															<td class=""pad"">
																<div style=""font-family:Arial, Helvetica Neue, Helvetica, sans-serif;text-align:center;"" align=""center""><div style=""margin-top: 25px;border-top:1px dashed #D6D6D6;margin-bottom: 20px;""></div></div>
															</td>
														</tr>
													</table>
													<table class=""text_block block-4"" width=""100%"" border=""0"" cellpadding=""10"" cellspacing=""0"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt; word-break: break-word;"">
														<tr>
															<td class=""pad"">
																<div style=""font-family: Tahoma, Verdana, sans-serif"">
																	<div class style=""font-size: 12px; font-family: Tahoma, Verdana, Segoe, sans-serif; mso-line-height-alt: 14.399999999999999px; color: #C0C0C0; line-height: 1.2;"">
																		<p style=""margin: 0; text-align: center; mso-line-height-alt: 14.399999999999999px;"">Duis euismod neque at lacus rutrum, nec suscipit eros tincidunt nterdum et malesuada.</p>
																		<p style=""margin: 0; text-align: center; mso-line-height-alt: 14.399999999999999px;"">Fames ac ante ipsum vestibulum.</p>
																		<p style=""margin: 0; text-align: center; mso-line-height-alt: 14.399999999999999px;"">&nbsp;</p>
																		<p style=""margin: 0; text-align: center; mso-line-height-alt: 14.399999999999999px;"">Your Street 12, 34567 AB City&nbsp; /&nbsp;&nbsp;info@example.com /&nbsp;(+1) 123 456 789<a href=""http://www.example.com"" style></a></p>
																		<p style=""margin: 0; font-size: 12px; text-align: center; mso-line-height-alt: 14.399999999999999px;""><span style=""color:#c0c0c0;"">&nbsp;</span></p>
																	</div>
																</div>
															</td>
														</tr>
													</table>
													<table class=""html_block block-5"" width=""100%"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt;"">
														<tr>
															<td class=""pad"">
																<div style=""font-family:Arial, Helvetica Neue, Helvetica, sans-serif;text-align:center;"" align=""center""><div style=""height-top: 20px;"">&nbsp;</div></div>
															</td>
														</tr>
													</table>
												</td>
											</tr>
										</tbody>
									</table>
								</td>
							</tr>
						</tbody>
					</table>
					<table class=""row row-5"" align=""center"" width=""100%"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt;"">
						<tbody>
							<tr>
								<td>
									<table class=""row-content stack"" align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt; color: #000; width: 500px; margin: 0 auto;"" width=""500"">
										<tbody>
											<tr>
												<td class=""column column-1"" width=""100%"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt; text-align: left; font-weight: 400; padding-bottom: 5px; padding-top: 5px; vertical-align: top; border-top: 0px; border-right: 0px; border-bottom: 0px; border-left: 0px;"">
													<table class=""icons_block block-1"" width=""100%"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt;"">
														<tr>
															<td class=""pad"" style=""vertical-align: middle; color: #9d9d9d; font-family: inherit; font-size: 15px; padding-bottom: 5px; padding-top: 5px; text-align: center;"">
																<table width=""100%"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt;"">
																	<tr>
																		<td class=""alignment"" style=""vertical-align: middle; text-align: center;""><!--[if vml]><table align=""left"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""display:inline-block;padding-left:0px;padding-right:0px;mso-table-lspace: 0pt;mso-table-rspace: 0pt;""><![endif]-->
																			<!--[if !vml]><!-->
																			<table class=""icons-inner"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt; display: inline-block; margin-right: -4px; padding-left: 0px; padding-right: 0px;"" cellpadding=""0"" cellspacing=""0"" role=""presentation""><!--<![endif]-->
																				<tr>
																					<td style=""vertical-align: middle; text-align: center; padding-top: 5px; padding-bottom: 5px; padding-left: 5px; padding-right: 6px;""><a href=""https://www.designedwithbee.com/"" target=""_blank"" style=""text-decoration: none;""><img class=""icon"" alt=""Designed with BEE"" src=""https://d15k2d11r6t6rl.cloudfront.net/public/users/Integrators/BeeProAgency/53601_510656/Signature/bee.png"" height=""32"" width=""34"" align=""center"" style=""height: auto; display: block; margin: 0 auto; border: 0;""></a></td>
																					<td style=""font-family: Arial, Helvetica Neue, Helvetica, sans-serif; font-size: 15px; color: #9d9d9d; vertical-align: middle; letter-spacing: undefined; text-align: center;""><a href=""https://www.designedwithbee.com/"" target=""_blank"" style=""color: #9d9d9d; text-decoration: none;"">Designed with BEE</a></td>
																				</tr>
																			</table>
																		</td>
																	</tr>
																</table>
															</td>
														</tr>
													</table>
												</td>
											</tr>
										</tbody>
									</table>
								</td>
							</tr>
						</tbody>
					</table>
				</td>
			</tr>
		</tbody>
	</table><!-- End -->
</body>
</html>"
        };
        await _emailService.SendEmail(content);
        response.Messages = ["Please check your mail to get the token to reset password"];
        response.ResultStatus = ResultStatus.Success;

        return response;
            
    }
	public async Task<Result<string>> SendMailRegister(string email)
	{
		var response = new Result<string>();
		SendEmailRequest content = new SendEmailRequest
		{
			To = email,
			Subject = "[GIVEAWAY] Verify Account",
			Body = $@"<a href="""">Verify</a>",
		};
		await _emailService.SendEmail(content);
		response.Messages = ["Please check your email for verification"];
		response.ResultStatus = ResultStatus.Success;
		return response;
	}

    public Task<Result<string>> VerifyEmail(string email)
    {
        throw new NotImplementedException();
    }
	private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
	{
		using(var hmac = new HMACSHA512())
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
