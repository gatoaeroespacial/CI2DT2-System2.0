import { describe, expect, it } from "vitest";
import { aPeticionUsuario, esquemaDefinirContrasena, esquemaLogin, esquemaUsuario, type DatosUsuario } from "./esquemas";
import { fechaLarga } from "./fechas";

const usuario = (cambios: Partial<DatosUsuario> = {}): DatosUsuario => ({
  nombres: "Ana",
  apellidos: "Pérez",
  tipoDocumento: "CC",
  numeroDocumento: "123",
  email: "ana@ucaldas.edu.co",
  telefono: "",
  cargo: "",
  dependencia: "",
  cvLacUrl: "",
  orcid: "",
  aliadoId: "",
  roles: ["Investigador"],
  ...cambios,
});

const valido = (d: DatosUsuario) => esquemaUsuario.safeParse(d).success;

describe("login", () => {
  it("exige un correo válido y la contraseña", () => {
    expect(esquemaLogin.safeParse({ email: "no-es-correo", password: "x" }).success).toBe(false);
    expect(esquemaLogin.safeParse({ email: "a@b.co", password: "" }).success).toBe(false);
    expect(esquemaLogin.safeParse({ email: "a@b.co", password: "x" }).success).toBe(true);
  });
});

describe("usuario (HU-04)", () => {
  it("valida el formato ORCID 0000-0000-0000-000X", () => {
    expect(valido(usuario({ orcid: "0000-0002-1694-233X" }))).toBe(true);
    expect(valido(usuario({ orcid: "0000-0002-1694-233" }))).toBe(false);
  });

  it("acepta CvLAC solo del dominio scienti.minciencias.gov.co", () => {
    expect(valido(usuario({ cvLacUrl: "https://scienti.minciencias.gov.co/cvlac/x" }))).toBe(true);
    expect(valido(usuario({ cvLacUrl: "https://scienti.minciencias.gov.co.falso.com/cvlac" }))).toBe(false);
    expect(valido(usuario({ cvLacUrl: "no es url" }))).toBe(false);
  });

  it("un Externo debe tener aliado", () => {
    expect(valido(usuario({ roles: ["Externo"] }))).toBe(false);
    expect(valido(usuario({ roles: ["Externo"], aliadoId: "4" }))).toBe(true);
  });

  it("exige al menos un rol", () => {
    expect(valido(usuario({ roles: [] }))).toBe(false);
  });

  it("convierte los campos vacíos en null para la API", () => {
    const peticion = aPeticionUsuario(usuario({ telefono: "  ", aliadoId: "7" }));
    expect(peticion.telefono).toBeNull();
    expect(peticion.aliadoId).toBe(7);
  });
});

describe("política de contraseña", () => {
  const ok = (nuevaContrasena: string, confirmacion = nuevaContrasena) =>
    esquemaDefinirContrasena.safeParse({ nuevaContrasena, confirmacion }).success;

  it("mínimo 10 caracteres con mayúscula, minúscula y número", () => {
    expect(ok("Valida12345")).toBe(true);
    expect(ok("Corta1A")).toBe(false);
    expect(ok("sinmayuscula1")).toBe(false);
    expect(ok("SINMINUSCULA1")).toBe(false);
    expect(ok("SinNumeroAqui")).toBe(false);
  });

  it("la confirmación debe coincidir", () => {
    expect(ok("Valida12345", "Valida123456")).toBe(false);
  });
});

describe("fechas en español", () => {
  it("formatea con date-fns en español", () => {
    expect(fechaLarga(new Date(2026, 9, 7))).toBe("miércoles, 7 de octubre de 2026");
  });
});
