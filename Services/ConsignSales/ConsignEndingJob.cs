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
            .Include(c => c.ConsignSaleLineItems)
            .ThenInclude(c => c.IndividualFashionItem)
            .FirstOrDefaultAsync(c => c.ConsignSaleId == consignId);
        if (consignToEnd == null)
        {
            Console.WriteLine("No consign to end");
            return;
        }

        if (consignToEnd.ConsignSaleLineItems.Count == 0)
        {
            Console.WriteLine("No details valid");
            return;
        }

        try
        {
            consignToEnd.Status = ConsignSaleStatus.Completed;
            foreach (var detail in consignToEnd.ConsignSaleLineItems)
            {
                if (detail.IndividualFashionItem.Status is FashionItemStatus.Available or FashionItemStatus.Unavailable)
                {
                    detail.Status = ConsignSaleLineItemStatus.UnSold;
                    detail.IndividualFashionItem.Status = FashionItemStatus.UnSold;
                }
            }
            
            dbContext.ConsignSales.Update(consignToEnd);
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