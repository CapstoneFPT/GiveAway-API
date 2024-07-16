using BusinessObjects.Dtos.Account;
using BusinessObjects.Dtos.Account.Request;
using BusinessObjects.Dtos.Account.Response;
using BusinessObjects.Dtos.Auth;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.ConsignSales;
using BusinessObjects.Dtos.Deliveries;
using BusinessObjects.Dtos.Inquiries;
using BusinessObjects.Dtos.Orders;
using BusinessObjects.Dtos.Refunds;
using BusinessObjects.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.Accounts;
using Services.ConsignSales;
using Services.Deliveries;
using Services.OrderDetails;
using Services.Orders;

namespace WebApi.Controllers
{
    [Route("api/accounts")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;
        private readonly IDeliveryService _deliveryService;
        private readonly IOrderService _orderService;
        private readonly IConsignSaleService _consignSaleService;
        private readonly IOrderDetailService _orderDetailService;
        public AccountController(IAccountService accountService, IDeliveryService deliveryService, 
            IOrderService orderService, IConsignSaleService consignSaleService, IOrderDetailService orderDetailService)
        {
            _accountService = accountService;
            _deliveryService = deliveryService;
            _orderService = orderService;
            _consignSaleService = consignSaleService;
            _orderDetailService = orderDetailService;
        }

        [HttpGet]
        public async Task<ActionResult<PaginationResponse<AccountResponse>>> GetAccounts(
            [FromQuery] GetAccountsRequest request)
        {
            var result = await _accountService.GetAccounts(request);
            return Ok(result);
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
        [HttpGet("{accountId}/orders")]
        public async Task<ActionResult<Result<PaginationResponse<OrderResponse>>>> GetOrdersByAccountId([FromRoute] Guid accountId, [FromQuery] OrderRequest request)
        {
            return await _orderService.GetOrdersByAccountId(accountId, request);
        }
        [HttpPost("{accountId}/orders")]
        public async Task<ActionResult<Result<OrderResponse>>> CreateOrder([FromRoute] Guid accountId, [FromBody] CartRequest cart)
        {
            return await _orderService.CreateOrder(accountId, cart);
        }
        [HttpGet("{accountId}/consignsales")]
        public async Task<ActionResult<Result<PaginationResponse<ConsignSaleResponse>>>> GetAllConsignSale([FromRoute] Guid accountId, [FromQuery] ConsignSaleRequest request)
        {
            return await _consignSaleService.GetAllConsignSales(accountId, request);
        }

        [HttpPost("{accountId}/consignsales")]
        public async Task<ActionResult<Result<ConsignSaleResponse>>> CreateConsignSale([FromRoute] Guid accountId, [FromBody] CreateConsignSaleRequest request)
        {
            return await _consignSaleService.CreateConsignSale(accountId, request);
        }

        [HttpPost("{accountId}/orderdetails/{orderdetailId}/refunds")]
        public async Task<ActionResult<Result<RefundResponse>>> RequestRefundItemToShop([FromRoute] Guid accountId,
            [FromRoute] Guid orderdetailId,
            [FromBody] CreateRefundRequest refundRequest)
        {
            return await _orderDetailService.RequestRefundToShop(accountId, orderdetailId, refundRequest);
        }

        [HttpPost("{accountId}/inquiries")]
        public async Task<ActionResult<CreateInquiryResponse>> CreateInquiry([FromRoute] Guid accountId,
            [FromBody] CreateInquiryRequest request)
        {
            var result = await _accountService.CreateInquiry(accountId, request);
            return Ok(result);
        }
        
    }
}
