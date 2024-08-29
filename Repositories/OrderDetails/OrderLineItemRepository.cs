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
using BusinessObjects.Dtos.OrderLineItems;
using BusinessObjects.Dtos.Orders;
using BusinessObjects.Dtos.Refunds;
using BusinessObjects.Entities;
using Dao;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1.Ocsp;

namespace Repositories.OrderLineItems
{
    public class OrderLineItemRepository : IOrderLineItemRepository
    {
      
        private readonly IMapper _mapper;

        public OrderLineItemRepository(IMapper mapper)
        {
            _mapper = mapper;
        }

        public async Task<OrderLineItem> CreateOrderLineItem(OrderLineItem orderLineItem)
        {
            return await GenericDao<OrderLineItem>.Instance.AddAsync(orderLineItem);
        }

        public async Task<PaginationResponse<OrderLineItemDetailedResponse>> GetAllOrderLineItemsByOrderId(Guid orderId,
            OrderLineItemRequest request)
        {
            var query = GenericDao<OrderLineItem>.Instance.GetQueryable();
            query = query.Include(c => c.PointPackage)
                .Include(c => c.IndividualFashionItem)
                .ThenInclude(c => c.Variation)
                .ThenInclude(c => c.MasterItem)
                .ThenInclude(c => c.Shop)
                .Where(c => c.OrderId == orderId);
            
            if (request.ShopId != null)
            {
                query = query.Where(c => c.IndividualFashionItem.Variation!.MasterItem.ShopId == request.ShopId);
            }
            var count = await query.CountAsync();
            query = query.Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize);

            
            var items = await query
                .ProjectTo<OrderLineItemDetailedResponse>(_mapper.ConfigurationProvider)
                .AsNoTracking().ToListAsync();

            var result = new PaginationResponse<OrderLineItemDetailedResponse>
            {
                Items = items,
                PageSize = request.PageSize,
                TotalCount = count,
                PageNumber = request.PageNumber,
            };
            return result;
        }

        public async Task<List<OrderLineItem>> GetOrderLineItems(Expression<Func<OrderLineItem, bool>> predicate)
        {
            var result = await GenericDao<OrderLineItem>.Instance.GetQueryable()
                .Include(x => x.IndividualFashionItem)
                .Where(predicate)
                .ToListAsync();
            return result;
        }


        public async Task<OrderLineItem> GetOrderLineItemById(Guid id)
        {
            var query = await GenericDao<OrderLineItem>.Instance.GetQueryable()
                .Include(c => c.IndividualFashionItem)
                .Include(c => c.Order)
                .Where(c => c.OrderLineItemId == id)
                .FirstOrDefaultAsync();
            return query;
        }

        public async Task<RefundResponse> CreateRefundToShop(
            CreateRefundRequest refundRequest)
        {
            

            var fashionItem = await GenericDao<OrderLineItem>.Instance.GetQueryable()
                .Include(c => c.IndividualFashionItem)
                .Where(c => c.OrderLineItemId == refundRequest.OrderLineItemId)
                .Select(c => c.IndividualFashionItem)
                .FirstOrDefaultAsync();
            fashionItem.Status = FashionItemStatus.PendingForRefund;
            await GenericDao<IndividualFashionItem>.Instance.UpdateAsync(fashionItem);
            var refund = new Refund()
            {
                OrderLineItemId = refundRequest.OrderLineItemId,
                Description = refundRequest.Description,
                CreatedDate = DateTime.UtcNow,
                RefundStatus = RefundStatus.Pending
            };
            await GenericDao<Refund>.Instance.AddAsync(refund);

            List<Image> listImage = new List<Image>();
            foreach (var img in refundRequest.Images)
            {
                var newImg = new Image()
                {
                    IndividualFashionItemId = fashionItem.ItemId,
                    RefundId = refund.RefundId,
                    CreatedDate = DateTime.UtcNow,
                    Url = img,
                }; 
                listImage.Add(newImg);
            }

            await GenericDao<Image>.Instance.AddRange(listImage);
            var refundResponse = await GenericDao<Refund>.Instance.GetQueryable()
                
                .Where(c => c.OrderLineItemId == refundRequest.OrderLineItemId)
                .ProjectTo<RefundResponse>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();
            return refundResponse;
        }

        public async Task<(List<T> Items, int Page, int PageSize, int TotalCount)>
            GetOrderLineItemsPaginate<T>(Expression<Func<OrderLineItem, bool>>? predicate,
                Expression<Func<OrderLineItem, T>>? selector, bool isTracking, int page = -1, int pageSize = -1)
        {
            var query = GenericDao<OrderLineItem>.Instance.GetQueryable();

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

        public async Task UpdateRange(List<OrderLineItem> orderLineItems)
        {
            await GenericDao<OrderLineItem>.Instance.UpdateRange(orderLineItems);
        }

        public IQueryable<OrderLineItem> GetQueryable()
        {
            return GenericDao<OrderLineItem>.Instance.GetQueryable();
        }
    }
}