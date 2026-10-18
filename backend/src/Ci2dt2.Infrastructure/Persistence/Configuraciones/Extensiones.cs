using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ci2dt2.Infrastructure.Persistence.Configuraciones;

internal static class Extensiones
{
    /// <summary>Columna char(n): códigos fijos como monedas, hashes SHA-256 o periodos AAAA-MM.</summary>
    public static PropertyBuilder<T> Char<T>(this PropertyBuilder<T> propiedad, int longitud) =>
        propiedad.IsUnicode(false).IsFixedLength().HasMaxLength(longitud);

    /// <summary>Montos: decimal(18,2).</summary>
    public static PropertyBuilder<T> Dinero<T>(this PropertyBuilder<T> propiedad) => propiedad.HasPrecision(18, 2);

    /// <summary>Tasas de cambio: decimal(18,4).</summary>
    public static PropertyBuilder<T> Tasa<T>(this PropertyBuilder<T> propiedad) => propiedad.HasPrecision(18, 4);

    /// <summary>Porcentajes: decimal(5,2).</summary>
    public static PropertyBuilder<T> Porcentaje<T>(this PropertyBuilder<T> propiedad) => propiedad.HasPrecision(5, 2);
}
