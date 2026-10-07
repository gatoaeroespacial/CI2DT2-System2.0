**CI²DT² · Universidad de Caldas**

**Product Backlog del MVP**

Sistema de información y portal web institucional del Centro de
Investigación, Innovación, Desarrollo y Transferencia de Tecnología

|                   |                                                                                                               |
|-------------------|---------------------------------------------------------------------------------------------------------------|
| **Versión**       | 2.0 (ajustada a un plazo de 2 meses)                                                                          |
| **Fecha**         | 7 de octubre de 2026                                                                                          |
| **Elaborado por** | Juan Mauricio Arias Hernández                                                                                 |
| **Base**          | Análisis de procesos del sistema CI²DT², Modelo entidad-relación v3 (aprobado) y Stack tecnológico            |
| **Contenido**     | 11 épicas · 29 historias de usuario · 3 historias técnicas · 129 puntos de historia planificados en 8 semanas |

1\. Objetivo y alcance del MVP

1.1 Objetivo del MVP

Poner en producción, en 8 semanas (2 meses), una primera versión del
sistema que resuelva el problema más crítico del CI²DT²: la gestión
dispersa del ciclo de vida de los proyectos de I+D+i. El MVP debe
permitir que el centro:

- Registre en un único lugar las propuestas y proyectos de sus 5
  programas, con su equipo, financiación y convocatoria.

- Formalice el flujo institucional de revisión (IP → Coordinador →
  Director) con trazabilidad de cada decisión.

- Haga seguimiento a hitos y avances con alertas automáticas que eviten
  incumplimientos ante las entidades financiadoras.

- Dé al Director un tablero consolidado para la toma de decisiones y los
  informes a la VIP.

- Tenga una presencia web institucional que muestre sus programas,
  proyectos, capacidades y convocatorias abiertas.

1.2 Criterios de priorización

Cada historia se priorizó con la técnica MoSCoW según cuatro criterios:
(1) valor directo para el centro y sus objetivos estratégicos, (2)
dependencia técnica (lo que otras funciones necesitan para existir), (3)
reducción de riesgo institucional (cumplimiento legal, incumplimiento
ante financiadores) y (4) esfuerzo. Todo lo que no cumple al menos uno
de los tres primeros criterios con fuerza queda para releases
posteriores.

| **Prioridad** | **Significado en este backlog**                                                                                                                                       |
|---------------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| **Must**      | Sin esta historia el MVP no cumple su objetivo. Se compromete en el plan de sprints.                                                                                  |
| **Should**    | Está planificada, pero forma parte de la línea de corte (sección 5.2): es lo primero que se mueve a Release 2 si la velocidad real del equipo es menor a la estimada. |

1.3 Módulos del modelo ER incluidos en el MVP

| **Módulo ER v3**                       | **Alcance en el MVP**                                                                                                                                         |
|----------------------------------------|---------------------------------------------------------------------------------------------------------------------------------------------------------------|
| **M01 Seguridad**                      | Completo                                                                                                                                                      |
| **M02 Organización**                   | Programas, ObjetivosEspecificos y ProgramaIntegrantes (GruposInvestigacion en Release 2)                                                                      |
| **M03 Proyectos**                      | Proyectos, estados, tipos, integrantes y revisiones (ProyectoGrupos, ProyectoAliados, ProyectoAntecedentes, ProyectoProductos y ProyectoCierres en Release 2) |
| **M04 Convocatorias y Financiamiento** | Completo                                                                                                                                                      |
| **M05 Cronogramas y Seguimiento**      | Cronogramas, hitos y avances de proyectos, vía web; validación sin rúbrica                                                                                    |
| **M06 Documentos**                     | Documentos y TiposDocumento sin versionado (DocumentoFirmas en Release 2)                                                                                     |
| **M07 Notificaciones**                 | Notificaciones por correo y bandeja en la aplicación (PreferenciasNotificacion y push en Release 2)                                                           |
| **M11 Entorno Empresarial**            | Aliados, TiposAliado y ProgramaAliados                                                                                                                        |
| **M13 Indicadores y Reportes**         | Fuera del MVP: el tablero del Director consulta directamente los módulos operativos                                                                           |
| **M14 Visitas y Datos Personales**     | Solo PoliticasTratamientoDatos y ConsentimientosDatos                                                                                                         |
| **M15 Auditoría e IA**                 | Solo LogActividades (FuentesConocimiento en Release 3)                                                                                                        |
| **M08, M09, M10, M12**                 | Fuera del MVP (ver sección 1.4)                                                                                                                               |

1.4 Fuera del alcance del MVP

| **Funcionalidad / proceso**                                                                         | **Motivo**                                                                                                 | **Release** |
|-----------------------------------------------------------------------------------------------------|------------------------------------------------------------------------------------------------------------|-------------|
| M08 Formación (cursos, diplomados, inscripciones, asistencia, calificaciones) – P8, P9              | Alto valor, pero independiente del núcleo de proyectos; requiere gestión de pagos y viabilidad financiera. | Release 2   |
| Certificados digitales con blockchain (SparLum) – P10                                               | Depende de M08 y de la integración con un tercero.                                                         | Release 2   |
| M09 Eventos e "Innovación en Órbita" – P12                                                          | No bloquea la gestión de I+D+i; puede difundirse inicialmente por el portal estático.                      | Release 2   |
| M10 Semilleros – P15                                                                                | Valor para indicadores, pero con flujo independiente.                                                      | Release 2   |
| M14 Registro de visitas físicas – P17                                                               | En el MVP solo se incluye la política y los consentimientos.                                               | Release 2   |
| Aplicación móvil (React Native) con GPS, cámara y push – P3, P18                                    | El modelo ya soporta GPS y origen "Mobile"; el MVP registra avances desde la web.                          | Release 2   |
| Firma digital encadenada de documentos (DocumentoFirmas)                                            | En el MVP el aval queda registrado en ProyectoRevisiones y en la auditoría.                                | Release 2   |
| Exportación de informes en PDF y Word (ReportesGenerados) – P20                                     | El tablero cubre la necesidad de consulta; la exportación formal viene después.                            | Release 2   |
| M12 Producción académica, propiedad intelectual, trabajos de grado y sustentaciones – P6, P7        | En el MVP los productos se registran como lista en ProyectoProductos.                                      | Release 3   |
| Convenios institucionales – P13                                                                     | Requiere ampliar el modelo de datos (no está en el ER v3).                                                 | Release 3   |
| Servicios de laboratorio de la UTA (cotización, pago y reporte) – P14                               | Requiere ampliar el modelo (catálogo de tarifas, cotizaciones, muestras).                                  | Release 3   |
| Asistente de IA con RAG y base vectorial (FuentesConocimiento); automatizaciones con agentes en n8n | Necesita primero un volumen de documentos y datos que el MVP empezará a generar.                           | Release 3   |
| Integraciones con SIGP, MGA Web, GESPROY, CvLAC y TRM automática                                    | En el MVP los códigos y tasas se registran manualmente.                                                    | Posterior   |

1.5 Ajuste del alcance al plazo de 2 meses

La versión 1.0 del backlog sumaba 213 puntos de historia para 14
semanas. Para cumplir el plazo de 8 semanas se redujo a 129 puntos con
tres medidas: (1) se eliminó el Sprint 0 y su trabajo técnico se hace
dentro del Sprint 1, (2) se movieron a Release 2 las historias que no
bloquean el flujo central de proyectos y (3) se recortaron detalles
dentro de varias historias que se mantienen. El flujo central (registrar
→ revisar y avalar → aprobar → hacer seguimiento → ver en el tablero y
en el portal) queda completo.

**Historias que pasan a Release 2**

| **Historia**                                                          | **Épica** | **SP** | **Cómo se cubre mientras tanto**                                                                             |
|-----------------------------------------------------------------------|-----------|--------|--------------------------------------------------------------------------------------------------------------|
| Consultar y actualizar mi perfil                                      | EP-01     | 2      | El usuario puede pedir cambios al Administrador; no bloquea ningún flujo.                                    |
| Registrar los grupos de investigación                                 | EP-02     | 3      | Útil para convocatorias FJC; se puede registrar el grupo en el resumen del proyecto mientras tanto.          |
| Vincular grupos de investigación y aliados al proyecto                | EP-03     | 3      | Las cartas de compromiso se cargan como documentos del proyecto (tipo "Carta de compromiso").                |
| Registrar los antecedentes de presentación de una propuesta           | EP-03     | 3      | Los antecedentes se pueden anotar en el resumen; la trazabilidad formal llega con Release 2.                 |
| Cerrar y finalizar un proyecto                                        | EP-03     | 5      | En 8 semanas casi ningún proyecto nuevo llegará a cierre; los finalizados se cargan por importación.         |
| Recibir alertas de cierre de convocatorias                            | EP-04     | 2      | Las fechas de cierre se ven en el catálogo de convocatorias; la alerta es una mejora.                        |
| Ver la bitácora cronológica del proyecto                              | EP-06     | 3      | La ficha del proyecto ya muestra avances, hitos y revisiones por pestañas.                                   |
| Configurar mis preferencias de notificación                           | EP-08     | 2      | Todas las notificaciones del MVP son relevantes; no hace falta configurarlas.                                |
| Tablero de control del Coordinador de programa                        | EP-09     | 5      | El Coordinador usa el listado filtrado por su programa y la bandeja de validaciones.                         |
| Indicadores estratégicos con semáforo                                 | EP-09     | 8      | Requiere metas acordadas con la VIP; el tablero del Director cubre la consulta inicial.                      |
| Consultar las convocatorias abiertas en el portal                     | EP-10     | 3      | Las convocatorias abiertas se muestran como sección de la página de inicio.                                  |
| Consultar los servicios del centro y enviar una solicitud de contacto | EP-10     | 5      | El portal muestra servicios y un correo de contacto en contenido estático; el formulario llega en Release 2. |
| Consultar el log de auditoría                                         | EP-11     | 3      | El Administrador consulta el log directamente en la base de datos ante un incidente.                         |

**Simplificaciones dentro de historias del MVP**

| **Historia**                   | **Qué incluye el MVP**                                                                                           | **Qué pasa a Release 2**                          |
|--------------------------------|------------------------------------------------------------------------------------------------------------------|---------------------------------------------------|
| **Validar avances**            | Sin rúbrica con puntaje: el validador aprueba o pide ajustes con observaciones.                                  | Rúbrica de 4 criterios                            |
| **Repositorio documental**     | Carga, descarga con permisos y hash SHA-256, sin control de versiones.                                           | Versionado de documentos                          |
| **Registro de auditoría**      | Se registran todas las mutaciones, sin hash encadenado.                                                          | Hash encadenado y verificación de integridad      |
| **Notificaciones**             | Correo y bandeja en la aplicación, sin preferencias ni actualización automática del contador.                    | Preferencias y contador en tiempo real            |
| **Postulación a convocatoria** | Datos de radicación, código MGA (SGR), fase y resultado; sin control de una sola postulación pendiente.          | Validaciones avanzadas por tipo de convocatoria   |
| **Tablero del Director**       | Tarjetas de indicadores, proyectos por estado y programa, hitos y avances pendientes; sin navegación al detalle. | Navegación al detalle, más gráficos y exportación |
| **Carga inicial de datos**     | Script de importación ejecutado por el equipo sobre una plantilla CSV, sin pantalla.                             | Importación desde la interfaz                     |
| **Página de inicio**           | Contenido institucional estático (MDX) con cifras dinámicas y convocatorias abiertas.                            | Contenido administrable                           |

*Además se fusionaron dos pares de historias: "Administrar programas"
con "Gestionar integrantes", y "Definir hitos" con "Completar o
reprogramar hitos".*

1.6 Actores del MVP

| **Actor**                   | **Descripción**                                                                                                           |
|-----------------------------|---------------------------------------------------------------------------------------------------------------------------|
| **Administrador**           | Configura el sistema y gestiona usuarios y roles.                                                                         |
| **Director del Centro**     | Gestiona programas y coordinadores, otorga el aval institucional, registra aprobaciones y consulta el tablero del centro. |
| **Coordinador de programa** | Gestiona su programa, revisa propuestas, valida avances y hace seguimiento a hitos.                                       |
| **Investigador / IP**       | Formula propuestas, conforma equipos, define hitos y registra avances de sus proyectos.                                   |
| **Externo**                 | Representante de un aliado con acceso limitado a su perfil y notificaciones.                                              |
| **Visitante**               | Persona no autenticada que consulta el portal público.                                                                    |
| **Sistema**                 | Motor de alertas de hitos y despacho de notificaciones.                                                                   |

2\. Resumen del backlog

2.1 Épicas

