using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.OrderDetails;
using BusinessObjects.Dtos.Orders;
using BusinessObjects.Entities;
using Org.BouncyCastle.Asn1.Ocsp;
using Repositories.OrderDetails;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.OrderDetails
{
    public class OrderDetailService : IOrderDetailService
    {
        private readonly IOrderDetailRepository _orderDetailRepository;

        public OrderDetailService(IOrderDetailRepository orderDetailRepository)
        {
            _orderDetailRepository = orderDetailRepository;
        }

        public async Task<Result<OrderDetailResponse<FashionItem>>> GetOrderDetailById(Guid orderId)
        {
            try
            {
                var response = new Result<OrderDetailResponse<FashionItem>>();
                var listOrder = await _orderDetailRepository.GetOrderDetailById(orderId);
                if (listOrder is null)
                {
                    response.Messages = ["Can not found the order detail"];
                    response.ResultStatus = ResultStatus.NotFound;
                    return response;
                }
                response.Data = listOrder;
                response.Messages = ["Successfully"];
                response.ResultStatus = ResultStatus.Success;
                return response;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<Result<PaginationResponse<OrderDetailResponse<FashionItem>>>> GetOrderDetailsByOrderId(Guid orderId, OrderDetailRequest request)
        {
            try
            {
                var response = new Result<PaginationResponse<OrderDetailResponse<FashionItem>>>();
                var listOrder = await _orderDetailRepository.GetAllOrderDetailByOrderId(orderId, request);
                if (listOrder.TotalCount == 0)
                {
                    response.Messages = ["You don't have any order"];
                    response.ResultStatus = ResultStatus.Empty;
                    return response;
                }
                response.Data = listOrder;
                response.Messages = ["There are " + listOrder.TotalCount + " in total"];
                response.ResultStatus = ResultStatus.Success;
                return response;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
