using AutosPA;
using AutosPA.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Cliente HTTP para comunicarse con el backend
builder.Services.AddScoped<AuthorizationMessageHandler>();

builder.Services.AddScoped(sp =>
{
    var handler = sp.GetRequiredService<AuthorizationMessageHandler>();
    handler.InnerHandler = new HttpClientHandler();

    return new HttpClient(handler)
    {
        BaseAddress = new Uri("http://localhost:5064/")
    };
});

// Cliente Supabase
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

// Servicios
builder.Services.AddScoped<IAgenteService, AgenteServiceSupabase>();
builder.Services.AddScoped<NotificacionesBadgeService>();

await builder.Build().RunAsync();