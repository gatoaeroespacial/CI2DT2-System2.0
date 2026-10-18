"use client";

import { useQueryClient } from "@tanstack/react-query";
import { useRouter } from "next/navigation";
import { toast } from "sonner";
import { Encabezado } from "@/components/ui";
import { api, json } from "@/lib/api";
import { aPeticionProyecto } from "@/lib/esquemas";
import { FormularioProyecto, proyectoVacio } from "../formulario-proyecto";

/** HU-08: la propuesta se guarda como borrador en estado "En elaboración". */
export default function NuevaPropuesta() {
  const router = useRouter();
  const queryClient = useQueryClient();
  return (
    <>
      <Encabezado titulo="Nueva propuesta" />
      <FormularioProyecto
        inicial={proyectoVacio}
        creando
        textoGuardar="Guardar borrador"
        onCancelar={() => router.push("/proyectos")}
        onGuardar={async (datos) => {
          const { id } = await api<{ id: number }>("/proyectos", json("POST", aPeticionProyecto(datos)));
          await queryClient.invalidateQueries({ queryKey: ["proyectos"] });
          toast.success("Propuesta registrada en estado En elaboración");
          router.push(`/proyectos/${id}`);
        }}
      />
    </>
  );
}
