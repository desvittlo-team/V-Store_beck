using System.ComponentModel.DataAnnotations;

namespace AspNetCore.WebAPI.Models
{
    public class GlobalMessage
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        [Required]
        [MaxLength(500)]
        public string Text { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}