namespace BusinessObjects.Dtos.Commons;

public class Result<T>
{
    public T Data { get; set; }
    public ResultStatus ResultStatus { get; set; }
    public string[] Messages { get; set; }
}

public enum ResultStatus
{
    Success,
    NotFound,
    Duplicated,
    Error,

}

public enum Roles
{
    Account,
    Staff,
    Member,
    Admin
}

public enum AccountStatus
{
    Active,
    Inactive,
    NotVerified
}

public enum RechargeStatus
{
    Pending,
    Completed,
    Failed,
    Cancelled
}

public enum FashionItemStatus
{
    Available,
    Unavailable,
    OnDelivery,
    ReadyForDelivery,
    Sold,
    UnSold,
    Reserved,
    Refundable,
    Draft,
    PendingForConsignSale,
    PendingAuction,
    PendingForRefund,
    PendingForOrder,
    AwaitingAuction,
    AwaitingReturn,
    Bidding,
    Rejected,
    Returned,
    Won
}

public enum GenderType
{
    Male,
    Female,
}

public enum TimeSlotStatus
{
    Enabled,
    Disabled
}

public enum FashionItemType
{
    ItemBase,
    ConsignedForSale,
    ConsignedForAuction,
    CustomerSale
}

public enum ConsignSaleStatus
{
    Pending,
    AwaitDelivery,
    Processing,
    Negotiating,
    ReadyToSale,
    OnSale,
    Completed,
    Rejected,
    Cancelled
}

public enum ConsignSaleLineItemStatus
{
    Pending,
    AwaitDelivery,
    Negotiating,
    Received,
    Returned,
    Rejected,
    ReadyForConsignSale,
    OnSale,
    Sold,
    UnSold
}
public enum OrderStatus
{
    AwaitingPayment,
    OnDelivery,
    Completed,
    Cancelled,
    Pending
}


public enum RefundStatus
{
    Pending,
    Approved,
    Rejected,
    Completed,
    Cancelled
}

public enum PointPackageStatus
{
    Active,
    Inactive
}

public enum AuctionStatus
{
    Pending,
    Rejected,
    Approved,
    OnGoing,
    Finished,
    Cancelled
}

public enum TransactionType
{
    
    Purchase,
    CustomerSale,
    AddFund,
    Withdraw,
    RefundProduct,
    AuctionDeposit,
    ConsignPayout,
    RefundAuctionDeposit
}

public enum WithdrawStatus
{
    Processing,
    Expired,
    Completed,
}

public enum PurchaseType
{
    Online,
    Offline
}

public enum ItemCondition
{
    Never_Worn_W_Tag,
    Never_Worn,
    Very_Good,
    Good,
    Fair
}

public enum GhnErrorCode
{
    GHN_ERR81
}

public enum ConsignSaleType
{
    ConsignedForSale,
    ConsignedForAuction,
    CustomerSale
}

public enum ConsignSaleMethod
{
    Online,
    Offline
}

public enum AddressType
{
    Home,
    Business
}

public enum CategoryStatus
{
    Available,
    Unavailable
}

public enum PaymentMethod
{
    COD,
    Point,
    Banking,
    Cash
}

public enum SizeType
{
    XS,
    S,
    M,
    L,
    XL,
    XXL,
    XXXL,
    XXXXL,
}

public enum InquiryStatus
{
    Processing,
    Completed
}