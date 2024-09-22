using BusinessObjects.Dtos.Commons;
using BusinessObjects.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Configuration;

namespace Dao;

public class GiveAwayDbContext : DbContext
{
    public DbSet<Account> Accounts { get; set; }
    public DbSet<Inquiry> Inquiries { get; set; }
    public DbSet<MasterFashionItem> MasterFashionItems { get; set; }
    
    public DbSet<IndividualFashionItem> IndividualFashionItems { get; set; }
    public DbSet<IndividualConsignedForSaleFashionItem> IndividualConsignedForSaleFashionItems { get; set; }
    public DbSet<IndividualAuctionFashionItem> IndividualAuctionFashionItems { get; set; }
    public DbSet<ConsignSale> ConsignSales { get; set; }
    public DbSet<ConsignSaleLineItem> ConsignSaleLineItems { get; set; }
    public DbSet<Shop> Shops { get; set; }
    public DbSet<Withdraw> Withdraws { get; set; }
    public DbSet<BankAccount> BankAccounts { get; set; }
    public DbSet<Image> Images { get; set; }
    public DbSet<Auction> Auctions { get; set; }
    public DbSet<Address> Addresses { get; set; }
    public DbSet<OrderLineItem> OrderLineItems { get; set; }
    public DbSet<Member> Members { get; set; }
    public DbSet<Staff> Staffs { get; set; }
    public DbSet<Admin> Admins { get; set; }
    public DbSet<Recharge> Recharges { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<Bid> Bids { get; set; }
    public DbSet<AuctionDeposit> AuctionDeposits { get; set; }
    public DbSet<Refund> Refunds { get; set; }
    public DbSet<Feedback> Feedbacks { get; set; }


    public GiveAwayDbContext(DbContextOptions<GiveAwayDbContext> options) : base(options)
    {
    }


    public GiveAwayDbContext()
    {
    }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(GetConnectionString(), builder => builder.UseNetTopologySuite());
        optionsBuilder.LogTo(Console.WriteLine);
        optionsBuilder.EnableSensitiveDataLogging();
    }

    private static string? GetConnectionString()
    {
        var envConnectionString = Environment.GetEnvironmentVariable("ASPNETCORE_ConnectionStrings__DefaultDB");

        if(!string.IsNullOrEmpty(envConnectionString))
        {
            return envConnectionString;
        }

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        return configuration.GetConnectionString("DeployDB");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        #region TimeConfig

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var dateTimeProperties = entityType.GetProperties().Where(p => p.ClrType == typeof(DateTime));
            foreach (var property in dateTimeProperties)
            {
                property.SetValueConverter(new DateTimeToUtcConverter());
            }
        }

        #endregion


        #region Account

        modelBuilder.Entity<Account>()
            .ToTable("Account")
            .HasKey(x => x.AccountId);

        modelBuilder.Entity<Account>().HasDiscriminator(x => x.Role)
            .HasValue<Account>(Roles.Account)
            .HasValue<Member>(Roles.Member)
            .HasValue<Staff>(Roles.Staff)
            .HasValue<Admin>(Roles.Admin);

        modelBuilder.Entity<Account>()
            .Property(e => e.Email).HasColumnType("varchar").HasMaxLength(50);

        modelBuilder.Entity<Account>()
            .Property(e => e.Fullname).HasColumnType("varchar").HasMaxLength(100);

        modelBuilder.Entity<Account>().Property(e => e.Role).HasColumnType("varchar").HasMaxLength(20);

        modelBuilder.Entity<Account>().Property(e => e.Status)
            .HasConversion(prop => prop.ToString(), s => (AccountStatus)Enum.Parse(typeof(AccountStatus), s))
            .HasColumnType("varchar").HasMaxLength(20);

        modelBuilder.Entity<Account>()
            .Property(x => x.Role)
            .HasConversion(prop => prop.ToString(), s => (Roles)Enum.Parse(typeof(Roles), s))
            .HasColumnType("varchar").HasMaxLength(20);

        modelBuilder.Entity<Account>().Property(e => e.Phone).HasColumnType("varchar").HasMaxLength(10);
        modelBuilder.Entity<Account>().HasIndex(x => x.Email).IsUnique();
        modelBuilder.Entity<Account>().HasIndex(x => x.Phone).IsUnique();
        modelBuilder.Entity<Account>()
            .HasMany(x => x.SentTransactions)
            .WithOne(x => x.Sender)
            .HasForeignKey(x => x.SenderId);
        modelBuilder.Entity<Account>()
            .HasMany(x => x.ReceivedTransactions)
            .WithOne(x => x.Receiver)
            .HasForeignKey(x => x.ReceiverId);
        #endregion