| **Épica**                                     | **Valor para el centro**                                                                                                                                                 | **Procesos**                                   | **HU** | **SP** |
|-----------------------------------------------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------|------------------------------------------------|--------|--------|
| **EP-01 Seguridad y control de acceso**       | Base obligatoria del sistema: sin identidad y permisos por rol no es posible operar ningún otro módulo ni proteger la información de los proyectos.                      | P16                                            | 5      | 23     |
| **EP-02 Estructura organizacional y aliados** | Modela los 5 programas, sus coordinadores, investigadores, grupos y aliados. Todo proyecto, indicador y contenido público depende de esta estructura.                    | P1, P15 (parcial)                              | 2      | 8      |
| **EP-03 Ciclo de vida de proyectos de I+D+i** | Núcleo del valor del centro: centraliza las propuestas y proyectos que hoy se gestionan en archivos dispersos, con trazabilidad desde la formulación hasta la ejecución. | P1, P2, P5                                     | 7      | 35     |
| **EP-04 Convocatorias y postulaciones**       | Permite saber a qué convocatorias se presenta el centro, con qué montos y con qué resultado; es la principal fuente de recursos del centro.                              | P1.1 a P1.5                                    | 2      | 6      |
| **EP-05 Cronogramas, hitos y alertas**        | Reduce el riesgo de incumplir entregables ante entidades financiadoras mediante alertas automáticas.                                                                     | P4                                             | 2      | 10     |
| **EP-06 Avances y seguimiento**               | Da visibilidad real del progreso de cada proyecto y formaliza la validación mensual por parte del IP y del Coordinador.                                                  | P3                                             | 2      | 6      |
| **EP-07 Gestión documental**                  | Centraliza soportes (propuestas, actos administrativos, cartas, informes) con integridad verificable por hash y control de versiones.                                    | Transversal (P1, P2, P5)                       | 1      | 3      |
| **EP-08 Notificaciones**                      | Hace que los flujos de revisión, hitos y avances avancen sin seguimiento manual por correo o mensajería personal.                                                        | P18                                            | 2      | 5      |
| **EP-09 Tablero de control**                  | Entrega al Director la información consolidada para la toma de decisiones y los informes a la VIP.                                                                       | P20                                            | 1      | 5      |
| **EP-10 Portal web institucional**            | Cara pública del centro: da a conocer programas, proyectos, capacidades y convocatorias abiertas.                                                                        | Instrucción del proyecto; P17 (consentimiento) | 3      | 9      |
| **EP-11 Auditoría y protección de datos**     | Cumplimiento de la Ley 1581 de 2012 y del Acuerdo 31 de 2015, y trazabilidad de todas las acciones sobre la información.                                                 | P17 (parcial), P19                             | 2      | 6      |

*Procesos: numeración del documento "Análisis de procesos sistema
CI2DT2" (P1 a P20). SP: puntos de historia (escala Fibonacci).*

2.2 Historias de usuario

| **ID**    | **Historia de usuario**                                             | **Épica** | **Prioridad** | **SP** | **Sprint** |
|-----------|---------------------------------------------------------------------|-----------|---------------|--------|------------|
| **HU-01** | Iniciar sesión en la plataforma                                     | EP-01     | Must          | 5      | Sprint 1   |
| **HU-02** | Mantener y cerrar la sesión de forma segura                         | EP-01     | Must          | 3      | Sprint 1   |
| **HU-03** | Restablecer una contraseña olvidada                                 | EP-01     | Should        | 2      | Sprint 3   |
| **HU-04** | Administrar usuarios y roles                                        | EP-01     | Must          | 5      | Sprint 1   |
| **HU-05** | Restringir el acceso según el rol y el programa                     | EP-01     | Must          | 8      | Sprint 1   |
| **HU-06** | Administrar programas, objetivos e integrantes                      | EP-02     | Must          | 5      | Sprint 2   |
| **HU-07** | Gestionar el catálogo de aliados                                    | EP-02     | Must          | 3      | Sprint 2   |
| **HU-08** | Registrar una propuesta o proyecto                                  | EP-03     | Must          | 8      | Sprint 2   |
| **HU-09** | Conformar el equipo del proyecto                                    | EP-03     | Must          | 3      | Sprint 2   |
| **HU-10** | Registrar fuentes de financiamiento y contrapartidas                | EP-03     | Must          | 3      | Sprint 2   |
| **HU-11** | Enviar la propuesta a revisión y obtener el aval del Director       | EP-03     | Must          | 8      | Sprint 3   |
| **HU-12** | Registrar la aprobación e inicio de ejecución del proyecto          | EP-03     | Must          | 5      | Sprint 3   |
| **HU-13** | Consultar mis proyectos y los proyectos del programa                | EP-03     | Must          | 5      | Sprint 2   |
| **HU-14** | Carga inicial de los proyectos y propuestas existentes              | EP-03     | Must          | 3      | Sprint 4   |
| **HU-15** | Registrar y mantener el catálogo de convocatorias                   | EP-04     | Must          | 3      | Sprint 3   |
| **HU-16** | Vincular la propuesta a una convocatoria y registrar la postulación | EP-04     | Must          | 3      | Sprint 3   |
| **HU-17** | Definir, completar y reprogramar los hitos del proyecto             | EP-05     | Must          | 5      | Sprint 3   |
| **HU-18** | Motor automático de alertas de hitos                                | EP-05     | Must          | 5      | Sprint 4   |
| **HU-19** | Registrar avances del proyecto                                      | EP-06     | Must          | 3      | Sprint 4   |
| **HU-20** | Validar avances                                                     | EP-06     | Should        | 3      | Sprint 4   |
| **HU-21** | Repositorio documental de proyectos                                 | EP-07     | Must          | 3      | Sprint 2   |
| **HU-22** | Centro de notificaciones en la aplicación                           | EP-08     | Should        | 2      | Sprint 3   |
| **HU-23** | Envío de notificaciones por correo electrónico                      | EP-08     | Must          | 3      | Sprint 3   |
| **HU-24** | Tablero de control del Director                                     | EP-09     | Must          | 5      | Sprint 4   |
| **HU-25** | Página de inicio institucional                                      | EP-10     | Must          | 3      | Sprint 4   |
| **HU-26** | Consultar los programas de investigación en el portal               | EP-10     | Should        | 3      | Sprint 4   |
| **HU-27** | Consultar el portafolio público de proyectos                        | EP-10     | Must          | 3      | Sprint 4   |
| **HU-28** | Registro automático de auditoría                                    | EP-11     | Must          | 3      | Sprint 2   |
| **HU-29** | Aceptar la política de tratamiento de datos personales              | EP-11     | Must          | 3      | Sprint 3   |

**Must: 25 historias (106 SP) · Should (línea de corte): 4 historias (10
SP) · Historias técnicas: 13 SP**

3\. Historias de usuario detalladas

Cada historia incluye la narrativa, sus metadatos, las reglas de negocio
derivadas del análisis de procesos y del modelo de datos, los criterios
de aceptación en formato Dado / Cuando / Entonces y notas técnicas
cuando aplican.

EP-01 · Seguridad y control de acceso

**Valor:** Base obligatoria del sistema: sin identidad y permisos por
rol no es posible operar ningún otro módulo ni proteger la información
de los proyectos.

**Procesos:** P16 **Módulos ER:** M01 Seguridad

HU-01 · Iniciar sesión en la plataforma

|                                                                                                                                                                                                                        |
|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| **Como** usuario registrado (Administrador, Director, Coordinador, Investigador o Externo)**, quiero** iniciar sesión con mi correo y mi contraseña**, para** acceder a las funcionalidades que corresponden a mi rol. |

|                    |                                                              |                |                      |
|--------------------|--------------------------------------------------------------|----------------|----------------------|
| **Prioridad**      | **Must (imprescindible)**                                    | **Estimación** | 5 puntos de historia |
| **Épica**          | EP-01 Seguridad y control de acceso                          | **Sprint**     | Sprint 1             |
| **Tablas (ER v3)** | Usuarios, UsuarioRoles, Roles, RefreshTokens, LogActividades |                |                      |
| **Endpoints**      | POST /api/v1/auth/login                                      |                |                      |
| **Dependencias**   | HT-02; HT-03                                                 |                |                      |

**Reglas de negocio**

- Solo pueden autenticarse usuarios con Usuarios.TieneAcceso = 1.

- Las contraseñas se almacenan únicamente como hash adaptativo
  (PasswordHasher de ASP.NET Identity, PBKDF2) y nunca en texto plano.

- El token de acceso (JWT) dura 15 minutos e incluye: id del usuario,
  correo, roles y programas a los que pertenece.

- El token de refresco es un valor aleatorio de 7 días de vigencia que
  se guarda solo como hash en RefreshTokens.TokenHash, junto con la IP
  de origen.

- Tras 5 intentos fallidos consecutivos en 15 minutos la cuenta se
  bloquea temporalmente durante 15 minutos.

**Criterios de aceptación**

**CA-1. Dado** un usuario activo con credenciales correctas, **cuando**
envía el formulario de inicio de sesión, **entonces** el sistema
responde HTTP 200 con el token de acceso y el token de refresco, y lo
redirige a su página de inicio según su rol (Director → tablero del
centro; Coordinador → tablero de su programa; Investigador → Mis
proyectos; Administrador → Usuarios).

**CA-2. Dado** un correo o una contraseña incorrectos, **cuando** el
usuario intenta iniciar sesión, **entonces** ve el mensaje genérico
"Correo o contraseña incorrectos", sin revelar cuál de los dos campos
falló (HTTP 401).

**CA-3. Dado** un usuario con TieneAcceso = 0, **cuando** intenta
iniciar sesión, **entonces** se rechaza el acceso con el mensaje "Su
cuenta está inactiva. Contacte al administrador del sistema".

**CA-4. Dado** 5 intentos fallidos consecutivos, **cuando** el usuario
realiza un sexto intento, **entonces** la cuenta queda bloqueada 15
minutos y se le informa el tiempo restante.

**CA-5. Dado** cualquier intento de inicio de sesión, exitoso o fallido,
**cuando** se procesa la solicitud, **entonces** queda registrado en
LogActividades con la IP, la fecha UTC y el resultado.

**Notas técnicas**

- *Formulario con react-hook-form + Zod (correo con formato válido y
  contraseña obligatoria).*

- *El token de acceso se mantiene en memoria (Zustand). El token de
  refresco viaja en cookie HttpOnly, Secure. Como el frontend (Vercel) y
  el backend (MonsterASP) están en dominios distintos, se recomienda un
  subdominio común (p. ej. app.\* y api.\*) o un BFF con rutas API de
  Next.js para poder usar SameSite=Lax/Strict.*

- *El contador de intentos fallidos se lleva en caché del servidor
  (IMemoryCache); no requiere cambios al modelo de datos.*

HU-02 · Mantener y cerrar la sesión de forma segura

|                                                                                                                                                                                                                                            |
|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| **Como** usuario autenticado**, quiero** que mi sesión se renueve automáticamente mientras trabajo y poder cerrarla cuando termine**, para** no perder mi trabajo por expiración de la sesión y proteger mi cuenta en equipos compartidos. |

|                    |                                                      |                |                      |
|--------------------|------------------------------------------------------|----------------|----------------------|
| **Prioridad**      | **Must (imprescindible)**                            | **Estimación** | 3 puntos de historia |
| **Épica**          | EP-01 Seguridad y control de acceso                  | **Sprint**     | Sprint 1             |
| **Tablas (ER v3)** | RefreshTokens                                        |                |                      |
| **Endpoints**      | POST /api/v1/auth/refresh · POST /api/v1/auth/logout |                |                      |
| **Dependencias**   | HU-01 (Iniciar sesión en la plataforma)              |                |                      |

**Reglas de negocio**

- Rotación obligatoria: cada uso de un token de refresco lo revoca
  (RefreshTokens.RevocadoEn) y emite uno nuevo.

- Si se presenta un token de refresco ya revocado (posible robo), se
  revocan todos los tokens activos del usuario.

- Cerrar sesión revoca el token de refresco actual. Cambiar la
  contraseña o desactivar al usuario revoca todos sus tokens.

**Criterios de aceptación**

**CA-1. Dado** un token de acceso expirado y un token de refresco
válido, **cuando** el cliente realiza una petición a la API,
**entonces** el cliente llama a /auth/refresh de forma transparente y
reintenta la petición original sin que el usuario lo note.

**CA-2. Dado** un token de refresco expirado o revocado, **cuando** el
cliente intenta renovar la sesión, **entonces** el usuario es redirigido
al inicio de sesión con el mensaje "Su sesión expiró. Inicie sesión
nuevamente".

**CA-3. Dado** un token de refresco que ya fue rotado, **cuando**
alguien lo reutiliza, **entonces** la API responde 401 y todos los
tokens activos de ese usuario quedan revocados.

**CA-4. Dado** un usuario autenticado, **cuando** hace clic en "Cerrar
sesión", **entonces** el token se revoca, el estado del cliente se
limpia, se redirige al inicio de sesión y el botón "atrás" del navegador
no muestra datos protegidos.

**Notas técnicas**

- *Interceptor de peticiones en el cliente (TanStack Query / fetch
  wrapper) que gestione una sola renovación concurrente.*

HU-03 · Restablecer una contraseña olvidada

|                                                                                                                                                                        |
|------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| **Como** usuario registrado**, quiero** restablecer mi contraseña mediante un enlace enviado a mi correo**, para** recuperar el acceso sin depender del administrador. |

|                    |                                                                                                 |                |                      |
|--------------------|-------------------------------------------------------------------------------------------------|----------------|----------------------|
| **Prioridad**      | **Should (importante)**                                                                         | **Estimación** | 2 puntos de historia |
| **Épica**          | EP-01 Seguridad y control de acceso                                                             | **Sprint**     | Sprint 3             |
| **Tablas (ER v3)** | Usuarios, RefreshTokens                                                                         |                |                      |
| **Endpoints**      | POST /api/v1/auth/forgot-password · POST /api/v1/auth/reset-password                            |                |                      |
| **Dependencias**   | HU-01 (Iniciar sesión en la plataforma); HU-23 (Envío de notificaciones por correo electrónico) |                |                      |

**Reglas de negocio**

- El enlace es válido durante 30 minutos y es de un solo uso.

- Política de contraseña: mínimo 10 caracteres, con al menos una
  mayúscula, una minúscula y un número.

