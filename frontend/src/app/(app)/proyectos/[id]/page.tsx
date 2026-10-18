"use client";

import { useQuery, useQueryClient } from "@tanstack/react-query";
import Link from "next/link";
import { useParams } from "next/navigation";
import { useState, type ReactNode } from "react";
import { toast } from "sonner";
import { BarraAvance, InsigniaEstado, Pestanas } from "@/components/controles";
import { Boton, Cargando, Encabezado, EstadoError, EstadoVacio, Tabla, type Columna } from "@/components/ui";
import { api, json } from "@/lib/api";
import { aPeticionProyecto, type DatosProyecto } from "@/lib/esquemas";
import { aFechaInput, fechaCorta, formatoCop, formatoMonto } from "@/lib/formato";
import { FormularioProyecto } from "../formulario-proyecto";
import { Documentos } from "./documentos";
import { Equipo } from "./equipo";
import { Financiacion } from "./financiacion";
import type { Ficha, ProyectoDetalle } from "./tipos";

type IdPestana = "general" | "equipo" | "financiacion" | "convocatorias" | "cronograma" | "avances" | "revisiones" | "documentos";

const PESTANAS: { id: IdPestana; etiqueta: string }[] = [
  { id: "general", etiqueta: "General" },
  { id: "equipo", etiqueta: "Equipo" },
  { id: "financiacion", etiqueta: "Financiación" },
  { id: "convocatorias", etiqueta: "Convocatorias" },
  { id: "cronograma", etiqueta: "Cronograma" },
  { id: "avances", etiqueta: "Avances" },
  { id: "revisiones", etiqueta: "Revisiones" },
  { id: "documentos", etiqueta: "Documentos" },
];

/** HU-13 CA-3: ficha del proyecto con pestañas; las acciones dependen de los permisos del usuario. */
export default function FichaProyecto() {
  const { id } = useParams<{ id: string }>();
  const [pestana, setPestana] = useState<IdPestana>("general");
  const ficha = useQuery({ queryKey: ["proyecto", id], queryFn: () => api<Ficha>(`/proyectos/${id}`) });

  if (ficha.isPending) return <Cargando />;
  if (ficha.isError) return <EstadoError mensaje={ficha.error.message} onReintentar={() => ficha.refetch()} />;

  const { proyecto, permisos } = ficha.data;

  return (
    <>
      <Encabezado titulo={proyecto.titulo}>
        <Link href="/proyectos" className="text-sm text-emerald-800 hover:underline">
          ← Proyectos
        </Link>
      </Encabezado>
      <div className="mb-4 flex flex-wrap items-center gap-3 text-sm text-slate-700">
        <span className="font-mono">{proyecto.codigo ?? "Sin código"}</span>
        <InsigniaEstado estado={proyecto.estado} />
        <span>{proyecto.programa}</span>
        <span>IP: {proyecto.investigadorPrincipal}</span>
        <BarraAvance valor={proyecto.porcentajeAvance} />
      </div>
      <Pestanas pestanas={PESTANAS} activa={pestana} onCambio={setPestana} />
      {pestana === "general" && <General proyecto={proyecto} puedeEditar={permisos.puedeEditar} />}
      {pestana === "equipo" && <Equipo proyectoId={id} permisos={permisos} />}
      {pestana === "financiacion" && <Financiacion proyectoId={id} puedeEditar={permisos.puedeEditar} />}
      {pestana === "convocatorias" && <Convocatorias proyecto={proyecto} />}
      {pestana === "cronograma" && <Cronograma proyecto={proyecto} />}
      {pestana === "avances" && <Avances proyecto={proyecto} />}
      {pestana === "revisiones" && <Revisiones proyecto={proyecto} />}
      {pestana === "documentos" && <Documentos proyectoId={Number(id)} permisos={permisos} />}
    </>
  );
}

function Dato({ etiqueta, children }: { etiqueta: string; children: ReactNode }) {
  return (
    <div>
      <dt className="text-xs font-medium uppercase text-slate-500">{etiqueta}</dt>
      <dd className="text-sm text-slate-900">{children}</dd>
    </div>
  );
}

const aFormulario = (p: ProyectoDetalle): DatosProyecto => ({
  titulo: p.titulo,
  resumen: p.resumen ?? "",
  programaId: String(p.programaId),
  tipoProyectoId: String(p.tipoProyectoId),
  investigadorPrincipalId: String(p.investigadorPrincipalId),
  fechaInicio: aFechaInput(p.fechaInicio),
  fechaFin: aFechaInput(p.fechaFin),
  moneda: (p.moneda ?? "") as DatosProyecto["moneda"],
  presupuestoTotal: p.presupuestoTotal === null ? "" : String(p.presupuestoTotal),
  tasaCambio: p.tasaCambio === null ? "" : String(p.tasaCambio),
});