        #region Withdraw

        modelBuilder.Entity<Withdraw>()
            .HasOne(x => x.Transaction)
            .WithOne(x => x.Withdraw)
            .HasForeignKey<Transaction>(x => x.WithdrawId);

        modelBuilder.Entity<Withdraw>()
            .Property(x => x.Status)
            .HasConversion(prop => prop.ToString(), s => (WithdrawStatus)Enum.Parse(typeof(WithdrawStatus), s))
            .HasColumnType("varchar").HasMaxLength(20);

        #endregion

        #region Member

        modelBuilder.Entity<Member>()
            .HasMany(x => x.Bids)
            .WithOne(x => x.Member)
            .HasForeignKey(x => x.MemberId);


        


        modelBuilder.Entity<Member>()
            .HasMany(x => x.AuctionDeposits)
            .WithOne(x => x.Member)
            .HasForeignKey(x => x.MemberId);

        modelBuilder.Entity<Member>()
            .HasMany(x => x.BankAccounts)
            .WithOne(x => x.Member)
            .HasForeignKey(x => x.MemberId);
        modelBuilder.Entity<Member>()
            .HasMany(x => x.Addresses)
            .WithOne(x => x.Member)
            .HasForeignKey(x => x.MemberId);

        #endregion

        #region Auction

        modelBuilder.Entity<Auction>()
            .ToTable("Auction")
            .HasKey(x => x.AuctionId);

        modelBuilder.Entity<Auction>()
            .Property(e => e.StartDate).HasColumnType("timestamptz");
        modelBuilder.Entity<Auction>()
            .Property(e => e.EndDate).HasColumnType("timestamptz");
        modelBuilder.Entity<Auction>()
            .Property(e => e.Status)
            .HasConversion(
                prop => prop.ToString(),
                v => (AuctionStatus)Enum.Parse(typeof(AuctionStatus), v))
            .HasColumnType("varchar").HasMaxLength(20);
        modelBuilder.Entity<Auction>()
            .Property(e => e.Title).HasColumnType("varchar").HasMaxLength(100);
        modelBuilder.Entity<Auction>().Property(e => e.Status).HasColumnType("varchar").HasMaxLength(20);
        modelBuilder.Entity<Auction>()
            .HasMany(x => x.Bids)
            .WithOne(x => x.Auction)
            .HasForeignKey(x => x.AuctionId);
        modelBuilder.Entity<Auction>()
            .HasMany(x=>x.AuctionDeposits)
            .WithOne(x=>x.Auction)
            .HasForeignKey(x=>x.AuctionId);

        #endregion

        #region AuctionDeposit

        modelBuilder.Entity<AuctionDeposit>()
            .ToTable("AuctionDeposit")
            .HasKey(x => x.AuctionDepositId);
        
        modelBuilder.Entity<AuctionDeposit>()
            .HasMany(x=>x.Transactions)
            .WithOne(x=>x.AuctionDeposit)
            .HasForeignKey(x=>x.AuctionDepositId);

        modelBuilder.Entity<AuctionDeposit>().Property(e => e.CreatedDate).HasColumnType("timestamptz")
            .ValueGeneratedOnAdd();

        #endregion

        #region FashionAuctionItem

        #region MasterItem

        modelBuilder.Entity<MasterFashionItem>().HasKey(x => x.MasterItemId);

        

        

        modelBuilder.Entity<MasterFashionItem>()
            .Property(x => x.Gender)
            .HasConversion(prop => prop.ToString(), s => (GenderType)Enum.Parse(typeof(GenderType), s))
            .HasColumnType("varchar").HasMaxLength(20);

        modelBuilder.Entity<MasterFashionItem>()
            .HasMany(x => x.Images)
            .WithOne(x => x.MasterFashionItem)
            .HasForeignKey(x => x.MasterFashionItemId);

        modelBuilder.Entity<MasterFashionItem>()
            .HasMany(x => x.IndividualFashionItems)
            .WithOne(x => x.MasterItem)
            .HasForeignKey(x => x.MasterItemId);
        
        modelBuilder.Entity<MasterFashionItem>()
            .Property(x=>x.MasterItemCode)
            .HasColumnType("varchar").HasMaxLength(20);

