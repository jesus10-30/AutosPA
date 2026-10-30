using AutosPA.models;

namespace AutosPA.Services;

public class AgenteServiceMock : IAgenteService
{
    private static DateTime Hoy => DateTime.Today;

    // Fecha futura que no caiga en domingo (el panel no muestra domingos)
    private static DateTime Habil(int dias)
    {
        var d = Hoy.AddDays(dias);
        return d.DayOfWeek == DayOfWeek.Sunday ? d.AddDays(1) : d;
    }

    private static string Hace(int minutos)
    {
        if (minutos >= 1440) return $"Hace {minutos / 1440} d";
        if (minutos >= 60) return $"Hace {minutos / 60} h";
        return $"Hace {minutos} min";
    }

    private readonly Agente _agente = new() { Id = 1, Nombre = "Carlos Ruiz", Especialidad = "Mecánica general" };
    private readonly List<Trabajo> _trabajos;
    private readonly List<Solicitud> _solicitudes;
    private readonly List<Notificacion> _notificaciones;

    public AgenteServiceMock()
    {
        _trabajos = new()
        {
            new() { Id = 1, Cliente = "Laura Jiménez", Vehiculo = "Mazda 3 2022", Placa = "P 987 XYZ", Servicio = "Diagnóstico general", Fecha = Hoy, Hora = new TimeOnly(10, 0), Estado = "proceso", CostoEstimado = "Q100–Q200", CostoSugerido = 150, EstimadoMin = 60, Inicio = DateTime.Now.AddMinutes(-35) },
            new() { Id = 2, Cliente = "Juan Pérez", Vehiculo = "Toyota Corolla 2018", Placa = "P 640 MKD", Servicio = "Cambio de aceite y filtros", Fecha = Hoy, Hora = new TimeOnly(15, 0), CostoEstimado = "Q150–Q300", CostoSugerido = 220, EstimadoMin = 45 },
            new() { Id = 3, Cliente = "Marta Orozco", Vehiculo = "Suzuki Swift 2020", Placa = "P 114 NQV", Servicio = "Alineación y balanceo", Fecha = Habil(1), Hora = new TimeOnly(10, 0), CostoEstimado = "Q60–Q180", CostoSugerido = 120, EstimadoMin = 50 },
            new() { Id = 4, Cliente = "Ana Morales", Vehiculo = "Kia Rio 2017", Placa = "P 622 HLT", Servicio = "Mantenimiento preventivo", Fecha = Habil(1), Hora = new TimeOnly(9, 0), CostoEstimado = "Q150–Q300", CostoSugerido = 240, EstimadoMin = 60 },
            new() { Id = 5, Cliente = "Carlos Méndez", Vehiculo = "Nissan Sentra 2021", Placa = "P 774 GHF", Servicio = "Llantas y alineación", Fecha = Habil(2), Hora = new TimeOnly(9, 0), CostoEstimado = "Q60–Q180", CostoSugerido = 120, EstimadoMin = 50 },
            new() { Id = 6, Cliente = "Mario Castillo", Vehiculo = "Hyundai Tucson 2020", Placa = "P 221 LLT", Servicio = "Reemplazo de frenos", Fecha = Hoy.AddDays(-1), Hora = new TimeOnly(9, 0), Estado = "finalizado", CostoEstimado = "Q200–Q450", EstimadoMin = 90, DuracionMin = 95, CostoFinal = 380, Comentario = "Excelente atención, muy rápido y explicó todo bien.", Calificacion = 5 },
            new() { Id = 7, Cliente = "Ana Castañeda", Vehiculo = "Kia Rio 2018", Placa = "P 553 ABH", Servicio = "Cambio de aceite", Fecha = Hoy.AddDays(-3), Hora = new TimeOnly(11, 0), Estado = "finalizado", CostoEstimado = "Q150–Q300", EstimadoMin = 40, DuracionMin = 55, CostoFinal = 185, Comentario = "Buen trabajo, pero tardó un poco más de lo esperado.", Calificacion = 4 },
            new() { Id = 8, Cliente = "Luis García", Vehiculo = "Mazda 3 2019", Placa = "P 987 XYZ", Servicio = "Frenos delanteros", Fecha = Hoy.AddDays(-5), Hora = new TimeOnly(14, 0), Estado = "finalizado", CostoEstimado = "Q200–Q450", EstimadoMin = 90, DuracionMin = 80, CostoFinal = 310 },
            // viene de la solicitud 5 (ya aceptada)
            new() { Id = 9, SolicitudId = 5, Cliente = "Luis García", Vehiculo = "Mazda 3 2019", Placa = "P 987 XYZ", Servicio = "Frenos delanteros", Fecha = Hoy, Hora = new TimeOnly(14, 0), CostoEstimado = "Q200–Q450", CostoSugerido = 320, EstimadoMin = 90 },
        };

        _solicitudes = new()
        {
            new() { Id = 1, Cliente = "Esteban Jiménez", Vehiculo = "Toyota Corolla 2019", Placa = "P 428 DKL", Servicio = "Mantenimiento preventivo", Fecha = Hoy, Hora = 11, Costo = "Q150–Q300", CostoSugerido = 240, Tiempo = "45–60 min", EstimadoMin = 60, HaceMin = 120, Notas = "Quiero que revisen también el aire acondicionado.", Docs = new() { new Doc("Seguro vigente", "verde"), new Doc("Inspección vence en 19 d", "ambar") } },
            new() { Id = 2, Cliente = "Est. Jiménez", Vehiculo = "Honda CR-V 2021", Placa = "P 019 CTM", Servicio = "Diagnóstico general", Fecha = Habil(4), Hora = 14, Costo = "Q100–Q200", CostoSugerido = 150, Tiempo = "30 min", EstimadoMin = 30, HaceMin = 1500, Notas = "Se enciende la luz del motor desde hace unos días.", Docs = new() { new Doc("Seguro vencido", "rojo"), new Doc("Inspección vigente", "verde") } },
            new() { Id = 3, Cliente = "Pedro Castillo", Vehiculo = "Toyota Hilux 2016", Placa = "P 311 BQR", Servicio = "Sistema de frenos", Fecha = Habil(2), Hora = 9, Costo = "Q200–Q450", CostoSugerido = 350, Tiempo = "1–2 h", EstimadoMin = 90, HaceMin = 300, Notas = "Hace ruido al frenar y el pedal baja demasiado.", Docs = new() { new Doc("Seguro vigente", "verde"), new Doc("Inspección sin cargar", "neutro") } },
            new() { Id = 4, Cliente = "Ana Morales", Vehiculo = "Kia Rio 2017", Placa = "P 622 HLT", Servicio = "Llantas y alineación", Fecha = Habil(1), Hora = 15, Costo = "Q60–Q180", CostoSugerido = 120, Tiempo = "50 min", EstimadoMin = 50, HaceMin = 30, Docs = new() { new Doc("Seguro vigente", "verde"), new Doc("Inspección vigente", "verde") } },
            new() { Id = 5, Cliente = "Luis García", Vehiculo = "Mazda 3 2019", Placa = "P 987 XYZ", Servicio = "Frenos delanteros", Fecha = Hoy, Hora = 14, Costo = "Q200–Q450", CostoSugerido = 320, Tiempo = "1–2 h", EstimadoMin = 90, HaceMin = 2900, Estado = "Aceptada", Docs = new() { new Doc("Seguro vigente", "verde") } },
            new() { Id = 6, Cliente = "Sofía Ramírez", Vehiculo = "Hyundai Accent 2018", Placa = "P 205 TRD", Servicio = "Reemplazo de componentes", Fecha = Habil(3), Hora = 10, Costo = "Desde Q80", Tiempo = "Según pieza", HaceMin = 4300, Estado = "Rechazada", Motivo = "No cuento con el repuesto o la herramienta", Docs = new() { new Doc("Seguro vigente", "verde") } },
        };

        _notificaciones = _solicitudes
            .Where(s => s.Estado == "Pendiente")
            .Select(s => new Notificacion
            {
                SolicitudId = s.Id,
                Tipo = "solicitud",
                Titulo = $"Nueva solicitud: {s.Servicio}",
                Mensaje = $"{s.Cliente} · {s.Vehiculo}",
                Hace = Hace(s.HaceMin),
                Destino = "/agente/solicitudes"
            })
            .ToList();

        _notificaciones.Add(new Notificacion { Tipo = "recordatorio", Titulo = "Recordatorio de trabajo", Mensaje = "Hoy a las 2:00 p. m.: Frenos delanteros · Mazda 3 2019 · Luis García", Hace = "Hace 1 h", Destino = "/agente/trabajos" });
        _notificaciones.Add(new Notificacion { Tipo = "calificacion", Titulo = "Nueva calificación", Mensaje = "Mario Castillo te calificó con 5 estrellas.", Hace = "Hace 2 días", Destino = "/agente/trabajos", Leida = true });

        for (var i = 0; i < _notificaciones.Count; i++) _notificaciones[i].Id = i + 1;
    }

