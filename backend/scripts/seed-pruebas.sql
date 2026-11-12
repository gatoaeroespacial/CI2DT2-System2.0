/*
  Datos de prueba para desarrollo y demostraciones (CI²DT²). NO ejecutar con datos reales.
  Requisito: la API ya arrancó una vez (migraciones y catálogos semilla aplicados).
  Se puede ejecutar varias veces: si los datos ya existen, no hace nada.
  Contraseña de todos los usuarios de prueba: Prueba12345
  Los NIT, correos y nombres de personas son ficticios.

  Cubre todo el ciclo de vida (HU-08 a HU-20): un proyecto por cada estado, propuestas listas para
  enviar, en revisión (etapa Coordinador y Director), devueltas y avaladas; proyectos aprobados con
  y sin hitos; proyectos en ejecución con hitos completados, en curso, próximos y vencidos; avances
  pendientes, validados por el IP, validados y rechazados; convocatorias abiertas, próximas y cerradas.
*/
SET NOCOUNT ON;
SET XACT_ABORT ON;
SET QUOTED_IDENTIFIER ON; -- requerido por los índices filtrados
SET ANSI_NULLS ON;

IF EXISTS (SELECT 1 FROM Usuarios WHERE Email = 'director@pruebas.ci2dt2.test')
BEGIN
    PRINT 'Los datos de prueba ya existen.';
    RETURN;
END;

BEGIN TRANSACTION;

DECLARE @hash varchar(200) = 'AQAAAAIAAYagAAAAEFbqbiGZuoeV5jJnOxGJggO/73X/VFGtDqilFN4ZPM3Yf376D5HzkyzGnNxqXoL5Og==';
DECLARE @hoy date = CAST(DATEADD(HOUR, -5, GETUTCDATE()) AS date); -- fecha de Colombia
DECLARE @ahora datetime2 = SYSUTCDATETIME();
DECLARE @a int = YEAR(@hoy);
DECLARE @a1 int = @a - 1;
DECLARE @a2 int = @a - 2;

-- ===================== Aliados (TiposAliado: 1 Universidad, 2 Empresa, 3 Entidad pública, 4 Cámara de Comercio, 5 ONG)
INSERT INTO Aliados (TipoAliadoId, Nombre, Nit, Pais, Ciudad, Sector, RepresentanteLegal, Email) VALUES
 (2, 'CHEC - Central Hidroeléctrica de Caldas', '890800128-6', 'Colombia', 'Manizales', 'Energía', 'Representante CHEC', 'contacto@chec.test'),
 (1, 'Universidad Autónoma de Manizales', '890805051-0', 'Colombia', 'Manizales', 'Educación', 'Rector UAM', 'contacto@uam.test'),
 (3, 'Ministerio de Ciencia, Tecnología e Innovación', '899999031-8', 'Colombia', 'Bogotá', 'Ciencia y tecnología', 'Ministro(a)', 'contacto@minciencias.test'),
 (1, 'Universidad Nacional de Colombia - Sede Manizales', '899999063-3', 'Colombia', 'Manizales', 'Educación', 'Vicerrector de sede', 'contacto@unal.test'),
 (4, 'Cámara de Comercio de Manizales por Caldas', '890801055-1', 'Colombia', 'Manizales', 'Gremial', 'Presidente ejecutivo', 'contacto@ccm.test'),
 (5, 'SwissContact', NULL, 'Suiza', 'Zúrich', 'Cooperación internacional', 'Director país', 'contacto@swisscontact.test'),
 (3, 'Cenicafé', '860007538-2', 'Colombia', 'Manizales', 'Investigación cafetera', 'Director Cenicafé', 'contacto@cenicafe.test'),
 (2, 'BIOS - Centro de Bioinformática y Biología Computacional', '900145678-0', 'Colombia', 'Manizales', 'Bioinformática', 'Director BIOS', 'contacto@bios.test'),
 (1, 'UNISARC', '891408261-1', 'Colombia', 'Santa Rosa de Cabal', 'Educación', 'Rector UNISARC', 'contacto@unisarc.test');

DECLARE @chec int = (SELECT Id FROM Aliados WHERE Nit = '890800128-6');
DECLARE @uam int = (SELECT Id FROM Aliados WHERE Nit = '890805051-0');
DECLARE @minciencias int = (SELECT Id FROM Aliados WHERE Nit = '899999031-8');
DECLARE @unal int = (SELECT Id FROM Aliados WHERE Nit = '899999063-3');
DECLARE @ccm int = (SELECT Id FROM Aliados WHERE Nit = '890801055-1');
DECLARE @swiss int = (SELECT Id FROM Aliados WHERE Nombre = 'SwissContact');
DECLARE @cenicafe int = (SELECT Id FROM Aliados WHERE Nit = '860007538-2');
DECLARE @bios int = (SELECT Id FROM Aliados WHERE Nit = '900145678-0');
DECLARE @unisarc int = (SELECT Id FROM Aliados WHERE Nit = '891408261-1');

