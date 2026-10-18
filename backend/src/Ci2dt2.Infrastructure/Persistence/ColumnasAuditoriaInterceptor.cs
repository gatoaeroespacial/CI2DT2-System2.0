using Ci2dt2.Application.Common;
using Ci2dt2.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Ci2dt2.Infrastructure.Persistence;

/// <summary>
/// Llena CreadoPorId al insertar y ActualizadoEn al modificar. CreadoEn lo asigna la base de
/// datos (DEFAULT SYSUTCDATETIME()).
/// </summary>
internal sealed class ColumnasAuditoriaInterceptor(IContextoPeticion? contexto, TimeProvider reloj) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        Aplicar(eventData.Context);
        return result;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        Aplicar(eventData.Context);
        return ValueTask.FromResult(result);
    }

    private void Aplicar(DbContext? db)
    {
        if (db is null)
            return;

        var ahora = reloj.GetUtcNow().UtcDateTime;
        foreach (var entrada in db.ChangeTracker.Entries<EntidadAuditable>())
        {
            if (entrada.State == EntityState.Added)
                entrada.Entity.CreadoPorId ??= contexto?.UsuarioId;
            else if (entrada.State == EntityState.Modified)
                entrada.Entity.ActualizadoEn = ahora;
        }
    }
}
