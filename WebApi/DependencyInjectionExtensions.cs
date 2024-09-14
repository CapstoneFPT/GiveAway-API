using BusinessObjects.Entities;
using Dao;
using Repositories;
using Repositories.Accounts;
using Repositories.AuctionDeposits;
using Repositories.AuctionItems;
using Repositories.Auctions;
using Repositories.BankAccounts;
using Repositories.Bids;
using Repositories.Categories;
using Repositories.ConsignedForSaleItems;
using Repositories.ConsignSaleLineItems;
using Repositories.ConsignSales;
using Repositories.Deliveries;
using Repositories.Images;
using Repositories.Inquiries;
using Repositories.FashionItems;
using Repositories.Feedbacks;
using Repositories.OrderLineItems;
using Repositories.Orders;
using Repositories.Recharges;
using Repositories.Refunds;
using Repositories.Revenues;
using Repositories.Schedules;
using Repositories.Shops;
using Repositories.Transactions;
using Repositories.Withdraws;

using Services;
using Services.Accounts;
using Services.AuctionDeposits;
using Services.AuctionItems;
using Services.Auctions;
using Services.Auth;
using Services.Bids;
using Services.Categories;
using Services.ConsignedForSaleItems;
using Services.ConsignLineItems;
using Services.ConsignSales;
using Services.Deliveries;
using Services.Emails;
using Services.FashionItems;
using Services.Feedbacks;
using Services.GiaoHangNhanh;
using Services.Images;
using Services.Inquiries;
using Services.OrderLineItems;
using Services.Orders;
using Services.Recharges;
using Services.Refunds;
using Services.Revenue;
using Services.Schedules;
using Services.Shops;
using Services.Transactions;
using Services.VnPayService;
using Services.Withdraws;
using Member = AutoMapper.Execution.Member;

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
        serviceCollection.AddScoped<IOrderLineItemService, OrderLineItemService>();
        serviceCollection.AddScoped<IOrderService, OrderService>();
        serviceCollection.AddScoped<IRechargeService, RechargeService>();
        serviceCollection.AddScoped<IConsignSaleService, ConsignSaleService>();
        serviceCollection.AddScoped<IScheduleService, ScheduleService>();
        serviceCollection.AddScoped<IShopService, ShopService>();
        serviceCollection.AddScoped<ITransactionService, TransactionService>();
        serviceCollection.AddScoped<IVnPayService, VnPayService>();
        serviceCollection.AddScoped<IRevenueService, RevenueService>();
        serviceCollection.AddScoped<IRefundService, RefundService>();
        serviceCollection.AddScoped<IWithdrawService, WithdrawService>();
        serviceCollection.AddScoped<IGiaoHangNhanhService, GiaoHangNhanhService>();
        serviceCollection.AddScoped<IConsignLineItemService, ConsignLineItemService>();
        serviceCollection.AddScoped<IFeedbackService, FeedbackService>();
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
        serviceCollection.AddScoped<IOrderLineItemRepository, OrderLineItemRepository>();
        serviceCollection.AddScoped<IOrderRepository, OrderRepository>();
        serviceCollection.AddScoped<IRechargeRepository, RechargeRepository>();
        serviceCollection.AddScoped<IConsignSaleRepository, ConsignSaleRepository>();
        serviceCollection.AddScoped<IScheduleRepository, ScheduleRepository>();
        serviceCollection.AddScoped<IShopRepository, ShopRepository>();
        serviceCollection.AddScoped<ITransactionRepository, TransactionRepository>();
        serviceCollection.AddScoped<IRevenueRepository, RevenueRepository>();
        serviceCollection.AddScoped<IRefundRepository, RefundRepository>();
        serviceCollection.AddScoped<IWithdrawRepository, WithdrawRepository>();
        serviceCollection.AddScoped<IConsignSaleLineItemRepository, ConsignSaleLineItemRepository>();
        serviceCollection.AddScoped<IBankAccountRepository, BankAccountRepository>();
        serviceCollection.AddScoped<IFeedbackRepository, FeedbackRepository>();
        return serviceCollection;
    }
    
    public static IServiceCollection AddDao(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<GenericDao<Account>>();
        serviceCollection.AddScoped<GenericDao<MasterFashionItem>>();
        serviceCollection.AddScoped<GenericDao<IndividualFashionItem>>();
        serviceCollection.AddScoped<GenericDao<IndividualAuctionFashionItem>>();
        serviceCollection.AddScoped<GenericDao<Auction>>();
        serviceCollection.AddScoped<GenericDao<AuctionDeposit>>();
        serviceCollection.AddScoped<GenericDao<Bid>>();
        serviceCollection.AddScoped<GenericDao<Category>>();
        serviceCollection.AddScoped<GenericDao<Address>>();
        serviceCollection.AddScoped<GenericDao<Image>>();
        serviceCollection.AddScoped<GenericDao<Inquiry>>();
        serviceCollection.AddScoped<GenericDao<OrderLineItem>>();
        serviceCollection.AddScoped<GenericDao<Order>>();
        serviceCollection.AddScoped<GenericDao<Recharge>>();
        serviceCollection.AddScoped<GenericDao<ConsignSale>>();
        serviceCollection.AddScoped<GenericDao<ConsignSaleLineItem>>();
        serviceCollection.AddScoped<GenericDao<Shop>>();
        serviceCollection.AddScoped<GenericDao<Transaction>>();
        serviceCollection.AddScoped<GenericDao<Refund>>();
        serviceCollection.AddScoped<GenericDao<Withdraw>>();
        serviceCollection.AddScoped<GenericDao<BankAccount>>();
        serviceCollection.AddScoped<GenericDao<Feedback>>();
        serviceCollection.AddScoped<GenericDao<BusinessObjects.Entities.Member>>();
        return serviceCollection;
    }

    public static IServiceCollection AddLongRunningServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddHostedService<AuctionEndingService>();
        serviceCollection.AddHostedService<AuctionStartingService>();
        serviceCollection.AddHostedService<OrderCancelingService>();
        serviceCollection.AddHostedService<CompleteRefundableItemsService>();
        return serviceCollection;
    }
}