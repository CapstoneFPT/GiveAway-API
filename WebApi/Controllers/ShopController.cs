using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Runtime.CompilerServices;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.ConsignSales;
using BusinessObjects.Dtos.FashionItems;
using BusinessObjects.Dtos.Feedbacks;
using BusinessObjects.Dtos.Inquiries;
using BusinessObjects.Dtos.Orders;
using BusinessObjects.Dtos.Refunds;
using BusinessObjects.Dtos.Shops;
using BusinessObjects.Dtos.Transactions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.ConsignSales;
using Services.FashionItems;
using Services.GiaoHangNhanh;
using Services.Orders;
using Services.Refunds;
using Services.Shops;

namespace WebApi.Controllers
{
    [Route("api/shops")]
    [ApiController]
    public class ShopController : ControllerBase
    {
        private readonly IFashionItemService _fashionItemService;
        private readonly IShopService _shopService;
        private readonly IOrderService _orderService;
        private readonly IConsignSaleService _consignSaleService;
        private readonly IRefundService _refundService;
        private readonly IGiaoHangNhanhService _giaoHangNhanhService;

        public ShopController(IFashionItemService fashionItemService, IShopService shopService,
            IOrderService orderService, IConsignSaleService consignSaleService,
            IRefundService refundService, IGiaoHangNhanhService giaoHangNhanhService)
        {
            _fashionItemService = fashionItemService;
            _shopService = shopService;
            _orderService = orderService;
            _consignSaleService = consignSaleService;
            _refundService = refundService;
            _giaoHangNhanhService = giaoHangNhanhService;
        }

