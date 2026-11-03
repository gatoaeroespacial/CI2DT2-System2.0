"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { useRouter, useSearchParams } from "next/navigation";
import { useState } from "react";
import { useForm } from "react-hook-form";
import { ApiError, apiAuth, MENSAJE_SESION_EXPIRADA } from "@/lib/api";
import { useAuth, type RespuestaSesion } from "@/lib/auth-store";
import { esquemaLogin, type DatosLogin } from "@/lib/esquemas";
import { inicioPara } from "@/lib/roles";

export function FormularioLogin() {
  const router = useRouter();
  const parametros = useSearchParams();
  const establecerSesion = useAuth((s) => s.establecerSesion);
  const [error, setError] = useState<string | null>(null);

  const aviso =
    parametros.get("sesion") === "expirada"
      ? MENSAJE_SESION_EXPIRADA
      : parametros.get("contrasena") === "definida"
        ? "Su contraseña quedó definida. Ya puede iniciar sesión."
        : null;

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<DatosLogin>({ resolver: zodResolver(esquemaLogin), defaultValues: { email: "", password: "" } });

  const enviar = handleSubmit(async (datos) => {
    setError(null);
    try {
      const sesion = await apiAuth<RespuestaSesion>("/login", datos);
      establecerSesion(sesion);
      router.replace(inicioPara(sesion.usuario.roles));
    } catch (e) {
      setError(e instanceof ApiError ? e.message : "No fue posible conectar con el servidor. Intente nuevamente.");
    }
  });

  return (
    <form onSubmit={enviar} noValidate className="flex flex-col gap-5">
      {aviso && !error && (
        <p role="status" className="rounded-lg border border-amber-400/30 bg-amber-400/10 px-3 py-2 text-sm text-amber-200">
          {aviso}
        </p>
      )}
      {error && (
        <p role="alert" className="rounded-lg border border-red-400/30 bg-red-400/10 px-3 py-2 text-sm text-red-200">
          {error}
        </p>
      )}
      <div className="flex flex-col gap-2">
        <label htmlFor="email" className="text-xs font-semibold uppercase tracking-wide text-slate-300">
          Correo Electrónico / Usuario
        </label>
        <input
          id="email"
          type="email"
          autoComplete="username"
          className="w-full rounded-lg border border-slate-700 bg-slate-900/60 px-4 py-3 text-sm text-white outline-none transition placeholder:text-slate-500 focus:border-blue-500 focus:ring-2 focus:ring-blue-500"
          aria-invalid={!!errors.email}
          aria-describedby={errors.email ? "email-error" : undefined}
          {...register("email")}
        />
        {errors.email?.message && (
          <p id="email-error" role="alert" className="text-xs text-red-300">
            {errors.email.message}
          </p>
        )}
      </div>
      <div className="flex flex-col gap-2">
        <div className="flex items-center justify-between gap-3">
          <label htmlFor="password" className="text-xs font-semibold uppercase tracking-wide text-slate-300">
            Contraseña
          </label>
          <a href="/recuperar-contrasena" className="text-xs font-medium text-blue-400 transition hover:text-blue-300">
            ¿Olvidaste tu contraseña?
          </a>
        </div>
        <input
          id="password"
          type="password"
          autoComplete="current-password"
          className="w-full rounded-lg border border-slate-700 bg-slate-900/60 px-4 py-3 text-sm text-white outline-none transition placeholder:text-slate-500 focus:border-blue-500 focus:ring-2 focus:ring-blue-500"
          aria-invalid={!!errors.password}
          aria-describedby={errors.password ? "password-error" : undefined}
          {...register("password")}
        />
        {errors.password?.message && (
          <p id="password-error" role="alert" className="text-xs text-red-300">
            {errors.password.message}
          </p>
        )}
      </div>
      <label className="flex items-center gap-2 text-sm text-slate-400">
        <input
          type="checkbox"
          name="remember-device"
          className="h-4 w-4 rounded border-slate-600 bg-slate-900 text-blue-600 accent-blue-600 focus:ring-2 focus:ring-blue-500"
        />
        Recordar este dispositivo
      </label>
      <button
        type="submit"
        disabled={isSubmitting}
        className="mt-1 inline-flex w-full items-center justify-center rounded-lg bg-blue-600 px-4 py-3 text-sm font-semibold text-white shadow-lg shadow-blue-600/30 transition hover:bg-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 focus:ring-offset-slate-800 disabled:cursor-not-allowed disabled:opacity-60"
      >
        {isSubmitting ? "Ingresando…" : "Iniciar sesión"}
      </button>
    </form>
  );
}
