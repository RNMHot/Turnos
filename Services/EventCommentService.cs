using Microsoft.EntityFrameworkCore;
using Turnos.Data;
using Turnos.Models;

namespace Turnos.Services;

public class EventCommentService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private readonly AuditService _audit;

    public EventCommentService(IDbContextFactory<AppDbContext> dbFactory, AuditService audit)
    {
        _dbFactory = dbFactory;
        _audit = audit;
    }

    public async Task<List<EventComment>> GetForEventAsync(int eventId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        return await db.EventComments
            .Include(c => c.Person)
            .Where(c => c.EventId == eventId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<(bool Success, string Error, EventComment? Comment)> AddAsync(int eventId, int personId, string text, string actorUserId)
    {
        if (string.IsNullOrWhiteSpace(text))
            return (false, "El comentario no puede estar vacío.", null);

        await using var db = await _dbFactory.CreateDbContextAsync();
        var comment = new EventComment
        {
            EventId = eventId,
            PersonId = personId,
            Text = text.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        db.EventComments.Add(comment);
        await db.SaveChangesAsync();
        await _audit.LogAsync(actorUserId, "Create", "EventComment", comment.EventCommentId,
            $"Comment added to event {eventId}");

        return (true, string.Empty, comment);
    }
}
