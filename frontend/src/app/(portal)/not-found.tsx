import type { Metadata } from "next";
import { NoEncontrado } from "@/components/no-encontrado";

export const metadata: Metadata = { title: "Página no encontrada · CI²DT²" };

/** 404 dentro del portal (con su encabezado y pie), p. ej. un proyecto que no es público. */
export default function NotFound() {
  return <NoEncontrado />;
}