-- ===================== Usuarios
INSERT INTO Usuarios (Email, PasswordHash, Nombres, Apellidos, TipoDocumento, NumeroDocumento, Telefono, Cargo, Dependencia, AliadoId, CvLacUrl, Orcid, TieneAcceso) VALUES
 ('director@pruebas.ci2dt2.test',        @hash, 'Carlos',    'Montoya Ríos',     'CC', '10200300', '3001110001', 'Director del Centro', 'CI²DT²', NULL, NULL, NULL, 1),
 ('coord.biotec@pruebas.ci2dt2.test',    @hash, 'Ana',       'Gómez Arango',     'CC', '30400500', '3001110002', 'Docente titular', 'Facultad de Ciencias Agropecuarias', NULL, 'https://scienti.minciencias.gov.co/cvlac/visualizador/generarCurriculoCv.do?cod_rh=0000000001', '0000-0002-1825-0097', 1),
 ('coord.bioing@pruebas.ci2dt2.test',    @hash, 'Felipe',    'Arango Toro',      'CC', '75100200', '3001110010', 'Docente asociado', 'Facultad de Ingenierías', NULL, NULL, '0000-0001-5109-3700', 1),
 ('coord.ia@pruebas.ci2dt2.test',        @hash, 'Jorge',     'Salazar López',    'CC', '75600700', '3001110003', 'Docente asociado', 'Facultad de Ingenierías', NULL, NULL, '0000-0002-1694-233X', 1),
 ('coord.sistemas@pruebas.ci2dt2.test',  @hash, 'Natalia',   'Rendón Gil',       'CC', '30500600', '3001110011', 'Docente titular', 'Facultad de Ciencias Exactas', NULL, NULL, NULL, 1),
 ('coord.ciudades@pruebas.ci2dt2.test',  @hash, 'Andrés',    'Quintero Mejía',   'CC', '75200300', '3001110012', 'Docente asociado', 'Facultad de Ingenierías', NULL, NULL, NULL, 1),
 ('inv.lucia@pruebas.ci2dt2.test',       @hash, 'Lucía',     'Herrera Castaño',  'CC', '24800900', '3001110004', 'Docente investigadora', 'Facultad de Ciencias Exactas', NULL, 'https://scienti.minciencias.gov.co/cvlac/visualizador/generarCurriculoCv.do?cod_rh=0000000002', NULL, 1),
 ('inv.mateo@pruebas.ci2dt2.test',       @hash, 'Mateo',     'Ospina Valencia',  'CC', '10901100', '3001110005', 'Investigador', 'Facultad de Ingenierías', NULL, NULL, NULL, 1),
 ('inv.sara@pruebas.ci2dt2.test',        @hash, 'Sara',      'Cardona Mejía',    'CE', 'E1234567', '3001110006', 'Investigadora posdoctoral', 'Facultad de Ciencias Agropecuarias', NULL, NULL, '0000-0003-1415-9269', 1),
 ('inv.diego@pruebas.ci2dt2.test',       @hash, 'Diego',     'Marín Ocampo',     'CC', '10301300', '3001110013', 'Docente investigador', 'Facultad de Ingenierías', NULL, NULL, NULL, 1),
 ('inv.camila@pruebas.ci2dt2.test',      @hash, 'Camila',    'Restrepo Duque',   'CC', '30601400', '3001110014', 'Investigadora', 'Facultad de Ciencias Exactas', NULL, NULL, NULL, 1),
 ('inv.julian@pruebas.ci2dt2.test',      @hash, 'Julián',    'Giraldo Patiño',   'CC', '10401500', '3001110015', 'Investigador', 'Facultad de Ingenierías', NULL, NULL, NULL, 1),
 ('inv.valeria@pruebas.ci2dt2.test',     @hash, 'Valeria',   'López Hoyos',      'CC', '30701600', '3001110016', 'Docente investigadora', 'Facultad de Ciencias Agropecuarias', NULL, NULL, NULL, 1),
 ('inv.tomas@pruebas.ci2dt2.test',       @hash, 'Tomás',     'Zuluaga Henao',    'CC', '10501700', '3001110017', 'Estudiante de maestría', 'Facultad de Ingenierías', NULL, NULL, NULL, 1),
 ('inv.inactivo@pruebas.ci2dt2.test',    @hash, 'Pedro',     'Ramírez Duque',    'CC', '10301200', '3001110007', 'Investigador', 'Facultad de Ciencias para la Salud', NULL, NULL, NULL, 0),
 ('externo.chec@pruebas.ci2dt2.test',    @hash, 'Valentina', 'Marín Zuluaga',    'CC', '30501300', '3001110008', 'Líder de innovación', 'CHEC', @chec, NULL, NULL, 1),
 ('externo.uam@pruebas.ci2dt2.test',     @hash, 'Ricardo',   'Peña Salazar',     'CC', '75301800', '3001110018', 'Director de investigaciones', 'UAM', @uam, NULL, NULL, 1);

DECLARE @director int = (SELECT Id FROM Usuarios WHERE Email = 'director@pruebas.ci2dt2.test');
DECLARE @coordBio int = (SELECT Id FROM Usuarios WHERE Email = 'coord.biotec@pruebas.ci2dt2.test');
DECLARE @coordBing int = (SELECT Id FROM Usuarios WHERE Email = 'coord.bioing@pruebas.ci2dt2.test');
DECLARE @coordIa int = (SELECT Id FROM Usuarios WHERE Email = 'coord.ia@pruebas.ci2dt2.test');
DECLARE @coordSis int = (SELECT Id FROM Usuarios WHERE Email = 'coord.sistemas@pruebas.ci2dt2.test');
DECLARE @coordCiu int = (SELECT Id FROM Usuarios WHERE Email = 'coord.ciudades@pruebas.ci2dt2.test');
DECLARE @lucia int = (SELECT Id FROM Usuarios WHERE Email = 'inv.lucia@pruebas.ci2dt2.test');
DECLARE @mateo int = (SELECT Id FROM Usuarios WHERE Email = 'inv.mateo@pruebas.ci2dt2.test');
DECLARE @sara int = (SELECT Id FROM Usuarios WHERE Email = 'inv.sara@pruebas.ci2dt2.test');
DECLARE @diego int = (SELECT Id FROM Usuarios WHERE Email = 'inv.diego@pruebas.ci2dt2.test');
DECLARE @camila int = (SELECT Id FROM Usuarios WHERE Email = 'inv.camila@pruebas.ci2dt2.test');
DECLARE @julian int = (SELECT Id FROM Usuarios WHERE Email = 'inv.julian@pruebas.ci2dt2.test');
DECLARE @valeria int = (SELECT Id FROM Usuarios WHERE Email = 'inv.valeria@pruebas.ci2dt2.test');
DECLARE @tomas int = (SELECT Id FROM Usuarios WHERE Email = 'inv.tomas@pruebas.ci2dt2.test');
DECLARE @pedro int = (SELECT Id FROM Usuarios WHERE Email = 'inv.inactivo@pruebas.ci2dt2.test');
DECLARE @extChec int = (SELECT Id FROM Usuarios WHERE Email = 'externo.chec@pruebas.ci2dt2.test');
DECLARE @extUam int = (SELECT Id FROM Usuarios WHERE Email = 'externo.uam@pruebas.ci2dt2.test');

-- Roles (1 Admin, 2 Director, 3 Coordinador, 4 Investigador, 5 Externo). Los coordinadores también investigan.
INSERT INTO UsuarioRoles (UsuarioId, RolId) VALUES
 (@director, 2),
 (@coordBio, 3), (@coordBio, 4), (@coordBing, 3), (@coordBing, 4), (@coordIa, 3), (@coordIa, 4),
 (@coordSis, 3), (@coordSis, 4), (@coordCiu, 3), (@coordCiu, 4),
 (@lucia, 4), (@mateo, 4), (@sara, 4), (@diego, 4), (@camila, 4), (@julian, 4), (@valeria, 4), (@tomas, 4), (@pedro, 4),
 (@extChec, 5), (@extUam, 5);

