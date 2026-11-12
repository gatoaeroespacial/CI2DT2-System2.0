"use client";

import { useQuery } from "@tanstack/react-query";
import Link from "next/link";
import { useRouter, useSearchParams } from "next/navigation";
import { useState, type FormEvent } from "react";
import { ErrorFormulario, ReglasContrasena } from "@/components/controles";
import { Boton, Campo, Cargando, claseEntrada } from "@/components/ui";
import { ApiError, apiAuth } from "@/lib/api";
import { reglasContrasena } from "@/lib/esquemas";

/** HU-03 CA-2 a CA-4: define la nueva contraseña con el enlace recibido por correo. */
export function FormularioRestablecer() {
  const router = useRouter();
  const token = useSearchParams().get("token") ?? "";
  const [contrasena, setContrasena] = useState("");
  const [confirmacion, setConfirmacion] = useState("");
  const [error, setError] = useState<string>();
  const [enviando, setEnviando] = useState(false);

  // CA-3: al abrir el enlace se verifica si sigue vigente y sin usar.
  const validez = useQuery({
    queryKey: ["validez-enlace", token],
    queryFn: async () => {
      const r = await fetch(`/api/v1/auth/reset-password/validar?token=${encodeURIComponent(token)}`);
      return r.ok ? ((await r.json()) as { valido: boolean }).valido : false;
    },
    enabled: token !== "",
  });

  const cumple = reglasContrasena.every((r) => r.cumple(contrasena));

  const enviar = async (e: FormEvent) => {
    e.preventDefault();
    setError(undefined);
    if (!cumple) return setError("La contraseña no cumple la política.");
    if (contrasena !== confirmacion) return setError("Las contraseñas no coinciden.");
    setEnviando(true);
    try {
      await apiAuth("/reset-password", { token, contrasena });
      router.replace("/login?contrasena=definida");
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "No fue posible conectar con el servidor. Intente nuevamente.");
    } finally {
      setEnviando(false);
    }
  };

  if (token === "" || validez.data === false)
    return (
      <div className="mt-4 flex flex-col gap-3">
        <p role="alert" className="rounded-md bg-amber-50 px-3 py-2 text-sm text-amber-900">
          El enlace para restablecer la contraseña expiró o ya fue utilizado.
        </p>
        <Link href="/recuperar-contrasena" className="text-sm text-emerald-800 hover:underline">Solicitar un nuevo enlace</Link>
      </div>
    );
  if (validez.isPending) return <Cargando texto="Verificando el enlace…" />;

  return (
    <form onSubmit={enviar} noValidate className="mt-4 flex flex-col gap-4">
      <ErrorFormulario mensaje={error} />
      <Campo id="contrasena" etiqueta="Nueva contraseña" obligatorio>
        <input id="contrasena" type="password" autoComplete="new-password" className={claseEntrada} value={contrasena}
          aria-describedby="reglas-contrasena" onChange={(e) => setContrasena(e.target.value)} />
      </Campo>
      <ReglasContrasena valor={contrasena} id="reglas-contrasena" />
      <Campo id="confirmacion" etiqueta="Confirmar contraseña" obligatorio>
        <input id="confirmacion" type="password" autoComplete="new-password" className={claseEntrada} value={confirmacion}
          onChange={(e) => setConfirmacion(e.target.value)} />
      </Campo>
      {/* CA-4: no se puede enviar mientras no cumpla la política. */}
      <Boton type="submit" disabled={enviando || !cumple || contrasena !== confirmacion}>
        {enviando ? "Guardando…" : "Guardar contraseña"}
      </Boton>
    </form>
  );
}
