using Ci2dt2.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ci2dt2.Infrastructure.Persistence.Configuraciones;

// M09 Eventos
internal sealed class TipoEventoConfig : IEntityTypeConfiguration<TipoEvento>
{
    public void Configure(EntityTypeBuilder<TipoEvento> b)
    {
        b.ToTable("TiposEvento");
        b.Property(x => x.Nombre).HasMaxLength(60).IsRequired();
        b.HasData(DatosSemilla.TiposEvento);
    }
}

internal sealed class EventoConfig : IEntityTypeConfiguration<Evento>
{
    public void Configure(EntityTypeBuilder<Evento> b)
    {
        b.ToTable("Eventos");
        b.Property(x => x.Nombre).HasMaxLength(200).IsRequired();
        b.Property(x => x.Lugar).HasMaxLength(200);
        b.Property(x => x.Estado).HasMaxLength(15).IsRequired();
        b.HasOne(x => x.Programa).WithMany().HasForeignKey(x => x.ProgramaId);
        b.HasOne(x => x.TipoEvento).WithMany().HasForeignKey(x => x.TipoEventoId);
        b.HasOne(x => x.Modalidad).WithMany().HasForeignKey(x => x.ModalidadId);
        b.HasOne(x => x.ResponsableLogistica).WithMany().HasForeignKey(x => x.ResponsableLogisticaId);
    }
}

internal sealed class EventoAsistenteConfig : IEntityTypeConfiguration<EventoAsistente>
{
    public void Configure(EntityTypeBuilder<EventoAsistente> b)
    {
        b.ToTable("EventoAsistentes");
        b.Property(x => x.Cargo).HasMaxLength(100);
        b.Property(x => x.Estado).HasMaxLength(15).IsRequired();
        b.HasOne(x => x.Evento).WithMany().HasForeignKey(x => x.EventoId);
        b.HasOne(x => x.Usuario).WithMany().HasForeignKey(x => x.UsuarioId);
        b.HasOne(x => x.Aliado).WithMany().HasForeignKey(x => x.AliadoId);
        b.HasOne(x => x.Consentimiento).WithMany().HasForeignKey(x => x.ConsentimientoId);
    }
}

// M10 Semilleros
internal sealed class SemilleroConfig : IEntityTypeConfiguration<Semillero>
{
    public void Configure(EntityTypeBuilder<Semillero> b)
    {
        b.ToTable("Semilleros");
        b.Property(x => x.Nombre).HasMaxLength(150).IsRequired();
        b.HasOne(x => x.Programa).WithMany().HasForeignKey(x => x.ProgramaId);
        b.HasOne(x => x.Coordinador).WithMany().HasForeignKey(x => x.CoordinadorId);
    }
}

internal sealed class TipoIntegranteSemilleroConfig : IEntityTypeConfiguration<TipoIntegranteSemillero>
{
    public void Configure(EntityTypeBuilder<TipoIntegranteSemillero> b)
    {
        b.ToTable("TiposIntegranteSemillero");
        b.Property(x => x.Nombre).HasMaxLength(40).IsRequired();
        b.HasData(DatosSemilla.TiposIntegranteSemillero);
    }
}

internal sealed class SemilleroIntegranteConfig : IEntityTypeConfiguration<SemilleroIntegrante>
{
    public void Configure(EntityTypeBuilder<SemilleroIntegrante> b)
    {
        b.ToTable("SemilleroIntegrantes");
        b.Property(x => x.EstadoAprobacion).HasMaxLength(15);
        b.HasOne(x => x.Semillero).WithMany().HasForeignKey(x => x.SemilleroId);
        b.HasOne(x => x.Usuario).WithMany().HasForeignKey(x => x.UsuarioId);
        b.HasOne(x => x.TipoIntegrante).WithMany().HasForeignKey(x => x.TipoIntegranteId);
        b.HasOne(x => x.Proyecto).WithMany().HasForeignKey(x => x.ProyectoId);
        b.HasOne(x => x.AprobadoPor).WithMany().HasForeignKey(x => x.AprobadoPorId);
    }
}

// M11 Entorno empresarial
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
        b.Property(x => x.Email).HasMaxLength(150).IsRequired();
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
