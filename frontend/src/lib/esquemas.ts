import { z } from "zod";
import { ROLES, TODOS_LOS_ROLES } from "./roles";

/** Validaciones del cliente; el servidor aplica las mismas reglas con FluentValidation. */

export const esquemaLogin = z.object({
  email: z.string().trim().min(1, "El correo es obligatorio.").email("El correo no tiene un formato válido."),
  password: z.string().min(1, "La contraseña es obligatoria."),
});
export type DatosLogin = z.infer<typeof esquemaLogin>;

/** Política de contraseña: mínimo 10 caracteres, con mayúscula, minúscula y número. */
export const reglasContrasena = [
  { mensaje: "Al menos 10 caracteres", cumple: (v: string) => v.length >= 10 },
  { mensaje: "Al menos una mayúscula", cumple: (v: string) => /\p{Lu}/u.test(v) },
  { mensaje: "Al menos una minúscula", cumple: (v: string) => /\p{Ll}/u.test(v) },
  { mensaje: "Al menos un número", cumple: (v: string) => /\d/.test(v) },
];

const contrasena = z
  .string()
  .refine((v) => reglasContrasena.every((r) => r.cumple(v)), "La contraseña no cumple la política.");

export const esquemaAsignarContrasena = z
  .object({ contrasena, confirmacion: z.string() })
  .refine((d) => d.contrasena === d.confirmacion, { path: ["confirmacion"], message: "Las contraseñas no coinciden." });
export type DatosAsignarContrasena = z.infer<typeof esquemaAsignarContrasena>;

// ----- Usuarios (HU-04) -----

export const TIPOS_DOCUMENTO = ["CC", "CE", "TI", "PA", "PEP"] as const;
export const PATRON_ORCID = /^\d{4}-\d{4}-\d{4}-\d{3}[\dX]$/;
export const DOMINIO_CVLAC = "scienti.minciencias.gov.co";

export function esUrlCvLac(valor: string): boolean {
  try {
    const url = new URL(valor);
    return (url.protocol === "https:" || url.protocol === "http:") && url.hostname.toLowerCase() === DOMINIO_CVLAC;
  } catch {
    return false;
  }
}

const opcional = (max: number) => z.string().trim().max(max, `Máximo ${max} caracteres.`);

const camposUsuario = {
  nombres: z.string().trim().min(1, "Los nombres son obligatorios.").max(100, "Máximo 100 caracteres."),
  apellidos: z.string().trim().min(1, "Los apellidos son obligatorios.").max(100, "Máximo 100 caracteres."),
  tipoDocumento: z.union([z.enum(TIPOS_DOCUMENTO), z.literal("")]),
  numeroDocumento: opcional(30),
  email: z.string().trim().min(1, "El correo es obligatorio.").email("El correo no tiene un formato válido.").max(150),
  telefono: opcional(30),
  cargo: opcional(100),
  dependencia: opcional(150),
  cvLacUrl: opcional(300).refine((v) => v === "" || esUrlCvLac(v), `El CvLAC debe ser una URL del dominio ${DOMINIO_CVLAC}.`),
  orcid: z
    .string()
    .trim()
    .refine((v) => v === "" || PATRON_ORCID.test(v), "El ORCID debe tener el formato 0000-0000-0000-000X."),
  aliadoId: z.string(),
  roles: z.array(z.enum(TODOS_LOS_ROLES as [string, ...string[]])).min(1, "Debe asignar al menos un rol."),
  // Solo se exige al crear (HU-04 CA-1); en la edición la contraseña se asigna aparte (CA-6).
  contrasena: z.string(),
};

const externoConAliado = (d: { roles: string[]; aliadoId: string }) => !d.roles.includes(ROLES.Externo) || d.aliadoId !== "";
const mensajeExterno = { path: ["aliadoId"], message: "Un usuario Externo debe estar asociado a un aliado." };

export const esquemaUsuario = z.object(camposUsuario).refine(externoConAliado, mensajeExterno);

export const esquemaUsuarioNuevo = z
  .object({ ...camposUsuario, contrasena })
  .refine(externoConAliado, mensajeExterno);

export type DatosUsuario = z.infer<typeof esquemaUsuario>;

/** Convierte el formulario al cuerpo de la API (cadenas vacías como null). */
export function aPeticionUsuario(datos: DatosUsuario) {
  return {
    nombres: datos.nombres.trim(),
    apellidos: datos.apellidos.trim(),
    tipoDocumento: nulo(datos.tipoDocumento),
    numeroDocumento: nulo(datos.numeroDocumento),
    email: datos.email.trim(),
    telefono: nulo(datos.telefono),
    cargo: nulo(datos.cargo),
    dependencia: nulo(datos.dependencia),
    cvLacUrl: nulo(datos.cvLacUrl),
    orcid: nulo(datos.orcid),
    aliadoId: datos.aliadoId === "" ? null : Number(datos.aliadoId),
  };
}

