"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";

const enlaces = [
  { href: "/", label: "Inicio", match: (p: string) => p === "/" },
  { href: "/portal/proyectos", label: "Proyectos", match: (p: string) => p.startsWith("/portal/proyectos") },
  { href: "/portal/convocatorias", label: "Convocatorias", match: (p: string) => p.startsWith("/portal/convocatorias") },
];

export function NavPortal() {
  const pathname = usePathname();

  const cerrarMenu = () => {
    const el = document.getElementById("portal-menu") as (HTMLElement & { hidePopover?: () => void }) | null;
    el?.hidePopover?.();
  };

  return (
    <nav aria-label="Portal" className="flex items-center gap-space-sm">
      <ul className="hidden xl:flex items-center gap-space-xs">
        {enlaces.map((item) => {
          const activo = item.match(pathname);
          return (
            <li key={item.href}>
              <Link
                href={item.href}
                aria-current={activo ? "page" : undefined}
                className={
                  activo
                    ? "px-space-sm py-space-xs rounded-lg transition-all text-primary font-semibold bg-surface-container-high"
                    : "px-space-sm py-space-xs rounded-lg font-body-md text-body-md text-on-surface-variant hover:text-on-surface hover:bg-surface-container transition-all"
                }
              >
                {item.label}
              </Link>
            </li>
          );
        })}
      </ul>

      <button
        type="button"
        popoverTarget="portal-menu"
        popoverTargetAction="toggle"
        aria-controls="portal-menu"
        className="xl:hidden inline-flex items-center gap-space-xs px-space-sm py-space-xs rounded-lg bg-surface-container text-primary font-body-sm font-semibold hover:bg-surface-variant transition-colors"
      >
        <span className="material-symbols-outlined text-[20px]" aria-hidden="true">menu</span>
        <span>Menú</span>
      </button>

      <div
        id="portal-menu"
        popover="auto"
        className="xl:hidden fixed inset-auto top-20 right-4 w-[min(80dvw,20rem)] max-h-[calc(100dvh-6rem)] overflow-auto rounded-xl bg-surface-container-lowest p-space-md shadow-xl border border-outline-variant/40"
      >
        <ul className="flex flex-col gap-space-xs">
          {enlaces.map((item) => {
            const activo = item.match(pathname);
            return (
              <li key={item.href}>
                <Link
                  href={item.href}
                  onClick={cerrarMenu}
                  aria-current={activo ? "page" : undefined}
                  className={
                    activo
                      ? "block px-space-md py-space-sm rounded-lg text-primary font-semibold bg-surface-container-high"
                      : "block px-space-md py-space-sm rounded-lg text-on-surface-variant hover:text-on-surface hover:bg-surface-container transition-colors"
                  }
                >
                  {item.label}
                </Link>
              </li>
            );
          })}
          <li className="pt-space-xs mt-space-xs border-t border-outline-variant/30 sm:hidden">
            <Link
              href="/login"
              onClick={cerrarMenu}
              className="block text-center px-space-md py-space-sm rounded-lg bg-primary text-on-primary font-headline-sm text-body-sm hover:bg-primary-container transition-colors"
            >
              Iniciar Sesión
            </Link>
          </li>
        </ul>
      </div>
    </nav>
  );
}
