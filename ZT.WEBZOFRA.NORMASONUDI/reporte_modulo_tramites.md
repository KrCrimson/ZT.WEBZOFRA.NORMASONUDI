# Reporte de Creación: Módulo "Registrar Trámite" (Fase 3)
**Proyecto:** ZT.WEBZOFRA.NORMASONUDI

Se desarrolló de forma nativa e integrada el módulo central de registro de trámites (`RegistrarTramite.aspx` y su respectivo CodeFile). Esta implementación se alinea completamente a la arquitectura libre de ORMs y subnamespaces, operando al 100% mediante lógica parametrizada de `System.Data.SqlClient` (ADO.NET puro).

## 1. Diseño de la Vista Principal (`RegistrarTramite.aspx`)
Se construyó un esqueleto transaccional usando HTML nativo interconectado con WebForms para administrar la subida de documentos y la distribución de sus firmantes.

- **Directivas y Entorno:** Implementado con `CodeFile="RegistrarTramite.aspx.cs"` y ejecutado sobre el `Inherits="RegistrarTramite"`. Sin vinculaciones maestras (`MasterPageFile`) por lo pronto.
- **Formularios Dinámicos y Controles:**
  - Componentes estáticos de recolección de metadatos del documento (Asunto, Área, Fecha, Código).
  - Integración de `<asp:FileUpload>` validado para binarios exclusivos en `.pdf`.
  - Construcción del control dinámico `<asp:GridView ID="GvFirmantes">`. Este grid cuenta con 4 columnas transmutadas en `TemplateField` para posibilitar la edición a tiempo real del usuario (`TxtNombreFirmante`, `TxtCorreoFirmante`, `TxtOrdenFirma`, `CbxRol`).

## 2. Desarrollo del Controlador (`RegistrarTramite.aspx.cs`)
El procesamiento y ejecución de las reglas de negocio de la ZOFRA se centralizó en este CodeFile.

- **Manejo de Seguridad y Sesión:**
  - El sistema detecta en la fase del `Page_Load` si no existe la variable `Session["strUsuario"]` y expulsa silenciosamente hacia el `Login.aspx`.
  - Del mismo modo, asegura que únicamente los perfiles asignados como `REGISTRADOR` o `ADMIN` puedan renderizar y procesar la vista.
- **Gestión Multi-Estado de Firmantes (GridView):**
  - Ya que los firmantes se capturan dinámicamente, se implementó el método `ObtenerDatosFirmantes()` que itera las filas del Grid, rescata los datos tipeados, reconstruye el `DataTable` en memoria y lo salva bajo `ViewState["Firmantes"]`. Esto previene de forma segura la pérdida de texto en la vista cuando el usuario da click en el botón "+ Agregar Firmante".

## 3. Transacción Distribuida de Almacenamiento Estructurado
El mayor requerimiento cubierto en esta fase fue procesar limpiamente el PDF y dividir su almacenamiento entre dos bases de datos interconectadas.

Al desencadenar `BtnRegistrar_Click`:
1. **Filtro Transaccional:** Comprueba la existencia y validez formal del archivo PDF. Analiza además las filas previas verificando que no existan datos vacíos ni números duplicados en las jerarquías de `OrdenFirma`.
2. **Subida Externa y Generación Binaria (BD Archivos):**
   - Ejecuta primero `ARC_I_GuardarArchivo_OUT` sobre la cadena independiente `FirmadorArchivos`. 
   - Aislado por un `using (SqlConnection)` se manda el parámetro varbinary recuperando por referencia (`OUTPUT`) la llave unívoca del blob: `@IDArchivo`.
3. **Registro Maestro Metadatos (BD Firmador):**
   - Abriendo un nuevo hilo `SqlConnection` en la otra base maestra, se llama al procedimiento `FIR_I_Documento_OUT`. Aquí se transfiere silenciosamente la ruta combinada `"ARC::" + idArchivo` a la columna `RutaArchivoPDF` extrayendo un `@IDDocumento` global.
4. **Ciclo de Distribución en Lote:**
   - La misma conexión viva es empleada iterativamente para leer a todos los integrantes del `GridView`.
   - Lanza en pares el SP para `FIR_I_DocumentoRevisor` (que asigna 3 días de plazo automático) seguido estrechamente del `FIR_I_DocumentoFirmante` (donde aterriza el orden y el rol jerárquico real).
5. **Autoejecución de Flujo Documental:**
   - Para finalizar, dispara un último SP `FIR_U_IniciarRevision` activando formalmente el estatus `EN_REV` e inyectando las IPs requeridas desde `HttpContext.Current.Request.UserHostAddress`.
   - Se concluye redirigiendo automáticamente a la bandeja de trámites.

Todo el ecosistema fue protegido bloqueando las clases en `using (...)` garantizando que, aunque falle por timeout, la basura sea recolectada y ninguna conexión hacia la base de datos de ZOFRATACNA se quede abierta o congelada.
