"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useState, type FormEvent } from "react";
import { toast } from "sonner";
import { ErrorFormulario } from "@/components/controles";
import { Boton, Campo, Cargando, claseEntrada, EstadoError, EstadoVacio } from "@/components/ui";
import { api, json } from "@/lib/api";
import { useAuth } from "@/lib/auth-store";
import { useCatalogos } from "@/lib/consultas";
import { aFechaInput, fechaCorta } from "@/lib/formato";
import type { ProyectoDetalle } from "./tipos";

type Hito = {
  id: number;
  tipoHitoId: number;
  tipoHito: string;
  titulo: string;
  fechaLimite: string;
  alertarDias: number | null;
  responsableId: number | null;
  responsable: string | null;
  estado: string;
  fechaCompletado: string | null;
  avancesVinculados: number;
};

type Integrante = { usuarioId: number; nombre: string; rolEnProyecto: string; vigente: boolean };
type AvanceOpcion = { id: number; titulo: string; porcentajeAvance: number | null };

const coloresEstado: Record<string, string> = {
  Pendiente: "bg-slate-400",
  "En curso": "bg-sky-600",
  Completado: "bg-emerald-600",
  Vencido: "bg-red-600",
};

/** HU-17: hitos del cronograma ordenados por fecha, en lista y línea de tiempo. */
export function Hitos({ proyecto, puedeGestionar }: { proyecto: ProyectoDetalle; puedeGestionar: boolean }) {
  const queryClient = useQueryClient();
  const usuarioId = useAuth((s) => s.usuario?.id);
  const [edicion, setEdicion] = useState<Hito | "nuevo" | null>(null);
  const [completando, setCompletando] = useState<Hito | null>(null);
  const hitos = useQuery({ queryKey: ["hitos", proyecto.id], queryFn: () => api<Hito[]>(`/proyectos/${proyecto.id}/hitos`) });
  const refrescar = async () => {
    await queryClient.invalidateQueries({ queryKey: ["hitos", proyecto.id] });
    await queryClient.invalidateQueries({ queryKey: ["proyecto", String(proyecto.id)] });
  };

  const iniciar = useMutation({
    mutationFn: (h: Hito) => api(`/hitos/${h.id}/iniciar`, { method: "POST" }),
    onSuccess: async () => {
      toast.success("Hito marcado en curso");
      await refrescar();
    },
    onError: (e) => toast.error(e.message),
  });

  if (hitos.isPending) return <Cargando />;
  if (hitos.isError) return <EstadoError mensaje={hitos.error.message} onReintentar={() => hitos.refetch()} />;

  const sinCronograma = proyecto.estadoId < 5 || proyecto.estadoId === 8;

  return (
    <section className="flex flex-col gap-4 rounded-lg border border-slate-200 bg-white p-6">
      {hitos.data.length === 0 ? (
        <EstadoVacio titulo="El proyecto no tiene hitos registrados"
          descripcion={sinCronograma ? "El cronograma se crea cuando el Director registra la aprobación del proyecto." : undefined} />
      ) : (
        <ol className="relative flex flex-col gap-4 border-l-2 border-slate-200 pl-6" aria-label="Línea de tiempo de hitos">
          {hitos.data.map((h) => {
            const esResponsable = h.responsableId === usuarioId;
            const activo = h.estado !== "Completado";
            return (
              <li key={h.id} className="relative">
                <span className={`absolute -left-[31px] top-1.5 h-3 w-3 rounded-full ${coloresEstado[h.estado] ?? "bg-slate-400"}`} aria-hidden />
                <div className="flex flex-wrap items-start justify-between gap-2">
                  <div>
                    <p className="text-sm font-medium text-slate-900">{h.titulo}</p>
                    <p className="text-xs text-slate-600">
                      {fechaCorta(h.fechaLimite)} · {h.tipoHito} · {h.estado}
                      {h.responsable ? ` · Responsable: ${h.responsable}` : ""}
                      {h.fechaCompletado ? ` · Completado el ${fechaCorta(h.fechaCompletado)}` : ""}
                      {h.avancesVinculados > 0 ? ` · ${h.avancesVinculados} avance(s) de evidencia` : ""}
                      {` · Alerta ${h.alertarDias ?? 7} días antes`}
                    </p>
                  </div>
                  {activo && (
                    <div className="flex flex-wrap gap-2 text-sm">
                      {h.estado === "Pendiente" && (esResponsable || puedeGestionar) && (
                        <button type="button" className="text-sky-800 hover:underline" onClick={() => iniciar.mutate(h)}>Marcar en curso</button>
                      )}
                      {(esResponsable || puedeGestionar) && (
                        <button type="button" className="text-emerald-800 hover:underline" onClick={() => setCompletando(h)}>Completar</button>
                      )}
                      {puedeGestionar && (
                        <button type="button" className="text-slate-700 hover:underline" onClick={() => setEdicion(h)}>Editar / reprogramar</button>
                      )}
                    </div>
                  )}
                </div>
                {completando?.id === h.id && (
                  <Completar proyectoId={proyecto.id} hito={h} onTerminar={async () => { setCompletando(null); await refrescar(); }} />
                )}
              </li>
            );
          })}
        </ol>
      )}
      {puedeGestionar && (edicion === null
        ? <div><Boton onClick={() => setEdicion("nuevo")}>Agregar hito</Boton></div>
        : <FormularioHito key={edicion === "nuevo" ? "nuevo" : edicion.id} proyecto={proyecto} hito={edicion === "nuevo" ? null : edicion}
            onTerminar={async () => { setEdicion(null); await refrescar(); }} />)}
    </section>
  );
}

