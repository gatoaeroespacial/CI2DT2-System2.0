import { format } from "date-fns";
import { es } from "date-fns/locale";

/** Fecha larga en español, p. ej. "miércoles, 7 de octubre de 2026". */
export function fechaLarga(fecha: Date | string): string {
  return format(typeof fecha === "string" ? new Date(fecha) : fecha, "EEEE, d 'de' MMMM 'de' yyyy", { locale: es });
}

