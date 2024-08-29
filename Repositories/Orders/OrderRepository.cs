using AutoMapper;
using AutoMapper.Execution;
using AutoMapper.QueryableExtensions;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.FashionItems;
using BusinessObjects.Dtos.OrderLineItems;
using BusinessObjects.Dtos.Orders;
using BusinessObjects.Dtos.Shops;
using BusinessObjects.Entities;
using Dao;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1.Ocsp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Utils;
using static Org.BouncyCastle.Asn1.Cmp.Challenge;
using Member = BusinessObjects.Entities.Member;

namespace Repositories.Orders
{
    public class OrderRepository : IOrderRepository
    {
        private readonly GiveAwayDbContext _giveAwayDbContext;
        private readonly IMapper _mapper;
        private static HashSet<string> generatedStrings = new HashSet<string>();
        private static Random random = new Random();
        private const string prefix = "GA-OD-";

        public OrderRepository(IMapper mapper, GiveAwayDbContext giveAwayDbContext)
        {
            _mapper = mapper;
            _giveAwayDbContext = giveAwayDbContext;
        }

        public async Task<Order?> CreateOrder(Order? order)
        {
            await GenericDao<Order>.Instance.AddAsync(order);
            return order;
        }

        public async Task<OrderResponse> CreateOrderHierarchy(Guid accountId,
            CartRequest cart)
        {
            var listItem = await GenericDao<IndividualFashionItem>.Instance.GetQueryable()
                .Include(individualFashionItem => individualFashionItem.Variation)
                .ThenInclude(itemVariation => itemVariation.MasterItem)
                .Where(individualFashionItem => 
                    cart.CartItems.Select(ci => ci.ItemId).Contains(individualFashionItem.ItemId))
                .ToListAsync();
            var memberAccount = await GenericDao<Account>.Instance.GetQueryable()
                .FirstOrDefaultAsync(c => c.AccountId == accountId);

            
            Order order = new Order();
            order.MemberId = accountId;

            order.PaymentMethod = cart.PaymentMethod;
            order.Address = cart.Address;
            order.GhnDistrictId = cart.GhnDistrictId;
            order.GhnWardCode = cart.GhnWardCode;
            order.GhnProvinceId = cart.GhnProvinceId;
            order.AddressType = cart.AddressType;
            order.PurchaseType = PurchaseType.Online;
            order.RecipientName = cart.RecipientName;
            order.AddressType = cart.AddressType;
            order.ShippingFee = cart.ShippingFee;
            order.Discount = cart.Discount;
            order.Phone = cart.Phone;
            order.Email = memberAccount.Email;
            if (cart.PaymentMethod.Equals(PaymentMethod.COD))
            {
                order.Status = OrderStatus.Pending;
            }
            else
            {
                order.Status = OrderStatus.AwaitingPayment;
            }

            order.CreatedDate = DateTime.UtcNow;
            order.TotalPrice = listItem.Sum(c => c.SellingPrice!.Value) + cart.ShippingFee - cart.Discount;
            order.OrderCode = GenerateUniqueString();

            var result = await GenericDao<Order>.Instance.AddAsync(order);
            var listOrderDetailResponse = new List<OrderLineItemDetailedResponse>();

            foreach (var individualItem in listItem)
            {
                OrderLineItem orderLineItem = new OrderLineItem();
                orderLineItem.OrderId = order.OrderId;
                orderLineItem.UnitPrice = individualItem.SellingPrice!.Value;
                orderLineItem.CreatedDate = DateTime.UtcNow;
                orderLineItem.Quantity = 1;
                orderLineItem.IndividualFashionItemId = individualItem.ItemId;

                await GenericDao<OrderLineItem>.Instance.AddAsync(orderLineItem);
                if (cart.PaymentMethod.Equals(PaymentMethod.COD))
                {
                    individualItem.Status = FashionItemStatus.PendingForOrder;
                    await GenericDao<IndividualFashionItem>.Instance.UpdateAsync(individualItem);
                }

                var orderDetailResponse = new OrderLineItemDetailedResponse()
                {
                    OrderLineItemId = orderLineItem.OrderLineItemId,
                    ItemName = individualItem.Variation!.MasterItem.Name,
                    UnitPrice = orderLineItem.UnitPrice,
                    CreatedDate = orderLineItem.CreatedDate,
                    OrderCode = order.OrderCode,
                    ItemCode = individualItem.ItemCode,
                    Quantity = orderLineItem.Quantity,
                };
                /*totalPrice += orderDetail.UnitPrice;*/


                listOrderDetailResponse.Add(orderDetailResponse);
            }

            /*order.TotalPrice = totalPrice;
            var resultUpdate = await GenericDao<Order>.Instance.UpdateAsync(result);*/


            var orderResponse = new OrderResponse()
            {
                OrderId = result.OrderId,
                Quantity = listOrderDetailResponse.Count,
                TotalPrice = result.TotalPrice,
                OrderCode = result.OrderCode,
                CreatedDate = result.CreatedDate,
                MemberId = result.MemberId,
                PaymentMethod = result.PaymentMethod,
                PurchaseType = result.PurchaseType,
                Address = result.Address,
                RecipientName = result.RecipientName,
                ContactNumber = result.Phone,
                CustomerName = memberAccount.Fullname,
                Email = result.Email,
                ShippingFee = result.ShippingFee,
                Discount = result.Discount,
                Status = result.Status,
                OrderLineItems = listOrderDetailResponse,
            };
            return orderResponse;
        }

