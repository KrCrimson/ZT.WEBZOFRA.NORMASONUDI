# ZofraFirmaAgent - Script de actualizacion
# Ejecutar como Administrador

Write-Host "=== ZofraFirmaAgent Actualizador ===" -ForegroundColor Cyan

# 1. Matar instancias anteriores
Write-Host "[1/3] Cerrando agente actual..." -ForegroundColor Yellow
Get-Process -Name ZofraFirmaAgent -ErrorAction SilentlyContinue | Stop-Process -Force
Start-Sleep -Milliseconds 1000

# 2. Verificar que no hay instancias activas
$procs = Get-Process -Name ZofraFirmaAgent -ErrorAction SilentlyContinue
if ($procs) {
    Write-Host "AVISO: Aun hay instancias corriendo (PIDs: $($procs.Id -join ', ')). Esperando..." -ForegroundColor Red
    Start-Sleep -Seconds 2
} else {
    Write-Host "Agente cerrado correctamente." -ForegroundColor Green
}

# 3. Lanzar nuevo agente
$exePath = "C:\Users\windows11\Desktop\ZT.WEBZOFRA.NORMASONUDI\ZofraFirmaAgent\bin\Release\net48\ZofraFirmaAgent.exe"
if (Test-Path $exePath) {
    Write-Host "[2/3] Lanzando nueva version del agente..." -ForegroundColor Yellow
    Start-Process $exePath -Verb runas
    Start-Sleep -Milliseconds 1500
    
    $newProcs = Get-Process -Name ZofraFirmaAgent -ErrorAction SilentlyContinue
    if ($newProcs) {
        Write-Host "[3/3] Agente iniciado correctamente (PID: $($newProcs.Id -join ', '))" -ForegroundColor Green
    } else {
        Write-Host "[3/3] El agente podria estar iniciando... verifique la bandeja del sistema." -ForegroundColor Yellow
    }
} else {
    Write-Host "ERROR: No se encontro el ejecutable en: $exePath" -ForegroundColor Red
    Write-Host "Asegurese de haber compilado el proyecto primero con 'dotnet build'." -ForegroundColor Red
}

Write-Host ""
Write-Host "=== Listo. El agente deberia aparecer en la bandeja del sistema (icono de escudo). ===" -ForegroundColor Cyan
Read-Host "Presione Enter para cerrar"
