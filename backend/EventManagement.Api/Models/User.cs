using System.ComponentModel.DataAnnotations;

namespace EventManagement.Api.Models;

public class User
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [MaxLength(20)]
    public string Role { get; set; } = Roles.Attendee;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Event> OrganizedEvents { get; set; } = new List<Event>();

    public ICollection<Registration> Registrations { get; set; } = new List<Registration>();
}

public static class Roles
{
    public const string Organizer = "Organizer";
    public const string Attendee = "Attendee";
}
