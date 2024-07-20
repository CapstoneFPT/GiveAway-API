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
    private readonly ILogger _logger;

    public AuctionStartingJob(IServiceProvider serviceProvider, ILogger logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
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
                _logger.LogInformation("No auction to start");
                return;
            }

            auctionToStart.Status = AuctionStatus.OnGoing;
            auctionToStart.AuctionFashionItem.Status = FashionItemStatus.Bidding;

            _logger.LogInformation("Auction {AuctionId} has been started", auctionToStart.AuctionId);

            dbContext.Auctions.UpdateRange(auctionToStart);
            await dbContext.SaveChangesAsync();

            _logger.LogInformation("Auction {AuctionId} has been started", auctionToStart.AuctionId);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error starting auction : {AuctionId}", auctionId);
        }
    }
}