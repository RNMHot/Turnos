using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Turnos.Data;
using Turnos.Models;

namespace Turnos.Services;

public class WhatsAppService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private readonly IConfiguration _config;
    private readonly IHttpClientFactory _httpFactory;
    private readonly ILogger<WhatsAppService> _logger;

    public WhatsAppService(IDbContextFactory<AppDbContext> dbFactory, IConfiguration config, IHttpClientFactory httpFactory, ILogger<WhatsAppService> logger)
    {
        _dbFactory = dbFactory;
        _config = config;
        _httpFactory = httpFactory;
        _logger = logger;
    }

    public async Task<List<WhatsAppGroup>> GetGroupsAsync()
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        return await db.WhatsAppGroups.Where(g => g.Active).OrderBy(g => g.Name).ToListAsync();
    }

    public async Task<bool> SendToGroupAsync(int groupId, string messageType, string messageBody, int? eventId = null)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var group = await db.WhatsAppGroups.FindAsync(groupId);
        if (group is null) return false;

        var log = new MessageLog
        {
            WhatsAppGroupId = groupId,
            EventId = eventId,
            MessageType = messageType,
            MessageBody = messageBody,
            SentDateTime = DateTime.UtcNow,
            DeliveryStatus = "Pendiente"
        };
        db.MessageLogs.Add(log);
        await db.SaveChangesAsync();

        try
        {
            var sent = await SendViaApiAsync(group.PhoneNumber, messageBody);
            log.DeliveryStatus = sent ? "Enviado" : "Fallido";
            await db.SaveChangesAsync();
            return sent;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar WhatsApp al grupo {GroupId}", groupId);
            log.DeliveryStatus = "Fallido";
            await db.SaveChangesAsync();
            return false;
        }
    }

    public async Task<bool> SendMessageAsync(int personId, string messageType, string messageBody, int? eventId = null)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var person = await db.Persons.FindAsync(personId);
        if (person is null) return false;

        var log = new MessageLog
        {
            PersonId = personId,
            EventId = eventId,
            MessageType = messageType,
            MessageBody = messageBody,
            SentDateTime = DateTime.UtcNow,
            DeliveryStatus = "Pendiente"
        };
        db.MessageLogs.Add(log);
        await db.SaveChangesAsync();

        try
        {
            var sent = await SendViaApiAsync(person.PhoneNumber, messageBody);
            log.DeliveryStatus = sent ? "Enviado" : "Fallido";
            await db.SaveChangesAsync();
            return sent;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar WhatsApp para la persona {PersonId}", personId);
            log.DeliveryStatus = "Fallido";
            await db.SaveChangesAsync();
            return false;
        }
    }

    public async Task SendToAllAssignedAsync(int eventId, string messageType, string messageBody)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var assignments = await db.Assignments
            .Include(a => a.Person)
            .Where(a => a.EventId == eventId && a.Status != AssignmentStatus.Candidato && !a.Deleted)
            .ToListAsync();

        foreach (var assignment in assignments)
            await SendMessageAsync(assignment.PersonId, messageType, messageBody, eventId);
    }

    public string BuildTemplateMessage(string templateName, Event ev, Person person, string? calendarUrl = null)
    {
        var otherLabel = string.IsNullOrWhiteSpace(ev.RequiredOtherLabel) ? "Otro" : ev.RequiredOtherLabel;
        var staffingText = $"Equipo necesario: Supervisores {ev.RequiredSupervisors}, Ushers {ev.RequiredUshers}, {otherLabel} {ev.RequiredOther}.";

        return templateName switch
        {
            "OpenAssignment" =>
                $"Hola {person.FullName}, ¿estás disponible para {ev.EventName} el {ev.StartDateTime:MMM d 'a las' h:mm tt}? {staffingText} Responde SÍ o NO.",
            "AssignmentNotification" =>
                $"Hola {person.FullName}, has sido asignado a {ev.EventName} el {ev.StartDateTime:MMM d 'a las' h:mm tt} en {ev.Location}. {staffingText}{(calendarUrl != null ? $" Agregar al calendario: {calendarUrl}" : "")}",
            "EventReminder" =>
                $"Recordatorio: {ev.EventName} mañana a las {ev.StartDateTime:h:mm tt} — {ev.Location}. {staffingText} ¡Nos vemos allí!",
            "SupervisorDetails" =>
                $"Información del supervisor — {ev.EventName}: Ubicación: {ev.Location}. Notas: {ev.Notes}. Inicio: {ev.StartDateTime:MMM d h:mm tt}. {staffingText}",
            _ => string.Empty
        };
    }

    private async Task<bool> SendViaApiAsync(string phoneNumber, string message)
    {
        var token = _config["WhatsApp:AccessToken"];
        var phoneNumberId = _config["WhatsApp:PhoneNumberId"];

        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(phoneNumberId))
        {
            _logger.LogWarning("WhatsApp API no está configurada.");
            return false;
        }

        var client = _httpFactory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var payload = new
        {
            messaging_product = "whatsapp",
            to = phoneNumber.Replace("+", "").Replace(" ", ""),
            type = "text",
            text = new { body = message }
        };

        var response = await client.PostAsJsonAsync(
            $"https://graph.facebook.com/v18.0/{phoneNumberId}/messages", payload);

        return response.IsSuccessStatusCode;
    }
}
