"use client";

import { useQueryClient } from "@tanstack/react-query";
import { useRouter } from "next/navigation";
import { toast } from "sonner";
import { Encabezado } from "@/components/ui";
import { api, json } from "@/lib/api";
import { aPeticionAliado } from "@/lib/esquemas";
import { FormularioAliado } from "../formulario-aliado";

export default function NuevoAliado() {
  const router = useRouter();
  const queryClient = useQueryClient();
  return (
    <>
      <Encabezado titulo="Nuevo aliado" />
      <FormularioAliado
        textoGuardar="Registrar aliado"
        onGuardar={async (datos) => {
          await api("/aliados", json("POST", aPeticionAliado(datos)));
          await queryClient.invalidateQueries({ queryKey: ["aliados"] });
          toast.success("Aliado registrado");
          router.push("/aliados");
        }}
      />
    </>
  );
}
