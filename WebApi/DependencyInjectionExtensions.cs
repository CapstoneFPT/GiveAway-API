using Repositories.Accounts;
using Repositories.AuctionDeposits;
using Repositories.AuctionItems;
using Repositories.Auctions;
using Repositories.Bids;
using Repositories.Categories;
using Repositories.ConsignedForSaleItems;
using Repositories.Deliveries;
using Repositories.Images;
using Repositories.Inquiries;
using Repositories.Items;
using Repositories.OrderDetails;
using Repositories.Orders;
using Repositories.Packages;
using Repositories.Requests;
using Repositories.Schedules;
using Repositories.Shops;
using Repositories.Timeslots;
using Repositories.Transactions;
using Repositories.User;
using Repositories.Wallets;
using Services.Accounts;
using Services.AuctionDeposits;
using Services.AuctionItems;
using Services.Auctions;
using Services.Auth;
using Services.Bids;
using Services.Categories;
using Services.ConsignedForSaleItems;
using Services.Deliveries;
using Services.Emails;
using Services.Images;
using Services.Inquiries;
using Services.Items;
using Services.OrderDetails;
using Services.Orders;
using Services.Packages;
using Services.Requests;
using Services.Schedules;
using Services.Shops;
using Services.Timeslots;
using Services.Transactions;
using Services.Wallets;

namespace WebApi;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<ITokenService, TokenService>();
        serviceCollection.AddScoped<IAuthService, AuthService>();
        serviceCollection.AddScoped<IEmailService, EmailService>();
        serviceCollection.AddScoped<IAccountService, AccountService>();
        serviceCollection.AddScoped<IAuctionService, AuctionService>();
        serviceCollection.AddScoped<IAuctionItemService, AuctionItemService>();
        serviceCollection.AddScoped<IAuctionDepositService, AuctionDepositService>();
        serviceCollection.AddScoped<IBidService, BidService>();
        serviceCollection.AddScoped<ICategoryService, CategoryService>();
        serviceCollection.AddScoped<IConsignedForSaleItemService, ConsignedForSaleItemService>();
        serviceCollection.AddScoped<IDeliveryService, DeliveryService>();
        serviceCollection.AddScoped<IImageService, ImageService>();
        serviceCollection.AddScoped<IInquiryService, InquiryService>();
        serviceCollection.AddScoped<IItemService, ItemService>();
        serviceCollection.AddScoped<IOrderDetailService, OrderDetailService>();
        serviceCollection.AddScoped<IOrderService, OrderService>();
        serviceCollection.AddScoped<IPackageService, PackageService>();
        serviceCollection.AddScoped<IRequestService, RequestService>();
        serviceCollection.AddScoped<IScheduleService, ScheduleService>();
        serviceCollection.AddScoped<IShopService, ShopService>();
        serviceCollection.AddScoped<ITimeslotService, TimeslotService>();
        serviceCollection.AddScoped<ITransactionService, TransactionService>();
        serviceCollection.AddScoped<IWalletService, WalletService>();
        return serviceCollection;
    }

    public static IServiceCollection AddRepositories(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IUserRepository, UserRepository>();
        serviceCollection.AddScoped<IAccountRepository, AccountRepository>();
        serviceCollection.AddScoped<IAuctionDepositRepository, AuctionDepositRepository>();
        serviceCollection.AddScoped<IAuctionItemRepository, AuctionItemRepository>();
        serviceCollection.AddScoped<IAuctionRepository, AuctionRepository>();
        serviceCollection.AddScoped<IBidRepository, BidRepository>();
        serviceCollection.AddScoped<ICategoryRepository, CategoryRepository>();
        serviceCollection.AddScoped<IConsignedForSaleItemRepository, ConsignedItemForSaleRepository>();
        serviceCollection.AddScoped<IDeliveryRepository, DeliveryRepository>();
        serviceCollection.AddScoped<IImageRepository, ImageRepository>();
        serviceCollection.AddScoped<IInquiryRepository, InquiryRepository>();
        serviceCollection.AddScoped<IItemRepository, ItemRepository>();
        serviceCollection.AddScoped<IOrderDetailRepository, OrderDetailRepository>();
        serviceCollection.AddScoped<IOrderRepository, OrderRepository>();
        serviceCollection.AddScoped<IPackageRepository, PackageRepository>();
        serviceCollection.AddScoped<IRequestRepository, RequestRepository>();
        serviceCollection.AddScoped<IScheduleRepository, ScheduleRepository>();
        serviceCollection.AddScoped<IShopRepository, ShopRepository>();
        serviceCollection.AddScoped<ITimeslotRepository, TimeslotRepository>();
        serviceCollection.AddScoped<ITransactionRepository, TransactionRepository>();
        serviceCollection.AddScoped<IWalletRepository, WalletRepository>();
        return serviceCollection;
    }
}
