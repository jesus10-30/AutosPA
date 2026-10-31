using System;

namespace AutosPALibrary.Dtos
{
    public class CitaClienteDto
    {
        public long IdCita { get; set; }

        public DateOnly FechaCita { get; set; }

        public TimeOnly HoraInicio { get; set; }

        public string Servicio { get; set; } = string.Empty;

        public string Estado { get; set; } = string.Empty;

        public string Placa { get; set; } = string.Empty;

        public string Marca { get; set; } = string.Empty;

        public string Modelo { get; set; } = string.Empty;

        public int Anio { get; set; }

        public decimal Costo { get; set; }

        public int TiempoEstimadoMinutos { get; set; }

        public string? Observaciones { get; set; }
    }
}