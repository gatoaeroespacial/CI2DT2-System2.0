"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useRef, useState, type FormEvent } from "react";
import { toast } from "sonner";
import { BarraAvance, ErrorFormulario } from "@/components/controles";
import { Boton, Campo, Cargando, claseEntrada, EstadoError, EstadoVacio } from "@/components/ui";
import { api, json } from "@/lib/api";
import { useAuth } from "@/lib/auth-store";
import { FORMATOS_ADJUNTO, MAX_ADJUNTOS, validarAdjunto } from "@/lib/esquemas";
import { fechaCortaUtc } from "@/lib/formato";
import { ROLES, tieneAlgunRol } from "@/lib/roles";
import type { ProyectoDetalle } from "./tipos";

type Avance = {
  id: number;
  hitoId: number | null;
  hito: string | null;
  autorId: number;
  autor: string;
  titulo: string;
  descripcion: string | null;
  porcentajeAvance: number | null;
  fechaCaptura: string | null;
  estadoValidacion: string | null;
  adjuntos: { id: number; nombre: string; tipoMime: string | null }[];
  validaciones: { id: number; validador: string; rolValidador: string; resultado: string | null; observaciones: string | null; fecha: string }[];
};

type HitoOpcion = { id: number; titulo: string; estado: string };

const coloresValidacion: Record<string, string> = {
  Pendiente: "bg-amber-100 text-amber-900",
  Validado: "bg-emerald-100 text-emerald-900",
  Rechazado: "bg-red-100 text-red-900",
};

export async function descargarAdjunto(id: number) {
  const { url } = await api<{ url: string }>(`/avances/adjuntos/${id}/descarga`);
  window.location.assign(url);
}

/** HU-19 y HU-20: avances del proyecto con evidencias, corrección por el autor y validación. */
export function Avances({ proyecto, puedeRegistrar }: { proyecto: ProyectoDetalle; puedeRegistrar: boolean }) {
  const queryClient = useQueryClient();
  const usuario = useAuth((s) => s.usuario);
  const [edicion, setEdicion] = useState<Avance | "nuevo" | null>(null);
  const avances = useQuery({ queryKey: ["avances", proyecto.id], queryFn: () => api<Avance[]>(`/proyectos/${proyecto.id}/avances`) });
  const refrescar = async () => {
    await queryClient.invalidateQueries({ queryKey: ["avances", proyecto.id] });
    await queryClient.invalidateQueries({ queryKey: ["proyecto", String(proyecto.id)] });
  };
  // El backend decide si el usuario valida (IP, Coordinador del programa o Director).
  const puedeValidarRol = tieneAlgunRol(usuario?.roles ?? [], [ROLES.Director, ROLES.Coordinador])
    || proyecto.investigadorPrincipalId === usuario?.id;

  if (avances.isPending) return <Cargando />;
  if (avances.isError) return <EstadoError mensaje={avances.error.message} onReintentar={() => avances.refetch()} />;

  return (
    <section className="flex flex-col gap-4 rounded-lg border border-slate-200 bg-white p-6">
      {proyecto.estadoId !== 6 && (
        <p className="text-sm text-slate-600">Los avances se registran cuando el proyecto está &quot;En ejecución&quot;.</p>
      )}
      {avances.data.length === 0 ? (
        <EstadoVacio titulo="El proyecto no tiene avances registrados" />
      ) : (
        <ul className="flex flex-col gap-3">
          {avances.data.map((a) => {
            const esAutor = a.autorId === usuario?.id;
            const editable = esAutor && proyecto.estadoId === 6 && (a.estadoValidacion === "Pendiente" || a.estadoValidacion === "Rechazado");
            return (
              <li key={a.id} className="rounded-md border border-slate-200 p-4">
                <div className="flex flex-wrap items-start justify-between gap-2">
                  <div>
                    <p className="font-medium text-slate-900">{a.titulo}</p>
                    <p className="text-xs text-slate-600">
                      {a.autor} · {fechaCortaUtc(a.fechaCaptura)}{a.hito ? ` · Hito: ${a.hito}` : ""}
                    </p>
                  </div>
                  <div className="flex items-center gap-3">
                    <BarraAvance valor={a.porcentajeAvance} />
                    <span className={`rounded-full px-2 py-0.5 text-xs font-medium ${coloresValidacion[a.estadoValidacion ?? "Pendiente"] ?? ""}`}>
                      {a.estadoValidacion ?? "Pendiente"}
                    </span>
                  </div>
                </div>
                {a.descripcion && <p className="mt-2 whitespace-pre-line text-sm text-slate-800">{a.descripcion}</p>}
                {a.adjuntos.length > 0 && (
                  <ul className="mt-2 flex flex-wrap gap-3 text-sm">
                    {a.adjuntos.map((ad) => (
                      <li key={ad.id}>
                        <button type="button" className="text-emerald-800 hover:underline"
                          onClick={() => descargarAdjunto(ad.id).catch((e: Error) => toast.error(e.message))}>
                          {ad.nombre}
                        </button>
                      </li>
                    ))}
                  </ul>
                )}
                {a.validaciones.length > 0 && (
                  <ol className="mt-2 flex flex-col gap-1 border-l-2 border-slate-200 pl-3 text-xs text-slate-700">
                    {a.validaciones.map((v) => (
                      <li key={v.id}>
                        {fechaCortaUtc(v.fecha)} · {v.resultado} por {v.validador} ({v.rolValidador}){v.observaciones ? `: ${v.observaciones}` : ""}
                      </li>
                    ))}
                  </ol>
                )}
                <div className="mt-2 flex flex-wrap gap-3 text-sm">
                  {editable && (
                    <button type="button" className="text-emerald-800 hover:underline" onClick={() => setEdicion(a)}>
                      {a.estadoValidacion === "Rechazado" ? "Corregir" : "Editar"}
                    </button>
                  )}
                </div>
                {puedeValidarRol && a.estadoValidacion !== "Rechazado" && (
                  <Validar avance={a} onTerminar={refrescar} />
                )}
                {edicion !== "nuevo" && edicion?.id === a.id && (
                  <FormularioAvance proyectoId={proyecto.id} avance={a} onTerminar={async () => { setEdicion(null); await refrescar(); }} />
                )}
              </li>
            );
          })}
        </ul>
      )}
      {puedeRegistrar && (edicion === "nuevo"
        ? <FormularioAvance proyectoId={proyecto.id} avance={null} onTerminar={async () => { setEdicion(null); await refrescar(); }} />
        : <div><Boton onClick={() => setEdicion("nuevo")}>Registrar avance</Boton></div>)}
    </section>
  );
}

