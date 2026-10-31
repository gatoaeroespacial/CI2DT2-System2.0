import { create } from "zustand";

export type ResumenConvocatoria = {
  entidad: string;
  objetivoConvocatoria: string;
  montoMaximo?: string;
  rubrosFinanciables: string[];
  requisitosElegibilidad: string[];
  criteriosEvaluacion: string[];
  entregablesEsperados: string[];
  fechaCierre?: string;
};

export type IdeaProyectoPropuesta = {
  id: string;
  titulo: string;
  descripcionBreve: string;
  justificacionAlineacion: string;
  impactoEsperado: string;
  palabrasClave: string[];
};

export type SeccionCanvas = {
  id?: number;
  codigoSeccion: string;
  tituloSeccion: string;
  contenido: string;
  orden: number;
  estado: "Vacio" | "Generado" | "Modificado" | "Aprobado";
  observacionesIA?: string | null;
  conteoPalabras: number;
};

export type MensajeChat = {
  id: string;
  rol: "usuario" | "asistente" | "sistema";
  texto: string;
  tipoMensaje?: "chat" | "analisis_tdr" | "propuesta_ideas" | "propuesta_seccion" | "auditoria_brechas";
  seccionDestino?: string;
  metadatos?: unknown;
  fecha: string;
};

export const SECCIONES_PREDETERMINADAS: { codigo: string; titulo: string; orden: number }[] = [
  { codigo: "titulo", titulo: "Título del Proyecto", orden: 1 },
  { codigo: "resumen", titulo: "Resumen Ejecutivo", orden: 2 },
  { codigo: "justificacion", titulo: "Planteamiento del Problema y Justificación", orden: 3 },
  { codigo: "estado_arte", titulo: "Marco Teórico y Estado del Arte", orden: 4 },
  { codigo: "objetivo_general", titulo: "Objetivo General", orden: 5 },
  { codigo: "objetivos_especificos", titulo: "Objetivos Específicos", orden: 6 },
  { codigo: "metodologia", titulo: "Metodología y Diseño Experimental", orden: 7 },
  { codigo: "cronograma", titulo: "Plan de Trabajo y Cronograma", orden: 8 },
  { codigo: "presupuesto", titulo: "Presupuesto y Rubros Solicitados", orden: 9 },
  { codigo: "impactos", titulo: "Resultados Esperados e Impactos", orden: 10 },
];

function contarPalabras(texto: string): number {
  if (!texto) return 0;
  return texto.trim().split(/\s+/).filter(Boolean).length;
}

type FormulacionState = {
  borradorId: number | null;
  tituloTentativo: string;
  programaId: number | null;
  lineaInvestigacion: string | null;
  convocatoriaNombre: string | null;
  convocatoriaResumen: ResumenConvocatoria | null;
  ideasPropuestas: IdeaProyectoPropuesta[];
  ideaSeleccionada: IdeaProyectoPropuesta | null;
  secciones: Record<string, SeccionCanvas>;
  mensajes: MensajeChat[];
  pasoActivo: "inicio" | "analisis" | "ideas" | "canvas";
  seccionActiva: string;
  streamingTexto: string | null;
  seccionEnStreaming: string | null;
  cargando: boolean;

  // Acciones
  setBorradorId: (id: number) => void;
  setTituloTentativo: (titulo: string) => void;
  setContextoInvestigador: (programaId: number | null, linea: string | null) => void;
  setConvocatoriaYAnalisis: (
    nombre: string,
    resumen: ResumenConvocatoria,
    ideas: IdeaProyectoPropuesta[]
  ) => void;
  seleccionarIdea: (idea: IdeaProyectoPropuesta) => void;
  setIdeaPersonalizada: (titulo: string, descripcion: string) => void;
  cargarSecciones: (secciones: SeccionCanvas[]) => void;
  actualizarSeccion: (
    codigo: string,
    contenido: string,
    estado?: SeccionCanvas["estado"],
    observaciones?: string | null
  ) => void;
  agregarMensaje: (mensaje: Omit<MensajeChat, "id" | "fecha">) => void;
  setStreaming: (seccionCodigo: string | null, texto: string | null) => void;
  setPasoActivo: (paso: FormulacionState["pasoActivo"]) => void;
  setSeccionActiva: (codigo: string) => void;
  setCargando: (cargando: boolean) => void;
  reiniciarEstado: () => void;
};

const estadoInicialSecciones: Record<string, SeccionCanvas> = {};
for (const s of SECCIONES_PREDETERMINADAS) {
  estadoInicialSecciones[s.codigo] = {
    codigoSeccion: s.codigo,
    tituloSeccion: s.titulo,
    contenido: "",
    orden: s.orden,
    estado: "Vacio",
    conteoPalabras: 0,
  };
}

