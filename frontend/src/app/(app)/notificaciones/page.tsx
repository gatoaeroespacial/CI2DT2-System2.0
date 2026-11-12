"use client";

import { keepPreviousData, useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useRouter } from "next/navigation";
import { useState } from "react";
import { toast } from "sonner";
import { Boton, Cargando, Encabezado, EstadoError, EstadoVacio } from "@/components/ui";
import { api } from "@/lib/api";
import type { Pagina } from "@/lib/consultas";
import { fechaHora } from "@/lib/formato";

type Notificacion = {
  id: number;
  tipo: string;
  titulo: string;
  mensaje: string;
  leida: boolean;
  fecha: string;
  ruta: string | null;
};

type Bandeja = { pagina: Pagina<Notificacion>; noLeidas: number };

/** HU-22: centro de notificaciones. Al abrir una notificación se navega al recurso y queda leída. */
export default function Notificaciones() {
  const router = useRouter();
  const queryClient = useQueryClient();
  const [soloNoLeidas, setSoloNoLeidas] = useState(false);
  const [pagina, setPagina] = useState(1);
  const bandeja = useQuery({
    queryKey: ["notificaciones", soloNoLeidas, pagina],
    queryFn: () => api<Bandeja>(`/notificaciones/mias?soloNoLeidas=${soloNoLeidas}&pagina=${pagina}`),
    placeholderData: keepPreviousData,
  });
  const refrescar = () => {
    queryClient.invalidateQueries({ queryKey: ["notificaciones"] });
    queryClient.invalidateQueries({ queryKey: ["no-leidas"] });
  };

  const leer = useMutation({ mutationFn: (id: number) => api(`/notificaciones/${id}/leer`, { method: "POST" }), onSuccess: refrescar });
  const leerTodas = useMutation({
    mutationFn: () => api("/notificaciones/leer-todas", { method: "POST" }),
    onSuccess: () => {
      toast.success("Todas las notificaciones quedaron leídas");
      refrescar();
    },
    onError: (e) => toast.error(e.message),
  });

  const abrir = async (n: Notificacion) => {
    if (!n.leida) await leer.mutateAsync(n.id).catch(() => undefined);
    if (n.ruta) router.push(n.ruta);
  };

  const datos = bandeja.data;
  const totalPaginas = datos ? Math.max(1, Math.ceil(datos.pagina.total / datos.pagina.tamanoPagina)) : 1;

  return (
    <>
      <Encabezado titulo="Notificaciones">
        <div className="flex flex-wrap items-center gap-3">
          <label className="flex items-center gap-2 text-sm text-slate-700">
            <input type="checkbox" checked={soloNoLeidas} onChange={(e) => { setSoloNoLeidas(e.target.checked); setPagina(1); }} />
            Solo no leídas
          </label>
          <Boton variante="secundario" disabled={!datos?.noLeidas || leerTodas.isPending} onClick={() => leerTodas.mutate()}>
            Marcar todas como leídas
          </Boton>
        </div>
      </Encabezado>
      {bandeja.isPending ? (
        <Cargando />
      ) : bandeja.isError ? (
        <EstadoError mensaje={bandeja.error.message} onReintentar={() => bandeja.refetch()} />
      ) : datos!.pagina.items.length === 0 ? (
        <EstadoVacio titulo={soloNoLeidas ? "No tiene notificaciones sin leer" : "No tiene notificaciones"} />
      ) : (
        <>
          <p className="mb-3 text-sm text-slate-600">{datos!.noLeidas} sin leer</p>
          <ul className="flex flex-col gap-2">
            {datos!.pagina.items.map((n) => (
              <li key={n.id}>
                <button
                  type="button"
                  onClick={() => abrir(n)}
                  className={`w-full rounded-lg border px-4 py-3 text-left hover:bg-slate-50 ${n.leida ? "border-slate-200 bg-white" : "border-emerald-300 bg-emerald-50"}`}
                >
                  <div className="flex flex-wrap items-baseline justify-between gap-2">
                    <span className={`text-sm ${n.leida ? "text-slate-800" : "font-semibold text-slate-900"}`}>{n.titulo}</span>
                    <span className="text-xs text-slate-500">{fechaHora(n.fecha)}</span>
                  </div>
                  <p className="mt-1 text-sm text-slate-700">{n.mensaje}</p>
                </button>
              </li>
            ))}
          </ul>
          <nav aria-label="Paginación" className="mt-4 flex items-center justify-between text-sm text-slate-600">
            <span>Página {datos!.pagina.paginaActual} de {totalPaginas}</span>
            <div className="flex gap-2">
              <Boton variante="secundario" disabled={pagina <= 1} onClick={() => setPagina((p) => p - 1)}>Anterior</Boton>
              <Boton variante="secundario" disabled={pagina >= totalPaginas} onClick={() => setPagina((p) => p + 1)}>Siguiente</Boton>
            </div>
          </nav>
        </>
      )}
    </>
  );
}
