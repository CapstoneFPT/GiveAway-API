using AutoMapper;
using AutoMapper.QueryableExtensions;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.ConsignSales;
using BusinessObjects.Dtos.FashionItems;
using BusinessObjects.Entities;
using Dao;
using Microsoft.EntityFrameworkCore;
using Repositories.ConsignSales;

namespace Repositories.ConsignSales
{
    public class ConsignSaleRepository : IConsignSaleRepository
    {
        private readonly GenericDao<ConsignSale> _consignSaleDao;
        private readonly GenericDao<FashionItem> _fashionItemDao;
        private readonly GenericDao<Image> _imageDao;
        private readonly GenericDao<ConsignSaleDetail> _consignSaleDetailDao;
        private readonly IMapper _mapper;
        private static HashSet<string> generatedStrings = new HashSet<string>();
        private static Random random = new Random();
        private const string prefix = "GA-CS-";

        public ConsignSaleRepository(GenericDao<ConsignSale> consignSaleDao, GenericDao<FashionItem> fashionItemDao, 
            GenericDao<Image> imageDao, GenericDao<ConsignSaleDetail> consignSaleDetailDao, IMapper mapper)
        {
            _consignSaleDao = consignSaleDao;
            _fashionItemDao = fashionItemDao;
            _imageDao = imageDao;
            _consignSaleDetailDao = consignSaleDetailDao;
            _mapper = mapper;
        }

        public async Task<ConsignSaleResponse> CreateConsignSale(Guid accountId, CreateConsignSaleRequest request)
        {
            try
            {
                //tao moi 1 consign
                ConsignSale newConsign = new ConsignSale()
                {
                    Type = request.Type,
                    CreatedDate = DateTime.UtcNow,
                    ConsignDuration = 60,
                    ShopId = request.ShopId,
                    MemberId = accountId,
                    Status = ConsignSaleStatus.Pending,
                    TotalPrice = request.fashionItemForConsigns.Sum(c => c.ConfirmedPrice),
                    SoldPrice = 0,
                    MemberReceivedAmount = 0,
                    ConsignSaleCode = GenerateUniqueString(),
                };
                await _consignSaleDao.AddAsync(newConsign);

                //tao nhung~ mon do trong consign moi'
                foreach(var item in request.fashionItemForConsigns)
                {
                    FashionItem fashionItem = new FashionItem()
                    {
                        SellingPrice = item.ConfirmedPrice,
                        Name = item.Name,
                        Note = item.Note,
                        Value = item.Value,
                        Condition = item.Condition,
                        ShopId = newConsign.ShopId,
                        CategoryId = item.CategoryId,
                        Status = FashionItemStatus.Pending,
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
                    await _fashionItemDao.AddAsync(fashionItem);

                    //them image tuong ung voi moi mon do
                    for(int i = 0; i < item.Image.Count(); i++)
                    {
                        Image img = new Image()
                        {
                            FashionItemId = fashionItem.ItemId,
                            Url = item.Image[i]
                        };
                        await _imageDao.AddAsync(img);
                    }


                    //tao moi consigndetail tuong ung voi mon do
                    ConsignSaleDetail consignDetail = new ConsignSaleDetail()
                    {
                        ConfirmedPrice = item.ConfirmedPrice,
                        DealPrice = item.DealPrice,
                        FashionItemId = fashionItem.ItemId,
                        FashionItem = fashionItem,
                        ConsignSaleId = newConsign.ConsignSaleId
                    };
                    await _consignSaleDetailDao.AddAsync(consignDetail);
                }
                var consignResponse = await _consignSaleDao.GetQueryable()
                    .Where(c => c.ConsignSaleId == newConsign.ConsignSaleId)
                    .ProjectTo<ConsignSaleResponse>(_mapper.ConfigurationProvider)
                    .FirstOrDefaultAsync();
                return consignResponse;
            }catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<PaginationResponse<ConsignSaleResponse>> GetAllConsignSale(Guid accountId, ConsignSaleRequest request)
        {
            try
            {
                var query = _consignSaleDao.GetQueryable();

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
                query = query.Where(c => c.MemberId == accountId || c.ShopId == accountId);

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
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<ConsignSaleResponse> GetConsignSaleById(Guid consignId)
        {
            try
            {
                var consignSale = await _consignSaleDao.GetQueryable().Where(c => c.ConsignSaleId == consignId)
                    .ProjectTo<ConsignSaleResponse>(_mapper.ConfigurationProvider).FirstOrDefaultAsync();
                return consignSale;

            }catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
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

        public async Task<ConsignSaleResponse> ApprovalConsignSale(Guid consignId, ConsignSaleStatus status)
        {
            try
            {
                var consign = await _consignSaleDao.GetQueryable()
                .Include(c => c.ConsignSaleDetails).ThenInclude(c => c.FashionItem)
                .Where(c => c.ConsignSaleId == consignId)
                .FirstOrDefaultAsync();
                if (status.Equals(ConsignSaleStatus.Rejected))
                {
                    consign.Status = ConsignSaleStatus.Rejected;
                    foreach (var consigndetail in consign.ConsignSaleDetails)
                    {
                        var item = await _fashionItemDao.GetQueryable().FirstOrDefaultAsync(c => c.ItemId == consigndetail.FashionItemId);
                        item.Status = FashionItemStatus.Rejected;
                        await _fashionItemDao.UpdateAsync(item);
                    }
                }
                else
                {
                    consign.Status = ConsignSaleStatus.AwaitDelivery;
                }
                await _consignSaleDao.UpdateAsync(consign);
                return _mapper.Map<ConsignSaleResponse>(consign);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public async Task<List<ConsignSale>> GetAllConsignPendingByAccountId(Guid accountId)
        {
            return await _consignSaleDao.GetQueryable().Where(c => c.MemberId == accountId).ToListAsync();
        }

        public async Task<ConsignSaleResponse> ConfirmReceivedFromShop(Guid consignId)
        {
            try
            {
                var consign = await _consignSaleDao.GetQueryable()
                .Include(c => c.ConsignSaleDetails).ThenInclude(c => c.FashionItem)
                .Where(c => c.ConsignSaleId == consignId)
                .FirstOrDefaultAsync();

                consign.Status = ConsignSaleStatus.Received;
                consign.StartDate = DateTime.UtcNow;
                consign.EndDate = DateTime.UtcNow.AddDays(60);
                await _consignSaleDao.UpdateAsync(consign);
                foreach (var consigndetail in consign.ConsignSaleDetails)
                {
                    var item = await _fashionItemDao.GetQueryable().FirstOrDefaultAsync(c => c.ItemId == consigndetail.FashionItemId);
                    item.Status = FashionItemStatus.Unavailable;
                    await _fashionItemDao.UpdateAsync(item);
                }
                return _mapper.Map<ConsignSaleResponse>(consign);
            }
            catch (Exception ex)
            {
                throw new Exception($"{ex.Message}");
            }
        }
    }
}