- La respuesta es idéntica exista o no el correo, para evitar la
  enumeración de usuarios.

- Al restablecer la contraseña se revocan todas las sesiones activas del
  usuario.

**Criterios de aceptación**

**CA-1. Dado** que el usuario ingresa su correo, **cuando** solicita
restablecer la contraseña, **entonces** ve el mensaje "Si el correo está
registrado, recibirá un enlace" y, si el correo existe, el mensaje llega
en menos de 2 minutos.

**CA-2. Dado** un enlace vigente y una contraseña que cumple la
política, **cuando** el usuario confirma la nueva contraseña,
**entonces** la contraseña se actualiza, se revocan las sesiones y se le
redirige al inicio de sesión con un mensaje de confirmación.

**CA-3. Dado** un enlace expirado o ya utilizado, **cuando** el usuario
lo abre, **entonces** ve un mensaje que lo indica y la opción de
solicitar un nuevo enlace.

**CA-4. Dado** una contraseña que no cumple la política, **cuando** el
usuario la escribe, **entonces** ve en tiempo real qué reglas no se
cumplen y no puede enviar el formulario.

**Notas técnicas**

- *Token firmado con la Data Protection API de ASP.NET (propósito
  "reset" + sello del PasswordHash actual, lo que lo invalida tras el
  cambio). No requiere tablas nuevas.*

HU-04 · Administrar usuarios y roles

|                                                                                                                                                                                                  |
|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| **Como** Administrador del sistema**, quiero** crear, editar, activar y desactivar usuarios y asignarles uno o varios roles**, para** controlar quién accede a la plataforma y con qué permisos. |

|                    |                                                                                                      |                |                      |
|--------------------|------------------------------------------------------------------------------------------------------|----------------|----------------------|
| **Prioridad**      | **Must (imprescindible)**                                                                            | **Estimación** | 5 puntos de historia |
| **Épica**          | EP-01 Seguridad y control de acceso                                                                  | **Sprint**     | Sprint 1             |
| **Tablas (ER v3)** | Usuarios, UsuarioRoles, Roles, Aliados                                                               |                |                      |
| **Endpoints**      | GET/POST/PUT /api/v1/usuarios · PATCH /api/v1/usuarios/{id}/estado · PUT /api/v1/usuarios/{id}/roles |                |                      |
| **Dependencias**   | HU-05 (Restringir el acceso según el rol y el programa)                                              |                |                      |

**Reglas de negocio**

- El correo (Usuarios.Email) es único en el sistema.

- Roles disponibles: Administrador, Director, Coordinador, Investigador
  y Externo. Un usuario puede tener varios roles (UsuarioRoles).

- Los usuarios no se eliminan físicamente: se desactivan con TieneAcceso
  = 0.

- Un usuario Externo que representa a una entidad debe quedar asociado a
  un Aliado (Usuarios.AliadoId).

- Al crear un usuario se envía un correo de bienvenida con un enlace
  para definir la contraseña (mismo mecanismo de restablecimiento, con
  vigencia de 72 horas).

- El Administrador no puede desactivarse a sí mismo ni quitar el rol al
  último Administrador activo.

**Criterios de aceptación**

**CA-1. Dado** el formulario con Nombres, Apellidos, TipoDocumento,
NumeroDocumento, Email, Teléfono, Cargo, Dependencia, CvLacUrl, ORCID y
roles, **cuando** el Administrador lo guarda, **entonces** el usuario
queda creado y recibe el correo de bienvenida.

**CA-2. Dado** un correo que ya está registrado, **cuando** el
Administrador intenta crear el usuario, **entonces** ve el error "Ya
existe un usuario con este correo".

**CA-3. Dado** un usuario activo, **cuando** el Administrador lo
desactiva, **entonces** el usuario ya no puede iniciar sesión y sus
tokens de refresco quedan revocados de inmediato.

**CA-4. Dado** el listado de usuarios, **cuando** el Administrador lo
consulta, **entonces** ve una tabla paginada (20 por página) con
búsqueda por nombre, correo o documento y filtros por rol y estado.

**CA-5. Dado** un ORCID o una URL de CvLAC, **cuando** se guardan,
**entonces** el ORCID se valida con el formato 0000-0000-0000-000X y el
CvLAC como URL del dominio scienti.minciencias.gov.co.

**CA-6. Dado** cualquier creación o edición de un usuario, **cuando** se
guarda, **entonces** queda en LogActividades con los valores anteriores
y los nuevos (sin incluir el hash de la contraseña).

HU-05 · Restringir el acceso según el rol y el programa

|                                                                                                                                                                                                                                          |
|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| **Como** Director del Centro**, quiero** que cada usuario solo vea y modifique la información que le corresponde según su rol y su programa**, para** proteger la información de los proyectos y cumplir el modelo de permisos aprobado. |

|                    |                                                               |                |                      |
|--------------------|---------------------------------------------------------------|----------------|----------------------|
| **Prioridad**      | **Must (imprescindible)**                                     | **Estimación** | 8 puntos de historia |
| **Épica**          | EP-01 Seguridad y control de acceso                           | **Sprint**     | Sprint 1             |
| **Tablas (ER v3)** | Roles, UsuarioRoles, ProgramaIntegrantes, ProyectoIntegrantes |                |                      |
| **Endpoints**      | Transversal a todos los endpoints del MVP                     |                |                      |
| **Dependencias**   | HT-03                                                         |                |                      |

**Reglas de negocio**

- Se aplica la matriz de permisos del Anexo A.

- Un Coordinador solo modifica información de los programas donde
  ProgramaIntegrantes.RolEnPrograma = "Coordinador" y la vinculación
  está vigente (FechaFin nula o futura).

- Un Investigador solo accede a los proyectos donde es integrante
  (ProyectoIntegrantes).

- El rol Externo no accede a los módulos internos en el MVP; solo a su
  perfil y a sus notificaciones.

- La autorización se valida en el backend en cada petición; ocultar
  opciones en la interfaz no es suficiente.

**Criterios de aceptación**

**CA-1. Dado** un Coordinador del programa A, **cuando** intenta editar
por API un proyecto del programa B, **entonces** la API responde 403 y
el intento queda registrado en LogActividades.

**CA-2. Dado** un Investigador, **cuando** consulta /proyectos/{id} de
un proyecto donde no es integrante, **entonces** la API responde 404
(para no revelar su existencia).

**CA-3. Dado** un usuario con rol Investigador, **cuando** inicia
sesión, **entonces** el menú lateral solo muestra las opciones
permitidas (no ve "Usuarios" ni "Tablero del centro").

**CA-4. Dado** un usuario con varios roles, **cuando** accede al
sistema, **entonces** sus permisos son la unión de los permisos de sus
roles.

**CA-5.** Existen pruebas de integración automatizadas que cubren todas
las celdas de la matriz del Anexo A para los endpoints del MVP.

**Notas técnicas**

- *Políticas de autorización de ASP.NET (RequireRole) más manejadores de
  autorización por recurso (programa/proyecto). Los programas viajan
  como claims en el JWT, pero en las operaciones de escritura se
  verifican contra la base de datos.*

EP-02 · Estructura organizacional y aliados

**Valor:** Modela los 5 programas, sus coordinadores, investigadores,
grupos y aliados. Todo proyecto, indicador y contenido público depende
de esta estructura.

**Procesos:** P1, P15 (parcial) **Módulos ER:** M02 Organización, M11
Entorno Empresarial

HU-06 · Administrar programas, objetivos e integrantes

|                                                                                                                                                                                                                                                                                                      |
|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| **Como** Director del Centro**, quiero** mantener la información de los 5 programas del centro, sus objetivos específicos y sus integrantes (Coordinador e Investigadores)**, para** que la estructura del centro y sus responsables estén disponibles para proyectos, permisos y el portal público. |

|                    |                                                                                                                     |                |                      |
|--------------------|---------------------------------------------------------------------------------------------------------------------|----------------|----------------------|
| **Prioridad**      | **Must (imprescindible)**                                                                                           | **Estimación** | 5 puntos de historia |
| **Épica**          | EP-02 Estructura organizacional y aliados                                                                           | **Sprint**     | Sprint 2             |
| **Tablas (ER v3)** | Programas, ObjetivosEspecificos, ProgramaIntegrantes, Usuarios                                                      |                |                      |
| **Endpoints**      | GET/PUT /api/v1/programas · CRUD /api/v1/programas/{id}/objetivos · GET/POST/PUT /api/v1/programas/{id}/integrantes |                |                      |
| **Dependencias**   | HU-04 (Administrar usuarios y roles)                                                                                |                |                      |

**Reglas de negocio**

- Los 5 programas se cargan como datos semilla: Biotecnología y
  Agroindustria; Bioingeniería; Inteligencia Artificial, Modelado y
  Simulación; Sistemas Computacionales y Bioinformática; Ciudades
  Inteligentes.

- Los objetivos específicos tienen un orden (ObjetivosEspecificos.Orden)
  que se respeta en todas las vistas.

- Un usuario puede pertenecer a varios programas con roles distintos (p.
  ej. Coordinador de uno e Investigador de otro).

- El retiro de un integrante se registra con FechaFin; no se borra el
  registro.

- La adscripción vigente a un programa es requisito para ser
  Investigador Principal de un proyecto en ese programa (P1.1).

- Solo el Director y el Administrador asignan Coordinadores; un
  Coordinador puede vincular o retirar Investigadores de su propio
  programa.

- Cada programa tiene como máximo un Coordinador vigente; para asignar
  otro, primero se cierra la vinculación del anterior.

**Criterios de aceptación**

**CA-1. Dado** el Director en el detalle de un programa, **cuando**
edita el nombre o la descripción y guarda, **entonces** los cambios se
reflejan en la plataforma y en el portal público en la siguiente
revalidación.

**CA-2. Dado** la lista de objetivos de un programa, **cuando** el
Director agrega, edita, elimina o reordena objetivos (arrastrar y
soltar), **entonces** el nuevo orden se guarda y se muestra en todas las
vistas.

**CA-3. Dado** un programa, **cuando** el usuario autorizado consulta
los integrantes, **entonces** ve las pestañas "Vigentes" e "Históricos"
con nombre, rol, fecha de inicio y fecha de fin.

**CA-4. Dado** una FechaFin anterior a la FechaInicio, **cuando** se
intenta guardar la vinculación, **entonces** la API responde 422 con el
mensaje correspondiente.

**CA-5. Dado** un usuario vinculado a un programa, **cuando** se guarda
la vinculación, **entonces** el usuario recibe una notificación y, en su
siguiente renovación de sesión, sus permisos incluyen ese programa.

HU-07 · Gestionar el catálogo de aliados

|                                                                                                                                                                                                                                                                                            |
|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| **Como** Director o Coordinador de programa**, quiero** registrar entidades aliadas (universidades, empresas, entidades públicas, ONG, organismos internacionales) y asociarlas a programas**, para** reutilizarlas como entidades convocantes, cofinanciadoras y socias de los proyectos. |

|                    |                                                           |                |                      |
|--------------------|-----------------------------------------------------------|----------------|----------------------|
| **Prioridad**      | **Must (imprescindible)**                                 | **Estimación** | 3 puntos de historia |
| **Épica**          | EP-02 Estructura organizacional y aliados                 | **Sprint**     | Sprint 2             |
| **Tablas (ER v3)** | Aliados, TiposAliado, ProgramaAliados                     |                |                      |
| **Endpoints**      | CRUD /api/v1/aliados · PUT /api/v1/programas/{id}/aliados |                |                      |
| **Dependencias**   | HU-06 (Administrar programas, objetivos e integrantes)    |                |                      |

**Reglas de negocio**

- TiposAliado semilla: Universidad, Empresa, Entidad pública, ONG /
  Fundación, Organismo internacional, Gremio / Clúster.

- El NIT es único y obligatorio para aliados colombianos (con dígito de
  verificación). Para aliados extranjeros el NIT es opcional y el país
  es obligatorio.

- Se precargan los aliados conocidos del centro (p. ej. CHEC, UAM,
  Minciencias, BIOS, Cenicafé, SwissContact, UNAL, UNISARC, Cámara de
  Comercio de Manizales), previa validación de la lista con el Director.

**Criterios de aceptación**

**CA-1. Dado** un NIT ya registrado, **cuando** se intenta crear el
aliado, **entonces** el sistema lo impide y muestra el aliado existente.

**CA-2. Dado** cualquier formulario que requiera un aliado, **cuando**
el usuario escribe parte del nombre, **entonces** un autocompletado
sugiere aliados existentes y permite crear uno nuevo si tiene permiso.

**CA-3. Dado** un aliado asociado a un programa, **cuando** se consulta
el programa en el portal público, **entonces** el aliado aparece en la
sección de aliados del programa.

EP-03 · Ciclo de vida de proyectos de I+D+i

**Valor:** Núcleo del valor del centro: centraliza las propuestas y
proyectos que hoy se gestionan en archivos dispersos, con trazabilidad
desde la formulación hasta la ejecución.

**Procesos:** P1, P2, P5 **Módulos ER:** M03 Proyectos, M04
(FuentesFinanciamiento)

HU-08 · Registrar una propuesta o proyecto

|                                                                                                                                                                                                                                                    |
|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| **Como** Investigador Principal (IP) o Coordinador de programa**, quiero** registrar una nueva propuesta con su información técnica y presupuestal básica**, para** iniciar su trámite institucional y dejar de gestionarla en archivos dispersos. |

