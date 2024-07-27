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
        [Required] public ConsignSaleType Type { get; set; }
        [Required] public Guid ShopId { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "You must add at least 1 fashion item")]
        public List<AddFashionItemForConsignRequest> fashionItemForConsigns { get; set; } =
            new List<AddFashionItemForConsignRequest>();

        [MaxLength(100, ErrorMessage = "Maximum length is 100 characters")]
        public string? ConsignorName { get; set; }

        [Phone(ErrorMessage = "Invalid phone number")]
        public string? Phone { get; set; }
    }

    public class CreateConsignSaleByShopRequest
    {
        public ConsignSaleType Type { get; set; }
        public string? Consigner { get; set; }
        public required string Phone { get; set; }
        public string? Address { get; set; }
        [EmailAddress] public string? Email { get; set; }

        public List<AddFashionItemForConsignRequest> fashionItemForConsigns { get; set; } =
            new List<AddFashionItemForConsignRequest>();
    }
}