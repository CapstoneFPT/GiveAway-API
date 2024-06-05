CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;

CREATE TABLE "Account" (
    "AccountId" uuid NOT NULL,
    "Email" varchar NOT NULL,
    "Phone" varchar NOT NULL,
    "PasswordHash" text NOT NULL,
    "PasswordSalt" text NOT NULL,
    "Fullname" varchar NOT NULL,
    "VerifiedAt" timestamp with time zone,
    "PasswordResetToken" text,
    "ResetTokenExpires" timestamp with time zone,
    "Role" varchar NOT NULL,
    "Status" varchar NOT NULL,
    CONSTRAINT "PK_Account" PRIMARY KEY ("AccountId")
);

CREATE TABLE "Category" (
    "CategoryId" uuid NOT NULL,
    "Name" varchar NOT NULL,
    "ParentId" uuid,
    CONSTRAINT "PK_Category" PRIMARY KEY ("CategoryId"),
    CONSTRAINT "FK_Category_Category_ParentId" FOREIGN KEY ("ParentId") REFERENCES "Category" ("CategoryId")
);

CREATE TABLE "Package" (
    "PackageId" uuid NOT NULL,
    "Points" integer NOT NULL,
    "Price" numeric NOT NULL,
    "Status" text NOT NULL,
    CONSTRAINT "PK_Package" PRIMARY KEY ("PackageId")
);

CREATE TABLE "Timeslot" (
    "TimeslotId" uuid NOT NULL,
    "Slot" integer NOT NULL,
    "StartTime" time without time zone NOT NULL,
    "EndTime" time without time zone NOT NULL,
    CONSTRAINT "PK_Timeslot" PRIMARY KEY ("TimeslotId")
);

CREATE TABLE "Delivery" (
    "DeliveryId" uuid NOT NULL,
    "RecipientName" varchar NOT NULL,
    "PhoneNumeber" varchar NOT NULL,
    "Address" varchar NOT NULL,
    "AddressType" text NOT NULL,
    "MemberId" uuid NOT NULL,
    CONSTRAINT "PK_Delivery" PRIMARY KEY ("DeliveryId"),
    CONSTRAINT "FK_Delivery_Account_MemberId" FOREIGN KEY ("MemberId") REFERENCES "Account" ("AccountId") ON DELETE CASCADE
);

CREATE TABLE "Shops" (
    "ShopId" uuid NOT NULL,
    "Address" text NOT NULL,
    "StaffId" uuid NOT NULL,
    CONSTRAINT "PK_Shops" PRIMARY KEY ("ShopId"),
    CONSTRAINT "FK_Shops_Account_StaffId" FOREIGN KEY ("StaffId") REFERENCES "Account" ("AccountId") ON DELETE CASCADE
);

CREATE TABLE "Wallet" (
    "WalletId" uuid NOT NULL,
    "Balance" integer NOT NULL,
    "MemberId" uuid NOT NULL,
    CONSTRAINT "PK_Wallet" PRIMARY KEY ("WalletId"),
    CONSTRAINT "FK_Wallet_Account_MemberId" FOREIGN KEY ("MemberId") REFERENCES "Account" ("AccountId") ON DELETE CASCADE
);

CREATE TABLE "Order" (
    "OrderId" uuid NOT NULL,
    "TotalPrice" numeric NOT NULL,
    "CreatedDate" timestamptz NOT NULL,
    "PaymentMethod" varchar NOT NULL,
    "PaymentDate" timestamptz NOT NULL,
    "MemberId" uuid NOT NULL,
    "DeliveryId" uuid NOT NULL,
    CONSTRAINT "PK_Order" PRIMARY KEY ("OrderId"),
    CONSTRAINT "FK_Order_Account_MemberId" FOREIGN KEY ("MemberId") REFERENCES "Account" ("AccountId") ON DELETE CASCADE,
    CONSTRAINT "FK_Order_Delivery_DeliveryId" FOREIGN KEY ("DeliveryId") REFERENCES "Delivery" ("DeliveryId") ON DELETE CASCADE
);