-- ===================== Programas (1 Biotecnología y Agroindustria, 2 Bioingeniería, 3 IA, 4 Sistemas Computacionales y Bioinformática, 5 Ciudades Inteligentes)
UPDATE Programas SET Descripcion = N'Investigación y desarrollo de bioproductos, bioprocesos y soluciones para las cadenas agroindustriales de la región.' WHERE Id = 1;
UPDATE Programas SET Descripcion = N'Desarrollo de dispositivos, materiales y tecnologías biomédicas aplicadas a la salud y la rehabilitación.' WHERE Id = 2;
UPDATE Programas SET Descripcion = N'Aplicación de inteligencia artificial, modelado matemático y simulación a problemas productivos y sociales.' WHERE Id = 3;
UPDATE Programas SET Descripcion = N'Computación de alto desempeño, ingeniería de software y bioinformática para la investigación científica.' WHERE Id = 4;
UPDATE Programas SET Descripcion = N'Soluciones tecnológicas para la movilidad, la energía, el agua y la calidad de vida en las ciudades.' WHERE Id = 5;

INSERT INTO ObjetivosEspecificos (ProgramaId, Descripcion, Orden) VALUES
 (1, N'Desarrollar bioproductos a partir de residuos agroindustriales de la región.', 1),
 (1, N'Fortalecer la transferencia tecnológica al sector cafetero.', 2),
 (1, N'Formar talento humano en bioprocesos.', 3),
 (2, N'Diseñar dispositivos biomédicos de bajo costo.', 1),
 (2, N'Validar tecnologías de rehabilitación con usuarios reales.', 2),
 (3, N'Aplicar modelos de inteligencia artificial a problemas productivos del departamento.', 1),
 (3, N'Formar talento humano en simulación y analítica de datos.', 2),
 (4, N'Consolidar capacidades en bioinformática y computación de alto desempeño.', 1),
 (4, N'Desarrollar software científico de código abierto.', 2),
 (5, N'Proponer soluciones tecnológicas para la movilidad y la energía urbanas.', 1),
 (5, N'Monitorear variables ambientales con redes de sensores.', 2);

-- Integrantes de programas: un Coordinador vigente por programa; investigadores vigentes e históricos.
INSERT INTO ProgramaIntegrantes (ProgramaId, UsuarioId, RolEnPrograma, FechaInicio, FechaFin) VALUES
 (1, @coordBio,  'Coordinador',  DATEADD(YEAR, -2, @hoy), NULL),
 (2, @coordBing, 'Coordinador',  DATEADD(YEAR, -2, @hoy), NULL),
 (3, @coordIa,   'Coordinador',  DATEADD(YEAR, -2, @hoy), NULL),
 (4, @coordSis,  'Coordinador',  DATEADD(YEAR, -1, @hoy), NULL),
 (5, @coordCiu,  'Coordinador',  DATEADD(YEAR, -1, @hoy), NULL),
 (1, @sara,    'Investigador', DATEADD(YEAR, -2, @hoy), NULL),
 (1, @valeria, 'Investigador', DATEADD(MONTH, -14, @hoy), NULL),
 (2, @diego,   'Investigador', DATEADD(YEAR, -2, @hoy), NULL),
 (2, @tomas,   'Investigador', DATEADD(MONTH, -10, @hoy), NULL),
 (3, @lucia,   'Investigador', DATEADD(YEAR, -2, @hoy), NULL),
 (3, @mateo,   'Investigador', DATEADD(MONTH, -20, @hoy), NULL),
 (3, @valeria, 'Investigador', DATEADD(MONTH, -8, @hoy), NULL),
 (4, @camila,  'Investigador', DATEADD(YEAR, -1, @hoy), NULL),
 (5, @mateo,   'Investigador', DATEADD(MONTH, -12, @hoy), NULL),
 (5, @julian,  'Investigador', DATEADD(YEAR, -2, @hoy), NULL),
 -- Históricos (retirados)
 (2, @pedro,   'Investigador', DATEADD(YEAR, -3, @hoy), DATEADD(MONTH, -2, @hoy)),
 (4, @tomas,   'Investigador', DATEADD(YEAR, -2, @hoy), DATEADD(MONTH, -11, @hoy));

INSERT INTO ProgramaAliados (ProgramaId, AliadoId) VALUES
 (1, @uam), (1, @ccm), (1, @cenicafe), (2, @unal), (2, @unisarc), (3, @chec), (3, @minciencias),
 (4, @unal), (4, @bios), (5, @chec), (5, @swiss);

-- ===================== Convocatorias (TiposConvocatoria: 1 Interna, 2 Nacional, 3 Internacional, 4 Minciencias FJC, 5 SGR, 6 Conjunta)
INSERT INTO Convocatorias (Nombre, Numero, TipoConvocatoriaId, EntidadConvocanteId, Plataforma, Url, FechaApertura, FechaCierre, Moneda, MontoMaximo) VALUES
 (N'Convocatoria interna de investigación VIP ' + CAST(@a AS nvarchar(4)), 'VIP-01-' + CAST(@a AS varchar(4)), 1, NULL, 'Portal VIP', 'https://vip.ucaldas.test/convocatoria', DATEADD(DAY, -30, @hoy), DATEADD(DAY, 20, @hoy), 'COP', 60000000),
 (N'Minciencias SGR - CTeI regional', '45-' + CAST(@a AS varchar(4)), 5, @minciencias, 'MGA Web', 'https://minciencias.test/convocatorias/sgr', DATEADD(DAY, -60, @hoy), DATEADD(DAY, 45, @hoy), 'COP', 1500000000),
 (N'Retos de innovación CHEC ' + CAST(@a AS nvarchar(4)), 'CHEC-R-' + CAST(@a AS varchar(4)), 2, @chec, 'Plataforma del otorgante', 'https://chec.test/retos', DATEADD(DAY, -120, @hoy), DATEADD(DAY, -5, @hoy), 'COP', 250000000),
 (N'Fondo de cooperación SwissContact', 'SC-' + CAST(@a AS varchar(4)), 3, @swiss, 'Plataforma del otorgante', 'https://swisscontact.test/fondo', DATEADD(DAY, 15, @hoy), DATEADD(DAY, 75, @hoy), 'USD', 80000),
 (N'Minciencias Fondo FJC - Proyectos de I+D', '12-' + CAST(@a AS varchar(4)), 4, @minciencias, 'SIGP', 'https://minciencias.test/convocatorias/fjc', DATEADD(DAY, -10, @hoy), DATEADD(DAY, 30, @hoy), 'COP', 400000000);

