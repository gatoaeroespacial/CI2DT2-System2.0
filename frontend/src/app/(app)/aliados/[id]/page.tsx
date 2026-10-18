"use client";

import { useQuery, useQueryClient } from "@tanstack/react-query";
import { useParams, useRouter } from "next/navigation";
import { toast } from "sonner";
import { Cargando, Encabezado, EstadoError } from "@/components/ui";
import { api, json } from "@/lib/api";
import { aPeticionAliado } from "@/lib/esquemas";
import { aFormularioAliado, FormularioAliado, type Aliado } from "../formulario-aliado";

export default function EditarAliado() {
  const { id } = useParams<{ id: string }>();
  const router = useRouter();
  const queryClient = useQueryClient();
  const aliado = useQuery({ queryKey: ["aliado", id], queryFn: () => api<Aliado>(`/aliados/${id}`) });

  if (aliado.isPending) return <Cargando />;
  if (aliado.isError) return <EstadoError mensaje={aliado.error.message} onReintentar={() => aliado.refetch()} />;

  return (
    <>
      <Encabezado titulo={`Editar aliado · ${aliado.data.nombre}`} />
      <FormularioAliado
        inicial={aFormularioAliado(aliado.data)}
        textoGuardar="Guardar cambios"
        onGuardar={async (datos) => {
          await api(`/aliados/${id}`, json("PUT", aPeticionAliado(datos)));
          await queryClient.invalidateQueries({ queryKey: ["aliados"] });
          await queryClient.invalidateQueries({ queryKey: ["aliado", id] });
          toast.success("Cambios guardados");
          router.push("/aliados");
        }}
      />
    </>
  );
}
