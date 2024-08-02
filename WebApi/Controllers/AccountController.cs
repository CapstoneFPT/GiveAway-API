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
[EnableCors("AllowAll")]
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
    public async Task<ActionResult<PaginationResponse<AccountResponse>>> GetAccounts(
        [FromQuery] GetAccountsRequest request)
    {
        var result = await _accountService.GetAccounts(request);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Result<AccountResponse>>> GetAccountById(Guid id)
    {
        var result = await _accountService.GetAccountById(id);

        if (result.ResultStatus != ResultStatus.Success)
            return StatusCode((int)HttpStatusCode.InternalServerError, result);

        return Ok(result);
    }

    [HttpPut("{id}/ban")]
    public async Task<ActionResult<Result<AccountResponse>>> BanAccount([FromRoute] Guid id)
    {
        var result = await _accountService.BanAccountById(id);

        if (result.ResultStatus != ResultStatus.Success)
            return StatusCode((int)HttpStatusCode.InternalServerError, result);

        return Ok(result);
    }

    [HttpPut("{accountId}")]
    public async Task<ActionResult<Result<AccountResponse>>> UpdateAccount([FromRoute] Guid accountId,
        [FromBody] UpdateAccountRequest request)
    {
        var result = await _accountService.UpdateAccount(accountId, request);

        if (result.ResultStatus != ResultStatus.Success)
            return StatusCode((int)HttpStatusCode.InternalServerError, result);

        return Ok(result);
    }

    [HttpGet("{accountId}/deliveries")]
    public async Task<ActionResult<Result<List<DeliveryResponse>>>> GetAllDeliveriesByMemberId(
        [FromRoute] Guid accountId)
    {
        var result = await _deliveryService.GetAllDeliveriesByMemberId(accountId);

        if (result.ResultStatus != ResultStatus.Success)
            return StatusCode((int)HttpStatusCode.InternalServerError, result);

        return Ok(result);
    }

    [HttpPost("{accountId}/deliveries")]
    public async Task<ActionResult<Result<DeliveryResponse>>> CreateDelivery([FromRoute] Guid accountId,
        [FromBody] DeliveryRequest deliveryRequest)
    {
        var result = await _deliveryService.CreateDelivery(accountId, deliveryRequest);
        
        if (result.ResultStatus != ResultStatus.Success)
            return StatusCode((int)HttpStatusCode.InternalServerError, result);

        return Ok(result);
    }

    [HttpPut("{accountId}/deliveries/{deliveryId}")]
    public async Task<ActionResult<Result<DeliveryResponse>>> UpdateDelivery([FromRoute] Guid deliveryId,
        [FromBody] UpdateDeliveryRequest deliveryRequest)
    {
        var result = await _deliveryService.UpdateDelivery(deliveryId, deliveryRequest);

        if (result.ResultStatus != ResultStatus.Success)
            return StatusCode((int)HttpStatusCode.InternalServerError, result);

        return Ok(result);
    }

    [HttpDelete("{accountId}/deliveries/{deliveryId}")]
    public async Task<ActionResult<Result<string>>> DeleteDelivery([FromRoute] Guid deliveryId)
    {
        var result = await _deliveryService.DeleteDelivery(deliveryId);

        if (result.ResultStatus != ResultStatus.Success)
            return StatusCode((int)HttpStatusCode.InternalServerError, result);

        return Ok(result);
    }

    [HttpGet("{accountId}/orders")]
    public async Task<ActionResult<Result<PaginationResponse<OrderResponse>>>> GetOrdersByAccountId(
        [FromRoute] Guid accountId, [FromQuery] OrderRequest request)
    {
        var result = await _orderService.GetOrdersByAccountId(accountId, request);

        if (result.ResultStatus != ResultStatus.Success)
            return StatusCode((int)HttpStatusCode.InternalServerError, result);

        return Ok(result);
    }

    [HttpPost("{accountId}/orders")]
    public async Task<ActionResult<Result<OrderResponse>>> CreateOrder([FromRoute] Guid accountId,
        [FromBody] CartRequest cart)
    {
        var result = await _orderService.CreateOrder(accountId, cart);

        if (result.ResultStatus != ResultStatus.Success)
            return StatusCode((int)HttpStatusCode.InternalServerError, result);

        return Ok(result);
    }

    [HttpGet("{accountId}/consignsales")]
    public async Task<ActionResult<Result<PaginationResponse<ConsignSaleResponse>>>> GetAllConsignSale(
        [FromRoute] Guid accountId, [FromQuery] ConsignSaleRequest request)
    {
        var result = await _consignSaleService.GetAllConsignSales(accountId, request);

        if (result.ResultStatus != ResultStatus.Success)
            return StatusCode((int)HttpStatusCode.InternalServerError, result);

        return Ok(result);
    }

    [HttpPost("{accountId}/consignsales")]
    public async Task<ActionResult<Result<ConsignSaleResponse>>> CreateConsignSale([FromRoute] Guid accountId,
        [FromBody] CreateConsignSaleRequest request)
    {
        var result = await _consignSaleService.CreateConsignSale(accountId, request);

        if (result.ResultStatus != ResultStatus.Success)
            return StatusCode((int)HttpStatusCode.InternalServerError, result);

        return Ok(result);
    }

    [HttpPost("{accountId}/inquiries")]
    public async Task<ActionResult<CreateInquiryResponse>> CreateInquiry([FromRoute] Guid accountId,
        [FromBody] CreateInquiryRequest request)
    {
        var result = await _accountService.CreateInquiry(accountId, request);
        return Ok(result);
    }

    [HttpPost("{accountId}/withdraws")]
    public async Task<ActionResult<CreateWithdrawResponse>> CreateWithdraw([FromRoute] Guid accountId,
        [FromBody] CreateWithdrawRequest request)
    {
        var result = await _accountService.RequestWithdraw(accountId, request);
        return Ok(result);
    }

    [HttpGet("{accountId}/withdraws")]
    public async Task<ActionResult<PaginationResponse<GetWithdrawsResponse>>> GetWithdraws(
        [FromRoute] Guid accountId,
        [FromQuery] GetWithdrawsRequest request)
    {
        var result = await _accountService.GetWithdraws(accountId, request);
        return Ok(result);
    }

    [HttpGet("{accountId}/transactions")]
    public async Task<ActionResult<PaginationResponse<GetTransactionsResponse>>> GetTransactions([FromRoute] Guid accountId,
        [FromQuery] GetTransactionsRequest request)
    {
        var result = await _accountService.GetTransactions(accountId, request);
        return Ok(result);
    }
}