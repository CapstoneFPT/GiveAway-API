using BusinessObjects;
using BusinessObjects.Dtos.Auctions;
using BusinessObjects.Entities;

namespace Repositories.Auctions
{
    public class AuctionRepository : IAuctionRepository
    {
        private readonly GiveAwayDbContext _context;

        public AuctionRepository()
        {
            _context = new GiveAwayDbContext();
        }

        public async Task CreateAuction(CreateAuctionRequest request)
        {
            try
            {
                var newAuction = new Auction()
                {
                    Title = request.Title,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    ShopId = request.ShopId,
                    AuctionItemId = request.AuctionItemId,
                    Status = "Pending",
                };

                var newSchedule = new Schedule()
                {
                    TimeslotId = request.TimeslotId,
                    Date = request.ScheduleDate
                };
                
                await _context.Schedules.AddAsync(newSchedule); 
                await _context.Auctions.AddAsync(newAuction);

                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
    }
}