import type { Metadata } from "next";
import Link from "next/link";
import { apiPublica, type ResumenPortal } from "@/lib/portal";
import { fechaCorta, formatoMonto } from "@/lib/formato";
import { contenidoInstitucional as c } from "./contenido-institucional";

// HU-25: renderizado estático con revalidación incremental cada 10 minutos.
export const revalidate = 600;

export const metadata: Metadata = {
  title: `${c.nombre} · ${c.institucion}`,
  description: `${c.nombreCompleto} de la ${c.institucion}: programas de investigación, proyectos y convocatorias abiertas.`,
  openGraph: { title: c.nombre, description: c.nombreCompleto, locale: "es_CO", type: "website" },
};

function Cifra({ valor, etiqueta }: { valor: number; etiqueta: string }) {
  return (
    <div className="rounded-lg border border-slate-200 bg-white p-4 text-center">
      <p className="text-3xl font-semibold text-emerald-900">{valor}</p>
      <p className="text-sm text-slate-600">{etiqueta}</p>
    </div>
  );
}

export default async function Inicio() {
  const resumen = await apiPublica<ResumenPortal>("/resumen");

  return (
    <>
      <section className="bg-emerald-950 text-emerald-50">
        <div className="mx-auto max-w-6xl px-4 py-14">
          <p className="text-sm uppercase tracking-wide text-emerald-200">{c.institucion}</p>
          <h1 className="mt-2 text-3xl font-semibold sm:text-4xl">{c.nombreCompleto}</h1>
          {c.proposito && <p className="mt-4 max-w-3xl text-lg text-emerald-100">{c.proposito}</p>}
          <div className="mt-6 flex flex-wrap gap-3">
            <Link href="/portal/proyectos" className="rounded-md bg-white px-4 py-2 text-sm font-medium text-emerald-900 hover:bg-emerald-50">
              Conocer los proyectos
            </Link>
            <Link href="#convocatorias" className="rounded-md border border-emerald-300 px-4 py-2 text-sm font-medium text-white hover:bg-emerald-900">
              Convocatorias abiertas
            </Link>
          </div>
        </div>
      </section>

      <div className="mx-auto flex max-w-6xl flex-col gap-12 px-4 py-10">
        {resumen ? (
          <section aria-label="Cifras del centro" className="grid grid-cols-2 gap-4 md:grid-cols-4">
            <Cifra valor={resumen.cifras.proyectosEnEjecucion} etiqueta="Proyectos en ejecución" />
            <Cifra valor={resumen.cifras.proyectosFinalizados} etiqueta="Proyectos finalizados" />
            <Cifra valor={resumen.cifras.aliados} etiqueta="Aliados" />
            <Cifra valor={resumen.cifras.programas} etiqueta="Programas" />
          </section>
        ) : (
          <p className="rounded-md bg-slate-50 px-4 py-3 text-sm text-slate-600">La información del centro no está disponible en este momento.</p>
        )}

        {c.objetivosEstrategicos.length > 0 && (
          <section>
            <h2 className="text-2xl font-semibold">Objetivos estratégicos</h2>
            <ol className="mt-4 grid gap-3 md:grid-cols-2">
              {c.objetivosEstrategicos.map((o, i) => (
                <li key={o} className="rounded-lg border border-slate-200 p-4 text-sm text-slate-800"><strong>{i + 1}.</strong> {o}</li>
              ))}
            </ol>
          </section>
        )}

        {resumen && resumen.programas.length > 0 && (
          <section id="programas">
            <h2 className="text-2xl font-semibold">Programas de investigación</h2>
            <ul className="mt-4 grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
              {resumen.programas.map((p) => (
                <li key={p.id}>
                  <Link href={`/portal/programas/${p.slug}`} className="block h-full rounded-lg border border-slate-200 p-4 hover:border-emerald-400 hover:bg-emerald-50">
                    <p className="font-semibold text-emerald-900">{p.nombre}</p>
                    {p.descripcion && <p className="mt-1 line-clamp-3 text-sm text-slate-700">{p.descripcion}</p>}
                    <p className="mt-2 text-xs text-slate-500">{p.proyectosPublicos} proyecto(s) públicos{p.coordinador ? ` · Coordina: ${p.coordinador}` : ""}</p>
                  </Link>
                </li>
              ))}
            </ul>
          </section>
        )}

        {c.servicios.length > 0 && (
          <section>
            <h2 className="text-2xl font-semibold">Servicios y capacidades</h2>
            <ul className="mt-4 grid list-disc gap-2 pl-5 text-sm text-slate-800 md:grid-cols-2">
              {c.servicios.map((s) => <li key={s}>{s}</li>)}
            </ul>
          </section>
        )}

        <section id="convocatorias">
          <h2 className="text-2xl font-semibold">Convocatorias abiertas</h2>
          {!resumen || resumen.convocatoriasAbiertas.length === 0 ? (
            <p className="mt-3 text-sm text-slate-600">No hay convocatorias abiertas en este momento.</p>
          ) : (
            <ul className="mt-4 grid gap-4 md:grid-cols-2">
              {resumen.convocatoriasAbiertas.map((cv) => (
                <li key={cv.id} className="rounded-lg border border-slate-200 p-4">
                  <p className="font-semibold text-slate-900">{cv.nombre}{cv.numero ? ` (${cv.numero})` : ""}</p>
                  <p className="text-sm text-slate-600">{cv.tipoConvocatoria}{cv.entidadConvocante ? ` · ${cv.entidadConvocante}` : ""}</p>
                  <p className="mt-1 text-sm text-slate-700">
                    Cierre: {fechaCorta(cv.fechaCierre)}{cv.montoMaximo ? ` · Hasta ${formatoMonto(cv.montoMaximo, cv.moneda)}` : ""}
                  </p>
                  {cv.url && (
                    <a href={cv.url} target="_blank" rel="noopener noreferrer" className="mt-2 inline-block text-sm text-emerald-800 hover:underline">
                      Términos de referencia
                    </a>
                  )}
                </li>
              ))}
            </ul>
          )}
        </section>
      </div>
    </>
  );
}
