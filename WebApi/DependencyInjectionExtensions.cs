using BusinessObjects.Entities;
using Dao;
using Repositories.Accounts;
using Repositories.AuctionDeposits;
using Repositories.AuctionItems;
using Repositories.Auctions;
using Repositories.Bids;
using Repositories.Categories;
using Repositories.ConsignedForSaleItems;
using Repositories.ConsignSales;
using Repositories.Deliveries;
using Repositories.Images;
using Repositories.Inquiries;
using Repositories.FashionItems;
using Repositories.OrderDetails;
using Repositories.Orders;
using Repositories.PointPackages;
using Repositories.Requests;
using Repositories.Schedules;
using Repositories.Shops;
using Repositories.Timeslots;
using Repositories.Transactions;
using Services;
using Services.Accounts;
using Services.AuctionDeposits;
using Services.AuctionItems;
using Services.Auctions;
using Services.Auth;
using Services.Bids;
using Services.Categories;
using Services.ConsignedForSaleItems;
using Services.ConsignSales;
using Services.Deliveries;
using Services.Emails;
using Services.FashionItems;
using Services.Images;
using Services.Inquiries;
using Services.OrderDetails;
using Services.Orders;
using Services.PointPackages;
using Services.Schedules;
using Services.Shops;
using Services.Timeslots;
using Services.Transactions;

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
        serviceCollection.AddScoped<IFashionItemService, FashionItemService>();
        serviceCollection.AddScoped<IOrderDetailService, OrderDetailService>();
        serviceCollection.AddScoped<IOrderService, OrderService>();
        serviceCollection.AddScoped<IPointPackageService, PointPackageService>();
        serviceCollection.AddScoped<IConsignSaleService, ConsignSaleService>();
        serviceCollection.AddScoped<IScheduleService, ScheduleService>();
        serviceCollection.AddScoped<IShopService, ShopService>();
        serviceCollection.AddScoped<ITimeslotService, TimeslotService>();
        serviceCollection.AddScoped<ITransactionService, TransactionService>();
        serviceCollection.AddAutoMapper(typeof(MappingProfile).Assembly);
        return serviceCollection;
    }

    public static IServiceCollection AddRepositories(this IServiceCollection serviceCollection)
    {
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
        serviceCollection.AddScoped<IFashionItemRepository, FashionItemRepository>();
        serviceCollection.AddScoped<IOrderDetailRepository, OrderDetailRepository>();
        serviceCollection.AddScoped<IOrderRepository, OrderRepository>();
        serviceCollection.AddScoped<IPointPackageRepository, PointPackageRepository>();
        serviceCollection.AddScoped<IConsignSaleRepository, ConsignSaleRepository>();
        serviceCollection.AddScoped<IScheduleRepository, ScheduleRepository>();
        serviceCollection.AddScoped<IShopRepository, ShopRepository>();
        serviceCollection.AddScoped<ITimeslotRepository, TimeslotRepository>();
        serviceCollection.AddScoped<ITransactionRepository, TransactionRepository>();
        return serviceCollection;
    }
    
    public static IServiceCollection AddDao(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<GenericDao<Account>>();
        serviceCollection.AddScoped<GenericDao<FashionItem>>();
        serviceCollection.AddScoped<GenericDao<AuctionFashionItem>>();
        serviceCollection.AddScoped<GenericDao<Auction>>();
        serviceCollection.AddScoped<GenericDao<AuctionDeposit>>();
        serviceCollection.AddScoped<GenericDao<Bid>>();
        serviceCollection.AddScoped<GenericDao<Category>>();
        serviceCollection.AddScoped<GenericDao<Address>>();
        serviceCollection.AddScoped<GenericDao<Image>>();
        serviceCollection.AddScoped<GenericDao<Inquiry>>();
        serviceCollection.AddScoped<GenericDao<OrderDetail>>();
        serviceCollection.AddScoped<GenericDao<Order>>();
        serviceCollection.AddScoped<GenericDao<PointPackage>>();
        serviceCollection.AddScoped<GenericDao<ConsignSale>>();
        serviceCollection.AddScoped<GenericDao<ConsignSaleDetail>>();
        serviceCollection.AddScoped<GenericDao<Schedule>>();
        serviceCollection.AddScoped<GenericDao<Shop>>();
        serviceCollection.AddScoped<GenericDao<Timeslot>>();
        serviceCollection.AddScoped<GenericDao<Transaction>>();
        return serviceCollection;
    }
}