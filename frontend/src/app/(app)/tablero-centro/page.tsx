"use client";

import { keepPreviousData, useQuery, type UseQueryResult } from "@tanstack/react-query";
import Link from "next/link";
import { useState } from "react";
import { Cargando, EstadoVacio } from "@/components/ui";
import { api } from "@/lib/api";
import { type Pagina } from "@/lib/consultas";
import { fechaCorta, formatoCop } from "@/lib/formato";

type Hito = {
  hitoId: number;
  titulo: string;
  fechaLimite: string;
  estado: string;
  proyectoId: number;
  codigo: string | null;
  programa: string;
  responsable: string | null;
};

type Tablero = {
  anio: number | null;
  tarjetas: {
    proyectosEnEjecucion: number;
    propuestasEnElaboracion: number;
    propuestasEnRevision: number;
    propuestasDevueltas: number;
    propuestasPresentadas: number;
    tasaAprobacion: number | null;
    postulacionesConResultado: number;
    montoGestionadoCop: number;
  };
  proyectosPorEstadoYPrograma: { programa: string; estado: string; cantidad: number }[];
  hitosProximos: Hito[];
  hitosVencidos: Hito[];
  avancesPendientes: { programa: string; cantidad: number }[];
};

type Convocatoria = {
  id: number;
  nombre: string;
  numero: string | null;
  fechaCierre: string | null;
  estado: string;
};

type Proyecto = {
  id: number;
  codigo: string | null;
  titulo: string;
  programa: string;
  investigadorPrincipal: string;
  estado: string;
};

const coloresEstado: Record<string, string> = {
  "En elaboración": "#94a3b8",
  "En revisión": "#f59e0b",
  Devuelto: "#fb923c",
  Presentado: "#0ea5e9",
  Aprobado: "#10b981",
  "En ejecución": "#6366f1",
  Finalizado: "#14b8a6",
  "No aprobado": "#ef4444",
  Suspendido: "#71717a",
};

const tarjeta = "rounded-xl bg-surface-container-lowest p-space-lg shadow-sm transition-shadow hover:shadow-md";
const enlace = "font-medium text-primary transition hover:text-secondary";
const entradaFiltro = "w-full rounded-lg border border-outline-variant bg-surface px-4 py-3 text-sm text-on-surface outline-none transition focus:border-primary focus:ring-2 focus:ring-primary/15";

function IconoMetrica({ nombre }: { nombre: "proyectos" | "propuestas" | "convocatorias" | "tasa" | "hitos" | "avances" | "monto" }) {
  const paths = {
    proyectos: <><path d="M3 7a2 2 0 0 1 2-2h5l2 2h7a2 2 0 0 1 2 2v9a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2Z" /><path d="M3 10h18" /></>,
    propuestas: <><path d="M7 3h7l4 4v14H7a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2Z" /><path d="M14 3v5h5M9 13h6M9 17h6" /></>,
    convocatorias: <><path d="M3 11v2a2 2 0 0 0 2 2h3l7 4V5l-7 4H5a2 2 0 0 0-2 2Z" /><path d="M8 15v4a2 2 0 0 0 2 2h2M18 9a4 4 0 0 1 0 6" /></>,
    tasa: <><path d="M4 19V5M4 19h17" /><path d="m7 15 3-4 3 2 5-7" /></>,
    hitos: <><circle cx="12" cy="12" r="9" /><path d="M12 7v5l3 2" /></>,
    avances: <><circle cx="12" cy="12" r="9" /><path d="m8 12 2.5 2.5L16.5 8" /></>,
    monto: <><path d="M12 2v20M17 6H9.5a3.5 3.5 0 0 0 0 7h5a3.5 3.5 0 0 1 0 7H6" /></>,
  }[nombre];

  return (
    <span className="flex h-10 w-10 shrink-0 items-center justify-center rounded-lg bg-primary-fixed/30 text-primary">
      <svg aria-hidden="true" className="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round">
        {paths}
      </svg>
    </span>
  );
}