        public async Task<Order?> GetOrderById(Guid id)
        {
            return await GenericDao<Order>.Instance.GetQueryable().Include(c => c.Member)
                .FirstOrDefaultAsync(c => c.OrderId == id);
        }

        public async Task<Order?> GetSingleOrder(Expression<Func<Order, bool>> predicate)
        {
            var result = await GenericDao<Order>.Instance
                .GetQueryable()
                .Include(c => c.Member)
                .Include(order => order.OrderDetails)
                .ThenInclude(orderDetail => orderDetail.IndividualFashionItem)
                .ThenInclude(c => c.Variation)
                .ThenInclude(c => c.MasterItem)
                .SingleOrDefaultAsync(predicate);
            return result;
        }

        public async Task<PaginationResponse<OrderResponse>> GetOrdersByAccountId(Guid accountId, OrderRequest request)
        {
            var query = _giveAwayDbContext.Orders.AsQueryable();
            query = query.Include(c => c.Member)
                .Where(c => c.MemberId == accountId && c.OrderDetails.Any(c => c.PointPackageId == null))
                .OrderByDescending(c => c.CreatedDate);

            if (request.Status != null)
            {
                query = query.Where(f => f.Status == request.Status);
            }

            if (request.OrderCode != null)
            {
                query = query.Where(f => f.OrderCode.ToUpper().Equals(f.OrderCode.ToUpper()));
            }

            var count = await query.CountAsync();
            query = query.Skip((request.PageNumber.Value - 1) * request.PageSize.Value)
                .Take(request.PageSize.Value);

            var list = await _giveAwayDbContext.OrderLineItems.CountAsync();

            var items = await query
                .Select(x => new OrderResponse
                {
                    OrderId = x.OrderId,
                    Quantity = x.OrderDetails.Count,
                    TotalPrice = x.TotalPrice,
                    CreatedDate = x.CreatedDate,
                    OrderCode = x.OrderCode,
                    PaymentMethod = x.PaymentMethod,
                    // PaymentDate = x.PaymentDate,
                    MemberId = x.MemberId,
                    CompletedDate = x.CompletedDate,
                    CustomerName = x.Member.Fullname,
                    RecipientName = x.RecipientName,
                    ContactNumber = x.Phone,
                    Email = x.Email,
                    Address = x.Address,
                    PurchaseType = x.PurchaseType,
                    Status = x.Status,
                })
                .AsNoTracking().ToListAsync();

            var result = new PaginationResponse<OrderResponse>
            {
                Items = items,
                PageSize = request.PageSize.Value,
                TotalCount = count,
                PageNumber = request.PageNumber.Value,
            };
            return result;
        }

