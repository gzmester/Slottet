namespace SlottetBlazor.Services;

public class AuthStateService
{
    private bool _isLoggedIn = false;

    public bool IsLoggedIn => _isLoggedIn;

    public event Action? OnAuthChanged;

    public void SetLoggedIn(bool value)
    {
        if (_isLoggedIn == value) return;
        _isLoggedIn = value;
        OnAuthChanged?.Invoke();
    }
}
