using System.Linq.Expressions;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.OrderLineItems;
using BusinessObjects.Dtos.Refunds;
using BusinessObjects.Entities;
using BusinessObjects.Utils;
using Dao;
using DotNext;
using Microsoft.EntityFrameworkCore;

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

        public async Task<Result<PaginationResponse<OrderLineItemListResponse>, ErrorCode>>
            GetAllOrderLineItemsByOrderId(Guid orderId,
                OrderLineItemRequest request)
        {
            try
            {
                var query = GenericDao<OrderLineItem>.Instance.GetQueryable();
                Expression<Func<OrderLineItem, OrderLineItemListResponse>> selector = order =>
                    new()
                    {
                        OrderLineItemId = order.OrderLineItemId,
                        CreatedDate = order.CreatedDate,
                        OrderCode = order.Order.OrderCode,
                        ItemCode = order.IndividualFashionItem.ItemCode,
                        ItemColor = order.IndividualFashionItem.Color,
                        Condition = order.IndividualFashionItem.Condition,
                        CategoryName = order.IndividualFashionItem.MasterItem.Category.Name,
                        UnitPrice = order.UnitPrice,
                        ItemGender = order.IndividualFashionItem.MasterItem.Gender,
                        ItemBrand = order.IndividualFashionItem.MasterItem.Brand,
                        ItemImage = order.IndividualFashionItem.Images.Select(x => x.Url).ToList(),
                        ItemName = order.IndividualFashionItem.MasterItem.Name,
                        Quantity = order.Quantity,
                        ShopId = order.IndividualFashionItem.MasterItem.ShopId,
                        ItemStatus = order.IndividualFashionItem.Status,
                        ItemType = order.IndividualFashionItem.Type,
                        ItemNote = order.IndividualFashionItem.Note,
                        ItemSize = order.IndividualFashionItem.Size,
                        PaymentDate = order.PaymentDate,
                        ShopAddress = order.IndividualFashionItem.MasterItem.Shop.Address,
                        PointPackageId = order.PointPackageId,
                        RefundExpirationDate = order.RefundExpirationDate
                    };

                query = query.Include(c => c.PointPackage)
                    .Include(c => c.IndividualFashionItem)
                    .ThenInclude(c => c.MasterItem)
                    .ThenInclude(c => c.Shop)
                    .Where(c => c.OrderId == orderId);

                if (request.ShopId != null)
                {
                    query = query.Where(c => c.IndividualFashionItem.MasterItem.ShopId == request.ShopId);
                }

                var count = await query.CountAsync();


                var page = request.PageSize ?? -1;
                var pageSize = request.PageSize ?? -1;

                if (page > 0 && pageSize > 0)
                {
                    query = query
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize);
                }


                var items = await query
                    .Select(selector)
                    .AsNoTracking().ToListAsync();

                var result = new PaginationResponse<OrderLineItemListResponse>
                {
                    Items = items,
                    PageSize = request.PageSize ?? -1,
                    TotalCount = count,
                    PageNumber = request.PageNumber ?? -1,
                };
                return new Result<PaginationResponse<OrderLineItemListResponse>, ErrorCode>(result);
            }
            catch (Exception e)
            {
                return new Result<PaginationResponse<OrderLineItemListResponse>, ErrorCode>(ErrorCode.ServerError);
            }
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

            if (fashionItem == null)
            {
                throw new FashionItemNotFoundException();
            }

            fashionItem.Status = FashionItemStatus.PendingForRefund;

            await GenericDao<IndividualFashionItem>.Instance.UpdateAsync(fashionItem);
            var refund = new Refund()
            {
                OrderLineItemId = refundRequest.OrderLineItemId,
                Description = refundRequest.Description,
                CreatedDate = DateTime.UtcNow,
                RefundStatus = RefundStatus.Pending,
                Images = refundRequest.Images.Select(x => new Image()
                {
                    CreatedDate = DateTime.UtcNow,
                    Url = x
                }).ToList(),
            };
            var refundCreated = await GenericDao<Refund>.Instance.AddAsync(refund);

            var refundResponse = await GenericDao<Refund>.Instance.GetQueryable()
                .Include(x => x.OrderLineItem)
                .ThenInclude(x => x.Order)
                .ThenInclude(x=>x.Member)
                .Include(x => x.OrderLineItem)
                .ThenInclude(x => x.IndividualFashionItem)
                .Where(x => x.RefundId == refundCreated.RefundId).Select(x => new RefundResponse()
                {
                    CreatedDate = refundCreated.CreatedDate,
                    RefundId = refundCreated.RefundId,
                    Description = refundRequest.Description,
                    RefundStatus = refundCreated.RefundStatus,
                    OrderCode = refund.OrderLineItem.Order.OrderCode,
                    ItemCode = refund.OrderLineItem.IndividualFashionItem.ItemCode,
                    UnitPrice = refund.OrderLineItem.UnitPrice,
                    ItemName = refund.OrderLineItem.IndividualFashionItem.MasterItem.Name,
                    CustomerEmail =
                        refund.OrderLineItem.Order != null ? refund.OrderLineItem.Order.Email : string.Empty,
                    OrderLineItemId = refundRequest.OrderLineItemId,
                    CustomerName = refund.OrderLineItem.Order != null
                        ? refund.OrderLineItem.Order.Member != null
                            ? refund.OrderLineItem.Order.Member.Fullname
                            : string.Empty
                        : string.Empty,
                    RecipientName = refund.OrderLineItem.Order != null
                        ? refund.OrderLineItem.Order.RecipientName
                        : string.Empty,
                    CustomerPhone =
                        refund.OrderLineItem.Order != null ? refund.OrderLineItem.Order.Phone : string.Empty,
                    ItemImages = refund.OrderLineItem.IndividualFashionItem.Images.Select(c => c.Url).ToArray(),
                    RefundAmount = refund.OrderLineItem.UnitPrice * refund.RefundPercentage / 100,
                    RefundPercentage = refund.RefundPercentage,
                    ImagesForCustomer = refund.Images.Select(image => image.Url).ToArray(),
                    ResponseFromShop = refund.ResponseFromShop
                }).FirstOrDefaultAsync();
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