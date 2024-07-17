using BusinessObjects.Dtos.Commons;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Dtos.ConsignSales
{
    public class CreateConsignSaleRequest
    {
        public ConsignSaleType Type { get; set; }
        public Guid ShopId { get; set; }
        public List<AddFashionItemForConsignRequest> fashionItemForConsigns { get; set; } = new List<AddFashionItemForConsignRequest>();
    }
    public class CreateConsignSaleByShopRequest
    {
        public ConsignSaleType Type { get; set; }
        public string? Consigner { get; set; }
        public required string Phone {  get; set; }
        public string? Address { get; set; }
        [EmailAddress] public string? Email { get; set; }
        public List<AddFashionItemForConsignRequest> fashionItemForConsigns { get; set; } = new List<AddFashionItemForConsignRequest>();
    }
}
