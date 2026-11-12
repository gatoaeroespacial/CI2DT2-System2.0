"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useState, type FormEvent } from "react";
import { toast } from "sonner";
import { ErrorFormulario } from "@/components/controles";
import { Boton, Campo, Cargando, claseEntrada, EstadoError, EstadoVacio, Tabla, type Columna } from "@/components/ui";
import { api, json } from "@/lib/api";
import type { Pagina } from "@/lib/consultas";
import { enCop, MONEDAS, validarArchivo } from "@/lib/esquemas";
import { aFechaInput, fechaCorta, formatoCop, formatoMonto } from "@/lib/formato";

type Postulacion = {
  id: number;
  convocatoriaId: number;
  convocatoria: string;
  tipoConvocatoriaId: number;
  tipoConvocatoria: string;
  fechaPresentacion: string | null;
  idPostulacionExterna: string | null;
  codigoMga: string | null;
  montoSolicitado: number | null;
  moneda: string | null;
  tasaCambioEstimada: number | null;
  montoSolicitadoCop: number | null;
  resultado: string | null;
  puntaje: number | null;
  faseExterna: string | null;
  fechaResultado: string | null;
  comprobanteDocId: number | null;
};

type ConvocatoriaOpcion = { id: number; nombre: string; numero: string | null; tipoConvocatoriaId: number; estado: string; moneda: string | null; montoMaximo: number | null };

const TIPO_SGR = 5;
const TIPO_INTERNA = 1;
const COMPROBANTE_RADICACION = 11;
const RESULTADOS = ["Pendiente", "Seleccionada", "No seleccionada"];
const FASES_SGR = ["Banco de elegibles", "Viabilización OCAD", "Designación de ejecutor"];

type Datos = {
  convocatoriaId: string;
  fechaPresentacion: string;
  idPostulacionExterna: string;
  codigoMga: string;
  montoSolicitado: string;
  moneda: string;
  tasaCambioEstimada: string;
  resultado: string;
  puntaje: string;
  faseExterna: string;
  fechaResultado: string;
};

const vacio: Datos = {
  convocatoriaId: "", fechaPresentacion: "", idPostulacionExterna: "", codigoMga: "", montoSolicitado: "", moneda: "COP",
  tasaCambioEstimada: "", resultado: "Pendiente", puntaje: "", faseExterna: "", fechaResultado: "",
};

const aDatos = (p: Postulacion): Datos => ({
  convocatoriaId: String(p.convocatoriaId),
  fechaPresentacion: aFechaInput(p.fechaPresentacion),
  idPostulacionExterna: p.idPostulacionExterna ?? "",
  codigoMga: p.codigoMga ?? "",
  montoSolicitado: p.montoSolicitado === null ? "" : String(p.montoSolicitado),
  moneda: p.moneda ?? "COP",
  tasaCambioEstimada: p.tasaCambioEstimada === null ? "" : String(p.tasaCambioEstimada),
  resultado: p.resultado ?? "Pendiente",
  puntaje: p.puntaje === null ? "" : String(p.puntaje),
  faseExterna: p.faseExterna ?? "",
  fechaResultado: aFechaInput(p.fechaResultado),
});

const nuloSiVacio = (v: string) => (v.trim() === "" ? null : v.trim());
const numeroSiHay = (v: string) => (v.trim() === "" ? null : Number(v));