        [HttpPost("{shopId}/fashionitems")]
        public async Task<ActionResult<Result<FashionItemDetailResponse>>> AddFashionItem([FromRoute] Guid shopId,
            [FromBody] FashionItemDetailRequest request)
        {
            var result = await _fashionItemService.AddFashionItem(shopId, request);

            if (result.ResultStatus != ResultStatus.Success)
                return StatusCode((int)HttpStatusCode.InternalServerError, result);

            return Ok(result);
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<Result<List<ShopDetailResponse>>>> GetAllShop()
        {
            var result = await _shopService.GetAllShop();

            if (result.ResultStatus != ResultStatus.Success)
                return StatusCode((int)HttpStatusCode.InternalServerError, result);

            return Ok(result);
        }
        [AllowAnonymous]
        [HttpGet("{shopId}")]
        public async Task<ActionResult<Result<ShopDetailResponse>>> GetShopById([FromRoute] Guid shopId)
        {
            var result = await _shopService.GetShopById(shopId);

            if (result.ResultStatus != ResultStatus.Success)
                return StatusCode((int)HttpStatusCode.InternalServerError, result);

            return Ok(result);
        }

        [Authorize(Roles = "Staff")]
        [HttpPost("{shopId}/orders")]
        public async Task<ActionResult<Result<OrderResponse>>> CreateOrderByShop([FromRoute] Guid shopId,
            [FromBody] CreateOrderRequest orderRequest)
        {
            var result = await _orderService.CreateOrderByShop(shopId, orderRequest);

            if (result.ResultStatus != ResultStatus.Success)
                return StatusCode((int)HttpStatusCode.InternalServerError, result);

            return Ok(result);
        }

        [HttpPost("{shopId}/orders/{orderId}/pay-offline")]
        [ProducesResponseType<PayOrderOfflineResponse>((int)HttpStatusCode.OK)]
        public async Task<IActionResult> PayWithCash([FromRoute] Guid shopId, [FromRoute] Guid orderId)
        {
            var result = await _orderService.OfflinePay(shopId, orderId);

            if (!result.IsSuccessful)
            {
                return result.Error switch
                {
                    _ => StatusCode(500,
                        new ErrorResponse("Error paying order", ErrorType.ApiError, HttpStatusCode.InternalServerError,
                            result.Error))
                };
            }

            return Ok(result.Value);
        }
        [Authorize(Roles = "Staff")]
        [HttpPut("{shopId}/orders/{OrderId}/confirm-deliveried")]
        public async Task<ActionResult<Result<OrderResponse>>> ConfirmOrderDelivered(
            [FromRoute] Guid shopId, [FromRoute] Guid OrderId)
        {
            var result = await _orderService.ConfirmOrderDeliveried(shopId, OrderId);

            if (result.ResultStatus != ResultStatus.Success)
                return StatusCode((int)HttpStatusCode.InternalServerError, result);

            return Ok(result);
        }
        [Authorize(Roles = "Staff")]
        [HttpPost("{shopId}/consignsales")]
        public async Task<ActionResult<Result<ConsignSaleDetailedResponse>>> CreateConsignSaleByShop(
            [FromRoute] Guid shopId,
            [FromBody] CreateConsignSaleByShopRequest consignRequest)
        {
            var result = await _consignSaleService.CreateConsignSaleByShop(shopId, consignRequest);

            if (result.ResultStatus != ResultStatus.Success)
                return StatusCode((int)HttpStatusCode.InternalServerError, result);

            return Ok(result);
        }

        [HttpPost("{shopId}/refunds")]
        [ProducesResponseType<RefundResponse>((int) HttpStatusCode.OK)]
        [ProducesResponseType<ErrorResponse>((int) HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CreateRefundByShop([FromRoute] Guid shopId,
            CreateRefundByShopRequest request)
        {
            var result = await _refundService.CreateRefundByShop(shopId, request);

            if (!result.IsSuccessful)
            {
                return result.Error switch
                {
                    _ => StatusCode(500,
                        new ErrorResponse("Error creating refund", ErrorType.ApiError, HttpStatusCode.InternalServerError,
                            result.Error))
                };
            }

            return Ok(result.Value);
        }


        

        [HttpPost("{shopId}/feedbacks")]
        public async Task<ActionResult<FeedbackResponse>> CreateFeedbackByShop([FromRoute] Guid shopId,
            [FromBody] CreateFeedbackRequest feedbackRequest)
        {
            return await _shopService.CreateFeedbackForShop(shopId, feedbackRequest);
        }

        [HttpGet("ghnshops")]
        [ProducesResponseType<GHNApiResponse<List<GHNShop>>>((int)HttpStatusCode.OK)]
        [ProducesResponseType<ErrorResponse>((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetGhnShops()
        {
            var result = await _giaoHangNhanhService.GetShops();

            if (result.IsSuccessful)
            {
                return Ok(result.Value);
            }

            return result.Error switch
            {
                ErrorCode.Unauthorized => Unauthorized(new ErrorResponse("Unauthorized access to GiaoHangNhanh API",
                    ErrorType.ApiError, HttpStatusCode.Unauthorized, ErrorCode.Unauthorized)),

                _ => StatusCode(500,
                    new ErrorResponse("Unexpected error from GiaoHangNhanh API", ErrorType.ApiError,
                        HttpStatusCode.InternalServerError, ErrorCode.ExternalServiceError))
            };
        }

        [HttpPost("ghnshops")]
        [Obsolete("Use CreateShop instead")]
        [ProducesResponseType<GHNApiResponse<GHNShopCreateResponse>>((int)HttpStatusCode.OK)]
        public async Task<IActionResult> CreateGhnShop([FromBody] GHNShopCreateRequest request)
        {
            var result = await _giaoHangNhanhService.CreateShop(request);

            if (result.IsSuccessful)
            {
                return Ok(result.Value);
            }

            return result.Error switch
            {
                ErrorCode.Unauthorized => Unauthorized(new ErrorResponse("Unauthorized access to GiaoHangNhanh API",
                    ErrorType.ApiError, HttpStatusCode.Unauthorized, ErrorCode.Unauthorized)),

                _ => StatusCode(500,
                    new ErrorResponse("Unexpected error from GiaoHangNhanh API", ErrorType.ApiError,
                        HttpStatusCode.InternalServerError, ErrorCode.ExternalServiceError))
            };
        }

        [HttpPost]
        [ProducesResponseType<CreateShopResponse>((int)HttpStatusCode.OK)]
        [ProducesResponseType<ErrorResponse>((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CreateShop([FromBody] CreateShopRequest request)
        {
            var result = await _shopService.CreateShop(request);

            if (result.IsSuccessful)
            {
                return Ok(result.Value);
            }

            return result.Error switch
            {
                ErrorCode.Unauthorized => Unauthorized(new ErrorResponse(
                    "Unauthorized access to External Service",
                    ErrorType.ApiError,
                    HttpStatusCode.Unauthorized,
                    ErrorCode.Unauthorized)),

                ErrorCode.ServerError => StatusCode(500, new ErrorResponse(
                    "Unexpected error from server",
                    ErrorType.ApiError,
                    HttpStatusCode.InternalServerError,
                    ErrorCode.ServerError)),

                _ => StatusCode(500, new ErrorResponse(
                    "Unexpected error from server",
                    ErrorType.ApiError,
                    HttpStatusCode.InternalServerError,
                    ErrorCode.ExternalServiceError))
            };
        }

        [HttpPost("{shopId}/create-master-for-offline-consign")]
        [ProducesResponseType<MasterItemResponse>((int)HttpStatusCode.OK)]
        [ProducesResponseType<ErrorResponse>((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CreateMasterOfflineConsign([FromRoute] Guid shopId,
            CreateMasterOfflineConsignRequest request)
        {
            var result = await _fashionItemService.CreateMasterItemForOfflineConsign(shopId, request);

            if (!result.IsSuccessful)
            {
                return result.Error switch
                {
                    _ => StatusCode(500,
                        new ErrorResponse("Error creating refund", ErrorType.ApiError, HttpStatusCode.InternalServerError,
                            result.Error))
                };
            }

            return Ok(result.Value);
        }

    }
}