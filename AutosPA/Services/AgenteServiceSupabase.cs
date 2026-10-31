
using AutosPA.Data;
using AutosPA.models;
using System.Globalization;
using static AutosPA.Data.UsuarioDb;
using Op = Supabase.Postgrest.Constants.Operator;

namespace AutosPA.Services
{
    public class AgenteServiceSupabase : IAgenteService
    {
        // ids de cat_estados_cita y cat_roles
        private const long EstPendiente = 1, EstConfirmada = 2, EstEnProceso = 3, EstFinalizada = 4;
        private const long RolAgente = 2;

        private readonly Supabase.Client _client;
        private bool _init;
        private UsuarioDb? _agente;

        // Disponibilidad temporal: la tabla usuarios no tiene esta columna.
        private bool _disponible = true;

        // Las notificaciones se derivan de citas; "leída" vive en memoria.
        private readonly HashSet<int> _leidas = new();

        public AgenteServiceSupabase(Supabase.Client client) => _client = client;

        // ---------- Infraestructura ----------
        private async Task<Supabase.Client> Db()
        {
            if (!_init)
            {
                await _client.InitializeAsync();
                _init = true;
            }

            return _client;
        }

        private async Task<UsuarioDb> AgenteAsync()
        {
            if (_agente is not null)
                return _agente;

            var db = await Db();
            UsuarioDb? u = null;

            var authId = db.Auth.CurrentUser?.Id;
            Console.WriteLine($"SUPABASE AUTH ID: {authId ?? "SIN SESIÓN"}");
            Console.WriteLine($"SUPABASE CURRENT USER: {db.Auth.CurrentUser?.Email ?? "SIN CORREO"}");
            Console.WriteLine($"AGENTE ENCONTRADO: {u?.IdUsuario}");
            if (!string.IsNullOrEmpty(authId))
            {
                u = await db.From<UsuarioDb>()
                    .Select(UsuarioDb.Columnas)
                    .Filter("auth_user_id", Op.Equals, authId)
                    .Single();
            }

#if DEBUG
            // SOLO PARA PRUEBAS: si no hay sesión, usa el primer agente de la base.
            u ??= await db.From<UsuarioDb>()
                .Select(UsuarioDb.Columnas)
                .Filter("id_rol", Op.Equals, RolAgente.ToString())
                .Order("id_usuario", Supabase.Postgrest.Constants.Ordering.Ascending)
                .Limit(1)
                .Single();
#endif
            Console.WriteLine($"AGENTE ENCONTRADO: {u.IdUsuario}");
            Console.WriteLine($"NOMBRE DEL AGENTE: {u.Nombres} {u.Apellidos}");
            if (u is null || u.IdRol != RolAgente)
                throw new InvalidOperationException("No hay un agente con sesión iniciada.");

            return _agente = u;
        }

        // ---------- Lectura ----------
        public async Task<Agente> ObtenerAgenteActualAsync()
        {
            var a = await AgenteAsync();

            return new Agente
            {
                Id = (int)a.IdUsuario,
                Nombre = $"{a.Nombres} {a.Apellidos}".Trim(),
                Especialidad = "Mecánica general",
                Disponible = _disponible
            };
        }

        public async Task<List<Trabajo>> ObtenerTrabajosAsync(DateTime desde, DateTime hasta)
        {
            var db = await Db();
            var a = await AgenteAsync();

            var res = await db.From<CitaDb>()
                .Filter("id_agente", Op.Equals, a.IdUsuario.ToString())
                .Filter("id_estado_cita", Op.In, new List<long>
                {
                    EstPendiente, EstConfirmada, EstEnProceso, EstFinalizada
                })
                .Filter("fecha_cita", Op.GreaterThanOrEqual, desde.ToString("yyyy-MM-dd"))
                .Filter("fecha_cita", Op.LessThanOrEqual, hasta.ToString("yyyy-MM-dd"))
                .Get();

            var rel = await CargarRel(res.Models);

            return res.Models.Select(c => AUnTrabajo(c, rel)).ToList();
        }

