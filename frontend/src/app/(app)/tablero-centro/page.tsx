"use client";

import { keepPreviousData, useQuery } from "@tanstack/react-query";
import Link from "next/link";
import { useState } from "react";
import { Cargando, claseEntrada, Encabezado, EstadoError, EstadoVacio, Tabla, type Columna } from "@/components/ui";
import { api } from "@/lib/api";
import { fechaCorta, formatoCop } from "@/lib/formato";

type Hito = { hitoId: number; titulo: string; fechaLimite: string; estado: string; proyectoId: number; codigo: string | null; programa: string; responsable: string | null };

type Tablero = {
  anio: number | null;
  tarjetas: {
    proyectosEnEjecucion: number;
    propuestasEnElaboracion: number;
    propuestasEnRevision: number;
    propuestasDevueltas: number;
    propuestasPresentadas: number;
    tasaAprobacion: number | null;
    postulacionesConResultado: number;
    montoGestionadoCop: number;
  };
  proyectosPorEstadoYPrograma: { programa: string; estado: string; cantidad: number }[];
  hitosProximos: Hito[];
  hitosVencidos: Hito[];
  avancesPendientes: { programa: string; cantidad: number }[];
};

const coloresEstado: Record<string, string> = {
  "En elaboración": "#94a3b8",
  "En revisión": "#f59e0b",
  Devuelto: "#fb923c",
  Presentado: "#0ea5e9",
  Aprobado: "#10b981",
  "En ejecución": "#6366f1",
  Finalizado: "#14b8a6",
  "No aprobado": "#ef4444",
  Suspendido: "#71717a",
};

function Tarjeta({ titulo, valor, detalle }: { titulo: string; valor: string; detalle?: string }) {
  return (
    <div className="rounded-lg border border-slate-200 bg-white p-4">
      <p className="text-xs font-medium uppercase text-slate-500">{titulo}</p>
      <p className="mt-1 text-2xl font-semibold text-slate-900">{valor}</p>
      {detalle && <p className="text-xs text-slate-500">{detalle}</p>}
    </div>
  );
}

/** HU-24: tablero de control del Director con filtro por año. */
export default function TableroCentro() {
  const actual = new Date().getFullYear();
  const [anio, setAnio] = useState<string>("");
  const tablero = useQuery({
    queryKey: ["tablero", anio],
    queryFn: () => api<Tablero>(`/dashboard/centro${anio ? `?anio=${anio}` : ""}`),
    placeholderData: keepPreviousData,
  });

  const columnasHitos: Columna<Hito>[] = [
    { titulo: "Fecha", className: "whitespace-nowrap", celda: (h) => fechaCorta(h.fechaLimite) },
    { titulo: "Hito", celda: (h) => h.titulo },
    { titulo: "Proyecto", celda: (h) => <Link href={`/proyectos/${h.proyectoId}`} className="text-emerald-800 hover:underline">{h.codigo}</Link> },
    { titulo: "Programa", celda: (h) => h.programa },
    { titulo: "Responsable", celda: (h) => h.responsable ?? "—" },
  ];

  return (
    <>
      <Encabezado titulo="Tablero del centro">
        <select aria-label="Año" className={`${claseEntrada} w-40`} value={anio} onChange={(e) => setAnio(e.target.value)}>
          <option value="">Todos los años</option>
          {Array.from({ length: 6 }, (_, i) => actual + 1 - i).map((a) => <option key={a} value={a}>{a}</option>)}
        </select>
      </Encabezado>
      {tablero.isPending ? (
        <Cargando />
      ) : tablero.isError ? (
        <EstadoError mensaje={tablero.error.message} onReintentar={() => tablero.refetch()} />
      ) : (
        <Contenido t={tablero.data} columnasHitos={columnasHitos} />
      )}
    </>
  );
}