function Completar({ proyectoId, hito, onTerminar }: { proyectoId: number; hito: Hito; onTerminar: () => void }) {
  const [avanceId, setAvanceId] = useState("");
  const avances = useQuery({ queryKey: ["avances", proyectoId], queryFn: () => api<AvanceOpcion[]>(`/proyectos/${proyectoId}/avances`) });
  const completar = useMutation({
    mutationFn: () => api(`/hitos/${hito.id}/completar`, json("POST", { avanceId: avanceId === "" ? null : Number(avanceId) })),
    onSuccess: () => {
      toast.success("Hito completado");
      onTerminar();
    },
    onError: (e) => toast.error(e.message),
  });
  return (
    <div className="mt-2 flex flex-wrap items-end gap-2 rounded-md bg-slate-50 p-3">
      <Campo id={`evidencia-${hito.id}`} etiqueta="Avance de evidencia (opcional)">
        <select id={`evidencia-${hito.id}`} className={claseEntrada} value={avanceId} onChange={(e) => setAvanceId(e.target.value)}>
          <option value="">Sin avance vinculado</option>
          {avances.data?.map((a) => <option key={a.id} value={a.id}>{a.titulo} ({a.porcentajeAvance ?? 0} %)</option>)}
        </select>
      </Campo>
      <Boton disabled={completar.isPending} onClick={() => completar.mutate()}>Confirmar</Boton>
      <Boton variante="secundario" onClick={onTerminar}>Cancelar</Boton>
    </div>
  );
}

