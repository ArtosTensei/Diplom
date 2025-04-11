namespace WebApplication3.Models
{
    public class Task
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Deadline { get; set; }
        public int Reward { get; set; }
        public string? Status { get; set; }
    }
}