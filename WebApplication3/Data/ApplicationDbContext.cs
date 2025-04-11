using Microsoft.EntityFrameworkCore;
using WebApplication3.Models;
using Microsoft.Extensions.Configuration;

namespace WebApplication3.Data
{
    public class ApplicationDbContext : DbContext
    {
        private readonly IConfiguration _configuration;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IConfiguration configuration)
            : base(options)
        {
            _configuration = configuration;
        }

        public DbSet<Parent> Parents { get; set; }
        public DbSet<Child> Children { get; set; }
        public DbSet<WebApplication3.Models.Task> Tasks { get; set; }
        public DbSet<Reward> Rewards { get; set; }
        public DbSet<ChildReward> ChildRewards { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseNpgsql(_configuration.GetConnectionString("DefaultConnection"));
            }
        }
    }
}