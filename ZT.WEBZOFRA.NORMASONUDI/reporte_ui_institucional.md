# Reporte de Mejoras: UI Institucional y Validaciones

Este documento resume las actualizaciones implementadas en el módulo de Registro de Trámites para cumplir con los estándares de diseño institucional y fortalecer la integridad de los datos ingresados.

## 1. Rediseño de la Interfaz (Diseño Institucional)

Se ha reemplazado completamente el esquema de colores oscuro inicial por un tema claro institucional que favorece la legibilidad y se alinea con sistemas administrativos empresariales o gubernamentales.

*   **Paleta de Colores:** Se estableció un fondo claro (`#f1f5f9`), utilizando un Azul Institucional (`#1E3A8A`) como color principal para títulos y botones, y un Rojo Institucional (`#b91c1c`) para resaltes e iconos.
*   **Estructura del Formulario (`RegistrarTramite.aspx`):** El formulario monolítico se dividió en secciones lógicas utilizando tarjetas (`section-card`) con bordes sutiles:
    1.  Subir Documento Original
    2.  Clasificación del Documento
    3.  Ruta de Firmas Definitiva
*   **Distribución (Grid System):** Se implementó un sistema de filas (`row`) y columnas (`col-md-6`) para agrupar campos relacionados (ej. Área de Origen y Tipo de Documento en la misma fila), ahorrando espacio vertical.
*   **Tipografía:** Se integró la fuente *Roboto* desde Google Fonts, estándar en sistemas de interfaz de usuario de alta claridad.

## 2. Soporte y Codificación de Caracteres

Se resolvieron los problemas donde las tildes, las letras "ñ" y otros caracteres especiales se renderizaban como símbolos ilegibles (ej. `Ã³`).

*   **Meta Charset UTF-8:** Se agregó `<meta charset="utf-8" />` al `<head>` de los archivos clave (`RegistrarTramite.aspx`, `Bandeja.aspx`, `Login.aspx`) garantizando que el navegador interprete el texto estático correctamente.

## 3. Validación de Campos de Texto

Se implementó validación de expresiones regulares (`RegularExpressionValidator`) en los campos de entrada para permitir texto enriquecido sin comprometer la seguridad.

*   **Caracteres Permitidos:** Los campos `Asunto`, `Área Responsable` y `Código de Documento` ahora permiten explícitamente letras (mayúsculas y minúsculas), números, espacios, **tildes**, la letra **ñ/Ñ**, la diéresis (**ü/Ü**), y signos gramaticales comunes (`.,;:-_()!¡?¿`).
*   **Desactivación de UnobtrusiveValidationMode:** Para evitar errores internos de ASP.NET 4.5+ al usar validadores sin jQuery, se modificó el `Web.config` añadiendo:
    `<add key="ValidationSettings:UnobtrusiveValidationMode" value="None" />`
    Esto asegura que las validaciones del cliente funcionen mediante el JavaScript clásico de ASP.NET sin requerir dependencias externas.

## 4. Reglas de Negocio en la Ruta de Firmas

Se fortaleció tanto la experiencia de usuario (Frontend) como la validación de servidor (Backend) para la selección de firmantes.

### 4.1. Prevención de Usuarios Duplicados (Frontend)
*   Se agregó la clase `ddl-firmante` a la lista de usuarios.
*   Se modificó el JavaScript interno de la vista (`updateAllDropdowns`) para que comparta la lógica que ya tenían los órdenes. Si un empleado es seleccionado en una fila, su nombre **se deshabilita automáticamente y se marca en rojo** en los selectores del resto de filas, previniendo visualmente que se intente añadir dos veces a la misma persona en el flujo.

### 4.2. Secuencialidad Estricta de Órdenes (Backend)
*   En el evento `BtnRegistrar_Click` (C#), se añadió una validación matemática de la lista de órdenes.
*   El sistema ahora captura los órdenes asignados, los ordena numéricamente y valida de manera iterativa que comiencen en el número 1 y avancen de 1 en 1.
*   Si se detecta **cualquier salto** (por ejemplo, intentar enviar un orden `1, 2, 4`), el registro se detiene y se lanza la alerta: *"El orden de firmas debe ser secuencial y sin saltos empezando desde 1 (ej: 1, 2, 3...)"*.
