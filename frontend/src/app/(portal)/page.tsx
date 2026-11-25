import type { Metadata } from "next";
import Image from "next/image";
import Link from "next/link";
import logoCi2dt2 from "@/assets/img/LOGO_CI2DT2.png";
import { apiPublica, type PaginaPublica, type ProyectoPublico, type ResumenPortal } from "@/lib/portal";
import { fechaCorta, formatoMonto } from "@/lib/formato";
import { contenidoInstitucional as c } from "./contenido-institucional";

// HU-25: renderizado estático con revalidación incremental cada 10 minutos.
export const revalidate = 600;

export const metadata: Metadata = {
  title: `${c.nombre} · ${c.institucion}`,
  description: `${c.nombreCompleto} de la ${c.institucion}: programas de investigación, proyectos y convocatorias abiertas.`,
  openGraph: { title: c.nombre, description: c.nombreCompleto, locale: "es_CO", type: "website" },
};

function TarjetaCifra({
  categoria,
  icono,
  valor,
  titulo,
  subtitulo,
}: {
  categoria: string;
  icono: string;
  valor: number;
  titulo: string;
  subtitulo: string;
}) {
  return (
    <article className="p-space-lg bg-surface-container-lowest rounded-xl shadow-sm hover:shadow-md transition-shadow">
      <div className="flex items-center justify-between mb-space-xs">
        <span className="font-label-sm text-label-sm text-on-surface-variant uppercase">{categoria}</span>
        <span className="material-symbols-outlined text-secondary text-[22px]" aria-hidden="true">
          {icono}
        </span>
      </div>
      <p className="font-headline-xl text-headline-lg md:text-headline-xl text-primary font-bold tracking-tight">
        {valor}
      </p>
      <p className="font-body-sm text-body-sm text-on-surface font-semibold mt-space-xs">{titulo}</p>
      <p className="font-label-sm text-label-sm text-on-surface-variant">{subtitulo}</p>
    </article>
  );
}

const pilaresEstrategicos = [
  {
    icono: "biotech",
    etiqueta: "Pilar 01 • Ciencia Básica",
    titulo: "Investigación Científica de Frontera",
    descripcion:
      "Generación de nuevo conocimiento riguroso en biocatálisis, bioingeniería de tejidos, bioinformática de precisión y computación de alto rendimiento con indexación internacional.",
    accion: "Explorar proyectos de investigación",
    href: "/portal/proyectos",
    fondoIcono: "bg-primary-fixed/30 text-primary group-hover:bg-primary group-hover:text-on-primary",
  },
  {
    icono: "precision_manufacturing",
    etiqueta: "Pilar 02 • Maduración Tecnológica",
    titulo: "Desarrollo Tecnológico y Prototipado",
    descripcion:
      "Escalamiento de invenciones desde nivel de prueba de concepto (TRL 3) hasta prototipos integrados y operacionales en entorno relevante (TRL 7) en bancos de prueba especializados.",
    accion: "Ver programas y capacidades",
    href: "/#programas",
    fondoIcono: "bg-secondary-container/40 text-secondary group-hover:bg-secondary group-hover:text-on-secondary",
  },
  {
    icono: "hub",
    etiqueta: "Pilar 03 • Impacto Socio-Productivo",
    titulo: "Transferencia y Alianzas Empresariales",
    descripcion:
      "Resolución de retos industriales mediante licenciamiento de tecnología, spin-offs universitarias y estructuración de proyectos de coinversión bajo beneficios tributarios de Minciencias.",
    accion: "Conocer modelo de cooperación",
    href: "/#aliados",
    fondoIcono: "bg-primary-fixed/40 text-primary-container group-hover:bg-primary-container group-hover:text-on-primary",
  },
];

