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
    [Migration("20240626053703_ReviseDB4")]
    partial class ReviseDB4
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

                    b.HasDiscriminator<string>("Role").HasValue("Account");

                    b.UseTphMappingStrategy();
                });

            modelBuilder.Entity("BusinessObjects.Entities.Auction", b =>
                {
                    b.Property<Guid>("AuctionId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("AuctionFashionItemId")
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

                    b.Property<int>("StepIncrement")
                        .HasColumnType("integer");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("varchar");

                    b.HasKey("AuctionId");

                    b.HasIndex("AuctionFashionItemId");

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

                    b.Property<int>("Level")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer")
                        .HasDefaultValue(1);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("varchar");

                    b.Property<Guid?>("ParentId")
                        .HasColumnType("uuid");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("varchar");

                    b.HasKey("CategoryId");

                    b.HasIndex("ParentId");

                    b.ToTable("Category", (string)null);
                });

            modelBuilder.Entity("BusinessObjects.Entities.ConsignSale", b =>
                {
                    b.Property<Guid>("ConsignSaleId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<int?>("ConsignDuration")
                        .HasColumnType("integer");

                    b.Property<string>("ConsignSaleCode")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("CreatedDate")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamptz");

                    b.Property<DateTime?>("EndDate")
                        .HasColumnType("timestamptz");

                    b.Property<Guid>("MemberId")
                        .HasColumnType("uuid");

                    b.Property<int>("MemberReceivedAmount")
                        .HasColumnType("integer");

                    b.Property<Guid>("ShopId")
                        .HasColumnType("uuid");

                    b.Property<int>("SoldPrice")
                        .HasColumnType("integer");

                    b.Property<DateTime?>("StartDate")
                        .HasColumnType("timestamptz");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("varchar");

                    b.Property<int>("TotalPrice")
                        .HasColumnType("integer");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("varchar");

                    b.HasKey("ConsignSaleId");

                    b.HasIndex("ConsignSaleCode")
                        .IsUnique();

                    b.HasIndex("MemberId");

                    b.HasIndex("ShopId");

                    b.ToTable("ConsignSale", (string)null);
                });

            modelBuilder.Entity("BusinessObjects.Entities.ConsignSaleDetail", b =>
                {
                    b.Property<Guid>("ConsignSaleDetailId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<int>("ConfirmedPrice")
                        .HasColumnType("integer");

                    b.Property<Guid>("ConsignSaleId")
                        .HasColumnType("uuid");

                    b.Property<int>("DealPrice")
                        .HasColumnType("integer");

                    b.Property<Guid>("FashionItemId")
                        .HasColumnType("uuid");

                    b.HasKey("ConsignSaleDetailId");

                    b.HasIndex("ConsignSaleId");

                    b.HasIndex("FashionItemId")
                        .IsUnique();

                    b.ToTable("ConsignSaleDetail", (string)null);
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
                        .HasMaxLength(20)
                        .HasColumnType("varchar");

                    b.Property<bool>("IsDefault")
                        .HasColumnType("boolean");

                    b.Property<Guid>("MemberId")
                        .HasColumnType("uuid");

                    b.Property<string>("Phone")
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

                    b.Property<string>("Brand")
                        .HasColumnType("text");

                    b.Property<Guid>("CategoryId")
                        .HasColumnType("uuid");

                    b.Property<string>("Color")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Condition")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Gender")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("varchar");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("varchar");

                    b.Property<string>("Note")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("varchar");

                    b.Property<int>("SellingPrice")
                        .HasColumnType("integer");

                    b.Property<Guid>("ShopId")
                        .HasColumnType("uuid");

                    b.Property<string>("Size")
                        .IsRequired()
                        .HasMaxLength(5)
                        .HasColumnType("varchar");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("varchar");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("varchar");

                    b.Property<decimal?>("Value")
                        .HasColumnType("numeric");

                    b.HasKey("ItemId");

                    b.HasIndex("CategoryId");

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

                    b.Property<Guid>("FashionItemId")
                        .HasColumnType("uuid");

                    b.Property<string>("Url")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("ImageId");

                    b.HasIndex("FashionItemId");

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

                    b.Property<Guid?>("BidId")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedDate")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamptz");

                    b.Property<Guid>("DeliveryId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("MemberId")
                        .HasColumnType("uuid");

                    b.Property<string>("OrderCode")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("PaymentDate")
                        .HasColumnType("timestamptz");

                    b.Property<string>("PaymentMethod")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("varchar");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("varchar");

                    b.Property<int>("TotalPrice")
                        .HasColumnType("integer");

                    b.HasKey("OrderId");

                    b.HasIndex("BidId")
                        .IsUnique();

                    b.HasIndex("DeliveryId");

                    b.HasIndex("MemberId");

                    b.HasIndex("OrderCode")
                        .IsUnique();

                    b.ToTable("Order", (string)null);
                });

            modelBuilder.Entity("BusinessObjects.Entities.OrderDetail", b =>
                {
                    b.Property<Guid>("OrderDetailId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid?>("ConsignSaleId")
                        .HasColumnType("uuid");

                    b.Property<Guid?>("FashionItemId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("OrderId")
                        .HasColumnType("uuid");

                    b.Property<Guid?>("PointPackageId")
                        .HasColumnType("uuid");

                    b.Property<int>("UnitPrice")
                        .HasColumnType("integer");

                    b.HasKey("OrderDetailId");

                    b.HasIndex("ConsignSaleId");

                    b.HasIndex("FashionItemId");

                    b.HasIndex("OrderId");

                    b.HasIndex("PointPackageId");

                    b.ToTable("OrderDetail", (string)null);
                });

            modelBuilder.Entity("BusinessObjects.Entities.PointPackage", b =>
                {
                    b.Property<Guid>("PointPackageId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<int>("Points")
                        .HasColumnType("integer");

                    b.Property<int>("Price")
                        .HasColumnType("integer");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("varchar");

                    b.HasKey("PointPackageId");

                    b.ToTable("PointPackage", (string)null);
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

                    b.Property<string>("Phone")
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

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("varchar");

                    b.HasKey("TimeslotId");

                    b.ToTable("Timeslot", (string)null);
                });

            modelBuilder.Entity("BusinessObjects.Entities.Transaction", b =>
                {
                    b.Property<Guid>("TransactionId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<int>("Amount")
                        .HasColumnType("integer");

                    b.Property<DateTime>("CreatedDate")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamptz");

                    b.Property<Guid>("MemberId")
                        .HasColumnType("uuid");

                    b.Property<Guid?>("OrderId")
                        .HasColumnType("uuid");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("varchar");

                    b.HasKey("TransactionId");

                    b.HasIndex("MemberId");

                    b.HasIndex("OrderId")
                        .IsUnique();

                    b.ToTable("Transaction", (string)null);
                });

            modelBuilder.Entity("BusinessObjects.Entities.Admin", b =>
                {
                    b.HasBaseType("BusinessObjects.Entities.Account");

                    b.HasDiscriminator().HasValue("Admin");
                });

            modelBuilder.Entity("BusinessObjects.Entities.Member", b =>
                {
                    b.HasBaseType("BusinessObjects.Entities.Account");

                    b.Property<int>("Balance")
                        .HasColumnType("integer");

                    b.HasDiscriminator().HasValue("Member");
                });

            modelBuilder.Entity("BusinessObjects.Entities.Staff", b =>
                {
                    b.HasBaseType("BusinessObjects.Entities.Account");

                    b.HasDiscriminator().HasValue("Staff");
                });

            modelBuilder.Entity("BusinessObjects.Entities.AuctionFashionItem", b =>
                {
                    b.HasBaseType("BusinessObjects.Entities.FashionItem");

                    b.Property<int>("Duration")
                        .HasColumnType("integer");

                    b.Property<int>("InitialPrice")
                        .HasColumnType("integer");

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
                        .WithMany("Auctions")
                        .HasForeignKey("AuctionFashionItemId")
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

            modelBuilder.Entity("BusinessObjects.Entities.ConsignSale", b =>
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

            modelBuilder.Entity("BusinessObjects.Entities.ConsignSaleDetail", b =>
                {
                    b.HasOne("BusinessObjects.Entities.ConsignSale", "ConsignSale")
                        .WithMany("ConsignSaleDetails")
                        .HasForeignKey("ConsignSaleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("BusinessObjects.Entities.FashionItem", "FashionItem")
                        .WithOne("ConsignSaleDetail")
                        .HasForeignKey("BusinessObjects.Entities.ConsignSaleDetail", "FashionItemId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ConsignSale");

                    b.Navigation("FashionItem");
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
                        .WithMany("FashionItems")
                        .HasForeignKey("CategoryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("BusinessObjects.Entities.Shop", "Shop")
                        .WithMany()
                        .HasForeignKey("ShopId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Category");

                    b.Navigation("Shop");
                });

            modelBuilder.Entity("BusinessObjects.Entities.Image", b =>
                {
                    b.HasOne("BusinessObjects.Entities.FashionItem", "FashionItem")
                        .WithMany()
                        .HasForeignKey("FashionItemId")
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
                    b.HasOne("BusinessObjects.Entities.Bid", "Bid")
                        .WithOne("Order")
                        .HasForeignKey("BusinessObjects.Entities.Order", "BidId");

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

                    b.Navigation("Bid");

                    b.Navigation("Delivery");

                    b.Navigation("Member");
                });

            modelBuilder.Entity("BusinessObjects.Entities.OrderDetail", b =>
                {
                    b.HasOne("BusinessObjects.Entities.ConsignSale", "ConsignSale")
                        .WithMany()
                        .HasForeignKey("ConsignSaleId");

                    b.HasOne("BusinessObjects.Entities.FashionItem", "FashionItem")
                        .WithMany()
                        .HasForeignKey("FashionItemId");

                    b.HasOne("BusinessObjects.Entities.Order", "Order")
                        .WithMany()
                        .HasForeignKey("OrderId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("BusinessObjects.Entities.PointPackage", "PointPackage")
                        .WithMany("OrderDetails")
                        .HasForeignKey("PointPackageId");

                    b.Navigation("ConsignSale");

                    b.Navigation("FashionItem");

                    b.Navigation("Order");

                    b.Navigation("PointPackage");
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
                    b.HasOne("BusinessObjects.Entities.Staff", "Staff")
                        .WithOne("Shop")
                        .HasForeignKey("BusinessObjects.Entities.Shop", "StaffId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Staff");
                });

            modelBuilder.Entity("BusinessObjects.Entities.Transaction", b =>
                {
                    b.HasOne("BusinessObjects.Entities.Member", "Member")
                        .WithMany()
                        .HasForeignKey("MemberId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("BusinessObjects.Entities.Order", "Order")
                        .WithOne("Transaction")
                        .HasForeignKey("BusinessObjects.Entities.Transaction", "OrderId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.Navigation("Member");

                    b.Navigation("Order");
                });

            modelBuilder.Entity("BusinessObjects.Entities.Auction", b =>
                {
                    b.Navigation("Schedule")
                        .IsRequired();
                });

            modelBuilder.Entity("BusinessObjects.Entities.Bid", b =>
                {
                    b.Navigation("Order")
                        .IsRequired();
                });

            modelBuilder.Entity("BusinessObjects.Entities.Category", b =>
                {
                    b.Navigation("FashionItems");
                });

            modelBuilder.Entity("BusinessObjects.Entities.ConsignSale", b =>
                {
                    b.Navigation("ConsignSaleDetails");
                });

            modelBuilder.Entity("BusinessObjects.Entities.FashionItem", b =>
                {
                    b.Navigation("ConsignSaleDetail")
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

            modelBuilder.Entity("BusinessObjects.Entities.Transaction", b =>
                {
                    b.Navigation("AuctionDeposit");
                });

            modelBuilder.Entity("BusinessObjects.Entities.Staff", b =>
                {
                    b.Navigation("Shop")
                        .IsRequired();
                });

            modelBuilder.Entity("BusinessObjects.Entities.AuctionFashionItem", b =>
                {
                    b.Navigation("Auctions");
                });
#pragma warning restore 612, 618
        }
    }
}
