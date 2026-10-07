import { Suspense } from "react";
import { Cargando } from "@/components/ui";
import { EditarUsuario } from "./editar-usuario";

export default function PaginaEditarUsuario() {
  return (
    <Suspense fallback={<Cargando />}>
      <EditarUsuario />
    </Suspense>
  );
}