        public async Task<List<Solicitud>> ObtenerSolicitudesAsync()
        {
            var db = await Db();
            var a = await AgenteAsync();
            var yo = a.IdUsuario.ToString();

            // Rechazos hechos por este agente.
            var rechazos = (await db.From<RechazoDb>()
                .Filter("id_agente", Op.Equals, yo)
                .Get()).Models;

            var idsRech = rechazos.Select(r => r.IdCita).ToHashSet();

            // Pendientes dirigidas a mí.
            var p1 = (await db.From<CitaDb>()
                .Filter("id_estado_cita", Op.Equals, EstPendiente.ToString())
                .Filter("id_agente_destino", Op.Equals, yo)
                .Get()).Models;

            var p2 = (await db.From<CitaDb>()
                .Filter("id_estado_cita", Op.Equals, EstPendiente.ToString())
                .Filter("id_agente", Op.Equals, yo)
                .Get()).Models;

            // Aceptadas recientes (últimos 30 días).
            var desde = DateTime.Today.AddDays(-30).ToString("yyyy-MM-dd");

            var aceptadas = (await db.From<CitaDb>()
                .Filter("id_agente", Op.Equals, yo)
                .Filter("id_estado_cita", Op.In, new List<long>
                {
                    EstConfirmada, EstEnProceso, EstFinalizada
                })
                .Filter("fecha_cita", Op.GreaterThanOrEqual, desde)
                .Get()).Models;

            // Citas que yo rechacé.
            var rechazadas = idsRech.Count == 0
                ? new List<CitaDb>()
                : (await db.From<CitaDb>()
                    .Filter("id_cita", Op.In, idsRech.ToList())
                    .Get()).Models;

            var items = new List<(CitaDb Cita, string Estado)>();

            items.AddRange(
                p1.Concat(p2)
                    .Where(c => !idsRech.Contains(c.IdCita))
                    .Select(c => (c, "Pendiente")));

            items.AddRange(aceptadas.Select(c => (c, "Aceptada")));
            items.AddRange(rechazadas.Select(c => (c, "Rechazada")));

            items = items
                .GroupBy(i => i.Cita.IdCita)
                .Select(g => g.First())
                .ToList();

            var citas = items.Select(i => i.Cita).ToList();
            var rel = await CargarRel(citas);
            var docs = await DocsPorVehiculo(citas.Select(c => c.IdVehiculo));

            return items.Select(i =>
            {
                var motivo = rechazos
                    .FirstOrDefault(r => r.IdCita == i.Cita.IdCita)?.Motivo ?? "";

                docs.TryGetValue(i.Cita.IdVehiculo, out var d);

                return AUnaSolicitud(
                    i.Cita,
                    rel,
                    i.Estado,
                    i.Estado == "Rechazada" ? motivo : "",
                    d ?? new());
            }).ToList();
        }

        public async Task<List<Solicitud>> ObtenerSolicitudesPendientesAsync()
        {
            var solicitudes = await ObtenerSolicitudesAsync();

            return solicitudes
                .Where(s => s.Estado == "Pendiente")
                .ToList();
        }

        // Las notificaciones del agente se derivan de sus citas.
        public async Task<List<Notificacion>> ObtenerNotificacionesAsync()
        {
            var lista = new List<Notificacion>();

            var pendientes = await ObtenerSolicitudesPendientesAsync();

            foreach (var s in pendientes.OrderBy(x => x.HaceMin))
            {
                lista.Add(new Notificacion
                {
                    Id = s.Id,
                    Tipo = "solicitud",
                    Titulo = $"Nueva solicitud: {s.Servicio}",
                    Mensaje = $"{s.Cliente} · {s.Vehiculo}",
                    Hace = Hace(s.HaceMin),
                    Destino = "/agente/solicitudes",
                    Leida = _leidas.Contains(s.Id)
                });
            }

            var hoy = await ObtenerTrabajosAsync(DateTime.Today, DateTime.Today);

            var siguiente = hoy
                .Where(t => t.Estado == "porhacer")
                .OrderBy(t => t.Hora)
                .FirstOrDefault();

            if (siguiente is not null)
            {
                lista.Add(new Notificacion
                {
                    Id = -siguiente.Id,
                    Tipo = "recordatorio",
                    Titulo = "Recordatorio de trabajo",
                    Mensaje = $"Hoy a las {FormatoHora(siguiente.Hora)}: {siguiente.Servicio} · {siguiente.Vehiculo} · {siguiente.Cliente}",
                    Hace = "Hoy",
                    Destino = "/agente/trabajos",
                    Leida = _leidas.Contains(-siguiente.Id)
                });
            }

            return lista;
        }

