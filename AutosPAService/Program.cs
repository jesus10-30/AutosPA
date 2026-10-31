using AutosPAService.Data;
using AutosPAService.Repositories;
using AutosPAService.Services;
using AutosPAService.Authentication;
using AutosPAService.Interfaces;
using AutosPAService;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

// Controladores
builder.Services.AddControllers();

// Autenticación Supabase
builder.Services
    .AddAuthentication("Supabase")
    .AddScheme<AuthenticationSchemeOptions, SupabaseAuthenticationHandler>(
        "Supabase", options => { });

builder.Services.AddAuthorization();
builder.Services.AddHttpClient();

// Swagger
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    // OpenAPI representa estos tipos como los formatos que espera la API y PostgreSQL.
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

// Repositorios y servicios del agente
builder.Services.AddScoped<IAgenteRepository, AgenteRepository>();
builder.Services.AddScoped<AgenteService>();

// Repositorios
builder.Services.AddScoped<ServiciosRepository>();
builder.Services.AddScoped<VehiculosRepository>();
builder.Services.AddScoped<DocumentosVehiculoRepository>();
builder.Services.AddScoped<NotificacionesRepository>();
builder.Services.AddScoped<INotificacionesRepository>(sp =>
    sp.GetRequiredService<NotificacionesRepository>());
builder.Services.AddScoped<CitasRepository>();
builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
builder.Services.AddScoped<ClienteService>();

// Servicios
builder.Services.AddScoped<ServiciosService>();
builder.Services.AddScoped<VehiculosService>();
builder.Services.AddScoped<DocumentosVehiculoService>();
builder.Services.AddScoped<NotificacionesService>();
builder.Services.AddScoped<INotificacionesService>(sp =>
    sp.GetRequiredService<NotificacionesService>());
builder.Services.AddScoped<CitasService>();
builder.Services.AddScoped<AutenticacionService>();

// CORS: permitir comunicación con el frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins("http://localhost:5031")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Base de datos Supabase
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Swagger
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