CREATE TABLE "Inquiry" (
    "InquiryId" uuid NOT NULL,
    "Email" text NOT NULL,
    "Fullname" varchar NOT NULL,
    "Phone" varchar NOT NULL,
    "Message" text NOT NULL,
    "CreatedDate" timestamptz NOT NULL,
    "MemberId" uuid NOT NULL,
    "ShopId" uuid NOT NULL,
    CONSTRAINT "PK_Inquiry" PRIMARY KEY ("InquiryId"),
    CONSTRAINT "FK_Inquiry_Account_MemberId" FOREIGN KEY ("MemberId") REFERENCES "Account" ("AccountId") ON DELETE CASCADE,
    CONSTRAINT "FK_Inquiry_Shops_ShopId" FOREIGN KEY ("ShopId") REFERENCES "Shops" ("ShopId") ON DELETE CASCADE
);

CREATE TABLE "Request" (
    "RequestId" uuid NOT NULL,
    "Type" varchar NOT NULL,
    "CreatedDate" timestamptz NOT NULL,
    "ConsignDuration" integer,
    "StartDate" timestamptz,
    "EndDate" timestamptz,
    "ShopId" uuid NOT NULL,
    "MemberId" uuid NOT NULL,
    "Status" varchar NOT NULL,
    CONSTRAINT "PK_Request" PRIMARY KEY ("RequestId"),
    CONSTRAINT "FK_Request_Account_MemberId" FOREIGN KEY ("MemberId") REFERENCES "Account" ("AccountId") ON DELETE CASCADE,
    CONSTRAINT "FK_Request_Shops_ShopId" FOREIGN KEY ("ShopId") REFERENCES "Shops" ("ShopId") ON DELETE CASCADE
);

CREATE TABLE "Transaction" (
    "TransactionId" uuid NOT NULL,
    "Amount" numeric(10,2) NOT NULL,
    "CreatedDate" timestamptz NOT NULL,
    "Type" varchar NOT NULL,
    "OrderId" uuid,
    "PackageId" uuid,
    "WalletId" uuid NOT NULL,
    CONSTRAINT "PK_Transaction" PRIMARY KEY ("TransactionId"),
    CONSTRAINT "FK_Transaction_Order_OrderId" FOREIGN KEY ("OrderId") REFERENCES "Order" ("OrderId"),
    CONSTRAINT "FK_Transaction_Package_PackageId" FOREIGN KEY ("PackageId") REFERENCES "Package" ("PackageId"),
    CONSTRAINT "FK_Transaction_Wallet_WalletId" FOREIGN KEY ("WalletId") REFERENCES "Wallet" ("WalletId") ON DELETE CASCADE
);

CREATE TABLE "Item" (
    "ItemId" uuid NOT NULL,
    "Type" character varying(21) NOT NULL,
    "Price" numeric(10,2) NOT NULL,
    "Name" varchar NOT NULL,
    "Note" varchar NOT NULL,
    "Value" numeric NOT NULL,
    "Condition" integer NOT NULL,
    "RequestId" uuid NOT NULL,
    "ShopId" uuid NOT NULL,
    "CategoryId" uuid NOT NULL,
    "Status" varchar NOT NULL,
    "Duration" integer,
    "InitialPrice" numeric,
    "AuctionItemStatus" text,
    CONSTRAINT "PK_Item" PRIMARY KEY ("ItemId"),
    CONSTRAINT "FK_Item_Category_CategoryId" FOREIGN KEY ("CategoryId") REFERENCES "Category" ("CategoryId") ON DELETE CASCADE,
    CONSTRAINT "FK_Item_Request_RequestId" FOREIGN KEY ("RequestId") REFERENCES "Request" ("RequestId") ON DELETE CASCADE,
    CONSTRAINT "FK_Item_Shops_ShopId" FOREIGN KEY ("ShopId") REFERENCES "Shops" ("ShopId") ON DELETE CASCADE
);

