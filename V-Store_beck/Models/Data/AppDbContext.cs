// Data/AppDbContext.cs

using Microsoft.EntityFrameworkCore;
using AspNetCore.WebAPI.Models;
using Microsoft.EntityFrameworkCore.Diagnostics;

//using VStore.Models.Catalog;

namespace AspNetCore.WebAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<UserGame> UserGames { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Game> Game { get; set; }
        public DbSet<Screenshot> Screenshots { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<GlobalMessage> GlobalMessages { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<InventoryItem> InventoryItems { get; set; }
        public DbSet<WishlistItem> WishlistItems { get; set; }

        public DbSet<Message> Messages { get; set; }  // <- добавь эту ст
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Seed data for Students
            modelBuilder.Entity<Game>().HasData(
                new Game { Id = 1, Name = "Elden Ring", Surname = "FromSoftware", Age = 52, GPA = 10.5, Photo = "eldenring.png", Price = 59.99m },
                new Game { Id = 2, Name = "Cyberpunk 2077", Surname = "CD Projekt", Age = 70, GPA = 9.8, Photo = "cyberpunk2077.png", Price = 49.99m },
                new Game { Id = 3, Name = "Dota 2", Surname = "Valve", Age = 15, GPA = 9.5, Photo = "dota2.png", Price = 0m },
                new Game { Id = 4, Name = "Counter-Strike: Global Offensive", Surname = "Valve", Age = 20, GPA = 9.7, Photo = "csgo.png", Price = 0m },
                new Game { Id = 5, Name = "The Witcher 3: Wild Hunt", Surname = "CD Projekt", Age = 50, GPA = 9.9, Photo = "witcher3.png", Price = 39.99m },
                new Game { Id = 6, Name = "Hades", Surname = "Supergiant Games", Age = 15, GPA = 9.6, Photo = "hades.png", Price = 24.99m },
                new Game { Id = 7, Name = "Stardew Valley", Surname = "ConcernedApe", Age = 1, GPA = 9.4, Photo = "stardew.png", Price = 14.99m },
                new Game { Id = 8, Name = "Terraria", Surname = "Re-Logic", Age = 0, GPA = 9.3, Photo = "terraria.png", Price = 9.99m },
                new Game { Id = 9, Name = "Red Dead Redemption 2", Surname = "Rockstar Games", Age = 95, GPA = 10.3, Photo = "rdr2.png", Price = 59.99m },
                new Game { Id = 10, Name = "The Legend of Zelda: BoTW", Surname = "Nintendo", Age = 99, GPA = 10.4, Photo = "zelda_botw.png", Price = 59.99m },
                new Game { Id = 11, Name = "Grand Theft Auto V", Surname = "Rockstar Games", Age = 93, GPA = 9.2, Photo = "gtav.png", Price = 29.99m },
                new Game { Id = 12, Name = "Minecraft", Surname = "Mojang", Age = 100, GPA = 9.0, Photo = "minecraft.png", Price = 26.99m },
                new Game { Id = 13, Name = "Half-Life 2", Surname = "Valve", Age = 94, GPA = 10.0, Photo = "half_life2.png", Price = 9.99m },
                new Game { Id = 14, Name = "Portal 2", Surname = "Valve", Age = 90, GPA = 9.8, Photo = "portal2.png", Price = 9.99m },
                new Game { Id = 15, Name = "The Elder Scrolls V: Skyrim", Surname = "Bethesda Game Studios", Age = 96, GPA = 9.6, Photo = "skyrim.png", Price = 39.99m },
                new Game { Id = 16, Name = "Mass Effect 2", Surname = "BioWare", Age = 88, GPA = 9.9, Photo = "masseffect2.png", Price = 19.99m },
                new Game { Id = 17, Name = "Diablo II", Surname = "Blizzard Entertainment", Age = 82, GPA = 9.4, Photo = "diablo2.png", Price = 19.99m },
                new Game { Id = 18, Name = "Doom (1993)", Surname = "id Software", Age = 85, GPA = 9.1, Photo = "doom93.png", Price = 4.99m },
                new Game { Id = 19, Name = "Resident Evil 4", Surname = "Capcom", Age = 87, GPA = 9.7, Photo = "re4.png", Price = 29.99m },
                new Game { Id = 20, Name = "Super Mario World", Surname = "Nintendo EAD", Age = 97, GPA = 10.1, Photo = "smw.png", Price = 19.99m },
                new Game { Id = 21, Name = "Tetris", Surname = "Alexey Pajitnov", Age = 100, GPA = 8.9, Photo = "tetris.png", Price = 4.99m },
                new Game { Id = 22, Name = "Final Fantasy VII", Surname = "Square", Age = 95, GPA = 9.5, Photo = "ff7.png", Price = 15.99m },
                new Game { Id = 23, Name = "Metal Gear Solid", Surname = "Konami", Age = 89, GPA = 9.7, Photo = "mgs.png", Price = 19.99m },
                new Game { Id = 24, Name = "World of Warcraft", Surname = "Blizzard Entertainment", Age = 94, GPA = 9.3, Photo = "wow.png", Price = 14.99m },
                new Game { Id = 25, Name = "BioShock", Surname = "Irrational Games", Age = 86, GPA = 9.6, Photo = "bioshock.png", Price = 19.99m },
                new Game { Id = 26, Name = "StarCraft", Surname = "Blizzard Entertainment", Age = 81, GPA = 9.0, Photo = "starcraft.png", Price = 14.99m },
                new Game { Id = 27, Name = "SimCity 2000", Surname = "Maxis", Age = 78, GPA = 8.8, Photo = "simcity2000.png", Price = 9.99m },
                new Game { Id = 28, Name = "Baldur's Gate 3", Surname = "Larian Studios", Age = 97, GPA = 10.4, Photo = "bg3.png", Price = 59.99m },
                new Game { Id = 29, Name = "Hollow Knight", Surname = "Team Cherry", Age = 84, GPA = 9.5, Photo = "hollow_knight.png", Price = 14.99m },
                new Game { Id = 30, Name = "Shadow of the Colossus", Surname = "Team Ico", Age = 88, GPA = 9.7, Photo = "sotc.png", Price = 19.99m },
                new Game { Id = 31, Name = "Pokemon Red/Blue", Surname = "Game Freak", Age = 91, GPA = 9.1, Photo = "pokemon_rb.png", Price = 9.99m },
                new Game { Id = 32, Name = "Super Metroid", Surname = "Nintendo R&D1", Age = 93, GPA = 10.0, Photo = "super_metroid.png", Price = 9.99m },
                new Game { Id = 33, Name = "The Last of Us", Surname = "Naughty Dog", Age = 95, GPA = 9.9, Photo = "tlou.png", Price = 39.99m },
                new Game { Id = 34, Name = "Castlevania: SotN", Surname = "Konami", Age = 87, GPA = 9.5, Photo = "sotn.png", Price = 9.99m },
                new Game { Id = 35, Name = "Monkey Island 2", Surname = "LucasArts", Age = 80, GPA = 9.3, Photo = "mi2.png", Price = 9.99m },
                new Game { Id = 36, Name = "Uncharted 2: Among Thieves", Surname = "Naughty Dog", Age = 86, GPA = 9.6, Photo = "uncharted2.png", Price = 19.99m },
                new Game { Id = 37, Name = "Dishonored", Surname = "Arkane Studios", Age = 79, GPA = 9.4, Photo = "dishonored.png", Price = 19.99m },
                new Game { Id = 38, Name = "Disco Elysium", Surname = "ZA/UM", Age = 91, GPA = 10.1, Photo = "disco_elysium.png", Price = 29.99m },
                new Game { Id = 39, Name = "Fallout: New Vegas", Surname = "Obsidian Entertainment", Age = 83, GPA = 9.2, Photo = "new_vegas.png", Price = 9.99m },
                new Game { Id = 40, Name = "Deus Ex", Surname = "Ion Storm", Age = 85, GPA = 9.5, Photo = "deus_ex.png", Price = 9.99m },
                new Game { Id = 41, Name = "Civilization VI", Surname = "Firaxis Games", Age = 70, GPA = 9.0, Photo = "civ6.png", Price = 29.99m },
                new Game { Id = 42, Name = "Rocket League", Surname = "Psyonix", Age = 68, GPA = 8.9, Photo = "rocket_league.png", Price = 0m },
                new Game { Id = 43, Name = "Inside", Surname = "Playdead", Age = 75, GPA = 9.3, Photo = "inside.png", Price = 19.99m },
                new Game { Id = 44, Name = "Street Fighter II", Surname = "Capcom", Age = 90, GPA = 9.6, Photo = "sf2.png", Price = 14.99m },
                new Game { Id = 45, Name = "The Sims", Surname = "Maxis", Age = 72, GPA = 8.7, Photo = "the_sims.png", Price = 19.99m },
                new Game { Id = 46, Name = "Doom Eternal", Surname = "id Software", Age = 78, GPA = 9.8, Photo = "doom_eternal.png", Price = 39.99m },
                new Game { Id = 47, Name = "Journey", Surname = "Thatgamecompany", Age = 76, GPA = 9.4, Photo = "journey.png", Price = 14.99m },
                new Game { Id = 48, Name = "Outer Wilds", Surname = "Mobius Digital", Age = 89, GPA = 9.9, Photo = "outer_wilds.png", Price = 24.99m },
                new Game { Id = 49, Name = "Forza Horizon 5", Surname = "Playground Games", Age = 65, GPA = 9.2, Photo = "forza5.png", Price = 59.99m },
                new Game { Id = 50, Name = "Gran Turismo 7", Surname = "Polyphony Digital", Age = 58, GPA = 8.7, Photo = "gt7.png", Price = 69.99m },
                new Game { Id = 51, Name = "Need for Speed: Underground 2", Surname = "EA Black Box", Age = 85, GPA = 9.5, Photo = "nfsu2.png", Price = 9.99m },
                new Game { Id = 52, Name = "Silent Hill 2", Surname = "Team Silent", Age = 92, GPA = 9.8, Photo = "sh2.png", Price = 14.99m },
                new Game { Id = 53, Name = "Amnesia: The Dark Descent", Surname = "Frictional Games", Age = 74, GPA = 9.0, Photo = "amnesia.png", Price = 19.99m },
                new Game { Id = 54, Name = "Phasmophobia", Surname = "Kinetic Games", Age = 25, GPA = 8.8, Photo = "phasmophobia.png", Price = 13.99m },
                new Game { Id = 55, Name = "Celeste", Surname = "Extremely OK Games", Age = 45, GPA = 9.7, Photo = "celeste.png", Price = 19.99m },
                new Game { Id = 56, Name = "Undertale", Surname = "Toby Fox", Age = 66, GPA = 9.9, Photo = "undertale.png", Price = 9.99m },
                new Game { Id = 57, Name = "Persona 5 Royal", Surname = "Atlus", Age = 55, GPA = 9.8, Photo = "p5r.png", Price = 59.99m },
                new Game { Id = 58, Name = "NieR:Automata", Surname = "PlatinumGames", Age = 60, GPA = 9.4, Photo = "nier.png", Price = 39.99m },
                new Game { Id = 59, Name = "God of War", Surname = "Santa Monica Studio", Age = 82, GPA = 9.9, Photo = "gow.png", Price = 49.99m },
                new Game { Id = 60, Name = "Tony Hawk's Pro Skater 1+2", Surname = "Vicarious Visions", Age = 50, GPA = 9.1, Photo = "thps.png", Price = 39.99m },
                new Game { Id = 61, Name = "EA SPORTS FC 24", Surname = "EA Sports", Age = 10, GPA = 7.5, Photo = "fc24.png", Price = 69.99m },
                new Game { Id = 62, Name = "Cities: Skylines", Surname = "Colossal Order", Age = 68, GPA = 8.9, Photo = "cities_skylines.png", Price = 29.99m },
                new Game { Id = 63, Name = "Euro Truck Simulator 2", Surname = "SCS Software", Age = 75, GPA = 9.6, Photo = "ets2.png", Price = 19.99m },
                new Game { Id = 64, Name = "Microsoft Flight Simulator", Surname = "Asobo Studio", Age = 40, GPA = 9.2, Photo = "mfs.png", Price = 59.99m },
                new Game { Id = 65, Name = "Subnautica", Surname = "Unknown Worlds", Age = 53, GPA = 9.5, Photo = "subnautica.png", Price = 29.99m },
                new Game { Id = 66, Name = "Don't Starve Together", Surname = "Klei Entertainment", Age = 61, GPA = 9.3, Photo = "dont_starve.png", Price = 14.99m },
                new Game { Id = 67, Name = "Dead Cells", Surname = "Motion Twin", Age = 48, GPA = 9.4, Photo = "dead_cells.png", Price = 24.99m },
                new Game { Id = 68, Name = "Slay the Spire", Surname = "Mega Crit Games", Age = 38, GPA = 9.7, Photo = "slaythespire.png", Price = 24.99m },
                new Game { Id = 69, Name = "Beat Saber", Surname = "Beat Games", Age = 42, GPA = 9.5, Photo = "beatsaber.png", Price = 29.99m },
                new Game { Id = 70, Name = "Among Us", Surname = "Innersloth", Age = 12, GPA = 8.5, Photo = "amongus.png", Price = 4.99m },
                new Game { Id = 71, Name = "Fall Guys", Surname = "Mediatonic", Age = 18, GPA = 8.2, Photo = "fallguys.png", Price = 0m },
                new Game { Id = 72, Name = "Super Smash Bros. Ultimate", Surname = "Bandai Namco", Age = 77, GPA = 9.6, Photo = "ssbu.png", Price = 59.99m },
                new Game { Id = 73, Name = "Tekken 8", Surname = "Bandai Namco", Age = 5, GPA = 9.1, Photo = "tekken8.png", Price = 69.99m },
                new Game { Id = 74, Name = "Mortal Kombat 1", Surname = "NetherRealm Studios", Age = 8, GPA = 8.6, Photo = "mk1.png", Price = 69.99m },
                new Game { Id = 75, Name = "Genshin Impact", Surname = "HoYoverse", Age = 22, GPA = 8.8, Photo = "genshin.png", Price = 0m },
                new Game { Id = 76, Name = "League of Legends", Surname = "Riot Games", Age = 85, GPA = 9.0, Photo = "lol.png", Price = 0m },
                new Game { Id = 77, Name = "Valorant", Surname = "Riot Games", Age = 30, GPA = 8.9, Photo = "valorant.png", Price = 0m },
                new Game { Id = 78, Name = "Overwatch 2", Surname = "Blizzard Entertainment", Age = 15, GPA = 7.9, Photo = "ow2.png", Price = 0m },
                new Game { Id = 79, Name = "Apex Legends", Surname = "Respawn Entertainment", Age = 35, GPA = 8.7, Photo = "apex.png", Price = 0m },
                new Game { Id = 80, Name = "Fortnite", Surname = "Epic Games", Age = 45, GPA = 9.1, Photo = "fortnite.png", Price = 0m },
                new Game { Id = 81, Name = "Return of the Obra Dinn", Surname = "Lucas Pope", Age = 55, GPA = 9.8, Photo = "obradinn.png", Price = 19.99m },
                new Game { Id = 82, Name = "The Witness", Surname = "Thekla, Inc.", Age = 62, GPA = 8.9, Photo = "thewitness.png", Price = 39.99m },
                new Game { Id = 83, Name = "The Talos Principle", Surname = "Croteam", Age = 58, GPA = 9.3, Photo = "talos.png", Price = 29.99m },
                new Game { Id = 84, Name = "Age of Empires II: Def. Ed.", Surname = "Forgotten Empires", Age = 88, GPA = 9.6, Photo = "aoe2.png", Price = 19.99m },
                new Game { Id = 85, Name = "XCOM 2", Surname = "Firaxis Games", Age = 71, GPA = 9.2, Photo = "xcom2.png", Price = 59.99m },
                new Game { Id = 86, Name = "Darkest Dungeon", Surname = "Red Hook Studios", Age = 64, GPA = 9.1, Photo = "darkestdungeon.png", Price = 24.99m },
                new Game { Id = 87, Name = "Spelunky 2", Surname = "Mossmouth", Age = 33, GPA = 9.4, Photo = "spelunky2.png", Price = 19.99m },
                new Game { Id = 88, Name = "Cuphead", Surname = "Studio MDHR", Age = 52, GPA = 9.6, Photo = "cuphead.png", Price = 19.99m },
                new Game { Id = 89, Name = "It Takes Two", Surname = "Hazelight Studios", Age = 41, GPA = 9.7, Photo = "ittakestwo.png", Price = 39.99m },
                new Game { Id = 90, Name = "Factorio", Surname = "Wube Software", Age = 80, GPA = 9.9, Photo = "factorio.png", Price = 35.00m }
            );
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Username = "admin",
                    Email = "admin@vstore.com",
                    // Use a static precomputed hash here to avoid dynamic values in HasData
                    PasswordHash = "$2a$11$KIX7YB1r0A2Gx6GxVqQe6OLt6qj1Q6i8zB8uYwYh9a1b2c3d4e5fO",
                    Role = "Admin",
                    Photo = "User.png"

                }
            );
            modelBuilder.Entity<Message>()
                .HasOne(m => m.Sender)
                .WithMany()
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

             modelBuilder.Entity<Message>()
                .HasOne(m => m.Receiver)
                .WithMany()
                .HasForeignKey(m => m.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Item>()
                .HasOne(i => i.Seller)
                .WithMany()
                .HasForeignKey(i => i.SellerId)
                .OnDelete(DeleteBehavior.Restrict);

             modelBuilder.Entity<Item>()
                .HasOne(i => i.Game)
                .WithMany()
                .HasForeignKey(i => i.GameId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Message>()
                .HasOne(m => m.InventoryItem)
                .WithMany()
                .HasForeignKey(m => m.ItemId)
                .OnDelete(DeleteBehavior.SetNull);
        }

    }
}