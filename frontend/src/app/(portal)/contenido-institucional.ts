/**
 * Contenido institucional estático de la página de inicio (HU-25), versionado en el frontend.
 * Las secciones vacías no se muestran. El propósito, los 5 objetivos estratégicos, los servicios
 * y los datos de contacto deben diligenciarse con el texto oficial validado por el Director del centro.
 */
export const contenidoInstitucional = {
  nombre: "CI²DT²",
  nombreCompleto: "Centro de Investigación, Innovación, Desarrollo y Transferencia de Tecnología",
  institucion: "Universidad de Caldas",
  /** Propósito del centro (texto oficial). */
  proposito: "",
  /** Los 5 objetivos estratégicos del centro (texto oficial). */
  objetivosEstrategicos: [] as string[],
  /** Servicios y capacidades que el centro ofrece a empresas y entidades (texto oficial). */
  servicios: [] as string[],
  contacto: {
    correo: "",
    telefono: "",
  },
};
