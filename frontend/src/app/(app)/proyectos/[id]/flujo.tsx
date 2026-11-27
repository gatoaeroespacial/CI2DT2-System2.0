"use client";

import { useQueryClient } from "@tanstack/react-query";
import { useState } from "react";
import { toast } from "sonner";
import { ErrorFormulario } from "@/components/controles";
import { Boton, Campo, claseEntrada } from "@/components/ui";
import { api, json } from "@/lib/api";
import { hoyIso } from "@/lib/formato";
import type { Flujo, Permisos, ProyectoDetalle } from "./tipos";

const MINIMO_OBSERVACIONES = 20;

const textoDecision: Record<string, string> = {
  Aprueba: "Aprobar y enviar al Director",
  Avala: "Otorgar aval (Presentado)",
  Devuelve: "Devolver al IP",
};

/**
 * Acciones del ciclo de vida en la ficha: lista de verificación y envío a revisión (HU-11),
 * decisiones de revisión y aval (HU-11), aprobación e inicio de ejecución (HU-12) y suspensión o
 * reanudación de un proyecto en ejecución.
 */
export function PanelFlujo({ proyecto, flujo, permisos }: { proyecto: ProyectoDetalle; flujo: Flujo; permisos: Permisos }) {
  const queryClient = useQueryClient();
  const [error, setError] = useState<string>();
  const [enviando, setEnviando] = useState(false);
  const [observaciones, setObservaciones] = useState("");
  const [presupuesto, setPresupuesto] = useState("");
  const [fechaInicio, setFechaInicio] = useState(hoyIso());
  const [justificacion, setJustificacion] = useState("");
  const [cierre, setCierre] = useState("");

  const ejecutar = async (accion: () => Promise<unknown>, exito: string) => {
    setError(undefined);
    setEnviando(true);
    try {
      await accion();
      toast.success(exito);
      setObservaciones("");
      setJustificacion("");
      setCierre("");
      await queryClient.invalidateQueries({ queryKey: ["proyecto", String(proyecto.id)] });
      await queryClient.invalidateQueries({ queryKey: ["proyectos"] });
    } catch (e) {
      setError(e instanceof Error ? e.message : "Ocurrió un error inesperado.");
    } finally {
      setEnviando(false);
    }
  };

  const pendientes = flujo.requisitos.filter((r) => !r.cumple);
  const hayAcciones = flujo.puedeEnviarRevision || flujo.decisionesDisponibles.length > 0
    || flujo.puedeRegistrarAprobacion || flujo.puedeIniciarEjecucion || flujo.etapaRevision !== null
    || flujo.puedeSuspender || flujo.puedeReanudar || flujo.puedeFinalizar;
  if (!hayAcciones) return null;

  return (
    <section aria-label="Trámite institucional" className="mb-4 flex flex-col gap-3 rounded-lg border border-emerald-200 bg-emerald-50/60 p-4">
      <ErrorFormulario mensaje={error} />

      {flujo.etapaRevision && (
        <p className="text-sm text-slate-800">
          La propuesta está <strong>en revisión</strong>, etapa <strong>{flujo.etapaRevision}</strong>.
          {flujo.decisionesDisponibles.length === 0
            && (permisos.puedeEditar
              ? " Mientras tanto el IP no puede editarla."
              : " Mientras tanto no puede editarse.")}
        </p>
      )}

      {flujo.puedeEnviarRevision && (
        <div className="flex flex-col gap-2">
          <h2 className="text-sm font-semibold text-slate-900">Lista de verificación para enviar a revisión</h2>
          <ul className="flex flex-col gap-1 text-sm">
            {flujo.requisitos.map((r) => (
              <li key={r.descripcion} className={r.cumple ? "text-emerald-800" : "text-amber-800"}>
                {r.cumple ? "✓" : "○"} {r.descripcion}
              </li>
            ))}
          </ul>
          <div>
            <Boton
              disabled={pendientes.length > 0 || enviando}
              title={pendientes.length > 0 ? "Complete los requisitos pendientes" : undefined}
              onClick={() => ejecutar(() => api(`/proyectos/${proyecto.id}/enviar-revision`, { method: "POST" }), "Propuesta enviada a revisión")}
            >
              Enviar a revisión
            </Boton>
          </div>
        </div>
      )}

      {flujo.decisionesDisponibles.length > 0 && (
        <div className="flex flex-col gap-2">
          <h2 className="text-sm font-semibold text-slate-900">Decisión de revisión</h2>
          <Campo id="observaciones-revision" etiqueta={`Observaciones (obligatorias para devolver, mínimo ${MINIMO_OBSERVACIONES} caracteres)`}>
            <textarea id="observaciones-revision" rows={3} className={claseEntrada} value={observaciones}
              onChange={(e) => setObservaciones(e.target.value)} />
          </Campo>
          <div className="flex flex-wrap gap-2">
            {flujo.decisionesDisponibles.map((d) => (
              <Boton
                key={d}
                variante={d === "Devuelve" ? "peligro" : "primario"}
                disabled={enviando || (d === "Devuelve" && observaciones.trim().length < MINIMO_OBSERVACIONES)}
                onClick={() =>
                  ejecutar(
                    () => api(`/proyectos/${proyecto.id}/revisiones`, json("POST", { decision: d, observaciones: observaciones.trim() || null })),
                    d === "Devuelve" ? "Propuesta devuelta al IP" : d === "Avala" ? "Aval otorgado" : "Propuesta aprobada",
                  )
                }
              >
                {textoDecision[d] ?? d}
              </Boton>
            ))}
          </div>
        </div>
      )}

      {flujo.puedeRegistrarAprobacion && (
        <div className="flex flex-col gap-2">
          <h2 className="text-sm font-semibold text-slate-900">Resultado de la postulación</h2>
          <p className="text-xs text-slate-600">
            Para aprobar, adjunte antes en Documentos el &quot;Acto administrativo de aprobación&quot; en PDF.
          </p>
          <Campo id="presupuesto-aprobado" etiqueta={`Presupuesto aprobado (${proyecto.moneda ?? "COP"}), si difiere del solicitado`}>
            <input id="presupuesto-aprobado" type="number" min={0} className={claseEntrada} value={presupuesto}
              onChange={(e) => setPresupuesto(e.target.value)} />
          </Campo>
          <div className="flex flex-wrap gap-2">
            <Boton disabled={enviando} onClick={() => ejecutar(() => api(`/proyectos/${proyecto.id}/aprobacion`, json("POST", {
              aprobado: true, presupuestoAprobado: presupuesto === "" ? null : Number(presupuesto),
            })), "Proyecto aprobado: se creó su cronograma")}>
              Registrar aprobación
            </Boton>
            <Boton variante="peligro" disabled={enviando}
              onClick={() => window.confirm("¿Registrar la postulación como no aprobada? Es un estado final.")
                && ejecutar(() => api(`/proyectos/${proyecto.id}/aprobacion`, json("POST", { aprobado: false })), "Registrado como no aprobado")}>
              No aprobado
            </Boton>
          </div>
        </div>
      )}

      {flujo.puedeIniciarEjecucion && (
        <div className="flex flex-col gap-2">
          <h2 className="text-sm font-semibold text-slate-900">Inicio de la ejecución</h2>
          <p className="text-xs text-slate-600">
            Requiere el &quot;Plan de trabajo&quot; en Documentos. Los retos empresariales necesitan además 3 hitos tipo Sprint y 1 de MVP.
          </p>
          <div className="flex flex-wrap items-end gap-2">
            <Campo id="fecha-inicio-real" etiqueta="Fecha de inicio real">
              <input id="fecha-inicio-real" type="date" className={claseEntrada} value={fechaInicio} onChange={(e) => setFechaInicio(e.target.value)} />
            </Campo>
            <Boton disabled={enviando || !fechaInicio}
              onClick={() => ejecutar(() => api(`/proyectos/${proyecto.id}/iniciar-ejecucion`, json("POST", { fechaInicio })), "Proyecto en ejecución")}>
              Iniciar ejecución
            </Boton>
          </div>
        </div>
      )}

      {(flujo.puedeSuspender || flujo.puedeReanudar) && (
        <div className="flex flex-col gap-2">
          <h2 className="text-sm font-semibold text-slate-900">
            {flujo.puedeSuspender ? "Suspender la ejecución" : "Reanudar la ejecución"}
          </h2>
          <Campo id="justificacion-ejecucion"
            etiqueta={`${flujo.puedeSuspender ? "Motivo de la suspensión" : "Observaciones de la reanudación"} (mínimo ${MINIMO_OBSERVACIONES} caracteres)`}>
            <textarea id="justificacion-ejecucion" rows={2} className={claseEntrada} value={justificacion}
              onChange={(e) => setJustificacion(e.target.value)} />
          </Campo>
          <div>
            <Boton
              variante={flujo.puedeSuspender ? "peligro" : "primario"}
              disabled={enviando || justificacion.trim().length < MINIMO_OBSERVACIONES}
              onClick={() => ejecutar(
                () => api(`/proyectos/${proyecto.id}/${flujo.puedeSuspender ? "suspension" : "reanudacion"}`,
                  json("POST", { observaciones: justificacion.trim() })),
                flujo.puedeSuspender ? "Proyecto suspendido" : "Proyecto reanudado",
              )}
            >
              {flujo.puedeSuspender ? "Suspender proyecto" : "Reanudar ejecución"}
            </Boton>
          </div>
        </div>
      )}

      {flujo.puedeFinalizar && (
        <div className="flex flex-col gap-2 border-t border-emerald-200 pt-3">
          <h2 className="text-sm font-semibold text-slate-900">Finalizar el proyecto</h2>
          <p className="text-xs text-slate-600">
            Requiere todos los hitos del cronograma completados y ningún avance pendiente de validación. Es un estado final.
          </p>
          <Campo id="observaciones-cierre" etiqueta={`Observaciones de cierre (mínimo ${MINIMO_OBSERVACIONES} caracteres)`}>
            <textarea id="observaciones-cierre" rows={2} className={claseEntrada} value={cierre}
              onChange={(e) => setCierre(e.target.value)} />
          </Campo>
          <div>
            <Boton
              disabled={enviando || cierre.trim().length < MINIMO_OBSERVACIONES}
              onClick={() => window.confirm("¿Finalizar el proyecto? Es un estado final y no admite más cambios.")
                && ejecutar(() => api(`/proyectos/${proyecto.id}/finalizacion`, json("POST", { observaciones: cierre.trim() })), "Proyecto finalizado")}
            >
              Finalizar proyecto
            </Boton>
          </div>
        </div>
      )}
    </section>
  );
}
