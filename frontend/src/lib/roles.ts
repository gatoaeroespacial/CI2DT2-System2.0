export const ROLES = {
  Administrador: "Administrador",
  Director: "Director",
  Coordinador: "Coordinador",
  Investigador: "Investigador",
  Externo: "Externo",
} as const;

export type Rol = (typeof ROLES)[keyof typeof ROLES];

export const TODOS_LOS_ROLES: Rol[] = Object.values(ROLES);

const { Administrador, Director, Coordinador, Investigador, Externo } = ROLES;

export type ItemMenu = { href: string; etiqueta: string; roles: Rol[] };

/**
 * Menú lateral según el Anexo A. Ocultar opciones es solo una ayuda de interfaz:
 * la autorización real se valida en el backend en cada petición.
 */
export const MENU: ItemMenu[] = [
  { href: "/tablero-centro", etiqueta: "Tablero del centro", roles: [Administrador, Director] },
  { href: "/proyectos", etiqueta: "Proyectos", roles: [Administrador, Director, Coordinador, Investigador] },
  { href: "/usuarios", etiqueta: "Usuarios", roles: [Administrador] },
  { href: "/notificaciones", etiqueta: "Notificaciones", roles: [Administrador, Director, Coordinador, Investigador, Externo] },
];

/** Los permisos de un usuario con varios roles son la unión de los permisos de sus roles. */
export function tieneAlgunRol(rolesUsuario: readonly string[], permitidos: readonly Rol[]): boolean {
  return permitidos.some((rol) => rolesUsuario.includes(rol));
}

export function menuPara(rolesUsuario: readonly string[]): ItemMenu[] {
  return MENU.filter((item) => tieneAlgunRol(rolesUsuario, item.roles));
}

/** Roles con acceso a una ruta del menú (o a sus subrutas). */
export function rolesDeRuta(ruta: string): Rol[] | undefined {
  return MENU.find((item) => ruta === item.href || ruta.startsWith(`${item.href}/`))?.roles;
}

/**
 * Página de inicio según el rol (HU-01 CA-1). El tablero del Coordinador pasó a Release 2:
 * el Coordinador inicia en el listado de proyectos de su programa.
 */
export function inicioPara(rolesUsuario: readonly string[]): string {
  if (rolesUsuario.includes(Director)) return "/tablero-centro";
  if (rolesUsuario.includes(Coordinador)) return "/proyectos";
  if (rolesUsuario.includes(Investigador)) return "/proyectos";
  if (rolesUsuario.includes(Administrador)) return "/usuarios";
  return "/notificaciones";
}
