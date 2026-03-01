using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AspNetCore.WebAPI.Models
{
    public class Comment
    {
        public int Id { get; set; }

        [Required]
        public string Author { get; set; } = string.Empty;

        [Required]
        public string Text { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int ScreenshotId { get; set; }

        [JsonIgnore]
        public Screenshot? Screenshot { get; set; }
    }
}
