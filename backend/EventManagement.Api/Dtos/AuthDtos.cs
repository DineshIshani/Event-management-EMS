using System.ComponentModel.DataAnnotations;

namespace EventManagement.Api.Dtos;

public record RegisterRequest(
    [property: Required, EmailAddress, MaxLength(200)] string Email,
    [property: Required, MaxLength(200)] string FullName,
    [property: Required, MinLength(8), MaxLength(100)] string Password,
    string? Role);

public record LoginRequest(
    [property: Required, EmailAddress] string Email,
    [property: Required] string Password);

public record AuthResponse(string Token, DateTime ExpiresAt, UserDto User);

public record UserDto(int Id, string Email, string FullName, string Role);
