using EventManagement.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EventManagement.Api.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db, IPasswordHasher<User> passwordHasher)
    {
        if (await db.Users.AnyAsync())
        {
            return;
        }

        var organizer = new User
        {
            Email = "organizer@example.com",
            FullName = "Olivia Organizer",
            Role = Roles.Organizer
        };
        organizer.PasswordHash = passwordHasher.HashPassword(organizer, "Password123!");

        var attendee = new User
        {
            Email = "attendee@example.com",
            FullName = "Alex Attendee",
            Role = Roles.Attendee
        };
        attendee.PasswordHash = passwordHasher.HashPassword(attendee, "Password123!");

        db.Users.AddRange(organizer, attendee);
        await db.SaveChangesAsync();

        var now = DateTime.UtcNow;
        db.Events.AddRange(
            new Event
            {
                Title = "Angular & .NET Meetup",
                Description = "Monthly meetup covering Angular 18 signals and minimal APIs in .NET 8.",
                Location = "Bengaluru",
                StartsAt = now.AddDays(7),
                EndsAt = now.AddDays(7).AddHours(3),
                Capacity = 100,
                OrganizerId = organizer.Id
            },
            new Event
            {
                Title = "Cloud Native Conference",
                Description = "Two-day conference on Kubernetes, observability and platform engineering.",
                Location = "Hyderabad",
                StartsAt = now.AddDays(30),
                EndsAt = now.AddDays(31),
                Capacity = 2,
                OrganizerId = organizer.Id
            });

        await db.SaveChangesAsync();
    }
}
