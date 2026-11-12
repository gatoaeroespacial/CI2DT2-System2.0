"use client";

import { useQuery, useQueryClient } from "@tanstack/react-query";
import Link from "next/link";
import { useState } from "react";
import { Cargando, claseEntrada, Encabezado, EstadoError, EstadoVacio } from "@/components/ui";
import { BarraAvance } from "@/components/controles";
import { api } from "@/lib/api";
import { fechaCortaUtc } from "@/lib/formato";
import { Validar } from "../proyectos/[id]/avances";

type Pendiente = {
  id: number;
  proyectoId: number;
  codigo: string | null;
  proyecto: string;
  programa: string;
  titulo: string;
  autor: string;
  porcentajeAvance: number | null;
  fechaCaptura: string | null;
  estadoValidacion: string | null;
  rolValidador: string;
};

/** HU-20: bandeja "Avances por validar" con contador, filtrable por proyecto y periodo. */
export default function AvancesPorValidar() {
  const queryClient = useQueryClient();
  const [proyectoId, setProyectoId] = useState("");
  const [periodo, setPeriodo] = useState("");
  const parametros = new URLSearchParams();
  if (proyectoId) parametros.set("proyectoId", proyectoId);
  if (periodo) parametros.set("periodo", periodo);
  const bandeja = useQuery({
    queryKey: ["avances-pendientes", proyectoId, periodo],
    queryFn: () => api<{ total: number; items: Pendiente[] }>(`/avances/pendientes?${parametros}`),
  });
  // Opciones de proyecto a partir de la bandeja sin filtrar.
  const todos = useQuery({ queryKey: ["avances-pendientes", "", ""], queryFn: () => api<{ total: number; items: Pendiente[] }>("/avances/pendientes") });
  const proyectos = [...new Map((todos.data?.items ?? []).map((p) => [p.proyectoId, `${p.codigo ?? ""} ${p.proyecto}`])).entries()];

  return (
    <>
      <Encabezado titulo={`Avances por validar${bandeja.data ? ` (${bandeja.data.total})` : ""}`} />
      <div className="mb-4 grid gap-3 md:grid-cols-3">
        <select aria-label="Proyecto" className={claseEntrada} value={proyectoId} onChange={(e) => setProyectoId(e.target.value)}>
          <option value="">Proyecto: todos</option>
          {proyectos.map(([id, nombre]) => <option key={id} value={id}>{nombre}</option>)}
        </select>
        <input aria-label="Periodo" type="month" className={claseEntrada} value={periodo} onChange={(e) => setPeriodo(e.target.value)} />
      </div>
      {bandeja.isPending ? (
        <Cargando />
      ) : bandeja.isError ? (
        <EstadoError mensaje={bandeja.error.message} onReintentar={() => bandeja.refetch()} />
      ) : bandeja.data.items.length === 0 ? (
        <EstadoVacio titulo="No tiene avances pendientes de validar" />
      ) : (
        <ul className="flex flex-col gap-3">
          {bandeja.data.items.map((a) => (
            <li key={a.id} className="rounded-lg border border-slate-200 bg-white p-4">
              <div className="flex flex-wrap items-start justify-between gap-2">
                <div>
                  <p className="font-medium text-slate-900">{a.titulo}</p>
                  <p className="text-xs text-slate-600">
                    <Link href={`/proyectos/${a.proyectoId}`} className="text-emerald-800 hover:underline">{a.codigo} · {a.proyecto}</Link>
                    {` · ${a.programa} · ${a.autor} · ${fechaCortaUtc(a.fechaCaptura)}`}
                    {a.estadoValidacion === "Validado" ? " · Validado por el IP" : ""}
                  </p>
                </div>
                <BarraAvance valor={a.porcentajeAvance} />
              </div>
              <p className="mt-1 text-xs text-slate-500">Validará como: {a.rolValidador}</p>
              <Validar avance={a} onTerminar={() => queryClient.invalidateQueries({ queryKey: ["avances-pendientes"] })} />
            </li>
          ))}
        </ul>
      )}
    </>
  );
}
