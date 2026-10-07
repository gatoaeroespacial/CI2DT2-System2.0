# CI²DT² · Sistema de información

Monorepo del MVP: `backend/` (.NET 10, SQL Server) y `frontend/` (Next.js + React + TypeScript).

```
backend/
  src/Ci2dt2.Domain           Entidades del ER v3 (tablas del MVP)
  src/Ci2dt2.Application      Casos de uso: servicios (EF Core), consultas (Dapper), validadores
  src/Ci2dt2.Infrastructure   EF Core, migraciones, datos semilla, JWT, correo
  src/Ci2dt2.Api              Minimal APIs ({Modulo}Endpoints.Map), autorización, Swagger, health
  tests/                      Pruebas unitarias y de integración (SQL Server real)
frontend/                     Next.js (App Router)
```

## Ejecución local

Requisitos: .NET SDK 10, Node.js 20.9+, SQL Server o LocalDB.

```bash
dotnet run --project backend/src/Ci2dt2.Api --launch-profile http
```

```bash
npm run dev --prefix frontend
```

- API en `http://localhost:5132` (Swagger en `/swagger`, health check en `/health`). Al iniciar aplica las
  migraciones sobre la base `Ci2dt2Dev` de LocalDB y crea el Administrador inicial de desarrollo
  (credenciales en `backend/src/Ci2dt2.Api/appsettings.Development.json`).
- Frontend en `http://localhost:3000`. Las rutas `/api/v1/*` se reenvían a la API (`API_URL`).
- Sin SMTP configurado, los correos (p. ej. el de bienvenida con el enlace para definir la contraseña)
  se escriben en el log de la API.

## Pruebas

```bash
dotnet test backend
```

```bash
npm test --prefix frontend
```

Las pruebas de integración crean y eliminan una base temporal. Por defecto usan LocalDB; con la
variable `CI2DT2_TEST_SQL` se indica otro servidor (así lo hace el pipeline de CI).

## Ambientes y secretos

Dos ambientes: **Pruebas** (rama `develop`) y **Producción** (rama `main`). Los pipelines están en
`.github/workflows/`. Ningún secreto vive en el código:

- **Backend (MonsterASP, Web Deploy)**: en GitHub, *Settings › Environments* `Pruebas` y `Produccion`, con los
  secretos `WEBSITE_NAME`, `SERVER_COMPUTER_NAME`, `SERVER_USERNAME`, `SERVER_PASSWORD` y `APPSETTINGS_JSON`.
  Este último se escribe como `appsettings.{Ambiente}.json` al publicar:

  ```json
  {
    "ConnectionStrings": { "Default": "Server=...;Database=...;User Id=...;Password=...;TrustServerCertificate=True" },
    "Jwt": { "Clave": "<mínimo 32 caracteres aleatorios>" },
    "AdminInicial": { "Email": "<correo>", "Password": "<contraseña inicial>" },
    "Frontend": { "Url": "https://<dominio-frontend>" },
    "Cors": { "Origenes": [ "https://<dominio-frontend>" ] },
    "Smtp": { "Host": "...", "Puerto": 587, "UsarSsl": true, "Usuario": "...", "Contrasena": "...", "Remitente": "..." },
    "DataProtection": { "RutaLlaves": "<carpeta persistente con permisos de escritura>" }
  }
  ```

- **Frontend (Vercel)**: proyecto con *Root Directory* `frontend`, integración con GitHub (vista previa por
  pull request) y la variable `API_URL` por ambiente (URL HTTPS de la API).

HTTPS es obligatorio (HSTS y redirección en la API) y CORS solo admite los orígenes configurados.
El token de refresco viaja en una cookie HttpOnly, Secure, SameSite=Strict; como el navegador solo habla
con el dominio del frontend (proxy de Next.js), la cookie no se bloquea entre dominios.

## Base de datos: respaldo y restauración

Ver [docs/operacion.md](docs/operacion.md).