function TarjetaMetrica({
  icono,
  etiqueta,
  valor,
  detalle,
}: {
  icono: Parameters<typeof IconoMetrica>[0]["nombre"];
  etiqueta: string;
  valor: string;
  detalle: string;
}) {
  return (
    <article className={tarjeta}>
      <div className="flex items-start justify-between gap-3">
        <p className="font-label-sm text-label-sm uppercase text-on-surface-variant">{etiqueta}</p>
        <IconoMetrica nombre={icono} />
      </div>
      <p className="mt-space-md font-headline-xl text-2xl font-bold tracking-tight text-primary">{valor}</p>
      <p className="mt-space-xs text-sm leading-relaxed text-on-surface-variant">{detalle}</p>
    </article>
  );
}

function EstadoPildora({ estado }: { estado: string }) {
  const clase =
    estado === "En ejecución" || estado === "Aprobado" || estado === "Abierta"
      ? "bg-secondary-container/40 text-on-secondary-container"
      : estado === "Vencido" || estado === "No aprobado"
        ? "bg-error-container/60 text-error"
        : estado === "En revisión" || estado === "Próxima"
          ? "bg-surface-container text-primary-container"
          : "bg-surface-container-low text-on-surface-variant";

  return <span className={`inline-flex rounded-full px-2.5 py-1 text-xs font-medium ${clase}`}>{estado}</span>;
}

function ErrorSeccion({ mensaje, reintentar }: { mensaje: string; reintentar: () => void }) {
  return (
    <div role="alert" className="rounded-lg border border-error/20 bg-error-container/40 p-4 text-sm text-error">
      <p>{mensaje}</p>
      <button type="button" className="mt-2 font-semibold underline underline-offset-2" onClick={reintentar}>
        Reintentar
      </button>
    </div>
  );
}

function diferenciaDias(fecha: string | null): number | null {
  if (!fecha) return null;
  const cierre = new Date(`${fecha.slice(0, 10)}T00:00:00`);
  if (Number.isNaN(cierre.getTime())) return null;
  const hoy = new Date();
  hoy.setHours(0, 0, 0, 0);
  return Math.ceil((cierre.getTime() - hoy.getTime()) / 86_400_000);
}

/** HU-24: tablero de control del Director con filtro por año. */
export default function TableroCentro() {
  const actual = new Date().getFullYear();
  const [anio, setAnio] = useState("");
  const tablero = useQuery({
    queryKey: ["tablero", anio],
    queryFn: () => api<Tablero>(`/dashboard/centro${anio ? `?anio=${anio}` : ""}`),
    placeholderData: keepPreviousData,
  });
  const convocatorias = useQuery({
    queryKey: ["tablero-convocatorias-abiertas"],
    queryFn: () => api<Pagina<Convocatoria>>("/convocatorias?estado=Abierta&pagina=1"),
  });
  const proyectos = useQuery({
    queryKey: ["tablero-proyectos-recientes"],
    queryFn: () => api<Pagina<Proyecto>>("/proyectos?pagina=1"),
  });

  return (
    <>
      <div className="mb-space-lg flex flex-wrap items-end justify-between gap-4">
        <div>
          <p className="font-label-sm text-label-sm uppercase text-secondary">CI²DT² · Universidad de Caldas</p>
          <h1 className="mt-1 font-headline-md text-headline-md font-semibold text-on-surface">Tablero del centro</h1>
        </div>
        <label className="flex items-center gap-2 text-sm text-on-surface-variant">
          <span>Periodo</span>
          <select aria-label="Año" className={`${entradaFiltro} w-40`} value={anio} onChange={(e) => setAnio(e.target.value)}>
            <option value="">Todos los años</option>
            {Array.from({ length: 6 }, (_, i) => actual + 1 - i).map((a) => <option key={a} value={a}>{a}</option>)}
          </select>
        </label>
      </div>

      <section className="mb-space-lg rounded-xl bg-gradient-to-r from-surface-container-low to-surface px-5 py-5 sm:px-7">
        <p className="font-label-sm text-label-sm uppercase text-secondary">CI²DT² · Universidad de Caldas</p>
        <h2 className="mt-2 font-headline-md text-headline-md font-semibold text-on-surface">Resumen ejecutivo</h2>
        <p className="mt-1 max-w-3xl text-sm leading-relaxed text-on-surface-variant">
          Estado de proyectos, seguimiento operativo y oportunidades de financiación del centro.
        </p>
      </section>

      {tablero.isPending ? (
        <Cargando />
      ) : tablero.isError ? (
        <ErrorSeccion mensaje={tablero.error.message} reintentar={() => tablero.refetch()} />
      ) : (
        <Contenido
          t={tablero.data}
          convocatorias={convocatorias}
          proyectos={proyectos}
        />
      )}
    </>
  );
}