    public Task<Agente> ObtenerAgenteActualAsync() => Task.FromResult(_agente);

    public Task<List<Trabajo>> ObtenerTrabajosAsync(DateTime desde, DateTime hasta) =>
        Task.FromResult(_trabajos.Where(t => t.Fecha.Date >= desde.Date && t.Fecha.Date <= hasta.Date).ToList());

    public Task<List<Solicitud>> ObtenerSolicitudesAsync() => Task.FromResult(_solicitudes.ToList());

    public Task<List<Solicitud>> ObtenerSolicitudesPendientesAsync() =>
        Task.FromResult(_solicitudes.Where(s => s.Estado == "Pendiente").ToList());

    // Las notificaciones de solicitudes ya respondidas dejan de mostrarse
    public Task<List<Notificacion>> ObtenerNotificacionesAsync() =>
        Task.FromResult(_notificaciones
            .Where(n => n.SolicitudId is null || _solicitudes.Any(s => s.Id == n.SolicitudId && s.Estado == "Pendiente"))
            .ToList());

    public Task CambiarDisponibilidadAsync(bool disponible)
    {
        _agente.Disponible = disponible;
        return Task.CompletedTask;
    }

    public Task IniciarTrabajoAsync(int id, DateTime inicio)
    {
        var t = _trabajos.First(x => x.Id == id);
        t.Estado = "proceso";
        t.Inicio = inicio;
        return Task.CompletedTask;
    }

