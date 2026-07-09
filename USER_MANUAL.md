# Turnos — User Manual

Turnos is the staffing and scheduling system used to plan events, assign ushers/supervisors to them, track attendance and hours, and communicate with staff over WhatsApp. This manual explains what the app does and how to use it day to day. For installation/technical setup, see [README.md](README.md).

## 1. Who Uses Turnos

| Role | What it means | What they can do |
|---|---|---|
| **Admin** | The single super-admin account | Everything, including User Sessions monitoring |
| **Gerencia** (Management) | Back-office / office staff | Full access to scheduling, personnel, attendance, companies, locations, records, messaging, audit log, and settings (except User Sessions) |
| **Supervisor** | On-site crew lead | The Supervisor Dashboard: event details, running check-in for their whole crew, and posting event notes — for events they are assigned to supervise |
| **Ujier (Usher)** / staff | Field personnel | A narrow self-service "Tools" menu: their own availability, self check-in/out, accepting or declining shift offers, and changing their password |

A person can hold more than one operational role (e.g., someone can be both an Usher and a Supervisor). Office/management access (Gerencia) is separate from these field roles and is granted per person.

## 2. Getting In

- **Login** (`/account/login`): sign in with your email and password. If you don't see the full back-office menu after logging in, you likely only have field-level (staff) access — that's expected for ushers and supervisors.
- **Register** (`/account/register`): new staff can self-register with name, email, phone, and a password. New accounts start as **pending approval** and cannot sign in until a manager approves them from the Personnel list.
- **Forced password change**: if a manager sets you a temporary password, you'll be required to change it on your next login.
- **Change password**: available any time from Settings, for anyone already logged in.

## 3. Key Concepts

- **Company**: the client who hires the staffing for an event (e.g., a promoter or venue operator).
- **Location**: the physical venue, optionally with GPS coordinates, parking notes, and named **Positions** (specific posts/spots within the venue, e.g., "Main Door," "Right Balcony") that staff can be assigned to.
- **Event**: a single gig/show/shift window at a location for a company, with a required headcount of Supervisors, Ushers, and a customizable "Other" category (e.g., "Technicians"). Events can be created individually or generated in bulk as **recurring occurrences** (a run of shows across multiple dates/weekdays).
- **Assignment**: linking one person to one event (optionally to a specific position and a custom time). Every assignment moves through a status lifecycle — see below.
- **Availability**: staff mark periods where they are **unavailable** (not the other way around). The system blocks assigning someone during a window they've marked unavailable, and blocks double-booking someone into two overlapping shifts.

### Assignment status lifecycle

| Status | Meaning |
|---|---|
| Candidato (Candidate) | Tentatively placed on the schedule during planning; not yet notified |
| Ofrecido (Offered) | The person has been notified and asked whether they want the shift |
| Aceptado (Accepted) | The person accepted the offer |
| Denegado (Declined) | The person declined the offer |
| Asignado (Assigned) | Management has locked the person in for the shift |
| Confirmado (Confirmed) | The person has confirmed they're aware of their assignment |

Only **Asignado** and **Confirmado** assignments allow self check-in. While a shift is **Ofrecido**, the staff member sees Accept/Deny buttons on their check-in screen.

## 4. Planning and Staffing an Event

1. **Create the event** (Events → New, or double-click an empty slot on the weekly calendar): set the client company, location, start/end time, and how many Supervisors/Ushers/Other are required. Optionally generate several occurrences at once (specific dates or a weekday pattern over a date range) and attach the signed contract (PDF or image).
2. **Staff it**: from the weekly calendar (Home) or the "Eventos Activos" board (a Kanban-style column per active event), drag a person from the personnel panel onto the event, or click to open the person picker. This creates an assignment in **Candidato** status.
3. **Notify staff**: use the WhatsApp button on the event to send an "Open Assignment" message asking if they're available, an assignment notification, a reminder, or full event details (with location/parking/GPS). Recipients can be individual assigned people or a saved WhatsApp group.
4. **Track responses**: as people respond, update/observe their status (Ofrecido → Aceptado/Denegado). Once you've settled the crew, set them to **Asignado**; once they acknowledge, they move to **Confirmado**.
5. **Adjust as needed**: from the event detail popup you can change someone's role, position, give them a custom start/end time (e.g., early setup arrival), or remove them.

