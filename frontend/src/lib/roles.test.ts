import { describe, expect, it } from "vitest";
import { inicioPara, menuPara, rolesDeRuta, tieneAlgunRol } from "./roles";

const etiquetas = (roles: string[]) => menuPara(roles).map((i) => i.etiqueta);

describe("menú por rol (HU-05 CA-3)", () => {
  it("el Investigador no ve Usuarios ni el Tablero del centro", () => {
    expect(etiquetas(["Investigador"])).toEqual(["Proyectos", "Notificaciones"]);
  });

  it("el Administrador ve Usuarios y el Tablero del centro", () => {
    expect(etiquetas(["Admin"])).toEqual(["Asistente IA", "Tablero del centro", "Proyectos", "Programas", "Aliados", "Usuarios", "Notificaciones"]);
  });

  it("el Externo solo ve sus notificaciones", () => {
    expect(etiquetas(["Externo"])).toEqual(["Notificaciones"]);
  });

  it("con varios roles el menú es la unión (CA-4)", () => {
    expect(etiquetas(["Investigador", "Director"])).toEqual(["Asistente IA", "Tablero del centro", "Proyectos", "Programas", "Aliados", "Notificaciones"]);
  });

  it("el Coordinador gestiona programas y aliados, pero no usuarios", () => {
    expect(etiquetas(["Coordinador"])).toEqual(["Proyectos", "Programas", "Aliados", "Notificaciones"]);
  });

  it("protege las subrutas con los roles de la opción del menú", () => {
    expect(rolesDeRuta("/usuarios/15")).toEqual(["Admin"]);
    expect(tieneAlgunRol(["Coordinador"], rolesDeRuta("/usuarios/nuevo")!)).toBe(false);
    expect(rolesDeRuta("/asistente")).toEqual(["Admin", "Director"]);
    expect(tieneAlgunRol(["Investigador"], rolesDeRuta("/asistente")!)).toBe(false);
    expect(rolesDeRuta("/login")).toBeUndefined();
  });
});

describe("página de inicio según el rol (HU-01 CA-1)", () => {
  it.each([
    [["Director"], "/tablero-centro"],
    [["Coordinador"], "/proyectos"],
    [["Investigador"], "/proyectos"],
    [["Admin"], "/usuarios"],
    [["Externo"], "/notificaciones"],
    [["Admin", "Director"], "/tablero-centro"],
  ])("%j inicia en %s", (roles, ruta) => {
    expect(inicioPara(roles)).toBe(ruta);
  });
});
