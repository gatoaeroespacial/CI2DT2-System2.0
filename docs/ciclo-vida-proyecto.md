# Ciclo de vida de un proyecto, paso a paso y por rol

Desde la formulación hasta la finalización:

**En elaboración → En revisión (Coordinador) → En revisión (Director) → Presentado → Aprobado → En ejecución → Finalizado**

En cualquier momento de la revisión la propuesta puede pasar a **Devuelto** y volver a revisión. Si el resultado de la convocatoria es negativo, pasa a **No aprobado**, que es un estado final. Durante la ejecución se puede **suspender** y **reanudar**.

Cada decisión queda en la pestaña **Revisiones** de la ficha, con fecha, responsable y observaciones. Cada paso notifica en la campana y por correo a quien debe actuar después.

| Rol | Qué hace en el ciclo |
|---|---|
| Investigador (IP) | Formula, completa y envía la propuesta; define hitos; registra avances; completa hitos |
| Coordinador del programa | Aprueba o devuelve la propuesta; valida avances; reprograma hitos |
| Director | Avala o devuelve; registra la aprobación con el acto; inicia la ejecución; valida avances; suspende, reanuda y finaliza |
| Administrador | Puede hacer las acciones del Director (aval, aprobación, ejecución y cierre) |
| Visitante | Ve en el portal los proyectos Aprobados, En ejecución y Finalizados |

---

## 1. Formulación (Investigador)

1. **Proyectos → Nueva propuesta.** Diligencie:
   - título y resumen;
   - **programa**: solo aparecen los programas a los que el investigador está adscrito;
   - tipo (I+D+i, Reto empresarial, Consultoría o Tesis);
   - fechas de inicio y fin;
   - moneda y presupuesto total.

   Pulse **Guardar borrador**. El sistema asigna el código `CI2DT2-AAAA-NNN`, deja la propuesta **En elaboración** y avisa al Coordinador ("Nueva propuesta").
2. **Pestaña Financiación → Agregar fuente:** registre al menos una fuente (convocatoria, contrapartida, cofinanciación o recursos propios), con su aportante, tipo de aporte, monto y estado. El sistema muestra la diferencia entre la suma de las fuentes y el presupuesto.
3. **Pestañas Equipo, Convocatorias y Documentos (opcionales):**
   - Equipo: co-investigadores.
   - Convocatorias: postulación, con su ID externo o código MGA.
   - Documentos: propuesta técnica, cartas de compromiso, etc.
4. **Lista de verificación** (arriba en la ficha). Cuando todo está en verde se habilita **Enviar a revisión**:
   - datos básicos completos;
   - al menos una fuente;
   - según el caso: carta de compromiso si hay aportes de aliados, Documento MGA si la convocatoria es SGR, y contrapartidas de la Universidad si la convocatoria es nacional.
5. **Enviar a revisión:** la propuesta queda **En revisión, etapa Coordinador** y el IP ya no puede editarla. El Coordinador recibe notificación y correo.

## 2. Revisión (Coordinador del programa)

1. Abra la notificación "Propuesta … enviada a revisión"; lleva directo a la propuesta.
2. En **Decisión de revisión** escriba sus observaciones y elija:
   - **Aprobar y enviar al Director:** la propuesta sigue En revisión, pero pasa a la **etapa Director**, que recibe notificación y correo ("pendiente de aval").
   - **Devolver al IP:** exige al menos 20 caracteres de observaciones. La propuesta pasa a **Devuelto**; el IP la corrige y la vuelve a enviar, y regresa a la etapa Coordinador.

## 3. Aval institucional (Director)

1. En la propuesta en etapa Director elija:
   - **Otorgar aval:** pasa a **Presentado**, es decir, lista para radicar ante la entidad financiadora. Se notifica al IP y al Coordinador.
   - **Devolver al IP**, con observaciones.

## 4. Aprobación (Director)

Cuando la convocatoria publica el resultado:
1. **Documentos:** cargue el **Acto administrativo de aprobación** en PDF. Queda versionado y con su huella SHA-256.
2. **Resultado de la postulación:**
   - **Registrar aprobación:** puede indicar el presupuesto aprobado si difiere del solicitado. El proyecto pasa a **Aprobado**, se crea su **cronograma** y se notifica al equipo. Sin el acto, el sistema lo rechaza.
   - **No aprobado:** estado final.

