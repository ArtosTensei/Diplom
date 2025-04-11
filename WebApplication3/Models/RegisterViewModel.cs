using System.ComponentModel.DataAnnotations;

namespace WebApplication3.Models
{
    public class RegisterViewModel
    {
        [Required]
        [Display(Name = "Имя пользователя")]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Пароль должен содержать не менее {2} символов.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Подтвердите пароль")]
        [Compare("Password", ErrorMessage = "Пароли не совпадают.")]
        public string ConfirmPassword { get; set; }

        [Required]
        [Display(Name = "Роль")]
        public string Role { get; set; } // "Parent" or "Child"

        [Required(ErrorMessage = "Возраст обязателен для заполнения.")]
        [Range(0, 120, ErrorMessage = "Введите корректный возраст.")]
        [Display(Name = "Возраст")]
        public int Age { get; set; }

        [EmailAddress(ErrorMessage = "Некорректный формат email.")]
        [Display(Name = "Email родителя")]
        public string ParentEmail { get; set; }
    }
}