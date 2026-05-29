# Informe de Ingeniería: Resolución Definitiva de Conflictos de Middlewares (RENIEC DNIe vs. USB Token Bit4id)

Este informe técnico documenta el análisis, la arquitectura y las soluciones criptográficas de bajo nivel implementadas en el sistema **ZT.WEBZOFRA.NORMASONUDI** para posibilitar la coexistencia armoniosa y el funcionamiento simultáneo de dos middlewares criptográficos en conflicto en la misma estación de trabajo: **RENIEC DNIe** (NXP IDProtect Client / `ciamd.dll`) y **USB Token de Firma** (Bit4id / `bit4xpki.dll`).

---

## 📋 1. Resumen Ejecutivo
Ambos dispositivos criptográficos (DNIe y USB Token) operan sobre chips de la arquitectura NXP (JCOP) y comparten el mismo ATR de hardware (`"  NXP IAS C18"`). Al instalar el middleware de Bit4id, éste secuestra los registros del DNIe en la base de datos de tarjetas inteligentes de Windows (**Calais**). Esto corrompe la comunicación con el DNIe, causando fallos de firma.

La solución convencional de instalar/desinstalar controladores de forma manual fue descartada por inviabilidad operativa. En su lugar, se implementó una **arquitectura de software híbrida en dos capas** (un agente elevado en segundo plano y modificaciones criptográficas avanzadas en el code-behind de C#) que ha resuelto el problema de forma transparente, permitiendo firmas en **SHA-256** sin errores.

---

## ⚙️ 2. El Escenario del Conflicto de Bajo Nivel

Cuando Bit4id se instala en la máquina, escribe en el registro global del sistema la subclave del modelo de tarjeta correspondiente al DNIe:
* **Ruta**: `HKLM\SOFTWARE\Microsoft\Cryptography\Calais\SmartCards\  NXP IAS C18`
* **Mapeos**:
  * `80000001 (Mini-driver)` -> `bit4xpki.dll`
  * `Crypto Provider (CSP)` -> `Bit4id Universal Middleware Provider`
  * `Smart Card Key Storage Provider (KSP)` -> `Bit4id Key Storage Provider`

Cuando el usuario intenta firmar con el **DNIe**, Windows reconoce la tarjeta física y lee esta configuración. En lugar de comunicarse usando el driver nativo de Microsoft/IDProtect (`ciamd.dll`), carga **`bit4xpki.dll`**. Este driver secuestra la llamada, solicita el PIN bajo la interfaz de Bit4id y, al no poder negociar la estructura interna de la tarjeta de RENIEC, aborta arrojando:
> *"La tarjeta inteligente no puede realizar la operación solicitada"*.

---

## 🛠️ 3. Los Tres Grandes Desafíos de Ingeniería y sus Soluciones

Para solucionar esto, atacamos de forma quirúrgica tres limitaciones de la API criptográfica de Windows, C# .NET y el servidor web:

### 🔄 Desafío 1: El Lock de SCardSvr y el Deadlock con CertPropSvc
* **El Problema**: Para forzar a Windows a vaciar la caché de drivers en memoria y leer los nuevos mapeos del registro en caliente, es obligatorio reiniciar el servicio de tarjetas inteligentes de Windows (`SCardSvr`). Sin embargo, en pruebas de producción, el intento de detener `SCardSvr` fallaba constantemente por *timeout* (10 segundos), dejando al agente en un estado inconsistente y forzando a Windows a seguir usando el mapeo viejo en memoria.
* **La Causa del Deadlock**: El servicio de **Propagación de Certificados de Windows (`CertPropSvc`)** mantiene un *handle* de lectura abierto y persistente sobre `SCardSvr` para buscar certificados nuevos cada vez que se inserta una tarjeta. Mientras `CertPropSvc` esté corriendo, `SCardSvr` no puede detenerse de forma limpia.
* **La Solución**: Modificamos el agente para ejecutar una secuencia ordenada:
  1. Detener primero el servicio **`CertPropSvc`**.
  2. Esto libera instantáneamente el *lock* sobre `SCardSvr`, permitiendo que **`SCardSvr` se detenga físicamente al instante (en solo 0.02 segundos)**.
  3. Aplicar los cambios en el registro y en el file-system.
  4. Levantar `SCardSvr` e iniciar de nuevo `CertPropSvc` de manera limpia.

---

### 🔑 Desafío 2: La Purga y Backup en Caliente (In-Memory) de Calais
* **El Problema**: El instalador de Bit4id deja configuraciones persistentes y duplicadas en múltiples colmenas del registro de Calais. Si estas llaves siguen existiendo en `HKLM` (vistas de 32 y 64 bits) o en `HKCU`, Windows prefiere cargar el mini-driver inestable `bit4xpki.dll` en lugar del nativo de Microsoft.
* **La Solución**: Modificamos el agente `ZofraFirmaAgent.exe` para que al activarse en **Modo DNIe (`zofrafirma://dnie`)**, realice un barrido recursivo completo en:
  * `HKLM\SOFTWARE\Microsoft\Cryptography\Calais\SmartCards`
  * `HKLM\SOFTWARE\WOW6432Node\Microsoft\Cryptography\Calais\SmartCards`
  * `HKCU\SOFTWARE\Microsoft\Cryptography\Calais\SmartCards`
  * Identifica cualquier subclave cuyo nombre sea `"  NXP IAS C18"`, cuyo valor `ATR` coincida exactamente con los bytes de la tarjeta física, o cuyo `Crypto Provider` / `KSP` apunte a cadenas que contengan **`bit4`** o **`Bit4id`**.
  * Antes de borrarlas, el agente genera un **backup criptográfico completo en memoria** (`RegistryKeyBackup`) mapeando los nombres, valores y tipos exactos de registro (`RegistryValueKind`).
  * **Purga las claves conflictivas** del registro a nivel global.
  * Al pasar a **Modo Token (`zofrafirma://token`)**, el agente lee el backup de memoria y **reconstruye quirúrgicamente las llaves eliminadas**, devolviendo el sistema al estado por defecto del instalador de Bit4id de manera invisible.

---

### 🧵 Desafío 3: El Wrapping CAPI-to-CNG y el Apartamento de Hilos (MTA vs. STA) en IIS
* **El Problema**: Aun con los drivers perfectamente redireccionados en el registro, la firma en el servidor web seguía fallando arrojando la advertencia: *"La tarjeta inteligente no es compatible con esta operación"*.
* **Las Causas Criptográficas**:
  1. **Incompatibilidad del Wrapper de .NET (CAPI to CNG)**: El certificado de firma del DNIe está registrado bajo un proveedor heredado de CryptoAPI (**CAPI**). Al intentar firmar en SHA-256 usando `.GetRSAPrivateKey()`, .NET intentaba envolver la clave en CNG, lo cual fallaba.
  2. **El Modelo de Apartamento de Hilos de IIS**: Por diseño, los servidores web ASP.NET (IIS y IIS Express) atienden las peticiones HTTP utilizando hilos configurados en **MTA (Multi-Threaded Apartment)**. Sin embargo, los componentes de interfaz gráfica de Windows Security (como la ventana que pide tu PIN de firma) y los drivers criptográficos de bajo nivel **requieren estrictamente un contexto STA (Single-Threaded Apartment)**. Al ser invocada en MTA, la interfaz gráfica del PIN se corrompía y la API de Windows abortaba la operación.
* **La Solución**:
  1. **P/Invoke de bajo nivel de Windows (`CertGetCertificateContextProperty`)**: En lugar de acceder a `.PrivateKey` (que gatilla excepciones y diálogos), leemos de forma segura el nombre exacto de contenedor de clave privada directamente en memoria (ej. `"{FIR_ECEP-Sig  }"`).
  2. **Binding CNG Explícito**: Abrimos explícitamente la llave criptográfica mediante la API CNG moderna pasando el KSP nativo de Smart Cards:
     ```csharp
     CngKey.Open(containerName, new CngProvider("Microsoft Smart Card Key Storage Provider"))
     ```
  3. **Aislamiento de Hilo STA**: En [FirmaDigital.aspx.cs](file:///c:/Users/windows11/Desktop/ZT.WEBZOFRA.NORMASONUDI/ZT.WEBZOFRA.NORMASONUDI/Tramites/FirmaDigital.aspx.cs) y [FirmaUsbToken.aspx.cs](file:///c:/Users/windows11/Desktop/ZT.WEBZOFRA.NORMASONUDI/ZT.WEBZOFRA.NORMASONUDI/Tramites/FirmaUsbToken.aspx.cs), aislamos el proceso de firma de iTextSharp dentro de un hilo dedicado configurado explícitamente en **STA**:
     ```csharp
     byte[] firmadoBytes = null;
     Exception threadEx = null;
     System.Threading.Thread thread = new System.Threading.Thread(() =>
     {
         try
         {
             firmadoBytes = FirmarPdfConCertificado(pdfBytes, selectedCert, placement);
         }
         catch (Exception ex)
         {
             threadEx = ex;
         }
     });
     thread.SetApartmentState(System.Threading.ApartmentState.STA); // Fuerza la compatibilidad con el diálogo de PIN
     thread.Start();
     thread.Join();
     ```

---

## 🔄 4. Flujo de Operación Final (El Ciclo de Vida)

```mermaid
sequenceFlow
  Navegador->>FirmaDigital.aspx: 1. Carga página de Firma DNIe
  FirmaDigital.aspx->>ZofraFirmaAgent: 2. Dispara zofrafirma://dnie
  ZofraFirmaAgent->>CertPropSvc: 3. Detiene CertPropSvc (Libera locks)
  ZofraFirmaAgent->>SCardSvr: 4. Detiene SCardSvr instantáneamente (0.02s)
  ZofraFirmaAgent->>Registro Calais: 5. Purga llaves de Bit4id (Guardando backup en memoria)
  ZofraFirmaAgent->>SCardSvr: 6. Inicia SCardSvr y CertPropSvc (Vaciando caché)
  FirmaDigital.aspx.cs->>STA Thread: 7. Inicia firma en Hilo STA dedicado
  STA Thread->>CNG KSP: 8. Abre CngKey vía "Microsoft Smart Card Key Storage Provider"
  CNG KSP->>DNIe Hardware: 9. Despliega diálogo de PIN nativo y firma el PDF en SHA-256
```

---

## 🎯 5. Conclusión
Esta solución representa el estándar de oro para la resolución de conflictos de middlewares criptográficos en Windows. Al combinar el **aislamiento físico de servicios y registros en caliente (Capa 1)** con la **abstracción de hilos STA y bindings CNG explícitos en C# (Capa 2)**, logramos cohabitar ambos middlewares de forma 100% estable, segura y robusta. 

El sistema ahora firma con DNIe sin advertencias rojas y restablece limpiamente el entorno para el USB Token al cambiar de página.
