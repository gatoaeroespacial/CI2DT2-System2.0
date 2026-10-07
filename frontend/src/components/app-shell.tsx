"use client";

import { useQueryClient } from "@tanstack/react-query";
import Link from "next/link";
import { usePathname } from "next/navigation";
import { useEffect, useState, type ReactNode } from "react";
import { apiAuth, renovarSesion } from "@/lib/api";
import { useAuth } from "@/lib/auth-store";
import { fechaLarga } from "@/lib/fechas";
import { menuPara, rolesDeRuta, tieneAlgunRol } from "@/lib/roles";
import { Boton, Cargando, EstadoError } from "./ui";

/** Layout autenticado: valida la sesión, muestra el menú según el rol y protege cada ruta. */
export function AppShell({ children }: { children: ReactNode }) {
  const usuario = useAuth((s) => s.usuario);
  const queryClient = useQueryClient();
  const ruta = usePathname();
  const [menuAbierto, setMenuAbierto] = useState(false);

  // Al recargar la página la sesión se recupera con el token de refresco (cookie HttpOnly).
  useEffect(() => {
    if (usuario) return;
    renovarSesion().then((ok) => {
      if (!ok) window.location.replace("/login?sesion=expirada");
    });
  }, [usuario]);

  if (!usuario) return <Cargando texto="Verificando la sesión…" />;

  const cerrarSesion = async () => {
    await apiAuth("/logout").catch(() => undefined);
    queryClient.clear();
    // La recarga completa descarta el estado en memoria (token y sesión), así el botón
    // "atrás" no muestra datos protegidos. No se limpia el store antes para que el guard
    // no lo tome como una sesión expirada.
    window.location.replace("/login");
  };

  const permitidos = rolesDeRuta(ruta);
  const autorizado = !permitidos || tieneAlgunRol(usuario.roles, permitidos);
  const menu = menuPara(usuario.roles);

  return (
    <div className="flex min-h-screen flex-col md:flex-row">
      <aside className="border-b border-slate-200 bg-emerald-950 text-emerald-50 md:w-64 md:shrink-0 md:border-b-0">
        <div className="flex items-center justify-between px-5 py-4">
          <div>
            <p className="text-lg font-semibold">CI²DT²</p>
            <p className="text-xs text-emerald-200">Universidad de Caldas</p>
          </div>
          <button
            type="button"
            className="rounded p-2 text-sm md:hidden"
            aria-expanded={menuAbierto}
            aria-controls="menu-principal"
            onClick={() => setMenuAbierto((v) => !v)}
          >
            Menú
          </button>
        </div>
        <nav id="menu-principal" aria-label="Menú principal" className={`${menuAbierto ? "block" : "hidden"} px-3 pb-4 md:block`}>
          <ul className="flex flex-col gap-1">
            {menu.map((item) => {
              const activo = ruta === item.href || ruta.startsWith(`${item.href}/`);
              return (
                <li key={item.href}>
                  <Link
                    href={item.href}
                    aria-current={activo ? "page" : undefined}
                    onClick={() => setMenuAbierto(false)}
                    className={`block rounded-md px-3 py-2 text-sm ${activo ? "bg-emerald-800 font-medium" : "hover:bg-emerald-900"}`}
                  >
                    {item.etiqueta}
                  </Link>
                </li>
              );
            })}
          </ul>
        </nav>
      </aside>

      <div className="flex min-w-0 flex-1 flex-col">
        <header className="flex flex-wrap items-center justify-between gap-3 border-b border-slate-200 bg-white px-6 py-3">
          <p className="text-sm text-slate-500 first-letter:uppercase">{fechaLarga(new Date())}</p>
          <div className="flex items-center gap-4">
            <div className="text-right">
              <p className="text-sm font-medium text-slate-900">
                {usuario.nombres} {usuario.apellidos}
              </p>
              <p className="text-xs text-slate-500">{usuario.roles.join(", ")}</p>
            </div>
            <Boton variante="secundario" onClick={cerrarSesion}>
              Cerrar sesión
            </Boton>
          </div>
        </header>
        <main className="flex-1 px-4 py-6 md:px-8">
          {autorizado ? children : <EstadoError mensaje="No tiene permisos para ver esta página." />}
        </main>
      </div>
    </div>
  );
}
