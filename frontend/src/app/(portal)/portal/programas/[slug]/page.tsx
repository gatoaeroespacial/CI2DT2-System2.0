import type { Metadata } from "next";
import Link from "next/link";
import { notFound } from "next/navigation";
import { apiPublica, type PersonaPublica, type ProgramaPublico, type ProgramaPublicoDetalle } from "@/lib/portal";

export const revalidate = 600;

export async function generateStaticParams() {
  const programas = await apiPublica<ProgramaPublico[]>("/programas");
  return (programas ?? []).map((p) => ({ slug: p.slug }));
}

export async function generateMetadata({ params }: PageProps<"/portal/programas/[slug]">): Promise<Metadata> {
  const { slug } = await params;
  const programa = await apiPublica<ProgramaPublicoDetalle>(`/programas/${slug}`);
  return programa
    ? { title: `${programa.nombre} · CI²DT²`, description: programa.descripcion ?? `Programa de investigación ${programa.nombre} del CI²DT².` }
    : { title: "Programa no encontrado · CI²DT²" };
}

function Persona({ p }: { p: PersonaPublica }) {
  return (
    <li className="text-sm text-slate-800">
      {p.nombre}
      {p.cvLacUrl && <a href={p.cvLacUrl} target="_blank" rel="noopener noreferrer" className="ml-2 text-emerald-800 hover:underline">CvLAC</a>}
      {p.orcid && <a href={`https://orcid.org/${p.orcid}`} target="_blank" rel="noopener noreferrer" className="ml-2 text-emerald-800 hover:underline">ORCID</a>}
    </li>
  );
}

/** HU-26: detalle público del programa. Nunca se publican correos, teléfonos ni documentos de identidad. */
export default async function ProgramaPortal({ params }: PageProps<"/portal/programas/[slug]">) {
  const { slug } = await params;
  const programa = await apiPublica<ProgramaPublicoDetalle>(`/programas/${slug}`);
  if (!programa) notFound();

  return (
    <div className="mx-auto flex max-w-6xl flex-col gap-8 px-4 py-10">
      <Link href="/#programas" className="text-sm text-emerald-800 hover:underline">← Programas</Link>
      <header>
        <h1 className="text-3xl font-semibold text-emerald-950">{programa.nombre}</h1>
        {programa.descripcion && <p className="mt-3 max-w-3xl whitespace-pre-line text-slate-700">{programa.descripcion}</p>}
      </header>

      {programa.objetivos.length > 0 && (
        <section>
          <h2 className="text-xl font-semibold">Objetivos específicos</h2>
          <ol className="mt-3 list-decimal space-y-1 pl-5 text-sm text-slate-800">
            {programa.objetivos.map((o) => <li key={o}>{o}</li>)}
          </ol>
        </section>
      )}

      <section className="grid gap-6 md:grid-cols-2">
        <div>
          <h2 className="text-xl font-semibold">Equipo</h2>
          {programa.coordinador && (
            <>
              <h3 className="mt-3 text-sm font-medium text-slate-600">Coordinador</h3>
              <ul><Persona p={programa.coordinador} /></ul>
            </>
          )}
          {programa.investigadores.length > 0 && (
            <>
              <h3 className="mt-3 text-sm font-medium text-slate-600">Investigadores</h3>
              <ul className="space-y-1">{programa.investigadores.map((p) => <Persona key={p.nombre} p={p} />)}</ul>
            </>
          )}
        </div>
        <div>
          <h2 className="text-xl font-semibold">Aliados</h2>
          {programa.aliados.length === 0
            ? <p className="mt-3 text-sm text-slate-600">Sin aliados registrados.</p>
            : <ul className="mt-3 flex flex-wrap gap-2">{programa.aliados.map((a) => <li key={a} className="rounded-full bg-slate-100 px-3 py-1 text-sm">{a}</li>)}</ul>}
        </div>
      </section>

      <section>
        <h2 className="text-xl font-semibold">Proyectos</h2>
        {programa.proyectos.length === 0 ? (
          <p className="mt-3 text-sm text-slate-600">El programa aún no tiene proyectos públicos.</p>
        ) : (
          <ul className="mt-3 grid gap-3 md:grid-cols-2">
            {programa.proyectos.map((p) => (
              <li key={p.codigo}>
                <Link href={`/portal/proyectos/${p.slug}`} className="block rounded-lg border border-slate-200 p-4 hover:border-emerald-400">
                  <p className="font-medium text-slate-900">{p.titulo}</p>
                  <p className="text-xs text-slate-600">{p.codigo} · {p.estado}</p>
                </Link>
              </li>
            ))}
          </ul>
        )}
      </section>
    </div>
  );
}
