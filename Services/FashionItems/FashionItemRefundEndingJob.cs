using BusinessObjects.Dtos.Commons;
using BusinessObjects.Entities;
using BusinessObjects.Utils;
using Dao;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Repositories.Accounts;
using Repositories.Transactions;

namespace Services.FashionItems;

public class FashionItemRefundEndingJob : IJob
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IAccountRepository _accountRepository;
    private readonly ITransactionRepository _transactionRepository;

    public FashionItemRefundEndingJob(IServiceProvider serviceProvider, IAccountRepository accountRepository, ITransactionRepository transactionRepository)
    {
        _serviceProvider = serviceProvider;
        _accountRepository = accountRepository;
        _transactionRepository = transactionRepository;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<GiveAwayDbContext>();
        var individualItemId = context.JobDetail.JobDataMap.GetGuid("RefundItemId");
        var refundItemToEnd =
            await dbContext.IndividualFashionItems
                .Include(c => c.ConsignSaleLineItem)
                .ThenInclude(c => c!.ConsignSale)
                .ThenInclude(c => c.Member)
                .FirstOrDefaultAsync(c => c.ItemId == individualItemId);
        if (refundItemToEnd is null || !refundItemToEnd.Status.Equals(FashionItemStatus.Refundable))
        {
            Console.WriteLine("No refundable item to end");
            return;
        }

        try
        {
            refundItemToEnd.Status = FashionItemStatus.Sold;
            if (refundItemToEnd.ConsignSaleLineItem != null)
            {
                var amountConsignorReceive = refundItemToEnd.SellingPrice!.Value * 80 / 100;
                
                refundItemToEnd.ConsignSaleLineItem.ConsignSale.Member!.Balance += amountConsignorReceive;
                var admin = await _accountRepository.FindOne(c => c.Role.Equals(Roles.Admin));
                if (admin == null)
                    throw new AccountNotFoundException();
                admin.Balance -= amountConsignorReceive;
                await _accountRepository.UpdateAccount(admin);

                refundItemToEnd.ConsignSaleLineItem.ConsignSale.SoldPrice += refundItemToEnd.SellingPrice!.Value;
                refundItemToEnd.ConsignSaleLineItem.ConsignSale.ConsignorReceivedAmount += amountConsignorReceive;
                
                var transaction = new Transaction() 
                {
                    MemberId = refundItemToEnd.ConsignSaleLineItem.ConsignSale.MemberId,
                    Amount = refundItemToEnd.SellingPrice!.Value,
                    CreatedDate = DateTime.UtcNow,
                    Type = TransactionType.Payout,
                    ConsignSaleId = refundItemToEnd.ConsignSaleLineItem.ConsignSaleId
                };
                await _transactionRepository.CreateTransaction(transaction);
            }
            
            dbContext.IndividualFashionItems.Update(refundItemToEnd);
            await dbContext.SaveChangesAsync();
            //await _emailService.SendMailSoldItemConsign();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    
}