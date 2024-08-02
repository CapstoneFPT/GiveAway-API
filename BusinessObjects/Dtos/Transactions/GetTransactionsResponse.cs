using BusinessObjects.Dtos.Commons;

namespace BusinessObjects.Dtos.Transactions;

public class GetTransactionsResponse
{
    public Guid TransactionId { get; set; }
    public string? OrderCode { get; set; }
    public int Amount { get; set; }
    public DateTime CreatedDate { get; set; }
    public TransactionType Type { get; set; }
    public Guid? MemberId { get; set; }
}