|                    |                                                                |                |                      |
|--------------------|----------------------------------------------------------------|----------------|----------------------|
| **Prioridad**      | **Must (imprescindible)**                                      | **Estimación** | 8 puntos de historia |
| **Épica**          | EP-03 Ciclo de vida de proyectos de I+D+i                      | **Sprint**     | Sprint 2             |
| **Tablas (ER v3)** | Proyectos, TiposProyecto, EstadosProyecto, ProyectoIntegrantes |                |                      |
| **Endpoints**      | POST /api/v1/proyectos · PUT /api/v1/proyectos/{id}            |                |                      |
| **Dependencias**   | HU-06 (Administrar programas, objetivos e integrantes)         |                |                      |

**Reglas de negocio**

- El estado inicial es siempre "En elaboración".

- El código se genera automáticamente con el formato
  CI2DT2-{AAAA}-{consecutivo de 3 dígitos}; es único y no editable.

- El IP debe tener adscripción vigente al programa seleccionado
  (ProgramaIntegrantes).

- Un Investigador solo puede crear propuestas en los programas donde
  está adscrito y queda automáticamente como IP. Un Coordinador puede
  crearla en su programa designando al IP.

- Moneda según ISO 4217 (COP, USD, EUR). Si es distinta de COP, la
  TasaCambio es obligatoria (\> 0) y el sistema calcula
  PresupuestoTotalCop = PresupuestoTotal × TasaCambio.

- FechaFin posterior a FechaInicio; título entre 10 y 300 caracteres;
  resumen de máximo 4.000 caracteres.

- TiposProyecto semilla: Investigación básica, Investigación aplicada,
  Desarrollo tecnológico, Innovación, Reto empresarial
  (RequiereHitosFijos = 1) y Consultoría / servicio.

- El IP solo puede editar mientras el estado sea "En elaboración" y la
  propuesta no esté en revisión; después solo editan el Coordinador y el
  Director.

**Criterios de aceptación**

**CA-1. Dado** un IP adscrito al programa, **cuando** diligencia los
campos obligatorios y guarda, **entonces** se crea el proyecto con su
código, en estado "En elaboración", y el IP queda como integrante con
rol "Investigador principal".

**CA-2. Dado** un presupuesto de 10.000 USD con tasa de cambio 4.000,
**cuando** se guarda la propuesta, **entonces** PresupuestoTotalCop =
40.000.000 y se muestra como \$40.000.000 COP.

**CA-3. Dado** un Investigador no adscrito a un programa, **cuando**
intenta crear una propuesta en ese programa, **entonces** el programa no
aparece en la lista y, si se fuerza por API, la respuesta es 422.

**CA-4. Dado** un formulario con cambios sin guardar, **cuando** el
usuario intenta salir de la página, **entonces** se le advierte que
perderá los cambios; puede guardar como borrador sin completar los
campos opcionales.

**CA-5. Dado** la creación exitosa del proyecto, **cuando** se guarda,
**entonces** se registra en LogActividades y se notifica al Coordinador
del programa.

HU-09 · Conformar el equipo del proyecto

|                                                                                                                                                                                                               |
|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| **Como** Investigador Principal**, quiero** agregar coinvestigadores, auxiliares y estudiantes al proyecto con su rol y sus fechas**, para** formalizar el equipo y darle acceso al seguimiento del proyecto. |

|                    |                                                 |                |                      |
|--------------------|-------------------------------------------------|----------------|----------------------|
| **Prioridad**      | **Must (imprescindible)**                       | **Estimación** | 3 puntos de historia |
| **Épica**          | EP-03 Ciclo de vida de proyectos de I+D+i       | **Sprint**     | Sprint 2             |
| **Tablas (ER v3)** | ProyectoIntegrantes, Usuarios                   |                |                      |
| **Endpoints**      | GET/POST/PUT /api/v1/proyectos/{id}/integrantes |                |                      |
| **Dependencias**   | HU-08 (Registrar una propuesta o proyecto)      |                |                      |

**Reglas de negocio**

- Roles en el proyecto: Investigador principal (exactamente uno
  vigente), Coinvestigador, Auxiliar de investigación, Estudiante y
  Asesor externo.

- El integrante debe existir como usuario. Si no existe, el IP solicita
  su creación al Coordinador o al Administrador.

- El cambio de IP solo lo realizan el Coordinador o el Director y queda
  registrado en el log.

- El retiro de un integrante se registra con FechaFin; se conserva el
  historial.

**Criterios de aceptación**

**CA-1. Dado** un usuario existente, **cuando** el IP lo agrega como
Coinvestigador, **entonces** el usuario ve el proyecto en "Mis
proyectos" y recibe una notificación.

**CA-2. Dado** un usuario que ya tiene una vinculación vigente con el
mismo rol, **cuando** se intenta agregar de nuevo, **entonces** la API
responde 422.

**CA-3. Dado** un integrante retirado, **cuando** intenta registrar un
avance, **entonces** no puede hacerlo, pero sus avances anteriores se
conservan con su autoría.

**CA-4. Dado** la pestaña "Equipo", **cuando** se consulta, **entonces**
muestra nombre, rol, fechas y enlaces a CvLAC y ORCID de cada
integrante.

HU-10 · Registrar fuentes de financiamiento y contrapartidas

|                                                                                                                                                                                                                                                            |
|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| **Como** Investigador Principal**, quiero** registrar cada fuente de financiamiento con su tipo de aporte (efectivo o especie), moneda y monto**, para** conocer el presupuesto real del proyecto y las contrapartidas de la Universidad y de los aliados. |

|                    |                                                                                      |                |                      |
|--------------------|--------------------------------------------------------------------------------------|----------------|----------------------|
| **Prioridad**      | **Must (imprescindible)**                                                            | **Estimación** | 3 puntos de historia |
| **Épica**          | EP-03 Ciclo de vida de proyectos de I+D+i                                            | **Sprint**     | Sprint 2             |
| **Tablas (ER v3)** | FuentesFinanciamiento, Aliados                                                       |                |                      |
| **Endpoints**      | CRUD /api/v1/proyectos/{id}/fuentes                                                  |                |                      |
| **Dependencias**   | HU-08 (Registrar una propuesta o proyecto); HU-07 (Gestionar el catálogo de aliados) |                |                      |

**Reglas de negocio**

- TipoFuente: Entidad financiadora, Contrapartida Universidad de Caldas,
  Contrapartida aliado o Recursos propios.

- TipoAporte: Efectivo o Especie. Estado: Proyectada, Confirmada o
  Desembolsada.

- MontoCop se calcula con la TasaCambio cuando la moneda no es COP.

- Una contrapartida de aliado exige seleccionar el aliado (AliadoId).

- Para propuestas vinculadas a convocatorias externas nacionales es
  obligatorio registrar el desglose de contrapartidas de la Universidad
  de Caldas (P1.2); se valida al enviar a revisión.

**Criterios de aceptación**

**CA-1. Dado** varias fuentes registradas, **cuando** se consulta la
pestaña "Financiación", **entonces** se muestran los totales en COP por
tipo de fuente y por tipo de aporte.

**CA-2. Dado** una suma de fuentes distinta del PresupuestoTotalCop,
**cuando** se consulta la pestaña, **entonces** aparece una advertencia
visible (no bloqueante) con la diferencia.

**CA-3. Dado** una contrapartida de aliado sin aliado seleccionado,
**cuando** se intenta guardar, **entonces** la API responde 422.

HU-11 · Enviar la propuesta a revisión y obtener el aval del Director

|                                                                                                                                                                                                              |
|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| **Como** Investigador Principal**, quiero** enviar mi propuesta al Coordinador y luego al Director para obtener el aval institucional**, para** formalizar la postulación con trazabilidad de cada decisión. |

|                    |                                                                                                                                                                                                                     |                |                      |
|--------------------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|----------------|----------------------|
| **Prioridad**      | **Must (imprescindible)**                                                                                                                                                                                           | **Estimación** | 8 puntos de historia |
| **Épica**          | EP-03 Ciclo de vida de proyectos de I+D+i                                                                                                                                                                           | **Sprint**     | Sprint 3             |
| **Tablas (ER v3)** | Proyectos, ProyectoRevisiones, EstadosProyecto, Notificaciones                                                                                                                                                      |                |                      |
| **Endpoints**      | POST /api/v1/proyectos/{id}/enviar-revision · POST /api/v1/proyectos/{id}/revisiones                                                                                                                                |                |                      |
| **Dependencias**   | HU-08 (Registrar una propuesta o proyecto); HU-10 (Registrar fuentes de financiamiento y contrapartidas); HU-22 (Centro de notificaciones en la aplicación); HU-23 (Envío de notificaciones por correo electrónico) |                |                      |

**Reglas de negocio**

- Flujo (P1): En elaboración → revisión del Coordinador → revisión del
  Director → Presentado. Cualquier revisor puede devolver la propuesta
  al IP (compuerta XOR) con observaciones obligatorias de al menos 20
  caracteres.

- La etapa de revisión se registra en ProyectoRevisiones.Etapa
  ("Coordinador" o "Dirección"); el estado del proyecto permanece "En
  elaboración" hasta el aval del Director.

- Cada decisión queda en ProyectoRevisiones (RevisorId, Etapa, Decision,
  EstadoAnteriorId, EstadoNuevoId, Observaciones, Fecha).

- Mientras la propuesta está en revisión, el IP no puede editarla.

- Validaciones antes del envío: campos obligatorios completos, fuentes
  de financiamiento registradas, cartas de compromiso cuando apliquen y,
  si está vinculada a una convocatoria SGR, Documento Conceptual MGA
  adjunto.

- Solo el Director otorga el estado "Presentado" (aval digital).

**Criterios de aceptación**

**CA-1. Dado** una propuesta completa, **cuando** el IP hace clic en
"Enviar a revisión", **entonces** la propuesta pasa a la etapa "Revisión
coordinador", el Coordinador recibe una notificación (en la aplicación y
por correo) y el IP ya no puede editarla.

**CA-2. Dado** una propuesta en revisión del Coordinador, **cuando** el
Coordinador la devuelve con observaciones, **entonces** vuelve a edición
del IP, quien recibe una notificación con las observaciones, visibles
también en la ficha.

**CA-3. Dado** una propuesta en revisión del Coordinador, **cuando** el
Coordinador la aprueba, **entonces** pasa a la etapa "Revisión
dirección" y se notifica al Director.

**CA-4. Dado** una propuesta en revisión de la Dirección, **cuando** el
Director otorga el aval, **entonces** el estado cambia a "Presentado",
se registran fecha y usuario del aval, y se notifica al IP y al
Coordinador.

**CA-5. Dado** un revisor que intenta devolver la propuesta sin
observaciones, **cuando** confirma la acción, **entonces** el sistema no
lo permite.

**CA-6. Dado** requisitos incompletos, **cuando** el IP abre la
propuesta, **entonces** el botón "Enviar a revisión" está deshabilitado
y se muestra una lista de verificación con los pendientes.

**CA-7. Dado** cualquier propuesta, **cuando** se abre la pestaña
"Historial de revisiones", **entonces** se ve la línea de tiempo
cronológica de todas las decisiones.

**Notas técnicas**

- *Implementar como máquina de estados explícita en el dominio para
  impedir transiciones no válidas.*

HU-12 · Registrar la aprobación e inicio de ejecución del proyecto

|                                                                                                                                                                                                                               |
|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| **Como** Director del Centro**, quiero** registrar el resultado de la postulación y, si es aprobada, el acto administrativo y el inicio de la ejecución**, para** que el proyecto pase formalmente a la etapa de seguimiento. |

|                    |                                                                                                                    |                |                      |
|--------------------|--------------------------------------------------------------------------------------------------------------------|----------------|----------------------|
| **Prioridad**      | **Must (imprescindible)**                                                                                          | **Estimación** | 5 puntos de historia |
| **Épica**          | EP-03 Ciclo de vida de proyectos de I+D+i                                                                          | **Sprint**     | Sprint 3             |
| **Tablas (ER v3)** | Proyectos, ProyectoRevisiones, Documentos, Cronogramas                                                             |                |                      |
| **Endpoints**      | POST /api/v1/proyectos/{id}/aprobacion · POST /api/v1/proyectos/{id}/iniciar-ejecucion                             |                |                      |
| **Dependencias**   | HU-11 (Enviar la propuesta a revisión y obtener el aval del Director); HU-21 (Repositorio documental de proyectos) |                |                      |

**Reglas de negocio**

- Transiciones (P2): Presentado → Aprobado (requiere acto administrativo
  en PDF) o Presentado → No aprobado (estado final).

- Aprobado → En ejecución requiere la fecha de inicio real y el plan de
  trabajo aprobado como documento.

- Si el presupuesto aprobado difiere del solicitado, se actualiza
  PresupuestoTotal y el cambio queda en el log.

- Al pasar a "Aprobado" se crea automáticamente el Cronograma del
  proyecto.

- El IP debe confirmar en su tablero que la información registrada es
  correcta (P2).

**Criterios de aceptación**

**CA-1. Dado** un proyecto "Presentado", **cuando** el Director registra
la aprobación con el acto administrativo adjunto, **entonces** el estado
pasa a "Aprobado", se crea el cronograma y se notifica al equipo.

**CA-2. Dado** un proyecto "Presentado", **cuando** el Director intenta
aprobarlo sin acto administrativo, **entonces** la API responde 422.

**CA-3. Dado** un proyecto "Aprobado", **cuando** el Director registra
la fecha de inicio y el plan de trabajo, **entonces** el estado pasa a
"En ejecución".

