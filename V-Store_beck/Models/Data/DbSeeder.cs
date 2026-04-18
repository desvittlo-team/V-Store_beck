using AspNetCore.WebAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace AspNetCore.WebAPI.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            var admin = await context.Users
                .FirstOrDefaultAsync(u => u.Username == "admin");

            if (admin == null)
            {
                context.Users.Add(new User
                {
                    Username = "admin",
                    Email = "admin@admin.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                    Role = "Admin"
                });
                Console.WriteLine(">>> Админ создан!");
            }
            else
            {
                // Принудительно обновляем пароль и email
                admin.Email = "admin@admin.com";
                admin.PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123");
                Console.WriteLine(">>> Пароль админа обновлён!");
            }

            await context.SaveChangesAsync();
        }
    }
}