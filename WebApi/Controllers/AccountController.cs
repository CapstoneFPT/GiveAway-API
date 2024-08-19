using System.Net;
using BusinessObjects.Dtos.Account;
using BusinessObjects.Dtos.Account.Request;
using BusinessObjects.Dtos.Account.Response;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.ConsignSales;
using BusinessObjects.Dtos.Deliveries;
using BusinessObjects.Dtos.Inquiries;
using BusinessObjects.Dtos.Orders;
using BusinessObjects.Dtos.Transactions;
using BusinessObjects.Dtos.Withdraws;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Services.Accounts;
using Services.ConsignSales;
using Services.Deliveries;
using Services.OrderDetails;
using Services.Orders;

namespace WebApi.Controllers;

[Route("api/accounts")]
[ApiController]
public class AccountController : ControllerBase
{
    private readonly IAccountService _accountService;
    private readonly IConsignSaleService _consignSaleService;
    private readonly IDeliveryService _deliveryService;
    private readonly IOrderDetailService _orderDetailService;
    private readonly IOrderService _orderService;

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
    [ProducesResponseType<PaginationResponse<AccountResponse>>((int)HttpStatusCode.OK)]
    public async Task<IActionResult> GetAccounts(
        [FromQuery] GetAccountsRequest request)
    {
        var result = await _accountService.GetAccounts(request);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType<Result<AccountResponse>>((int)HttpStatusCode.OK)]
    public async Task<IActionResult> GetAccountById(Guid id)
    {
        var result = await _accountService.GetAccountById(id);

        if (result.ResultStatus != ResultStatus.Success)
            return StatusCode((int)HttpStatusCode.InternalServerError, result);

        return Ok(result);
    }

    [HttpPut("{id}/ban")]
    [ProducesResponseType<Result<AccountResponse>>((int)HttpStatusCode.OK)]
    public async Task<IActionResult> BanAccount([FromRoute] Guid id)
    {
        var result = await _accountService.BanAccountById(id);

        if (result.ResultStatus != ResultStatus.Success)
            return StatusCode((int)HttpStatusCode.InternalServerError, result);

        return Ok(result);
    }

    [HttpPut("{accountId}")]
    [ProducesResponseType<Result<AccountResponse>>((int)HttpStatusCode.OK)]
    public async Task<IActionResult> UpdateAccount([FromRoute] Guid accountId,
        [FromBody] UpdateAccountRequest request)
    {
        var result = await _accountService.UpdateAccount(accountId, request);

        if (result.ResultStatus != ResultStatus.Success)
            return StatusCode((int)HttpStatusCode.InternalServerError, result);

        return Ok(result);
    }

