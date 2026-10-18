using Ci2dt2.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ci2dt2.Infrastructure.Persistence.Configuraciones;

// M04 Convocatorias y financiamiento
internal sealed class ConvocatoriaConfig : IEntityTypeConfiguration<Convocatoria>
{
    public void Configure(EntityTypeBuilder<Convocatoria> b)
    {
        b.ToTable("Convocatorias");
        b.Property(x => x.Nombre).HasMaxLength(250).IsRequired();
        b.Property(x => x.Numero).HasMaxLength(30);
        b.Property(x => x.Plataforma).HasMaxLength(50).IsRequired();
        b.Property(x => x.Url).HasMaxLength(400);
        b.Property(x => x.Moneda).Char(3);
        b.Property(x => x.MontoMaximo).Dinero();
        b.HasOne(x => x.TipoConvocatoria).WithMany().HasForeignKey(x => x.TipoConvocatoriaId);
        b.HasOne(x => x.EntidadConvocante).WithMany().HasForeignKey(x => x.EntidadConvocanteId);
    }
}

internal sealed class PropuestaConvocatoriaConfig : IEntityTypeConfiguration<PropuestaConvocatoria>
{
    public void Configure(EntityTypeBuilder<PropuestaConvocatoria> b)
    {
        b.ToTable("PropuestasConvocatoria");
        b.Property(x => x.IdPostulacionExterna).HasMaxLength(60);
        b.Property(x => x.CodigoMga).HasMaxLength(30);
        b.Property(x => x.MontoSolicitado).Dinero();
        b.Property(x => x.Moneda).Char(3);
        b.Property(x => x.TasaCambioEstimada).Tasa();
        b.Property(x => x.MontoSolicitadoCop).Dinero();
        b.Property(x => x.Resultado).HasMaxLength(20);
        b.Property(x => x.Puntaje).HasPrecision(6, 2);
        b.Property(x => x.FaseExterna).HasMaxLength(40);
        b.HasOne(x => x.Proyecto).WithMany().HasForeignKey(x => x.ProyectoId);
        b.HasOne(x => x.Convocatoria).WithMany().HasForeignKey(x => x.ConvocatoriaId);
        b.HasOne(x => x.ComprobanteDoc).WithMany().HasForeignKey(x => x.ComprobanteDocId);
    }
}

internal sealed class FuenteFinanciamientoConfig : IEntityTypeConfiguration<FuenteFinanciamiento>
{
    public void Configure(EntityTypeBuilder<FuenteFinanciamiento> b)
    {
        b.ToTable("FuentesFinanciamiento");
        b.Property(x => x.TipoFuente).HasMaxLength(30);
        b.Property(x => x.TipoAporte).HasMaxLength(10);
        b.Property(x => x.Moneda).Char(3);
        b.Property(x => x.Monto).Dinero();
        b.Property(x => x.TasaCambio).Tasa();
        b.Property(x => x.MontoCop).Dinero();
        b.Property(x => x.Estado).HasMaxLength(20).IsRequired();
        b.HasOne(x => x.Proyecto).WithMany().HasForeignKey(x => x.ProyectoId);
        b.HasOne(x => x.Aliado).WithMany().HasForeignKey(x => x.AliadoId);
    }
}

// M05 Cronogramas y seguimiento
internal sealed class CronogramaConfig : IEntityTypeConfiguration<Cronograma>
{
    public void Configure(EntityTypeBuilder<Cronograma> b)
    {
        b.ToTable("Cronogramas", t => t.HasCheckConstraint("CK_Cronogramas_UnaEntidad",
            "IIF([ProyectoId] IS NULL, 0, 1) + IIF([FormacionId] IS NULL, 0, 1) + IIF([EventoId] IS NULL, 0, 1) = 1"));
        b.Property(x => x.Nombre).HasMaxLength(150).IsRequired();
        b.HasOne(x => x.Proyecto).WithMany().HasForeignKey(x => x.ProyectoId);
        b.HasOne(x => x.Formacion).WithMany().HasForeignKey(x => x.FormacionId);
        b.HasOne(x => x.Evento).WithMany().HasForeignKey(x => x.EventoId);
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
        b.HasIndex(x => new { x.FechaLimite, x.Estado });
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
        b.Property(x => x.PorcentajeAvance).Porcentaje();
        b.Property(x => x.OrigenRegistro).HasMaxLength(10);
        b.Property(x => x.Latitud).HasPrecision(9, 6);
        b.Property(x => x.Longitud).HasPrecision(9, 6);
        b.Property(x => x.PrecisionGpsM).HasPrecision(7, 2);
        b.Property(x => x.EstadoValidacion).HasMaxLength(15);
        b.HasIndex(x => x.IdCliente).IsUnique().HasFilter("[IdCliente] IS NOT NULL");
        b.HasIndex(x => new { x.ProyectoId, x.FechaCaptura });
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
        b.Property(x => x.RolValidador).HasMaxLength(15).IsRequired();
        b.Property(x => x.Periodo).Char(7);
        b.Property(x => x.Resultado).HasMaxLength(15);
        b.Property(x => x.PuntajeRubrica).HasPrecision(5, 2);
        b.HasOne(x => x.Avance).WithMany().HasForeignKey(x => x.AvanceId);
        b.HasOne(x => x.Validador).WithMany().HasForeignKey(x => x.ValidadorId);
    }
}
