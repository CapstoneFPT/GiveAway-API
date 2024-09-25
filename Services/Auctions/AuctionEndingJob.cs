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
using Services.Emails;

namespace Services.Auctions;

public class AuctionEndingJob : IJob
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IHubContext<AuctionHub> _hubContext;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IEmailService _emailService;
    private readonly ILogger<AuctionEndingJob> _logger;

    public AuctionEndingJob(IServiceProvider serviceProvider, IHubContext<AuctionHub> hubContext,
        ITransactionRepository transactionRepository, IEmailService emailService, ILogger<AuctionEndingJob> logger)
    {
        _serviceProvider = serviceProvider;
        _hubContext = hubContext;
        _transactionRepository = transactionRepository;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<GiveAwayDbContext>();
        var auctionId = context.JobDetail.JobDataMap.GetGuid("AuctionEndId");

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

                    var memberWinning =
                        await dbContext.Members.FirstOrDefaultAsync(x => x.AccountId == orderRequest.MemberId);


                    var newOrder = new Order()
                    {
                        BidId = orderRequest.BidId,
                        OrderCode = orderRequest.OrderCode,
                        PaymentMethod = orderRequest.PaymentMethod,
                        Discount = auctionToEnd.DepositFee,
                        MemberId = orderRequest.MemberId,
                        TotalPrice = orderRequest.TotalPrice,

                        Email = memberWinning!.Email,
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
                        Quantity = 1,
                        CreatedDate = DateTime.UtcNow,
                    };

                    dbContext.OrderLineItems.Add(orderDetail);
                    await _emailService.SendEmailAuctionWon(auctionId, winningBid);
                    foreach (var auctionDeposit in auctionToEnd.AuctionDeposits)
                    {
                        if (await dbContext.Bids.AnyAsync(c => c.MemberId == auctionDeposit.MemberId))
                        {
                            if (winningBid.MemberId == auctionDeposit.MemberId) continue;
                            var member =
                                await dbContext.Members.FirstOrDefaultAsync(c =>
                                    c.AccountId == auctionDeposit.MemberId);
                            if (member is not { Status: AccountStatus.Active }) continue;
                            var admin = await dbContext.Admins.FirstOrDefaultAsync();
                            if (admin is not { Status: AccountStatus.Active }) continue;
                            var deposit = await dbContext.AuctionDeposits.FirstOrDefaultAsync(x =>
                                x.AuctionId == auctionId && x.MemberId == auctionDeposit.MemberId);
                            _logger.LogInformation("Member {MemberId} balance before refund: {Balance}",
                                member.AccountId, member.Balance);
                            _logger.LogInformation("Admin balance before refund: {Balance}", admin.Balance);

                            member.Balance += auctionToEnd.DepositFee;
                            admin.Balance += auctionToEnd.DepositFee;
                            _logger.LogInformation("Member {MemberId} has received {Amount} from auction deposit",
                                member.AccountId, auctionToEnd.DepositFee);
                            _logger.LogInformation("Admin has received {Amount} from auction deposit",
                                auctionToEnd.DepositFee);
                            
                            _logger.LogInformation("Member {MemberId} balance after refund: {Balance}",
                                member.AccountId, member.Balance);
                            _logger.LogInformation("Admin balance after refund: {Balance}", admin.Balance);

                            dbContext.Members.Update(member);
                            dbContext.Admins.Update(admin);
                            var refundDepositTransaction = new Transaction()
                            {
                                SenderId = admin.AccountId,
                                SenderBalance = admin.Balance,
                                ReceiverId = member.AccountId,
                                ReceiverBalance = member.Balance,
                                CreatedDate = DateTime.UtcNow,
                                AuctionDepositId = deposit!.AuctionDepositId,
                                Amount = auctionToEnd.DepositFee,
                                Type = TransactionType.RefundAuctionDeposit,
                                PaymentMethod = PaymentMethod.Point,
                                TransactionCode = await _transactionRepository.GenerateUniqueString()
                            };
                            dbContext.Transactions.Add(refundDepositTransaction);
                        }
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