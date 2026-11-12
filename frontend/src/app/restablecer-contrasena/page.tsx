import { Suspense } from "react";
import { Cargando } from "@/components/ui";
import { FormularioRestablecer } from "./formulario-restablecer";

export default function RestablecerContrasena() {
  return (
    <main className="flex min-h-screen items-center justify-center bg-slate-50 px-4">
      <div className="w-full max-w-md rounded-lg border border-slate-200 bg-white p-6 shadow-sm">
        <h1 className="text-xl font-semibold text-slate-900">Nueva contraseña</h1>
        <Suspense fallback={<Cargando />}>
          <FormularioRestablecer />
        </Suspense>
      </div>
    </main>
  );
}
