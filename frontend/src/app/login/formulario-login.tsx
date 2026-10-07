"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { useRouter, useSearchParams } from "next/navigation";
import { useState } from "react";
import { useForm } from "react-hook-form";
import { Boton, Campo, claseEntrada } from "@/components/ui";
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
    <form onSubmit={enviar} noValidate className="flex flex-col gap-4">
      {aviso && !error && (
        <p role="status" className="rounded-md bg-amber-50 px-3 py-2 text-sm text-amber-900">
          {aviso}
        </p>
      )}
      {error && (
        <p role="alert" className="rounded-md bg-red-50 px-3 py-2 text-sm text-red-800">
          {error}
        </p>
      )}
      <Campo id="email" etiqueta="Correo" error={errors.email?.message}>
        <input
          id="email"
          type="email"
          autoComplete="username"
          className={claseEntrada}
          aria-invalid={!!errors.email}
          aria-describedby={errors.email ? "email-error" : undefined}
          {...register("email")}
        />
      </Campo>
      <Campo id="password" etiqueta="Contraseña" error={errors.password?.message}>
        <input
          id="password"
          type="password"
          autoComplete="current-password"
          className={claseEntrada}
          aria-invalid={!!errors.password}
          aria-describedby={errors.password ? "password-error" : undefined}
          {...register("password")}
        />
      </Campo>
      <Boton type="submit" disabled={isSubmitting} className="mt-2">
        {isSubmitting ? "Ingresando…" : "Iniciar sesión"}
      </Boton>
    </form>
  );
}
