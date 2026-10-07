using Ci2dt2.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ci2dt2.Infrastructure.Persistence.Configuraciones;

internal sealed class ConvocatoriaConfig : IEntityTypeConfiguration<Convocatoria>
{
    public void Configure(EntityTypeBuilder<Convocatoria> b)
    {
        b.ToTable("Convocatorias");
        b.Property(x => x.Nombre).HasMaxLength(250).IsRequired();
        b.Property(x => x.Numero).HasMaxLength(30);
        b.Property(x => x.Plataforma).HasMaxLength(50);
        b.Property(x => x.Url).HasMaxLength(400);
        b.Property(x => x.Moneda).HasMaxLength(3).IsFixedLength();
        b.Property(x => x.MontoMaximo).HasPrecision(18, 2);
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
        b.Property(x => x.MontoSolicitado).HasPrecision(18, 2);
        b.Property(x => x.Moneda).HasMaxLength(3).IsFixedLength();
        b.Property(x => x.TasaCambioEstimada).HasPrecision(18, 4);
        b.Property(x => x.MontoSolicitadoCop).HasPrecision(18, 2);
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
        b.Property(x => x.TipoFuente).HasMaxLength(30).IsRequired();
        b.Property(x => x.TipoAporte).HasMaxLength(10).IsRequired();
        b.Property(x => x.Moneda).HasMaxLength(3).IsFixedLength().IsRequired();
        b.Property(x => x.Monto).HasPrecision(18, 2);
        b.Property(x => x.TasaCambio).HasPrecision(18, 4);
        b.Property(x => x.MontoCop).HasPrecision(18, 2);
        b.Property(x => x.Estado).HasMaxLength(20);
        b.HasOne(x => x.Proyecto).WithMany().HasForeignKey(x => x.ProyectoId);
        b.HasOne(x => x.Aliado).WithMany().HasForeignKey(x => x.AliadoId);
    }
}
