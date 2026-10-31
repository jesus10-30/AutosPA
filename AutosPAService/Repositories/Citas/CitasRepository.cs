using AutosPALibrary.Dtos;
using AutosPAService.Data;
using AutosPAService.Entities;
using Microsoft.EntityFrameworkCore;

namespace AutosPAService.Repositories.Citas;

public class CitasRepository : ICitasRepository
{
    private readonly ApplicationDbContext _context;

    public CitasRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<CitaClienteDto>> ObtenerCitasPorClienteAsync(
        long idCliente)
    {
        return await _context.Citas
            .AsNoTracking()
            .Where(c => c.IdCliente == idCliente)
            .Select(c => new CitaClienteDto
            {
                IdCita = c.IdCita,
                FechaCita = c.FechaCita,
                HoraInicio = c.HoraInicio,
                Servicio = c.IdTipoServicioNavigation.Nombre,
                Estado = c.IdEstadoCitaNavigation.Nombre,
                Placa = c.IdVehiculoNavigation.Placa,
                Marca = c.IdVehiculoNavigation.IdMarcaNavigation.Nombre,
                Modelo = c.IdVehiculoNavigation.Modelo,
                Anio = c.IdVehiculoNavigation.Anio,
                Costo = c.Costo,
                TiempoEstimadoMinutos = c.TiempoEstimadoMinutos,
                Observaciones = c.Observaciones
            })
            .OrderByDescending(c => c.FechaCita)
            .ThenByDescending(c => c.HoraInicio)
            .ToListAsync();
    }

    public async Task<List<VehiculoClienteDto>> ObtenerVehiculosPorClienteAsync(
        long idCliente)
    {
        return await _context.Vehiculos
            .AsNoTracking()
            .Where(v => v.IdConductorNavigation.IdUsuario == idCliente)
            .OrderBy(v => v.Placa)
            .Select(v => new VehiculoClienteDto
            {
                IdVehiculo = v.IdVehiculo,
                Placa = v.Placa,
                Marca = v.IdMarcaNavigation.Nombre,
                Modelo = v.Modelo,
                Anio = v.Anio,
                Color = v.Color,
                Kilometraje = v.Kilometraje
            })
            .ToListAsync();
    }

    public async Task<List<OpcionCatalogoDto>> ObtenerMarcasActivasAsync()
    {
        return await _context.CatMarcas
            .AsNoTracking()
            .Where(m => m.Activo)
            .OrderBy(m => m.Nombre)
            .Select(m => new OpcionCatalogoDto
            {
                Id = m.IdMarca,
                Nombre = m.Nombre
            })
            .ToListAsync();
    }

    public async Task<List<OpcionCatalogoDto>> ObtenerTiposVehiculoActivosAsync()
    {
        return await _context.CatTiposVehiculos
            .AsNoTracking()
            .Where(t => t.Activo)
            .OrderBy(t => t.Nombre)
            .Select(t => new OpcionCatalogoDto
            {
                Id = t.IdTipoVehiculo,
                Nombre = t.Nombre
            })
            .ToListAsync();
    }

    public async Task<(VehiculoClienteDto? Vehiculo, string? Error)>
        RegistrarVehiculoAsync(RegistrarVehiculoDto dto)
    {
        var placa = dto.Placa.Trim().ToUpperInvariant();

        // Verificar que el usuario tenga un conductor asociado.
        var conductor = await _context.Conductores
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.IdUsuario == dto.IdCliente);

        if (conductor is null)
            return (null, "El usuario no tiene un perfil de conductor registrado.");

        // Verificar que la marca exista y esté activa.
        var marca = await _context.CatMarcas
            .AsNoTracking()
            .FirstOrDefaultAsync(m =>
                m.IdMarca == dto.IdMarca && m.Activo);

        if (marca is null)
            return (null, "La marca seleccionada no es válida.");

        // Verificar que el tipo exista y esté activo.
        var tipo = await _context.CatTiposVehiculos
            .AsNoTracking()
            .FirstOrDefaultAsync(t =>
                t.IdTipoVehiculo == dto.IdTipoVehiculo && t.Activo);

        if (tipo is null)
            return (null, "El tipo de vehículo seleccionado no es válido.");

        // Verificar que la placa no esté registrada.
        var placaExiste = await _context.Vehiculos
            .AsNoTracking()
            .AnyAsync(v => v.Placa.ToUpper() == placa);

        if (placaExiste)
            return (null, "Ya existe un vehículo registrado con esa placa.");

        var vehiculo = new Vehiculo
        {
            IdConductor = conductor.IdConductor,
            IdMarca = marca.IdMarca,
            IdTipoVehiculo = tipo.IdTipoVehiculo,
            Modelo = dto.Modelo.Trim(),
            Anio = dto.Anio,
            Placa = placa,
            Color = string.IsNullOrWhiteSpace(dto.Color)
                ? null
                : dto.Color.Trim(),
            Kilometraje = dto.Kilometraje,
            FechaRegistro = DateTime.UtcNow,
            FechaActualizacion = DateTime.UtcNow
        };

