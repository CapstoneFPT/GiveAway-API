using BusinessObjects.Dtos.Commons;
using BusinessObjects.Entities;
using BusinessObjects.Utils;
using Dao;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Repositories.Accounts;
using Repositories.ConsignSales;
using Repositories.FashionItems;
using Repositories.Transactions;
using Services.Emails;

namespace Services.FashionItems;

public class FashionItemRefundEndingJob : IJob
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IAccountRepository _accountRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IEmailService _emailService;
    private readonly IConsignSaleRepository _consignSaleRepository;
    private readonly IFashionItemRepository _fashionItemRepository;
    public FashionItemRefundEndingJob(IServiceProvider serviceProvider, IAccountRepository accountRepository, 
        ITransactionRepository transactionRepository, IEmailService emailService, IConsignSaleRepository consignSaleRepository, IFashionItemRepository fashionItemRepository)
    {
        _serviceProvider = serviceProvider;
        _accountRepository = accountRepository;
        _transactionRepository = transactionRepository;
        _emailService = emailService;
        _consignSaleRepository = consignSaleRepository;
        _fashionItemRepository = fashionItemRepository;
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
                admin.Balance += amountConsignorReceive;
                await _accountRepository.UpdateAccount(admin);
                refundItemToEnd.ConsignSaleLineItem.Status = ConsignSaleLineItemStatus.Sold;
                refundItemToEnd.ConsignSaleLineItem.ConsignSale.SoldPrice += refundItemToEnd.SellingPrice!.Value;
                refundItemToEnd.ConsignSaleLineItem.ConsignSale.ConsignorReceivedAmount += amountConsignorReceive;
                
                var transaction = new Transaction() 
                {
                    SenderId = admin.AccountId,
                    SenderBalance = admin.Balance,
                    ReceiverBalance = refundItemToEnd.ConsignSaleLineItem.ConsignSale.Member.Balance,
                    ReceiverId = refundItemToEnd.ConsignSaleLineItem.ConsignSale.MemberId,
                    Amount = amountConsignorReceive,
                    CreatedDate = DateTime.UtcNow,
                    Type = TransactionType.ConsignPayout,
                    ConsignSaleId = refundItemToEnd.ConsignSaleLineItem.ConsignSaleId,
                    PaymentMethod = PaymentMethod.Point
                };
                await _transactionRepository.CreateTransaction(transaction);
                await _emailService.SendMailSoldItemConsign(refundItemToEnd.ConsignSaleLineItem.ConsignSaleLineItemId, amountConsignorReceive);
                
            }
            
            await _fashionItemRepository.UpdateFashionItem(refundItemToEnd);
            var consign = await _consignSaleRepository.GetQueryable()
                .Include(c => c.ConsignSaleLineItems)
                .Where(c => c.ConsignSaleId == refundItemToEnd.ConsignSaleLineItem!.ConsignSaleId).FirstOrDefaultAsync();
            if (consign.ConsignSaleLineItems.All(c => c.Status == ConsignSaleLineItemStatus.Sold))
            {
                consign.Status = ConsignSaleStatus.Completed;
                consign.EndDate = DateTime.UtcNow;
                await _consignSaleRepository.UpdateConsignSale(consign);
            }
            await dbContext.SaveChangesAsync();
            
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    
}