namespace AutosPA.models
{
    public class Agente
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public string Especialidad { get; set; } = "";
        public bool Disponible { get; set; } = true;
    }
}