The flat **Assignments** list (`/assignments`) gives the same information as a searchable/exportable spreadsheet, useful for bulk review across all events rather than one event at a time.

## 5. Attendance & Check-In

- **Self check-in ("Ponchador")**: staff open their own check-in screen, see "My Shifts," accept/decline pending offers, and tap a shift to record their entry/exit time and log a meal/break period.
- **Supervisor-run check-in**: from the Supervisor Dashboard, a supervisor for an event can enter or adjust check-in/check-out times and notes for everyone on their crew, and add extra rows (e.g., a re-entry).
- **Event notes**: the Supervisor Dashboard also has a running notes/comments log per event for hand-off information or incident notes, visible to anyone who opens that event's dashboard.
- **Payroll reporting**: the Attendance list (back office) filters by person and by period (calendar week, a configurable biweekly pay period, or a custom range), and shows either a detailed check-in/out log or a summary of total hours per person — including break time deducted from worked hours. Attendance can also be entered/corrected manually here.

## 6. Managing Personnel

- The Personnel list is the staff roster: search, filter by role/status/group, and manage each person.
- **Approving new staff**: self-registered accounts appear as "pending approval" — approve to grant access, or reject to discard the registration.
- **Person record** covers: contact info, which operational roles they hold (Usher, Supervisor, etc.), whether they belong to the "Masivos" (mass/bulk-hire) pool used for large events, whether sign-in is enabled, active/inactive status, and password management (set/reset, or force a change on next login).
- **Availability**: each person's unavailable periods (recurring weekly windows or specific dates) can be managed by a manager or by the staff member themselves ("My Availability"), and directly affect who can be assigned to an event.

## 7. Companies, Locations & Contracts

- **Companies**: your client roster — name, contact, phone, email, notes, and an optional business-registration document.
- **Locations**: venues with address, GPS coordinates (with a map link), parking notes, and named Positions used when assigning staff to a specific post at that venue. A location can't be deleted while an event still references it.
- **Event contracts**: each event can have one attached document (the signed paperwork), uploaded from the event form and viewable from the event detail popup.

## 8. Messaging (WhatsApp)

- Messages are sent from an event or assignment using ready-made templates (offer, notification, reminder, supervisor details, full event details), auto-filled from the event's data but editable before sending.
- Recipients can be individual staff (already-assigned people are pre-checked) or a saved WhatsApp broadcast group (managed under Settings → WhatsApp Groups).
- Every send — successful or failed — is recorded in the **Message Log**, which can be filtered by date and delivery status for a full history of what was sent to whom.

## 9. Records & Audit Log

- **Records** is a general-purpose, category-tagged notebook for anything that doesn't belong to a specific structured module — incident notes, miscellaneous documentation — optionally linked to a person, event, or other entity.
- **Audit Log** is a read-only history of who changed what and when across the whole system (creates, updates, deletes), useful for accountability and troubleshooting "who changed this."

## 10. Settings (Management/Admin only)

| Setting | Purpose |
|---|---|
| Logo | Replace the default branding shown in the app |
| Time Zone | The single offset used to display all dates/times consistently app-wide |
| Biweekly Period | Anchor date for the recurring 14-day pay periods used in Attendance reporting |
| Email | SMTP credentials used to send system emails |
| WhatsApp | WhatsApp Business API credentials used to send messages |
| WhatsApp Groups | Reusable broadcast groups available as message recipients |
| User Sessions (Admin only) | Live view of who is logged in, from where, and since when |

## 11. Quick Troubleshooting

- **"Can't sign in / pending approval"**: your account hasn't been approved yet — ask a manager to approve it from the Personnel list.
- **"Can't assign a person to an event"**: they likely have an overlapping shift or a marked unavailable period during that time — check their Availability.
- **"WhatsApp message shows as failed"**: check the Message Log for the delivery status, and confirm the WhatsApp settings/credentials are current.
- **"Times look wrong"**: check the configured Time Zone under Settings — all times are stored internally in UTC and converted for display.
