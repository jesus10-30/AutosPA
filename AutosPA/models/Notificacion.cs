namespace AutosPA.models
{
    public class Notificacion
    {
        public int Id { get; set; }
        public int? SolicitudId { get; set; }
        public string Tipo { get; set; } = "solicitud";   // solicitud | recordatorio | calificacion
        public string Titulo { get; set; } = "";
        public string Mensaje { get; set; } = "";
        public string Hace { get; set; } = "";
        public string Destino { get; set; } = "";
        public bool Leida { get; set; }
    }
}
