using SlottetBlazor.Components;
using SlottetBlazor.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add ProtectedBrowserStorage for secure token storage
builder.Services.AddScoped<ProtectedLocalStorage>();

// Add HttpClient for API communication - single registration via IHttpClientFactory
builder.Services.AddHttpClient("API", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"]!);
});

// Register HttpClient that resolves via IHttpClientFactory (used by MainLayout & ApiAuthenticationStateProvider)
builder.Services.AddScoped(sp =>
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    return factory.CreateClient("API");
});

// Shared JSON options with enum-as-string support (matches API serialization)
builder.Services.AddSingleton(new JsonSerializerOptions(JsonSerializerDefaults.Web)
{
    Converters = { new JsonStringEnumConverter() }
});

// Add custom authentication state provider
builder.Services.AddScoped<AuthStateService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

// NOTE: Do NOT use UseHttpsRedirection in development - it causes redirect issues
// and the "Failed to determine the https port" warning which breaks Blazor SSR

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
