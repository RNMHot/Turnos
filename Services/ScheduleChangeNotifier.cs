namespace Turnos.Services;

// Singleton pub/sub so all active Blazor Server circuits can react when another
// user changes events, assignments, or personnel — each circuit has its own
// component instances/state, so there's no other channel between them.
public class ScheduleChangeNotifier
{
    public event Action? Changed;

    public void NotifyChanged() => Changed?.Invoke();
}
