using Ci2dt2.Application.Avances;
using Ci2dt2.Application.Common;
using Ci2dt2.Application.Convocatorias;
using Ci2dt2.Application.Cronogramas;
using Ci2dt2.Application.Importacion;
using Ci2dt2.Application.Proyectos;
using Ci2dt2.Application.Publico;
using Ci2dt2.Application.Tablero;

namespace Ci2dt2.UnitTests;

/// <summary>Reglas de negocio de los Sprints 3 y 4.</summary>
public class Sprint34ReglasTests
{
    [Theory]
    [InlineData(1, 2, true)]   // En elaboración → En revisión
    [InlineData(3, 2, true)]   // Devuelto → En revisión
    [InlineData(2, 3, true)]   // En revisión → Devuelto
    [InlineData(2, 4, true)]   // En revisión → Presentado (aval)
    [InlineData(4, 5, true)]   // Presentado → Aprobado
    [InlineData(4, 8, true)]   // Presentado → No aprobado
    [InlineData(5, 6, true)]   // Aprobado → En ejecución
    [InlineData(1, 6, false)]  // En elaboración → En ejecución (HU-12 CA-5)
    [InlineData(1, 4, false)]
    [InlineData(8, 5, false)]  // No aprobado es final
    [InlineData(2, 5, false)]
    public void HU12_maquina_de_estados(int desde, int hacia, bool permitida)
    {
        Assert.Equal(permitida, MaquinaEstadosProyecto.EsPermitida(desde, hacia));
        if (!permitida)
            Assert.Equal("Transición de estado no permitida.",
                Assert.Throws<ReglaNegocioException>(() => MaquinaEstadosProyecto.Validar(desde, hacia)).Message);
    }

    [Fact]
    public void HU11_devolver_exige_observaciones_de_20_caracteres()
    {
        var v = new RevisionRequestValidator();
        Assert.False(v.Validate(new RevisionRequest("Devuelve", null)).IsValid);
        Assert.False(v.Validate(new RevisionRequest("Devuelve", "Corta")).IsValid);
        Assert.True(v.Validate(new RevisionRequest("Devuelve", "Observaciones suficientes para devolver")).IsValid);
        Assert.True(v.Validate(new RevisionRequest("Aprueba", null)).IsValid);
        Assert.False(v.Validate(new RevisionRequest("Otra", null)).IsValid);
    }

    [Fact]
    public void HU15_estado_de_la_convocatoria_por_fechas()
    {
        var hoy = new DateOnly(2026, 11, 7);
        Assert.Equal("Próxima", EstadosConvocatoria.Calcular(hoy.AddDays(1), hoy.AddDays(30), hoy));
        Assert.Equal("Abierta", EstadosConvocatoria.Calcular(hoy.AddDays(-1), hoy, hoy));
        Assert.Equal("Cerrada", EstadosConvocatoria.Calcular(hoy.AddDays(-30), hoy.AddDays(-1), hoy));
        Assert.Equal("Abierta", EstadosConvocatoria.Calcular(null, null, hoy));
    }

    [Fact]
    public void HU15_HU16_validaciones()
    {
        var conv = new ConvocatoriaRequestValidator();
        Assert.False(conv.Validate(new ConvocatoriaRequest("Convocatoria", null, 1, null, "VIP", null,
            new DateOnly(2026, 12, 1), new DateOnly(2026, 11, 1), null, null)).IsValid);
        Assert.True(conv.Validate(new ConvocatoriaRequest("Convocatoria", null, 1, null, "VIP", "https://vip.ucaldas.edu.co",
            new DateOnly(2026, 11, 1), new DateOnly(2026, 12, 1), "COP", 1000)).IsValid);

        var post = new PostulacionRequestValidator();
        Assert.False(post.Validate(new PostulacionRequest(1, null, null, null, 1000, "USD", null, null, null, null, null)).IsValid);
        Assert.False(post.Validate(new PostulacionRequest(1, null, null, null, null, null, null, "Ganada", null, null, null)).IsValid);
        Assert.True(post.Validate(new PostulacionRequest(1, null, null, null, 1000, "USD", 4000, "Pendiente", null, "Banco de elegibles", null)).IsValid);
    }

    [Fact]
    public void HU17_HU19_HU20_validaciones()
    {
        var hito = new HitoRequestValidator();
        Assert.False(hito.Validate(new HitoRequest(1, "Entregable", new DateOnly(2027, 1, 1), 5, 61, null)).IsValid);
        Assert.True(hito.Validate(new HitoRequest(1, "Entregable", new DateOnly(2027, 1, 1), 5, null, null)).IsValid);

        var avance = new AvanceRequestValidator();
        Assert.False(avance.Validate(new AvanceRequest("Avance", null, 101, null, null)).IsValid);
        Assert.True(avance.Validate(new AvanceRequest("Avance", null, 50, null, Guid.NewGuid())).IsValid);

        var validacion = new ValidacionRequestValidator();
        Assert.False(validacion.Validate(new ValidacionRequest("Rechazado", " ")).IsValid);
        Assert.True(validacion.Validate(new ValidacionRequest("Validado", null)).IsValid);
    }

    [Theory]
    [InlineData("Ciudades Inteligentes", "ciudades-inteligentes")]
    [InlineData("Inteligencia Artificial, Modelado y Simulación", "inteligencia-artificial-modelado-y-simulacion")]
    [InlineData("Bioingeniería", "bioingenieria")]
    public void HU26_slug_del_programa(string nombre, string esperado) => Assert.Equal(esperado, PortalQueries.Slug(nombre));

    [Fact]
    public void HU24_tasa_de_aprobacion()
    {
        Assert.Null(TableroQueries.TasaAprobacion(0, 0));
        Assert.Equal(66.7m, TableroQueries.TasaAprobacion(2, 3));
    }

    [Fact]
    public void HU14_lector_csv_con_comillas_y_separador()
    {
        var filas = ImportadorProyectos.LeerCsv("﻿a;b;c\r\n\"x;y\";\"con \"\"comillas\"\"\";z\n");
        Assert.Equal(2, filas.Count);
        Assert.Equal(["x;y", "con \"comillas\"", "z"], filas[1]);
        Assert.Equal(ImportadorProyectos.Columnas.Length, ImportadorProyectos.LeerCsv(ImportadorProyectos.Plantilla())[1].Length);
    }
}
