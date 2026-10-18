namespace Ci2dt2.Domain.Entities;

// M02 Organización del Centro
public class Programa : EntidadAuditable
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string? Descripcion { get; set; }
}

public class ObjetivoEspecifico : EntidadAuditable
{
    public int Id { get; set; }
    public int ProgramaId { get; set; }
    public string? Descripcion { get; set; }
    public int? Orden { get; set; }

    public Programa? Programa { get; set; }
}

public class ProgramaIntegrante : EntidadAuditable
{
    public int Id { get; set; }
    public int ProgramaId { get; set; }
    public int UsuarioId { get; set; }

    /// <summary>Coordinador | Investigador</summary>
    public string RolEnPrograma { get; set; } = "";
    public DateOnly? FechaInicio { get; set; }
    public DateOnly? FechaFin { get; set; }

    public Programa? Programa { get; set; }
    public Usuario? Usuario { get; set; }
}

public class GrupoInvestigacion : EntidadAuditable
{
    public int Id { get; set; }
    public int ProgramaId { get; set; }
    public string Nombre { get; set; } = "";
    public string? CodigoGrupLac { get; set; }
    public string? CategoriaMinciencias { get; set; }

    public Programa? Programa { get; set; }
}
