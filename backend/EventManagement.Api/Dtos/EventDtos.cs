using System.ComponentModel.DataAnnotations;

namespace EventManagement.Api.Dtos;

public record EventDto(
    int Id,
    string Title,
    string Description,
    string Location,
    DateTime StartsAt,
    DateTime EndsAt,
    int Capacity,
    int RegisteredCount,
    int SpotsLeft,
    int OrganizerId,
    string OrganizerName,
    bool IsRegistered);

public record EventCreateRequest(
    [Required, MaxLength(200)] string Title,
    [MaxLength(4000)] string? Description,
    [MaxLength(300)] string? Location,
    [Required] DateTime StartsAt,
    [Required] DateTime EndsAt,
    [Range(1, 100000)] int Capacity);

public record EventUpdateRequest(
    [Required, MaxLength(200)] string Title,
    [MaxLength(4000)] string? Description,
    [MaxLength(300)] string? Location,
    [Required] DateTime StartsAt,
    [Required] DateTime EndsAt,
    [Range(1, 100000)] int Capacity);

public record RegistrationDto(
    int Id,
    int EventId,
    string EventTitle,
    DateTime EventStartsAt,
    int UserId,
    string UserFullName,
    string UserEmail,
    string Status,
    DateTime RegisteredAt);
