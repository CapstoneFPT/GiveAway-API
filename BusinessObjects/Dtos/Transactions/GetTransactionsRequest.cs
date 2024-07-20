using BusinessObjects.Dtos.Commons;

namespace BusinessObjects.Dtos.Transactions;

public class GetTransactionsRequest
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public TransactionType Type { get; set; }
}