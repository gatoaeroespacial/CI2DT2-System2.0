import type { Metadata } from "next";
import { Suspense } from "react";
import { FormularioLogin } from "./formulario-login";

export const metadata: Metadata = { title: "Iniciar sesión · CI²DT²" };

export default function PaginaLogin() {
  return (
    <main className="flex flex-1 items-center justify-center px-4 py-12">
      <div className="w-full max-w-sm rounded-xl border border-slate-200 bg-white p-8 shadow-sm">
        <p className="text-sm font-medium text-emerald-800">Universidad de Caldas</p>
        <h1 className="mb-6 text-2xl font-semibold text-slate-900">CI²DT² · Iniciar sesión</h1>
        <Suspense>
          <FormularioLogin />
        </Suspense>
      </div>
    </main>
  );
}
