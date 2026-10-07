import { useAuth, type RespuestaSesion } from "./auth-store";

/** Las rutas /api/v1 se sirven desde el mismo dominio del frontend (proxy de Next.js hacia la API). */
const BASE = "/api/v1";

export const MENSAJE_SESION_EXPIRADA = "Su sesión expiró. Inicie sesión nuevamente";

export class ApiError extends Error {
  constructor(
    public readonly status: number,
    mensaje: string,
    public readonly errores?: Record<string, string[]>,
  ) {
    super(mensaje);
  }
}

type CuerpoError = { error?: string; errores?: Record<string, string[]> };

async function leerError(respuesta: Response): Promise<ApiError> {
  const cuerpo = (await respuesta.json().catch(() => ({}))) as CuerpoError;
  const porDefecto =
    respuesta.status === 403
      ? "No tiene permisos para realizar esta acción."
      : respuesta.status === 404
        ? "El recurso solicitado no existe."
        : "Ocurrió un error inesperado. Intente nuevamente.";
  return new ApiError(respuesta.status, cuerpo.error ?? porDefecto, cuerpo.errores);
}

let renovacionEnCurso: Promise<boolean> | null = null;

/**
 * Renueva la sesión con el token de refresco (cookie HttpOnly). Garantiza una sola renovación
 * concurrente en la pestaña y, con Web Locks, entre pestañas: la rotación del token invalida
 * el anterior, así que dos renovaciones simultáneas se tomarían como reutilización.
 */
export function renovarSesion(): Promise<boolean> {
  renovacionEnCurso ??= conBloqueo(async () => {
    const respuesta = await fetch(`${BASE}/auth/refresh`, { method: "POST", credentials: "same-origin" });
    if (!respuesta.ok) {
      useAuth.getState().limpiar();
      return false;
    }
    useAuth.getState().establecerSesion((await respuesta.json()) as RespuestaSesion);
    return true;
  }).finally(() => {
    renovacionEnCurso = null;
  });
  return renovacionEnCurso;
}

function conBloqueo<T>(tarea: () => Promise<T>): Promise<T> {
  if (typeof navigator !== "undefined" && navigator.locks) {
    // request() resuelve con el valor de la tarea ya esperado.
    return navigator.locks.request("ci2dt2-renovar-sesion", tarea) as Promise<T>;
  }
  return tarea();
}

function irAlLoginPorSesionExpirada() {
  if (typeof window !== "undefined" && !window.location.pathname.startsWith("/login")) {
    window.location.replace("/login?sesion=expirada");
  }
}

/** Cliente de la API: agrega el token y, ante un 401, renueva la sesión y reintenta una vez. */
export async function api<T>(ruta: string, opciones: RequestInit = {}): Promise<T> {
  const ejecutar = () => {
    const headers = new Headers(opciones.headers);
    const token = useAuth.getState().accessToken;
    if (token) headers.set("Authorization", `Bearer ${token}`);
    if (opciones.body && !headers.has("Content-Type")) headers.set("Content-Type", "application/json");
    return fetch(`${BASE}${ruta}`, { ...opciones, headers, credentials: "same-origin" });
  };

  let respuesta = await ejecutar();
  if (respuesta.status === 401) {
    if (!(await renovarSesion())) {
      irAlLoginPorSesionExpirada();
      throw new ApiError(401, MENSAJE_SESION_EXPIRADA);
    }
    respuesta = await ejecutar();
  }

  if (!respuesta.ok) throw await leerError(respuesta);
  if (respuesta.status === 204) return undefined as T;
  return (await respuesta.json()) as T;
}

/** Peticiones de autenticación (sin token ni reintento). */
export async function apiAuth<T>(ruta: string, cuerpo?: unknown): Promise<T> {
  const respuesta = await fetch(`${BASE}/auth${ruta}`, {
    method: "POST",
    credentials: "same-origin",
    headers: cuerpo === undefined ? undefined : { "Content-Type": "application/json" },
    body: cuerpo === undefined ? undefined : JSON.stringify(cuerpo),
  });
  if (!respuesta.ok) throw await leerError(respuesta);
  if (respuesta.status === 204) return undefined as T;
  return (await respuesta.json()) as T;
}
