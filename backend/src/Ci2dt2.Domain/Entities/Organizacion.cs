namespace Ci2dt2.Domain.Entities;

// M02 Organización
public class Programa
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string? Descripcion { get; set; }
}

public class ObjetivoEspecifico
{
    public int Id { get; set; }
    public int ProgramaId { get; set; }
    public string Descripcion { get; set; } = "";
    public int Orden { get; set; }

    public Programa? Programa { get; set; }
}

public class ProgramaIntegrante
{
    public int Id { get; set; }
    public int ProgramaId { get; set; }
    public int UsuarioId { get; set; }
    public string RolEnPrograma { get; set; } = "";
    public DateOnly FechaInicio { get; set; }
    public DateOnly? FechaFin { get; set; }

    public Programa? Programa { get; set; }
    public Usuario? Usuario { get; set; }
}
