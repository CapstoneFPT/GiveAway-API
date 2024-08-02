﻿using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices.JavaScript;
using BusinessObjects.Dtos.Commons;

namespace BusinessObjects.Entities;

public class ConsignSale
{
    [Key] public Guid ConsignSaleId { get; set; }
    public ConsignSaleType Type { get; set; }
    public string ConsignSaleCode { get; set; }
    public DateTime CreatedDate { get; set; }
    public int? ConsignDuration { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public Shop Shop { get; set; }
    public Guid ShopId { get; set; }
    public Account? Member { get; set; }
    public Guid? MemberId { get; set; }
    public ConsignSaleStatus Status { get; set; }
    public int TotalPrice { get; set; }
    public int SoldPrice { get; set; }
    public int ConsignorReceivedAmount { get; set; }
    public Transaction? Transaction { get; set; }
    
    public string? ConsignorName { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    [EmailAddress] public string? Email { get; set; }
    public ConsignSaleMethod ConsignSaleMethod { get; set; }
    public ICollection<ConsignSaleDetail> ConsignSaleDetails { get; set; } = new List<ConsignSaleDetail>();
}

