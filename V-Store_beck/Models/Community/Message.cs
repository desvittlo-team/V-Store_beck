using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AspNetCore.WebAPI.Models
{
    public class Message
    {
        public int Id { get; set; }

        public int SenderId { get; set; }

        [ForeignKey("SenderId")]
        public User Sender { get; set; } = null!;

        public int ReceiverId { get; set; }

        [ForeignKey("ReceiverId")]
        public User Receiver { get; set; } = null!;

        [Required]
        [MaxLength(1000)]
        public string Text { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsRead { get; set; } = false;
    }
}