"use client";

import { useQuery } from "@tanstack/react-query";
import Link from "next/link";
import { Cargando, Encabezado, EstadoError, EstadoVacio, Tabla, type Columna } from "@/components/ui";
import { api } from "@/lib/api";
import { fechaCorta } from "@/lib/formato";

type Alerta = {
  hitoId: number;
  titulo: string;
  fechaLimite: string;
  estado: string;
  proyectoId: number;
  codigo: string | null;
  proyecto: string;
  programa: string | null;
  responsable: string | null;
};

/** HU-18 CA-4: "Mis alertas", hitos próximos (30 días) y vencidos de mis proyectos. */
export default function MisAlertas() {
  const alertas = useQuery({ queryKey: ["mis-alertas"], queryFn: () => api<Alerta[]>("/alertas/pendientes") });

  const columnas: Columna<Alerta>[] = [
    { titulo: "Fecha límite", className: "whitespace-nowrap", celda: (a) => fechaCorta(a.fechaLimite) },
    {
      titulo: "Estado",
      celda: (a) => (
        <span className={`rounded-full px-2 py-0.5 text-xs font-medium ${a.estado === "Vencido" ? "bg-red-100 text-red-900" : "bg-amber-100 text-amber-900"}`}>
          {a.estado === "Vencido" ? "Vencido" : "Próximo"}
        </span>
      ),
    },
    { titulo: "Hito", celda: (a) => a.titulo },
    {
      titulo: "Proyecto",
      celda: (a) => <Link href={`/proyectos/${a.proyectoId}`} className="text-emerald-800 hover:underline">{a.codigo} · {a.proyecto}</Link>,
    },
    { titulo: "Responsable", celda: (a) => a.responsable ?? "—" },
  ];

  return (
    <>
      <Encabezado titulo="Mis alertas" />
      {alertas.isPending ? (
        <Cargando />
      ) : alertas.isError ? (
        <EstadoError mensaje={alertas.error.message} onReintentar={() => alertas.refetch()} />
      ) : alertas.data.length === 0 ? (
        <EstadoVacio titulo="No tiene hitos próximos ni vencidos" />
      ) : (
        <Tabla columnas={columnas} filas={alertas.data} clave={(a) => a.hitoId} />
      )}
    </>
  );
}