/** Validar o rechazar con observaciones (HU-20). */
export function Validar({ avance, onTerminar }: { avance: { id: number; estadoValidacion: string | null }; onTerminar: () => void }) {
  const [abierto, setAbierto] = useState(false);
  const [observaciones, setObservaciones] = useState("");
  const validar = useMutation({
    mutationFn: (resultado: string) => api(`/avances/${avance.id}/validaciones`, json("POST", { resultado, observaciones: observaciones.trim() || null })),
    onSuccess: (_, resultado) => {
      toast.success(resultado === "Validado" ? "Avance validado" : "Avance rechazado");
      setAbierto(false);
      setObservaciones("");
      onTerminar();
    },
    onError: (e) => toast.error(e.message),
  });
  if (!abierto)
    return (
      <button type="button" className="mt-2 text-sm text-sky-800 hover:underline" onClick={() => setAbierto(true)}>
        Validar o rechazar
      </button>
    );
  return (
    <div className="mt-2 flex flex-col gap-2 rounded-md bg-slate-50 p-3">
      <Campo id={`obs-${avance.id}`} etiqueta="Observaciones (obligatorias para rechazar)">
        <textarea id={`obs-${avance.id}`} rows={2} className={claseEntrada} value={observaciones} onChange={(e) => setObservaciones(e.target.value)} />
      </Campo>
      <div className="flex flex-wrap gap-2">
        <Boton disabled={validar.isPending} onClick={() => validar.mutate("Validado")}>Validar</Boton>
        <Boton variante="peligro" disabled={validar.isPending || observaciones.trim() === ""} onClick={() => validar.mutate("Rechazado")}>
          Rechazar
        </Boton>
        <Boton variante="secundario" onClick={() => setAbierto(false)}>Cancelar</Boton>
      </div>
    </div>
  );
}

