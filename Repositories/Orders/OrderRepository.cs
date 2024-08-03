using AutoMapper;
using AutoMapper.Execution;
using AutoMapper.QueryableExtensions;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.FashionItems;
using BusinessObjects.Dtos.OrderDetails;
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
            var listItem = await GenericDao<FashionItem>.Instance.GetQueryable().Include(c => c.Shop)
                .Where(c => cart.ItemIds.Contains(c.ItemId)).ToListAsync();
            var shopIds = listItem.Select(c => c.ShopId).Distinct().ToList();
            var memberAccount = await GenericDao<Account>.Instance.GetQueryable()
                .FirstOrDefaultAsync(c => c.AccountId == accountId);

            int totalPrice = 0;
            Order order = new Order();
            order.MemberId = accountId;

            order.PaymentMethod = cart.PaymentMethod;
            order.Address = cart.Address;
            order.PurchaseType = PurchaseType.Online;
            order.RecipientName = cart.RecipientName;
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
            order.TotalPrice = totalPrice;
            order.OrderCode = GenerateUniqueString();

            var result = await GenericDao<Order>.Instance.AddAsync(order);
            var listOrderDetailResponse = new List<OrderDetailsResponse>();

            foreach (var id in cart.ItemIds)
            {
                var item = await GenericDao<FashionItem>.Instance.GetQueryable().Include(c => c.Shop)
                    .FirstOrDefaultAsync(c => c.ItemId == id);
                OrderDetail orderDetail = new OrderDetail();
                orderDetail.OrderId = order.OrderId;
                orderDetail.UnitPrice = item.SellingPrice.Value;
                orderDetail.CreatedDate = DateTime.UtcNow;
                orderDetail.FashionItemId = id;

                await GenericDao<OrderDetail>.Instance.AddAsync(orderDetail);
                if (cart.PaymentMethod.Equals(PaymentMethod.COD))
                {
                    item.Status = FashionItemStatus.PendingForOrder;
                    await GenericDao<FashionItem>.Instance.UpdateAsync(item);
                }

                var orderDetailResponse = new OrderDetailsResponse()
                {
                    OrderDetailId = orderDetail.OrderDetailId,
                    ItemName = item.Name,
                    UnitPrice = orderDetail.UnitPrice,
                    CreatedDate = orderDetail.CreatedDate
                };
                totalPrice += orderDetail.UnitPrice;


                listOrderDetailResponse.Add(orderDetailResponse);
            }

            order.TotalPrice = totalPrice;
            var resultUpdate = await GenericDao<Order>.Instance.UpdateAsync(result);


            var orderResponse = new OrderResponse()
            {
                OrderId = resultUpdate.OrderId,
                Quantity = listOrderDetailResponse.Count,
                TotalPrice = resultUpdate.TotalPrice,
                OrderCode = resultUpdate.OrderCode,
                CreatedDate = resultUpdate.CreatedDate,
                MemberId = resultUpdate.MemberId,
                PaymentMethod = resultUpdate.PaymentMethod,
                PurchaseType = resultUpdate.PurchaseType,
                Address = resultUpdate.Address,
                RecipientName = resultUpdate.RecipientName,
                ContactNumber = resultUpdate.Phone,
                CustomerName = memberAccount.Fullname,
                Email = resultUpdate.Email,
                Status = resultUpdate.Status,
                OrderDetailItems = listOrderDetailResponse,
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
                .ThenInclude(orderDetail => orderDetail.FashionItem)
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

            var list = await _giveAwayDbContext.OrderDetails.CountAsync();

            var items = await query
                .Select(x => new OrderResponse
                {
                    OrderId = x.OrderId,
                    Quantity = x.OrderDetails.Count,
                    TotalPrice = x.TotalPrice,
                    CreatedDate = x.CreatedDate,
                    OrderCode = x.OrderCode,
                    PaymentMethod = x.PaymentMethod,
                    PaymentDate = x.PaymentDate,
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

        public async Task<List<OrderDetail>> IsOrderExisted(List<Guid?> listItemId, Guid memberId)
        {
            var listorderdetail = await GenericDao<OrderDetail>.Instance.GetQueryable()
                .Where(c => c.Order.MemberId == memberId && c.Order.Status.Equals(OrderStatus.AwaitingPayment))
                .Where(c => listItemId.Contains(c.FashionItemId)).ToListAsync();
            return listorderdetail;
        }

        public async Task<List<Guid?>> IsOrderAvailable(List<Guid?> listItemId)
        {
            var listItemNotAvailable = new List<Guid?>();
            foreach (var itemId in listItemId)
            {
                var item = await GenericDao<FashionItem>.Instance.GetQueryable()
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
                listItemId = await GenericDao<FashionItem>.Instance.GetQueryable()
                    .Where(c => c.ShopId == request.ShopId)
                    .Select(c => c.ItemId).ToListAsync();
            }
            else
            {
                listItemId = await GenericDao<FashionItem>.Instance.GetQueryable()
                    .Select(c => c.ItemId).ToListAsync();
            }

            var listOrderdetail = new List<OrderDetailResponse<FashionItem>>();
            foreach (var itemId in listItemId)
            {
                var orderDetail = await GenericDao<OrderDetail>.Instance.GetQueryable()
                    .FirstOrDefaultAsync(c => c.FashionItemId.Equals(itemId));
                if (orderDetail != null)
                {
                    var newOrderDetail = new OrderDetailResponse<FashionItem>();
                    newOrderDetail.OrderId = orderDetail.OrderId;
                    newOrderDetail.UnitPrice = orderDetail.UnitPrice;
                    newOrderDetail.FashionItemDetail = await GenericDao<FashionItem>.Instance.GetQueryable()
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
                    PaymentDate = order.PaymentDate,
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

        public async Task<OrderResponse> ConfirmOrderDelivered(Guid orderId)
        {
            var listorderdetail = await GenericDao<OrderDetail>.Instance.GetQueryable().Include(c => c.FashionItem)
                .Where(c => c.OrderId == orderId)
                .AsNoTracking().ToListAsync();

            foreach (var orderDetail in listorderdetail)
            {
                var fashionItem = orderDetail.FashionItem;
                if (fashionItem != null && fashionItem.Status.Equals(FashionItemStatus.OnDelivery))
                {
                    fashionItem.Status = FashionItemStatus.Refundable;
                    orderDetail.RefundExpirationDate = DateTime.UtcNow.AddDays(7);
                }
                else
                {
                    throw new FashionItemNotFoundException();
                }
            }

            await GenericDao<OrderDetail>.Instance.UpdateRange(listorderdetail);
            var order = await GenericDao<Order>.Instance.GetQueryable().Where(c => c.OrderId == orderId)
                .FirstOrDefaultAsync();
            order.Status = OrderStatus.Completed;
            order.CompletedDate = DateTime.UtcNow;
            order.PaymentDate = DateTime.UtcNow;
            await GenericDao<Order>.Instance.UpdateAsync(order);
            var orderResponse = await GenericDao<Order>.Instance.GetQueryable().Include(c => c.OrderDetails)
                .Where(c => c.OrderId == orderId)
                .ProjectTo<OrderResponse>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();
            return orderResponse;
        }

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
            var listItem = await GenericDao<FashionItem>.Instance.GetQueryable().Include(c => c.Shop)
                .Where(c => orderRequest.ItemIds.Contains(c.ItemId)).ToListAsync();


            int totalPrice = 0;

            Order order = new Order();
            order.PurchaseType = PurchaseType.Offline;
            order.PaymentMethod = PaymentMethod.Cash;
            order.Address = orderRequest.Address;
            order.RecipientName = orderRequest.RecipientName;
            order.Phone = orderRequest.Phone;
            order.Email = orderRequest.Email;
            order.Status = OrderStatus.AwaitingPayment;


            order.CreatedDate = DateTime.UtcNow;
            order.TotalPrice = totalPrice;
            order.OrderCode = GenerateUniqueString();

            var orderresult = await CreateOrder(order);

            var listOrderDetailResponse = new List<OrderDetailsResponse>();

            foreach (var id in orderRequest.ItemIds)
            {
                var item = await GenericDao<FashionItem>.Instance.GetQueryable().Include(c => c.Shop)
                    .FirstOrDefaultAsync(c => c.ItemId == id);

                OrderDetail orderDetail = new OrderDetail();
                orderDetail.OrderId = orderresult.OrderId;
                orderDetail.UnitPrice = item.SellingPrice.Value;
                orderDetail.CreatedDate = DateTime.UtcNow;

                orderDetail.FashionItemId = id;

                await GenericDao<OrderDetail>.Instance.AddAsync(orderDetail);
                item.Status = FashionItemStatus.OnDelivery;
                await GenericDao<FashionItem>.Instance.UpdateAsync(item);

                var orderDetailResponse = await _giveAwayDbContext.OrderDetails.AsQueryable()
                    .Where(c => c.FashionItemId == id)
                    .ProjectTo<OrderDetailsResponse>(_mapper.ConfigurationProvider)
                    .AsNoTracking()
                    .FirstOrDefaultAsync();
                totalPrice += orderDetail.UnitPrice;

                listOrderDetailResponse.Add(orderDetailResponse);
            }

            orderresult.TotalPrice = totalPrice;
            var orderresultUpdate = await GenericDao<Order>.Instance.UpdateAsync(orderresult);


            var orderResponse = new OrderResponse()
            {
                OrderId = orderresultUpdate.OrderId,
                Quantity = listOrderDetailResponse.Count,
                TotalPrice = orderresultUpdate.TotalPrice,
                CreatedDate = orderresultUpdate.CreatedDate,
                Address = orderresultUpdate.Address,
                ContactNumber = orderresultUpdate.Phone,
                RecipientName = orderresultUpdate.RecipientName,
                Email = orderresultUpdate.Email,
                PaymentMethod = orderresultUpdate.PaymentMethod,
                PurchaseType = orderresultUpdate.PurchaseType,
                OrderCode = orderresultUpdate.OrderCode,
                Status = orderresultUpdate.Status,
                OrderDetailItems = listOrderDetailResponse
            };
            return orderResponse;
        }
    }
}