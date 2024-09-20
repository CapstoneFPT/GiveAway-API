using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Dtos.Commons;

namespace BusinessObjects.Dtos.Account.Response
{
    public class AccountResponse
    {
        public Guid AccountId { get; set; }
        [EmailAddress] public string Email { get; set; }
        public string Phone { get; set; }
        public string Fullname { get; set; }
        public Roles Role { get; set; }
        public decimal Balance { get; set; }
        public AccountStatus Status { get; set; }
        public Guid? ShopId { get; set; }
        public string? ShopCode { get; set; }
    }
}
