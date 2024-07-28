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
        public async Task<AuctionDetailResponse> CreateAuction(CreateAuctionRequest request)
        {
            var auctionItem = await GenericDao<AuctionFashionItem>.Instance.GetQueryable()
                .FirstOrDefaultAsync(x => x.ItemId == request.AuctionItemId);

            if (auctionItem == null)
            {
                throw new AuctionItemNotFoundException();
            }

            if (auctionItem.Status != FashionItemStatus.Available)
            {
                throw new AuctionItemNotAvailableForAuctioningException();
            }

            var shop = await GenericDao<Shop>.Instance.GetQueryable()
                .FirstOrDefaultAsync(x => x.ShopId == request.ShopId);

            if (shop == null)
            {
                throw new ShopNotFoundException();
            }

            var timeslot = await GenericDao<Timeslot>.Instance.GetQueryable()
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
            var auctionDetail = await GenericDao<Auction>.Instance.AddAsync(newAuction);

            auctionItem.Status = FashionItemStatus.PendingAuction;
            await GenericDao<AuctionFashionItem>.Instance.UpdateAsync(auctionItem);

            var newSchedule = new Schedule()
            {
                AuctionId = auctionDetail.AuctionId,
                Date = request.ScheduleDate,
                TimeslotId = request.TimeslotId
            };
            await GenericDao<Schedule>.Instance.AddAsync(newSchedule);

            auctionItem.Status = FashionItemStatus.AwaitingAuction;


            return new AuctionDetailResponse
            {
                AuctionId = auctionDetail.AuctionId,
                StartDate = auctionDetail.StartDate,
                EndDate = auctionDetail.EndDate,
                Status = auctionDetail.Status,
                DepositFee = auctionDetail.DepositFee,
                StepIncrement = auctionDetail.StepIncrement,
                Title = auctionDetail.Title,
                AuctionItem = new AuctionItemDetailResponse()
                {
                    ItemId = auctionItem.ItemId,
                    Name = auctionItem.Name,
                    FashionItemType = auctionItem.Type,
                    Condition = auctionItem.Condition,
                    InitialPrice = auctionItem.InitialPrice,
                    SellingPrice = auctionItem.SellingPrice.Value,
                    Note = auctionItem.Note,
                    Images = auctionItem.Images.Select(x => new AuctionItemImage()
                    {
                        ImageId = x.ImageId,
                        ImageUrl = x.Url
                    }).ToList(),
                    Status = auctionItem.Status,
                    Shop = new ShopAuctionDetailResponse()
                    {
                        ShopId = shop.ShopId,
                        Address = shop.Address,
                    },
                    Category =
                    {
                        CategoryId = auctionItem.CategoryId.Value,
                        CategoryName = auctionItem.Category.Name
                    }
                },
            };
        }


        public async Task<PaginationResponse<AuctionListResponse>> GetAuctions(GetAuctionsRequest request)
        {
            var query = GenericDao<Auction>.Instance.GetQueryable();

            query = query.Include(x => x.Shop).Include(x => x.AuctionFashionItem)
                .ThenInclude(x => x.Images);

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
                    AuctionItemId = x.AuctionFashionItemId,
                    DepositFee = x.DepositFee,
                    ImageUrl = x.AuctionFashionItem.Images.FirstOrDefault().Url
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

        public async Task<Auction?> GetAuction(Guid id, bool includeRelations = false)
        {
            var query = GenericDao<Auction>.Instance.GetQueryable();

            if (includeRelations)
            {
                query = query.Include(x => x.AuctionFashionItem)
                    .ThenInclude(x => x.Images)
                    .Include(x => x.AuctionFashionItem).ThenInclude(x => x.Category)
                    .Include(x => x.Shop);
            }

            query = query.Where(x => x.AuctionId == id);
            var result = await query.FirstOrDefaultAsync();


            return result;
        }

        public async Task<AuctionDetailResponse?> DeleteAuction(Guid id)
        {
            var toBeDeleted = await GenericDao<Auction>.Instance.GetQueryable()
                .FirstOrDefaultAsync(x => x.AuctionId == id);

            if (toBeDeleted is null)
            {
                throw new AuctionNotFoundException();
            }

            await GenericDao<Auction>.Instance.DeleteAsync(toBeDeleted);
            return new AuctionDetailResponse
            {
                AuctionId = toBeDeleted.AuctionId
            };
        }

        public async Task<AuctionDetailResponse> UpdateAuction(Guid id, UpdateAuctionRequest request)
        {
            var toBeUpdated = await GenericDao<Auction>.Instance.GetQueryable()
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
                var shop = await GenericDao<Shop>.Instance.GetQueryable()
                    .FirstOrDefaultAsync(x => x.ShopId == request.ShopId);

                if (shop is null)
                {
                    throw new ShopNotFoundException();
                }

                toBeUpdated.ShopId = request.ShopId.Value;
            }

            if (request.AuctionItemId.HasValue)
            {
                var auctionFashionItem = await GenericDao<AuctionFashionItem>.Instance.GetQueryable()
                    .FirstOrDefaultAsync(x => x.ItemId == request.AuctionItemId.Value);

                if (auctionFashionItem is null)
                {
                    throw new AuctionItemNotFoundException();
                }

                toBeUpdated.AuctionFashionItemId = request.AuctionItemId.Value;
            }

            await GenericDao<Auction>.Instance.UpdateAsync(toBeUpdated);
            return new AuctionDetailResponse
            {
                AuctionId = toBeUpdated.AuctionId
            };
        }

        public async Task<AuctionDetailResponse?> ApproveAuction(Guid id)
        {
            var toBeApproved = await GenericDao<Auction>.Instance.GetQueryable()
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
            await GenericDao<Auction>.Instance.UpdateAsync(toBeApproved);

            return new AuctionDetailResponse()
            {
                AuctionId = toBeApproved.AuctionId,
                Status = toBeApproved.Status,
            };
        }


        public async Task<RejectAuctionResponse?> RejectAuction(Guid id)
        {
            var toBeRejected = await GenericDao<Auction>.Instance.GetQueryable()
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
            var auctionItem = await GenericDao<AuctionFashionItem>.Instance.GetQueryable()
                .FirstOrDefaultAsync(x => x.ItemId == toBeRejected.AuctionFashionItemId);

            if (auctionItem == null)
            {
                throw new AuctionItemNotFoundException();
            }

            auctionItem.Status = FashionItemStatus.Available;

            var result = await GenericDao<Auction>.Instance.UpdateAsync(toBeRejected);
            return new RejectAuctionResponse()
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
            var toBeUpdated = GenericDao<Auction>.Instance.GetQueryable()
                .FirstOrDefault(x => x.AuctionId == auctionId);
            if (toBeUpdated == null)
            {
                throw new AuctionNotFoundException();
            }

            toBeUpdated.Status = auctionStatus;
            return GenericDao<Auction>.Instance.UpdateAsync(toBeUpdated);
        }

        public async Task<List<Guid>> GetAuctionEndingNow()
        {
            var now = DateTime.UtcNow;
            var result = await GenericDao<Auction>.Instance.GetQueryable()
                .Where(a => a.EndDate <= now && a.Status == AuctionStatus.OnGoing)
                .Select(x => x.AuctionId)
                .ToListAsync();

            return result;
        }

        public async Task<List<Guid>> GetAuctionStartingNow()
        {
            var result = await GenericDao<Auction>.Instance.GetQueryable()
                .Where(a => a.StartDate <= DateTime.UtcNow && a.Status == AuctionStatus.Approved)
                .Select(x => x.AuctionId)
                .ToListAsync();

            return result;
        }
    }
}