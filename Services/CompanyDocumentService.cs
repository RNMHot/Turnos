using Microsoft.EntityFrameworkCore;
using Turnos.Data;
using Turnos.Models;

namespace Turnos.Services;

public class CompanyDocumentService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private readonly AuditService _audit;

    public CompanyDocumentService(IDbContextFactory<AppDbContext> dbFactory, AuditService audit)
    {
        _dbFactory = dbFactory;
        _audit = audit;
    }

    public async Task<(string FileName, string ContentType)?> GetMetadataAsync(int companyId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var row = await db.CompanyDocuments.AsNoTracking()
            .Where(d => d.CompanyId == companyId)
            .Select(d => new { d.FileName, d.ContentType })
            .FirstOrDefaultAsync();

        return row is null ? null : (row.FileName, row.ContentType);
    }

    public async Task<(string FileName, string ContentType, byte[] Data)?> GetDataAsync(int companyId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var row = await db.CompanyDocuments.AsNoTracking()
            .Where(d => d.CompanyId == companyId)
            .Select(d => new { d.FileName, d.ContentType, d.Data })
            .FirstOrDefaultAsync();

        return row is null ? null : (row.FileName, row.ContentType, row.Data);
    }

    public async Task SaveAsync(int companyId, string fileName, string contentType, byte[] data, string actorUserId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var existing = await db.CompanyDocuments.FirstOrDefaultAsync(d => d.CompanyId == companyId);
        if (existing is null)
        {
            db.CompanyDocuments.Add(new CompanyDocument
            {
                CompanyId = companyId,
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
        await _audit.LogAsync(actorUserId, "Update", "CompanyDocument", companyId, $"Registro de comerciante actualizado: {fileName}");
    }

    public async Task RemoveAsync(int companyId, string actorUserId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var existing = await db.CompanyDocuments.FirstOrDefaultAsync(d => d.CompanyId == companyId);
        if (existing is null) return;

        db.CompanyDocuments.Remove(existing);
        await db.SaveChangesAsync();
        await _audit.LogAsync(actorUserId, "Delete", "CompanyDocument", companyId, "Registro de comerciante eliminado");
    }
}