        // ---------- Escritura ----------
        public Task CambiarDisponibilidadAsync(bool disponible)
        {
            _disponible = disponible;
            return Task.CompletedTask;
        }

        public async Task FinalizarTrabajoAsync(
    int id,
    decimal costoFinal,
    int duracionMin,
    string notas)
        {
            var cita = await ActualizarCita(id, c =>
            {
                c.IdEstadoCita = EstFinalizada;

                if (costoFinal > 0)
                    c.Costo = costoFinal;

                if (!string.IsNullOrWhiteSpace(notas))
                    c.Observaciones = notas.Trim();
            });

            await NotificarCliente(cita, "Tu vehículo ya está listo para recoger.");
        }

        public Task GuardarNotasAsync(int id, string notas) =>
            ActualizarCita(id, c => c.Observaciones = notas);

        public async Task AceptarSolicitudAsync(int id)
        {
            var a = await AgenteAsync();

            var cita = await ActualizarCita(id, c =>
            {
                c.IdAgente = a.IdUsuario;
                c.IdEstadoCita = EstConfirmada;
            });

            await NotificarCliente(
                cita,
                $"Tu cita del {cita.FechaCita:dd/MM/yyyy} fue confirmada.");
        }

        public async Task RechazarSolicitudAsync(int id, string motivo)
        {
            var db = await Db();
            var a = await AgenteAsync();

            await db.From<RechazoDb>().Insert(new RechazoDb
            {
                IdCita = id,
                IdAgente = a.IdUsuario,
                Motivo = motivo,
                FechaRechazo = DateTime.UtcNow
            });

            // La cita no se cancela: el sistema decide si se reasigna a otro agente.
            var cita = await db.From<CitaDb>()
                .Filter("id_cita", Op.Equals, id.ToString())
                .Single();

            if (cita is not null)
                await NotificarCliente(cita, $"Tu cita no pudo ser atendida: {motivo}");
        }

        public async Task DeshacerSolicitudAsync(int id)
        {
            var db = await Db();
            var a = await AgenteAsync();
            var yo = a.IdUsuario.ToString();

            var rechazo = await db.From<RechazoDb>()
                .Filter("id_cita", Op.Equals, id.ToString())
                .Filter("id_agente", Op.Equals, yo)
                .Single();

            if (rechazo is not null)
            {
                await db.From<RechazoDb>()
                    .Filter("id_rechazo", Op.Equals, rechazo.IdRechazo.ToString())
                    .Delete();

                return;
            }

            // Deshacer una aceptación = volver a pendiente.
            await ActualizarCita(id, c =>
            {
                c.IdEstadoCita = EstPendiente;

                if (c.IdAgenteDestino == a.IdUsuario)
                    c.IdAgente = null;
            });
        }

        public Task MarcarNotificacionLeidaAsync(int id)
        {
            _leidas.Add(id);
            return Task.CompletedTask;
        }

        public async Task MarcarTodasLeidasAsync()
        {
            foreach (var n in await ObtenerNotificacionesAsync())
                _leidas.Add(n.Id);
        }

        // ---------- Apoyo ----------

        private async Task<CitaDb> ActualizarCita(int id, Action<CitaDb> cambios)
        {
            var db = await Db();

            var c = await db.From<CitaDb>()
                .Filter("id_cita", Op.Equals, id.ToString())
                .Single()
                ?? throw new InvalidOperationException("No se encontró la cita.");

            cambios(c);
            c.FechaActualizacion = DateTime.UtcNow;

            await db.From<CitaDb>()
                .Filter("id_cita", Op.Equals, id.ToString())
                .Set(x => x.IdAgente, c.IdAgente)
                .Set(x => x.IdEstadoCita, c.IdEstadoCita)
                .Set(x => x.Costo, c.Costo)
                .Set(x => x.Observaciones, c.Observaciones)
                .Set(x => x.FechaActualizacion, c.FechaActualizacion)
                .Update();

            return c;
        }


