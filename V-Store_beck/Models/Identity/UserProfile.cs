namespace AspNetCore.WebAPI.Models
{
    public class UserProfile
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public string? Bio { get; set; }
        public string? BackgroundUrl { get; set; }
        public bool HideComments { get; set; } = false;
        public bool PrivateLibrary { get; set; } = false;
        public bool ShowOnline { get; set; } = true;
    }
}