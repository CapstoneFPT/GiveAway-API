﻿using System.Linq.Expressions;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.ConsignSaleLineItems;
using BusinessObjects.Dtos.ConsignSales;
using BusinessObjects.Dtos.FashionItems;
using BusinessObjects.Entities;
using BusinessObjects.Utils;
using Dao;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Repositories.ConsignSales;

namespace Repositories.ConsignSales
{
    public class ConsignSaleRepository : IConsignSaleRepository
    {
        private readonly IMapper _mapper;
        private static HashSet<string> generatedStrings = new HashSet<string>();
        private static Random random = new Random();
        private const string prefix = "GA-CS-";
        private readonly GiveAwayDbContext _giveAwayDbContext;

        public ConsignSaleRepository(IMapper mapper, GiveAwayDbContext dbContext)
        {
            _mapper = mapper;
            _giveAwayDbContext = dbContext;
        }

        public async Task<ConsignSaleDetailedResponse> CreateConsignSale(Guid accountId, CreateConsignSaleRequest request)
        {
            var account = await _giveAwayDbContext.Members.AsQueryable()
                .Include(c => c.Addresses)
                .FirstOrDefaultAsync(c => c.AccountId == accountId);
            //tao moi 1 consign
            ConsignSale newConsign = new ConsignSale()
            {
                ConsignorName = request.ConsignorName,
                Phone = request.Phone,
                Email = account!.Email,
                Address = request.Address,
                Type = request.Type,
                CreatedDate = DateTime.UtcNow,
                
                ShopId = request.ShopId,
                MemberId = accountId,
                Status = ConsignSaleStatus.Pending,
                TotalPrice = request.ConsignDetailRequests.Sum(c => c.ExpectedPrice),
                ConsignSaleMethod = ConsignSaleMethod.Online,
                ConsignSaleCode = await GenerateUniqueString(),
            };
            await GenericDao<ConsignSale>.Instance.AddAsync(newConsign);


            //tao nhung consign detail do trong consign moi'
            foreach (var item in request.ConsignDetailRequests)
            {
                //tao moi consigndetail tuong ung voi mon do
                ConsignSaleLineItem consignLineItem = new ConsignSaleLineItem()
                {
                    ConfirmedPrice = 0,
                    ExpectedPrice = item.ExpectedPrice,
                    ConsignSaleId = newConsign.ConsignSaleId,
                    Brand = item.Brand,
                    Color = item.Color,
                    Size = item.Size,
                    ProductName = item.ProductName,
                    Condition = item.Condition,
                    Images = item.ImageUrls.Select(x => new Image()
                    {
                        CreatedDate = DateTime.UtcNow,
                        Url = x,
                    }).ToList(),
                    CreatedDate = DateTime.UtcNow,
                    Note = item.Note,
                    Status = ConsignSaleLineItemStatus.Pending
                };
                await GenericDao<ConsignSaleLineItem>.Instance.AddAsync(consignLineItem);
            }

            var consignResponse = await GenericDao<ConsignSale>.Instance.GetQueryable()
                .ProjectTo<ConsignSaleDetailedResponse>(_mapper.ConfigurationProvider)
                .Where(c => c.ConsignSaleId == newConsign.ConsignSaleId)
                .FirstOrDefaultAsync();
            return consignResponse;
        }

        public async Task<PaginationResponse<ConsignSaleDetailedResponse>> GetAllConsignSale(Guid accountId,
            ConsignSaleRequest request)
        {
            var query = GenericDao<ConsignSale>.Instance.GetQueryable();

            if (!string.IsNullOrWhiteSpace(request.ConsignSaleCode))
                query = query.Where(x => EF.Functions.ILike(x.ConsignSaleCode, $"%{request.ConsignSaleCode}%"));
            if (request.Status != null)
            {
                query = query.Where(f => f.Status == request.Status);
            }

            if (request.ShopId != null)
            {
                query = query.Where(f => f.ShopId.Equals(request.ShopId));
            }

            query = query.Where(c => c.MemberId == accountId);

            var count = await query.CountAsync();
            query = query
                .OrderByDescending(c => c.CreatedDate)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize);

