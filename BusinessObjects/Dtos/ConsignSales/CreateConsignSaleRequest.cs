using BusinessObjects.Dtos.Commons;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Dtos.ConsignSaleLineItems;

namespace BusinessObjects.Dtos.ConsignSales
{
    public class CreateConsignSaleRequest
    {
        [Required]
        public string ConsignorName { get; set; }
        [Required]
        public string Phone { get; set; }
        [Required]
        public string Address { get; set; }
        [Required] public ConsignSaleType Type { get; set; }
        [Required] public Guid ShopId { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "You must add at least 1 fashion item")]
        public List<CreateConsignDetailRequest> ConsignDetailRequests { get; set; } =
            new List<CreateConsignDetailRequest>();

        /*[MaxLength(100, ErrorMessage = "Maximum length is 100 characters")]
        public string? ConsignorName { get; set; }

        [Phone(ErrorMessage = "Invalid phone number")]
        public string? Phone { get; set; }*/
    }

    public class CreateConsignSaleByShopRequest
    {
        public ConsignSaleType Type { get; set; }
        public string? ConsignorName { get; set; }
        public required string Phone { get; set; }
        public string? Address { get; set; }
        [EmailAddress] public string? Email { get; set; }
        [Required]
        [MinLength(1, ErrorMessage = "You must add at least 1 fashion item")]
        public List<CreateConsignDetailOfflineRequest> ConsignDetailRequests { get; set; } =
            new List<CreateConsignDetailOfflineRequest>();
    }
    public class CreateConsignForSaleByShopRequest
    {
        public ConsignSaleType Type { get; set; }
        public string? ConsignorName { get; set; }
        public required string Phone { get; set; }
        public string? Address { get; set; }
        [EmailAddress] public string? Email { get; set; }
        [Required]
        [MinLength(1, ErrorMessage = "You must add at least 1 fashion item")]
        public List<CreateConsignForSaleOfflineRequest> ConsignDetailRequests { get; set; } =
            new List<CreateConsignForSaleOfflineRequest>();
    }
}