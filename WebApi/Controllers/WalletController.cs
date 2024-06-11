using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.Wallet;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.Wallets;

namespace WebApi.Controllers
{
    [Route("api/wallets")]
    [ApiController]
    public class WalletController : ControllerBase
    {
        private readonly IWalletService _wallet;

        public WalletController(IWalletService wallet)
        {
            _wallet = wallet;
        }
        [HttpGet("account-id")]
        public async Task<ActionResult<Result<WalletResponse>>> GetWalletByAccountId(Guid accountId)
        {
            return await _wallet.GetWalletByAccountId(accountId);
        }
        [HttpPut("update")]
        public async Task<ActionResult<Result<WalletResponse>>> UpdateWallet(Guid accountId, [FromBody]UpdateWalletRequest request)
        {
            return await _wallet.UpdateWallet(accountId, request);
        }
    }
}
