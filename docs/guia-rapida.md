# Manual de usuario y pruebas — CI²DT² (Sprints 1 y 2)

## Arranque local

1. API: `dotnet run --project backend/src/Ci2dt2.Api --launch-profile http` → `http://localhost:5132` (Swagger en `/swagger`, estado en `/health`). Aplica migraciones sobre LocalDB `Ci2dt2Dev` y crea el Administrador inicial (credenciales en `backend/src/Ci2dt2.Api/appsettings.Development.json`).
2. Frontend: `npm run dev --prefix frontend` → `http://localhost:3000`.
3. Datos de prueba (opcional, sobre una base vacía): `sqlcmd -S "(localdb)\MSSQLLocalDB" -d Ci2dt2Dev -I -i backend/scripts/seed-pruebas.sql`.

### Usuarios de prueba (contraseña `Prueba12345`)

| Correo | Roles | Programa |
|---|---|---|
| director@pruebas.ci2dt2.test | Director | — |
| coord.biotec@pruebas.ci2dt2.test | Coordinador + Investigador | Coordina Biotecnología y Agroindustria |
| coord.ia@pruebas.ci2dt2.test | Coordinador + Investigador | Coordina IA, Modelado y Simulación |
| inv.lucia@pruebas.ci2dt2.test | Investigador | IA (IP de CI2DT2-2026-002, Aprobado) |
| inv.mateo@pruebas.ci2dt2.test | Investigador | IA y Ciudades Inteligentes (IP de -003 En elaboración y -004 Presentado) |
| inv.sara@pruebas.ci2dt2.test | Investigador | Biotecnología (IP de -001 En ejecución) |
| inv.inactivo@pruebas.ci2dt2.test | Investigador (cuenta inactiva) | Retirado de Bioingeniería |
| externo.chec@pruebas.ci2dt2.test | Externo (aliado CHEC) | — |

## Procesos disponibles

| Proceso | HU | Roles |
|---|---|---|
| Iniciar sesión, cerrar sesión, renovación automática de sesión, bloqueo por intentos | HU-01, HU-02 | Todos |
| Crear usuarios con contraseña inicial, editar, asignar roles, activar/desactivar, asignar contraseña | HU-04 | Administrador |
| Control de acceso por rol y por recurso (menú, 403/404) | HU-05 | Todos |
| Programas: editar datos y objetivos (crear, editar, eliminar, reordenar) | HU-06 | Administrador, Director |
| Programas: vincular/retirar Coordinador | HU-06 | Administrador, Director |
| Programas: vincular/retirar Investigadores | HU-06 | Administrador, Director, Coordinador vigente del programa |
| Catálogo de aliados (crear, buscar, editar, eliminar) y aliados del programa | HU-07 | Administrador, Director, Coordinador |
| Registrar y editar propuestas (borrador "En elaboración", código automático) | HU-08 | Administrador, Director, Coordinador, Investigador |
| Equipo del proyecto (agregar, retirar, cambiar IP) | HU-09 | Editores del proyecto; cambiar IP solo Director/Coordinador |
| Fuentes de financiamiento y contrapartidas con totales en COP | HU-10 | Editores del proyecto |
| Consultar proyectos con filtros, orden, paginación y ficha con pestañas | HU-13 | Administrador, Director, Coordinador, Investigador |
| Repositorio documental del proyecto (carga con SHA-256, descarga por URL firmada) | HU-21 | Administrador, Director, Coordinador, Investigador (con acceso al proyecto) |
| Auditoría automática de cambios y accesos denegados (tabla `LogActividades`) | HU-28 | Automático |

Aún no tienen pantalla funcional (sprints siguientes): Tablero del centro, Notificaciones (bandeja y correo), revisión/aval de propuestas, convocatorias, cronograma/hitos y avances. Las pestañas Convocatorias, Cronograma, Avances y Revisiones de la ficha solo muestran datos existentes.

## Qué puede hacer cada rol y cómo verificarlo

### Todos — Sesión (HU-01, HU-02)
1. Ingrese en `/login` con un usuario de la tabla. **Esperado:** Director → Tablero; Coordinador e Investigador → Proyectos; Administrador → Usuarios; Externo → Notificaciones.
2. Escriba mal la contraseña 5 veces. **Esperado:** cuenta bloqueada 15 minutos con el tiempo restante.
3. Ingrese con `inv.inactivo@…`. **Esperado:** rechazo (cuenta inactiva).
4. Cierre sesión y use "atrás" del navegador. **Esperado:** vuelve al login, sin datos.
5. La sesión dura 15 min y se renueva sola hasta 7 días (cookie HttpOnly); dos pestañas no se desconectan entre sí.

### Administrador — Usuarios (HU-04)
1. **Usuarios → Nuevo usuario**: complete datos, roles y contraseña inicial (mín. 10, mayúscula, minúscula y número; las reglas se marcan en vivo). Si elige **Externo**, debe elegir un aliado (autocompletar). **Esperado:** el usuario aparece en la lista y puede iniciar sesión.
2. Correo repetido, ORCID mal formado (`0000-0000-0000-000X`) o CvLAC fuera de `scienti.minciencias.gov.co`. **Esperado:** error en el campo.
3. Editar → cambie roles o "Asignar contraseña". **Esperado:** se guardan; al asignar contraseña se cierran las sesiones abiertas de ese usuario.
4. Desactivar/activar desde la lista. **Esperado:** el usuario desactivado no puede ingresar y pierde sus sesiones.

