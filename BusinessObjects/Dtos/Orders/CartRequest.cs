using BusinessObjects.Dtos.Commons;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Dtos.Orders
{
    public class CartRequest
    {
        public PaymentMethod PaymentMethod { get; set; }
        [Required]
        public string Address { get; set; }
        public int? GhnDistrictId { get; set; }
        public int? GhnWardCode { get; set; }
        public int? GhnProvinceId { get; set; }
        public AddressType? AddressType { get; set; }
        public string RecipientName { get; set; }
        [Phone]
        public string? Phone { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal Discount { get; set; }
        public List<CartItem> CartItems { get; set; } = [];
    }

    public class CartItem
    {
        public Guid ItemId { get; set; }
        // public int Quantity { get; set; } = 1;
    }
}