    [HttpGet("{accountId}/deliveries")]
    [ProducesResponseType<Result<List<DeliveryListResponse>>>((int)HttpStatusCode.OK)]
    [ProducesResponseType<ErrorResponse>((int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> GetAllDeliveriesByMemberId(
        [FromRoute] Guid accountId)
    {
        var result = await _deliveryService.GetAllDeliveriesByMemberId(accountId);

        if (result.ResultStatus != ResultStatus.Success)
            return StatusCode((int)HttpStatusCode.InternalServerError, result);

        return Ok(result);
    }

    [HttpPost("{accountId}/deliveries")]
    [ProducesResponseType<Result<DeliveryListResponse>>((int)HttpStatusCode.OK)]
    public async Task<IActionResult> CreateDelivery([FromRoute] Guid accountId,
        [FromBody] DeliveryRequest deliveryRequest)
    {
        var result = await _deliveryService.CreateDelivery(accountId, deliveryRequest);
        
        if (result.ResultStatus != ResultStatus.Success)
            return StatusCode((int)HttpStatusCode.InternalServerError, result);

        return Ok(result);
    }

    [HttpPut("{accountId}/deliveries/{deliveryId}")]
    [ProducesResponseType<Result<DeliveryListResponse>>((int)HttpStatusCode.OK)]
    public async Task<IActionResult> UpdateDelivery([FromRoute] Guid deliveryId,
        [FromBody] UpdateDeliveryRequest deliveryRequest)
    {
        var result = await _deliveryService.UpdateDelivery(deliveryId, deliveryRequest);

        if (result.ResultStatus != ResultStatus.Success)
            return StatusCode((int)HttpStatusCode.InternalServerError, result);

        return Ok(result);
    }

    [HttpDelete("{accountId}/deliveries/{deliveryId}")]
    [ProducesResponseType<Result<string>>((int)HttpStatusCode.OK)]
    public async Task<IActionResult> DeleteDelivery([FromRoute] Guid deliveryId)
    {
        var result = await _deliveryService.DeleteDelivery(deliveryId);

        if (result.ResultStatus != ResultStatus.Success)
            return StatusCode((int)HttpStatusCode.InternalServerError, result);

        return Ok(result);
    }

    [HttpGet("{accountId}/orders")]
    [ProducesResponseType<Result<PaginationResponse<OrderResponse>>>((int)HttpStatusCode.OK)]
    public async Task<IActionResult> GetOrdersByAccountId(
        [FromRoute] Guid accountId, [FromQuery] OrderRequest request)
    {
        var result = await _orderService.GetOrdersByAccountId(accountId, request);

        if (result.ResultStatus != ResultStatus.Success)
            return StatusCode((int)HttpStatusCode.InternalServerError, result);

        return Ok(result);
    }

    [HttpPost("{accountId}/orders")]
    [ProducesResponseType<Result<OrderResponse>>((int)HttpStatusCode.OK)]
    public async Task<IActionResult> CreateOrder([FromRoute] Guid accountId,
        [FromBody] CartRequest cart)
    {
        var result = await _orderService.CreateOrder(accountId, cart);

        if (result.ResultStatus != ResultStatus.Success)
            return StatusCode((int)HttpStatusCode.InternalServerError, result);

        return Ok(result);
    }

    [HttpGet("{accountId}/consignsales")]
    [ProducesResponseType<Result<PaginationResponse<ConsignSaleResponse>>>((int)HttpStatusCode.OK)]
    public async Task<IActionResult> GetAllConsignSale(
        [FromRoute] Guid accountId, [FromQuery] ConsignSaleRequest request)
    {
        var result = await _consignSaleService.GetAllConsignSales(accountId, request);

        if (result.ResultStatus != ResultStatus.Success)
            return StatusCode((int)HttpStatusCode.InternalServerError, result);

        return Ok(result);
    }

    [HttpPost("{accountId}/consignsales")]
    [ProducesResponseType<Result<ConsignSaleResponse>>((int)HttpStatusCode.OK)]
    public async Task<IActionResult> CreateConsignSale([FromRoute] Guid accountId,
        [FromBody] CreateConsignSaleRequest request)
    {
        var result = await _consignSaleService.CreateConsignSale(accountId, request);

        if (result.ResultStatus != ResultStatus.Success)
            return StatusCode((int)HttpStatusCode.InternalServerError, result);

        return Ok(result);
    }

    [HttpPost("{accountId}/inquiries")]
    [ProducesResponseType<Result<CreateInquiryResponse>>((int)HttpStatusCode.OK)]
    public async Task<IActionResult> CreateInquiry([FromRoute] Guid accountId,
        [FromBody] CreateInquiryRequest request)
    {
        var result = await _accountService.CreateInquiry(accountId, request);
        return Ok(result);
    }

    [HttpPost("{accountId}/withdraws")]
    [ProducesResponseType<Result<CreateWithdrawResponse>>((int)HttpStatusCode.OK)]
    public async Task<IActionResult> CreateWithdraw([FromRoute] Guid accountId,
        [FromBody] CreateWithdrawRequest request)
    {
        var result = await _accountService.RequestWithdraw(accountId, request);
        return Ok(result);
    }

    [HttpGet("{accountId}/withdraws")]
    [ProducesResponseType<Result<PaginationResponse<GetWithdrawsResponse>>>((int)HttpStatusCode.OK)]
    public async Task<IActionResult> GetWithdraws(
        [FromRoute] Guid accountId,
        [FromQuery] GetWithdrawsRequest request)
    {
        var result = await _accountService.GetWithdraws(accountId, request);
        return Ok(result);
    }

    [HttpGet("{accountId}/transactions")]
    [ProducesResponseType<Result<PaginationResponse<GetTransactionsResponse>>>((int)HttpStatusCode.OK)]
    public async Task<IActionResult> GetTransactions([FromRoute] Guid accountId,
        [FromQuery] GetTransactionsRequest request)
    {
        var result = await _accountService.GetTransactions(accountId, request);
        return Ok(result);
    }
}