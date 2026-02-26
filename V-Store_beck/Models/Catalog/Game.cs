using System.ComponentModel.DataAnnotations;

namespace VStore.Models.Catalog;

public class Game
{
    public int Id { get; set; }

    [Required]
    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    [Range(0, 100000)]
    public decimal Price { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}