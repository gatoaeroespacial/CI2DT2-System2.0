# Manual de usuario y pruebas — CI²DT² (Sprints 1 a 4)

## 1. Arranque local

1. SQL Server: `sqllocaldb start MSSQLLocalDB`.
2. API: `dotnet run --project backend/src/Ci2dt2.Api --launch-profile http` → `http://localhost:5132` (Swagger en `/swagger`, estado en `/health`). Aplica las migraciones sobre LocalDB `Ci2dt2Dev` y crea el Administrador inicial (`admin@ci2dt2.local` / `Admin12345Dev`, en `appsettings.Development.json`).
3. Frontend: `npm run dev --prefix frontend` → `http://localhost:3000` (portal público) y `http://localhost:3000/login` (plataforma).
4. Datos de prueba: la semilla solo se carga en una base sin los usuarios de prueba. Para partir de cero:
   ```bash
   sqlcmd -S "(localdb)\MSSQLLocalDB" -Q "DROP DATABASE IF EXISTS Ci2dt2Dev"
   ```
   Arranque la API una vez (crea las tablas) y luego:
   ```bash
   sqlcmd -S "(localdb)\MSSQLLocalDB" -d Ci2dt2Dev -I -i backend/scripts/seed-pruebas.sql
   ```

En desarrollo, sin SMTP configurado, los correos se escriben en el log de la API. El motor de alertas corre solo a partir de las 6:00 a. m. (hora de Colombia) y también se puede disparar con:
```bash
curl -X POST http://localhost:5132/api/v1/jobs/alertas -H "X-Api-Key: clave-de-jobs-solo-desarrollo"
```

## 2. Usuarios de prueba (contraseña `Prueba12345`)

| Correo (@pruebas.ci2dt2.test) | Roles | Programa |
|---|---|---|
| director | Director | — |
| coord.biotec | Coordinador + Investigador | Coordina Biotecnología y Agroindustria |
| coord.bioing | Coordinador + Investigador | Coordina Bioingeniería |
| coord.ia | Coordinador + Investigador | Coordina IA, Modelado y Simulación |
| coord.sistemas | Coordinador + Investigador | Coordina Sistemas Computacionales y Bioinformática |
| coord.ciudades | Coordinador + Investigador | Coordina Ciudades Inteligentes |
| inv.sara | Investigador | Biotecnología |
| inv.valeria | Investigador | Biotecnología e IA |
| inv.diego, inv.tomas | Investigador | Bioingeniería (Tomás, histórico de Sistemas) |
| inv.lucia | Investigador | IA |
| inv.mateo | Investigador | IA y Ciudades Inteligentes |
| inv.camila | Investigador | Sistemas Computacionales |
| inv.julian | Investigador | Ciudades Inteligentes |
| inv.inactivo | Investigador (cuenta inactiva) | Retirado de Bioingeniería |
| externo.chec, externo.uam | Externo (aliados CHEC y UAM) | — |

### Proyectos de la semilla (códigos si se carga en 2026)

| Código | Proyecto | IP | Estado | Para demostrar |
|---|---|---|---|---|
| CI2DT2-2026-002 | Gemelo digital de movilidad | Mateo | En elaboración (incompleto) | Lista de verificación con pendientes |
| CI2DT2-2026-003 | Monitoreo de calidad del aire | Mateo | En elaboración (completo) | Enviar a revisión → aprobar → avalar → aprobar |
| CI2DT2-2026-004 | Plataforma bioinformática | Camila | En revisión, etapa Coordinador | Decisión del Coordinador (coord.sistemas) |
| CI2DT2-2026-005 | Prótesis de mano 3D | Diego | En revisión, etapa Director | Aval del Director |
| CI2DT2-2026-006 | Analítica de café | Valeria | Devuelto | Observaciones, corrección y reenvío |
| CI2DT2-2026-007 | Alerta de calidad del agua | Julián | Presentado (SGR seleccionada) | Registrar aprobación con acto administrativo |
| CI2DT2-2026-001 | Demanda energética | Lucía | Aprobado (reto empresarial) | Iniciar ejecución bloqueado por hitos faltantes |
| CI2DT2-2026-008 | Redes neuronales (tesis) | Camila | Aprobado sin hitos | Definir hitos, plan de trabajo e iniciar ejecución |
| CI2DT2-2025-001 | Bioinsumos de pulpa de café | Sara | En ejecución | Hitos próximo/vencido, avances en todos los estados |
| CI2DT2-2025-002 | Diagnóstico de roya con IA | Lucía | En ejecución | Hito vencido, avance pendiente |
| CI2DT2-2025-003 | Mantenimiento predictivo CHEC | Mateo | En ejecución (reto) | Hitos Sprint y MVP, avances |
| CI2DT2-2024-001 | Biorrefinería de plátano | Sara | Finalizado | Portal público |
| CI2DT2-2025-004 | Rehabilitación de rodilla | Tomás | No aprobado | Estado final |
| CI2DT2-2025-005 | Movilidad eléctrica | Julián | Suspendido | Estado no publicable |