const actoresCooperacion = [
  {
    icono: "factory",
    titulo: "Sector Productivo e Industrial",
    descripcion:
      "Empresas, gremios y cadenas de valor agroindustriales que demandan soluciones tecnológicas a la medida, I+D por encargo y escalamiento de procesos.",
    fondoIcono: "bg-primary-fixed/30 text-primary",
  },
  {
    icono: "account_balance",
    titulo: "Sector Público y Gubernamental",
    descripcion:
      "Ministerios, entidades territoriales y convocatorias del Sistema General de Regalías (SGR) para proyectos de alto impacto territorial y comunitario.",
    fondoIcono: "bg-secondary-container/40 text-secondary",
  },
  {
    icono: "school",
    titulo: "Academia y Centros Tecnológicos",
    descripcion:
      "Redes universitarias, grupos de investigación nacionales y centros de biotecnología para coautoría científica y movilidad de talento de posgrado.",
    fondoIcono: "bg-primary-fixed/40 text-primary-container",
  },
  {
    icono: "public",
    titulo: "Cooperación Internacional",
    descripcion:
      "Organismos multilaterales, agencias de cooperación y consorcios globales orientados al cambio climático, bioeconomía y ciencia abierta.",
    fondoIcono: "bg-surface-container text-primary",
  },
];

