import type { ButtonHTMLAttributes, ReactNode } from "react";

type Variante = "primario" | "secundario" | "peligro";

const estilos: Record<Variante, string> = {
  primario: "bg-emerald-800 text-white hover:bg-emerald-900 disabled:bg-emerald-800/60",
  secundario: "border border-slate-300 bg-white text-slate-800 hover:bg-slate-50 disabled:text-slate-400",
  peligro: "border border-red-300 bg-white text-red-700 hover:bg-red-50 disabled:text-red-300",
};

export function Boton({
  variante = "primario",
  className = "",
  ...props
}: ButtonHTMLAttributes<HTMLButtonElement> & { variante?: Variante }) {
  return (
    <button
      type="button"
      className={`inline-flex items-center justify-center rounded-md px-4 py-2 text-sm font-medium transition-colors focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-emerald-700 disabled:cursor-not-allowed ${estilos[variante]} ${className}`}
      {...props}
    />
  );
}

export const claseEntrada =
  "w-full rounded-md border border-slate-300 bg-white px-3 py-2 text-sm text-slate-900 focus:border-emerald-700 focus:outline-none focus:ring-2 focus:ring-emerald-700/20 aria-[invalid=true]:border-red-500";

/** Campo de formulario accesible: etiqueta, control y mensaje de error asociados. */
export function Campo({
  id,
  etiqueta,
  error,
  obligatorio,
  children,
}: {
  id: string;
  etiqueta: string;
  error?: string;
  obligatorio?: boolean;
  children: ReactNode;
}) {
  return (
    <div className="flex flex-col gap-1">
      <label htmlFor={id} className="text-sm font-medium text-slate-700">
        {etiqueta}
        {obligatorio && <span className="text-red-600"> *</span>}
      </label>
      {children}
      {error && (
        <p id={`${id}-error`} role="alert" className="text-xs text-red-600">
          {error}
        </p>
      )}
    </div>
  );
}

export function Cargando({ texto = "Cargando…" }: { texto?: string }) {
  return (
    <div role="status" className="flex items-center gap-2 py-10 text-sm text-slate-600">
      <span className="h-4 w-4 animate-spin rounded-full border-2 border-emerald-700 border-t-transparent" />
      {texto}
    </div>
  );
}

export function EstadoVacio({ titulo, descripcion }: { titulo: string; descripcion?: string }) {
  return (
    <div className="rounded-lg border border-dashed border-slate-300 bg-white px-6 py-12 text-center">
      <p className="font-medium text-slate-800">{titulo}</p>
      {descripcion && <p className="mt-1 text-sm text-slate-600">{descripcion}</p>}
    </div>
  );
}

export function EstadoError({ mensaje, onReintentar }: { mensaje: string; onReintentar?: () => void }) {
  return (
    <div role="alert" className="rounded-lg border border-red-200 bg-red-50 px-6 py-6 text-center">
      <p className="text-sm text-red-800">{mensaje}</p>
      {onReintentar && (
        <Boton variante="secundario" className="mt-3" onClick={onReintentar}>
          Reintentar
        </Boton>
      )}
    </div>
  );
}

export function Encabezado({ titulo, children }: { titulo: string; children?: ReactNode }) {
  return (
    <div className="mb-6 flex flex-wrap items-center justify-between gap-3">
      <h1 className="text-2xl font-semibold text-slate-900">{titulo}</h1>
      {children}
    </div>
  );
}

export type Columna<T> = { titulo: string; celda: (fila: T) => ReactNode; className?: string };

/** Tabla genérica adaptable: en pantallas pequeñas se desplaza horizontalmente. */
export function Tabla<T>({ columnas, filas, clave }: { columnas: Columna<T>[]; filas: T[]; clave: (fila: T) => string | number }) {
  return (
    <div className="overflow-x-auto rounded-lg border border-slate-200 bg-white">
      <table className="min-w-full divide-y divide-slate-200 text-sm">
        <thead className="bg-slate-50">
          <tr>
            {columnas.map((c) => (
              <th key={c.titulo} scope="col" className={`px-4 py-3 text-left font-medium text-slate-700 ${c.className ?? ""}`}>
                {c.titulo}
              </th>
            ))}
          </tr>
        </thead>
        <tbody className="divide-y divide-slate-100">
          {filas.map((fila) => (
            <tr key={clave(fila)} className="hover:bg-slate-50">
              {columnas.map((c) => (
                <td key={c.titulo} className={`px-4 py-3 text-slate-800 ${c.className ?? ""}`}>
                  {c.celda(fila)}
                </td>
              ))}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
