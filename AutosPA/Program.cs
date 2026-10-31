using AutosPA;
using AutosPA.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped<AuthorizationMessageHandler>();
builder.Services.AddScoped(sp =>
{
    var handler = sp.GetRequiredService<AuthorizationMessageHandler>();
    handler.InnerHandler = new HttpClientHandler();
    var apiUrl = builder.Configuration["ApiBaseUrl"]
        ?? throw new InvalidOperationException("No se configuró ApiBaseUrl.");

    return new HttpClient(handler) { BaseAddress = new Uri(apiUrl) };
});

builder.Services.AddScoped(_ => new Supabase.Client(
    builder.Configuration["Supabase:Url"]
        ?? throw new InvalidOperationException("Falta Supabase:Url"),
    builder.Configuration["Supabase:AnonKey"]
        ?? throw new InvalidOperationException("Falta Supabase:AnonKey"),
    new Supabase.SupabaseOptions
    {
        AutoRefreshToken = true,
        AutoConnectRealtime = false
    }));

builder.Services.AddScoped<IAgenteService, AgenteServiceSupabase>();
builder.Services.AddScoped<NotificacionesBadgeService>();
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<SessionAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(
    sp => sp.GetRequiredService<SessionAuthenticationStateProvider>());

await builder.Build().RunAsync();
