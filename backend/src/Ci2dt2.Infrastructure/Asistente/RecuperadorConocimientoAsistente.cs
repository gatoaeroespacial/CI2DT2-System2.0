using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Ci2dt2.Application.Asistente;
using Ci2dt2.Domain.Entities;
using Ci2dt2.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Caching.Memory;

namespace Ci2dt2.Infrastructure.Asistente;

public sealed class RecuperadorConocimientoAsistente(
    AppDbContext db,
    IMemoryCache cache) : IRecuperadorConocimientoAsistente
{
    private const string ClaveIndice = "asistente:indice-rag";
    private const int MaximoFragmentos = 8;
    private static readonly TimeSpan VigenciaIndice = TimeSpan.FromMinutes(5);
    private static readonly SemaphoreSlim BloqueoIndice = new(1, 1);
    private static readonly MethodInfo MetodoLectura = typeof(RecuperadorConocimientoAsistente)
        .GetMethod(nameof(LeerEntidadesAsync), BindingFlags.NonPublic | BindingFlags.Static)!;
    private static readonly HashSet<string> EntidadesExcluidas = new(StringComparer.Ordinal)
    {
        nameof(RefreshToken),
        nameof(Rol),
        nameof(Usuario),
        nameof(UsuarioRol),
        nameof(LogActividad),
        nameof(DocumentoFirma),
        nameof(ConsentimientoDatos),
        nameof(Visita),
        nameof(FuenteConocimiento),
        nameof(PreferenciaNotificacion),
        nameof(Notificacion),
    };
    private static readonly HashSet<string> EntidadesSoloAdministrador = new(StringComparer.Ordinal)
    {
        nameof(ProgramaIntegrante),
        nameof(ProyectoIntegrante),
        nameof(SemilleroIntegrante),
        nameof(EventoAsistente),
        nameof(FormacionDocente),
        nameof(Inscripcion),
        nameof(Asistencia),
        nameof(Calificacion),
        nameof(Certificado),
        nameof(PublicacionAutor),
        nameof(PropiedadIntelectualAutor),
        nameof(TrabajoGrado),
        nameof(TrabajoGradoJurado),
        nameof(Sustentacion),
    };
    private static readonly HashSet<string> PropiedadesExcluidas = new(StringComparer.OrdinalIgnoreCase)
    {
        "Activo",
        "Apellidos",
        "Autor",
        "Autores",
        "CreadoEn",
        "CreadoPorId",
        "Contacto",
        "Email",
        "ExpoPushToken",
        "HashContenido",
        "HashDocumento",
        "HashEncadenado",
        "HashEvidencia",
        "HashSha256",
        "HashTexto",
        "Id",
        "Identificacion",
        "IpOrigen",
        "Nombres",
        "NombreCompleto",
        "NumeroDocumento",
        "PasswordHash",
        "Responsable",
        "RepresentanteLegal",
        "Nit",
        "Orcid",
        "CodigoVerificacion",
        "Estudiante",
        "Docente",
        "Telefono",
        "Celular",
        "TokenHash",
        "ActualizadoEn",
        "UrlArchivo",
        "TextoUrl",
        "NombreArchivo",
        "ConsultaClave",
    };
    private static readonly HashSet<string> PalabrasIgnoradas = new(StringComparer.Ordinal)
    {
        "como", "con", "cual", "cuales", "cuanto", "cuantos", "cuantas", "de", "del", "desde",
        "el", "ella", "ellas", "ellos", "en", "es", "esa", "ese", "eso", "esta", "este", "esto",
        "fue", "hay", "la", "las", "le", "les", "lo", "los", "mas", "mi", "mis", "muy", "no",
        "o", "para", "por", "que", "quien", "se", "ser", "su", "sus", "un", "una", "uno", "y",
    };
    private static readonly Regex ExpresionCorreo = new(
        @"\b[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
    private static readonly IReadOnlyDictionary<string, string> NombresEntidad = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["Aliado"] = "entidades aliadas",
        ["Avance"] = "avances de proyectos",
        ["Convocatoria"] = "convocatorias",
        ["Cronograma"] = "cronogramas",
        ["Documento"] = "documentos",
        ["Evento"] = "eventos",
        ["Formacion"] = "formaciones",
        ["GrupoInvestigacion"] = "grupos de investigación",
        ["Hito"] = "hitos de proyectos",
        ["Indicador"] = "indicadores",
        ["IndicadorMedicion"] = "mediciones de indicadores",
        ["ObjetivoEspecifico"] = "objetivos de programas",
        ["Programa"] = "programas",
        ["Proyecto"] = "proyectos",
        ["Publicacion"] = "publicaciones académicas",
        ["Semillero"] = "semilleros",
        ["TipoAliado"] = "tipos de entidad aliada",
        ["TipoDocumento"] = "tipos de documento",
        ["TipoEvento"] = "tipos de evento",
        ["TipoFormacion"] = "tipos de formación",
        ["TipoHito"] = "tipos de hito",
        ["TipoIntegranteSemillero"] = "tipos de integrantes de semillero",
        ["TipoProyecto"] = "tipos de proyecto",
        ["TipoPublicacion"] = "tipos de publicación",
        ["TipoConvocatoria"] = "tipos de convocatoria",
        ["TiposIntegranteSemillero"] = "tipos de integrante de semillero",
    };

    public async Task<IReadOnlyList<FragmentoConocimientoAsistente>> RecuperarAsync(
        string pregunta,
        IReadOnlyCollection<string> roles,
        CancellationToken ct)
    {
        var documentos = await ObtenerIndiceAsync(ct);
        var tokensConsulta = Tokenizar(pregunta).Distinct(StringComparer.Ordinal).ToArray();
        if (tokensConsulta.Length == 0)
            return [];

        var administrador = roles.Contains(Ci2dt2.Domain.Roles.Administrador);
        return documentos
            .Where(documento => administrador || !documento.SoloAdministrador)
            .Select(documento => (Documento: documento, Relevancia: CalcularRelevancia(documento, tokensConsulta)))
            .Where(x => x.Relevancia > 0)
            .OrderByDescending(x => x.Relevancia)
            .ThenBy(x => x.Documento.Titulo, StringComparer.Ordinal)
            .Take(MaximoFragmentos)
            .Select(x => new FragmentoConocimientoAsistente(x.Documento.Titulo, x.Documento.Contenido, x.Relevancia))
            .ToArray();
    }

    private async Task<IReadOnlyList<DocumentoIndexado>> ObtenerIndiceAsync(CancellationToken ct)
    {
        if (cache.TryGetValue(ClaveIndice, out IReadOnlyList<DocumentoIndexado>? indice) && indice is not null)
            return indice;

        await BloqueoIndice.WaitAsync(ct);
        try
        {
            if (cache.TryGetValue(ClaveIndice, out indice) && indice is not null)
                return indice;

            indice = await ConstruirIndiceAsync(ct);
            cache.Set(ClaveIndice, indice, VigenciaIndice);
            return indice;
        }
        finally
        {
            BloqueoIndice.Release();
        }
    }

    private async Task<IReadOnlyList<DocumentoIndexado>> ConstruirIndiceAsync(CancellationToken ct)
    {
        var entidades = db.Model.GetEntityTypes()
            .Where(tipo => tipo.BaseType is null &&
                           !tipo.IsOwned() &&
                           tipo.ClrType.Namespace == typeof(Programa).Namespace &&
                           !EntidadesExcluidas.Contains(tipo.ClrType.Name))
            .Where(tipo => tipo.ClrType.Name != nameof(Usuario))
            .OrderBy(tipo => tipo.ClrType.Name, StringComparer.Ordinal)
            .ToArray();
        var datos = new List<DatosEntidad>(entidades.Length);

        foreach (var entidad in entidades)
        {
            ct.ThrowIfCancellationRequested();
            var filas = await LeerFilasAsync(entidad.ClrType, ct);
            var propiedadActivo = entidad.FindProperty(nameof(EntidadAuditable.Activo));
            if (propiedadActivo is not null)
                filas = filas.Where(fila => propiedadActivo.PropertyInfo?.GetValue(fila) is true).ToList();

            datos.Add(new DatosEntidad(entidad, filas));
        }

        var etiquetasRelacionadas = CrearEtiquetasRelacionadas(datos);
        var documentos = new List<DocumentoIndexado>(datos.Sum(d => d.Filas.Count + 1));
        foreach (var entidad in datos)
        {
            var etiqueta = ObtenerNombreEntidad(entidad.Tipo.ClrType.Name);
            var soloAdministrador = EntidadesSoloAdministrador.Contains(entidad.Tipo.ClrType.Name);
            documentos.Add(CrearResumen(entidad, etiqueta, etiquetasRelacionadas, soloAdministrador));
            foreach (var fila in entidad.Filas)
            {
                var contenido = CrearContenidoRegistro(entidad, fila, etiquetasRelacionadas);
                if (!string.IsNullOrWhiteSpace(contenido))
                    documentos.Add(new DocumentoIndexado(etiqueta, contenido, soloAdministrador));
            }
        }

        return documentos;
    }

    private async Task<List<object>> LeerFilasAsync(Type tipo, CancellationToken ct)
    {
        var metodo = MetodoLectura.MakeGenericMethod(tipo);
        var tarea = (Task<List<object>>)metodo.Invoke(null, [db, ct])!;
        return await tarea;
    }

    private static async Task<List<object>> LeerEntidadesAsync<TEntity>(AppDbContext contexto, CancellationToken ct)
        where TEntity : class =>
        (await contexto.Set<TEntity>().AsNoTracking().ToListAsync(ct)).Cast<object>().ToList();

    private static Dictionary<Type, Dictionary<string, string>> CrearEtiquetasRelacionadas(
        IReadOnlyList<DatosEntidad> entidades)
    {
        var resultado = new Dictionary<Type, Dictionary<string, string>>();
        foreach (var entidad in entidades)
        {
            var clave = entidad.Tipo.FindPrimaryKey();
            if (clave is null || clave.Properties.Count != 1)
                continue;

            var etiquetas = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var fila in entidad.Filas)
            {
                var valorClave = clave.Properties[0].PropertyInfo?.GetValue(fila);
                if (valorClave is null)
                    continue;
                var descripcion = ObtenerCamposSeguros(entidad.Tipo, fila)
                    .Where(campo => EsCampoDescriptivo(campo.Nombre))
                    .Take(2)
                    .Select(campo => campo.Valor)
                    .ToArray();
                if (descripcion.Length > 0)
                    etiquetas[Convert.ToString(valorClave, CultureInfo.InvariantCulture)!] = string.Join(" — ", descripcion);
            }

            resultado[entidad.Tipo.ClrType] = etiquetas;
        }

        return resultado;
    }

    private static DocumentoIndexado CrearResumen(
        DatosEntidad entidad,
        string etiqueta,
        IReadOnlyDictionary<Type, Dictionary<string, string>> etiquetasRelacionadas,
        bool soloAdministrador)
    {
        var estadoRegistro = typeof(EntidadAuditable).IsAssignableFrom(entidad.Tipo.ClrType)
            ? " activos"
            : string.Empty;
        var resumen = new StringBuilder($"En el centro hay {entidad.Filas.Count} registros{estadoRegistro} de {etiqueta}.");
        foreach (var relacion in entidad.Tipo.GetForeignKeys())
        {
            if (!etiquetasRelacionadas.TryGetValue(relacion.PrincipalEntityType.ClrType, out var etiquetas))
                continue;
            var distribucion = entidad.Filas
                .Select(fila => ObtenerEtiquetaRelacionada(relacion, fila, etiquetas))
                .Where(valor => valor is not null)
                .GroupBy(valor => valor!, StringComparer.OrdinalIgnoreCase)
                .OrderByDescending(grupo => grupo.Count())
                .Take(8)
                .Select(grupo => $"{grupo.Key}: {grupo.Count()}")
                .ToArray();
            if (distribucion.Length > 0)
                resumen.Append($" Distribución por {ObtenerNombreEntidad(relacion.PrincipalEntityType.ClrType.Name)}: {string.Join(", ", distribucion)}.");
        }

        return new DocumentoIndexado(etiqueta, resumen.ToString(), soloAdministrador);
    }

    private static string CrearContenidoRegistro(
        DatosEntidad entidad,
        object fila,
        IReadOnlyDictionary<Type, Dictionary<string, string>> etiquetasRelacionadas)
    {
        var campos = ObtenerCamposSeguros(entidad.Tipo, fila)
            .Where(campo => !string.IsNullOrWhiteSpace(campo.Valor))
            .Select(campo => $"{Humanizar(campo.Nombre)}: {campo.Valor}")
            .ToList();

        foreach (var relacion in entidad.Tipo.GetForeignKeys())
        {
            if (!etiquetasRelacionadas.TryGetValue(relacion.PrincipalEntityType.ClrType, out var etiquetas))
                continue;
            var etiqueta = ObtenerEtiquetaRelacionada(relacion, fila, etiquetas);
            if (etiqueta is not null)
                campos.Add($"{ObtenerNombreEntidad(relacion.PrincipalEntityType.ClrType.Name)}: {etiqueta}");
        }

        if (campos.Count == 0)
            return string.Empty;

        var contenido = $"{ObtenerNombreEntidad(entidad.Tipo.ClrType.Name)}. {string.Join(". ", campos)}";
        return contenido.Length > 2500 ? contenido[..2500] : contenido;
    }

    private static string? ObtenerEtiquetaRelacionada(
        IForeignKey relacion,
        object fila,
        IReadOnlyDictionary<string, string> etiquetas)
    {
        if (relacion.Properties.Count != 1 || relacion.PrincipalKey.Properties.Count != 1)
            return null;
        var valorForaneo = relacion.Properties[0].PropertyInfo?.GetValue(fila);
        return valorForaneo is null
            ? null
            : etiquetas.GetValueOrDefault(Convert.ToString(valorForaneo, CultureInfo.InvariantCulture)!);
    }

    private static IEnumerable<(string Nombre, string Valor)> ObtenerCamposSeguros(IEntityType entidad, object fila)
    {
        foreach (var propiedad in entidad.GetProperties())
        {
            if (propiedad.PropertyInfo is null || !EsPropiedadSegura(propiedad.Name, propiedad.ClrType))
                continue;
            var valor = propiedad.PropertyInfo.GetValue(fila);
            var texto = FormatearValor(valor);
            if (!string.IsNullOrWhiteSpace(texto))
                yield return (propiedad.Name, texto);
        }
    }

    private static bool EsPropiedadSegura(string nombre, Type tipo)
    {
        if (PropiedadesExcluidas.Contains(nombre) ||
            nombre.EndsWith("Id", StringComparison.OrdinalIgnoreCase) ||
            nombre.Contains("email", StringComparison.OrdinalIgnoreCase) ||
            nombre.Contains("mail", StringComparison.OrdinalIgnoreCase) ||
            nombre.Contains("correo", StringComparison.OrdinalIgnoreCase) ||
            nombre.Contains("password", StringComparison.OrdinalIgnoreCase) ||
            nombre.Contains("contrasena", StringComparison.OrdinalIgnoreCase) ||
            nombre.Contains("token", StringComparison.OrdinalIgnoreCase) ||
            nombre.Contains("hash", StringComparison.OrdinalIgnoreCase) ||
            nombre.Contains("identificacion", StringComparison.OrdinalIgnoreCase) ||
            nombre.Contains("telefono", StringComparison.OrdinalIgnoreCase) ||
            nombre.Contains("celular", StringComparison.OrdinalIgnoreCase) ||
            nombre.Contains("representante", StringComparison.OrdinalIgnoreCase) ||
            nombre.Contains("contacto", StringComparison.OrdinalIgnoreCase) ||
            nombre.Contains("autor", StringComparison.OrdinalIgnoreCase) ||
            nombre.Contains("estudiante", StringComparison.OrdinalIgnoreCase) ||
            nombre.Contains("docente", StringComparison.OrdinalIgnoreCase) ||
            nombre.Contains("responsable", StringComparison.OrdinalIgnoreCase) ||
            nombre.Equals("Ip", StringComparison.OrdinalIgnoreCase) ||
            nombre.Equals("IpOrigen", StringComparison.OrdinalIgnoreCase) ||
            nombre.Contains("url", StringComparison.OrdinalIgnoreCase) ||
            nombre.Contains("archivo", StringComparison.OrdinalIgnoreCase) ||
            nombre.Contains("consulta", StringComparison.OrdinalIgnoreCase) ||
            nombre.Contains("creado", StringComparison.OrdinalIgnoreCase) ||
            nombre.Contains("actualizado", StringComparison.OrdinalIgnoreCase))
            return false;

        var tipoReal = Nullable.GetUnderlyingType(tipo) ?? tipo;
        return tipoReal == typeof(string) ||
               tipoReal == typeof(bool) ||
               tipoReal.IsEnum ||
               tipoReal.IsPrimitive ||
               tipoReal == typeof(decimal) ||
               tipoReal == typeof(DateTime) ||
               tipoReal == typeof(DateOnly);
    }

    private static bool EsCampoDescriptivo(string nombre) =>
        nombre.Contains("Nombre", StringComparison.OrdinalIgnoreCase) ||
        nombre.Contains("Titulo", StringComparison.OrdinalIgnoreCase) ||
        nombre.Contains("Codigo", StringComparison.OrdinalIgnoreCase) ||
        nombre.Contains("Estado", StringComparison.OrdinalIgnoreCase) ||
        nombre.Contains("Tipo", StringComparison.OrdinalIgnoreCase) ||
        nombre.Contains("Descripcion", StringComparison.OrdinalIgnoreCase);

    private static double CalcularRelevancia(DocumentoIndexado documento, IReadOnlyCollection<string> consulta)
    {
        var tokensContenido = Tokenizar(documento.Contenido).ToHashSet(StringComparer.Ordinal);
        var tokensTitulo = Tokenizar(documento.Titulo).ToHashSet(StringComparer.Ordinal);
        var coincidenciasContenido = consulta.Count(tokensContenido.Contains);
        var coincidenciasTitulo = consulta.Count(tokensTitulo.Contains);
        if (coincidenciasContenido == 0 && coincidenciasTitulo == 0)
            return 0;

        var cobertura = (double)(coincidenciasContenido + coincidenciasTitulo * 2) / consulta.Count;
        return cobertura + coincidenciasTitulo * 0.5 + (coincidenciasContenido > 0 ? 0.1 : 0);
    }

    private static IEnumerable<string> Tokenizar(string texto)
    {
        foreach (Match coincidencia in Regex.Matches(Normalizar(texto), @"[\p{L}\p{N}]+"))
        {
            var token = coincidencia.Value;
            if (token.Length < 3 || PalabrasIgnoradas.Contains(token))
                continue;
            if (token.Length > 5 && token.EndsWith("es", StringComparison.Ordinal))
                token = token[..^2];
            else if (token.Length > 4 && token.EndsWith('s'))
                token = token[..^1];
            yield return token;
        }
    }

    private static string Normalizar(string texto)
    {
        var descompuesta = texto.Normalize(NormalizationForm.FormD);
        return new string(descompuesta.Where(caracter =>
                CharUnicodeInfo.GetUnicodeCategory(caracter) != UnicodeCategory.NonSpacingMark)
            .ToArray()).ToLowerInvariant();
    }

    private static string ObtenerNombreEntidad(string nombre) =>
        NombresEntidad.GetValueOrDefault(nombre) ?? Humanizar(nombre);

    private static string Humanizar(string nombre) =>
        Regex.Replace(nombre, @"(?<=[a-záéíóúñ])(?=[A-ZÁÉÍÓÚÑ])", " ");

    private static string? FormatearValor(object? valor) => valor switch
    {
        null => null,
        string texto => ExpresionCorreo.Replace(texto, "[correo omitido]"),
        bool booleano => booleano ? "sí" : "no",
        DateOnly fecha => fecha.ToString("d 'de' MMMM 'de' yyyy", CultureInfo.GetCultureInfo("es-CO")),
        DateTime fecha => fecha.ToString("d 'de' MMMM 'de' yyyy", CultureInfo.GetCultureInfo("es-CO")),
        decimal numero => numero.ToString("N2", CultureInfo.GetCultureInfo("es-CO")),
        IFormattable formateable => formateable.ToString(null, CultureInfo.GetCultureInfo("es-CO")),
        _ => valor.ToString(),
    };

    private sealed record DatosEntidad(IEntityType Tipo, IReadOnlyList<object> Filas);
    private sealed record DocumentoIndexado(string Titulo, string Contenido, bool SoloAdministrador);
}
