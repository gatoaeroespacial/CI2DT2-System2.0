"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { useRouter, useSearchParams } from "next/navigation";
import { useState } from "react";
import { useForm, useWatch } from "react-hook-form";
import { ApiError, apiAuth, MENSAJE_SESION_EXPIRADA } from "@/lib/api";
import { useAuth, type RespuestaSesion } from "@/lib/auth-store";
import { esquemaLogin, type DatosLogin } from "@/lib/esquemas";
import { inicioPara } from "@/lib/roles";

function IconoVisibilidad({ oculto }: { oculto: boolean }) {
  return (
    <svg aria-hidden="true" className="h-[21px] w-[21px]" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth="1.8">
      {oculto ? (
        <>
          <path d="M3 3l18 18" />
          <path d="M10.6 10.7a2 2 0 0 0 2.7 2.7M9.9 5.2A10.7 10.7 0 0 1 12 5c5.2 0 8.7 4.2 9.8 7a11.8 11.8 0 0 1-3.1 4.4M6.2 6.2C3.9 7.8 2.6 10.2 2.2 12c.5 2.1 2.2 5 5.6 6.6A10.8 10.8 0 0 0 12 19c1.2 0 2.3-.2 3.3-.6" />
        </>
      ) : (
        <>
          <path d="M2.2 12C3.3 9.2 6.8 5 12 5s8.7 4.2 9.8 7c-1.1 2.8-4.6 7-9.8 7s-8.7-4.2-9.8-7Z" />
          <circle cx="12" cy="12" r="2.5" />
        </>
      )}
    </svg>
  );
}

function IconoCheck() {
  return (
    <svg aria-hidden="true" className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth="2.5">
      <path d="m5 12 4 4L19 6" />
    </svg>
  );
}

export function FormularioLogin() {
  const router = useRouter();
  const parametros = useSearchParams();
  const establecerSesion = useAuth((s) => s.establecerSesion);
  const [error, setError] = useState<string | null>(null);
  const [mostrarContrasena, setMostrarContrasena] = useState(false);

  const aviso =
    parametros.get("sesion") === "expirada"
      ? MENSAJE_SESION_EXPIRADA
      : parametros.get("contrasena") === "definida"
        ? "Su contraseña quedó definida. Ya puede iniciar sesión."
        : null;

  const {
    register,
    handleSubmit,
    control,
    formState: { errors, isSubmitting },
  } = useForm<DatosLogin>({ resolver: zodResolver(esquemaLogin), defaultValues: { email: "", password: "" } });
  const correo = useWatch({ control, name: "email" });
  const correoValido = /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(correo);

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
        <div role="status" className="flex items-start gap-3 rounded-lg border border-secondary/20 bg-secondary/5 px-4 py-3 text-sm text-secondary">
          <span className="material-symbols-outlined text-[20px]" aria-hidden="true">check_circle</span>
          <p>{aviso}</p>
        </div>
      )}
      {error && (
        <div role="alert" className="flex items-start gap-3 rounded-lg border border-error/20 bg-error-container/40 px-4 py-3 text-sm text-error">
          <span className="material-symbols-outlined text-[20px]" aria-hidden="true">error</span>
          <p>{error}</p>
        </div>
      )}
      <div className="flex flex-col gap-2">
        <div className="flex items-center justify-between gap-3">
          <label htmlFor="email" className="font-label-md text-label-md font-semibold text-on-surface">
            Correo electrónico / usuario
          </label>
          {correoValido && !errors.email && (
            <span className="flex items-center gap-1 text-xs font-medium text-secondary">
              <IconoCheck />
              Formato válido
            </span>
          )}
        </div>
        <input
          id="email"
          type="email"
          autoComplete="username"
          placeholder="nombre@institucion.edu.co"
          className="w-full rounded-lg border border-outline-variant bg-surface px-4 py-3 text-sm text-on-surface outline-none transition placeholder:text-on-surface-variant/60 focus:border-primary focus:ring-2 focus:ring-primary/15"
          aria-invalid={!!errors.email}
          aria-describedby={errors.email ? "email-error" : undefined}
          {...register("email")}
        />
        {errors.email?.message && (
          <p id="email-error" role="alert" className="text-xs text-error">
            {errors.email.message}
          </p>
        )}
      </div>
      <div className="flex flex-col gap-2">
        <div className="flex items-center justify-between gap-3">
          <label htmlFor="password" className="font-label-md text-label-md font-semibold text-on-surface">
            Contraseña
          </label>
          <a href="/recuperar-contrasena" className="text-xs font-medium text-primary transition hover:text-secondary">
            ¿Olvidaste tu contraseña?
          </a>
        </div>
        <div className="relative">
          <input
            id="password"
            type={mostrarContrasena ? "text" : "password"}
            autoComplete="current-password"
            className="w-full rounded-lg border border-outline-variant bg-surface px-4 py-3 pr-12 text-sm text-on-surface outline-none transition placeholder:text-on-surface-variant/60 focus:border-primary focus:ring-2 focus:ring-primary/15"
            aria-invalid={!!errors.password}
            aria-describedby={errors.password ? "password-error" : undefined}
            {...register("password")}
          />
          <button
            type="button"
            onClick={() => setMostrarContrasena((visible) => !visible)}
            className="absolute inset-y-0 right-0 flex w-11 items-center justify-center text-on-surface-variant transition hover:text-primary"
            aria-label={mostrarContrasena ? "Ocultar contraseña" : "Mostrar contraseña"}
          >
            <IconoVisibilidad oculto={!mostrarContrasena} />
          </button>
        </div>
        {errors.password?.message && (
          <p id="password-error" role="alert" className="text-xs text-error">
            {errors.password.message}
          </p>
        )}
      </div>
      <label className="flex items-center gap-2 text-sm text-on-surface-variant">
        <input
          type="checkbox"
          name="remember-device"
          className="h-4 w-4 rounded border-outline-variant accent-primary focus:ring-2 focus:ring-primary"
        />
        Recordar este dispositivo
      </label>
      <button
        type="submit"
        disabled={isSubmitting}
        className="mt-1 inline-flex w-full items-center justify-center gap-2 rounded-lg bg-primary px-4 py-3 text-sm font-semibold text-on-primary shadow-lg shadow-primary/20 transition hover:bg-primary-container focus:outline-none focus:ring-2 focus:ring-primary focus:ring-offset-2 focus:ring-offset-surface-container-lowest disabled:cursor-not-allowed disabled:opacity-60"
      >
        {isSubmitting && (
          <span
            className="h-4 w-4 animate-spin rounded-full border-2 border-current border-t-transparent"
            aria-hidden="true"
          />
        )}
        {isSubmitting ? "Validando credenciales..." : "Iniciar sesión"}
      </button>
    </form>
  );
}
