using BusinessObjects.Dtos.Commons;
using BusinessObjects.Entities;
using Dao;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace Services.ConsignSales;

public class ConsignEndingJob : IJob
{
    private readonly IServiceProvider _serviceProvider;

    public ConsignEndingJob(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<GiveAwayDbContext>();
        var consignId = context.JobDetail.JobDataMap.GetGuid("ConsignId");

        var consignToEnd = await dbContext.ConsignSales
            .Include(c => c.Member)
            .Include(c => c.ConsignSaleDetails)
            .ThenInclude(c => c.FashionItem)
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
                if (!detail.FashionItem.Status.Equals(FashionItemStatus.Sold) &&
                    !detail.FashionItem.Status.Equals(FashionItemStatus.Refundable))
                {
                    detail.FashionItem.Status = FashionItemStatus.UnSold;
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
    }
}