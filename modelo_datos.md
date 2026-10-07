# CI²DT² — Diccionario de Datos y Modelo Incremental

## Tablas del sistema (35 total)

### Seguridad
| Tabla | Propósito | Relaciones clave |
|-------|-----------|-----------------|
| `Roles` | Director, Coordinador, Investigador, Externo, Admin | → UsuarioRoles |
| `Usuarios` | Todos los actores del sistema | → UsuarioRoles, ProgramaIntegrantes, ProyectoIntegrantes |
| `UsuarioRoles` | N:N Usuarios ↔ Roles | |
| `RefreshTokens` | Tokens de renovación JWT | → Usuarios |

### Organización
| Tabla | Propósito | Relaciones clave |
|-------|-----------|-----------------|
| `Programas` | Los 5 programas del CI²DT² | → todo |
| `ObjetivosEspecificos` | Objetivos de cada programa | → Programas |
| `ProgramaIntegrantes` | N:N Usuarios ↔ Programas con rol | Un usuario puede ser coordinador de un programa e investigador de otro |

### Proyectos
| Tabla | Propósito | Notas |
|-------|-----------|-------|
| `Proyectos` | Entidad central del sistema | Tiene EstadoId, porcentajeAvance, montos |
| `EstadosProyecto` | Catálogo de estados | En elaboración → Presentado → Aprobado → En ejecución → Finalizado |
| `TiposConvocatoria` | Nacional, Internacional, Interna, Conjunta | |
| `ProyectoIntegrantes` | N:N Usuarios ↔ Proyectos | Con rol específico |
| `ProyectoAntecedentes` | Historial de presentaciones anteriores | |

### Cronogramas y Seguimiento
| Tabla | Propósito | Notas |
|-------|-----------|-------|
| `Cronogramas` | Contenedor de hitos | EntidadTipo genérico: Proyecto, Diplomado, Evento |
| `TiposHito` | Entregable, Informe, Reunión, Pago, etc. | |
| `Hitos` | Fechas clave con alerta | AlertarDias: días antes de la fecha para alertar |
| `Avances` | Bitácora de progreso | OrigenRegistro: Web o Mobile; soporta GPS |
| `AvancesAdjuntos` | Archivos adjuntos a avances | URL al archivo en servidor |

### Notificaciones
| Tabla | Propósito | Notas |
|-------|-----------|-------|
| `Notificaciones` | In-app + push + email | EnviadaPush, EnviadaEmail como flags |

### Formación
| Tabla | Propósito |
|-------|-----------|
| `TiposFormacion` | Diplomado, Curso, Microcertificación, Taller |
| `Modalidades` | Presencial, Virtual, Híbrido |
| `Formaciones` | Diplomados y cursos por programa |
| `FormacionDocentes` | N:N Docentes ↔ Formaciones |

### Eventos
| Tabla | Propósito |
|-------|-----------|
| `TiposEvento` | Congreso, Seminario, Taller, etc. |
| `Eventos` | Eventos del centro y de cada programa |

### Semilleros
| Tabla | Propósito |
|-------|-----------|
| `Semilleros` | Semilleros de investigación por programa |
| `TiposIntegranteSemillero` | Estudiante, Docente, Egresado |
| `SemilleroIntegrantes` | N:N Usuarios ↔ Semilleros |

### Convocatorias y Financiamiento
| Tabla | Propósito |
|-------|-----------|
| `Convocatorias` | Convocatorias disponibles con fechas |
| `PropuestasConvocatoria` | Proyectos presentados a convocatorias |
| `FuentesFinanciamiento` | Fuentes con montos estimados |

### Entorno Empresarial
| Tabla | Propósito |
|-------|-----------|
| `TiposAliado` | Universidad, Empresa, Entidad Pública, etc. |
| `Aliados` | Entidades aliadas (CHEC, UAM, Minciencias...) |
| `ProgramaAliados` | N:N Programas ↔ Aliados |

### Producción Académica e Indicadores
| Tabla | Propósito |
|-------|-----------|
| `TiposPublicacion` | Q1, Q2, Libro, Ponencia, Tesis |
| `Publicaciones` | Artículos y otras salidas académicas |
| `Indicadores` | KPIs con meta, valor actual y período |

