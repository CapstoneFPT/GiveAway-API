﻿using System.Runtime.CompilerServices;
using BusinessObjects;
using BusinessObjects.Dtos.AuctionItems;
using BusinessObjects.Dtos.Auctions;
using BusinessObjects.Dtos.Bids;
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
                var auctionItem = await _auctionFashionItemDao.GetQueryable()
                    .FirstOrDefaultAsync(x => x.ItemId == request.AuctionItemId);

                if (auctionItem == null)
                {
                    throw new Exception("Auction item not found");
                }

                if (auctionItem.Status != FashionItemStatus.Available)
                {
                    throw new Exception("Auction item is not available for auctioning");
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
                    StepIncrement = auctionItem.InitialPrice * (request.StepIncrementPercentage / 100),
                    StartDate = request.ScheduleDate.ToDateTime(timeslot.StartTime),
                    EndDate = request.ScheduleDate.ToDateTime(timeslot.EndTime),
                    Status = AuctionStatus.Pending
                };
                var auctionDetail = await _auctionDao.AddAsync(newAuction);

                auctionItem.Status = FashionItemStatus.AwaitingAuction;
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
                toBeUpdated.Status = request.Status;

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
                        .FirstOrDefaultAsync(x => x.ItemId == request.AuctionItemId.Value);

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

        public async Task<AuctionDetailResponse?> ApproveAuction(Guid id)
        {
            try
            {
                var toBeApproved = await _auctionDao.GetQueryable()
                    .FirstOrDefaultAsync(x => x.AuctionId == id);

                if (toBeApproved == null)
                {
                    throw new Exception("Not Found");
                }

                if (toBeApproved.Status == AuctionStatus.Rejected)
                {
                    throw new Exception("Auction already rejected");
                }

                toBeApproved.Status = AuctionStatus.Approved;
                await _auctionDao.UpdateAsync(toBeApproved);

                return new AuctionDetailResponse()
                {
                    AuctionId = toBeApproved.AuctionId,
                    Status = toBeApproved.Status,
                };
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public async Task<AuctionDetailResponse?> RejectAuction(Guid id)
        {
            try
            {
                var toBeRejected = _auctionDao.GetQueryable()
                    .FirstOrDefault(x => x.AuctionId == id);


                if (toBeRejected == null)
                {
                    throw new Exception("Auction Not Found");
                }

                if (toBeRejected.Status == AuctionStatus.Approved)
                {
                    throw new Exception("Auction already approved");
                }

                toBeRejected.Status = AuctionStatus.Rejected;
                var auctionItem = await _auctionFashionItemDao.GetQueryable()
                    .FirstOrDefaultAsync(x => x.ItemId == toBeRejected!.AuctionFashionItemId);

                if (auctionItem == null)
                {
                    throw new Exception(" Auction Fashion Item Not Found");
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
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public Task<Auction> UpdateAuctionStatus(Guid auctionId, AuctionStatus auctionStatus)
        {
            try
            {
                var toBeUpdated = _auctionDao.GetQueryable()
                    .FirstOrDefault(x => x.AuctionId == auctionId);
                if (toBeUpdated == null)
                {
                    throw new Exception("Auction Not Found");
                }

                toBeUpdated.Status = auctionStatus;
                return _auctionDao.UpdateAsync(toBeUpdated);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public async Task<List<Auction>> GetAuctionEndingNow()
        {
            try
            {
                var now = DateTime.UtcNow;
                var result = await _auctionDao.GetQueryable()
                    .Where(a => a.EndDate <= now && a.Status == AuctionStatus.OnGoing)
                    .ToListAsync();

                return result;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public async Task<List<Auction>> GetAuctionStartingNow()
        {
            try
            {
                var result = await _auctionDao.GetQueryable()
                    .Where(a => a.StartDate <= DateTime.UtcNow && a.Status == AuctionStatus.Approved)
                    .ToListAsync();

                return result;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
    }
}