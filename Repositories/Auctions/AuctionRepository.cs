using BusinessObjects;
using BusinessObjects.Dtos.Auctions;
using BusinessObjects.Entities;
using Dao;

namespace Repositories.Auctions
{
    public class AuctionRepository : IAuctionRepository
    {
        private readonly GenericDao<Auction> _auctionDao;
        private readonly GenericDao<Schedule> _scheduleDao;

        public AuctionRepository()
        {
            _auctionDao = new GenericDao<Auction>();
            _scheduleDao = new GenericDao<Schedule>();
        }

        public async Task CreateAuction(CreateAuctionRequest request)
        {
            try
            {

            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
    }
}