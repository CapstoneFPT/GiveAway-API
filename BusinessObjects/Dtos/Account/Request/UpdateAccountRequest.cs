using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Dtos.Account.Request
{
    public class UpdateAccountRequest
    {
        [Required, Phone]
        public required string Phone { get; set; }

        [Required]
        public required string Fullname { get; set; }
    }
}
