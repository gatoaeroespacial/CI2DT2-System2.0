/*
  Datos de prueba para desarrollo (CI²DT²). NO ejecutar en Producción.
  Requisito: la API ya arrancó una vez (migraciones y catálogos semilla aplicados).
  Se puede ejecutar varias veces: si los datos ya existen, no hace nada.
  Contraseña de todos los usuarios de prueba: Prueba12345
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
DECLARE @hoy date = CAST(GETUTCDATE() AS date);

-- Aliados (TiposAliado: 1 Universidad, 2 Empresa, 3 Entidad pública, 4 Cámara de Comercio, 5 ONG)
INSERT INTO Aliados (TipoAliadoId, Nombre, Nit, Pais, Ciudad, Sector, RepresentanteLegal, Email) VALUES
 (2, 'CHEC - Central Hidroeléctrica de Caldas', '890800128-6', 'Colombia', 'Manizales', 'Energía', 'Representante CHEC', 'contacto@chec.test'),
 (1, 'Universidad Autónoma de Manizales', '890805051-0', 'Colombia', 'Manizales', 'Educación', 'Rector UAM', 'contacto@uam.test'),
 (3, 'Ministerio de Ciencia, Tecnología e Innovación', '899999031-8', 'Colombia', 'Bogotá', 'Ciencia y tecnología', 'Ministro(a)', 'contacto@minciencias.test'),
 (1, 'Universidad Nacional de Colombia - Sede Manizales', '899999063-3', 'Colombia', 'Manizales', 'Educación', 'Vicerrector de sede', 'contacto@unal.test'),
 (4, 'Cámara de Comercio de Manizales por Caldas', '890801055-1', 'Colombia', 'Manizales', 'Gremial', 'Presidente ejecutivo', 'contacto@ccm.test'),
 (5, 'SwissContact', NULL, 'Suiza', 'Zúrich', 'Cooperación internacional', 'Director país', 'contacto@swisscontact.test');

DECLARE @chec int = (SELECT Id FROM Aliados WHERE Nombre LIKE 'CHEC%');
DECLARE @uam int = (SELECT Id FROM Aliados WHERE Nombre LIKE 'Universidad Autónoma%');
DECLARE @minciencias int = (SELECT Id FROM Aliados WHERE Nombre LIKE 'Ministerio de Ciencia%');
DECLARE @unal int = (SELECT Id FROM Aliados WHERE Nombre LIKE 'Universidad Nacional%');
DECLARE @ccm int = (SELECT Id FROM Aliados WHERE Nombre LIKE 'Cámara de Comercio%');

-- Usuarios
INSERT INTO Usuarios (Email, PasswordHash, Nombres, Apellidos, TipoDocumento, NumeroDocumento, Telefono, Cargo, Dependencia, AliadoId, CvLacUrl, Orcid, TieneAcceso) VALUES
 ('director@pruebas.ci2dt2.test',      @hash, 'Carlos',   'Montoya Ríos',    'CC', '10200300', '3001110001', 'Director del Centro',      'CI²DT²',                       NULL, NULL, NULL, 1),
 ('coord.biotec@pruebas.ci2dt2.test',  @hash, 'Ana',      'Gómez Arango',    'CC', '30400500', '3001110002', 'Docente titular',         'Facultad de Ciencias Agropecuarias', NULL, 'https://scienti.minciencias.gov.co/cvlac/visualizador/generarCurriculoCv.do?cod_rh=0000000001', '0000-0002-1825-0097', 1),
 ('coord.ia@pruebas.ci2dt2.test',      @hash, 'Jorge',    'Salazar López',   'CC', '75600700', '3001110003', 'Docente asociado',        'Facultad de Ingenierías',      NULL, NULL, '0000-0002-1694-233X', 1),
 ('inv.lucia@pruebas.ci2dt2.test',     @hash, 'Lucía',    'Herrera Castaño', 'CC', '24800900', '3001110004', 'Docente investigadora',   'Facultad de Ciencias Exactas', NULL, NULL, NULL, 1),
 ('inv.mateo@pruebas.ci2dt2.test',     @hash, 'Mateo',    'Ospina Valencia', 'CC', '10901100', '3001110005', 'Investigador',            'Facultad de Ingenierías',      NULL, NULL, NULL, 1),
 ('inv.sara@pruebas.ci2dt2.test',      @hash, 'Sara',     'Cardona Mejía',   'CE', 'E1234567', '3001110006', 'Investigadora posdoctoral','Facultad de Ciencias Agropecuarias', NULL, NULL, NULL, 1),
 ('inv.inactivo@pruebas.ci2dt2.test',  @hash, 'Pedro',    'Ramírez Duque',   'CC', '10301200', '3001110007', 'Investigador',            'Facultad de Ciencias para la Salud', NULL, NULL, NULL, 0),
 ('externo.chec@pruebas.ci2dt2.test',  @hash, 'Valentina','Marín Zuluaga',   'CC', '30501300', '3001110008', 'Líder de innovación',     'CHEC',                         @chec, NULL, NULL, 1);

DECLARE @director int = (SELECT Id FROM Usuarios WHERE Email = 'director@pruebas.ci2dt2.test');
DECLARE @coordBio int = (SELECT Id FROM Usuarios WHERE Email = 'coord.biotec@pruebas.ci2dt2.test');
DECLARE @coordIa int = (SELECT Id FROM Usuarios WHERE Email = 'coord.ia@pruebas.ci2dt2.test');
DECLARE @lucia int = (SELECT Id FROM Usuarios WHERE Email = 'inv.lucia@pruebas.ci2dt2.test');
DECLARE @mateo int = (SELECT Id FROM Usuarios WHERE Email = 'inv.mateo@pruebas.ci2dt2.test');
DECLARE @sara int = (SELECT Id FROM Usuarios WHERE Email = 'inv.sara@pruebas.ci2dt2.test');
DECLARE @pedro int = (SELECT Id FROM Usuarios WHERE Email = 'inv.inactivo@pruebas.ci2dt2.test');
DECLARE @externo int = (SELECT Id FROM Usuarios WHERE Email = 'externo.chec@pruebas.ci2dt2.test');

-- Roles (1 Admin, 2 Director, 3 Coordinador, 4 Investigador, 5 Externo, 6 Estudiante, 7 Docente, 8 Logistica, 9 Secretaria)
INSERT INTO UsuarioRoles (UsuarioId, RolId) VALUES
 (@director, 2),
 (@coordBio, 3), (@coordBio, 4),
 (@coordIa, 3),  (@coordIa, 4),
 (@lucia, 4), (@mateo, 4), (@sara, 4), (@pedro, 4),
 (@externo, 5);

-- Objetivos específicos de los programas
INSERT INTO ObjetivosEspecificos (ProgramaId, Descripcion, Orden) VALUES
 (1, 'Desarrollar bioproductos a partir de residuos agroindustriales de la región.', 1),
 (1, 'Fortalecer la transferencia tecnológica al sector cafetero.', 2),
 (2, 'Diseñar dispositivos biomédicos de bajo costo.', 1),
 (3, 'Aplicar modelos de inteligencia artificial a problemas productivos del departamento.', 1),
 (3, 'Formar talento humano en simulación y analítica de datos.', 2),
 (4, 'Consolidar capacidades en bioinformática y computación de alto desempeño.', 1),
 (5, 'Proponer soluciones tecnológicas para la movilidad y la energía urbanas.', 1);

-- Integrantes de programas (vigentes, salvo uno retirado)
INSERT INTO ProgramaIntegrantes (ProgramaId, UsuarioId, RolEnPrograma, FechaInicio, FechaFin) VALUES
 (1, @coordBio, 'Coordinador', DATEADD(YEAR, -1, @hoy), NULL),
 (3, @coordIa,  'Coordinador', DATEADD(YEAR, -1, @hoy), NULL),
 (1, @sara,  'Investigador', DATEADD(MONTH, -8, @hoy), NULL),
 (3, @lucia, 'Investigador', DATEADD(MONTH, -10, @hoy), NULL),
 (3, @mateo, 'Investigador', DATEADD(MONTH, -6, @hoy), NULL),
 (5, @mateo, 'Investigador', DATEADD(MONTH, -6, @hoy), NULL),
 (2, @pedro, 'Investigador', DATEADD(YEAR, -2, @hoy), DATEADD(MONTH, -1, @hoy));

-- Aliados por programa
INSERT INTO ProgramaAliados (ProgramaId, AliadoId) VALUES
 (1, @uam), (1, @ccm), (3, @chec), (3, @minciencias), (4, @unal), (5, @chec);

-- Convocatorias (TiposConvocatoria: 1 Interna, 2 Nacional, 5 SGR)
INSERT INTO Convocatorias (Nombre, Numero, TipoConvocatoriaId, EntidadConvocanteId, Plataforma, Url, FechaApertura, FechaCierre, Moneda, MontoMaximo) VALUES
 ('Convocatoria interna de investigación VIP 2026', 'VIP-01-2026', 1, NULL, 'VIP', 'https://vip.ucaldas.test/convocatoria-2026', DATEADD(DAY, -30, @hoy), DATEADD(DAY, 20, @hoy), 'COP', 60000000),
 ('Convocatoria Minciencias SGR - CTeI regional', '45-2026', 5, @minciencias, 'SIGP', 'https://minciencias.test/convocatorias/45-2026', DATEADD(DAY, -60, @hoy), DATEADD(DAY, 45, @hoy), 'COP', 1500000000),
 ('Retos de innovación CHEC 2026', 'CHEC-R-2026', 2, @chec, 'Portal CHEC', 'https://chec.test/retos', DATEADD(DAY, -90, @hoy), DATEADD(DAY, -5, @hoy), 'COP', 200000000);

-- Proyectos (Estados: 1 En elaboración, 4 Presentado, 5 Aprobado, 6 En ejecución; Tipos: 1 I+D+i, 2 Reto Empresarial)
INSERT INTO Proyectos (Codigo, Titulo, Resumen, ProgramaId, TipoProyectoId, EstadoId, InvestigadorPrincipalId, FechaInicio, FechaFin, PorcentajeAvance, Moneda, PresupuestoTotal, TasaCambio, PresupuestoTotalCop) VALUES
 ('CI2DT2-2026-001', 'Bioinsumos a partir de pulpa de café para cultivos de la región', 'Aprovechamiento de residuos del beneficio del café para producir bioinsumos agrícolas.', 1, 1, 6, @sara, DATEADD(MONTH, -4, @hoy), DATEADD(MONTH, 14, @hoy), 25.00, 'COP', 180000000, NULL, 180000000),
 ('CI2DT2-2026-002', 'Predicción de demanda energética con aprendizaje automático', 'Modelos de pronóstico de carga para la red de distribución de Caldas.', 3, 2, 5, @lucia, DATEADD(MONTH, 1, @hoy), DATEADD(MONTH, 13, @hoy), 0.00, 'COP', 200000000, NULL, 200000000),
 ('CI2DT2-2026-003', 'Gemelo digital para la movilidad urbana de Manizales', 'Simulación de tráfico y transporte público para apoyar decisiones de movilidad.', 3, 1, 1, @mateo, NULL, NULL, 0.00, 'USD', 50000, 4000.0000, 200000000),
 ('CI2DT2-2026-004', 'Monitoreo de calidad del aire con sensores de bajo costo', 'Red de sensores IoT para medir material particulado en zonas urbanas.', 5, 1, 4, @mateo, DATEADD(MONTH, 2, @hoy), DATEADD(MONTH, 20, @hoy), 0.00, 'COP', 95000000, NULL, 95000000);

DECLARE @p1 int = (SELECT Id FROM Proyectos WHERE Codigo = 'CI2DT2-2026-001');
DECLARE @p2 int = (SELECT Id FROM Proyectos WHERE Codigo = 'CI2DT2-2026-002');
DECLARE @p3 int = (SELECT Id FROM Proyectos WHERE Codigo = 'CI2DT2-2026-003');
DECLARE @p4 int = (SELECT Id FROM Proyectos WHERE Codigo = 'CI2DT2-2026-004');

INSERT INTO ProyectoIntegrantes (ProyectoId, UsuarioId, RolEnProyecto, FechaInicio, FechaFin) VALUES
 (@p1, @sara, 'IP', DATEADD(MONTH, -4, @hoy), NULL),
 (@p1, @coordBio, 'Co-investigador', DATEADD(MONTH, -4, @hoy), NULL),
 (@p2, @lucia, 'IP', @hoy, NULL),
 (@p2, @mateo, 'Co-investigador', @hoy, NULL),
 (@p3, @mateo, 'IP', @hoy, NULL),
 (@p4, @mateo, 'IP', @hoy, NULL),
 (@p4, @lucia, 'Co-investigador', @hoy, NULL);

-- Financiamiento
INSERT INTO FuentesFinanciamiento (ProyectoId, AliadoId, TipoFuente, TipoAporte, Moneda, Monto, TasaCambio, MontoCop, Estado) VALUES
 (@p1, NULL, 'Recursos propios', 'Efectivo', 'COP', 120000000, NULL, 120000000, 'Desembolsado'),
 (@p1, @uam, 'Contrapartida', 'Especie', 'COP', 60000000, NULL, 60000000, 'Comprometido'),
 (@p2, @chec, 'Cofinanciación', 'Efectivo', 'COP', 150000000, NULL, 150000000, 'Comprometido'),
 (@p2, NULL, 'Contrapartida', 'Especie', 'COP', 50000000, NULL, 50000000, 'Comprometido'),
 (@p3, @minciencias, 'Convocatoria', 'Efectivo', 'USD', 50000, 4000, 200000000, 'Estimado');

-- Postulaciones
INSERT INTO PropuestasConvocatoria (ProyectoId, ConvocatoriaId, FechaPresentacion, IdPostulacionExterna, MontoSolicitado, Moneda, MontoSolicitadoCop, Resultado, FaseExterna) VALUES
 (@p2, (SELECT Id FROM Convocatorias WHERE Numero = 'CHEC-R-2026'), DATEADD(DAY, -10, @hoy), 'CHEC-R-2026-017', 150000000, 'COP', 150000000, 'Seleccionada', NULL),
 (@p4, (SELECT Id FROM Convocatorias WHERE Numero = 'VIP-01-2026'), DATEADD(DAY, -3, @hoy), 'VIP-2026-044', 60000000, 'COP', 60000000, 'Pendiente', NULL);

-- Cronograma e hitos del proyecto en ejecución (TiposHito: 1 Entregable, 2 Informe, 3 Reunión)
INSERT INTO Cronogramas (Nombre, ProyectoId) VALUES ('Cronograma CI2DT2-2026-001', @p1);
DECLARE @crono int = SCOPE_IDENTITY();

INSERT INTO Hitos (CronogramaId, TipoHitoId, Titulo, FechaLimite, AlertarDias, ResponsableId, Estado, FechaCompletado, Orden) VALUES
 (@crono, 3, 'Reunión de arranque con aliados', DATEADD(MONTH, -4, @hoy), 7, @sara, 'Completado', DATEADD(MONTH, -4, @hoy), 1),
 (@crono, 2, 'Primer informe de avance', DATEADD(DAY, -15, @hoy), 7, @sara, 'Vencido', NULL, 2),
 (@crono, 1, 'Prototipo de bioinsumo validado en laboratorio', DATEADD(DAY, 10, @hoy), 7, @coordBio, 'En curso', NULL, 3),
 (@crono, 2, 'Segundo informe de avance', DATEADD(MONTH, 3, @hoy), 14, @sara, 'Pendiente', NULL, 4);

-- Avances
INSERT INTO Avances (ProyectoId, HitoId, AutorId, Titulo, Descripcion, PorcentajeAvance, OrigenRegistro, FechaCaptura, EstadoValidacion) VALUES
 (@p1, (SELECT Id FROM Hitos WHERE CronogramaId = @crono AND Orden = 1), @sara, 'Arranque del proyecto', 'Se realizó la reunión de arranque y se firmó el acta.', 10.00, 'Web', DATEADD(MONTH, -4, GETUTCDATE()), 'Validado'),
 (@p1, NULL, @sara, 'Caracterización de la pulpa de café', 'Análisis fisicoquímico de 12 muestras.', 25.00, 'Web', DATEADD(DAY, -20, GETUTCDATE()), 'Pendiente');

-- Notificaciones
INSERT INTO Notificaciones (UsuarioId, Tipo, Titulo, Mensaje, ReferenciaTipo, ReferenciaId, Leida, EnviadaPush, EnviadaEmail, Intentos) VALUES
 (@sara, 'HitoPorVencer', 'Hito vencido: Primer informe de avance', 'El hito del proyecto CI2DT2-2026-001 venció.', 'Proyecto', @p1, 0, 0, 0, 0),
 (@coordBio, 'AvancePendiente', 'Avance pendiente de validación', 'Hay un avance del proyecto CI2DT2-2026-001 por validar.', 'Proyecto', @p1, 0, 0, 0, 0),
 (@externo, 'Bienvenida', 'Bienvenido a CI²DT²', 'Su cuenta de aliado quedó activa.', NULL, NULL, 1, 0, 0, 0);

-- Grupos de investigación y vínculo con proyectos
INSERT INTO GruposInvestigacion (ProgramaId, Nombre, CodigoGrupLac, CategoriaMinciencias) VALUES
 (1, 'GruCalPro', 'COL0000001', 'A'),
 (3, 'GITIR', 'COL0000002', 'A1'),
 (3, 'IA', 'COL0000003', 'B');
INSERT INTO ProyectoGrupos (ProyectoId, GrupoId) VALUES
 (@p1, (SELECT Id FROM GruposInvestigacion WHERE Nombre = 'GruCalPro')),
 (@p2, (SELECT Id FROM GruposInvestigacion WHERE Nombre = 'GITIR'));

-- Aliados del proyecto y productos comprometidos
INSERT INTO ProyectoAliados (ProyectoId, AliadoId, RolAliado) VALUES
 (@p1, @uam, 'Co-investigador'),
 (@p2, @chec, 'Cofinanciador');
INSERT INTO ProyectoProductos (ProyectoId, TipoProducto, Descripcion, Estado, FechaCompromiso) VALUES
 (@p1, 'Artículo', 'Artículo en revista indexada sobre bioinsumos', 'Comprometido', DATEADD(MONTH, 12, @hoy)),
 (@p2, 'Software', 'Modelo de pronóstico desplegado para CHEC', 'Comprometido', DATEADD(MONTH, 10, @hoy));

-- Semillero (TiposIntegranteSemillero: 1 Estudiante, 2 Docente)
INSERT INTO Semilleros (ProgramaId, CoordinadorId, Nombre) VALUES (3, @coordIa, 'IoT');
INSERT INTO SemilleroIntegrantes (SemilleroId, UsuarioId, TipoIntegranteId, ProyectoId, EstadoAprobacion, AprobadoPorId, FechaIngreso) VALUES
 ((SELECT Id FROM Semilleros WHERE Nombre = 'IoT'), @mateo, 2, @p4, 'Aprobado', @coordIa, DATEADD(MONTH, -3, @hoy));

-- Educación continua (TiposFormacion: 1 Diplomado; Modalidades: 3 Híbrido)
INSERT INTO Formaciones (ProgramaId, TipoFormacionId, ModalidadId, Nombre, IntensidadHoras, ValorMatricula, CupoMaximo, PuntoEquilibrio, MetaFinancieraCop, FechaInicioInscripcion, FechaFinInscripcion, FechaInicio, FechaFin, AsistenciaMinimaPct, NotaMinima, Estado) VALUES
 (3, 1, 3, 'Diplomado en analítica de datos con Python', 120, 1800000, 30, 15, 27000000, DATEADD(DAY, -10, @hoy), DATEADD(DAY, 20, @hoy), DATEADD(DAY, 30, @hoy), DATEADD(MONTH, 4, @hoy), 80, 3.5, 'Inscripciones');
DECLARE @formacion int = (SELECT Id FROM Formaciones WHERE Nombre LIKE 'Diplomado en analítica%');
INSERT INTO FormacionDocentes (FormacionId, DocenteId) VALUES (@formacion, @lucia);
INSERT INTO Inscripciones (FormacionId, UsuarioId, Estado, ValorPagado) VALUES (@formacion, @sara, 'Prematriculado', NULL);

-- Evento del Centro (TiposEvento: 4 Innovación en Órbita; Modalidades: 1 Presencial)
INSERT INTO Eventos (ProgramaId, TipoEventoId, ModalidadId, ResponsableLogisticaId, Nombre, FechaInicio, FechaFin, Lugar, CupoMaximo, Estado) VALUES
 (NULL, 4, 1, @director, 'Innovación en Órbita 2026', DATEADD(DAY, 40, GETUTCDATE()), DATEADD(DAY, 41, GETUTCDATE()), 'Auditorio Central, Universidad de Caldas', 200, 'Programado');
INSERT INTO EventoAsistentes (EventoId, UsuarioId, AliadoId, Cargo, Estado) VALUES
 ((SELECT Id FROM Eventos WHERE Nombre = 'Innovación en Órbita 2026'), @externo, @chec, 'Líder de innovación', 'Inscrito');

COMMIT TRANSACTION;
PRINT 'Datos de prueba cargados. Contraseña de los usuarios de prueba: Prueba12345';
