using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using server.Models;

namespace server.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        try
        {
            using var scope = serviceProvider.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            await dbContext.Database.MigrateAsync();
            await SeedUsersAsync(userManager);
            await SeedMoviesAsync(dbContext);
        }
        catch (Exception ex)
        {
            var errorLog = $"[{DateTime.Now}]\n{ex}\n";
            await File.WriteAllTextAsync("seed_error.log", errorLog);
            throw;
        }
    }

    private static async Task SeedUsersAsync(UserManager<ApplicationUser> userManager)
    {
        if (userManager.Users.Any()) return;

        var seedUsers = new List<(ApplicationUser User, string Password)>
        {
            (new ApplicationUser
            {
                UserName = "admin@movies.com",
                Email = "admin@movies.com",
                FullName = "Admin User",
                EmailConfirmed = true
            }, "Admin123"),

            (new ApplicationUser
            {
                UserName = "user@movies.com",
                Email = "user@movies.com",
                FullName = "Normal User",
                EmailConfirmed = true
            }, "User1234")
        };

        foreach (var (user, password) in seedUsers)
        {
            var result = await userManager.CreateAsync(user, password);
            if (result.Succeeded)
                Console.WriteLine($"   ✅ Seeded: {user.Email} / {password}");
            else
                Console.WriteLine($"   ❌ Failed: {user.Email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }
    }

    private static async Task SeedMoviesAsync(AppDbContext dbContext)
    {
        if (dbContext.Movies.Any()) return;

        var today = DateTime.UtcNow.Date;
        var movies = new List<Movie>
        {
            new()
            {
                Title = "Dune: Part Three",
                Description = "The epic conclusion of the Dune saga as Paul Atreides faces the consequences of his rise to power across the galaxy.",
                PosterUrl = "https://image.tmdb.org/t/p/w500/d5NXSklXo0qyIYkgV94XAgMIckC.jpg",
                Genre = "Sci-Fi",
                DurationMinutes = 166,
                Rating = "PG-13",
                ReleaseDate = new DateTime(2026, 3, 15, 0, 0, 0, DateTimeKind.Utc),
            },
            new()
            {
                Title = "The Batman: Part II",
                Description = "Bruce Wayne continues his crusade against crime in Gotham City, uncovering a conspiracy that threatens everything he holds dear.",
                PosterUrl = "https://image.tmdb.org/t/p/w500/74xTEgt7R36Fpooo50r9T25onhq.jpg",
                Genre = "Action",
                DurationMinutes = 155,
                Rating = "PG-13",
                ReleaseDate = new DateTime(2026, 10, 2, 0, 0, 0, DateTimeKind.Utc),
            },
            new()
            {
                Title = "Spirited Away: Return to the Spirit World",
                Description = "Chihiro returns to the mysterious spirit world to save her old friend Haku from a new and dangerous threat.",
                PosterUrl = "https://image.tmdb.org/t/p/w500/39wmItIWsg5sZMyRUHLkWBcuVCM.jpg",
                Genre = "Animation",
                DurationMinutes = 130,
                Rating = "PG",
                ReleaseDate = new DateTime(2026, 7, 20, 0, 0, 0, DateTimeKind.Utc),
            },
            new()
            {
                Title = "Interstellar: Beyond",
                Description = "A new crew ventures beyond the known universe following mysterious signals that could reshape humanity's understanding of space-time.",
                PosterUrl = "https://image.tmdb.org/t/p/w500/gEU2QniE6E77NI6lCU6MxlNBvIx.jpg",
                Genre = "Sci-Fi",
                DurationMinutes = 175,
                Rating = "PG-13",
                ReleaseDate = new DateTime(2026, 11, 7, 0, 0, 0, DateTimeKind.Utc),
            },
            new()
            {
                Title = "Ocean's Fourteen",
                Description = "Danny Ocean's legacy lives on as a new team of con artists plans the most ambitious heist the world has ever seen.",
                PosterUrl = "https://image.tmdb.org/t/p/w500/cPUtMOH2ERGY3VPJnZOjNO7L8YC.jpg",
                Genre = "Comedy",
                DurationMinutes = 128,
                Rating = "PG-13",
                ReleaseDate = new DateTime(2026, 6, 12, 0, 0, 0, DateTimeKind.Utc),
            },
            new()
            {
                Title = "A Quiet Place: Day Zero",
                Description = "The origin story reveals the first terrifying day when the alien creatures arrived on Earth and changed humanity forever.",
                PosterUrl = "https://image.tmdb.org/t/p/w500/yrpPYKijwdMHyTGIOd1iK1h0Xno.jpg",
                Genre = "Horror",
                DurationMinutes = 110,
                Rating = "R",
                ReleaseDate = new DateTime(2026, 8, 28, 0, 0, 0, DateTimeKind.Utc),
            },
        };

        dbContext.Movies.AddRange(movies);
        await dbContext.SaveChangesAsync();

        // Seed showtimes for each movie
        var theaters = new[] { "Theater 1", "Theater 2", "Theater 3" };
        var times = new[] { 10, 13, 16, 19, 21 }; // hours

        foreach (var movie in movies)
        {
            for (var day = 0; day < 4; day++)
            {
                var date = today.AddDays(day);
                foreach (var hour in times)
                {
                    var theater = theaters[(hour + day) % theaters.Length];
                    dbContext.Showtimes.Add(new Showtime
                    {
                        MovieId = movie.Id,
                        StartTime = date.AddHours(hour),
                        Theater = theater,
                        Price = hour >= 19 ? 350m : 250m,
                        TotalSeats = 120,
                        AvailableSeats = 120,
                    });
                }
            }
        }

        await dbContext.SaveChangesAsync();
        Console.WriteLine($"   🎬 Seeded {movies.Count} movies with showtimes");
    }
}
