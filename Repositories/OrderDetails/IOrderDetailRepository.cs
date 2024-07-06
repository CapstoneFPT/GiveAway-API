﻿using BusinessObjects.Dtos.Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Dtos.OrderDetails;
using BusinessObjects.Entities;

namespace Repositories.OrderDetails
{
    public interface IOrderDetailRepository
    {
        Task<PaginationResponse<OrderDetailResponse<FashionItem>>> GetAllOrderDetailByOrderId(Guid id, OrderDetailRequest request);
        Task<List<OrderDetail>> GetOrderDetails(Expression<Func<OrderDetail, bool>> predicate);
        Task<OrderDetailResponse<FashionItem>> GetOrderDetailById(Guid id);
        Task<OrderDetail> CreateOrderDetail(OrderDetail orderDetail);
    }
}
