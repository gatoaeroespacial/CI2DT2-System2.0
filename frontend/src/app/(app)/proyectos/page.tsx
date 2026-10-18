"use client";

import { keepPreviousData, useQuery } from "@tanstack/react-query";
import Link from "next/link";
import { useState } from "react";
import { Autocompletar, BarraAvance, buscarUsuarios, InsigniaEstado } from "@/components/controles";
import { Boton, Cargando, claseEntrada, Encabezado, EstadoError, EstadoVacio, Tabla, type Columna } from "@/components/ui";
import { api } from "@/lib/api";
import { useCatalogos, type Pagina } from "@/lib/consultas";
import { fechaCorta } from "@/lib/formato";

type ProyectoListado = {
  id: number;
  codigo: string | null;
  titulo: string;
  programa: string;
  investigadorPrincipal: string;
  estado: string;
  tipoProyecto: string;
  porcentajeAvance: number | null;
  fechaFin: string | null;
  proximoHitoFecha: string | null;
  proximoHitoTitulo: string | null;
};

type Filtros = {
  programaId: string;
  estadoId: string;
  tipoProyectoId: string;
  anio: string;
  investigadorPrincipalId: string;
  busqueda: string;
  orden: string;
  desc: boolean;
  pagina: number;
};

const ORDENES = [
  { id: "", etiqueta: "Más recientes" },
  { id: "codigo", etiqueta: "Código" },
  { id: "titulo", etiqueta: "Título" },
  { id: "programa", etiqueta: "Programa" },
  { id: "ip", etiqueta: "Investigador principal" },
  { id: "estado", etiqueta: "Estado" },
  { id: "avance", etiqueta: "% avance" },
  { id: "fechaFin", etiqueta: "Fecha de fin" },
  { id: "proximoHito", etiqueta: "Próximo hito" },
];

const inicial: Filtros = {
  programaId: "", estadoId: "", tipoProyectoId: "", anio: "", investigadorPrincipalId: "", busqueda: "", orden: "", desc: false, pagina: 1,
};