CREATE TABLE "Auction" (
    "ActionId" uuid NOT NULL,
    "Title" varchar NOT NULL,
    "StartDate" timestamptz NOT NULL,
    "EndDate" timestamptz NOT NULL,
    "DepositFee" integer NOT NULL,
    "ShopId" uuid NOT NULL,
    "AuctionItemId" uuid NOT NULL,
    "Status" varchar NOT NULL,
    CONSTRAINT "PK_Auction" PRIMARY KEY ("ActionId"),
    CONSTRAINT "FK_Auction_Item_AuctionItemId" FOREIGN KEY ("AuctionItemId") REFERENCES "Item" ("ItemId") ON DELETE CASCADE,
    CONSTRAINT "FK_Auction_Shops_ShopId" FOREIGN KEY ("ShopId") REFERENCES "Shops" ("ShopId") ON DELETE CASCADE
);

CREATE TABLE "Image" (
    "ImageId" uuid NOT NULL,
    "Url" text NOT NULL,
    "ItemId" uuid NOT NULL,
    CONSTRAINT "PK_Image" PRIMARY KEY ("ImageId"),
    CONSTRAINT "FK_Image_Item_ItemId" FOREIGN KEY ("ItemId") REFERENCES "Item" ("ItemId") ON DELETE CASCADE
);

CREATE TABLE "OrderDetail" (
    "OrderDetailId" uuid NOT NULL,
    "UnitPrice" numeric(10,2) NOT NULL,
    "OrderId" uuid NOT NULL,
    "ItemId" uuid NOT NULL,
    CONSTRAINT "PK_OrderDetail" PRIMARY KEY ("OrderDetailId"),
    CONSTRAINT "FK_OrderDetail_Item_ItemId" FOREIGN KEY ("ItemId") REFERENCES "Item" ("ItemId") ON DELETE CASCADE,
    CONSTRAINT "FK_OrderDetail_Order_OrderId" FOREIGN KEY ("OrderId") REFERENCES "Order" ("OrderId") ON DELETE CASCADE
);

CREATE TABLE "AuctionDeposit" (
    "AuctionDepositId" uuid NOT NULL,
    "CreatedDate" timestamptz NOT NULL,
    "MemberId" uuid NOT NULL,
    "AuctionId" uuid NOT NULL,
    CONSTRAINT "PK_AuctionDeposit" PRIMARY KEY ("AuctionDepositId"),
    CONSTRAINT "FK_AuctionDeposit_Account_MemberId" FOREIGN KEY ("MemberId") REFERENCES "Account" ("AccountId") ON DELETE CASCADE,
    CONSTRAINT "FK_AuctionDeposit_Auction_AuctionId" FOREIGN KEY ("AuctionId") REFERENCES "Auction" ("ActionId") ON DELETE CASCADE
);

CREATE TABLE "Bid" (
    "BidId" uuid NOT NULL,
    "Amount" integer NOT NULL,
    "CreatedDate" timestamptz NOT NULL,
    "AuctionId" uuid NOT NULL,
    "MemberId" uuid NOT NULL,
    "IsWinning" boolean NOT NULL,
    CONSTRAINT "PK_Bid" PRIMARY KEY ("BidId"),
    CONSTRAINT "FK_Bid_Account_MemberId" FOREIGN KEY ("MemberId") REFERENCES "Account" ("AccountId") ON DELETE CASCADE,
    CONSTRAINT "FK_Bid_Auction_AuctionId" FOREIGN KEY ("AuctionId") REFERENCES "Auction" ("ActionId") ON DELETE CASCADE
);

