import Image from "next/image";
import Link from "next/link";
import logoCi2dt2 from "@/assets/img/LOGO_CI2DT2.png";
import { contenidoInstitucional as c } from "./contenido-institucional";
import { NavPortal } from "./nav-portal";
import { BotonVolverArriba } from "./boton-volver-arriba";

/** Layout del portal público institucional (EP-10). */
export default function LayoutPortal({ children }: LayoutProps<"/">) {
  return (
    <div className="flex min-h-screen flex-col bg-surface font-body-md text-body-md text-on-surface antialiased">
      <a
        href="#contenido-principal"
        className="sr-only focus:not-sr-only focus:fixed focus:top-3 focus:left-3 focus:z-[60] focus:rounded-lg focus:bg-primary focus:px-4 focus:py-2 focus:text-on-primary focus:shadow-lg"
      >
        Saltar al contenido principal
      </a>

      <header className="fixed top-0 left-0 right-0 z-50 bg-surface-container-lowest/90 backdrop-blur-xl shadow-[0_1px_8px_rgba(21,71,52,0.06)]">
        <div className="h-20 w-full px-margin md:px-margin-tablet lg:px-margin-desktop flex items-center justify-between gap-space-md">
          <Link href="/" className="flex items-center gap-space-md flex-shrink-0">
            <Image
              src={logoCi2dt2}
              alt="Logo CI²DT²"
              width={40}
              height={40}
              className="h-8 w-auto object-contain"
              priority
            />
            <div className="flex flex-col">
              <span className="font-headline-sm text-headline-sm text-primary tracking-tight leading-none">
                {c.nombre}
              </span>
              <span className="font-label-sm text-label-sm text-on-surface-variant tracking-wider uppercase mt-space-xs hidden sm:inline-block">
                {c.institucion}
              </span>
            </div>
          </Link>

          <NavPortal />

          <div className="hidden sm:flex items-center gap-space-sm">
            <Link
              href="/login"
              className="inline-flex items-center justify-center px-space-md py-space-sm rounded-lg bg-primary text-on-primary font-headline-sm text-body-sm hover:bg-primary-container hover:text-on-primary-container transition-all shadow-[0_2px_4px_rgba(21,71,52,0.12)]"
            >
              Iniciar Sesión
            </Link>
          </div>
        </div>
      </header>

      <main id="contenido-principal" tabIndex={-1} className="w-full pt-20 bg-surface flex-1">
        {children}
      </main>

      <footer className="w-full bg-surface-container-low text-on-surface mt-space-2xl" id="contacto">
        <div className="w-full px-margin md:px-margin-tablet lg:px-margin-desktop py-space-2xl">
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-gutter-desktop">
            <div className="flex flex-col gap-space-sm">
              <div className="flex items-center gap-space-sm">
                <Image
                  src={logoCi2dt2}
                  alt=""
                  width={32}
                  height={32}
                  className="h-8 w-auto object-contain"
                />
                <span className="font-headline-sm text-headline-sm text-primary">{c.nombre}</span>
              </div>
              <p className="font-body-sm text-body-sm text-on-surface-variant">
                {c.nombreCompleto}. Articulando rigor biológico, ecosistémico y biotecnología avanzada al servicio del conocimiento científico.
              </p>
              <span className="font-label-md text-label-md text-primary font-semibold">
                {c.institucion} • Manizales, Colombia
              </span>
            </div>

            <div className="flex flex-col gap-space-sm">
              <h3 className="font-headline-sm text-headline-sm text-primary">Ecosistema Científico</h3>
              <ul className="flex flex-col gap-space-xs font-body-sm text-body-sm text-on-surface-variant">
                <li>
                  <Link className="hover:text-primary transition-colors" href="/#programas">
                    Líneas de Investigación
                  </Link>
                </li>
                <li>
                  <Link className="hover:text-primary transition-colors" href="/portal/proyectos">
                    Portafolio de Proyectos
                  </Link>
                </li>
                <li>
                  <Link className="hover:text-primary transition-colors" href="/portal/convocatorias">
                    Convocatorias Abiertas
                  </Link>
                </li>
                <li>
                  <Link className="hover:text-primary transition-colors" href="/#aliados">
                    Red de Cooperación e Innovación
                  </Link>
                </li>
              </ul>
            </div>

            <div className="flex flex-col gap-space-sm">
              <h3 className="font-headline-sm text-headline-sm text-primary">Gobernanza y Marco Legal</h3>
              <ul className="flex flex-col gap-space-xs font-body-sm text-body-sm text-on-surface-variant">
                <li>
                  <Link className="hover:text-primary transition-colors" href="/politica-datos">
                    Tratamiento de Datos (Ley 1581 de 2012)
                  </Link>
                </li>
                <li>
                  <Link className="hover:text-primary transition-colors" href="/#mision-pilares">
                    Misión y Pilares Estratégicos
                  </Link>
                </li>
                <li>
                  <Link className="hover:text-primary transition-colors" href="/login">
                    Acceso al Sistema de Información
                  </Link>
                </li>
              </ul>
            </div>

            <div className="flex flex-col gap-space-sm">
              <h3 className="font-headline-sm text-headline-sm text-primary">Contacto Institucional</h3>
              <p className="font-body-sm text-body-sm text-on-surface-variant">
                Campus Palogrande / Campus Bicentenario
                <br />
                Carrera 23 # 58-65, Manizales - Caldas
                {c.contacto.correo && (
                  <>
                    <br />
                    Email:{" "}
                    <a href={`mailto:${c.contacto.correo}`} className="hover:text-primary underline">
                      {c.contacto.correo}
                    </a>
                  </>
                )}
                {c.contacto.telefono && (
                  <>
                    <br />
                    Tel: {c.contacto.telefono}
                  </>
                )}
              </p>
              <div className="flex flex-wrap items-center gap-space-sm mt-space-xs">
                <span className="px-space-sm py-space-xs rounded bg-surface-container font-label-sm text-label-sm text-on-surface-variant">
                  NIT: 890.801.059-1
                </span>
                <span className="px-space-sm py-space-xs rounded bg-secondary-container font-label-sm text-label-sm text-on-secondary-container font-semibold">
                  Vigilada Mineducación
                </span>
              </div>
            </div>
          </div>

          <div className="mt-space-xl pt-space-lg border-t border-outline-variant/30 flex flex-col sm:flex-row items-center justify-between gap-space-md">
            <p className="font-body-sm text-body-sm text-on-surface-variant">
              © 2026 {c.institucion} • Vicerrectoría de Investigaciones y Posgrados • {c.nombre}
            </p>
            <div className="flex items-center gap-space-md">
              <Link className="font-label-md text-label-md text-on-surface-variant hover:text-primary transition-colors" href="/politica-datos">
                Política de Privacidad
              </Link>
              <Link className="font-label-md text-label-md text-on-surface-variant hover:text-primary transition-colors" href="/portal/proyectos">
                Proyectos Públicos
              </Link>
              <Link className="font-label-md text-label-md text-on-surface-variant hover:text-primary transition-colors" href="/login">
                Ingresar
              </Link>
            </div>
          </div>
        </div>
      </footer>
      <BotonVolverArriba />
    </div>
  );
}