/** HU-13: listado de proyectos según el alcance del rol, con filtros, búsqueda, orden y paginación. */
export default function Proyectos() {
  const catalogos = useCatalogos();
  const [filtros, setFiltros] = useState<Filtros>(inicial);
  const [busqueda, setBusqueda] = useState("");
  const [claveIp, setClaveIp] = useState(0);
  const cambiar = (cambios: Partial<Filtros>) => setFiltros((f) => ({ ...f, pagina: 1, ...cambios }));

  const parametros = new URLSearchParams();
  for (const [k, v] of Object.entries(filtros)) if (v !== "" && v !== false) parametros.set(k, String(v));

  const consulta = useQuery({
    queryKey: ["proyectos", filtros],
    queryFn: () => api<Pagina<ProyectoListado>>(`/proyectos?${parametros}`),
    placeholderData: keepPreviousData,
  });

  const columnas: Columna<ProyectoListado>[] = [
    { titulo: "Código", className: "whitespace-nowrap", celda: (p) => p.codigo ?? "—" },
    {
      titulo: "Título",
      celda: (p) => (
        <Link href={`/proyectos/${p.id}`} className="font-medium text-emerald-800 hover:underline">
          {p.titulo}
        </Link>
      ),
    },
    { titulo: "Programa", celda: (p) => p.programa },
    { titulo: "IP", celda: (p) => p.investigadorPrincipal },
    { titulo: "Estado", celda: (p) => <InsigniaEstado estado={p.estado} /> },
    { titulo: "Avance", celda: (p) => <BarraAvance valor={p.porcentajeAvance} /> },
    { titulo: "Fin", className: "whitespace-nowrap", celda: (p) => fechaCorta(p.fechaFin) },
    {
      titulo: "Próximo hito",
      celda: (p) => (p.proximoHitoFecha ? `${fechaCorta(p.proximoHitoFecha)} · ${p.proximoHitoTitulo}` : "—"),
    },
  ];

  const selector = (campo: "programaId" | "estadoId" | "tipoProyectoId", etiqueta: string, opciones?: { id: number; nombre: string }[]) => (
    <select aria-label={etiqueta} className={claseEntrada} value={filtros[campo]} onChange={(e) => cambiar({ [campo]: e.target.value })}>
      <option value="">{etiqueta}: todos</option>
      {opciones?.map((o) => (
        <option key={o.id} value={o.id}>
          {o.nombre}
        </option>
      ))}
    </select>
  );

  const datos = consulta.data;
  const totalPaginas = datos ? Math.max(1, Math.ceil(datos.total / datos.tamanoPagina)) : 1;

  return (
    <>
      <Encabezado titulo="Proyectos">
        <Link href="/proyectos/nuevo" className="rounded-md bg-emerald-800 px-4 py-2 text-sm font-medium text-white hover:bg-emerald-900">
          Nueva propuesta
        </Link>
      </Encabezado>
      <form
        role="search"
        className="mb-4 grid gap-3 md:grid-cols-4"
        onSubmit={(e) => {
          e.preventDefault();
          cambiar({ busqueda: busqueda.trim() });
        }}
      >
        <input aria-label="Buscar por título o código" placeholder="Buscar por título o código"
          className={`${claseEntrada} md:col-span-3`} value={busqueda} onChange={(e) => setBusqueda(e.target.value)} />
        <Boton type="submit" variante="secundario">Buscar</Boton>
        {selector("programaId", "Programa", catalogos.data?.programas)}
        {selector("estadoId", "Estado", catalogos.data?.estadosProyecto)}
        {selector("tipoProyectoId", "Tipo", catalogos.data?.tiposProyecto)}
        <input aria-label="Año" type="number" placeholder="Año" className={claseEntrada} value={filtros.anio}
          onChange={(e) => cambiar({ anio: e.target.value })} />
        <Autocompletar key={claveIp} id="filtro-ip" valor={filtros.investigadorPrincipalId} placeholder="Investigador principal"
          buscar={buscarUsuarios} claveConsulta="buscar-usuarios" onCambio={(v) => cambiar({ investigadorPrincipalId: v })} />
        <select aria-label="Ordenar por" className={claseEntrada} value={filtros.orden} onChange={(e) => cambiar({ orden: e.target.value })}>
          {ORDENES.map((o) => (
            <option key={o.id} value={o.id}>
              Ordenar: {o.etiqueta}
            </option>
          ))}
        </select>
        <select aria-label="Dirección" className={claseEntrada} value={filtros.desc ? "1" : ""} onChange={(e) => cambiar({ desc: e.target.value === "1" })}>
          <option value="">Ascendente</option>
          <option value="1">Descendente</option>
        </select>
        <Boton
          variante="secundario"
          onClick={() => {
            setFiltros(inicial);
            setBusqueda("");
            setClaveIp((c) => c + 1);
          }}
        >
          Limpiar filtros
        </Boton>
      </form>
      {consulta.isPending ? (
        <Cargando />
      ) : consulta.isError ? (
        <EstadoError mensaje={consulta.error.message} onReintentar={() => consulta.refetch()} />
      ) : datos!.items.length === 0 ? (
        <EstadoVacio titulo="No hay proyectos para mostrar" descripcion="Ajuste los filtros o registre una nueva propuesta." />
      ) : (
        <>
          <Tabla columnas={columnas} filas={datos!.items} clave={(p) => p.id} />
          <nav aria-label="Paginación" className="mt-4 flex items-center justify-between text-sm text-slate-600">
            <span>
              {datos!.total} proyecto(s) · Página {datos!.paginaActual} de {totalPaginas}
            </span>
            <div className="flex gap-2">
              <Boton variante="secundario" disabled={filtros.pagina <= 1} onClick={() => setFiltros((f) => ({ ...f, pagina: f.pagina - 1 }))}>
                Anterior
              </Boton>
              <Boton variante="secundario" disabled={filtros.pagina >= totalPaginas} onClick={() => setFiltros((f) => ({ ...f, pagina: f.pagina + 1 }))}>
                Siguiente
              </Boton>
            </div>
          </nav>
        </>
      )}
    </>
  );
}
