using System.Security.Claims;
using EventManagement.Api.Data;
using EventManagement.Api.Dtos;
using EventManagement.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventManagement.Api.Controllers;

[ApiController]
[Route("api/events")]
public class EventsController : ControllerBase
{
    private readonly AppDbContext _db;

    public EventsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<EventDto>>> GetEvents([FromQuery] string? search, [FromQuery] bool upcomingOnly = false)
    {
        var query = _db.Events.Include(e => e.Organizer).Include(e => e.Registrations).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(e => EF.Functions.Like(e.Title, $"%{term}%") || EF.Functions.Like(e.Location, $"%{term}%"));
        }

        if (upcomingOnly)
        {
            var now = DateTime.UtcNow;
            query = query.Where(e => e.StartsAt >= now);
        }

        var events = await query.OrderBy(e => e.StartsAt).ToListAsync();
        var userId = GetUserId();
        return Ok(events.Select(e => ToDto(e, userId)));
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<ActionResult<EventDto>> GetEvent(int id)
    {
        var ev = await _db.Events
            .Include(e => e.Organizer)
            .Include(e => e.Registrations)
            .SingleOrDefaultAsync(e => e.Id == id);

        if (ev is null)
        {
            return NotFound();
        }

        return Ok(ToDto(ev, GetUserId()));
    }

    [HttpPost]
    [Authorize(Roles = Roles.Organizer)]
    public async Task<ActionResult<EventDto>> CreateEvent(EventCreateRequest request)
    {
        if (request.EndsAt <= request.StartsAt)
        {
            return BadRequest(new { message = "End time must be after the start time." });
        }

        var organizerId = GetUserId();
        if (organizerId is null)
        {
            return Unauthorized();
        }

        var ev = new Event
        {
            Title = request.Title.Trim(),
            Description = request.Description?.Trim() ?? string.Empty,
            Location = request.Location?.Trim() ?? string.Empty,
            StartsAt = request.StartsAt,
            EndsAt = request.EndsAt,
            Capacity = request.Capacity,
            OrganizerId = organizerId.Value
        };

        _db.Events.Add(ev);
        await _db.SaveChangesAsync();
        await _db.Entry(ev).Reference(e => e.Organizer).LoadAsync();

        return CreatedAtAction(nameof(GetEvent), new { id = ev.Id }, ToDto(ev, organizerId));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.Organizer)]
    public async Task<ActionResult<EventDto>> UpdateEvent(int id, EventUpdateRequest request)
    {
        if (request.EndsAt <= request.StartsAt)
        {
            return BadRequest(new { message = "End time must be after the start time." });
        }

        var ev = await _db.Events
            .Include(e => e.Organizer)
            .Include(e => e.Registrations)
            .SingleOrDefaultAsync(e => e.Id == id);

        if (ev is null)
        {
            return NotFound();
        }

        var userId = GetUserId();
        if (ev.OrganizerId != userId)
        {
            return Forbid();
        }

        var confirmed = ev.Registrations.Count(r => r.Status == RegistrationStatus.Confirmed);
        if (request.Capacity < confirmed)
        {
            return BadRequest(new { message = $"Capacity cannot be lower than the {confirmed} confirmed registrations." });
        }

        ev.Title = request.Title.Trim();
        ev.Description = request.Description?.Trim() ?? string.Empty;
        ev.Location = request.Location?.Trim() ?? string.Empty;
        ev.StartsAt = request.StartsAt;
        ev.EndsAt = request.EndsAt;
        ev.Capacity = request.Capacity;

        await _db.SaveChangesAsync();
        return Ok(ToDto(ev, userId));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.Organizer)]
    public async Task<IActionResult> DeleteEvent(int id)
    {
        var ev = await _db.Events.SingleOrDefaultAsync(e => e.Id == id);
        if (ev is null)
        {
            return NotFound();
        }

        if (ev.OrganizerId != GetUserId())
        {
            return Forbid();
        }

        _db.Events.Remove(ev);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("{id:int}/registrations")]
    [Authorize(Roles = Roles.Organizer)]
    public async Task<ActionResult<IEnumerable<RegistrationDto>>> GetEventRegistrations(int id)
    {
        var ev = await _db.Events.SingleOrDefaultAsync(e => e.Id == id);
        if (ev is null)
        {
            return NotFound();
        }

        if (ev.OrganizerId != GetUserId())
        {
            return Forbid();
        }

        var registrations = await _db.Registrations
            .Include(r => r.User)
            .Include(r => r.Event)
            .Where(r => r.EventId == id)
            .OrderBy(r => r.RegisteredAt)
            .ToListAsync();

        return Ok(registrations.Select(RegistrationsController.ToDto));
    }

    private int? GetUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var id) ? id : null;
    }

    internal static EventDto ToDto(Event ev, int? currentUserId)
    {
        var confirmed = ev.Registrations.Count(r => r.Status == RegistrationStatus.Confirmed);
        return new EventDto(
            ev.Id,
            ev.Title,
            ev.Description,
            ev.Location,
            ev.StartsAt,
            ev.EndsAt,
            ev.Capacity,
            confirmed,
            Math.Max(0, ev.Capacity - confirmed),
            ev.OrganizerId,
            ev.Organizer?.FullName ?? string.Empty,
            currentUserId is not null && ev.Registrations.Any(r => r.UserId == currentUserId && r.Status == RegistrationStatus.Confirmed));
    }
}
