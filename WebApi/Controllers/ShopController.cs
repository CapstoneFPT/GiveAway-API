using System.Net;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.ConsignSales;
using BusinessObjects.Dtos.FashionItems;
using BusinessObjects.Dtos.Feedbacks;
using BusinessObjects.Dtos.Inquiries;
using BusinessObjects.Dtos.Orders;
using BusinessObjects.Dtos.Refunds;
using BusinessObjects.Dtos.Shops;
using BusinessObjects.Dtos.Transactions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.ConsignSales;
using Services.FashionItems;
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

        public ShopController(IFashionItemService fashionItemService, IShopService shopService,
            IOrderService orderService, IConsignSaleService consignSaleService,
            IRefundService refundService)
        {
            _fashionItemService = fashionItemService;
            _shopService = shopService;
            _orderService = orderService;
            _consignSaleService = consignSaleService;
            _refundService = refundService;
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


        [HttpGet]
        public async Task<ActionResult<Result<List<ShopDetailResponse>>>> GetAllShop()
        {
            var result = await _shopService.GetAllShop();
            
            if (result.ResultStatus != ResultStatus.Success)
                return StatusCode((int)HttpStatusCode.InternalServerError, result);
            
            return Ok(result);
        }

        [HttpGet("{shopId}")]
        public async Task<ActionResult<Result<ShopDetailResponse>>> GetShopById([FromRoute] Guid shopId)
        {
            var result = await _shopService.GetShopById(shopId);
            
            if (result.ResultStatus != ResultStatus.Success)
                return StatusCode((int)HttpStatusCode.InternalServerError, result);
            
            return Ok(result);
        }

        

        [HttpPost("{shopId}/orders")]
        public async Task<ActionResult<Result<OrderResponse>>> CreateOrderByShop([FromRoute] Guid shopId,
            [FromBody] CreateOrderRequest orderRequest)
        {
            var result = await _orderService.CreateOrderByShop(shopId, orderRequest);
            
            if (result.ResultStatus != ResultStatus.Success)
                return StatusCode((int)HttpStatusCode.InternalServerError, result);
            
            return Ok(result);
        }

        [HttpPost("{shopId}/orders/{orderId}/pay-with-cash")]
        public async Task<
            ActionResult<PayOrderWithCashResponse>> PayWithCash([FromRoute] Guid shopId, [FromRoute] Guid orderId,
            [FromBody] PayOrderWithCashRequest request)
        {
            PayOrderWithCashResponse result = await _orderService.PayWithCash(shopId, orderId, request);
            return Ok(result);
        }

        

        [HttpGet("{shopId}/consignsales")]
        public async Task<ActionResult<Result<PaginationResponse<ConsignSaleResponse>>>> GetAllConsignSaleByShopId(
            [FromRoute] Guid shopId, [FromQuery] ConsignSaleRequestForShop request)
        {
            var result = await _consignSaleService.GetAllConsignSalesByShopId(shopId, request);
            
            if (result.ResultStatus != ResultStatus.Success)
                return StatusCode((int)HttpStatusCode.InternalServerError, result);
            
            return Ok(result);
        }
    
        [HttpPost("{shopId}/consignsales")]
        public async Task<ActionResult<Result<ConsignSaleResponse>>> CreateConsignSaleByShop([FromRoute] Guid shopId,
            [FromBody] CreateConsignSaleByShopRequest consignRequest)
        {
            var result = await _consignSaleService.CreateConsignSaleByShop(shopId, consignRequest);
            
            if (result.ResultStatus != ResultStatus.Success)
                return StatusCode((int)HttpStatusCode.InternalServerError, result);
            
            return Ok(result);
        }

        

        

        [HttpGet("{shopId}/offline-transactions")]
        public async Task<ActionResult<PaginationResponse<TransactionResponse>>> GetTransactionsByShopId(
            [FromRoute] Guid shopId, TransactionRequest transactionRequest)
        {
            var result = await _shopService.GetOfflineTransactionsByShopId(shopId, transactionRequest);
            return Ok(result);
        }

        [HttpPost("{shopId}/feedbacks")]
        public async Task<ActionResult<FeedbackResponse>> CreateFeedbackByShop([FromRoute] Guid shopId,
            [FromBody] CreateFeedbackRequest feedbackRequest)
        {
            return await _shopService.CreateFeedbackForShop(shopId, feedbackRequest);
        }
    }
}