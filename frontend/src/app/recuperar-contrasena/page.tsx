"use client";

import Link from "next/link";
import { useState, type FormEvent } from "react";
import { Boton, Campo, claseEntrada } from "@/components/ui";
import { ErrorFormulario } from "@/components/controles";
import { ApiError, apiAuth } from "@/lib/api";

/** HU-03 CA-1: solicitud del enlace para restablecer la contraseña. */
export default function RecuperarContrasena() {
  const [email, setEmail] = useState("");
  const [mensaje, setMensaje] = useState<string>();
  const [error, setError] = useState<string>();
  const [enviando, setEnviando] = useState(false);

  const enviar = async (e: FormEvent) => {
    e.preventDefault();
    setError(undefined);
    if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email.trim())) return setError("Escriba un correo válido.");
    setEnviando(true);
    try {
      const respuesta = await apiAuth<{ mensaje: string }>("/forgot-password", { email: email.trim() });
      setMensaje(respuesta.mensaje);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "No fue posible conectar con el servidor. Intente nuevamente.");
    } finally {
      setEnviando(false);
    }
  };

  return (
    <main className="flex min-h-screen items-center justify-center bg-slate-50 px-4">
      <div className="w-full max-w-md rounded-lg border border-slate-200 bg-white p-6 shadow-sm">
        <h1 className="text-xl font-semibold text-slate-900">Restablecer contraseña</h1>
        <p className="mt-1 text-sm text-slate-600">Le enviaremos un enlace válido por 30 minutos al correo registrado.</p>
        {mensaje ? (
          <p role="status" className="mt-4 rounded-md bg-emerald-50 px-3 py-2 text-sm text-emerald-900">{mensaje}</p>
        ) : (
          <form onSubmit={enviar} noValidate className="mt-4 flex flex-col gap-4">
            <ErrorFormulario mensaje={error} />
            <Campo id="email" etiqueta="Correo" obligatorio>
              <input id="email" type="email" autoComplete="email" className={claseEntrada} value={email} onChange={(e) => setEmail(e.target.value)} />
            </Campo>
            <Boton type="submit" disabled={enviando}>{enviando ? "Enviando…" : "Enviar enlace"}</Boton>
          </form>
        )}
        <Link href="/login" className="mt-4 inline-block text-sm text-emerald-800 hover:underline">← Volver al inicio de sesión</Link>
      </div>
    </main>
  );
}
