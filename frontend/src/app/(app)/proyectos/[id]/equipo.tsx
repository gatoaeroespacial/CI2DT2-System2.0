"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useState } from "react";
import { Controller, useForm } from "react-hook-form";
import { toast } from "sonner";
import { Autocompletar, buscarUsuarios, ErrorFormulario } from "@/components/controles";
import { Boton, Campo, Cargando, claseEntrada, EstadoError, EstadoVacio, Tabla, type Columna } from "@/components/ui";
import { api, json } from "@/lib/api";
import { esquemaIntegrante, nulo, ROLES_PROYECTO, type DatosIntegrante } from "@/lib/esquemas";
import { fechaCorta, hoyIso } from "@/lib/formato";
import type { Permisos } from "./tipos";

type Integrante = {
  id: number;
  usuarioId: number;
  nombre: string;
  email: string;
  rolEnProyecto: string;
  fechaInicio: string | null;
  fechaFin: string | null;
  vigente: boolean;
  cvLacUrl: string | null;
  orcid: string | null;
};

/** HU-09: equipo del proyecto con enlaces a CvLAC y ORCID. */
export function Equipo({ proyectoId, permisos }: { proyectoId: string; permisos: Permisos }) {
  const queryClient = useQueryClient();
  const integrantes = useQuery({
    queryKey: ["proyecto-integrantes", proyectoId],
    queryFn: () => api<Integrante[]>(`/proyectos/${proyectoId}/integrantes`),
  });
  const refrescar = async () => {
    await queryClient.invalidateQueries({ queryKey: ["proyecto-integrantes", proyectoId] });
    await queryClient.invalidateQueries({ queryKey: ["proyecto", proyectoId] });
  };

  const retirar = useMutation({
    mutationFn: (i: Integrante) =>
      api(`/proyectos/${proyectoId}/integrantes/${i.id}`, json("PUT", { fechaInicio: i.fechaInicio?.slice(0, 10) ?? null, fechaFin: hoyIso() })),
    onSuccess: async () => {
      toast.success("Retiro registrado");
      await refrescar();
    },
    onError: (e) => toast.error(e.message),
  });

  if (integrantes.isPending) return <Cargando />;
  if (integrantes.isError) return <EstadoError mensaje={integrantes.error.message} onReintentar={() => integrantes.refetch()} />;

  const columnas: Columna<Integrante>[] = [
    { titulo: "Nombre", celda: (i) => i.nombre },
    { titulo: "Rol", celda: (i) => i.rolEnProyecto },
    { titulo: "Inicio", celda: (i) => fechaCorta(i.fechaInicio) },
    { titulo: "Fin", celda: (i) => fechaCorta(i.fechaFin) },
    { titulo: "Estado", celda: (i) => (i.vigente ? "Vigente" : "Retirado") },
    {
      titulo: "Perfiles",
      celda: (i) => (
        <div className="flex gap-3 text-sm">
          {i.cvLacUrl && (
            <a href={i.cvLacUrl} target="_blank" rel="noopener noreferrer" className="text-emerald-800 hover:underline">CvLAC</a>
          )}
          {i.orcid && (
            <a href={`https://orcid.org/${i.orcid}`} target="_blank" rel="noopener noreferrer" className="text-emerald-800 hover:underline">ORCID</a>
          )}
          {!i.cvLacUrl && !i.orcid && "—"}
        </div>
      ),
    },
    ...(permisos.puedeEditar
      ? [{
          titulo: "Acciones",
          celda: (i: Integrante) =>
            i.vigente && i.rolEnProyecto !== "IP" ? (
              <button type="button" className="text-sm text-red-700 hover:underline"
                onClick={() => window.confirm(`¿Registrar el retiro de ${i.nombre} con fecha de hoy?`) && retirar.mutate(i)}>
                Retirar
              </button>
            ) : null,
        }]
      : []),
  ];

  return (
    <section className="flex flex-col gap-4 rounded-lg border border-slate-200 bg-white p-6">
      {integrantes.data.length === 0 ? (
        <EstadoVacio titulo="El proyecto no tiene integrantes" />
      ) : (
        <Tabla columnas={columnas} filas={integrantes.data} clave={(i) => i.id} />
      )}
      {permisos.puedeEditar && <AgregarIntegrante proyectoId={proyectoId} puedeCambiarIp={permisos.puedeCambiarIp} onAgregado={refrescar} />}
    </section>
  );
}

function AgregarIntegrante({ proyectoId, puedeCambiarIp, onAgregado }: { proyectoId: string; puedeCambiarIp: boolean; onAgregado: () => Promise<void> }) {
  const [clave, setClave] = useState(0);
  const {
    register,
    control,
    handleSubmit,
    setError,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<DatosIntegrante>({
    resolver: zodResolver(esquemaIntegrante),
    defaultValues: { usuarioId: "", rolEnProyecto: "Co-investigador", fechaInicio: hoyIso(), fechaFin: "" },
  });

  const enviar = handleSubmit(async (d) => {
    if (d.rolEnProyecto === "IP" && !window.confirm("El investigador principal actual quedará retirado con fecha de hoy. ¿Continuar?")) return;
    try {
      await api(`/proyectos/${proyectoId}/integrantes`, json("POST", {
        usuarioId: Number(d.usuarioId),
        rolEnProyecto: d.rolEnProyecto,
        fechaInicio: nulo(d.fechaInicio),
        fechaFin: nulo(d.fechaFin),
      }));
      toast.success(d.rolEnProyecto === "IP" ? "Investigador principal actualizado" : "Integrante agregado");
      reset();
      setClave((c) => c + 1);
      await onAgregado();
    } catch (e) {
      setError("root", { message: e instanceof Error ? e.message : "Ocurrió un error inesperado." });
    }
  });

  return (
    <form onSubmit={enviar} noValidate className="flex flex-col gap-3 border-t border-slate-100 pt-4">
      <h3 className="text-sm font-semibold text-slate-800">Agregar integrante</h3>
      <ErrorFormulario mensaje={errors.root?.message} />
      <div className="grid gap-3 md:grid-cols-4">
        <Campo id="usuarioId" etiqueta="Usuario" error={errors.usuarioId?.message} obligatorio>
          <Controller
            control={control}
            name="usuarioId"
            render={({ field }) => (
              <Autocompletar key={clave} id="usuarioId" valor={field.value} onCambio={(v) => field.onChange(v)}
                buscar={buscarUsuarios} claveConsulta="buscar-usuarios" placeholder="Nombre o correo" invalido={!!errors.usuarioId} />
            )}
          />
        </Campo>
        <Campo id="rolEnProyecto" etiqueta="Rol" error={errors.rolEnProyecto?.message} obligatorio>
          <select id="rolEnProyecto" className={claseEntrada} {...register("rolEnProyecto")}>
            {ROLES_PROYECTO.filter((r) => r !== "IP" || puedeCambiarIp).map((r) => (
              <option key={r} value={r}>
                {r === "IP" ? "IP (cambiar investigador principal)" : r}
              </option>
            ))}
          </select>
        </Campo>
        <Campo id="fechaInicio" etiqueta="Fecha de inicio" error={errors.fechaInicio?.message}>
          <input id="fechaInicio" type="date" className={claseEntrada} {...register("fechaInicio")} />
        </Campo>
        <Campo id="fechaFin" etiqueta="Fecha de fin" error={errors.fechaFin?.message}>
          <input id="fechaFin" type="date" className={claseEntrada} {...register("fechaFin")} />
        </Campo>
      </div>
      <div>
        <Boton type="submit" disabled={isSubmitting}>Agregar</Boton>
      </div>
    </form>
  );
}
