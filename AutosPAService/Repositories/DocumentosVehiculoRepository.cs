using AutosPAService.Data;
using AutosPAService.Entities;
using AutosPAService.DTOs;
using Microsoft.EntityFrameworkCore;

namespace AutosPAService.Repositories;

public class DocumentosVehiculoRepository
{
    private readonly ApplicationDbContext _context;

    public DocumentosVehiculoRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> VehiculoPerteneceAUsuarioAsync(
        long idVehiculo, long idUsuario)
    {
        return await _context.Vehiculos.AnyAsync(v =>
            v.IdVehiculo == idVehiculo &&
            v.IdConductorNavigation.IdUsuario == idUsuario);
    }

    public async Task<List<CatTiposDocumento>> ObtenerTiposAsync()
    {
        return await _context.CatTiposDocumentos
            .AsNoTracking()
            .Where(t => t.Activo)
            .OrderBy(t => t.Nombre)
            .ToListAsync();
    }

    public async Task<List<DocumentoVehiculoDTO>> ObtenerPorVehiculoAsync(
        long idVehiculo)
    {
        return await _context.DocumentosVehiculos
            .AsNoTracking()
            .Where(d => d.IdVehiculo == idVehiculo)
            .Include(d => d.IdTipoDocumentoNavigation)
            .Include(d => d.IdEstadoDocumentoNavigation)
            .OrderByDescending(d => d.FechaRegistro)
            .Select(d => new DocumentoVehiculoDTO
            {
                IdDocumento = d.IdDocumento,
                IdTipoDocumento = d.IdTipoDocumento,
                Tipo = d.IdTipoDocumentoNavigation.Nombre,
                Archivo = d.NombreArchivo ?? "",
                FechaVencimiento = d.FechaVencimiento.HasValue
                    ? d.FechaVencimiento.Value.ToDateTime(TimeOnly.MinValue)
                    : null,
                Estado = d.IdEstadoDocumentoNavigation.Nombre
            })
            .ToListAsync();
    }

    public async Task<long?> ObtenerEstadoActivoAsync()
    {
        return await _context.CatEstadosDocumentos
            .AsNoTracking()
            .Where(e => e.Activo)
            .OrderBy(e => e.IdEstadoDocumento)
            .Select(e => (long?)e.IdEstadoDocumento)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> TipoActivoAsync(long idTipoDocumento)
    {
        return await _context.CatTiposDocumentos.AnyAsync(t =>
            t.IdTipoDocumento == idTipoDocumento && t.Activo);
    }

    public async Task AgregarAsync(DocumentosVehiculo documento)
    {
        _context.DocumentosVehiculos.Add(documento);
        await _context.SaveChangesAsync();
    }

    public async Task<DocumentosVehiculo?> ObtenerDocumentoDeUsuarioAsync(
        long idDocumento, long idUsuario)
    {
        return await _context.DocumentosVehiculos
            .Include(d => d.IdVehiculoNavigation)
                .ThenInclude(v => v.IdConductorNavigation)
            .FirstOrDefaultAsync(d =>
                d.IdDocumento == idDocumento &&
                d.IdVehiculoNavigation.IdConductorNavigation.IdUsuario == idUsuario);
    }

    public async Task EliminarAsync(DocumentosVehiculo documento)
    {
        _context.DocumentosVehiculos.Remove(documento);
        await _context.SaveChangesAsync();
    }
}