DECLARE @cVip int = (SELECT Id FROM Convocatorias WHERE Numero = 'VIP-01-' + CAST(@a AS varchar(4)));
DECLARE @cSgr int = (SELECT Id FROM Convocatorias WHERE Numero = '45-' + CAST(@a AS varchar(4)));
DECLARE @cChec int = (SELECT Id FROM Convocatorias WHERE Numero = 'CHEC-R-' + CAST(@a AS varchar(4)));
DECLARE @cFjc int = (SELECT Id FROM Convocatorias WHERE Numero = '12-' + CAST(@a AS varchar(4)));

-- ===================== Proyectos
-- Estados: 1 En elaboración, 2 En revisión, 3 Devuelto, 4 Presentado, 5 Aprobado, 6 En ejecución, 7 Finalizado, 8 No aprobado, 9 Suspendido
-- Tipos: 1 I+D+i, 2 Reto Empresarial (exige 3 hitos Sprint y 1 MVP), 3 Consultoría, 4 Tesis
DECLARE @c varchar(10) = 'CI2DT2-';
INSERT INTO Proyectos (Codigo, Titulo, Resumen, ProgramaId, TipoProyectoId, EstadoId, InvestigadorPrincipalId, FechaInicio, FechaFin, PorcentajeAvance, Moneda, PresupuestoTotal, TasaCambio, PresupuestoTotalCop) VALUES
 (@c + CAST(@a1 AS varchar(4)) + '-001', N'Bioinsumos a partir de pulpa de café para cultivos de la región', N'Aprovechamiento de residuos del beneficio del café para producir bioinsumos agrícolas validados en campo.', 1, 1, 6, @sara, DATEADD(MONTH, -8, @hoy), DATEADD(MONTH, 10, @hoy), 20, 'COP', 180000000, NULL, 180000000),
 (@c + CAST(@a AS varchar(4)) + '-001', N'Predicción de demanda energética con aprendizaje automático', N'Modelos de pronóstico de carga para la red de distribución de Caldas, en alianza con CHEC.', 3, 2, 5, @lucia, DATEADD(MONTH, 1, @hoy), DATEADD(MONTH, 13, @hoy), 0, 'COP', 200000000, NULL, 200000000),
 (@c + CAST(@a AS varchar(4)) + '-002', N'Gemelo digital para la movilidad urbana de Manizales', N'Simulación de tráfico y transporte público para apoyar decisiones de movilidad.', 3, 1, 1, @mateo, NULL, NULL, 0, 'USD', 50000, 4000, 200000000),
 (@c + CAST(@a AS varchar(4)) + '-003', N'Monitoreo de calidad del aire con sensores de bajo costo', N'Red de sensores IoT para medir material particulado en zonas urbanas de Manizales.', 5, 1, 1, @mateo, DATEADD(MONTH, 2, @hoy), DATEADD(MONTH, 20, @hoy), 0, 'COP', 95000000, NULL, 95000000),
 (@c + CAST(@a AS varchar(4)) + '-004', N'Plataforma bioinformática para el análisis de genomas de café', N'Flujo de trabajo reproducible para ensamblar y anotar genomas de variedades de café.', 4, 1, 2, @camila, DATEADD(MONTH, 3, @hoy), DATEADD(MONTH, 27, @hoy), 0, 'COP', 320000000, NULL, 320000000),
 (@c + CAST(@a AS varchar(4)) + '-005', N'Prótesis de mano de bajo costo con impresión 3D', N'Diseño y validación de una prótesis mioeléctrica de bajo costo fabricada con impresión 3D.', 2, 1, 2, @diego, DATEADD(MONTH, 2, @hoy), DATEADD(MONTH, 18, @hoy), 0, 'COP', 140000000, NULL, 140000000),
 (@c + CAST(@a AS varchar(4)) + '-006', N'Analítica de datos para la calidad del café de especialidad', N'Consultoría para modelar la relación entre variables de proceso y la calidad en taza.', 1, 3, 3, @valeria, DATEADD(MONTH, 1, @hoy), DATEADD(MONTH, 7, @hoy), 0, 'COP', 45000000, NULL, 45000000),
 (@c + CAST(@a AS varchar(4)) + '-007', N'Sistema de alerta temprana de calidad del agua con sensores', N'Red de sensores y tablero de control para vigilar la calidad del agua en acueductos rurales.', 5, 1, 4, @julian, DATEADD(MONTH, 3, @hoy), DATEADD(MONTH, 27, @hoy), 0, 'COP', 900000000, NULL, 900000000),
 (@c + CAST(@a1 AS varchar(4)) + '-002', N'Modelos de IA para el diagnóstico temprano de enfermedades del café', N'Visión por computador sobre imágenes de hojas para detectar roya y otras enfermedades.', 3, 1, 6, @lucia, DATEADD(MONTH, -10, @hoy), DATEADD(MONTH, 8, @hoy), 60, 'COP', 210000000, NULL, 210000000),
 (@c + CAST(@a1 AS varchar(4)) + '-003', N'Mantenimiento predictivo de transformadores para CHEC', N'Reto empresarial: modelo de mantenimiento predictivo a partir de datos de sensores de transformadores.', 3, 2, 6, @mateo, DATEADD(MONTH, -4, @hoy), DATEADD(MONTH, 4, @hoy), 45, 'COP', 160000000, NULL, 160000000),
 (@c + CAST(@a2 AS varchar(4)) + '-001', N'Biorrefinería de residuos de plátano', N'Obtención de bioproductos de valor agregado a partir de residuos de la cadena del plátano.', 1, 1, 7, @sara, DATEADD(YEAR, -2, @hoy), DATEADD(MONTH, -2, @hoy), 100, 'COP', 150000000, NULL, 150000000),
 (@c + CAST(@a1 AS varchar(4)) + '-004', N'Dispositivo de rehabilitación de rodilla asistido', N'Dispositivo de rehabilitación pasiva con control de rango de movimiento.', 2, 1, 8, @tomas, NULL, NULL, 0, 'COP', 70000000, NULL, 70000000),
 (@c + CAST(@a1 AS varchar(4)) + '-005', N'Movilidad eléctrica para el transporte público', N'Estudio de factibilidad de buses eléctricos para rutas de Manizales.', 5, 1, 9, @julian, DATEADD(MONTH, -12, @hoy), DATEADD(MONTH, 6, @hoy), 30, 'USD', 30000, 4100, 123000000),
 (@c + CAST(@a AS varchar(4)) + '-008', N'Redes neuronales para la predicción de estructuras de proteínas', N'Tesis doctoral sobre arquitecturas de aprendizaje profundo para el plegamiento de proteínas.', 4, 4, 5, @camila, DATEADD(MONTH, 1, @hoy), DATEADD(MONTH, 37, @hoy), 0, 'COP', 60000000, NULL, 60000000);

