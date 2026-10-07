"use client";

import { useQueryClient } from "@tanstack/react-query";
import { useRouter } from "next/navigation";
import { toast } from "sonner";
import { Encabezado } from "@/components/ui";
import { api } from "@/lib/api";
import { aPeticionUsuario } from "@/lib/esquemas";
import { FormularioUsuario } from "../formulario-usuario";

export default function NuevoUsuario() {
  const router = useRouter();
  const queryClient = useQueryClient();

  return (
    <>
      <Encabezado titulo="Nuevo usuario" />
      <FormularioUsuario
        textoGuardar="Crear usuario"
        onGuardar={async (datos) => {
          await api("/usuarios", { method: "POST", body: JSON.stringify({ ...aPeticionUsuario(datos), roles: datos.roles }) });
          await queryClient.invalidateQueries({ queryKey: ["usuarios"] });
          toast.success("Usuario creado. Se envió el correo de bienvenida.");
          router.push("/usuarios");
        }}
      />
    </>
  );
}
