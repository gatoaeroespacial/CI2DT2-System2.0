namespace Ci2dt2.Domain.Entities;

// M11 Entorno empresarial
public class TipoAliado : EntidadAuditable
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
}

public class Aliado : EntidadAuditable
{
    public int Id { get; set; }
    public int TipoAliadoId { get; set; }
    public string Nombre { get; set; } = "";
    public string? Nit { get; set; }
    public string? Pais { get; set; }
    public string? Ciudad { get; set; }
    public string? Sector { get; set; }
    public string? RepresentanteLegal { get; set; }
    public string Email { get; set; } = "";

    public TipoAliado? TipoAliado { get; set; }
}

public class ProgramaAliado : EntidadAuditable
{
    public int ProgramaId { get; set; }
    public int AliadoId { get; set; }

    public Programa? Programa { get; set; }
    public Aliado? Aliado { get; set; }
}