### Director y Administrador — Programas (HU-06, HU-07)
1. **Programas**: lista con Coordinador vigente y nº de integrantes. Abra un programa.
2. **Editar** datos del programa. Objetivos: agregar, editar, eliminar, reordenar con ↑↓ o arrastrando. **Esperado:** el orden se conserva al recargar.
3. **Vincular integrante**: busque un usuario, rol Coordinador o Investigador, fecha de inicio. **Esperado (422):** segundo Coordinador vigente, usuario ya vinculado, usuario sin el rol del sistema correspondiente, o fecha fin anterior a inicio.
4. **Retirar**: registra la fecha de fin de hoy; pasa a la pestaña **Históricos**.
5. **Aliados del programa**: asocie con el buscador o quite con ×.

### Coordinador — Programas y proyectos de su programa
Con `coord.ia@…`:
1. **Proyectos** muestra solo proyectos de su programa (IA).
2. En **Programas → IA** puede vincular/retirar Investigadores (no Coordinadores) y gestionar aliados; los objetivos son de solo lectura.
3. Abra **Programas → Biotecnología**. **Esperado:** "Solo el Coordinador vigente de este programa…" (no ve sus integrantes).
4. En la ficha de un proyecto de su programa puede editarlo (salvo estados finales), y en **Equipo** aparece la opción **IP (cambiar investigador principal)**. **Esperado:** el IP anterior queda retirado con fecha de hoy.

### Director, Coordinador e Investigador — Aliados (HU-07)
(El menú Aliados se muestra a Administrador, Director y Coordinador; el Investigador solo usa el buscador de aliados en Financiación).
1. **Aliados → Nuevo aliado**, país Colombia con NIT `890801063-5`. **Esperado:** error de dígito de verificación. Con `890801063-0` se guarda.
2. Registre otro aliado con el mismo NIT. **Esperado:** "Ya existe un aliado con este NIT: … (Id …)".
3. Busque por nombre o NIT; edite; elimine (borrado lógico).

### Investigador — Propuestas y proyectos (HU-08, HU-09, HU-10, HU-13, HU-21)
Con `inv.lucia@…`:
1. **Proyectos** muestra solo proyectos donde es o fue integrante. Pruebe filtros (programa, estado, tipo, año, IP), búsqueda por título/código, orden y paginación.
2. **Nueva propuesta**: el programa solo ofrece aquellos a los que está adscrita. Título de menos de 10 caracteres o moneda USD/EUR sin tasa. **Esperado:** errores en campo; con tasa se muestra el equivalente en COP. Al guardar: código `CI2DT2-AAAA-NNN`, estado "En elaboración", usted como IP y aviso al Coordinador del programa (fila en `Notificaciones`). Salir con cambios sin guardar muestra advertencia del navegador.
3. En la propuesta propia: **Editar propuesta** (solo en "En elaboración" o "Devuelto"). Abra `CI2DT2-2026-002` (Aprobado). **Esperado:** sin botón de edición.
4. **Equipo**: agregar co-investigador/auxiliar/estudiante, retirar (fecha fin hoy), enlaces CvLAC/ORCID. El IP vigente no se puede retirar; la opción IP no aparece para el Investigador.
5. **Financiación**: agregar fuente (Cofinanciación exige aliado; moneda ≠ COP exige tasa), editar, eliminar. **Esperado:** totales por tipo de fuente y aporte, total en COP y aviso si la suma no coincide con el presupuesto (no bloquea).
6. **Documentos**: elija tipo y archivo (PDF, DOCX, XLSX, PPTX, JPG, PNG o ZIP, máx. 25 MB). **Esperado:** aparece con versión 1 y huella SHA-256; un `.exe` o un archivo de más de 25 MB se rechaza. **Descargar** genera un enlace válido por 5 minutos.
7. Escriba en la barra `/proyectos/<id>` de un proyecto ajeno. **Esperado:** "El proyecto no existe" (404, no revela su existencia).

### Externo
Ingresa y solo ve **Notificaciones** (pantalla pendiente de HU-22). Cualquier otra ruta o llamada a la API responde 403.

### Auditoría (HU-28) — verificación en SQL Server
Tras cualquiera de las acciones anteriores:
```sql
SELECT TOP 20 FechaUtc, UsuarioId, Accion, Recurso, RecursoId, Ruta, EstadoHttp, ValoresAnteriores, ValoresNuevos
FROM LogActividades ORDER BY Id DESC;
```
**Esperado:** una fila por intento de inicio de sesión (exitoso, fallido, bloqueado o cuenta inactiva), por cada creación/edición/eliminación con valores anteriores y nuevos, y por cada acceso denegado (403). La tabla no admite UPDATE ni DELETE.

## Datos y seguridad
- Base local: `(localdb)\MSSQLLocalDB`, base `Ci2dt2Dev` (67 tablas del diccionario de datos). Las contraseñas se guardan con hash.
- Los documentos se guardan fuera de la carpeta pública (`App_Data/documentos` de la API) y solo se descargan con URL firmada.
- No reutilice en producción las credenciales ni claves de `appsettings.Development.json`. Respaldo y restauración: [operacion.md](./operacion.md).
