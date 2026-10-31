using Ci2dt2.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ci2dt2.Infrastructure.Persistence.Configuraciones;

internal sealed class BorradorProyectoConfig : IEntityTypeConfiguration<BorradorProyecto>
{
    public void Configure(EntityTypeBuilder<BorradorProyecto> b)
    {
        b.ToTable("BorradoresProyecto");
        b.Property(x => x.TituloTentativo).HasMaxLength(300).IsRequired();
        b.Property(x => x.LineaInvestigacion).HasMaxLength(200);
        b.Property(x => x.ConvocatoriaNombre).HasMaxLength(250);
        b.Property(x => x.EstadoAvance).HasMaxLength(30).IsRequired();

        b.HasOne(x => x.Usuario).WithMany().HasForeignKey(x => x.UsuarioId);
        b.HasOne(x => x.Programa).WithMany().HasForeignKey(x => x.ProgramaId);
        b.HasOne(x => x.DocumentoTdr).WithMany().HasForeignKey(x => x.DocumentoTdrId);

        b.HasMany(x => x.Secciones).WithOne(x => x.BorradorProyecto).HasForeignKey(x => x.BorradorProyectoId);
        b.HasMany(x => x.Mensajes).WithOne(x => x.BorradorProyecto).HasForeignKey(x => x.BorradorProyectoId);
    }
}

internal sealed class BorradorProyectoSeccionConfig : IEntityTypeConfiguration<BorradorProyectoSeccion>
{
    public void Configure(EntityTypeBuilder<BorradorProyectoSeccion> b)
    {
        b.ToTable("BorradorProyectoSecciones");
        b.Property(x => x.CodigoSeccion).HasMaxLength(50).IsRequired();
        b.Property(x => x.TituloSeccion).HasMaxLength(150).IsRequired();
        b.Property(x => x.Estado).HasMaxLength(30).IsRequired();

        b.HasIndex(x => new { x.BorradorProyectoId, x.CodigoSeccion }).IsUnique();
    }
}

internal sealed class BorradorProyectoMensajeConfig : IEntityTypeConfiguration<BorradorProyectoMensaje>
{
    public void Configure(EntityTypeBuilder<BorradorProyectoMensaje> b)
    {
        b.ToTable("BorradorProyectoMensajes");
        b.Property(x => x.Rol).HasMaxLength(20).IsRequired();
        b.Property(x => x.TipoMensaje).HasMaxLength(30).IsRequired();
        b.Property(x => x.SeccionDestino).HasMaxLength(50);
        b.Property(x => x.FechaUtc).HasDefaultValueSql("SYSUTCDATETIME()");
    }
}

