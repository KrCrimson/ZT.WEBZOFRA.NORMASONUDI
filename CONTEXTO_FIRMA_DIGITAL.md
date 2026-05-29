# Contexto Técnico y Soluciones: Firma Digital (DNIe vs. USB Token Bit4id)

Este documento detalla el objetivo, la historia del conflicto técnico, las fases de solución implementadas hasta el momento, y la resolución definitiva para permitir la firma electrónica segura utilizando el **DNI electrónico peruano (DNIe)** y **Tokens USB de Firma** simultáneamente en la misma PC sin conflictos.

---

## 🎯 1. El Objetivo General
Permitir que los usuarios del sistema **ZT.WEBZOFRA.NORMASONUDI** firmen documentos en formato PDF desde el servidor web local utilizando cualquiera de los dos dispositivos de firma digital en la misma PC local de forma transparente:
1. **DNI electrónico (DNIe - RENIEC)**: Tarjeta inteligente tipo JCOP de NXP que utiliza el middleware **NXP IDProtect Client** (controlador principal: `ciamd.dll`).
2. **USB Token (Firma Estándar)**: Token criptográfico que requiere el middleware **Bit4id Universal Middleware** (controlador principal/mini-driver: `bit4xpki.dll`).

---

## ⚠️ 2. El Conflicto Histórico (Por qué fallaba)
Ambos dispositivos están construidos sobre chips físicamente compatibles con la arquitectura NXP (JCOP).
Cuando el software de **Bit4id** se instala en Windows, se adueña de la base de datos de tarjetas inteligentes de Windows (**Calais**), registrando el ATR de la tarjeta `"  NXP IAS C18"` (que es el ATR exacto de los chips del DNIe) apuntando a sus propios controladores (`bit4xpki.dll`).

### Consecuencias:
Cuando el usuario introducía el DNIe e intentaba firmar, Windows cargaba el driver de Bit4id en lugar del nativo de Microsoft/IDProtect (`ciamd.dll`). Esto causaba que la comunicación con el chip del DNIe fallara de inmediato, lanzando un cuadro de PIN erróneo de Bit4id o tirando la excepción:
> *"La tarjeta inteligente no puede realizar la operación solicitada o la operación requiere otra tarjeta inteligente."*

Si se desinstalaba Bit4id, el DNIe funcionaba perfectamente. Pero al hacerlo, se rompía la firma con el USB Token. Ambos controladores deben coexistir y funcionar dinámicamente.

---

## 🛠️ 3. El Camino de Soluciones Implementadas hasta el Momento

Hemos atacado el problema desde tres frentes técnicos sucesivos:

### 📁 Fase 1: Aislamiento Dinámico por Agente (`ZofraFirmaAgent.exe`)
Diseñamos un agente de fondo ligero en C# que corre con privilegios elevados de Administrador. Este se despierta por esquemas de URI personalizados (`zofrafirma://dnie` y `zofrafirma://token`) disparados automáticamente al cargar cada página de firma:
* **Modo DNIe (`zofrafirma://dnie`)**: El agente detiene el servicio de tarjetas inteligentes de Windows (`SCardSvr`), renombra la DLL intrusa `bit4xpki.dll` a `bit4xpki.dll.disabled` en `System32` y `SysWOW64`, modifica los mapeos en el Registro de Calais (`HKLM` y `HKCU`) para apuntar `"  NXP IAS C18"` al nativo `ciamd.dll`, y reinicia el servicio.
* **Modo Token (`zofrafirma://token`)**: Hace el proceso inverso, habilitando de nuevo la DLL de Bit4id y restaurando los registros.

*Resultado*: Aislamiento físico de drivers perfecto, pero surgió el segundo conflicto en la capa de criptografía de la aplicación web (.NET).

---

### 🔑 Fase 2: El Envoltorio CAPI a CNG y el error "Tarjeta no compatible"
Cuando el agente dejaba el camino libre a `ciamd.dll`, la firma con DNIe en el servidor fallaba con el mensaje:
> *`La tarjeta inteligente no es compatible con esta operación.`*

