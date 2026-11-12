/**
 * Lectura de la API pública del portal desde el servidor de Next.js (HU-25 a HU-27). Las páginas
 * se renderizan de forma estática y se revalidan cada 10 minutos (ISR). Si la API no responde
 * (p. ej. durante la compilación), se devuelve null y la página muestra un estado alternativo.
 */
const API = process.env.API_URL ?? "http://localhost:5132";

export const REVALIDAR_SEGUNDOS = 600;

export async function apiPublica<T>(ruta: string): Promise<T | null> {
  try {
    const respuesta = await fetch(`${API}/api/v1/publico${ruta}`, { next: { revalidate: REVALIDAR_SEGUNDOS } });
    if (!respuesta.ok) return null;
    return (await respuesta.json()) as T;
  } catch {
    return null;
  }
}

/** URL pública del sitio (metadatos, sitemap). */
export const URL_SITIO =
  process.env.SITE_URL ?? (process.env.VERCEL_PROJECT_PRODUCTION_URL ? `https://${process.env.VERCEL_PROJECT_PRODUCTION_URL}` : "http://localhost:3000");

export type ProgramaPublico = { id: number; nombre: string; slug: string; descripcion: string | null; coordinador: string | null; proyectosPublicos: number };

export type ConvocatoriaPublica = {
  id: number;
  nombre: string;
  numero: string | null;
  tipoConvocatoria: string;
  entidadConvocante: string | null;
  plataforma: string;
  url: string | null;
  fechaApertura: string | null;
  fechaCierre: string | null;
  moneda: string | null;
  montoMaximo: number | null;
};

export type ResumenPortal = {
  cifras: { proyectosEnEjecucion: number; proyectosFinalizados: number; aliados: number; programas: number };
  programas: ProgramaPublico[];
  convocatoriasAbiertas: ConvocatoriaPublica[];
};

export type PersonaPublica = { nombre: string; cvLacUrl: string | null; orcid: string | null };

export type ProyectoPublico = {
  codigo: string;
  slug: string;
  titulo: string;
  resumen: string | null;
  programa: string;
  investigadorPrincipal: string;
  estado: string;
  porcentajeAvance: number | null;
  fechaInicio: string | null;
  fechaFin: string | null;
};

export type ProgramaPublicoDetalle = {
  nombre: string;
  slug: string;
  descripcion: string | null;
  objetivos: string[];
  coordinador: PersonaPublica | null;
  investigadores: PersonaPublica[];
  aliados: string[];
  proyectos: ProyectoPublico[];
};

export type ProyectoPublicoDetalle = { proyecto: ProyectoPublico; equipo: PersonaPublica[] };

export type PaginaPublica<T> = { items: T[]; total: number; paginaActual: number; tamanoPagina: number };
