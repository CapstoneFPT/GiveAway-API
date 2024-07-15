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
using static Org.BouncyCastle.Asn1.Cmp.Challenge;
using Member = BusinessObjects.Entities.Member;

namespace Repositories.Orders
{
    public class OrderRepository : IOrderRepository
    {
        private readonly GenericDao<Order> _orderDao;
        private readonly GenericDao<OrderDetail> _orderDetailDao;
        private readonly GenericDao<FashionItem> _fashionItemDao;
        private readonly GenericDao<Shop> _shopDao;
        private readonly GenericDao<Account> _accountDao;
        private readonly IMapper _mapper;
        private static HashSet<string> generatedStrings = new HashSet<string>();
        private static Random random = new Random();
        private const string prefix = "GA-OD-";

        public OrderRepository(GenericDao<Order> orderDao, GenericDao<OrderDetail> orderDetailDao,
            GenericDao<FashionItem> fashionItemDao,
            GenericDao<Shop> shopDao, GenericDao<Account> accountDao, IMapper mapper)
        {
            _orderDao = orderDao;
            _orderDetailDao = orderDetailDao;
            _fashionItemDao = fashionItemDao;
            _shopDao = shopDao;
            _accountDao = accountDao;
            _mapper = mapper;
        }

        public async Task<Order> CreateOrder(Order order)
        {
            await _orderDao.AddAsync(order);
            return order;
        }

        public async Task<OrderResponse> CreateOrderHierarchy(Guid accountId,
            CreateOrderRequest orderRequest)
        {
            var listItem = await _fashionItemDao.GetQueryable().Include(c => c.Shop)
                .Where(c => orderRequest.listItemId.Contains(c.ItemId)).ToListAsync();
            var shopIds = listItem.Select(c => c.ShopId).Distinct().ToList();

            int totalPrice = 0;
            Order order = new Order();
            order.MemberId = accountId;
            order.Member = await _accountDao.GetQueryable().FirstOrDefaultAsync(c => c.AccountId == accountId);
            order.PaymentMethod = orderRequest.PaymentMethod;
            order.Address = orderRequest.Address;
            order.PurchaseType = PurchaseType.Online;
            order.RecipientName = orderRequest.RecipientName;
            order.Phone = orderRequest.Phone;
            if (orderRequest.PaymentMethod.Equals(PaymentMethod.COD))
            {
                order.Status = OrderStatus.OnDelivery;
            }
            else
            {
                order.Status = OrderStatus.AwaitingPayment;
            }

            order.CreatedDate = DateTime.UtcNow;
            order.TotalPrice = totalPrice;
            order.OrderCode = GenerateUniqueString();

            var result = await CreateOrder(order);

            var listOrderDetailResponse = new List<OrderDetailResponse<FashionItemDetailResponse>>();

            foreach (var id in orderRequest.listItemId)
            {
                var item = await _fashionItemDao.GetQueryable().Include(c => c.Shop)
                    .FirstOrDefaultAsync(c => c.ItemId == id);
                OrderDetail orderDetail = new OrderDetail();
                orderDetail.OrderId = order.OrderId;
                orderDetail.UnitPrice = item.SellingPrice;
                orderDetail.FashionItemId = id;
                orderDetail.FashionItem = item;
                await _orderDetailDao.AddAsync(orderDetail);
                totalPrice += item.SellingPrice;

                var mapresult = _mapper.Map<OrderDetailResponse<FashionItemDetailResponse>>(orderDetail);
                listOrderDetailResponse.Add(mapresult);
            }

            order.TotalPrice = totalPrice;
            var resultUpdate = await _orderDao.UpdateAsync(order);


            var listShopOrderResponse = new List<ShopOrderResponse>();
            foreach (var id in shopIds)
            {
                var shop = await _shopDao.GetQueryable().FirstOrDefaultAsync(c => c.ShopId == id);
                var shopOrder = new ShopOrderResponse();
                shopOrder.ShopId = id;
                shopOrder.ShopAddress = shop.Address;
                shopOrder.Items = listOrderDetailResponse.Where(c => c.FashionItemDetail.ShopId == id).ToList();
                listShopOrderResponse.Add(shopOrder);
            }

            var orderResponse = new OrderResponse()
            {
                OrderId = order.OrderId,
                Quantity = listOrderDetailResponse.Count,
                TotalPrice = order.TotalPrice,
                OrderCode = order.OrderCode,
                CreatedDate = order.CreatedDate,
                PaymentMethod = order.PaymentMethod,
                PurchaseType = order.PurchaseType,
                Address = order.Address,
                RecipientName = order.RecipientName,
                ContactNumber = order.Phone,
                CustomerName = order.Member.Fullname,
                Status = order.Status,
                ShopOrderResponses = listShopOrderResponse,
            };
            return orderResponse;
        }

