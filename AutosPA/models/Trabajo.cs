namespace AutosPA.models
{
    public class Trabajo
    {
        public int Id { get; set; }
        public int? SolicitudId { get; set; }
        public string Cliente { get; set; } = "";
        public string Vehiculo { get; set; } = "";
        public string Placa { get; set; } = "";
        public string Servicio { get; set; } = "";
        public DateTime Fecha { get; set; }
        public TimeOnly Hora { get; set; }
        public string Estado { get; set; } = "porhacer";   // porhacer | proceso | finalizado
        public string CostoEstimado { get; set; } = "";
        public decimal CostoSugerido { get; set; }
        public int EstimadoMin { get; set; } = 60;
        public DateTime? Inicio { get; set; }
        public int DuracionMin { get; set; }
        public decimal CostoFinal { get; set; }
        public string Notas { get; set; } = "";
        public string? Comentario { get; set; }
        public int Calificacion { get; set; }
    }
}
