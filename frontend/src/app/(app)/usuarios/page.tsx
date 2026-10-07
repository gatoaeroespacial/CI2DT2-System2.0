"use client";

import { keepPreviousData, useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import Link from "next/link";
import { useState, type FormEvent } from "react";
import { toast } from "sonner";
import { Boton, Cargando, claseEntrada, Encabezado, EstadoError, EstadoVacio, Tabla, type Columna } from "@/components/ui";
import { api } from "@/lib/api";
import { useAuth } from "@/lib/auth-store";
import { TODOS_LOS_ROLES } from "@/lib/roles";
import type { Pagina, UsuarioListado } from "./tipos";

type Filtros = { busqueda: string; rol: string; estado: string; pagina: number };

export default function Usuarios() {
  const [filtros, setFiltros] = useState<Filtros>({ busqueda: "", rol: "", estado: "", pagina: 1 });
  const [busqueda, setBusqueda] = useState("");
  const usuarioActualId = useAuth((s) => s.usuario?.id);
  const queryClient = useQueryClient();

  const parametros = new URLSearchParams({ pagina: String(filtros.pagina) });
  if (filtros.busqueda) parametros.set("busqueda", filtros.busqueda);
  if (filtros.rol) parametros.set("rol", filtros.rol);
  if (filtros.estado) parametros.set("estado", filtros.estado);

  const consulta = useQuery({
    queryKey: ["usuarios", filtros],
    queryFn: () => api<Pagina<UsuarioListado>>(`/usuarios?${parametros}`),
    placeholderData: keepPreviousData,
  });

  const cambiarEstado = useMutation({
    mutationFn: (u: UsuarioListado) =>
      api(`/usuarios/${u.id}/estado`, { method: "PATCH", body: JSON.stringify({ tieneAcceso: !u.tieneAcceso }) }),
    onSuccess: (_, u) => {
      toast.success(u.tieneAcceso ? "Usuario desactivado" : "Usuario activado");
      queryClient.invalidateQueries({ queryKey: ["usuarios"] });
    },
    onError: (e) => toast.error(e.message),
  });

  const buscar = (e: FormEvent) => {
    e.preventDefault();
    setFiltros((f) => ({ ...f, busqueda: busqueda.trim(), pagina: 1 }));
  };

  const confirmarCambio = (u: UsuarioListado) => {
    const accion = u.tieneAcceso ? "desactivar" : "activar";
    if (window.confirm(`¿Desea ${accion} a ${u.nombres} ${u.apellidos}?`)) cambiarEstado.mutate(u);
  };

  const columnas: Columna<UsuarioListado>[] = [
    { titulo: "Nombre", celda: (u) => `${u.nombres} ${u.apellidos}` },
    { titulo: "Correo", celda: (u) => u.email },
    { titulo: "Documento", celda: (u) => (u.numeroDocumento ? `${u.tipoDocumento ?? ""} ${u.numeroDocumento}` : "—") },
    { titulo: "Roles", celda: (u) => u.roles.join(", ") },
    {
      titulo: "Estado",
      celda: (u) => (
        <span className={`rounded-full px-2 py-0.5 text-xs font-medium ${u.tieneAcceso ? "bg-emerald-100 text-emerald-800" : "bg-slate-200 text-slate-700"}`}>
          {u.tieneAcceso ? "Activo" : "Inactivo"}
        </span>
      ),
    },
    {
      titulo: "Acciones",
      className: "whitespace-nowrap",
      celda: (u) => (
        <div className="flex gap-2">
          <Link href={`/usuarios/${u.id}`} className="rounded-md px-2 py-1 text-sm text-emerald-800 hover:underline">
            Editar
          </Link>
          {u.id !== usuarioActualId && (
            <button
              type="button"
              disabled={cambiarEstado.isPending}
              onClick={() => confirmarCambio(u)}
              className="rounded-md px-2 py-1 text-sm text-slate-700 hover:underline disabled:opacity-50"
            >
              {u.tieneAcceso ? "Desactivar" : "Activar"}
            </button>
          )}
        </div>
      ),
    },
  ];

  const datos = consulta.data;
  const totalPaginas = datos ? Math.max(1, Math.ceil(datos.total / datos.tamanoPagina)) : 1;

  return (
    <>
      <Encabezado titulo="Usuarios">
        <Link href="/usuarios/nuevo" className="rounded-md bg-emerald-800 px-4 py-2 text-sm font-medium text-white hover:bg-emerald-900">
          Nuevo usuario
        </Link>
      </Encabezado>

      <form onSubmit={buscar} className="mb-4 grid gap-3 md:grid-cols-[1fr_auto_auto_auto]" role="search">
        <input
          aria-label="Buscar por nombre, correo o documento"
          placeholder="Buscar por nombre, correo o documento"
          className={claseEntrada}
          value={busqueda}
          onChange={(e) => setBusqueda(e.target.value)}
        />
        <select
          aria-label="Filtrar por rol"
          className={claseEntrada}
          value={filtros.rol}
          onChange={(e) => setFiltros((f) => ({ ...f, rol: e.target.value, pagina: 1 }))}
        >
          <option value="">Todos los roles</option>
          {TODOS_LOS_ROLES.map((r) => (
            <option key={r} value={r}>
              {r}
            </option>
          ))}
        </select>
        <select
          aria-label="Filtrar por estado"
          className={claseEntrada}
          value={filtros.estado}
          onChange={(e) => setFiltros((f) => ({ ...f, estado: e.target.value, pagina: 1 }))}
        >
          <option value="">Todos los estados</option>
          <option value="activo">Activos</option>
          <option value="inactivo">Inactivos</option>
        </select>
        <Boton type="submit" variante="secundario">
          Buscar
        </Boton>
      </form>

      {consulta.isPending ? (
        <Cargando />
      ) : consulta.isError ? (
        <EstadoError mensaje={consulta.error.message} onReintentar={() => consulta.refetch()} />
      ) : datos!.items.length === 0 ? (
        <EstadoVacio titulo="No se encontraron usuarios" descripcion="Ajuste la búsqueda o los filtros." />
      ) : (
        <>
          <Tabla columnas={columnas} filas={datos!.items} clave={(u) => u.id} />
          <nav aria-label="Paginación" className="mt-4 flex items-center justify-between text-sm text-slate-600">
            <span>
              {datos!.total} usuario(s) · Página {datos!.paginaActual} de {totalPaginas}
            </span>
            <div className="flex gap-2">
              <Boton
                variante="secundario"
                disabled={filtros.pagina <= 1}
                onClick={() => setFiltros((f) => ({ ...f, pagina: f.pagina - 1 }))}
              >
                Anterior
              </Boton>
              <Boton
                variante="secundario"
                disabled={filtros.pagina >= totalPaginas}
                onClick={() => setFiltros((f) => ({ ...f, pagina: f.pagina + 1 }))}
              >
                Siguiente
              </Boton>
            </div>
          </nav>
        </>
      )}
    </>
  );
}
