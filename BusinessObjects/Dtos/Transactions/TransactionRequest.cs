using BusinessObjects.Dtos.Commons;

namespace BusinessObjects.Dtos.Transactions;

public class TransactionRequest
{
    public int? Page { get; set; }
    public int? PageSize { get; set; }
    public Guid? ShopId { get; set; }
    public TransactionType? TransactionType { get; set; }
}