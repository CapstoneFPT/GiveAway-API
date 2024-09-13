using BusinessObjects.Dtos.Commons;
using Dao;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Services.Auctions;

public class AuctionStartingJob : IJob
{
    private readonly IServiceProvider _serviceProvider;

    public AuctionStartingJob(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<GiveAwayDbContext>();


        var dataMap = context.JobDetail.JobDataMap;
        var auctionId = dataMap.GetGuid("AuctionStartId");

        try
        {
            var auctionToStart = await dbContext.Auctions
                .Include(auction => auction.IndividualAuctionFashionItem)
                .FirstOrDefaultAsync(x => x.AuctionId == auctionId);

            if (auctionToStart == null)
            {
                return;
            }

            if (auctionToStart.Status == AuctionStatus.Pending)
            {
                Console.WriteLine("Auction is not approved");
                auctionToStart.Status = AuctionStatus.Cancelled;
                auctionToStart.IndividualAuctionFashionItem.Status = FashionItemStatus.Available;
            }
            if(auctionToStart.Status == AuctionStatus.Approved)
            {
                auctionToStart.Status = AuctionStatus.OnGoing;
                auctionToStart.IndividualAuctionFashionItem.Status = FashionItemStatus.Bidding;
            }
            else
            {
                return;
            }
            dbContext.Auctions.Update(auctionToStart);
            await dbContext.SaveChangesAsync();

        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}