**CA-4. Dado** un proyecto recién aprobado, **cuando** el IP ingresa a
la plataforma, **entonces** ve un aviso "Confirme la información del
proyecto aprobado" y, al confirmar, la confirmación queda registrada.

**CA-5. Dado** una transición no válida (p. ej. En elaboración → En
ejecución), **cuando** se intenta por API, **entonces** la respuesta es
422 con el mensaje "Transición de estado no permitida".

**Notas técnicas**

- *Requiere agregar "No aprobado" al catálogo EstadosProyecto (EsFinal =
  1). Es un cambio de datos semilla, no de estructura.*

HU-13 · Consultar mis proyectos y los proyectos del programa

|                                                                                                                                                                                                                  |
|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| **Como** Investigador, Coordinador o Director**, quiero** ver un listado de proyectos con filtros y una ficha detallada de cada uno**, para** encontrar rápidamente la información y el estado de cada proyecto. |

|                    |                                                                                                       |                |                      |
|--------------------|-------------------------------------------------------------------------------------------------------|----------------|----------------------|
| **Prioridad**      | **Must (imprescindible)**                                                                             | **Estimación** | 5 puntos de historia |
| **Épica**          | EP-03 Ciclo de vida de proyectos de I+D+i                                                             | **Sprint**     | Sprint 2             |
| **Tablas (ER v3)** | Proyectos y tablas relacionadas                                                                       |                |                      |
| **Endpoints**      | GET /api/v1/proyectos/mios · GET /api/v1/proyectos/programa/{programaId} · GET /api/v1/proyectos/{id} |                |                      |
| **Dependencias**   | HU-08 (Registrar una propuesta o proyecto); HU-05 (Restringir el acceso según el rol y el programa)   |                |                      |

**Reglas de negocio**

- El Investigador ve "Mis proyectos" (vinculaciones vigentes e
  históricas); el Coordinador ve todos los de su programa; el Director y
  el Administrador ven todos.

**Criterios de aceptación**

**CA-1. Dado** el listado de proyectos, **cuando** el usuario lo
consulta, **entonces** ve las columnas código, título, programa, IP,
estado (etiqueta de color), % de avance (barra), fecha de fin y próximo
hito.

**CA-2. Dado** el listado, **cuando** el usuario aplica filtros por
programa, estado, tipo, año e IP, o busca por texto, **entonces** los
resultados se filtran con paginación en el servidor y se pueden ordenar
por columna.

**CA-3. Dado** un proyecto, **cuando** el usuario abre su ficha,
**entonces** ve las pestañas General, Equipo, Financiación,
Convocatorias, Cronograma, Avances, Documentos y Revisiones, según sus
permisos.

**CA-4. Dado** una base de 500 proyectos, **cuando** se consulta el
listado, **entonces** la respuesta tarda menos de 2 segundos.

**Notas técnicas**

- *Consultas de listado con Dapper; mutaciones con EF Core, según las
  convenciones del proyecto.*

HU-14 · Carga inicial de los proyectos y propuestas existentes

|                                                                                                                                                                                                                                                                      |
|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| **Como** Director del Centro**, quiero** que los proyectos, propuestas e integrantes que el centro ya tiene en curso queden cargados antes del lanzamiento**, para** que la plataforma arranque con información real y los programas no tengan que digitarla a mano. |

|                    |                                                                                                                                                                                                                                 |                |                      |
|--------------------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|----------------|----------------------|
| **Prioridad**      | **Must (imprescindible)**                                                                                                                                                                                                       | **Estimación** | 3 puntos de historia |
| **Épica**          | EP-03 Ciclo de vida de proyectos de I+D+i                                                                                                                                                                                       | **Sprint**     | Sprint 4             |
| **Tablas (ER v3)** | Proyectos, ProyectoIntegrantes, FuentesFinanciamiento, PropuestasConvocatoria                                                                                                                                                   |                |                      |
| **Endpoints**      | Script de importación (CLI) sobre plantilla CSV, ejecutado por el equipo de desarrollo                                                                                                                                          |                |                      |
| **Dependencias**   | HU-08 (Registrar una propuesta o proyecto); HU-09 (Conformar el equipo del proyecto); HU-10 (Registrar fuentes de financiamiento y contrapartidas); HU-16 (Vincular la propuesta a una convocatoria y registrar la postulación) |                |                      |

**Reglas de negocio**

- Plantilla CSV acordada con los Coordinadores; cada programa entrega
  sus datos a más tardar en la semana 6.

- El script valida todo el archivo y reporta errores por fila; si hay
  errores no importa nada (todo o nada).

- Los proyectos importados conservan su estado real (incluido
  "Finalizado") y quedan marcados en el log con origen "Importación".

**Criterios de aceptación**

**CA-1. Dado** un archivo con filas válidas, **cuando** el Administrador
lo valida y confirma, **entonces** se crean los proyectos con sus
integrantes y fuentes, y se muestra un resumen de registros creados.

**CA-2. Dado** un archivo con un IP inexistente en la fila 12,
**cuando** se valida, **entonces** el reporte indica "Fila 12: el
usuario con correo X no existe" y no se importa ningún registro.

**Notas técnicas**

- *Esta historia es clave para la adopción: sin datos iniciales, el
  tablero del Director no aporta valor el día del lanzamiento.*

EP-04 · Convocatorias y postulaciones

**Valor:** Permite saber a qué convocatorias se presenta el centro, con
qué montos y con qué resultado; es la principal fuente de recursos del
centro.

**Procesos:** P1.1 a P1.5 **Módulos ER:** M04 Convocatorias y
Financiamiento

HU-15 · Registrar y mantener el catálogo de convocatorias

|                                                                                                                                                                                                                         |
|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| **Como** Director o Coordinador de programa**, quiero** registrar convocatorias con su tipo, entidad convocante, fechas y montos**, para** que los investigadores identifiquen oportunidades y vinculen sus propuestas. |

|                    |                                                       |                |                      |
|--------------------|-------------------------------------------------------|----------------|----------------------|
| **Prioridad**      | **Must (imprescindible)**                             | **Estimación** | 3 puntos de historia |
| **Épica**          | EP-04 Convocatorias y postulaciones                   | **Sprint**     | Sprint 3             |
| **Tablas (ER v3)** | Convocatorias, TiposConvocatoria, Aliados, Documentos |                |                      |
| **Endpoints**      | CRUD /api/v1/convocatorias                            |                |                      |
| **Dependencias**   | HU-07 (Gestionar el catálogo de aliados)              |                |                      |

**Reglas de negocio**

- TiposConvocatoria semilla: Interna (VIP UCaldas), Externa nacional,
  Externa internacional, Minciencias – Fondo FJC, Minciencias – SGR
  (Fondo CTeI) y Conjunta.

- La entidad convocante se selecciona del catálogo de aliados.

- FechaCierre no puede ser anterior a FechaApertura.

- Se registran moneda, monto máximo, URL de los términos de referencia y
  plataforma de radicación (SIGP, MGA Web, portal VIP, plataforma del
  otorgante). Los términos de referencia pueden adjuntarse como
  documento.

**Criterios de aceptación**

**CA-1. Dado** el listado de convocatorias, **cuando** se consulta,
**entonces** cada convocatoria muestra un estado calculado por fechas:
Próxima, Abierta o Cerrada.

**CA-2. Dado** el listado, **cuando** el usuario filtra por tipo,
entidad o estado, **entonces** se muestran solo las convocatorias que
cumplen los filtros.

**CA-3. Dado** una convocatoria abierta, **cuando** se guarda,
**entonces** aparece en el portal público de convocatorias en la
siguiente revalidación.

HU-16 · Vincular la propuesta a una convocatoria y registrar la
postulación

|                                                                                                                                                                                                                                     |
|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| **Como** Investigador Principal**, quiero** asociar mi propuesta a la convocatoria a la que se presenta y registrar los datos de radicación externa y el resultado**, para** hacer seguimiento del proceso en la entidad otorgante. |

|                    |                                                                                                                                                    |                |                      |
|--------------------|----------------------------------------------------------------------------------------------------------------------------------------------------|----------------|----------------------|
| **Prioridad**      | **Must (imprescindible)**                                                                                                                          | **Estimación** | 3 puntos de historia |
| **Épica**          | EP-04 Convocatorias y postulaciones                                                                                                                | **Sprint**     | Sprint 3             |
| **Tablas (ER v3)** | PropuestasConvocatoria, Convocatorias, Documentos                                                                                                  |                |                      |
| **Endpoints**      | CRUD /api/v1/proyectos/{id}/postulaciones                                                                                                          |                |                      |
| **Dependencias**   | HU-15 (Registrar y mantener el catálogo de convocatorias); HU-08 (Registrar una propuesta o proyecto); HU-21 (Repositorio documental de proyectos) |                |                      |

**Reglas de negocio**

- MontoSolicitado no puede superar el MontoMaximo de la convocatoria.

- Si la moneda no es COP, TasaCambioEstimada es obligatoria y el sistema
  calcula MontoSolicitadoCop (P1.3).

- Para convocatorias externas, IdPostulacionExterna y el comprobante de
  radicación son obligatorios para marcar la postulación como radicada.

- Convocatorias SGR: CodigoMga obligatorio; FaseExterna: Banco de
  elegibles → Viabilización OCAD → Aprobación y designación de ejecutor
  (P1.5).

- FaseExterna es texto de un catálogo simple (p. ej. Banco de elegibles,
  Viabilización OCAD, Banco preliminar). Resultado: Pendiente, Aprobada,
  No aprobada o Elegible.

**Criterios de aceptación**

**CA-1. Dado** una convocatoria SGR, **cuando** el IP registra la
postulación sin código MGA, **entonces** la API responde 422.

**CA-2. Dado** un monto solicitado mayor al máximo, **cuando** se
intenta guardar, **entonces** ve el error con el monto máximo permitido.

**CA-3. Dado** una postulación, **cuando** se registra el resultado
"Aprobada", **entonces** el Director recibe una notificación para
registrar la aprobación del proyecto.

EP-05 · Cronogramas, hitos y alertas

**Valor:** Reduce el riesgo de incumplir entregables ante entidades
financiadoras mediante alertas automáticas.

**Procesos:** P4 **Módulos ER:** M05 Cronogramas y Seguimiento

HU-17 · Definir, completar y reprogramar los hitos del proyecto

|                                                                                                                                                                                                                            |
|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| **Como** Investigador Principal**, quiero** definir los hitos críticos del proyecto con fecha límite, responsable y días de anticipación para la alerta**, para** planear la ejecución y que el sistema me avise a tiempo. |

|                    |                                                                                                   |                |                      |
|--------------------|---------------------------------------------------------------------------------------------------|----------------|----------------------|
| **Prioridad**      | **Must (imprescindible)**                                                                         | **Estimación** | 5 puntos de historia |
| **Épica**          | EP-05 Cronogramas, hitos y alertas                                                                | **Sprint**     | Sprint 3             |
| **Tablas (ER v3)** | Cronogramas, Hitos, TiposHito                                                                     |                |                      |
| **Endpoints**      | GET/POST/PUT /api/v1/cronogramas · GET/POST/PUT /api/v1/hitos · POST /api/v1/hitos/{id}/completar |                |                      |
| **Dependencias**   | HU-12 (Registrar la aprobación e inicio de ejecución del proyecto)                                |                |                      |

**Reglas de negocio**

- En el MVP el cronograma solo se asocia a proyectos
  (Cronogramas.ProyectoId); FormacionId y EventoId quedan nulos hasta
  las siguientes fases.

- TiposHito semilla: Entregable, Informe de avance, Informe final,
  Reunión técnica, Desembolso / pago, Sprint y Entrega de MVP.

- AlertarDias toma por defecto 7 y admite valores entre 1 y 60.

- Si la fecha límite queda fuera de la vigencia del proyecto, se muestra
  una advertencia.

- El responsable debe ser un integrante vigente del proyecto.

- Proyectos tipo "Reto empresarial" (RequiereHitosFijos = 1): mínimo 3
  hitos de tipo Sprint y 1 Entrega de MVP antes de pasar a "En
  ejecución" (P11).

- El IP solo gestiona los hitos de sus propios proyectos (P4).

- Estados del hito: Pendiente, Completado, Vencido (lo asigna el
  sistema) y Cancelado.

- Completar registra FechaCompletado y permite vincular un avance o un
  documento como evidencia.

- Reprogramar exige una justificación; la fecha anterior y la nueva
  quedan en el log y se notifica al Coordinador.

**Criterios de aceptación**

**CA-1. Dado** un proyecto aprobado, **cuando** el IP agrega un hito con
título, tipo, fecha límite, responsable y días de alerta, **entonces**
el hito aparece en la lista ordenada por fecha y en una vista de línea
de tiempo.

**CA-2. Dado** un responsable que no pertenece al proyecto, **cuando**
se intenta guardar el hito, **entonces** la API responde 422.

**CA-3. Dado** un Reto empresarial con solo 2 hitos tipo Sprint,
**cuando** el Director intenta iniciar la ejecución, **entonces** el
sistema lo impide e indica los hitos faltantes.

**CA-4. Dado** un hito pendiente, **cuando** el responsable lo marca
como completado con evidencia, **entonces** el estado pasa a
"Completado" con la fecha del día.

**CA-5. Dado** un hito pendiente, **cuando** el IP cambia la fecha
límite sin justificación, **entonces** el sistema no permite guardar.

HU-18 · Motor automático de alertas de hitos

