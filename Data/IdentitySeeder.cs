using Air.Data;
using Air.Models;

public static class DbSeeder
{
    public static void Seed(ApplicationDbContext context)
    {
        context.Database.EnsureCreated();

        if (!context.Users.Any(u => u.Email == "saimonemon46@gmail.com"))
        {
            context.Users.Add(new User
            {
                Username = "Admin",
                Email = "saimonemon46@gmail.com",
                Password = "123456",
                Phone = "01622015799"
            });

            context.SaveChanges();
        }
    }
}
