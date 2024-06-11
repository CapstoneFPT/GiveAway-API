using BusinessObjects.Dtos.Account.Request;
using BusinessObjects.Dtos.Account.Response;
using BusinessObjects.Dtos.Auth;
using BusinessObjects.Dtos.Commons;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.Accounts;

namespace WebApi.Controllers
{
    [Route("api/accounts")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }
        [HttpGet]
        public async Task<ActionResult<List<AccountResponse>>> GetAllAccounts()
        {
            return await _accountService.GetAllAccounts();
        }
        [HttpGet("id")]
        public async Task<ActionResult<Result<AccountResponse>>> GetAccountById(Guid id)
        {
            return await _accountService.GetAccountById(id);
        }
        [HttpPut("ban-account")]
        public async Task<ActionResult<Result<AccountResponse>>> BanAccount(Guid id)
        {
            return await _accountService.BanAccountById(id);
        }
        [HttpPut("update-account")]
        public async Task<ActionResult<Result<AccountResponse>>> UpdateAccount(Guid id, [FromBody]UpdateAccountRequest request)
        {
            return await _accountService.UpdateAccount(id, request);
        }
    }
}
