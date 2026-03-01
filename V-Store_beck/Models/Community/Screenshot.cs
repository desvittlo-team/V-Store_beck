using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AspNetCore.WebAPI.Models
{
    public class Screenshot
    {
        public int Id { get; set; }

        [Required]
        public string ImageUrl { get; set; } = string.Empty;

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Author { get; set; } = string.Empty;

        public int Rating { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public List<Comment> Comments { get; set; } = new List<Comment>();
    }
}
