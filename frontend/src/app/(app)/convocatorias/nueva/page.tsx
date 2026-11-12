"use client";

import { useQueryClient } from "@tanstack/react-query";
import { useRouter } from "next/navigation";
import { toast } from "sonner";
import { Encabezado } from "@/components/ui";
import { api, json } from "@/lib/api";
import { FormularioConvocatoria } from "../formulario-convocatoria";

export default function NuevaConvocatoria() {
  const router = useRouter();
  const queryClient = useQueryClient();
  return (
    <>
      <Encabezado titulo="Nueva convocatoria" />
      <FormularioConvocatoria
        textoGuardar="Registrar convocatoria"
        onCancelar={() => router.push("/convocatorias")}
        onGuardar={async (cuerpo) => {
          const { id } = await api<{ id: number }>("/convocatorias", json("POST", cuerpo));
          await queryClient.invalidateQueries({ queryKey: ["convocatorias"] });
          toast.success("Convocatoria registrada");
          router.push(`/convocatorias/${id}`);
        }}
      />
    </>
  );
}
