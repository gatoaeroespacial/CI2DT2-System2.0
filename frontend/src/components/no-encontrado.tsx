import Link from "next/link";

/** Contenido de la página 404 (ruta inexistente o recurso no público). */
export function NoEncontrado() {
  return (
    <section className="mx-auto flex max-w-xl flex-col items-center gap-4 px-4 py-20 text-center">
      <p className="text-5xl font-semibold text-emerald-900">404</p>
      <h1 className="text-xl font-semibold text-slate-900">No encontramos esta página</h1>
      <p className="text-slate-600">
        La dirección no existe o el contenido no está publicado.
      </p>
      <div className="flex flex-wrap justify-center gap-3">
        <Link href="/" className="rounded-md bg-emerald-800 px-4 py-2 text-sm font-medium text-white hover:bg-emerald-900">
          Ir al inicio
        </Link>
      </div>
    </section>
  );
}
