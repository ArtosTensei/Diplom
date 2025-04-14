using System.ComponentModel.DataAnnotations;

namespace WebApplication3.Models
{
    public class Reward
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        public int Points { get; set; }

        public int ChildId { get; set; }

        public ICollection<ChildReward> ChildRewards { get; set; }
    }
}