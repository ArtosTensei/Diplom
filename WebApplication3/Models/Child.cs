using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication3.Models
{
    public class Child
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public int Points { get; set; }

        public int Age { get; set; }

        [ForeignKey("Parent")]
        public int ParentId { get; set; }

        public Parent Parent { get; set; }

        public ICollection<ChildReward> ChildRewards { get; set; }
    }
}