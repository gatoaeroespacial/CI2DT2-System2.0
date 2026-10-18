"use client";

import { useQuery } from "@tanstack/react-query";
import Link from "next/link";
import { Cargando, Encabezado, EstadoError, EstadoVacio, Tabla, type Columna } from "@/components/ui";
import { api } from "@/lib/api";

type ProgramaResumen = {
  id: number;
  nombre: string;
  descripcion: string | null;
  coordinador: string | null;
  integrantesVigentes: number;
};

/** HU-06: programas del centro con su Coordinador vigente. */
export default function Programas() {
  const programas = useQuery({ queryKey: ["programas"], queryFn: () => api<ProgramaResumen[]>("/programas") });

  const columnas: Columna<ProgramaResumen>[] = [
    {
      titulo: "Programa",
      celda: (p) => (
        <Link href={`/programas/${p.id}`} className="font-medium text-emerald-800 hover:underline">
          {p.nombre}
        </Link>
      ),
    },
    { titulo: "Coordinador vigente", celda: (p) => p.coordinador ?? "Sin coordinador" },
    { titulo: "Integrantes vigentes", celda: (p) => p.integrantesVigentes },
  ];

  return (
    <>
      <Encabezado titulo="Programas" />
      {programas.isPending ? (
        <Cargando />
      ) : programas.isError ? (
        <EstadoError mensaje={programas.error.message} onReintentar={() => programas.refetch()} />
      ) : programas.data.length === 0 ? (
        <EstadoVacio titulo="No hay programas registrados" />
      ) : (
        <Tabla columnas={columnas} filas={programas.data} clave={(p) => p.id} />
      )}
    </>
  );
}