#### Diagnóstico:
La aplicación web utiliza .NET Framework 4.8. El certificado de firma DNIe está registrado en Windows bajo un proveedor de criptografía heredado (**CAPI**: `Microsoft Base Smart Card Crypto Provider`). 
Cuando el código de la aplicación intentaba acceder a la llave privada vía `.GetRSAPrivateKey()` o `.PrivateKey` para firmar en **SHA-256**, la biblioteca de .NET intentaba realizar un wrapping automático a la API moderna de **CNG**. Sin embargo, la comunicación a través del wrapper para este minidriver fallaba al negociar algoritmos de hash modernos (SHA-256), bloqueando la firma antes de permitir ingresar el PIN.

#### Solución en C#:
Reescribimos la firma criptográfica en caliente en [FirmaDigital.aspx.cs](file:///c:/Users/windows11/Desktop/ZT.WEBZOFRA.NORMASONUDI/ZT.WEBZOFRA.NORMASONUDI/Tramites/FirmaDigital.aspx.cs):
1. Usamos llamadas a la API de Windows de bajo nivel (`CertGetCertificateContextProperty`) para extraer silenciosamente el nombre del contenedor de la clave privada (ej. `{FIR_ECEP-Sig  }`) directamente en memoria, sin gatillar diálogos.
2. Bypasseamos el wrapper defectuoso abriendo la clave de forma explícita directamente en el proveedor moderno de Smart Cards de Windows:
   ```csharp
   CngKey.Open(containerName, new CngProvider("Microsoft Smart Card Key Storage Provider"))
   ```
3. Firmamos usando `RSACng` directamente, forzando la carga de `ciamd.dll` nativo sin errores de CAPI.

---

### 🧵 Fase 3: El Apartamento de Hilos (MTA vs. STA) en ASP.NET (¡Resuelto!)
Aunque la solución de la **Fase 2** funcionaba perfectamente en scripts de consola independientes (como PowerShell), al integrarlo a la aplicación web volvía a fallar arrojando el mismo error en el diálogo del PIN.

#### Diagnóstico:
Los servidores de aplicaciones web de ASP.NET (IIS Express / IIS) atienden las peticiones http a través de un pool de hilos con estado de apartamento **MTA (Multi-Threaded Apartment)** por defecto.
Sin embargo, las APIs de seguridad criptográfica de Windows que controlan los diálogos gráficos (como el cuadro de diálogo interactivo donde escribes tu PIN de DNIe) e interactúan con hardware COM **requieren estrictamente un contexto de apartamento STA (Single-Threaded Apartment)** para inicializarse de manera compatible. Al ejecutarse en MTA, el diálogo se corrompía internamente y arrojaba inmediatamente el error en letras rojas `"La tarjeta inteligente no es compatible con esta operación"`.

#### Solución Definitiva (¡Implementada!):
Hemos modificado el code-behind de ambas páginas de firma (**`FirmaDigital.aspx.cs`** y **`FirmaUsbToken.aspx.cs`**) para que la operación de firma digital de iTextSharp se ejecute aislada dentro de un hilo dedicado configurado explícitamente en **STA**:

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
thread.SetApartmentState(System.Threading.ApartmentState.STA);
thread.Start();
thread.Join();

if (threadEx != null)
{
    throw threadEx;
}
```

---

## 📈 Estado Actual del Proyecto
1. **Código Actualizado**: Tanto [FirmaDigital.aspx.cs](file:///c:/Users/windows11/Desktop/ZT.WEBZOFRA.NORMASONUDI/ZT.WEBZOFRA.NORMASONUDI/Tramites/FirmaDigital.aspx.cs) como [FirmaUsbToken.aspx.cs](file:///c:/Users/windows11/Desktop/ZT.WEBZOFRA.NORMASONUDI/ZT.WEBZOFRA.NORMASONUDI/Tramites/FirmaUsbToken.aspx.cs) cuentan ahora con el aislamiento de hilos STA y la apertura directa mediante el KSP moderno de Smart Cards de Microsoft.
2. **Compilación Correcta**: El proyecto fue compilado en su totalidad con **MSBuild (Roslyn Compiler)**, arrojando **0 errores** y generando la DLL actualizada `ZT.WEBZOFRA.NORMASONUDI.dll` exitosamente.
3. **Servidor Reiniciado**: Se detuvieron todos los procesos remanentes de `iisexpress.exe` para asegurar que el servidor recargue de forma limpia y forzada la nueva DLL criptográfica desde cero.
