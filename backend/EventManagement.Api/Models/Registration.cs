namespace EventManagement.Api.Models;

public enum RegistrationStatus
{
    Confirmed = 0,
    Cancelled = 1,
    Waitlisted = 2
}

public class Registration
{
    public int Id { get; set; }

    public int EventId { get; set; }

    public Event? Event { get; set; }

    public int UserId { get; set; }

    public User? User { get; set; }

    public RegistrationStatus Status { get; set; } = RegistrationStatus.Confirmed;

    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
}