        // Aviso al cliente: si falla, no debe romper la acción principal.
        private async Task NotificarCliente(CitaDb cita, string mensaje)
        {
            try
            {
                var db = await Db();

                await db.From<NotificacionDb>().Insert(new NotificacionDb
                {
                    IdCliente = cita.IdCliente,
                    IdCita = cita.IdCita,
                    Mensaje = mensaje,
                    Via = "APP",
                    FechaEnvio = DateTime.Today,
                    Estado = "PENDIENTE"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"No se pudo notificar al cliente: {ex.Message}");
            }
        }

        private sealed class Rel
        {
            public Dictionary<long, UsuarioDb> Usuarios = new();
            public Dictionary<long, VehiculoDb> Vehiculos = new();
            public Dictionary<long, string> Marcas = new();
            public Dictionary<long, TipoServicioDb> Servicios = new();
        }

        private async Task<Rel> CargarRel(IEnumerable<CitaDb> citas)
        {
            var rel = new Rel();
            var lista = citas.ToList();

            if (lista.Count == 0)
                return rel;

            var db = await Db();

            var idsCli = lista.Select(c => c.IdCliente).Distinct().ToList();
            var idsVeh = lista.Select(c => c.IdVehiculo).Distinct().ToList();

            var us = await db.From<UsuarioDb>()
                .Select(UsuarioDb.Columnas)
                .Filter("id_usuario", Op.In, idsCli)
                .Get();

            rel.Usuarios = us.Models.ToDictionary(u => u.IdUsuario);

            var vs = await db.From<VehiculoDb>()
                .Filter("id_vehiculo", Op.In, idsVeh)
                .Get();

            rel.Vehiculos = vs.Models.ToDictionary(v => v.IdVehiculo);

            var marcas = await db.From<MarcaDb>().Get();

            rel.Marcas = marcas.Models.ToDictionary(
                m => m.IdMarca,
                m => m.Nombre);

            var servs = await db.From<TipoServicioDb>().Get();

            rel.Servicios = servs.Models.ToDictionary(
                s => s.IdTipoServicio);

            return rel;
        }

        private async Task<Dictionary<long, List<Doc>>> DocsPorVehiculo(
            IEnumerable<long> idsVeh)
        {
            var res = new Dictionary<long, List<Doc>>();
            var ids = idsVeh.Distinct().ToList();

            if (ids.Count == 0)
                return res;

            var db = await Db();

            var docs = (await db.From<DocumentoDb>()
                .Filter("id_vehiculo", Op.In, ids)
                .Get()).Models;

            var tipos = (await db.From<TipoDocumentoDb>().Get()).Models
                .ToDictionary(t => t.IdTipoDocumento, t => t.Nombre);

            foreach (var g in docs.GroupBy(d => (d.IdVehiculo, d.IdTipoDocumento)))
            {
                var vence = g.Max(d => d.FechaVencimiento);

                var tipo = tipos.TryGetValue(g.Key.IdTipoDocumento, out var n)
                    ? n
                    : "Documento";

                if (!res.TryGetValue(g.Key.IdVehiculo, out var lista))
                    res[g.Key.IdVehiculo] = lista = new();

                lista.Add(ADoc(tipo, vence));
            }

            return res;
        }

        // ---------- Conversión base de datos → modelos de las páginas ----------
        private static string NombreCliente(CitaDb c, Rel r) =>
            r.Usuarios.TryGetValue(c.IdCliente, out var u)
                ? $"{u.Nombres} {u.Apellidos}".Trim()
                : $"Cliente #{c.IdCliente}";

        private static string NombreVehiculo(CitaDb c, Rel r)
        {
            if (!r.Vehiculos.TryGetValue(c.IdVehiculo, out var v))
                return "Vehículo";

            var marca = r.Marcas.TryGetValue(v.IdMarca, out var m) ? m : "";

            return $"{marca} {v.Modelo} {v.Anio}"
                .Replace("  ", " ")
                .Trim();
        }

        private static Trabajo AUnTrabajo(CitaDb c, Rel r)
        {
            r.Servicios.TryGetValue(c.IdTipoServicio, out var serv);
            r.Vehiculos.TryGetValue(c.IdVehiculo, out var veh);

            var costo = c.Costo ?? serv?.CostoBase ?? 0m;

            var estado = c.IdEstadoCita switch
            {
                EstEnProceso => "proceso",
                EstFinalizada => "finalizado",
                _ => "porhacer"
            };

            return new Trabajo
            {
                Id = (int)c.IdCita,
                // Las citas nuevas se muestran en la agenda como pendientes,
                // pero se aceptan desde Solicitudes antes de poder iniciarlas.
                SolicitudId = c.IdEstadoCita == EstPendiente ? (int)c.IdCita : null,
                Cliente = NombreCliente(c, r),
                Vehiculo = NombreVehiculo(c, r),
                Placa = veh?.Placa ?? "",
                Servicio = Bonito(serv?.Nombre ?? "Servicio"),
                Fecha = c.FechaCita.Date,
                Hora = TimeOnly.FromTimeSpan(c.HoraInicio),
                Estado = estado,
                CostoEstimado = Quetzales(costo),
                CostoSugerido = costo,
                EstimadoMin = c.TiempoEstimadoMinutos ?? serv?.TiempoEstimadoMinutos ?? 60,
                Inicio = estado == "proceso" && c.FechaActualizacion is { } fa
                    ? Local(fa)
                    : null,
                DuracionMin = 0,
                CostoFinal = estado == "finalizado" ? c.Costo ?? costo : 0m,
                Notas = c.Observaciones ?? ""
            };
        }

        private static Solicitud AUnaSolicitud(
            CitaDb c,   
            Rel r,
            string estado,
            string motivo,
            List<Doc> docs)
        {
            r.Servicios.TryGetValue(c.IdTipoServicio, out var serv);
            r.Vehiculos.TryGetValue(c.IdVehiculo, out var veh);

            var costo = c.Costo ?? serv?.CostoBase ?? 0m;
            var minutos = c.TiempoEstimadoMinutos ?? serv?.TiempoEstimadoMinutos ?? 60;
            var creada = c.FechaCreacion is { } fc ? Local(fc) : DateTime.Now;

            return new Solicitud
            {
                Id = (int)c.IdCita,
                Cliente = NombreCliente(c, r),
                Vehiculo = NombreVehiculo(c, r),
                Placa = veh?.Placa ?? "",
                Servicio = Bonito(serv?.Nombre ?? "Servicio"),
                Fecha = c.FechaCita.Date,
                Hora = c.HoraInicio.Hours,
                Costo = Quetzales(costo),
                CostoSugerido = costo,
                Tiempo = $"{minutos} min",
                EstimadoMin = minutos,
                Notas = c.Observaciones ?? "",
                HaceMin = Math.Max(0, (int)(DateTime.Now - creada).TotalMinutes),
                Docs = docs,
                Estado = estado,
                Motivo = motivo
            };
        }

        private static Doc ADoc(string tipo, DateTime? vence)
        {
            var nombre = Bonito(tipo);

            if (vence is null)
                return new Doc($"{nombre} registrado", "neutro");

            var dias = (vence.Value.Date - DateTime.Today).Days;

            if (dias < 0)
                return new Doc($"{nombre} vencido", "rojo");

            if (dias <= 30)
                return new Doc($"{nombre} vence en {dias} d", "ambar");

            return new Doc($"{nombre} vigente", "verde");
        }

        // ---------- Formatos ----------
        private static string Quetzales(decimal monto) =>
            "Q" + monto.ToString("N2", CultureInfo.InvariantCulture);

        private static string Bonito(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return s;

            s = s.Replace('_', ' ').Trim().ToLowerInvariant();

            return char.ToUpper(s[0]) + s[1..];
        }

        private static DateTime Local(DateTime d) => d.Kind switch
        {
            DateTimeKind.Utc => d.ToLocalTime(),
            DateTimeKind.Unspecified => DateTime.SpecifyKind(d, DateTimeKind.Utc).ToLocalTime(),
            _ => d
        };

        private static string Hace(int minutos)
        {
            if (minutos >= 1440)
                return $"Hace {minutos / 1440} d";

            if (minutos >= 60)
                return $"Hace {minutos / 60} h";

            return $"Hace {minutos} min";
        }

        private static string FormatoHora(TimeOnly h)
        {
            var h12 = h.Hour % 12 == 0 ? 12 : h.Hour % 12;

            return $"{h12}:{h.Minute:00} {(h.Hour > 11 ? "p. m." : "a. m.")}";
        }


        public async Task IniciarTrabajoAsync(int trabajoId, DateTime inicio)
        {
            var cita = await ActualizarCita(trabajoId, c =>
            {
                c.IdEstadoCita = EstEnProceso;
                c.FechaActualizacion = inicio.ToUniversalTime();
            });

            await NotificarCliente(
                cita,
                "El agente ha comenzado a trabajar en tu vehículo.");
        }

    }
}