DECLARE @pBioinsumos int = (SELECT Id FROM Proyectos WHERE Codigo = @c + CAST(@a1 AS varchar(4)) + '-001');
DECLARE @pEnergia int = (SELECT Id FROM Proyectos WHERE Codigo = @c + CAST(@a AS varchar(4)) + '-001');
DECLARE @pGemelo int = (SELECT Id FROM Proyectos WHERE Codigo = @c + CAST(@a AS varchar(4)) + '-002');
DECLARE @pAire int = (SELECT Id FROM Proyectos WHERE Codigo = @c + CAST(@a AS varchar(4)) + '-003');
DECLARE @pGenomas int = (SELECT Id FROM Proyectos WHERE Codigo = @c + CAST(@a AS varchar(4)) + '-004');
DECLARE @pProtesis int = (SELECT Id FROM Proyectos WHERE Codigo = @c + CAST(@a AS varchar(4)) + '-005');
DECLARE @pCafe int = (SELECT Id FROM Proyectos WHERE Codigo = @c + CAST(@a AS varchar(4)) + '-006');
DECLARE @pAgua int = (SELECT Id FROM Proyectos WHERE Codigo = @c + CAST(@a AS varchar(4)) + '-007');
DECLARE @pRoya int = (SELECT Id FROM Proyectos WHERE Codigo = @c + CAST(@a1 AS varchar(4)) + '-002');
DECLARE @pTransfo int = (SELECT Id FROM Proyectos WHERE Codigo = @c + CAST(@a1 AS varchar(4)) + '-003');
DECLARE @pPlatano int = (SELECT Id FROM Proyectos WHERE Codigo = @c + CAST(@a2 AS varchar(4)) + '-001');
DECLARE @pRodilla int = (SELECT Id FROM Proyectos WHERE Codigo = @c + CAST(@a1 AS varchar(4)) + '-004');
DECLARE @pBuses int = (SELECT Id FROM Proyectos WHERE Codigo = @c + CAST(@a1 AS varchar(4)) + '-005');
DECLARE @pProteinas int = (SELECT Id FROM Proyectos WHERE Codigo = @c + CAST(@a AS varchar(4)) + '-008');

-- ===================== Equipos (el IP siempre es integrante con rol IP)
INSERT INTO ProyectoIntegrantes (ProyectoId, UsuarioId, RolEnProyecto, FechaInicio, FechaFin) VALUES
 (@pBioinsumos, @sara, 'IP', DATEADD(MONTH, -8, @hoy), NULL),
 (@pBioinsumos, @valeria, 'Co-investigador', DATEADD(MONTH, -8, @hoy), NULL),
 (@pBioinsumos, @coordBio, 'Co-investigador', DATEADD(MONTH, -8, @hoy), NULL),
 (@pBioinsumos, @tomas, 'Estudiante', DATEADD(MONTH, -8, @hoy), DATEADD(MONTH, -2, @hoy)), -- retirado
 (@pEnergia, @lucia, 'IP', @hoy, NULL),
 (@pEnergia, @mateo, 'Co-investigador', @hoy, NULL),
 (@pGemelo, @mateo, 'IP', @hoy, NULL),
 (@pAire, @mateo, 'IP', @hoy, NULL),
 (@pAire, @julian, 'Co-investigador', @hoy, NULL),
 (@pGenomas, @camila, 'IP', @hoy, NULL),
 (@pProtesis, @diego, 'IP', @hoy, NULL),
 (@pProtesis, @tomas, 'Estudiante', @hoy, NULL),
 (@pCafe, @valeria, 'IP', @hoy, NULL),
 (@pAgua, @julian, 'IP', @hoy, NULL),
 (@pAgua, @coordCiu, 'Co-investigador', @hoy, NULL),
 (@pRoya, @lucia, 'IP', DATEADD(MONTH, -10, @hoy), NULL),
 (@pRoya, @valeria, 'Co-investigador', DATEADD(MONTH, -10, @hoy), NULL),
 (@pRoya, @mateo, 'Auxiliar de campo', DATEADD(MONTH, -10, @hoy), NULL),
 (@pTransfo, @mateo, 'IP', DATEADD(MONTH, -4, @hoy), NULL),
 (@pTransfo, @lucia, 'Co-investigador', DATEADD(MONTH, -4, @hoy), NULL),
 (@pPlatano, @sara, 'IP', DATEADD(YEAR, -2, @hoy), NULL),
 (@pPlatano, @coordBio, 'Co-investigador', DATEADD(YEAR, -2, @hoy), NULL),
 (@pRodilla, @tomas, 'IP', DATEADD(MONTH, -10, @hoy), NULL),
 (@pBuses, @julian, 'IP', DATEADD(MONTH, -12, @hoy), NULL),
 (@pProteinas, @camila, 'IP', @hoy, NULL);

