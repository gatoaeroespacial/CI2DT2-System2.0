"use client";

import { keepPreviousData, useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import Link from "next/link";
import { useState, type FormEvent } from "react";
import { toast } from "sonner";
import { Boton, Cargando, claseEntrada, Encabezado, EstadoError, EstadoVacio, Tabla, type Columna } from "@/components/ui";
import { api } from "@/lib/api";
import type { Pagina } from "@/lib/consultas";
import type { Aliado } from "./formulario-aliado";

/** HU-07: catálogo de aliados con búsqueda por nombre o NIT. */
export default function Aliados() {
  const [busqueda, setBusqueda] = useState("");
  const [filtro, setFiltro] = useState({ busqueda: "", pagina: 1 });
  const queryClient = useQueryClient();

  const consulta = useQuery({
    queryKey: ["aliados", filtro],
    queryFn: () => api<Pagina<Aliado>>(`/aliados?busqueda=${encodeURIComponent(filtro.busqueda)}&pagina=${filtro.pagina}`),
    placeholderData: keepPreviousData,
  });

  const eliminar = useMutation({
    mutationFn: (a: Aliado) => api(`/aliados/${a.id}`, { method: "DELETE" }),
    onSuccess: () => {
      toast.success("Aliado eliminado");
      queryClient.invalidateQueries({ queryKey: ["aliados"] });
    },
    onError: (e) => toast.error(e.message),
  });

  const buscar = (e: FormEvent) => {
    e.preventDefault();
    setFiltro({ busqueda: busqueda.trim(), pagina: 1 });
  };

  const columnas: Columna<Aliado>[] = [
    { titulo: "Nombre", celda: (a) => a.nombre },
    { titulo: "Tipo", celda: (a) => a.tipoAliado },
    { titulo: "NIT", celda: (a) => a.nit ?? "—" },
    { titulo: "País / ciudad", celda: (a) => [a.pais, a.ciudad].filter(Boolean).join(" · ") || "—" },
    { titulo: "Correo", celda: (a) => a.email },
    {
      titulo: "Acciones",
      className: "whitespace-nowrap",
      celda: (a) => (
        <div className="flex gap-2">
          <Link href={`/aliados/${a.id}`} className="px-2 py-1 text-sm text-emerald-800 hover:underline">
            Editar
          </Link>
          <button
            type="button"
            className="px-2 py-1 text-sm text-red-700 hover:underline"
            onClick={() => window.confirm(`¿Eliminar a ${a.nombre}?`) && eliminar.mutate(a)}
          >
            Eliminar
          </button>
        </div>
      ),
    },
  ];

  const datos = consulta.data;
  const totalPaginas = datos ? Math.max(1, Math.ceil(datos.total / datos.tamanoPagina)) : 1;

  return (
    <>
      <Encabezado titulo="Aliados">
        <Link href="/aliados/nuevo" className="rounded-md bg-emerald-800 px-4 py-2 text-sm font-medium text-white hover:bg-emerald-900">
          Nuevo aliado
        </Link>
      </Encabezado>
      <form onSubmit={buscar} role="search" className="mb-4 flex gap-3">
        <input aria-label="Buscar por nombre o NIT" placeholder="Buscar por nombre o NIT" className={claseEntrada}
          value={busqueda} onChange={(e) => setBusqueda(e.target.value)} />
        <Boton type="submit" variante="secundario">
          Buscar
        </Boton>
      </form>
      {consulta.isPending ? (
        <Cargando />
      ) : consulta.isError ? (
        <EstadoError mensaje={consulta.error.message} onReintentar={() => consulta.refetch()} />
      ) : datos!.items.length === 0 ? (
        <EstadoVacio titulo="No hay aliados registrados" descripcion="Registre el primero con el botón Nuevo aliado." />
      ) : (
        <>
          <Tabla columnas={columnas} filas={datos!.items} clave={(a) => a.id} />
          <nav aria-label="Paginación" className="mt-4 flex items-center justify-between text-sm text-slate-600">
            <span>
              {datos!.total} aliado(s) · Página {datos!.paginaActual} de {totalPaginas}
            </span>
            <div className="flex gap-2">
              <Boton variante="secundario" disabled={filtro.pagina <= 1} onClick={() => setFiltro((f) => ({ ...f, pagina: f.pagina - 1 }))}>
                Anterior
              </Boton>
              <Boton variante="secundario" disabled={filtro.pagina >= totalPaginas} onClick={() => setFiltro((f) => ({ ...f, pagina: f.pagina + 1 }))}>
                Siguiente
              </Boton>
            </div>
          </nav>
        </>
      )}
    </>
  );
}
