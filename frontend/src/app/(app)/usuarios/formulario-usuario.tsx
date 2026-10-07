"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { useQuery } from "@tanstack/react-query";
import Link from "next/link";
import { useForm, useWatch } from "react-hook-form";
import { Boton, Campo, claseEntrada } from "@/components/ui";
import { api, ApiError } from "@/lib/api";
import { esquemaUsuario, TIPOS_DOCUMENTO, type DatosUsuario } from "@/lib/esquemas";
import { ROLES, TODOS_LOS_ROLES } from "@/lib/roles";
import type { AliadoOpcion, UsuarioDetalle } from "./tipos";

const vacio: DatosUsuario = {
  nombres: "",
  apellidos: "",
  tipoDocumento: "",
  numeroDocumento: "",
  email: "",
  telefono: "",
  cargo: "",
  dependencia: "",
  cvLacUrl: "",
  orcid: "",
  aliadoId: "",
  roles: [],
};

export function aFormulario(u: UsuarioDetalle): DatosUsuario {
  return {
    nombres: u.nombres,
    apellidos: u.apellidos,
    tipoDocumento: (u.tipoDocumento ?? "") as DatosUsuario["tipoDocumento"],
    numeroDocumento: u.numeroDocumento ?? "",
    email: u.email,
    telefono: u.telefono ?? "",
    cargo: u.cargo ?? "",
    dependencia: u.dependencia ?? "",
    cvLacUrl: u.cvLacUrl ?? "",
    orcid: u.orcid ?? "",
    aliadoId: u.aliadoId ? String(u.aliadoId) : "",
    roles: u.roles,
  };
}

type CampoTexto = Exclude<keyof DatosUsuario, "roles" | "tipoDocumento" | "aliadoId">;

/** Formulario de creación y edición de usuarios (HU-04). */
export function FormularioUsuario({
  inicial,
  textoGuardar,
  onGuardar,
}: {
  inicial?: DatosUsuario;
  textoGuardar: string;
  onGuardar: (datos: DatosUsuario) => Promise<void>;
}) {
  const {
    register,
    handleSubmit,
    control,
    setError,
    formState: { errors, isSubmitting },
  } = useForm<DatosUsuario>({ resolver: zodResolver(esquemaUsuario), defaultValues: inicial ?? vacio });

  const esExterno = useWatch({ control, name: "roles" }).includes(ROLES.Externo);
  const aliados = useQuery({
    queryKey: ["aliados"],
    queryFn: () => api<AliadoOpcion[]>("/aliados"),
    enabled: esExterno,
  });

  const enviar = handleSubmit(async (datos) => {
    try {
      await onGuardar(datos);
    } catch (e) {
      if (!(e instanceof ApiError)) {
        setError("root", { message: "No fue posible conectar con el servidor. Intente nuevamente." });
        return;
      }
      // Errores de validación del servidor asociados a cada campo.
      for (const [campo, mensajes] of Object.entries(e.errores ?? {})) {
        if (campo in vacio) setError(campo as keyof DatosUsuario, { message: mensajes[0] });
      }
      setError("root", { message: e.message });
    }
  });

  const texto = (campo: CampoTexto, etiqueta: string, opciones: { obligatorio?: boolean; tipo?: string; ayuda?: string } = {}) => (
    <Campo id={campo} etiqueta={etiqueta} error={errors[campo]?.message} obligatorio={opciones.obligatorio}>
      <input
        id={campo}
        type={opciones.tipo ?? "text"}
        placeholder={opciones.ayuda}
        className={claseEntrada}
        aria-invalid={!!errors[campo]}
        aria-describedby={errors[campo] ? `${campo}-error` : undefined}
        {...register(campo)}
      />
    </Campo>
  );

  return (
    <form onSubmit={enviar} noValidate className="flex max-w-3xl flex-col gap-6 rounded-lg border border-slate-200 bg-white p-6">
      {errors.root && (
        <p role="alert" className="rounded-md bg-red-50 px-3 py-2 text-sm text-red-800">
          {errors.root.message}
        </p>
      )}

      <div className="grid gap-4 md:grid-cols-2">
        {texto("nombres", "Nombres", { obligatorio: true })}
        {texto("apellidos", "Apellidos", { obligatorio: true })}
        <Campo id="tipoDocumento" etiqueta="Tipo de documento" error={errors.tipoDocumento?.message}>
          <select id="tipoDocumento" className={claseEntrada} {...register("tipoDocumento")}>
            <option value="">Sin especificar</option>
            {TIPOS_DOCUMENTO.map((t) => (
              <option key={t} value={t}>
                {t}
              </option>
            ))}
          </select>
        </Campo>
        {texto("numeroDocumento", "Número de documento")}
        {texto("email", "Correo", { obligatorio: true, tipo: "email" })}
        {texto("telefono", "Teléfono")}
        {texto("cargo", "Cargo")}
        {texto("dependencia", "Dependencia")}
        {texto("cvLacUrl", "CvLAC", { ayuda: "https://scienti.minciencias.gov.co/cvlac/…" })}
        {texto("orcid", "ORCID", { ayuda: "0000-0000-0000-000X" })}
      </div>

      <fieldset aria-describedby={errors.roles ? "roles-error" : undefined}>
        <legend className="text-sm font-medium text-slate-700">
          Roles <span className="text-red-600">*</span>
        </legend>
        <div className="mt-2 flex flex-wrap gap-4">
          {TODOS_LOS_ROLES.map((rol) => (
            <label key={rol} className="flex items-center gap-2 text-sm text-slate-800">
              <input type="checkbox" value={rol} className="h-4 w-4 accent-emerald-800" {...register("roles")} />
              {rol}
            </label>
          ))}
        </div>
        {errors.roles && (
          <p id="roles-error" role="alert" className="mt-1 text-xs text-red-600">
            {errors.roles.message}
          </p>
        )}
      </fieldset>

      {esExterno && (
        <Campo id="aliadoId" etiqueta="Aliado que representa" error={errors.aliadoId?.message} obligatorio>
          <select id="aliadoId" className={claseEntrada} aria-invalid={!!errors.aliadoId} {...register("aliadoId")}>
            <option value="">{aliados.isPending ? "Cargando aliados…" : "Seleccione un aliado"}</option>
            {aliados.data?.map((a) => (
              <option key={a.id} value={a.id}>
                {a.nombre}
              </option>
            ))}
          </select>
          {aliados.data?.length === 0 && <p className="text-xs text-slate-500">Aún no hay aliados registrados.</p>}
        </Campo>
      )}

      <div className="flex gap-3">
        <Boton type="submit" disabled={isSubmitting}>
          {isSubmitting ? "Guardando…" : textoGuardar}
        </Boton>
        <Link href="/usuarios" className="rounded-md border border-slate-300 px-4 py-2 text-sm text-slate-800 hover:bg-slate-50">
          Cancelar
        </Link>
      </div>
    </form>
  );
}
