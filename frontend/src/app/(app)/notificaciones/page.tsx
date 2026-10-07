import { Encabezado, EstadoVacio } from "@/components/ui";

export default function Notificaciones() {
  return (
    <>
      <Encabezado titulo="Notificaciones" />
      <EstadoVacio titulo="No tiene notificaciones" descripcion="El centro de notificaciones estará disponible próximamente." />
    </>
  );
}
