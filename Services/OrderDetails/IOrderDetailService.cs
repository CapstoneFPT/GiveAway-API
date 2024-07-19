using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.OrderDetails;
using BusinessObjects.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Dtos.FashionItems;
using BusinessObjects.Dtos.Refunds;

namespace Services.OrderDetails
{
    public interface IOrderDetailService
    {
        Task<Result<PaginationResponse<OrderDetailResponse<FashionItem>>>> GetOrderDetailsByOrderId(Guid orderId, OrderDetailRequest request);
        Task<Result<OrderDetailResponse<FashionItem>>> GetOrderDetailById(Guid orderId);

        Task<Result<List<RefundResponse>>> RequestRefundToShop(List<CreateRefundRequest> refundRequest);

        Task ChangeFashionItemsStatus(List<OrderDetail> orderDetails, FashionItemStatus fashionItemStatus);
    }
}
