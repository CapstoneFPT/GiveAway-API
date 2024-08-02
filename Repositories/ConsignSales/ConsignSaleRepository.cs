using AutoMapper;
using AutoMapper.QueryableExtensions;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.ConsignSaleDetails;
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

        public ConsignSaleRepository(IMapper mapper)
        {
            _mapper = mapper;
        }

        public async Task<ConsignSaleResponse> CreateConsignSale(Guid accountId, CreateConsignSaleRequest request)
        {
            //tao moi 1 consign
            ConsignSale newConsign = new ConsignSale()
            {
                ConsignorName = request.ConsignorName,
                Phone = request.Phone,
                Type = request.Type,
                CreatedDate = DateTime.UtcNow,
                ConsignDuration = 60,
                ShopId = request.ShopId,
                MemberId = accountId,
                Status = ConsignSaleStatus.Pending,
                TotalPrice = request.fashionItemForConsigns.Sum(c => c.DealPrice),
                ConsignSaleMethod = ConsignSaleMethod.Online,
                ConsignSaleCode = await GenerateUniqueString(),
            };
            await GenericDao<ConsignSale>.Instance.AddAsync(newConsign);


            //tao nhung~ mon do trong consign moi'
            foreach (var item in request.fashionItemForConsigns)
            {
                FashionItem fashionItem = new FashionItem()
                {
                    Name = item.Name,
                    Note = item.Note,
                    Condition = item.Condition,
                    ShopId = newConsign.ShopId,
                    Status = FashionItemStatus.PendingForConsignSale,
                    Brand = item.Brand,
                    Color = item.Color,
                    Size = item.Size,
                    Gender = item.Gender,
                    CreatedDate = DateTime.UtcNow
                };
                fashionItem.Type = request.Type switch
                {
                    ConsignSaleType.ConsignedForSale => FashionItemType.ConsignedForSale,
                    ConsignSaleType.ConsignedForAuction => FashionItemType.ConsignedForAuction,
                    _ => FashionItemType.ItemBase
                };

                await GenericDao<FashionItem>.Instance.AddAsync(fashionItem);

                foreach (var image in item.Images)
                {
                    var img = new Image()
                    {
                        FashionItemId = fashionItem.ItemId,
                        Url = image
                    };
                    await GenericDao<Image>.Instance.AddAsync(img);
                }


                //tao moi consigndetail tuong ung voi mon do
                ConsignSaleDetail consignDetail = new ConsignSaleDetail()
                {
                    ConfirmedPrice = 0,
                    DealPrice = item.DealPrice,
                    FashionItemId = fashionItem.ItemId,
                    ConsignSaleId = newConsign.ConsignSaleId,
                    CreatedDate = DateTime.UtcNow
                };
                await GenericDao<ConsignSaleDetail>.Instance.AddAsync(consignDetail);
            }

            var consignResponse = await GenericDao<ConsignSale>.Instance.GetQueryable()
                .ProjectTo<ConsignSaleResponse>(_mapper.ConfigurationProvider)
                .Where(c => c.ConsignSaleId == newConsign.ConsignSaleId)
                .FirstOrDefaultAsync();
            return consignResponse;
        }

        public async Task<PaginationResponse<ConsignSaleResponse>> GetAllConsignSale(Guid accountId,
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
            query = query.Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize);

            var items = await query
                .ProjectTo<ConsignSaleResponse>(_mapper.ConfigurationProvider)
                .OrderByDescending(c => c.CreatedDate)
                .AsNoTracking().ToListAsync();

            var result = new PaginationResponse<ConsignSaleResponse>
            {
                Items = items,
                PageSize = request.PageSize,
                TotalCount = count,
                SearchTerm = request.ConsignSaleCode,
                PageNumber = request.PageNumber,
            };
            return result;
        }

        public async Task<ConsignSaleResponse> GetConsignSaleById(Guid consignId)
        {
            var consignSale = await GenericDao<ConsignSale>.Instance.GetQueryable()
                .Where(c => c.ConsignSaleId == consignId)
                .ProjectTo<ConsignSaleResponse>(_mapper.ConfigurationProvider).FirstOrDefaultAsync();
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

        public async Task<ConsignSaleResponse> ApprovalConsignSale(Guid consignId, ConsignSaleStatus status)
        {
            var consign = await GenericDao<ConsignSale>.Instance.GetQueryable()
                .Include(c => c.ConsignSaleDetails).ThenInclude(c => c.FashionItem)
                .Where(c => c.ConsignSaleId == consignId)
                .FirstOrDefaultAsync();
            if (status.Equals(ConsignSaleStatus.Rejected))
            {
                consign.Status = ConsignSaleStatus.Rejected;
                foreach (var consigndetail in consign.ConsignSaleDetails)
                {
                    var item = await GenericDao<FashionItem>.Instance.GetQueryable()
                        .FirstOrDefaultAsync(c => c.ItemId == consigndetail.FashionItemId);
                    item.Status = FashionItemStatus.Rejected;
                    await GenericDao<FashionItem>.Instance.UpdateAsync(item);
                }
            }
            else
            {
                consign.Status = ConsignSaleStatus.AwaitDelivery;
            }

            await GenericDao<ConsignSale>.Instance.UpdateAsync(consign);
            return _mapper.Map<ConsignSaleResponse>(consign);
        }

        public async Task<List<ConsignSale>> GetAllConsignPendingByAccountId(Guid accountId, bool isTracking = false)
        {
            var query = GenericDao<ConsignSale>.Instance.GetQueryable().Where(c => c.MemberId == accountId);

            if (!isTracking)
                query = query.AsNoTracking();
            return await query.ToListAsync();
        }

        public async Task<ConsignSaleResponse> ConfirmReceivedFromShop(Guid consignId,
            ConsignSaleStatus status)
        {
            var consign = await GenericDao<ConsignSale>.Instance.GetQueryable()
                .Include(c => c.ConsignSaleDetails!)
                .ThenInclude(cd => cd.FashionItem)
                .FirstOrDefaultAsync(c => c.ConsignSaleId == consignId);

            if (consign == null)
            {
                throw new ConsignSaleNotFoundException();
            }
            
            consign.Status = ConsignSaleStatus.Received;
            consign.StartDate = DateTime.UtcNow;
            consign.EndDate = DateTime.UtcNow.AddDays(60);
            
            /*var updateRequestIds = new HashSet<Guid>(request.FashionItemConsignUpdates.Select(u => u.FashionItemId));
            
            foreach (var detail in consign.ConsignSaleDetails)
            {
                if (detail.FashionItem != null && updateRequestIds.Contains(detail.FashionItem.ItemId))
                {
                    var updateRequest = request.FashionItemConsignUpdates.First(u => u.FashionItemId == detail.FashionItem.ItemId);
                    detail.FashionItem.CategoryId = updateRequest.CategoryId;
                    detail.FashionItem.SellingPrice = updateRequest.SellingPrice;
                    detail.FashionItem.Status = FashionItemStatus.Unavailable;
                }
            }*/

            await GenericDao<ConsignSale>.Instance.UpdateAsync(consign);

            return _mapper.Map<ConsignSaleResponse>(consign);

        }

        public async Task<ConsignSaleResponse> CreateConsignSaleByShop(Guid shopId,
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
                ConsignDuration = 60,
                ShopId = shopId,
                Status = ConsignSaleStatus.Received,
                ConsignSaleMethod = ConsignSaleMethod.Offline,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(60),
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
                FashionItem fashionItem = new FashionItem()
                {
                    SellingPrice = item.ConfirmedPrice,
                    Name = item.Name,
                    Note = item.Note,
                    Description = item.Description,
                    Condition = item.Condition,
                    CategoryId = item.CategoryId,
                    ShopId = shopId,
                    Status = FashionItemStatus.Unavailable,
                    Brand = item.Brand,
                    Color = item.Color,
                    Size = item.Size,
                    Gender = item.Gender,
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

                await GenericDao<FashionItem>.Instance.AddAsync(fashionItem);

                //them image tuong ung voi moi mon do
                for (int i = 0; i < item.Images.Length; i++)
                {
                    Image img = new Image()
                    {
                        FashionItemId = fashionItem.ItemId,
                        Url = item.Images[i]
                    };
                    await GenericDao<Image>.Instance.AddAsync(img);
                }


                //tao moi consigndetail tuong ung voi mon do
                ConsignSaleDetail consignDetail = new ConsignSaleDetail()
                {
                    ConfirmedPrice = item.ConfirmedPrice,
                    DealPrice = 0,
                    FashionItemId = fashionItem.ItemId,
                    ConsignSaleId = newConsign.ConsignSaleId
                };
                await GenericDao<ConsignSaleDetail>.Instance.AddAsync(consignDetail);
            }


            var consignResponse = await GenericDao<ConsignSale>.Instance.GetQueryable()
                .ProjectTo<ConsignSaleResponse>(_mapper.ConfigurationProvider)
                .Where(c => c.ConsignSaleId == newConsign.ConsignSaleId)
                .FirstOrDefaultAsync();
            return consignResponse;
        }

        public async Task<PaginationResponse<ConsignSaleResponse>> GetAllConsignSaleByShopId(Guid shopId,
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
            query = query.Where(c => c.ShopId == shopId);

            var count = await query.CountAsync();
            query = query.Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize);

            var items = await query
                /*.Include(c => c.ConsignSaleDetails).ThenInclude(c => c.FashionItem)*/
                .ProjectTo<ConsignSaleResponse>(_mapper.ConfigurationProvider)
                .OrderByDescending(c => c.CreatedDate)
                .AsNoTracking().ToListAsync();


            var result = new PaginationResponse<ConsignSaleResponse>
            {
                Items = items,
                PageSize = request.PageSize,
                TotalCount = count,
                Filters = new[] { request.Status.ToString() },
                SearchTerm = request.ConsignSaleCode,
                PageNumber = request.PageNumber,
            };
            return result;
        }
    }
}