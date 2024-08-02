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
        var auctionId = dataMap.GetGuid("AuctionId");

        try
        {
            var auctionToStart = await dbContext.Auctions
                .Include(auction => auction.AuctionFashionItem)
                .FirstOrDefaultAsync(x => x.AuctionId == auctionId);

            if (auctionToStart == null)
            {
                return;
            }

            auctionToStart.Status = AuctionStatus.OnGoing;
            auctionToStart.AuctionFashionItem.Status = FashionItemStatus.Bidding;


            dbContext.Auctions.Update(auctionToStart);
            await dbContext.SaveChangesAsync();

        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}