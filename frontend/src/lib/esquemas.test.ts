import { describe, expect, it } from "vitest";
import {
  aPeticionUsuario, digitoVerificacionNit, enCop, esquemaAliado, esquemaAsignarContrasena, esquemaFuente, esquemaLogin,
  esquemaProyecto, esquemaUsuario, esquemaUsuarioNuevo, nitValido, validarArchivo, type DatosAliado, type DatosFuente,
  type DatosProyecto, type DatosUsuario,
} from "./esquemas";
import { formatoCop } from "./formato";
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
  contrasena: "",
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
  const ok = (contrasena: string, confirmacion = contrasena) =>
    esquemaAsignarContrasena.safeParse({ contrasena, confirmacion }).success;

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

describe("usuario nuevo (HU-04)", () => {
  it("exige la contraseña inicial con la política", () => {
    expect(esquemaUsuarioNuevo.safeParse({ ...usuario(), contrasena: "corta" }).success).toBe(false);
    expect(esquemaUsuarioNuevo.safeParse({ ...usuario(), contrasena: "Valida12345" }).success).toBe(true);
  });
});

describe("aliado (HU-07)", () => {
  const aliado = (cambios: Partial<DatosAliado> = {}): DatosAliado => ({
    tipoAliadoId: "1", nombre: "Universidad de Caldas", nit: "890801063-0", pais: "Colombia",
    ciudad: "", sector: "", representanteLegal: "", email: "contacto@ucaldas.edu.co", ...cambios,
  });

  it("calcula el dígito de verificación de la DIAN", () => {
    expect(digitoVerificacionNit("890801063")).toBe(0);
    expect(nitValido("890801063-0")).toBe(true);
    expect(nitValido("890801063-1")).toBe(false);
  });

  it("exige NIT válido solo a aliados colombianos", () => {
    expect(esquemaAliado.safeParse(aliado()).success).toBe(true);
    expect(esquemaAliado.safeParse(aliado({ nit: "" })).success).toBe(false);
    expect(esquemaAliado.safeParse(aliado({ nit: "", pais: "España" })).success).toBe(true);
  });
});

describe("proyecto (HU-08)", () => {
  const proyecto = (cambios: Partial<DatosProyecto> = {}): DatosProyecto => ({
    titulo: "Propuesta de investigación", resumen: "", programaId: "1", tipoProyectoId: "1", investigadorPrincipalId: "",
    fechaInicio: "", fechaFin: "", moneda: "", presupuestoTotal: "", tasaCambio: "", ...cambios,
  });
  const valido = (d: DatosProyecto) => esquemaProyecto.safeParse(d).success;

  it("título entre 10 y 300 caracteres", () => {
    expect(valido(proyecto({ titulo: "Corto" }))).toBe(false);
    expect(valido(proyecto())).toBe(true);
  });

  it("la fecha de fin debe ser posterior a la de inicio", () => {
    expect(valido(proyecto({ fechaInicio: "2026-05-01", fechaFin: "2026-05-01" }))).toBe(false);
    expect(valido(proyecto({ fechaInicio: "2026-05-01", fechaFin: "2026-05-02" }))).toBe(true);
  });

  it("exige tasa de cambio si la moneda no es COP y convierte a pesos", () => {
    expect(valido(proyecto({ moneda: "USD", presupuestoTotal: "1000" }))).toBe(false);
    expect(valido(proyecto({ moneda: "USD", presupuestoTotal: "1000", tasaCambio: "4000" }))).toBe(true);
    expect(enCop(1000, "USD", 4000)).toBe(4_000_000);
    expect(enCop(1000, "COP", null)).toBe(1000);
  });

  it("formatea pesos colombianos", () => {
    expect(formatoCop(40_000_000)).toBe("$40.000.000 COP");
  });
});

describe("fuente de financiamiento (HU-10)", () => {
  const fuente = (cambios: Partial<DatosFuente> = {}): DatosFuente => ({
    aliadoId: "", tipoFuente: "Convocatoria", tipoAporte: "Efectivo", moneda: "COP", monto: "100", tasaCambio: "", estado: "Estimado", ...cambios,
  });

  it("la cofinanciación exige aliado", () => {
    expect(esquemaFuente.safeParse(fuente({ tipoFuente: "Cofinanciación" })).success).toBe(false);
    expect(esquemaFuente.safeParse(fuente({ tipoFuente: "Cofinanciación", aliadoId: "3" })).success).toBe(true);
  });

  it("el monto debe ser mayor que 0", () => {
    expect(esquemaFuente.safeParse(fuente({ monto: "0" })).success).toBe(false);
  });
});

describe("documentos (HU-21)", () => {
  it("valida formato y tamaño máximo de 25 MB", () => {
    expect(validarArchivo("acta.pdf", 1000)).toBeNull();
    expect(validarArchivo("script.exe", 1000)).not.toBeNull();
    expect(validarArchivo("grande.zip", 26 * 1024 * 1024)).not.toBeNull();
  });
});

describe("fechas en español", () => {
  it("formatea con date-fns en español", () => {
    expect(fechaLarga(new Date(2026, 9, 7))).toBe("miércoles, 7 de octubre de 2026");
  });
});