-- ===================== Financiación (sin aliado = Universidad de Caldas)
INSERT INTO FuentesFinanciamiento (ProyectoId, AliadoId, TipoFuente, TipoAporte, Moneda, Monto, TasaCambio, MontoCop, Estado) VALUES
 (@pBioinsumos, NULL, 'Convocatoria', 'Efectivo', 'COP', 60000000, NULL, 60000000, 'Desembolsado'),
 (@pBioinsumos, NULL, 'Contrapartida', 'Especie', 'COP', 40000000, NULL, 40000000, 'Comprometido'),
 (@pBioinsumos, @uam, 'Contrapartida', 'Especie', 'COP', 30000000, NULL, 30000000, 'Comprometido'),
 (@pBioinsumos, @cenicafe, 'Cofinanciación', 'Efectivo', 'COP', 50000000, NULL, 50000000, 'Comprometido'),
 (@pEnergia, @chec, 'Cofinanciación', 'Efectivo', 'COP', 150000000, NULL, 150000000, 'Comprometido'),
 (@pEnergia, NULL, 'Contrapartida', 'Especie', 'COP', 50000000, NULL, 50000000, 'Comprometido'),
 (@pAire, NULL, 'Convocatoria', 'Efectivo', 'COP', 80000000, NULL, 80000000, 'Estimado'),
 (@pAire, NULL, 'Contrapartida', 'Especie', 'COP', 15000000, NULL, 15000000, 'Estimado'),
 (@pGenomas, @minciencias, 'Convocatoria', 'Efectivo', 'COP', 280000000, NULL, 280000000, 'Estimado'),
 (@pGenomas, NULL, 'Contrapartida', 'Especie', 'COP', 40000000, NULL, 40000000, 'Estimado'),
 (@pProtesis, NULL, 'Convocatoria', 'Efectivo', 'COP', 100000000, NULL, 100000000, 'Estimado'),
 (@pProtesis, @unal, 'Contrapartida', 'Especie', 'COP', 30000000, NULL, 30000000, 'Estimado'),
 (@pCafe, NULL, 'Recursos propios', 'Efectivo', 'COP', 45000000, NULL, 45000000, 'Estimado'),
 (@pAgua, @minciencias, 'Convocatoria', 'Efectivo', 'COP', 800000000, NULL, 800000000, 'Comprometido'),
 (@pAgua, NULL, 'Contrapartida', 'Especie', 'COP', 100000000, NULL, 100000000, 'Comprometido'),
 (@pRoya, NULL, 'Convocatoria', 'Efectivo', 'COP', 150000000, NULL, 150000000, 'Desembolsado'),
 (@pRoya, @cenicafe, 'Cofinanciación', 'Especie', 'COP', 60000000, NULL, 60000000, 'Desembolsado'),
 (@pTransfo, @chec, 'Cofinanciación', 'Efectivo', 'COP', 160000000, NULL, 160000000, 'Desembolsado'),
 (@pPlatano, NULL, 'Recursos propios', 'Efectivo', 'COP', 150000000, NULL, 150000000, 'Desembolsado'),
 (@pBuses, @swiss, 'Cofinanciación', 'Efectivo', 'USD', 30000, 4100, 123000000, 'Comprometido'),
 (@pProteinas, NULL, 'Recursos propios', 'Efectivo', 'COP', 60000000, NULL, 60000000, 'Comprometido');

-- ===================== Postulaciones (HU-16)
INSERT INTO PropuestasConvocatoria (ProyectoId, ConvocatoriaId, FechaPresentacion, IdPostulacionExterna, CodigoMga, MontoSolicitado, Moneda, MontoSolicitadoCop, Resultado, FaseExterna, FechaResultado) VALUES
 (@pBioinsumos, @cVip, DATEADD(DAY, -25, @hoy), NULL, NULL, 60000000, 'COP', 60000000, 'Seleccionada', NULL, DATEADD(DAY, -10, @hoy)),
 (@pEnergia, @cChec, DATEADD(DAY, -40, @hoy), 'CHEC-R-017', NULL, 150000000, 'COP', 150000000, 'Seleccionada', NULL, DATEADD(DAY, -7, @hoy)),
 (@pAgua, @cSgr, DATEADD(DAY, -20, @hoy), 'SGR-2026-0458', 'MGA-2026-0458', 800000000, 'COP', 800000000, 'Seleccionada', N'Viabilización OCAD', DATEADD(DAY, -2, @hoy)),
 (@pGenomas, @cSgr, NULL, NULL, 'MGA-2026-0512', 280000000, 'COP', 280000000, 'Pendiente', N'Banco de elegibles', NULL),
 (@pAire, @cFjc, NULL, NULL, NULL, 80000000, 'COP', 80000000, 'Pendiente', NULL, NULL),
 (@pRodilla, @cVip, DATEADD(DAY, -28, @hoy), NULL, NULL, 50000000, 'COP', 50000000, 'No seleccionada', NULL, DATEADD(DAY, -12, @hoy));

-- ===================== Historial de revisiones (HU-11, HU-12). EstadoAnterior → EstadoNuevo
INSERT INTO ProyectoRevisiones (ProyectoId, RevisorId, Etapa, Decision, EstadoAnteriorId, EstadoNuevoId, Observaciones, Fecha) VALUES
 -- Prótesis: en revisión, etapa Director (el Coordinador ya aprobó)
 (@pProtesis, @coordBing, 'Coordinador', 'Aprueba', 2, 2, N'La propuesta cumple los lineamientos del programa.', DATEADD(DAY, -2, @ahora)),
 -- Analítica de café: devuelta por el Coordinador
 (@pCafe, @coordBio, 'Coordinador', 'Devuelve', 2, 3, N'Ajustar el cronograma y detallar los entregables de la consultoría para el cliente.', DATEADD(DAY, -3, @ahora)),
 -- Agua: aprobada por el Coordinador y avalada por el Director (Presentado)
 (@pAgua, @coordCiu, 'Coordinador', 'Aprueba', 2, 2, NULL, DATEADD(DAY, -30, @ahora)),
 (@pAgua, @director, 'Director', 'Avala', 2, 4, NULL, DATEADD(DAY, -25, @ahora)),
 -- Energía: avalada y aprobada (Aprobado, con cronograma)
 (@pEnergia, @coordIa, 'Coordinador', 'Aprueba', 2, 2, NULL, DATEADD(DAY, -60, @ahora)),
 (@pEnergia, @director, 'Director', 'Avala', 2, 4, NULL, DATEADD(DAY, -55, @ahora)),
 (@pEnergia, @director, 'Director', 'Aprobado', 4, 5, N'Acta de aprobación del comité de CHEC.', DATEADD(DAY, -5, @ahora)),
 -- Proteínas: avalada y aprobada
 (@pProteinas, @coordSis, 'Coordinador', 'Aprueba', 2, 2, NULL, DATEADD(DAY, -40, @ahora)),
 (@pProteinas, @director, 'Director', 'Avala', 2, 4, NULL, DATEADD(DAY, -35, @ahora)),
 (@pProteinas, @director, 'Director', 'Aprobado', 4, 5, NULL, DATEADD(DAY, -4, @ahora)),
 -- Rodilla: no aprobada
 (@pRodilla, @coordBing, 'Coordinador', 'Aprueba', 2, 2, NULL, DATEADD(DAY, -50, @ahora)),
 (@pRodilla, @director, 'Director', 'Avala', 2, 4, NULL, DATEADD(DAY, -45, @ahora)),
 (@pRodilla, @director, 'Director', 'No aprobado', 4, 8, N'La convocatoria no seleccionó la propuesta.', DATEADD(DAY, -12, @ahora)),
 -- Proyectos en ejecución: inicio de la ejecución
 (@pBioinsumos, @director, 'Director', 'Inicia ejecución', 5, 6, NULL, DATEADD(MONTH, -8, @ahora)),
 (@pRoya, @director, 'Director', 'Inicia ejecución', 5, 6, NULL, DATEADD(MONTH, -10, @ahora)),
 (@pTransfo, @director, 'Director', 'Inicia ejecución', 5, 6, NULL, DATEADD(MONTH, -4, @ahora));

