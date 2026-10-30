using AutosPA;
using AutosPA.Services;
using Microsoft.AspNetCore.Components.Web;

using AutosPA;
using AutosPA.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp =>
    new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

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

await builder.Build().RunAsync();