function Contenido({ t, columnasHitos }: { t: Tablero; columnasHitos: Columna<Hito>[] }) {
  const k = t.tarjetas;
  const programas = [...new Set(t.proyectosPorEstadoYPrograma.map((x) => x.programa))];
  const estados = [...new Set(t.proyectosPorEstadoYPrograma.map((x) => x.estado))];
  const maximo = Math.max(1, ...programas.map((p) => t.proyectosPorEstadoYPrograma.filter((x) => x.programa === p).reduce((s, x) => s + x.cantidad, 0)));

  return (
    <div className="flex flex-col gap-6">
      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
        <Tarjeta titulo="Proyectos en ejecución" valor={String(k.proyectosEnEjecucion)} />
        <Tarjeta titulo="Propuestas en trámite" valor={String(k.propuestasEnElaboracion + k.propuestasEnRevision + k.propuestasDevueltas)}
          detalle={`${k.propuestasEnElaboracion} en elaboración · ${k.propuestasEnRevision} en revisión · ${k.propuestasDevueltas} devueltas`} />
        <Tarjeta titulo="Propuestas presentadas" valor={String(k.propuestasPresentadas)} detalle="Con fecha de presentación en el periodo" />
        <Tarjeta titulo="Tasa de aprobación" valor={k.tasaAprobacion === null ? "—" : `${k.tasaAprobacion} %`}
          detalle={`Seleccionadas / ${k.postulacionesConResultado} postulaciones con resultado`} />
        <Tarjeta titulo="Monto gestionado" valor={formatoCop(k.montoGestionadoCop)} detalle="Monto solicitado de las postulaciones seleccionadas" />
      </div>

      <section className="rounded-lg border border-slate-200 bg-white p-6">
        <h2 className="mb-4 text-lg font-semibold text-slate-900">Proyectos por estado y programa</h2>
        {programas.length === 0 ? (
          <EstadoVacio titulo="No hay proyectos en el periodo" />
        ) : (
          <>
            <ul className="flex flex-col gap-3" aria-label="Gráfico de barras apiladas">
              {programas.map((p) => {
                const filas = t.proyectosPorEstadoYPrograma.filter((x) => x.programa === p);
                const total = filas.reduce((s, x) => s + x.cantidad, 0);
                return (
                  <li key={p}>
                    <div className="mb-1 flex justify-between text-sm"><span className="text-slate-800">{p}</span><span className="text-slate-600">{total}</span></div>
                    <div className="flex h-5 overflow-hidden rounded bg-slate-100" style={{ width: `${(total / maximo) * 100}%` }}>
                      {filas.map((f) => (
                        <div key={f.estado} title={`${f.estado}: ${f.cantidad}`} style={{ width: `${(f.cantidad / total) * 100}%`, backgroundColor: coloresEstado[f.estado] ?? "#64748b" }} />
                      ))}
                    </div>
                  </li>
                );
              })}
            </ul>
            <ul className="mt-4 flex flex-wrap gap-3 text-xs text-slate-700" aria-label="Convenciones">
              {estados.map((e) => (
                <li key={e} className="flex items-center gap-1">
                  <span className="inline-block h-3 w-3 rounded-sm" style={{ backgroundColor: coloresEstado[e] ?? "#64748b" }} /> {e}
                </li>
              ))}
            </ul>
          </>
        )}
      </section>

      <div className="grid gap-6 lg:grid-cols-2">
        <section>
          <h2 className="mb-2 text-lg font-semibold text-slate-900">Hitos próximos (30 días)</h2>
          {t.hitosProximos.length === 0 ? <EstadoVacio titulo="Sin hitos próximos" /> : <Tabla columnas={columnasHitos} filas={t.hitosProximos} clave={(h) => h.hitoId} />}
        </section>
        <section>
          <h2 className="mb-2 text-lg font-semibold text-slate-900">Hitos vencidos</h2>
          {t.hitosVencidos.length === 0 ? <EstadoVacio titulo="Sin hitos vencidos" /> : <Tabla columnas={columnasHitos} filas={t.hitosVencidos} clave={(h) => h.hitoId} />}
        </section>
      </div>

      <section>
        <h2 className="mb-2 text-lg font-semibold text-slate-900">Avances pendientes de validación por programa</h2>
        {t.avancesPendientes.length === 0 ? (
          <EstadoVacio titulo="Sin avances pendientes" />
        ) : (
          <Tabla columnas={[{ titulo: "Programa", celda: (a: { programa: string }) => a.programa }, { titulo: "Pendientes", celda: (a: { cantidad: number }) => a.cantidad }]}
            filas={t.avancesPendientes} clave={(a) => a.programa} />
        )}
      </section>
    </div>
  );
}