-- ===================== Cronogramas e hitos (HU-17). TiposHito: 1 Entregable, 2 Informe, 3 Reunión, 4 Pago, 5 Sprint, 6 MVP
INSERT INTO Cronogramas (Nombre, ProyectoId) VALUES
 (N'Cronograma bioinsumos', @pBioinsumos), (N'Cronograma demanda energética', @pEnergia), (N'Cronograma diagnóstico de roya', @pRoya),
 (N'Cronograma mantenimiento predictivo', @pTransfo), (N'Cronograma biorrefinería', @pPlatano), (N'Cronograma movilidad eléctrica', @pBuses),
 (N'Cronograma tesis proteínas', @pProteinas);

DECLARE @crBio int = (SELECT Id FROM Cronogramas WHERE ProyectoId = @pBioinsumos);
DECLARE @crEne int = (SELECT Id FROM Cronogramas WHERE ProyectoId = @pEnergia);
DECLARE @crRoya int = (SELECT Id FROM Cronogramas WHERE ProyectoId = @pRoya);
DECLARE @crTra int = (SELECT Id FROM Cronogramas WHERE ProyectoId = @pTransfo);
DECLARE @crPla int = (SELECT Id FROM Cronogramas WHERE ProyectoId = @pPlatano);
DECLARE @crBus int = (SELECT Id FROM Cronogramas WHERE ProyectoId = @pBuses);

INSERT INTO Hitos (CronogramaId, TipoHitoId, Titulo, FechaLimite, AlertarDias, ResponsableId, Estado, FechaCompletado, Orden) VALUES
 -- Bioinsumos: completado, vencido (pendiente con fecha pasada: el motor lo marca Vencido), en curso, próximo (alerta) y futuro
 (@crBio, 3, N'Reunión de arranque con aliados', DATEADD(MONTH, -8, @hoy), 7, @sara, 'Completado', DATEADD(MONTH, -8, @hoy), 1),
 (@crBio, 2, N'Primer informe de avance', DATEADD(DAY, -3, @hoy), 7, @sara, 'Pendiente', NULL, 2),
 (@crBio, 1, N'Prototipo de bioinsumo validado en laboratorio', DATEADD(DAY, 25, @hoy), 7, @valeria, 'En curso', NULL, 3),
 (@crBio, 4, N'Segundo desembolso de Cenicafé', DATEADD(DAY, 5, @hoy), 7, @sara, 'Pendiente', NULL, 4),
 (@crBio, 2, N'Informe final', DATEADD(MONTH, 10, @hoy), 14, @sara, 'Pendiente', NULL, 5),
 -- Energía (reto empresarial aprobado): solo 2 Sprint y sin MVP → no puede iniciar ejecución (HU-17 CA-3)
 (@crEne, 5, N'Sprint 1: ingesta y limpieza de datos', DATEADD(MONTH, 2, @hoy), 7, @lucia, 'Pendiente', NULL, 1),
 (@crEne, 5, N'Sprint 2: modelos base de pronóstico', DATEADD(MONTH, 4, @hoy), 7, @mateo, 'Pendiente', NULL, 2),
 -- Roya
 (@crRoya, 1, N'Base de datos de imágenes etiquetadas', DATEADD(MONTH, -6, @hoy), 7, @valeria, 'Completado', DATEADD(MONTH, -6, @hoy), 1),
 (@crRoya, 1, N'Modelo de clasificación entrenado', DATEADD(DAY, -10, @hoy), 7, @lucia, 'Vencido', NULL, 2),
 (@crRoya, 2, N'Informe técnico de validación en finca', DATEADD(DAY, 12, @hoy), 15, @lucia, 'Pendiente', NULL, 3),
 -- Transformadores (reto empresarial en ejecución con sus hitos fijos)
 (@crTra, 5, N'Sprint 1: integración de datos de sensores', DATEADD(MONTH, -3, @hoy), 7, @mateo, 'Completado', DATEADD(MONTH, -3, @hoy), 1),
 (@crTra, 5, N'Sprint 2: modelo de detección de anomalías', DATEADD(MONTH, -1, @hoy), 7, @lucia, 'Completado', DATEADD(MONTH, -1, @hoy), 2),
 (@crTra, 5, N'Sprint 3: tablero para operadores', DATEADD(DAY, 6, @hoy), 7, @mateo, 'En curso', NULL, 3),
 (@crTra, 6, N'Entrega del MVP a CHEC', DATEADD(MONTH, 3, @hoy), 10, @mateo, 'Pendiente', NULL, 4),
 -- Biorrefinería (finalizado)
 (@crPla, 2, N'Informe final aprobado', DATEADD(MONTH, -2, @hoy), 7, @sara, 'Completado', DATEADD(MONTH, -2, @hoy), 1),
 -- Movilidad eléctrica (suspendido)
 (@crBus, 2, N'Estudio de factibilidad', DATEADD(MONTH, -1, @hoy), 7, @julian, 'Vencido', NULL, 1);

-- ===================== Avances (HU-19) y validaciones (HU-20)
INSERT INTO Avances (ProyectoId, HitoId, AutorId, Titulo, Descripcion, PorcentajeAvance, OrigenRegistro, FechaCaptura, EstadoValidacion) VALUES
 (@pBioinsumos, (SELECT Id FROM Hitos WHERE CronogramaId = @crBio AND Orden = 1), @sara, N'Arranque del proyecto', N'Se realizó la reunión de arranque y se firmó el acta con los aliados.', 10, 'Web', DATEADD(MONTH, -7, @ahora), 'Validado'),
 (@pBioinsumos, NULL, @sara, N'Caracterización de la pulpa de café', N'Análisis fisicoquímico de 12 muestras de tres municipios.', 20, 'Web', DATEADD(MONTH, -3, @ahora), 'Validado'),
 (@pBioinsumos, NULL, @valeria, N'Ensayos de fermentación', N'Primeras corridas de fermentación en biorreactor de 5 L.', 30, 'Web', DATEADD(DAY, -15, @ahora), 'Validado'),
 (@pBioinsumos, NULL, @sara, N'Formulación del bioinsumo', N'Se definieron dos formulaciones candidatas para las pruebas en campo.', 35, 'Web', DATEADD(DAY, -4, @ahora), 'Pendiente'),
 (@pBioinsumos, NULL, @valeria, N'Muestreo en campo', N'Muestreo de suelos en dos fincas piloto.', 33, 'Web', DATEADD(DAY, -8, @ahora), 'Rechazado'),
 (@pRoya, NULL, @valeria, N'Etiquetado de 5.000 imágenes', N'Imágenes etiquetadas por agrónomos de Cenicafé.', 40, 'Web', DATEADD(MONTH, -5, @ahora), 'Validado'),
 (@pRoya, NULL, @lucia, N'Primer modelo de clasificación', N'Exactitud de 87 % en el conjunto de validación.', 60, 'Web', DATEADD(MONTH, -1, @ahora), 'Validado'),
 (@pRoya, NULL, @mateo, N'Captura de imágenes en finca', N'Captura con teléfonos de 300 plantas en Chinchiná.', 65, 'Web', DATEADD(DAY, -2, @ahora), 'Pendiente'),
 (@pTransfo, NULL, @mateo, N'Integración con el sistema SCADA', N'Lectura de 40 transformadores cada 15 minutos.', 30, 'Web', DATEADD(MONTH, -2, @ahora), 'Validado'),
 (@pTransfo, NULL, @lucia, N'Detección de anomalías', N'Modelo de bosque de aislamiento con 92 % de precisión.', 45, 'Web', DATEADD(DAY, -20, @ahora), 'Validado'),
 (@pTransfo, NULL, @mateo, N'Prototipo del tablero', N'Primera versión del tablero para operadores.', 55, 'Web', DATEADD(DAY, -1, @ahora), 'Pendiente');

