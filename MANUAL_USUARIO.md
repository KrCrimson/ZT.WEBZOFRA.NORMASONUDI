# Manual de Usuario: Sistema de Firma Digital (Zofratacna)

Este manual está dirigido a los usuarios finales del sistema **ZT.WEBZOFRA.NORMASONUDI** (Registradores de trámites y Firmantes). Aquí encontrará los pasos sencillos para acceder, registrar un documento, firmarlo utilizando su **DNI electrónico (DNIe)** o su **USB Token**, y realizar el seguimiento de sus trámites en la bandeja.

---

## 📌 Contenido
1. [Cómo Iniciar Sesión (Acceso al Sistema)](#1-cómo-iniciar-sesión-acceso-al-sistema)
2. [Cómo Registrar un Trámite y Subir el PDF](#2-cómo-registrar-un-trámite-y-subir-el-pdf)
3. [Cómo Firmar con DNI electrónico (DNIe)](#3-cómo-firmar-con-dni-electrónico-dnie)
4. [Cómo Firmar con USB Token de Firma](#4-cómo-firmar-con-usb-token-de-firma)
5. [Cómo Revisar el Estado de un Trámite en la Bandeja](#5-cómo-revisar-el-estado-de-un-trámite-en-la-bandeja)
6. [¿Qué hacer si algo falla? (Mensajes de error comunes)](#6-qué-hacer-si-algo-falla-mensajes-de-error-comunes)

---

## 1. Cómo Iniciar Sesión (Acceso al Sistema)

Para ingresar al sistema firmador, siga estos sencillos pasos:

1. Abra su navegador web e ingrese a la dirección del sistema proporcionada por la institución.
2. Verá la pantalla de acceso con el título **ZOFRA TACNA - SISTEMA DE GESTIÓN DE FIRMA DIGITAL**.
3. En el campo **"Seleccione su cuenta"**, haga clic en la lista desplegable.
4. Busque y seleccione su nombre completo de la lista de usuarios activos.
5. Haga clic en el botón azul **"Iniciar Sesión"**.
6. El sistema lo redirigirá automáticamente a su **Bandeja de Trámites** según su rol de usuario (Registrador o Firmante).

> [!NOTE]
> Este sistema no requiere que ingrese una contraseña escrita. El acceso se valida directamente seleccionando su cuenta autorizada.

---

## 2. Cómo Registrar un Trámite y Subir el PDF
*(Pasos exclusivos para el rol de **Registrador** o **Administrador**)*

Si su función consiste en iniciar trámites y subir documentos para que otros los firmen, realice lo siguiente:

1. En el menú superior o lateral, haga clic en la opción **"Registrar Trámite"**.
2. Complete la sección **Datos del Documento**:
   - **Tipo de Documento**: Seleccione una opción de la lista (por ejemplo: Oficio, Informe, Carta, etc.). Al elegirlo, el sistema autogenerará un código de documento único en la parte superior (ejemplo: *OFI-2026-0005*).
   - **Asunto**: Escriba una breve descripción o título del documento. Sea claro para que los firmantes identifiquen el trámite fácilmente.
   - **Área Responsable**: Elija el departamento o área encargada de generar el trámite de la lista desplegable.
   - **Fecha Límite** *(Opcional)*: Si necesita que el documento sea revisado o firmado antes de una fecha límite, haga clic en el campo y seleccione la fecha en el calendario.
3. Suba el archivo **PDF**:
   - Haga clic en el botón **"Examinar"** o **"Seleccionar archivo"**.
   - Busque y elija el documento en formato PDF en su computadora.
   - **Requisitos obligatorios del archivo:**
     - Debe ser estrictamente un archivo con extensión **.pdf**.
     - No debe superar el tamaño máximo de **50 Megabytes (MB)**.
     - Debe ser un PDF limpio, **sin firmas digitales previas** aplicadas fuera del sistema.
4. Defina la **Secuencia de Firmantes** (Línea de Tiempo):
   - En la sección de firmantes, busque al personal encargado en la lista de empleados disponibles.
   - Añádalos al trámite. El orden en el que los agregue definirá la secuencia en la que deberán firmar (el Firmante 1 debe firmar primero para que el sistema le habilite la opción al Firmante 2, y así sucesivamente).
   - Recuerde que debe agregar **al menos un firmante** para poder continuar.
5. Guarde y envíe el documento:
   - Una vez completados los datos, verificado el PDF y asignados los firmantes, haga clic en el botón verde **"Registrar Trámite"**.
   - Si todo está correcto, el sistema guardará el archivo, enviará notificaciones por correo electrónico a los involucrados e iniciará la ronda de firmas. Será redirigido a su bandeja con un mensaje de éxito.

---

## 3. Cómo Firmar con DNI electrónico (DNIe)
*(Pasos para **Firmantes** que usan su DNI electrónico)*

Para firmar un documento utilizando su DNIe de RENIEC, siga estos pasos:

1. **Preparación en su computadora:**
   - Asegúrese de que el lector de tarjetas inteligentes esté conectado a un puerto USB de su computadora.
   - Introduzca su **DNI electrónico** en el lector.
   - Verifique que la aplicación de soporte local (**ZofraFirmaAgent.exe**) esté abierta y activa en su PC (notará un icono con forma de **escudo de seguridad** en la esquina inferior derecha de su pantalla de Windows, cerca de la hora).
2. **Acceso al documento:**
   - Inicie sesión en el sistema web y vaya a su bandeja de **"Pendientes de Firma"** o **"Pendientes de Revisión"**.
   - Busque el documento correspondiente y haga clic en **"Ver Detalle"** o haga clic directamente para firmar.
   - Seleccione la opción de firma: **"Firma con DNI electrónico"**.
3. **Selección del certificado y configuración:**
   - El sistema cargará una pantalla con la previsualización del documento.
   - En el campo **"Certificado disponible"**, haga clic en la lista. Debería visualizar sus certificados personales de firma digital emitidos por RENIEC.
   - Si la lista aparece vacía o no encuentra su certificado actual, asegúrese de que su DNIe esté bien insertado en el lector y haga clic en el botón **"Refrescar"**.
   - Seleccione el certificado correspondiente a su firma digital.
   - Elija la **Orientación de la firma**: marque **"Horizontal"** (diseño estándar y recomendado) o **"Vertical"** (ideal para márgenes estrechos).
4. **Posicionamiento de la firma:**
   - En el recuadro visual del documento PDF que se muestra en pantalla, haga clic con el mouse sobre la página donde desea estampar su firma.
   - Aparecerá un **recuadro de color rojo**.
   - Haga clic sostenido sobre el recuadro rojo y **arrástrelo** con el mouse exactamente al lugar de la página donde quiere que aparezca su sello de firma.
   - **Regla importante:** No coloque el recuadro rojo encima de otras firmas ya existentes. Busque un espacio en blanco adecuado.
5. **Aplicar la firma:**
   - Haga clic en el botón rojo **"Firmar Documento"**.
   - En ese instante, Windows abrirá una pequeña ventana de seguridad de RENIEC solicitándole su **clave secreta o PIN de firma** del DNIe.
   - Escriba su PIN con cuidado y haga clic en **Aceptar**.
6. **Finalización:**
   - El sistema procesará el documento durante unos segundos.
   - Si todo es correcto, se ocultará el panel y se mostrará una pantalla de color verde con el mensaje: **"¡Éxito! Firma Registrada"**.
   - Haga clic en el botón **"Ir a mi Bandeja"** para continuar con otros trámites. El sistema notificará por correo electrónico al siguiente firmante en la cola.

---

## 4. Cómo Firmar con USB Token de Firma
*(Pasos para **Firmantes** que usan un Token USB criptográfico - Bit4id)*

Si usted utiliza un dispositivo en forma de memoria USB para firmar digitalmente (USB Token), el procedimiento es el siguiente:

1. **Preparación en su computadora:**
   - Conecte el **USB Token** en un puerto USB libre de su computadora.
   - Asegúrese de que el agente local (**ZofraFirmaAgent.exe**) esté activo en segundo plano (icono del escudo cerca de la hora de Windows).
2. **Acceso al documento:**
   - Inicie sesión, vaya a su bandeja de pendientes y abra el detalle del documento.
   - Seleccione la opción de firma: **"Firma con USB-Token"**.
3. **Selección del certificado y configuración:**
   - Verá la previsualización del PDF en pantalla.
   - En el campo **"Certificado disponible"**, elija su certificado personal de la lista desplegable.
   - Si no aparece listado, verifique la conexión física de su USB Token y haga clic en **"Refrescar"**. *(Nota: El sistema filtra automáticamente y no mostrará certificados del DNIe en esta sección para evitar confusiones).*
   - Marque la **Orientación de la firma** deseada (Horizontal o Vertical).
4. **Posicionamiento de la firma:**
   - Haga clic en la hoja del documento que corresponda para que se dibuje un **recuadro de color azul**.
   - **Arrastre** el recuadro azul con el mouse y colóquelo sobre el espacio en blanco designado para su firma.
5. **Aplicar la firma:**
   - Haga clic en el botón azul **"Firmar Documento"**.
   - Se abrirá la ventana gráfica de seguridad correspondiente al software del fabricante de su Token (Bit4id).
   - Escriba el **PIN de seguridad** de su USB Token y haga clic en **Aceptar**.
6. **Finalización:**
   - Al terminar de procesar, aparecerá la pantalla verde de **"¡Éxito! Firma Registrada"**.
   - Presione **"Ir a mi Bandeja"** para regresar al menú principal.

---

## 5. Cómo Revisar el Estado de un Trámite en la Bandeja

La **Bandeja de Trámites** es su centro de control. En ella podrá buscar documentos, filtrar por estados y ver alertas urgentes:

### A. Los Estados del Documento
En la columna "Estado" de su lista de trámites, verá los siguientes estados que le indican en qué parte del camino se encuentra el archivo:
- **En Revisión**: El documento ha sido creado y se encuentra bajo la supervisión de los firmantes previos.
- **En Firma / Pendiente de Firma**: El documento está listo para ser firmado. Si ya fue firmado por algunos usuarios pero faltan otros, se mostrará en este estado como firma parcial.
- **Observados**: El documento tiene correcciones pendientes o fue rechazado por algún revisor.
- **Completados**: El trámite ha finalizado con éxito. El documento cuenta con todas las firmas requeridas y está listo para su descarga o archivado final.

### B. El Sistema de Alertas por Colores
Para evitar retrasos en trámites con fecha de vencimiento, la bandeja utiliza un semáforo de colores en sus filas:
- 🔴 **Fila de color Rojo**: Significa que el documento **está vencido**. Ha superado la "Fecha Límite" establecida sin que se haya completado su revisión o firma. Requiere atención inmediata.
- 🟡 **Fila de color Coral o Rosado**: Significa que el documento está **próximo a vencer** (le quedan 7 días o menos antes de llegar a su Fecha Límite).
- ⚪ **Fila de color Blanco o Gris Estándar**: El documento está a tiempo y tiene un plazo amplio disponible.

### C. Alertas al Iniciar Sesión
Si al ingresar al sistema tiene algún trámite pendiente que ya se venció o que está a punto de vencer, el sistema le mostrará un cuadro de advertencia de color rojo en la parte superior de su pantalla indicándole la cantidad exacta de revisiones que debe atender de inmediato.

### D. Búsqueda y Limpieza de Filtros
- **Buscador**: Escriba el código del documento (por ejemplo: *OFI-2026-0005*) o alguna palabra clave del asunto en la barra de búsqueda y haga clic en **"Buscar"** para filtrar la grilla al instante.
- **Limpiar**: Si quiere quitar el filtro y volver a ver toda su lista, haga clic en el botón **"Limpiar"**.
- **Menú Lateral / Superior**: Puede hacer clic en las diferentes categorías del menú (Historial, En Revisión, En Firma, Completados) para filtrar automáticamente los documentos según la acción seleccionada.

---

## 6. ¿Qué hacer si algo falla? (Mensajes de error comunes)

Si experimenta algún inconveniente durante el uso del sistema, verifique aquí la causa y su solución sencilla:

### 1. Mensaje: *"No se encontraron certificados con clave privada. Inserte el DNIe y haga clic en Refrescar."*
- **Causa:** Su lector de tarjetas no detecta físicamente el chip de su DNI electrónico o USB Token, o los conectores del chip están sucios.
- **Solución:**
  1. Retire su DNIe del lector y limpie suavemente el chip dorado con un paño seco y suave.
  2. Introduzca firmemente el DNIe en el lector (con el chip mirando hacia arriba o en la dirección que indique su ranura).
  3. Asegúrese de que la luz del lector esté encendida o parpadeando.
  4. Haga clic en el botón **"Refrescar"** en la página web.

### 2. Mensaje: *"La posición seleccionada se superpone con una firma existente."*
- **Causa:** Intentó colocar el recuadro de su firma (rojo o azul) encima de un área que ya fue firmada por otro participante. El sistema bloquea esto para evitar que las firmas digitales se encimen y se vuelvan ilegibles.
- **Solución:** Mueva el recuadro arrastrándolo a una zona libre o en blanco del documento y haga clic en "Firmar Documento" nuevamente.

### 3. Mensaje: *"No es su turno de firmar o el documento no requiere su firma en este momento."*
- **Causa:** El documento requiere firmas en orden secuencial estricto y todavía hay firmantes previos que no han estampado su firma digital.
- **Solución:** Debe esperar a que sus colegas firmen el archivo. El sistema le enviará una notificación automática por correo electrónico en el instante en que sea su turno.

### 4. Mensaje con letras rojas: *"La tarjeta inteligente no es compatible con esta operación"* o *"La tarjeta inteligente no puede realizar la operación solicitada..."*
- **Causa:** Ocurrió un conflicto interno entre los controladores de su DNIe y su USB Token en el sistema Windows. Esto suele suceder si tiene instalados los programas de DNIe (RENIEC) y de USB Token (Bit4id) al mismo tiempo en la computadora.
- **Solución rápida:**
  1. Diríjase al menú de iconos ocultos de Windows (en la barra de tareas abajo a la derecha, al lado del reloj).
  2. Busque el icono con forma de **escudo gris/azul** correspondiente a la aplicación local **ZofraFirmaAgent**.
  3. Haga clic derecho sobre dicho escudo y seleccione la opción **"Restaurar Middlewares"**.
  4. El agente reparará y alineará la configuración de Windows en un par de segundos.
  5. Vuelva al navegador web, recargue o refresque la página (presione la tecla **F5** de su teclado) e intente firmar de nuevo.
