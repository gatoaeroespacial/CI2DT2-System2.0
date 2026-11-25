"use client";

import { useEffect, useState } from "react";

const UMBRAL_PX = 300;

function scrollSuave() {
  const reducida = window.matchMedia("(prefers-reduced-motion: reduce)").matches;
  window.scrollTo({ top: 0, behavior: reducida ? "instant" : "smooth" });
}

/** Botón flotante "Volver al inicio" — aparece tras bajar 300 px. */
export function BotonVolverArriba() {
  const [visible, setVisible] = useState(false);

  useEffect(() => {
    // Solo el listener de scroll — setState se llama desde el callback (no del body del effect)
    const handler = () => setVisible(window.scrollY > UMBRAL_PX);
    window.addEventListener("scroll", handler, { passive: true });
    return () => window.removeEventListener("scroll", handler);
  }, []);

  return (
    <button
      type="button"
      onClick={scrollSuave}
      aria-label="Volver al inicio de la página"
      aria-hidden={!visible}
      tabIndex={visible ? 0 : -1}
      className={[
        "fixed bottom-6 right-6 z-40",
        "flex items-center justify-center w-11 h-11 rounded-full",
        "bg-primary text-on-primary shadow-[0_4px_12px_rgba(21,71,52,0.25)]",
        "hover:bg-primary-container hover:text-on-primary-container",
        "focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary focus-visible:ring-offset-2",
        "transition-all duration-300",
        visible
          ? "opacity-100 scale-100 pointer-events-auto"
          : "opacity-0 scale-75 pointer-events-none",
      ].join(" ")}
    >
      <span
        className="material-symbols-outlined select-none"
        aria-hidden="true"
        style={{ fontSize: "20px", fontVariationSettings: "'FILL' 1, 'wght' 500" }}
      >
        arrow_upward
      </span>
    </button>
  );
}
