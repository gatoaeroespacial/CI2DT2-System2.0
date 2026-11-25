import type { Metadata } from "next";
import Link from "next/link";
import { notFound } from "next/navigation";
import { BarraAvance } from "@/components/controles";
import { apiPublica, type ProyectoPublicoDetalle } from "@/lib/portal";
import { fechaCorta } from "@/lib/formato";

export const revalidate = 600;

export async function generateMetadata({ params }: PageProps<"/portal/proyectos/[codigo]">): Promise<Metadata> {
  const { codigo } = await params;
  const detalle = await apiPublica<ProyectoPublicoDetalle>(`/proyectos/${codigo}`);
  return detalle
    ? { title: `${detalle.proyecto.titulo} · CI²DT²`, description: detalle.proyecto.resumen?.slice(0, 160) ?? detalle.proyecto.titulo }
    : { title: "Proyecto no encontrado · CI²DT²" };
}

/**
 * HU-27: ficha pública del proyecto por su código. Solo Aprobado, En ejecución o Finalizado; nunca
 * presupuestos, fuentes, documentos, revisiones ni datos de contacto (los demás responden 404).
 */
export default async function ProyectoPortal({ params }: PageProps<"/portal/proyectos/[codigo]">) {
  const { codigo } = await params;
  const detalle = await apiPublica<ProyectoPublicoDetalle>(`/proyectos/${codigo}`);
  if (!detalle) notFound();
  const p = detalle.proyecto;

  return (
    <div className="w-full px-margin md:px-margin-tablet lg:px-margin-desktop py-space-xl bg-surface">
      <article className="max-w-4xl mx-auto flex flex-col gap-space-lg bg-surface-container-lowest p-space-xl rounded-xl shadow-sm border border-outline-variant/30">
        <Link
          href="/portal/proyectos"
          className="inline-flex items-center gap-space-xs font-headline-sm text-body-sm text-primary hover:text-secondary transition-colors w-fit"
        >
          <span className="material-symbols-outlined text-[18px]" aria-hidden="true">arrow_back</span>
          <span>Volver al portafolio de proyectos</span>
        </Link>

        <header className="flex flex-col gap-space-sm pb-space-md border-b border-outline-variant/30">
          <div className="flex flex-wrap items-center gap-space-sm">
            <span className="px-space-sm py-0.5 rounded bg-primary-fixed/40 font-label-sm text-label-sm text-primary font-semibold">
              {p.codigo}
            </span>
            <span className="font-label-sm text-label-sm text-on-surface-variant uppercase">
              {p.programa}
            </span>
          </div>
          <h1 className="font-headline-lg text-headline-lg-mobile md:text-headline-lg text-primary tracking-tight">
            {p.titulo}
          </h1>
          <div className="flex flex-wrap items-center gap-space-md text-body-sm text-on-surface-variant pt-space-xs">
            <span className="px-space-sm py-0.5 rounded bg-secondary-container/40 text-on-secondary-container font-label-sm font-semibold">
              {p.estado}
            </span>
            <span>
              {fechaCorta(p.fechaInicio)} – {fechaCorta(p.fechaFin)}
            </span>
            <div className="min-w-[160px] flex-1 max-w-xs">
              <BarraAvance valor={p.porcentajeAvance} />
            </div>
          </div>
        </header>

        {p.resumen && (
          <section className="flex flex-col gap-space-xs">
            <h2 className="font-headline-sm text-headline-sm text-primary">Resumen del proyecto</h2>
            <p className="whitespace-pre-line font-body-md text-body-md text-on-surface-variant">{p.resumen}</p>
          </section>
        )}

        <section className="flex flex-col gap-space-sm pt-space-md border-t border-outline-variant/30">
          <h2 className="font-headline-sm text-headline-sm text-primary">Equipo investigador</h2>
          <p className="font-body-sm text-body-sm text-on-surface">
            <strong>Investigador principal:</strong> {p.investigadorPrincipal}
          </p>
          {detalle.equipo.length > 0 && (
            <ul className="grid grid-cols-1 sm:grid-cols-2 gap-space-sm mt-space-xs">
              {detalle.equipo.map((e) => (
                <li
                  key={e.nombre}
                  className="rounded-lg bg-surface-container-low px-space-md py-space-sm text-body-sm text-on-surface flex flex-wrap items-center justify-between gap-space-xs"
                >
                  <span className="font-medium">{e.nombre}</span>
                  <div className="flex items-center gap-space-sm">
                    {e.cvLacUrl && (
                      <a
                        href={e.cvLacUrl}
                        target="_blank"
                        rel="noopener noreferrer"
                        className="font-label-sm text-label-sm text-primary hover:text-secondary underline"
                      >
                        CvLAC
                      </a>
                    )}
                    {e.orcid && (
                      <a
                        href={`https://orcid.org/${e.orcid}`}
                        target="_blank"
                        rel="noopener noreferrer"
                        className="font-label-sm text-label-sm text-primary hover:text-secondary underline"
                      >
                        ORCID
                      </a>
                    )}
                  </div>
                </li>
              ))}
            </ul>
          )}
        </section>
      </article>
    </div>
  );
}
