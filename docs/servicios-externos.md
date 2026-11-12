# Servicios externos gratuitos (configuración obligatoria)

Todo se configura en el secreto `APPSETTINGS_JSON` del Environment `Produccion` en GitHub (y en `Pruebas` si existe). Después de cambiarlo, vuelva a ejecutar el workflow **Backend** para que llegue al servidor.

## 1. Correo: Brevo (HU-23 y HU-03)

Plan gratuito: 300 correos al día, suficiente para el piloto.

1. Cree una cuenta en <https://www.brevo.com> (plan Free).
2. **Remitente:** *Senders, Domains & Dedicated IPs → Senders → Add a sender*. Use un buzón que usted controle (p. ej. el correo del centro) y confírmelo desde el enlace que le llega. Si la Universidad autoriza, autentique el dominio en *Domains* con los registros DNS que indica Brevo (mejora la entrega).
3. **Credenciales SMTP:** *SMTP & API → SMTP*. Anote el servidor (`smtp-relay.brevo.com`), el puerto (587) y el usuario (*Login*). Pulse *Generate a new SMTP key* y copie la clave (solo se muestra una vez).
4. Agregue al `APPSETTINGS_JSON`:
   ```json
   "Smtp": {
     "Host": "smtp-relay.brevo.com",
     "Puerto": 587,
     "UsarSsl": true,
     "Usuario": "<Login SMTP de Brevo>",
     "Contrasena": "<SMTP key>",
     "Remitente": "<correo remitente verificado>",
     "NombreRemitente": "CI²DT² Universidad de Caldas"
   },
   "Frontend": { "Url": "https://ci-2-dt-2-system2-0.vercel.app" }
   ```
   `Frontend:Url` es obligatorio: con él se arman los enlaces de los correos (ir al proyecto, restablecer contraseña).
5. **Prueba:** en el login use "¿Olvidaste tu contraseña?" con su correo; debe llegar en menos de 2 minutos. Si no llega, revise `UltimoError` en la tabla `Notificaciones` o el log `logs/ci2dt2-*.json` del sitio.

## 2. Programador de alertas: cron-job.org (HU-18)

MonsterASP recicla la aplicación cuando no tiene visitas y detiene las tareas en segundo plano. Un programador externo garantiza que las alertas corran a diario y que los correos pendientes salgan.

1. Genere una clave larga (PowerShell):
   ```powershell
   [Convert]::ToBase64String((1..32 | ForEach-Object { Get-Random -Maximum 256 }))
   ```
2. Agréguela al `APPSETTINGS_JSON`: `"Trabajos": { "ApiKey": "<clave>" }` y vuelva a desplegar.
3. Cree una cuenta gratuita en <https://cron-job.org> → *Create cronjob*:
   - **URL:** `https://ci2dt2prueba.runasp.net/api/v1/jobs/alertas`
   - **Schedule:** todos los días a las 06:00, zona horaria *America/Bogota*.
   - **Advanced → Request method:** `POST`; **Headers:** `X-Api-Key: <clave>`.
4. Pulse *Test run*: la respuesta debe ser 200 con el resumen `{"alertas":…,"correosEnviados":…}`. Sin la clave correcta responde 401.
5. Opcional: un segundo cronjob cada 15 minutos a la misma URL hace que los correos pendientes salgan rápido aunque nadie use la plataforma (las alertas no se duplican).

## 3. Inteligencia artificial: Google Gemini (asistente y agente de formulación)

El plan gratuito de Google AI Studio tiene límites de solicitudes por minuto y por día; en el plan gratuito Google puede usar los datos enviados para mejorar sus productos, así que no envíe información confidencial hasta pasar a un plan de pago.

1. Entre a <https://aistudio.google.com> con una cuenta de Google → *Get API key → Create API key*.
2. Agregue al `APPSETTINGS_JSON`: `"Gemini": { "ApiKey": "<clave>", "Modelo": "gemini-3.5-flash" }`.
3. En local puede ponerla en un archivo `.env` junto a la API (no se sube a Git): `Gemini__ApiKey=<clave>`.
4. Prueba: menú **Asistente IA** (Admin o Director) o **Proyectos → Asistente de Formulación IA**. Sin clave responden 503 "asistente no configurado".

## 4. Archivos y llaves en MonsterASP (HU-21, D-7)

Los documentos, los adjuntos de avances y las llaves de firma se guardan por defecto en `App_Data` dentro de la carpeta publicada. Para que un despliegue nunca los borre, apúntelos a una carpeta del sitio **fuera de `wwwroot`**:

1. En el WebFTP de MonsterASP cree, al lado de `wwwroot`, las carpetas `datos/documentos` y `datos/llaves`.
2. Averigüe la ruta física del sitio en el panel (sección del sitio o soporte) y agregue al `APPSETTINGS_JSON`:
   ```json
   "Archivos": { "Ruta": "<ruta física>\\datos\\documentos" },
   "DataProtection": { "RutaLlaves": "<ruta física>\\datos\\llaves" }
   ```
3. Prueba: suba un documento, vuelva a desplegar y descárguelo. Incluya la carpeta `datos` en las copias de seguridad: el respaldo de la base no copia archivos. El plan gratuito tiene cuota de disco limitada; vigílela desde el panel.

## 5. Vercel

- Variable `API_URL = https://ci2dt2prueba.runasp.net` (Production y Preview).
- Variable opcional `SITE_URL = https://ci-2-dt-2-system2-0.vercel.app` para los metadatos y el `sitemap.xml` del portal.
- Prueba de subida grande: suba un PDF de unos 20 MB en Documentos. Si falla y uno pequeño funciona, es el límite del proxy de Vercel; avise para enviar la carga directo a la API.