/** HU-16: vincular la propuesta a convocatorias y registrar la radicación externa y el resultado. */
export function Postulaciones({ proyectoId, puedeGestionar }: { proyectoId: number; puedeGestionar: boolean }) {
  const queryClient = useQueryClient();
  const [edicion, setEdicion] = useState<Postulacion | "nueva" | null>(null);
  const postulaciones = useQuery({
    queryKey: ["postulaciones", proyectoId],
    queryFn: () => api<Postulacion[]>(`/proyectos/${proyectoId}/postulaciones`),
  });
  const refrescar = async () => {
    await queryClient.invalidateQueries({ queryKey: ["postulaciones", proyectoId] });
    await queryClient.invalidateQueries({ queryKey: ["proyecto", String(proyectoId)] });
  };

  const eliminar = useMutation({
    mutationFn: (p: Postulacion) => api(`/proyectos/${proyectoId}/postulaciones/${p.id}`, { method: "DELETE" }),
    onSuccess: async () => {
      toast.success("Postulación eliminada");
      await refrescar();
    },
    onError: (e) => toast.error(e.message),
  });

  const subirComprobante = useMutation({
    mutationFn: ({ postulacion, archivo }: { postulacion: Postulacion; archivo: File }) => {
      const datos = new FormData();
      datos.append("archivo", archivo);
      datos.append("tipoDocumentoId", String(COMPROBANTE_RADICACION));
      datos.append("entidadTipo", "PropuestaConvocatoria");
      datos.append("entidadId", String(postulacion.id));
      return api("/documentos", { method: "POST", body: datos });
    },
    onSuccess: () => toast.success("Comprobante adjunto. Ahora puede registrar la fecha de presentación."),
    onError: (e) => toast.error(e.message),
  });

  if (postulaciones.isPending) return <Cargando />;
  if (postulaciones.isError) return <EstadoError mensaje={postulaciones.error.message} onReintentar={() => postulaciones.refetch()} />;

  const columnas: Columna<Postulacion>[] = [
    { titulo: "Convocatoria", celda: (p) => `${p.convocatoria} (${p.tipoConvocatoria})` },
    { titulo: "Presentación", celda: (p) => fechaCorta(p.fechaPresentacion) },
    { titulo: "Id externo / MGA", celda: (p) => [p.idPostulacionExterna, p.codigoMga].filter(Boolean).join(" · ") || "—" },
    { titulo: "Monto solicitado", celda: (p) => `${formatoMonto(p.montoSolicitado, p.moneda)}${p.moneda && p.moneda !== "COP" ? ` (${formatoCop(p.montoSolicitadoCop)})` : ""}` },
    { titulo: "Fase externa", celda: (p) => p.faseExterna ?? "—" },
    { titulo: "Resultado", celda: (p) => p.resultado ?? "Pendiente" },
    { titulo: "Comprobante", celda: (p) => (p.comprobanteDocId ? "Adjunto" : "—") },
    ...(puedeGestionar
      ? [{
          titulo: "Acciones",
          className: "whitespace-nowrap",
          celda: (p: Postulacion) => (
            <div className="flex flex-wrap items-center gap-2">
              <button type="button" className="text-sm text-emerald-800 hover:underline" onClick={() => setEdicion(p)}>Editar</button>
              {p.tipoConvocatoriaId !== TIPO_INTERNA && (
                <label className="cursor-pointer text-sm text-emerald-800 hover:underline">
                  Adjuntar comprobante
                  <input type="file" accept=".pdf,.jpg,.jpeg,.png" className="sr-only" onChange={(e) => {
                    const archivo = e.target.files?.[0];
                    e.target.value = "";
                    if (!archivo) return;
                    const invalido = validarArchivo(archivo.name, archivo.size);
                    if (invalido) toast.error(invalido);
                    else subirComprobante.mutate({ postulacion: p, archivo });
                  }} />
                </label>
              )}
              <button type="button" className="text-sm text-red-700 hover:underline"
                onClick={() => window.confirm("¿Eliminar esta postulación?") && eliminar.mutate(p)}>Eliminar</button>
            </div>
          ),
        }]
      : []),
  ];

  return (
    <section className="flex flex-col gap-4 rounded-lg border border-slate-200 bg-white p-6">
      {postulaciones.data.length === 0
        ? <EstadoVacio titulo="El proyecto no se ha postulado a convocatorias" />
        : <Tabla columnas={columnas} filas={postulaciones.data} clave={(p) => p.id} />}
      {puedeGestionar && (edicion === null
        ? <div><Boton onClick={() => setEdicion("nueva")}>Vincular a una convocatoria</Boton></div>
        : <FormularioPostulacion key={edicion === "nueva" ? "nueva" : edicion.id} proyectoId={proyectoId}
            postulacion={edicion === "nueva" ? null : edicion} onTerminar={async () => { setEdicion(null); await refrescar(); }} />)}
    </section>
  );
}

