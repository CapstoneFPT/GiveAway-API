using System.Net;
using BusinessObjects.Dtos.Account;
using BusinessObjects.Dtos.Account.Request;
using BusinessObjects.Dtos.Account.Response;
using BusinessObjects.Dtos.AuctionDeposits;
using BusinessObjects.Dtos.Auctions;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.ConsignSales;
using BusinessObjects.Dtos.Deliveries;
using BusinessObjects.Dtos.Inquiries;
using BusinessObjects.Dtos.Orders;
using BusinessObjects.Dtos.Recharges;
using BusinessObjects.Dtos.Transactions;
using BusinessObjects.Dtos.Withdraws;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Services.Accounts;
using Services.ConsignSales;
using Services.Deliveries;
using Services.OrderLineItems;
using Services.Orders;
using Services.Recharges;

namespace WebApi.Controllers;

[Route("api/accounts")]
[ApiController]
public class AccountController : ControllerBase
{
    private readonly IAccountService _accountService;
    private readonly IConsignSaleService _consignSaleService;
    private readonly IDeliveryService _deliveryService;
    private readonly IOrderLineItemService _orderLineItemService;
    private readonly IOrderService _orderService;
    private readonly IRechargeService _rechargeService;
    private readonly IHttpContextAccessor _contextAccessor;
    public AccountController(IAccountService accountService, IDeliveryService deliveryService,
        IOrderService orderService, IConsignSaleService consignSaleService, IOrderLineItemService orderLineItemService,
        IHttpContextAccessor contextAccessor,IRechargeService rechargeService)
    {
        _accountService = accountService;
        _deliveryService = deliveryService;
        _orderService = orderService;
        _consignSaleService = consignSaleService;
        _orderLineItemService = orderLineItemService;
        _contextAccessor = contextAccessor;
        _rechargeService = rechargeService;
    }
    [Authorize]
    [HttpGet]
    [ProducesResponseType<PaginationResponse<AccountResponse>>((int)HttpStatusCode.OK)]
    public async Task<IActionResult> GetAccounts(
        [FromQuery] GetAccountsRequest request)
    {
        var result = await _accountService.GetAccounts(request);
        return Ok(result);
    }
    [Authorize]
    [HttpGet("{id}")]
    [ProducesResponseType<Result<AccountResponse>>((int)HttpStatusCode.OK)]
    public async Task<IActionResult> GetAccountById(Guid id)
    {
        var result = await _accountService.GetAccountById(id);

        if (result.ResultStatus != ResultStatus.Success)
            return StatusCode((int)HttpStatusCode.InternalServerError, result);

        return Ok(result);
    }
    [Authorize]
    [HttpPost("get-current-account")]
    [ProducesResponseType<Result<AccountResponse>>((int)HttpStatusCode.OK)]
    [ProducesResponseType<ErrorResponse>((int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> GetAccountByToken()
    {
        var user = _contextAccessor.HttpContext!.User?.Claims?.FirstOrDefault(c => c.Type.Contains("AccountId"))?.Value;
        if (user is null)
        {
            return StatusCode((int)HttpStatusCode.NotFound);
        }
        var shop = _contextAccessor.HttpContext!.User?.Claims?.FirstOrDefault(c => c.Type.Contains("ShopId"))?.Value;
        var result = await _accountService.GetAccountById(Guid.Parse(user!));
        if (shop != null)
        {
            result.Data.ShopId = Guid.Parse(shop);
        }
        return result.ResultStatus != ResultStatus.Success ? StatusCode((int)HttpStatusCode.InternalServerError, result) : Ok(result);
    }
    [Authorize(Roles = "Admin")]
    [HttpPut("{id}/ban")]
    [ProducesResponseType<Result<AccountResponse>>((int)HttpStatusCode.OK)]
    public async Task<IActionResult> BanAccount([FromRoute] Guid id)
    {
        var result = await _accountService.BanAccountById(id);

        if (result.ResultStatus != ResultStatus.Success)
            return StatusCode((int)HttpStatusCode.InternalServerError, result);

        return Ok(result);
    }
    [Authorize]
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
    [Authorize(Roles = "Member")]
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
    [Authorize(Roles = "Member")]
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
    [Authorize(Roles = "Member")]
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
    [Authorize(Roles = "Member")]
    [HttpDelete("{accountId}/deliveries/{deliveryId}")]
    [ProducesResponseType<Result<string>>((int)HttpStatusCode.OK)]
    public async Task<IActionResult> DeleteDelivery([FromRoute] Guid deliveryId)
    {
        var result = await _deliveryService.DeleteDelivery(deliveryId);

        if (result.ResultStatus != ResultStatus.Success)
            return StatusCode((int)HttpStatusCode.InternalServerError, result);

        return Ok(result);
    }
    [Authorize(Roles = "Member")]
    [HttpGet("{accountId}/orders")]
    [ProducesResponseType<PaginationResponse<OrderListResponse>>((int)HttpStatusCode.OK)]
    [ProducesResponseType<ErrorResponse>((int)HttpStatusCode.InternalServerError)]
    public async Task<IActionResult> GetOrdersByAccountId(
        [FromRoute] Guid accountId, [FromQuery] OrderRequest request)
    {
        var result = await _orderService.GetOrdersByAccountId(accountId, request);
        if (!result.IsSuccessful)
        {
            return result.Error switch
            {
                _ => StatusCode(500, new ErrorResponse("Error getting orders", ErrorType.ApiError,
                    HttpStatusCode.InternalServerError, result.Error)),
            };
        }
        return Ok(result.Value);
    }
    [Authorize(Roles = "Member")]
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
    [Authorize(Roles = "Member")]
    [HttpGet("{accountId}/consignsales")]
    [ProducesResponseType<PaginationResponse<ConsignSaleDetailedResponse>>((int)HttpStatusCode.OK)]
    [ProducesResponseType<ErrorResponse>((int)HttpStatusCode.InternalServerError)]
    public async Task<IActionResult> GetAllConsignSale(
        [FromRoute] Guid accountId, [FromQuery] ConsignSaleRequest request)
    {
        var result = await _consignSaleService.GetAllConsignSales(accountId, request);

        if (!result.IsSuccessful)
        {
            return result.Error switch
            {
                _ => StatusCode(500, new ErrorResponse("Error getting consign sales", ErrorType.ApiError,
                    HttpStatusCode.InternalServerError, result.Error)),
            };
        }
        
        return Ok(result.Value);
     
    }
    [Authorize(Roles = "Member")]
    [HttpPost("{accountId}/consignsales")]
    [ProducesResponseType<Result<ConsignSaleDetailedResponse>>((int)HttpStatusCode.OK)]
    public async Task<IActionResult> CreateConsignSale([FromRoute] Guid accountId,
        [FromBody] CreateConsignSaleRequest request)
    {
        var result = await _consignSaleService.CreateConsignSale(accountId, request);

        if (result.ResultStatus != ResultStatus.Success)
            return StatusCode((int)HttpStatusCode.InternalServerError, result);

        return Ok(result);
    }
    [Authorize(Roles = "Member")]
    [HttpPost("{accountId}/inquiries")]
    [ProducesResponseType<CreateInquiryResponse>((int)HttpStatusCode.OK)]
    public async Task<IActionResult> CreateInquiry([FromRoute] Guid accountId,
        [FromBody] CreateInquiryRequest request)
    {
        var result = await _accountService.CreateInquiry(accountId, request);
        return Ok(result);
    }
    [Authorize(Roles = "Member")]
    [HttpPost("{accountId}/withdraws")]
    [ProducesResponseType<CreateWithdrawResponse>((int)HttpStatusCode.OK)]
    public async Task<IActionResult> CreateWithdraw([FromRoute] Guid accountId,
        [FromBody] CreateWithdrawRequest request)
    {
        var result = await _accountService.RequestWithdraw(accountId, request);
        return Ok(result);
    }
    [Authorize(Roles = "Member")]
    [HttpGet("{accountId}/recharges")]
    [ProducesResponseType<PaginationResponse<RechargeListResponse>>((int)HttpStatusCode.OK)]
    [ProducesResponseType<ErrorResponse>((int)HttpStatusCode.InternalServerError)]
    public async Task<IActionResult> GetAccountRecharges(
        [FromRoute] Guid accountId,
        [FromQuery] GetAccountRechargesRequest request)
    {
        var result = await _rechargeService.GetRecharges(new GetRechargesRequest()
        {
            PageSize = request.PageSize,
            MemberId = accountId,
            Page = request.Page,
            RechargeStatus = request.RechargeStatus,
            RechargeCode = request.RechargeCode
        });

        if (!result.IsSuccessful)
        {
            return result.Error switch
            {
                _ => StatusCode(500,
                    new ErrorResponse("Network error", ErrorType.ApiError, HttpStatusCode.InternalServerError,
                        ErrorCode.ServerError))
            };
        }
        
        return Ok(result.Value);
    }
    [Authorize(Roles = "Member")]
    [HttpGet("{accountId}/auctions")]
    [ProducesResponseType<PaginationResponse<AuctionListResponse>>((int)HttpStatusCode.OK)]
    [ProducesResponseType<ErrorResponse>((int)HttpStatusCode.InternalServerError)]
    public async Task<IActionResult> GetAuctions([FromRoute] Guid accountId,
        [FromQuery] GetAccountAuctionsRequest request)
    {
        DotNext.Result<PaginationResponse<AuctionListResponse>> result =
            await _accountService.GetAuctions(accountId, request);

        if (!result.IsSuccessful)
        {
            return result.Error switch
            {
                _ => StatusCode(500,
                    new ErrorResponse("Network error", ErrorType.ApiError, HttpStatusCode.InternalServerError,
                        ErrorCode.ServerError))
            };
        }

        return Ok(result.Value);
    }
        
    [Authorize(Roles = "Member")]
    [HttpGet("{accountId}/withdraws")]
    [ProducesResponseType<PaginationResponse<GetWithdrawsResponse>>((int)HttpStatusCode.OK)]
    public async Task<IActionResult> GetWithdraws(
        [FromRoute] Guid accountId,
        [FromQuery] GetWithdrawsRequest request)
    {
        var result = await _accountService.GetWithdraws(accountId, request);
        return Ok(result);
    }
    [Authorize(Roles = "Member")]
    [HttpGet("{accountId}/bankaccounts")]
    [ProducesResponseType<List<BankAccountsListResponse>>((int)HttpStatusCode.OK)]
    [ProducesResponseType<ErrorResponse>((int)HttpStatusCode.InternalServerError)]
    public async Task<IActionResult> GetBankAccounts([FromRoute] Guid accountId)
    {
        DotNext.Result<List<BankAccountsListResponse>, ErrorCode> result =
            await _accountService.GetBankAccounts(accountId);

        if (!result.IsSuccessful)
        {
            return result.Error switch
            {
                ErrorCode.NetworkError => StatusCode(500,
                    new ErrorResponse("Network error", ErrorType.ApiError, HttpStatusCode.InternalServerError,
                        ErrorCode.NetworkError)),
                _ => StatusCode(500,
                    new ErrorResponse("Internal server error", ErrorType.ApiError, HttpStatusCode.InternalServerError,
                        ErrorCode.UnknownError))
            };
        }

        return Ok(result.Value);
    }
    [Authorize(Roles = "Member")]
    [HttpPost("{accountId}/bankaccounts")]
    [ProducesResponseType<CreateBankAccountResponse>((int)HttpStatusCode.OK)]
    [ProducesResponseType<ErrorResponse>((int)HttpStatusCode.InternalServerError)]
    public async Task<IActionResult> CreateBankAccount([FromRoute] Guid accountId,
        [FromBody] CreateBankAccountRequest request)
    {
        DotNext.Result<CreateBankAccountResponse, ErrorCode> result =
            await _accountService.CreateBankAccount(accountId, request);

        if (!result.IsSuccessful)
        {
            return result.Error switch
            {
                ErrorCode.DuplicateBankAccount => Conflict(new ErrorResponse("Bank account already exists", ErrorType.AccountError,HttpStatusCode.Conflict, ErrorCode.DuplicateBankAccount)),
                ErrorCode.ServerError => StatusCode(500,
                    new ErrorResponse("Error saving bank account", ErrorType.ApiError,
                        HttpStatusCode.InternalServerError, ErrorCode.ServerError)),
                ErrorCode.NetworkError => StatusCode(500,
                    new ErrorResponse("Network error", ErrorType.ApiError, HttpStatusCode.InternalServerError,
                        ErrorCode.NetworkError)),
                _ => StatusCode(500,
                    new ErrorResponse("Error saving bank account", ErrorType.ApiError,
                        HttpStatusCode.InternalServerError,
                        ErrorCode.UnknownError))
            };
        }

        return Ok(result.Value);
    }
    [Authorize(Roles = "Member")]
    [HttpPut("{accountId}/bankaccounts/{bankAccountId}")]
    [ProducesResponseType<UpdateBankAccountResponse>((int)HttpStatusCode.OK)]
    [ProducesResponseType<ErrorResponse>((int)HttpStatusCode.InternalServerError)]
    public async Task<IActionResult> UpdateBankAccount([FromRoute] Guid accountId, [FromRoute] Guid bankAccountId,
        [FromBody] UpdateBankAccountRequest request)
    {
        DotNext.Result<UpdateBankAccountResponse, ErrorCode> result =
            await _accountService.UpdateBankAccount(accountId, bankAccountId, request);

        if (!result.IsSuccessful)
        {
            return result.Error switch
            {
                ErrorCode.ServerError => StatusCode(500,
                    new ErrorResponse("Error saving bank account", ErrorType.ApiError,
                        HttpStatusCode.InternalServerError, ErrorCode.ServerError)),
                ErrorCode.NoBankAccountLeft => BadRequest(new ErrorResponse(
                    "There must be at least 1 default bank account", ErrorType.AccountError, HttpStatusCode.BadRequest,
                    ErrorCode.NoBankAccountLeft)),
                ErrorCode.DuplicateBankAccount => Conflict(new ErrorResponse(
                    "The bank accounts must be unique", ErrorType.AccountError, HttpStatusCode.Conflict,
                    ErrorCode.DuplicateBankAccount)),
                ErrorCode.Unauthorized => Unauthorized(new ErrorResponse("Unauthorized", ErrorType.AccountError,
                    HttpStatusCode.Unauthorized, ErrorCode.Unauthorized)),
                ErrorCode.NetworkError => StatusCode(500,
                    new ErrorResponse("Network error", ErrorType.ApiError, HttpStatusCode.InternalServerError,
                        ErrorCode.NetworkError)),
                _ => StatusCode(500,
                    new ErrorResponse("Error saving bank account", ErrorType.ApiError,
                        HttpStatusCode.InternalServerError,
                        ErrorCode.UnknownError))
            };
        }

        return Ok(result.Value);
    }
    [Authorize(Roles = "Member")]
    [HttpDelete("{accountId}/bankaccounts/{bankAccountId}")]
    [ProducesResponseType<DeleteBankAccountResponse>((int)HttpStatusCode.OK)]
    [ProducesResponseType<ErrorResponse>((int)HttpStatusCode.InternalServerError)]
    public async Task<IActionResult> DeleteBankAccount([FromRoute] Guid accountId, [FromRoute] Guid bankAccountId)
    {
        DotNext.Result<DeleteBankAccountResponse, ErrorCode> result =
            await _accountService.DeleteBankAccount(accountId, bankAccountId);

        if (!result.IsSuccessful)
        {
            return result.Error switch
            {
                ErrorCode.NotFound => NotFound(new ErrorResponse("Bank account not found", ErrorType.ApiError,
                    HttpStatusCode.NotFound, ErrorCode.NotFound)),
                ErrorCode.Unauthorized => Unauthorized(new ErrorResponse("Unauthorized", ErrorType.ApiError,
                    HttpStatusCode.Unauthorized, ErrorCode.Unauthorized)),
                ErrorCode.NoBankAccountLeft => BadRequest(new ErrorResponse(
                    "You can't delete the only bank account left", ErrorType.ApiError, HttpStatusCode.BadRequest,
                    ErrorCode.NoBankAccountLeft)),
                ErrorCode.ServerError => StatusCode(500,
                    new ErrorResponse("Error deleting bank account", ErrorType.ApiError,
                        HttpStatusCode.InternalServerError, ErrorCode.ServerError)),
                ErrorCode.NetworkError => StatusCode(500,
                    new ErrorResponse("Network error", ErrorType.ApiError, HttpStatusCode.InternalServerError,
                        ErrorCode.NetworkError)),
                _ => StatusCode(500,
                    new ErrorResponse("Error deleting bank account", ErrorType.ApiError,
                        HttpStatusCode.InternalServerError,
                        ErrorCode.UnknownError))
            };
        }

        return Ok(result.Value);
    }
    [Authorize(Roles = "Member")]
    [HttpPatch("{accountId}/bankaccounts/{bankAccountId}/set-default")]
    [ProducesResponseType<ErrorResponse>((int)HttpStatusCode.InternalServerError)]
    public async Task<IActionResult> SetDefaultBankAccount([FromRoute] Guid accountId, [FromRoute] Guid bankAccountId)
    {
        DotNext.Result<UpdateBankAccountResponse, ErrorCode> result =
            await _accountService.SetDefaultBankAccount(accountId, bankAccountId);

        if (!result.IsSuccessful)
        {
            return result.Error switch
            {
                ErrorCode.NotFound => NotFound(new ErrorResponse("Bank account not found", ErrorType.ApiError,
                    HttpStatusCode.NotFound, ErrorCode.NotFound)),
                ErrorCode.Unauthorized => Unauthorized(new ErrorResponse("Unauthorized", ErrorType.ApiError,
                    HttpStatusCode.Unauthorized, ErrorCode.Unauthorized)),
                ErrorCode.ServerError => StatusCode(500,
                    new ErrorResponse("Error deleting bank account", ErrorType.ApiError,
                        HttpStatusCode.InternalServerError, ErrorCode.ServerError)),
                ErrorCode.NetworkError => StatusCode(500,
                    new ErrorResponse("Network error", ErrorType.ApiError, HttpStatusCode.InternalServerError,
                        ErrorCode.NetworkError)),
                _ => StatusCode(500,
                    new ErrorResponse("Error deleting bank account", ErrorType.ApiError,
                        HttpStatusCode.InternalServerError,
                        ErrorCode.UnknownError))
            };
        }

        return Ok(result.Value);
    }
    [Authorize(Roles = "Member")]
    [HttpGet("{accountId}/deposits")]
    [ProducesResponseType<PaginationResponse<AccountDepositsListResponse>>((int)HttpStatusCode.OK)]
    public async Task<IActionResult> GetDeposits([FromRoute] Guid accountId,
        [FromQuery] GetDepositsRequest request)
    {
        DotNext.Result<PaginationResponse<AccountDepositsListResponse>,ErrorCode> result = await _accountService.GetDeposits(accountId, request);
        if (!result.IsSuccessful)
        {
            return result.Error switch
            {
                _ => StatusCode(500,
                    new ErrorResponse("Network error", ErrorType.ApiError, HttpStatusCode.InternalServerError,
                        ErrorCode.ServerError))
            };
        }
        
        return Ok(result.Value);
    }


    [HttpGet("{accountId}/transactions")]
    [Authorize]
    [ProducesResponseType<PaginationResponse<AccountTransactionsListResponse>>((int)HttpStatusCode.OK)]
    public async Task<IActionResult> GetTransactions([FromRoute] Guid accountId,
        [FromQuery] GetTransactionsRequest request)
    {
        var result = await _accountService.GetTransactions(accountId, request);
        return Ok(result);
    }
}