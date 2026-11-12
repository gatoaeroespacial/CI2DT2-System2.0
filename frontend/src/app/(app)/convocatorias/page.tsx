"use client";

import { keepPreviousData, useQuery } from "@tanstack/react-query";
import Link from "next/link";
import { useState } from "react";
import { Autocompletar, buscarAliados } from "@/components/controles";
import { Boton, Cargando, claseEntrada, Encabezado, EstadoError, EstadoVacio, Tabla, type Columna } from "@/components/ui";
import { api } from "@/lib/api";
import { useAuth } from "@/lib/auth-store";
import { useCatalogos, type Pagina } from "@/lib/consultas";
import { fechaCorta, formatoMonto } from "@/lib/formato";
import { ROLES, tieneAlgunRol } from "@/lib/roles";
import type { Convocatoria } from "./formulario-convocatoria";

const coloresEstado: Record<string, string> = {
  Abierta: "bg-emerald-100 text-emerald-900",
  Próxima: "bg-sky-100 text-sky-900",
  Cerrada: "bg-slate-200 text-slate-700",
};

/** HU-15: catálogo de convocatorias con estado calculado por fechas y filtros. */
export default function Convocatorias() {
  const catalogos = useCatalogos();
  const roles = useAuth((s) => s.usuario?.roles ?? []);
  const gestiona = tieneAlgunRol(roles, [ROLES.Administrador, ROLES.Director, ROLES.Coordinador]);
  const [filtros, setFiltros] = useState({ tipo: "", entidad: "", estado: "", busqueda: "", pagina: 1 });
  const [texto, setTexto] = useState("");
  const [claveEntidad, setClaveEntidad] = useState(0);
  const cambiar = (cambios: Partial<typeof filtros>) => setFiltros((f) => ({ ...f, pagina: 1, ...cambios }));

  const parametros = new URLSearchParams({ pagina: String(filtros.pagina) });
  if (filtros.tipo) parametros.set("tipoConvocatoriaId", filtros.tipo);
  if (filtros.entidad) parametros.set("entidadConvocanteId", filtros.entidad);
  if (filtros.estado) parametros.set("estado", filtros.estado);
  if (filtros.busqueda) parametros.set("busqueda", filtros.busqueda);
  const consulta = useQuery({
    queryKey: ["convocatorias", filtros],
    queryFn: () => api<Pagina<Convocatoria>>(`/convocatorias?${parametros}`),
    placeholderData: keepPreviousData,
  });

  const columnas: Columna<Convocatoria>[] = [
    {
      titulo: "Convocatoria",
      celda: (c) => (
        <Link href={`/convocatorias/${c.id}`} className="font-medium text-emerald-800 hover:underline">
          {c.nombre}{c.numero ? ` (${c.numero})` : ""}
        </Link>
      ),
    },
    { titulo: "Tipo", celda: (c) => c.tipoConvocatoria },
    { titulo: "Entidad", celda: (c) => c.entidadConvocante ?? "—" },
    { titulo: "Apertura", className: "whitespace-nowrap", celda: (c) => fechaCorta(c.fechaApertura) },
    { titulo: "Cierre", className: "whitespace-nowrap", celda: (c) => fechaCorta(c.fechaCierre) },
    { titulo: "Monto máximo", celda: (c) => formatoMonto(c.montoMaximo, c.moneda) },
    { titulo: "Estado", celda: (c) => <span className={`rounded-full px-2 py-0.5 text-xs font-medium ${coloresEstado[c.estado] ?? ""}`}>{c.estado}</span> },
  ];

  const datos = consulta.data;
  const totalPaginas = datos ? Math.max(1, Math.ceil(datos.total / datos.tamanoPagina)) : 1;

  return (
    <>
      <Encabezado titulo="Convocatorias">
        {gestiona && (
          <Link href="/convocatorias/nueva" className="rounded-md bg-emerald-800 px-4 py-2 text-sm font-medium text-white hover:bg-emerald-900">
            Nueva convocatoria
          </Link>
        )}
      </Encabezado>
      <form role="search" className="mb-4 grid gap-3 md:grid-cols-4" onSubmit={(e) => { e.preventDefault(); cambiar({ busqueda: texto.trim() }); }}>
        <input aria-label="Buscar por nombre o número" placeholder="Buscar por nombre o número" className={`${claseEntrada} md:col-span-3`}
          value={texto} onChange={(e) => setTexto(e.target.value)} />
        <Boton type="submit" variante="secundario">Buscar</Boton>
        <select aria-label="Tipo" className={claseEntrada} value={filtros.tipo} onChange={(e) => cambiar({ tipo: e.target.value })}>
          <option value="">Tipo: todos</option>
          {catalogos.data?.tiposConvocatoria.map((t) => <option key={t.id} value={t.id}>{t.nombre}</option>)}
        </select>
        <Autocompletar key={claveEntidad} id="filtro-entidad" valor={filtros.entidad} placeholder="Entidad convocante"
          buscar={buscarAliados} claveConsulta="buscar-aliados" onCambio={(v) => cambiar({ entidad: v })} />
        <select aria-label="Estado" className={claseEntrada} value={filtros.estado} onChange={(e) => cambiar({ estado: e.target.value })}>
          <option value="">Estado: todos</option>
          <option value="Abierta">Abierta</option>
          <option value="Próxima">Próxima</option>
          <option value="Cerrada">Cerrada</option>
        </select>
        <Boton variante="secundario" onClick={() => { setFiltros({ tipo: "", entidad: "", estado: "", busqueda: "", pagina: 1 }); setTexto(""); setClaveEntidad((c) => c + 1); }}>
          Limpiar filtros
        </Boton>
      </form>
      {consulta.isPending ? (
        <Cargando />
      ) : consulta.isError ? (
        <EstadoError mensaje={consulta.error.message} onReintentar={() => consulta.refetch()} />
      ) : datos!.items.length === 0 ? (
        <EstadoVacio titulo="No hay convocatorias para mostrar" descripcion={gestiona ? "Registre una con el botón Nueva convocatoria." : undefined} />
      ) : (
        <>
          <Tabla columnas={columnas} filas={datos!.items} clave={(c) => c.id} />
          <nav aria-label="Paginación" className="mt-4 flex items-center justify-between text-sm text-slate-600">
            <span>{datos!.total} convocatoria(s) · Página {datos!.paginaActual} de {totalPaginas}</span>
            <div className="flex gap-2">
              <Boton variante="secundario" disabled={filtros.pagina <= 1} onClick={() => setFiltros((f) => ({ ...f, pagina: f.pagina - 1 }))}>Anterior</Boton>
              <Boton variante="secundario" disabled={filtros.pagina >= totalPaginas} onClick={() => setFiltros((f) => ({ ...f, pagina: f.pagina + 1 }))}>Siguiente</Boton>
            </div>
          </nav>
        </>
      )}
    </>
  );
}