|                                                                                                                                                                                                                                  |
|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| **Como** Coordinador de programa**, quiero** que el sistema avise automáticamente al responsable, al IP y a mí cuando un hito esté próximo a vencer o vencido**, para** evitar incumplimientos ante las entidades financiadoras. |

|                    |                                                                                                                         |                |                      |
|--------------------|-------------------------------------------------------------------------------------------------------------------------|----------------|----------------------|
| **Prioridad**      | **Must (imprescindible)**                                                                                               | **Estimación** | 5 puntos de historia |
| **Épica**          | EP-05 Cronogramas, hitos y alertas                                                                                      | **Sprint**     | Sprint 4             |
| **Tablas (ER v3)** | Hitos, Notificaciones                                                                                                   |                |                      |
| **Endpoints**      | GET /api/v1/alertas/pendientes · GET /api/v1/dashboard/hitos-proximos · POST /api/v1/jobs/alertas (protegido)           |                |                      |
| **Dependencias**   | HU-17 (Definir, completar y reprogramar los hitos del proyecto); HU-23 (Envío de notificaciones por correo electrónico) |                |                      |

**Reglas de negocio**

- Se ejecuta diariamente a las 6:00 a. m. (hora de Colombia).

- Próximo a vencer: hoy ≥ FechaLimite − AlertarDias y estado Pendiente →
  alerta al responsable y al IP. Se envía una sola vez al alcanzar el
  umbral y otra el día del vencimiento.

- Vencido: FechaLimite \< hoy y estado Pendiente → el hito pasa a
  "Vencido" y se alerta al responsable, al IP y al Coordinador.

- Idempotente: si la tarea se reejecuta el mismo día no duplica
  notificaciones.

**Criterios de aceptación**

**CA-1. Dado** un hito pendiente con fecha límite en 7 días y
AlertarDias = 7, **cuando** se ejecuta la tarea, **entonces** el
responsable y el IP reciben la alerta en la aplicación y por correo.

**CA-2. Dado** un hito pendiente cuya fecha límite fue ayer, **cuando**
se ejecuta la tarea, **entonces** el hito pasa a "Vencido" y se notifica
también al Coordinador.

**CA-3. Dado** la tarea ya ejecutada hoy, **cuando** se ejecuta de
nuevo, **entonces** no se crean notificaciones duplicadas.

**CA-4. Dado** un usuario autenticado, **cuando** consulta "Mis
alertas", **entonces** ve los hitos próximos (30 días) y vencidos de sus
proyectos.

**Notas técnicas**

- *Riesgo: en hosting compartido (MonsterASP) el proceso puede
  reciclarse por inactividad y detener tareas en segundo plano. Se
  recomienda disparar el job desde n8n (ya en el stack) con un cron que
  llame a un endpoint protegido por API key, o desde un programador
  externo.*

EP-06 · Avances y seguimiento

**Valor:** Da visibilidad real del progreso de cada proyecto y formaliza
la validación mensual por parte del IP y del Coordinador.

**Procesos:** P3 **Módulos ER:** M05 Cronogramas y Seguimiento

HU-19 · Registrar avances del proyecto

|                                                                                                                                                                                           |
|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| **Como** integrante de un proyecto**, quiero** registrar avances con una descripción, el porcentaje acumulado y evidencias adjuntas**, para** documentar el progreso de la investigación. |

|                    |                                                                                                                 |                |                      |
|--------------------|-----------------------------------------------------------------------------------------------------------------|----------------|----------------------|
| **Prioridad**      | **Must (imprescindible)**                                                                                       | **Estimación** | 3 puntos de historia |
| **Épica**          | EP-06 Avances y seguimiento                                                                                     | **Sprint**     | Sprint 4             |
| **Tablas (ER v3)** | Avances, AvancesAdjuntos, Hitos                                                                                 |                |                      |
| **Endpoints**      | POST /api/v1/proyectos/{id}/avances · GET /api/v1/proyectos/{id}/avances                                        |                |                      |
| **Dependencias**   | HU-12 (Registrar la aprobación e inicio de ejecución del proyecto); HU-21 (Repositorio documental de proyectos) |                |                      |

**Reglas de negocio**

- Solo se registran avances en proyectos "En ejecución".

- PorcentajeAvance es el acumulado (0–100) y no puede ser menor que el
  último avance validado.

- Hasta 5 adjuntos por avance, de máximo 20 MB cada uno (PDF, DOCX,
  XLSX, CSV, JPG, PNG).

- OrigenRegistro = "Web" en el MVP; los campos de GPS quedan nulos y
  preparados para la futura aplicación móvil.

- IdCliente (GUID generado por el cliente) evita duplicados por doble
  envío.

- EstadoValidacion inicial: "Pendiente". El avance puede vincularse a un
  hito.

- El autor puede editar el avance solo mientras esté "Pendiente".

**Criterios de aceptación**

**CA-1. Dado** un integrante vigente de un proyecto en ejecución,
**cuando** registra un avance con título, descripción, porcentaje y
adjuntos, **entonces** el avance queda "Pendiente" y se notifica al IP y
al Coordinador.

**CA-2. Dado** un porcentaje menor al último validado, **cuando** se
intenta guardar, **entonces** la API responde 422.

**CA-3. Dado** un doble clic en "Guardar", **cuando** se envían dos
peticiones con el mismo IdCliente, **entonces** solo se crea un avance.

**CA-4. Dado** un archivo de 30 MB o de un tipo no permitido, **cuando**
se intenta adjuntar, **entonces** el cliente lo rechaza antes de subirlo
con un mensaje claro.

HU-20 · Validar avances

|                                                                                                                                                                                                                           |
|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| **Como** Coordinador de programa o Investigador Principal**, quiero** revisar los avances pendientes y validarlos con una rúbrica y observaciones**, para** asegurar la calidad del seguimiento mensual de los proyectos. |

|                    |                                                                         |                |                      |
|--------------------|-------------------------------------------------------------------------|----------------|----------------------|
| **Prioridad**      | **Should (importante)**                                                 | **Estimación** | 3 puntos de historia |
| **Épica**          | EP-06 Avances y seguimiento                                             | **Sprint**     | Sprint 4             |
| **Tablas (ER v3)** | AvanceValidaciones, Avances, Proyectos                                  |                |                      |
| **Endpoints**      | GET /api/v1/avances/pendientes · POST /api/v1/avances/{id}/validaciones |                |                      |
| **Dependencias**   | HU-19 (Registrar avances del proyecto)                                  |                |                      |

**Reglas de negocio**

- El IP valida los avances de su equipo y el Coordinador los de los
  proyectos de su programa. RolValidador registra "IP" o "Coordinador".

- Resultado: Aprobado o Requiere ajustes. En el MVP no se usa rúbrica
  (PuntajeRubrica queda nulo); "Requiere ajustes" exige observaciones y
  notifica al autor.

- Cuando el Coordinador aprueba un avance, Proyectos.PorcentajeAvance se
  actualiza con el porcentaje de ese avance.

**Criterios de aceptación**

**CA-1. Dado** avances pendientes de validar, **cuando** el validador
abre la bandeja "Avances por validar", **entonces** ve la lista con un
contador, filtrable por proyecto y periodo.

**CA-2. Dado** un avance pendiente, **cuando** el Coordinador lo aprueba
con puntaje, **entonces** el avance queda "Aprobado" y el porcentaje del
proyecto se actualiza.

**CA-3. Dado** un avance, **cuando** el validador elige "Requiere
ajustes" sin observaciones, **entonces** el sistema no lo permite.

**CA-4. Dado** un avance con "Requiere ajustes", **cuando** el autor lo
corrige, **entonces** vuelve a "Pendiente" y conserva el historial de
validaciones.

EP-07 · Gestión documental

**Valor:** Centraliza soportes (propuestas, actos administrativos,
cartas, informes) con integridad verificable por hash y control de
versiones.

**Procesos:** Transversal (P1, P2, P5) **Módulos ER:** M06 Documentos

HU-21 · Repositorio documental de proyectos

|                                                                                                                                                                                                                                                         |
|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| **Como** integrante de un proyecto o directivo del centro**, quiero** cargar, versionar y descargar documentos (propuestas, actos administrativos, cartas, informes)**, para** centralizar los soportes y no depender de correos o carpetas personales. |

|                    |                                                                                                                 |                |                      |
|--------------------|-----------------------------------------------------------------------------------------------------------------|----------------|----------------------|
| **Prioridad**      | **Must (imprescindible)**                                                                                       | **Estimación** | 3 puntos de historia |
| **Épica**          | EP-07 Gestión documental                                                                                        | **Sprint**     | Sprint 2             |
| **Tablas (ER v3)** | Documentos, TiposDocumento                                                                                      |                |                      |
| **Endpoints**      | POST /api/v1/documentos · GET /api/v1/documentos?entidadTipo=&entidadId= · GET /api/v1/documentos/{id}/descarga |                |                      |
| **Dependencias**   | HU-05 (Restringir el acceso según el rol y el programa)                                                         |                |                      |

**Reglas de negocio**

- Modelo genérico por EntidadTipo / EntidadId (Proyecto, Convocatoria,
  PropuestaConvocatoria, Avance, ProyectoCierre).

- TiposDocumento semilla: Propuesta técnica, Presupuesto, Acto
  administrativo de aprobación, Carta de compromiso, Plan de trabajo,
  Términos de referencia, Comprobante de radicación, Documento
  conceptual MGA, Informe técnico, Informe financiero, Acta de comité,
  Paz y salvo y Otro.

- Al cargar se calcula el HashSha256 para verificar la integridad.

- Tamaño máximo 25 MB. Formatos: PDF, DOCX, XLSX, PPTX, JPG, PNG y ZIP.

- EsPublico = 0 por defecto. La descarga exige permiso sobre la entidad
  y se hace mediante URL firmada temporal, nunca una ruta pública.

- Los documentos que soportan actos (aprobación, cierre) no se eliminan.

- En el MVP no hay control de versiones: una nueva carga crea un
  documento adicional.

**Criterios de aceptación**

**CA-1. Dado** un integrante del proyecto, **cuando** carga un PDF de 3
MB como "Plan de trabajo", **entonces** el documento queda registrado
con su hash, versión 1, autor y fecha.

**CA-2. Dado** un usuario sin permiso sobre el proyecto, **cuando**
intenta descargar un documento, **entonces** la API responde 403.

**CA-3. Dado** un documento descargado, **cuando** se recalcula su
SHA-256, **entonces** coincide con el hash almacenado.

**Notas técnicas**

- *Verificar la cuota de almacenamiento de MonsterASP. Guardar los
  archivos fuera de wwwroot o en un almacenamiento de objetos externo.*

EP-08 · Notificaciones

**Valor:** Hace que los flujos de revisión, hitos y avances avancen sin
seguimiento manual por correo o mensajería personal.

**Procesos:** P18 **Módulos ER:** M07 Notificaciones

HU-22 · Centro de notificaciones en la aplicación

|                                                                                                                                                                         |
|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| **Como** usuario autenticado**, quiero** ver mis notificaciones en la plataforma y marcarlas como leídas**, para** enterarme de las acciones que requieren mi atención. |

|                    |                                                                                                                  |                |                      |
|--------------------|------------------------------------------------------------------------------------------------------------------|----------------|----------------------|
| **Prioridad**      | **Should (importante)**                                                                                          | **Estimación** | 2 puntos de historia |
| **Épica**          | EP-08 Notificaciones                                                                                             | **Sprint**     | Sprint 3             |
| **Tablas (ER v3)** | Notificaciones                                                                                                   |                |                      |
| **Endpoints**      | GET /api/v1/notificaciones/mias · POST /api/v1/notificaciones/{id}/leer · POST /api/v1/notificaciones/leer-todas |                |                      |
| **Dependencias**   | HU-01 (Iniciar sesión en la plataforma)                                                                          |                |                      |

**Reglas de negocio**

- Cada usuario solo ve sus propias notificaciones.

**Criterios de aceptación**

**CA-1. Dado** notificaciones no leídas, **cuando** el usuario carga
cualquier página, **entonces** ve un ícono de campana con el contador de
no leídas.

**CA-2. Dado** una notificación, **cuando** el usuario hace clic en
ella, **entonces** navega al recurso relacionado (ReferenciaTipo /
ReferenciaId) y la notificación queda leída.

**CA-3. Dado** varias no leídas, **cuando** el usuario elige "Marcar
todas como leídas", **entonces** el contador queda en cero.

HU-23 · Envío de notificaciones por correo electrónico

|                                                                                                                                                               |
|---------------------------------------------------------------------------------------------------------------------------------------------------------------|
| **Como** usuario de la plataforma**, quiero** recibir por correo las notificaciones importantes**, para** enterarme aunque no esté conectado a la plataforma. |

|                    |                                          |                |                      |
|--------------------|------------------------------------------|----------------|----------------------|
| **Prioridad**      | **Must (imprescindible)**                | **Estimación** | 3 puntos de historia |
| **Épica**          | EP-08 Notificaciones                     | **Sprint**     | Sprint 3             |
| **Tablas (ER v3)** | Notificaciones, PreferenciasNotificacion |                |                      |
| **Endpoints**      | Servicio interno de despacho             |                |                      |
| **Dependencias**   | HT-03                                    |                |                      |

**Reglas de negocio**

- Despacho asíncrono (P18): primero se registra la notificación y luego
  un worker la envía por correo con MailKit.

- Plantillas HTML con la identidad del CI²DT² y un enlace directo al
  recurso.

