using Ci2dt2.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ci2dt2.Infrastructure.Persistence.Configuraciones;

/// <summary>Tablas inmutables (regla 8): un trigger INSTEAD OF UPDATE, DELETE las protege.</summary>
internal static class Inmutables
{
    public static readonly string[] Tablas = ["LogActividades", "DocumentoFirmas", "ConsentimientosDatos"];

    public static string Trigger(string tabla) => $"TR_{tabla}_Inmutable";
}

// M13 Indicadores y reportes
internal sealed class IndicadorConfig : IEntityTypeConfiguration<Indicador>
{
    public void Configure(EntityTypeBuilder<Indicador> b)
    {
        b.ToTable("Indicadores");
        b.Property(x => x.Codigo).HasMaxLength(40);
        b.Property(x => x.Nombre).HasMaxLength(200).IsRequired();
        b.Property(x => x.Unidad).HasMaxLength(30);
        b.Property(x => x.ConsultaClave).HasMaxLength(80);
        b.Property(x => x.UmbralVerdePct).Porcentaje();
        b.Property(x => x.UmbralAmarilloPct).Porcentaje();
        b.Property(x => x.Periodicidad).HasMaxLength(15);
        b.HasOne(x => x.Programa).WithMany().HasForeignKey(x => x.ProgramaId);
    }
}

internal sealed class IndicadorMedicionConfig : IEntityTypeConfiguration<IndicadorMedicion>
{
    public void Configure(EntityTypeBuilder<IndicadorMedicion> b)
    {
        b.ToTable("IndicadorMediciones");
        b.Property(x => x.Periodo).HasMaxLength(7);
        b.Property(x => x.Meta).Dinero();
        b.Property(x => x.ValorActual).Dinero();
        b.Property(x => x.Semaforo).HasMaxLength(8);
        b.HasIndex(x => new { x.IndicadorId, x.Periodo });
        b.HasOne(x => x.Indicador).WithMany().HasForeignKey(x => x.IndicadorId);
    }
}

internal sealed class ReporteGeneradoConfig : IEntityTypeConfiguration<ReporteGenerado>
{
    public void Configure(EntityTypeBuilder<ReporteGenerado> b)
    {
        b.ToTable("ReportesGenerados");
        b.Property(x => x.Tipo).HasMaxLength(40).IsRequired();
        b.Property(x => x.Periodo).HasMaxLength(7);
        b.Property(x => x.Formato).HasMaxLength(5);
        b.HasOne(x => x.Programa).WithMany().HasForeignKey(x => x.ProgramaId);
        b.HasOne(x => x.GeneradoPor).WithMany().HasForeignKey(x => x.GeneradoPorId);
        b.HasOne(x => x.Documento).WithMany().HasForeignKey(x => x.DocumentoId);
    }
}

// M14 Visitas y datos personales
internal sealed class PoliticaTratamientoDatosConfig : IEntityTypeConfiguration<PoliticaTratamientoDatos>
{
    public void Configure(EntityTypeBuilder<PoliticaTratamientoDatos> b)
    {
        b.ToTable("PoliticasTratamientoDatos");
        b.Property(x => x.Version).HasMaxLength(10).IsRequired();
        b.Property(x => x.TextoUrl).HasMaxLength(400).IsRequired();
        // Admite NULL mientras se publica el texto oficial de la política (HU-29).
        b.Property(x => x.HashTexto).Char(64);
        b.HasData(DatosSemilla.PoliticasTratamientoDatos);
    }
}

internal sealed class ConsentimientoDatosConfig : IEntityTypeConfiguration<ConsentimientoDatos>
{
    public void Configure(EntityTypeBuilder<ConsentimientoDatos> b)
    {
        b.ToTable("ConsentimientosDatos", t => t.HasTrigger(Inmutables.Trigger("ConsentimientosDatos")));
        b.Property(x => x.Correo).HasMaxLength(150);
        b.Property(x => x.Contexto).HasMaxLength(20).IsRequired();
        b.Property(x => x.IpOrigen).HasMaxLength(45);
        b.Property(x => x.HashEvidencia).Char(64);
        b.HasOne(x => x.Politica).WithMany().HasForeignKey(x => x.PoliticaId);
        b.HasOne(x => x.Usuario).WithMany().HasForeignKey(x => x.UsuarioId);
    }
}

internal sealed class VisitaConfig : IEntityTypeConfiguration<Visita>
{
    public void Configure(EntityTypeBuilder<Visita> b)
    {
        b.ToTable("Visitas");
        b.Property(x => x.Identificacion).HasMaxLength(30).IsRequired();
        b.Property(x => x.NombreCompleto).HasMaxLength(200).IsRequired();
        b.Property(x => x.Correo).HasMaxLength(150);
        b.Property(x => x.Celular).HasMaxLength(30);
        b.Property(x => x.Dependencia).HasMaxLength(150);
        b.Property(x => x.Cargo).HasMaxLength(100);
        b.Property(x => x.MotivoVisita).HasMaxLength(300).IsRequired();
        b.HasIndex(x => x.FechaHoraEntrada);
        b.HasOne(x => x.Consentimiento).WithMany().HasForeignKey(x => x.ConsentimientoId);
        b.HasOne(x => x.Usuario).WithMany().HasForeignKey(x => x.UsuarioId);
        b.HasOne(x => x.RegistradaPor).WithMany().HasForeignKey(x => x.RegistradaPorId);
    }
}

// M15 Auditoría e IA
internal sealed class LogActividadConfig : IEntityTypeConfiguration<LogActividad>
{
    public void Configure(EntityTypeBuilder<LogActividad> b)
    {
        b.ToTable("LogActividades", t => t.HasTrigger(Inmutables.Trigger("LogActividades")));
        b.Property(x => x.IpOrigen).HasMaxLength(45);
        b.Property(x => x.Recurso).HasMaxLength(60).IsRequired();
        b.Property(x => x.RecursoId).HasMaxLength(40);
        b.Property(x => x.Accion).HasMaxLength(10).IsRequired();
        b.Property(x => x.Ruta).HasMaxLength(300);
        b.Property(x => x.HashEncadenado).Char(64);
        b.HasIndex(x => new { x.UsuarioId, x.FechaUtc });
        b.HasIndex(x => new { x.Recurso, x.RecursoId });
        b.HasOne(x => x.Usuario).WithMany().HasForeignKey(x => x.UsuarioId);
    }
}

internal sealed class FuenteConocimientoConfig : IEntityTypeConfiguration<FuenteConocimiento>
{
    public void Configure(EntityTypeBuilder<FuenteConocimiento> b)
    {
        b.ToTable("FuentesConocimiento");
        b.Property(x => x.Titulo).HasMaxLength(250).IsRequired();
        b.Property(x => x.TipoFuente).HasMaxLength(20);
        b.Property(x => x.EntidadTipo).HasMaxLength(40);
        b.Property(x => x.HashContenido).Char(64);
        b.Property(x => x.EstadoIndexacion).HasMaxLength(12);
        b.Property(x => x.IdVectorStore).HasMaxLength(100);
        b.HasIndex(x => new { x.EntidadTipo, x.EntidadId });
        b.HasOne(x => x.Documento).WithMany().HasForeignKey(x => x.DocumentoId);
    }
}
