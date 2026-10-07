export type UsuarioListado = {
  id: number;
  nombres: string;
  apellidos: string;
  email: string;
  tipoDocumento: string | null;
  numeroDocumento: string | null;
  tieneAcceso: boolean;
  roles: string[];
};

export type UsuarioDetalle = Omit<UsuarioListado, "roles"> & {
  telefono: string | null;
  cargo: string | null;
  dependencia: string | null;
  cvLacUrl: string | null;
  orcid: string | null;
  aliadoId: number | null;
  roles: string[];
};

export type Pagina<T> = { items: T[]; total: number; paginaActual: number; tamanoPagina: number };

export type AliadoOpcion = { id: number; nombre: string };
