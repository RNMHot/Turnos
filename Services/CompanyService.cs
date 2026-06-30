using Microsoft.EntityFrameworkCore;
using Turnos.Data;
using Turnos.Models;

namespace Turnos.Services;

public class CompanyService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private readonly AuditService _audit;

    public CompanyService(IDbContextFactory<AppDbContext> dbFactory, AuditService audit)
    {
        _dbFactory = dbFactory;
        _audit = audit;
    }

    public async Task<List<Company>> GetAllAsync(string? search = null)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var query = db.Companies.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(c => c.Name.Contains(search) || (c.ContactName != null && c.ContactName.Contains(search)));
        return await query.OrderBy(c => c.Name).ToListAsync();
    }

    public async Task<Company?> GetByIdAsync(int id)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        return await db.Companies.FindAsync(id);
    }

    public async Task<Company> CreateAsync(Company company, string actorUserId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        db.Companies.Add(company);
        await db.SaveChangesAsync();
        await _audit.LogAsync(actorUserId, "Create", "Company", company.CompanyId, $"Created {company.Name}");
        return company;
    }

    public async Task UpdateAsync(Company company, string actorUserId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        db.Companies.Update(company);
        await db.SaveChangesAsync();
        await _audit.LogAsync(actorUserId, "Update", "Company", company.CompanyId, $"Updated {company.Name}");
    }

    public async Task DeleteAsync(int id, string actorUserId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var company = await db.Companies.FindAsync(id);
        if (company is null) return;
        company.Deleted = true;
        await db.SaveChangesAsync();
        await _audit.LogAsync(actorUserId, "Delete", "Company", id, $"Deleted {company.Name}");
    }
}
