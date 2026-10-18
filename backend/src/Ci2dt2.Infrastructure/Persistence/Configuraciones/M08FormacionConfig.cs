using Ci2dt2.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ci2dt2.Infrastructure.Persistence.Configuraciones;

internal sealed class TipoFormacionConfig : IEntityTypeConfiguration<TipoFormacion>
{
    public void Configure(EntityTypeBuilder<TipoFormacion> b)
    {
        b.ToTable("TiposFormacion");
        b.Property(x => x.Nombre).HasMaxLength(60).IsRequired();
        b.HasData(DatosSemilla.TiposFormacion);
    }
}

internal sealed class ModalidadConfig : IEntityTypeConfiguration<Modalidad>
{
    public void Configure(EntityTypeBuilder<Modalidad> b)
    {
        b.ToTable("Modalidades");
        b.Property(x => x.Nombre).HasMaxLength(30).IsRequired();
        b.HasData(DatosSemilla.Modalidades);
    }
}

internal sealed class FormacionConfig : IEntityTypeConfiguration<Formacion>
{
    public void Configure(EntityTypeBuilder<Formacion> b)
    {
        b.ToTable("Formaciones");
        b.Property(x => x.Nombre).HasMaxLength(200).IsRequired();
        b.Property(x => x.ValorMatricula).Dinero();
        b.Property(x => x.MetaFinancieraCop).Dinero();
        b.Property(x => x.AsistenciaMinimaPct).Porcentaje();
        b.Property(x => x.NotaMinima).HasPrecision(4, 2);
        b.Property(x => x.Estado).HasMaxLength(25).IsRequired();
        b.HasOne(x => x.Programa).WithMany().HasForeignKey(x => x.ProgramaId);
        b.HasOne(x => x.TipoFormacion).WithMany().HasForeignKey(x => x.TipoFormacionId);
        b.HasOne(x => x.Modalidad).WithMany().HasForeignKey(x => x.ModalidadId);
        b.HasOne(x => x.AutorizadaPor).WithMany().HasForeignKey(x => x.AutorizadaPorId);
    }
}

internal sealed class FormacionDocenteConfig : IEntityTypeConfiguration<FormacionDocente>
{
    public void Configure(EntityTypeBuilder<FormacionDocente> b)
    {
        b.ToTable("FormacionDocentes");
        b.HasOne(x => x.Formacion).WithMany().HasForeignKey(x => x.FormacionId);
        b.HasOne(x => x.Docente).WithMany().HasForeignKey(x => x.DocenteId);
    }
}

internal sealed class InscripcionConfig : IEntityTypeConfiguration<Inscripcion>
{
    public void Configure(EntityTypeBuilder<Inscripcion> b)
    {
        b.ToTable("Inscripciones");
        b.Property(x => x.Estado).HasMaxLength(15).IsRequired();
        b.Property(x => x.ValorPagado).Dinero();
        b.Property(x => x.PorcentajeAsistencia).Porcentaje();
        b.Property(x => x.NotaFinal).HasPrecision(4, 2);
        // Una persona no puede inscribirse dos veces a la misma formación.
        b.HasIndex(x => new { x.FormacionId, x.UsuarioId }).IsUnique();
        b.HasOne(x => x.Formacion).WithMany().HasForeignKey(x => x.FormacionId);
        b.HasOne(x => x.Usuario).WithMany().HasForeignKey(x => x.UsuarioId);
        b.HasOne(x => x.Consentimiento).WithMany().HasForeignKey(x => x.ConsentimientoId);
        b.HasOne(x => x.SoportePagoDoc).WithMany().HasForeignKey(x => x.SoportePagoDocId);
    }
}

internal sealed class FormacionSesionConfig : IEntityTypeConfiguration<FormacionSesion>
{
    public void Configure(EntityTypeBuilder<FormacionSesion> b)
    {
        b.ToTable("FormacionSesiones");
        b.Property(x => x.Tema).HasMaxLength(200);
        b.HasOne(x => x.Formacion).WithMany().HasForeignKey(x => x.FormacionId);
        b.HasOne(x => x.Docente).WithMany().HasForeignKey(x => x.DocenteId);
    }
}

internal sealed class AsistenciaConfig : IEntityTypeConfiguration<Asistencia>
{
    public void Configure(EntityTypeBuilder<Asistencia> b)
    {
        b.ToTable("Asistencias");
        b.HasOne(x => x.Sesion).WithMany().HasForeignKey(x => x.SesionId);
        b.HasOne(x => x.Inscripcion).WithMany().HasForeignKey(x => x.InscripcionId);
    }
}

internal sealed class CalificacionConfig : IEntityTypeConfiguration<Calificacion>
{
    public void Configure(EntityTypeBuilder<Calificacion> b)
    {
        b.ToTable("Calificaciones");
        b.Property(x => x.Actividad).HasMaxLength(150).IsRequired();
        b.Property(x => x.Nota).HasPrecision(4, 2);
        b.Property(x => x.PesoPct).Porcentaje();
        b.HasOne(x => x.Inscripcion).WithMany().HasForeignKey(x => x.InscripcionId);
    }
}

internal sealed class CertificadoConfig : IEntityTypeConfiguration<Certificado>
{
    public void Configure(EntityTypeBuilder<Certificado> b)
    {
        b.ToTable("Certificados");
        b.Property(x => x.CodigoVerificacion).HasMaxLength(40);
        b.Property(x => x.HashCertificado).Char(64);
        b.Property(x => x.TxHashBlockchain).HasMaxLength(100);
        b.Property(x => x.Estado).HasMaxLength(20).IsRequired();
        b.Property(x => x.UrlVerificacion).HasMaxLength(400);
        // Un certificado por inscripción y código de verificación único (se usa en el QR).
        b.HasIndex(x => x.InscripcionId).IsUnique();
        b.HasIndex(x => x.CodigoVerificacion).IsUnique().HasFilter("[CodigoVerificacion] IS NOT NULL");
        b.HasOne(x => x.Inscripcion).WithMany().HasForeignKey(x => x.InscripcionId);
        b.HasOne(x => x.FirmaDirector).WithMany().HasForeignKey(x => x.FirmaDirectorId);
        b.HasOne(x => x.FirmaCoordinador).WithMany().HasForeignKey(x => x.FirmaCoordinadorId);
    }
}
