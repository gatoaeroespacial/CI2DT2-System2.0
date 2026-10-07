using Ci2dt2.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ci2dt2.Infrastructure.Persistence.Configuraciones;

internal sealed class ProgramaConfig : IEntityTypeConfiguration<Programa>
{
    public void Configure(EntityTypeBuilder<Programa> b)
    {
        b.ToTable("Programas");
        b.Property(x => x.Nombre).HasMaxLength(150).IsRequired();
        b.Property(x => x.Descripcion);
        b.HasData(DatosSemilla.Programas);
    }
}

internal sealed class ObjetivoEspecificoConfig : IEntityTypeConfiguration<ObjetivoEspecifico>
{
    public void Configure(EntityTypeBuilder<ObjetivoEspecifico> b)
    {
        b.ToTable("ObjetivosEspecificos");
        b.Property(x => x.Descripcion).IsRequired();
        b.HasOne(x => x.Programa).WithMany().HasForeignKey(x => x.ProgramaId);
    }
}

internal sealed class ProgramaIntegranteConfig : IEntityTypeConfiguration<ProgramaIntegrante>
{
    public void Configure(EntityTypeBuilder<ProgramaIntegrante> b)
    {
        b.ToTable("ProgramaIntegrantes");
        b.Property(x => x.RolEnPrograma).HasMaxLength(30).IsRequired();
        b.HasOne(x => x.Programa).WithMany().HasForeignKey(x => x.ProgramaId);
        b.HasOne(x => x.Usuario).WithMany().HasForeignKey(x => x.UsuarioId);
        b.HasIndex(x => new { x.UsuarioId, x.ProgramaId });
    }
}
