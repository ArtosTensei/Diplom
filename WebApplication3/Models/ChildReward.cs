using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace WebApplication3.Models
{
    [PrimaryKey(nameof(ChildId), nameof(RewardId))]
    public class ChildReward
    {
        [ForeignKey("Child")]
        public int ChildId { get; set; }

        [ForeignKey("Reward")]
        public int RewardId { get; set; }

        public Child Child { get; set; }
        public Reward Reward { get; set; }
    }
}