## 3. Procesos disponibles

| Proceso | HU | Roles |
|---|---|---|
| Iniciar y cerrar sesión, renovación automática, bloqueo por intentos | HU-01, HU-02 | Todos |
| Restablecer contraseña olvidada por correo | HU-03 | Todos |
| Usuarios: crear con contraseña inicial, editar, roles, activar/desactivar, asignar contraseña | HU-04 | Administrador |
| Acceso por rol y por recurso (menú, 403 y 404) | HU-05 | Todos |
| Programas: datos, objetivos (ordenables), Coordinador e Investigadores | HU-06 | Admin, Director; Coordinador vincula investigadores de su programa |
| Aliados y aliados del programa | HU-07 | Admin, Director, Coordinador |
| Registrar y editar propuestas | HU-08 | Admin, Director, Coordinador, Investigador (como IP) |
| Equipo del proyecto y cambio de IP | HU-09 | Editores; cambio de IP solo Director o Coordinador |
| Fuentes de financiamiento y contrapartidas | HU-10 | Editores |
| Enviar a revisión, revisar (Coordinador), avalar (Director) | HU-11 | IP; Coordinador del programa; Director |
| Registrar aprobación o no aprobación e iniciar ejecución | HU-12 | Administrador, Director |
| Consultar proyectos y ficha con 8 pestañas | HU-13 | Admin y Director todos; Coordinador su programa; Investigador los suyos |
| Carga inicial desde plantilla CSV (línea de comandos) | HU-14 | Equipo técnico |
| Catálogo de convocatorias (Próxima, Abierta, Cerrada) | HU-15 | Gestionan Admin, Director, Coordinador; todos consultan |
| Postulaciones: radicación, código MGA, fase SGR y resultado | HU-16 | IP, Coordinador, Director, Admin |
| Hitos: definir, iniciar, completar y reprogramar con justificación | HU-17 | Gestiona el IP, Coordinador, Director, Admin; el responsable inicia y completa |
| Motor de alertas de hitos y "Mis alertas" | HU-18 | Automático; todos ven sus alertas |
| Registrar avances con evidencias | HU-19 | Integrantes vigentes, Coordinador, Director, Admin |
| Validar o rechazar avances | HU-20 | IP, Coordinador del programa, Director |
| Repositorio documental (SHA-256, descarga firmada) | HU-21 | Según acceso al proyecto |
| Centro de notificaciones con campana | HU-22 | Todos |
| Notificaciones por correo con reintentos | HU-23 | Automático |
| Tablero del Director | HU-24 | Administrador, Director |
| Portal público: inicio, programas y portafolio de proyectos | HU-25 a HU-27 | Visitantes |
| Auditoría de cambios, accesos denegados e inicios de sesión | HU-28 | Automático |
| Aceptación de la política de tratamiento de datos | P17 | Todos (primer ingreso) |
| Asistente IA y agente de formulación (Gemini) | Release 3 | Asistente: Admin y Director; formulación: quienes crean propuestas |

## 4. Verificación por rol