        public async Task<Order> UpdateOrder(Order order)
        {
            await GenericDao<Order>.Instance.UpdateAsync(order);
            return order;
        }

        public string GenerateUniqueString()
        {
            string newString;
            do
            {
                newString = GenerateRandomString();
            } while (generatedStrings.Contains(newString) || IsCodeExisted(newString) is null);

            generatedStrings.Add(newString);
            return newString;
        }

        public async Task<(List<T> Items, int Page, int PageSize, int TotalCount)> GetOrdersProjection<T>(
            int? orderRequestPageNumber, int? orderRequestPageSize, Expression<Func<Order, bool>> predicate,
            Expression<Func<Order, T>> selector)
        {
            var query = _giveAwayDbContext.Orders.AsQueryable();
            query = query.OrderByDescending(c => c.CreatedDate);
            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            var count = await query.CountAsync();

            var pageNumber = orderRequestPageNumber ?? -1;
            var pageSize = orderRequestPageSize ?? -1;

            if (pageNumber > 0 && pageSize > 0)
            {
                query = query.Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize);
            }

            List<T> items;
            if (selector != null)
            {
                items = await query
                    .Select(selector)
                    .ToListAsync();
            }
            else
            {
                items = await query
                    .Cast<T>().ToListAsync();
            }

            return (items, pageNumber, pageSize, count);
        }

        private static string GenerateRandomString()
        {
            int number = random.Next(100000, 1000000);
            return prefix + number.ToString("D6");
        }

        private async Task<Order?> IsCodeExisted(string code)
        {
            return await GenericDao<Order>.Instance.GetQueryable().FirstOrDefaultAsync(c => c.OrderCode.Equals(code));
        }

        public async Task<List<OrderLineItem>> IsOrderExisted(List<Guid> listItemId, Guid memberId)
        {
            var listorderdetail = await GenericDao<OrderLineItem>.Instance.GetQueryable()
                .Where(c => c.Order.MemberId == memberId && c.Order.Status.Equals(OrderStatus.AwaitingPayment))
                .Where(c => listItemId.Contains(c.IndividualFashionItemId.Value)).ToListAsync();
            return listorderdetail;
        }

        public async Task<List<Guid>> IsOrderAvailable(List<Guid> listItemId)
        {
            var listItemNotAvailable = new List<Guid>();
            foreach (var itemId in listItemId)
            {
                var item = await GenericDao<IndividualFashionItem>.Instance.GetQueryable()
                    .FirstOrDefaultAsync(c => c.ItemId == itemId);
                if (item is null || !item.Status.Equals(FashionItemStatus.Available))
                {
                    listItemNotAvailable.Add(itemId);
                }
            }

            return listItemNotAvailable;
        }

