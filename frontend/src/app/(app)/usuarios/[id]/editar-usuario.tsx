"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { useQuery, useQueryClient } from "@tanstack/react-query";
import { useParams, useRouter } from "next/navigation";
import { useForm, useWatch } from "react-hook-form";
import { toast } from "sonner";
import { ErrorFormulario, ReglasContrasena } from "@/components/controles";
import { Boton, Campo, Cargando, claseEntrada, Encabezado, EstadoError } from "@/components/ui";
import { api, ApiError, json } from "@/lib/api";
import { aPeticionUsuario, esquemaAsignarContrasena, type DatosAsignarContrasena } from "@/lib/esquemas";
import { aFormulario, FormularioUsuario } from "../formulario-usuario";
import type { UsuarioDetalle } from "../tipos";

export function EditarUsuario() {
  const { id } = useParams<{ id: string }>();
  const router = useRouter();
  const queryClient = useQueryClient();
  const usuario = useQuery({ queryKey: ["usuario", id], queryFn: () => api<UsuarioDetalle>(`/usuarios/${id}`) });
  const aliadoId = usuario.data?.aliadoId;
  const aliado = useQuery({
    queryKey: ["aliado", aliadoId],
    queryFn: () => api<{ nombre: string }>(`/aliados/${aliadoId}`),
    enabled: !!aliadoId,
  });

  if (usuario.isPending || (aliadoId && aliado.isPending)) return <Cargando />;
  if (usuario.isError) return <EstadoError mensaje={usuario.error.message} onReintentar={() => usuario.refetch()} />;

  const original = usuario.data;

  return (
    <>
      <Encabezado titulo={`Editar usuario · ${original.nombres} ${original.apellidos}`} />
      <div className="flex flex-col gap-6">
        <FormularioUsuario
          inicial={aFormulario(original)}
          nombreAliado={aliado.data?.nombre}
          textoGuardar="Guardar cambios"
          onGuardar={async (datos) => {
            await api(`/usuarios/${id}`, json("PUT", aPeticionUsuario(datos)));
            const rolesCambiaron = [...datos.roles].sort().join() !== [...original.roles].sort().join();
            if (rolesCambiaron) await api(`/usuarios/${id}/roles`, json("PUT", { roles: datos.roles }));
            await queryClient.invalidateQueries({ queryKey: ["usuarios"] });
            await queryClient.invalidateQueries({ queryKey: ["usuario", id] });
            toast.success("Cambios guardados");
            router.push("/usuarios");
          }}
        />
        <AsignarContrasena usuarioId={id} />
      </div>
    </>
  );
}

/** HU-04 CA-6: el Administrador asigna una nueva contraseña y se revocan las sesiones del usuario. */
function AsignarContrasena({ usuarioId }: { usuarioId: string }) {
  const {
    register,
    handleSubmit,
    control,
    reset,
    setError,
    formState: { errors, isSubmitting },
  } = useForm<DatosAsignarContrasena>({
    resolver: zodResolver(esquemaAsignarContrasena),
    defaultValues: { contrasena: "", confirmacion: "" },
  });
  const contrasena = useWatch({ control, name: "contrasena" });

  const enviar = handleSubmit(async ({ contrasena }) => {
    try {
      await api(`/usuarios/${usuarioId}/contrasena`, json("PUT", { contrasena }));
      reset();
      toast.success("Contraseña asignada. Se cerraron las sesiones activas del usuario.");
    } catch (e) {
      setError("root", { message: e instanceof ApiError ? e.message : "No fue posible conectar con el servidor." });
    }
  });

  return (
    <form onSubmit={enviar} noValidate className="flex max-w-3xl flex-col gap-4 rounded-lg border border-slate-200 bg-white p-6">
      <h2 className="text-lg font-semibold text-slate-900">Asignar nueva contraseña</h2>
      <ErrorFormulario mensaje={errors.root?.message} />
      <div className="grid gap-4 md:grid-cols-2">
        <Campo id="nuevaContrasena" etiqueta="Nueva contraseña" error={errors.contrasena?.message} obligatorio>
          <input id="nuevaContrasena" type="password" autoComplete="new-password" className={claseEntrada}
            aria-invalid={!!errors.contrasena} aria-describedby="reglas-nueva" {...register("contrasena")} />
          <ReglasContrasena valor={contrasena} id="reglas-nueva" />
        </Campo>
        <Campo id="confirmacion" etiqueta="Confirmar contraseña" error={errors.confirmacion?.message} obligatorio>
          <input id="confirmacion" type="password" autoComplete="new-password" className={claseEntrada}
            aria-invalid={!!errors.confirmacion} {...register("confirmacion")} />
        </Campo>
      </div>
      <div>
        <Boton type="submit" variante="secundario" disabled={isSubmitting}>
          {isSubmitting ? "Guardando…" : "Asignar contraseña"}
        </Boton>
      </div>
    </form>
  );
}
