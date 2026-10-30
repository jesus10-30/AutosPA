
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace AutosPA.Data;

[Table("citas")]
public class CitaDb : BaseModel
{
    [PrimaryKey("id_cita", false)] public long IdCita { get; set; }
    [Column("id_cliente")] public long IdCliente { get; set; }
    [Column("id_agente")] public long? IdAgente { get; set; }
    [Column("id_vehiculo")] public long IdVehiculo { get; set; }
    [Column("id_tipo_servicio")] public long IdTipoServicio { get; set; }
    [Column("id_estado_cita")] public long IdEstadoCita { get; set; }
    [Column("fecha_cita")] public DateTime FechaCita { get; set; }
    [Column("hora_inicio")] public TimeSpan HoraInicio { get; set; }
    [Column("costo")] public decimal? Costo { get; set; }
    [Column("tiempo_estimado_minutos")] public int? TiempoEstimadoMinutos { get; set; }
    [Column("observaciones")] public string? Observaciones { get; set; }
    [Column("fecha_creacion")] public DateTime? FechaCreacion { get; set; }
    [Column("fecha_actualizacion")] public DateTime? FechaActualizacion { get; set; }
    [Column("modo_asignacion")] public string? ModoAsignacion { get; set; }
    [Column("id_agente_destino")] public long? IdAgenteDestino { get; set; }

   
}

[Table("usuarios")]
public class UsuarioDb : BaseModel
{
    // Se piden solo estas columnas; nunca password_hash
    public const string Columnas = "id_usuario,id_rol,id_estado_usuario,nombres,apellidos";

    [PrimaryKey("id_usuario", false)] public long IdUsuario { get; set; }
    [Column("id_rol")] public long IdRol { get; set; }
    [Column("nombres")] public string Nombres { get; set; } = "";
    [Column("apellidos")] public string Apellidos { get; set; } = "";
    [Column("id_estado_usuario")]
    public long IdEstadoUsuario { get; set; }

    [Table("vehiculos")]
    public class VehiculoDb : BaseModel
    {
        [PrimaryKey("id_vehiculo", false)] public long IdVehiculo { get; set; }
        [Column("id_marca")] public long IdMarca { get; set; }
        [Column("modelo")] public string Modelo { get; set; } = "";
        [Column("anio")] public int? Anio { get; set; }
        [Column("placa")] public string Placa { get; set; } = "";
    }

    [Table("cat_marcas")]
    public class MarcaDb : BaseModel
    {
        [PrimaryKey("id_marca", false)] public long IdMarca { get; set; }
        [Column("nombre")] public string Nombre { get; set; } = "";
    }

    [Table("cat_tipos_servicio")]
    public class TipoServicioDb : BaseModel
    {
        [PrimaryKey("id_tipo_servicio", false)] public long IdTipoServicio { get; set; }
        [Column("nombre")] public string Nombre { get; set; } = "";
        [Column("costo_base")] public decimal? CostoBase { get; set; }
        [Column("tiempo_estimado_minutos")] public int? TiempoEstimadoMinutos { get; set; }
    }

    [Table("documentos_vehiculo")]
    public class DocumentoDb : BaseModel
    {
        [PrimaryKey("id_documento", false)] public long IdDocumento { get; set; }
        [Column("id_vehiculo")] public long IdVehiculo { get; set; }
        [Column("id_tipo_documento")] public long IdTipoDocumento { get; set; }
        [Column("fecha_vencimiento")] public DateTime? FechaVencimiento { get; set; }
    }

    [Table("cat_tipos_documento")]
    public class TipoDocumentoDb : BaseModel
    {
        [PrimaryKey("id_tipo_documento", false)] public long IdTipoDocumento { get; set; }
        [Column("nombre")] public string Nombre { get; set; } = "";
    }

    [Table("rechazos_cita")]
    public class RechazoDb : BaseModel
    {
        [PrimaryKey("id_rechazo", false)] public long IdRechazo { get; set; }
        [Column("id_cita")] public long IdCita { get; set; }
        [Column("id_agente")] public long IdAgente { get; set; }
        [Column("motivo")] public string Motivo { get; set; } = "";
        [Column("fecha_rechazo")] public DateTime? FechaRechazo { get; set; }
    }

    [Table("notificaciones")]
    public class NotificacionDb : BaseModel
    {
        [PrimaryKey("id", false)] public long Id { get; set; }
        [Column("id_cliente")] public long IdCliente { get; set; }
        [Column("id_cita")] public long? IdCita { get; set; }
        [Column("mensaje")] public string Mensaje { get; set; } = "";
        [Column("via")] public string? Via { get; set; }
        [Column("fecha_envio")] public DateTime? FechaEnvio { get; set; }
        [Column("estado")] public string? Estado { get; set; }
    }
}
