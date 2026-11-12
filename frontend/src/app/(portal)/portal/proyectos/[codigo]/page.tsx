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
    <article className="mx-auto flex max-w-4xl flex-col gap-6 px-4 py-10">
      <Link href="/portal/proyectos" className="text-sm text-emerald-800 hover:underline">← Portafolio de proyectos</Link>
      <header>
        <p className="text-sm text-slate-500">{p.codigo} · {p.programa}</p>
        <h1 className="mt-1 text-3xl font-semibold text-emerald-950">{p.titulo}</h1>
        <div className="mt-3 flex flex-wrap items-center gap-4 text-sm text-slate-700">
          <span>{p.estado}</span>
          <span>{fechaCorta(p.fechaInicio)} – {fechaCorta(p.fechaFin)}</span>
          <BarraAvance valor={p.porcentajeAvance} />
        </div>
      </header>
      {p.resumen && <p className="whitespace-pre-line text-slate-800">{p.resumen}</p>}
      <section>
        <h2 className="text-xl font-semibold">Equipo</h2>
        <p className="mt-2 text-sm text-slate-700">Investigador principal: {p.investigadorPrincipal}</p>
        <ul className="mt-2 space-y-1 text-sm text-slate-800">
          {detalle.equipo.map((e) => (
            <li key={e.nombre}>
              {e.nombre}
              {e.cvLacUrl && <a href={e.cvLacUrl} target="_blank" rel="noopener noreferrer" className="ml-2 text-emerald-800 hover:underline">CvLAC</a>}
              {e.orcid && <a href={`https://orcid.org/${e.orcid}`} target="_blank" rel="noopener noreferrer" className="ml-2 text-emerald-800 hover:underline">ORCID</a>}
            </li>
          ))}
        </ul>
      </section>
    </article>
  );
}
