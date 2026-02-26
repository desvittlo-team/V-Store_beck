using System.ComponentModel.DataAnnotations;

namespace VStore.Models.Identity;

public class User
{
    public int Id { get; set; }

    [Required]
    public string Username { get; set; } = null!;

    [Required]
    public string Email { get; set; } = null!;

    [Required]
    public string PasswordHash { get; set; } = null!;

    public string Role { get; set; } = "User";

    public bool IsBlocked { get; set; } = false;
}