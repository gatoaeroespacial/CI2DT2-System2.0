# Operación de la base de datos (HT-02)

## Migraciones

- Las migraciones de EF Core están en `backend/src/Ci2dt2.Infrastructure/Persistence/Migrations`.
- La API las aplica al iniciar (`BaseDatos:MigrarAlIniciar = true`). Para generar el script SQL idempotente
  de un ambiente:

  ```bash
  dotnet ef migrations script --idempotent -p backend/src/Ci2dt2.Infrastructure -s backend/src/Ci2dt2.Infrastructure -o migraciones.sql
  ```

- Nueva migración:

  ```bash
  dotnet ef migrations add <Nombre> -p backend/src/Ci2dt2.Infrastructure -s backend/src/Ci2dt2.Infrastructure -o Persistence/Migrations
  ```

## Copias de seguridad diarias

En el panel de MonsterASP, para cada base (Pruebas y Producción):

1. *Databases › MSSQL › Backups*: activar la copia automática diaria y definir la retención.
2. Descargar periódicamente una copia (`.bak`) fuera del hosting.

## Procedimiento de restauración

1. En el ambiente de **Pruebas**, restaurar la última copia de Producción desde *Backups › Restore*
   (o cargar el `.bak` descargado).
2. Iniciar la API apuntando a esa base: el endpoint `/health` debe responder `Healthy` y las migraciones
   pendientes se aplican solas.
3. Verificar el inicio de sesión de un usuario y el listado de usuarios.
4. Registrar en el acta del sprint la fecha de la prueba y el tiempo de restauración.
