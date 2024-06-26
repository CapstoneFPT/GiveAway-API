using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.FashionItems;
using BusinessObjects.Dtos.Orders;
using BusinessObjects.Entities;
using Dao;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1.Ocsp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Orders
{
    public class OrderRepository : IOrderRepository
    {
        private readonly GenericDao<Order> _orderDao;
        private readonly GenericDao<OrderDetail> _orderDetailDao;

        public OrderRepository(GenericDao<Order> orderDao, GenericDao<OrderDetail> orderDetailDao)
        {
            _orderDao = orderDao;
            _orderDetailDao = orderDetailDao;
        }

        public async Task<PaginationResponse<OrderResponse>> GetOrdersByAccountId(Guid accountId, OrderRequest request)
        {
            try
            {
                var query = _orderDao.GetQueryable();
                    query = query.Where(c => c.MemberId == accountId);
                    
                var count = await query.CountAsync();
                query = query.Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize);

                var list = await _orderDetailDao.GetQueryable().CountAsync();

                var items = await query
                    .Select(x => new OrderResponse
                    {
                        OrderId = x.OrderId,
                        Quantity = _orderDetailDao.GetQueryable().Count(c => c.OrderId.Equals(x.OrderId)),
                        TotalPrice = _orderDetailDao.GetQueryable().Sum(c => c.UnitPrice),
                        CreatedDate = x.CreatedDate,
                        PaymentMethod = x.PaymentMethod,
                        PaymentDate = x.PaymentDate,
                        CustomerName = x.Member.Fullname,
                        RecipientName = x.Delivery.RecipientName,
                        ContactNumber = x.Delivery.Phone,
                        Address = x.Delivery.Address,
                        Status = x.Status,
                    })
                    .AsNoTracking().ToListAsync();

                var result = new PaginationResponse<OrderResponse>
                {
                    Items = items,
                    PageSize = request.PageSize,
                    TotalCount = count,
                    PageNumber = request.PageNumber,
                };
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
