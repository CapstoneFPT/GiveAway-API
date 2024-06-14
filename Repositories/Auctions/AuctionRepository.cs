using BusinessObjects;
using BusinessObjects.Dtos.AuctionItems;
using BusinessObjects.Dtos.Auctions;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.Shops;
using BusinessObjects.Entities;
using Dao;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

namespace Repositories.Auctions
{
    public class AuctionRepository : IAuctionRepository
    {
        private readonly GenericDao<Auction> _auctionDao;
        private readonly GenericDao<Schedule> _scheduleDao;
        private readonly GenericDao<AuctionFashionItem> _auctionFashionItemDao;
        private readonly GenericDao<Shop> _shopDao;
        private readonly GenericDao<Timeslot> _timeslotDao;


        public AuctionRepository(GenericDao<Auction> auctionDao, GenericDao<Schedule> scheduleDao,
            GenericDao<AuctionFashionItem> auctionFashionItemDao, GenericDao<Shop> shopDao,
            GenericDao<Timeslot> timeslotDao)
        {
            _auctionDao = auctionDao;
            _scheduleDao = scheduleDao;
            _auctionFashionItemDao = auctionFashionItemDao;
            _shopDao = shopDao;
            _timeslotDao = timeslotDao;
        }

        public async Task<AuctionDetailResponse> CreateAuction(CreateAuctionRequest request)
        {
            try
            {
                //insert auction
                //insert auction item
                //insert schedule
                var auctionItem = await _auctionFashionItemDao.GetQueryable()
                    .FirstOrDefaultAsync(x => x.ItemId == request.AuctionItemId);

                if (auctionItem == null)
                {
                    throw new Exception("Auction item not found");
                }

                var shop = await _shopDao.GetQueryable()
                    .FirstOrDefaultAsync(x => x.ShopId == request.ShopId);

                if (shop == null)
                {
                    throw new Exception("Shop not found");
                }

                var timeslot = await _timeslotDao.GetQueryable()
                    .FirstOrDefaultAsync(x => x.TimeslotId == request.TimeslotId);

                if (timeslot == null)
                {
                    throw new Exception("Timeslot not found");
                }

                var newAuction = new Auction
                {
                    AuctionFashionItemId = request.AuctionItemId,
                    Title = request.Title,
                    ShopId = request.ShopId,
                    DepositFee = request.DepositFee,
                    StartDate = request.ScheduleDate.ToDateTime(timeslot.StartTime),
                    EndDate = request.ScheduleDate.ToDateTime(timeslot.EndTime),
                    Status = "Pending"
                };
                var auctionDetail = await _auctionDao.AddAsync(newAuction);

                var newSchedule = new Schedule()
                {
                    AuctionId = auctionDetail.AuctionId,
                    Date = request.ScheduleDate,
                    TimeslotId = request.TimeslotId
                };
                var scheduleDetail = await _scheduleDao.AddAsync(newSchedule);


                return new AuctionDetailResponse
                {
                    AuctionId = auctionDetail.AuctionId,
                    StartDate = auctionDetail.StartDate,
                    EndDate = auctionDetail.EndDate,
                    Shop = new ShopDetailResponse()
                    {
                        ShopId = shop.ShopId,
                        Address = shop.Address,
                        StaffId = shop.StaffId,
                    },
                    Status = auctionDetail.Status,
                    Title = auctionDetail.Title,
                    AuctionFashionItem = new AuctionFashionItemDetailResponse()
                    {
                        ItemId = auctionItem.ItemId,
                        Name = auctionItem.Name,
                        Type = auctionItem.Type,
                        Condition = auctionItem.Condition,
                        Quantity = auctionItem.Quantity,
                        Duration = auctionItem.Duration,
                        InitialPrice = auctionItem.InitialPrice,
                        SellingPrice = auctionItem.SellingPrice,
                        AuctionItemStatus = auctionItem.AuctionItemStatus,
                        Note = auctionItem.Note,
                        Value = auctionItem.Value,
                        ShopId = auctionItem.ShopId,
                        CategoryId = auctionItem.CategoryId
                    },
                    DepositFee = auctionDetail.DepositFee,
                    ShopId = auctionDetail.ShopId,
                    AuctionItemId = auctionDetail.AuctionFashionItemId,
                };
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public async Task<PaginationResponse<AuctionListResponse>> GetAuctions(GetAuctionsRequest request)
        {
            try
            {
                var query = _auctionDao.GetQueryable();


                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                    query = query.Where(x => EF.Functions.ILike(x.Title, $"%{request.SearchTerm}%"));

                var count = await query.CountAsync();
                query = query.OrderByDescending(
                    x => x.StartDate);

                query = query.Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize);


                var items = await query
                    .Select(x => new AuctionListResponse
                    {
                        AuctionId = x.AuctionId,
                        Title = x.Title,
                        StartDate = x.StartDate,
                        EndDate = x.EndDate,
                        Status = x.Status,
                        ShopId = x.ShopId,
                        AuctionItemId = x.AuctionFashionItemId
                    }).AsNoTracking().ToListAsync();

                var result = new PaginationResponse<AuctionListResponse>
                {
                    Items = items,
                    PageSize = request.PageSize,
                    TotalCount = count,
                    SearchTerm = request.SearchTerm,
                    PageNumber = request.PageNumber,
                };
                return result;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public Task<AuctionDetailResponse?> GetAuction(Guid id)
        {
            try
            {
                var result = _auctionDao
                    .GetQueryable()
                    .Include(x => x.Shop)
                    .Include(x => x.AuctionFashionItem)
                    .Where(x => x.AuctionId == id)
                    .Select(x => new AuctionDetailResponse
                    {
                        AuctionId = x.AuctionId,
                        StartDate = x.StartDate,
                        EndDate = x.EndDate,
                        Shop = new ShopDetailResponse
                        {
                            ShopId = x.Shop.ShopId,
                            Address = x.Shop.Address,
                            StaffId = x.Shop.StaffId
                        },
                        Status = x.Status,
                        Title = x.Title,
                        AuctionFashionItem = new AuctionFashionItemDetailResponse
                        {
                            ItemId = x.AuctionFashionItem.ItemId,
                            Name = x.AuctionFashionItem.Name,
                            Type = x.AuctionFashionItem.Type,
                            Condition = x.AuctionFashionItem.Condition,
                            Quantity = x.AuctionFashionItem.Quantity,
                            Duration = x.AuctionFashionItem.Duration,
                            InitialPrice = x.AuctionFashionItem.InitialPrice,
                            SellingPrice = x.AuctionFashionItem.SellingPrice,
                            AuctionItemStatus = x.AuctionFashionItem.AuctionItemStatus,
                            Note = x.AuctionFashionItem.Note,
                            Value = x.AuctionFashionItem.Value,
                            ShopId = x.AuctionFashionItem.ShopId,
                            CategoryId = x.AuctionFashionItem.CategoryId
                        }
                    }).AsNoTracking().FirstOrDefaultAsync();

                return result;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public async Task<AuctionDetailResponse?> DeleteAuction(Guid id)
        {
            try
            {
                var toBeDeleted = await _auctionDao.GetQueryable()
                    .FirstOrDefaultAsync(x => x.AuctionId == id);

                if (toBeDeleted is null)
                {
                    throw new Exception("Auction not found");
                }

                await _auctionDao.DeleteAsync(toBeDeleted);
                return new AuctionDetailResponse
                {
                    AuctionId = toBeDeleted.AuctionId
                };
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public async Task<AuctionDetailResponse> UpdateAuction(Guid id, UpdateAuctionRequest request)
        {
            try
            {
                var toBeUpdated = await _auctionDao.GetQueryable()
                    .Include(x => x.AuctionFashionItem)
                    .Include(x => x.Shop)
                    .FirstOrDefaultAsync(x => x.AuctionId == id);


                if (toBeUpdated is null)
                {
                    throw new Exception("Auction Not Found");
                }

                toBeUpdated.Title = request.Title ?? toBeUpdated.Title;
                toBeUpdated.StartDate = request.StartDate ?? toBeUpdated.StartDate;
                toBeUpdated.EndDate = request.EndDate ?? toBeUpdated.EndDate;
                toBeUpdated.DepositFee = request.DepositFee ?? toBeUpdated.DepositFee;
                toBeUpdated.Status = request.Status ?? toBeUpdated.Status;

                if (request.ShopId.HasValue)
                {
                    var shop = await _shopDao.GetQueryable()
                        .FirstOrDefaultAsync(x => x.ShopId == request.ShopId);
                    
                    if (shop is null)
                    {
                        throw new Exception("Shop Not Found");
                    }
                    
                    toBeUpdated.ShopId = request.ShopId.Value;
                }
                
                if (request.AuctionItemId.HasValue)
                {
                    var auctionFashionItem = await _auctionFashionItemDao.GetQueryable()
                        .FirstOrDefaultAsync(x =>x.ItemId  == request.AuctionItemId.Value);
                    
                    if (auctionFashionItem is null)
                    {
                        throw new Exception("Auction Fashion Item Not Found");
                    }
                    
                    toBeUpdated.AuctionFashionItemId = request.AuctionItemId.Value;
                }
                
                await _auctionDao.UpdateAsync(toBeUpdated);
                return new AuctionDetailResponse
                {
                    AuctionId = toBeUpdated.AuctionId
                };
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
    }
}