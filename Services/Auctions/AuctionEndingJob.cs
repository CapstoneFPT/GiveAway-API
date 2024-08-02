using BusinessObjects.Dtos.Auctions;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Entities;
using Dao;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using Repositories.Orders;

namespace Services.Auctions;

public class AuctionEndingJob : IJob
{
    private readonly IServiceProvider _serviceProvider;

    public AuctionEndingJob(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<GiveAwayDbContext>();
        var auctionId = context.JobDetail.JobDataMap.GetGuid("AuctionId");

        var auctionToEnd = await dbContext.Auctions
            .Include(x => x.AuctionFashionItem)
            .Include(x => x.Bids)
            .FirstOrDefaultAsync(x => x.AuctionId == auctionId);

        if (auctionToEnd == null)
        {
            Console.WriteLine("No auction to end");
            return;
        }

        if (auctionToEnd.Bids.Count == 0)
        {
           Console.WriteLine("No bids");
           return;
        }
        
        try
        {
            var winningBid = auctionToEnd.Bids.MaxBy(x=> x.Amount);
            auctionToEnd.Status = AuctionStatus.Finished;
            dbContext.Auctions.Update(auctionToEnd);

            var orderRepository = scope.ServiceProvider.GetRequiredService<IOrderRepository>();

            var orderRequest = new CreateOrderFromBidRequest()
            {
                MemberId = winningBid.MemberId,
                OrderCode = orderRepository.GenerateUniqueString(),
                PaymentMethod = PaymentMethod.Point,
                TotalPrice = winningBid.Amount,
                BidId = winningBid.BidId,
                AuctionFashionItemId = auctionToEnd.AuctionFashionItemId
            };

            var newOrder = new Order()
            {
                BidId = orderRequest.BidId,
                OrderCode = orderRequest.OrderCode,
                PaymentMethod = orderRequest.PaymentMethod,
                MemberId = orderRequest.MemberId,
                TotalPrice = orderRequest.TotalPrice,
                CreatedDate = DateTime.UtcNow,
            };
            dbContext.Orders.Add(newOrder);


            var orderDetail = new OrderDetail()
            {
                OrderId = newOrder.OrderId,
                FashionItemId = orderRequest.AuctionFashionItemId,
                UnitPrice = orderRequest.TotalPrice,
            };

            dbContext.OrderDetails.Add(orderDetail);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        await dbContext.SaveChangesAsync();
    }
}
