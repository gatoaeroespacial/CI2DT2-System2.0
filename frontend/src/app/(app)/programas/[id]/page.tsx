"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import Link from "next/link";
import { useParams } from "next/navigation";
import { useState, type ReactNode } from "react";
import { Controller, useForm } from "react-hook-form";
import { toast } from "sonner";
import { Autocompletar, buscarAliados, buscarUsuarios, ErrorFormulario, Pestanas } from "@/components/controles";
import { Boton, Campo, Cargando, claseEntrada, Encabezado, EstadoError, EstadoVacio, Tabla, type Columna } from "@/components/ui";
import { api, ApiError, json } from "@/lib/api";
import { useAuth } from "@/lib/auth-store";
import { esquemaPrograma, esquemaVinculacion, nulo, type DatosPrograma, type DatosVinculacion } from "@/lib/esquemas";
import { fechaCorta, hoyIso } from "@/lib/formato";
import { ROLES, tieneAlgunRol } from "@/lib/roles";

type Objetivo = { id: number; descripcion: string | null; orden: number | null };
type ProgramaDetalle = {
  id: number;
  nombre: string;
  descripcion: string | null;
  objetivos: Objetivo[];
  aliados: { id: number; nombre: string }[];
};
type Integrante = {
  id: number;
  usuarioId: number;
  nombre: string;
  email: string;
  rolEnPrograma: string;
  fechaInicio: string | null;
  fechaFin: string | null;
  vigente: boolean;
};

const mensajeError = (e: unknown) => (e instanceof Error ? e.message : "Ocurrió un error inesperado.");

function Seccion({ titulo, children, acciones }: { titulo: string; children: ReactNode; acciones?: ReactNode }) {
  return (
    <section className="flex flex-col gap-4 rounded-lg border border-slate-200 bg-white p-6">
      <div className="flex items-center justify-between gap-4">
        <h2 className="text-lg font-semibold text-slate-900">{titulo}</h2>
        {acciones}
      </div>
      {children}
    </section>
  );
}

/** HU-06 (programa, objetivos e integrantes) y HU-07 CA-2 (aliados del programa). */
export default function DetallePrograma() {
  const { id } = useParams<{ id: string }>();
  const roles = useAuth((s) => s.usuario?.roles ?? []);
  const esDirectivo = tieneAlgunRol(roles, [ROLES.Administrador, ROLES.Director]);
  const programa = useQuery({ queryKey: ["programa", id], queryFn: () => api<ProgramaDetalle>(`/programas/${id}`) });

  if (programa.isPending) return <Cargando />;
  if (programa.isError) return <EstadoError mensaje={programa.error.message} onReintentar={() => programa.refetch()} />;

  return (
    <>
      <Encabezado titulo={programa.data.nombre}>
        <Link href="/programas" className="text-sm text-emerald-800 hover:underline">
          ← Programas
        </Link>
      </Encabezado>
      <div className="flex flex-col gap-6">
        <DatosGenerales programa={programa.data} editable={esDirectivo} />
        <Objetivos programaId={id} objetivos={programa.data.objetivos} editable={esDirectivo} />
        <Integrantes programaId={id} puedeAsignarCoordinador={esDirectivo} />
        <AliadosPrograma programaId={id} aliados={programa.data.aliados} />
      </div>
    </>
  );
}