export const nulo = (v: string) => (v.trim() === "" ? null : v.trim());
const numeroONulo = (v: string) => (v.trim() === "" ? null : Number(v));

// ----- Aliados (HU-07) -----

/** Dígito de verificación del NIT (algoritmo de la DIAN). */
export function digitoVerificacionNit(numero: string): number {
  const pesos = [3, 7, 13, 17, 19, 23, 29, 37, 41, 43, 47, 53, 59, 67, 71];
  let suma = 0;
  for (let i = 0; i < numero.length; i++) suma += Number(numero[numero.length - 1 - i]) * pesos[i];
  const residuo = suma % 11;
  return residuo > 1 ? 11 - residuo : residuo;
}

export function nitValido(nit: string): boolean {
  const m = /^(\d{6,15})-(\d)$/.exec(nit.trim());
  return m !== null && digitoVerificacionNit(m[1]) === Number(m[2]);
}

export const esColombia = (pais: string) => pais.trim().toLowerCase() === "colombia";

export const esquemaAliado = z
  .object({
    tipoAliadoId: z.string().min(1, "Debe seleccionar el tipo de aliado."),
    nombre: z.string().trim().min(1, "El nombre es obligatorio.").max(200),
    nit: opcional(30),
    pais: z.string().trim().min(1, "El país es obligatorio.").max(60),
    ciudad: opcional(80),
    sector: opcional(80),
    representanteLegal: opcional(150),
    email: z.string().trim().min(1, "El correo es obligatorio.").email("El correo no tiene un formato válido.").max(150),
  })
  .refine((d) => !esColombia(d.pais) || nitValido(d.nit), {
    path: ["nit"],
    message: "El NIT es obligatorio para aliados colombianos y debe tener dígito de verificación válido (ej. 890800128-6).",
  });
export type DatosAliado = z.infer<typeof esquemaAliado>;

export const aPeticionAliado = (d: DatosAliado) => ({
  tipoAliadoId: Number(d.tipoAliadoId),
  nombre: d.nombre.trim(),
  nit: nulo(d.nit),
  pais: d.pais.trim(),
  ciudad: nulo(d.ciudad),
  sector: nulo(d.sector),
  representanteLegal: nulo(d.representanteLegal),
  email: d.email.trim(),
});

// ----- Programas (HU-06) -----

export const esquemaPrograma = z.object({
  nombre: z.string().trim().min(1, "El nombre es obligatorio.").max(150),
  descripcion: z.string(),
});
export type DatosPrograma = z.infer<typeof esquemaPrograma>;

const fechasOrdenadas = (d: { fechaInicio: string; fechaFin: string }) => !d.fechaInicio || !d.fechaFin || d.fechaFin >= d.fechaInicio;
const mensajeFechas = { path: ["fechaFin"], message: "La fecha de fin no puede ser anterior a la fecha de inicio." };

export const esquemaVinculacion = z
  .object({
    usuarioId: z.string().min(1, "Debe seleccionar un usuario."),
    rolEnPrograma: z.enum(["Coordinador", "Investigador"]),
    fechaInicio: z.string().min(1, "La fecha de inicio es obligatoria."),
    fechaFin: z.string(),
  })
  .refine(fechasOrdenadas, mensajeFechas);
export type DatosVinculacion = z.infer<typeof esquemaVinculacion>;

// ----- Proyectos (HU-08) -----

export const MONEDAS = ["COP", "USD", "EUR"] as const;

export const esquemaProyecto = z
  .object({
    titulo: z.string().trim().min(10, "El título debe tener entre 10 y 300 caracteres.").max(300, "El título debe tener entre 10 y 300 caracteres."),
    resumen: z.string().max(4000, "El resumen admite máximo 4.000 caracteres."),
    programaId: z.string().min(1, "Debe seleccionar el programa."),
    tipoProyectoId: z.string().min(1, "Debe seleccionar el tipo de proyecto."),
    investigadorPrincipalId: z.string(),
    fechaInicio: z.string(),
    fechaFin: z.string(),
    moneda: z.union([z.enum(MONEDAS), z.literal("")]),
    presupuestoTotal: z.string(),
    tasaCambio: z.string(),
  })
  .refine((d) => !d.fechaInicio || !d.fechaFin || d.fechaFin > d.fechaInicio, {
    path: ["fechaFin"],
    message: "La fecha de fin debe ser posterior a la fecha de inicio.",
  })
  .refine((d) => d.presupuestoTotal === "" || Number(d.presupuestoTotal) >= 0, {
    path: ["presupuestoTotal"],
    message: "El presupuesto no puede ser negativo.",
  })
  .refine((d) => d.presupuestoTotal === "" || d.moneda !== "", { path: ["moneda"], message: "Indique la moneda del presupuesto." })
  .refine((d) => d.moneda === "" || d.moneda === "COP" || Number(d.tasaCambio) > 0, {
    path: ["tasaCambio"],
    message: "La tasa de cambio es obligatoria y mayor que 0 cuando la moneda no es COP.",
  });
