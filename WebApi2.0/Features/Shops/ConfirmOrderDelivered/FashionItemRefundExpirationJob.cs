using Microsoft.EntityFrameworkCore;
using Quartz;
using Serilog;
using WebApi2._0.Common;
using WebApi2._0.Domain.Entities;
using WebApi2._0.Domain.Enums;
using WebApi2._0.Infrastructure.Persistence;

namespace WebApi2._0.Features.Shops.ConfirmOrderDelivered;

public record CreateConsignmentTransactionParams
{
    public required decimal Amount { get; init; }
    public required Account Consignor { get; init; }
    public required Account AdminAccount { get; init; }
    public required IndividualFashionItem RefundItem { get; init; }
}

public sealed class FashionItemRefundExpirationJob : IJob
{
    private readonly GiveAwayDbContext _dbContext;

    public FashionItemRefundExpirationJob(GiveAwayDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var itemId = context.JobDetail.JobDataMap.GetGuid("RefundItemId");

        try
        {
            var refundItem = await GetRefundableItem(itemId);
            if (refundItem is null)
            {
                Log.Error("Refundable item not found {ItemId}", itemId);
                return;
            }

            await ProcessRefundableItem(refundItem);
            await _dbContext.SaveChangesAsync();
        }
        catch (Exception e)
        {
            Log.Error(e, "Error ending refundable item {ItemId}", itemId);
        }
    }

    private async Task<IndividualFashionItem?> GetRefundableItem(Guid itemId)
    {
        var refundItem = await _dbContext.IndividualFashionItems
            .Include(x => x.ConsignSaleLineItem)
            .ThenInclude(x => x.ConsignSale)
            .ThenInclude(x => x.Member)
            .Include(x => x.ConsignSaleLineItem)
            .ThenInclude(x => x.ConsignSale)
            .ThenInclude(x => x.ConsignSaleLineItems)
            .FirstOrDefaultAsync(x => x.ItemId == itemId && x.Status == FashionItemStatus.Refundable);


        return refundItem;
    }

    private async Task ProcessRefundableItem(IndividualFashionItem refundItem)
    {
        refundItem.Status = FashionItemStatus.Sold;

        if (refundItem.ConsignSaleLineItem != null)
        {
            await ProcessConsignmentItem(refundItem);
        }

        await UpdateConsignmentStatus(refundItem);
    }

    private async Task ProcessConsignmentItem(IndividualFashionItem refundItem)
    {
        var amountConsignorReceive = CalculateConsignorAmount(refundItem);
        var consignLineItem = refundItem.ConsignSaleLineItem;

        if (consignLineItem == null)
        {
            throw new ConsignSaleLineItemNotFoundException();
        }

        var consignor = refundItem.ConsignSaleLineItem?.ConsignSale.Member;

        if (consignor == null)
        {
            throw new ConsignorNotFoundException();
        }

        consignor.Balance += amountConsignorReceive;

        var adminAccount = await GetAdminAccount();
        adminAccount.Balance += amountConsignorReceive;

        UpdateConsignSaleLineItem(refundItem, amountConsignorReceive);
        await CreateConsignmentTransaction(new CreateConsignmentTransactionParams()
        {
            Amount = amountConsignorReceive,
            Consignor = consignor,
            AdminAccount = adminAccount,
            RefundItem = refundItem
        });
    }

    private static decimal CalculateConsignorAmount(IndividualFashionItem refundItem)
    {
        return refundItem.SellingPrice!.Value * 0.8m;
    }

    private async Task<Account> GetAdminAccount()
    {
        var adminAccount = await _dbContext.Accounts.FirstOrDefaultAsync(x => x.Role == Roles.Admin);
        if (adminAccount == null)
        {
            throw new AdminAccountNotFoundException();
        }

        return adminAccount;
    }

    private static void UpdateConsignSaleLineItem(IndividualFashionItem refundItem, decimal amountConsignorReceive)
    {
        var consignSaleLineItem = refundItem.ConsignSaleLineItem;

        if (consignSaleLineItem == null)
        {
            throw new ConsignSaleLineItemNotFoundException();
        }

        consignSaleLineItem.Status = ConsignSaleLineItemStatus.Sold;
        consignSaleLineItem.ConsignSale.SoldPrice += refundItem.SellingPrice!.Value;
        consignSaleLineItem.ConsignSale.ConsignorReceivedAmount += amountConsignorReceive;
    }

    private async Task CreateConsignmentTransaction(
        CreateConsignmentTransactionParams @params)
    {
        var refundItemConsignSaleLineItem = @params.RefundItem.ConsignSaleLineItem;
        var adminAccount = @params.AdminAccount;
        var consignor = @params.Consignor;
        var amount = @params.Amount;
        
        if (refundItemConsignSaleLineItem == null)
        {
            throw new ConsignSaleLineItemNotFoundException();
        }


        var transaction = new Transaction
        {
            SenderId = adminAccount.AccountId,
            SenderBalance = adminAccount.Balance,
            ReceiverBalance = consignor.Balance,
            ReceiverId = consignor.AccountId,
            Amount = amount,
            CreatedDate = DateTime.UtcNow,
            Type = TransactionType.ConsignPayout,
            ConsignSaleId = refundItemConsignSaleLineItem.ConsignSaleId,
            PaymentMethod = PaymentMethod.Point
        };
         _dbContext.Transactions.Add(transaction);
    }

    private static Task UpdateConsignmentStatus(IndividualFashionItem refundItem)
    {
        var consignment = refundItem.ConsignSaleLineItem?.ConsignSale;
        if (consignment == null || consignment.ConsignSaleLineItems.Any(x => x.Status != ConsignSaleLineItemStatus.Sold))
            return Task.CompletedTask;
        consignment.Status = ConsignSaleStatus.Completed;
        consignment.EndDate = DateTime.UtcNow;

        return Task.CompletedTask;
    }
}

public class ConsignSaleLineItemNotFoundException : Exception
{
}

public class ConsignorNotFoundException : Exception
{
}