### Todos
1. **Sesión (HU-01/02):** al ingresar, Director → Tablero; Coordinador e Investigador → Proyectos; Administrador → Usuarios; Externo → Notificaciones. Cinco contraseñas erradas bloquean la cuenta 15 minutos. `inv.inactivo` es rechazado.
2. **Política de datos:** en el primer ingreso aparece la ventana de la política; sin aceptarla no se usa la plataforma.
3. **Recuperar contraseña (HU-03):** en el login, "¿Olvidaste tu contraseña?". El mensaje es el mismo exista o no el correo; el enlace (en el log sin SMTP) vale 30 minutos y un solo uso. Al abrir un enlace usado o vencido se ofrece pedir otro.
4. **Notificaciones (HU-22):** la campana muestra las no leídas; al abrir una se navega al proyecto y queda leída; "Marcar todas como leídas".

### Administrador
- **Usuarios (HU-04):** crear (con contraseña inicial y reglas en vivo), correo repetido rechazado, ORCID y CvLAC validados, editar roles, asignar contraseña (revoca sesiones), desactivar.
- No ve "Avances por validar" (no valida avances, Anexo A).

### Director (`director`)
1. **Tablero (HU-24):** tarjetas (en ejecución, en trámite, presentadas, tasa de aprobación, monto gestionado), barras apiladas por programa y estado, hitos próximos y vencidos, avances pendientes por programa; filtro por año.
2. **Avalar (HU-11):** abra CI2DT2-2026-005 → "Otorgar aval" → queda Presentado y se notifica al IP y al Coordinador.
3. **Aprobar (HU-12):** abra CI2DT2-2026-007 → "Registrar aprobación" sin documento → mensaje de error. Suba en Documentos un PDF tipo "Acto administrativo de aprobación" y repita → Aprobado y se crea el cronograma.
4. **Iniciar ejecución:** en CI2DT2-2026-001 (reto) → "Iniciar ejecución" responde qué hitos faltan (1 Sprint y el MVP). En CI2DT2-2026-008 agregue un hito, suba el "Plan de trabajo" y pulse "Iniciar ejecución".
5. **Validar avances (HU-20):** "Avances por validar" → valide "Ensayos de fermentación" → el porcentaje del proyecto sube a 30 %.

### Coordinador (`coord.sistemas`, `coord.biotec`, …)
1. **Revisar (HU-11):** `coord.sistemas` abre CI2DT2-2026-004 → "Devolver al IP" solo se habilita con 20 o más caracteres de observaciones; "Aprobar y enviar al Director" lo pasa a etapa Director.
2. **Convocatorias (HU-15):** registre una convocatoria; fecha de cierre anterior a la apertura → error; el estado (Próxima, Abierta, Cerrada) se calcula solo; filtros por tipo, entidad y estado.
3. **Programas (HU-06):** solo ve y gestiona los integrantes de su programa.

### Investigador (`inv.mateo`, `inv.valeria`, …)
1. **Lista de verificación (HU-11 CA-6):** `inv.mateo` abre CI2DT2-2026-002 → "Enviar a revisión" deshabilitado con pendientes. En CI2DT2-2026-003 todo está ✓ → enviar → ya no puede editar.
2. **Devuelto:** `inv.valeria` abre CI2DT2-2026-006 → ve las observaciones en Revisiones y la notificación; edita y reenvía.
3. **Postulaciones (HU-16):** pestaña Convocatorias → vincule a la SGR sin código MGA → error; monto mayor al máximo → error con el máximo; en convocatorias externas la fecha de presentación exige id externo y comprobante adjunto.
4. **Hitos (HU-17):** en un proyecto aprobado propio, "Agregar hito" (responsable solo entre integrantes vigentes; aviso si queda fuera de la vigencia). Cambiar la fecha exige justificación y notifica al Coordinador. El responsable marca "En curso" y "Completar" (opcionalmente con un avance de evidencia).
5. **Avances (HU-19):** en CI2DT2-2025-003, "Registrar avance": porcentaje menor al último validado → error; adjuntos de más de 20 MB o tipo no permitido se rechazan antes de subir; con un CSV o PDF queda "Pendiente" y se notifica al IP y al Coordinador. Un avance rechazado se corrige y vuelve a "Pendiente".
6. **Mis alertas (HU-18):** hitos próximos (30 días) y vencidos de sus proyectos.

