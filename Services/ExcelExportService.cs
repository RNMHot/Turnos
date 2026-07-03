using ClosedXML.Excel;
using Turnos.Models;

namespace Turnos.Services;

public class ExcelExportService
{
    public byte[] ExportPersons(List<Person> persons)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Personnel");

        ws.Cell(1, 1).Value = "ID";
        ws.Cell(1, 2).Value = "Full Name";
        ws.Cell(1, 3).Value = "Phone";
        ws.Cell(1, 4).Value = "Email";
        ws.Cell(1, 5).Value = "Active";
        ws.Cell(1, 6).Value = "Roles";
        ws.Cell(1, 7).Value = "Group";
        ws.Row(1).Style.Font.Bold = true;

        for (int i = 0; i < persons.Count; i++)
        {
            var p = persons[i];
            var row = i + 2;
            ws.Cell(row, 1).Value = p.PersonId;
            ws.Cell(row, 2).Value = p.FullName;
            ws.Cell(row, 3).Value = p.PhoneNumber;
            ws.Cell(row, 4).Value = p.Email ?? "";
            ws.Cell(row, 5).Value = p.Active ? "Yes" : "No";
            ws.Cell(row, 6).Value = string.Join(", ", p.PersonRoles.Select(pr => pr.Role?.Name ?? ""));
            ws.Cell(row, 7).Value = p.IsMassGroup ? "Mass" : "Regular";
        }

        ws.Columns().AdjustToContents();
        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }

    public byte[] ExportEvents(List<Event> events)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Events");

        ws.Cell(1, 1).Value = "ID";
        ws.Cell(1, 2).Value = "Event Name";
        ws.Cell(1, 3).Value = "Company";
        ws.Cell(1, 4).Value = "Start";
        ws.Cell(1, 5).Value = "End";
        ws.Cell(1, 6).Value = "Location";
        ws.Cell(1, 7).Value = "Required Supervisors";
        ws.Cell(1, 8).Value = "Required Ushers";
        ws.Cell(1, 9).Value = "Other Label";
        ws.Cell(1, 10).Value = "Required Other";
        ws.Cell(1, 11).Value = "Active";
        ws.Row(1).Style.Font.Bold = true;

        for (int i = 0; i < events.Count; i++)
        {
            var e = events[i];
            var row = i + 2;
            ws.Cell(row, 1).Value = e.EventId;
            ws.Cell(row, 2).Value = e.EventName;
            ws.Cell(row, 3).Value = e.Company?.Name ?? "";
            ws.Cell(row, 4).Value = e.StartDateTime.ToString("yyyy-MM-dd HH:mm");
            ws.Cell(row, 5).Value = e.EndDateTime.ToString("yyyy-MM-dd HH:mm");
            ws.Cell(row, 6).Value = e.Location?.Name ?? "";
            ws.Cell(row, 7).Value = e.RequiredSupervisors;
            ws.Cell(row, 8).Value = e.RequiredUshers;
            ws.Cell(row, 9).Value = e.RequiredOtherLabel;
            ws.Cell(row, 10).Value = e.RequiredOther;
            ws.Cell(row, 11).Value = e.Active ? "Yes" : "No";
        }

        ws.Columns().AdjustToContents();
        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }

    public byte[] ExportAssignments(List<Assignment> assignments)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Assignments");

        ws.Cell(1, 1).Value = "ID";
        ws.Cell(1, 2).Value = "Person";
        ws.Cell(1, 3).Value = "Event";
        ws.Cell(1, 4).Value = "Company";
        ws.Cell(1, 5).Value = "Start";
        ws.Cell(1, 6).Value = "End";
        ws.Cell(1, 7).Value = "Status";
        ws.Row(1).Style.Font.Bold = true;

        for (int i = 0; i < assignments.Count; i++)
        {
            var a = assignments[i];
            var row = i + 2;
            ws.Cell(row, 1).Value = a.AssignmentId;
            ws.Cell(row, 2).Value = a.Person?.FullName ?? "";
            ws.Cell(row, 3).Value = a.Event?.EventName ?? "";
            ws.Cell(row, 4).Value = a.Event?.Company?.Name ?? "";
            ws.Cell(row, 5).Value = a.StartDateTime.ToString("yyyy-MM-dd HH:mm");
            ws.Cell(row, 6).Value = a.EndDateTime.ToString("yyyy-MM-dd HH:mm");
            ws.Cell(row, 7).Value = a.Status.ToString();
        }

        ws.Columns().AdjustToContents();
        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }

    public byte[] ExportAttendance(List<Attendance> attendance)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Attendance");

        ws.Cell(1, 1).Value = "ID";
        ws.Cell(1, 2).Value = "Person";
        ws.Cell(1, 3).Value = "Event";
        ws.Cell(1, 4).Value = "Company";
        ws.Cell(1, 5).Value = "Check-In";
        ws.Cell(1, 6).Value = "Check-Out";
        ws.Cell(1, 7).Value = "Break Minutes";
        ws.Cell(1, 8).Value = "Worked Hours";
        ws.Cell(1, 9).Value = "Notes";
        ws.Row(1).Style.Font.Bold = true;

        for (int i = 0; i < attendance.Count; i++)
        {
            var item = attendance[i];
            var row = i + 2;
            var checkOut = item.CheckOutDateTime ?? DateTime.UtcNow;
            var breakMinutes = item.Breaks
                .Where(b => b.BreakEndDateTime.HasValue)
                .Sum(b => (b.BreakEndDateTime!.Value - b.BreakStartDateTime).TotalMinutes);

            var workedHours = Math.Max(0, (checkOut - item.CheckInDateTime).TotalHours - (breakMinutes / 60.0));

            ws.Cell(row, 1).Value = item.AttendanceId;
            ws.Cell(row, 2).Value = item.Person?.FullName ?? "";
            ws.Cell(row, 3).Value = item.Event?.EventName ?? "";
            ws.Cell(row, 4).Value = item.Event?.Company?.Name ?? "";
            ws.Cell(row, 5).Value = item.CheckInDateTime.ToLocalTime().ToString("yyyy-MM-dd HH:mm");
            ws.Cell(row, 6).Value = item.CheckOutDateTime.HasValue
                ? item.CheckOutDateTime.Value.ToLocalTime().ToString("yyyy-MM-dd HH:mm")
                : "";
            ws.Cell(row, 7).Value = Math.Round(breakMinutes, 2);
            ws.Cell(row, 8).Value = Math.Round(workedHours, 2);
            ws.Cell(row, 9).Value = item.Notes ?? "";
        }

        ws.Columns().AdjustToContents();
        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }
}
