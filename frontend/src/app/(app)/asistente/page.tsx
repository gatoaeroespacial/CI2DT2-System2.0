"use client";

import { startTransition, useEffect, useRef, useState, type FormEvent, type KeyboardEvent } from "react";
import { Boton, Encabezado } from "@/components/ui";
import { api, json } from "@/lib/api";
import { useAuth } from "@/lib/auth-store";

type Mensaje = { id: number; autor: "usuario" | "asistente"; texto: string };
type RespuestaAsistente = { respuesta: string };
type SolicitudAsistente = {
  pregunta: string;
  resumenConversacion: string;
  historialReciente: { rol: "usuario" | "asistente"; texto: string }[];
};

const MAXIMO_MENSAJES_VISIBLES = 100;
const MAXIMO_MENSAJES_RECIENTES = 8;
const MAXIMO_RESUMEN = 2000;

export default function AsistentePage() {
  const usuario = useAuth((estado) => estado.usuario);
  const [pregunta, setPregunta] = useState("");
  const [mensajes, setMensajes] = useState<Mensaje[]>([]);
  const [cargando, setCargando] = useState(false);
  const [chatUsuarioId, setChatUsuarioId] = useState<number | null>(null);
  const [errorAlmacenamiento, setErrorAlmacenamiento] = useState<string | null>(null);
  const finalMensajesRef = useRef<HTMLDivElement>(null);
  const mensajesRef = useRef<Mensaje[]>([]);
  const siguienteIdRef = useRef(0);
  const usuarioCargadoRef = useRef<number | null>(null);
  const chatListo = usuario?.id === chatUsuarioId;
  const rolesUsuario = usuario?.roles.slice().sort().join(",") ?? "";
  const claveAlmacenamiento = usuario ? `ci2dt2-asistente-v1:${usuario.id}:${rolesUsuario}` : null;
  const sugerencias = usuario?.roles.some((rol) => rol === "Admin" || rol === "Director")
    ? [
        "¿Cuántos proyectos hay en ejecución?",
        "¿Qué convocatorias están abiertas?",
        "¿Cuántos programas tiene el centro?",
        "¿Qué indicadores tienen semáforo rojo?",
      ]
    : ["Mis proyectos activos", "¿Cuántos proyectos tengo en ejecución?", "¿Qué programas tengo asociados?"];

  useEffect(() => {
    if (!usuario || !claveAlmacenamiento) return;

    usuarioCargadoRef.current = null;
    try {
      const guardado = localStorage.getItem(claveAlmacenamiento);
      const parsed: unknown = guardado ? JSON.parse(guardado) : [];
      const restaurados = Array.isArray(parsed)
        ? parsed.filter(esMensaje).slice(-MAXIMO_MENSAJES_VISIBLES)
        : [];
      mensajesRef.current = restaurados;
      siguienteIdRef.current = restaurados.reduce((mayor, mensaje) => Math.max(mayor, mensaje.id), 0);
      startTransition(() => {
        setMensajes(restaurados);
        setChatUsuarioId(usuario.id);
        setErrorAlmacenamiento(null);
      });
      usuarioCargadoRef.current = usuario.id;
    } catch {
      mensajesRef.current = [];
      startTransition(() => {
        setMensajes([]);
        setChatUsuarioId(usuario.id);
        setErrorAlmacenamiento("No pude recuperar el historial guardado en este navegador.");
      });
      usuarioCargadoRef.current = usuario.id;
    }
  }, [usuario, claveAlmacenamiento]);

  useEffect(() => {
    finalMensajesRef.current?.scrollIntoView({ behavior: "smooth", block: "end" });
  }, [mensajes, cargando]);

  function guardarMensajes(nuevosMensajes: Mensaje[]) {
    const acotados = nuevosMensajes.slice(-MAXIMO_MENSAJES_VISIBLES);
    mensajesRef.current = acotados;
    startTransition(() => setMensajes(acotados));
    if (!claveAlmacenamiento || usuarioCargadoRef.current !== usuario?.id) return;

    try {
      localStorage.setItem(claveAlmacenamiento, JSON.stringify(acotados));
      setErrorAlmacenamiento(null);
    } catch {
      setErrorAlmacenamiento("No pude guardar el historial. Revisa el espacio disponible en este navegador.");
    }
  }

  function agregarMensaje(mensaje: Mensaje) {
    guardarMensajes([...mensajesRef.current, mensaje]);
  }

  function iniciarNuevoChat() {
    guardarMensajes([]);
    if (claveAlmacenamiento) {
      try {
        localStorage.removeItem(claveAlmacenamiento);
      } catch {
        setErrorAlmacenamiento("No pude limpiar el historial guardado en este navegador.");
      }
    }
  }

  async function enviar(texto: string) {
    const valor = texto.trim();
    if (!valor || cargando || !chatListo) return;

    setPregunta("");
    const historial = mensajesRef.current;
    const inicioBuffer = Math.max(0, historial.length - MAXIMO_MENSAJES_RECIENTES);
    const resumenCompleto = historial
      .slice(0, inicioBuffer)
      .map((mensaje) => `${mensaje.autor === "usuario" ? "Usuario" : "Asistente"}: ${mensaje.texto.slice(0, 180)}`)
      .join("\n");
    const resumenConversacion = resumenCompleto.length > MAXIMO_RESUMEN
      ? resumenCompleto.slice(-MAXIMO_RESUMEN)
      : resumenCompleto;
    const historialReciente = historial.slice(-MAXIMO_MENSAJES_RECIENTES).map((mensaje) => ({
      rol: mensaje.autor,
      texto: mensaje.texto,
    }));
    siguienteIdRef.current += 1;
    agregarMensaje({ id: siguienteIdRef.current, autor: "usuario", texto: valor });
    setCargando(true);
    try {
      const solicitud: SolicitudAsistente = { pregunta: valor, resumenConversacion, historialReciente };
      const respuesta = await api<RespuestaAsistente>("/asistente/consultas", json("POST", solicitud));
      siguienteIdRef.current += 1;
      agregarMensaje({ id: siguienteIdRef.current, autor: "asistente", texto: respuesta.respuesta });
    } catch (error) {
      const mensaje = error instanceof Error ? error.message : "No fue posible consultar al asistente.";
      siguienteIdRef.current += 1;
      agregarMensaje({ id: siguienteIdRef.current, autor: "asistente", texto: mensaje });
    } finally {
      setCargando(false);
    }
  }

  function enviarFormulario(evento: FormEvent<HTMLFormElement>) {
    evento.preventDefault();
    void enviar(pregunta);
  }

  function manejarTecla(evento: KeyboardEvent<HTMLTextAreaElement>) {
    if (evento.key === "Enter" && !evento.shiftKey) {
      evento.preventDefault();
      void enviar(pregunta);
    }
  }

  return (
    <section className="mx-auto flex h-full min-h-0 w-full max-w-4xl flex-col">
      <Encabezado titulo="Asistente IA">
        <Boton variante="secundario" onClick={iniciarNuevoChat} disabled={!chatListo || cargando}>
          Nuevo chat
        </Boton>
      </Encabezado>
      {errorAlmacenamiento && <p role="alert" className="mb-3 text-sm text-amber-800">{errorAlmacenamiento}</p>}
      <div role="log" aria-live="polite" className="flex min-h-0 flex-1 flex-col gap-4 overflow-y-auto overscroll-contain pb-6">
        {!chatListo ? (
          <p role="status" className="m-auto text-sm text-slate-600">Recuperando tu conversación…</p>
        ) : mensajes.length === 0 ? (
          <div className="flex flex-1 flex-col items-center justify-center px-4 py-12 text-center">
            <div className="mb-3 grid h-12 w-12 place-items-center rounded-2xl bg-emerald-100 text-2xl text-emerald-900" aria-hidden="true">
              ✦
            </div>
            <h2 className="text-2xl font-semibold text-slate-900">Hola, ¿qué quieres consultar?</h2>
            <p className="mt-2 max-w-lg text-sm text-slate-600">
              Pregunta en lenguaje natural sobre la información del centro disponible para tu rol.
            </p>
            <div className="mt-7 flex max-w-2xl flex-wrap justify-center gap-2">
              {sugerencias.map((sugerencia) => (
                <button
                  key={sugerencia}
                  type="button"
                  disabled={cargando}
                  onClick={() => void enviar(sugerencia)}
                  className="rounded-2xl border border-slate-200 bg-white px-4 py-3 text-left text-sm text-slate-700 shadow-sm transition hover:border-emerald-300 hover:bg-emerald-50 disabled:opacity-60"
                >
                  {sugerencia}
                </button>
              ))}
            </div>
          </div>
        ) : (
          <div className="space-y-4 px-2 pt-4">
            {mensajes.map((mensaje) => (
              <p
                key={mensaje.id}
                className={`max-w-[85%] whitespace-pre-wrap rounded-2xl px-4 py-3 text-sm ${
                  mensaje.autor === "usuario"
                    ? "ml-auto bg-emerald-800 text-white"
                    : "mr-auto border border-slate-200 bg-white text-slate-800 shadow-sm"
                }`}
              >
                {mensaje.texto}
              </p>
            ))}
            {cargando && (
              <p role="status" className="mr-auto flex items-center gap-2 rounded-2xl border border-slate-200 bg-white px-4 py-3 text-sm text-slate-600">
                <span className="h-3 w-3 animate-spin rounded-full border-2 border-emerald-700 border-t-transparent" />
                Estoy revisando la información para ti…
              </p>
            )}
          </div>
        )}
        <div ref={finalMensajesRef} aria-hidden="true" />
      </div>
      <form onSubmit={enviarFormulario} className="mx-auto w-full max-w-3xl shrink-0 rounded-3xl border border-slate-200 bg-white p-3 shadow-lg">
        <label htmlFor="pregunta-asistente" className="sr-only">
          Escribe una consulta
        </label>
        <div className="flex items-end gap-3">
          <textarea
            id="pregunta-asistente"
            rows={1}
            maxLength={500}
            value={pregunta}
            onChange={(evento) => setPregunta(evento.target.value)}
            onKeyDown={manejarTecla}
            placeholder="Pregunta lo que quieras saber…"
            className="max-h-32 min-h-12 flex-1 resize-y bg-transparent px-3 py-3 text-sm outline-none"
          />
          <Boton type="submit" disabled={!chatListo || cargando || !pregunta.trim()} className="rounded-full">
            Enviar
          </Boton>
        </div>
        <p className="px-3 pb-1 text-xs text-slate-500">Las respuestas solo usan información autorizada para tu rol.</p>
      </form>
    </section>
  );
}

function esMensaje(valor: unknown): valor is Mensaje {
  if (!valor || typeof valor !== "object") return false;
  const mensaje = valor as Partial<Mensaje>;
  return typeof mensaje.id === "number" &&
    (mensaje.autor === "usuario" || mensaje.autor === "asistente") &&
    typeof mensaje.texto === "string";
}