        modelBuilder.Entity<MasterFashionItem>()
            .Property(x => x.Name)
            .HasColumnType("varchar")
            .HasMaxLength(100);

        modelBuilder.Entity<MasterFashionItem>()
            .Property(x => x.Brand)
            .HasColumnType("varchar")
            .HasMaxLength(50);
        
        #endregion

        // #region Variation
        //
        // modelBuilder.Entity<FashionItemVariation>().HasKey(x => x.VariationId);
        //
        
        //
        // modelBuilder.Entity<FashionItemVariation>()
        //     .HasMany(x => x.IndividualItems)
        //     .WithOne(x => x.Variation)
        //     .HasForeignKey(x => x.VariationId);
        //
        // modelBuilder.Entity<FashionItemVariation>()
        //     .Property(x => x.Condition)
        //     .HasColumnType("varchar")
        //     .HasMaxLength(30);
        //
        // #endregion

        modelBuilder.Entity<IndividualFashionItem>().HasKey(x => x.ItemId);


        modelBuilder.Entity<IndividualFashionItem>()
            .HasDiscriminator(x => x.Type)
            .HasValue<IndividualFashionItem>(FashionItemType.ItemBase)
            .HasValue<IndividualAuctionFashionItem>(FashionItemType.ConsignedForAuction)
            .HasValue<IndividualConsignedForSaleFashionItem>(FashionItemType.ConsignedForSale)
            .HasValue<IndividualCustomerSaleFashionItem>(FashionItemType.CustomerSale);

        modelBuilder.Entity<IndividualFashionItem>()
            .HasIndex(x => x.ItemCode)
            .IsUnique();
        
        modelBuilder.Entity<IndividualFashionItem>()
            .Property(x=>x.Type)
            .HasConversion(prop => prop.ToString(), s => (FashionItemType)Enum.Parse(typeof(FashionItemType), s))
            .HasColumnType("varchar").HasMaxLength(20);
        modelBuilder.Entity<IndividualFashionItem>()
            .Property(x=>x.Status)
            .HasConversion(prop => prop.ToString(), s => (FashionItemStatus)Enum.Parse(typeof(FashionItemStatus), s))
            .HasColumnType("varchar").HasMaxLength(20);
        modelBuilder.Entity<IndividualFashionItem>()
            .HasMany(x=>x.Images)
            .WithOne(x=>x.IndividualFashionItem)
            .HasForeignKey(x=>x.IndividualFashionItemId);

        modelBuilder.Entity<IndividualAuctionFashionItem>()
            .HasMany(x => x.Auctions)
            .WithOne(x => x.IndividualAuctionFashionItem)
            .HasForeignKey(x => x.IndividualAuctionFashionItemId);
        modelBuilder.Entity<IndividualFashionItem>()
        .Property(x => x.Condition)
        .HasColumnType("varchar")
        .HasMaxLength(30);
        modelBuilder.Entity<IndividualFashionItem>()
            .Property(x => x.Size)
            .HasConversion(prop => prop.ToString(), s => (SizeType)Enum.Parse(typeof(SizeType), s))
            .HasColumnType("varchar").HasMaxLength(20);
        modelBuilder.Entity<IndividualFashionItem>()
            .Property(x => x.Color)
            .HasColumnType("varchar")
            .HasMaxLength(30);
        #endregion

        #region Bid

        modelBuilder.Entity<Bid>().ToTable("Bid").HasKey(e => e.BidId);
        modelBuilder.Entity<Bid>().Property(e => e.CreatedDate).HasColumnType("timestamptz").ValueGeneratedOnAdd();

        modelBuilder.Entity<Bid>()
            .HasOne(x => x.Order)
            .WithOne(x => x.Bid)
            .HasForeignKey<Order>(x => x.BidId);

        #endregion

        #region Category

        modelBuilder.Entity<Category>().ToTable("Category").HasKey(e => e.CategoryId);
        modelBuilder.Entity<Category>().Property(e => e.Name).HasColumnType("varchar").HasMaxLength(50);

        modelBuilder.Entity<Category>().Property(e => e.Level).HasDefaultValue(1);
        modelBuilder.Entity<Category>().Property(e => e.Status).HasConversion(prop => prop.ToString(), s =>
                (CategoryStatus)Enum.Parse(typeof(CategoryStatus), s))
            .HasColumnType("varchar").HasMaxLength(20);

        #endregion

        #region Address

