"use client";

import { useQuery, useQueryClient } from "@tanstack/react-query";
import Link from "next/link";
import { useParams, useRouter } from "next/navigation";
import { useState, type ReactNode } from "react";
import { toast } from "sonner";
import { Boton, Cargando, Encabezado, EstadoError } from "@/components/ui";
import { api, json } from "@/lib/api";
import { useAuth } from "@/lib/auth-store";
import { fechaCorta, formatoMonto } from "@/lib/formato";
import { ROLES, tieneAlgunRol } from "@/lib/roles";
import { Documentos } from "../../proyectos/[id]/documentos";
import { FormularioConvocatoria, type Convocatoria } from "../formulario-convocatoria";

function Dato({ etiqueta, children }: { etiqueta: string; children: ReactNode }) {
  return (
    <div>
      <dt className="text-xs font-medium uppercase text-slate-500">{etiqueta}</dt>
      <dd className="text-sm text-slate-900">{children}</dd>
    </div>
  );
}

/** HU-15: detalle de la convocatoria, edición y términos de referencia adjuntos. */
export default function DetalleConvocatoria() {
  const { id } = useParams<{ id: string }>();
  const router = useRouter();
  const queryClient = useQueryClient();
  const roles = useAuth((s) => s.usuario?.roles ?? []);
  const gestiona = tieneAlgunRol(roles, [ROLES.Administrador, ROLES.Director, ROLES.Coordinador]);
  const [editando, setEditando] = useState(false);
  const convocatoria = useQuery({ queryKey: ["convocatoria", id], queryFn: () => api<Convocatoria>(`/convocatorias/${id}`) });

  if (convocatoria.isPending) return <Cargando />;
  if (convocatoria.isError) return <EstadoError mensaje={convocatoria.error.message} onReintentar={() => convocatoria.refetch()} />;
  const c = convocatoria.data;

  const eliminar = async () => {
    if (!window.confirm(`¿Eliminar la convocatoria "${c.nombre}"?`)) return;
    try {
      await api(`/convocatorias/${id}`, { method: "DELETE" });
      await queryClient.invalidateQueries({ queryKey: ["convocatorias"] });
      toast.success("Convocatoria eliminada");
      router.push("/convocatorias");
    } catch (e) {
      toast.error(e instanceof Error ? e.message : "No fue posible eliminarla.");
    }
  };

  return (
    <>
      <Encabezado titulo={c.nombre}>
        <Link href="/convocatorias" className="text-sm text-emerald-800 hover:underline">← Convocatorias</Link>
      </Encabezado>
      {editando ? (
        <FormularioConvocatoria
          inicial={c}
          textoGuardar="Guardar cambios"
          onCancelar={() => setEditando(false)}
          onGuardar={async (cuerpo) => {
            await api(`/convocatorias/${id}`, json("PUT", cuerpo));
            await queryClient.invalidateQueries({ queryKey: ["convocatoria", id] });
            await queryClient.invalidateQueries({ queryKey: ["convocatorias"] });
            toast.success("Cambios guardados");
            setEditando(false);
          }}
        />
      ) : (
        <section className="mb-6 flex flex-col gap-4 rounded-lg border border-slate-200 bg-white p-6">
          <dl className="grid gap-4 md:grid-cols-3">
            <Dato etiqueta="Estado">{c.estado}</Dato>
            <Dato etiqueta="Tipo">{c.tipoConvocatoria}</Dato>
            <Dato etiqueta="Número">{c.numero ?? "—"}</Dato>
            <Dato etiqueta="Entidad convocante">{c.entidadConvocante ?? "—"}</Dato>
            <Dato etiqueta="Plataforma de radicación">{c.plataforma}</Dato>
            <Dato etiqueta="Monto máximo">{formatoMonto(c.montoMaximo, c.moneda)}</Dato>
            <Dato etiqueta="Apertura">{fechaCorta(c.fechaApertura)}</Dato>
            <Dato etiqueta="Cierre">{fechaCorta(c.fechaCierre)}</Dato>
            <Dato etiqueta="Términos de referencia">
              {c.url ? <a href={c.url} target="_blank" rel="noopener noreferrer" className="text-emerald-800 hover:underline">Abrir enlace</a> : "—"}
            </Dato>
          </dl>
          {gestiona && (
            <div className="flex gap-3">
              <Boton variante="secundario" onClick={() => setEditando(true)}>Editar</Boton>
              <Boton variante="peligro" onClick={eliminar}>Eliminar</Boton>
            </div>
          )}
        </section>
      )}
      {gestiona && (
        <>
          <h2 className="mb-2 text-lg font-semibold text-slate-900">Documentos (términos de referencia)</h2>
          <Documentos proyectoId={c.id} entidadTipo="Convocatoria" permisos={{ puedeGestionarDocumentos: true }} />
        </>
      )}
    </>
  );
}