            var items = await query
                .ProjectTo<ConsignSaleDetailedResponse>(_mapper.ConfigurationProvider)
                .AsNoTracking().ToListAsync();

            var result = new PaginationResponse<ConsignSaleDetailedResponse>
            {
                Items = items,
                PageSize = request.PageSize,
                TotalCount = count,
                SearchTerm = request.ConsignSaleCode,
                PageNumber = request.PageNumber,
            };
            return result;
        }

        public async Task<ConsignSaleDetailedResponse> GetConsignSaleById(Guid consignId)
        {
            var consignSale = await GenericDao<ConsignSale>.Instance.GetQueryable()
                .Where(c => c.ConsignSaleId == consignId)
                .ProjectTo<ConsignSaleDetailedResponse>(_mapper.ConfigurationProvider).FirstOrDefaultAsync();
            return consignSale;
        }

        public async Task<string> GenerateUniqueString()
        {
            string newString;
            do
            {
                newString = GenerateRandomString();
                var isCodeExisted = await GenericDao<ConsignSale>.Instance.GetQueryable()
                    .FirstOrDefaultAsync(c => c.ConsignSaleCode.Equals(newString));
            } while (generatedStrings.Contains(newString));

            generatedStrings.Add(newString);
            return newString;
        }

        private static string GenerateRandomString()
        {
            int number = random.Next(100000, 1000000);
            return prefix + number.ToString("D6");
        }

        public async Task<ConsignSaleDetailedResponse> ApprovalConsignSale(Guid consignId, ApproveConsignSaleRequest request)
        {
            var consign = await GenericDao<ConsignSale>.Instance.GetQueryable()
                .Include(c => c.ConsignSaleLineItems)
                .Where(c => c.ConsignSaleId == consignId)
                .FirstOrDefaultAsync();
            if (request.Status.Equals(ConsignSaleStatus.Rejected))
            {
                if (request.ResponseFromShop is null)
                {
                    throw new MissingFeatureException("You have to give reason for rejection");
                }
                consign!.Status = ConsignSaleStatus.Rejected;
                consign.ResponseFromShop = request.ResponseFromShop;
            }
            else
            {
                consign!.Status = ConsignSaleStatus.AwaitDelivery;
                foreach (var consignSaleLineItem in consign.ConsignSaleLineItems)
                {
                    consignSaleLineItem.Status = ConsignSaleLineItemStatus.AwaitDelivery;
                }
            }

            await GenericDao<ConsignSale>.Instance.UpdateAsync(consign);
            return _mapper.Map<ConsignSaleDetailedResponse>(consign);
        }

        public async Task<List<ConsignSale>> GetAllConsignPendingByAccountId(Guid accountId, bool isTracking = false)
        {
            var query = GenericDao<ConsignSale>.Instance.GetQueryable().Where(c => c.MemberId == accountId);

            if (!isTracking)
                query = query.AsNoTracking();
            return await query.ToListAsync();
        }

        public async Task<ConsignSaleDetailedResponse> ConfirmReceivedFromShop(Guid consignId)
        {
            var consign = await GenericDao<ConsignSale>.Instance.GetQueryable()
                .Include(c => c.ConsignSaleLineItems!)
                .FirstOrDefaultAsync(c => c.ConsignSaleId == consignId);

            if (consign == null)
            {
                throw new ConsignSaleNotFoundException();
            }

            // var consignSaleDetailIds = consign.ConsignSaleLineItems.Select(d => d.ConsignSaleLineItemId).ToList();

            /*var listItemInConsign = await GenericDao<IndividualFashionItem>.Instance.GetQueryable()
                .Where(c => consignSaleDetailIds.Contains(c.ConsignSaleLineItemId!.Value)).ToListAsync();*/
            foreach (var consignSaleLineItem in consign.ConsignSaleLineItems)
            {
                consignSaleLineItem.Status = ConsignSaleLineItemStatus.Received;
            }

            consign.Status = ConsignSaleStatus.Processing;
            /*consign.StartDate = DateTime.UtcNow;
            consign.EndDate = DateTime.UtcNow.AddDays(1);*/

            
            /*if (consign.Type.Equals(ConsignSaleType.ForSale))
            {
                var shop = await _giveAwayDbContext.Shops.AsQueryable()
                    .Where(c => c.ShopId == consign.ShopId)
                    .Select(c => c.Staff)
                    .FirstOrDefaultAsync();
                shop.Balance -= totalprice;
                var member = await _giveAwayDbContext.Accounts.AsQueryable()
                    .Where(c => c.AccountId == consign.MemberId)
                    .FirstOrDefaultAsync();
                member.Balance += totalprice;
                await GenericDao<Account>.Instance.UpdateAsync(member);
            }*/

            await GenericDao<ConsignSale>.Instance.UpdateAsync(consign);

            return _mapper.Map<ConsignSaleDetailedResponse>(consign);
        }

