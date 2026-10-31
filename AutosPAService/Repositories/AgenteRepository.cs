using System.Data;
using AutosPAService.Data;
using AutosPAService.Entities;
using AutosPAService.Interfaces;
using AutosPALibrary.DTOs;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace AutosPAService.Repositories;

public class AgenteRepository : IAgenteRepository
{
    private const long EstadoCitaPendiente = 1;
    private const long EstadoCitaConfirmada = 2;
    private const long EstadoCitaEnProceso = 3;
    private const long EstadoCitaFinalizada = 4;

    private readonly ApplicationDbContext _context;

    public AgenteRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> EsAgenteAsync(long idUsuario)
    {
        var usuario = await _context.Usuarios
            .Include(u => u.IdRolNavigation)
            .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);

        return usuario != null &&
               string.Equals(
                   usuario.IdRolNavigation.Nombre?.Trim(),
                   "AGENTE",
                   StringComparison.OrdinalIgnoreCase);
    }

    public async Task<List<AgenteTrabajoDTO>> ObtenerTrabajosAsync(long idAgente)
    {
        return await _context.Citas
            .AsNoTracking()
            .Where(c => c.IdAgente == idAgente)
            .OrderBy(c => c.FechaCita)
            .ThenBy(c => c.HoraInicio)
            .Select(c => new AgenteTrabajoDTO
            {
                IdCita = c.IdCita,
                FechaCita = c.FechaCita,
                HoraInicio = c.HoraInicio,
                Cliente = c.IdClienteNavigation.Nombres + " " +
                          c.IdClienteNavigation.Apellidos,
                Vehiculo = c.IdVehiculoNavigation.IdMarcaNavigation.Nombre + " " +
                           c.IdVehiculoNavigation.Modelo + " " +
                           c.IdVehiculoNavigation.Anio,
                Placa = c.IdVehiculoNavigation.Placa,
                Servicio = c.IdTipoServicioNavigation.Nombre,
                Estado = c.IdEstadoCitaNavigation.Nombre,
                Costo = c.Costo,
                TiempoEstimadoMinutos = c.TiempoEstimadoMinutos,
                Observaciones = c.Observaciones
            })
            .ToListAsync();
    }

    public async Task<List<SolicitudAgenteDTO>> ObtenerSolicitudesAsync(long idAgente)
    {
        var rechazos = await _context.RechazosCita
            .AsNoTracking()
            .Where(r => r.IdAgente == idAgente)
            .OrderByDescending(r => r.FechaRechazo)
            .ThenByDescending(r => r.IdRechazo)
            .Select(r => new { r.IdCita, r.Motivo })
            .ToListAsync();

        var motivosRechazo = rechazos
            .GroupBy(r => r.IdCita)
            .ToDictionary(g => g.Key, g => g.First().Motivo);
        var idsRechazados = motivosRechazo.Keys.ToList();

        var idsPendientes = await _context.Citas
            .AsNoTracking()
            .Where(c => c.IdEstadoCita == EstadoCitaPendiente
                && (c.IdAgenteDestino == idAgente || c.IdAgente == idAgente)
                && !idsRechazados.Contains(c.IdCita))
            .Select(c => c.IdCita)
            .ToListAsync();

        var fechaDesdeAceptadas = DateOnly.FromDateTime(DateTime.Today.AddDays(-30));
        var idsAceptados = await _context.Citas
            .AsNoTracking()
            .Where(c => c.IdAgente == idAgente
                && (c.IdEstadoCita == EstadoCitaConfirmada
                    || c.IdEstadoCita == EstadoCitaEnProceso
                    || c.IdEstadoCita == EstadoCitaFinalizada)
                && c.FechaCita >= fechaDesdeAceptadas)
            .Select(c => c.IdCita)
            .ToListAsync();

        // Mantiene el orden de prioridad usado por la pantalla: pendientes,
        // aceptadas y finalmente rechazadas, sin repetir IDs de cita.
        var estadosPorCita = idsPendientes
            .Select(id => (IdCita: id, Estado: "Pendiente"))
            .Concat(idsAceptados.Select(id => (IdCita: id, Estado: "Aceptada")))
            .Concat(idsRechazados.Select(id => (IdCita: id, Estado: "Rechazada")))
            .GroupBy(item => item.IdCita)
            .ToDictionary(g => g.Key, g => g.First().Estado);

        if (estadosPorCita.Count == 0)
            return new List<SolicitudAgenteDTO>();

        var idsCitas = estadosPorCita.Keys.ToList();
        var citas = await _context.Citas
            .AsNoTracking()
            .Where(c => idsCitas.Contains(c.IdCita))
            .Include(c => c.IdClienteNavigation)
            .Include(c => c.IdTipoServicioNavigation)
            .Include(c => c.IdEstadoCitaNavigation)
            .Include(c => c.IdVehiculoNavigation)
                .ThenInclude(v => v.IdMarcaNavigation)
            .Include(c => c.IdVehiculoNavigation)
                .ThenInclude(v => v.DocumentosVehiculos)
                    .ThenInclude(d => d.IdTipoDocumentoNavigation)
            .Include(c => c.IdVehiculoNavigation)
                .ThenInclude(v => v.DocumentosVehiculos)
                    .ThenInclude(d => d.IdEstadoDocumentoNavigation)
            .ToListAsync();

        return citas.Select(c =>
        {
            var estado = estadosPorCita[c.IdCita];
            var vehiculo = c.IdVehiculoNavigation;
            var documentos = vehiculo.DocumentosVehiculos
                .GroupBy(d => d.IdTipoDocumento)
                .Select(g => g
                    .OrderByDescending(d => d.FechaVencimiento)
                    .ThenByDescending(d => d.FechaRegistro)
                    .First())
                .Select(d => new DocumentoSolicitudDTO
                {
                    Tipo = d.IdTipoDocumentoNavigation.Nombre,
                    Estado = d.IdEstadoDocumentoNavigation.Nombre,
                    EstadoActivo = d.IdEstadoDocumentoNavigation.Activo,
                    FechaVencimiento = d.FechaVencimiento
                })
                .ToList();

            return new SolicitudAgenteDTO
            {
                IdCita = c.IdCita,
                Cliente = $"{c.IdClienteNavigation.Nombres} {c.IdClienteNavigation.Apellidos}".Trim(),
                Vehiculo = $"{vehiculo.IdMarcaNavigation.Nombre} {vehiculo.Modelo} {vehiculo.Anio}"
                    .Replace("  ", " ")
                    .Trim(),
                Placa = vehiculo.Placa,
                Servicio = c.IdTipoServicioNavigation.Nombre,
                FechaCita = c.FechaCita,
                HoraInicio = c.HoraInicio,
                Costo = c.Costo,
                TiempoEstimadoMinutos = c.TiempoEstimadoMinutos,
                Observaciones = c.Observaciones,
                FechaCreacion = c.FechaCreacion,
                Estado = estado,
                MotivoRechazo = estado == "Rechazada"
                    ? motivosRechazo.GetValueOrDefault(c.IdCita)
                    : null,
                Documentos = documentos
            };
        }).ToList();
    }

    public async Task<List<AgenteTrabajoDTO>> ObtenerTrabajosAsync(
        long idAgente,
        DateOnly desde,
        DateOnly hasta)
    {
        return await _context.Citas
            .AsNoTracking()
            .Where(c => c.IdAgente == idAgente
                && c.FechaCita >= desde
                && c.FechaCita <= hasta
                && (c.IdEstadoCita == EstadoCitaConfirmada
                    || c.IdEstadoCita == EstadoCitaEnProceso
                    || c.IdEstadoCita == EstadoCitaFinalizada))
            .OrderBy(c => c.FechaCita)
            .ThenBy(c => c.HoraInicio)
            .Select(c => new AgenteTrabajoDTO
            {
                IdCita = c.IdCita,
                FechaCita = c.FechaCita,
                HoraInicio = c.HoraInicio,
                Cliente = c.IdClienteNavigation.Nombres + " " +
                          c.IdClienteNavigation.Apellidos,
                Vehiculo = c.IdVehiculoNavigation.IdMarcaNavigation.Nombre + " " +
                           c.IdVehiculoNavigation.Modelo + " " +
                           c.IdVehiculoNavigation.Anio,
                Placa = c.IdVehiculoNavigation.Placa,
                Servicio = c.IdTipoServicioNavigation.Nombre,
                Estado = c.IdEstadoCitaNavigation.Nombre,
                Costo = c.Costo,
                TiempoEstimadoMinutos = c.TiempoEstimadoMinutos,
                Observaciones = c.Observaciones
            })
            .ToListAsync();
    }

    public Task<ResultadoOperacionSolicitud> AceptarSolicitudAsync(
        long idCita,
        long idAgente)
    {
        return EjecutarTransaccionSolicitudAsync(async () =>
        {
            var cita = await _context.Citas
                .FirstOrDefaultAsync(c => c.IdCita == idCita);

            if (cita is null)
                return ResultadoOperacionSolicitud.SolicitudInexistente;

            if (!PuedeGestionarSolicitud(cita, idAgente))
                return ResultadoOperacionSolicitud.SolicitudNoAsignada;

            if (cita.IdEstadoCita != EstadoCitaPendiente)
                return ResultadoOperacionSolicitud.EstadoInvalido;

            if (cita.TiempoEstimadoMinutos <= 0)
                return ResultadoOperacionSolicitud.DuracionInvalida;

            var yaRechazadaPorAgente = await _context.RechazosCita
                .AsNoTracking()
                .AnyAsync(r => r.IdCita == idCita && r.IdAgente == idAgente);

            if (yaRechazadaPorAgente)
                return ResultadoOperacionSolicitud.EstadoInvalido;

            var trabajosOcupados = await _context.Citas
                .AsNoTracking()
                .Where(c => c.IdCita != idCita
                    && c.IdAgente == idAgente
                    && (c.IdEstadoCita == EstadoCitaConfirmada
                        || c.IdEstadoCita == EstadoCitaEnProceso
                        || c.IdEstadoCita == EstadoCitaFinalizada))
                .Select(c => new
                {
                    c.FechaCita,
                    c.HoraInicio,
                    c.TiempoEstimadoMinutos
                })
                .ToListAsync();

            // Un intervalo ocupado con duración inválida no puede considerarse libre.
            if (trabajosOcupados.Any(trabajo => trabajo.TiempoEstimadoMinutos <= 0))
                return ResultadoOperacionSolicitud.DuracionInvalida;

            var existeChoque = trabajosOcupados.Any(trabajo => SeSolapan(
                cita.FechaCita,
                cita.HoraInicio,
                cita.TiempoEstimadoMinutos,
                trabajo.FechaCita,
                trabajo.HoraInicio,
                trabajo.TiempoEstimadoMinutos));

            if (existeChoque)
                return ResultadoOperacionSolicitud.ConflictoHorario;

            cita.IdAgente = idAgente;
            cita.IdEstadoCita = EstadoCitaConfirmada;
            cita.FechaActualizacion = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return ResultadoOperacionSolicitud.Exito;
        });
    }

    public Task<ResultadoOperacionSolicitud> RechazarSolicitudAsync(
        long idCita,
        long idAgente,
        RechazarSolicitudDTO solicitud)
    {
        if (solicitud is null || string.IsNullOrWhiteSpace(solicitud.Motivo))
            return Task.FromResult(ResultadoOperacionSolicitud.MotivoInvalido);

        return EjecutarTransaccionSolicitudAsync(async () =>
        {
            var cita = await _context.Citas
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.IdCita == idCita);

            if (cita is null)
                return ResultadoOperacionSolicitud.SolicitudInexistente;

            if (!PuedeGestionarSolicitud(cita, idAgente))
                return ResultadoOperacionSolicitud.SolicitudNoAsignada;

            if (cita.IdEstadoCita != EstadoCitaPendiente)
                return ResultadoOperacionSolicitud.EstadoInvalido;

            var yaRechazadaPorAgente = await _context.RechazosCita
                .AsNoTracking()
                .AnyAsync(r => r.IdCita == idCita && r.IdAgente == idAgente);

            if (yaRechazadaPorAgente)
                return ResultadoOperacionSolicitud.EstadoInvalido;

            _context.RechazosCita.Add(new RechazoCita
            {
                IdCita = idCita,
                IdAgente = idAgente,
                Motivo = solicitud.Motivo.Trim(),
                FechaRechazo = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return ResultadoOperacionSolicitud.Exito;
        });
    }

    public Task<ResultadoOperacionSolicitud> DeshacerSolicitudAsync(
        long idCita,
        long idAgente)
    {
        return EjecutarTransaccionSolicitudAsync(async () =>
        {
            var cita = await _context.Citas
                .FirstOrDefaultAsync(c => c.IdCita == idCita);

            if (cita is null)
                return ResultadoOperacionSolicitud.SolicitudInexistente;

            var rechazo = await _context.RechazosCita
                .FirstOrDefaultAsync(r => r.IdCita == idCita && r.IdAgente == idAgente);

            if (rechazo is not null)
            {
                if (cita.IdEstadoCita != EstadoCitaPendiente)
                    return ResultadoOperacionSolicitud.EstadoInvalido;

                _context.RechazosCita.Remove(rechazo);
                await _context.SaveChangesAsync();
                return ResultadoOperacionSolicitud.Exito;
            }

            if (cita.IdAgente != idAgente)
                return ResultadoOperacionSolicitud.SolicitudNoAsignada;

            if (cita.IdEstadoCita != EstadoCitaConfirmada)
                return ResultadoOperacionSolicitud.EstadoInvalido;

            cita.IdEstadoCita = EstadoCitaPendiente;
            if (cita.IdAgenteDestino == idAgente)
                cita.IdAgente = null;
            cita.FechaActualizacion = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return ResultadoOperacionSolicitud.Exito;
        });
    }

    private static bool PuedeGestionarSolicitud(Cita cita, long idAgente) =>
        cita.IdAgenteDestino == idAgente || cita.IdAgente == idAgente;

    private static bool SeSolapan(
        DateOnly fechaA,
        TimeOnly horaA,
        int duracionAMinutos,
        DateOnly fechaB,
        TimeOnly horaB,
        int duracionBMinutos)
    {
        // La validez se comprueba antes de invocar este cálculo. Mantener esta
        // salvaguarda evita que un intervalo inválido se interprete como libre.
        if (duracionAMinutos <= 0 || duracionBMinutos <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(duracionAMinutos),
                "Las duraciones deben ser mayores que cero.");

        var inicioA = (long)fechaA.DayNumber * TimeSpan.TicksPerDay + horaA.Ticks;
        var finA = inicioA + (long)duracionAMinutos * TimeSpan.TicksPerMinute;
        var inicioB = (long)fechaB.DayNumber * TimeSpan.TicksPerDay + horaB.Ticks;
        var finB = inicioB + (long)duracionBMinutos * TimeSpan.TicksPerMinute;

        // Intervalos [inicio, fin): trabajos consecutivos sin solapamiento son válidos.
        return inicioA < finB && inicioB < finA;
    }

    private async Task<ResultadoOperacionSolicitud> EjecutarTransaccionSolicitudAsync(
        Func<Task<ResultadoOperacionSolicitud>> operacion)
    {
        await using var transaccion = await _context.Database
            .BeginTransactionAsync(IsolationLevel.Serializable);

        try
        {
            var resultado = await operacion();

            if (resultado == ResultadoOperacionSolicitud.Exito)
                await transaccion.CommitAsync();
            else
                await transaccion.RollbackAsync();

            return resultado;
        }
        catch (Exception ex) when (EsFalloDeSerializacion(ex))
        {
            await transaccion.RollbackAsync();
            return ResultadoOperacionSolicitud.ConflictoConcurrencia;
        }
    }

    private static bool EsFalloDeSerializacion(Exception exception)
    {
        for (Exception? actual = exception; actual is not null; actual = actual.InnerException)
        {
            if (actual is PostgresException postgres && postgres.SqlState == "40001")
                return true;
        }

        return false;
    }

    public async Task<bool> IniciarTrabajoAsync(long idCita, long idAgente)
    {
        var cita = await _context.Citas.FirstOrDefaultAsync(c =>
            c.IdCita == idCita &&
            c.IdAgente == idAgente &&
            c.IdEstadoCita == 2);

        if (cita == null)
            return false;

        cita.IdEstadoCita = 3;
        cita.FechaActualizacion = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> FinalizarTrabajoAsync(long idCita, long idAgente)
    {
        var cita = await _context.Citas.FirstOrDefaultAsync(c =>
            c.IdCita == idCita &&
            c.IdAgente == idAgente &&
            c.IdEstadoCita == 3);

        if (cita == null)
            return false;

        cita.IdEstadoCita = 4;
        cita.FechaActualizacion = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<PerfilAgenteDTO?> ObtenerPerfilAsync(long idUsuario)
    {
        return await _context.Usuarios
            .AsNoTracking()
            .Where(u => u.IdUsuario == idUsuario)
            .Select(u => new PerfilAgenteDTO
            {
                Nombres = u.Nombres,
                Apellidos = u.Apellidos,
                Email = u.Email,
                Telefono = u.Telefono
            })
            .FirstOrDefaultAsync();
    }

    public async Task<bool> ActualizarPerfilAsync(
        long idUsuario,
        ActualizarPerfilAgenteDTO perfil)
    {
        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);

        if (usuario == null)
            return false;

        usuario.Nombres = perfil.Nombres.Trim();
        usuario.Apellidos = perfil.Apellidos.Trim();
        usuario.Telefono = string.IsNullOrWhiteSpace(perfil.Telefono)
            ? null
            : perfil.Telefono.Trim();

        usuario.FechaActualizacion = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public Task<bool> CorreoEnUsoAsync(long idUsuario, string email)
    {
        var correoNormalizado = email.Trim().ToLowerInvariant();
        return _context.Usuarios
            .AsNoTracking()
            .AnyAsync(u => u.IdUsuario != idUsuario &&
                           u.Email.ToLower() == correoNormalizado);
    }
}