### Externo (`externo.chec`)
Solo ve Notificaciones; cualquier otra ruta o llamada responde 403.

### Visitante (sin sesión)
`http://localhost:3000`: cifras dinámicas, programas, convocatorias abiertas; `/portal/programas/ciudades-inteligentes`; `/portal/proyectos` con filtros y paginación. Un proyecto en elaboración no aparece y su URL responde 404. Nunca se muestran correos, presupuestos ni documentos.

### Auditoría (HU-28), en SQL Server
```sql
SELECT TOP 30 FechaUtc, UsuarioId, Accion, Recurso, RecursoId, EstadoHttp, ValoresAnteriores, ValoresNuevos
FROM LogActividades ORDER BY Id DESC;
```

## 5. Carga inicial (HU-14)
```bash
dotnet run --project backend/src/Ci2dt2.Api -- plantilla > plantilla.csv
dotnet run --project backend/src/Ci2dt2.Api -- importar plantilla.csv
dotnet run --project backend/src/Ci2dt2.Api -- importar plantilla.csv --confirmar
```
Sin `--confirmar` solo valida. Con un error en cualquier fila (p. ej. "Fila 12: el usuario con correo X no existe") no importa nada. Separador `;`, fechas `AAAA-MM-DD`; integrantes `correo:Rol|…`; fuentes `TipoFuente:TipoAporte:Moneda:Monto:Tasa:Estado:NIT|…`.

## 6. Guion de demostración para stakeholders (≈ 20 minutos)

Prepare: base con la semilla recién cargada, la API y el frontend arriba, un PDF cualquiera a mano y dos navegadores (uno en incógnito) para cambiar de usuario rápido.

1. **Portal público (2 min)** — sin sesión, abra el inicio: cifras del centro, los 5 programas y las convocatorias abiertas. Entre a "Ciudades Inteligentes" (URL amigable) y al portafolio; filtre por programa. *Mensaje: la cara pública se alimenta sola de la plataforma.*
2. **Seguridad (2 min)** — intente ingresar con contraseña errada (mensaje genérico), luego con `inv.mateo`. Muestre la aceptación de la política de datos y el menú reducido del investigador.
3. **Propuesta y lista de verificación (3 min)** — `inv.mateo` abre CI2DT2-2026-002 (pendientes en ámbar) y CI2DT2-2026-003 (todo ✓): pestañas Equipo, Financiación (totales en COP y aviso de diferencia) y Convocatorias. Pulse "Enviar a revisión".
4. **Revisión del Coordinador (2 min)** — `coord.ciudades`: campana con la notificación → abra la propuesta → muestre que "Devolver" exige observaciones → "Aprobar y enviar al Director".
5. **Aval y aprobación del Director (4 min)** — `director`: "Otorgar aval" → Presentado. Intente "Registrar aprobación" sin acto (error), suba el PDF como "Acto administrativo de aprobación" y apruebe: se crea el cronograma. Muestre la pestaña Revisiones con la línea de tiempo.
6. **Seguimiento (4 min)** — CI2DT2-2025-001: Cronograma con hitos completados, en curso, próximos y vencidos; Avances con validado, pendiente y rechazado. En "Avances por validar" valide uno y vuelva al proyecto: el porcentaje sube. Abra "Mis alertas".
7. **Tablero del Director (2 min)** — tarjetas, barras por programa y estado, hitos próximos y vencidos, avances pendientes; cambie el año.
8. **Trazabilidad (1 min)** — consulta de `LogActividades`: cada acción de la demo quedó registrada con valores anteriores y nuevos.
9. **Cierre** — mencione el asistente IA y el agente de formulación (con la clave de Gemini configurada) y lo que sigue: correo real (Brevo), programador de alertas y carga inicial de los proyectos reales.