- Hasta 3 reintentos con espera creciente; se registran Intentos,
  UltimoError, EnviadaEmail y FechaEnvioEmail.

- Eventos del MVP: bienvenida, restablecer contraseña, asignación a
  programa o proyecto, propuesta enviada a revisión, devuelta, aprobada
  por el Coordinador y avalada, resultado de postulación, hito próximo o
  vencido, avance registrado, validado o rechazado.

- Las notificaciones push quedan fuera del MVP; EnviadaPush permanece en
  falso.

**Criterios de aceptación**

**CA-1. Dado** un evento disparador, **cuando** ocurre, **entonces** el
destinatario recibe el correo en menos de 5 minutos.

**CA-2. Dado** un fallo del servidor SMTP, **cuando** se intenta el
envío, **entonces** se reintenta hasta 3 veces y el error queda en
UltimoError sin afectar la operación del usuario.

**Notas técnicas**

- *Definir el proveedor de correo: SMTP institucional de la Universidad
  o un servicio transaccional (p. ej. Brevo o SendGrid), considerando
  límites de envío y reputación del dominio.*

EP-09 · Tablero de control

**Valor:** Entrega al Director la información consolidada para la toma
de decisiones y los informes a la VIP.

**Procesos:** P20 **Módulos ER:** M13 Indicadores y Reportes

HU-24 · Tablero de control del Director

|                                                                                                                                                                                                                                      |
|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| **Como** Director del Centro**, quiero** un tablero con el estado consolidado de proyectos, propuestas, financiación y alertas de los 5 programas**, para** tomar decisiones y responder a la VIP sin consolidar información a mano. |

|                    |                                                                                                                                                                                                                                                                                  |                |                      |
|--------------------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|----------------|----------------------|
| **Prioridad**      | **Must (imprescindible)**                                                                                                                                                                                                                                                        | **Estimación** | 5 puntos de historia |
| **Épica**          | EP-09 Tablero de control                                                                                                                                                                                                                                                         | **Sprint**     | Sprint 4             |
| **Tablas (ER v3)** | Proyectos, PropuestasConvocatoria, FuentesFinanciamiento, Hitos, Avances, Convocatorias                                                                                                                                                                                          |                |                      |
| **Endpoints**      | GET /api/v1/dashboard/centro                                                                                                                                                                                                                                                     |                |                      |
| **Dependencias**   | HU-13 (Consultar mis proyectos y los proyectos del programa); HU-16 (Vincular la propuesta a una convocatoria y registrar la postulación); HU-18 (Motor automático de alertas de hitos); HU-20 (Validar avances); HU-14 (Carga inicial de los proyectos y propuestas existentes) |                |                      |

**Reglas de negocio**

- Tarjetas: proyectos en ejecución, propuestas en elaboración o
  revisión, propuestas presentadas en el año, tasa de aprobación
  (aprobadas / con resultado) y monto gestionado en COP.

- Gráfico de proyectos por estado y programa (barras apiladas).

- Tablas: hitos próximos (30 días) y vencidos; avances pendientes de
  validación por programa.

- Filtro por año. Consultas con Dapper (P20).

**Criterios de aceptación**

**CA-1. Dado** el Director, **cuando** abre el tablero, **entonces**
carga en menos de 3 segundos con todos los indicadores.

**CA-2. Dado** un indicador del tablero, **cuando** se compara con el
listado filtrado equivalente, **entonces** las cifras coinciden (prueba
de conciliación automatizada).

**CA-3. Dado** un usuario sin rol Director ni Administrador, **cuando**
intenta acceder, **entonces** la API responde 403.

EP-10 · Portal web institucional

**Valor:** Cara pública del centro: da a conocer programas, proyectos,
capacidades y convocatorias abiertas.

**Procesos:** Instrucción del proyecto; P17 (consentimiento) **Módulos
ER:** Lectura de M02, M03, M04, M11; M14

HU-25 · Página de inicio institucional

|                                                                                                                                                                                                                                        |
|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| **Como** visitante del sitio (empresa, entidad pública, estudiante o investigador externo)**, quiero** conocer qué es el CI²DT², sus objetivos, áreas y capacidades**, para** identificar oportunidades de colaboración con el centro. |

|                    |                                                                                                                      |                |                      |
|--------------------|----------------------------------------------------------------------------------------------------------------------|----------------|----------------------|
| **Prioridad**      | **Must (imprescindible)**                                                                                            | **Estimación** | 3 puntos de historia |
| **Épica**          | EP-10 Portal web institucional                                                                                       | **Sprint**     | Sprint 4             |
| **Tablas (ER v3)** | Lectura de Programas, Proyectos, Aliados, Convocatorias                                                              |                |                      |
| **Endpoints**      | GET /api/v1/publico/resumen                                                                                          |                |                      |
| **Dependencias**   | HU-06 (Administrar programas, objetivos e integrantes); HU-13 (Consultar mis proyectos y los proyectos del programa) |                |                      |

**Reglas de negocio**

- Contenido estático versionado en el frontend (MDX): propósito del
  centro, 5 objetivos estratégicos, áreas, servicios y capacidades, y
  datos de contacto (correo y teléfono del centro).

- Cifras dinámicas desde la API pública: proyectos en ejecución,
  proyectos finalizados, aliados y programas.

- Sección de convocatorias abiertas (FechaCierre ≥ hoy).

- Diseño adaptable de 360 px a 1920 px, con la identidad visual de la
  Universidad de Caldas; accesibilidad WCAG 2.1 AA; metadatos SEO y
  sitemap.xml.

- Renderizado estático con revalidación incremental (ISR) cada 10
  minutos en Vercel.

**Criterios de aceptación**

**CA-1. Dado** un visitante en un teléfono, **cuando** abre la página de
inicio, **entonces** el contenido se ve completo sin desplazamiento
horizontal.

**CA-2. Dado** la página publicada, **cuando** se audita con Lighthouse,
**entonces** obtiene 90 o más en rendimiento, accesibilidad y SEO.

**CA-3. Dado** un nuevo proyecto en ejecución, **cuando** pasan 10
minutos, **entonces** la cifra de proyectos en ejecución se actualiza.

**Notas técnicas**

- *Los logos de los aliados requieren cargarse como Documentos con
  EsPublico = 1, ya que la tabla Aliados no tiene campo de logo.*

HU-26 · Consultar los programas de investigación en el portal

|                                                                                                                                                                   |
|-------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| **Como** visitante del sitio**, quiero** ver el detalle de cada programa de investigación**, para** conocer su enfoque, su equipo y los proyectos que desarrolla. |

|                    |                                                                                            |                |                      |
|--------------------|--------------------------------------------------------------------------------------------|----------------|----------------------|
| **Prioridad**      | **Should (importante)**                                                                    | **Estimación** | 3 puntos de historia |
| **Épica**          | EP-10 Portal web institucional                                                             | **Sprint**     | Sprint 4             |
| **Tablas (ER v3)** | Programas, ObjetivosEspecificos, ProgramaIntegrantes, GruposInvestigacion, ProgramaAliados |                |                      |
| **Endpoints**      | GET /api/v1/publico/programas · GET /api/v1/publico/programas/{slug}                       |                |                      |
| **Dependencias**   | HU-06 (Administrar programas, objetivos e integrantes)                                     |                |                      |

**Reglas de negocio**

- Se muestran: descripción, objetivos específicos, Coordinador,
  investigadores (solo nombre y enlaces a CvLAC/ORCID), grupos, aliados
  y proyectos públicos del programa.

- Nunca se publican correos, teléfonos ni documentos de identidad.

**Criterios de aceptación**

**CA-1. Dado** un visitante, **cuando** abre
/programas/ciudades-inteligentes, **entonces** ve el detalle del
programa con URL amigable y metadatos para SEO.

HU-27 · Consultar el portafolio público de proyectos

|                                                                                                                                                                             |
|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| **Como** visitante del sitio**, quiero** consultar los proyectos del centro filtrando por programa, estado y año**, para** conocer los resultados y capacidades del centro. |

|                    |                                                                      |                |                      |
|--------------------|----------------------------------------------------------------------|----------------|----------------------|
| **Prioridad**      | **Must (imprescindible)**                                            | **Estimación** | 3 puntos de historia |
| **Épica**          | EP-10 Portal web institucional                                       | **Sprint**     | Sprint 4             |
| **Tablas (ER v3)** | Proyectos, ProyectoIntegrantes, ProyectoAliados                      |                |                      |
| **Endpoints**      | GET /api/v1/publico/proyectos · GET /api/v1/publico/proyectos/{slug} |                |                      |
| **Dependencias**   | HU-13 (Consultar mis proyectos y los proyectos del programa)         |                |                      |

**Reglas de negocio**

- Solo se publican proyectos en estado Aprobado, En ejecución o
  Finalizado; nunca En elaboración, Presentado ni No aprobado.

- Campos visibles: título, resumen, programa, IP, equipo (nombres),
  aliados, fechas, estado y % de avance.

- Nunca se publican: presupuestos, fuentes de financiamiento, documentos
  no públicos, revisiones ni datos de contacto personales.

**Criterios de aceptación**

**CA-1. Dado** un proyecto en elaboración, **cuando** un visitante busca
su título, **entonces** no aparece en el portal y su URL responde 404.

**CA-2. Dado** el portafolio, **cuando** el visitante filtra por
programa y año, **entonces** ve los resultados paginados con su estado.

**Notas técnicas**

- *Decisión a validar: agregar Proyectos.EsPublico (bit) para que el
  Director oculte proyectos confidenciales (p. ej. con propiedad
  intelectual en trámite).*

EP-11 · Auditoría y protección de datos

**Valor:** Cumplimiento de la Ley 1581 de 2012 y del Acuerdo 31 de 2015,
y trazabilidad de todas las acciones sobre la información.

**Procesos:** P17 (parcial), P19 **Módulos ER:** M14 Visitas y Datos
Personales (políticas y consentimientos), M15 (LogActividades)

HU-28 · Registro automático de auditoría

|                                                                                                                                                                                |
|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| **Como** Administrador del sistema**, quiero** que toda acción que modifique datos quede registrada automáticamente**, para** tener trazabilidad ante incidentes y revisiones. |

|                    |                                       |                |                      |
|--------------------|---------------------------------------|----------------|----------------------|
| **Prioridad**      | **Must (imprescindible)**             | **Estimación** | 3 puntos de historia |
| **Épica**          | EP-11 Auditoría y protección de datos | **Sprint**     | Sprint 2             |
| **Tablas (ER v3)** | LogActividades                        |                |                      |
| **Endpoints**      | Middleware transversal                |                |                      |
| **Dependencias**   | HT-03                                 |                |                      |

**Reglas de negocio**

- El middleware intercepta las peticiones POST, PUT, PATCH y DELETE,
  además de los eventos de autenticación (P19).

- Registra UsuarioId, IpOrigen, Recurso, RecursoId, Accion, Ruta,
  EstadoHttp, ValoresAnteriores, ValoresNuevos (JSON) y FechaUtc.

- Nunca registra contraseñas, hashes ni tokens.

- Escritura asíncrona y no bloqueante (cola en memoria con un worker en
  segundo plano).

- No existen endpoints para modificar ni borrar registros de auditoría.

- HashEncadenado queda nulo en el MVP; la cadena de integridad se activa
  en Release 2.

**Criterios de aceptación**

**CA-1. Dado** una edición de un proyecto, **cuando** se guarda,
**entonces** queda un registro con los valores anteriores y los nuevos.

**CA-2. Dado** un fallo al escribir la auditoría, **cuando** ocurre,
**entonces** la operación del usuario se completa y el fallo queda en el
log técnico.

HU-29 · Aceptar la política de tratamiento de datos personales

|                                                                                                                                                                                                                                                 |
|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| **Como** usuario o visitante que entrega datos personales**, quiero** conocer y aceptar de forma explícita la política de tratamiento de datos**, para** que el centro cumpla la Ley 1581 de 2012 y el Acuerdo 31 de 2015 del Consejo Superior. |

|                    |                                                                   |                |                      |
|--------------------|-------------------------------------------------------------------|----------------|----------------------|
| **Prioridad**      | **Must (imprescindible)**                                         | **Estimación** | 3 puntos de historia |
| **Épica**          | EP-11 Auditoría y protección de datos                             | **Sprint**     | Sprint 3             |
| **Tablas (ER v3)** | PoliticasTratamientoDatos, ConsentimientosDatos                   |                |                      |
| **Endpoints**      | GET /api/v1/publico/politica-datos · POST /api/v1/consentimientos |                |                      |
| **Dependencias**   | HU-01 (Iniciar sesión en la plataforma)                           |                |                      |

**Reglas de negocio**

- La política se versiona con HashTexto y solo una versión está vigente
  a la vez.

- En el primer inicio de sesión, y cada vez que cambie la versión
  vigente, el usuario debe aceptarla antes de continuar.

- Los formularios públicos exigen una casilla de aceptación no
  premarcada.

- ConsentimientosDatos es inmutable (sin actualización ni borrado) y
  guarda correo, fecha, IP, contexto y HashEvidencia.

- Si la persona no acepta, no se registran sus datos (compuerta XOR,
  P17).

**Criterios de aceptación**

**CA-1. Dado** un usuario que nunca ha aceptado la política, **cuando**
inicia sesión, **entonces** ve la política y no puede usar la plataforma
hasta aceptarla.

**CA-2. Dado** una nueva versión de la política, **cuando** un usuario
que aceptó la anterior inicia sesión, **entonces** debe aceptar la nueva
versión.

