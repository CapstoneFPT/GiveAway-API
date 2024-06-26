﻿namespace BusinessObjects.Dtos.Commons;

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
    Empty
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

public enum FashionItemStatus
{
    Available,
    Unavailable,
    Sold,
    Pending,
    AwaitingAuction,
    Bidding,
    Won,
    Rejected,
    Returned
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
    ConsignedForAuction
}

public enum ConsignSaleStatus
{
    Pending,
    AwaitDelivery,
    Received,
    Completed,
    Rejected,
    Cancelled
}

public enum OrderStatus
{
    AwaitingPayment,
    OnDelivery,
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
    Finished
}

public enum TransactionType
{
    AuctionDeposit,
    Withdraw,
    Purchase,
    Refund,
    Recharge
}

public enum ConsignSaleType
{
    ConsignedForSale,
    ConsignedForAuction
}

public enum AddressType
{
    Home,
    Business
}

public enum CategoryStatus
{
    Available,
    Unavailable,
    Special
}

public enum PaymentMethod
{
    COD,
    Point,
    QRCode
}