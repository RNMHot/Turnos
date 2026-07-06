using Microsoft.EntityFrameworkCore;
using Turnos.Data;
using Turnos.Models;

namespace Turnos.Services;

public class EventContractService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private readonly AuditService _audit;

    public EventContractService(IDbContextFactory<AppDbContext> dbFactory, AuditService audit)
    {
        _dbFactory = dbFactory;
        _audit = audit;
    }

    public async Task<(string FileName, string ContentType)?> GetMetadataAsync(int eventId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var row = await db.EventContracts.AsNoTracking()
            .Where(c => c.EventId == eventId)
            .Select(c => new { c.FileName, c.ContentType })
            .FirstOrDefaultAsync();

        return row is null ? null : (row.FileName, row.ContentType);
    }

    public async Task<(string FileName, string ContentType, byte[] Data)?> GetDataAsync(int eventId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var row = await db.EventContracts.AsNoTracking()
            .Where(c => c.EventId == eventId)
            .Select(c => new { c.FileName, c.ContentType, c.Data })
            .FirstOrDefaultAsync();

        return row is null ? null : (row.FileName, row.ContentType, row.Data);
    }

    public async Task SaveAsync(int eventId, string fileName, string contentType, byte[] data, string actorUserId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var existing = await db.EventContracts.FirstOrDefaultAsync(c => c.EventId == eventId);
        if (existing is null)
        {
            db.EventContracts.Add(new EventContract
            {
                EventId = eventId,
                FileName = fileName,
                ContentType = contentType,
                Data = data,
                UploadedAt = DateTime.UtcNow
            });
        }
        else
        {
            existing.FileName = fileName;
            existing.ContentType = contentType;
            existing.Data = data;
            existing.UploadedAt = DateTime.UtcNow;
        }

        await db.SaveChangesAsync();
        await _audit.LogAsync(actorUserId, "Update", "EventContract", eventId, $"Contrato actualizado: {fileName}");
    }

    public async Task RemoveAsync(int eventId, string actorUserId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var existing = await db.EventContracts.FirstOrDefaultAsync(c => c.EventId == eventId);
        if (existing is null) return;

        db.EventContracts.Remove(existing);
        await db.SaveChangesAsync();
        await _audit.LogAsync(actorUserId, "Delete", "EventContract", eventId, "Contrato eliminado");
    }
}