CREATE TABLE "Schedule" (
    "ScheduleId" uuid NOT NULL,
    "Date" date NOT NULL,
    "TimeslotId" uuid NOT NULL,
    "AuctionId" uuid NOT NULL,
    CONSTRAINT "PK_Schedule" PRIMARY KEY ("ScheduleId"),
    CONSTRAINT "FK_Schedule_Auction_AuctionId" FOREIGN KEY ("AuctionId") REFERENCES "Auction" ("ActionId") ON DELETE CASCADE,
    CONSTRAINT "FK_Schedule_Timeslot_TimeslotId" FOREIGN KEY ("TimeslotId") REFERENCES "Timeslot" ("TimeslotId") ON DELETE CASCADE
);

CREATE UNIQUE INDEX "IX_Account_Email" ON "Account" ("Email");

CREATE UNIQUE INDEX "IX_Account_Phone" ON "Account" ("Phone");

CREATE INDEX "IX_Auction_AuctionItemId" ON "Auction" ("AuctionItemId");

CREATE INDEX "IX_Auction_ShopId" ON "Auction" ("ShopId");

CREATE INDEX "IX_AuctionDeposit_AuctionId" ON "AuctionDeposit" ("AuctionId");

CREATE INDEX "IX_AuctionDeposit_MemberId" ON "AuctionDeposit" ("MemberId");

CREATE INDEX "IX_Bid_AuctionId" ON "Bid" ("AuctionId");

CREATE INDEX "IX_Bid_MemberId" ON "Bid" ("MemberId");

CREATE UNIQUE INDEX "IX_Category_Name" ON "Category" ("Name");

CREATE INDEX "IX_Category_ParentId" ON "Category" ("ParentId");

CREATE INDEX "IX_Delivery_MemberId" ON "Delivery" ("MemberId");

CREATE INDEX "IX_Image_ItemId" ON "Image" ("ItemId");

CREATE INDEX "IX_Inquiry_MemberId" ON "Inquiry" ("MemberId");

CREATE INDEX "IX_Inquiry_ShopId" ON "Inquiry" ("ShopId");

CREATE INDEX "IX_Item_CategoryId" ON "Item" ("CategoryId");

CREATE INDEX "IX_Item_RequestId" ON "Item" ("RequestId");

CREATE INDEX "IX_Item_ShopId" ON "Item" ("ShopId");

CREATE INDEX "IX_Order_DeliveryId" ON "Order" ("DeliveryId");

CREATE INDEX "IX_Order_MemberId" ON "Order" ("MemberId");

CREATE INDEX "IX_OrderDetail_ItemId" ON "OrderDetail" ("ItemId");

CREATE INDEX "IX_OrderDetail_OrderId" ON "OrderDetail" ("OrderId");

CREATE INDEX "IX_Request_MemberId" ON "Request" ("MemberId");

CREATE INDEX "IX_Request_ShopId" ON "Request" ("ShopId");

CREATE INDEX "IX_Schedule_AuctionId" ON "Schedule" ("AuctionId");

CREATE INDEX "IX_Schedule_TimeslotId" ON "Schedule" ("TimeslotId");

CREATE INDEX "IX_Shops_StaffId" ON "Shops" ("StaffId");

CREATE INDEX "IX_Transaction_OrderId" ON "Transaction" ("OrderId");

CREATE INDEX "IX_Transaction_PackageId" ON "Transaction" ("PackageId");

CREATE INDEX "IX_Transaction_WalletId" ON "Transaction" ("WalletId");

CREATE UNIQUE INDEX "IX_Wallet_MemberId" ON "Wallet" ("MemberId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20240605051120_Initial', '8.0.4');

COMMIT;

START TRANSACTION;

DROP INDEX "IX_Transaction_OrderId";

DROP INDEX "IX_Shops_StaffId";

DROP INDEX "IX_Schedule_AuctionId";

CREATE UNIQUE INDEX "IX_Transaction_OrderId" ON "Transaction" ("OrderId");

CREATE UNIQUE INDEX "IX_Shops_StaffId" ON "Shops" ("StaffId");

CREATE UNIQUE INDEX "IX_Schedule_AuctionId" ON "Schedule" ("AuctionId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20240605054446_Fix Relations', '8.0.4');

COMMIT;

