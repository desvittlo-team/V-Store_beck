// Models/Identity/User.cs
using System.ComponentModel.DataAnnotations;

namespace AspNetCore.WebAPI.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        public string Role { get; set; } = "User";

        public string Photo { get; set; } = "User.png";
    }
}