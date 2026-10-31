using AutosPAService;
using AutosPAService.Authentication;
using AutosPAService.Data;
using AutosPAService.Interfaces;
using AutosPAService.Repositories;
using AutosPAService.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddAuthentication("Supabase")
    .AddScheme<AuthenticationSchemeOptions, SupabaseAuthenticationHandler>(
        "Supabase", _ => { });
builder.Services.AddAuthorization();
builder.Services.AddHttpClient();

builder.Services.AddHttpClient<SupabaseAuthService>();
builder.Services.AddScoped<AuthRepository>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<AutosPAService.Repositories.Registro.IRegistroRepository, AutosPAService.Repositories.Registro.RegistroRepository>();
builder.Services.AddScoped<AutosPAService.Services.Registro.IRegistroService, AutosPAService.Services.RegistroService>();
builder.Services.AddScoped<AutosPAService.Repositories.Citas.ICitasRepository, AutosPAService.Repositories.Citas.CitasRepository>();
builder.Services.AddScoped<AutosPAService.Services.Citas.ICitasService, AutosPAService.Services.Citas.CitasService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.MapType<DateOnly>(() => new Microsoft.OpenApi.Models.OpenApiSchema
    {
        Type = "string",
        Format = "date",
        Example = new Microsoft.OpenApi.Any.OpenApiString("2026-10-30")
    });
    options.MapType<TimeOnly>(() => new Microsoft.OpenApi.Models.OpenApiSchema
    {
        Type = "string",
        Format = "time",
        Example = new Microsoft.OpenApi.Any.OpenApiString("09:30:00")
    });
    options.AddSecurityDefinition("Bearer",
        new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            Description = "Ingresa el access_token de Supabase."
        });
    options.OperationFilter<SwaggerAuthorizeOperationFilter>();
});

builder.Services.AddScoped<IAgenteRepository, AgenteRepository>();
builder.Services.AddScoped<AgenteService>();
builder.Services.AddScoped<ServiciosRepository>();
builder.Services.AddScoped<VehiculosRepository>();
builder.Services.AddScoped<DocumentosVehiculoRepository>();
builder.Services.AddScoped<NotificacionesRepository>();
builder.Services.AddScoped<INotificacionesRepository>(sp => sp.GetRequiredService<NotificacionesRepository>());
builder.Services.AddScoped<AutosPAService.Repositories.CitasRepository>();
builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
builder.Services.AddScoped<ClienteService>();
builder.Services.AddScoped<ServiciosService>();
builder.Services.AddScoped<VehiculosService>();
builder.Services.AddScoped<DocumentosVehiculoService>();
builder.Services.AddScoped<NotificacionesService>();
builder.Services.AddScoped<INotificacionesService>(sp => sp.GetRequiredService<NotificacionesService>());
builder.Services.AddScoped<AutosPAService.Services.CitasService>();
builder.Services.AddScoped<AutenticacionService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy => policy
        .WithOrigins("http://localhost:5031", "https://localhost:7119")
        .AllowAnyHeader()
        .AllowAnyMethod());
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

