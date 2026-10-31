
using AutosPAService.Data;
using AutosPALibrary.DTOs;
using Microsoft.EntityFrameworkCore;

namespace AutosPAService.Repositories;

public class NotificacionesRepository
{
    private readonly ApplicationDbContext _context;

    public NotificacionesRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<NotificacionDTO>> ObtenerPorClienteAsync(long idCliente)
{
    var consulta =
        from n in _context.Notificaciones.AsNoTracking()
        join b in _context.NotificacionesBackups.AsNoTracking()
            on n.Id equals b.IdNotificacion into respaldos
        from b in respaldos.DefaultIfEmpty()
        join t in _context.CatTiposNotificacions.AsNoTracking()
            on b.IdTipoNotificacion equals (long?)t.IdTipoNotificacion
            into tipos
        from t in tipos.DefaultIfEmpty()
        where n.IdCliente == idCliente
        orderby n.CreatedAt descending
        select new NotificacionDTO
        {
            Id = n.Id,
            IdCita = n.IdCita,
            IdTipoNotificacion = b.IdNotificacion != null
                ? b.IdTipoNotificacion
                : null,
            Titulo = b.Titulo ?? string.Empty,
            Mensaje = n.Mensaje,
            Tipo = t != null && t.Nombre != null
                ? t.Nombre
                : string.Empty,
            CreatedAt = n.CreatedAt,
            Leida = b.Leida == true
        };

    return await consulta.ToListAsync();
}

    public async Task<bool> MarcarComoLeidaAsync(
        long idNotificacion,
        long idCliente)
    {
        await using var transaccion =
            await _context.Database.BeginTransactionAsync();

        var existe = await _context.Notificaciones.AnyAsync(n =>
            n.Id == idNotificacion &&
            n.IdCliente == idCliente);

        if (!existe)
            return false;

        var filas = await _context.Database.ExecuteSqlInterpolatedAsync(
            $"""
            UPDATE public.notificaciones_backup
            SET leida = TRUE,
                fecha_lectura = COALESCE(fecha_lectura, NOW())
            WHERE id_notificacion = {idNotificacion}
              AND id_usuario = {idCliente}
            """);

        if (filas == 0)
        {
            await _context.Database.ExecuteSqlInterpolatedAsync(
                $"""
                INSERT INTO public.notificaciones_backup (
                    id_notificacion,
                    id_usuario,
                    id_tipo_notificacion,
                    id_cita,
                    titulo,
                    mensaje,
                    leida,
                    fecha_creacion,
                    fecha_lectura
                )
                SELECT
                    n.id,
                    n.id_cliente,
                    NULL,
                    n.id_cita,
                    NULL,
                    n.mensaje,
                    TRUE,
                    n.created_at,
                    NOW()
                FROM public.notificaciones AS n
                WHERE n.id = {idNotificacion}
                  AND n.id_cliente = {idCliente}
                """);
        }

        await transaccion.CommitAsync();

        return true;
    }

    public async Task<int> MarcarTodasComoLeidasAsync(long idCliente)
    {
        await using var transaccion =
            await _context.Database.BeginTransactionAsync();

        // Crear los respaldos que todavía no existen.
        await _context.Database.ExecuteSqlInterpolatedAsync(
            $"""
            INSERT INTO public.notificaciones_backup (
                id_notificacion,
                id_usuario,
                id_tipo_notificacion,
                id_cita,
                titulo,
                mensaje,
                leida,
                fecha_creacion,
                fecha_lectura
            )
            SELECT
                n.id,
                n.id_cliente,
                NULL,
                n.id_cita,
                NULL,
                n.mensaje,
                TRUE,
                n.created_at,
                NOW()
            FROM public.notificaciones AS n
            WHERE n.id_cliente = {idCliente}
              AND NOT EXISTS (
                  SELECT 1
                  FROM public.notificaciones_backup AS b
                  WHERE b.id_notificacion = n.id
                    AND b.id_usuario = n.id_cliente
              )
            """);

        // Marcar como leídos los respaldos existentes.
        var filas = await _context.Database.ExecuteSqlInterpolatedAsync(
            $"""
            UPDATE public.notificaciones_backup AS b
            SET leida = TRUE,
                fecha_lectura = COALESCE(b.fecha_lectura, NOW())
            WHERE b.id_usuario = {idCliente}
              AND b.leida IS DISTINCT FROM TRUE
              AND EXISTS (
                  SELECT 1
                  FROM public.notificaciones AS n
                  WHERE n.id = b.id_notificacion
                    AND n.id_cliente = {idCliente}
              )
            """);

        await transaccion.CommitAsync();

        return filas;
    }

    public async Task<bool> EliminarPorClienteAsync(
        long idNotificacion,
        long idCliente)
    {
        await using var transaccion =
            await _context.Database.BeginTransactionAsync();

        var existe = await _context.Notificaciones.AnyAsync(n =>
            n.Id == idNotificacion &&
            n.IdCliente == idCliente);

        if (!existe)
            return false;

        await _context.Database.ExecuteSqlInterpolatedAsync(
            $"""
            DELETE FROM public.notificaciones_backup
            WHERE id_notificacion = {idNotificacion}
              AND id_usuario = {idCliente}
            """);

        await _context.Database.ExecuteSqlInterpolatedAsync(
            $"""
            DELETE FROM public.notificaciones
            WHERE id = {idNotificacion}
              AND id_cliente = {idCliente}
            """);

        await transaccion.CommitAsync();

        return true;
    }
}
