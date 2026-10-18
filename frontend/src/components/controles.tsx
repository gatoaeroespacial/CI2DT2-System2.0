"use client";

import { useQuery } from "@tanstack/react-query";
import { useEffect, useId, useState, type ReactNode } from "react";
import { api } from "@/lib/api";
import type { Pagina } from "@/lib/consultas";
import { reglasContrasena } from "@/lib/esquemas";
import { claseEntrada } from "./ui";

export type OpcionAutocompletar = { id: number; etiqueta: string; detalle?: string };

function useDebounce(valor: string, ms = 300) {
  const [demorado, setDemorado] = useState(valor);
  useEffect(() => {
    const t = setTimeout(() => setDemorado(valor), ms);
    return () => clearTimeout(t);
  }, [valor, ms]);
  return demorado;
}

/** Campo de búsqueda con sugerencias (combobox accesible). */
export function Autocompletar({
  id,
  valor,
  etiquetaInicial,
  onCambio,
  buscar,
  claveConsulta,
  placeholder,
  invalido,
  accionExtra,
}: {
  id: string;
  valor: string;
  etiquetaInicial?: string;
  onCambio: (id: string, etiqueta: string) => void;
  buscar: (texto: string) => Promise<OpcionAutocompletar[]>;
  claveConsulta: string;
  placeholder?: string;
  invalido?: boolean;
  accionExtra?: ReactNode;
}) {
  const [texto, setTexto] = useState(etiquetaInicial ?? "");
  const [abierto, setAbierto] = useState(false);
  const demorado = useDebounce(texto);
  const listaId = useId();
  const sugerencias = useQuery({
    queryKey: [claveConsulta, demorado],
    queryFn: () => buscar(demorado),
    enabled: abierto && demorado.trim().length >= 2,
  });

  return (
    <div className="relative">
      <input
        id={id}
        role="combobox"
        aria-expanded={abierto}
        aria-controls={listaId}
        aria-autocomplete="list"
        aria-invalid={invalido}
        autoComplete="off"
        className={claseEntrada}
        placeholder={placeholder ?? "Escriba al menos 2 letras"}
        value={texto}
        onChange={(e) => {
          setTexto(e.target.value);
          setAbierto(true);
          if (valor) onCambio("", "");
        }}
        onFocus={() => setAbierto(true)}
        onBlur={() => setTimeout(() => setAbierto(false), 150)}
      />
      {abierto && demorado.trim().length >= 2 && (
        <ul id={listaId} role="listbox" className="absolute z-10 mt-1 max-h-60 w-full overflow-auto rounded-md border border-slate-200 bg-white shadow-lg">
          {sugerencias.isPending && <li className="px-3 py-2 text-sm text-slate-500">Buscando…</li>}
          {sugerencias.data?.length === 0 && <li className="px-3 py-2 text-sm text-slate-500">Sin resultados.</li>}
          {sugerencias.data?.map((o) => (
            <li
              key={o.id}
              role="option"
              aria-selected={String(o.id) === valor}
              className="cursor-pointer px-3 py-2 text-sm hover:bg-emerald-50"
              onMouseDown={(e) => e.preventDefault()}
              onClick={() => {
                setTexto(o.etiqueta);
                setAbierto(false);
                onCambio(String(o.id), o.etiqueta);
              }}
            >
              <span className="text-slate-900">{o.etiqueta}</span>
              {o.detalle && <span className="ml-2 text-xs text-slate-500">{o.detalle}</span>}
            </li>
          ))}
          {accionExtra && <li className="border-t border-slate-100 px-3 py-2 text-sm">{accionExtra}</li>}
        </ul>
      )}
    </div>
  );
}

type AliadoItem = { id: number; nombre: string; nit: string | null; pais: string | null };

export const buscarAliados = async (texto: string): Promise<OpcionAutocompletar[]> => {
  const pagina = await api<Pagina<AliadoItem>>(`/aliados?busqueda=${encodeURIComponent(texto)}`);
  return pagina.items.map((a) => ({ id: a.id, etiqueta: a.nombre, detalle: a.nit ?? a.pais ?? undefined }));
};

export const buscarUsuarios = async (texto: string): Promise<OpcionAutocompletar[]> => {
  const usuarios = await api<{ id: number; nombre: string; email: string }[]>(`/usuarios/buscar?texto=${encodeURIComponent(texto)}`);
  return usuarios.map((u) => ({ id: u.id, etiqueta: u.nombre, detalle: u.email }));
};

/** Pestañas accesibles. */
export function Pestanas<T extends string>({
  pestanas,
  activa,
  onCambio,
}: {
  pestanas: { id: T; etiqueta: string }[];
  activa: T;
  onCambio: (id: T) => void;
}) {
  return (
    <div role="tablist" className="mb-4 flex flex-wrap gap-1 border-b border-slate-200">
      {pestanas.map((p) => (
        <button
          key={p.id}
          type="button"
          role="tab"
          aria-selected={p.id === activa}
          onClick={() => onCambio(p.id)}
          className={`-mb-px border-b-2 px-3 py-2 text-sm ${p.id === activa ? "border-emerald-800 font-medium text-emerald-900" : "border-transparent text-slate-600 hover:text-slate-900"}`}
        >
          {p.etiqueta}
        </button>
      ))}
    </div>
  );
}

const coloresEstado: Record<string, string> = {
  "En elaboración": "bg-slate-200 text-slate-800",
  "En revisión": "bg-amber-100 text-amber-900",
  Devuelto: "bg-orange-100 text-orange-900",
  Presentado: "bg-sky-100 text-sky-900",
  Aprobado: "bg-emerald-100 text-emerald-900",
  "En ejecución": "bg-indigo-100 text-indigo-900",
  Finalizado: "bg-teal-100 text-teal-900",
  "No aprobado": "bg-red-100 text-red-900",
  Suspendido: "bg-zinc-200 text-zinc-800",
};

/** Etiqueta de color del estado del proyecto (HU-13 CA-1). */
export function InsigniaEstado({ estado }: { estado: string }) {
  return (
    <span className={`whitespace-nowrap rounded-full px-2 py-0.5 text-xs font-medium ${coloresEstado[estado] ?? "bg-slate-100 text-slate-700"}`}>
      {estado}
    </span>
  );
}

/** Barra de % de avance (HU-13 CA-1). */
export function BarraAvance({ valor }: { valor: number | null }) {
  const pct = Math.max(0, Math.min(100, valor ?? 0));
  return (
    <div className="flex items-center gap-2" aria-label={`Avance ${pct}%`}>
      <div className="h-2 w-20 overflow-hidden rounded-full bg-slate-200">
        <div className="h-full bg-emerald-700" style={{ width: `${pct}%` }} />
      </div>
      <span className="text-xs text-slate-600">{pct}%</span>
    </div>
  );
}

/** Reglas de la política de contraseña evaluadas en tiempo real. */
export function ReglasContrasena({ valor, id }: { valor: string; id?: string }) {
  return (
    <ul id={id} className="flex flex-col gap-1 text-xs">
      {reglasContrasena.map((regla) => {
        const cumple = regla.cumple(valor);
        return (
          <li key={regla.mensaje} className={cumple ? "text-emerald-700" : "text-slate-500"}>
            {cumple ? "✓" : "○"} {regla.mensaje}
          </li>
        );
      })}
    </ul>
  );
}

/** Mensaje de error general del formulario. */
export function ErrorFormulario({ mensaje }: { mensaje?: string }) {
  if (!mensaje) return null;
  return (
    <p role="alert" className="rounded-md bg-red-50 px-3 py-2 text-sm text-red-800">
      {mensaje}
    </p>
  );
}
