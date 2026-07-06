using Microsoft.EntityFrameworkCore;
using Turnos.Data;
using Turnos.Models;

namespace Turnos.Services;

public class AttendanceService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private readonly AuditService _audit;
    private readonly AppSettingsState _settings;

    public AttendanceService(IDbContextFactory<AppDbContext> dbFactory, AuditService audit, AppSettingsState settings)
    {
        _dbFactory = dbFactory;
        _audit = audit;
        _settings = settings;
    }

    public async Task<List<Attendance>> GetByRangeAsync(DateTime fromUtc, DateTime toUtc, int? personId = null)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var query = db.Attendances
            .Include(a => a.Person)
            .Include(a => a.Event).ThenInclude(e => e.Company)
            .Include(a => a.Breaks)
            .Where(a => a.CheckInDateTime < toUtc && (a.CheckOutDateTime ?? DateTime.UtcNow) >= fromUtc)
            .AsQueryable();

        if (personId.HasValue)
            query = query.Where(a => a.PersonId == personId.Value);

        return await query
            .OrderByDescending(a => a.CheckInDateTime)
            .ToListAsync();
    }

    public async Task<List<Attendance>> GetByEventAsync(int eventId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        return await db.Attendances
            .Include(a => a.Person)
            .Include(a => a.Breaks)
            .Where(a => a.EventId == eventId)
            .OrderBy(a => a.Person.FullName)
            .ThenBy(a => a.CheckInDateTime)
            .ToListAsync();
    }

    public async Task<Attendance?> GetByIdAsync(int id)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        return await db.Attendances
            .Include(a => a.Person)
            .Include(a => a.Event).ThenInclude(e => e.Company)
            .Include(a => a.Breaks.OrderBy(b => b.BreakStartDateTime))
            .FirstOrDefaultAsync(a => a.AttendanceId == id);
    }

    public async Task<Attendance?> GetOpenAttendanceForPersonAsync(int personId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        return await db.Attendances
            .Include(a => a.Event)
            .Include(a => a.Breaks)
            .Where(a => a.PersonId == personId && a.CheckOutDateTime == null)
            .OrderByDescending(a => a.CheckInDateTime)
            .FirstOrDefaultAsync();
    }

    public async Task<(bool Success, string Error, Attendance? Attendance)> CreateAsync(Attendance attendance, string actorUserId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        attendance.CheckInDateTime = NormalizeUtc(attendance.CheckInDateTime);
        attendance.CheckOutDateTime = attendance.CheckOutDateTime.HasValue
            ? NormalizeUtc(attendance.CheckOutDateTime.Value)
            : null;
        attendance.CreatedAt = DateTime.UtcNow;
        attendance.UpdatedAt = DateTime.UtcNow;

        var validationError = await ValidateAttendanceAsync(db, attendance, attendance.AttendanceId);
        if (!string.IsNullOrWhiteSpace(validationError))
            return (false, validationError, null);

        db.Attendances.Add(attendance);
        await db.SaveChangesAsync();
        await _audit.LogAsync(actorUserId, "Create", "Attendance", attendance.AttendanceId,
            $"Attendance created for person {attendance.PersonId} in event {attendance.EventId}");

        return (true, string.Empty, attendance);
    }

    public async Task<(bool Success, string Error)> UpdateAsync(Attendance attendance, string actorUserId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        attendance.CheckInDateTime = NormalizeUtc(attendance.CheckInDateTime);
        attendance.CheckOutDateTime = attendance.CheckOutDateTime.HasValue
            ? NormalizeUtc(attendance.CheckOutDateTime.Value)
            : null;
        attendance.UpdatedAt = DateTime.UtcNow;

        var validationError = await ValidateAttendanceAsync(db, attendance, attendance.AttendanceId);
        if (!string.IsNullOrWhiteSpace(validationError))
            return (false, validationError);

        db.Attendances.Update(attendance);
        await db.SaveChangesAsync();
        await _audit.LogAsync(actorUserId, "Update", "Attendance", attendance.AttendanceId, "Attendance updated");

        return (true, string.Empty);
    }

    public async Task DeleteAsync(int id, string actorUserId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var attendance = await db.Attendances.FindAsync(id);
        if (attendance is null) return;

        attendance.Deleted = true;
        await db.SaveChangesAsync();
        await _audit.LogAsync(actorUserId, "Delete", "Attendance", id, "Attendance deleted");
    }

    public async Task<(bool Success, string Error, Attendance? Attendance)> StartShiftAsync(int personId, int eventId, DateTime checkIn, string actorUserId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();

        var attendance = new Attendance
        {
            PersonId = personId,
            EventId = eventId,
            CheckInDateTime = NormalizeUtc(checkIn),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.Attendances.Add(attendance);
        await db.SaveChangesAsync();
        await _audit.LogAsync(actorUserId, "Create", "Attendance", attendance.AttendanceId, "Shift started");

        return (true, string.Empty, attendance);
    }

    public async Task<(bool Success, string Error)> EndShiftAsync(int attendanceId, DateTime checkOut, string actorUserId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var attendance = await db.Attendances
            .Include(a => a.Breaks)
            .FirstOrDefaultAsync(a => a.AttendanceId == attendanceId);

        if (attendance is null)
            return (false, "No se encontró la asistencia.");

        var checkOutUtc = NormalizeUtc(checkOut);
        if (checkOutUtc <= attendance.CheckInDateTime)
            return (false, "La salida debe ser posterior a la entrada.");

        var openBreak = attendance.Breaks.FirstOrDefault(b => b.BreakEndDateTime == null);
        if (openBreak is not null)
            openBreak.BreakEndDateTime = checkOutUtc;

        attendance.CheckOutDateTime = checkOutUtc;
        attendance.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        await _audit.LogAsync(actorUserId, "Update", "Attendance", attendanceId, "Shift ended");

        return (true, string.Empty);
    }

    public async Task<(bool Success, string Error, AttendanceBreak? Break)> AddBreakAsync(int attendanceId, DateTime breakStart, DateTime? breakEnd, string? notes, string actorUserId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var attendance = await db.Attendances
            .Include(a => a.Breaks)
            .FirstOrDefaultAsync(a => a.AttendanceId == attendanceId);

        if (attendance is null)
            return (false, "No se encontró la asistencia.", null);

        var startUtc = NormalizeUtc(breakStart);
        DateTime? endUtc = breakEnd.HasValue ? NormalizeUtc(breakEnd.Value) : null;

        var validation = ValidateBreakWindow(attendance, startUtc, endUtc, null);
        if (!string.IsNullOrWhiteSpace(validation))
            return (false, validation, null);

        var attendanceBreak = new AttendanceBreak
        {
            AttendanceId = attendanceId,
            BreakStartDateTime = startUtc,
            BreakEndDateTime = endUtc,
            Notes = notes
        };

        db.AttendanceBreaks.Add(attendanceBreak);
        attendance.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        await _audit.LogAsync(actorUserId, "Create", "AttendanceBreak", attendanceBreak.AttendanceBreakId, "Break added");

        return (true, string.Empty, attendanceBreak);
    }

    public async Task<(bool Success, string Error)> UpdateBreakAsync(AttendanceBreak attendanceBreak, string actorUserId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var attendance = await db.Attendances
            .Include(a => a.Breaks)
            .FirstOrDefaultAsync(a => a.AttendanceId == attendanceBreak.AttendanceId);

        if (attendance is null)
            return (false, "No se encontró la asistencia.");

        attendanceBreak.BreakStartDateTime = NormalizeUtc(attendanceBreak.BreakStartDateTime);
        attendanceBreak.BreakEndDateTime = attendanceBreak.BreakEndDateTime.HasValue
            ? NormalizeUtc(attendanceBreak.BreakEndDateTime.Value)
            : null;

        var validation = ValidateBreakWindow(attendance, attendanceBreak.BreakStartDateTime, attendanceBreak.BreakEndDateTime, attendanceBreak.AttendanceBreakId);
        if (!string.IsNullOrWhiteSpace(validation))
            return (false, validation);

        db.AttendanceBreaks.Update(attendanceBreak);
        attendance.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        await _audit.LogAsync(actorUserId, "Update", "AttendanceBreak", attendanceBreak.AttendanceBreakId, "Break updated");

        return (true, string.Empty);
    }

    public async Task<(bool Success, string Error)> EndOpenBreakAsync(int attendanceId, DateTime breakEnd, string actorUserId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var attendance = await db.Attendances
            .Include(a => a.Breaks)
            .FirstOrDefaultAsync(a => a.AttendanceId == attendanceId);

        if (attendance is null)
            return (false, "No se encontró la asistencia.");

        var openBreak = attendance.Breaks
            .OrderByDescending(b => b.BreakStartDateTime)
            .FirstOrDefault(b => b.BreakEndDateTime == null);

        if (openBreak is null)
            return (false, "No hay descanso abierto.");

        openBreak.BreakEndDateTime = NormalizeUtc(breakEnd);
        var validation = ValidateBreakWindow(attendance, openBreak.BreakStartDateTime, openBreak.BreakEndDateTime, openBreak.AttendanceBreakId);
        if (!string.IsNullOrWhiteSpace(validation))
            return (false, validation);

        attendance.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        await _audit.LogAsync(actorUserId, "Update", "AttendanceBreak", openBreak.AttendanceBreakId, "Break ended");

        return (true, string.Empty);
    }

    public async Task DeleteBreakAsync(int breakId, string actorUserId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var attendanceBreak = await db.AttendanceBreaks.FindAsync(breakId);
        if (attendanceBreak is null) return;

        var attendance = await db.Attendances.FindAsync(attendanceBreak.AttendanceId);
        attendanceBreak.Deleted = true;
        if (attendance is not null)
            attendance.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        await _audit.LogAsync(actorUserId, "Delete", "AttendanceBreak", breakId, "Break deleted");
    }

    private static Task<string?> ValidateAttendanceAsync(AppDbContext db, Attendance attendance, int? currentAttendanceId)
    {
        if (attendance.CheckOutDateTime.HasValue && attendance.CheckOutDateTime <= attendance.CheckInDateTime)
            return Task.FromResult<string?>("La salida debe ser posterior a la entrada.");

        return Task.FromResult<string?>(null);
    }

    private static string? ValidateBreakWindow(Attendance attendance, DateTime breakStartUtc, DateTime? breakEndUtc, int? currentBreakId)
    {
        if (breakEndUtc.HasValue && breakEndUtc <= breakStartUtc)
            return "El fin del descanso debe ser posterior al inicio.";

        if (breakStartUtc < attendance.CheckInDateTime)
            return "El inicio del descanso no puede ser antes de la entrada.";

        var attendanceEnd = attendance.CheckOutDateTime;
        if (attendanceEnd.HasValue)
        {
            if (breakStartUtc > attendanceEnd.Value)
                return "El descanso no puede iniciar después de la salida.";
            if (breakEndUtc.HasValue && breakEndUtc.Value > attendanceEnd.Value)
                return "El descanso no puede terminar después de la salida.";
        }

        if (!breakEndUtc.HasValue)
        {
            var openBreakExists = attendance.Breaks.Any(b => b.BreakEndDateTime == null && b.AttendanceBreakId != currentBreakId);
            if (openBreakExists)
                return "Ya existe un descanso abierto para esta asistencia.";
        }

        var candidateEnd = breakEndUtc ?? DateTime.MaxValue;
        var overlap = attendance.Breaks.Any(b =>
            b.AttendanceBreakId != currentBreakId &&
            breakStartUtc < (b.BreakEndDateTime ?? DateTime.MaxValue) &&
            candidateEnd > b.BreakStartDateTime);

        if (overlap)
            return "El descanso se superpone con otro periodo de descanso.";

        return null;
    }

    private DateTime NormalizeUtc(DateTime value) =>
        value.Kind == DateTimeKind.Utc ? value : _settings.ToUtc(value);
}