function Contenido({
  t,
  convocatorias,
  proyectos,
}: {
  t: Tablero;
  convocatorias: UseQueryResult<Pagina<Convocatoria>, Error>;
  proyectos: UseQueryResult<Pagina<Proyecto>, Error>;
}) {
  const k = t.tarjetas;
  const propuestasEnTramite = k.propuestasEnElaboracion + k.propuestasEnRevision + k.propuestasDevueltas;
  const programas = [...new Set(t.proyectosPorEstadoYPrograma.map((x) => x.programa))];
  const estados = [...new Set(t.proyectosPorEstadoYPrograma.map((x) => x.estado))];
  const llamadasAbiertas = convocatorias.data?.items ?? [];
  const ultimosProyectos = proyectos.data?.items.slice(0, 4) ?? [];

  return (
    <div className="flex flex-col gap-space-lg">
      <section aria-label="Indicadores clave" className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
        <TarjetaMetrica icono="proyectos" etiqueta="Proyectos en ejecución" valor={String(k.proyectosEnEjecucion)} detalle="Proyectos activos en fase de ejecución" />
        <TarjetaMetrica icono="propuestas" etiqueta="Propuestas en trámite" valor={String(propuestasEnTramite)}
          detalle={`${k.propuestasEnElaboracion} en elaboración · ${k.propuestasEnRevision} en revisión · ${k.propuestasDevueltas} devueltas`} />
        <TarjetaMetrica icono="propuestas" etiqueta="Propuestas presentadas" valor={String(k.propuestasPresentadas)} detalle="Con fecha de presentación en el periodo" />
        <TarjetaMetrica icono="convocatorias" etiqueta="Convocatorias abiertas" valor={convocatorias.isError ? "—" : convocatorias.isPending ? "…" : String(convocatorias.data.total)}
          detalle={convocatorias.isError ? "No fue posible consultar el catálogo" : "Oportunidades vigentes para postular"} />
        <TarjetaMetrica icono="tasa" etiqueta="Tasa de aprobación" valor={k.tasaAprobacion === null ? "—" : `${k.tasaAprobacion} %`}
          detalle={`${k.postulacionesConResultado} postulaciones con resultado`} />
        <TarjetaMetrica icono="hitos" etiqueta="Hitos vencidos" valor={String(t.hitosVencidos.length)} detalle="Requieren revisión y seguimiento" />
        <TarjetaMetrica icono="avances" etiqueta="Avances por validar" valor={String(t.avancesPendientes.reduce((total, a) => total + a.cantidad, 0))}
          detalle="Pendientes de revisión del centro" />
        <TarjetaMetrica icono="monto" etiqueta="Monto gestionado" valor={formatoCop(k.montoGestionadoCop)}
          detalle="Monto solicitado en postulaciones seleccionadas" />
      </section>

      <section className={tarjeta} aria-labelledby="titulo-distribucion">
        <header className="mb-5">
          <p className="font-label-sm text-label-sm uppercase text-secondary">Portafolio</p>
          <h2 id="titulo-distribucion" className="mt-1 font-headline-sm text-headline-sm font-semibold text-on-surface">Proyectos por estado y programa</h2>
          <p className="mt-1 text-sm text-on-surface-variant">Distribución del portafolio según su etapa actual.</p>
        </header>
        {programas.length === 0 ? (
          <EstadoVacio titulo="No hay proyectos en el periodo" />
        ) : (
          <>
            <ul className="flex flex-col gap-5" aria-label="Proyectos por estado agrupados por programa">
              {programas.map((programa) => {
                const filas = t.proyectosPorEstadoYPrograma.filter((x) => x.programa === programa);
                const total = filas.reduce((s, x) => s + x.cantidad, 0);
                return (
                  <li key={programa}>
                    <div className="mb-2 flex items-baseline justify-between gap-3 text-sm">
                      <span className="font-medium text-on-surface">{programa}</span>
                      <span className="shrink-0 text-on-surface-variant">{total} proyectos</span>
                    </div>
                    <div className="flex h-3 overflow-hidden rounded-full bg-surface-container" role="img" aria-label={`${programa}: ${filas.map((f) => `${f.cantidad} ${f.estado}`).join(", ")}`}>
                      {filas.map((f) => (
                        <span key={f.estado} title={`${f.estado}: ${f.cantidad}`}
                          style={{ width: `${(f.cantidad / total) * 100}%`, backgroundColor: coloresEstado[f.estado] ?? "#64748b" }} />
                      ))}
                    </div>
                  </li>
                );
              })}
            </ul>
            <ul className="mt-5 flex flex-wrap gap-x-4 gap-y-2 border-t border-outline-variant/50 pt-4 text-xs text-on-surface-variant" aria-label="Estados de proyectos">
              {estados.map((estado) => (
                <li key={estado} className="flex items-center gap-2">
                  <span className="h-2.5 w-2.5 rounded-sm" style={{ backgroundColor: coloresEstado[estado] ?? "#64748b" }} />
                  {estado}
                </li>
              ))}
            </ul>
          </>
        )}
      </section>

      <div className="grid gap-space-lg xl:grid-cols-2">
        <section className={tarjeta} aria-labelledby="titulo-convocatorias">
          <header className="mb-4 flex items-start justify-between gap-3">
            <div>
              <p className="font-label-sm text-label-sm uppercase text-secondary">Financiación</p>
              <h2 id="titulo-convocatorias" className="mt-1 font-headline-sm text-headline-sm font-semibold text-on-surface">Convocatorias abiertas</h2>
            </div>
            <Link href="/convocatorias" className={`${enlace} shrink-0 text-sm`}>Ver todas</Link>
          </header>
          {convocatorias.isPending ? <p className="py-5 text-sm text-on-surface-variant">Cargando oportunidades…</p>
            : convocatorias.isError ? <ErrorSeccion mensaje={convocatorias.error.message} reintentar={() => convocatorias.refetch()} />
              : llamadasAbiertas.length === 0 ? <EstadoVacio titulo="No hay convocatorias abiertas" />
                : (
                  <ul className="divide-y divide-outline-variant/40">
                    {llamadasAbiertas.slice(0, 4).map((c) => {
                      const dias = diferenciaDias(c.fechaCierre);
                      return (
                        <li key={c.id} className="flex items-center justify-between gap-4 py-3 first:pt-0 last:pb-0">
                          <div className="min-w-0">
                            <Link href={`/convocatorias/${c.id}`} className={`${enlace} line-clamp-2 text-sm`}>{c.nombre}</Link>
                            <p className="mt-1 text-xs text-on-surface-variant">{c.numero ? `N.º ${c.numero} · ` : ""}Cierre: {fechaCorta(c.fechaCierre)}</p>
                          </div>
                          <span className={`shrink-0 text-xs font-semibold ${dias !== null && dias <= 7 ? "text-error" : "text-secondary"}`}>
                            {dias === null ? "Abierta" : dias < 0 ? "Cierre vencido" : dias === 0 ? "Cierra hoy" : `${dias} días`}
                          </span>
                        </li>
                      );
                    })}
                  </ul>
                )}
        </section>

        <section className={tarjeta} aria-labelledby="titulo-recientes">
          <header className="mb-4 flex items-start justify-between gap-3">
            <div>
              <p className="font-label-sm text-label-sm uppercase text-secondary">Actividad</p>
              <h2 id="titulo-recientes" className="mt-1 font-headline-sm text-headline-sm font-semibold text-on-surface">Proyectos registrados recientemente</h2>
            </div>
            <Link href="/proyectos" className={`${enlace} shrink-0 text-sm`}>Ver todos</Link>
          </header>
          {proyectos.isPending ? <p className="py-5 text-sm text-on-surface-variant">Cargando proyectos…</p>
            : proyectos.isError ? <ErrorSeccion mensaje={proyectos.error.message} reintentar={() => proyectos.refetch()} />
              : ultimosProyectos.length === 0 ? <EstadoVacio titulo="No hay proyectos registrados" />
                : (
                  <ul className="divide-y divide-outline-variant/40">
                    {ultimosProyectos.map((p) => (
                      <li key={p.id} className="flex items-start justify-between gap-3 py-3 first:pt-0 last:pb-0">
                        <div className="min-w-0">
                          <Link href={`/proyectos/${p.id}`} className={`${enlace} line-clamp-2 text-sm`}>{p.titulo}</Link>
                          <p className="mt-1 truncate text-xs text-on-surface-variant">{p.codigo ?? "Sin código"} · {p.programa} · {p.investigadorPrincipal}</p>
                        </div>
                        <EstadoPildora estado={p.estado} />
                      </li>
                    ))}
                  </ul>
                )}
        </section>
      </div>

      <div className="grid gap-space-lg xl:grid-cols-2">
        <ListaHitos titulo="Próximos hitos" contexto="Vencen en los próximos 30 días" hitos={t.hitosProximos} tipo="proximo" />
        <ListaHitos titulo="Hitos vencidos" contexto="Requieren una acción de seguimiento" hitos={t.hitosVencidos} tipo="vencido" />
      </div>

      <section className={tarjeta} aria-labelledby="titulo-avances">
        <header className="mb-4 flex items-start justify-between gap-3">
          <div>
            <p className="font-label-sm text-label-sm uppercase text-secondary">Seguimiento</p>
            <h2 id="titulo-avances" className="mt-1 font-headline-sm text-headline-sm font-semibold text-on-surface">Avances pendientes de validación</h2>
          </div>
          <Link href="/avances-por-validar" className={`${enlace} shrink-0 text-sm`}>Ir a validaciones</Link>
        </header>
        {t.avancesPendientes.length === 0 ? <EstadoVacio titulo="Sin avances pendientes" />
          : (
            <ul className="grid gap-3 sm:grid-cols-2 xl:grid-cols-3">
              {t.avancesPendientes.map((a) => (
                <li key={a.programa} className="flex items-center justify-between gap-3 rounded-lg bg-surface-container-low px-4 py-3">
                  <span className="text-sm text-on-surface">{a.programa}</span>
                  <span className="rounded-full bg-primary-fixed/40 px-2.5 py-1 text-xs font-semibold text-primary">{a.cantidad} pendientes</span>
                </li>
              ))}
            </ul>
          )}
      </section>
    </div>
  );
}