function FormularioHito({ proyecto, hito, onTerminar }: { proyecto: ProyectoDetalle; hito: Hito | null; onTerminar: () => void }) {
  const catalogos = useCatalogos();
  const integrantes = useQuery({
    queryKey: ["proyecto-integrantes", String(proyecto.id)],
    queryFn: () => api<Integrante[]>(`/proyectos/${proyecto.id}/integrantes`),
  });
  const [datos, setDatos] = useState({
    tipoHitoId: hito ? String(hito.tipoHitoId) : "",
    titulo: hito?.titulo ?? "",
    fechaLimite: hito ? aFechaInput(hito.fechaLimite) : "",
    responsableId: hito?.responsableId ? String(hito.responsableId) : "",
    alertarDias: String(hito?.alertarDias ?? 7),
    justificacion: "",
  });
  const [error, setError] = useState<string>();
  const [guardando, setGuardando] = useState(false);
  const cambiar = (campo: keyof typeof datos) => (e: { target: { value: string } }) => setDatos((d) => ({ ...d, [campo]: e.target.value }));
  const reprograma = hito !== null && datos.fechaLimite !== aFechaInput(hito.fechaLimite);
  const fueraVigencia = datos.fechaLimite !== "" && (
    (proyecto.fechaInicio && datos.fechaLimite < aFechaInput(proyecto.fechaInicio)) || (proyecto.fechaFin && datos.fechaLimite > aFechaInput(proyecto.fechaFin)));

  const enviar = async (e: FormEvent) => {
    e.preventDefault();
    setError(undefined);
    const dias = Number(datos.alertarDias);
    if (!datos.tipoHitoId || !datos.titulo.trim() || !datos.fechaLimite || !datos.responsableId)
      return setError("Complete el tipo, el título, la fecha límite y el responsable.");
    if (!(dias >= 1 && dias <= 60)) return setError("Los días de anticipación de la alerta deben estar entre 1 y 60.");
    if (reprograma && !datos.justificacion.trim()) return setError("Para cambiar la fecha límite escriba la justificación.");
    setGuardando(true);
    try {
      const cuerpo = {
        tipoHitoId: Number(datos.tipoHitoId),
        titulo: datos.titulo.trim(),
        fechaLimite: datos.fechaLimite,
        responsableId: Number(datos.responsableId),
        alertarDias: dias,
        justificacion: datos.justificacion.trim() || null,
      };
      const respuesta = await api<{ id: number; advertencia: string | null }>(
        hito ? `/hitos/${hito.id}` : `/proyectos/${proyecto.id}/hitos`, json(hito ? "PUT" : "POST", cuerpo));
      toast.success(hito ? "Hito actualizado" : "Hito agregado");
      if (respuesta?.advertencia) toast.warning(respuesta.advertencia);
      onTerminar();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Ocurrió un error inesperado.");
    } finally {
      setGuardando(false);
    }
  };

  return (
    <form onSubmit={enviar} noValidate className="flex flex-col gap-3 border-t border-slate-100 pt-4">
      <h3 className="text-sm font-semibold text-slate-800">{hito ? "Editar o reprogramar hito" : "Nuevo hito"}</h3>
      <ErrorFormulario mensaje={error} />
      <div className="grid gap-3 md:grid-cols-3">
        <Campo id="titulo-hito" etiqueta="Título" obligatorio>
          <input id="titulo-hito" className={claseEntrada} value={datos.titulo} onChange={cambiar("titulo")} />
        </Campo>
        <Campo id="tipoHitoId" etiqueta="Tipo" obligatorio>
          <select id="tipoHitoId" className={claseEntrada} value={datos.tipoHitoId} onChange={cambiar("tipoHitoId")}>
            <option value="">Seleccione…</option>
            {catalogos.data?.tiposHito.map((t) => <option key={t.id} value={t.id}>{t.nombre}</option>)}
          </select>
        </Campo>
        <Campo id="fechaLimite" etiqueta="Fecha límite" obligatorio>
          <input id="fechaLimite" type="date" className={claseEntrada} value={datos.fechaLimite} onChange={cambiar("fechaLimite")} />
        </Campo>
        <Campo id="responsableId" etiqueta="Responsable (integrante vigente)" obligatorio>
          <select id="responsableId" className={claseEntrada} value={datos.responsableId} onChange={cambiar("responsableId")}>
            <option value="">Seleccione…</option>
            {integrantes.data?.filter((i) => i.vigente).map((i) => (
              <option key={i.usuarioId} value={i.usuarioId}>{i.nombre} ({i.rolEnProyecto})</option>
            ))}
          </select>
        </Campo>
        <Campo id="alertarDias" etiqueta="Días de anticipación de la alerta (1 a 60)">
          <input id="alertarDias" type="number" min={1} max={60} className={claseEntrada} value={datos.alertarDias} onChange={cambiar("alertarDias")} />
        </Campo>
        {reprograma && (
          <Campo id="justificacion" etiqueta="Justificación de la reprogramación" obligatorio>
            <input id="justificacion" className={claseEntrada} value={datos.justificacion} onChange={cambiar("justificacion")} />
          </Campo>
        )}
      </div>
      {fueraVigencia && <p role="status" className="text-sm text-amber-800">La fecha límite queda fuera de la vigencia del proyecto.</p>}
      <div className="flex gap-3">
        <Boton type="submit" disabled={guardando}>Guardar hito</Boton>
        <Boton variante="secundario" onClick={onTerminar}>Cancelar</Boton>
      </div>
    </form>
  );
}
