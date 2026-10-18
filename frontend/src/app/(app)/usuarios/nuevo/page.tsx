"use client";

import { useQueryClient } from "@tanstack/react-query";
import { useRouter } from "next/navigation";
import { toast } from "sonner";
import { Encabezado } from "@/components/ui";
import { api, json } from "@/lib/api";
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
          await api("/usuarios", json("POST", { ...aPeticionUsuario(datos), roles: datos.roles, contrasena: datos.contrasena }));
          await queryClient.invalidateQueries({ queryKey: ["usuarios"] });
          toast.success("Usuario creado. Ya puede iniciar sesión con la contraseña inicial.");
          router.push("/usuarios");
        }}
      />
    </>
  );
}
