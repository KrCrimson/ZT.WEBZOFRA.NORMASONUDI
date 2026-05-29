# Reporte Técnico: Mejoras en Módulo de Detalle de Documentos

**Fecha:** 05 de Mayo de 2026
**Módulo:** Trámites / Detalle de Documentos
**Archivos Modificados:** 
- `ZT.WEBZOFRA.NORMASONUDI/Tramites/Detalle.aspx`
- `ZT.WEBZOFRA.NORMASONUDI/Tramites/Detalle.aspx.cs`

---

## 1. Objetivo
Implementar una serie de cuatro (4) mejoras funcionales y de interfaz de usuario en la pantalla de detalle de documentos, enfocadas en la validación de acciones, el seguimiento del historial, la comunicación automática por correo y la estandarización en la nomenclatura de archivos.

---

## 2. Detalle de las Mejoras Implementadas

### Mejora 1: Bloqueo Condicional de Botón "Visto Bueno"
**Descripción:** Control de interfaz de usuario para evitar que un revisor apruebe un documento accidentalmente o de forma inconsistente cuando ha redactado una observación.
* **Frontend (`Detalle.aspx`):** Se integró el evento `onkeyup="syncButtons()"` en el campo de texto `TxtObservacion`.
* **Lógica JS:** La función determina la longitud de la cadena (ignorando espacios vacíos mediante `.trim()`). Si se detecta texto, el botón `BtnConforme` (Visto Bueno) se deshabilita instantáneamente, dejando solo habilitado `BtnObservar` (Registrar Observación).

### Mejora 2: Historial de Versiones del Documento
**Descripción:** Nueva sección visual para consultar el ciclo de vida del documento, versiones iteradas previas y acceso a los documentos correspondientes.
* **Frontend:** Se incluyó un nuevo panel contenedor con el componente `GridView` (`GvVersiones`), estructurado con las columnas: Versión, Motivo, Fecha y Acción.
* **Backend (`Detalle.aspx.cs`):** 
  * Se desarrolló el método `CargarVersiones` ejecutando una instrucción `SELECT` contra la vista/tabla `FIR_VersionDocumento` relacionada al `IDDocumento`.
  * Mediante el evento `RowDataBound`, se formatea visualmente la versión con el prefijo `"v"` y se transforma la ruta del archivo (`ARC::[ID]`) en un enlace válido hacia el manejador de descargas `~/Handlers/VerPDF.ashx?id=[ID]`.

### Mejora 3: Notificaciones de Correo Automáticas
**Descripción:** Automatización del flujo de notificaciones para comunicar de forma inmediata al usuario registrador sobre el avance o rechazo de su documento. Se utilizó el procedimiento del sistema `GEN_X_EnviarMail`.
* **Evento de Observación (`BtnObservar_Click`):** Se consulta el email del Registrador utilizando una subconsulta en `FIR_VW_EmpleadosActivos`. Si se encuentra el correo, se remite en tiempo real la observación redactada precisando el código de trámite y el usuario revisor.
* **Evento de Conformidad total (`BtnConforme_Click`):** Si tras aplicar una conformidad, el procedimiento `FIR_X_RevisionCierre_OUT` retorna que `@Cerrado=1` (estado completado), paralelamente a notificar al primer firmante, se agregó la notificación dirigida al registrador informando el éxito en la fase de revisión.

### Mejora 4: Nomenclatura Automática en Archivos Subidos
**Descripción:** Control de versionamiento estricto a nivel de archivo para las correcciones subidas en la plataforma. 
* **Backend (`BtnCorregir_Click`):** Antes de consumir el procedimiento `ARC_I_GuardarArchivo_OUT`, se implementó una consulta a la base de datos `Firmador` para extraer la versión actual del registro en `FIR_Documento`.
* **Formatos de Nombre:** Se concatenó lógicamente el Código del documento actual (obtenido de interfaz) más el sufijo `_v` sumado al entero precalculado de `version + 1`. Ejemplo resultante: `RES-2026-0005_v2.pdf`.

---

## 3. Consideraciones Técnicas y de Código
* **Tecnología:** .NET Framework con WebForms clásico. C# en Code-Behind.
* **Acceso a Datos:** Se implementó usando de forma exclusiva clases de `System.Data.SqlClient` (ADO.NET Nativo), respetando estrictamente bloques `using()` para garantizar la liberación y correcto descarte de las conexiones SQL independientemente si el flujo finaliza correctamente o detecta excepciones.
* **Sin ORMs:** No se acopló Entity Framework, respetando la arquitectura preexistente basada en consultas directas de T-SQL y Procedimientos Almacenados.