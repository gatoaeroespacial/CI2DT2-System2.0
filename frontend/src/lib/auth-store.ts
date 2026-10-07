import { create } from "zustand";

export type UsuarioSesion = {
  id: number;
  email: string;
  nombres: string;
  apellidos: string;
  roles: string[];
  programas: number[];
};

export type RespuestaSesion = {
  accessToken: string;
  expiraEn: string;
  usuario: UsuarioSesion;
};

type EstadoAuth = {
  /** El token de acceso vive solo en memoria; el de refresco viaja en una cookie HttpOnly. */
  accessToken: string | null;
  usuario: UsuarioSesion | null;
  establecerSesion: (sesion: RespuestaSesion) => void;
  limpiar: () => void;
};

export const useAuth = create<EstadoAuth>((set) => ({
  accessToken: null,
  usuario: null,
  establecerSesion: (sesion) => set({ accessToken: sesion.accessToken, usuario: sesion.usuario }),
  limpiar: () => set({ accessToken: null, usuario: null }),
}));
