"use client";

import Link from "next/link";
import { useEffect, useRef, useState, useTransition } from "react";
import { Boton, Cargando, claseEntrada } from "@/components/ui";
import { api } from "@/lib/api";
import { useAuth } from "@/lib/auth-store";
import {
  SECCIONES_PREDETERMINADAS,
  useFormulacionStore,
  type IdeaProyectoPropuesta,
  type ResumenConvocatoria,
  type SeccionCanvas,
} from "@/lib/formulacion-store";

export default function AsistenteFormulacionPage() {
  const usuario = useAuth((s) => s.usuario);
  const token = useAuth((s) => s.accessToken);
  const [, startTransition] = useTransition();

  const {
    borradorId,
    tituloTentativo,
    lineaInvestigacion,
    convocatoriaNombre,
    convocatoriaResumen,
    ideasPropuestas,
    ideaSeleccionada,
    secciones,
    mensajes,
    pasoActivo,
    seccionActiva,
    streamingTexto,
    seccionEnStreaming,
    cargando,
    setBorradorId,
    setTituloTentativo,
    setContextoInvestigador,
    setConvocatoriaYAnalisis,
    seleccionarIdea,
    setIdeaPersonalizada,
    actualizarSeccion,
    agregarMensaje,
    setStreaming,
    setPasoActivo,
    setSeccionActiva,
    setCargando,
    reiniciarEstado,
  } = useFormulacionStore();

  const [archivoSeleccionado, setArchivoSeleccionado] = useState<File | null>(null);
  const [tipoCarga, setTipoCarga] = useState<"tdr" | "borrador">("tdr");
  const [mensajePrompt, setMensajePrompt] = useState("");
  const [seccionObjetivo, setSeccionObjetivo] = useState("justificacion");
  const [ideaPersonalizadaTitulo, setIdeaPersonalizadaTitulo] = useState("");
  const [ideaPersonalizadaDesc, setIdeaPersonalizadaDesc] = useState("");
  const [mostrarModalIdeaPropia, setMostrarModalIdeaPropia] = useState(false);
  const [notificacionExito, setNotificacionExito] = useState<string | null>(null);
  const [errorLocal, setErrorLocal] = useState<string | null>(null);
  const [exportando, setExportando] = useState(false);

  const scrollChatRef = useRef<HTMLDivElement>(null);
  const inputArchivoRef = useRef<HTMLInputElement>(null);

  // Inicializar contexto del investigador autenticado
  useEffect(() => {
    if (usuario && !lineaInvestigacion) {
      const lineaDefecto =
        usuario.roles.includes("Investigador") || usuario.roles.includes("Coordinador")
          ? "Tecnologías de Información y Ciencias Computacionales"
          : "Línea General Institucional CI²DT²";
      setContextoInvestigador(null, lineaDefecto);
    }
  }, [usuario, lineaInvestigacion, setContextoInvestigador]);

  // Autoscroll del chat
  useEffect(() => {
    scrollChatRef.current?.scrollIntoView({ behavior: "smooth" });
  }, [mensajes, streamingTexto]);

  // Limpiar notificaciones temporales
  useEffect(() => {
    if (notificacionExito) {
      const timer = setTimeout(() => setNotificacionExito(null), 3500);
      return () => clearTimeout(timer);
    }
  }, [notificacionExito]);

  // Manejo de carga de documento (TDR o Borrador previo)
  async function procesarDocumento() {
    if (!archivoSeleccionado) return;
    setCargando(true);
    setErrorLocal(null);

    try {
      const formData = new FormData();
      formData.append("archivo", archivoSeleccionado);

      // 1. Extraer texto estructurado con el servicio backend
      const resExtraer = await fetch("/api/v1/proyectos/asistente/documentos/extraer", {
        method: "POST",
        headers: token ? { Authorization: `Bearer ${token}` } : {},
        body: formData,
      });

      if (!resExtraer.ok) {
        const errorData = await resExtraer.json().catch(() => ({}));
        throw new Error(errorData.error ?? "Error al procesar el archivo.");
      }

      const datosDoc = await resExtraer.json();

      agregarMensaje({
        rol: "usuario",
        texto: `He subido el documento "${archivoSeleccionado.name}" (${datosDoc.totalPaginas} páginas, ${datosDoc.totalPalabras.toLocaleString()} palabras).`,
      });

      if (tipoCarga === "tdr") {
        // Analizar TDR y generar ideas
        setPasoActivo("analisis");
        const resAnalisis = await api<{
          resumen: ResumenConvocatoria;
          ideasPropuestas: IdeaProyectoPropuesta[];
        }>("/proyectos/asistente/analizar-tdr", {
          method: "POST",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify({
            textoTdr: datosDoc.textoCompleto,
            nombreConvocatoria: archivoSeleccionado.name.replace(/\.[^/.]+$/, ""),
            lineaInvestigacion: lineaInvestigacion,
          }),
        });

        setConvocatoriaYAnalisis(
          archivoSeleccionado.name.replace(/\.[^/.]+$/, ""),
          resAnalisis.resumen,
          resAnalisis.ideasPropuestas
        );

        agregarMensaje({
          rol: "asistente",
          texto: `Analicé con éxito los Términos de Referencia de **${resAnalisis.resumen.entidad}**. He identificado los rubros elegibles y formulado 3 ideas de proyectos estratégicamente alineadas con tu línea de investigación. Revisa las opciones a continuación:`,
          tipoMensaje: "propuesta_ideas",
          metadatos: resAnalisis,
        });
      } else {
        // Auditar borrador existente (Gap Analysis)
        const resAuditoria = await api<{
          diagnosticoGeneral: string;
          secciones: {
            codigoSeccion: string;
            tituloSeccion: string;
            estado: "Completa" | "Incompleta" | "Faltante";
            contenidoDetectado?: string;
            recomendaciones: string;
          }[];
        }>("/proyectos/asistente/auditar-borrador", {
          method: "POST",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify({
            textoBorrador: datosDoc.textoCompleto,
            convocatoriaMetadataJson: convocatoriaResumen ? JSON.stringify(convocatoriaResumen) : null,
          }),
        });

        // Poblar secciones detectadas
        for (const sec of resAuditoria.secciones) {
          if (sec.contenidoDetectado && sec.contenidoDetectado.trim().length > 0) {
            actualizarSeccion(
              sec.codigoSeccion,
              sec.contenidoDetectado,
              sec.estado === "Completa" ? "Aprobado" : "Generado",
              sec.recomendaciones
            );
          }
        }

        setPasoActivo("canvas");
        agregarMensaje({
          rol: "asistente",
          texto: `### Diagnóstico del Borrador Previo\n${resAuditoria.diagnosticoGeneral}\n\nHe precargado en el panel derecho los apartados identificados en tu documento. ¿Qué sección te gustaría afinar primero?`,
          tipoMensaje: "auditoria_brechas",
        });
      }
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : "Error desconocido al procesar el archivo.";
      setErrorLocal(msg);
      agregarMensaje({
        rol: "asistente",
        texto: `Ocurrió un inconveniente al analizar el archivo: ${msg}. Por favor intenta nuevamente o sube otro formato.`,
      });
    } finally {
      setCargando(false);
      setArchivoSeleccionado(null);
    }
  }

  // Crear o asegurar borrador en backend
  async function asegurarBorradorEnServidor(): Promise<number> {
    if (borradorId) return borradorId;

    const res = await api<{ id: number }>("/proyectos/asistente/borradores", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({
        tituloTentativo,
        lineaInvestigacion,
        convocatoriaNombre,
        convocatoriaMetadataJson: convocatoriaResumen ? JSON.stringify(convocatoriaResumen) : null,
      }),
    });

    setBorradorId(res.id);
    return res.id;
  }

  // Redacción en streaming SSE de una sección
  async function redactarSeccionConIa(codigo: string, instruccionesExtra?: string) {
    setCargando(true);
    setStreaming(codigo, "");
    setErrorLocal(null);

    try {
      const idBorrador = await asegurarBorradorEnServidor();

      agregarMensaje({
        rol: "usuario",
        texto: `Por favor redacta la sección **${secciones[codigo]?.tituloSeccion ?? codigo}**${
          instruccionesExtra ? `. Instrucciones adicionales: ${instruccionesExtra}` : ""
        }.`,
      });

      const res = await fetch("/api/v1/proyectos/asistente/redactar-seccion", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          ...(token ? { Authorization: `Bearer ${token}` } : {}),
        },
        body: JSON.stringify({
          borradorId: idBorrador,
          codigoSeccion: codigo,
          instruccionesUsuario: instruccionesExtra || mensajePrompt || null,
          ideaSeleccionada: ideaSeleccionada ? `${ideaSeleccionada.titulo}: ${ideaSeleccionada.descripcionBreve}` : null,
        }),
      });

      if (!res.ok) throw new Error("Error en el servicio de redacción con streaming.");
      if (!res.body) throw new Error("Flujo de respuesta no disponible.");

      const reader = res.body.getReader();
      const decoder = new TextDecoder();
      let acumulado = "";

      while (true) {
        const { value, done } = await reader.read();
        if (done) break;

        const chunkText = decoder.decode(value, { stream: true });
        const lineas = chunkText.split("\n\n");

        for (const linea of lineas) {
          const limpia = linea.trim();
          if (!limpia.startsWith("data: ")) continue;
          const datos = limpia.slice(6);
          if (datos === "[DONE]") break;

          try {
            const parsed = JSON.parse(datos);
            if (parsed.chunk) {
              acumulado += parsed.chunk;
              setStreaming(codigo, acumulado);
            }
          } catch {
            // Ignorar chunk parcial
          }
        }
      }

      agregarMensaje({
        rol: "asistente",
        texto: acumulado,
        tipoMensaje: "propuesta_seccion",
        seccionDestino: codigo,
      });

      setMensajePrompt("");
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : "Error al redactar la sección.";
      setErrorLocal(msg);
      agregarMensaje({
        rol: "asistente",
        texto: `No pude completar la redacción de la sección: ${msg}`,
      });
    } finally {
      setCargando(false);
      setStreaming(null, null);
    }
  }

  // Transferencia de propuesta al Canvas
  async function transferirAlBorrador(codigoSeccion: string, contenido: string) {
    startTransition(() => {
      actualizarSeccion(codigoSeccion, contenido, "Generado");
      setSeccionActiva(codigoSeccion);
      setNotificacionExito(`¡Sección "${secciones[codigoSeccion]?.tituloSeccion}" añadida al borrador con éxito!`);
    });

    if (borradorId) {
      try {
        await api(`/proyectos/asistente/borradores/${borradorId}/secciones/${codigoSeccion}`, {
          method: "PUT",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify({ contenido, estado: "Generado" }),
        });
      } catch {
        // Sync optimista en cliente ya aplicado
      }
    }
  }

  // Guardado manual o aprobación
  async function alternarAprobacionSeccion(codigo: string) {
    const sec = secciones[codigo];
    if (!sec) return;
    const nuevoEstado = sec.estado === "Aprobado" ? "Modificado" : "Aprobado";
    actualizarSeccion(codigo, sec.contenido, nuevoEstado);

    if (borradorId) {
      await api(`/proyectos/asistente/borradores/${borradorId}/secciones/${codigo}`, {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ contenido: sec.contenido, estado: nuevoEstado }),
      }).catch(() => {});
    }
  }

  // Descarga del borrador consolidado en formato Word (.docx)
  async function descargarDocx() {
    setExportando(true);
    setErrorLocal(null);

    try {
      // 1. Asegurar borrador y sincronizar secciones en el servidor
      const id = await asegurarBorradorEnServidor();
      for (const [codigo, sec] of Object.entries(secciones)) {
        if (sec.contenido) {
          await api(`/proyectos/asistente/borradores/${id}/secciones/${codigo}`, {
            method: "PUT",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ contenido: sec.contenido, estado: sec.estado }),
          }).catch(() => {});
        }
      }

      // 2. Solicitar el archivo binario DOCX
      const res = await fetch(`/api/v1/proyectos/asistente/borradores/${id}/exportar-docx`, {
        headers: token ? { Authorization: `Bearer ${token}` } : {},
      });

      if (!res.ok) {
        throw new Error("No se pudo generar el archivo DOCX.");
      }

      const blob = await res.blob();
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement("a");
      a.href = url;
      const nombreLimpio = tituloTentativo.replace(/[^\w\-\.]+/g, "_").slice(0, 40);
      a.download = `${nombreLimpio || "Proyecto"}_Borrador.docx`;
      document.body.appendChild(a);
      a.click();
      window.URL.revokeObjectURL(url);
      document.body.removeChild(a);

      setNotificacionExito("¡Documento Word (.docx) generado y descargado exitosamente!");
    } catch (err: unknown) {
      const msg = err instanceof Error ? err.message : "Error al exportar el documento.";
      setErrorLocal(msg);
    } finally {
      setExportando(false);
    }
  }

  // Métricas de progreso
  const totalPalabrasProyecto = Object.values(secciones).reduce((acc, s) => acc + s.conteoPalabras, 0);
  const seccionesConContenido = Object.values(secciones).filter((s) => s.contenido.trim().length > 0).length;
  const porcentajeAvance = Math.round((seccionesConContenido / SECCIONES_PREDETERMINADAS.length) * 100);

  return (
    <div className="flex flex-col gap-6">
      {/* 1. Encabezado de Navegación y Contexto */}
      <div className="flex flex-col justify-between gap-4 border-b border-slate-200 pb-4 sm:flex-row sm:items-center">
        <div>
          <div className="flex items-center gap-2 text-xs text-slate-500">
            <Link href="/proyectos" className="hover:underline">
              Proyectos
            </Link>
            <span>/</span>
            <span className="font-semibold text-emerald-800">Asistente de Formulación IA</span>
          </div>
          <h1 className="text-2xl font-bold tracking-tight text-slate-900">{tituloTentativo}</h1>
          <p className="text-xs text-slate-600">
            Investigador: <span className="font-medium text-slate-800">{usuario ? `${usuario.nombres} ${usuario.apellidos}` : "Investigador"}</span> | Línea:{" "}
            <span className="font-medium text-emerald-800">{lineaInvestigacion ?? "CI²DT²"}</span>
            {convocatoriaNombre && (
              <>
                {" "}
                | Convocatoria: <span className="font-medium text-slate-800">{convocatoriaNombre}</span>
              </>
            )}
          </p>
        </div>

        <div className="flex flex-wrap items-center gap-2">
          {borradorId && (
            <span className="rounded-full bg-emerald-50 px-3 py-1 text-xs font-semibold text-emerald-800 border border-emerald-200">
              Borrador #{borradorId} Guardado
            </span>
          )}
          <Boton
            variante="primario"
            disabled={exportando || cargando}
            onClick={descargarDocx}
            className="flex items-center gap-1.5 shadow-xs text-xs py-2 px-3 font-semibold"
          >
            <span>📥 {exportando ? "Generando Word..." : "Descargar Borrador (.DOCX)"}</span>
          </Boton>
          <Boton
            variante="secundario"
            onClick={() => {
              if (confirm("¿Deseas reiniciar la sesión de formulación? Se limpiará el lienzo actual.")) {
                reiniciarEstado();
              }
            }}
          >
            Reiniciar
          </Boton>
          <Link
            href="/proyectos"
            className="inline-flex items-center justify-center rounded-md border border-slate-300 bg-white px-4 py-2 text-sm font-medium text-slate-800 hover:bg-slate-50"
          >
            Salir a Proyectos
          </Link>
        </div>
      </div>

      {/* Alerta de Notificación de Inserción */}
      {notificacionExito && (
        <div
          role="status"
          className="flex items-center justify-between rounded-md border border-emerald-300 bg-emerald-50 px-4 py-3 text-sm font-medium text-emerald-900 shadow-sm animate-pulse"
        >
          <span>✨ {notificacionExito}</span>
          <button
            type="button"
            onClick={() => setNotificacionExito(null)}
            className="text-emerald-700 hover:text-emerald-900 text-xs uppercase font-bold"
          >
            Cerrar
          </button>
        </div>
      )}

      {errorLocal && (
        <div role="alert" className="rounded-md border border-red-300 bg-red-50 p-3 text-sm text-red-800">
          {errorLocal}
        </div>
      )}

      {/* 2. Layout Dual-Pane: Chat Interactivo (Izquierda) + Draft Canvas (Derecha) */}
      <div className="grid grid-cols-1 gap-6 lg:grid-cols-12">
        {/* PANEL IZQUIERDO: CHAT, INGESTA E INTERACCIÓN (5 columnas) */}
        <div className="flex flex-col rounded-lg border border-slate-200 bg-white shadow-sm lg:col-span-5 h-[calc(100vh-14rem)] min-h-[580px]">
          {/* Header del Panel */}
          <div className="flex items-center justify-between border-b border-slate-200 bg-slate-50 px-4 py-3">
            <div className="flex items-center gap-2">
              <span className="flex h-2.5 w-2.5 rounded-full bg-emerald-600 animate-pulse" />
              <h2 className="text-sm font-semibold text-slate-800">Asistente de Formulación</h2>
            </div>
            <div className="flex items-center gap-1.5 text-xs text-slate-500">
              <span className="capitalize">{pasoActivo}</span>
            </div>
          </div>

          {/* Área de Mensajes y Formularios de Etapa */}
          <div className="flex-1 overflow-y-auto p-4 space-y-4 text-sm">
            {/* Mensaje de Bienvenida */}
            <div className="rounded-lg bg-slate-50 p-3.5 text-slate-700 border border-slate-200">
              <p className="font-semibold text-slate-900 mb-1">
                ¡Hola, {usuario ? `${usuario.nombres} ${usuario.apellidos}` : "Investigador"}!
              </p>
              <p className="text-xs text-slate-600 mb-3">
                Soy tu Asistente Científico para estructurar y redactar propuestas de investigación. Para comenzar:
              </p>
              <div className="flex flex-col gap-2">
                <button
                  type="button"
                  onClick={() => {
                    setTipoCarga("tdr");
                    inputArchivoRef.current?.click();
                  }}
                  className="flex items-center justify-between rounded-md border border-emerald-300 bg-white p-2.5 text-left text-xs font-medium text-emerald-800 hover:bg-emerald-50 transition-colors shadow-sm"
                >
                  <span>📄 Subir Términos de Referencia (TDR)</span>
                  <span className="text-[10px] bg-emerald-100 text-emerald-800 px-1.5 py-0.5 rounded font-mono">
                    PDF / DOCX
                  </span>
                </button>
                <button
                  type="button"
                  onClick={() => {
                    setTipoCarga("borrador");
                    inputArchivoRef.current?.click();
                  }}
                  className="flex items-center justify-between rounded-md border border-slate-300 bg-white p-2.5 text-left text-xs font-medium text-slate-700 hover:bg-slate-50 transition-colors"
                >
                  <span>📝 Importar un Borrador Previo</span>
                  <span className="text-[10px] bg-slate-100 text-slate-600 px-1.5 py-0.5 rounded font-mono">
                    Gap Analysis
                  </span>
                </button>
                <button
                  type="button"
                  onClick={() => setMostrarModalIdeaPropia(true)}
                  className="flex items-center justify-between rounded-md border border-dashed border-slate-300 bg-white p-2 text-left text-xs font-medium text-slate-600 hover:bg-slate-50"
                >
                  <span>💡 Comenzar con mi propia idea personalizada</span>
                </button>
              </div>

              <input
                ref={inputArchivoRef}
                type="file"
                accept=".pdf,.docx"
                className="hidden"
                onChange={(e) => {
                  const file = e.target.files?.[0];
                  if (file) {
                    setArchivoSeleccionado(file);
                  }
                }}
              />
            </div>

            {/* Archivo Seleccionado pendiente de procesamiento */}
            {archivoSeleccionado && (
              <div className="rounded-lg border border-emerald-300 bg-emerald-50/70 p-3 text-xs">
                <p className="font-semibold text-emerald-900 mb-1">
                  Archivo listo para {tipoCarga === "tdr" ? "análisis de TDR" : "auditoría de borrador"}:
                </p>
                <p className="text-emerald-800 font-mono mb-2">
                  {archivoSeleccionado.name} ({(archivoSeleccionado.size / (1024 * 1024)).toFixed(2)} MB)
                </p>
                <div className="flex gap-2">
                  <Boton disabled={cargando} onClick={procesarDocumento} className="text-xs py-1.5 px-3">
                    {cargando ? "Procesando..." : "Analizar con Agente IA"}
                  </Boton>
                  <Boton
                    variante="secundario"
                    disabled={cargando}
                    onClick={() => setArchivoSeleccionado(null)}
                    className="text-xs py-1.5 px-3"
                  >
                    Cancelar
                  </Boton>
                </div>
              </div>
            )}

            {/* Tarjeta de Resumen Ejecutivo de la Convocatoria si existe */}
            {convocatoriaResumen && (
              <div className="rounded-lg border border-slate-200 bg-slate-50 p-3 text-xs space-y-2">
                <div className="flex items-center justify-between border-b border-slate-200 pb-1.5">
                  <span className="font-bold text-slate-900 uppercase tracking-wide">
                    {convocatoriaResumen.entidad}
                  </span>
                  <span className="rounded bg-emerald-100 px-2 py-0.5 text-[10px] font-semibold text-emerald-800">
                    Tope: {convocatoriaResumen.montoMaximo ?? "Consultar"}
                  </span>
                </div>
                <p className="text-slate-700">{convocatoriaResumen.objetivoConvocatoria}</p>
                <div>
                  <span className="font-semibold text-slate-800 block mb-1">Rubros Financiables:</span>
                  <div className="flex flex-wrap gap-1">
                    {convocatoriaResumen.rubrosFinanciables.map((rubro, idx) => (
                      <span key={idx} className="rounded-md bg-white border border-slate-200 px-2 py-0.5 text-[11px]">
                        {rubro}
                      </span>
                    ))}
                  </div>
                </div>
              </div>
            )}

            {/* Ideas Propuestas por el Asistente */}
            {ideasPropuestas.length > 0 && !ideaSeleccionada && (
              <div className="space-y-3">
                <p className="text-xs font-semibold uppercase text-slate-500 tracking-wider">
                  Ideas de Proyectos Propuestas para tu Línea:
                </p>
                {ideasPropuestas.map((idea) => (
                  <div
                    key={idea.id}
                    className="rounded-lg border border-slate-200 bg-white p-3 hover:border-emerald-500 hover:shadow-sm transition-all space-y-2"
                  >
                    <h4 className="font-semibold text-emerald-900 text-xs">{idea.titulo}</h4>
                    <p className="text-xs text-slate-600">{idea.descripcionBreve}</p>
                    <div className="rounded bg-slate-50 p-2 text-[11px] text-slate-600 border border-slate-100">
                      <strong>Alineación TDR:</strong> {idea.justificacionAlineacion}
                    </div>
                    <div className="flex items-center justify-between pt-1">
                      <div className="flex flex-wrap gap-1">
                        {idea.palabrasClave.map((k, i) => (
                          <span key={i} className="text-[10px] bg-slate-100 text-slate-600 px-1.5 py-0.2 rounded">
                            #{k}
                          </span>
                        ))}
                      </div>
                      <Boton onClick={() => seleccionarIdea(idea)} className="text-xs py-1 px-2.5">
                        Seleccionar esta Idea
                      </Boton>
                    </div>
                  </div>
                ))}
              </div>
            )}

            {/* Historial de Mensajes de Conversación */}
            {mensajes.map((msg) => (
              <div
                key={msg.id}
                className={`flex flex-col ${msg.rol === "usuario" ? "items-end" : "items-start"}`}
              >
                <div
                  className={`max-w-[92%] rounded-lg p-3 text-xs leading-relaxed ${
                    msg.rol === "usuario"
                      ? "bg-emerald-800 text-white"
                      : "bg-slate-100 text-slate-800 border border-slate-200"
                  }`}
                >
                  <div className="prose prose-sm max-w-none whitespace-pre-wrap">{msg.texto}</div>

                  {/* Botón de Inserción directa en el borrador si es una sección redactada */}
                  {msg.tipoMensaje === "propuesta_seccion" && msg.seccionDestino && (
                    <div className="mt-3 border-t border-slate-300 pt-2 flex items-center justify-between">
                      <span className="text-[11px] font-semibold text-slate-600">
                        Sección: {secciones[msg.seccionDestino]?.tituloSeccion ?? msg.seccionDestino}
                      </span>
                      <Boton
                        onClick={() => transferirAlBorrador(msg.seccionDestino!, msg.texto)}
                        className="text-xs py-1 px-3 bg-emerald-700 hover:bg-emerald-800 text-white font-medium shadow-sm"
                      >
                        ✨ Añadir al Borrador
                      </Boton>
                    </div>
                  )}
                </div>
              </div>
            ))}

            {/* Indicador de Streaming en Vivo */}
            {seccionEnStreaming && (
              <div className="rounded-lg border border-emerald-300 bg-emerald-50/50 p-3 text-xs space-y-2">
                <div className="flex items-center gap-2 text-emerald-800 font-semibold">
                  <span className="h-2 w-2 rounded-full bg-emerald-600 animate-ping" />
                  Redactando sección: {secciones[seccionEnStreaming]?.tituloSeccion ?? seccionEnStreaming}...
                </div>
                <div className="whitespace-pre-wrap font-sans text-slate-700 bg-white p-2 rounded border border-emerald-100 max-h-48 overflow-y-auto">
                  {streamingTexto || "Conectando con el redactor científico..."}
                </div>
              </div>
            )}

            {cargando && !seccionEnStreaming && <Cargando texto="El Asistente está analizando..." />}

            <div ref={scrollChatRef} />
          </div>

          {/* Input conversacional y selector de sección objetivo */}
          <div className="border-t border-slate-200 bg-slate-50 p-3 space-y-2">
            <div className="flex items-center gap-2 text-xs">
              <span className="text-slate-500 font-medium">Sección objetivo:</span>
              <select
                value={seccionObjetivo}
                onChange={(e) => setSeccionObjetivo(e.target.value)}
                className="rounded border border-slate-300 bg-white px-2 py-1 text-xs text-slate-800 focus:outline-none focus:border-emerald-600"
              >
                {SECCIONES_PREDETERMINADAS.map((s) => (
                  <option key={s.codigo} value={s.codigo}>
                    {s.orden}. {s.titulo}
                  </option>
                ))}
              </select>
            </div>

            <div className="flex gap-2">
              <textarea
                rows={2}
                value={mensajePrompt}
                onChange={(e) => setMensajePrompt(e.target.value)}
                placeholder="Indica qué deseas que el asistente formule o mejore para esta sección..."
                className={`${claseEntrada} text-xs resize-none`}
                onKeyDown={(e) => {
                  if (e.key === "Enter" && !e.shiftKey) {
                    e.preventDefault();
                    if (mensajePrompt.trim() && !cargando) {
                      redactarSeccionConIa(seccionObjetivo, mensajePrompt.trim());
                    }
                  }
                }}
              />
              <Boton
                disabled={cargando || !mensajePrompt.trim()}
                onClick={() => redactarSeccionConIa(seccionObjetivo, mensajePrompt.trim())}
                className="self-end text-xs px-3 py-2"
              >
                Redactar
              </Boton>
            </div>
          </div>
        </div>

        {/* PANEL DERECHO: DRAFT CANVAS (LIENZO DE FORMULACIÓN) (7 columnas) */}
        <div className="flex flex-col rounded-lg border border-slate-200 bg-white shadow-sm lg:col-span-7 h-[calc(100vh-14rem)] min-h-[580px]">
          {/* Header del Canvas */}
          <div className="flex flex-col gap-2 border-b border-slate-200 bg-slate-50 px-4 py-3 sm:flex-row sm:items-center sm:justify-between">
            <div>
              <h2 className="text-sm font-bold text-slate-900">Draft Canvas (Lienzo del Borrador)</h2>
              <p className="text-xs text-slate-500">
                {seccionesConContenido} de {SECCIONES_PREDETERMINADAS.length} apartados redactados ·{" "}
                <span className="font-semibold text-emerald-800">
                  {totalPalabrasProyecto.toLocaleString()} palabras
                </span>
              </p>
            </div>

            {/* Barra de Progreso */}
            <div className="flex items-center gap-3">
              <div className="w-28 bg-slate-200 rounded-full h-2 overflow-hidden">
                <div
                  className="bg-emerald-600 h-2 rounded-full transition-all duration-500"
                  style={{ width: `${porcentajeAvance}%` }}
                />
              </div>
              <span className="text-xs font-semibold text-slate-700">{porcentajeAvance}%</span>
            </div>
          </div>

          {/* Navegación por Pestañas / Acordeón de Secciones */}
          <div className="flex border-b border-slate-200 bg-slate-100/60 overflow-x-auto text-xs px-2 pt-2 gap-1">
            {SECCIONES_PREDETERMINADAS.map((s) => {
              const sec = secciones[s.codigo];
              const tieneTexto = sec?.contenido && sec.contenido.trim().length > 0;
              const estaActiva = seccionActiva === s.codigo;

              return (
                <button
                  key={s.codigo}
                  type="button"
                  onClick={() => setSeccionActiva(s.codigo)}
                  className={`flex items-center gap-1.5 whitespace-nowrap rounded-t-md px-3 py-2 font-medium transition-colors border-t border-x ${
                    estaActiva
                      ? "bg-white border-slate-200 text-emerald-900 shadow-sm"
                      : "border-transparent text-slate-600 hover:text-slate-900 hover:bg-slate-200/50"
                  }`}
                >
                  <span
                    className={`h-2 w-2 rounded-full ${
                      sec?.estado === "Aprobado"
                        ? "bg-emerald-600"
                        : tieneTexto
                        ? "bg-amber-500"
                        : "bg-slate-300"
                    }`}
                  />
                  <span>{s.titulo}</span>
                  {tieneTexto && (
                    <span className="text-[10px] text-slate-400 font-mono">({sec.conteoPalabras}p)</span>
                  )}
                </button>
              );
            })}
          </div>

          {/* Editor Activo de la Sección Seleccionada */}
          <div className="flex-1 flex flex-col p-4 overflow-y-auto">
            {(() => {
              const sec = secciones[seccionActiva] ?? {
                codigoSeccion: seccionActiva,
                tituloSeccion: seccionActiva,
                contenido: "",
                estado: "Vacio",
                conteoPalabras: 0,
              };

              return (
                <div className="flex flex-col flex-1 gap-3">
                  <div className="flex items-center justify-between border-b border-slate-100 pb-2">
                    <div>
                      <h3 className="text-base font-bold text-slate-900">{sec.tituloSeccion}</h3>
                      <p className="text-xs text-slate-500">
                        Estado:{" "}
                        <span
                          className={`font-semibold rounded px-1.5 py-0.5 text-[10px] ${
                            sec.estado === "Aprobado"
                              ? "bg-emerald-100 text-emerald-800"
                              : sec.estado === "Generado"
                              ? "bg-amber-100 text-amber-800"
                              : sec.estado === "Modificado"
                              ? "bg-blue-100 text-blue-800"
                              : "bg-slate-100 text-slate-600"
                          }`}
                        >
                          {sec.estado}
                        </span>{" "}
                        | {sec.conteoPalabras} palabras
                      </p>
                    </div>

                    <div className="flex items-center gap-2">
                      <Boton
                        variante="secundario"
                        className="text-xs py-1 px-2.5"
                        onClick={() => {
                          setSeccionObjetivo(sec.codigoSeccion);
                          redactarSeccionConIa(sec.codigoSeccion);
                        }}
                      >
                        ⚡ Redactar con IA
                      </Boton>
                      <Boton
                        variante={sec.estado === "Aprobado" ? "secundario" : "primario"}
                        className="text-xs py-1 px-2.5"
                        onClick={() => alternarAprobacionSeccion(sec.codigoSeccion)}
                      >
                        {sec.estado === "Aprobado" ? "Desmarcar Aprobado" : "✓ Aprobar Sección"}
                      </Boton>
                    </div>
                  </div>

                  {/* Observaciones del Asistente si existen */}
                  {sec.observacionesIA && (
                    <div className="rounded-md border border-amber-200 bg-amber-50 p-2.5 text-xs text-amber-900">
                      <strong>Recomendación IA:</strong> {sec.observacionesIA}
                    </div>
                  )}

                  {/* Área de Edición Directa */}
                  <div className="flex-1 flex flex-col">
                    <textarea
                      value={sec.contenido}
                      onChange={(e) => actualizarSeccion(sec.codigoSeccion, e.target.value, "Modificado")}
                      placeholder={`El contenido de la sección "${sec.tituloSeccion}" aparecerá aquí cuando el asistente la redacte, o puedes redactarla directamente...`}
                      className="w-full flex-1 rounded-md border border-slate-300 p-3 text-xs font-sans text-slate-800 leading-relaxed focus:border-emerald-600 focus:outline-none focus:ring-1 focus:ring-emerald-600 min-h-[300px]"
                    />
                  </div>
                </div>
              );
            })()}
          </div>
        </div>
      </div>

      {/* Modal para ingresar Idea Personalizada */}
      {mostrarModalIdeaPropia && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/40 backdrop-blur-xs p-4">
          <div className="w-full max-w-md rounded-lg bg-white p-5 shadow-lg border border-slate-200 space-y-4">
            <h3 className="text-base font-bold text-slate-900">Definir Idea de Proyecto Personalizada</h3>
            <div className="space-y-3 text-xs">
              <div>
                <label className="font-medium text-slate-700 block mb-1">Título Tentativo:</label>
                <input
                  type="text"
                  value={ideaPersonalizadaTitulo}
                  onChange={(e) => setIdeaPersonalizadaTitulo(e.target.value)}
                  placeholder="Ej: Plataforma de Analítica Predictiva para Cultivos..."
                  className={claseEntrada}
                />
              </div>
              <div>
                <label className="font-medium text-slate-700 block mb-1">Descripción y Alcance:</label>
                <textarea
                  rows={3}
                  value={ideaPersonalizadaDesc}
                  onChange={(e) => setIdeaPersonalizadaDesc(e.target.value)}
                  placeholder="Describe brevemente el problema, la propuesta y la solución esperada..."
                  className={claseEntrada}
                />
              </div>
            </div>
            <div className="flex justify-end gap-2 pt-2 border-t border-slate-100">
              <Boton
                variante="secundario"
                onClick={() => setMostrarModalIdeaPropia(false)}
                className="text-xs py-1.5 px-3"
              >
                Cancelar
              </Boton>
              <Boton
                disabled={!ideaPersonalizadaTitulo.trim()}
                onClick={() => {
                  setIdeaPersonalizada(ideaPersonalizadaTitulo.trim(), ideaPersonalizadaDesc.trim());
                  setMostrarModalIdeaPropia(false);
                }}
                className="text-xs py-1.5 px-3"
              >
                Establecer en Lienzo
              </Boton>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
