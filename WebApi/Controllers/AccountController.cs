using BusinessObjects.Dtos.Account.Request;
using BusinessObjects.Dtos.Account.Response;
using BusinessObjects.Dtos.Auth;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.Deliveries;
using BusinessObjects.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.Accounts;
using Services.Deliveries;

namespace WebApi.Controllers
{
    [Route("api/accounts")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;
        private readonly IDeliveryService _deliveryService;

        public AccountController(IAccountService accountService, IDeliveryService deliveryService)
        {
            _accountService = accountService;
            _deliveryService = deliveryService;
        }
        [HttpGet]
        public async Task<ActionResult<List<AccountResponse>>> GetAllAccounts()
        {
            return await _accountService.GetAllAccounts();
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<Result<AccountResponse>>> GetAccountById(Guid id)
        {
            return await _accountService.GetAccountById(id);
        }
        [HttpPut("{id}/ban")]
        public async Task<ActionResult<Result<AccountResponse>>> BanAccount([FromRoute]Guid id)
        {
            return await _accountService.BanAccountById(id);
        }
        [HttpPut("{accountId}")]
        public async Task<ActionResult<Result<AccountResponse>>> UpdateAccount([FromRoute]Guid accountId, [FromBody]UpdateAccountRequest request)
        {
            return await _accountService.UpdateAccount(accountId, request);
        }
        [HttpGet("{accountId}/deliveries")]
        public async Task<ActionResult<Result<List<DeliveryResponse>>>> GetAllDeliveriesByMemberId([FromRoute] Guid accountId)
        {
            return await _deliveryService.GetAllDeliveriesByMemberId(accountId);
        }
        [HttpPost("{accountId}/deliveries")]
        public async Task<ActionResult<Result<DeliveryResponse>>> CreateDelivery([FromRoute] Guid accountId, [FromBody] DeliveryRequest deliveryRequest)
        {
            return await _deliveryService.CreateDelivery(accountId, deliveryRequest);
        }
        [HttpPut("{accountId}/deliveries/{deliveryId}")]
        public async Task<ActionResult<Result<DeliveryResponse>>> UpdateDelivery([FromRoute] Guid deliveryId, [FromBody] DeliveryRequest deliveryRequest)
        {
            return await _deliveryService.UpdateDelivery(deliveryId, deliveryRequest);
        }
        [HttpDelete("{accountId}/deliveries/{deliveryId}")]
        public async Task<ActionResult<Result<string>>> DeleteDelivery([FromRoute] Guid deliveryId)
        {
            return await _deliveryService.DeleteDelivery(deliveryId);
        }
      
    }
}