        _context.Vehiculos.Add(vehiculo);
        await _context.SaveChangesAsync();

        var resultado = new VehiculoClienteDto
        {
            IdVehiculo = vehiculo.IdVehiculo,
            Placa = vehiculo.Placa,
            Marca = marca.Nombre,
            Modelo = vehiculo.Modelo,
            Anio = vehiculo.Anio,
            Color = vehiculo.Color,
            Kilometraje = vehiculo.Kilometraje
        };

        return (resultado, null);
    }

    public async Task<(CitaClienteDto? Cita, string? Error)>
    RegistrarCitaAsync(RegistrarCitaDto dto)
    {
        // 1. Validar fecha y horario.
        var hoy = DateOnly.FromDateTime(DateTime.Today);

        if (dto.FechaCita < hoy)
            return (null, "La fecha de la cita no puede estar en el pasado.");

        if (dto.FechaCita.DayOfWeek == DayOfWeek.Sunday)
            return (null, "No se pueden registrar citas los domingos.");

        if (dto.FechaCita == hoy &&
            dto.HoraInicio <= TimeOnly.FromDateTime(DateTime.Now))
        {
            return (null, "Selecciona una hora posterior a la actual.");
        }

        // 2. Verificar que el cliente tenga perfil de conductor.
        var conductor = await _context.Conductores
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.IdUsuario == dto.IdCliente);

        if (conductor is null)
            return (null, "El usuario no tiene un perfil de conductor registrado.");

        // 3. Verificar que el vehículo pertenezca al cliente.
        var vehiculo = await _context.Vehiculos
            .Include(v => v.IdMarcaNavigation)
            .FirstOrDefaultAsync(v =>
                v.IdVehiculo == dto.IdVehiculo &&
                v.IdConductor == conductor.IdConductor);

        if (vehiculo is null)
            return (null, "El vehículo seleccionado no pertenece al cliente.");

        // 4. Obtener el servicio real, su precio y su duración.
        var servicio = await _context.CatTiposServicios
            .AsNoTracking()
            .FirstOrDefaultAsync(s =>
                s.IdTipoServicio == dto.IdTipoServicio &&
                s.Activo);

        if (servicio is null)
            return (null, "El servicio seleccionado no existe o está inactivo.");

        if (servicio.TiempoEstimadoMinutos <= 0)
            return (null, "El servicio no tiene una duración válida.");

        // 5. Buscar el estado inicial.
        var estado = await _context.CatEstadosCita
            .AsNoTracking()
            .FirstOrDefaultAsync(e =>
                e.Nombre == "PENDIENTE" &&
                e.Activo);

        if (estado is null)
            return (null, "No está configurado el estado PENDIENTE.");

        // 6. Validar que se haya seleccionado un agente.
        if (dto.IdAgenteSeleccionado is null ||
            dto.IdAgenteSeleccionado <= 0)
        {
            return (null, "Debes seleccionar un agente.");
        }

        // 7. Verificar que el agente exista y esté activo.
        var agente = await _context.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(u =>
                u.IdUsuario == dto.IdAgenteSeleccionado.Value &&
                u.IdRol == 2 &&
                u.IdEstadoUsuario == 1);

        if (agente is null)
            return (null, "El agente seleccionado no está activo.");

        // 8. Comprobar nuevamente la disponibilidad del agente.
        // Se consideran ocupadas las citas pendientes, confirmadas
        // y las que ya están en proceso.
        var estadosOcupados = new[]
        {
        "PENDIENTE",
        "CONFIRMADA",
        "EN_PROCESO"
    };

        var citasOcupadas = await _context.Citas
            .AsNoTracking()
            .Where(c =>
                c.FechaCita == dto.FechaCita &&
                c.IdAgente == agente.IdUsuario &&
                estadosOcupados.Contains(c.IdEstadoCitaNavigation.Nombre))
            .Select(c => new
            {
                c.HoraInicio,
                c.TiempoEstimadoMinutos
            })
            .ToListAsync();

        var inicioNuevo = dto.HoraInicio.ToTimeSpan();

        var finNuevo = inicioNuevo.Add(
            TimeSpan.FromMinutes(servicio.TiempoEstimadoMinutos));

        var horarioOcupado = citasOcupadas.Any(c =>
        {
            var inicioExistente = c.HoraInicio.ToTimeSpan();

            var finExistente = inicioExistente.Add(
                TimeSpan.FromMinutes(c.TiempoEstimadoMinutos));

            // Hay traslape si un intervalo comienza antes de que
            // termine el otro y termina después de que este comienza.
            return inicioExistente < finNuevo &&
                   finExistente > inicioNuevo;
        });

        if (horarioOcupado)
        {
            return (
                null,
                "El agente seleccionado ya tiene una cita en ese horario. " +
                "Selecciona otro agente u otra hora."
            );
        }

        // 9. Crear la cita y asignar el agente seleccionado.
        var ahora = DateTime.UtcNow;

        var nuevaCita = new Cita
        {
            IdCliente = dto.IdCliente,
            IdAgente = agente.IdUsuario,
            IdVehiculo = vehiculo.IdVehiculo,
            IdTipoServicio = servicio.IdTipoServicio,
            IdEstadoCita = estado.IdEstadoCita,

            FechaCita = dto.FechaCita,
            HoraInicio = dto.HoraInicio,

            Costo = servicio.CostoBase,
            TiempoEstimadoMinutos = servicio.TiempoEstimadoMinutos,

            Observaciones = string.IsNullOrWhiteSpace(dto.Observaciones)
                ? null
                : dto.Observaciones.Trim(),

            FechaCreacion = ahora,
            FechaActualizacion = ahora
        };

        _context.Citas.Add(nuevaCita);

        await _context.SaveChangesAsync();

        // 10. Devolver los datos de la cita registrada.
        var resultado = new CitaClienteDto
        {
            IdCita = nuevaCita.IdCita,
            FechaCita = nuevaCita.FechaCita,
            HoraInicio = nuevaCita.HoraInicio,
            Servicio = servicio.Nombre,
            Estado = estado.Nombre,
            Placa = vehiculo.Placa,
            Marca = vehiculo.IdMarcaNavigation.Nombre,
            Modelo = vehiculo.Modelo,
            Anio = vehiculo.Anio,
            Costo = nuevaCita.Costo,
            TiempoEstimadoMinutos = nuevaCita.TiempoEstimadoMinutos,
            Observaciones = nuevaCita.Observaciones
        };

        return (resultado, null);
    }


    public async Task<List<AgenteDisponibleDto>> ObtenerAgentesActivosAsync()
    {
        return await _context.Usuarios
            .AsNoTracking()
            .Where(u => u.IdRol == 2 && u.IdEstadoUsuario == 1)
            .OrderBy(u => u.Nombres)
            .ThenBy(u => u.Apellidos)
            .Select(u => new AgenteDisponibleDto
            {
                IdAgente = u.IdUsuario,
                Nombre = u.Nombres + " " + u.Apellidos,
                Iniciales = u.Nombres.Substring(0, 1) +
                            u.Apellidos.Substring(0, 1)
            })
            .ToListAsync();
    }

    public async Task<List<AgenteDisponibleDto>> ObtenerAgentesDisponiblesAsync(
        DateOnly fecha,
        TimeOnly horaInicio,
        long idTipoServicio)
    {
        var servicio = await _context.CatTiposServicios
            .AsNoTracking()
            .FirstOrDefaultAsync(s =>
                s.IdTipoServicio == idTipoServicio && s.Activo);

        if (servicio is null || servicio.TiempoEstimadoMinutos <= 0)
            return new List<AgenteDisponibleDto>();

        var agentes = await _context.Usuarios
            .AsNoTracking()
            .Where(u => u.IdRol == 2 && u.IdEstadoUsuario == 1)
            .Select(u => new AgenteDisponibleDto
            {
                IdAgente = u.IdUsuario,
                Nombre = u.Nombres + " " + u.Apellidos,
                Iniciales = u.Nombres.Substring(0, 1) +
                            u.Apellidos.Substring(0, 1)
            })
            .ToListAsync();

        var estadosOcupados = new[] { "PENDIENTE", "CONFIRMADA", "EN_PROCESO" };

        var citasOcupadas = await _context.Citas
            .AsNoTracking()
            .Where(c =>
                c.FechaCita == fecha &&
                c.IdAgente != null &&
                estadosOcupados.Contains(c.IdEstadoCitaNavigation.Nombre))
            .Select(c => new
            {
                IdAgente = c.IdAgente!.Value,
                c.HoraInicio,
                c.TiempoEstimadoMinutos
            })
            .ToListAsync();

        var inicioNuevo = horaInicio.ToTimeSpan();
        var finNuevo = inicioNuevo.Add(
            TimeSpan.FromMinutes(servicio.TiempoEstimadoMinutos));

        return agentes
            .Where(agente => !citasOcupadas.Any(cita =>
            {
                if (cita.IdAgente != agente.IdAgente)
                    return false;

                var inicioExistente = cita.HoraInicio.ToTimeSpan();
                var finExistente = inicioExistente.Add(
                    TimeSpan.FromMinutes(cita.TiempoEstimadoMinutos));

                return inicioExistente < finNuevo &&
                       finExistente > inicioNuevo;
            }))
            .ToList();
    }

    public async Task<List<TipoServicioDto>> ObtenerTiposServicioActivosAsync()
    {
        return await _context.CatTiposServicios
            .AsNoTracking()
            .Where(s => s.Activo)
            .OrderBy(s => s.Nombre)
            .Select(s => new TipoServicioDto
            {
                IdTipoServicio = s.IdTipoServicio,
                Nombre = s.Nombre,
                Descripcion = s.Descripcion,
                CostoBase = s.CostoBase,
                TiempoEstimadoMinutos = s.TiempoEstimadoMinutos
            })
            .ToListAsync();
    }



}