namespace BusinessObjects.Utils;

public class AuctionNotFoundException : Exception
{
}

public class BankAccountNotSetException(string message)
    : Exception(message);

public class WithdrawNotFoundException : Exception
{
}

public class ScheduledTimeOverlappedException : Exception
{
}

public class ConsignSaleNotFoundException : Exception
{
}

public class AnomalousWithdrawStatusException(string message) : Exception(message);

public class FashionItemNotFoundException : Exception
{
}

public class AuctionAlreadyApprovedException : Exception
{
}

public class AuctionItemNotFoundException : Exception
{
}

public class AuctionAlreadyRejectedException : Exception
{
}

public class ShopNotFoundException : Exception
{
}

public class TimeslotNotFoundException : Exception
{
}

public class AuctionItemNotAvailableForAuctioningException : Exception
{
}

public class AccountNotFoundException : Exception
{
}

public class NotAuthorizedToPayOrderException : Exception
{
}

public class CannotUpdateShopBalanceException(string message) : Exception(message);

public class AuctionDepositNotFoundException : Exception
{
}

public class InsufficientBalanceException : Exception
{
}

public class OrderNotFoundException : Exception
{
}

public class WrongPaymentMethodException(string message) : Exception(message);

public class RefundExpiredException(string message) : Exception(message);

public class RefundNoFoundException : Exception
{
}

public class StatusNotAvailableException : Exception
{
}
public class StatusNotAvailableWithMessageException(string message) : Exception(message){}

public class ConsignSaleLineItemNotFoundException : Exception
{
}
public class AddressNotFoundException : Exception{}
public class OrderDetailNotFoundException : Exception{}
public class MissingFeatureException(string message) : Exception(message){}
public class OverStockException(string message) : Exception(message){}
public class MasterItemNotAvailableException(string message): Exception(message){}
public class CategoryNotFound(string message): Exception(message){}
public class StockCountUnavailableException(string message): Exception(message){}
public class ConfirmPriceIsNullException(string message): Exception(message){}