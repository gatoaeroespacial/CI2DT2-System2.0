import type { Metadata } from "next";
import { Suspense } from "react";
import { FormularioLogin } from "./formulario-login";

export const metadata: Metadata = { title: "Iniciar sesión · CI²DT²" };

export default function PaginaLogin() {
  return (
    <main className="flex min-h-screen flex-1 items-center justify-center bg-slate-900 bg-[radial-gradient(circle_at_top,_rgba(30,64,175,0.2),_transparent_55%)] px-4 py-8 font-institutional sm:py-12">
      <div className="w-full max-w-md rounded-2xl border border-slate-700/50 bg-slate-800/85 p-6 shadow-2xl shadow-slate-950/30 backdrop-blur-xl sm:p-8">
        <header className="mb-8 text-center">
          <div className="mx-auto mb-5 flex h-16 w-16 items-center justify-center rounded-xl border border-blue-500/30 bg-blue-600/20 text-2xl font-bold text-blue-400">
            CI²
          </div>
          <h1 className="text-2xl font-bold tracking-tight text-white">Portal Inteligente de Convocatorias</h1>
          <p className="mt-2 text-sm text-slate-400">CI2DT2 SYSTEM - Autenticación Institucional</p>
        </header>
        <Suspense>
          <FormularioLogin />
        </Suspense>
        <footer className="mt-8 text-center text-xs text-slate-500">
          Centro de Investigación, Innovación, Desarrollo y Transferencia de Tecnología © 2026
        </footer>
      </div>
    </main>
  );
}