export default async function Inicio() {
  const [resumen, proyectosPagina] = await Promise.all([
    apiPublica<ResumenPortal>("/resumen"),
    apiPublica<PaginaPublica<ProyectoPublico>>("/proyectos?pagina=1"),
  ]);

  const proyectosPreview = proyectosPagina?.items.slice(0, 5) ?? [];
  const hayMasProyectos = (proyectosPagina?.total ?? 0) > 5;
  const convocatoriasPreview = resumen?.convocatoriasAbiertas.slice(0, 5) ?? [];
  const hayMasConvocatorias = (resumen?.convocatoriasAbiertas.length ?? 0) > 5;

  return (
    <div className="flex flex-col w-full">
      {/* HERO SECTION CON LOGO OFICIAL Y KPIS */}
      <section className="relative w-full bg-gradient-to-b from-surface-container-low via-surface to-surface px-margin md:px-margin-tablet lg:px-margin-desktop pt-space-xl pb-space-2xl overflow-hidden">
        <div
          aria-hidden="true"
          className="absolute -top-24 right-1/4 w-96 h-96 bg-primary-fixed/20 rounded-full blur-3xl pointer-events-none"
        />
        <div
          aria-hidden="true"
          className="absolute top-1/3 -left-20 w-80 h-80 bg-secondary-container/25 rounded-full blur-2xl pointer-events-none"
        />

        <div className="max-w-7xl mx-auto flex flex-col items-center text-center relative z-10">
          <div className="flex items-center justify-center mb-space-lg transform hover:scale-105 transition-transform duration-300">
            <div className="p-space-sm bg-surface-container-lowest shadow-md rounded-xl flex items-center justify-center">
              <Image
                src={logoCi2dt2}
                alt="Logo CI²DT² - Universidad de Caldas"
                width={128}
                height={128}
                className="h-24 sm:h-28 md:h-32 w-auto object-contain drop-shadow-sm"
                priority
              />
            </div>
          </div>

          <h1 className="font-headline-xl text-headline-xl-mobile md:text-headline-xl text-primary max-w-4xl tracking-tight mb-space-md">
            Ciencia Aplicada, Innovación y Transferencia que Transforman el Territorio
          </h1>

          <p className="font-body-lg text-body-md md:text-body-lg text-on-surface-variant max-w-3xl mb-space-xl">
            {c.proposito ||
              "Articulamos la excelencia investigativa de la Universidad de Caldas con las necesidades del sector productivo, la biotecnología de vanguardia y la transformación digital sostenible en Colombia."}
          </p>

          <div className="flex flex-col sm:flex-row items-center justify-center gap-space-md w-full sm:w-auto mb-space-2xl">
            <Link
              href="/portal/proyectos"
              className="w-full sm:w-auto inline-flex items-center justify-center gap-space-xs px-space-lg py-space-sm rounded-lg bg-primary text-on-primary font-headline-sm text-body-sm hover:bg-primary-container transition-colors shadow-sm"
            >
              <span>Conocer los proyectos</span>
              <span className="material-symbols-outlined text-[18px]" aria-hidden="true">arrow_forward</span>
            </Link>
            <Link
              href="/portal/convocatorias"
              className="w-full sm:w-auto inline-flex items-center justify-center px-space-lg py-space-sm rounded-lg bg-surface-container-lowest border border-outline-variant text-primary font-headline-sm text-body-sm hover:bg-surface-container transition-colors"
            >
              Convocatorias abiertas
            </Link>
          </div>

          {resumen ? (
            <div
              aria-label="Cifras clave del centro"
              className="grid grid-cols-2 lg:grid-cols-4 gap-space-md md:gap-gutter-desktop w-full text-left"
            >
              <TarjetaCifra
                categoria="Estructura Base"
                icono="account_tree"
                valor={resumen.cifras.programas}
                titulo="Programas de Investigación"
                subtitulo="Vanguardia biotecnológica e IA"
              />
              <TarjetaCifra
                categoria="Producción I+D"
                icono="science"
                valor={resumen.cifras.proyectosEnEjecucion}
                titulo="Proyectos en Ejecución"
                subtitulo="Convocatorias activas y regalías"
              />
              <TarjetaCifra
                categoria="Resultados"
                icono="task_alt"
                valor={resumen.cifras.proyectosFinalizados}
                titulo="Proyectos Finalizados"
                subtitulo="Transferencia y cierre exitoso"
              />
              <TarjetaCifra
                categoria="Articulación"
                icono="handshake"
                valor={resumen.cifras.aliados}
                titulo="Aliados y Empresas"
                subtitulo="Red regional e internacional"
              />
            </div>
          ) : (
            <p className="w-full max-w-xl rounded-xl bg-surface-container-lowest px-space-lg py-space-md text-body-sm text-on-surface-variant shadow-sm border border-outline-variant/30">
              La información estadística del centro no está disponible en este momento.
            </p>
          )}
        </div>
      </section>

      {/* SECCIÓN 2: ¿QUÉ HACEMOS EN EL CI²DT²? (3 PILARES ESTRATÉGICOS) */}
      <section className="w-full px-margin md:px-margin-tablet lg:px-margin-desktop py-space-2xl bg-surface" id="mision-pilares">
        <div className="max-w-7xl mx-auto flex flex-col gap-space-xl">
          <div className="flex flex-col md:flex-row md:items-end justify-between gap-space-md">
            <div>
              <span className="font-label-md text-label-md text-secondary font-semibold uppercase tracking-wider">
                Misión y Capacidades Operativas
              </span>
              <h2 className="font-headline-lg text-headline-lg-mobile md:text-headline-lg text-primary tracking-tight mt-space-xs">
                ¿Qué Hacemos en el CI²DT²?
              </h2>
            </div>
            <p className="font-body-md text-body-md text-on-surface-variant max-w-xl">
              Estructuramos un modelo tridimensional que asegura que la producción científica del laboratorio trascienda a patentes, prototipos validados y soluciones adoptadas por el sector productivo regional.
            </p>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-3 gap-gutter-desktop">
            {pilaresEstrategicos.map((pilar) => (
              <article
                key={pilar.etiqueta}
                className="group p-space-xl rounded-xl bg-surface-container-lowest shadow-sm hover:shadow-xl transition-all duration-300 flex flex-col justify-between"
              >
                <div>
                  <div
                    className={`w-14 h-14 rounded-xl flex items-center justify-center mb-space-lg transition-colors ${pilar.fondoIcono}`}
                  >
                    <span className="material-symbols-outlined text-[32px]" aria-hidden="true">
                      {pilar.icono}
                    </span>
                  </div>
                  <div className="inline-block px-space-sm py-0.5 rounded bg-surface-container font-label-sm text-label-sm text-on-surface-variant uppercase font-semibold mb-space-xs">
                    {pilar.etiqueta}
                  </div>
                  <h3 className="font-headline-md text-headline-md text-primary mb-space-sm">{pilar.titulo}</h3>
                  <p className="font-body-md text-body-md text-on-surface-variant">{pilar.descripcion}</p>
                </div>
                <Link
                  href={pilar.href}
                  className="mt-space-lg pt-space-md flex items-center gap-space-xs text-primary font-headline-sm text-body-sm group-hover:text-secondary transition-colors"
                >
                  <span>{pilar.accion}</span>
                  <span className="material-symbols-outlined text-[18px]" aria-hidden="true">
                    arrow_forward
                  </span>
                </Link>
              </article>
            ))}
          </div>
        </div>
      </section>

      {/* OBJETIVOS ESTRATÉGICOS (SI ESTÁN CONFIGURADOS) */}
      {c.objetivosEstrategicos.length > 0 && (
        <section className="w-full px-margin md:px-margin-tablet lg:px-margin-desktop py-space-xl bg-surface">
          <div className="max-w-7xl mx-auto">
            <h2 className="font-headline-lg text-headline-lg-mobile md:text-headline-lg text-primary tracking-tight">
              Objetivos estratégicos
            </h2>
            <ol className="mt-space-lg grid gap-space-md md:grid-cols-2">
              {c.objetivosEstrategicos.map((o, i) => (
                <li
                  key={o}
                  className="rounded-xl border border-outline-variant/30 bg-surface-container-lowest p-space-lg text-body-sm text-on-surface shadow-sm"
                >
                  <strong className="text-primary">{i + 1}.</strong> {o}
                </li>
              ))}
            </ol>
          </div>
        </section>
      )}

      {/* SECCIÓN 3: PROGRAMAS DE INVESTIGACIÓN (BENTO GRID DINÁMICO) */}
      <section className="w-full px-margin md:px-margin-tablet lg:px-margin-desktop py-space-2xl bg-surface-container-low" id="programas">
        <div className="max-w-7xl mx-auto flex flex-col gap-space-xl">
          <div className="flex flex-col md:flex-row md:items-end justify-between gap-space-md">
            <div>
              <h2 className="font-headline-lg text-headline-lg-mobile md:text-headline-lg text-primary tracking-tight">
                Nuestros Programas de Investigación
              </h2>
            </div>
            <p className="font-body-md text-body-md text-on-surface-variant max-w-lg">
              Líneas interdisciplinares reconocidas institucionalmente para la formulación de proyectos, semilleros de investigación y captación de fondos internacionales.
            </p>
          </div>

          {resumen && resumen.programas.length > 0 ? (
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-gutter-desktop">
              {resumen.programas.map((p, idx) => {
                const esUltimoCinco = resumen.programas.length === 5 && idx === 4;
                return (
                  <article
                    key={p.id}
                    className={`bg-surface-container-lowest p-space-lg rounded-xl shadow-sm hover:shadow-lg transition-all flex flex-col justify-between ${
                      esUltimoCinco ? "md:col-span-2 lg:col-span-2" : ""
                    }`}
                  >
                    <div>
                      <div className="flex items-center justify-between gap-space-xs mb-space-sm">
                        <span className="font-label-md text-label-md font-semibold text-primary px-space-sm py-space-xs bg-primary-fixed/40 rounded uppercase">
                          {p.slug}
                        </span>
                        <span className="inline-flex items-center gap-1 font-label-sm text-label-sm text-secondary font-semibold">
                          <span className="w-2 h-2 rounded-full bg-secondary" aria-hidden="true" />
                          Activo
                        </span>
                      </div>
                      <h3 className="font-headline-sm text-headline-sm text-primary mb-space-xs">{p.nombre}</h3>
                      {p.descripcion && (
                        <p className="font-body-sm text-body-sm text-on-surface-variant mb-space-md line-clamp-3">
                          {p.descripcion}
                        </p>
                      )}
                      <div className="flex flex-wrap gap-space-xs mb-space-md">
                        <span className="px-space-xs py-0.5 rounded bg-surface-container font-label-sm text-label-sm text-on-surface-variant">
                          {p.proyectosPublicos} proyecto(s) público(s)
                        </span>
                        {p.coordinador && (
                          <span className="px-space-xs py-0.5 rounded bg-surface-container font-label-sm text-label-sm text-on-surface-variant">
                            Coordina: {p.coordinador}
                          </span>
                        )}
                      </div>
                    </div>

                    <div className={esUltimoCinco ? "flex items-center justify-end" : ""}>
                      <Link
                        href={`/portal/programas/${p.slug}`}
                        className={`${
                          esUltimoCinco ? "inline-flex gap-space-xs px-space-lg" : "w-full inline-flex justify-between px-space-md"
                        } items-center py-space-sm rounded-lg bg-surface-container hover:bg-surface-variant text-primary font-headline-sm text-body-sm transition-colors`}
                      >
                        <span>Conocer Líneas y Equipo</span>
                        <span className="material-symbols-outlined text-[18px]" aria-hidden="true">
                          chevron_right
                        </span>
                      </Link>
                    </div>
                  </article>
                );
              })}
            </div>
          ) : (
            <p className="rounded-xl bg-surface-container-lowest p-space-lg text-body-sm text-on-surface-variant shadow-sm">
              Los programas de investigación no están disponibles en este momento.
            </p>
          )}
        </div>
      </section>

      {/* SERVICIOS Y CAPACIDADES (SI ESTÁN CONFIGURADOS) */}
      {c.servicios.length > 0 && (
        <section className="w-full px-margin md:px-margin-tablet lg:px-margin-desktop py-space-xl bg-surface">
          <div className="max-w-7xl mx-auto">
            <h2 className="font-headline-lg text-headline-lg-mobile md:text-headline-lg text-primary tracking-tight">
              Servicios y capacidades
            </h2>
            <ul className="mt-space-lg grid list-disc gap-space-sm pl-5 text-body-sm text-on-surface-variant md:grid-cols-2">
              {c.servicios.map((s) => (
                <li key={s}>{s}</li>
              ))}
            </ul>
          </div>
        </section>
      )}

      {/* SECCIÓN 4: VISTA PREVIA DE PROYECTOS PÚBLICOS (MÁXIMO 5) */}
      <section className="w-full px-margin md:px-margin-tablet lg:px-margin-desktop py-space-2xl bg-surface" id="proyectos">
        <div className="max-w-7xl mx-auto flex flex-col gap-space-xl">
          <div className="flex flex-col md:flex-row md:items-end justify-between gap-space-md">
            <div>
              <span className="font-label-md text-label-md text-secondary font-semibold uppercase tracking-wider">
                Portafolio Público
              </span>
              <h2 className="font-headline-lg text-headline-lg-mobile md:text-headline-lg text-primary tracking-tight mt-space-xs">
                Proyectos Destacados
              </h2>
            </div>
            <p className="font-body-md text-body-md text-on-surface-variant max-w-lg">
              Iniciativas aprobadas, en ejecución y finalizadas con impacto científico, tecnológico y territorial.
            </p>
          </div>

          {!proyectosPagina ? (
            <div className="rounded-xl bg-surface-container-lowest p-space-lg shadow-sm border border-outline-variant/30">
              <p className="font-body-md text-body-md text-on-surface-variant">
                Los proyectos públicos no están disponibles en este momento.
              </p>
            </div>
          ) : proyectosPreview.length === 0 ? (
            <div className="rounded-xl bg-surface-container-lowest p-space-lg shadow-sm border border-outline-variant/30">
              <p className="font-body-md text-body-md text-on-surface-variant">
                No hay proyectos públicos registrados en este momento.
              </p>
            </div>
          ) : (
            <>
              <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-gutter-desktop">
                {proyectosPreview.map((p) => (
                  <Link key={p.codigo} href={`/portal/proyectos/${p.slug}`} className="group block h-full">
                    <article className="h-full bg-surface-container-lowest p-space-lg rounded-xl shadow-sm border border-outline-variant/30 group-hover:shadow-lg transition-all flex flex-col justify-between gap-space-md">
                      <div>
                        <div className="flex items-center justify-between gap-space-xs mb-space-xs">
                          <span className="font-label-sm text-label-sm font-semibold text-primary px-space-sm py-0.5 bg-primary-fixed/40 rounded">
                            {p.codigo}
                          </span>
                          <span className="font-label-sm text-label-sm text-secondary font-semibold">{p.estado}</span>
                        </div>
                        <h3 className="font-headline-sm text-headline-sm text-primary group-hover:text-secondary transition-colors">
                          {p.titulo}
                        </h3>
                        <p className="font-label-sm text-label-sm text-on-surface-variant mt-space-xs">{p.programa}</p>
                        {p.resumen && (
                          <p className="font-body-sm text-body-sm text-on-surface-variant line-clamp-3 mt-space-sm">
                            {p.resumen}
                          </p>
                        )}
                      </div>
                      <div className="pt-space-sm border-t border-outline-variant/30 flex flex-col gap-space-xs">
                        <p className="font-body-sm text-body-sm text-on-surface-variant">
                          <strong className="text-on-surface">Investigador:</strong> {p.investigadorPrincipal}
                        </p>
                        <div className="flex items-center justify-between text-primary font-headline-sm text-body-sm group-hover:text-secondary transition-colors">
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

              {hayMasProyectos && (
                <div className="flex justify-center">
                  <Link
                    href="/portal/proyectos"
                    className="inline-flex items-center gap-space-xs px-space-lg py-space-sm rounded-lg bg-primary text-on-primary font-headline-sm text-body-sm hover:bg-primary-container transition-colors shadow-sm"
                  >
                    <span>Ver más proyectos</span>
                    <span className="material-symbols-outlined text-[18px]" aria-hidden="true">
                      arrow_forward
                    </span>
                  </Link>
                </div>
              )}
            </>
          )}
        </div>
      </section>

      {/* SECCIÓN 5: VISTA PREVIA DE CONVOCATORIAS ABIERTAS (MÁXIMO 5, AUTOCONTENIDAS) */}
      <section className="w-full px-margin md:px-margin-tablet lg:px-margin-desktop py-space-2xl bg-surface-container-low" id="convocatorias">
        <div className="max-w-7xl mx-auto flex flex-col gap-space-xl">
          <div className="flex flex-col md:flex-row md:items-end justify-between gap-space-md">
            <div>
              <span className="font-label-md text-label-md text-secondary font-semibold uppercase tracking-wider">
                Oportunidades de Financiación
              </span>
              <h2 className="font-headline-lg text-headline-lg-mobile md:text-headline-lg text-primary tracking-tight mt-space-xs">
                Convocatorias Abiertas
              </h2>
            </div>
            <p className="font-body-md text-body-md text-on-surface-variant max-w-lg">
              Información pública de convocatorias vigentes. La postulación se realiza exclusivamente tras iniciar sesión en el sistema.
            </p>
          </div>

          {!resumen || convocatoriasPreview.length === 0 ? (
            <div className="rounded-xl bg-surface-container-lowest p-space-lg shadow-sm border border-outline-variant/30">
              <p className="font-body-md text-body-md text-on-surface-variant">
                No hay convocatorias abiertas en este momento.
              </p>
            </div>
          ) : (
            <>
              <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-gutter-desktop">
                {convocatoriasPreview.map((cv) => (
                  <article
                    key={cv.id}
                    className="bg-surface-container-lowest p-space-lg rounded-xl shadow-sm border border-outline-variant/30 flex flex-col justify-between gap-space-md"
                  >
                    <div className="flex flex-col gap-space-xs">
                      <div className="flex flex-wrap items-center justify-between gap-space-xs mb-space-xs">
                        <span className="px-space-sm py-0.5 rounded bg-primary-fixed/40 font-label-sm text-label-sm text-primary font-semibold uppercase">
                          {cv.tipoConvocatoria}
                        </span>
                        <span className="inline-flex items-center gap-1 font-label-sm text-label-sm text-secondary font-semibold">
                          <span className="w-2 h-2 rounded-full bg-secondary" aria-hidden="true" />
                          Abierta
                        </span>
                      </div>
                      <h3 className="font-headline-sm text-headline-sm text-primary">{cv.nombre}</h3>
                      {cv.numero && (
                        <p className="font-label-sm text-label-sm text-on-surface-variant">
                          Código / Número: {cv.numero}
                        </p>
                      )}
                      {cv.entidadConvocante && (
                        <p className="font-body-sm text-body-sm text-on-surface-variant mt-space-xs">
                          <strong className="text-on-surface">Entidad:</strong> {cv.entidadConvocante}
                        </p>
                      )}
                      <p className="font-body-sm text-body-sm text-on-surface-variant">
                        <strong className="text-on-surface">Plataforma:</strong> {cv.plataforma}
                      </p>
                    </div>

                    <dl className="pt-space-sm border-t border-outline-variant/30 grid grid-cols-2 gap-space-sm text-body-sm">
                      <div>
                        <dt className="font-label-sm text-label-sm text-on-surface-variant uppercase">Apertura</dt>
                        <dd className="font-body-sm text-on-surface font-medium">{fechaCorta(cv.fechaApertura)}</dd>
                      </div>
                      <div>
                        <dt className="font-label-sm text-label-sm text-on-surface-variant uppercase">Cierre</dt>
                        <dd className="font-body-sm text-primary font-semibold">{fechaCorta(cv.fechaCierre)}</dd>
                      </div>
                      {cv.montoMaximo && (
                        <div className="col-span-2">
                          <dt className="font-label-sm text-label-sm text-on-surface-variant uppercase">
                            Financiación máxima
                          </dt>
                          <dd className="font-body-sm text-on-surface font-semibold">
                            Hasta {formatoMonto(cv.montoMaximo, cv.moneda)}
                          </dd>
                        </div>
                      )}
                    </dl>
                  </article>
                ))}
              </div>

              {hayMasConvocatorias && (
                <div className="flex justify-center">
                  <Link
                    href="/portal/convocatorias"
                    className="inline-flex items-center gap-space-xs px-space-lg py-space-sm rounded-lg bg-primary text-on-primary font-headline-sm text-body-sm hover:bg-primary-container transition-colors shadow-sm"
                  >
                    <span>Ver más convocatorias</span>
                    <span className="material-symbols-outlined text-[18px]" aria-hidden="true">
                      arrow_forward
                    </span>
                  </Link>
                </div>
              )}
            </>
          )}
        </div>
      </section>

      {/* SECCIÓN 5: MODELO DE COOPERACIÓN Y ARTICULACIÓN MULTIACTOR */}
      <section className="w-full px-margin md:px-margin-tablet lg:px-margin-desktop py-space-xl bg-surface" id="aliados">
        <div className="max-w-7xl mx-auto flex flex-col items-center gap-space-xl text-center">
          <div className="max-w-3xl mx-auto flex flex-col items-center">
            <h2 className="font-headline-lg text-headline-lg-mobile md:text-headline-lg text-primary mt-space-xs tracking-tight">
              Modelo de Cooperación y Articulación Multiactor
            </h2>
            <p className="font-body-md text-body-md text-on-surface-variant mt-space-sm">
              Articulamos sinergias multisectoriales para dinamizar la investigación aplicada, el co-desarrollo tecnológico y la transferencia de valor hacia la sociedad y las cadenas productivas.
            </p>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-gutter-desktop w-full text-left">
            {actoresCooperacion.map((actor) => (
              <article
                key={actor.titulo}
                className="p-space-lg bg-surface-container-lowest rounded-xl shadow-sm border border-outline-variant/30 flex flex-col justify-between hover:shadow-md transition-shadow"
              >
                <div>
                  <div className={`w-12 h-12 rounded-xl flex items-center justify-center mb-space-md ${actor.fondoIcono}`}>
                    <span className="material-symbols-outlined text-[28px]" aria-hidden="true">
                      {actor.icono}
                    </span>
                  </div>
                  <h3 className="font-headline-sm text-headline-sm text-primary mb-space-xs">{actor.titulo}</h3>
                  <p className="font-body-sm text-body-sm text-on-surface-variant">{actor.descripcion}</p>
                </div>
              </article>
            ))}
          </div>
        </div>
      </section>
    </div>
  );
}
