"use client";

import { useState, type FormEvent } from "react";
import { Autocompletar, buscarAliados, ErrorFormulario } from "@/components/controles";
import { Boton, Campo, claseEntrada } from "@/components/ui";
import { useCatalogos } from "@/lib/consultas";
import { MONEDAS } from "@/lib/esquemas";
import { aFechaInput } from "@/lib/formato";

export type Convocatoria = {
  id: number;
  nombre: string;
  numero: string | null;
  tipoConvocatoriaId: number;
  tipoConvocatoria: string;
  entidadConvocanteId: number | null;
  entidadConvocante: string | null;
  plataforma: string;
  url: string | null;
  fechaApertura: string | null;
  fechaCierre: string | null;
  moneda: string | null;
  montoMaximo: number | null;
  estado: string;
};

export const PLATAFORMAS = ["Portal VIP", "SIGP", "MGA Web", "Plataforma del otorgante"];

type Datos = {
  nombre: string;
  numero: string;
  tipoConvocatoriaId: string;
  entidadConvocanteId: string;
  plataforma: string;
  url: string;
  fechaApertura: string;
  fechaCierre: string;
  moneda: string;
  montoMaximo: string;
};

const aDatos = (c?: Convocatoria): Datos => ({
  nombre: c?.nombre ?? "",
  numero: c?.numero ?? "",
  tipoConvocatoriaId: c ? String(c.tipoConvocatoriaId) : "",
  entidadConvocanteId: c?.entidadConvocanteId ? String(c.entidadConvocanteId) : "",
  plataforma: c?.plataforma ?? "",
  url: c?.url ?? "",
  fechaApertura: aFechaInput(c?.fechaApertura),
  fechaCierre: aFechaInput(c?.fechaCierre),
  moneda: c?.moneda ?? "COP",
  montoMaximo: c?.montoMaximo === null || c?.montoMaximo === undefined ? "" : String(c.montoMaximo),
});

/** Formulario del catálogo de convocatorias (HU-15). La entidad convocante sale del catálogo de aliados. */
export function FormularioConvocatoria({
  inicial,
  textoGuardar,
  onGuardar,
  onCancelar,
}: {
  inicial?: Convocatoria;
  textoGuardar: string;
  onGuardar: (cuerpo: unknown) => Promise<void>;
  onCancelar: () => void;
}) {
  const catalogos = useCatalogos();
  const [datos, setDatos] = useState<Datos>(aDatos(inicial));
  const [error, setError] = useState<string>();
  const [guardando, setGuardando] = useState(false);
  const cambiar = (campo: keyof Datos) => (e: { target: { value: string } }) => setDatos((d) => ({ ...d, [campo]: e.target.value }));

  const enviar = async (e: FormEvent) => {
    e.preventDefault();
    setError(undefined);
    if (!datos.nombre.trim() || !datos.tipoConvocatoriaId || !datos.plataforma.trim())
      return setError("El nombre, el tipo y la plataforma de radicación son obligatorios.");
    if (datos.fechaApertura && datos.fechaCierre && datos.fechaCierre < datos.fechaApertura)
      return setError("La fecha de cierre no puede ser anterior a la fecha de apertura.");
    if (datos.montoMaximo && !(Number(datos.montoMaximo) > 0)) return setError("El monto máximo debe ser mayor que 0.");
    setGuardando(true);
    try {
      await onGuardar({
        nombre: datos.nombre.trim(),
        numero: datos.numero.trim() || null,
        tipoConvocatoriaId: Number(datos.tipoConvocatoriaId),
        entidadConvocanteId: datos.entidadConvocanteId === "" ? null : Number(datos.entidadConvocanteId),
        plataforma: datos.plataforma.trim(),
        url: datos.url.trim() || null,
        fechaApertura: datos.fechaApertura || null,
        fechaCierre: datos.fechaCierre || null,
        moneda: datos.montoMaximo ? datos.moneda : null,
        montoMaximo: datos.montoMaximo ? Number(datos.montoMaximo) : null,
      });
    } catch (err) {
      setError(err instanceof Error ? err.message : "Ocurrió un error inesperado.");
    } finally {
      setGuardando(false);
    }
  };

  return (
    <form onSubmit={enviar} noValidate className="flex max-w-4xl flex-col gap-6 rounded-lg border border-slate-200 bg-white p-6">
      <ErrorFormulario mensaje={error} />
      <div className="grid gap-4 md:grid-cols-2">
        <Campo id="nombre" etiqueta="Nombre" obligatorio>
          <input id="nombre" className={claseEntrada} value={datos.nombre} onChange={cambiar("nombre")} />
        </Campo>
        <Campo id="numero" etiqueta="Número">
          <input id="numero" className={claseEntrada} value={datos.numero} onChange={cambiar("numero")} />
        </Campo>
        <Campo id="tipoConvocatoriaId" etiqueta="Tipo" obligatorio>
          <select id="tipoConvocatoriaId" className={claseEntrada} value={datos.tipoConvocatoriaId} onChange={cambiar("tipoConvocatoriaId")}>
            <option value="">Seleccione…</option>
            {catalogos.data?.tiposConvocatoria.map((t) => <option key={t.id} value={t.id}>{t.nombre}</option>)}
          </select>
        </Campo>
        <Campo id="entidadConvocanteId" etiqueta="Entidad convocante (aliado)">
          <Autocompletar id="entidadConvocanteId" valor={datos.entidadConvocanteId} etiquetaInicial={inicial?.entidadConvocante ?? undefined}
            onCambio={(v) => setDatos((d) => ({ ...d, entidadConvocanteId: v }))} buscar={buscarAliados} claveConsulta="buscar-aliados"
            placeholder="Nombre o NIT del aliado" />
        </Campo>
        <Campo id="plataforma" etiqueta="Plataforma de radicación" obligatorio>
          <input id="plataforma" list="plataformas" className={claseEntrada} value={datos.plataforma} onChange={cambiar("plataforma")} />
          <datalist id="plataformas">{PLATAFORMAS.map((p) => <option key={p} value={p} />)}</datalist>
        </Campo>
        <Campo id="url" etiqueta="URL de los términos de referencia">
          <input id="url" type="url" className={claseEntrada} placeholder="https://" value={datos.url} onChange={cambiar("url")} />
        </Campo>
        <Campo id="fechaApertura" etiqueta="Fecha de apertura">
          <input id="fechaApertura" type="date" className={claseEntrada} value={datos.fechaApertura} onChange={cambiar("fechaApertura")} />
        </Campo>
        <Campo id="fechaCierre" etiqueta="Fecha de cierre">
          <input id="fechaCierre" type="date" className={claseEntrada} value={datos.fechaCierre} onChange={cambiar("fechaCierre")} />
        </Campo>
        <Campo id="montoMaximo" etiqueta="Monto máximo">
          <input id="montoMaximo" type="number" min={0} className={claseEntrada} value={datos.montoMaximo} onChange={cambiar("montoMaximo")} />
        </Campo>
        <Campo id="moneda" etiqueta="Moneda">
          <select id="moneda" className={claseEntrada} value={datos.moneda} onChange={cambiar("moneda")}>
            {MONEDAS.map((m) => <option key={m} value={m}>{m}</option>)}
          </select>
        </Campo>
      </div>
      <div className="flex gap-3">
        <Boton type="submit" disabled={guardando}>{guardando ? "Guardando…" : textoGuardar}</Boton>
        <Boton variante="secundario" onClick={onCancelar}>Cancelar</Boton>
      </div>
    </form>
  );
}
