using static Ci2dt2.Domain.Roles;

namespace Ci2dt2.Application.Seguridad;

/// <summary>
/// Matriz de permisos del MVP (Anexo A). Cada acción es una política de autorización
/// con los roles que la tienen; los permisos de un usuario con varios roles son la unión.
/// Las restricciones de alcance ("Su programa", "Sus proyectos", "Si es IP") se validan
/// además contra la base de datos con <see cref="AccesoRecursos"/>.
/// </summary>
public static class Politicas
{
    public const string GestionarUsuarios = nameof(GestionarUsuarios);
    public const string EditarProgramas = nameof(EditarProgramas);
    public const string AsignarCoordinadores = nameof(AsignarCoordinadores);
    public const string VincularInvestigadores = nameof(VincularInvestigadores);
    public const string GestionarAliadosConvocatorias = nameof(GestionarAliadosConvocatorias);
    public const string CrearPropuestas = nameof(CrearPropuestas);
    public const string EditarPropuestas = nameof(EditarPropuestas);
    public const string RevisarPropuestas = nameof(RevisarPropuestas);
    public const string AvalarAprobarProyectos = nameof(AvalarAprobarProyectos);
    public const string VerProyectos = nameof(VerProyectos);
    public const string UsarAsistente = nameof(UsarAsistente);
    public const string GestionarHitos = nameof(GestionarHitos);
    public const string RegistrarAvances = nameof(RegistrarAvances);
    public const string ValidarAvances = nameof(ValidarAvances);
    public const string GestionarDocumentos = nameof(GestionarDocumentos);
    public const string VerTableroCentro = nameof(VerTableroCentro);
    public const string VerNotificaciones = nameof(VerNotificaciones);

    public static readonly IReadOnlyDictionary<string, string[]> Matriz = new Dictionary<string, string[]>
    {
        [GestionarUsuarios] = [Administrador],
        [EditarProgramas] = [Administrador, Director],
        [AsignarCoordinadores] = [Administrador, Director],
        [VincularInvestigadores] = [Administrador, Director, Coordinador],
        [GestionarAliadosConvocatorias] = [Administrador, Director, Coordinador],
        [CrearPropuestas] = [Administrador, Director, Coordinador, Investigador],
        [EditarPropuestas] = [Administrador, Director, Coordinador, Investigador],
        [RevisarPropuestas] = [Director, Coordinador],
        [AvalarAprobarProyectos] = [Administrador, Director],
        [VerProyectos] = [Administrador, Director, Coordinador, Investigador],
        [UsarAsistente] = [Administrador, Director],
        [GestionarHitos] = [Administrador, Director, Coordinador, Investigador],
        [RegistrarAvances] = [Administrador, Director, Coordinador, Investigador],
        [ValidarAvances] = [Director, Coordinador, Investigador],
        [GestionarDocumentos] = [Administrador, Director, Coordinador, Investigador],
        [VerTableroCentro] = [Administrador, Director],
        [VerNotificaciones] = [Administrador, Director, Coordinador, Investigador, Externo],
    };
}
