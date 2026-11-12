import type { Metadata } from "next";

export const metadata: Metadata = { title: "Política de tratamiento de datos · CI²DT²" };

/**
 * Página de la política de tratamiento de datos personales (PoliticasTratamientoDatos.TextoUrl).
 * El texto oficial debe publicarse aquí con la versión vigente aprobada por la Universidad.
 */
export default function PoliticaDatos() {
  return (
    <article className="mx-auto flex max-w-3xl flex-col gap-4 px-4 py-10 text-slate-800">
      <h1 className="text-3xl font-semibold text-emerald-950">Política de tratamiento de datos personales</h1>
      <p>
        La Universidad de Caldas trata los datos personales registrados en esta plataforma conforme a la Ley 1581 de 2012,
        sus decretos reglamentarios y la política institucional de tratamiento de datos personales.
      </p>
      <p>
        Los datos se usan para gestionar los programas, proyectos, convocatorias y el seguimiento del Centro de
        Investigación, Innovación, Desarrollo y Transferencia de Tecnología. En el portal público nunca se publican
        correos, teléfonos ni documentos de identidad.
      </p>
      <p className="rounded-md bg-amber-50 px-4 py-3 text-sm text-amber-900">
        Versión 1.0. El texto completo de la política institucional vigente debe incorporarse en esta página.
      </p>
    </article>
  );
}
