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
        b.ToTable("Proyectos", t =>
            t.HasCheckConstraint("CK_Proyectos_PorcentajeAvance", "[PorcentajeAvance] BETWEEN 0 AND 100"));
        b.Property(x => x.Codigo).HasMaxLength(30);
        b.Property(x => x.Titulo).HasMaxLength(300).IsRequired();
        b.Property(x => x.PorcentajeAvance).Porcentaje();
        b.Property(x => x.Moneda).Char(3);
        b.Property(x => x.PresupuestoTotal).Dinero();
        b.Property(x => x.TasaCambio).Tasa();
        b.Property(x => x.PresupuestoTotalCop).Dinero();
        b.HasIndex(x => x.Codigo).IsUnique().HasFilter("[Codigo] IS NOT NULL");
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
    }
}

internal sealed class ProyectoAntecedenteConfig : IEntityTypeConfiguration<ProyectoAntecedente>
{
    public void Configure(EntityTypeBuilder<ProyectoAntecedente> b)
    {
        b.ToTable("ProyectoAntecedentes");
        b.Property(x => x.NombreConvocatoria).HasMaxLength(200).IsRequired();
        b.Property(x => x.Resultado).HasMaxLength(30);
        b.Property(x => x.Puntaje).HasPrecision(6, 2);
        b.Property(x => x.EstadoBanco).HasMaxLength(30);
        b.HasOne(x => x.Proyecto).WithMany().HasForeignKey(x => x.ProyectoId);
        b.HasOne(x => x.Convocatoria).WithMany().HasForeignKey(x => x.ConvocatoriaId);
    }
}

internal sealed class ProyectoRevisionConfig : IEntityTypeConfiguration<ProyectoRevision>
{
    public void Configure(EntityTypeBuilder<ProyectoRevision> b)
    {
        b.ToTable("ProyectoRevisiones");
        b.Property(x => x.Etapa).HasMaxLength(30).IsRequired();
        b.Property(x => x.Decision).HasMaxLength(20).IsRequired();
        b.HasOne(x => x.Proyecto).WithMany().HasForeignKey(x => x.ProyectoId);
        b.HasOne(x => x.Revisor).WithMany().HasForeignKey(x => x.RevisorId);
        b.HasOne(x => x.EstadoAnterior).WithMany().HasForeignKey(x => x.EstadoAnteriorId);
        b.HasOne(x => x.EstadoNuevo).WithMany().HasForeignKey(x => x.EstadoNuevoId);
    }
}

internal sealed class ProyectoGrupoConfig : IEntityTypeConfiguration<ProyectoGrupo>
{
    public void Configure(EntityTypeBuilder<ProyectoGrupo> b)
    {
        b.ToTable("ProyectoGrupos");
        b.HasKey(x => new { x.ProyectoId, x.GrupoId });
        b.HasOne(x => x.Proyecto).WithMany().HasForeignKey(x => x.ProyectoId);
        b.HasOne(x => x.Grupo).WithMany().HasForeignKey(x => x.GrupoId);
    }
}

internal sealed class ProyectoAliadoConfig : IEntityTypeConfiguration<ProyectoAliado>
{
    public void Configure(EntityTypeBuilder<ProyectoAliado> b)
    {
        b.ToTable("ProyectoAliados");
        b.Property(x => x.RolAliado).HasMaxLength(30);
        b.HasOne(x => x.Proyecto).WithMany().HasForeignKey(x => x.ProyectoId);
        b.HasOne(x => x.Aliado).WithMany().HasForeignKey(x => x.AliadoId);
        b.HasOne(x => x.CartaCompromisoDoc).WithMany().HasForeignKey(x => x.CartaCompromisoDocId);
    }
}

internal sealed class ProyectoProductoConfig : IEntityTypeConfiguration<ProyectoProducto>
{
    public void Configure(EntityTypeBuilder<ProyectoProducto> b)
    {
        b.ToTable("ProyectoProductos");
        b.Property(x => x.TipoProducto).HasMaxLength(30).IsRequired();
        b.Property(x => x.Descripcion).HasMaxLength(300);
        b.Property(x => x.Estado).HasMaxLength(20).IsRequired();
        b.HasOne(x => x.Proyecto).WithMany().HasForeignKey(x => x.ProyectoId);
        b.HasOne(x => x.Publicacion).WithMany().HasForeignKey(x => x.PublicacionId);
        b.HasOne(x => x.PropiedadIntelectual).WithMany().HasForeignKey(x => x.PropiedadIntelectualId);
        b.HasOne(x => x.Documento).WithMany().HasForeignKey(x => x.DocumentoId);
    }
}

internal sealed class ProyectoCierreConfig : IEntityTypeConfiguration<ProyectoCierre>
{
    public void Configure(EntityTypeBuilder<ProyectoCierre> b)
    {
        b.ToTable("ProyectoCierres");
        b.Property(x => x.ConceptoComite).HasMaxLength(20);
        b.HasIndex(x => x.ProyectoId).IsUnique();
        b.HasOne(x => x.Proyecto).WithMany().HasForeignKey(x => x.ProyectoId);
        b.HasOne(x => x.InformeTecnicoDoc).WithMany().HasForeignKey(x => x.InformeTecnicoDocId);
        b.HasOne(x => x.InformeFinancieroDoc).WithMany().HasForeignKey(x => x.InformeFinancieroDocId);
        b.HasOne(x => x.ActaComiteDoc).WithMany().HasForeignKey(x => x.ActaComiteDocId);
        b.HasOne(x => x.PazYSalvoDoc).WithMany().HasForeignKey(x => x.PazYSalvoDocId);
    }
}
