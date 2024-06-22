using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.Order;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Orders
{
    public interface IOrderService
    {
        Task<Result<PaginationResponse<OrderResponse>>> GetOrdersByAccountId(Guid accountId, OrderRequest request);
    }
}
