using System;
using System.Collections.Generic;
using AutosPAService.Entities;
using Microsoft.EntityFrameworkCore;

namespace AutosPAService.Data;

public partial class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<CatEstadosCitum> CatEstadosCita { get; set; }

    public virtual DbSet<CatEstadosDocumento> CatEstadosDocumentos { get; set; }

    public virtual DbSet<CatEstadosUsuario> CatEstadosUsuarios { get; set; }

    public virtual DbSet<CatMarca> CatMarcas { get; set; }

    public virtual DbSet<CatRole> CatRoles { get; set; }

    public virtual DbSet<CatTiposDocumento> CatTiposDocumentos { get; set; }

    public virtual DbSet<CatTiposNotificacion> CatTiposNotificacions { get; set; }

    public virtual DbSet<CatTiposServicio> CatTiposServicios { get; set; }

    public virtual DbSet<CatTiposVehiculo> CatTiposVehiculos { get; set; }

    public virtual DbSet<Cita> Citas { get; set; }

    public virtual DbSet<Conductore> Conductores { get; set; }

    public virtual DbSet<DocumentosVehiculo> DocumentosVehiculos { get; set; }

    public virtual DbSet<Notificacione> Notificaciones { get; set; }

    public virtual DbSet<NotificacionesBackup> NotificacionesBackups { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    public virtual DbSet<Vehiculo> Vehiculos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasPostgresEnum("auth", "aal_level", new[] { "aal1", "aal2", "aal3" })
            .HasPostgresEnum("auth", "code_challenge_method", new[] { "s256", "plain" })
            .HasPostgresEnum("auth", "factor_status", new[] { "unverified", "verified" })
            .HasPostgresEnum("auth", "factor_type", new[] { "totp", "webauthn", "phone", "recovery_code" })
            .HasPostgresEnum("auth", "oauth_authorization_status", new[] { "pending", "approved", "denied", "expired" })
            .HasPostgresEnum("auth", "oauth_client_type", new[] { "public", "confidential" })
            .HasPostgresEnum("auth", "oauth_registration_type", new[] { "dynamic", "manual" })
            .HasPostgresEnum("auth", "oauth_response_type", new[] { "code" })
            .HasPostgresEnum("auth", "one_time_token_type", new[] { "confirmation_token", "reauthentication_token", "recovery_token", "email_change_token_new", "email_change_token_current", "phone_change_token" })
            .HasPostgresEnum("realtime", "action", new[] { "INSERT", "UPDATE", "DELETE", "TRUNCATE", "ERROR" })
            .HasPostgresEnum("realtime", "equality_op", new[] { "eq", "neq", "lt", "lte", "gt", "gte", "in", "like", "ilike", "is", "match", "imatch", "isdistinct" })
            .HasPostgresEnum("storage", "buckettype", new[] { "STANDARD", "ANALYTICS", "VECTOR" })
            .HasPostgresExtension("extensions", "pg_stat_statements")
            .HasPostgresExtension("extensions", "pgcrypto")
            .HasPostgresExtension("extensions", "uuid-ossp")
            .HasPostgresExtension("vault", "supabase_vault");

        modelBuilder.Entity<CatEstadosCitum>(entity =>
        {
            entity.HasKey(e => e.IdEstadoCita).HasName("cat_estados_cita_pkey");

            entity.ToTable("cat_estados_cita");

            entity.HasIndex(e => e.Nombre, "cat_estados_cita_nombre_key").IsUnique();

            entity.Property(e => e.IdEstadoCita).HasColumnName("id_estado_cita");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(150)
                .HasColumnName("descripcion");
            entity.Property(e => e.Nombre)
                .HasMaxLength(40)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<CatEstadosDocumento>(entity =>
        {
            entity.HasKey(e => e.IdEstadoDocumento).HasName("cat_estados_documento_pkey");

            entity.ToTable("cat_estados_documento");

            entity.HasIndex(e => e.Nombre, "cat_estados_documento_nombre_key").IsUnique();

            entity.Property(e => e.IdEstadoDocumento).HasColumnName("id_estado_documento");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.Nombre)
                .HasMaxLength(30)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<CatEstadosUsuario>(entity =>
        {
            entity.HasKey(e => e.IdEstadoUsuario).HasName("cat_estados_usuario_pkey");

            entity.ToTable("cat_estados_usuario");

            entity.HasIndex(e => e.Nombre, "cat_estados_usuario_nombre_key").IsUnique();

            entity.Property(e => e.IdEstadoUsuario).HasColumnName("id_estado_usuario");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(100)
                .HasColumnName("descripcion");
            entity.Property(e => e.Nombre)
                .HasMaxLength(30)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<CatMarca>(entity =>
        {
            entity.HasKey(e => e.IdMarca).HasName("cat_marcas_pkey");

            entity.ToTable("cat_marcas");

            entity.HasIndex(e => e.Nombre, "cat_marcas_nombre_key").IsUnique();

            entity.Property(e => e.IdMarca).HasColumnName("id_marca");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.Nombre)
                .HasMaxLength(80)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<CatRole>(entity =>
        {
            entity.HasKey(e => e.IdRol).HasName("cat_roles_pkey");

            entity.ToTable("cat_roles");

            entity.HasIndex(e => e.Nombre, "cat_roles_nombre_key").IsUnique();

            entity.Property(e => e.IdRol).HasColumnName("id_rol");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(150)
                .HasColumnName("descripcion");
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<CatTiposDocumento>(entity =>
        {
            entity.HasKey(e => e.IdTipoDocumento).HasName("cat_tipos_documento_pkey");

            entity.ToTable("cat_tipos_documento");

            entity.HasIndex(e => e.Nombre, "cat_tipos_documento_nombre_key").IsUnique();

            entity.Property(e => e.IdTipoDocumento).HasColumnName("id_tipo_documento");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(150)
                .HasColumnName("descripcion");
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<CatTiposNotificacion>(entity =>
        {
            entity.HasKey(e => e.IdTipoNotificacion).HasName("cat_tipos_notificacion_pkey");

            entity.ToTable("cat_tipos_notificacion");

            entity.HasIndex(e => e.Nombre, "cat_tipos_notificacion_nombre_key").IsUnique();

            entity.Property(e => e.IdTipoNotificacion).HasColumnName("id_tipo_notificacion");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(150)
                .HasColumnName("descripcion");
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<CatTiposServicio>(entity =>
        {
            entity.HasKey(e => e.IdTipoServicio).HasName("cat_tipos_servicio_pkey");

            entity.ToTable("cat_tipos_servicio");

            entity.HasIndex(e => e.Nombre, "cat_tipos_servicio_nombre_key").IsUnique();

            entity.Property(e => e.IdTipoServicio).HasColumnName("id_tipo_servicio");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.CostoBase)
                .HasPrecision(10, 2)
                .HasColumnName("costo_base");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(250)
                .HasColumnName("descripcion");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .HasColumnName("nombre");
            entity.Property(e => e.TiempoEstimadoMinutos).HasColumnName("tiempo_estimado_minutos");
        });

        modelBuilder.Entity<CatTiposVehiculo>(entity =>
        {
            entity.HasKey(e => e.IdTipoVehiculo).HasName("cat_tipos_vehiculo_pkey");

            entity.ToTable("cat_tipos_vehiculo");

            entity.HasIndex(e => e.Nombre, "cat_tipos_vehiculo_nombre_key").IsUnique();

            entity.Property(e => e.IdTipoVehiculo).HasColumnName("id_tipo_vehiculo");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(100)
                .HasColumnName("descripcion");
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<Cita>(entity =>
        {
            entity.HasKey(e => e.IdCita).HasName("citas_pkey");

            entity.ToTable("citas");

            entity.HasIndex(e => e.IdAgente, "idx_citas_agente");

            entity.HasIndex(e => e.IdCliente, "idx_citas_cliente");

            entity.HasIndex(e => e.IdEstadoCita, "idx_citas_estado");

            entity.HasIndex(e => e.FechaCita, "idx_citas_fecha");

            entity.HasIndex(e => e.IdTipoServicio, "idx_citas_tipo_servicio");

            entity.HasIndex(e => e.IdVehiculo, "idx_citas_vehiculo");

            entity.Property(e => e.IdCita).HasColumnName("id_cita");
            entity.Property(e => e.Costo)
                .HasPrecision(10, 2)
                .HasColumnName("costo");
            entity.Property(e => e.FechaActualizacion)
                .HasDefaultValueSql("now()")
                .HasColumnName("fecha_actualizacion");
            entity.Property(e => e.FechaCita).HasColumnName("fecha_cita");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("now()")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.HoraInicio).HasColumnName("hora_inicio");
            entity.Property(e => e.IdAgente).HasColumnName("id_agente");
            entity.Property(e => e.IdCliente).HasColumnName("id_cliente");
            entity.Property(e => e.IdEstadoCita).HasColumnName("id_estado_cita");
            entity.Property(e => e.IdTipoServicio).HasColumnName("id_tipo_servicio");
            entity.Property(e => e.IdVehiculo).HasColumnName("id_vehiculo");
            entity.Property(e => e.Observaciones).HasColumnName("observaciones");
            entity.Property(e => e.TiempoEstimadoMinutos).HasColumnName("tiempo_estimado_minutos");

            entity.HasOne(d => d.IdAgenteNavigation).WithMany(p => p.CitaIdAgenteNavigations)
                .HasForeignKey(d => d.IdAgente)
                .HasConstraintName("fk_cita_agente");

            entity.HasOne(d => d.IdClienteNavigation).WithMany(p => p.CitaIdClienteNavigations)
                .HasForeignKey(d => d.IdCliente)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_cita_cliente");

            entity.HasOne(d => d.IdEstadoCitaNavigation).WithMany(p => p.Cita)
                .HasForeignKey(d => d.IdEstadoCita)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_cita_estado");

            entity.HasOne(d => d.IdTipoServicioNavigation).WithMany(p => p.Cita)
                .HasForeignKey(d => d.IdTipoServicio)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_cita_tipo_servicio");

            entity.HasOne(d => d.IdVehiculoNavigation).WithMany(p => p.Cita)
                .HasForeignKey(d => d.IdVehiculo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_cita_vehiculo");
        });

        modelBuilder.Entity<Conductore>(entity =>
        {
            entity.HasKey(e => e.IdConductor).HasName("conductores_pkey");

            entity.ToTable("conductores");

            entity.HasIndex(e => e.IdUsuario, "conductores_id_usuario_key").IsUnique();

            entity.HasIndex(e => e.NumeroLicencia, "conductores_numero_licencia_key").IsUnique();

            entity.HasIndex(e => e.IdUsuario, "idx_conductores_usuario");

            entity.Property(e => e.IdConductor).HasColumnName("id_conductor");
            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("now()")
                .HasColumnName("fecha_registro");
            entity.Property(e => e.FechaVencimientoLicencia).HasColumnName("fecha_vencimiento_licencia");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.NumeroLicencia)
                .HasMaxLength(50)
                .HasColumnName("numero_licencia");

            entity.HasOne(d => d.IdUsuarioNavigation).WithOne(p => p.Conductore)
                .HasForeignKey<Conductore>(d => d.IdUsuario)
                .HasConstraintName("fk_conductor_usuario");
        });

        modelBuilder.Entity<DocumentosVehiculo>(entity =>
        {
            entity.HasKey(e => e.IdDocumento).HasName("documentos_vehiculo_pkey");

            entity.ToTable("documentos_vehiculo");

            entity.HasIndex(e => e.IdEstadoDocumento, "idx_documentos_estado");

            entity.HasIndex(e => e.IdTipoDocumento, "idx_documentos_tipo");

            entity.HasIndex(e => e.IdVehiculo, "idx_documentos_vehiculo");

            entity.Property(e => e.IdDocumento).HasColumnName("id_documento");
            entity.Property(e => e.FechaEmision).HasColumnName("fecha_emision");
            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("now()")
                .HasColumnName("fecha_registro");
            entity.Property(e => e.FechaVencimiento).HasColumnName("fecha_vencimiento");
            entity.Property(e => e.IdEstadoDocumento).HasColumnName("id_estado_documento");
            entity.Property(e => e.IdTipoDocumento).HasColumnName("id_tipo_documento");
            entity.Property(e => e.IdVehiculo).HasColumnName("id_vehiculo");
            entity.Property(e => e.NombreArchivo)
                .HasMaxLength(255)
                .HasColumnName("nombre_archivo");
            entity.Property(e => e.NumeroDocumento)
                .HasMaxLength(100)
                .HasColumnName("numero_documento");
            entity.Property(e => e.UrlArchivo).HasColumnName("url_archivo");

            entity.HasOne(d => d.IdEstadoDocumentoNavigation).WithMany(p => p.DocumentosVehiculos)
                .HasForeignKey(d => d.IdEstadoDocumento)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_documento_estado");

            entity.HasOne(d => d.IdTipoDocumentoNavigation).WithMany(p => p.DocumentosVehiculos)
                .HasForeignKey(d => d.IdTipoDocumento)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_documento_tipo");

            entity.HasOne(d => d.IdVehiculoNavigation).WithMany(p => p.DocumentosVehiculos)
                .HasForeignKey(d => d.IdVehiculo)
                .HasConstraintName("fk_documento_vehiculo");
        });

        modelBuilder.Entity<Notificacione>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("notificaciones_pkey");

            entity.ToTable("notificaciones");

            entity.HasIndex(e => e.IdCita, "idx_notificaciones_cita");

            entity.HasIndex(e => e.IdCliente, "idx_notificaciones_usuario");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.Estado).HasColumnName("estado");
            entity.Property(e => e.FechaEnvio).HasColumnName("fecha_envio");
            entity.Property(e => e.IdCita).HasColumnName("id_cita");
            entity.Property(e => e.IdCliente).HasColumnName("id_cliente");
            entity.Property(e => e.Mensaje).HasColumnName("mensaje");
            entity.Property(e => e.Via).HasColumnName("via");

            entity.HasOne(d => d.IdCitaNavigation).WithMany(p => p.Notificaciones)
                .HasForeignKey(d => d.IdCita)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_notificacion_cita");

            entity.HasOne(d => d.IdClienteNavigation).WithMany(p => p.Notificaciones)
                .HasForeignKey(d => d.IdCliente)
                .HasConstraintName("fk_notificacion_usuario");
        });

        modelBuilder.Entity<NotificacionesBackup>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("notificaciones_backup");

            entity.Property(e => e.FechaCreacion).HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaLectura).HasColumnName("fecha_lectura");
            entity.Property(e => e.IdCita).HasColumnName("id_cita");
            entity.Property(e => e.IdNotificacion).HasColumnName("id_notificacion");
            entity.Property(e => e.IdTipoNotificacion).HasColumnName("id_tipo_notificacion");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.Leida).HasColumnName("leida");
            entity.Property(e => e.Mensaje).HasColumnName("mensaje");
            entity.Property(e => e.Titulo)
                .HasMaxLength(150)
                .HasColumnName("titulo");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.IdUsuario).HasName("usuarios_pkey");

            entity.ToTable("usuarios");

            entity.HasIndex(e => e.IdEstadoUsuario, "idx_usuarios_estado");

            entity.HasIndex(e => e.IdRol, "idx_usuarios_rol");

            entity.HasIndex(e => e.AuthUserId, "usuarios_auth_user_id_key").IsUnique();

            entity.HasIndex(e => e.Email, "usuarios_email_key").IsUnique();

            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.Apellidos)
                .HasMaxLength(100)
                .HasColumnName("apellidos");
            entity.Property(e => e.AuthUserId).HasColumnName("auth_user_id");
            entity.Property(e => e.Email)
                .HasMaxLength(150)
                .HasColumnName("email");
            entity.Property(e => e.FechaActualizacion)
                .HasDefaultValueSql("now()")
                .HasColumnName("fecha_actualizacion");
            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("now()")
                .HasColumnName("fecha_registro");
            entity.Property(e => e.IdEstadoUsuario).HasColumnName("id_estado_usuario");
            entity.Property(e => e.IdRol).HasColumnName("id_rol");
            entity.Property(e => e.Nombres)
                .HasMaxLength(100)
                .HasColumnName("nombres");
            entity.Property(e => e.PasswordHash).HasColumnName("password_hash");
            entity.Property(e => e.Telefono)
                .HasMaxLength(20)
                .HasColumnName("telefono");

            entity.HasOne(d => d.IdEstadoUsuarioNavigation).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.IdEstadoUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_usuario_estado");

            entity.HasOne(d => d.IdRolNavigation).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.IdRol)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_usuario_rol");
        });

        modelBuilder.Entity<Vehiculo>(entity =>
        {
            entity.HasKey(e => e.IdVehiculo).HasName("vehiculos_pkey");

            entity.ToTable("vehiculos");

            entity.HasIndex(e => e.IdConductor, "idx_vehiculos_conductor");

            entity.HasIndex(e => e.IdMarca, "idx_vehiculos_marca");

            entity.HasIndex(e => e.IdTipoVehiculo, "idx_vehiculos_tipo");

            entity.HasIndex(e => e.Placa, "vehiculos_placa_key").IsUnique();

            entity.Property(e => e.IdVehiculo).HasColumnName("id_vehiculo");
            entity.Property(e => e.Anio).HasColumnName("anio");
            entity.Property(e => e.Color)
                .HasMaxLength(50)
                .HasColumnName("color");
            entity.Property(e => e.FechaActualizacion)
                .HasDefaultValueSql("now()")
                .HasColumnName("fecha_actualizacion");
            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("now()")
                .HasColumnName("fecha_registro");
            entity.Property(e => e.IdConductor).HasColumnName("id_conductor");
            entity.Property(e => e.IdMarca).HasColumnName("id_marca");
            entity.Property(e => e.IdTipoVehiculo).HasColumnName("id_tipo_vehiculo");
            entity.Property(e => e.Kilometraje)
                .HasDefaultValue(0)
                .HasColumnName("kilometraje");
            entity.Property(e => e.Modelo)
                .HasMaxLength(100)
                .HasColumnName("modelo");
            entity.Property(e => e.Placa)
                .HasMaxLength(20)
                .HasColumnName("placa");

            entity.HasOne(d => d.IdConductorNavigation).WithMany(p => p.Vehiculos)
                .HasForeignKey(d => d.IdConductor)
                .HasConstraintName("fk_vehiculo_conductor");

            entity.HasOne(d => d.IdMarcaNavigation).WithMany(p => p.Vehiculos)
                .HasForeignKey(d => d.IdMarca)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_vehiculo_marca");

            entity.HasOne(d => d.IdTipoVehiculoNavigation).WithMany(p => p.Vehiculos)
                .HasForeignKey(d => d.IdTipoVehiculo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_vehiculo_tipo");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
