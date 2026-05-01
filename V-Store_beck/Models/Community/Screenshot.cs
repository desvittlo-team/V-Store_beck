namespace AspNetCore.WebAPI.Models
{
    public class Screenshot
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public int? GameId { get; set; }   
        public Game? Game { get; set; }    
        public string FileName { get; set; } = string.Empty;
        public string? Caption { get; set; }
        public int Likes { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}