import type { Metadata } from "next";
import { Suspense } from "react";
import { FormularioContrasena } from "./formulario-contrasena";

export const metadata: Metadata = { title: "Definir contraseña · CI²DT²" };

export default function PaginaDefinirContrasena() {
  return (
    <main className="flex flex-1 items-center justify-center px-4 py-12">
      <div className="w-full max-w-sm rounded-xl border border-slate-200 bg-white p-8 shadow-sm">
        <h1 className="mb-6 text-2xl font-semibold text-slate-900">Definir contraseña</h1>
        <Suspense>
          <FormularioContrasena />
        </Suspense>
      </div>
    </main>
  );
}
