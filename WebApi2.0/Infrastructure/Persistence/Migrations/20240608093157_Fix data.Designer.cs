﻿// <auto-generated />
using System;
using WebApi2._0.Infrastructure.Persistence.Migrations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WebApi2._0.Infrastructure.Persistence.Migrations.Migrations
{
    [DbContext(typeof(GiveAwayDbContext))]
    [Migration("20240608093157_Fix data")]
    partial class Fixdata
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("BusinessObjects.Entities.Account", b =>
                {
                    b.Property<Guid>("AccountId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("varchar");

                    b.Property<string>("Fullname")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("varchar");

                    b.Property<byte[]>("PasswordHash")
                        .IsRequired()
                        .HasColumnType("bytea");

                    b.Property<string>("PasswordResetToken")
                        .HasColumnType("text");

                    b.Property<byte[]>("PasswordSalt")
                        .IsRequired()
                        .HasColumnType("bytea");

                    b.Property<string>("Phone")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("varchar");

                    b.Property<DateTime?>("ResetTokenExpires")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Role")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("varchar");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("varchar");

                    b.Property<DateTime?>("VerifiedAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("AccountId");

                    b.HasIndex("Email")
                        .IsUnique();

                    b.HasIndex("Phone")
                        .IsUnique();

                    b.ToTable("Account", (string)null);
                });

            modelBuilder.Entity("BusinessObjects.Entities.Auction", b =>
                {
                    b.Property<Guid>("ActionId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("AuctionFashionItemItemId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("AuctionItemId")
                        .HasColumnType("uuid");

                    b.Property<int>("DepositFee")
                        .HasColumnType("integer");

                    b.Property<DateTime>("EndDate")
                        .HasColumnType("timestamptz");

                    b.Property<Guid>("ShopId")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("StartDate")
                        .HasColumnType("timestamptz");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("varchar");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("varchar");

                    b.HasKey("ActionId");

                    b.HasIndex("AuctionFashionItemItemId");

                    b.HasIndex("ShopId");

                    b.ToTable("Auction", (string)null);
                });

            modelBuilder.Entity("BusinessObjects.Entities.AuctionDeposit", b =>
                {
                    b.Property<Guid>("AuctionDepositId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("AuctionId")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedDate")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamptz");

                    b.Property<Guid>("MemberId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("TransactionId")
                        .HasColumnType("uuid");

                    b.HasKey("AuctionDepositId");

                    b.HasIndex("AuctionId");

                    b.HasIndex("MemberId");

                    b.HasIndex("TransactionId")
                        .IsUnique();

                    b.ToTable("AuctionDeposit", (string)null);
                });

            modelBuilder.Entity("BusinessObjects.Entities.Bid", b =>
                {
                    b.Property<Guid>("BidId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<int>("Amount")
                        .HasColumnType("integer");

                    b.Property<Guid>("AuctionId")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedDate")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamptz");

                    b.Property<bool>("IsWinning")
                        .HasColumnType("boolean");

                    b.Property<Guid>("MemberId")
                        .HasColumnType("uuid");

                    b.HasKey("BidId");

                    b.HasIndex("AuctionId");

                    b.HasIndex("MemberId");

                    b.ToTable("Bid", (string)null);
                });

            modelBuilder.Entity("BusinessObjects.Entities.Category", b =>
                {
                    b.Property<Guid>("CategoryId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("varchar");

                    b.Property<Guid?>("ParentId")
                        .HasColumnType("uuid");

                    b.HasKey("CategoryId");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.HasIndex("ParentId");

                    b.ToTable("Category", (string)null);
                });

            modelBuilder.Entity("BusinessObjects.Entities.Delivery", b =>
                {
                    b.Property<Guid>("DeliveryId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Address")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("varchar");

                    b.Property<string>("AddressType")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid>("MemberId")
                        .HasColumnType("uuid");

                    b.Property<string>("PhoneNumeber")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("varchar");

                    b.Property<string>("RecipientName")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("varchar");

                    b.HasKey("DeliveryId");

                    b.HasIndex("MemberId");

                    b.ToTable("Delivery", (string)null);
                });

            modelBuilder.Entity("BusinessObjects.Entities.FashionItem", b =>
                {
                    b.Property<Guid>("ItemId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("CategoryId")
                        .HasColumnType("uuid");

                    b.Property<string>("Condition")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("varchar");

                    b.Property<string>("Note")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("varchar");

                    b.Property<int>("Quantity")
                        .HasColumnType("integer");

                    b.Property<Guid>("RequestId")
                        .HasColumnType("uuid");

                    b.Property<decimal>("SellingPrice")
                        .HasPrecision(10, 2)
                        .HasColumnType("numeric");

                    b.Property<Guid>("ShopId")
                        .HasColumnType("uuid");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("varchar");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasMaxLength(21)
                        .HasColumnType("character varying(21)");

                    b.Property<decimal?>("Value")
                        .HasColumnType("numeric");

                    b.HasKey("ItemId");

                    b.HasIndex("CategoryId");

                    b.HasIndex("RequestId");

                    b.HasIndex("ShopId");

                    b.ToTable("FashionItem", (string)null);

                    b.HasDiscriminator<string>("Type").HasValue("ItemBase");

                    b.UseTphMappingStrategy();
                });

            modelBuilder.Entity("BusinessObjects.Entities.Image", b =>
                {
                    b.Property<Guid>("ImageId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("FashionItemItemId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("ItemId")
                        .HasColumnType("uuid");

                    b.Property<string>("Url")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("ImageId");

                    b.HasIndex("FashionItemItemId");

                    b.ToTable("Image", (string)null);
                });

            modelBuilder.Entity("BusinessObjects.Entities.Inquiry", b =>
                {
                    b.Property<Guid>("InquiryId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedDate")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamptz");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Fullname")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("varchar");

                    b.Property<Guid>("MemberId")
                        .HasColumnType("uuid");

                    b.Property<string>("Message")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Phone")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("varchar");

                    b.Property<Guid>("ShopId")
                        .HasColumnType("uuid");

                    b.HasKey("InquiryId");

                    b.HasIndex("MemberId");

                    b.HasIndex("ShopId");

                    b.ToTable("Inquiry", (string)null);
                });

            modelBuilder.Entity("BusinessObjects.Entities.Order", b =>
                {
                    b.Property<Guid>("OrderId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedDate")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamptz");

                    b.Property<Guid>("DeliveryId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("MemberId")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("PaymentDate")
                        .HasColumnType("timestamptz");

                    b.Property<string>("PaymentMethod")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("varchar");

                    b.Property<decimal>("TotalPrice")
                        .HasColumnType("numeric");

                    b.HasKey("OrderId");

                    b.HasIndex("DeliveryId");

                    b.HasIndex("MemberId");

                    b.ToTable("Order", (string)null);
                });

            modelBuilder.Entity("BusinessObjects.Entities.OrderDetail", b =>
                {
                    b.Property<Guid>("OrderDetailId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid?>("FashionItemItemId")
                        .HasColumnType("uuid");

                    b.Property<Guid?>("ItemId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("OrderId")
                        .HasColumnType("uuid");

                    b.Property<Guid?>("PointPackageId")
                        .HasColumnType("uuid");

                    b.Property<int>("Quantity")
                        .HasColumnType("integer");

                    b.Property<Guid?>("RequestId")
                        .HasColumnType("uuid");

                    b.Property<decimal>("UnitPrice")
                        .HasPrecision(10, 2)
                        .HasColumnType("numberic");

                    b.HasKey("OrderDetailId");

                    b.HasIndex("FashionItemItemId");

                    b.HasIndex("OrderId");

                    b.HasIndex("PointPackageId");

                    b.HasIndex("RequestId")
                        .IsUnique();

                    b.ToTable("OrderDetail", (string)null);
                });

            modelBuilder.Entity("BusinessObjects.Entities.PointPackage", b =>
                {
                    b.Property<Guid>("PointPackageId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<int>("Points")
                        .HasColumnType("integer");

                    b.Property<decimal>("Price")
                        .HasColumnType("numeric");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("PointPackageId");

                    b.ToTable("PointPackage", (string)null);
                });

            modelBuilder.Entity("BusinessObjects.Entities.Request", b =>
                {
                    b.Property<Guid>("RequestId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<int?>("ConsignDuration")
                        .HasColumnType("integer");

                    b.Property<DateTime>("CreatedDate")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamptz");

                    b.Property<DateTime?>("EndDate")
                        .HasColumnType("timestamptz");

                    b.Property<Guid>("MemberId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("ShopId")
                        .HasColumnType("uuid");

                    b.Property<DateTime?>("StartDate")
                        .HasColumnType("timestamptz");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("varchar");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("varchar");

                    b.HasKey("RequestId");

                    b.HasIndex("MemberId");

                    b.HasIndex("ShopId");

                    b.ToTable("Request", (string)null);
                });

            modelBuilder.Entity("BusinessObjects.Entities.Schedule", b =>
                {
                    b.Property<Guid>("ScheduleId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("AuctionId")
                        .HasColumnType("uuid");

                    b.Property<DateOnly>("Date")
                        .HasColumnType("date");

                    b.Property<Guid>("TimeslotId")
                        .HasColumnType("uuid");

                    b.HasKey("ScheduleId");

                    b.HasIndex("AuctionId")
                        .IsUnique();

                    b.HasIndex("TimeslotId");

                    b.ToTable("Schedule", (string)null);
                });

            modelBuilder.Entity("BusinessObjects.Entities.Shop", b =>
                {
                    b.Property<Guid>("ShopId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Address")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid>("StaffId")
                        .HasColumnType("uuid");

                    b.HasKey("ShopId");

                    b.HasIndex("StaffId")
                        .IsUnique();

                    b.ToTable("Shops");
                });

            modelBuilder.Entity("BusinessObjects.Entities.Timeslot", b =>
                {
                    b.Property<Guid>("TimeslotId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<TimeOnly>("EndTime")
                        .HasColumnType("time without time zone");

                    b.Property<int>("Slot")
                        .HasColumnType("integer");

                    b.Property<TimeOnly>("StartTime")
                        .HasColumnType("time without time zone");

                    b.HasKey("TimeslotId");

                    b.ToTable("Timeslot", (string)null);
                });

            modelBuilder.Entity("BusinessObjects.Entities.Transaction", b =>
                {
                    b.Property<Guid>("TransactionId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<decimal>("Amount")
                        .HasPrecision(10, 2)
                        .HasColumnType("numeric");

                    b.Property<DateTime>("CreatedDate")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamptz");

                    b.Property<Guid?>("OrderId")
                        .HasColumnType("uuid");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("varchar");

                    b.Property<Guid>("WalletId")
                        .HasColumnType("uuid");

                    b.HasKey("TransactionId");

                    b.HasIndex("OrderId")
                        .IsUnique();

                    b.HasIndex("WalletId");

                    b.ToTable("Transaction", (string)null);
                });

            modelBuilder.Entity("BusinessObjects.Entities.Wallet", b =>
                {
                    b.Property<Guid>("WalletId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<int>("Balance")
                        .HasColumnType("integer");

                    b.Property<string>("BankAccountNumber")
                        .HasMaxLength(20)
                        .HasColumnType("varchar");

                    b.Property<string>("BankName")
                        .HasMaxLength(100)
                        .HasColumnType("varchar");

                    b.Property<Guid>("MemberId")
                        .HasColumnType("uuid");

                    b.HasKey("WalletId");

                    b.HasIndex("MemberId")
                        .IsUnique();

                    b.ToTable("Wallet", (string)null);
                });

            modelBuilder.Entity("BusinessObjects.Entities.AuctionFashionItem", b =>
                {
                    b.HasBaseType("BusinessObjects.Entities.FashionItem");

                    b.Property<string>("AuctionItemStatus")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("Duration")
                        .HasColumnType("integer");

                    b.Property<decimal>("InitialPrice")
                        .HasColumnType("numeric");

                    b.HasDiscriminator().HasValue("ConsignedForAuction");
                });

            modelBuilder.Entity("BusinessObjects.Entities.ConsignedForSaleFashionItem", b =>
                {
                    b.HasBaseType("BusinessObjects.Entities.FashionItem");

                    b.HasDiscriminator().HasValue("ConsignedForSale");
                });

            modelBuilder.Entity("BusinessObjects.Entities.Auction", b =>
                {
                    b.HasOne("BusinessObjects.Entities.AuctionFashionItem", "AuctionFashionItem")
                        .WithMany()
                        .HasForeignKey("AuctionFashionItemItemId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("BusinessObjects.Entities.Shop", "Shop")
                        .WithMany()
                        .HasForeignKey("ShopId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("AuctionFashionItem");

                    b.Navigation("Shop");
                });

            modelBuilder.Entity("BusinessObjects.Entities.AuctionDeposit", b =>
                {
                    b.HasOne("BusinessObjects.Entities.Auction", "Auction")
                        .WithMany()
                        .HasForeignKey("AuctionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("BusinessObjects.Entities.Account", "Member")
                        .WithMany()
                        .HasForeignKey("MemberId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("BusinessObjects.Entities.Transaction", "Transaction")
                        .WithOne("AuctionDeposit")
                        .HasForeignKey("BusinessObjects.Entities.AuctionDeposit", "TransactionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Auction");

                    b.Navigation("Member");

                    b.Navigation("Transaction");
                });

            modelBuilder.Entity("BusinessObjects.Entities.Bid", b =>
                {
                    b.HasOne("BusinessObjects.Entities.Auction", "Auction")
                        .WithMany()
                        .HasForeignKey("AuctionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("BusinessObjects.Entities.Account", "Member")
                        .WithMany()
                        .HasForeignKey("MemberId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Auction");

                    b.Navigation("Member");
                });

            modelBuilder.Entity("BusinessObjects.Entities.Category", b =>
                {
                    b.HasOne("BusinessObjects.Entities.Category", "Parent")
                        .WithMany()
                        .HasForeignKey("ParentId");

                    b.Navigation("Parent");
                });

            modelBuilder.Entity("BusinessObjects.Entities.Delivery", b =>
                {
                    b.HasOne("BusinessObjects.Entities.Account", "Member")
                        .WithMany()
                        .HasForeignKey("MemberId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Member");
                });

            modelBuilder.Entity("BusinessObjects.Entities.FashionItem", b =>
                {
                    b.HasOne("BusinessObjects.Entities.Category", "Category")
                        .WithMany()
                        .HasForeignKey("CategoryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("BusinessObjects.Entities.Request", "Request")
                        .WithMany()
                        .HasForeignKey("RequestId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("BusinessObjects.Entities.Shop", "Shop")
                        .WithMany()
                        .HasForeignKey("ShopId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Category");

                    b.Navigation("Request");

                    b.Navigation("Shop");
                });

            modelBuilder.Entity("BusinessObjects.Entities.Image", b =>
                {
                    b.HasOne("BusinessObjects.Entities.FashionItem", "FashionItem")
                        .WithMany()
                        .HasForeignKey("FashionItemItemId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("FashionItem");
                });

            modelBuilder.Entity("BusinessObjects.Entities.Inquiry", b =>
                {
                    b.HasOne("BusinessObjects.Entities.Account", "Member")
                        .WithMany()
                        .HasForeignKey("MemberId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("BusinessObjects.Entities.Shop", "Shop")
                        .WithMany()
                        .HasForeignKey("ShopId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Member");

                    b.Navigation("Shop");
                });

            modelBuilder.Entity("BusinessObjects.Entities.Order", b =>
                {
                    b.HasOne("BusinessObjects.Entities.Delivery", "Delivery")
                        .WithMany()
                        .HasForeignKey("DeliveryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("BusinessObjects.Entities.Account", "Member")
                        .WithMany()
                        .HasForeignKey("MemberId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Delivery");

                    b.Navigation("Member");
                });

            modelBuilder.Entity("BusinessObjects.Entities.OrderDetail", b =>
                {
                    b.HasOne("BusinessObjects.Entities.FashionItem", "FashionItem")
                        .WithMany()
                        .HasForeignKey("FashionItemItemId");

                    b.HasOne("BusinessObjects.Entities.Order", "Order")
                        .WithMany()
                        .HasForeignKey("OrderId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("BusinessObjects.Entities.PointPackage", "PointPackage")
                        .WithMany("OrderDetails")
                        .HasForeignKey("PointPackageId");

                    b.HasOne("BusinessObjects.Entities.Request", "Request")
                        .WithOne("OrderDetail")
                        .HasForeignKey("BusinessObjects.Entities.OrderDetail", "RequestId");

                    b.Navigation("FashionItem");

                    b.Navigation("Order");

                    b.Navigation("PointPackage");

                    b.Navigation("Request");
                });

            modelBuilder.Entity("BusinessObjects.Entities.Request", b =>
                {
                    b.HasOne("BusinessObjects.Entities.Account", "Member")
                        .WithMany()
                        .HasForeignKey("MemberId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("BusinessObjects.Entities.Shop", "Shop")
                        .WithMany()
                        .HasForeignKey("ShopId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Member");

                    b.Navigation("Shop");
                });

            modelBuilder.Entity("BusinessObjects.Entities.Schedule", b =>
                {
                    b.HasOne("BusinessObjects.Entities.Auction", "Auction")
                        .WithOne("Schedule")
                        .HasForeignKey("BusinessObjects.Entities.Schedule", "AuctionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("BusinessObjects.Entities.Timeslot", "Timeslot")
                        .WithMany()
                        .HasForeignKey("TimeslotId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Auction");

                    b.Navigation("Timeslot");
                });

            modelBuilder.Entity("BusinessObjects.Entities.Shop", b =>
                {
                    b.HasOne("BusinessObjects.Entities.Account", "Staff")
                        .WithOne("Shop")
                        .HasForeignKey("BusinessObjects.Entities.Shop", "StaffId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Staff");
                });

            modelBuilder.Entity("BusinessObjects.Entities.Transaction", b =>
                {
                    b.HasOne("BusinessObjects.Entities.Order", "Order")
                        .WithOne("Transaction")
                        .HasForeignKey("BusinessObjects.Entities.Transaction", "OrderId");

                    b.HasOne("BusinessObjects.Entities.Wallet", "Wallet")
                        .WithMany()
                        .HasForeignKey("WalletId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Order");

                    b.Navigation("Wallet");
                });

            modelBuilder.Entity("BusinessObjects.Entities.Wallet", b =>
                {
                    b.HasOne("BusinessObjects.Entities.Account", "Member")
                        .WithOne("Wallet")
                        .HasForeignKey("BusinessObjects.Entities.Wallet", "MemberId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Member");
                });

            modelBuilder.Entity("BusinessObjects.Entities.Account", b =>
                {
                    b.Navigation("Shop")
                        .IsRequired();

                    b.Navigation("Wallet")
                        .IsRequired();
                });

            modelBuilder.Entity("BusinessObjects.Entities.Auction", b =>
                {
                    b.Navigation("Schedule")
                        .IsRequired();
                });

            modelBuilder.Entity("BusinessObjects.Entities.Order", b =>
                {
                    b.Navigation("Transaction")
                        .IsRequired();
                });

            modelBuilder.Entity("BusinessObjects.Entities.PointPackage", b =>
                {
                    b.Navigation("OrderDetails");
                });

            modelBuilder.Entity("BusinessObjects.Entities.Request", b =>
                {
                    b.Navigation("OrderDetail");
                });

            modelBuilder.Entity("BusinessObjects.Entities.Transaction", b =>
                {
                    b.Navigation("AuctionDeposit");
                });
#pragma warning restore 612, 618
        }
    }
}
