using Ci2dt2.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ci2dt2.Infrastructure.Persistence.Configuraciones;

// M06 Gestión documental
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
        b.Property(x => x.EntidadTipo).HasMaxLength(40);
        b.Property(x => x.NombreArchivo).HasMaxLength(200).IsRequired();
        b.Property(x => x.UrlArchivo).HasMaxLength(400).IsRequired();
        b.Property(x => x.HashSha256).Char(64).IsRequired();
        b.HasIndex(x => new { x.EntidadTipo, x.EntidadId });
        b.HasOne(x => x.TipoDocumento).WithMany().HasForeignKey(x => x.TipoDocumentoId);
        b.HasOne(x => x.SubidoPor).WithMany().HasForeignKey(x => x.SubidoPorId);
    }
}

internal sealed class DocumentoFirmaConfig : IEntityTypeConfiguration<DocumentoFirma>
{
    public void Configure(EntityTypeBuilder<DocumentoFirma> b)
    {
        // Tabla inmutable: un trigger INSTEAD OF bloquea UPDATE y DELETE.
        b.ToTable("DocumentoFirmas", t => t.HasTrigger(Inmutables.Trigger("DocumentoFirmas")));
        b.Property(x => x.RolFirmante).HasMaxLength(40).IsRequired();
        b.Property(x => x.HashDocumento).Char(64).IsRequired();
        b.Property(x => x.IpFirma).HasMaxLength(45);
        b.Property(x => x.HashEncadenado).Char(64);
        b.HasOne(x => x.Documento).WithMany().HasForeignKey(x => x.DocumentoId);
        b.HasOne(x => x.Firmante).WithMany().HasForeignKey(x => x.FirmanteId);
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
        b.Property(x => x.Mensaje).IsRequired();
        b.Property(x => x.ReferenciaTipo).HasMaxLength(40);
        b.Property(x => x.UltimoError).HasMaxLength(300);
        b.HasIndex(x => new { x.UsuarioId, x.Leida });
        b.HasOne(x => x.Usuario).WithMany().HasForeignKey(x => x.UsuarioId);
    }
}

internal sealed class PreferenciaNotificacionConfig : IEntityTypeConfiguration<PreferenciaNotificacion>
{
    public void Configure(EntityTypeBuilder<PreferenciaNotificacion> b)
    {
        b.ToTable("PreferenciasNotificacion");
        b.Property(x => x.Tipo).HasMaxLength(40).IsRequired();
        b.HasOne(x => x.Usuario).WithMany().HasForeignKey(x => x.UsuarioId);
    }
}
