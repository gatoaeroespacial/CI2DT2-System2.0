using Ci2dt2.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ci2dt2.Infrastructure.Persistence.Configuraciones;

internal sealed class CronogramaConfig : IEntityTypeConfiguration<Cronograma>
{
    public void Configure(EntityTypeBuilder<Cronograma> b)
    {
        b.ToTable("Cronogramas");
        b.Property(x => x.Nombre).HasMaxLength(150).IsRequired();
        b.HasOne(x => x.Proyecto).WithMany().HasForeignKey(x => x.ProyectoId);
        // FormacionId y EventoId no tienen FK en el MVP: Formaciones (M08) y Eventos (M09) son de Release 2.
    }
}

internal sealed class TipoHitoConfig : IEntityTypeConfiguration<TipoHito>
{
    public void Configure(EntityTypeBuilder<TipoHito> b)
    {
        b.ToTable("TiposHito");
        b.Property(x => x.Nombre).HasMaxLength(60).IsRequired();
        b.HasData(DatosSemilla.TiposHito);
    }
}

internal sealed class HitoConfig : IEntityTypeConfiguration<Hito>
{
    public void Configure(EntityTypeBuilder<Hito> b)
    {
        b.ToTable("Hitos");
        b.Property(x => x.Titulo).HasMaxLength(200).IsRequired();
        b.Property(x => x.Estado).HasMaxLength(15).IsRequired();
        b.HasOne(x => x.Cronograma).WithMany().HasForeignKey(x => x.CronogramaId);
        b.HasOne(x => x.TipoHito).WithMany().HasForeignKey(x => x.TipoHitoId);
        b.HasOne(x => x.Responsable).WithMany().HasForeignKey(x => x.ResponsableId);
    }
}

internal sealed class AvanceConfig : IEntityTypeConfiguration<Avance>
{
    public void Configure(EntityTypeBuilder<Avance> b)
    {
        b.ToTable("Avances");
        b.Property(x => x.Titulo).HasMaxLength(200).IsRequired();
        b.Property(x => x.Descripcion);
        b.Property(x => x.PorcentajeAvance).HasPrecision(5, 2);
        b.Property(x => x.OrigenRegistro).HasMaxLength(10).IsRequired();
        b.Property(x => x.Latitud).HasPrecision(9, 6);
        b.Property(x => x.Longitud).HasPrecision(9, 6);
        b.Property(x => x.PrecisionGpsM).HasPrecision(7, 2);
        b.Property(x => x.EstadoValidacion).HasMaxLength(15);
        b.HasOne(x => x.Proyecto).WithMany().HasForeignKey(x => x.ProyectoId);
        b.HasOne(x => x.Hito).WithMany().HasForeignKey(x => x.HitoId);
        b.HasOne(x => x.Autor).WithMany().HasForeignKey(x => x.AutorId);
    }
}

internal sealed class AvanceAdjuntoConfig : IEntityTypeConfiguration<AvanceAdjunto>
{
    public void Configure(EntityTypeBuilder<AvanceAdjunto> b)
    {
        b.ToTable("AvancesAdjuntos");
        b.Property(x => x.UrlArchivo).HasMaxLength(400).IsRequired();
        b.Property(x => x.TipoMime).HasMaxLength(60);
        b.HasOne(x => x.Avance).WithMany().HasForeignKey(x => x.AvanceId);
    }
}

internal sealed class AvanceValidacionConfig : IEntityTypeConfiguration<AvanceValidacion>
{
    public void Configure(EntityTypeBuilder<AvanceValidacion> b)
    {
        b.ToTable("AvanceValidaciones");
        b.Property(x => x.RolValidador).HasMaxLength(15);
        b.Property(x => x.Periodo).HasMaxLength(7).IsFixedLength();
        b.Property(x => x.Resultado).HasMaxLength(15).IsRequired();
        b.Property(x => x.PuntajeRubrica).HasPrecision(5, 2);
        b.Property(x => x.Observaciones);
        b.HasOne(x => x.Avance).WithMany().HasForeignKey(x => x.AvanceId);
        b.HasOne(x => x.Validador).WithMany().HasForeignKey(x => x.ValidadorId);
    }
}