        public async Task<ConsignSaleDetailedResponse> CreateConsignSaleByShop(Guid shopId,
            CreateConsignSaleByShopRequest request)
        {
            //tao moi 1 consign
            var memberId = await GenericDao<Account>.Instance.GetQueryable()
                .Where(c => c.Phone.Equals(request.Phone)).Select(x => x.AccountId)
                .FirstOrDefaultAsync();
            ConsignSale newConsign = new ConsignSale()
            {
                Type = request.Type,
                CreatedDate = DateTime.UtcNow,
                
                ShopId = shopId,
                Status = ConsignSaleStatus.Processing,
                ConsignSaleMethod = ConsignSaleMethod.Offline,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMinutes(5),
                TotalPrice = request.fashionItemForConsigns.Sum(c => c.ConfirmedPrice),
                SoldPrice = 0,
                ConsignorReceivedAmount = 0,
                ConsignSaleCode = await GenerateUniqueString(),
            };
            if (memberId != Guid.Empty)
            {
                newConsign.MemberId = memberId;
            }
            else
            {
                newConsign.ConsignorName = request.Consigner;
                newConsign.Address = request.Address;
                newConsign.Phone = request.Phone;
                newConsign.Email = request.Email;
            }

            await GenericDao<ConsignSale>.Instance.AddAsync(newConsign);

            foreach (var item in request.fashionItemForConsigns)
            {
                var fashionItem = new IndividualFashionItem()
                {
                    SellingPrice = item.ConfirmedPrice,
                    Note = item.Note,
                    // ShopId = shopId,
                    Status = FashionItemStatus.Unavailable,
                };
                switch (request.Type)
                {
                    case ConsignSaleType.ConsignedForSale:
                        fashionItem.Type = FashionItemType.ConsignedForSale;
                        break;
                    case ConsignSaleType.ConsignedForAuction:
                        fashionItem.Type = FashionItemType.ConsignedForAuction;
                        break;
                    default:
                        fashionItem.Type = FashionItemType.ItemBase;
                        break;
                }

                await GenericDao<IndividualFashionItem>.Instance.AddAsync(fashionItem);

                //them image tuong ung voi moi mon do
                for (int i = 0; i < item.Images.Length; i++)
                {
                    Image img = new Image()
                    {
                        IndividualFashionItemId = fashionItem.ItemId,
                        Url = item.Images[i]
                    };
                    await GenericDao<Image>.Instance.AddAsync(img);
                }


                //tao moi consigndetail tuong ung voi mon do
                ConsignSaleLineItem consignLineItem = new ConsignSaleLineItem()
                {
                    ConfirmedPrice = item.ConfirmedPrice,
                    DealPrice = 0,
                    ConsignSaleId = newConsign.ConsignSaleId
                };
                await GenericDao<ConsignSaleLineItem>.Instance.AddAsync(consignLineItem);
            }


            var consignResponse = await GenericDao<ConsignSale>.Instance.GetQueryable()
                .ProjectTo<ConsignSaleDetailedResponse>(_mapper.ConfigurationProvider)
                .Where(c => c.ConsignSaleId == newConsign.ConsignSaleId)
                .FirstOrDefaultAsync();
            return consignResponse;
        }

