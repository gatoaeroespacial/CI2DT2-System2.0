"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import Link from "next/link";
import { useForm } from "react-hook-form";
import { ErrorFormulario } from "@/components/controles";
import { Boton, Campo, claseEntrada } from "@/components/ui";
import { ApiError } from "@/lib/api";
import { useCatalogos } from "@/lib/consultas";
import { esquemaAliado, type DatosAliado } from "@/lib/esquemas";

export type Aliado = {
  id: number;
  tipoAliadoId: number;
  tipoAliado: string;
  nombre: string;
  nit: string | null;
  pais: string | null;
  ciudad: string | null;
  sector: string | null;
  representanteLegal: string | null;
  email: string;
};

export const aFormularioAliado = (a: Aliado): DatosAliado => ({
  tipoAliadoId: String(a.tipoAliadoId),
  nombre: a.nombre,
  nit: a.nit ?? "",
  pais: a.pais ?? "",
  ciudad: a.ciudad ?? "",
  sector: a.sector ?? "",
  representanteLegal: a.representanteLegal ?? "",
  email: a.email,
});

const vacio: DatosAliado = {
  tipoAliadoId: "", nombre: "", nit: "", pais: "Colombia", ciudad: "", sector: "", representanteLegal: "", email: "",
};

/** Formulario del catálogo de aliados (HU-07): NIT con dígito de verificación para aliados colombianos. */
export function FormularioAliado({
  inicial,
  textoGuardar,
  onGuardar,
}: {
  inicial?: DatosAliado;
  textoGuardar: string;
  onGuardar: (datos: DatosAliado) => Promise<void>;
}) {
  const catalogos = useCatalogos();
  const {
    register,
    handleSubmit,
    setError,
    formState: { errors, isSubmitting },
  } = useForm<DatosAliado>({ resolver: zodResolver(esquemaAliado), defaultValues: inicial ?? vacio });

  const enviar = handleSubmit(async (datos) => {
    try {
      await onGuardar(datos);
    } catch (e) {
      if (!(e instanceof ApiError)) {
        setError("root", { message: "No fue posible conectar con el servidor. Intente nuevamente." });
        return;
      }
      for (const [campo, mensajes] of Object.entries(e.errores ?? {})) {
        if (campo in vacio) setError(campo as keyof DatosAliado, { message: mensajes[0] });
      }
      setError("root", { message: e.message });
    }
  });

  const texto = (campo: keyof DatosAliado, etiqueta: string, obligatorio = false, ayuda?: string) => (
    <Campo id={campo} etiqueta={etiqueta} error={errors[campo]?.message} obligatorio={obligatorio}>
      <input id={campo} className={claseEntrada} placeholder={ayuda} aria-invalid={!!errors[campo]} {...register(campo)} />
    </Campo>
  );

  return (
    <form onSubmit={enviar} noValidate className="flex max-w-3xl flex-col gap-6 rounded-lg border border-slate-200 bg-white p-6">
      <ErrorFormulario mensaje={errors.root?.message} />
      <div className="grid gap-4 md:grid-cols-2">
        {texto("nombre", "Nombre", true)}
        <Campo id="tipoAliadoId" etiqueta="Tipo de aliado" error={errors.tipoAliadoId?.message} obligatorio>
          <select id="tipoAliadoId" className={claseEntrada} aria-invalid={!!errors.tipoAliadoId} {...register("tipoAliadoId")}>
            <option value="">Seleccione…</option>
            {catalogos.data?.tiposAliado.map((t) => (
              <option key={t.id} value={t.id}>
                {t.nombre}
              </option>
            ))}
          </select>
        </Campo>
        {texto("pais", "País", true)}
        {texto("nit", "NIT", false, "890800128-6 (obligatorio si es de Colombia)")}
        {texto("ciudad", "Ciudad")}
        {texto("sector", "Sector")}
        {texto("representanteLegal", "Representante legal")}
        {texto("email", "Correo", true)}
      </div>
      <div className="flex gap-3">
        <Boton type="submit" disabled={isSubmitting}>
          {isSubmitting ? "Guardando…" : textoGuardar}
        </Boton>
        <Link href="/aliados" className="rounded-md border border-slate-300 px-4 py-2 text-sm text-slate-800 hover:bg-slate-50">
          Cancelar
        </Link>
      </div>
    </form>
  );
}