### Auditoría
| Tabla | Propósito |
|-------|-----------|
| `LogActividades` | Trazabilidad de todas las acciones |

---

## Modelo de permisos por rol

| Acción | Admin | Director | Coordinador | Investigador | Externo |
|--------|-------|----------|-------------|--------------|---------|
| Ver dashboard centro | ✅ | ✅ | ❌ | ❌ | ❌ |
| Ver todos los programas | ✅ | ✅ | Solo el suyo | Solo los suyos | ❌ |
| Crear/editar proyectos | ✅ | ✅ | Solo su programa | ❌ | ❌ |
| Registrar avances | ✅ | ✅ | ✅ | Solo sus proyectos | ❌ |
| Gestionar hitos | ✅ | ✅ | ✅ | Solo sus proyectos | ❌ |
| Ver notificaciones | ✅ | ✅ | ✅ | Solo las suyas | Solo las suyas |
| Gestionar semilleros | ✅ | ✅ | Solo su programa | ❌ | ❌ |
| Exportar reportes | ✅ | ✅ | Solo su programa | ❌ | ❌ |
| Configuración sistema | ✅ | ❌ | ❌ | ❌ | ❌ |

---

## Roadmap por endpoints (Fase 1 → Fase 4)

### Fase 1 (Core — semanas 3-6)
```
POST   /api/v1/auth/login
POST   /api/v1/auth/refresh
GET    /api/v1/auth/me
POST   /api/v1/auth/device-token

GET    /api/v1/programas
GET    /api/v1/programas/{id}
GET    /api/v1/programas/{id}/integrantes

GET    /api/v1/proyectos/mios
GET    /api/v1/proyectos/programa/{programaId}
GET    /api/v1/proyectos/{id}
POST   /api/v1/proyectos
PUT    /api/v1/proyectos/{id}
POST   /api/v1/proyectos/{id}/avances
GET    /api/v1/proyectos/{id}/avances

GET    /api/v1/dashboard/centro
GET    /api/v1/dashboard/hitos-proximos
GET    /api/v1/notificaciones/mias
POST   /api/v1/notificaciones/{id}/leer
```

### Fase 2 (Cronogramas — semanas 7-10)
```
GET/POST/PUT  /api/v1/cronogramas
GET/POST/PUT  /api/v1/hitos
POST          /api/v1/hitos/{id}/completar
GET           /api/v1/alertas/pendientes
```

### Fase 3 (Académica — semanas 11-14)
```
CRUD /api/v1/formaciones
CRUD /api/v1/eventos
CRUD /api/v1/semilleros
CRUD /api/v1/servicios
CRUD /api/v1/convocatorias
CRUD /api/v1/aliados
```

### Fase 4 (Inteligencia — semanas 15-18)
```
GET  /api/v1/indicadores/programa/{id}
GET  /api/v1/indicadores/centro
GET  /api/v1/reportes/plan-trabajo/{programaId}
POST /api/v1/reportes/exportar-pdf
POST /api/v1/reportes/exportar-word
GET  /api/v1/publicaciones/programa/{id}
```

---

## Convenciones de código

### Backend (.NET)
- Rutas: `/api/v1/{recurso}` siempre
- Errores de negocio: `Results.UnprocessableEntity(new { error })`
- Errores de validación: FluentValidation lanza automáticamente vía pipeline
- Queries complejas: Dapper en el handler, sin `HsuDbContext` en Application
- Comandos/mutations: EF Core + `IApplicationDbContext`
- Nunca `Program.cs` modificado para endpoints — todo via `{Modulo}Endpoints.Map(app)`

### Frontend (Next.js)
- Estado servidor: TanStack Query (`useQuery`, `useMutation`)
- Estado cliente global: Zustand (solo auth y UI state)
- Formularios: react-hook-form + Zod
- Notificaciones UI: Sonner
- Fechas: date-fns en español

### Mobile (React Native)
- Credenciales: expo-secure-store (nunca AsyncStorage para tokens)
- Notificaciones push: expo-notifications
- GPS para avances de campo: expo-location
- Cámara para adjuntos: expo-image-picker
