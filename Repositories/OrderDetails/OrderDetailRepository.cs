using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.FashionItems;
using BusinessObjects.Dtos.OrderDetails;
using BusinessObjects.Dtos.Orders;
using BusinessObjects.Entities;
using Dao;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1.Ocsp;

namespace Repositories.OrderDetails
{
    public class OrderDetailRepository : IOrderDetailRepository
    {
        private readonly GenericDao<OrderDetail> _orderDetailDao;

        public OrderDetailRepository(GenericDao<OrderDetail> orderdetailDao)
        {
            _orderDetailDao = orderdetailDao;
        }

        public async Task<OrderDetail> CreateOrderDetail(OrderDetail orderDetail)
        {
            return await _orderDetailDao.AddAsync(orderDetail);
        }

        public async Task<PaginationResponse<OrderDetailResponse<FashionItem>>> GetAllOrderDetailByOrderId(Guid id,
            OrderDetailRequest request)
        {
            var query = _orderDetailDao.GetQueryable();
            query = query.Where(c => c.OrderId == id);

            var count = await query.CountAsync();
            query = query.Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize);


            var items = await query
                .Select(x => new OrderDetailResponse<FashionItem>
                {
                    FashionItemDetail = x.FashionItem,
                    OrderId = x.OrderId,
                    UnitPrice = x.UnitPrice,
                })
                .AsNoTracking().ToListAsync();

            var result = new PaginationResponse<OrderDetailResponse<FashionItem>>
            {
                Items = items,
                PageSize = request.PageSize,
                TotalCount = count,
                PageNumber = request.PageNumber,
            };
            return result;
        }

        public async Task<List<OrderDetail>> GetOrderDetails(Expression<Func<OrderDetail, bool>> predicate)
        {
            var result = await _orderDetailDao.GetQueryable()
                .Include(x => x.FashionItem)
                .Where(predicate)
                .ToListAsync();
            return result;
        }


        public async Task<OrderDetailResponse<FashionItem>> GetOrderDetailById(Guid id)
        {
            var query = await _orderDetailDao.GetQueryable()
                .Where(c => c.OrderId == id)
                .Select(x => new OrderDetailResponse<FashionItem>
                {
                    FashionItemDetail = x.FashionItem,
                    OrderId = x.OrderId,
                    UnitPrice = x.UnitPrice,
                }).FirstOrDefaultAsync();
            return query;
        }
    }
}