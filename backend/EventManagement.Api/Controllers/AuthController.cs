using EventManagement.Api.Data;
using EventManagement.Api.Dtos;
using EventManagement.Api.Models;
using EventManagement.Api.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventManagement.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ITokenService _tokenService;
    private readonly IPasswordHasher<User> _passwordHasher;

    public AuthController(AppDbContext db, ITokenService tokenService, IPasswordHasher<User> passwordHasher)
    {
        _db = db;
        _tokenService = tokenService;
        _passwordHasher = passwordHasher;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (await _db.Users.AnyAsync(u => u.Email == email))
        {
            return Conflict(new { message = "An account with this email already exists." });
        }

        var role = request.Role == Roles.Organizer ? Roles.Organizer : Roles.Attendee;
        var user = new User
        {
            Email = email,
            FullName = request.FullName.Trim(),
            Role = role
        };
        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return Ok(BuildResponse(user));
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _db.Users.SingleOrDefaultAsync(u => u.Email == email);
        if (user is null)
        {
            return Unauthorized(new { message = "Invalid email or password." });
        }

        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
        {
            return Unauthorized(new { message = "Invalid email or password." });
        }

        if (result == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);
            await _db.SaveChangesAsync();
        }

        return Ok(BuildResponse(user));
    }

    private AuthResponse BuildResponse(User user)
    {
        var (token, expiresAt) = _tokenService.CreateToken(user);
        return new AuthResponse(token, expiresAt, new UserDto(user.Id, user.Email, user.FullName, user.Role));
    }
}