## 5. Preparación e inicio de la ejecución

1. **Investigador, pestaña Cronograma → Agregar hito:** título, tipo (entregable, informe, sprint, MVP…), fecha límite, responsable (un integrante vigente) y días de anticipación de la alerta. Los retos empresariales exigen 3 hitos tipo Sprint y 1 de MVP.
2. **Director, Documentos:** cargue el **Plan de trabajo**.
3. **Director, Inicio de la ejecución:** indique la fecha de inicio real y pulse **Iniciar ejecución**. El proyecto pasa a **En ejecución** y se notifica al equipo.

## 6. Seguimiento (durante la ejecución)

- **Investigador y equipo, Avances → Registrar avance:**
  - título y porcentaje acumulado, que no puede ser menor que el último validado;
  - descripción y hito relacionado;
  - hasta 5 evidencias.

  Queda **Pendiente** y se notifica al IP y al Coordinador.
- **Validación**, desde **Avances por validar**:
  - el IP valida los avances de su equipo, como primer nivel; los suyos propios pasan directo al Coordinador;
  - el Coordinador o el Director dan la validación final, y entonces el **porcentaje del proyecto** toma el del avance;
  - rechazar exige observaciones; el autor corrige y el avance vuelve a Pendiente.
- **Hitos:**
  - "Marcar en curso" y "Completar", que permite vincular un avance como evidencia;
  - "Editar / reprogramar": cambiar la fecha exige justificación, que queda en el historial del hito y se notifica al Coordinador.
- **Alertas automáticas:** cada día a las 6:00 se avisa al responsable, al IP y al Coordinador de los hitos próximos, de los que vencen hoy y de los vencidos ("Mis alertas").
- **Director:**
  - **Suspender proyecto**, con motivo: lo deja **Suspendido**, sin registro de avances.
  - **Reanudar ejecución**, con observaciones.

## 7. Finalización (Director)

1. Requisitos:
   - **todos los hitos completados**;
   - **ningún avance pendiente** de validación ni rechazado sin corregir.
2. **Finalizar el proyecto:** escriba las observaciones de cierre (mínimo 20 caracteres) y confirme. El proyecto pasa a **Finalizado**, un estado final:
   - ya no admite hitos, avances ni cambios de estado;
   - se notifica al equipo y al Coordinador.
3. En el **portal público** el proyecto aparece como Finalizado y cuenta en "Proyectos finalizados".

---

## Prueba realizada

**Prueba automática** (`backend/tests/Ci2dt2.IntegrationTests/CicloVidaProyectoTests.cs`): recorre todo el ciclo con usuarios reales de cada rol, sin atajos en la base de datos. Verifica:
- cada estado;
- las notificaciones;
- las reglas: el IP no puede finalizar, sin observaciones no se finaliza, no se finaliza con hitos abiertos y un proyecto finalizado no admite más cambios;
- el historial ("Aprueba, Avala, Aprobado, Inicia ejecución, Finaliza");
- la publicación en el portal.

**Prueba en la interfaz** (entorno local, 27/11/2026): se recorrió el mismo ciclo con la propuesta **CI2DT2-2026-009**, "Plataforma de semaforización inteligente".

| Paso | Rol | Resultado |
|---|---|---|
| Crear la propuesta | Investigador | Se creó y quedó En elaboración |
| Registrar una fuente y enviar a revisión | Investigador | Quedó En revisión, etapa Coordinador; el Coordinador fue notificado |
| Aprobar | Coordinador | Pasó a la etapa Director |
| Avalar | Director | Quedó Presentado |
| Cargar el acto y registrar la aprobación | Director | Quedó Aprobado y se creó el cronograma |
| Cargar el plan e iniciar la ejecución | Director | Quedó En ejecución |
| Crear el hito y registrar el avance del 100 % | Investigador | El avance quedó Pendiente; el IP no lo puede validar |
| Validar el avance | Coordinador | El proyecto quedó al 100 % |
| Finalizar con el hito abierto | Director | Rechazado: "complete primero los hitos del cronograma (1 sin completar)" |
| Completar el hito con evidencia | Investigador | El hito quedó Completado |
| Finalizar | Director | Quedó **Finalizado**, con el historial completo |
| Consultar el portal | Visitante | El proyecto aparece como Finalizado |