export const useFormulacionStore = create<FormulacionState>((set, get) => ({
  borradorId: null,
  tituloTentativo: "Nuevo Proyecto en Formulación",
  programaId: null,
  lineaInvestigacion: null,
  convocatoriaNombre: null,
  convocatoriaResumen: null,
  ideasPropuestas: [],
  ideaSeleccionada: null,
  secciones: { ...estadoInicialSecciones },
  mensajes: [],
  pasoActivo: "inicio",
  seccionActiva: "justificacion",
  streamingTexto: null,
  seccionEnStreaming: null,
  cargando: false,

  setBorradorId: (id) => set({ borradorId: id }),
  setTituloTentativo: (titulo) => set({ tituloTentativo: titulo }),
  setContextoInvestigador: (programaId, linea) =>
    set({ programaId, lineaInvestigacion: linea }),

  setConvocatoriaYAnalisis: (nombre, resumen, ideas) =>
    set({
      convocatoriaNombre: nombre,
      convocatoriaResumen: resumen,
      ideasPropuestas: ideas,
      pasoActivo: "ideas",
    }),

  seleccionarIdea: (idea) => {
    const estado = get();
    const tituloSeccion = estado.secciones["titulo"];
    set({
      ideaSeleccionada: idea,
      tituloTentativo: idea.titulo,
      pasoActivo: "canvas",
      secciones: {
        ...estado.secciones,
        titulo: {
          ...tituloSeccion,
          contenido: idea.titulo,
          estado: "Generado",
          conteoPalabras: contarPalabras(idea.titulo),
        },
      },
    });
  },

  setIdeaPersonalizada: (titulo, descripcion) => {
    const estado = get();
    const nuevaIdea: IdeaProyectoPropuesta = {
      id: "personalizada",
      titulo,
      descripcionBreve: descripcion,
      justificacionAlineacion: "Propuesta directamente por el investigador",
      impactoEsperado: "Impacto científico / regional",
      palabrasClave: ["Investigación", "Personalizada"],
    };
    const tituloSeccion = estado.secciones["titulo"];
    set({
      ideaSeleccionada: nuevaIdea,
      tituloTentativo: titulo,
      pasoActivo: "canvas",
      secciones: {
        ...estado.secciones,
        titulo: {
          ...tituloSeccion,
          contenido: titulo,
          estado: "Generado",
          conteoPalabras: contarPalabras(titulo),
        },
      },
    });
  },

  cargarSecciones: (seccionesList) => {
    const mapa: Record<string, SeccionCanvas> = { ...get().secciones };
    for (const s of seccionesList) {
      mapa[s.codigoSeccion] = {
        ...s,
        conteoPalabras: contarPalabras(s.contenido),
      };
    }
    set({ secciones: mapa });
  },

  actualizarSeccion: (codigo, contenido, estado = "Modificado", observaciones = null) => {
    const estadoActual = get();
    const actual = estadoActual.secciones[codigo];
    if (!actual) return;

    set({
      secciones: {
        ...estadoActual.secciones,
        [codigo]: {
          ...actual,
          contenido,
          estado: contenido.trim().length === 0 ? "Vacio" : estado,
          observacionesIA: observaciones ?? actual.observacionesIA,
          conteoPalabras: contarPalabras(contenido),
        },
      },
    });
  },

  agregarMensaje: (msg) =>
    set((state) => ({
      mensajes: [
        ...state.mensajes,
        {
          ...msg,
          id: `${Date.now()}-${Math.random().toString(36).substring(2, 9)}`,
          fecha: new Date().toISOString(),
        },
      ],
    })),

  setStreaming: (seccionCodigo, texto) =>
    set({ seccionEnStreaming: seccionCodigo, streamingTexto: texto }),

  setPasoActivo: (paso) => set({ pasoActivo: paso }),
  setSeccionActiva: (codigo) => set({ seccionActiva: codigo }),
  setCargando: (cargando) => set({ cargando }),

  reiniciarEstado: () =>
    set({
      borradorId: null,
      tituloTentativo: "Nuevo Proyecto en Formulación",
      convocatoriaNombre: null,
      convocatoriaResumen: null,
      ideasPropuestas: [],
      ideaSeleccionada: null,
      secciones: { ...estadoInicialSecciones },
      mensajes: [],
      pasoActivo: "inicio",
      seccionActiva: "justificacion",
      streamingTexto: null,
      seccionEnStreaming: null,
      cargando: false,
    }),
}));

