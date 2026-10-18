import { format } from "date-fns";
import { es } from "date-fns/locale";

const pesos = new Intl.NumberFormat("es-CO", { maximumFractionDigits: 0 });
const numero = new Intl.NumberFormat("es-CO", { maximumFractionDigits: 2 });

/** "$40.000.000 COP" (HU-08 CA-2). */
export function formatoCop(valor: number | null | undefined): string {
  return valor === null || valor === undefined ? "—" : `$${pesos.format(valor)} COP`;
}

export function formatoMonto(valor: number | null | undefined, moneda: string | null | undefined): string {
  if (valor === null || valor === undefined) return "—";
  return moneda === "COP" ? formatoCop(valor) : `${numero.format(valor)} ${moneda ?? ""}`.trim();
}

/** Fecha corta en español, p. ej. "07/10/2026". Acepta "2026-10-07" o ISO completo. */
export function fechaCorta(valor: string | null | undefined): string {
  if (!valor) return "—";
  const fecha = new Date(valor.length === 10 ? `${valor}T00:00:00` : valor);
  return format(fecha, "dd/MM/yyyy", { locale: es });
}

/** Valor para un input type="date" a partir de una fecha ISO de la API. */
export const aFechaInput = (valor: string | null | undefined) => (valor ? valor.slice(0, 10) : "");

export const hoyIso = () => format(new Date(), "yyyy-MM-dd");
