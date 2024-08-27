using BusinessObjects.Dtos.Commons;
using Dao;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace Services.FashionItems;

public class FashionItemReservedEndingJob : IJob
{
    private readonly IServiceProvider _serviceProvider;

    public FashionItemReservedEndingJob(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<GiveAwayDbContext>();
        var individualItemId = context.JobDetail.JobDataMap.GetGuid("ReservedItemId");

        var reservedItemToEnd =
            await dbContext.IndividualFashionItems.FirstOrDefaultAsync(c => c.ItemId == individualItemId);
        if (reservedItemToEnd is null || !reservedItemToEnd.Status.Equals(FashionItemStatus.Reserved))
        {
            Console.WriteLine("No reserved item to end");
            return;
        }

        try
        {
            reservedItemToEnd.Status = FashionItemStatus.Unavailable;
            dbContext.IndividualFashionItems.Update(reservedItemToEnd);
            await dbContext.SaveChangesAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}