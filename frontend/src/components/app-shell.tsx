"use client";

import { useQuery, useQueryClient } from "@tanstack/react-query";
import Image from "next/image";
import Link from "next/link";
import { usePathname } from "next/navigation";
import { useEffect, useRef, useState, type ReactNode } from "react";
import { api, apiAuth, renovarSesion } from "@/lib/api";
import logoCi2dt2 from "@/assets/img/LOGO_CI2DT2.png";
import { useAuth } from "@/lib/auth-store";
import { fechaLarga } from "@/lib/fechas";
import { menuPara, rolesDeRuta, tieneAlgunRol } from "@/lib/roles";
import { Boton, Cargando, EstadoError } from "./ui";

function iconoMenu(href: string) {
  const iconos: Record<string, string> = {
    "/asistente": "smart_toy",
    "/tablero-centro": "dashboard",
    "/proyectos": "folder",
    "/convocatorias": "campaign",
    "/avances-por-validar": "task_alt",
    "/alertas": "notifications_active",
    "/programas": "account_tree",
    "/aliados": "handshake",
    "/usuarios": "group",
    "/notificaciones": "notifications",
  };
  return iconos[href] ?? "circle";
}

function IconoMenu({ nombre, className = "h-5 w-5" }: { nombre: string; className?: string }) {
  const trazoComun = {
    fill: "none",
    stroke: "currentColor",
    strokeWidth: 1.8,
    strokeLinecap: "round" as const,
    strokeLinejoin: "round" as const,
  };

  let contenido: ReactNode;
  switch (nombre) {
    case "chevron_right":
      contenido = <path d="m9 18 6-6-6-6" />;
      break;
    case "chevron_left":
      contenido = <path d="m15 18-6-6 6-6" />;
      break;
    case "menu":
      contenido = <path d="M4 6h16M4 12h16M4 18h16" />;
      break;
    case "close":
      contenido = <path d="m6 6 12 12M18 6 6 18" />;
      break;
    case "smart_toy":
      contenido = (
        <>
          <rect x="4" y="7" width="16" height="13" rx="3" />
          <path d="M12 7V3m-2 0h4M8 12h.01M16 12h.01M9 16h6" />
        </>
      );
      break;
    case "dashboard":
      contenido = (
        <>
          <rect x="3" y="3" width="7" height="7" rx="1" />
          <rect x="14" y="3" width="7" height="7" rx="1" />
          <rect x="3" y="14" width="7" height="7" rx="1" />
          <rect x="14" y="14" width="7" height="7" rx="1" />
        </>
      );
      break;
    case "folder":
      contenido = <path d="M3 7a2 2 0 0 1 2-2h5l2 2h7a2 2 0 0 1 2 2v9a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2Z" />;
      break;
    case "campaign":
      contenido = (
        <>
          <path d="M3 11v2a2 2 0 0 0 2 2h3l7 4V5l-7 4H5a2 2 0 0 0-2 2Z" />
          <path d="M8 15v4a2 2 0 0 0 2 2h2M18 9a4 4 0 0 1 0 6M21 6a8 8 0 0 1 0 12" />
        </>
      );
      break;
    case "task_alt":
      contenido = (
        <>
          <circle cx="12" cy="12" r="9" />
          <path d="m8 12 2.5 2.5L16.5 8" />
        </>
      );
      break;
    case "notifications_active":
    case "notifications":
      contenido = (
        <>
          <path d="M18 8a6 6 0 0 0-12 0c0 7-3 9-3 9h18s-3-2-3-9" />
          <path d="M10 21h4" />
        </>
      );
      break;
    case "account_tree":
      contenido = (
        <>
          <circle cx="12" cy="5" r="2" />
          <circle cx="5" cy="19" r="2" />
          <circle cx="19" cy="19" r="2" />
          <path d="M12 7v4M5 17v-3h14v3M12 11v3" />
        </>
      );
      break;
    case "handshake":
      contenido = (
        <>
          <path d="m8 12 4 4 7-7-3-3-4 4-2-2-5 5 3 3" />
          <path d="m6 7-3 3 5 5m10-7 3 3-5 5" />
          <path d="m10 15 2 2m2-2 2 2" />
        </>
      );
      break;
    case "group":
      contenido = (
        <>
          <circle cx="9" cy="8" r="3" />
          <circle cx="17" cy="9" r="2" />
          <path d="M3 20v-2a6 6 0 0 1 12 0v2M14 14a5 5 0 0 1 7 4v2" />
        </>
      );
      break;
    default:
      contenido = <circle cx="12" cy="12" r="8" />;
  }

  return (
    <svg
      aria-hidden="true"
      viewBox="0 0 24 24"
      className={className}
      {...trazoComun}
    >
      {contenido}
    </svg>
  );
}

