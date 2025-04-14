using System.ComponentModel.DataAnnotations;

namespace WebApplication3.Models
{
    public class EditProfileViewModel
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [EmailAddress]
        public string Email { get; set; } // Keep email editable for now, handle restriction in controller

        public string Role { get; set; } // Add Role to the ViewModel

        public int? Age { get; set; } // Add Age to the ViewModel

        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; }

        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
}