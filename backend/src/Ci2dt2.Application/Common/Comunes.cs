using System.Data;
using Ci2dt2.Domain.Entities;

namespace Ci2dt2.Application.Common;

/// <summary>Abre conexiones para las consultas con Dapper.</summary>
public interface IDbConnectionFactory
{
    IDbConnection Crear();
}

/// <summary>Datos de la petición HTTP en curso (usuario, IP y ruta).</summary>
public interface IContextoPeticion
{
    int? UsuarioId { get; }
    string? Ip { get; }
    string? Ruta { get; }
}

/// <summary>Cola en memoria de registros de auditoría; un worker en segundo plano los escribe (HU-28).</summary>
public interface IColaAuditoria
{
    void Encolar(LogActividad registro);
}

/// <summary>Cambio de una entidad capturado al guardar (sin contraseñas, hashes ni tokens).</summary>
public sealed record CambioEntidad(
    string Entidad,
    string Estado,
    string? Id,
    IReadOnlyDictionary<string, object?> Anteriores,
    IReadOnlyDictionary<string, object?> Nuevos);

/// <summary>Cambios que la petición en curso guardó en la base de datos.</summary>
public interface ICambiosPeticion
{
    IReadOnlyList<CambioEntidad> Obtener();
}

/// <summary>Archivos físicos del repositorio documental (fuera de wwwroot).</summary>
public interface IAlmacenArchivos
{
    /// <summary>Guarda el contenido y devuelve la clave de almacenamiento.</summary>
    Task<string> GuardarAsync(Stream contenido, string extension, CancellationToken ct = default);

    Stream Abrir(string clave);
}

/// <summary>Error de regla de negocio: se responde con HTTP 422.</summary>
public sealed class ReglaNegocioException(string mensaje) : Exception(mensaje);

/// <summary>Recurso inexistente o no visible para el usuario: HTTP 404.</summary>
public sealed class NoEncontradoException(string mensaje) : Exception(mensaje);

/// <summary>El usuario no tiene permiso sobre el recurso: HTTP 403.</summary>
public sealed class AccesoDenegadoException(string mensaje = "No tiene permisos para realizar esta acción.") : Exception(mensaje);

public sealed record Pagina<T>(IReadOnlyList<T> Items, int Total, int PaginaActual, int TamanoPagina);
