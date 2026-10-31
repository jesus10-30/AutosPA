using AutosPA;
using AutosPA.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp =>
{
    var apiUrl = builder.Configuration["ApiBaseUrl"]
        ?? throw new InvalidOperationException(
            "No se configuró ApiBaseUrl.");

    return new HttpClient
    {
        BaseAddress = new Uri(apiUrl)
    };
});

builder.Services.AddAuthorizationCore();

builder.Services.AddScoped<SessionAuthenticationStateProvider>();

builder.Services.AddScoped<AuthenticationStateProvider>(
    sp => sp.GetRequiredService<SessionAuthenticationStateProvider>());
await builder.Build().RunAsync();