function FormularioAvance({ proyectoId, avance, onTerminar }: { proyectoId: number; avance: Avance | null; onTerminar: () => void }) {
  const entrada = useRef<HTMLInputElement>(null);
  // CA-3: el IdCliente se genera una vez por formulario y evita duplicados por doble envío.
  const [idCliente] = useState(() => crypto.randomUUID());
  const [datos, setDatos] = useState({
    titulo: avance?.titulo ?? "",
    descripcion: avance?.descripcion ?? "",
    porcentajeAvance: avance?.porcentajeAvance === null || avance === null ? "" : String(avance.porcentajeAvance),
    hitoId: avance?.hitoId ? String(avance.hitoId) : "",
  });
  const [error, setError] = useState<string>();
  const [guardando, setGuardando] = useState(false);
  const hitos = useQuery({ queryKey: ["hitos", proyectoId], queryFn: () => api<HitoOpcion[]>(`/proyectos/${proyectoId}/hitos`) });
  const cambiar = (campo: keyof typeof datos) => (e: { target: { value: string } }) => setDatos((d) => ({ ...d, [campo]: e.target.value }));

  const enviar = async (e: FormEvent) => {
    e.preventDefault();
    setError(undefined);
    const porcentaje = Number(datos.porcentajeAvance);
    if (!datos.titulo.trim()) return setError("El título es obligatorio.");
    if (datos.porcentajeAvance === "" || !(porcentaje >= 0 && porcentaje <= 100)) return setError("El porcentaje acumulado debe estar entre 0 y 100.");

    // CA-4: los adjuntos se validan antes de subirlos.
    const archivos = Array.from(entrada.current?.files ?? []);
    if (archivos.length + (avance?.adjuntos.length ?? 0) > MAX_ADJUNTOS) return setError(`Un avance admite hasta ${MAX_ADJUNTOS} adjuntos.`);
    for (const a of archivos) {
      const invalido = validarAdjunto(a.name, a.size);
      if (invalido) return setError(`${a.name}: ${invalido}`);
    }

    setGuardando(true);
    try {
      const cuerpo = {
        titulo: datos.titulo.trim(),
        descripcion: datos.descripcion.trim() || null,
        porcentajeAvance: porcentaje,
        hitoId: datos.hitoId === "" ? null : Number(datos.hitoId),
        idCliente: avance ? null : idCliente,
      };
      let id = avance?.id;
      if (avance) await api(`/avances/${avance.id}`, json("PUT", cuerpo));
      else id = (await api<{ id: number }>(`/proyectos/${proyectoId}/avances`, json("POST", cuerpo))).id;
      for (const archivo of archivos) {
        const formulario = new FormData();
        formulario.append("archivo", archivo);
        await api(`/avances/${id}/adjuntos`, { method: "POST", body: formulario });
      }
      toast.success(avance ? "Avance corregido: queda pendiente de validación" : "Avance registrado: queda pendiente de validación");
      onTerminar();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Ocurrió un error inesperado.");
    } finally {
      setGuardando(false);
    }
  };

  return (
    <form onSubmit={enviar} noValidate className="mt-3 flex flex-col gap-3 border-t border-slate-100 pt-4">
      <h3 className="text-sm font-semibold text-slate-800">{avance ? "Corregir avance" : "Nuevo avance"}</h3>
      <ErrorFormulario mensaje={error} />
      <div className="grid gap-3 md:grid-cols-3">
        <Campo id="titulo-avance" etiqueta="Título" obligatorio>
          <input id="titulo-avance" className={claseEntrada} value={datos.titulo} onChange={cambiar("titulo")} />
        </Campo>
        <Campo id="porcentaje-avance" etiqueta="Porcentaje acumulado (0 a 100)" obligatorio>
          <input id="porcentaje-avance" type="number" min={0} max={100} className={claseEntrada} value={datos.porcentajeAvance} onChange={cambiar("porcentajeAvance")} />
        </Campo>
        <Campo id="hito-avance" etiqueta="Hito relacionado (opcional)">
          <select id="hito-avance" className={claseEntrada} value={datos.hitoId} onChange={cambiar("hitoId")}>
            <option value="">Ninguno</option>
            {hitos.data?.map((h) => <option key={h.id} value={h.id}>{h.titulo} ({h.estado})</option>)}
          </select>
        </Campo>
      </div>
      <Campo id="descripcion-avance" etiqueta="Descripción">
        <textarea id="descripcion-avance" rows={3} className={claseEntrada} value={datos.descripcion} onChange={cambiar("descripcion")} />
      </Campo>
      <Campo id="adjuntos-avance" etiqueta={`Evidencias (hasta ${MAX_ADJUNTOS}, máx. 20 MB c/u: PDF, DOCX, XLSX, CSV, JPG o PNG)`}>
        <input id="adjuntos-avance" ref={entrada} type="file" multiple accept={FORMATOS_ADJUNTO.join(",")} className="text-sm" />
      </Campo>
      <div className="flex gap-3">
        <Boton type="submit" disabled={guardando}>{guardando ? "Guardando…" : "Guardar avance"}</Boton>
        <Boton variante="secundario" onClick={onTerminar}>Cancelar</Boton>
      </div>
    </form>
  );
}
