"use client";

import { useQuery, useQueryClient } from "@tanstack/react-query";
import Link from "next/link";
import { usePathname } from "next/navigation";
import { useEffect, useState, type ReactNode } from "react";
import { api, apiAuth, renovarSesion } from "@/lib/api";
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
    <div className={`flex ${ruta === "/asistente" ? "h-dvh overflow-hidden" : "min-h-screen"} flex-col md:flex-row`}>
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

      <div className={`flex min-w-0 flex-1 flex-col ${ruta === "/asistente" ? "min-h-0" : ""}`}>
        <header className="flex flex-wrap items-center justify-between gap-3 border-b border-slate-200 bg-white px-6 py-3">
          <p className="text-sm text-slate-500 first-letter:uppercase">{fechaLarga(new Date())}</p>
          <div className="flex items-center gap-4">
            <Campana ruta={ruta} />
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
        <main className={`flex-1 px-4 py-6 md:px-8 ${ruta === "/asistente" ? "flex min-h-0 flex-col overflow-hidden" : ""}`}>
          {autorizado ? children : <EstadoError mensaje="No tiene permisos para ver esta página." />}
        </main>
        <AvisoPoliticaDatos />
      </div>
    </div>
  );
}

/** HU-22 CA-1: campana con el contador de no leídas; se consulta al cargar cada página. */
function Campana({ ruta }: { ruta: string }) {
  const conteo = useQuery({
    queryKey: ["no-leidas", ruta],
    queryFn: () => api<{ noLeidas: number }>("/notificaciones/no-leidas"),
  });
  const n = conteo.data?.noLeidas ?? 0;
  return (
    <Link href="/notificaciones" aria-label={`Notificaciones: ${n} sin leer`} className="relative rounded-md p-2 text-slate-700 hover:bg-slate-100">
      <svg aria-hidden="true" viewBox="0 0 24 24" className="h-5 w-5" fill="none" stroke="currentColor" strokeWidth="1.8">
        <path d="M6 8a6 6 0 1 1 12 0c0 7 3 9 3 9H3s3-2 3-9" />
        <path d="M10.3 21a1.94 1.94 0 0 0 3.4 0" />
      </svg>
      {n > 0 && (
        <span className="absolute -right-1 -top-1 min-w-5 rounded-full bg-red-600 px-1 text-center text-xs font-semibold text-white">
          {n > 99 ? "99+" : n}
        </span>
      )}
    </Link>
  );
}

/** Ley 1581 de 2012: aceptación de la política de tratamiento de datos vigente. */
function AvisoPoliticaDatos() {
  const queryClient = useQueryClient();
  const estado = useQuery({
    queryKey: ["consentimiento"],
    queryFn: () => api<{ politicaId: number | null; version: string | null; textoUrl: string | null; aceptado: boolean }>("/consentimientos/estado"),
    staleTime: Infinity,
  });
  const [guardando, setGuardando] = useState(false);
  if (!estado.data || estado.data.aceptado) return null;

  const aceptar = async () => {
    setGuardando(true);
    try {
      await api("/consentimientos", { method: "POST" });
      await queryClient.invalidateQueries({ queryKey: ["consentimiento"] });
    } finally {
      setGuardando(false);
    }
  };

  return (
    <div role="dialog" aria-modal="true" aria-labelledby="titulo-politica" className="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/50 p-4">
      <div className="max-w-lg rounded-lg bg-white p-6 shadow-xl">
        <h2 id="titulo-politica" className="text-lg font-semibold text-slate-900">Política de tratamiento de datos personales</h2>
        <p className="mt-2 text-sm text-slate-700">
          Para usar la plataforma debe aceptar la política de tratamiento de datos personales de la Universidad de Caldas
          (versión {estado.data.version}), conforme a la Ley 1581 de 2012.
        </p>
        <a href={estado.data.textoUrl ?? "/politica-datos"} target="_blank" rel="noopener noreferrer" className="mt-2 inline-block text-sm text-emerald-800 hover:underline">
          Leer la política
        </a>
        <div className="mt-4 flex justify-end">
          <Boton disabled={guardando} onClick={aceptar}>Acepto la política</Boton>
        </div>
      </div>
    </div>
  );
}
