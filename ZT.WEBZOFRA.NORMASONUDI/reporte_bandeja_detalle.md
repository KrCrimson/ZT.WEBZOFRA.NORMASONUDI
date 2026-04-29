# Reporte de Implementación: Bandeja, Detalle y Fecha Límite

**Fecha:** 28/04/2026  
**Módulos:** Bandeja de Trámites, Detalle de Documento, Handler PDF, Fecha Límite de Revisión

---

## 1. Bandeja de Trámites (Modificación)

### Archivos: `Tramites/Bandeja.aspx` + `Bandeja.aspx.cs`

**Layout de 3 columnas:**
- **Sidebar izquierdo** (220px): menú de navegación con opciones según rol
  - REGISTRADOR: Registrar Documento, Mis Trámites
  - FIRMADOR: Pendientes de Revisión, Pendientes de Firma, Completados
  - ADMIN: Todos los Trámites, Gestionar Roles
  - Botón Cerrar Sesión (destruye la sesión)
- **Centro** (flexible): GridView `GvTramites` con columnas Código, Asunto, Tipo, Área, Fecha, Estado (badge con color) y enlace "Ver Detalle"
- **Derecha** (260px): Calendario ASP.NET con días marcados según trámites

**Queries por rol:**
- REGISTRADOR: filtra por `LoginRegistrador = @LoginUsuario`
- FIRMADOR: filtra por `FIR_DocumentoFirmante.LoginUsuario = @LoginUsuario`
- ADMIN: muestra todos sin filtro

**Filtros del sidebar:** cada botón guarda `ViewState["FiltroEstado"]` y recarga con `DataTable.Select()` sobre el DataTable ya cargado.

**Calendario:**
- `DayRender`: resalta en azul los días que tienen trámites
- `SelectionChanged`: filtra la grilla por la fecha seleccionada
- Botón "Quitar filtro de fecha" para restaurar la vista completa

---

## 2. Detalle del Documento (Nuevo)

### Archivos: `Tramites/Detalle.aspx` + `Detalle.aspx.cs`

**Sección 1 — Información:**
Labels para Código, Asunto, Tipo, Área, Fecha, Versión, Registrador, Fecha Límite y badge de estado con color dinámico.

**Sección 2 — Preview PDF:**
IFrame que apunta a `~/Handlers/VerPDF.ashx?id=IDArchivo`. La ruta se extrae de `RutaArchivoPDF` (formato `ARC::3`).

**Sección 3 — Revisores:**
GridView `GvRevisores` con columnas Revisor, Correo, Completado (Sí/No), Resultado. Filas coloreadas: verde (CONF), rojo (OBS), gris (pendiente).

**Sección 3B — Observaciones:**
Panel visible solo si existen. Repeater con tarjetas amarillas mostrando autor, fecha y texto.

**Sección 4A — Acciones del Firmador:**
Visible si `rol==FIRMADOR`, `estado==EN_REV` y el usuario NO ha revisado aún.
- **Dar Visto Bueno** → `FIR_I_Conformidad` (@NombreRevisor, @Comentario="CONFORME", @IDEquipo) → `FIR_X_RevisionCierre_OUT` (@IDDocumento, OUTPUT @Cerrado)
- **Registrar Observación** → `FIR_I_Observacion` (@NombreRevisor, @Descripcion, @IDEquipo)

**Sección 4B — Acciones del Registrador:**
Visible si `rol==REGISTRADOR` o `ADMIN`.
- **Enviar Recordatorio** (estado EN_REV): obtiene revisores con `Completado=0` y llama `GEN_X_EnviarMail` con HTML formateado por cada uno.
- **Subir PDF Corregido** (estado OBS): guarda nuevo PDF en `ARC_I_GuardarArchivo_OUT` y llama `FIR_U_DocumentoCorreccion`.

---

## 3. Handler PDF (Nuevo)

### Archivo: `Handlers/VerPDF.ashx`

Clase `VerPDF : IHttpHandler` que:
1. Recibe `?id=IDArchivo` por QueryString
2. Conecta a `FirmadorArchivos`
3. Lee `Contenido` (byte[]) y `NombreOriginal` de `ARC_DocumentoArchivo`
4. Responde con `Content-Type: application/pdf` e `inline` para visualización en iframe

---

## 4. Fecha Límite de Revisión (Transversal)

### RegistrarTramite.aspx
- Campo `TxtFechaLimite` (TextMode=Date, opcional) agregado debajo del Asunto.

### RegistrarTramite.aspx.cs
- Parámetro `@FechaLimiteRevision` enviado a `FIR_I_Documento_OUT`. Si el campo está vacío, se envía `DBNull.Value`.

### Bandeja.aspx.cs — Queries
- Las 3 queries (ADMIN, FIRMADOR, REGISTRADOR) ahora incluyen `d.FechaLimiteRevision`.

### Bandeja.aspx.cs — RowDataBound
- Si `CodigoEstado == "EN_REV"` y `FechaLimiteRevision` no es null:
  - `diff.Days <= 0`: fila **roja** con texto blanco (VENCIDO)
  - `diff.Days <= 7`: fila **coral** (próximo a vencer)

### Bandeja.aspx.cs — Calendario DayRender
- Si una fecha del calendario coincide con `FechaLimiteRevision` de un trámite EN_REV:
  - Vencido: celda roja, texto blanco, tooltip "VENCIDO: código"
  - ≤7 días: celda coral, tooltip "Vence: código"
  - >7 días: celda azul claro, tooltip "Límite: código"

### Detalle.aspx + .cs
- Label `LblFechaLimite` en la sección de información
- Vencida: `⚠️ VENCIDA (dd/MM/yyyy)` en rojo negrita
- ≤7 días: `⚠️ dd/MM/yyyy (X días restantes)` en rojo
- >7 días: `dd/MM/yyyy (X días restantes)` normal
- Sin fecha: `Sin fecha límite` en gris

---

## 5. Corrección de Parámetros de SPs

| SP | Parámetro incorrecto | Corregido a |
|----|---------------------|-------------|
| `FIR_I_Conformidad` | `@IDUsuarioCreador` | `@NombreRevisor`, `@Comentario`, `@IDEquipo` |
| `FIR_X_RevisionCierre_OUT` | `@LoginUsuario` (sobrante) | Solo `@IDDocumento` + OUTPUT `@Cerrado` |
| `FIR_I_Observacion` | `@IDUsuarioCreador` | `@IDEquipo` |

---

## 6. Correcciones Menores

| Error | Solución |
|-------|----------|
| `Font-Size="0.8rem"` en Calendar | Cambiado a `Font-Size="Small"` (FontUnit no acepta rem) |
| `DataBinder` sin using | Reemplazado con cast a `DataRowView` |
| `.csproj` no incluía archivos nuevos | Agregados `Detalle.aspx`, `Detalle.aspx.cs` y `VerPDF.ashx` |

---

## SQL Pendiente (ejecutar manualmente)

```sql
ALTER PROCEDURE [dbo].[FIR_I_Documento_OUT]
    -- ... parámetros existentes ...
    @FechaLimiteRevision SMALLDATETIME = NULL,  -- AGREGAR
    @IDDocumento INT OUTPUT
AS
BEGIN
    -- En el INSERT agregar columna FechaLimiteRevision
    -- En VALUES agregar @FechaLimiteRevision
END
```
