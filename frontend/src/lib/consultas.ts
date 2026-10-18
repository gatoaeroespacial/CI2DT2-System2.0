import { useQuery } from "@tanstack/react-query";
import { api } from "./api";

export type Opcion = { id: number; nombre: string };

export type Catalogos = {
  programas: Opcion[];
  tiposProyecto: Opcion[];
  estadosProyecto: Opcion[];
  tiposDocumento: Opcion[];
  tiposAliado: Opcion[];
};

/** Catálogos de datos semilla (programas, tipos y estados) para formularios y filtros. */
export function useCatalogos() {
  return useQuery({
    queryKey: ["catalogos"],
    queryFn: () => api<Catalogos>("/catalogos"),
    staleTime: 5 * 60_000,
  });
}

export type Pagina<T> = { items: T[]; total: number; paginaActual: number; tamanoPagina: number };