**CA-3. Dado** un consentimiento registrado, **cuando** alguien intenta
modificarlo por API, **entonces** no existe un endpoint que lo permita.

4\. Historias técnicas habilitadoras

Trabajo técnico necesario antes de construir funcionalidad. Se ejecuta
en los primeros días del Sprint 1 con un alcance reducido; no entrega
valor visible al usuario, pero reduce el riesgo de todos los sprints
siguientes.

HT-01 · Repositorios, ambientes y pipelines de CI/CD (5 SP · Sprint 1)

Configurar los repositorios y la integración y despliegue continuos para
que cada incremento llegue a pruebas de forma automática.

- Repositorios (frontend Next.js y backend .NET 10) con ramas protegidas
  y revisión de código obligatoria.

- Pipeline de GitHub Actions: compilación, lint, pruebas unitarias y de
  integración en cada pull request.

- Frontend en Vercel con despliegues de vista previa por pull request;
  backend en MonsterASP mediante Web Deploy.

- HTTPS obligatorio y CORS restringido a los dominios del frontend.

- Dos ambientes: Pruebas y Producción, con secretos gestionados fuera
  del código.

HT-02 · Base de datos: migraciones y datos semilla (3 SP · Sprint 1)

Crear en SQL Server solo las tablas que usa el MVP, a partir del modelo
entidad-relación aprobado.

- Migraciones de EF Core solo para las tablas que usan las historias del
  MVP (M01, M02, M03, M04, M05, M06, M07, M11, M14 políticas y
  consentimientos, y LogActividades de M15).

- Datos semilla: roles, programas, estados de proyecto (incluido "No
  aprobado"), tipos de proyecto, tipos de convocatoria, tipos de hito,
  tipos de documento, tipos de aliado y la primera versión de la
  política de datos.

- Usuario Administrador inicial.

- Copias de seguridad diarias automáticas y procedimiento de
  restauración probado.

HT-03 · Arquitectura base de backend y frontend (5 SP · Sprint 1)

Dejar listos los patrones y convenciones que el equipo reutilizará en
todas las historias.

- Backend: Clean Architecture con Minimal APIs, registro de rutas
  mediante {Modulo}Endpoints.Map(app), FluentValidation, EF Core para
  comandos y Dapper para consultas.

- Errores de negocio con 422 (Results.UnprocessableEntity) y formato de
  error estándar.

- OpenAPI / Swagger, logging estructurado (Serilog) y endpoint de health
  check.

- Frontend: Next.js (App Router, TypeScript), TanStack Query, Zustand
  (solo autenticación y estado de UI), react-hook-form + Zod, Sonner y
  date-fns en español.

- Diseño base: layout autenticado, menú por rol, componentes de tabla,
  formulario y estados vacíos y de error.

5\. Plan de releases y sprints

Supuestos de planificación: equipo de 3 a 4 desarrolladores con
dedicación completa, 4 sprints de 2 semanas y una velocidad estimada de
32 a 34 puntos por sprint. El Sprint 4 lleva menos carga para dejar
espacio a pruebas de aceptación, corrección de errores y salida a
producción. La velocidad se revisa al cierre del Sprint 1; si es menor a
la estimada, se aplica la línea de corte.

| **Sprint**   | **Semanas** | **Objetivo del sprint**                                                                                                     | **Historias**                                                 | **SP** |
|--------------|-------------|-----------------------------------------------------------------------------------------------------------------------------|---------------------------------------------------------------|--------|
| **Sprint 1** | Semanas 1–2 | Arranque y acceso seguro: ambientes, CI/CD, base de datos, arquitectura base, login, sesión, permisos y usuarios.           | HT-01, HT-02, HT-03, HU-01, HU-02, HU-04, HU-05               | 34     |
| **Sprint 2** | Semanas 3–4 | Registro de proyectos: programas e integrantes, aliados, propuesta, equipo, financiación, documentos, auditoría y listados. | HU-06, HU-07, HU-08, HU-09, HU-10, HU-13, HU-21, HU-28        | 33     |
| **Sprint 3** | Semanas 5–6 | Flujo institucional: revisión y aval, aprobación, convocatorias, postulaciones, notificaciones, hitos y consentimiento.     | HU-03, HU-11, HU-12, HU-15, HU-16, HU-17, HU-22, HU-23, HU-29 | 34     |
| **Sprint 4** | Semanas 7–8 | Seguimiento y visibilidad: alertas, avances, tablero del Director, portal público, carga inicial y salida a producción.     | HU-14, HU-18, HU-19, HU-20, HU-24, HU-25, HU-26, HU-27        | 28     |
| **Total**    | Semanas 1–8 |                                                                                                                             |                                                               | 129    |

5.1 Hitos de entrega

- **Fin del Sprint 2 (semana 4):** demo interna. Los programas ya pueden
  registrar propuestas con su equipo, financiación y documentos.

- **Fin del Sprint 3 (semana 6):** flujo institucional completo en el
  ambiente de Pruebas (revisión, aval, aprobación y convocatorias).
  Fecha límite para que los programas entreguen sus datos.

- **Semana 7:** congelamiento de funcionalidades nuevas para Release 2;
  las historias del Sprint 4 terminan.

- **Semana 8:** pruebas de aceptación con el Director y los
  Coordinadores, carga inicial de datos y salida a producción.

5.2 Línea de corte

Si al cierre del Sprint 1 o del Sprint 2 la velocidad real es menor a la
estimada, estas historias se mueven a Release 2 en este orden, sin
afectar el flujo central:

| **Orden** | **Historia**                                                | **SP** | **Efecto de moverla**                                                                                |
|-----------|-------------------------------------------------------------|--------|------------------------------------------------------------------------------------------------------|
| 1         | HU-22 Centro de notificaciones en la aplicación             | 2      | Las notificaciones llegan solo por correo.                                                           |
| 2         | HU-26 Consultar los programas de investigación en el portal | 3      | Los programas se presentan en la página de inicio sin página de detalle.                             |
| 3         | HU-20 Validar avances                                       | 3      | Los avances quedan registrados sin validación formal; el porcentaje del proyecto lo actualiza el IP. |
| 4         | HU-03 Restablecer una contraseña olvidada                   | 2      | El Administrador reenvía el enlace para definir contraseña desde la gestión de usuarios.             |

**Aplicar toda la línea de corte libera 10 puntos de historia.**

5.3 Métricas de éxito del MVP

- 100 % de los proyectos vigentes de los 5 programas registrados en la
  plataforma un mes después del lanzamiento.

- 100 % de las nuevas propuestas tramitadas por el flujo de revisión del
  sistema (sin avales por correo).

- Al menos el 85 % de los hitos cumplidos a tiempo en el primer semestre
  de uso.

- El Director consulta el estado de los proyectos en el tablero, sin
  pedir consolidados por correo.

- Portal público en línea con puntaje Lighthouse ≥ 90.

6\. Supuestos, decisiones a validar y riesgos

6.1 Decisiones a validar con los stakeholders

**D-1.** Conflicto entre la matriz de permisos (el Investigador no crea
proyectos) y el proceso P1 (el IP redacta la propuesta). El backlog
asume que el Investigador puede crear y editar sus propias propuestas
solo mientras están "En elaboración" o devueltas. Debe confirmarse con
el Director.

**D-2.** Se asume que los "5 programas" del centro corresponden a sus 5
áreas de investigación (Biotecnología y Agroindustria; Bioingeniería;
IA, Modelado y Simulación; Sistemas Computacionales y Bioinformática;
Ciudades Inteligentes).

**D-3.** Agregar los estados "No aprobado" (y opcionalmente "Cancelado")
al catálogo EstadosProyecto. Es un cambio de datos semilla, no de
estructura.

**D-4.** Las etapas de revisión (Coordinador, Dirección) se modelan con
ProyectoRevisiones.Etapa, sin crear estados nuevos en el catálogo.

**D-5.** Cambio menor opcional al ER: Proyectos.EsPublico (bit) y
Proyectos.Slug, para controlar qué proyectos se publican en el portal y
con qué URL.

**D-6.** Definir el proveedor de correo (SMTP institucional o servicio
transaccional) y el dominio desde el que se enviará.

**D-7.** Definir dónde se almacenan los archivos (disco de MonsterASP o
almacenamiento de objetos externo) según la cuota contratada.

**D-8.** Confirmar el tamaño del equipo: el plan supone 3 a 4
desarrolladores con dedicación completa. Con 2 desarrolladores el
alcance no cabe en 8 semanas aunque se aplique la línea de corte.

**D-9.** Los Coordinadores deben entregar los datos de sus proyectos
vigentes en la plantilla CSV a más tardar en la semana 6 para la carga
inicial.

6.2 Riesgos

| **Riesgo**                                                                                                                | **Mitigación**                                                                                                                           |
|---------------------------------------------------------------------------------------------------------------------------|------------------------------------------------------------------------------------------------------------------------------------------|
| Plazo sin holgura: el plan usa toda la capacidad estimada del equipo.                                                     | Línea de corte definida (sección 5.2); revisar la velocidad real al final del Sprint 1 y aplicar la línea de corte sin esperar al final. |
| Las tareas programadas (alertas de hitos) se detienen cuando el hosting compartido recicla la aplicación por inactividad. | Disparar los jobs desde n8n o un programador externo que llame a endpoints protegidos con API key.                                       |
| Baja adopción por parte de los investigadores si la plataforma arranca vacía.                                             | Carga inicial de los proyectos vigentes en el Sprint 4 y acompañamiento a cada programa en la puesta en marcha.                          |
| Cookies de sesión bloqueadas entre dominios (Vercel y MonsterASP).                                                        | Subdominios bajo un mismo dominio o BFF en Next.js; decidir en el Sprint 1.                                                              |
| Límites de almacenamiento o de envío de correo del hosting.                                                               | Verificar cuotas en el Sprint 1 y tener una alternativa externa definida.                                                                |
| Cambios de alcance por parte de los stakeholders durante el MVP.                                                          | Revisión del backlog al cierre de cada sprint; los nuevos pedidos entran priorizados, no se agregan al sprint en curso.                  |

7\. Definition of Ready y Definition of Done

7.1 Definition of Ready (una historia puede entrar a un sprint cuando…)

- Tiene narrativa completa (Como / quiero / para) y el Product Owner la
  entiende igual que el equipo.

- Sus criterios de aceptación son verificables y sus reglas de negocio
  están validadas.

- Está estimada por el equipo y cabe en un sprint (≤ 8 SP; si es mayor,
  se divide).

- Sus dependencias están terminadas o planificadas en el mismo sprint.

- Las tablas del ER que usa están identificadas y las decisiones
  abiertas que la afectan están resueltas.

- Si tiene interfaz, cuenta con un boceto o wireframe aprobado.

7.2 Definition of Done (una historia está terminada cuando…)

- Cumple todos sus criterios de aceptación, verificados por el Product
  Owner en el ambiente de Pruebas.

- El código pasó revisión de al menos otro desarrollador y sigue las
  convenciones del proyecto (backend .NET y frontend Next.js).

- Tiene pruebas unitarias de las reglas de negocio y pruebas de
  integración de sus endpoints, incluida la autorización por rol; el
  pipeline de CI está en verde.

- Las validaciones existen tanto en el cliente (Zod) como en el servidor
  (FluentValidation).

- Las mutaciones quedan registradas en LogActividades.

- La interfaz es adaptable a móvil, cumple los criterios básicos de
  accesibilidad y muestra estados de carga, vacío y error.

- El endpoint está documentado en OpenAPI / Swagger.

- Está desplegada en el ambiente de Pruebas y las migraciones son
  reproducibles.

Anexo A. Matriz de permisos del MVP

Ajuste de la matriz del modelo de datos a las funciones del MVP. "Sí" =
permitido; "No" = no permitido; el texto indica una restricción de
alcance.

| **Acción**                                | **Admin** | **Director** | **Coordinador** | **Investigador** | **Externo** |
|-------------------------------------------|-----------|--------------|-----------------|------------------|-------------|
| **Gestionar usuarios y roles**            | Sí        | No           | No              | No               | No          |
| **Editar programas y objetivos**          | Sí        | Sí           | No              | No               | No          |
| **Asignar coordinadores de programa**     | Sí        | Sí           | No              | No               | No          |
| **Vincular investigadores a un programa** | Sí        | Sí           | Su programa     | No               | No          |
| **Gestionar aliados y convocatorias**     | Sí        | Sí           | Su programa     | No               | No          |
| **Crear propuestas**                      | Sí        | Sí           | Su programa     | Como IP (D-1)    | No          |
| **Editar propuestas en elaboración**      | Sí        | Sí           | Su programa     | Si es IP (D-1)   | No          |
| **Revisar / devolver propuestas**         | No        | Sí           | Su programa     | No               | No          |
| **Otorgar aval y aprobar proyectos**      | Sí        | Sí           | No              | No               | No          |
| **Ver proyectos**                         | Todos     | Todos        | Su programa     | Sus proyectos    | No          |
| **Gestionar hitos**                       | Sí        | Sí           | Su programa     | Si es IP         | No          |
| **Registrar avances**                     | Sí        | Sí           | Su programa     | Sus proyectos    | No          |
| **Validar avances**                       | No        | Sí           | Su programa     | Si es IP         | No          |
| **Cargar / descargar documentos**         | Sí        | Sí           | Su programa     | Sus proyectos    | No          |
| **Ver tablero del centro**                | Sí        | Sí           | No              | No               | No          |
| **Ver sus notificaciones**                | Sí        | Sí           | Sí              | Sí               | Sí          |