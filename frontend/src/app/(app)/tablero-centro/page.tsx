import { Encabezado, EstadoVacio } from "@/components/ui";

export default function TableroCentro() {
  return (
    <>
      <Encabezado titulo="Tablero del centro" />
      <EstadoVacio titulo="Aún no hay indicadores para mostrar" descripcion="El tablero del Director estará disponible próximamente." />
    </>
  );
}
