using System.Data;

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

/// <summary>Error de regla de negocio: se responde con HTTP 422.</summary>
public sealed class ReglaNegocioException(string mensaje) : Exception(mensaje);

/// <summary>Recurso inexistente o no visible para el usuario: HTTP 404.</summary>
public sealed class NoEncontradoException(string mensaje) : Exception(mensaje);

public sealed record Pagina<T>(IReadOnlyList<T> Items, int Total, int PaginaActual, int TamanoPagina);