        modelBuilder.Entity<Address>().ToTable("Address").HasKey(e => e.AddressId);
        modelBuilder.Entity<Address>().Property(e => e.RecipientName).HasColumnType("varchar").HasMaxLength(50);
        modelBuilder.Entity<Address>().Property(e => e.Phone).HasColumnType("varchar").HasMaxLength(10);
        modelBuilder.Entity<Address>().Property(e => e.Residence).HasColumnType("varchar").HasMaxLength(100);

        modelBuilder.Entity<Address>().Property(e => e.AddressType)
            .HasConversion(prop => prop.ToString(),
                s => (AddressType)Enum.Parse(typeof(AddressType), s))
            .HasColumnType("varchar").HasMaxLength(20);

        #endregion

        #region Image

        modelBuilder.Entity<Image>().ToTable("Image").HasKey(e => e.ImageId);

        #endregion

        #region Inquiry

        modelBuilder.Entity<Inquiry>().ToTable("Inquiry").HasKey(e => e.InquiryId);
        modelBuilder.Entity<Inquiry>().Property(e => e.CreatedDate).HasColumnType("timestamptz").ValueGeneratedOnAdd();
        modelBuilder.Entity<Inquiry>().Property(e => e.Status)
            .HasConversion(prop => prop.ToString(), s => (InquiryStatus)Enum.Parse(typeof(InquiryStatus), s))
            .HasColumnType("varchar").HasMaxLength(20);

        #endregion

      

        #region Order

        modelBuilder.Entity<Order>().ToTable("Order").HasKey(e => e.OrderId);
        modelBuilder.Entity<Order>().HasIndex(x => x.OrderCode).IsUnique();
        modelBuilder.Entity<Order>().Property(e => e.CreatedDate).HasColumnType("timestamptz").ValueGeneratedOnAdd();
        modelBuilder.Entity<Order>().Property(e => e.PaymentMethod).HasColumnType("varchar").HasMaxLength(20);
        
        modelBuilder.Entity<Order>()
            .HasMany(x => x.OrderLineItems)
            .WithOne(x => x.Order)
            .HasForeignKey(x => x.OrderId);

        modelBuilder.Entity<Order>().Property(e => e.PurchaseType)
            .HasConversion(prop => prop.ToString(), s => (PurchaseType)Enum.Parse(typeof(PurchaseType), s))
            .HasColumnType("varchar").HasMaxLength(20);

        modelBuilder.Entity<Order>().Property(e => e.Status).HasConversion(prop => prop.ToString(),
                s => (OrderStatus)Enum.Parse(typeof(OrderStatus), s))
            .HasColumnType("varchar").HasMaxLength(20);

        modelBuilder.Entity<Order>()
            .Property(x => x.PaymentMethod)
            .HasConversion(prop => prop.ToString(), s => (PaymentMethod)Enum.Parse(typeof(PaymentMethod), s))
            .HasColumnType("varchar").HasMaxLength(30);

        modelBuilder.Entity<Order>().HasMany(x => x.Transaction).WithOne(x => x.Order)
            .HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Order>().Property(x => x.Address)
            .HasColumnType("varchar").HasMaxLength(255);
        modelBuilder.Entity<Order>().Property(x => x.RecipientName).HasColumnType("varchar").HasMaxLength(100);
        modelBuilder.Entity<Order>().Property(x => x.Phone).HasColumnType("varchar").HasMaxLength(20);
        modelBuilder.Entity<Order>().Property(x => x.Email).HasColumnType("varchar").HasMaxLength(50)
            .IsRequired(false);
        modelBuilder.Entity<Order>().Property(e => e.AddressType).HasConversion(prop => prop.ToString(),
                s => (AddressType)Enum.Parse(typeof(AddressType), s))
            .HasColumnType("varchar").HasMaxLength(30);
        modelBuilder.Entity<Order>()
            .HasOne<Feedback>(x => x.Feedback)
            .WithOne(x => x.Order)
            .HasForeignKey<Feedback>(x => x.OrderId);
        #endregion

        #region OrderDetail

        modelBuilder.Entity<OrderLineItem>().ToTable("OrderLineItem").HasKey(e => e.OrderLineItemId);
        modelBuilder.Entity<OrderLineItem>().Property(e => e.PaymentDate).HasColumnType("timestamptz");
        modelBuilder.Entity<OrderLineItem>()
            .HasOne<Refund>(x => x.Refund)
            .WithOne(x => x.OrderLineItem)
            .HasForeignKey<Refund>(x => x.OrderLineItemId);
        modelBuilder.Entity<OrderLineItem>().Property(e => e.ReservedExpirationDate).HasColumnType("timestamptz")
            .IsRequired(false);

