using Ci2dt2.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ci2dt2.Infrastructure.Persistence.Configuraciones;

internal sealed class RolConfig : IEntityTypeConfiguration<Rol>
{
    public void Configure(EntityTypeBuilder<Rol> b)
    {
        b.ToTable("Roles");
        b.Property(x => x.Nombre).HasMaxLength(50).IsRequired();
        b.Property(x => x.Descripcion).HasMaxLength(200);
        b.HasIndex(x => x.Nombre).IsUnique();
        b.HasData(DatosSemilla.Roles);
    }
}

internal sealed class UsuarioConfig : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> b)
    {
        b.ToTable("Usuarios");
        b.Property(x => x.Email).HasMaxLength(150).IsRequired();
        b.Property(x => x.PasswordHash).HasMaxLength(200);
        b.Property(x => x.Nombres).HasMaxLength(100).IsRequired();
        b.Property(x => x.Apellidos).HasMaxLength(100).IsRequired();
        b.Property(x => x.TipoDocumento).HasMaxLength(10);
        b.Property(x => x.NumeroDocumento).HasMaxLength(30);
        b.Property(x => x.Telefono).HasMaxLength(30);
        b.Property(x => x.Cargo).HasMaxLength(100);
        b.Property(x => x.Dependencia).HasMaxLength(150);
        b.Property(x => x.CvLacUrl).HasMaxLength(300);
        b.Property(x => x.Orcid).HasMaxLength(30);
        b.Property(x => x.ExpoPushToken).HasMaxLength(200);
        b.HasIndex(x => x.Email).IsUnique();
        b.HasOne(x => x.Aliado).WithMany().HasForeignKey(x => x.AliadoId);
    }
}

internal sealed class UsuarioRolConfig : IEntityTypeConfiguration<UsuarioRol>
{
    public void Configure(EntityTypeBuilder<UsuarioRol> b)
    {
        b.ToTable("UsuarioRoles");
        b.HasKey(x => new { x.UsuarioId, x.RolId });
        b.HasOne(x => x.Usuario).WithMany(u => u.Roles).HasForeignKey(x => x.UsuarioId);
        b.HasOne(x => x.Rol).WithMany().HasForeignKey(x => x.RolId);
    }
}

internal sealed class RefreshTokenConfig : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> b)
    {
        b.ToTable("RefreshTokens");
        b.Property(x => x.TokenHash).HasMaxLength(200).IsRequired();
        b.Property(x => x.IpOrigen).HasMaxLength(45);
        b.HasIndex(x => x.TokenHash).IsUnique();
        b.HasOne(x => x.Usuario).WithMany().HasForeignKey(x => x.UsuarioId);
    }
}
