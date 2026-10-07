"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { useRouter, useSearchParams } from "next/navigation";
import { useState } from "react";
import { useForm, useWatch } from "react-hook-form";
import { Boton, Campo, claseEntrada } from "@/components/ui";
import { ApiError, apiAuth } from "@/lib/api";
import { esquemaDefinirContrasena, reglasContrasena, type DatosDefinirContrasena } from "@/lib/esquemas";

/** Define la contraseña con el enlace del correo de bienvenida (HU-04). */
export function FormularioContrasena() {
  const router = useRouter();
  const token = useSearchParams().get("token") ?? "";
  const [error, setError] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    control,
    formState: { errors, isSubmitting },
  } = useForm<DatosDefinirContrasena>({
    resolver: zodResolver(esquemaDefinirContrasena),
    defaultValues: { nuevaContrasena: "", confirmacion: "" },
  });
  const contrasena = useWatch({ control, name: "nuevaContrasena" });

  if (!token) {
    return (
      <p role="alert" className="text-sm text-red-800">
        El enlace no es válido. Solicite al administrador un nuevo enlace.
      </p>
    );
  }

  const enviar = handleSubmit(async ({ nuevaContrasena }) => {
    setError(null);
    try {
      await apiAuth("/reset-password", { token, nuevaContrasena });
      router.replace("/login?contrasena=definida");
    } catch (e) {
      setError(e instanceof ApiError ? e.message : "No fue posible conectar con el servidor. Intente nuevamente.");
    }
  });

  return (
    <form onSubmit={enviar} noValidate className="flex flex-col gap-4">
      {error && (
        <p role="alert" className="rounded-md bg-red-50 px-3 py-2 text-sm text-red-800">
          {error}
        </p>
      )}
      <Campo id="nuevaContrasena" etiqueta="Nueva contraseña" error={errors.nuevaContrasena?.message}>
        <input
          id="nuevaContrasena"
          type="password"
          autoComplete="new-password"
          className={claseEntrada}
          aria-invalid={!!errors.nuevaContrasena}
          aria-describedby="reglas-contrasena"
          {...register("nuevaContrasena")}
        />
      </Campo>
      <ul id="reglas-contrasena" className="flex flex-col gap-1 text-xs">
        {reglasContrasena.map((regla) => {
          const cumple = regla.cumple(contrasena);
          return (
            <li key={regla.mensaje} className={cumple ? "text-emerald-700" : "text-slate-500"}>
              {cumple ? "✓" : "○"} {regla.mensaje}
            </li>
          );
        })}
      </ul>
      <Campo id="confirmacion" etiqueta="Confirmar contraseña" error={errors.confirmacion?.message}>
        <input
          id="confirmacion"
          type="password"
          autoComplete="new-password"
          className={claseEntrada}
          aria-invalid={!!errors.confirmacion}
          {...register("confirmacion")}
        />
      </Campo>
      <Boton type="submit" disabled={isSubmitting}>
        {isSubmitting ? "Guardando…" : "Guardar contraseña"}
      </Boton>
    </form>
  );
}