function DatosGenerales({ programa, editable }: { programa: ProgramaDetalle; editable: boolean }) {
  const queryClient = useQueryClient();
  const [editando, setEditando] = useState(false);
  const {
    register,
    handleSubmit,
    setError,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<DatosPrograma>({
    resolver: zodResolver(esquemaPrograma),
    values: { nombre: programa.nombre, descripcion: programa.descripcion ?? "" },
  });

  const guardar = handleSubmit(async (d) => {
    try {
      await api(`/programas/${programa.id}`, json("PUT", { nombre: d.nombre.trim(), descripcion: nulo(d.descripcion) }));
      await queryClient.invalidateQueries({ queryKey: ["programa", String(programa.id)] });
      await queryClient.invalidateQueries({ queryKey: ["programas"] });
      toast.success("Programa actualizado");
      setEditando(false);
    } catch (e) {
      setError("root", { message: mensajeError(e) });
    }
  });

  return (
    <Seccion
      titulo="Datos del programa"
      acciones={editable && !editando && <Boton variante="secundario" onClick={() => setEditando(true)}>Editar</Boton>}
    >
      {editando ? (
        <form onSubmit={guardar} noValidate className="flex flex-col gap-4">
          <ErrorFormulario mensaje={errors.root?.message} />
          <Campo id="nombre" etiqueta="Nombre" error={errors.nombre?.message} obligatorio>
            <input id="nombre" className={claseEntrada} {...register("nombre")} />
          </Campo>
          <Campo id="descripcion" etiqueta="Descripción" error={errors.descripcion?.message}>
            <textarea id="descripcion" rows={4} className={claseEntrada} {...register("descripcion")} />
          </Campo>
          <div className="flex gap-3">
            <Boton type="submit" disabled={isSubmitting}>Guardar</Boton>
            <Boton variante="secundario" onClick={() => { reset(); setEditando(false); }}>Cancelar</Boton>
          </div>
        </form>
      ) : (
        <p className="whitespace-pre-line text-sm text-slate-700">{programa.descripcion || "Sin descripción."}</p>
      )}
    </Seccion>
  );
}

function Objetivos({ programaId, objetivos, editable }: { programaId: string; objetivos: Objetivo[]; editable: boolean }) {
  const queryClient = useQueryClient();
  const [nuevo, setNuevo] = useState("");
  const [edicion, setEdicion] = useState<{ id: number; texto: string } | null>(null);
  const [arrastrado, setArrastrado] = useState<number | null>(null);
  const refrescar = () => queryClient.invalidateQueries({ queryKey: ["programa", programaId] });

  const accion = useMutation({
    mutationFn: (peticion: () => Promise<unknown>) => peticion(),
    onSuccess: refrescar,
    onError: (e) => toast.error(e.message),
  });

  const reordenar = (ids: number[]) =>
    accion.mutate(() => api(`/programas/${programaId}/objetivos/orden`, json("PUT", { ids })));

  const mover = (desde: number, hasta: number) => {
    if (hasta < 0 || hasta >= objetivos.length || desde === hasta) return;
    const ids = objetivos.map((o) => o.id);
    const [movido] = ids.splice(desde, 1);
    ids.splice(hasta, 0, movido);
    reordenar(ids);
  };

  return (
    <Seccion titulo="Objetivos específicos">
      {objetivos.length === 0 && <EstadoVacio titulo="El programa no tiene objetivos registrados" />}
      <ol className="flex flex-col gap-2">
        {objetivos.map((o, i) => (
          <li
            key={o.id}
            draggable={editable && edicion === null}
            onDragStart={() => setArrastrado(i)}
            onDragOver={(e) => editable && e.preventDefault()}
            onDrop={() => {
              if (arrastrado !== null) mover(arrastrado, i);
              setArrastrado(null);
            }}
            className={`flex items-start gap-3 rounded-md border border-slate-200 p-3 text-sm ${editable ? "cursor-move" : ""}`}
          >
            <span className="font-semibold text-slate-500">{i + 1}.</span>
            {edicion?.id === o.id ? (
              <div className="flex flex-1 flex-col gap-2">
                <textarea aria-label="Descripción del objetivo" rows={3} className={claseEntrada} value={edicion.texto}
                  onChange={(e) => setEdicion({ id: o.id, texto: e.target.value })} />
                <div className="flex gap-2">
                  <Boton
                    disabled={edicion.texto.trim() === ""}
                    onClick={() =>
                      accion.mutate(
                        () => api(`/programas/${programaId}/objetivos/${o.id}`, json("PUT", { descripcion: edicion.texto.trim() })),
                        { onSuccess: () => setEdicion(null) },
                      )
                    }
                  >
                    Guardar
                  </Boton>
                  <Boton variante="secundario" onClick={() => setEdicion(null)}>Cancelar</Boton>
                </div>
              </div>
            ) : (
              <p className="flex-1 whitespace-pre-line text-slate-800">{o.descripcion}</p>
            )}
            {editable && edicion?.id !== o.id && (
              <div className="flex shrink-0 gap-1">
                <button type="button" aria-label="Subir" className="px-1 text-slate-600 hover:text-slate-900 disabled:opacity-30"
                  disabled={i === 0} onClick={() => mover(i, i - 1)}>↑</button>
                <button type="button" aria-label="Bajar" className="px-1 text-slate-600 hover:text-slate-900 disabled:opacity-30"
                  disabled={i === objetivos.length - 1} onClick={() => mover(i, i + 1)}>↓</button>
                <button type="button" className="px-2 text-emerald-800 hover:underline"
                  onClick={() => setEdicion({ id: o.id, texto: o.descripcion ?? "" })}>Editar</button>
                <button type="button" className="px-2 text-red-700 hover:underline"
                  onClick={() => window.confirm("¿Eliminar este objetivo?") &&
                    accion.mutate(() => api(`/programas/${programaId}/objetivos/${o.id}`, { method: "DELETE" }))}>
                  Eliminar
                </button>
              </div>
            )}
          </li>
        ))}
      </ol>
      {editable && (
        <div className="flex flex-col gap-2">
          <label htmlFor="nuevo-objetivo" className="text-sm font-medium text-slate-700">Nuevo objetivo</label>
          <textarea id="nuevo-objetivo" rows={2} className={claseEntrada} value={nuevo} onChange={(e) => setNuevo(e.target.value)} />
          <div>
            <Boton
              disabled={nuevo.trim() === "" || accion.isPending}
              onClick={() =>
                accion.mutate(() => api(`/programas/${programaId}/objetivos`, json("POST", { descripcion: nuevo.trim() })), {
                  onSuccess: () => setNuevo(""),
                })
              }
            >
              Agregar objetivo
            </Boton>
          </div>
        </div>
      )}
    </Seccion>
  );
}

function Integrantes({ programaId, puedeAsignarCoordinador }: { programaId: string; puedeAsignarCoordinador: boolean }) {
  const queryClient = useQueryClient();
  const [pestana, setPestana] = useState<"vigentes" | "historicos">("vigentes");
  const integrantes = useQuery({
    queryKey: ["programa-integrantes", programaId],
    queryFn: () => api<Integrante[]>(`/programas/${programaId}/integrantes`),
    retry: (n, e) => !(e instanceof ApiError && e.status === 403) && n < 2,
  });

  const retirar = useMutation({
    mutationFn: (i: Integrante) =>
      api(`/programas/${programaId}/integrantes/${i.id}`, json("PUT", { fechaInicio: i.fechaInicio?.slice(0, 10) ?? null, fechaFin: hoyIso() })),
    onSuccess: () => {
      toast.success("Retiro registrado");
      queryClient.invalidateQueries({ queryKey: ["programa-integrantes", programaId] });
      queryClient.invalidateQueries({ queryKey: ["programas"] });
    },
    onError: (e) => toast.error(e.message),
  });

  if (integrantes.isError && integrantes.error instanceof ApiError && integrantes.error.status === 403)
    return (
      <Seccion titulo="Integrantes">
        <p className="text-sm text-slate-600">Solo el Coordinador vigente de este programa, el Director o el Administrador pueden ver y gestionar sus integrantes.</p>
      </Seccion>
    );

  const columnas: Columna<Integrante>[] = [
    { titulo: "Nombre", celda: (i) => i.nombre },
    { titulo: "Correo", celda: (i) => i.email },
    { titulo: "Rol", celda: (i) => i.rolEnPrograma },
    { titulo: "Inicio", celda: (i) => fechaCorta(i.fechaInicio) },
    { titulo: "Fin", celda: (i) => fechaCorta(i.fechaFin) },
    ...(pestana === "vigentes"
      ? [{
          titulo: "Acciones",
          celda: (i: Integrante) =>
            i.rolEnPrograma === "Coordinador" && !puedeAsignarCoordinador ? null : (
              <button type="button" className="text-sm text-red-700 hover:underline"
                onClick={() => window.confirm(`¿Registrar el retiro de ${i.nombre} con fecha de hoy?`) && retirar.mutate(i)}>
                Retirar
              </button>
            ),
        }]
      : []),
  ];

  const filas = integrantes.data?.filter((i) => i.vigente === (pestana === "vigentes")) ?? [];

  return (
    <Seccion titulo="Integrantes">
      <Pestanas
        pestanas={[{ id: "vigentes", etiqueta: "Vigentes" }, { id: "historicos", etiqueta: "Históricos" }]}
        activa={pestana}
        onCambio={setPestana}
      />
      {integrantes.isPending ? (
        <Cargando />
      ) : integrantes.isError ? (
        <EstadoError mensaje={integrantes.error.message} onReintentar={() => integrantes.refetch()} />
      ) : filas.length === 0 ? (
        <EstadoVacio titulo={pestana === "vigentes" ? "No hay integrantes vigentes" : "No hay integrantes históricos"} />
      ) : (
        <Tabla columnas={columnas} filas={filas} clave={(i) => i.id} />
      )}
      <Vincular programaId={programaId} puedeAsignarCoordinador={puedeAsignarCoordinador} />
    </Seccion>
  );
}

function Vincular({ programaId, puedeAsignarCoordinador }: { programaId: string; puedeAsignarCoordinador: boolean }) {
  const queryClient = useQueryClient();
  const [clave, setClave] = useState(0);
  const {
    register,
    control,
    handleSubmit,
    setError,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<DatosVinculacion>({
    resolver: zodResolver(esquemaVinculacion),
    defaultValues: { usuarioId: "", rolEnPrograma: "Investigador", fechaInicio: hoyIso(), fechaFin: "" },
  });

  const enviar = handleSubmit(async (d) => {
    try {
      await api(`/programas/${programaId}/integrantes`, json("POST", {
        usuarioId: Number(d.usuarioId),
        rolEnPrograma: d.rolEnPrograma,
        fechaInicio: d.fechaInicio,
        fechaFin: nulo(d.fechaFin),
      }));
      toast.success("Integrante vinculado");
      reset();
      setClave((c) => c + 1);
      await queryClient.invalidateQueries({ queryKey: ["programa-integrantes", programaId] });
      await queryClient.invalidateQueries({ queryKey: ["programas"] });
    } catch (e) {
      setError("root", { message: mensajeError(e) });
    }
  });

  return (
    <form onSubmit={enviar} noValidate className="flex flex-col gap-3 border-t border-slate-100 pt-4">
      <h3 className="text-sm font-semibold text-slate-800">Vincular integrante</h3>
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
        <Campo id="rolEnPrograma" etiqueta="Rol" error={errors.rolEnPrograma?.message} obligatorio>
          <select id="rolEnPrograma" className={claseEntrada} {...register("rolEnPrograma")}>
            <option value="Investigador">Investigador</option>
            {puedeAsignarCoordinador && <option value="Coordinador">Coordinador</option>}
          </select>
        </Campo>
        <Campo id="fechaInicio" etiqueta="Fecha de inicio" error={errors.fechaInicio?.message} obligatorio>
          <input id="fechaInicio" type="date" className={claseEntrada} {...register("fechaInicio")} />
        </Campo>
        <Campo id="fechaFin" etiqueta="Fecha de fin" error={errors.fechaFin?.message}>
          <input id="fechaFin" type="date" className={claseEntrada} {...register("fechaFin")} />
        </Campo>
      </div>
      <div>
        <Boton type="submit" disabled={isSubmitting}>Vincular</Boton>
      </div>
    </form>
  );
}

function AliadosPrograma({ programaId, aliados }: { programaId: string; aliados: { id: number; nombre: string }[] }) {
  const queryClient = useQueryClient();
  const [clave, setClave] = useState(0);
  const guardar = useMutation({
    mutationFn: (ids: number[]) => api(`/programas/${programaId}/aliados`, json("PUT", { aliadoIds: ids })),
    onSuccess: () => {
      toast.success("Aliados del programa actualizados");
      setClave((c) => c + 1);
      queryClient.invalidateQueries({ queryKey: ["programa", programaId] });
    },
    onError: (e) => toast.error(e.message),
  });
  const ids = aliados.map((a) => a.id);

  return (
    <Seccion titulo="Aliados del programa">
      {aliados.length === 0 ? (
        <EstadoVacio titulo="El programa no tiene aliados asociados" />
      ) : (
        <ul className="flex flex-wrap gap-2">
          {aliados.map((a) => (
            <li key={a.id} className="flex items-center gap-2 rounded-full bg-slate-100 px-3 py-1 text-sm text-slate-800">
              {a.nombre}
              <button type="button" aria-label={`Quitar ${a.nombre}`} className="text-slate-500 hover:text-red-700"
                onClick={() => guardar.mutate(ids.filter((x) => x !== a.id))}>×</button>
            </li>
          ))}
        </ul>
      )}
      <div className="max-w-md">
        <label htmlFor="agregar-aliado" className="mb-1 block text-sm font-medium text-slate-700">Asociar aliado</label>
        <Autocompletar
          key={clave}
          id="agregar-aliado"
          valor=""
          buscar={buscarAliados}
          claveConsulta="buscar-aliados"
          placeholder="Nombre o NIT"
          onCambio={(v) => v && !ids.includes(Number(v)) && guardar.mutate([...ids, Number(v)])}
          accionExtra={<Link href="/aliados/nuevo" className="text-emerald-800 hover:underline">+ Registrar un nuevo aliado</Link>}
        />
      </div>
    </Seccion>
  );
}
