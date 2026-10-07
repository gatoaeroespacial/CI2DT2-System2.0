using Ci2dt2.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ci2dt2.Infrastructure.Persistence.Configuraciones;

// M06 Documentos
internal sealed class TipoDocumentoConfig : IEntityTypeConfiguration<TipoDocumento>
{
    public void Configure(EntityTypeBuilder<TipoDocumento> b)
    {
        b.ToTable("TiposDocumento");
        b.Property(x => x.Nombre).HasMaxLength(80).IsRequired();
        b.HasData(DatosSemilla.TiposDocumento);
    }
}

internal sealed class DocumentoConfig : IEntityTypeConfiguration<Documento>
{
    public void Configure(EntityTypeBuilder<Documento> b)
    {
        b.ToTable("Documentos");
        b.Property(x => x.EntidadTipo).HasMaxLength(40).IsRequired();
        b.Property(x => x.NombreArchivo).HasMaxLength(200).IsRequired();
        b.Property(x => x.UrlArchivo).HasMaxLength(400).IsRequired();
        b.Property(x => x.HashSha256).HasMaxLength(64).IsFixedLength().IsRequired();
        b.HasIndex(x => new { x.EntidadTipo, x.EntidadId });
        b.HasOne(x => x.TipoDocumento).WithMany().HasForeignKey(x => x.TipoDocumentoId);
        b.HasOne(x => x.SubidoPor).WithMany().HasForeignKey(x => x.SubidoPorId);
    }
}

// M07 Notificaciones
internal sealed class NotificacionConfig : IEntityTypeConfiguration<Notificacion>
{
    public void Configure(EntityTypeBuilder<Notificacion> b)
    {
        b.ToTable("Notificaciones");
        b.Property(x => x.Tipo).HasMaxLength(40).IsRequired();
        b.Property(x => x.Titulo).HasMaxLength(200).IsRequired();
        b.Property(x => x.Mensaje);
        b.Property(x => x.ReferenciaTipo).HasMaxLength(40);
        b.Property(x => x.UltimoError).HasMaxLength(300);
        b.HasOne(x => x.Usuario).WithMany().HasForeignKey(x => x.UsuarioId);
    }
}

// M11 Entorno Empresarial
internal sealed class TipoAliadoConfig : IEntityTypeConfiguration<TipoAliado>
{
    public void Configure(EntityTypeBuilder<TipoAliado> b)
    {
        b.ToTable("TiposAliado");
        b.Property(x => x.Nombre).HasMaxLength(60).IsRequired();
        b.HasData(DatosSemilla.TiposAliado);
    }
}

internal sealed class AliadoConfig : IEntityTypeConfiguration<Aliado>
{
    public void Configure(EntityTypeBuilder<Aliado> b)
    {
        b.ToTable("Aliados");
        b.Property(x => x.Nombre).HasMaxLength(200).IsRequired();
        b.Property(x => x.Nit).HasMaxLength(30);
        b.Property(x => x.Pais).HasMaxLength(60);
        b.Property(x => x.Ciudad).HasMaxLength(80);
        b.Property(x => x.Sector).HasMaxLength(80);
        b.Property(x => x.RepresentanteLegal).HasMaxLength(150);
        b.Property(x => x.Email).HasMaxLength(150);
        b.HasOne(x => x.TipoAliado).WithMany().HasForeignKey(x => x.TipoAliadoId);
    }
}

internal sealed class ProgramaAliadoConfig : IEntityTypeConfiguration<ProgramaAliado>
{
    public void Configure(EntityTypeBuilder<ProgramaAliado> b)
    {
        b.ToTable("ProgramaAliados");
        b.HasKey(x => new { x.ProgramaId, x.AliadoId });
        b.HasOne(x => x.Programa).WithMany().HasForeignKey(x => x.ProgramaId);
        b.HasOne(x => x.Aliado).WithMany().HasForeignKey(x => x.AliadoId);
    }
}

// M14 Datos personales
internal sealed class PoliticaTratamientoDatosConfig : IEntityTypeConfiguration<PoliticaTratamientoDatos>
{
    public void Configure(EntityTypeBuilder<PoliticaTratamientoDatos> b)
    {
        b.ToTable("PoliticasTratamientoDatos");
        b.Property(x => x.Version).HasMaxLength(10).IsRequired();
        b.Property(x => x.TextoUrl).HasMaxLength(400);
        b.Property(x => x.HashTexto).HasMaxLength(64).IsFixedLength();
        b.HasData(DatosSemilla.PoliticasTratamientoDatos);
    }
}

internal sealed class ConsentimientoDatosConfig : IEntityTypeConfiguration<ConsentimientoDatos>
{
    public void Configure(EntityTypeBuilder<ConsentimientoDatos> b)
    {
        b.ToTable("ConsentimientosDatos");
        b.Property(x => x.Correo).HasMaxLength(150);
        b.Property(x => x.Contexto).HasMaxLength(20);
        b.Property(x => x.IpOrigen).HasMaxLength(45);
        b.Property(x => x.HashEvidencia).HasMaxLength(64).IsFixedLength();
        b.HasOne(x => x.Politica).WithMany().HasForeignKey(x => x.PoliticaId);
        b.HasOne(x => x.Usuario).WithMany().HasForeignKey(x => x.UsuarioId);
    }
}

// M15 Auditoría
internal sealed class LogActividadConfig : IEntityTypeConfiguration<LogActividad>
{
    public void Configure(EntityTypeBuilder<LogActividad> b)
    {
        b.ToTable("LogActividades");
        b.Property(x => x.IpOrigen).HasMaxLength(45);
        b.Property(x => x.Recurso).HasMaxLength(60).IsRequired();
        b.Property(x => x.RecursoId).HasMaxLength(40);
        b.Property(x => x.Accion).HasMaxLength(10).IsRequired();
        b.Property(x => x.Ruta).HasMaxLength(300);
        b.Property(x => x.ValoresAnteriores);
        b.Property(x => x.ValoresNuevos);
        b.Property(x => x.HashEncadenado).HasMaxLength(64).IsFixedLength();
        b.HasIndex(x => x.FechaUtc);
        b.HasOne(x => x.Usuario).WithMany().HasForeignKey(x => x.UsuarioId);
    }
}
