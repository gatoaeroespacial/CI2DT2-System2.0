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
  const filtros = {
    programaId: valor("programaId"),
    estadoId: valor("estadoId"),
    anio: valor("anio"),
    busqueda: valor("busqueda"),
    pagina: valor("pagina") || "1",
  };
  const consulta = new URLSearchParams(Object.entries(filtros).filter(([, v]) => v !== ""));

  const [pagina, programas] = await Promise.all([
    apiPublica<PaginaPublica<ProyectoPublico>>(`/proyectos?${consulta}`),
    apiPublica<ProgramaPublico[]>("/programas"),
  ]);
  const totalPaginas = pagina ? Math.max(1, Math.ceil(pagina.total / pagina.tamanoPagina)) : 1;
  const enlacePagina = (n: number) =>
    `/portal/proyectos?${new URLSearchParams({ ...Object.fromEntries(consulta), pagina: String(n) })}`;
  const claseCampo =
    "w-full rounded-lg border border-outline-variant/60 bg-surface-container-lowest px-space-sm py-space-sm text-body-sm text-on-surface focus:border-primary focus:outline-none";

  return (
    <div className="w-full px-margin md:px-margin-tablet lg:px-margin-desktop py-space-xl bg-surface">
      <div className="max-w-7xl mx-auto flex flex-col gap-space-xl">
        <header className="flex flex-col gap-space-sm">
          <span className="font-label-md text-label-md text-secondary font-semibold uppercase tracking-wider">
            Producción Científica y Tecnológica
          </span>
          <h1 className="font-headline-lg text-headline-lg-mobile md:text-headline-lg text-primary tracking-tight">
            Portafolio de Proyectos
          </h1>
          <p className="font-body-md text-body-md text-on-surface-variant max-w-3xl">
            Consulta pública de proyectos aprobados, en ejecución y finalizados del CI²DT². Selecciona cualquier proyecto para conocer su ficha técnica pública y equipo investigador.
          </p>
        </header>

        <form
          className="grid gap-space-sm md:grid-cols-5 bg-surface-container-lowest p-space-md rounded-xl shadow-sm border border-outline-variant/30"
          action="/portal/proyectos"
        >
          <input
            name="busqueda"
            defaultValue={filtros.busqueda}
            placeholder="Buscar por título o código"
            aria-label="Buscar"
            className={`${claseCampo} md:col-span-2`}
          />
          <select name="programaId" defaultValue={filtros.programaId} aria-label="Programa" className={claseCampo}>
            <option value="">Programa: todos</option>
            {programas?.map((p) => (
              <option key={p.id} value={p.id}>
                {p.nombre}
              </option>
            ))}
          </select>
          <select name="estadoId" defaultValue={filtros.estadoId} aria-label="Estado" className={claseCampo}>
            <option value="">Estado: todos</option>
            {ESTADOS.map((e) => (
              <option key={e.id} value={e.id}>
                {e.nombre}
              </option>
            ))}
          </select>
          <input
            name="anio"
            type="number"
            defaultValue={filtros.anio}
            placeholder="Año"
            aria-label="Año"
            className={claseCampo}
          />
          <button
            type="submit"
            className="rounded-lg bg-primary px-space-md py-space-sm font-headline-sm text-body-sm text-on-primary hover:bg-primary-container transition-colors md:col-start-5"
          >
            Filtrar
          </button>
        </form>

        {!pagina ? (
          <div className="rounded-xl bg-surface-container-lowest p-space-lg shadow-sm border border-outline-variant/30">
            <p className="font-body-md text-body-md text-on-surface-variant">
              El portafolio no está disponible en este momento.
            </p>
          </div>
        ) : pagina.items.length === 0 ? (
          <div className="rounded-xl bg-surface-container-lowest p-space-lg shadow-sm border border-outline-variant/30">
            <p className="font-body-md text-body-md text-on-surface-variant">
              No hay proyectos que cumplan los filtros seleccionados.
            </p>
          </div>
        ) : (
          <>
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-gutter-desktop">
              {pagina.items.map((p) => (
                <Link key={p.codigo} href={`/portal/proyectos/${p.slug}`} className="group block h-full">
                  <article className="h-full bg-surface-container-lowest p-space-lg rounded-xl shadow-sm border border-outline-variant/30 group-hover:shadow-lg transition-all flex flex-col justify-between gap-space-md">
                    <div className="flex flex-col gap-space-xs">
                      <div className="flex items-center justify-between gap-space-xs mb-space-xs">
                        <span className="font-label-sm text-label-sm font-semibold text-primary px-space-sm py-0.5 bg-primary-fixed/40 rounded">
                          {p.codigo}
                        </span>
                        <span className="font-label-sm text-label-sm text-secondary font-semibold">{p.estado}</span>
                      </div>
                      <h2 className="font-headline-sm text-headline-sm text-primary group-hover:text-secondary transition-colors">
                        {p.titulo}
                      </h2>
                      <p className="font-label-sm text-label-sm text-on-surface-variant">{p.programa}</p>
                      {p.resumen && (
                        <p className="font-body-sm text-body-sm text-on-surface-variant line-clamp-3 mt-space-xs">
                          {p.resumen}
                        </p>
                      )}
                    </div>

                    <div className="flex flex-col gap-space-sm pt-space-sm border-t border-outline-variant/30">
                      <p className="font-body-sm text-body-sm text-on-surface-variant">
                        <strong className="text-on-surface">Investigador:</strong> {p.investigadorPrincipal}
                      </p>
                      <p className="font-label-sm text-label-sm text-on-surface-variant">
                        {fechaCorta(p.fechaInicio)} – {fechaCorta(p.fechaFin)}
                      </p>
                      <BarraAvance valor={p.porcentajeAvance} />
                      <div className="flex items-center justify-between text-primary font-headline-sm text-body-sm group-hover:text-secondary transition-colors pt-space-xs">
                        <span>Ver ficha pública</span>
                        <span className="material-symbols-outlined text-[18px]" aria-hidden="true">
                          arrow_forward
                        </span>
                      </div>
                    </div>
                  </article>
                </Link>
              ))}
            </div>

            <nav
              aria-label="Paginación"
              className="flex items-center justify-between text-body-sm text-on-surface-variant pt-space-md border-t border-outline-variant/30"
            >
              <span>
                {pagina.total} proyecto(s) · Página {pagina.paginaActual} de {totalPaginas}
              </span>
              <div className="flex gap-space-md">
                {pagina.paginaActual > 1 && (
                  <Link
                    href={enlacePagina(pagina.paginaActual - 1)}
                    className="font-headline-sm text-primary hover:text-secondary transition-colors"
                  >
                    Anterior
                  </Link>
                )}
                {pagina.paginaActual < totalPaginas && (
                  <Link
                    href={enlacePagina(pagina.paginaActual + 1)}
                    className="font-headline-sm text-primary hover:text-secondary transition-colors"
                  >
                    Siguiente
                  </Link>
                )}
              </div>
            </nav>
          </>
        )}
      </div>
    </div>
  );
}
