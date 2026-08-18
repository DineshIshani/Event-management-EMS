using System.ComponentModel.DataAnnotations;

namespace EventManagement.Api.Models;

public class Event
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(4000)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(300)]
    public string Location { get; set; } = string.Empty;

    public DateTime StartsAt { get; set; }

    public DateTime EndsAt { get; set; }

    public int Capacity { get; set; }

    public int OrganizerId { get; set; }

    public User? Organizer { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Registration> Registrations { get; set; } = new List<Registration>();
}
