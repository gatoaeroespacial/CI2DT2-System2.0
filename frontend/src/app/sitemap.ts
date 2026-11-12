import type { MetadataRoute } from "next";
import { apiPublica, URL_SITIO, type PaginaPublica, type ProgramaPublico, type ProyectoPublico } from "@/lib/portal";

export const revalidate = 600;

/** HU-25: sitemap.xml del portal público. */
export default async function sitemap(): Promise<MetadataRoute.Sitemap> {
  const [programas, proyectos] = await Promise.all([
    apiPublica<ProgramaPublico[]>("/programas"),
    apiPublica<PaginaPublica<ProyectoPublico>>("/proyectos?pagina=1"),
  ]);
  return [
    { url: `${URL_SITIO}/`, changeFrequency: "daily", priority: 1 },
    { url: `${URL_SITIO}/portal/proyectos`, changeFrequency: "daily", priority: 0.8 },
    ...(programas ?? []).map((p) => ({ url: `${URL_SITIO}/portal/programas/${p.slug}`, changeFrequency: "weekly" as const, priority: 0.7 })),
    ...(proyectos?.items ?? []).map((p) => ({ url: `${URL_SITIO}/portal/proyectos/${p.slug}`, changeFrequency: "weekly" as const, priority: 0.6 })),
  ];
}
