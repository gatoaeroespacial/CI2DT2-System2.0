"use client";

import { useQuery, useQueryClient } from "@tanstack/react-query";
import { useParams, useRouter } from "next/navigation";
import { toast } from "sonner";
import { Cargando, Encabezado, EstadoError } from "@/components/ui";
import { api } from "@/lib/api";
import { aPeticionUsuario } from "@/lib/esquemas";
import { aFormulario, FormularioUsuario } from "../formulario-usuario";
import type { UsuarioDetalle } from "../tipos";

export function EditarUsuario() {
  const { id } = useParams<{ id: string }>();
  const router = useRouter();
  const queryClient = useQueryClient();
  const usuario = useQuery({ queryKey: ["usuario", id], queryFn: () => api<UsuarioDetalle>(`/usuarios/${id}`) });

  if (usuario.isPending) return <Cargando />;
  if (usuario.isError) return <EstadoError mensaje={usuario.error.message} onReintentar={() => usuario.refetch()} />;

  const original = usuario.data;

  return (
    <>
      <Encabezado titulo={`Editar usuario · ${original.nombres} ${original.apellidos}`} />
      <FormularioUsuario
        inicial={aFormulario(original)}
        textoGuardar="Guardar cambios"
        onGuardar={async (datos) => {
          await api(`/usuarios/${id}`, { method: "PUT", body: JSON.stringify(aPeticionUsuario(datos)) });
          const rolesCambiaron = [...datos.roles].sort().join() !== [...original.roles].sort().join();
          if (rolesCambiaron) {
            await api(`/usuarios/${id}/roles`, { method: "PUT", body: JSON.stringify({ roles: datos.roles }) });
          }
          await queryClient.invalidateQueries({ queryKey: ["usuarios"] });
          await queryClient.invalidateQueries({ queryKey: ["usuario", id] });
          toast.success("Cambios guardados");
          router.push("/usuarios");
        }}
      />
    </>
  );
}
