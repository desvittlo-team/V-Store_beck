using System.ComponentModel.DataAnnotations;

namespace AspNetCore.WebAPI.Models
{
    public class Game
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Surname { get; set; } = string.Empty;

        public int Age { get; set; }
        public double GPA { get; set; }
        public string Photo { get; set; } = string.Empty;

        public decimal Price { get; set; } = 0m;
    }
}