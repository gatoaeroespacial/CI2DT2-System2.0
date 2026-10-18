"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useState } from "react";
import { Controller, useForm, useWatch } from "react-hook-form";
import { toast } from "sonner";
import { Autocompletar, buscarAliados, ErrorFormulario } from "@/components/controles";
import { Boton, Campo, Cargando, claseEntrada, EstadoError, EstadoVacio, Tabla, type Columna } from "@/components/ui";
import { api, json } from "@/lib/api";
import {
  aPeticionFuente, enCop, esquemaFuente, ESTADOS_FUENTE, MONEDAS, TIPOS_APORTE, TIPOS_FUENTE, type DatosFuente,
} from "@/lib/esquemas";
import { formatoCop, formatoMonto } from "@/lib/formato";

type Fuente = {
  id: number;
  aliadoId: number | null;
  aliado: string | null;
  tipoFuente: string | null;
  tipoAporte: string | null;
  moneda: string | null;
  monto: number | null;
  tasaCambio: number | null;
  montoCop: number | null;
  estado: string;
};
type Total = { categoria: string; totalCop: number };
type Datos = {
  fuentes: Fuente[];
  porTipoFuente: Total[];
  porTipoAporte: Total[];
  totalCop: number;
  presupuestoTotalCop: number | null;
  diferencia: number | null;
};

const vacia: DatosFuente = {
  aliadoId: "", tipoFuente: "Convocatoria", tipoAporte: "Efectivo", moneda: "COP", monto: "", tasaCambio: "", estado: "Estimado",
};

const aFormulario = (f: Fuente): DatosFuente => ({
  aliadoId: f.aliadoId === null ? "" : String(f.aliadoId),
  tipoFuente: (f.tipoFuente ?? "Convocatoria") as DatosFuente["tipoFuente"],
  tipoAporte: (f.tipoAporte ?? "Efectivo") as DatosFuente["tipoAporte"],
  moneda: (f.moneda ?? "COP") as DatosFuente["moneda"],
  monto: f.monto === null ? "" : String(f.monto),
  tasaCambio: f.tasaCambio === null ? "" : String(f.tasaCambio),
  estado: f.estado as DatosFuente["estado"],
});

/** HU-10: fuentes de financiamiento y contrapartidas con totales en COP. */
export function Financiacion({ proyectoId, puedeEditar }: { proyectoId: string; puedeEditar: boolean }) {
  const queryClient = useQueryClient();
  const [edicion, setEdicion] = useState<Fuente | "nueva" | null>(null);
  const datos = useQuery({ queryKey: ["proyecto-fuentes", proyectoId], queryFn: () => api<Datos>(`/proyectos/${proyectoId}/fuentes`) });
  const refrescar = () => queryClient.invalidateQueries({ queryKey: ["proyecto-fuentes", proyectoId] });

  const eliminar = useMutation({
    mutationFn: (f: Fuente) => api(`/proyectos/${proyectoId}/fuentes/${f.id}`, { method: "DELETE" }),
    onSuccess: () => {
      toast.success("Fuente eliminada");
      refrescar();
    },
    onError: (e) => toast.error(e.message),
  });

  if (datos.isPending) return <Cargando />;
  if (datos.isError) return <EstadoError mensaje={datos.error.message} onReintentar={() => datos.refetch()} />;
  const d = datos.data;

  const columnas: Columna<Fuente>[] = [
    { titulo: "Tipo de fuente", celda: (f) => f.tipoFuente ?? "—" },
    { titulo: "Aliado", celda: (f) => f.aliado ?? "—" },
    { titulo: "Aporte", celda: (f) => f.tipoAporte ?? "—" },
    { titulo: "Monto", celda: (f) => formatoMonto(f.monto, f.moneda) },
    { titulo: "Monto COP", celda: (f) => formatoCop(f.montoCop) },
    { titulo: "Estado", celda: (f) => f.estado },
    ...(puedeEditar
      ? [{
          titulo: "Acciones",
          className: "whitespace-nowrap",
          celda: (f: Fuente) => (
            <div className="flex gap-2">
              <button type="button" className="text-sm text-emerald-800 hover:underline" onClick={() => setEdicion(f)}>Editar</button>
              <button type="button" className="text-sm text-red-700 hover:underline"
                onClick={() => window.confirm("¿Eliminar esta fuente?") && eliminar.mutate(f)}>Eliminar</button>
            </div>
          ),
        }]
      : []),
  ];

  return (
    <section className="flex flex-col gap-4 rounded-lg border border-slate-200 bg-white p-6">
      {d.fuentes.length === 0 ? <EstadoVacio titulo="El proyecto no tiene fuentes registradas" /> : <Tabla columnas={columnas} filas={d.fuentes} clave={(f) => f.id} />}
      <div className="grid gap-4 text-sm md:grid-cols-3">
        <Totales titulo="Por tipo de fuente" totales={d.porTipoFuente} />
        <Totales titulo="Por tipo de aporte" totales={d.porTipoAporte} />
        <dl className="flex flex-col gap-1">
          <div className="flex justify-between"><dt>Total fuentes</dt><dd className="font-semibold">{formatoCop(d.totalCop)}</dd></div>
          <div className="flex justify-between"><dt>Presupuesto total</dt><dd>{formatoCop(d.presupuestoTotalCop)}</dd></div>
          <div className="flex justify-between"><dt>Diferencia</dt><dd>{formatoCop(d.diferencia)}</dd></div>
        </dl>
      </div>
      {d.diferencia !== null && d.diferencia !== 0 && (
        <p role="status" className="rounded-md bg-amber-50 px-3 py-2 text-sm text-amber-900">
          La suma de las fuentes no coincide con el presupuesto total del proyecto (diferencia de {formatoCop(d.diferencia)}).
        </p>
      )}
      {puedeEditar &&
        (edicion === null ? (
          <div>
            <Boton onClick={() => setEdicion("nueva")}>Agregar fuente</Boton>
          </div>
        ) : (
          <FormularioFuente
            key={edicion === "nueva" ? "nueva" : edicion.id}
            proyectoId={proyectoId}
            fuente={edicion === "nueva" ? null : edicion}
            onTerminar={() => {
              setEdicion(null);
              refrescar();
            }}
          />
        ))}
    </section>
  );
}

