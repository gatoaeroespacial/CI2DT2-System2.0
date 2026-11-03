import type { Metadata } from "next";
import Image from "next/image";
import { Suspense } from "react";
import logoCi2dt2 from "@/assets/img/LOGO_CI2DT2.png";
import { FormularioLogin } from "./formulario-login";

export const metadata: Metadata = { title: "Iniciar sesión · CI²DT²" };

type IconoCaracteristica = "proyectos" | "colaboracion" | "indicadores" | "soporte";

function Icono({ nombre, className = "" }: { nombre: IconoCaracteristica; className?: string }) {
  const paths = {
    proyectos: (
      <>
        <path d="M4 5.5A1.5 1.5 0 0 1 5.5 4h13A1.5 1.5 0 0 1 20 5.5v13a1.5 1.5 0 0 1-1.5 1.5h-13A1.5 1.5 0 0 1 4 18.5v-13Z" />
        <path d="m8 12 2.5 2.5L16 9" />
      </>
    ),
    colaboracion: (
      <>
        <circle cx="9" cy="8" r="3" />
        <circle cx="17" cy="10" r="2.5" />
        <path d="M3.5 19a5.5 5.5 0 0 1 11 0M14 16a4.5 4.5 0 0 1 6.5 3" />
      </>
    ),
    indicadores: (
      <>
        <path d="M4 19V5M4 19h17" />
        <path d="m7 15 3-4 3 2 5-7" />
      </>
    ),
    soporte: (
      <>
        <path d="M4 13v-2a8 8 0 0 1 16 0v2" />
        <path d="M4 13h3v5H5.5A1.5 1.5 0 0 1 4 16.5V13ZM20 13h-3v5h1.5a1.5 1.5 0 0 0 1.5-1.5V13Z" />
        <path d="M17 18c-.8 1.3-2.1 2-4 2h-1" />
      </>
    ),
  }[nombre];

  return (
    <svg aria-hidden="true" className={className} fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth="1.8">
      {paths}
    </svg>
  );
}

export default function PaginaLogin() {
  return (
    <main className="relative min-h-screen overflow-hidden bg-surface text-on-surface">
      <div className="pointer-events-none absolute -left-20 -top-20 h-96 w-96 rounded-full bg-secondary-container/20 blur-3xl" />
      <div className="pointer-events-none absolute bottom-0 right-0 h-80 w-80 rounded-full bg-primary-container/15 blur-2xl" />
      <div className="relative mx-auto flex min-h-screen w-full max-w-7xl items-center px-4 py-8 sm:px-8 lg:px-12 lg:py-16">
        <div className="relative grid w-full grid-cols-1 items-center gap-10 lg:grid-cols-12 lg:gap-14">
          <section className="lg:col-span-6">
            <div className="mb-8 flex items-center gap-4">
              <div className="flex h-16 w-16 shrink-0 items-center justify-center rounded-xl bg-surface-container-lowest p-2 text-xl font-bold text-primary shadow-sm">
                <Image
                  src={logoCi2dt2}
                  alt="Logo oficial del Centro de Investigación, Innovación, Desarrollo y Transferencia de Tecnología"
                  className="h-full w-full object-contain"
                  priority
                />
              </div>
              <div>
                <span className="block font-headline-sm text-headline-sm font-bold tracking-tight text-primary">CI²DT²</span>
                <span className="block max-w-sm text-sm font-medium leading-tight text-on-surface-variant">
                  Centro de Investigación, Innovación, Desarrollo y Transferencia de Tecnología
                </span>
              </div>
            </div>
            <h1 className="max-w-xl font-headline-lg text-headline-lg font-bold tracking-tight text-on-background">
              Bienvenido al Portal Institucional de Investigaciones
            </h1>
            <p className="mt-5 max-w-xl text-lg leading-relaxed text-on-surface-variant">
              Accede a la plataforma unificada para la formulación, seguimiento y gestión de proyectos de ciencia, tecnología e innovación de nuestra comunidad académica.
            </p>
            <div className="mt-8 flex flex-col gap-3">
              {([
                ["proyectos", "Gestión de proyectos y convocatorias vigentes", "Consulta bases, postula propuestas científicas y revisa dictámenes en tiempo real.", "bg-primary-container"],
                ["colaboracion", "Asistencia colaborativa y seguimiento de hitos", "Coordina equipos, actividades y avances desde un mismo espacio.", "bg-secondary"],
                ["indicadores", "Información para decisiones estratégicas", "Visualiza indicadores que impulsan la investigación institucional.", "bg-tertiary-container"],
              ] satisfies [IconoCaracteristica, string, string, string][]).map(([icono, titulo, descripcion, color]) => (
                <div key={titulo} className="flex items-start gap-4 rounded-xl bg-surface-container-low/70 p-3 transition-colors hover:bg-surface-container-low">
                  <div className={`flex h-10 w-10 shrink-0 items-center justify-center rounded-lg ${color} text-on-primary shadow-sm`}>
                    <Icono nombre={icono} className="h-5 w-5" />
                  </div>
                  <div>
                    <h2 className="text-base font-semibold text-on-surface">{titulo}</h2>
                    <p className="mt-0.5 text-sm text-on-surface-variant">{descripcion}</p>
                  </div>
                </div>
              ))}
            </div>
            <p className="mt-8 flex items-center gap-2 text-sm text-on-surface-variant">
              <Icono nombre="soporte" className="h-[18px] w-[18px] shrink-0 text-primary" />
              ¿Necesitas ayuda? <a className="font-semibold text-primary hover:text-secondary" href="mailto:soporte@ci2dt2.edu.co">soporte@ci2dt2.edu.co</a>
            </p>
          </section>
          <section className="lg:col-span-6">
            <div className="overflow-hidden rounded-xl bg-surface-container-lowest shadow-xl shadow-primary/10">
              <div className="h-1.5 bg-gradient-to-r from-primary via-secondary to-tertiary-fixed-dim" />
              <div className="p-6 sm:p-9">
                <header className="mb-7">
                  <h2 className="mt-2 font-headline-lg text-3xl font-bold tracking-tight text-on-surface sm:text-4xl">Inicia sesión en tu cuenta</h2>
                  <p className="mt-2 text-sm text-on-surface-variant">Ingresa tus credenciales para continuar al portal.</p>
                </header>
                <Suspense>
                  <FormularioLogin />
                </Suspense>
                
              </div>
            </div>
          </section>
        </div>
      </div>
    </main>
  );
}
