using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.FashionItems;
using BusinessObjects.Dtos.OrderDetails;
using BusinessObjects.Dtos.Orders;
using BusinessObjects.Dtos.Refunds;
using BusinessObjects.Entities;
using Dao;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1.Ocsp;

namespace Repositories.OrderDetails
{
    public class OrderDetailRepository : IOrderDetailRepository
    {
      
        private readonly IMapper _mapper;

        public OrderDetailRepository(IMapper mapper)
        {
            _mapper = mapper;
        }

        public async Task<OrderDetail> CreateOrderDetail(OrderDetail orderDetail)
        {
            return await GenericDao<OrderDetail>.Instance.AddAsync(orderDetail);
        }

        public async Task<PaginationResponse<OrderDetailResponse<FashionItem>>> GetAllOrderDetailByOrderId(Guid id,
            OrderDetailRequest request)
        {
            var query = GenericDao<OrderDetail>.Instance.GetQueryable();
            query = query.Where(c => c.OrderId == id);

            if (request.ShopId != null)
            {
                query = query.Where(c => c.FashionItem.ShopId == request.ShopId);
            }
            var count = await query.CountAsync();
            query = query.Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize);


            var items = await query
                .Select(x => new OrderDetailResponse<FashionItem>
                {
                    OrderDetailId = x.OrderDetailId,
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
            var result = await GenericDao<OrderDetail>.Instance.GetQueryable()
                .Include(x => x.FashionItem)
                .Where(predicate)
                .ToListAsync();
            return result;
        }


        public async Task<OrderDetailResponse<FashionItem>> GetOrderDetailById(Guid id)
        {
            var query = await GenericDao<OrderDetail>.Instance.GetQueryable()
                .Where(c => c.OrderDetailId == id)
                .Select(x => new OrderDetailResponse<FashionItem>
                {
                    OrderDetailId = id,
                    FashionItemDetail = x.FashionItem,
                    OrderId = x.OrderId,
                    UnitPrice = x.UnitPrice,
                    RefundExpirationDate = x.RefundExpirationDate,
                }).FirstOrDefaultAsync();
            return query;
        }

        public async Task<List<RefundResponse>> CreateRefundToShop(
            List<CreateRefundRequest> refundRequest)
        {
            var orderDetailIds = refundRequest.Select(r => r.OrderDetailIds).ToList();

            foreach (var item in refundRequest)
            {
                var fashionItem = await GenericDao<OrderDetail>.Instance.GetQueryable()
                    .Include(c => c.FashionItem)
                    .Where(c => c.OrderDetailId == item.OrderDetailIds)
                    .Select(c => c.FashionItem)
                    .FirstOrDefaultAsync();
                var refund = new Refund()
                {
                    OrderDetailId = item.OrderDetailIds,
                    Description = item.Description,
                    CreatedDate = DateTime.UtcNow,
                    RefundStatus = RefundStatus.Pending
                };
                await GenericDao<Refund>.Instance.AddAsync(refund);
                
                for (int i = 0; i < item.Images.Count(); i++)
                {
                    Image img = new Image()
                    {
                        FashionItemId = fashionItem.ItemId,
                        Url = item.Images[i],
                        RefundId = refund.RefundId
                    };
                    await GenericDao<Image>.Instance.AddAsync(img);
                }
            }


            var refundResponse = await GenericDao<Refund>.Instance.GetQueryable()
                
                .Where(c => orderDetailIds.Contains(c.OrderDetailId))
                .ProjectTo<RefundResponse>(_mapper.ConfigurationProvider)
                .ToListAsync();
            return refundResponse;
        }

        public async Task<(List<T> Items, int Page, int PageSize, int TotalCount)>
            GetOrderDetailsPaginate<T>(Expression<Func<OrderDetail, bool>>? predicate,
                Expression<Func<OrderDetail, T>>? selector, bool isTracking, int page = -1, int pageSize = -1)
        {
            var query = GenericDao<OrderDetail>.Instance.GetQueryable();

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            var count = await query.CountAsync();

            if (pageSize >= 0 && page > 0)
            {
                query = query.Skip((page - 1) * pageSize).Take(pageSize);
            }

            List<T> items;

            if (selector != null)
            {
                items = await query.Select(selector).ToListAsync();
            }

            else
            {
                items = await query.Cast<T>().ToListAsync();
            }


            return (items, page, pageSize, count);
        }

        public async Task UpdateRange(List<OrderDetail> orderDetails)
        {
            await GenericDao<OrderDetail>.Instance.UpdateRange(orderDetails);
        }
    }
}