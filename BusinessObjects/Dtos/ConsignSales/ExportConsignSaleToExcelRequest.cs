using System;
using BusinessObjects.Dtos.Commons;

namespace BusinessObjects.Dtos.ConsignSales;

public class ExportConsignSaleToExcelRequest
{
    public string? ConsignSaleCode { get; set; }
    public string? MemberName { get; set; }
    public string? Phone { get; set; }
    public Guid? ShopId { get; set; }
    public string? Email { get; set; }
    public ConsignSaleStatus[] Statuses { get; set; } = [];
    public ConsignSaleType[] Types { get; set; } = [];
    public ConsignSaleMethod[] ConsignSaleMethods { get; set; } = [];
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
