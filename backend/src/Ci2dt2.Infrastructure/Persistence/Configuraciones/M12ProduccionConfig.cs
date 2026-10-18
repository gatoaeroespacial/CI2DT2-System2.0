using Ci2dt2.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ci2dt2.Infrastructure.Persistence.Configuraciones;

internal sealed class TipoPublicacionConfig : IEntityTypeConfiguration<TipoPublicacion>
{
    public void Configure(EntityTypeBuilder<TipoPublicacion> b)
    {
        b.ToTable("TiposPublicacion");
        b.Property(x => x.Nombre).HasMaxLength(60).IsRequired();
        b.HasData(DatosSemilla.TiposPublicacion);
    }
}

internal sealed class PublicacionConfig : IEntityTypeConfiguration<Publicacion>
{
    public void Configure(EntityTypeBuilder<Publicacion> b)
    {
        b.ToTable("Publicaciones");
        b.Property(x => x.Titulo).HasMaxLength(400).IsRequired();
        b.Property(x => x.Revista).HasMaxLength(200);
        b.Property(x => x.Doi).HasMaxLength(100);
        b.HasOne(x => x.Programa).WithMany().HasForeignKey(x => x.ProgramaId);
        b.HasOne(x => x.Proyecto).WithMany().HasForeignKey(x => x.ProyectoId);
        b.HasOne(x => x.TipoPublicacion).WithMany().HasForeignKey(x => x.TipoPublicacionId);
    }
}

internal sealed class PublicacionAutorConfig : IEntityTypeConfiguration<PublicacionAutor>
{
    public void Configure(EntityTypeBuilder<PublicacionAutor> b)
    {
        b.ToTable("PublicacionAutores");
        b.HasKey(x => new { x.PublicacionId, x.UsuarioId });
        b.HasOne(x => x.Publicacion).WithMany().HasForeignKey(x => x.PublicacionId);
        b.HasOne(x => x.Usuario).WithMany().HasForeignKey(x => x.UsuarioId);
    }
}

internal sealed class PropiedadIntelectualConfig : IEntityTypeConfiguration<PropiedadIntelectual>
{
    public void Configure(EntityTypeBuilder<PropiedadIntelectual> b)
    {
        b.ToTable("PropiedadIntelectual");
        b.Property(x => x.Tipo).HasMaxLength(30).IsRequired();
        b.Property(x => x.Titulo).HasMaxLength(300).IsRequired();
        b.Property(x => x.EntidadTramite).HasMaxLength(10);
        b.Property(x => x.Estado).HasMaxLength(25).IsRequired();
        b.Property(x => x.NumeroRadicado).HasMaxLength(50);
        b.Property(x => x.NumeroRegistro).HasMaxLength(50);
        b.Property(x => x.NumeroResolucion).HasMaxLength(50);
        b.Property(x => x.ParticipacionUcaldasPct).Porcentaje();
        b.HasOne(x => x.Proyecto).WithMany().HasForeignKey(x => x.ProyectoId);
    }
}

internal sealed class PropiedadIntelectualAutorConfig : IEntityTypeConfiguration<PropiedadIntelectualAutor>
{
    public void Configure(EntityTypeBuilder<PropiedadIntelectualAutor> b)
    {
        b.ToTable("PropiedadIntelectualAutores");
        b.HasKey(x => new { x.PropiedadIntelectualId, x.UsuarioId });
        b.Property(x => x.RolAutor).HasMaxLength(15);
        b.Property(x => x.ParticipacionPct).Porcentaje();
        b.HasOne(x => x.PropiedadIntelectual).WithMany().HasForeignKey(x => x.PropiedadIntelectualId);
        b.HasOne(x => x.Usuario).WithMany().HasForeignKey(x => x.UsuarioId);
    }
}

internal sealed class TrabajoGradoConfig : IEntityTypeConfiguration<TrabajoGrado>
{
    public void Configure(EntityTypeBuilder<TrabajoGrado> b)
    {
        b.ToTable("TrabajosGrado");
        b.Property(x => x.Tipo).HasMaxLength(25).IsRequired();
        b.Property(x => x.Titulo).HasMaxLength(300).IsRequired();
        b.Property(x => x.Estado).HasMaxLength(25).IsRequired();
        b.Property(x => x.ConceptoFinal).HasMaxLength(15);
        b.HasOne(x => x.Programa).WithMany().HasForeignKey(x => x.ProgramaId);
        b.HasOne(x => x.Proyecto).WithMany().HasForeignKey(x => x.ProyectoId);
        b.HasOne(x => x.Estudiante).WithMany().HasForeignKey(x => x.EstudianteId);
        b.HasOne(x => x.Director).WithMany().HasForeignKey(x => x.DirectorId);
        b.HasOne(x => x.Codirector).WithMany().HasForeignKey(x => x.CodirectorId);
        b.HasOne(x => x.PropuestaDoc).WithMany().HasForeignKey(x => x.PropuestaDocId);
    }
}

internal sealed class TrabajoGradoJuradoConfig : IEntityTypeConfiguration<TrabajoGradoJurado>
{
    public void Configure(EntityTypeBuilder<TrabajoGradoJurado> b)
    {
        b.ToTable("TrabajoGradoJurados");
        b.Property(x => x.RolJurado).HasMaxLength(25);
        b.Property(x => x.Concepto).HasMaxLength(15);
        b.HasOne(x => x.TrabajoGrado).WithMany().HasForeignKey(x => x.TrabajoGradoId);
        b.HasOne(x => x.Usuario).WithMany().HasForeignKey(x => x.UsuarioId);
    }
}

internal sealed class SustentacionConfig : IEntityTypeConfiguration<Sustentacion>
{
    public void Configure(EntityTypeBuilder<Sustentacion> b)
    {
        b.ToTable("Sustentaciones");
        b.Property(x => x.Modalidad).HasMaxLength(12);
        b.Property(x => x.Lugar).HasMaxLength(150);
        b.Property(x => x.Concepto).HasMaxLength(15);
        b.HasOne(x => x.TrabajoGrado).WithMany().HasForeignKey(x => x.TrabajoGradoId);
        b.HasOne(x => x.ActaDocumento).WithMany().HasForeignKey(x => x.ActaDocumentoId);
    }
}