function ListaHitos({ titulo, contexto, hitos, tipo }: { titulo: string; contexto: string; hitos: Hito[]; tipo: "proximo" | "vencido" }) {
  return (
    <section className={tarjeta} aria-label={titulo}>
      <header className="mb-4">
        <p className="font-label-sm text-label-sm uppercase text-secondary">Cronograma</p>
        <h2 className="mt-1 font-headline-sm text-headline-sm font-semibold text-on-surface">{titulo}</h2>
        <p className="mt-1 text-sm text-on-surface-variant">{contexto}</p>
      </header>
      {hitos.length === 0 ? <EstadoVacio titulo={tipo === "proximo" ? "Sin hitos próximos" : "Sin hitos vencidos"} />
        : (
          <ul className="divide-y divide-outline-variant/40">
            {hitos.slice(0, 5).map((h) => (
              <li key={h.hitoId} className="flex items-start justify-between gap-3 py-3 first:pt-0 last:pb-0">
                <div className="min-w-0">
                  <p className="text-sm font-medium text-on-surface">{h.titulo}</p>
                  <Link href={`/proyectos/${h.proyectoId}`} className={`${enlace} mt-1 inline-block text-xs`}>
                    {h.codigo ?? "Proyecto"} · {h.programa}
                  </Link>
                  {h.responsable && <p className="mt-1 truncate text-xs text-on-surface-variant">{h.responsable}</p>}
                </div>
                <span className={`shrink-0 text-right text-xs font-semibold ${tipo === "vencido" ? "text-error" : "text-on-surface-variant"}`}>
                  {fechaCorta(h.fechaLimite)}
                </span>
              </li>
            ))}
          </ul>
        )}
    </section>
  );
}
