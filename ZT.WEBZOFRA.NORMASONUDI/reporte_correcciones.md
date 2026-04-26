# Reporte de Correcciones: Proyecto ZT.WEBZOFRA.NORMASONUDI

A continuación se detalla exhaustivamente cada una de las modificaciones aplicadas a los archivos de la solución para garantizar la correcta compilación y adaptarlo a la arquitectura WebForms estricta (mediante la directiva CodeFile y el directorio nativo de App_Code).

## 1. Resolución de Problemas de Compilador (Roslyn)
- **Problema:** El entorno carecía del binario `csc.exe` dentro de `bin\roslyn\`.
- **Acción:** Se copió todo el contenido empaquetado de la plataforma (desde `packages\Microsoft.CodeDom.Providers.DotNetCompilerPlatform.2.0.1\tools\RoslynLatest\*`) hacia el directorio `bin\roslyn\`.

## 2. Unificación y Limpieza de Sub-Namespaces
- **Problema:** Múltiples errores en Visual Studio para los sitios web manejados con la carpeta de ejecución en línea `App_Code` por la incompatibilidad de múltiples Namespaces.
- **Cambio Aplicado:** Se forzó a la directiva principal `namespace ZT.WEBZOFRA.NORMASONUDI` sobre todos los elementos modificados iterados bajo el directorio de App_Code.
  - Ejemplo: `ConexionDB.cs` eliminó su importación obsoleta `ZT.WEBZOFRA.FIRMADOR.DAL`.
  - Ejemplo: `Dashboard.aspx.cs` o `Bandeja.aspx.cs` perdieron los sufijos `.Admin` o `.Tramites`.

## 3. Sustitución Completa por la Directiva CodeFile
- **Problema:** Las vistas aspx rompían los enlaces directos de variables debido al comando genérico `CodeBehind` que exige al compilador que los ensamble antes del runtime general.
- **Acciones Implementadas:** 
  - Se revisaron `Login.aspx`, `Default.aspx`, `Bandeja.aspx`, `Dashboard.aspx`, referenciándose explícitamente y actualizándolos al formato web moderno exigido `CodeFile`.
  - Se les desvinculó de sus ubicaciones anidadas del atributo `Inherits`, apuntándolos a su clase de controlador sin sufijos jerárquicos (Por ejemplo, `Inherits="ZT.WEBZOFRA.NORMASONUDI.Bandeja"` en su lugar).

## 4. Refactorización para Variables de Configuración e Inicio
### Global.asax y Global.asax.cs
- Se configuró la vista `Global.asax` para usar un CodeFile estructurado correcto apuntado al `Inherits="Global"`.
- A dicho C#, se le eliminó su nivel de namespace dejándolo como una invocación genérica parcial: `public partial class Global : System.Web.HttpApplication`.

### MasterPage.master y MasterPage.master.cs
- El enlace de directivas a la página Master base se ajustó correctamente bajo `Inherits="Shared_MasterPage"`.
- En su backend C# se removió todo namespace empaquetador para simplificar el constructor en: `public partial class Shared_MasterPage`.

## 5. Implementación Auténtica con SQL (Raw ADO.NET) - Login.aspx.cs
- **Acción principal:** Reemplazo de dependencias y abstracciones (BLL, Entity Framework). Implementación en limpio utilizando clases directas bajo `System.Data.SqlClient`.
- **Proceso de Usuarios (Load):**
  - Conexión configurada ejecutando en crudo el Stored Procedure `FIR_S_UsuariosPrueba`.
  - Data transferida internamente reescribiendo la UI desplegable hacia `strNombre` iterado y rellenando los objetos nativos sin requerir DTOs extraños.
- **Validando e Ingresando (Ingresar):**
  - Validación bajo la carga y obtención al Stored Proc principal `FIR_S_ObtenerSesion` integrando `CbxUsuario.SelectedValue` por DataReader.
  - Al recibir datos correctos, llena eficientemente las variables internas `Session["strUsuario"]`, `Session["strRol"]`, `Session["strNombre"]` y `Session["strEmail"]`.
  - Por último, lee la URL final con el formato dinámico leída (`UrlDashboard`) y redirige bajo `Response.Redirect`. Todo contenido dentro de excepciones en una UI por `<asp:Label>` en color rojo (visible únicamente en error).

## 6. Corrección de los Cadenas de Base de Datos
- **Problema:** Apunte general en entornos mal configurados. 
- **Cambio en Web.config:** La cadena `Data Source=SERVIDOR` apuntaba al vacío. Reconstruimos los atributos bajo un marco integrado en BD.
- Modificación en `Firmador`, `Administracion` y `FirmadorArchivos` inyectándoles correctamente de entorno SQL al sistema activo en `DESKTOP-8DVILE2\MSSQLSERVER2` forzando las credenciales especificas del User ID=sa bajo la misma configuración original provista.