DECLARE @avFerm int = (SELECT Id FROM Avances WHERE ProyectoId = @pBioinsumos AND Titulo = N'Ensayos de fermentación');
DECLARE @avMuestreo int = (SELECT Id FROM Avances WHERE ProyectoId = @pBioinsumos AND Titulo = N'Muestreo en campo');

INSERT INTO AvanceValidaciones (AvanceId, ValidadorId, RolValidador, Periodo, Resultado, Observaciones, Fecha)
SELECT a.Id, v.ValidadorId, v.Rol, FORMAT(a.FechaCaptura, 'yyyy-MM'), v.Resultado, v.Obs, DATEADD(DAY, 1, a.FechaCaptura)
FROM Avances a
JOIN (VALUES
  (N'Arranque del proyecto', @coordBio, 'Coordinador', 'Validado', NULL),
  (N'Caracterización de la pulpa de café', @coordBio, 'Coordinador', 'Validado', NULL),
  (N'Etiquetado de 5.000 imágenes', @coordIa, 'Coordinador', 'Validado', NULL),
  (N'Primer modelo de clasificación', @coordIa, 'Coordinador', 'Validado', NULL),
  (N'Integración con el sistema SCADA', @coordIa, 'Coordinador', 'Validado', NULL),
  (N'Detección de anomalías', @coordIa, 'Coordinador', 'Validado', NULL)
) v (Titulo, ValidadorId, Rol, Resultado, Obs) ON v.Titulo = a.Titulo;

-- Validado por el IP: sigue pendiente para el Coordinador. Rechazado: el autor debe corregirlo.
INSERT INTO AvanceValidaciones (AvanceId, ValidadorId, RolValidador, Periodo, Resultado, Observaciones, Fecha) VALUES
 (@avFerm, @sara, 'IP', FORMAT(DATEADD(DAY, -15, @ahora), 'yyyy-MM'), 'Validado', NULL, DATEADD(DAY, -14, @ahora)),
 (@avMuestreo, @sara, 'IP', FORMAT(DATEADD(DAY, -8, @ahora), 'yyyy-MM'), 'Rechazado', N'Falta adjuntar el registro fotográfico y las coordenadas de los puntos de muestreo.', DATEADD(DAY, -7, @ahora));

-- ===================== Notificaciones de ejemplo (bandeja HU-22)
INSERT INTO Notificaciones (UsuarioId, Tipo, Titulo, Mensaje, ReferenciaTipo, ReferenciaId, Leida, EnviadaPush, EnviadaEmail, Intentos) VALUES
 (@coordBio, 'AvanceRegistrado', N'Nuevo avance en ' + @c + CAST(@a1 AS varchar(4)) + '-001', N'Se registró el avance "Formulación del bioinsumo" (35 %). Está pendiente de validación.', 'Proyecto', @pBioinsumos, 0, 0, 1, 0),
 (@coordBing, 'PropuestaAprobadaCoordinador', N'Propuesta enviada al Director', N'Usted aprobó la propuesta de prótesis de mano; quedó pendiente del aval del Director.', 'Proyecto', @pProtesis, 1, 0, 1, 0),
 (@director, 'PropuestaAprobadaCoordinador', N'Propuesta ' + @c + CAST(@a AS varchar(4)) + '-005 pendiente de aval', N'El Coordinador aprobó la propuesta "Prótesis de mano de bajo costo con impresión 3D". Está pendiente de su aval.', 'Proyecto', @pProtesis, 0, 0, 1, 0),
 (@director, 'PostulacionSeleccionada', N'Postulación seleccionada: ' + @c + CAST(@a AS varchar(4)) + '-007', N'La propuesta "Sistema de alerta temprana de calidad del agua con sensores" fue seleccionada en la convocatoria SGR. Registre la aprobación del proyecto.', 'Proyecto', @pAgua, 0, 0, 1, 0),
 (@valeria, 'PropuestaDevuelta', N'Propuesta ' + @c + CAST(@a AS varchar(4)) + '-006 devuelta', N'La propuesta fue devuelta en la etapa Coordinador. Observaciones: Ajustar el cronograma y detallar los entregables de la consultoría para el cliente.', 'Proyecto', @pCafe, 0, 0, 1, 0),
 (@valeria, 'AvanceRechazado', N'Avance rechazado en ' + @c + CAST(@a1 AS varchar(4)) + '-001', N'Su avance "Muestreo en campo" fue rechazado por el IP. Observaciones: Falta adjuntar el registro fotográfico y las coordenadas de los puntos de muestreo.', 'Proyecto', @pBioinsumos, 0, 0, 1, 0),
 (@coordSis, 'PropuestaEnRevision', N'Propuesta ' + @c + CAST(@a AS varchar(4)) + '-004 enviada a revisión', N'La propuesta "Plataforma bioinformática para el análisis de genomas de café" fue enviada a revisión (etapa Coordinador).', 'Proyecto', @pGenomas, 0, 0, 1, 0),
 (@lucia, 'ProyectoAprobado', N'Proyecto ' + @c + CAST(@a AS varchar(4)) + '-001 aprobado', N'El proyecto fue aprobado. Revise la información registrada y defina sus hitos.', 'Proyecto', @pEnergia, 1, 0, 1, 0);

COMMIT TRANSACTION;
PRINT 'Datos de prueba cargados. Contraseña de los usuarios de prueba: Prueba12345';
