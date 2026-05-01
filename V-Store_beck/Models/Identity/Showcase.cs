// Models/Showcase.cs
namespace AspNetCore.WebAPI.Models
{
    public class Showcase
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public string Type { get; set; } = string.Empty; // illustration, screenshots, inventory, games
        public string Title { get; set; } = string.Empty;
        public int Position { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public List<ShowcaseItem> Items { get; set; } = new();
    }

    public class ShowcaseItem
    {
        public int Id { get; set; }
        public int ShowcaseId { get; set; }
        public Showcase Showcase { get; set; } = null!;
        public int? InventoryItemId { get; set; }
        public InventoryItem? InventoryItem { get; set; }
        public int? ScreenshotId { get; set; }
        public Screenshot? Screenshot { get; set; }
        public int? UserGameId { get; set; }
        public UserGame? UserGame { get; set; }
        public string? IllustrationUrl { get; set; }
        public int Position { get; set; }
    }
}