using BusinessObjects;
using BusinessObjects.Dtos.AuctionItems;
using BusinessObjects.Dtos.Auctions;
using BusinessObjects.Dtos.Shops;
using BusinessObjects.Entities;
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
        

        public AuctionRepository()
        {
            _auctionDao = new GenericDao<Auction>();
            _scheduleDao = new GenericDao<Schedule>();
            _auctionFashionItemDao = new GenericDao<AuctionFashionItem>();
            _shopDao = new GenericDao<Shop>();
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
                var shop = await _shopDao.GetQueryable()
                    .FirstOrDefaultAsync(x => x.ShopId == request.ShopId);

                var newAuction = new Auction
                {
                    AuctionItemId = request.AuctionItemId,
                    Title = request.Title,
                    ShopId = request.ShopId,
                    DepositFee = request.DepositFee,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    Status = "Pending"
                };
                var auctionDetail = await _auctionDao.AddAsync(newAuction);

                var newSchedule = new Schedule()
                {
                    AuctionId = auctionDetail.AuctionId,
                    Date = DateOnly.FromDateTime(request.StartDate),
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
                       StaffId = shop.StaffId
                    },
                    Status = auctionDetail.Status,
                    Title = auctionDetail.Title,
                    AuctionFashionItem = new AuctionFashionItemDetailResponse()
                    {
                        
                    },
                    DepositFee = auctionDetail.DepositFee,
                    ShopId = auctionDetail.ShopId,
                    AuctionItemId = auctionDetail.AuctionItemId,
                };
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
    }
}