# ============================================================
# Instalador de certificados RENIEC/ECERNEP para Windows
# Ejecutar como ADMINISTRADOR
# ============================================================
# Este script instala la cadena de confianza completa del DNIe
# (Documento Nacional de Identidad electrónico de RENIEC/Peru)
# en el almacén de certificados de Windows.
# ============================================================

param([switch]$Silent)

function Write-Step($msg) {
    Write-Host "[*] $msg" -ForegroundColor Cyan
}
function Write-OK($msg) {
    Write-Host "[OK] $msg" -ForegroundColor Green
}
function Write-ERR($msg) {
    Write-Host "[ERROR] $msg" -ForegroundColor Red
}

$ErrorActionPreference = "Stop"
$workDir = Split-Path -Parent $MyInvocation.MyCommand.Path

Write-Host ""
Write-Host "======================================================" -ForegroundColor Yellow
Write-Host "  Instalador de Cadena PKI RENIEC / ECERNEP Peru" -ForegroundColor Yellow
Write-Host "======================================================" -ForegroundColor Yellow
Write-Host ""

# ---- Verificar que es Admin ----
$isAdmin = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $isAdmin) {
    Write-ERR "Este script debe ejecutarse como ADMINISTRADOR."
    Write-ERR "Clic derecho en el archivo > Ejecutar con PowerShell > Si al UAC"
    if (-not $Silent) { Read-Host "Presione Enter para cerrar" }
    exit 1
}

# ---- Definir certificados a instalar ----
$certs = @(
    @{
        Name    = "ECEP-RENIEC CA Class 2 II"
        Url     = "http://crl.reniec.gob.pe/crt/sha2/caclass2ii.crt"
        Store   = "CA"    # Intermediate CAs
        File    = "$workDir\caclass2ii.crt"
    },
    @{
        Name    = "ECEP-RENIEC"
        Url     = "http://www.reniec.gob.pe/crt/sha2/ecep.crt"
        Store   = "CA"    # Intermediate CAs
        File    = "$workDir\ecep.crt"
    }
    # NOTA: ECERNEP PERU CA ROOT 3 debe descargarse manualmente desde:
    # https://www.reniec.gob.pe/portal/html/identidad-digital
    # y luego instalar con: certutil -addstore -f "Root" ECERNEP-PERU-CA-ROOT-3.crt
)

$rootCertFile = "$workDir\ECERNEP-ROOT-3.crt"

# ---- Instalar certificados intermedios ----
foreach ($cert in $certs) {
    Write-Step "Procesando: $($cert.Name)"
    
    # Descargar si no existe
    if (-not (Test-Path $cert.File)) {
        Write-Step "  Descargando desde $($cert.Url)..."
        try {
            $r = Invoke-WebRequest $cert.Url -TimeoutSec 15 -UseBasicParsing -ErrorAction Stop
            [System.IO.File]::WriteAllBytes($cert.File, $r.Content)
            Write-OK "  Descargado ($($r.Content.Length) bytes)"
        } catch {
            Write-ERR "  No se pudo descargar: $_"
            continue
        }
    } else {
        Write-OK "  Ya existe localmente: $($cert.File)"
    }
    
    # Instalar en almacén de Windows
    Write-Step "  Instalando en almacén '$($cert.Store)'..."
    $result = certutil -addstore -f $cert.Store $cert.File 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-OK "  Instalado correctamente en '$($cert.Store)'"
    } else {
        # Puede que ya esté instalado
        if ($result -match "ya existe" -or $result -match "already exists") {
            Write-OK "  Ya estaba instalado."
        } else {
            Write-ERR "  Error al instalar: $result"
        }
    }
    Write-Host ""
}

# ---- Instalar ROOT 3 si existe localmente ----
Write-Host "======================================================" -ForegroundColor Yellow
Write-Host "  Certificado Raiz: ECERNEP PERU CA ROOT 3" -ForegroundColor Yellow  
Write-Host "======================================================" -ForegroundColor Yellow

if (Test-Path $rootCertFile) {
    Write-Step "Instalando ROOT 3 en almacén 'Root' (Entidades de certificación raíz de confianza)..."
    $result = certutil -addstore -f "Root" $rootCertFile 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-OK "ROOT 3 instalado. Puede aparecer un dialogo de confirmacion de seguridad."
    } else {
        Write-ERR "Error al instalar ROOT 3: $result"
    }
} else {
    Write-Host ""
    Write-Host "  [ACCION REQUERIDA]" -ForegroundColor Red
    Write-Host "  El certificado ECERNEP PERU CA ROOT 3 no se pudo descargar automaticamente." -ForegroundColor Yellow
    Write-Host "  Pasos manuales:" -ForegroundColor Yellow
    Write-Host "  1. Abrir el navegador y buscar 'RENIEC certificado raiz ECERNEP'" -ForegroundColor White
    Write-Host "  2. Descargar el archivo ECERNEP-PERU-CA-ROOT-3.crt desde reniec.gob.pe" -ForegroundColor White
    Write-Host "  3. Colocarlo en esta carpeta: $workDir\" -ForegroundColor White
    Write-Host "  4. Ejecutar de nuevo este script como Administrador" -ForegroundColor White
    Write-Host ""
    Write-Host "  O instalar manualmente:" -ForegroundColor Yellow
    Write-Host "  1. Abrir certmgr.msc (Administrador de certificados)" -ForegroundColor White
    Write-Host "  2. Ir a: Equipo local > Entidades de certificacion raiz de confianza" -ForegroundColor White
    Write-Host "  3. Clic derecho > Importar > seleccionar ECERNEP-PERU-CA-ROOT-3.crt" -ForegroundColor White
    Write-Host ""
}

# ---- Verificar cadena ----
Write-Host "======================================================" -ForegroundColor Yellow
Write-Host "  Verificando estado de la cadena..." -ForegroundColor Yellow
Write-Host "======================================================" -ForegroundColor Yellow

$scResult = certutil -scinfo 2>&1 | Out-String
if ($scResult -match "Prueba de coincidencia de clave p.blica correcta") {
    Write-OK "Tarjeta detectada y clave publica verificada."
} else {
    Write-Host "  No se pudo verificar la tarjeta (asegurese de que el DNIe este insertado)." -ForegroundColor Yellow
}

if ($scResult -match "CERT_E_CHAINING" -or $scResult -match "cadena.*no es v.lida") {
    if (Test-Path $rootCertFile) {
        Write-ERR "La cadena de certificados aun falla. Revise el archivo ECERNEP-ROOT-3.crt."
    } else {
        Write-Host "  [PENDIENTE] La cadena de certificados estara completa al instalar el ROOT 3." -ForegroundColor Yellow
    }
} elseif ($scResult -match "Prueba de coincidencia de clave p.blica correcta") {
    Write-OK "La cadena de certificados esta completa. El DNIe deberia funcionar para firma digital."
}

Write-Host ""
Write-Host "======================================================" -ForegroundColor Green
Write-Host "  Proceso completado." -ForegroundColor Green
Write-Host "======================================================" -ForegroundColor Green

if (-not $Silent) { Read-Host "Presione Enter para cerrar" }
