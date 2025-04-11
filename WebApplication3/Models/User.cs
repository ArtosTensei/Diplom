using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication3.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        public bool IsParent { get; set; }

        public int? ParentId { get; set; }

        [ForeignKey("ParentId")]
        public User Parent { get; set; }

        public List<User> Children { get; set; } = new List<User>();
    }
}