namespace BusinessObjects.Dtos.Transactions;

public class TransactionResponse
{
    public Guid TransactionId { get; set; }
    public Guid? OrderId { get; set; }
    public int Amount { get; set; }
    public DateTime CreatedDate { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
}