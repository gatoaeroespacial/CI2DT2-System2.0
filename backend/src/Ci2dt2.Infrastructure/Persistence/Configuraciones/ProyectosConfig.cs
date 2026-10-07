using Ci2dt2.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ci2dt2.Infrastructure.Persistence.Configuraciones;

internal sealed class EstadoProyectoConfig : IEntityTypeConfiguration<EstadoProyecto>
{
    public void Configure(EntityTypeBuilder<EstadoProyecto> b)
    {
        b.ToTable("EstadosProyecto");
        b.Property(x => x.Nombre).HasMaxLength(40).IsRequired();
        b.HasData(DatosSemilla.EstadosProyecto);
    }
}

internal sealed class TipoProyectoConfig : IEntityTypeConfiguration<TipoProyecto>
{
    public void Configure(EntityTypeBuilder<TipoProyecto> b)
    {
        b.ToTable("TiposProyecto");
        b.Property(x => x.Nombre).HasMaxLength(60).IsRequired();
        b.HasData(DatosSemilla.TiposProyecto);
    }
}

internal sealed class TipoConvocatoriaConfig : IEntityTypeConfiguration<TipoConvocatoria>
{
    public void Configure(EntityTypeBuilder<TipoConvocatoria> b)
    {
        b.ToTable("TiposConvocatoria");
        b.Property(x => x.Nombre).HasMaxLength(60).IsRequired();
        b.HasData(DatosSemilla.TiposConvocatoria);
    }
}

internal sealed class ProyectoConfig : IEntityTypeConfiguration<Proyecto>
{
    public void Configure(EntityTypeBuilder<Proyecto> b)
    {
        b.ToTable("Proyectos");
        b.Property(x => x.Codigo).HasMaxLength(30).IsRequired();
        b.Property(x => x.Titulo).HasMaxLength(300).IsRequired();
        b.Property(x => x.Resumen);
        b.Property(x => x.PorcentajeAvance).HasPrecision(5, 2);
        b.Property(x => x.Moneda).HasMaxLength(3).IsFixedLength().IsRequired();
        b.Property(x => x.PresupuestoTotal).HasPrecision(18, 2);
        b.Property(x => x.TasaCambio).HasPrecision(18, 4);
        b.Property(x => x.PresupuestoTotalCop).HasPrecision(18, 2);
        b.HasIndex(x => x.Codigo).IsUnique();
        b.HasOne(x => x.Programa).WithMany().HasForeignKey(x => x.ProgramaId);
        b.HasOne(x => x.TipoProyecto).WithMany().HasForeignKey(x => x.TipoProyectoId);
        b.HasOne(x => x.Estado).WithMany().HasForeignKey(x => x.EstadoId);
        b.HasOne(x => x.InvestigadorPrincipal).WithMany().HasForeignKey(x => x.InvestigadorPrincipalId);
    }
}

internal sealed class ProyectoIntegranteConfig : IEntityTypeConfiguration<ProyectoIntegrante>
{
    public void Configure(EntityTypeBuilder<ProyectoIntegrante> b)
    {
        b.ToTable("ProyectoIntegrantes");
        b.Property(x => x.RolEnProyecto).HasMaxLength(30).IsRequired();
        b.HasOne(x => x.Proyecto).WithMany().HasForeignKey(x => x.ProyectoId);
        b.HasOne(x => x.Usuario).WithMany().HasForeignKey(x => x.UsuarioId);
        b.HasIndex(x => new { x.UsuarioId, x.ProyectoId });
    }
}

internal sealed class ProyectoRevisionConfig : IEntityTypeConfiguration<ProyectoRevision>
{
    public void Configure(EntityTypeBuilder<ProyectoRevision> b)
    {
        b.ToTable("ProyectoRevisiones");
        b.Property(x => x.Etapa).HasMaxLength(30).IsRequired();
        b.Property(x => x.Decision).HasMaxLength(20).IsRequired();
        b.Property(x => x.Observaciones);
        b.HasOne(x => x.Proyecto).WithMany().HasForeignKey(x => x.ProyectoId);
        b.HasOne(x => x.Revisor).WithMany().HasForeignKey(x => x.RevisorId);
        b.HasOne(x => x.EstadoAnterior).WithMany().HasForeignKey(x => x.EstadoAnteriorId);
        b.HasOne(x => x.EstadoNuevo).WithMany().HasForeignKey(x => x.EstadoNuevoId);
    }
}
