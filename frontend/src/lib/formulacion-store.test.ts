import { describe, expect, it, beforeEach } from "vitest";
import { useFormulacionStore, type IdeaProyectoPropuesta } from "./formulacion-store";

describe("useFormulacionStore", () => {
  beforeEach(() => {
    useFormulacionStore.getState().reiniciarEstado();
  });

  it("inicializa con las 10 secciones estándar vacías", () => {
    const estado = useFormulacionStore.getState();
    const codigos = Object.keys(estado.secciones);

    expect(codigos).toHaveLength(10);
    expect(codigos).toContain("titulo");
    expect(codigos).toContain("justificacion");
    expect(codigos).toContain("objetivos_especificos");
    expect(codigos).toContain("metodologia");
    expect(estado.secciones["justificacion"].estado).toBe("Vacio");
  });

  it("seleccionarIdea actualiza el título del proyecto y el paso a canvas", () => {
    const idea: IdeaProyectoPropuesta = {
      id: "idea-1",
      titulo: "Sistema Inteligente de Monitoreo Agroclimático",
      descripcionBreve: "Uso de IoT y redes neuronales para predecir sequías.",
      justificacionAlineacion: "Alineado con Minciencias y Tecnologías de Información.",
      impactoEsperado: "Impacto en 500 productores.",
      palabrasClave: ["IoT", "IA", "Agroclima"],
    };

    useFormulacionStore.getState().seleccionarIdea(idea);

    const estado = useFormulacionStore.getState();
    expect(estado.ideaSeleccionada).toEqual(idea);
    expect(estado.tituloTentativo).toBe(idea.titulo);
    expect(estado.pasoActivo).toBe("canvas");
    expect(estado.secciones["titulo"].contenido).toBe(idea.titulo);
    expect(estado.secciones["titulo"].conteoPalabras).toBeGreaterThan(0);
  });

  it("actualizarSeccion calcula conteo de palabras y actualiza estado", () => {
    const contenido = "Este proyecto aborda la brecha tecnológica en la región cafetera.";
    useFormulacionStore.getState().actualizarSeccion("justificacion", contenido, "Generado");

    const sec = useFormulacionStore.getState().secciones["justificacion"];
    expect(sec.contenido).toBe(contenido);
    expect(sec.estado).toBe("Generado");
    expect(sec.conteoPalabras).toBe(10);
  });

  it("agregarMensaje genera id y fecha automáticamente", () => {
    useFormulacionStore.getState().agregarMensaje({
      rol: "asistente",
      texto: "He formulado la propuesta de metodología.",
      tipoMensaje: "propuesta_seccion",
      seccionDestino: "metodologia",
    });

    const mensajes = useFormulacionStore.getState().mensajes;
    expect(mensajes).toHaveLength(1);
    expect(mensajes[0].rol).toBe("asistente");
    expect(mensajes[0].seccionDestino).toBe("metodologia");
    expect(mensajes[0].id).toBeTruthy();
    expect(mensajes[0].fecha).toBeTruthy();
  });
});

