using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.Orders;
using Repositories.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Orders
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;

        public OrderService(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<Result<PaginationResponse<OrderResponse>>> GetOrdersByAccountId(Guid accountId, OrderRequest request)
        {
            try
            {
                var response = new Result<PaginationResponse<OrderResponse>>();
                var listOrder = await _orderRepository.GetOrdersByAccountId(accountId, request);
                if(listOrder.TotalCount == 0)
                {
                    response.Messages = ["You don't have any order"];
                    response.ResultStatus = ResultStatus.Empty;
                    return response;
                }
                response.Data = listOrder;
                response.Messages = ["There are " + listOrder.TotalCount + " in total"];
                response.ResultStatus = ResultStatus.Success;
                return response;
            }catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
