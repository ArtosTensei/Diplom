using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication3.Models
{
    public class Task
    {
        [Key]
        public int Id { get; set; }
        public string Description { get; set; }
        public int Points { get; set; }
        public bool IsCompleted { get; set; }

        [ForeignKey("Child")]
        public int ChildId { get; set; }

        public Child Child { get; set; } // Navigation property
    }
}