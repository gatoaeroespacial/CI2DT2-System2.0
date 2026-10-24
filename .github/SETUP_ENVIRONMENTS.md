# Protección de rama y Environments (Pruebas / Produccion)

Este archivo contiene los pasos para configurar la protección de la rama `main` y los Environments `Pruebas` y `Produccion` en GitHub, y las plantillas de secretos que deben añadirse.

---

## 1) Proteger `main`

1. Entra a: https://github.com/gatoaeroespacial/CI2DT2-System2.0 → Settings → Branches → Add branch ruleset (o Add rule).
2. Ponle un nombre al ruleset, déjalo en *Active* y en *Target branches* agrega: `main` (y si quieres también `develop`).
3. Marca estas opciones:
   - `Require a pull request before merging` → `Required approvals`: 1
   - `Require status checks to pass` → agrega los checks de Compilación, Lint y Pruebas (estos aparecen solo después de que los workflows hayan corrido al menos una vez)
   - `Block force pushes`
4. Guarda con `Create`.

Nota: Ejecuta al menos una vez los workflows (p. ej. abriendo un PR de prueba) para que los status checks estén disponibles y puedas seleccionarlos en la regla.

Desde ese momento ya no se puede hacer push directo a `main`: se trabaja en una rama y se abre un PR.

---

## 2) Crear los Environments y añadir secretos

1. Ve a: Settings → Environments → New environment y crea dos entornos con estos nombres EXACTOS (sin tilde y con P mayúscula):
   - `Pruebas`
   - `Produccion`

2. En cada Environment, entra a `Environment secrets` → `Add secret` y crea estos secretos (usa valores distintos para `Pruebas` y `Produccion`):

Secreto | Valor
---|---
WEBSITE_NAME | Nombre del sitio en MonsterASP
SERVER_COMPUTER_NAME | URL del servidor Web Deploy que te da MonsterASP (suele terminar en `:8172`)
SERVER_USERNAME | Usuario Web Deploy
SERVER_PASSWORD | Contraseña Web Deploy
APPSETTINGS_JSON | El JSON de configuración del ambiente (ver plantilla abajo)

> Importante: Los secrets de Environment solo pueden añadirse vía la interfaz web o la API con permisos adecuados; este archivo en el repo contiene la plantilla pero no almacena valores secretos.

### Plantilla APPSETTINGS_JSON (usar valores distintos para Pruebas y Produccion)

Ajusta los valores entre `<...>` y mantén las comillas y la estructura JSON.

{
  "ConnectionStrings": {
    "Default": "Server=<servidor>;Database=<base>;User Id=<usuario>;Password=<clave>;TrustServerCertificate=True"
  },
  "Jwt": {
    "Clave": "<mínimo 32 caracteres aleatorios>"
  },
  "AdminInicial": {
    "Email": "<tu correo admin>",
    "Password": "<contraseña fuerte>"
  },
  "Cors": {
    "Origenes": [
      "https://<tu-proyecto>.vercel.app"
    ]
  },
  "Gemini": {
    "ApiKey": "<clave de Gemini>",
    "Modelo": "gemini-2.5-flash"
  },
  "Archivos": {
    "Ruta": "<carpeta de documentos, ej. C:\\inetpub\\wwwroot\\MiSitio\\files>"
  },
  "DataProtection": {
    "RutaLlaves": "<carpeta para llaves, ej. C:\\inetpub\\wwwroot\\MiSitio\\keys>"
  }
}

### Generar la clave JWT (PowerShell)

Ejecuta en PowerShell para generar una cadena segura (base64):

```
[Convert]::ToBase64String((1..48 | ForEach-Object { Get-Random -Maximum 256 }))
```

---

## 3) Notas y recomendaciones finales

- Rellena los campos `<...>` con los valores reales para `Pruebas` y `Produccion`.
- Añade `APPSETTINGS_JSON` como secret en cada Environment pegando el JSON completo. GitHub acepta valores multilínea en el formulario de secrets.
- Si prefieres automatizar la creación de secrets, se puede usar la GitHub REST API para Environment Secrets pero necesitarás un token con permisos `repo` y `environment:secrets` y un script que cifre los valores con la public key del environment.
- Si quieres, puedo añadir un ejemplo de script PowerShell/CLI para crear los secrets vía API (me dices si prefieres PowerShell o bash + curl).

---

Archivo generado por assistant para ayudar en la configuración de la protección de ramas y environments.
