namespace AutosPA.models
{
    public class Solicitud
    {
        public int Id { get; set; }
        public string Cliente { get; set; } = "";
        public string Vehiculo { get; set; } = "";
        public string Placa { get; set; } = "";
        public string Servicio { get; set; } = "";
        public DateTime Fecha { get; set; }
        public int Hora { get; set; }
        public string Costo { get; set; } = "";
        public decimal CostoSugerido { get; set; }
        public string Tiempo { get; set; } = "";
        public int EstimadoMin { get; set; } = 60;
        public string Notas { get; set; } = "";
        public int HaceMin { get; set; }
        public List<Doc> Docs { get; set; } = new();
        public string Estado { get; set; } = "Pendiente";   // Pendiente | Aceptada | Rechazada
        public string Motivo { get; set; } = "";
    }
}
