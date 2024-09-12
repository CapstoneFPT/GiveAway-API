using BusinessObjects.Dtos.Auctions;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Entities;
using Dao;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using Repositories.Orders;
using Repositories.Transactions;

namespace Services.Auctions;

public class AuctionEndingJob : IJob
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IHubContext<AuctionHub> _hubContext;
    private readonly ITransactionRepository _transactionRepository;

    public AuctionEndingJob(IServiceProvider serviceProvider, IHubContext<AuctionHub> hubContext, ITransactionRepository transactionRepository)
    {
        _serviceProvider = serviceProvider;
        _hubContext = hubContext;
        _transactionRepository = transactionRepository;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<GiveAwayDbContext>();
        var auctionId = context.JobDetail.JobDataMap.GetGuid("AuctionId");

        var auctionToEnd = await dbContext.Auctions
            .Include(x => x.IndividualAuctionFashionItem)
            .Include(x => x.Bids)
            .Include(c => c.AuctionDeposits)
            .FirstOrDefaultAsync(x => x.AuctionId == auctionId);

        if (auctionToEnd == null)
        {
            Console.WriteLine("No auction to end");
            return;
        }

        /*if (auctionToEnd.Bids.Count == 0)
        {
            Console.WriteLine("No bids");

            dbContext.Auctions.Update(auctionToEnd);
            await dbContext.SaveChangesAsync();
            return;
        }*/

        try
        {
            if (auctionToEnd.AuctionDeposits.Count == 0)
            {
                Console.WriteLine("No participant");
                auctionToEnd.IndividualAuctionFashionItem.Status = FashionItemStatus.Unavailable;
                auctionToEnd.Status = AuctionStatus.Finished;
            }
            else
            {
                var winningBid = auctionToEnd.Bids.MaxBy(x => x.Amount);

                if (winningBid == null)
                {
                    Console.WriteLine("No winning bid");
                    auctionToEnd.IndividualAuctionFashionItem.Status = FashionItemStatus.Unavailable;
                    auctionToEnd.Status = AuctionStatus.Finished;
                }
                else
                {
                    auctionToEnd.Status = AuctionStatus.Finished;
                    auctionToEnd.IndividualAuctionFashionItem.Status = FashionItemStatus.Won;
                    auctionToEnd.IndividualAuctionFashionItem.SellingPrice = winningBid.Amount;

                    var orderRepository = scope.ServiceProvider.GetRequiredService<IOrderRepository>();

                    var orderRequest = new CreateOrderFromBidRequest()
                    {
                        MemberId = winningBid.MemberId,
                        OrderCode = orderRepository.GenerateUniqueString(),
                        PaymentMethod = PaymentMethod.Point,
                        TotalPrice = winningBid.Amount,
                        BidId = winningBid.BidId,
                        AuctionFashionItemId = auctionToEnd.IndividualAuctionFashionItemId
                    };

                    var address =
                        await dbContext.Addresses.FirstOrDefaultAsync(x =>
                            x.MemberId == orderRequest.MemberId && x.IsDefault);

                    var member = await dbContext.Members.FirstOrDefaultAsync(x => x.AccountId == orderRequest.MemberId);


                    var newOrder = new Order()
                    {
                        BidId = orderRequest.BidId,
                        OrderCode = orderRequest.OrderCode,
                        PaymentMethod = orderRequest.PaymentMethod,
                        MemberId = orderRequest.MemberId,
                        TotalPrice = orderRequest.TotalPrice,
                        Address = address?.Residence,
                        RecipientName = address?.RecipientName,
                        Email = member!.Email,
                        Phone = address?.Phone,
                        CreatedDate = DateTime.UtcNow,
                        Status = OrderStatus.AwaitingPayment
                    };
                    dbContext.Orders.Add(newOrder);


                    var orderDetail = new OrderLineItem()
                    {
                        OrderId = newOrder.OrderId,
                        IndividualFashionItemId = orderRequest.AuctionFashionItemId,
                        UnitPrice = orderRequest.TotalPrice,
                        CreatedDate = DateTime.UtcNow,
                    };

                    dbContext.OrderLineItems.Add(orderDetail);
                }

                foreach (var auctionDeposit in auctionToEnd.AuctionDeposits)
                {
                    if (dbContext.Bids.Any(c => c.MemberId == auctionDeposit.MemberId))
                    {
                        var member = await dbContext.Members.FirstOrDefaultAsync(c => c.AccountId == auctionDeposit.MemberId);
                        if (member is not { Status: AccountStatus.Active }) continue;
                        var admin = await dbContext.Admins.FirstOrDefaultAsync();
                        if (admin is not { Status: AccountStatus.Active }) continue;
                        member.Balance += auctionToEnd.DepositFee;
                        admin.Balance -= auctionToEnd.DepositFee;
                        dbContext.Members.Update(member);
                        dbContext.Admins.Update(admin);
                        var refundDepositTransaction = new Transaction()
                        {
                            SenderId = admin.AccountId,
                            ReceiverId = member.AccountId,
                            CreatedDate = DateTime.UtcNow,
                            Amount = auctionToEnd.DepositFee,
                            Type = TransactionType.RefundAuctionDeposit,
                            TransactionCode = await _transactionRepository.GenerateUniqueString()
                        };
                        dbContext.Transactions.Add(refundDepositTransaction);

                    }
                }
            }


            dbContext.Auctions.Update(auctionToEnd);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        await dbContext.SaveChangesAsync();
    }
}