# Guía rápida del CI²DT²

## Crear usuarios, definir contraseña e ingresar

1. Inicie sesión con una cuenta que tenga el rol **Administrador** y abra **Usuarios → Nuevo usuario**.
2. Complete los datos, el correo y seleccione uno o más roles. El correo debe ser único. Si selecciona **Externo**, también debe asociar un aliado.
3. Guarde el usuario. La pantalla no solicita contraseña porque esta se define después desde el enlace de bienvenida enviado al correo registrado.
4. Abra el enlace y establezca una contraseña de al menos 10 caracteres, con mayúsculas, minúsculas y números. El enlace vence a las 72 horas.
5. Ingrese en **/login** con el correo y la contraseña recién definida. No hay un selector de rol en el inicio de sesión: los permisos se aplican automáticamente según los roles asignados a esa cuenta. Si tiene varios, se combinan.

**Si no llega el correo:** cuando SMTP no está configurado, el mensaje —incluido el enlace para definir la contraseña— se escribe en el log de la API, tanto en consola como en la carpeta `logs` relativa al directorio desde el que se ejecuta. Trate ese enlace como secreto y no comparta el log. En producción se debe configurar SMTP. La cuenta se crea aunque falle el envío, así que un administrador puede verificar si ya aparece en la lista de usuarios.

Los usuarios se guardan en **SQL Server**, en la tabla `Usuarios`; los roles están en `Roles` y su asignación a usuarios en `UsuarioRoles`. La contraseña se almacena como hash, no como texto legible. En desarrollo local, la configuración apunta a LocalDB (`(localdb)\MSSQLLocalDB`, base `Ci2dt2Dev`). En los demás ambientes, el servidor y la base dependen de la cadena de conexión configurada para ese ambiente.

La primera cuenta Administrador se crea al iniciar la API si no existe ya un administrador. Su correo y contraseña se toman de la configuración `AdminInicial`; use credenciales propias y no reutilice las de desarrollo en producción.

## Recorrido del sistema

El sistema es un MVP con un frontend **Next.js/React/TypeScript** y una API **.NET 10**. La API usa **Entity Framework Core** para persistencia y migraciones, y **Dapper** para algunas consultas. Las migraciones se aplican al iniciar la API si `BaseDatos:MigrarAlIniciar` está habilitado.

En la interfaz están disponibles el inicio de sesión y la administración de usuarios (crear, editar, asignar roles y activar/desactivar). También existen accesos a tablero del centro, proyectos y notificaciones, pero sus pantallas actualmente indican que esas capacidades estarán disponibles próximamente.

El modelo de datos ya contempla programas, proyectos, convocatorias y financiación, cronogramas e hitos, avances, documentos, aliados, notificaciones, consentimientos y auditoría. Que esos datos estén modelados no significa que todas sus pantallas o flujos estén terminados.

### Roles

- **Administrador:** gestiona usuarios y roles; también tiene acceso al tablero y a los proyectos.
- **Director:** tablero del centro, gestión y revisión de proyectos y programas.
- **Coordinador:** gestión y revisión de proyectos en su ámbito.
- **Investigador:** trabajo con propuestas y proyectos.
- **Externo:** acceso limitado; en la matriz actual, puede ver notificaciones.

La autorización se valida en el backend, no solo ocultando opciones del menú. La cuenta debe estar activa y tener contraseña definida. Desactivar una cuenta revoca sus sesiones.

## Para desarrollo local

- API: `http://localhost:5132`; documentación OpenAPI/Swagger en `/swagger` y estado de la API en `/health`.
- Frontend: `http://localhost:3000`.
- La configuración local está en `backend/src/Ci2dt2.Api/appsettings.Development.json`. No publique ni reutilice sus valores secretos fuera de desarrollo.
- Si no hay servidor SMTP, revise los logs locales para el correo de bienvenida. Antes de usar datos reales, configure correo, cadena de conexión, clave JWT y orígenes permitidos para el ambiente.
- Hay instrucciones de respaldo y restauración en [operacion.md](./operacion.md).