export type DatosProyecto = z.infer<typeof esquemaProyecto>;

/** Valor en pesos: igual al monto si es COP; si no, monto × tasa. */
export function enCop(monto: number | null, moneda: string, tasa: number | null): number | null {
  if (monto === null || Number.isNaN(monto)) return null;
  if (moneda === "COP") return monto;
  return tasa !== null && tasa > 0 ? Math.round(monto * tasa * 100) / 100 : null;
}

export const aPeticionProyecto = (d: DatosProyecto) => ({
  titulo: d.titulo.trim(),
  resumen: nulo(d.resumen),
  programaId: Number(d.programaId),
  tipoProyectoId: Number(d.tipoProyectoId),
  investigadorPrincipalId: numeroONulo(d.investigadorPrincipalId),
  fechaInicio: nulo(d.fechaInicio),
  fechaFin: nulo(d.fechaFin),
  moneda: d.moneda === "" ? null : d.moneda,
  presupuestoTotal: numeroONulo(d.presupuestoTotal),
  tasaCambio: d.moneda === "COP" ? null : numeroONulo(d.tasaCambio),
});

// ----- Equipo (HU-09) -----

export const ROLES_PROYECTO = ["IP", "Co-investigador", "Auxiliar de campo", "Estudiante"] as const;

export const esquemaIntegrante = z
  .object({
    usuarioId: z.string().min(1, "Debe seleccionar un usuario."),
    rolEnProyecto: z.enum(ROLES_PROYECTO),
    fechaInicio: z.string(),
    fechaFin: z.string(),
  })
  .refine(fechasOrdenadas, mensajeFechas);
export type DatosIntegrante = z.infer<typeof esquemaIntegrante>;

// ----- Financiación (HU-10) -----

export const TIPOS_FUENTE = ["Convocatoria", "Contrapartida", "Cofinanciación", "Recursos propios"] as const;
export const TIPOS_APORTE = ["Efectivo", "Especie"] as const;
export const ESTADOS_FUENTE = ["Estimado", "Comprometido", "Desembolsado"] as const;

export const esquemaFuente = z
  .object({
    aliadoId: z.string(),
    tipoFuente: z.enum(TIPOS_FUENTE),
    tipoAporte: z.enum(TIPOS_APORTE),
    moneda: z.enum(MONEDAS),
    monto: z.string().refine((v) => Number(v) > 0, "El monto debe ser mayor que 0."),
    tasaCambio: z.string(),
    estado: z.enum(ESTADOS_FUENTE),
  })
  .refine((d) => d.tipoFuente !== "Cofinanciación" || d.aliadoId !== "", {
    path: ["aliadoId"],
    message: "Una fuente de tipo Cofinanciación exige seleccionar el aliado.",
  })
  .refine((d) => d.moneda === "COP" || Number(d.tasaCambio) > 0, {
    path: ["tasaCambio"],
    message: "La tasa de cambio es obligatoria y mayor que 0 cuando la moneda no es COP.",
  });
export type DatosFuente = z.infer<typeof esquemaFuente>;

export const aPeticionFuente = (d: DatosFuente) => ({
  aliadoId: numeroONulo(d.aliadoId),
  tipoFuente: d.tipoFuente,
  tipoAporte: d.tipoAporte,
  moneda: d.moneda,
  monto: Number(d.monto),
  tasaCambio: d.moneda === "COP" ? null : numeroONulo(d.tasaCambio),
  estado: d.estado,
});

// ----- Documentos (HU-21) -----

export const FORMATOS_DOCUMENTO = [".pdf", ".docx", ".xlsx", ".pptx", ".jpg", ".jpeg", ".png", ".zip"];
export const TAMANO_MAXIMO_DOCUMENTO = 25 * 1024 * 1024;

export function validarArchivo(nombre: string, tamano: number): string | null {
  const extension = nombre.slice(nombre.lastIndexOf(".")).toLowerCase();
  if (!FORMATOS_DOCUMENTO.includes(extension)) return "Formato no permitido. Use PDF, DOCX, XLSX, PPTX, JPG, PNG o ZIP.";
  if (tamano > TAMANO_MAXIMO_DOCUMENTO) return "El archivo supera el tamaño máximo de 25 MB.";
  if (tamano === 0) return "El archivo está vacío.";
  return null;
}
