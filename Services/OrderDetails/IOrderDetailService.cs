using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.OrderDetails;
using BusinessObjects.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.OrderDetails
{
    public interface IOrderDetailService
    {
        Task<Result<PaginationResponse<OrderDetailResponse<FashionItem>>>> GetOrderDetailsByOrderId(Guid orderId, OrderDetailRequest request);
        Task<Result<OrderDetailResponse<FashionItem>>> GetOrderDetailById(Guid orderId);
    }
}
