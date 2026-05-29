# Manual de Instalación y Configuración: Sistema de Firma Digital (Zofratacna)

Este manual está dirigido al administrador de TI o ingeniero de sistemas responsable del despliegue, configuración y puesta en producción del sistema de firmas **ZT.WEBZOFRA.NORMASONUDI**.

El sistema consta de dos componentes principales:
1. **Aplicación Web Servidor (ASP.NET Web Forms + SQL Server)**: Gestiona el portal web, la bandeja de trámites y la lógica criptográfica del servidor.
2. **Agente de Escritorio Local (ZofraFirmaAgent.exe)**: Servicio liviano instalado en cada PC cliente que resuelve dinámicamente la coexistencia e interoperabilidad entre los controladores del **DNI electrónico peruano (RENIEC)** y los **USB Tokens de Firma (Bit4id)**.

---

## 📌 Contenido
1. [Requisitos de Hardware y Software](#1-requisitos-de-hardware-y-software)
2. [Instalación de SQL Server y Creación de las 3 BDs](#2-instalación-de-sql-server-y-creación-de-las-3-bds)
3. [Configuración de IIS y Despliegue del Sistema](#3-configuración-de-iis-y-despliegue-del-sistema)
4. [Configuración del Web.config (Cadenas de Conexión)](#4-configuración-del-webconfig-cadenas-de-conexión)
5. [Instalación y Configuración en las Estaciones de Trabajo (Clientes)](#5-instalación-y-configuración-en-las-estaciones-de-trabajo-clientes)
   - [A. Instalación de IDProtect Client (RENIEC)](#a-instalación-de-idprotect-client-reniec)
   - [B. Instalación del Agente de Firma (ZofraFirmaAgent.exe)](#b-instalación-del-agente-de-firma-zofrafirmaagentexe)
   - [C. Instalación de Bit4id (Universal Middleware)](#c-instalación-de-bit4id-universal-middleware)
   - [D. Configuración de Tarjetas Adicionales (Archivos .reg)](#d-configuración-de-tarjetas-adicionales-archivos-reg)
6. [Pasos para Verificar que Todo Funciona Correctamente](#6-pasos-para-verificar-que-todo-funciona-correctamente)

---

## 1. Requisitos de Hardware y Software

### A. Servidor de Aplicaciones y Base de Datos
- **Procesador (CPU)**: Mínimo 2 núcleos a 2.0 GHz (x64) o superior.
- **Memoria RAM**: Mínimo 4 GB (Se recomiendan 8 GB o más si comparte IIS y SQL Server en la misma máquina).
- **Almacenamiento**: Mínimo 50 GB de espacio disponible. *(Nota: El repositorio de archivos PDF firmados reside en una base de datos SQL Server, por lo que el crecimiento del disco dependerá directamente del volumen documental).*
- **Sistema Operativo (OS)**: Windows Server 2012 R2, 2016, 2019 o 2022. (Para entornos de desarrollo o pruebas locales se permite Windows 10 u 11 Pro).
- **Servidor Web (IIS)**: Internet Information Services (IIS) versión 8.0 o superior, con la característica de ASP.NET 4.8 y la Extensibilidad de .NET 4.8 habilitadas.
- **Entorno de Ejecución**: .NET Framework 4.8 Runtime instalado en el servidor.
- **Motor de Base de Datos**: Microsoft SQL Server 2012 o superior (ediciones Express, Standard o Enterprise).

### B. Estación de Trabajo del Usuario (PC Cliente)
- **Sistema Operativo**: Windows 10 o Windows 11 (ediciones de 32 o 64 bits).
- **Entorno de Ejecución**: .NET Framework 4.8 Runtime (requerido para ejecutar `ZofraFirmaAgent.exe`).
- **Hardware de Firma**: 
  - Lector de tarjetas inteligentes compatible con el estándar ISO/IEC 7816 (para uso con DNIe).
  - Puerto USB físico libre (para uso con USB Token).

---

## 2. Instalación de SQL Server y Creación de las 3 BDs

El sistema utiliza tres bases de datos desacopladas para mantener la integridad, el rendimiento de los archivos y la gestión de accesos corporativos. Estas bases de datos se denominan:
- **`administracion`**: Gestiona el catálogo de empleados corporativos y sus roles.
- **`Firmador`**: Almacena el metadato de los documentos, el flujo de firmas, la secuencialidad, las auditorías y los estados del trámite.
- **`Firmador_Archivos`**: Repositorio binario exclusivo para almacenar los archivos PDF originales y los PDF firmados intermedios y finales en formato binario (BLOB).

### Pasos para la creación de las Bases de Datos:
1. Inicie sesión en SQL Server Management Studio (SSMS) con una cuenta que tenga privilegios de administrador (`sysadmin` o similar).
2. Abra y ejecute secuencialmente los tres scripts ubicados en la carpeta `/SQL` del proyecto:
   
   - **Paso 2.1: Base de Datos de Administración**
     - Abra el script [1_Administracion.sql](file:///c:/Users/windows11/Desktop/ZT.WEBZOFRA.NORMASONUDI/SQL/1_Administracion.sql) en SSMS.
     - Ejecútelo por completo. Este script creará la base de datos `administracion` e inicializará las tablas base de empleados, cargos corporativos, sedes y la vista `FIR_VW_EmpleadosActivos`.
   
   - **Paso 2.2: Base de Datos del Firmador**
     - Abra el script [2_Firmador.sql](file:///c:/Users/windows11/Desktop/ZT.WEBZOFRA.NORMASONUDI/SQL/2_Firmador.sql) en SSMS.
     - Ejecútelo por completo. Creará la base de datos `Firmador` junto con todas las tablas del core de firmas (`FIR_Documento`, `FIR_DocumentoFirmante`, `FIR_DocumentoRevisor`), las vistas de control de sesión y los procedimientos almacenados indispensables para la ejecución del portal.
   
   - **Paso 2.3: Base de Datos de Archivos**
     - Abra el script [3_Firmador_Archivos.sql](file:///c:/Users/windows11/Desktop/ZT.WEBZOFRA.NORMASONUDI/SQL/3_Firmador_Archivos.sql) en SSMS.
     - Ejecútelo. Creará la base de datos `Firmador_Archivos` y la estructura de tablas binarias `ARC_DocumentoArchivo` junto con los procedimientos almacenados de lectura y escritura de BLOBs.

3. Cree un usuario exclusivo de SQL Server (por ejemplo, `usr_firmador`) o configure una cuenta de Windows local para que IIS se conecte a las bases de datos. Dicha cuenta debe tener asignados los roles de `db_datareader`, `db_datawriter` y permisos de ejecución (`EXECUTE`) en las tres bases de datos indicadas.

---

## 3. Configuración de IIS y Despliegue del Sistema

Siga esta guía para alojar el sitio web de ASP.NET Web Forms en el servidor IIS:

1. Copie el directorio de la aplicación web del proyecto [ZT.WEBZOFRA.NORMASONUDI](file:///c:/Users/windows11/Desktop/ZT.WEBZOFRA.NORMASONUDI/ZT.WEBZOFRA.NORMASONUDI) a una ruta permanente en el servidor (ejemplo: `C:\inetpub\wwwroot\ZT.WEBZOFRA.NORMASONUDI`).
2. Abra el **Administrador de Internet Information Services (IIS)** (`inetmgr`).
3. **Crear el Application Pool (Pool de Aplicaciones)**:
   - En el panel izquierdo, haga clic derecho en "Pools de Aplicaciones" y seleccione **"Agregar pool de aplicaciones..."**.
   - Nombre: `ZofraFirmaPool`
   - Versión de .NET CLR: `.NET CLR versión v4.0.30319` (versión compatible con .NET 4.8).
   - Modo de canalización administrada (Pipeline): `Integrated`.
   - Haga clic en **Aceptar**.
   - *(Opcional)* Vaya a la Configuración Avanzada del Pool y verifique que la propiedad "Identidad" esté configurada como `ApplicationPoolIdentity` o el usuario de servicio de base de datos que haya designado.
4. **Crear el Sitio Web / Aplicación**:
   - Haga clic derecho sobre "Sitios" o sobre su sitio predeterminado (Default Web Site) y elija **"Agregar sitio web..."** o **"Agregar aplicación..."**.
   - Alias / Nombre: `ZT.WEBZOFRA.NORMASONUDI`
   - Pool de Aplicaciones: Seleccione `ZofraFirmaPool`.
   - Ruta de acceso física: Seleccione la carpeta donde copió el proyecto (que contiene el archivo `Web.config` y las páginas `.aspx`).
   - Configuración de Enlaces: Asigne el puerto web (ejemplo: `80` o un puerto alternativo) y el nombre de host correspondiente a la intranet.
5. **Configuración de Autenticación**:
   - Seleccione la aplicación recién creada en IIS.
   - En la sección central, haga doble clic en el icono **"Autenticación"**.
   - Asegúrese de que la **Autenticación Anónima** esté **Habilitada** (el inicio de sesión y validación de usuarios es administrado directamente por el code-behind en `Login.aspx` interactuando con la base de datos).
6. **Permisos de Archivos**:
   - Conceda permisos de Lectura al usuario local `IIS_IUSRS` o a la identidad de `ZofraFirmaPool` sobre la carpeta física del proyecto en el disco para evitar errores de acceso 401.3.

---

## 4. Configuración del Web.config (Cadenas de Conexión)

Abra el archivo `Web.config` de la raíz del portal desplegado y configure la sección `<connectionStrings>` para apuntar a la instancia de SQL Server de su institución.

El formato por defecto del archivo es el siguiente:

```xml
<configuration>
  <!-- ... -->
  <connectionStrings>
    <!-- Conexión principal del flujo de firmas -->
    <add name="Firmador" 
         connectionString="Data Source=SERVSQL\INSTANCIA;Initial Catalog=Firmador;User ID=usuario_bd;Password=clave_bd" 
         providerName="System.Data.SqlClient" />
    
    <!-- Conexión para validación de empleados corporativos -->
    <add name="Administracion" 
         connectionString="Data Source=SERVSQL\INSTANCIA;Initial Catalog=administracion;User ID=usuario_bd;Password=clave_bd" 
         providerName="System.Data.SqlClient" />
    
    <!-- Conexión exclusiva para el repositorio binario de documentos -->
    <add name="FirmadorArchivos" 
         connectionString="Data Source=SERVSQL\INSTANCIA;Initial Catalog=Firmador_Archivos;User ID=usuario_bd;Password=clave_bd" 
         providerName="System.Data.SqlClient" />
  </connectionStrings>
  
  <system.web>
    <!-- Obligatorio tener configurado .NET Framework 4.8 -->
    <compilation debug="false" targetFramework="4.8" />
    <httpRuntime targetFramework="4.8" />
    <globalization fileEncoding="utf-8" requestEncoding="utf-8" responseEncoding="utf-8" culture="es-PE" uiCulture="es-PE" />
  </system.web>
  
  <system.webServer>
    <defaultDocument>
      <files>
        <clear />
        <!-- Garantiza que el portal inicie en el control de acceso -->
        <add value="Login.aspx" />
      </files>
    </defaultDocument>
  </system.webServer>
  <!-- ... -->
</configuration>
```

> [!WARNING]
> En producción, asegúrese de cambiar el valor `debug="true"` a `debug="false"` dentro de la etiqueta `<compilation>` para optimizar el rendimiento criptográfico del servidor y evitar la exposición de trazas de error al usuario.

---

## 5. Instalación y Configuración en las Estaciones de Trabajo (Clientes)

Ambos middlewares criptográficos de firma compartían históricamente el mismo identificador ATR físico en la base de datos de tarjetas inteligentes de Windows (**Calais**), registrando la clave `"  NXP IAS C18"`. Al instalar Bit4id, éste secuestraba los accesos de DNIe, corrompiendo las firmas. 

La solución de Zofratacna consiste en instalar ambos middlewares en cada PC y dejar que el agente de escritorio liviano `ZofraFirmaAgent` resuelva el conflicto a nivel de registro y servicios en caliente cuando el navegador abre cada página de firma.

Siga estos pasos de instalación en cada PC cliente de los firmantes:

---

### A. Instalación de IDProtect Client (RENIEC)
1. Instale el instalador MSI oficial provisto por RENIEC correspondiente al software de comunicación para lectoras de tarjetas inteligentes: **NXP IDProtect Client**.
2. Este paquete escribirá la librería nativa de comunicación `ciamd.dll` y la herramienta de monitoreo del lector.
3. Copie el archivo de script de instalación de certificados [Instalar-Certificados-RENIEC.ps1](file:///c:/Users/windows11/Desktop/ZT.WEBZOFRA.NORMASONUDI/Instalar-Certificados-RENIEC.ps1) y el certificado [ECERNEP-ROOT-3.crt](file:///c:/Users/windows11/Desktop/ZT.WEBZOFRA.NORMASONUDI/ECERNEP-ROOT-3.crt) en una carpeta local de la estación de trabajo.
4. Abra una consola de **PowerShell como Administrador** y ejecute el instalador:
   ```powershell
   Set-ExecutionPolicy Bypass -Scope Process -Force
   .\Instalar-Certificados-RENIEC.ps1
   ```
5. El script realizará las siguientes operaciones críticas de confianza:
   - Descargará e instalará la cadena intermedia **ECEP-RENIEC CA Class 2 II** (`caclass2ii.crt`) en el almacén de entidades emisoras intermedias (`CA`).
   - Descargará e instalará la cadena intermedia **ECEP-RENIEC** (`ecep.crt`) en el almacén `CA`.
   - Registrará el certificado raíz de confianza **ECERNEP PERU CA ROOT 3** (`ECERNEP-ROOT-3.crt`) en el almacén local del equipo de entidades de certificación raíz de confianza (`Root`), desplegando el cuadro de confirmación de seguridad de Windows si fuese necesario.
   - Comprobará mediante la herramienta nativa `certutil -scinfo` si la lectora y la tarjeta responden de manera correcta.

---

### B. Instalación del Agente de Firma (ZofraFirmaAgent.exe)
1. Copie el ejecutable precompilado [ZofraFirmaAgent.exe](file:///c:/Users/windows11/Desktop/ZT.WEBZOFRA.NORMASONUDI/ZofraFirmaAgent.exe) ubicado en la raíz del proyecto a una ubicación definitiva en el disco local de la PC del firmante (ejemplo recomendado: `C:\Program Files\ZofraFirmaAgent\ZofraFirmaAgent.exe`).
2. Ejecute el programa **ZofraFirmaAgent.exe por primera vez como Administrador** (clic derecho > "Ejecutar como Administrador").
3. El agente se auto-registrará a nivel de sistema operativo en las claves de protocolo de comunicación de Windows de la siguiente manera:
   - Creará la subclave `Software\Classes\zofrafirma` en el registro de Windows del usuario actual (`HKCU`).
   - Asociará el esquema de protocolo personalizado `zofrafirma://` para apuntar a la ruta del ejecutable actual con el argumento `%1`.
4. A partir de este momento, cuando el usuario navegue en la aplicación web, el navegador despertará al agente en segundo plano mediante llamados de URL invisibles (`zofrafirma://dnie`, `zofrafirma://token`, `zofrafirma://reset`).
5. **Configuración de Auto-Inicio en Windows**:
   - Para que el usuario final no tenga que abrir manualmente el agente cada vez que encienda la PC, cree un acceso directo del ejecutable.
   - Presione las teclas `Win + R`, escriba `shell:startup` y presione Enter para abrir la carpeta de Inicio de Windows.
   - Pegue el acceso directo creado allí. Configure las propiedades del acceso directo para que se ejecute con privilegios elevados de Administrador (pestaña Compatibilidad > "Ejecutar este programa como administrador").

---

### C. Instalación de Bit4id (Universal Middleware)
1. En las estaciones de trabajo de los firmantes que utilicen un **USB Token físico**, instale el instalador provisto por el fabricante correspondiente a: **Bit4id Universal Middleware** (o Bit4id PKI Manager).
2. Este paquete instalará la librería mini-driver de comunicación `bit4xpki.dll` en los directorios de Windows `System32` y `SysWOW64`.
3. Al finalizar la instalación, se creará el registro por defecto del ATR `"  NXP IAS C18"` redireccionando el hardware hacia el proveedor de Bit4id.
*(Nota: No requiere configuraciones manuales de rediseño en el instalador de Bit4id ya que el agente de Zofratacna resolverá el redireccionamiento automáticamente cuando se requiera).*

---

### D. Configuración de Tarjetas Adicionales (Archivos .reg)
Si la institución utiliza lotes especiales de tarjetas inteligentes compatibles con la infraestructura PKI nacional de otras arquitecturas físicas (como tarjetas basadas en CHIPDOC o LPKI), debe registrar los ATR correspondientes en el almacén Calais global del sistema de Microsoft.

Para ello, ubique los archivos de registro en la raíz del instalador y aplíquelos en cada PC del firmante (haga doble clic sobre cada archivo y acepte la advertencia de importación o ejecútelos desde la consola de comandos de Windows como administrador):
- **[CHIPDOC.reg](file:///c:/Users/windows11/Desktop/ZT.WEBZOFRA.NORMASONUDI/CHIPDOC.reg)**: Asocia las tarjetas con ATR de CHIPDOC para que utilicen la DLL `ciamd.dll` nativa y el proveedor criptográfico de smart cards de Microsoft.
- **[LPKI.reg](file:///c:/Users/windows11/Desktop/ZT.WEBZOFRA.NORMASONUDI/LPKI.reg)**: Asocia las tarjetas LPKI para operar con el proveedor de Microsoft y evitar bloqueos.
- **[LPKI01.reg](file:///c:/Users/windows11/Desktop/ZT.WEBZOFRA.NORMASONUDI/LPKI01.reg)**: Mapea las especificaciones del lote LPKI01 a la librería nativa `ciamd.dll`.

---

## 6. Pasos para Verificar que Todo Funciona Correctamente

Una vez finalizada la configuración del Servidor, la Base de Datos y las PCs Clientes, realice la siguiente lista de verificación técnica:

### Paso 1: Verificación de la Conexión de Base de Datos y IIS
- Abra el navegador en cualquier estación de trabajo y cargue la URL del portal desplegado.
- La pantalla `Login.aspx` debe cargar sin errores HTTP de compilación o de acceso a datos.
- Despliegue el cuadro de lista **"Seleccione su cuenta"**. La lista de usuarios activos debe poblarse con los nombres completos extraídos directamente de la tabla `Empleado` en la base de datos `administracion`. Si se muestran nombres correctos, las credenciales de conexión del `Web.config` son 100% correctas.

### Paso 2: Verificación de la Autoelevación e Inicio del Agente
- Inicie la PC cliente del usuario. El agente `ZofraFirmaAgent.exe` debe iniciarse automáticamente y mostrar el icono del escudo en la bandeja del sistema de Windows (junto a la hora).
- Haga clic derecho en el escudo y seleccione **"Ver Log"**. Debe abrirse el bloc de notas cargando el archivo `agent_log.txt` con la línea de inicialización correcta, lo que comprueba que tiene permisos de escritura locales y named pipes operativos.

### Paso 3: Prueba Dinámica de Conmutación en Caliente (DNIe)
- Inicie sesión con un usuario que tenga el rol de Firmante y que cuente con un trámite pendiente en su bandeja.
- Inserte el **DNI electrónico** en el lector.
- Haga clic en el botón para firmar el documento mediante **"Firma con DNI electrónico"**.
- En el instante en que cargue la página `FirmaDigital.aspx`, la página web disparará en el cliente el protocolo `zofrafirma://dnie`.
- Verifique que Windows muestre inmediatamente un mensaje de notificación de globo cerca de la hora indicando: **"✔ Modo DNIe activado (IDProtect)"**.
- Revise la bitácora (`agent_log.txt`) en el cliente. Debe visualizarse la siguiente secuencia:
  1. Comando recibido: `DNIE`
  2. Detención de los servicios de Windows `CertPropSvc` y `SCardSvr` en secuencia ordenada para liberar locks físicos.
  3. Purga recursiva del ATR de Bit4id en `HKLM` (vistas de 32 y 64 bits) y `HKCU`.
  4. Creación del backup de claves Calais en memoria.
  5. Escritura de los mappings de `ciamd.dll`, `Microsoft Base Smart Card Crypto Provider` y `Microsoft Smart Card Key Storage Provider`.
  6. Reinicio exitoso del servicio de tarjetas inteligentes `SCardSvr` y del propagador `CertPropSvc`.
  7. Levantamiento automático del monitor nativo `IDProtect Monitor.exe`.
- En la página web, haga clic en **"Refrescar"**. La lista de certificados de RENIEC debe aparecer disponible.
- Mueva el recuadro de firma, haga clic en **"Firmar Documento"**. Debe mostrarse el cuadro gráfico de solicitud de PIN nativo de Microsoft/IDProtect de forma fluida. Ingrese el PIN y verifique que el sistema firme y guarde el PDF en `Firmador_Archivos` arrojando éxito.

### Paso 4: Prueba Dinámica de Conmutación en Caliente (USB Token)
- Regrese a la bandeja de pendientes.
- Conecte el **USB Token de Bit4id** en la PC y seleccione en el sistema web la opción **"Firma con USB-Token"**.
- En el instante de cargar `FirmaUsbToken.aspx`, la web enviará al agente el comando `zofrafirma://token`.
- Verifique que se despliegue el globo de notificación de Windows: **"✔ Modo USB Token activado (Bit4id)"**.
- Revise la bitácora del agente. Debe visualizarse el proceso inverso:
  1. Comando recibido: `TOKEN`
  2. Detención ordenada de servicios para liberar archivos.
  3. Restauración quirúrgica de las subclaves y mappings del registro a partir de los backups de memoria creados previamente.
  4. Reinicio de `SCardSvr` y `CertPropSvc`.
  5. Cierre definitivo de `IDProtect Monitor.exe` para liberar locks sobre los lectores compatibles con JCOP.
- Haga clic en **"Refrescar"** en la web. El certificado de su USB Token debe listarse de manera exclusiva (mientras que los del DNIe se omitirán por completo).
- Estampe su firma y verifique la solicitud del PIN bajo el cuadro gráfico correspondiente al middleware de Bit4id sin excepciones.

### Paso 5: Prueba de Cierre y Restauración
- En el portal web de firma, navegue hacia otra página, vuelva a la bandeja o cierre la sesión.
- El evento de navegación disparará el protocolo de limpieza `zofrafirma://reset`.
- Verifique que el agente notifique en pantalla: **"✔ Middlewares restaurados"**, devolviendo el registro del sistema operativo Windows a su estado limpio e inicial para evitar conflictos en ejecuciones externas.