function Totales({ titulo, totales }: { titulo: string; totales: Total[] }) {
  return (
    <dl className="flex flex-col gap-1">
      <dt className="font-medium text-slate-700">{titulo}</dt>
      {totales.length === 0 && <dd className="text-slate-500">—</dd>}
      {totales.map((t) => (
        <dd key={t.categoria} className="flex justify-between">
          <span>{t.categoria}</span>
          <span>{formatoCop(t.totalCop)}</span>
        </dd>
      ))}
    </dl>
  );
}

function FormularioFuente({ proyectoId, fuente, onTerminar }: { proyectoId: string; fuente: Fuente | null; onTerminar: () => void }) {
  const {
    register,
    control,
    handleSubmit,
    setError,
    formState: { errors, isSubmitting },
  } = useForm<DatosFuente>({ resolver: zodResolver(esquemaFuente), defaultValues: fuente ? aFormulario(fuente) : vacia });
  const [moneda, monto, tasa] = useWatch({ control, name: ["moneda", "monto", "tasaCambio"] });

  const enviar = handleSubmit(async (d) => {
    try {
      const ruta = fuente ? `/proyectos/${proyectoId}/fuentes/${fuente.id}` : `/proyectos/${proyectoId}/fuentes`;
      await api(ruta, json(fuente ? "PUT" : "POST", aPeticionFuente(d)));
      toast.success(fuente ? "Fuente actualizada" : "Fuente registrada");
      onTerminar();
    } catch (e) {
      setError("root", { message: e instanceof Error ? e.message : "Ocurrió un error inesperado." });
    }
  });

  const selector = <K extends "tipoFuente" | "tipoAporte" | "moneda" | "estado">(campo: K, etiqueta: string, opciones: readonly string[]) => (
    <Campo id={campo} etiqueta={etiqueta} error={errors[campo]?.message} obligatorio>
      <select id={campo} className={claseEntrada} {...register(campo)}>
        {opciones.map((o) => (
          <option key={o} value={o}>
            {o}
          </option>
        ))}
      </select>
    </Campo>
  );

  return (
    <form onSubmit={enviar} noValidate className="flex flex-col gap-3 border-t border-slate-100 pt-4">
      <h3 className="text-sm font-semibold text-slate-800">{fuente ? "Editar fuente" : "Nueva fuente"}</h3>
      <ErrorFormulario mensaje={errors.root?.message} />
      <div className="grid gap-3 md:grid-cols-4">
        {selector("tipoFuente", "Tipo de fuente", TIPOS_FUENTE)}
        <Campo id="aliadoId" etiqueta="Aliado" error={errors.aliadoId?.message}>
          <Controller
            control={control}
            name="aliadoId"
            render={({ field }) => (
              <Autocompletar id="aliadoId" valor={field.value} etiquetaInicial={fuente?.aliado ?? undefined}
                onCambio={(v) => field.onChange(v)} buscar={buscarAliados} claveConsulta="buscar-aliados"
                placeholder="Nombre o NIT" invalido={!!errors.aliadoId} />
            )}
          />
        </Campo>
        {selector("tipoAporte", "Tipo de aporte", TIPOS_APORTE)}
        {selector("estado", "Estado", ESTADOS_FUENTE)}
        {selector("moneda", "Moneda", MONEDAS)}
        <Campo id="monto" etiqueta="Monto" error={errors.monto?.message} obligatorio>
          <input id="monto" type="number" min={0} step="0.01" className={claseEntrada} {...register("monto")} />
        </Campo>
        {moneda !== "COP" && (
          <Campo id="tasaCambio" etiqueta={`Tasa (COP por 1 ${moneda})`} error={errors.tasaCambio?.message} obligatorio>
            <input id="tasaCambio" type="number" min={0} step="0.0001" className={claseEntrada} {...register("tasaCambio")} />
          </Campo>
        )}
        <div className="flex flex-col justify-end text-sm text-slate-700">
          <span className="font-medium">Equivalente en pesos</span>
          <span>{formatoCop(enCop(monto === "" ? null : Number(monto), moneda, tasa === "" ? null : Number(tasa)))}</span>
        </div>
      </div>
      <div className="flex gap-3">
        <Boton type="submit" disabled={isSubmitting}>Guardar fuente</Boton>
        <Boton variante="secundario" onClick={onTerminar}>Cancelar</Boton>
      </div>
    </form>
  );
}