        public async Task<PaginationResponse<ConsignSaleDetailedResponse>> GetAllConsignSaleByShopId(
            ConsignSaleRequestForShop request)
        {
            var query = GenericDao<ConsignSale>.Instance.GetQueryable();

            if (!string.IsNullOrWhiteSpace(request.ConsignSaleCode))
                query = query.Where(x => EF.Functions.ILike(x.ConsignSaleCode, $"%{request.ConsignSaleCode}%"));
            if (request.Status != null)
            {
                query = query.Where(f => f.Status == request.Status);
            }

            if (request.StartDate != null)
            {
                query = query.Where(f => f.StartDate >= request.StartDate);
            }

            if (request.EndDate != null)
            {
                query = query.Where(f => f.EndDate <= request.EndDate);
            }

            if (request.ShopId.HasValue)
            {
                query = query.Where(c => c.ShopId == request.ShopId);
            }

            var count = await query.CountAsync();
            query = query
                .OrderByDescending(c => c.CreatedDate)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize);

            var items = await query
                /*.Include(c => c.ConsignSaleDetails).ThenInclude(c => c.FashionItem)*/
                .ProjectTo<ConsignSaleDetailedResponse>(_mapper.ConfigurationProvider)
                .AsNoTracking().ToListAsync();


            var result = new PaginationResponse<ConsignSaleDetailedResponse>
            {
                Items = items ?? [],
                PageSize = request.PageSize,
                TotalCount = count,
                Filters = new[] { request.Status.ToString() },
                SearchTerm = request.ConsignSaleCode,
                PageNumber = request.PageNumber,
            };
            return result;
        }

        public async Task<ConsignSale?> GetSingleConsignSale(Expression<Func<ConsignSale, bool>> predicate)
        {
            var result = await GenericDao<ConsignSale>.Instance
                .GetQueryable()
                .Include(cons => cons.ConsignSaleLineItems)
                .ThenInclude(c => c.IndividualFashionItem)
                .Include(c => c.Shop)
                .SingleOrDefaultAsync(predicate);
            return result;
        }

        public async Task UpdateConsignSale(ConsignSale consignSale)
        {
            await GenericDao<ConsignSale>.Instance.UpdateAsync(consignSale);
        }

        public async Task<(List<T> Items, int Page, int PageSize, int TotalCount)> GetConsignSalesProjections<T>(Expression<Func<ConsignSale, bool>>? predicate, Expression<Func<ConsignSale, T>>? selector, int? requestPage, int? requestPageSize)
        {
            var query = _giveAwayDbContext.ConsignSales.AsQueryable();

            if (predicate != null)
            {
                query = query.Where(predicate);
            }
            
            var count = await query.CountAsync();

            var page = requestPage ?? -1;
            var pageSize = requestPageSize ?? -1;

            if (page > 0 && pageSize > 0)
            {
                query = query
                    .OrderByDescending(c => c.CreatedDate)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize);
            }

            query = query.OrderByDescending(c => c.CreatedDate);

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

        /*public async Task<bool> UpdateConsignSaleToOnSale(Guid fashionItemId)
        {
            var consignSaleDetail = await _giveAwayDbContext.ConsignSaleDetails.AsQueryable()
                .Include(c => c.ConsignSale)
                .Where(c => c.FashionItemId == fashionItemId)
                .FirstOrDefaultAsync();

            if (consignSaleDetail == null)
            {
                return false;
            }

            var consignSale = await _giveAwayDbContext.ConsignSales.AsQueryable()
                .Include(c => c.ConsignSaleDetails)
                .ThenInclude(c => c.FashionItem)
                .Where(c => c.ConsignSaleDetails.Contains(consignSaleDetail))
                .FirstOrDefaultAsync();
            if (!consignSale.ConsignSaleDetails.Any(c => c.FashionItem.Status.Equals(FashionItemStatus.Unavailable)))
            {
                consignSale.Status = ConsignSaleStatus.OnSale;
                await GenericDao<ConsignSale>.Instance.UpdateAsync(consignSale);
            }

            return true;
        }*/
    }
}