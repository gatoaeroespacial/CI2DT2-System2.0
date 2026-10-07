import { Encabezado, EstadoVacio } from "@/components/ui";

export default function Proyectos() {
  return (
    <>
      <Encabezado titulo="Proyectos" />
      <EstadoVacio titulo="Aún no hay proyectos registrados" descripcion="El registro y la consulta de proyectos estarán disponibles próximamente." />
    </>
  );
}
