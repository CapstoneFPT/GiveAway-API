using BusinessObjects.Dtos.Commons;

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
    public DateTime CreatedDate { get; set; }
    public string DepositCode { get; set; }
}

public class TransactionDetailResponse
{
    public Guid TransactionId { get; set; }
    public decimal Amount { get; set; }
    public DateTime CreatedDate { get; set; }
    public string Type { get; set; }
    public Guid OrderId { get; set; }
    public OrderDetailedResponse Order { get; set; }
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

public class OrderDetailedResponse
{
    public Guid OrderId { get; set; }
    public string OrderCode { get; set; } = "N/A";
    public decimal Subtotal { get; set; }
    public decimal ShippingFee { get; set; }
    public decimal Discount { get; set; }
    public decimal TotalPrice { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public DateTime? PaymentDate { get; set; }
    public PurchaseType PurchaseType { get; set; }
    public DateTime? CompletedDate { get; set; }
    public Guid MemberId { get; set; }
    public string? AuctionTitle { get; set; }
    public int Quantity { get; set; }
    public string ReciepientName { get; set; } = "N/A";
    public string CustomerName { get; set; }  = "N/A";
    public Guid BidId { get; set; }
    public decimal BidAmount { get; set; }
    public DateTime? BidCreatedDate { get; set; }
    public string Phone { get; set; } = "N/A";
    public string Address { get; set; } = "N/A";
    public AddressType AddressType { get; set; }
    public string Email { get; set; } = "N/A";
    public OrderStatus Status { get; set; } 
    public DateTime? CreatedDate { get; set; }
}

public class DeliveryDetailResponse
{
}

public class MemberDetailResponse
{
    public Guid MemberId { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public string Status { get; set; }
}