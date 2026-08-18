using System.ComponentModel.DataAnnotations;

namespace EventManagement.Api.Dtos;

public record RegisterRequest(
    [Required, EmailAddress, MaxLength(200)] string Email,
    [Required, MaxLength(200)] string FullName,
    [Required, MinLength(8), MaxLength(100)] string Password,
    string? Role);

public record LoginRequest(
    [Required, EmailAddress] string Email,
    [Required] string Password);

public record AuthResponse(string Token, DateTime ExpiresAt, UserDto User);

public record UserDto(int Id, string Email, string FullName, string Role);
