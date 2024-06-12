namespace BusinessObjects.Dtos.AuctionDeposits;

public class AuctionDepositDetailResponse
{
    public Guid Id { get; set; }
    public Guid AuctionId { get; set; }
    public Guid MemberId { get; set; }
    public MemberDetailResponse Member { get; set; }
    public decimal Amount { get; set; }
    public Guid TransactionId { get; set; }
    public TransactionDetailResponse Transaction { get; set; }
}

public class TransactionDetailResponse
{
    public Guid TransactionId { get; set; }
    public decimal Amount { get; set; }
    public DateTime CreatedDate { get; set; }
    public string Type { get; set; }
    public Guid OrderId { get; set; }
    public OrderDetailResponse Order { get; set; }
    public Guid WalletId { get; set; }
    public WalletDetailResponse Wallet { get; set; }
}

public class WalletDetailResponse
{
    public Guid WalletId { get; set; }
    public decimal Balance { get; set; }
    public Guid MemberId { get; set; }
    public MemberDetailResponse Member { get; set; }
    public string BankAccountNumber { get; set; }
    public string BankName { get; set; }
}

public class OrderDetailResponse
{
    public Guid OrderId { get; set; }
    public decimal TotalPrice { get; set; }
    public DateTime CreatedDate { get; set; }
    public string PaymentMethod { get; set; }
    public DateTime PayementDate { get; set; }
    public Guid MemberId { get; set; }
    public MemberDetailResponse Member { get; set; }
    public Guid DeliveryId { get; set; }
    public DeliveryDetailResponse Delivery { get; set; }
}

public class DeliveryDetailResponse
{
    public Guid DeliveryId { get; set; }
    public string FullName { get; set; }
    public string Phone { get; set; }
    public string Address { get; set; }
}

public class MemberDetailResponse
{
    public Guid MemberId { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public string Status { get; set; }
}