function General({ proyecto, puedeEditar }: { proyecto: ProyectoDetalle; puedeEditar: boolean }) {
  const queryClient = useQueryClient();
  const [editando, setEditando] = useState(false);

  if (editando)
    return (
      <FormularioProyecto
        inicial={aFormulario(proyecto)}
        creando={false}
        textoGuardar="Guardar cambios"
        onCancelar={() => setEditando(false)}
        onGuardar={async (datos) => {
          // Programa e investigador principal no se editan aquí (el IP cambia desde la pestaña Equipo).
          const { titulo, resumen, tipoProyectoId, fechaInicio, fechaFin, moneda, presupuestoTotal, tasaCambio } = aPeticionProyecto(datos);
          await api(`/proyectos/${proyecto.id}`,
            json("PUT", { titulo, resumen, tipoProyectoId, fechaInicio, fechaFin, moneda, presupuestoTotal, tasaCambio }));
          await queryClient.invalidateQueries({ queryKey: ["proyecto", String(proyecto.id)] });
          await queryClient.invalidateQueries({ queryKey: ["proyectos"] });
          toast.success("Cambios guardados");
          setEditando(false);
        }}
      />
    );

  return (
    <section className="flex flex-col gap-4 rounded-lg border border-slate-200 bg-white p-6">
      <dl className="grid gap-4 md:grid-cols-3">
        <Dato etiqueta="Tipo de proyecto">{proyecto.tipoProyecto}</Dato>
        <Dato etiqueta="Fecha de inicio">{fechaCorta(proyecto.fechaInicio)}</Dato>
        <Dato etiqueta="Fecha de fin">{fechaCorta(proyecto.fechaFin)}</Dato>
        <Dato etiqueta="Presupuesto total">{formatoMonto(proyecto.presupuestoTotal, proyecto.moneda)}</Dato>
        {proyecto.moneda && proyecto.moneda !== "COP" && <Dato etiqueta="Tasa de cambio">{proyecto.tasaCambio ?? "—"}</Dato>}
        <Dato etiqueta="Presupuesto en pesos">{formatoCop(proyecto.presupuestoTotalCop)}</Dato>
      </dl>
      <Dato etiqueta="Resumen">
        <p className="whitespace-pre-line">{proyecto.resumen || "Sin resumen."}</p>
      </Dato>
      {puedeEditar && (
        <div>
          <Boton variante="secundario" onClick={() => setEditando(true)}>
            Editar propuesta
          </Boton>
        </div>
      )}
    </section>
  );
}

function Listado<T>({ filas, columnas, vacio }: { filas: T[]; columnas: Columna<T>[]; vacio: string }) {
  return filas.length === 0 ? <EstadoVacio titulo={vacio} /> : <Tabla columnas={columnas} filas={filas} clave={(f) => (f as { id: number }).id} />;
}

function Convocatorias({ proyecto }: { proyecto: ProyectoDetalle }) {
  return (
    <Listado
      filas={proyecto.convocatorias}
      vacio="El proyecto no se ha postulado a convocatorias"
      columnas={[
        { titulo: "Convocatoria", celda: (c) => c.convocatoria },
        { titulo: "Presentación", celda: (c) => fechaCorta(c.fechaPresentacion) },
        { titulo: "Monto solicitado", celda: (c) => formatoCop(c.montoSolicitadoCop) },
        { titulo: "Fase externa", celda: (c) => c.faseExterna ?? "—" },
        { titulo: "Resultado", celda: (c) => c.resultado ?? "—" },
      ]}
    />
  );
}

function Cronograma({ proyecto }: { proyecto: ProyectoDetalle }) {
  return (
    <Listado
      filas={proyecto.cronograma}
      vacio="El proyecto no tiene hitos registrados"
      columnas={[
        { titulo: "Hito", celda: (h) => h.titulo },
        { titulo: "Tipo", celda: (h) => h.tipoHito },
        { titulo: "Fecha límite", celda: (h) => fechaCorta(h.fechaLimite) },
        { titulo: "Estado", celda: (h) => h.estado },
        { titulo: "Responsable", celda: (h) => h.responsable ?? "—" },
        { titulo: "Completado", celda: (h) => fechaCorta(h.fechaCompletado) },
      ]}
    />
  );
}

function Avances({ proyecto }: { proyecto: ProyectoDetalle }) {
  return (
    <Listado
      filas={proyecto.avances}
      vacio="El proyecto no tiene avances registrados"
      columnas={[
        { titulo: "Avance", celda: (a) => a.titulo },
        { titulo: "Autor", celda: (a) => a.autor },
        { titulo: "% avance", celda: (a) => <BarraAvance valor={a.porcentajeAvance} /> },
        { titulo: "Captura", celda: (a) => fechaCorta(a.fechaCaptura) },
        { titulo: "Validación", celda: (a) => a.estadoValidacion ?? "—" },
      ]}
    />
  );
}

function Revisiones({ proyecto }: { proyecto: ProyectoDetalle }) {
  return (
    <Listado
      filas={proyecto.revisiones}
      vacio="El proyecto no tiene revisiones registradas"
      columnas={[
        { titulo: "Fecha", celda: (r) => fechaCorta(r.fecha) },
        { titulo: "Etapa", celda: (r) => r.etapa },
        { titulo: "Decisión", celda: (r) => r.decision },
        { titulo: "Revisor", celda: (r) => r.revisor },
        { titulo: "Estado resultante", celda: (r) => r.estadoNuevo },
        { titulo: "Observaciones", celda: (r) => r.observaciones ?? "—" },
      ]}
    />
  );
}