        public async Task<PaginationResponse<OrderResponse>> GetOrders(OrderRequest request)
        {
            var listItemId = new List<Guid>();
            if (request.ShopId != null)
            {
                listItemId = await GenericDao<IndividualFashionItem>.Instance.GetQueryable()
                    // .Where(c => c.ShopId == request.ShopId)
                    .Select(c => c.ItemId).ToListAsync();
            }
            else
            {
                listItemId = await GenericDao<IndividualFashionItem>.Instance.GetQueryable()
                    .Select(c => c.ItemId).ToListAsync();
            }

            var listOrderdetail = new List<OrderLineItemResponse<IndividualFashionItem>>();
            foreach (var itemId in listItemId)
            {
                var orderDetail = await GenericDao<OrderLineItem>.Instance.GetQueryable()
                    .FirstOrDefaultAsync(c => c.IndividualFashionItemId.Equals(itemId));
                if (orderDetail != null)
                {
                    var newOrderDetail = new OrderLineItemResponse<IndividualFashionItem>();
                    newOrderDetail.OrderId = orderDetail.OrderId;
                    newOrderDetail.UnitPrice = orderDetail.UnitPrice;
                    newOrderDetail.FashionItemDetail = await GenericDao<IndividualFashionItem>.Instance.GetQueryable()
                        .FirstOrDefaultAsync(c => c.ItemId == itemId);

                    listOrderdetail.Add(newOrderDetail);
                }
            }

            var listOrderResponse = new List<OrderResponse>();

            foreach (var orderId in listOrderdetail.Select(c => c.OrderId).Distinct())
            {
                var order = await GenericDao<Order>.Instance.GetQueryable().Include(c => c.Member)
                    .Include(c => c.OrderDetails).FirstOrDefaultAsync(c => c.OrderId == orderId);
                var orderResponse = new OrderResponse()
                {
                    OrderId = order.OrderId,
                    Quantity = order.OrderDetails.Count,
                    TotalPrice = order.TotalPrice,
                    RecipientName = order.RecipientName,
                    OrderCode = order.OrderCode,
                    Address = order.Address,
                    ContactNumber = order.Phone,
                    CreatedDate = order.CreatedDate,
                    // PaymentDate = order.PaymentDate,
                    PaymentMethod = order.PaymentMethod,
                    PurchaseType = order.PurchaseType,
                    Email = order.Email,
                    Status = order.Status
                };
                if (order.MemberId != null)
                {
                    orderResponse.CustomerName = order.Member.Fullname;
                }

                /*orderResponse.OrderDetails = listOrderdetail.Where(c => c.OrderId == orderId).ToList();*/
                listOrderResponse.Add(orderResponse);
            }

            if (request.Status != null)
            {
                listOrderResponse = listOrderResponse.Where(f => f.Status == request.Status).ToList();
            }

            if (request.OrderCode != null)
            {
                listOrderResponse = listOrderResponse
                    .Where(f => f.OrderCode.ToUpper().Contains(request.OrderCode.ToUpper())).ToList();
            }

            var count = listOrderResponse.Count;
            listOrderResponse = listOrderResponse
                .OrderByDescending(c => c.CreatedDate).Skip((request.PageNumber.Value - 1) * request.PageSize.Value)
                .Take(request.PageSize.Value)
                .ToList();

            var result = new PaginationResponse<OrderResponse>
            {
                Items = listOrderResponse,
                PageSize = request.PageSize.Value,
                TotalCount = count,
                SearchTerm = request.OrderCode,
                PageNumber = request.PageNumber.Value,
            };
            return result;
        }

        /*public async Task<Order> ConfirmOrderDelivered(Guid shopId ,Guid orderId)
        {
            var listorderdetail = await GenericDao<OrderDetail>.Instance.GetQueryable()
                /*.Include(c => c.IndividualFashionItem)
                .ThenInclude(c => c.Variation).ThenInclude(c => c.MasterItem)#1#
                .Where(c => c.OrderId == orderId && c.IndividualFashionItem.Variation!.MasterItem.ShopId == shopId)
                .AsNoTracking().ToListAsync();
            var order = await GenericDao<Order>.Instance.GetQueryable().Where(c => c.OrderId == orderId)
                .Include(c => c.OrderDetails)
                .ThenInclude(c => c.IndividualFashionItem)
                .FirstOrDefaultAsync();
            foreach (var orderDetail in listorderdetail)
            {
                var fashionItem = orderDetail.IndividualFashionItem;
                if (fashionItem is { Status: FashionItemStatus.OnDelivery })
                {
                    fashionItem.Status = FashionItemStatus.Refundable;
                    orderDetail.RefundExpirationDate = DateTime.UtcNow.AddDays(7);
                    orderDetail.PaymentDate = DateTime.UtcNow;
                }
                else
                {
                    throw new FashionItemNotFoundException();
                }
            }
        
            await GenericDao<OrderDetail>.Instance.UpdateRange(listorderdetail);
            
            if (order != null && order.Status != OrderStatus.OnDelivery)
            {
                if (order.OrderDetails.All(c => c.IndividualFashionItem.Status.Equals(FashionItemStatus.Refundable)))
                {
                    order.Status = OrderStatus.Completed;
                    order.CompletedDate = DateTime.UtcNow;
                }
                
                await GenericDao<Order>.Instance.UpdateAsync(order);
            }
            else
            {
                throw new OrderNotFoundException();
            }
            
            return order;
        }*/

