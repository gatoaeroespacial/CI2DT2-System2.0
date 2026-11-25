import type { Metadata } from "next";
import { apiPublica, type ResumenPortal } from "@/lib/portal";
import { fechaCorta, formatoMonto } from "@/lib/formato";

// HU-25: renderizado estático con revalidación incremental cada 10 minutos.
export const revalidate = 600;

export const metadata: Metadata = {
  title: "Convocatorias abiertas · CI²DT²",
  description:
    "Convocatorias abiertas de investigación, innovación y transferencia tecnológica del CI²DT² de la Universidad de Caldas.",
};

/** Vista pública de convocatorias abiertas: cards autocontenidas sin enlaces ni postulación pública. */
export default async function ConvocatoriasPortal() {
  const resumen = await apiPublica<ResumenPortal>("/resumen");
  const convocatorias = resumen?.convocatoriasAbiertas ?? [];

  return (
    <div className="w-full px-margin md:px-margin-tablet lg:px-margin-desktop py-space-xl bg-surface">
      <div className="max-w-7xl mx-auto flex flex-col gap-space-xl">
        <header className="flex flex-col gap-space-sm">
          <span className="font-label-md text-label-md text-secondary font-semibold uppercase tracking-wider">
            Oportunidades de Financiación
          </span>
          <h1 className="font-headline-lg text-headline-lg-mobile md:text-headline-lg text-primary tracking-tight">
            Convocatorias Abiertas
          </h1>
          <p className="font-body-md text-body-md text-on-surface-variant max-w-3xl">
            Consulta informativa de convocatorias vigentes. La gestión de postulaciones y formulación de propuestas es exclusiva para usuarios autenticados en el sistema de información.
          </p>
        </header>

        {!resumen ? (
          <div className="rounded-xl bg-surface-container-lowest p-space-lg shadow-sm border border-outline-variant/30">
            <p className="font-body-md text-body-md text-on-surface-variant">
              Las convocatorias no están disponibles en este momento.
            </p>
          </div>
        ) : convocatorias.length === 0 ? (
          <div className="rounded-xl bg-surface-container-lowest p-space-lg shadow-sm border border-outline-variant/30">
            <p className="font-body-md text-body-md text-on-surface-variant">
              No hay convocatorias abiertas en este momento.
            </p>
          </div>
        ) : (
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-gutter-desktop">
            {convocatorias.map((cv) => (
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

                  <h2 className="font-headline-sm text-headline-sm text-primary">{cv.nombre}</h2>

                  {cv.numero && (
                    <p className="font-label-sm text-label-sm text-on-surface-variant">
                      Código / Número: {cv.numero}
                    </p>
                  )}

                  {cv.entidadConvocante && (
                    <p className="font-body-sm text-body-sm text-on-surface-variant mt-space-xs">
                      <strong className="text-on-surface">Entidad convocante:</strong> {cv.entidadConvocante}
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
        )}
      </div>
    </div>
  );
}

