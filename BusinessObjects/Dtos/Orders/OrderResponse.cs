﻿using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.FashionItems;
using BusinessObjects.Dtos.OrderDetails;
using BusinessObjects.Dtos.Shops;
using BusinessObjects.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Dtos.Orders
{
    public class OrderResponse
    {
        public Guid OrderId { get; set; }
        public int Quantity { get; set; }
        public int TotalPrice { get; set; }
        public string OrderCode { get; set; }
        public DateTime CreatedDate { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public DateTime? PaymentDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public Guid? MemberId { get; set; }
        public string? CustomerName { get; set; }
        public string? RecipientName { get; set; }
        public string? ContactNumber { get; set; }
        public string? Address { get; set; }
        public string? Email { get; set; }
        public PurchaseType PurchaseType { get; set; }
        public OrderStatus Status { get; set; }
        public List<ShopOrderResponse>? ShopOrderResponses { get; set; }
        public List<Guid?>? ListItemNotAvailable { get; set; }
        /*public List<OrderDetailResponse<FashionItem>>? OrderDetails { get; set;}*/

        public List<OrderDetailsResponse>? OrderDetailItems { get; set; }
    }

    public class OrderListResponse
    {
        public Guid OrderId { get; set; }
        public int Quantity { get; set; }
        public int TotalPrice { get; set; }
        public string OrderCode { get; set; }
        public DateTime CreatedDate { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public DateTime? PaymentDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public Guid? MemberId { get; set; }
        public string? CustomerName { get; set; }
        public string? RecipientName { get; set; }
        public string? ContactNumber { get; set; }
        public string? Address { get; set; }
        public string? Email { get; set; }
        public PurchaseType PurchaseType { get; set; }
        public OrderStatus Status { get; set; }
    }
}