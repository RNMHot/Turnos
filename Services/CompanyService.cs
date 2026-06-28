using Microsoft.EntityFrameworkCore;
using Turnos.Data;
using Turnos.Models;

namespace Turnos.Services;

public class CompanyService
{
    private readonly AppDbContext _db;
    private readonly AuditService _audit;

    public CompanyService(AppDbContext db, AuditService audit)
    {
        _db = db;
        _audit = audit;
    }

    public async Task<List<Company>> GetAllAsync(string? search = null)
    {
        var query = _db.Companies.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(c => c.Name.Contains(search) || (c.ContactName != null && c.ContactName.Contains(search)));
        return await query.OrderBy(c => c.Name).ToListAsync();
    }

    public async Task<Company?> GetByIdAsync(int id) => await _db.Companies.FindAsync(id);

    public async Task<Company> CreateAsync(Company company, string actorUserId)
    {
        _db.Companies.Add(company);
        await _db.SaveChangesAsync();
        await _audit.LogAsync(actorUserId, "Create", "Company", company.CompanyId, $"Created {company.Name}");
        return company;
    }

    public async Task UpdateAsync(Company company, string actorUserId)
    {
        _db.Companies.Update(company);
        await _db.SaveChangesAsync();
        await _audit.LogAsync(actorUserId, "Update", "Company", company.CompanyId, $"Updated {company.Name}");
    }

    public async Task DeleteAsync(int id, string actorUserId)
    {
        var company = await _db.Companies.FindAsync(id);
        if (company is null) return;
        _db.Companies.Remove(company);
        await _db.SaveChangesAsync();
        await _audit.LogAsync(actorUserId, "Delete", "Company", id, $"Deleted {company.Name}");
    }
}
