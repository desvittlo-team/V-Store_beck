namespace AspNetCore.WebAPI.Models
{
    public class UserProfile
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public string? Bio { get; set; }
        public string? BackgroundUrl { get; set; }
        public string? BannerUrl { get; set; }      // новое — шапка

        public bool HideComments { get; set; } = false;
        public bool PrivateLibrary { get; set; } = false;
        public bool ShowOnline { get; set; } = true;

        // новые — скрытие блоков
        public bool HideBadges { get; set; } = false;
        public bool HideGames { get; set; } = false;
        public bool HideDiscussions { get; set; } = false;
        public bool HideFriends { get; set; } = false;
    }
}