        public async Task<List<Order>> GetOrders(Expression<Func<Order, bool>> predicate)
        {
            var result = await GenericDao<Order>.Instance.GetQueryable()
                .Where(predicate)
                .ToListAsync();

            return result;
        }

        public async Task BulkUpdate(List<Order> ordersToUpdate)
        {
            await GenericDao<Order>.Instance.UpdateRange(ordersToUpdate);
        }

        public async Task<OrderResponse> CreateOrderByShop(Guid shopId, CreateOrderRequest orderRequest)
        {
            var listItem = await GenericDao<IndividualFashionItem>.Instance.GetQueryable()
                // .Include(c => c.Shop)
                .Where(c => orderRequest.ItemIds.Contains(c.ItemId)).ToListAsync();


            decimal totalPrice = 0;

            Order order = new Order();
            order.PurchaseType = PurchaseType.Offline;
            order.PaymentMethod = PaymentMethod.Cash;
            order.Address = orderRequest.Address;
            order.RecipientName = orderRequest.RecipientName;
            order.Phone = orderRequest.Phone;
            order.Email = orderRequest.Email;
            order.Status = OrderStatus.AwaitingPayment;


            order.CreatedDate = DateTime.UtcNow;
            order.TotalPrice = listItem.Sum(c => c.SellingPrice!.Value);
            order.OrderCode = GenerateUniqueString();

            await CreateOrder(order);

            var listOrderDetailResponse = new List<OrderLineItemDetailedResponse>();

            foreach (var item in listItem)
            {
                OrderLineItem orderLineItem = new OrderLineItem();
                orderLineItem.OrderId = order.OrderId;
                orderLineItem.UnitPrice = item.SellingPrice!.Value;
                orderLineItem.CreatedDate = DateTime.UtcNow;
                orderLineItem.Quantity = 1;
                orderLineItem.IndividualFashionItemId = item.ItemId;

                await GenericDao<OrderLineItem>.Instance.AddAsync(orderLineItem);
                item.Status = FashionItemStatus.OnDelivery;
                await GenericDao<IndividualFashionItem>.Instance.UpdateAsync(item);

                var orderDetailResponse = await _giveAwayDbContext.OrderLineItems.AsQueryable()
                    .Where(c => c.IndividualFashionItemId == item.ItemId)
                    .ProjectTo<OrderLineItemDetailedResponse>(_mapper.ConfigurationProvider)
                    .AsNoTracking()
                    .FirstOrDefaultAsync();
                totalPrice += orderLineItem.UnitPrice;

                listOrderDetailResponse.Add(orderDetailResponse);
            }

            var orderResponse = new OrderResponse()
            {
                OrderId = order.OrderId,
                Quantity = listOrderDetailResponse.Count,
                TotalPrice = order.TotalPrice,
                CreatedDate = order.CreatedDate,
                Address = order.Address,
                ContactNumber = order.Phone,
                RecipientName = order.RecipientName,
                Email = order.Email,
                PaymentMethod = order.PaymentMethod,
                PurchaseType = order.PurchaseType,
                OrderCode = order.OrderCode,
                Status = order.Status,
                OrderLineItems = listOrderDetailResponse
            };
            return orderResponse;
        }
    }
}