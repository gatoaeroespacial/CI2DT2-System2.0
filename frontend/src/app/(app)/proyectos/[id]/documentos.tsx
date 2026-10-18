"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useRef, useState, type FormEvent } from "react";
import { toast } from "sonner";
import { ErrorFormulario } from "@/components/controles";
import { Boton, Campo, Cargando, claseEntrada, EstadoError, EstadoVacio, Tabla, type Columna } from "@/components/ui";
import { api, descargarDocumento } from "@/lib/api";
import { useCatalogos } from "@/lib/consultas";
import { FORMATOS_DOCUMENTO, validarArchivo } from "@/lib/esquemas";
import { fechaCorta } from "@/lib/formato";
import type { Permisos } from "./tipos";

type Documento = {
  id: number;
  tipoDocumento: string;
  nombreArchivo: string;
  hashSha256: string;
  version: number;
  subidoPor: string;
  fechaSubida: string;
};

/** HU-21: documentos del proyecto con huella SHA-256 y descarga por URL firmada temporal. */
export function Documentos({ proyectoId, permisos }: { proyectoId: number; permisos: Permisos }) {
  const queryClient = useQueryClient();
  const catalogos = useCatalogos();
  const entrada = useRef<HTMLInputElement>(null);
  const [tipo, setTipo] = useState("");
  const [error, setError] = useState<string>();
  const clave = ["documentos", "Proyecto", proyectoId];
  const documentos = useQuery({
    queryKey: clave,
    queryFn: () => api<Documento[]>(`/documentos?entidadTipo=Proyecto&entidadId=${proyectoId}`),
  });

  const cargar = useMutation({
    mutationFn: (datos: FormData) => api("/documentos", { method: "POST", body: datos }),
    onSuccess: () => {
      toast.success("Documento cargado");
      setTipo("");
      if (entrada.current) entrada.current.value = "";
      queryClient.invalidateQueries({ queryKey: clave });
    },
    onError: (e) => setError(e.message),
  });

  const descargar = useMutation({ mutationFn: descargarDocumento, onError: (e) => toast.error(e.message) });

  const enviar = (e: FormEvent) => {
    e.preventDefault();
    setError(undefined);
    const archivo = entrada.current?.files?.[0];
    if (!tipo) return setError("Seleccione el tipo de documento.");
    if (!archivo) return setError("Seleccione un archivo.");
    const invalido = validarArchivo(archivo.name, archivo.size);
    if (invalido) return setError(invalido);
    const datos = new FormData();
    datos.append("archivo", archivo);
    datos.append("tipoDocumentoId", tipo);
    datos.append("entidadTipo", "Proyecto");
    datos.append("entidadId", String(proyectoId));
    cargar.mutate(datos);
  };

  const columnas: Columna<Documento>[] = [
    { titulo: "Archivo", celda: (d) => d.nombreArchivo },
    { titulo: "Tipo", celda: (d) => d.tipoDocumento },
    { titulo: "Versión", celda: (d) => d.version },
    { titulo: "Subido por", celda: (d) => d.subidoPor },
    { titulo: "Fecha", celda: (d) => fechaCorta(d.fechaSubida) },
    { titulo: "SHA-256", celda: (d) => <span className="font-mono text-xs" title={d.hashSha256}>{d.hashSha256.slice(0, 12)}…</span> },
    {
      titulo: "Acciones",
      celda: (d) => (
        <button type="button" className="text-sm text-emerald-800 hover:underline" onClick={() => descargar.mutate(d.id)}>
          Descargar
        </button>
      ),
    },
  ];

  return (
    <section className="flex flex-col gap-4 rounded-lg border border-slate-200 bg-white p-6">
      {documentos.isPending ? (
        <Cargando />
      ) : documentos.isError ? (
        <EstadoError mensaje={documentos.error.message} onReintentar={() => documentos.refetch()} />
      ) : documentos.data.length === 0 ? (
        <EstadoVacio titulo="El proyecto no tiene documentos" />
      ) : (
        <Tabla columnas={columnas} filas={documentos.data} clave={(d) => d.id} />
      )}
      {permisos.puedeGestionarDocumentos && (
        <form onSubmit={enviar} noValidate className="flex flex-col gap-3 border-t border-slate-100 pt-4">
          <h3 className="text-sm font-semibold text-slate-800">Cargar documento</h3>
          <ErrorFormulario mensaje={error} />
          <div className="grid gap-3 md:grid-cols-2">
            <Campo id="tipoDocumentoId" etiqueta="Tipo de documento" obligatorio>
              <select id="tipoDocumentoId" className={claseEntrada} value={tipo} onChange={(e) => setTipo(e.target.value)}>
                <option value="">Seleccione…</option>
                {catalogos.data?.tiposDocumento.map((t) => (
                  <option key={t.id} value={t.id}>
                    {t.nombre}
                  </option>
                ))}
              </select>
            </Campo>
            <Campo id="archivo" etiqueta="Archivo (máx. 25 MB: PDF, DOCX, XLSX, PPTX, JPG, PNG o ZIP)" obligatorio>
              <input id="archivo" ref={entrada} type="file" accept={FORMATOS_DOCUMENTO.join(",")} className="text-sm" />
            </Campo>
          </div>
          <div>
            <Boton type="submit" disabled={cargar.isPending}>{cargar.isPending ? "Cargando…" : "Cargar"}</Boton>
          </div>
        </form>
      )}
    </section>
  );
}
