namespace Turnos.Services;

public class AppSettingsState
{
    public string? BrandLogoBase64 { get; private set; }

    public event Action? OnChange;

    public void SetBrandLogo(string? value)
    {
        BrandLogoBase64 = value;
        OnChange?.Invoke();
    }
}
