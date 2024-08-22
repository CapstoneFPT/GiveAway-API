﻿using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Entities;

public class Image
{
    [Key]
    public Guid ImageId { get; set; }
    public string Url { get; set; }
    public IndividualFashionItem IndividualFashionItem { get; set; }
    public Guid? IndividualFashionItemId { get; set; }
    public MasterFashionItem MasterFashionItem { get; set; }
    public ConsignSaleDetail ConsignSaleDetail { get; set; }
    public Guid? ConsignSaleDetailId { get; set; }
    public Guid? MasterFashionItemId { get; set; }
    public Refund Refund { get; set; }
    public Guid? RefundId { get; set; }
    public DateTime CreatedDate { get; set; }
}