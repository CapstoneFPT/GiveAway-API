using BusinessObjects.Dtos.Commons;
using BusinessObjects.Entities;
using Dao;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Services.Emails;

namespace Services.ConsignSales;

public class ConsignEndingJob : IJob
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IEmailService _emailService;
    public ConsignEndingJob(IServiceProvider serviceProvider, IEmailService emailService)
    {
        _serviceProvider = serviceProvider;
        _emailService = emailService;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<GiveAwayDbContext>();
        var consignId = context.JobDetail.JobDataMap.GetGuid("ConsignId");

        var consignToEnd = await dbContext.ConsignSales
            .Include(c => c.Member)
            .Include(c => c.ConsignSaleDetails)
            .ThenInclude(c => c.IndividualFashionItem)
            .FirstOrDefaultAsync(c => c.ConsignSaleId == consignId);
        if (consignToEnd == null)
        {
            Console.WriteLine("No consign to end");
            return;
        }

        if (consignToEnd.ConsignSaleDetails.Count == 0)
        {
            Console.WriteLine("No details valid");
            return;
        }

        try
        {
            consignToEnd.Status = ConsignSaleStatus.Completed;
            foreach (var detail in consignToEnd.ConsignSaleDetails)
            {
                if (!detail.IndividualFashionItem.Status.Equals(FashionItemStatus.Sold) &&
                    !detail.IndividualFashionItem.Status.Equals(FashionItemStatus.Refundable))
                {
                    detail.IndividualFashionItem.Status = FashionItemStatus.UnSold;
                }
            }

            consignToEnd.Member.Balance += consignToEnd.ConsignorReceivedAmount;
            dbContext.ConsignSales.Update(consignToEnd);
            var transaction = new Transaction()
            {
                ConsignSaleId = consignToEnd.ConsignSaleId,
                MemberId = consignToEnd.MemberId,
                CreatedDate = DateTime.UtcNow,
                Amount = consignToEnd.ConsignorReceivedAmount,
                Type = TransactionType.Payout,
            };
            dbContext.Transactions.Add(transaction);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        await dbContext.SaveChangesAsync();
        await _emailService.SendEmailConsignSaleEndedMail(consignId);
    }
}