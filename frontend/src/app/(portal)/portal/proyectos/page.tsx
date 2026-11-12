import type { Metadata } from "next";
import Link from "next/link";
import { BarraAvance } from "@/components/controles";
import { apiPublica, type PaginaPublica, type ProgramaPublico, type ProyectoPublico } from "@/lib/portal";
import { fechaCorta } from "@/lib/formato";

export const metadata: Metadata = {
  title: "Portafolio de proyectos · CI²DT²",
  description: "Proyectos aprobados, en ejecución y finalizados del CI²DT² de la Universidad de Caldas.",
};

// Solo los estados que se publican (HU-27).
const ESTADOS = [
  { id: "5", nombre: "Aprobado" },
  { id: "6", nombre: "En ejecución" },
  { id: "7", nombre: "Finalizado" },
];

/** HU-27: portafolio público con filtros por programa, estado y año, paginado. */
export default async function PortafolioProyectos({ searchParams }: PageProps<"/portal/proyectos">) {
  const sp = await searchParams;
  const valor = (k: string) => (typeof sp[k] === "string" ? (sp[k] as string) : "");
  const filtros = { programaId: valor("programaId"), estadoId: valor("estadoId"), anio: valor("anio"), busqueda: valor("busqueda"), pagina: valor("pagina") || "1" };
  const consulta = new URLSearchParams(Object.entries(filtros).filter(([, v]) => v !== ""));

  const [pagina, programas] = await Promise.all([
    apiPublica<PaginaPublica<ProyectoPublico>>(`/proyectos?${consulta}`),
    apiPublica<ProgramaPublico[]>("/programas"),
  ]);
  const totalPaginas = pagina ? Math.max(1, Math.ceil(pagina.total / pagina.tamanoPagina)) : 1;
  const enlacePagina = (n: number) => `/portal/proyectos?${new URLSearchParams({ ...Object.fromEntries(consulta), pagina: String(n) })}`;
  const clase = "w-full rounded-md border border-slate-300 bg-white px-3 py-2 text-sm";

  return (
    <div className="mx-auto flex max-w-6xl flex-col gap-6 px-4 py-10">
      <h1 className="text-3xl font-semibold text-emerald-950">Portafolio de proyectos</h1>
      <form className="grid gap-3 md:grid-cols-5" action="/portal/proyectos">
        <input name="busqueda" defaultValue={filtros.busqueda} placeholder="Buscar por título o código" aria-label="Buscar" className={`${clase} md:col-span-2`} />
        <select name="programaId" defaultValue={filtros.programaId} aria-label="Programa" className={clase}>
          <option value="">Programa: todos</option>
          {programas?.map((p) => <option key={p.id} value={p.id}>{p.nombre}</option>)}
        </select>
        <select name="estadoId" defaultValue={filtros.estadoId} aria-label="Estado" className={clase}>
          <option value="">Estado: todos</option>
          {ESTADOS.map((e) => <option key={e.id} value={e.id}>{e.nombre}</option>)}
        </select>
        <input name="anio" type="number" defaultValue={filtros.anio} placeholder="Año" aria-label="Año" className={clase} />
        <button type="submit" className="rounded-md bg-emerald-800 px-4 py-2 text-sm font-medium text-white hover:bg-emerald-900 md:col-start-5">Filtrar</button>
      </form>

      {!pagina ? (
        <p className="text-sm text-slate-600">El portafolio no está disponible en este momento.</p>
      ) : pagina.items.length === 0 ? (
        <p className="text-sm text-slate-600">No hay proyectos que cumplan los filtros.</p>
      ) : (
        <>
          <ul className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
            {pagina.items.map((p) => (
              <li key={p.codigo}>
                <Link href={`/portal/proyectos/${p.slug}`} className="flex h-full flex-col gap-2 rounded-lg border border-slate-200 p-4 hover:border-emerald-400">
                  <p className="text-xs text-slate-500">{p.codigo} · {p.programa}</p>
                  <p className="font-semibold text-slate-900">{p.titulo}</p>
                  <p className="text-xs text-slate-600">{p.estado} · {fechaCorta(p.fechaInicio)} – {fechaCorta(p.fechaFin)}</p>
                  <BarraAvance valor={p.porcentajeAvance} />
                </Link>
              </li>
            ))}
          </ul>
          <nav aria-label="Paginación" className="flex items-center justify-between text-sm text-slate-600">
            <span>{pagina.total} proyecto(s) · Página {pagina.paginaActual} de {totalPaginas}</span>
            <div className="flex gap-3">
              {pagina.paginaActual > 1 && <Link href={enlacePagina(pagina.paginaActual - 1)} className="text-emerald-800 hover:underline">Anterior</Link>}
              {pagina.paginaActual < totalPaginas && <Link href={enlacePagina(pagina.paginaActual + 1)} className="text-emerald-800 hover:underline">Siguiente</Link>}
            </div>
          </nav>
        </>
      )}
    </div>
  );
}