        public async Task<Order?> GetOrderById(Guid id)
        {
            return await _orderDao.GetQueryable().Include(c => c.Member).FirstOrDefaultAsync(c => c.OrderId == id);
        }

        public async Task<Order?> GetSingleOrder(Expression<Func<Order, bool>> predicate)
        {
            var result = await _orderDao
                    .GetQueryable()
                    .Include(order => order.Member)
                    .Include(order => order.OrderDetails)
                    .ThenInclude(orderDetail => orderDetail.FashionItem)
                    .SingleOrDefaultAsync(predicate);
            return result;
        }

        public async Task<PaginationResponse<OrderResponse>> GetOrdersByAccountId(Guid accountId, OrderRequest request)
        {
            var query = _orderDao.GetQueryable();
            query = query.Include(c => c.Member).Where(c => c.MemberId == accountId).OrderByDescending(c => c.CreatedDate);

            if (request.Status != null)
            {
                query = query.Where(f => f.Status == request.Status);
            }

            if (request.OrderCode != null)
            {
                query = query.Where(f => f.OrderCode.ToUpper().Equals(f.OrderCode.ToUpper()));
            }

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
                    OrderCode = x.OrderCode,
                    PaymentMethod = x.PaymentMethod,
                    PaymentDate = x.PaymentDate,
                    CustomerName = x.Member.Fullname,
                    RecipientName = x.RecipientName,
                    ContactNumber = x.Phone,
                    Address = x.Address,
                    PurchaseType = x.PurchaseType,
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

        public async Task<Order> UpdateOrder(Order order)
        {
            await _orderDao.UpdateAsync(order);
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

        private static string GenerateRandomString()
        {
            int number = random.Next(100000, 1000000);
            return prefix + number.ToString("D6");
        }
        private async Task<Order?> IsCodeExisted(string code)
        {
            return await _orderDao.GetQueryable().FirstOrDefaultAsync(c => c.OrderCode.Equals(code));
        }
        public async Task<List<OrderDetail>> IsOrderExisted(List<Guid?> listItemId, Guid memberId)
        {
            var listorderdetail = await _orderDetailDao.GetQueryable().Where(c => c.Order.MemberId == memberId)
                .Where(c => listItemId.Contains(c.FashionItemId)).ToListAsync();
            return listorderdetail;
        }

        public async Task<List<Guid?>> IsOrderAvailable(List<Guid?> listItemId)
        {
            var listItemNotAvailable = new List<Guid?>();
            foreach (var itemId in listItemId)
            {
                var item = await _fashionItemDao.GetQueryable().FirstOrDefaultAsync(c => c.ItemId == itemId);
                if (item is null || !item.Status.Equals(FashionItemStatus.Available))
                {
                    listItemNotAvailable.Add(itemId);
                }
            }

            return listItemNotAvailable;
        }

        public async Task<PaginationResponse<OrderResponse>> GetOrdersByShopId(Guid shopId, OrderRequest request)
        {
            var listItemId = await _fashionItemDao.GetQueryable().Where(c => c.ShopId == shopId)
                    .Select(c => c.ItemId).ToListAsync();

            var listOrderdetail = new List<OrderDetailResponse<FashionItem>>();
            foreach (var itemId in listItemId)
            {
                var orderDetail = await _orderDetailDao.GetQueryable()
                    .FirstOrDefaultAsync(c => c.FashionItemId.Equals(itemId));
                if (orderDetail != null)
                {
                    var newOrderDetail = new OrderDetailResponse<FashionItem>();
                    newOrderDetail.OrderId = orderDetail.OrderId;
                    newOrderDetail.UnitPrice = orderDetail.UnitPrice;
                    newOrderDetail.FashionItemDetail = await _fashionItemDao.GetQueryable()
                        .FirstOrDefaultAsync(c => c.ItemId == itemId);

                    listOrderdetail.Add(newOrderDetail);
                }
            }

            var listOrderResponse = new List<OrderResponse>();

            foreach (var orderId in listOrderdetail.Select(c => c.OrderId).Distinct())
            {
                var orderRespponse = new OrderResponse();
                orderRespponse = await _orderDao.GetQueryable()
                    .ProjectTo<OrderResponse>(_mapper.ConfigurationProvider)
                    .FirstOrDefaultAsync(c => c.OrderId == orderId);
                orderRespponse.OrderDetails = listOrderdetail.Where(c => c.OrderId == orderId).ToList();
                listOrderResponse.Add(orderRespponse);
            }

            if (request.Status != null)
            {
                listOrderResponse = listOrderResponse.Where(f => f.Status == request.Status).ToList();
            }

            if (request.OrderCode != null)
            {
                listOrderResponse = listOrderResponse
                    .Where(f => f.OrderCode.ToUpper().Equals(f.OrderCode.ToUpper())).ToList();
            }

            var count = listOrderResponse.Count();
            listOrderResponse = listOrderResponse.Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize).ToList();

            var result = new PaginationResponse<OrderResponse>
            {
                Items = listOrderResponse,
                PageSize = request.PageSize,
                TotalCount = count,
                SearchTerm = request.OrderCode,
                PageNumber = request.PageNumber,
            };
            return result;
        }

        public async Task<OrderResponse> ConfirmOrderDelivered(Guid shopId, Guid orderId)
        {
            var orderResponse = new OrderResponse();
            var order = await _orderDao.GetQueryable().FirstOrDefaultAsync(c => c.OrderId == orderId);
            var listorderdetailEachShop = await _orderDetailDao.GetQueryable().Include(c => c.FashionItem)
                .Where(c => c.OrderId == orderId && c.FashionItem.ShopId == shopId)
                .AsNoTracking().ToListAsync();
            var listItemEachshop = listorderdetailEachShop.Select(c => c.FashionItem).ToList();


            foreach (var item in listItemEachshop)
            {
                item.Status = FashionItemStatus.Refundable;
                await _fashionItemDao.UpdateAsync(item);
            }

            var listorderdetail = await _orderDetailDao.GetQueryable().Include(c => c.FashionItem)
                .Where(c => c.OrderId == orderId)
                .AsNoTracking().ToListAsync();


            var listOrderdetailResponse = new List<OrderDetailResponse<FashionItem>>();
            foreach (var item in listItemEachshop)
            {
                var orderDetail = await _orderDetailDao.GetQueryable()
                    .FirstOrDefaultAsync(c => c.FashionItemId.Equals(item.ItemId));
                if (orderDetail != null)
                {
                    orderDetail.RefundExpirationDate = DateTime.UtcNow.AddDays(7);
                    await _orderDetailDao.UpdateAsync(orderDetail);

                    var newOrderDetail = new OrderDetailResponse<FashionItem>();
                    newOrderDetail.OrderId = orderDetail.OrderId;
                    newOrderDetail.UnitPrice = orderDetail.UnitPrice;
                    newOrderDetail.RefundExpirationDate = orderDetail.RefundExpirationDate;
                    newOrderDetail.FashionItemDetail = item;

                    listOrderdetailResponse.Add(newOrderDetail);
                }
                else
                    throw new Exception();
            }

            var listItem = listorderdetail.Select(c => c.FashionItem).ToList();
            if (!listItem.Any(c => c.Status.Equals(FashionItemStatus.OnDelivery)))
            {
                order.Status = OrderStatus.Completed;
                await _orderDao.UpdateAsync(order);
            }

            orderResponse = await _orderDao.GetQueryable().Where(c => c.OrderId == orderId)
                .ProjectTo<OrderResponse>(_mapper.ConfigurationProvider).FirstOrDefaultAsync();
            orderResponse.OrderDetails = listOrderdetailResponse;
            return orderResponse;
        }

        public async Task<List<Order>> GetOrders(Expression<Func<Order, bool>> predicate)
        {
            var result = await _orderDao.GetQueryable()
                    .Where(predicate)
                    .ToListAsync();

            return result;
        }

        public async Task BulkUpdate(List<Order> ordersToUpdate)
        {

            await _orderDao.UpdateRange(ordersToUpdate);

        }

        public async Task<OrderResponse> CreateOrderByShop(Guid shopId, CreateOrderRequest orderRequest)
        {

            var listItem = await _fashionItemDao.GetQueryable().Include(c => c.Shop)
                .Where(c => orderRequest.listItemId.Contains(c.ItemId)).ToListAsync();


            int totalPrice = 0;

            Order order = new Order();
            order.PurchaseType = PurchaseType.Offline;
            order.PaymentMethod = orderRequest.PaymentMethod;
            order.Address = orderRequest.Address;
            order.RecipientName = orderRequest.RecipientName;
            order.Phone = orderRequest.Phone;
            order.Email = orderRequest.Email;
            order.Status = OrderStatus.AwaitingPayment;


            order.CreatedDate = DateTime.UtcNow;
            order.TotalPrice = totalPrice;
            order.OrderCode = GenerateUniqueString();

            var orderresult = await CreateOrder(order);

            var listOrderDetailResponse = new List<OrderDetailResponse<FashionItemDetailResponse>>();

            foreach (var id in orderRequest.listItemId)
            {
                var item = await _fashionItemDao.GetQueryable().Include(c => c.Shop)
                    .FirstOrDefaultAsync(c => c.ItemId == id);
                item.Status = FashionItemStatus.Refundable;
                item = await _fashionItemDao.UpdateAsync(item);
                OrderDetail orderDetail = new OrderDetail();
                orderDetail.OrderId = order.OrderId;
                orderDetail.UnitPrice = item.SellingPrice;
                orderDetail.RefundExpirationDate = DateTime.UtcNow.AddDays(7);
                orderDetail.FashionItemId = id;
                orderDetail.FashionItem = item;
                orderDetail = await _orderDetailDao.AddAsync(orderDetail);
                totalPrice += item.SellingPrice;

                var mapresult = _mapper.Map<OrderDetailResponse<FashionItemDetailResponse>>(orderDetail);
                listOrderDetailResponse.Add(mapresult);
            }

            orderresult.TotalPrice = totalPrice;
            var orderresultUpdate = await _orderDao.UpdateAsync(orderresult);


            var listShopOrderResponse = new List<ShopOrderResponse>();

            var shop = await _shopDao.GetQueryable().FirstOrDefaultAsync(c => c.ShopId == shopId);
            var shopOrder = new ShopOrderResponse();
            shopOrder.ShopId = shopId;
            shopOrder.ShopAddress = shop.Address;
            shopOrder.Items = listOrderDetailResponse.Where(c => c.FashionItemDetail.ShopId == shopId).ToList();
            listShopOrderResponse.Add(shopOrder);



            var orderResponse = new OrderResponse()
            {
                OrderId = orderresultUpdate.OrderId,
                Quantity = listOrderDetailResponse.Count,
                TotalPrice = listOrderDetailResponse.Sum(c => c.UnitPrice),
                CreatedDate = orderresultUpdate.CreatedDate,
                Address = orderresultUpdate.Address,
                ContactNumber = orderresultUpdate.Phone,
                RecipientName = orderresultUpdate.RecipientName,
                PaymentMethod = orderresultUpdate.PaymentMethod,
                PurchaseType = orderresultUpdate.PurchaseType,
                OrderCode = orderresultUpdate.OrderCode,
                Status = orderresultUpdate.Status,
                ShopOrderResponses = listShopOrderResponse
            };
            return orderResponse;


        }
    }
}