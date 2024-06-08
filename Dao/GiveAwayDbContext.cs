using BusinessObjects.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Dao;

public class GiveAwayDbContext : DbContext
{
    public DbSet<Account> Accounts { get; set; }
    public DbSet<Inquiry> Inquiries { get; set; }
    public DbSet<Request> Requests { get; set; }
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
            .AddJsonFile("appsettings.Development.json")
            .Build();

        return configuration.GetConnectionString("DefaultDB");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
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
            .HasKey(x => x.ActionId);

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
        modelBuilder.Entity<Delivery>().Property(e => e.PhoneNumeber).HasColumnType("varchar").HasMaxLength(10);
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
        modelBuilder.Entity<FashionItem>().Property(e => e.SellingPrice).HasColumnType("numeric")
            .HasPrecision(10, 2);
        modelBuilder.Entity<FashionItem>().Property(e => e.Note).HasColumnType("varchar").HasMaxLength(100);
        modelBuilder.Entity<FashionItem>().Property(e => e.Value).HasColumnType("numeric");
        modelBuilder.Entity<FashionItem>().Property(e => e.Status).HasColumnType("varchar").HasMaxLength(20);

        #endregion

        #region Order

        modelBuilder.Entity<Order>().ToTable("Order").HasKey(e => e.OrderId);

        modelBuilder.Entity<Order>().Property(e => e.CreatedDate).HasColumnType("timestamptz").ValueGeneratedOnAdd();
        modelBuilder.Entity<Order>().Property(e => e.PaymentMethod).HasColumnType("varchar").HasMaxLength(20);
        modelBuilder.Entity<Order>().Property(e => e.PaymentDate).HasColumnType("timestamptz");

        modelBuilder.Entity<Order>().HasOne(x => x.Transaction).WithOne(x => x.Order)
            .HasForeignKey<Transaction>(x => x.OrderId);

        #endregion

        #region OrderDetail

        modelBuilder.Entity<OrderDetail>().ToTable("OrderDetail").HasKey(e => e.OrderDetailId);
        modelBuilder.Entity<OrderDetail>().Property(e => e.UnitPrice).HasColumnType("numberic").HasPrecision(10, 2);

       

        #endregion

        #region Request

        modelBuilder.Entity<Request>().ToTable("Request").HasKey(e => e.RequestId);

        modelBuilder.Entity<Request>().Property(e => e.CreatedDate).HasColumnType("timestamptz").ValueGeneratedOnAdd();
        modelBuilder.Entity<Request>().Property(e => e.Status).HasColumnType("varchar").HasMaxLength(20);
        modelBuilder.Entity<Request>().Property(e => e.Type).HasColumnType("varchar").HasMaxLength(50);
        modelBuilder.Entity<Request>().Property(e => e.StartDate).HasColumnType("timestamptz").IsRequired(false);
        modelBuilder.Entity<Request>().Property(e => e.EndDate).HasColumnType("timestamptz").IsRequired(false);

        modelBuilder.Entity<Request>().HasOne(x => x.OrderDetail).WithOne(x => x.Request).HasForeignKey<OrderDetail>(x=>x.RequestId);

        #endregion

        #region Schedule

        modelBuilder.Entity<Schedule>().ToTable("Schedule").HasKey(e => e.ScheduleId);

        #endregion

        #region Timeslot

        modelBuilder.Entity<Timeslot>().ToTable("Timeslot").HasKey(e => e.TimeslotId);

        #endregion

        #region Transaction

        modelBuilder.Entity<Transaction>().ToTable("Transaction").HasKey(e => e.TransactionId);
        modelBuilder.Entity<Transaction>().Property(e => e.Amount).HasColumnType("numeric").HasPrecision(10, 2);    
        modelBuilder.Entity<Transaction>().Property(e => e.CreatedDate).HasColumnType("timestamptz")
            .ValueGeneratedOnAdd();
        modelBuilder.Entity<Transaction>().Property(e => e.Type).HasColumnType("varchar").HasMaxLength(20);
        modelBuilder.Entity<Transaction>().HasOne(x => x.AuctionDeposit).WithOne(x => x.Transaction)
            .HasForeignKey<AuctionDeposit>(x => x.TransactionId);

        #endregion


        #region Wallet

        modelBuilder.Entity<Wallet>().ToTable("Wallet").HasKey(e => e.WalletId);

        #endregion

        #region PointPackage

        modelBuilder.Entity<PointPackage>().ToTable("PointPackage").HasKey(e => e.PointPackageId);

        #endregion
    }
}