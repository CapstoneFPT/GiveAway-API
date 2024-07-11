namespace BusinessObjects.Utils;

public class AuctionNotFoundException : Exception
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

public class WrongPaymentMethodException(string message) : Exception(message);