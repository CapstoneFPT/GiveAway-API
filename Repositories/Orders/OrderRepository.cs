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
        public OrderRepository(GenericDao<Order> orderDao, GenericDao<OrderDetail> orderDetailDao, GenericDao<FashionItem> fashionItemDao,
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

        public async Task<OrderResponse> CreateOrderHierarchy(Guid accountId, List<Guid?> listItemId, CreateOrderRequest orderRequest)
        {
            var listItem = await _fashionItemDao.GetQueryable().Include(c => c.Shop).Where(c => listItemId.Contains(c.ItemId)).ToListAsync();
            var shopIds = listItem.Select(c => c.ShopId).Distinct().ToList();

            int totalPrice = 0;
                Order order = new Order();
                order.MemberId = accountId;
                order.Member = await _accountDao.GetQueryable().FirstOrDefaultAsync(c => c.AccountId == accountId);
                order.PaymentMethod = orderRequest.PaymentMethod;
                order.Address = orderRequest.Address;
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

            foreach (var id in listItemId)
            {
                var item = await _fashionItemDao.GetQueryable().Include(c => c.Shop).FirstOrDefaultAsync(c => c.ItemId == id);
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
                TotalPrice = order.TotalPrice,
                OrderCode = order.OrderCode,
                CreatedDate = order.CreatedDate,
                PaymentMethod = order.PaymentMethod,
                Address = order.Address,
                RecipientName = order.RecipientName,
                ContactNumber = order.Phone,
                CustomerName = order.Member.Fullname,
                Status = order.Status,
                shopOrderResponses = listShopOrderResponse,
            };
            return orderResponse;
        }

        public async Task<Order> GetOrderById(Guid id)
        {
            return await _orderDao.GetQueryable().FirstOrDefaultAsync(c => c.OrderId == id);
        }

        public async Task<Order?> GetSingleOrder(Expression<Func<Order,bool>> predicate)
        {
            try
            {
                var result = await _orderDao
                    .GetQueryable()
                    .SingleOrDefaultAsync(predicate);
                return result;
            }
            catch (Exception e)
            {
                throw new Exception();
            } 
        }

        public async Task<PaginationResponse<OrderResponse>> GetOrdersByAccountId(Guid accountId, OrderRequest request)
        {
            try
            {
                var query = _orderDao.GetQueryable();
                    query = query.Where(c => c.MemberId == accountId).OrderByDescending(c => c.CreatedDate);

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

        public async Task<Order> UpdateOrder(Order order)
        {
            await _orderDao.UpdateAsync(order);
            return order;
        }
        public static string GenerateUniqueString()
        {
            string newString;
            do
            {
                newString = GenerateRandomString();
            } while (generatedStrings.Contains(newString));

            generatedStrings.Add(newString);
            return newString;
        }

        private static string GenerateRandomString()
        {
            int number = random.Next(100000, 1000000);
            return prefix + number.ToString("D6");
        }

        public async Task<List<OrderDetail>> IsOrderExisted(List<Guid?> listItemId, Guid memberId)
        {
            var listorderdetail = await _orderDetailDao.GetQueryable().Where(c => c.Order.MemberId == memberId).Where(c => listItemId.Contains(c.FashionItemId)).ToListAsync();
            return listorderdetail;
        }

        public async Task<List<Guid?>> IsOrderAvailable(List<Guid?> listItemId)
        {
            var listItemNotAvailable = new List<Guid?>();
            var listItem = await _fashionItemDao.GetQueryable().Include(c => c.Shop).Where(c => listItemId.Contains(c.ItemId)).ToListAsync();
            foreach (FashionItem item in listItem)
            {
                if (!item.Status.Equals(FashionItemStatus.Available.ToString()))
                {
                    listItemNotAvailable.Add(item.ItemId);
                }
            }
            return listItemNotAvailable;
        }

        public async Task<PaginationResponse<OrderResponse>> GetOrdersByShopId(Guid shopId, OrderRequest request)
        {
            try
            {
                var listItemId = await _fashionItemDao.GetQueryable().Where(c => c.ShopId == shopId).Select(c => c.ItemId).ToListAsync();

                var listOrderdetail = new List<OrderDetailResponse<FashionItem>>();
                foreach (var itemId in listItemId)
                {
                    var orderDetail = await _orderDetailDao.GetQueryable().FirstOrDefaultAsync(c => c.FashionItemId.Equals(itemId));
                    if (orderDetail != null)
                    {
                        var newOrderDetail = new OrderDetailResponse<FashionItem>();
                        newOrderDetail.OrderId = orderDetail.OrderId;
                        newOrderDetail.UnitPrice = orderDetail.UnitPrice;
                        newOrderDetail.FashionItemDetail = await _fashionItemDao.GetQueryable().FirstOrDefaultAsync(c => c.ItemId == itemId);

                        listOrderdetail.Add(newOrderDetail);
                    }
                }
                var listOrderResponse = new List<OrderResponse>();

                foreach (var orderId in listOrderdetail.Select(c => c.OrderId).Distinct())
                {
                    var orderRespponse = new OrderResponse();
                    orderRespponse = await _orderDao.GetQueryable().ProjectTo<OrderResponse>(_mapper.ConfigurationProvider).FirstOrDefaultAsync(c => c.OrderId == orderId);
                    orderRespponse.orderDetailResponses = listOrderdetail.Where(c => c.OrderId == orderId).ToList();
                    listOrderResponse.Add(orderRespponse);
                }

                if (request.Status != null)
                {
                    listOrderResponse = listOrderResponse.Where(f => f.Status == request.Status).ToList();
                }
                if (request.OrderCode != null)
                {
                    listOrderResponse = listOrderResponse.Where(f => f.OrderCode.ToUpper().Equals(f.OrderCode.ToUpper())).ToList();
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
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
