using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;

namespace SlottetBlazor.Services;

public class ApiAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly HttpClient _httpClient;
    private readonly ProtectedLocalStorage _localStorage;
    private readonly ILogger<ApiAuthenticationStateProvider> _logger;

    public ApiAuthenticationStateProvider(
        HttpClient httpClient,
        ProtectedLocalStorage localStorage,
        ILogger<ApiAuthenticationStateProvider> logger)
    {
        _httpClient = httpClient;
        _localStorage = localStorage;
        _logger = logger;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            // ProtectedLocalStorage is not available during pre-rendering
            // This try-catch handles that gracefully
            var result = await _localStorage.GetAsync<string>("authToken");
            var savedToken = result.Success ? result.Value : null;

            if (string.IsNullOrWhiteSpace(savedToken))
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            // Attach the token to HttpClient for API calls
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", savedToken);

            // Decode the JWT token to create claims
            var claims = ParseClaimsFromJwt(savedToken);
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt")));
        }
        catch (Exception ex)
        {
            // During pre-rendering, JavaScript interop is not available yet
            // Return anonymous user - will be re-evaluated after SignalR connection
            _logger.LogDebug("Could not read auth token during pre-render: {Message}", ex.Message);
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
    }

    /// <summary>Returns the stored JWT token, or null if not logged in.</summary>
    public async Task<string?> GetTokenAsync()
    {
        try
        {
            var result = await _localStorage.GetAsync<string>("authToken");
            return result.Success ? result.Value : null;
        }
        catch { return null; }
    }

    public async Task MarkUserAsAuthenticated(string token)
    {
        try
        {
            await _localStorage.SetAsync("authToken", token);
        }
        catch (Exception ex)
        {
            // May fail during pre-rendering, but we still update the auth state
            _logger.LogWarning("Could not save auth token to local storage: {Message}", ex.Message);
        }

        // Attach token to HttpClient for subsequent API calls
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var authenticatedUser = new ClaimsPrincipal(new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt"));
        var authState = Task.FromResult(new AuthenticationState(authenticatedUser));
        NotifyAuthenticationStateChanged(authState);
    }

    public async Task MarkUserAsLoggedOut()
    {
        try
        {
            await _localStorage.DeleteAsync("authToken");
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Could not delete auth token from local storage: {Message}", ex.Message);
        }

        // Remove token from HttpClient
        _httpClient.DefaultRequestHeaders.Authorization = null;

        var anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());
        var authState = Task.FromResult(new AuthenticationState(anonymousUser));
        NotifyAuthenticationStateChanged(authState);
    }

    private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var payload = jwt.Split('.')[1];
        var jsonBytes = ParseBase64WithoutPadding(payload);
        var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);
        return keyValuePairs!.Select(kvp => new Claim(kvp.Key, kvp.Value.ToString()!));
    }

    private byte[] ParseBase64WithoutPadding(string base64)
    {
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }
        return Convert.FromBase64String(base64);
    }
}