        #endregion

        #region ConsignSale

        modelBuilder.Entity<ConsignSale>().ToTable("ConsignSale").HasKey(e => e.ConsignSaleId);

        modelBuilder.Entity<ConsignSale>()
            .HasIndex(x => x.ConsignSaleCode)
            .IsUnique();

        modelBuilder.Entity<ConsignSale>().Property(e => e.CreatedDate).HasColumnType("timestamptz")
            .ValueGeneratedOnAdd();

        modelBuilder.Entity<ConsignSale>().Property(e => e.Status)
            .HasConversion(prop => prop.ToString(), s => (ConsignSaleStatus)Enum.Parse(typeof(ConsignSaleStatus), s))
            .HasColumnType("varchar").HasMaxLength(20);

        modelBuilder.Entity<ConsignSale>().Property(e => e.Type)
            .HasConversion(prop => prop.ToString(),
                s => (ConsignSaleType)Enum.Parse(typeof(ConsignSaleType), s)).HasColumnType("varchar").HasMaxLength(50);

        modelBuilder.Entity<ConsignSale>().Property(e => e.StartDate).HasColumnType("timestamptz").IsRequired(false);

        modelBuilder.Entity<ConsignSale>().Property(e => e.EndDate).HasColumnType("timestamptz").IsRequired(false);

        modelBuilder.Entity<ConsignSale>().HasMany(x => x.ConsignSaleLineItems).WithOne(x => x.ConsignSale)
            .HasForeignKey(x => x.ConsignSaleId);
        modelBuilder.Entity<ConsignSale>().Property(x => x.Address).HasColumnType("varchar").HasMaxLength(255);
        modelBuilder.Entity<ConsignSale>().Property(x => x.ConsignorName).HasColumnType("varchar").HasMaxLength(100);
        modelBuilder.Entity<ConsignSale>().Property(x => x.Phone).HasColumnType("varchar").HasMaxLength(20);
        modelBuilder.Entity<ConsignSale>().Property(x => x.Email).HasColumnType("varchar").HasMaxLength(50);
        modelBuilder.Entity<ConsignSale>().Property(e => e.ConsignSaleMethod)
            .HasConversion(prop => prop.ToString(), s => (ConsignSaleMethod)Enum.Parse(typeof(ConsignSaleMethod), s))
            .HasColumnType("varchar").HasMaxLength(20);
        modelBuilder.Entity<ConsignSale>()
            .Property(x => x.ResponseFromShop)
            .HasColumnType("text");
        #endregion

        #region ConsignedSaleLineItem

        modelBuilder.Entity<ConsignSaleLineItem>().ToTable("ConsignSaleLineItem").HasKey(x => x.ConsignSaleLineItemId);
        
        modelBuilder.Entity<ConsignSaleLineItem>()
            .HasOne<IndividualFashionItem>(x=>x.IndividualFashionItem)
            .WithOne(x=>x.ConsignSaleLineItem)
            .HasForeignKey<IndividualFashionItem>(x=>x.ConsignSaleLineItemId);
        
        modelBuilder.Entity<ConsignSaleLineItem>()
            .HasMany(x=>x.Images)
            .WithOne(x=>x.ConsignSaleLineItem)
            .HasForeignKey(x=>x.ConsignLineItemId);

        modelBuilder.Entity<ConsignSaleLineItem>()
            .Property(x => x.Size)
            .HasConversion(prop => prop.ToString(), s => (SizeType)Enum.Parse(typeof(SizeType), s))
            .HasColumnType("varchar")
            .HasMaxLength(15);
        
        modelBuilder.Entity<ConsignSaleLineItem>()
            .Property(x=>x.Gender)
            .HasConversion(prop => prop.ToString(), s => (GenderType)Enum.Parse(typeof(GenderType), s))
            .HasColumnType("varchar")
            .HasMaxLength(15);
        
        modelBuilder.Entity<ConsignSaleLineItem>()
            .Property(x => x.ProductName)
            .HasColumnType("varchar")
            .HasMaxLength(60);

        modelBuilder.Entity<ConsignSaleLineItem>()
            .Property(x => x.Brand)
            .HasColumnType("varchar")
            .HasMaxLength(30);

