"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { useEffect } from "react";
import { useQuery } from "@tanstack/react-query";
import { useForm, useWatch } from "react-hook-form";
import { ErrorFormulario } from "@/components/controles";
import { Boton, Campo, claseEntrada } from "@/components/ui";
import { api, ApiError } from "@/lib/api";
import { useAuth } from "@/lib/auth-store";
import { useCatalogos } from "@/lib/consultas";
import { enCop, esquemaProyecto, MONEDAS, type DatosProyecto } from "@/lib/esquemas";
import { formatoCop } from "@/lib/formato";
import { ROLES, tieneAlgunRol } from "@/lib/roles";

export const proyectoVacio: DatosProyecto = {
  titulo: "", resumen: "", programaId: "", tipoProyectoId: "", investigadorPrincipalId: "",
  fechaInicio: "", fechaFin: "", moneda: "", presupuestoTotal: "", tasaCambio: "",
};

/**
 * Formulario de la propuesta (HU-08). Al crear se elige el programa y, para Director,
 * Coordinador o Administrador, el investigador principal; al editar esos campos no cambian.
 */
export function FormularioProyecto({
  inicial,
  creando,
  textoGuardar,
  onGuardar,
  onCancelar,
}: {
  inicial: DatosProyecto;
  creando: boolean;
  textoGuardar: string;
  onGuardar: (datos: DatosProyecto) => Promise<void>;
  onCancelar: () => void;
}) {
  const catalogos = useCatalogos();
  const usuario = useAuth((s) => s.usuario);
  const designaIp = tieneAlgunRol(usuario?.roles ?? [], [ROLES.Administrador, ROLES.Director, ROLES.Coordinador]);
  const {
    register,
    control,
    handleSubmit,
    setError,
    formState: { errors, isSubmitting, isDirty },
  } = useForm<DatosProyecto>({ resolver: zodResolver(esquemaProyecto), defaultValues: inicial });

  // Advierte al salir con cambios sin guardar (HU-08 CA-3).
  useEffect(() => {
    if (!isDirty) return;
    const avisar = (e: BeforeUnloadEvent) => e.preventDefault();
    window.addEventListener("beforeunload", avisar);
    return () => window.removeEventListener("beforeunload", avisar);
  }, [isDirty]);

  // Director y Administrador proponen en cualquier programa; los demás, en aquellos a los que están adscritos.
  const programas = tieneAlgunRol(usuario?.roles ?? [], [ROLES.Administrador, ROLES.Director])
    ? catalogos.data?.programas
    : catalogos.data?.programas.filter((p) => usuario?.programas.includes(p.id));

  const [moneda, presupuesto, tasa, programaId] = useWatch({ control, name: ["moneda", "presupuestoTotal", "tasaCambio", "programaId"] });
  const adscritos = useQuery({
    queryKey: ["adscritos", programaId],
    queryFn: () => api<{ id: number; nombre: string; email: string }[]>(`/programas/${programaId}/adscritos`),
    enabled: creando && designaIp && !!programaId,
  });
  const valorCop = moneda === "" ? null : enCop(presupuesto === "" ? null : Number(presupuesto), moneda, tasa === "" ? null : Number(tasa));

  const enviar = handleSubmit(async (datos) => {
    try {
      await onGuardar(datos);
    } catch (e) {
      if (!(e instanceof ApiError)) {
        setError("root", { message: "No fue posible conectar con el servidor. Intente nuevamente." });
        return;
      }
      for (const [campo, mensajes] of Object.entries(e.errores ?? {})) {
        if (campo in proyectoVacio) setError(campo as keyof DatosProyecto, { message: mensajes[0] });
      }
      setError("root", { message: e.message });
    }
  });

  return (
    <form onSubmit={enviar} noValidate className="flex max-w-4xl flex-col gap-6 rounded-lg border border-slate-200 bg-white p-6">
      <ErrorFormulario mensaje={errors.root?.message} />
      <Campo id="titulo" etiqueta="Título" error={errors.titulo?.message} obligatorio>
        <input id="titulo" className={claseEntrada} aria-invalid={!!errors.titulo} {...register("titulo")} />
      </Campo>
      <Campo id="resumen" etiqueta="Resumen" error={errors.resumen?.message}>
        <textarea id="resumen" rows={5} className={claseEntrada} aria-invalid={!!errors.resumen} {...register("resumen")} />
      </Campo>
      <div className="grid gap-4 md:grid-cols-2">
        {creando && (
          <Campo id="programaId" etiqueta="Programa" error={errors.programaId?.message} obligatorio>
            <select id="programaId" className={claseEntrada} aria-invalid={!!errors.programaId} {...register("programaId")}>
              <option value="">Seleccione…</option>
              {programas?.map((p) => (
                <option key={p.id} value={p.id}>
                  {p.nombre}
                </option>
              ))}
            </select>
          </Campo>
        )}
        <Campo id="tipoProyectoId" etiqueta="Tipo de proyecto" error={errors.tipoProyectoId?.message} obligatorio>
          <select id="tipoProyectoId" className={claseEntrada} aria-invalid={!!errors.tipoProyectoId} {...register("tipoProyectoId")}>
            <option value="">Seleccione…</option>
            {catalogos.data?.tiposProyecto.map((t) => (
              <option key={t.id} value={t.id}>
                {t.nombre}
              </option>
            ))}
          </select>
        </Campo>
        {creando && designaIp && (
          <Campo id="investigadorPrincipalId" etiqueta="Investigador principal (vacío = usted)" error={errors.investigadorPrincipalId?.message}>
            {/* Solo los adscritos vigentes al programa pueden ser IP (HU-08, P1.1). */}
            <select id="investigadorPrincipalId" className={claseEntrada} disabled={!programaId} {...register("investigadorPrincipalId")}>
              <option value="">{programaId ? "Usted (si está adscrito al programa)" : "Seleccione primero el programa"}</option>
              {adscritos.data?.map((u) => (
                <option key={u.id} value={u.id}>
                  {u.nombre} · {u.email}
                </option>
              ))}
            </select>
            {programaId && adscritos.data?.length === 0 && (
              <p className="text-xs text-amber-700">El programa no tiene integrantes vigentes. Vincúlelos en Programas.</p>
            )}
          </Campo>
        )}
        <Campo id="fechaInicio" etiqueta="Fecha de inicio" error={errors.fechaInicio?.message}>
          <input id="fechaInicio" type="date" className={claseEntrada} {...register("fechaInicio")} />
        </Campo>
        <Campo id="fechaFin" etiqueta="Fecha de fin" error={errors.fechaFin?.message}>
          <input id="fechaFin" type="date" className={claseEntrada} aria-invalid={!!errors.fechaFin} {...register("fechaFin")} />
        </Campo>
        <Campo id="moneda" etiqueta="Moneda" error={errors.moneda?.message}>
          <select id="moneda" className={claseEntrada} aria-invalid={!!errors.moneda} {...register("moneda")}>
            <option value="">Seleccione…</option>
            {MONEDAS.map((m) => (
              <option key={m} value={m}>
                {m}
              </option>
            ))}
          </select>
        </Campo>
        <Campo id="presupuestoTotal" etiqueta="Presupuesto total" error={errors.presupuestoTotal?.message}>
          <input id="presupuestoTotal" type="number" min={0} step="0.01" className={claseEntrada} {...register("presupuestoTotal")} />
        </Campo>
        {moneda !== "" && moneda !== "COP" && (
          <Campo id="tasaCambio" etiqueta={`Tasa de cambio (COP por 1 ${moneda})`} error={errors.tasaCambio?.message} obligatorio>
            <input id="tasaCambio" type="number" min={0} step="0.0001" className={claseEntrada} aria-invalid={!!errors.tasaCambio} {...register("tasaCambio")} />
          </Campo>
        )}
        <div className="flex flex-col justify-end text-sm text-slate-700">
          <span className="font-medium">Equivalente en pesos</span>
          <span>{formatoCop(valorCop)}</span>
        </div>
      </div>
      <div className="flex gap-3">
        <Boton type="submit" disabled={isSubmitting}>
          {isSubmitting ? "Guardando…" : textoGuardar}
        </Boton>
        <Boton variante="secundario" onClick={onCancelar}>
          Cancelar
        </Boton>
      </div>
    </form>
  );
}
