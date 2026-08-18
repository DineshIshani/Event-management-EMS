using System.Security.Claims;
using EventManagement.Api.Data;
using EventManagement.Api.Dtos;
using EventManagement.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventManagement.Api.Controllers;

[ApiController]
[Route("api/registrations")]
[Authorize]
public class RegistrationsController : ControllerBase
{
    private readonly AppDbContext _db;

    public RegistrationsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet("me")]
    public async Task<ActionResult<IEnumerable<RegistrationDto>>> GetMyRegistrations()
    {
        var userId = GetUserId();
        var registrations = await _db.Registrations
            .Include(r => r.Event)
            .Include(r => r.User)
            .Where(r => r.UserId == userId)
            .OrderBy(r => r.Event!.StartsAt)
            .ToListAsync();

        return Ok(registrations.Select(ToDto));
    }

    [HttpPost("events/{eventId:int}")]
    public async Task<ActionResult<RegistrationDto>> Register(int eventId)
    {
        var userId = GetUserId();
        var ev = await _db.Events
            .Include(e => e.Registrations)
            .SingleOrDefaultAsync(e => e.Id == eventId);

        if (ev is null)
        {
            return NotFound();
        }

        if (ev.StartsAt <= DateTime.UtcNow)
        {
            return BadRequest(new { message = "This event has already started." });
        }

        var existing = ev.Registrations.SingleOrDefault(r => r.UserId == userId);
        if (existing is { Status: RegistrationStatus.Confirmed })
        {
            return Conflict(new { message = "You are already registered for this event." });
        }

        var confirmed = ev.Registrations.Count(r => r.Status == RegistrationStatus.Confirmed);
        var status = confirmed >= ev.Capacity ? RegistrationStatus.Waitlisted : RegistrationStatus.Confirmed;

        Registration registration;
        if (existing is not null)
        {
            existing.Status = status;
            existing.RegisteredAt = DateTime.UtcNow;
            registration = existing;
        }
        else
        {
            registration = new Registration
            {
                EventId = eventId,
                UserId = userId,
                Status = status
            };
            _db.Registrations.Add(registration);
        }

        await _db.SaveChangesAsync();
        await _db.Entry(registration).Reference(r => r.User).LoadAsync();
        await _db.Entry(registration).Reference(r => r.Event).LoadAsync();

        return Ok(ToDto(registration));
    }

    [HttpDelete("events/{eventId:int}")]
    public async Task<IActionResult> Cancel(int eventId)
    {
        var userId = GetUserId();
        var registration = await _db.Registrations
            .SingleOrDefaultAsync(r => r.EventId == eventId && r.UserId == userId);

        if (registration is null)
        {
            return NotFound();
        }

        registration.Status = RegistrationStatus.Cancelled;
        await _db.SaveChangesAsync();

        await PromoteWaitlistAsync(eventId);
        return NoContent();
    }

    private async Task PromoteWaitlistAsync(int eventId)
    {
        var ev = await _db.Events
            .Include(e => e.Registrations)
            .SingleOrDefaultAsync(e => e.Id == eventId);

        if (ev is null)
        {
            return;
        }

        var confirmed = ev.Registrations.Count(r => r.Status == RegistrationStatus.Confirmed);
        var waitlisted = ev.Registrations
            .Where(r => r.Status == RegistrationStatus.Waitlisted)
            .OrderBy(r => r.RegisteredAt)
            .ToList();

        foreach (var candidate in waitlisted)
        {
            if (confirmed >= ev.Capacity)
            {
                break;
            }

            candidate.Status = RegistrationStatus.Confirmed;
            confirmed++;
        }

        await _db.SaveChangesAsync();
    }

    private int GetUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.Parse(value!);
    }

    internal static RegistrationDto ToDto(Registration r) => new(
        r.Id,
        r.EventId,
        r.Event?.Title ?? string.Empty,
        r.Event?.StartsAt ?? default,
        r.UserId,
        r.User?.FullName ?? string.Empty,
        r.User?.Email ?? string.Empty,
        r.Status.ToString(),
        r.RegisteredAt);
}
