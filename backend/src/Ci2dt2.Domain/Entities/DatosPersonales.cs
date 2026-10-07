namespace Ci2dt2.Domain.Entities;

// M14 Visitas y Datos Personales (solo política y consentimientos en el MVP)
public class PoliticaTratamientoDatos
{
    public int Id { get; set; }
    public string Version { get; set; } = "";
    public string? TextoUrl { get; set; }
    public string? HashTexto { get; set; }
    public DateOnly VigenteDesde { get; set; }
    public DateOnly? VigenteHasta { get; set; }
}

public class ConsentimientoDatos
{
    public long Id { get; set; }
    public int PoliticaId { get; set; }
    public int? UsuarioId { get; set; }
    public string? Correo { get; set; }
    public string? Contexto { get; set; }
    public bool Aceptado { get; set; }
    public DateTime FechaAceptacion { get; set; }
    public string? IpOrigen { get; set; }
    public string? HashEvidencia { get; set; }

    public PoliticaTratamientoDatos? Politica { get; set; }
    public Usuario? Usuario { get; set; }
}
