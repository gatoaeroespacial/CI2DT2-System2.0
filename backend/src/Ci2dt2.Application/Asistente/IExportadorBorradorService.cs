using Ci2dt2.Application.Seguridad;

namespace Ci2dt2.Application.Asistente;

/// <summary>
/// Genera documentos formales (.docx) del proyecto en formulación con estándares institucionales.
/// </summary>
public interface IExportadorBorradorService
{
    Task<(byte[] Contenido, string NombreArchivo)> GenerarDocxAsync(
        int borradorId,
        UsuarioAutenticado usuario,
        CancellationToken ct = default);
}