        modelBuilder.Entity<ConsignSaleLineItem>()
            .Property(x => x.Color)
            .HasColumnType("varchar")
            .HasMaxLength(30);
        
        modelBuilder.Entity<ConsignSaleLineItem>()
            .Property(x=>x.Condition)
            .HasColumnType("varchar")
            .HasMaxLength(20);
        
        modelBuilder.Entity<ConsignSaleLineItem>()
            .Property(x=>x.Status)
            .HasConversion(prop => prop.ToString(), s => (ConsignSaleLineItemStatus)Enum.Parse(typeof(ConsignSaleLineItemStatus), s))
            .HasColumnType("varchar").HasMaxLength(20);

        modelBuilder.Entity<ConsignSaleLineItem>()
            .Property(x => x.ResponseFromShop)
            .HasColumnType("text");
            
        #endregion


        #region Transaction

        modelBuilder.Entity<Transaction>().ToTable("Transaction").HasKey(e => e.TransactionId);

        modelBuilder.Entity<Transaction>().Property(e => e.CreatedDate).HasColumnType("timestamptz")
            .ValueGeneratedOnAdd();

        modelBuilder.Entity<Transaction>().Property(e => e.Type)
            .HasConversion(prop => prop.ToString(),
                s => (TransactionType)Enum.Parse(typeof(TransactionType), s)).HasColumnType("varchar").HasMaxLength(20);
        modelBuilder.Entity<Transaction>()
            .Property(x => x.PaymentMethod)
            .HasConversion(prop => prop.ToString(), s => (PaymentMethod)Enum.Parse(typeof(PaymentMethod), s))
            .HasColumnType("varchar").HasMaxLength(30);
        #endregion

        #region Shop

        modelBuilder.Entity<Shop>()
            .Property(x => x.Location)
            .HasColumnType("geography(Point)");
        modelBuilder.Entity<Shop>()
            .HasMany(x => x.MasterFashionItems)
            .WithOne(x => x.Shop)
            .HasForeignKey(x => x.ShopId);
        modelBuilder.Entity<Shop>()
            .HasMany(x => x.Transactions)
            .WithOne(x => x.Shop)
            .HasForeignKey(x => x.ShopId);
        #endregion

        #region Recharge

        modelBuilder.Entity<Recharge>().ToTable("Recharge").HasKey(e => e.RechargeId);

        modelBuilder.Entity<Recharge>().Property(e => e.Status)
            .HasConversion(prop => prop.ToString(),
                s => (RechargeStatus)Enum.Parse(typeof(RechargeStatus), s)
            ).HasColumnType("varchar")
            .HasMaxLength(20);
        
        modelBuilder.Entity<Recharge>()
            .Property(x=>x.PaymentMethod)
            .HasConversion(prop => prop.ToString(),
                s => (PaymentMethod)Enum.Parse(typeof(PaymentMethod), s)
            ).HasColumnType("varchar")
            .HasMaxLength(20);

        modelBuilder.Entity<Recharge>()
            .HasOne<Transaction>(x => x.Transaction)
            .WithOne(x => x.Recharge)
            .HasForeignKey<Transaction>(x => x.RechargeId);

        #endregion

        #region Refund

        modelBuilder.Entity<Refund>().ToTable("Refund").HasKey(e => e.RefundId);

        modelBuilder.Entity<Refund>().Property(e => e.RefundStatus)
            .HasConversion(prop => prop.ToString(),
                s => (RefundStatus)Enum.Parse(typeof(RefundStatus), s)
            ).HasColumnType("varchar")
            .HasMaxLength(20);
        modelBuilder.Entity<Refund>().Property(e => e.RefundPercentage).HasColumnType("numeric").HasMaxLength(100);
        modelBuilder.Entity<Refund>().Property(e => e.ResponseFromShop).HasColumnType("varchar").HasMaxLength(255);
        modelBuilder.Entity<Refund>().HasMany(x => x.Images)
            .WithOne(x => x.Refund)
            .HasForeignKey(x => x.RefundId)
            .OnDelete(DeleteBehavior.Cascade);

        #endregion

        #region Feedback

        modelBuilder.Entity<Feedback>()
            .ToTable("Feedback")
            .HasKey(x => x.FeedbackId);

        #endregion
    }
}

public class DateTimeToUtcConverter : ValueConverter<DateTime, DateTime>
{
    public DateTimeToUtcConverter() : base(
        d => d.ToUniversalTime(),
        d => DateTime.SpecifyKind(d, DateTimeKind.Utc))
    {
    }
}