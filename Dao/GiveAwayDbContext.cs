using BusinessObjects.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Configuration;

namespace Dao;

public class GiveAwayDbContext : DbContext
{
    public DbSet<Account> Accounts { get; set; }
    public DbSet<Inquiry> Inquiries { get; set; }
    public DbSet<ConsignSale> ConsignSales { get; set; }
    public DbSet<ConsignSaleDetail> ConsignSaleDetails { get; set; }
    public DbSet<Shop> Shops { get; set; }
    public DbSet<FashionItem> FashionItems { get; set; }
    public DbSet<Image> Images { get; set; }
    public DbSet<Auction> Auctions { get; set; }
    public DbSet<Schedule> Schedules { get; set; }
    public DbSet<Delivery> Deliveries { get; set; }
    public DbSet<OrderDetail> OrderDetails { get; set; }
    public DbSet<AuctionFashionItem> AuctionFashionItems { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<Timeslot> TimeSlots { get; set; }
    public DbSet<Wallet> Wallets { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<Bid> Bids { get; set; }
    public DbSet<AuctionDeposit> AuctionDeposits { get; set; }

    public GiveAwayDbContext()
    {
    }

    public GiveAwayDbContext(DbContextOptions<GiveAwayDbContext> options) : base(options)
    {
    }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(GetConnectionString());
    }

    private string? GetConnectionString()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        return configuration.GetConnectionString("DefaultDB");
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
        modelBuilder.Entity<Account>()
            .Property(e => e.Email).HasColumnType("varchar").HasMaxLength(50);
        modelBuilder.Entity<Account>()
            .Property(e => e.Fullname).HasColumnType("varchar").HasMaxLength(100);
        modelBuilder.Entity<Account>().Property(e => e.Role).HasColumnType("varchar").HasMaxLength(20);
        modelBuilder.Entity<Account>().Property(e => e.Status).HasColumnType("varchar").HasMaxLength(20);
        modelBuilder.Entity<Account>().Property(e => e.Phone).HasColumnType("varchar").HasMaxLength(10);

        modelBuilder.Entity<Account>().HasIndex(x => x.Email).IsUnique();
        modelBuilder.Entity<Account>().HasIndex(x => x.Phone).IsUnique();

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
            .Property(e => e.Status).HasColumnType("varchar").HasMaxLength(20);
        modelBuilder.Entity<Auction>()
            .Property(e => e.Title).HasColumnType("varchar").HasMaxLength(100);
        modelBuilder.Entity<Auction>().Property(e => e.Status).HasColumnType("varchar").HasMaxLength(20);

        #endregion

        #region AuctionDeposit

        modelBuilder.Entity<AuctionDeposit>()
            .ToTable("AuctionDeposit")
            .HasKey(x => x.AuctionDepositId);

        modelBuilder.Entity<AuctionDeposit>().Property(e => e.CreatedDate).HasColumnType("timestamptz")
            .ValueGeneratedOnAdd();

        #endregion

        #region FashionAuctionItem

        modelBuilder.Entity<AuctionFashionItem>().HasMany(x => x.Auctions).WithOne(x => x.AuctionFashionItem)
            .HasForeignKey(x => x.AuctionFashionItemId);

        #endregion

        #region Bid

        modelBuilder.Entity<Bid>().ToTable("Bid").HasKey(e => e.BidId);
        modelBuilder.Entity<Bid>().Property(e => e.CreatedDate).HasColumnType("timestamptz").ValueGeneratedOnAdd();

        #endregion

        #region Category

        modelBuilder.Entity<Category>().ToTable("Category").HasKey(e => e.CategoryId);
        modelBuilder.Entity<Category>().Property(e => e.Name).HasColumnType("varchar").HasMaxLength(50);

        modelBuilder.Entity<Category>().HasIndex(x => x.Name).IsUnique();

        #endregion

        #region Delivery

        modelBuilder.Entity<Delivery>().ToTable("Delivery").HasKey(e => e.DeliveryId);
        modelBuilder.Entity<Delivery>().Property(e => e.RecipientName).HasColumnType("varchar").HasMaxLength(50);
        modelBuilder.Entity<Delivery>().Property(e => e.Phone).HasColumnType("varchar").HasMaxLength(10);
        modelBuilder.Entity<Delivery>().Property(e => e.Address).HasColumnType("varchar").HasMaxLength(100);

        #endregion

        #region Image

        modelBuilder.Entity<Image>().ToTable("Image").HasKey(e => e.ImageId);

        #endregion

        #region Inquiry

        modelBuilder.Entity<Inquiry>().ToTable("Inquiry").HasKey(e => e.InquiryId);
        modelBuilder.Entity<Inquiry>().Property(e => e.CreatedDate).HasColumnType("timestamptz").ValueGeneratedOnAdd();
        modelBuilder.Entity<Inquiry>().Property(e => e.Fullname).HasColumnType("varchar").HasMaxLength(50);
        modelBuilder.Entity<Inquiry>().Property(e => e.Phone).HasColumnType("varchar").HasMaxLength(10);

        #endregion

        #region FashionItem

        modelBuilder.Entity<FashionItem>().ToTable("FashionItem").HasKey(e => e.ItemId);
        modelBuilder.Entity<FashionItem>().HasDiscriminator(e => e.Type)
            .HasValue<FashionItem>("ItemBase")
            .HasValue<ConsignedForSaleFashionItem>("ConsignedForSale")
            .HasValue<AuctionFashionItem>("ConsignedForAuction");
        modelBuilder.Entity<FashionItem>().Property(e => e.Name).HasColumnType("varchar").HasMaxLength(50);
        modelBuilder.Entity<FashionItem>().Property(e => e.Note).HasColumnType("varchar").HasMaxLength(100);
        modelBuilder.Entity<FashionItem>().Property(e => e.Value).HasColumnType("numeric");
        modelBuilder.Entity<FashionItem>().Property(e => e.Status).HasColumnType("varchar").HasMaxLength(20);

        modelBuilder.Entity<FashionItem>().HasOne(x => x.ConsignSaleDetail).WithOne(x => x.FashionItem)
            .HasForeignKey<ConsignSaleDetail>(x => x.FashionItemId).OnDelete(DeleteBehavior.Cascade);
        #endregion

        #region Order

        modelBuilder.Entity<Order>().ToTable("Order").HasKey(e => e.OrderId);

        modelBuilder.Entity<Order>().Property(e => e.CreatedDate).HasColumnType("timestamptz").ValueGeneratedOnAdd();
        modelBuilder.Entity<Order>().Property(e => e.PaymentMethod).HasColumnType("varchar").HasMaxLength(20);
        modelBuilder.Entity<Order>().Property(e => e.PaymentDate).HasColumnType("timestamptz");

        modelBuilder.Entity<Order>().HasOne(x => x.Transaction).WithOne(x => x.Order)
            .HasForeignKey<Transaction>(x => x.OrderId).OnDelete(DeleteBehavior.Cascade);

        #endregion

        #region OrderDetail

        modelBuilder.Entity<OrderDetail>().ToTable("OrderDetail").HasKey(e => e.OrderDetailId);

        #endregion

        #region ConsignSale

        modelBuilder.Entity<ConsignSale>().ToTable("ConsignSale").HasKey(e => e.ConsignSaleId);

        modelBuilder.Entity<ConsignSale>().Property(e => e.CreatedDate).HasColumnType("timestamptz")
            .ValueGeneratedOnAdd();
        modelBuilder.Entity<ConsignSale>().Property(e => e.Status).HasColumnType("varchar").HasMaxLength(20);
        modelBuilder.Entity<ConsignSale>().Property(e => e.Type).HasColumnType("varchar").HasMaxLength(50);
        modelBuilder.Entity<ConsignSale>().Property(e => e.StartDate).HasColumnType("timestamptz").IsRequired(false);
        modelBuilder.Entity<ConsignSale>().Property(e => e.EndDate).HasColumnType("timestamptz").IsRequired(false);

        modelBuilder.Entity<ConsignSale>().HasMany(x => x.ConsignSaleDetails).WithOne(x => x.ConsignSale)
            .HasForeignKey(x => x.ConsignSaleId);

        #endregion

        #region ConsignedSaleDetail

        modelBuilder.Entity<ConsignSaleDetail>().ToTable("ConsignSaleDetail").HasKey(x => x.ConsignSaleDetailId); 
        

        #endregion

        #region Schedule

        modelBuilder.Entity<Schedule>().ToTable("Schedule").HasKey(e => e.ScheduleId);

        #endregion

        #region Timeslot

        modelBuilder.Entity<Timeslot>().ToTable("Timeslot").HasKey(e => e.TimeslotId);

        #endregion

        #region Transaction

        modelBuilder.Entity<Transaction>().ToTable("Transaction").HasKey(e => e.TransactionId);
        modelBuilder.Entity<Transaction>().Property(e => e.CreatedDate).HasColumnType("timestamptz")
            .ValueGeneratedOnAdd();
        modelBuilder.Entity<Transaction>().Property(e => e.Type).HasColumnType("varchar").HasMaxLength(20);
        modelBuilder.Entity<Transaction>().HasOne(x => x.AuctionDeposit).WithOne(x => x.Transaction)
            .HasForeignKey<AuctionDeposit>(x => x.TransactionId).OnDelete(DeleteBehavior.Cascade);

        #endregion


        #region Wallet

        modelBuilder.Entity<Wallet>().ToTable("Wallet").HasKey(e => e.WalletId);
        modelBuilder.Entity<Wallet>().Property(e => e.BankAccountNumber).HasColumnType("varchar").HasMaxLength(20);
        modelBuilder.Entity<Wallet>().Property(e => e.BankName).HasColumnType("varchar").HasMaxLength(100);

        #endregion

        #region PointPackage

        modelBuilder.Entity<PointPackage>().ToTable("PointPackage").HasKey(e => e.PointPackageId);

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