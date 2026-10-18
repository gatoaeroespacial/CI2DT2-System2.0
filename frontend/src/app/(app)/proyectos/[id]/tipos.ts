export type Permisos = { puedeEditar: boolean; puedeCambiarIp: boolean; puedeGestionarDocumentos: boolean };

export type ProyectoDetalle = {
  id: number;
  codigo: string | null;
  titulo: string;
  resumen: string | null;
  programaId: number;
  programa: string;
  tipoProyectoId: number;
  tipoProyecto: string;
  estadoId: number;
  estado: string;
  estadoEsFinal: boolean;
  investigadorPrincipalId: number;
  investigadorPrincipal: string;
  fechaInicio: string | null;
  fechaFin: string | null;
  porcentajeAvance: number | null;
  moneda: string | null;
  presupuestoTotal: number | null;
  tasaCambio: number | null;
  presupuestoTotalCop: number | null;
  convocatorias: {
    id: number;
    convocatoria: string;
    fechaPresentacion: string | null;
    montoSolicitadoCop: number | null;
    resultado: string | null;
    faseExterna: string | null;
  }[];
  cronograma: {
    id: number;
    titulo: string;
    tipoHito: string;
    fechaLimite: string;
    estado: string;
    responsable: string | null;
    fechaCompletado: string | null;
  }[];
  avances: {
    id: number;
    titulo: string;
    autor: string;
    porcentajeAvance: number | null;
    fechaCaptura: string | null;
    estadoValidacion: string | null;
  }[];
  revisiones: {
    id: number;
    etapa: string;
    decision: string;
    revisor: string;
    estadoNuevo: string;
    observaciones: string | null;
    fecha: string;
  }[];
};

export type Ficha = { proyecto: ProyectoDetalle; permisos: Permisos };
