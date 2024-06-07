using BusinessObjects.Dtos.Account.Response;
using BusinessObjects.Dtos.Commons;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.Accounts;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }
        [HttpGet("getallaccount")]
        public async Task<ActionResult<List<AccountResponse>>> GetAllAccounts()
        {
            return await _accountService.GetAllAccounts();
        }
        [HttpGet("getaccountbyid")]
        public async Task<ActionResult<Result<AccountResponse>>> GetAccountById(Guid id)
        {
            return await _accountService.GetAccountById(id);
        }
    }
}