/** Layout autenticado: valida la sesión, muestra el menú según el rol y protege cada ruta. */
export function AppShell({ children }: { children: ReactNode }) {
  const usuario = useAuth((s) => s.usuario);
  const queryClient = useQueryClient();
  const ruta = usePathname();
  const [menuAbierto, setMenuAbierto] = useState(false);
  const [menuColapsado, setMenuColapsado] = useState(false);
  const [confirmarCierreSesion, setConfirmarCierreSesion] = useState(false);
  const botonCerrarSesionRef = useRef<HTMLButtonElement>(null);
  const botonCancelarCierreRef = useRef<HTMLButtonElement>(null);
  const botonConfirmarCierreRef = useRef<HTMLButtonElement>(null);

  const alternarMenuColapsado = () => {
    setMenuColapsado((actual) => !actual);
  };

  // Al recargar la página la sesión se recupera con el token de refresco (cookie HttpOnly).
  useEffect(() => {
    if (usuario) return;
    void renovarSesion().then((ok) => {
      if (!ok) window.location.replace("/login?sesion=expirada");
    });
  }, [usuario]);

  useEffect(() => {
    if (!confirmarCierreSesion) return;

    const botonCerrarSesion = botonCerrarSesionRef.current;
    const manejarTecla = (evento: KeyboardEvent) => {
      if (evento.key === "Escape") {
        evento.preventDefault();
        setConfirmarCierreSesion(false);
      } else if (evento.key === "Tab") {
        const cancelar = botonCancelarCierreRef.current;
        const confirmar = botonConfirmarCierreRef.current;
        if (evento.shiftKey && document.activeElement === cancelar) {
          evento.preventDefault();
          confirmar?.focus();
        } else if (!evento.shiftKey && document.activeElement === confirmar) {
          evento.preventDefault();
          cancelar?.focus();
        }
      }
    };

    botonCancelarCierreRef.current?.focus();
    window.addEventListener("keydown", manejarTecla);
    return () => {
      window.removeEventListener("keydown", manejarTecla);
      botonCerrarSesion?.focus();
    };
  }, [confirmarCierreSesion]);

  if (!usuario) return <Cargando texto="Verificando la sesión…" />;

  const cerrarSesion = async () => {
    await apiAuth("/logout").catch(() => undefined);
    queryClient.clear();
    // La recarga completa descarta el estado en memoria (token y sesión), así el botón
    // "atrás" no muestra datos protegidos. No se limpia el store antes para que el guard
    // no lo tome como una sesión expirada.
    window.location.replace("/");
  };

  const permitidos = rolesDeRuta(ruta);
  const autorizado = !permitidos || tieneAlgunRol(usuario.roles, permitidos);
  const menu = menuPara(usuario.roles);

  return (
    <div className={`flex ${ruta === "/asistente" ? "h-dvh overflow-hidden" : "min-h-screen"} flex-col md:flex-row`}>
      <aside
        className={`${menuColapsado ? "md:w-20" : "md:w-64"} fixed inset-y-0 left-0 z-40 hidden h-dvh flex-col border-r border-slate-200 bg-emerald-950 text-emerald-50 transition-[width] duration-300 ease-in-out md:flex`}
        aria-label="Navegación lateral"
      >
        <div className={`flex h-[72px] items-center border-b border-emerald-900 ${menuColapsado ? "justify-center px-2" : "justify-between px-5"}`}>
          <div className="flex min-w-0 items-center gap-3">
            <Image
              src={logoCi2dt2}
              alt="Logo CI²DT²"
              width={40}
              height={40}
              className="h-10 w-10 shrink-0 rounded-md object-contain"
            />
            {!menuColapsado && (
              <div className="min-w-0">
                <p className="text-lg font-semibold leading-tight">CI²DT²</p>
                <p className="truncate text-xs text-emerald-200">Universidad de Caldas</p>
              </div>
            )}
          </div>
          <button
            type="button"
            className="absolute bottom-4 left-1/2 hidden -translate-x-1/2 rounded-md p-2 text-emerald-100 hover:bg-emerald-900 md:block"
            aria-expanded={!menuColapsado}
            aria-controls="menu-principal"
            aria-label={menuColapsado ? "Expandir menú principal" : "Colapsar menú principal"}
            title={menuColapsado ? "Expandir menú" : "Colapsar menú"}
            onClick={alternarMenuColapsado}
          >
            <IconoMenu
              nombre={menuColapsado ? "chevron_right" : "chevron_left"}
              className="h-5 w-5"
            />
          </button>
        </div>
        <nav id="menu-principal" aria-label="Menú principal" className="block px-3 pb-16 pt-3">
          <ul className="flex flex-col gap-1">
            {menu.map((item) => {
              const activo = ruta === item.href || ruta.startsWith(`${item.href}/`);
              return (
                <li key={item.href}>
                  <Link
                    href={item.href}
                    aria-current={activo ? "page" : undefined}
                    onClick={() => setMenuAbierto(false)}
                    title={menuColapsado ? item.etiqueta : undefined}
                    className={`group relative flex rounded-md py-2 text-sm ${menuColapsado ? "justify-center px-2" : "items-center gap-3 px-3"} ${activo ? "bg-emerald-800 font-medium" : "hover:bg-emerald-900"}`}
                  >
                    <IconoMenu nombre={iconoMenu(item.href)} className="h-5 w-5 shrink-0" />
                    {!menuColapsado && <span>{item.etiqueta}</span>}
                    {menuColapsado && (
                      <span
                        role="tooltip"
                        className="pointer-events-none absolute left-[calc(100%+8px)] top-1/2 z-50 -translate-y-1/2 whitespace-nowrap rounded-md bg-slate-900 px-2 py-1 text-xs font-normal text-white opacity-0 shadow-lg transition-opacity group-hover:opacity-100 group-focus-visible:opacity-100"
                      >
                        {item.etiqueta}
                      </span>
                    )}
                  </Link>
                </li>
              );
            })}
          </ul>
        </nav>
      </aside>

      <aside className="sticky top-0 z-40 border-b border-slate-200 bg-emerald-950 text-emerald-50 md:hidden">
        <div className="flex h-16 items-center justify-between px-4">
          <div className="flex items-center gap-3">
            <Image src={logoCi2dt2} alt="Logo CI²DT²" width={36} height={36} className="h-9 w-9 rounded-md object-contain" />
            <div>
              <p className="text-base font-semibold leading-tight">CI²DT²</p>
              <p className="text-[10px] text-emerald-200">Universidad de Caldas</p>
            </div>
          </div>
          <button
            type="button"
            className="rounded-md p-2"
            aria-expanded={menuAbierto}
            aria-controls="menu-principal-movil"
            aria-label={menuAbierto ? "Cerrar menú principal" : "Abrir menú principal"}
            onClick={() => setMenuAbierto((v) => !v)}
          >
            <IconoMenu
              nombre={menuAbierto ? "close" : "menu"}
              className="h-6 w-6"
            />
          </button>
        </div>
        <nav
          id="menu-principal-movil"
          aria-label="Menú principal móvil"
          className={`${menuAbierto ? "block" : "hidden"} border-t border-emerald-900 px-3 pb-4 pt-2`}
        >
          <ul className="flex flex-col gap-1">
            {menu.map((item) => {
              const activo = ruta === item.href || ruta.startsWith(`${item.href}/`);
              return (
                <li key={item.href}>
                  <Link
                    href={item.href}
                    aria-current={activo ? "page" : undefined}
                    onClick={() => setMenuAbierto(false)}
                    className={`flex items-center gap-3 rounded-md px-3 py-2 text-sm ${activo ? "bg-emerald-800 font-medium" : "hover:bg-emerald-900"}`}
                  >
                    <IconoMenu nombre={iconoMenu(item.href)} className="h-5 w-5 shrink-0" />
                    <span>{item.etiqueta}</span>
                  </Link>
                </li>
              );
            })}
          </ul>
        </nav>
      </aside>

      <div className={`flex min-w-0 flex-1 flex-col transition-[margin] duration-300 ease-in-out ${menuColapsado ? "md:ml-20" : "md:ml-64"} ${ruta === "/asistente" ? "min-h-0" : ""}`}>
        <header className="flex flex-wrap items-center justify-between gap-3 border-b border-outline-variant/50 bg-surface-container-lowest px-4 py-3 shadow-sm md:px-8">
          <p className="rounded-lg px-3 py-2 font-body-sm text-body-sm text-on-surface-variant first-letter:uppercase">
            {fechaLarga(new Date())}
          </p>
          <div className="ml-auto flex flex-wrap items-center justify-end gap-3 sm:gap-4">
            <Campana ruta={ruta} />
            <div className="max-w-48 text-right">
              <p className="truncate font-body-sm text-body-sm font-semibold text-on-surface">
                {usuario.nombres} {usuario.apellidos}
              </p>
              <p className="truncate text-xs text-on-surface-variant">{usuario.roles.join(", ")}</p>
            </div>
            <button
              ref={botonCerrarSesionRef}
              type="button"
              onClick={() => setConfirmarCierreSesion(true)}
              className="inline-flex cursor-pointer items-center justify-center rounded-lg border border-outline-variant bg-surface px-4 py-2 text-sm font-semibold text-primary shadow-sm transition hover:border-error hover:bg-error hover:text-on-error focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-primary"
            >
              Cerrar sesión
            </button>
          </div>
        </header>
        <main className={`flex-1 px-4 py-6 md:px-8 ${ruta === "/asistente" ? "flex min-h-0 flex-col overflow-hidden" : ""}`}>
          {autorizado ? children : <EstadoError mensaje="No tiene permisos para ver esta página." />}
        </main>
        <AvisoPoliticaDatos />
        {confirmarCierreSesion && (
          <div
            className="fixed inset-0 z-[60] flex items-center justify-center bg-on-surface/40 p-4"
            onClick={(evento) => {
              if (evento.target === evento.currentTarget) setConfirmarCierreSesion(false);
            }}
          >
            <section
              role="dialog"
              aria-modal="true"
              aria-labelledby="titulo-confirmar-cierre"
              aria-describedby="descripcion-confirmar-cierre"
              className="w-full max-w-md rounded-xl bg-surface-container-lowest p-6 shadow-xl shadow-primary/10"
            >
              <p className="font-label-sm text-label-sm uppercase text-error">Confirmar salida</p>
              <h2 id="titulo-confirmar-cierre" className="mt-2 font-headline-sm text-headline-sm font-semibold text-on-surface">
                ¿Seguro que quieres cerrar sesión?
              </h2>
              <p id="descripcion-confirmar-cierre" className="mt-2 text-sm leading-relaxed text-on-surface-variant">
                Tendrás que iniciar sesión nuevamente para volver a la plataforma.
              </p>
              <div className="mt-6 flex flex-col-reverse gap-3 sm:flex-row sm:justify-end">
                <button
                  ref={botonCancelarCierreRef}
                  type="button"
                  onClick={() => setConfirmarCierreSesion(false)}
                  className="inline-flex cursor-pointer items-center justify-center rounded-lg border border-outline-variant bg-surface px-4 py-2 text-sm font-semibold text-primary shadow-sm transition hover:bg-surface-container-low focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-primary"
                >
                  No, continuar
                </button>
                <button
                  ref={botonConfirmarCierreRef}
                  type="button"
                  onClick={async () => {
                    setConfirmarCierreSesion(false);
                    await cerrarSesion();
                  }}
                  className="inline-flex cursor-pointer items-center justify-center rounded-lg border border-error bg-error px-4 py-2 text-sm font-semibold text-on-error shadow-sm transition hover:bg-error/90 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-error"
                >
                  Sí, cerrar sesión
                </button>
              </div>
            </section>
          </div>
        )}
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
    <Link
      href="/notificaciones"
      aria-label={`Notificaciones: ${n} sin leer`}
      className="relative flex h-10 w-10 items-center justify-center rounded-lg bg-surface-container-low text-primary transition hover:bg-primary-fixed/30 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-primary"
    >
      <svg aria-hidden="true" viewBox="0 0 24 24" className="h-5 w-5" fill="none" stroke="currentColor" strokeWidth="1.8">
        <path d="M6 8a6 6 0 1 1 12 0c0 7 3 9 3 9H3s3-2 3-9" />
        <path d="M10.3 21a1.94 1.94 0 0 0 3.4 0" />
      </svg>
      {n > 0 && (
        <span className="absolute -right-1 -top-1 min-w-5 rounded-full border-2 border-surface-container-lowest bg-error px-1 text-center text-xs font-semibold text-on-error">
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
