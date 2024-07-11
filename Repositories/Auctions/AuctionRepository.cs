using BusinessObjects.Dtos.AuctionItems;
using BusinessObjects.Dtos.Auctions;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.Shops;
using BusinessObjects.Entities;
using BusinessObjects.Utils;
using Dao;
using Microsoft.EntityFrameworkCore;

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
            var auctionItem = await _auctionFashionItemDao.GetQueryable()
                .FirstOrDefaultAsync(x => x.ItemId == request.AuctionItemId);

            if (auctionItem == null)
            {
                throw new AuctionItemNotFoundException();
            }

            if (auctionItem.Status != FashionItemStatus.Available)
            {
                throw new AuctionItemNotAvailableForAuctioningException();
            }

            var shop = await _shopDao.GetQueryable()
                .FirstOrDefaultAsync(x => x.ShopId == request.ShopId);

            if (shop == null)
            {
                throw new ShopNotFoundException();
            }

            var timeslot = await _timeslotDao.GetQueryable()
                .FirstOrDefaultAsync(x => x.TimeslotId == request.TimeslotId);

            if (timeslot == null)
            {
                throw new TimeslotNotFoundException();
            }

            var newAuction = new Auction
            {
                AuctionFashionItemId = request.AuctionItemId,
                Title = request.Title,
                ShopId = request.ShopId,
                DepositFee = request.DepositFee,
                StepIncrement = auctionItem.InitialPrice * (request.StepIncrementPercentage / 100),
                StartDate = request.ScheduleDate.ToDateTime(timeslot.StartTime),
                EndDate = request.ScheduleDate.ToDateTime(timeslot.EndTime),
                Status = AuctionStatus.Pending
            };
            var auctionDetail = await _auctionDao.AddAsync(newAuction);

            auctionItem.Status = FashionItemStatus.PendingAuction;
            await _auctionFashionItemDao.UpdateAsync(auctionItem);

            var newSchedule = new Schedule()
            {
                AuctionId = auctionDetail.AuctionId,
                Date = request.ScheduleDate,
                TimeslotId = request.TimeslotId
            };
            await _scheduleDao.AddAsync(newSchedule);

            auctionItem.Status = FashionItemStatus.AwaitingAuction;


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
                    Duration = auctionItem.Duration,
                    InitialPrice = auctionItem.InitialPrice,
                    SellingPrice = auctionItem.SellingPrice,
                    AuctionItemStatus = auctionItem.Status,
                    Note = auctionItem.Note,
                    Value = auctionItem.Value,
                    ShopId = auctionItem.ShopId,
                    CategoryId = auctionItem.CategoryId
                },
                DepositFee = auctionDetail.DepositFee,
                ShopId = auctionDetail.ShopId,
                StepIncrement = auctionDetail.StepIncrement,
                AuctionItemId = auctionDetail.AuctionFashionItemId,
            };
    }

    

    public async Task<PaginationResponse<AuctionListResponse>> GetAuctions(GetAuctionsRequest request)
    {
        var query = _auctionDao.GetQueryable();

        query = query.Include(x => x.Shop);

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
                Shop = new ShopDetailResponse
                {
                    ShopId = x.Shop.ShopId,
                    Address = x.Shop.Address,
                    StaffId = x.Shop.StaffId,
                    Phone = x.Shop.Phone
                },
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

    public Task<AuctionDetailResponse?> GetAuction(Guid id)
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
                StepIncrement = x.StepIncrement,
                AuctionItemId = x.AuctionFashionItemId,
                AuctionFashionItem = new AuctionFashionItemDetailResponse
                {
                    ItemId = x.AuctionFashionItem.ItemId,
                    Name = x.AuctionFashionItem.Name,
                    Type = x.AuctionFashionItem.Type,
                    Condition = x.AuctionFashionItem.Condition,
                    Duration = x.AuctionFashionItem.Duration,
                    InitialPrice = x.AuctionFashionItem.InitialPrice,
                    SellingPrice = x.AuctionFashionItem.SellingPrice,
                    AuctionItemStatus = x.AuctionFashionItem.Status,
                    Note = x.AuctionFashionItem.Note,
                    Value = x.AuctionFashionItem.Value,
                    ShopId = x.AuctionFashionItem.ShopId,
                    CategoryId = x.AuctionFashionItem.CategoryId
                }
            }).AsNoTracking().FirstOrDefaultAsync();

        return result;
    }

    public async Task<AuctionDetailResponse?> DeleteAuction(Guid id)
    {
        var toBeDeleted = await _auctionDao.GetQueryable()
            .FirstOrDefaultAsync(x => x.AuctionId == id);

        if (toBeDeleted is null)
        {
            throw new AuctionNotFoundException();
        }

        await _auctionDao.DeleteAsync(toBeDeleted);
        return new AuctionDetailResponse
        {
            AuctionId = toBeDeleted.AuctionId
        };
    }

    public async Task<AuctionDetailResponse> UpdateAuction(Guid id, UpdateAuctionRequest request)
    {
        var toBeUpdated = await _auctionDao.GetQueryable()
            .Include(x => x.AuctionFashionItem)
            .Include(x => x.Shop)
            .FirstOrDefaultAsync(x => x.AuctionId == id);


        if (toBeUpdated is null)
        {
            throw new AuctionNotFoundException();
        }

        toBeUpdated.Title = request.Title ?? toBeUpdated.Title;
        toBeUpdated.StartDate = request.StartDate ?? toBeUpdated.StartDate;
        toBeUpdated.EndDate = request.EndDate ?? toBeUpdated.EndDate;
        toBeUpdated.DepositFee = request.DepositFee ?? toBeUpdated.DepositFee;
        toBeUpdated.Status = request.Status;

        if (request.ShopId.HasValue)
        {
            var shop = await _shopDao.GetQueryable()
                .FirstOrDefaultAsync(x => x.ShopId == request.ShopId);

            if (shop is null)
            {
                throw new ShopNotFoundException();
            }

            toBeUpdated.ShopId = request.ShopId.Value;
        }

        if (request.AuctionItemId.HasValue)
        {
            var auctionFashionItem = await _auctionFashionItemDao.GetQueryable()
                .FirstOrDefaultAsync(x => x.ItemId == request.AuctionItemId.Value);

            if (auctionFashionItem is null)
            {
                throw new AuctionItemNotFoundException();
            }

            toBeUpdated.AuctionFashionItemId = request.AuctionItemId.Value;
        }

        await _auctionDao.UpdateAsync(toBeUpdated);
        return new AuctionDetailResponse
        {
            AuctionId = toBeUpdated.AuctionId
        };
    }

    public async Task<AuctionDetailResponse?> ApproveAuction(Guid id)
    {
        var toBeApproved = await _auctionDao.GetQueryable()
            .FirstOrDefaultAsync(x => x.AuctionId == id);

        if (toBeApproved == null)
        {
            throw new AuctionNotFoundException();
        }

        if (toBeApproved.Status == AuctionStatus.Rejected)
        {
            throw new AuctionAlreadyRejectedException();
        }

        toBeApproved.Status = AuctionStatus.Approved;
        await _auctionDao.UpdateAsync(toBeApproved);

        return new AuctionDetailResponse()
        {
            AuctionId = toBeApproved.AuctionId,
            Status = toBeApproved.Status,
        };
    }


    public async Task<AuctionDetailResponse?> RejectAuction(Guid id)
    {
        var toBeRejected = await _auctionDao.GetQueryable()
            .FirstOrDefaultAsync(x => x.AuctionId == id);


        if (toBeRejected == null)
        {
            throw new AuctionNotFoundException();
        }

        if (toBeRejected.Status == AuctionStatus.Approved)
        {
            throw new AuctionAlreadyApprovedException();
        }

        toBeRejected.Status = AuctionStatus.Rejected;
        var auctionItem = await _auctionFashionItemDao.GetQueryable()
            .FirstOrDefaultAsync(x => x.ItemId == toBeRejected.AuctionFashionItemId);

        if (auctionItem == null)
        {
            throw new AuctionItemNotFoundException();
        }

        auctionItem.Status = FashionItemStatus.Available;

        var result = await _auctionDao.UpdateAsync(toBeRejected);
        return new AuctionDetailResponse()
        {
            AuctionId = result.AuctionId,
            Status = result.Status,
            AuctionFashionItem = new AuctionFashionItemDetailResponse()
            {
                AuctionItemStatus = result.AuctionFashionItem.Status
            }
        };
    }

    public Task<Auction> UpdateAuctionStatus(Guid auctionId, AuctionStatus auctionStatus)
    {
        var toBeUpdated = _auctionDao.GetQueryable()
            .FirstOrDefault(x => x.AuctionId == auctionId);
        if (toBeUpdated == null)
        {
            throw new AuctionNotFoundException();
        }

        toBeUpdated.Status = auctionStatus;
        return _auctionDao.UpdateAsync(toBeUpdated);
    }

    public async Task<List<Auction>> GetAuctionEndingNow()
    {
        var now = DateTime.UtcNow;
        var result = await _auctionDao.GetQueryable()
            .Where(a => a.EndDate <= now && a.Status == AuctionStatus.OnGoing)
            .ToListAsync();

        return result;
    }

    public async Task<List<Auction>> GetAuctionStartingNow()
    {
        var result = await _auctionDao.GetQueryable()
            .Where(a => a.StartDate <= DateTime.UtcNow && a.Status == AuctionStatus.Approved)
            .ToListAsync();

        return result;
    }
}

}