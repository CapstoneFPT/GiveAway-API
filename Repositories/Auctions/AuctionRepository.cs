﻿using System.Linq.Expressions;
using BusinessObjects.Dtos.Auctions;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Entities;
using BusinessObjects.Utils;
using Dao;
using LinqKit;
using Microsoft.EntityFrameworkCore;

namespace Repositories.Auctions
{
    public class AuctionRepository : IAuctionRepository
    {
        private static DateTime GetUtcDateTimeFromLocalDateTime(DateTime scheduledDate
            ,TimeZoneInfo timeZone)
        {
            var localDateTime = TimeZoneInfo.ConvertTime(scheduledDate, TimeZoneInfo.Local, timeZone);
            var utcTime = TimeZoneInfo.ConvertTimeToUtc(localDateTime, timeZone);
            return utcTime;
        }

        private static async Task<bool> IsDateTimeOverlapped(DateTime startDate, DateTime endDate,
            Guid? excludingAuction = null)
        {
            var query = GenericDao<Auction>.Instance.GetQueryable();
            Expression<Func<Auction, bool>> predicate = a =>
                (a.StartDate <= endDate && a.EndDate >= startDate) &&
                (a.Status == AuctionStatus.Pending || a.Status == AuctionStatus.Approved ||
                 a.Status == AuctionStatus.OnGoing);

            if (excludingAuction.HasValue)
            {
                predicate = predicate.And(a => a.AuctionId != excludingAuction.Value);
            }

            var potentiallyConflictedAuctions = await query.Where(predicate).ToListAsync();

            return potentiallyConflictedAuctions.Exists(existingAuction =>
                IsOverlapping(startDate, endDate, existingAuction.StartDate, existingAuction.EndDate));
        }

        private static bool IsOverlapping(DateTime startDate, DateTime endDate, DateTime existingAuctionStartDate,
            DateTime existingAuctionEndDate)
        {
            return startDate < existingAuctionEndDate && existingAuctionStartDate < endDate;
        }

        public async Task<AuctionDetailResponse> CreateAuction(CreateAuctionRequest request)
        {
            var auctionItem = await GenericDao<AuctionFashionItem>.Instance.GetQueryable().Include(x => x.Category)
                .Include(x => x.Images)
                .Include(x => x.Shop)
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

            if (await IsDateTimeOverlapped(request.StartTime, request.EndTime))
            {
                throw new ScheduledTimeOverlappedException();
            }

            var timezone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");
            var newAuction = new Auction
            {
                AuctionFashionItemId = request.AuctionItemId,
                Title = request.Title,
                ShopId = request.ShopId,
                DepositFee = request.DepositFee,
                CreatedDate = DateTime.UtcNow,
                StepIncrement = auctionItem.InitialPrice * (request.StepIncrementPercentage / 100),
                StartDate = request.StartTime,
                EndDate = request.EndTime,
                Status = AuctionStatus.Pending
            };
            var auctionDetail = await GenericDao<Auction>.Instance.AddAsync(newAuction);

            auctionItem.Status = FashionItemStatus.PendingAuction;
            await GenericDao<AuctionFashionItem>.Instance.UpdateAsync(auctionItem);

            auctionItem.Status = FashionItemStatus.AwaitingAuction;

            await GenericDao<AuctionFashionItem>.Instance.UpdateAsync(auctionItem);

            return new AuctionDetailResponse
            {
                AuctionId = auctionDetail.AuctionId,
                StartDate = auctionDetail.StartDate,
                EndDate = auctionDetail.EndDate,
                Status = auctionDetail.Status,
                DepositFee = auctionDetail.DepositFee,
                StepIncrement = auctionDetail.StepIncrement,
                Title = auctionDetail.Title,
                // AuctionItem = new AuctionItemDetailResponse()
                // {
                //     ItemId = auctionItem.ItemId,
                //     Name = auctionItem.Name,
                //     FashionItemType = auctionItem.Type,
                //     Condition = auctionItem.Condition,
                //     InitialPrice = auctionItem.InitialPrice,
                //     Note = auctionItem.Note,
                //     Images = auctionItem.Images.Count > 0 ? auctionItem.Images.Select(x => new AuctionItemImage()
                //     {
                //         ImageId = x.ImageId,
                //         ImageUrl = x.Url
                //     }).ToList() : [],
                //     Status = auctionItem.Status,
                //     Shop = new ShopAuctionDetailResponse()
                //     {
                //         ShopId = shop.ShopId,
                //         Address = shop.Address,
                //     },
                //     Category =
                //     {
                //         CategoryId = auctionItem.CategoryId.Value,
                //         CategoryName = auctionItem.Category.Name
                //     }
                // },
            };
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

            if (toBeApproved.Status != AuctionStatus.Pending)
            {
                throw new InvalidOperationException("Auction must be on pending");
            }

            toBeApproved.Status = AuctionStatus.Approved;
            await GenericDao<Auction>.Instance.UpdateAsync(toBeApproved);
            return new AuctionDetailResponse()
            {
                AuctionId = toBeApproved.AuctionId,
                Status = toBeApproved.Status,
                StartDate = toBeApproved.StartDate,
                EndDate = toBeApproved.EndDate
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

            if (toBeRejected.Status != AuctionStatus.Pending)
            {
                throw new InvalidOperationException("Auction must be on pending");
            }

            toBeRejected.Status = AuctionStatus.Rejected;
            var auctionItem = await GenericDao<AuctionFashionItem>.Instance.GetQueryable()
                .FirstOrDefaultAsync(x => x.ItemId == toBeRejected.AuctionFashionItemId);

            if (auctionItem == null)
            {
                throw new AuctionItemNotFoundException();
            }

            auctionItem.Status = FashionItemStatus.Available;

            await GenericDao<AuctionFashionItem>.Instance.UpdateAsync(auctionItem);
            var result = await GenericDao<Auction>.Instance.UpdateAsync(toBeRejected);
            return new RejectAuctionResponse()
            {
                AuctionId = result.AuctionId,
                Status = result.Status,
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

        public async Task<(List<T> Items, int Page, int PageSize, int Total)> GetAuctionProjections<T>(
            int? requestPageNumber, int? requestPageSize, Expression<Func<Auction, bool>> predicate,
            Expression<Func<Auction, T>> selector)
        {
            var query = GenericDao<Auction>.Instance.GetQueryable();

            if (predicate != null)
                query = query.Where(predicate);

            var totalCount = await query.CountAsync();

            var page = requestPageNumber ?? -1;
            var pageSize = requestPageSize ?? -1;

            if (page >= 0 && pageSize >= 0)
            {
                query = query.Skip((page - 1) * pageSize).Take(pageSize);
            }

            List<T> result;
            if (selector != null)
            {
                result = await query.Select(selector).ToListAsync();
            }
            else
            {
                result = await query.Cast<T>().ToListAsync();
            }

            return (result, page, pageSize, totalCount);
        }
    }
}