import { z } from "zod";
import { ROLES, TODOS_LOS_ROLES } from "./roles";

/** Validaciones del cliente; el servidor aplica las mismas reglas con FluentValidation. */

export const esquemaLogin = z.object({
  email: z.string().trim().min(1, "El correo es obligatorio.").email("El correo no tiene un formato válido."),
  password: z.string().min(1, "La contraseña es obligatoria."),
});
export type DatosLogin = z.infer<typeof esquemaLogin>;

export const reglasContrasena = [
  { mensaje: "Al menos 10 caracteres", cumple: (v: string) => v.length >= 10 },
  { mensaje: "Al menos una mayúscula", cumple: (v: string) => /\p{Lu}/u.test(v) },
  { mensaje: "Al menos una minúscula", cumple: (v: string) => /\p{Ll}/u.test(v) },
  { mensaje: "Al menos un número", cumple: (v: string) => /\d/.test(v) },
];

export const esquemaDefinirContrasena = z
  .object({
    nuevaContrasena: z
      .string()
      .refine((v) => reglasContrasena.every((r) => r.cumple(v)), "La contraseña no cumple la política."),
    confirmacion: z.string(),
  })
  .refine((d) => d.nuevaContrasena === d.confirmacion, {
    path: ["confirmacion"],
    message: "Las contraseñas no coinciden.",
  });
export type DatosDefinirContrasena = z.infer<typeof esquemaDefinirContrasena>;

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

export const esquemaUsuario = z
  .object({
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
  })
  .refine((d) => !d.roles.includes(ROLES.Externo) || d.aliadoId !== "", {
    path: ["aliadoId"],
    message: "Un usuario Externo debe estar asociado a un aliado.",
  });
export type DatosUsuario = z.infer<typeof esquemaUsuario>;

/** Convierte el formulario al cuerpo de la API (cadenas vacías como null). */
export function aPeticionUsuario(datos: DatosUsuario) {
  const nulo = (v: string) => (v.trim() === "" ? null : v.trim());
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
