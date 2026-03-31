namespace AspNetCore.WebAPI.Models
{
    public class InventoryItem
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public int ItemId { get; set; }
        public Item Item { get; set; } = null!;
        public DateTime AcquiredAt { get; set; } = DateTime.UtcNow;
    }
}