    public Task FinalizarTrabajoAsync(int id, decimal costoFinal, int duracionMin, string notas)
    {
        var t = _trabajos.First(x => x.Id == id);
        t.Estado = "finalizado";
        t.CostoFinal = costoFinal;
        t.DuracionMin = duracionMin;
        if (!string.IsNullOrWhiteSpace(notas)) t.Notas = notas.Trim();
        // Con Supabase: aquí se inserta la notificación para el cliente
        return Task.CompletedTask;
    }

    public Task GuardarNotasAsync(int id, string notas)
    {
        _trabajos.First(x => x.Id == id).Notas = notas;
        return Task.CompletedTask;
    }

    public Task AceptarSolicitudAsync(int id)
    {
        var s = _solicitudes.First(x => x.Id == id);
        s.Estado = "Aceptada";
        s.Motivo = "";

        // La solicitud aceptada pasa a ser un trabajo "por hacer"
        if (!_trabajos.Any(t => t.SolicitudId == id))
        {
            _trabajos.Add(new Trabajo
            {
                Id = _trabajos.Max(t => t.Id) + 1,
                SolicitudId = id,
                Cliente = s.Cliente,
                Vehiculo = s.Vehiculo,
                Placa = s.Placa,
                Servicio = s.Servicio,
                Fecha = s.Fecha,
                Hora = new TimeOnly(s.Hora, 0),
                CostoEstimado = s.Costo,
                CostoSugerido = s.CostoSugerido,
                EstimadoMin = s.EstimadoMin,
                Notas = s.Notas
            });
        }
        // Con Supabase: aquí también se notifica al cliente
        return Task.CompletedTask;
    }

    public Task RechazarSolicitudAsync(int id, string motivo)
    {
        var s = _solicitudes.First(x => x.Id == id);
        s.Estado = "Rechazada";
        s.Motivo = motivo;
        // Con Supabase: aquí se envía el motivo al cliente para que reprograme
        return Task.CompletedTask;
    }

    public Task DeshacerSolicitudAsync(int id)
    {
        var s = _solicitudes.First(x => x.Id == id);
        s.Estado = "Pendiente";
        s.Motivo = "";
        _trabajos.RemoveAll(t => t.SolicitudId == id && t.Estado == "porhacer");
        return Task.CompletedTask;
    }

    public Task MarcarNotificacionLeidaAsync(int id)
    {
        var n = _notificaciones.FirstOrDefault(x => x.Id == id);
        if (n is not null) n.Leida = true;
        return Task.CompletedTask;
    }

    public Task MarcarTodasLeidasAsync()
    {
        _notificaciones.ForEach(n => n.Leida = true);
        return Task.CompletedTask;
    }
}