function FormularioPostulacion({ proyectoId, postulacion, onTerminar }: { proyectoId: number; postulacion: Postulacion | null; onTerminar: () => void }) {
  const [datos, setDatos] = useState<Datos>(postulacion ? aDatos(postulacion) : vacio);
  const [busqueda, setBusqueda] = useState("");
  const [error, setError] = useState<string>();
  const [guardando, setGuardando] = useState(false);
  const convocatorias = useQuery({
    queryKey: ["convocatorias-opciones", busqueda],
    queryFn: () => api<Pagina<ConvocatoriaOpcion>>(`/convocatorias?busqueda=${encodeURIComponent(busqueda)}`),
  });
  const opciones = convocatorias.data?.items ?? [];
  const seleccionada = opciones.find((c) => String(c.id) === datos.convocatoriaId);
  const esSgr = (seleccionada?.tipoConvocatoriaId ?? postulacion?.tipoConvocatoriaId) === TIPO_SGR;
  const cambiar = (campo: keyof Datos) => (e: { target: { value: string } }) => setDatos((d) => ({ ...d, [campo]: e.target.value }));

  const enviar = async (e: FormEvent) => {
    e.preventDefault();
    setError(undefined);
    if (!datos.convocatoriaId) return setError("Seleccione la convocatoria.");
    if (datos.moneda !== "COP" && datos.montoSolicitado && !(Number(datos.tasaCambioEstimada) > 0))
      return setError("La tasa de cambio estimada es obligatoria cuando la moneda no es COP.");
    if (esSgr && !datos.codigoMga.trim()) return setError("Las convocatorias SGR exigen el código MGA.");
    setGuardando(true);
    try {
      const cuerpo = {
        convocatoriaId: Number(datos.convocatoriaId),
        fechaPresentacion: nuloSiVacio(datos.fechaPresentacion),
        idPostulacionExterna: nuloSiVacio(datos.idPostulacionExterna),
        codigoMga: nuloSiVacio(datos.codigoMga),
        montoSolicitado: numeroSiHay(datos.montoSolicitado),
        moneda: datos.montoSolicitado ? datos.moneda : null,
        tasaCambioEstimada: datos.moneda === "COP" ? null : numeroSiHay(datos.tasaCambioEstimada),
        resultado: datos.resultado,
        puntaje: numeroSiHay(datos.puntaje),
        faseExterna: esSgr ? nuloSiVacio(datos.faseExterna) : null,
        fechaResultado: nuloSiVacio(datos.fechaResultado),
      };
      await api(postulacion ? `/proyectos/${proyectoId}/postulaciones/${postulacion.id}` : `/proyectos/${proyectoId}/postulaciones`,
        json(postulacion ? "PUT" : "POST", cuerpo));
      toast.success(postulacion ? "Postulación actualizada" : "Propuesta vinculada a la convocatoria");
      onTerminar();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Ocurrió un error inesperado.");
    } finally {
      setGuardando(false);
    }
  };

  return (
    <form onSubmit={enviar} noValidate className="flex flex-col gap-3 border-t border-slate-100 pt-4">
      <h3 className="text-sm font-semibold text-slate-800">{postulacion ? "Editar postulación" : "Nueva postulación"}</h3>
      <ErrorFormulario mensaje={error} />
      <div className="grid gap-3 md:grid-cols-3">
        <Campo id="buscar-convocatoria" etiqueta="Buscar convocatoria">
          <input id="buscar-convocatoria" className={claseEntrada} placeholder="Nombre o número" value={busqueda} onChange={(e) => setBusqueda(e.target.value)} />
        </Campo>
        <Campo id="convocatoriaId" etiqueta="Convocatoria" obligatorio>
          <select id="convocatoriaId" className={claseEntrada} value={datos.convocatoriaId} onChange={cambiar("convocatoriaId")}>
            <option value="">Seleccione…</option>
            {postulacion && !opciones.some((c) => c.id === postulacion.convocatoriaId) && (
              <option value={postulacion.convocatoriaId}>{postulacion.convocatoria}</option>
            )}
            {opciones.map((c) => (
              <option key={c.id} value={c.id}>{c.nombre} · {c.estado}</option>
            ))}
          </select>
        </Campo>
        <Campo id="resultado" etiqueta="Resultado">
          <select id="resultado" className={claseEntrada} value={datos.resultado} onChange={cambiar("resultado")}>
            {RESULTADOS.map((r) => <option key={r} value={r}>{r}</option>)}
          </select>
        </Campo>
        <Campo id="montoSolicitado" etiqueta={seleccionada?.montoMaximo ? `Monto solicitado (máx. ${formatoMonto(seleccionada.montoMaximo, seleccionada.moneda)})` : "Monto solicitado"}>
          <input id="montoSolicitado" type="number" min={0} className={claseEntrada} value={datos.montoSolicitado} onChange={cambiar("montoSolicitado")} />
        </Campo>
        <Campo id="moneda-postulacion" etiqueta="Moneda">
          <select id="moneda-postulacion" className={claseEntrada} value={datos.moneda} onChange={cambiar("moneda")}>
            {MONEDAS.map((m) => <option key={m} value={m}>{m}</option>)}
          </select>
        </Campo>
        {datos.moneda !== "COP" ? (
          <Campo id="tasaCambioEstimada" etiqueta={`Tasa estimada (COP por 1 ${datos.moneda})`} obligatorio>
            <input id="tasaCambioEstimada" type="number" min={0} className={claseEntrada} value={datos.tasaCambioEstimada} onChange={cambiar("tasaCambioEstimada")} />
          </Campo>
        ) : <div />}
        <Campo id="fechaPresentacion" etiqueta="Fecha de presentación (radicada)">
          <input id="fechaPresentacion" type="date" className={claseEntrada} value={datos.fechaPresentacion} onChange={cambiar("fechaPresentacion")} />
        </Campo>
        <Campo id="idPostulacionExterna" etiqueta="Id de postulación externa">
          <input id="idPostulacionExterna" className={claseEntrada} value={datos.idPostulacionExterna} onChange={cambiar("idPostulacionExterna")} />
        </Campo>
        {esSgr && (
          <>
            <Campo id="codigoMga" etiqueta="Código MGA" obligatorio>
              <input id="codigoMga" className={claseEntrada} value={datos.codigoMga} onChange={cambiar("codigoMga")} />
            </Campo>
            <Campo id="faseExterna" etiqueta="Fase externa (SGR)">
              <select id="faseExterna" className={claseEntrada} value={datos.faseExterna} onChange={cambiar("faseExterna")}>
                <option value="">Sin fase</option>
                {FASES_SGR.map((f) => <option key={f} value={f}>{f}</option>)}
              </select>
            </Campo>
          </>
        )}
        <Campo id="puntaje" etiqueta="Puntaje">
          <input id="puntaje" type="number" min={0} className={claseEntrada} value={datos.puntaje} onChange={cambiar("puntaje")} />
        </Campo>
        <Campo id="fechaResultado" etiqueta="Fecha del resultado">
          <input id="fechaResultado" type="date" className={claseEntrada} value={datos.fechaResultado} onChange={cambiar("fechaResultado")} />
        </Campo>
      </div>
      {datos.moneda !== "COP" && datos.montoSolicitado && (
        <p className="text-sm text-slate-700">Equivalente: {formatoCop(enCop(Number(datos.montoSolicitado), datos.moneda, numeroSiHay(datos.tasaCambioEstimada)))}</p>
      )}
      <p className="text-xs text-slate-600">
        En convocatorias externas, para registrar la fecha de presentación indique el id externo y adjunte antes el comprobante de radicación.
      </p>
      <div className="flex gap-3">
        <Boton type="submit" disabled={guardando}>Guardar postulación</Boton>
        <Boton variante="secundario" onClick={onTerminar}>Cancelar</Boton>
      </div>
    </form>
  );
}
