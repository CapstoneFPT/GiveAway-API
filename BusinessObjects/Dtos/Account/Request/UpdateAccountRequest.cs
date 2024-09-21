using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.Dtos.Account.Request
{
    public class UpdateAccountRequest
    {
        [Required, Phone, Length(10,10)]
        public required string Phone { get; set; }

        [Required]
        public required string Fullname { get; set; }
    }
}
