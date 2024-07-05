using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Dtos.Account.Request
{
    public class ChangePasswordRequest
    {
        [Required]
        public string CurrentPassword { get; set; }
        [Required, MinLength(6, ErrorMessage = "Please enter at least 6 characters")]
        public string NewPassword { get; set; } = string.Empty;
        [Required, Compare("NewPassword")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }
}
