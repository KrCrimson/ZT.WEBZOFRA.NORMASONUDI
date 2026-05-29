using System;
using System.IO;
using System.IO.Pipes;
using System.ServiceProcess;
using System.Threading;
using System.Windows.Forms;
using System.Security.Principal;
using Microsoft.Win32;

namespace ZofraFirmaAgent
{
    // ============================================================
    // Modos de operación del agente
    // ============================================================
    internal enum SmartCardMode { DNIe, Token }

    // ============================================================
    // CLAVE CALAIS: la entrada del registro que Windows usa para
    // resolver qué mini-driver cargar según el ATR de la tarjeta.
    // Bit4id registró aquí la tarjeta NXP IAS C18 (mismo chip que
    // usa el DNIe de RENIEC), causando el conflicto de middlewares.
    // ============================================================
    internal static class CalaisKey
    {
        // Nombre de la clave tal como la registró Bit4id (con 2 espacios al inicio)
        public const string CardName = "  NXP IAS C18";

        // Valores para modo DNIe → mini-driver nativo de Microsoft / IDProtect
        public const string DnieMiniDriver = "ciamd.dll";
        public const string DnieCsp        = "Microsoft Base Smart Card Crypto Provider";
        public const string DnieKsp        = "Microsoft Smart Card Key Storage Provider";

        // Valores para modo USB Token → mini-driver de Bit4id
        public const string TokenMiniDriver = "bit4xpki.dll";
        public const string TokenCsp        = "Bit4id Universal Middleware Provider";
        public const string TokenKsp        = "Bit4id Key Storage Provider";

        public static string HklmPath => $@"SOFTWARE\Microsoft\Cryptography\Calais\SmartCards\{CardName}";
        public static string HkcuPath => $@"Software\Microsoft\Cryptography\Calais\SmartCards\{CardName}";
    }

    // ============================================================
    // Punto de entrada del programa
    // ============================================================
    internal static class Program
    {
        private static Mutex _mutex;
        private const string MutexName = @"Global\ZofraFirmaAgentMutex";
        private const string PipeName  = "ZofraFirmaAgentPipe";

        [STAThread]
        private static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // ── 1. Si no es Admin, intentar auto-elevarse con UAC ──────────
            if (!IsAdministrator())
            {
                try
                {
                    var psi = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName       = Application.ExecutablePath,
                        Arguments      = string.Join(" ", args),
                        Verb           = "runas",
                        UseShellExecute = true
                    };
                    System.Diagnostics.Process.Start(psi);
                }
                catch
                {
                    MessageBox.Show(
                        "ZofraFirmaAgent necesita ejecutarse como Administrador para gestionar los middlewares de firma digital.\n\nPor favor, acepte el diálogo de Control de Cuentas (UAC).",
                        "ZofraFirmaAgent — Requiere Administrador",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }
                return;
            }

            // ── 2. Matar instancias anteriores (para reemplazar versiones viejas) ──
            try
            {
                int myPid = System.Diagnostics.Process.GetCurrentProcess().Id;
                foreach (var p in System.Diagnostics.Process.GetProcessesByName("ZofraFirmaAgent"))
                {
                    if (p.Id != myPid)
                    {
                        p.Kill();
                        p.WaitForExit(2000);
                    }
                }
            }
            catch { }

            // ── 3. Mutex singleton ────────────────────────────────────────────
            _mutex = new Mutex(true, MutexName, out bool createdNew);
            if (!createdNew)
            {
                if (args.Length > 0) SendPipeMessage(args[0]);
                return;
            }

            // ── 4. Registrar el protocolo URI personalizado zofrafirma:// ─────
            RegistrarProtocolo();

            // ── 5. Crear contexto de bandeja y arrancar ───────────────────────
            var context = new ZofraAgentContext();

            if (args.Length > 0)
                context.ProcessCommand(args[0]);

            Application.Run(context);
            _mutex.ReleaseMutex();
        }

        private static bool IsAdministrator()
        {
            try
            {
                using (var id = WindowsIdentity.GetCurrent())
                    return new WindowsPrincipal(id).IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch { return false; }
        }

        private static void SendPipeMessage(string message)
        {
            try
            {
                using var pipe = new NamedPipeClientStream(".", PipeName, PipeDirection.Out);
                pipe.Connect(1000);
                using var writer = new StreamWriter(pipe);
                writer.WriteLine(message);
                writer.Flush();
            }
            catch { }
        }

        private static void RegistrarProtocolo()
        {
            try
            {
                string exe = Application.ExecutablePath;
                using var key = Registry.CurrentUser.CreateSubKey(@"Software\Classes\zofrafirma");
                key.SetValue("", "URL:ZofraFirma Protocol");
                key.SetValue("URL Protocol", "");
                using var cmd = key.CreateSubKey(@"shell\open\command");
                cmd.SetValue("", $"\"{exe}\" \"%1\"");
            }
            catch { }
        }
    }

    // ============================================================
    // Contexto principal del agente (bandeja del sistema)
    // ============================================================
    public class ZofraAgentContext : ApplicationContext
    {
        private readonly NotifyIcon _tray;
        private const string PipeName = "ZofraFirmaAgentPipe";

        // Guarda el modo actual para no repetir operaciones innecesarias
        private SmartCardMode? _currentMode = null;

        public ZofraAgentContext()
        {
            _tray = new NotifyIcon
            {
                Icon    = System.Drawing.SystemIcons.Shield,
                Text    = "Zofra Firma Agent",
                Visible = true
            };

            var menu = new ContextMenu();
            menu.MenuItems.Add("Restaurar Middlewares", (s, e) => ProcessCommand("zofrafirma://reset"));
            menu.MenuItems.Add("Ver Log",               (s, e) => AbrirLog());
            menu.MenuItems.Add("-");
            menu.MenuItems.Add("Salir",                 (s, e) => Exit());
            _tray.ContextMenu = menu;

            StartPipeServer();
        }

        // ── Servidor de Named Pipe (escucha comandos del navegador) ──────────
        private void StartPipeServer()
        {
            var t = new Thread(() =>
            {
                while (true)
                {
                    try
                    {
                        using var pipe = new NamedPipeServerStream(PipeName, PipeDirection.In);
                        pipe.WaitForConnection();
                        using var reader = new StreamReader(pipe);
                        string req = reader.ReadLine();
                        if (!string.IsNullOrEmpty(req))
                        {
                            if (SynchronizationContext.Current != null)
                                SynchronizationContext.Current.Post(_ => ProcessCommand(req), null);
                            else
                                ProcessCommand(req);
                        }
                    }
                    catch { Thread.Sleep(1000); }
                }
            }) { IsBackground = true };
            t.Start();
        }

        // ============================================================
        // COMANDO PRINCIPAL — router de modos
        // ============================================================
        public void ProcessCommand(string command)
        {
            if (string.IsNullOrEmpty(command)) return;
            string cmd = command.ToLowerInvariant().Trim();

            if (cmd.Contains("dnie"))
            {
                Log(">>> Comando recibido: DNIE");
                ActivarModo(SmartCardMode.DNIe);
                KillIDProtectMonitor();
                StartIDProtectMonitor();
                _tray.ShowBalloonTip(3000, "ZofraFirma Agent", "✔ Modo DNIe activado (IDProtect)", ToolTipIcon.Info);
            }
            else if (cmd.Contains("token"))
            {
                Log(">>> Comando recibido: TOKEN");
                ActivarModo(SmartCardMode.Token);
                KillIDProtectMonitor();
                _tray.ShowBalloonTip(3000, "ZofraFirma Agent", "✔ Modo USB Token activado (Bit4id)", ToolTipIcon.Info);
            }
            else if (cmd.Contains("reset"))
            {
                Log(">>> Comando recibido: RESET");
                ActivarModo(SmartCardMode.Token); // reset = volver a Bit4id
                StartIDProtectMonitor();
                _tray.ShowBalloonTip(3000, "ZofraFirma Agent", "✔ Middlewares restaurados", ToolTipIcon.Info);
            }
        }

        // ============================================================
        // ESTRATEGIA DUAL: HKCU → HKLM fallback
        //
        //  1. Intenta escribir el override en HKCU (sin detener ningún
        //     servicio, sin permisos extra, cambio instantáneo).
        //  2. Si falla o HKCU no produce efecto, escribe en HKLM y
        //     reinicia SCardSvr con espera activa (modo robusto).
        // ============================================================
        private void ActivarModo(SmartCardMode modo)
        {
            if (_currentMode == modo)
            {
                Log($"Modo {modo} ya está activo, no se repite la operación.");
                return;
            }

            Log($"Activando modo: {modo}");

            bool dnie = (modo == SmartCardMode.DNIe);

            if (dnie)
            {
                // Limpiar claves conflictivas de Bit4id
                LimpiarClavesRegistroBit4id();
            }
            else
            {
                // Restaurar claves desde backup
                RestaurarClavesRegistroBit4id();
            }

            // ── Estrategia 1: HKCU (sin Admin, sin reinicio de servicio) ────
            bool hkcuOk = EscribirCalaisHkcu(dnie);
            Log($"Estrategia HKCU: {(hkcuOk ? "OK" : "FALLÓ o no soportada")}");

            // ── Estrategia 2: HKLM (Admin requerido, con reinicio robusto) ──
            // Siempre la ejecutamos además de HKCU para garantizar consistencia.
            EscribirCalaisHklm(dnie);

            _currentMode = modo;
        }

        // ── Escritura en HKCU (colmena del usuario actual) ──────────────────
        // No requiere Admin. Si Windows 11 respeta HKCU para SmartCards,
        // el cambio es instantáneo sin reiniciar SCardSvr.
        private bool EscribirCalaisHkcu(bool dnie)
        {
            try
            {
                using var key = Registry.CurrentUser.CreateSubKey(CalaisKey.HkcuPath, true);
                if (key == null) { Log("HKCU: No se pudo crear la clave."); return false; }

                if (dnie)
                {
                    key.SetValue("80000001",                        CalaisKey.DnieMiniDriver, RegistryValueKind.String);
                    key.SetValue("Crypto Provider",                 CalaisKey.DnieCsp,        RegistryValueKind.String);
                    key.SetValue("Smart Card Key Storage Provider", CalaisKey.DnieKsp,        RegistryValueKind.String);
                    Log("HKCU: Escrito modo DNIe → ciamd.dll");
                }
                else
                {
                    // Para modo Token simplemente borramos el override y dejamos
                    // que HKLM (donde Bit4id instaló su configuración) domine.
                    Registry.CurrentUser.DeleteSubKeyTree(CalaisKey.HkcuPath, false);
                    Log("HKCU: Override eliminado → HKLM de Bit4id dominará.");
                }
                return true;
            }
            catch (Exception ex)
            {
                Log($"HKCU ERROR: {ex.Message}");
                return false;
            }
        }

        // ── Escritura en HKLM (colmena global del sistema) ──────────────────
        // Requiere Admin. Reinicia SCardSvr con espera activa para forzar
        // que Calais recargue el caché del registro sin race-condition.
        private void EscribirCalaisHklm(bool dnie)
        {
            try
            {
                // 1. Parar SCardSvr con espera real (no sleep fijo)
                ReiniciarSCardSvr(detenerSolo: true);

                // 2. Escribir en ambas vistas (64 y 32 bits)
                foreach (var view in new[] { RegistryView.Registry64, RegistryView.Registry32 })
                {
                    using var hklm  = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view);
                    using var clave = hklm.OpenSubKey(CalaisKey.HklmPath, writable: true);
                    if (clave == null) { Log($"HKLM [{view}]: Clave no encontrada."); continue; }

                    if (dnie)
                    {
                        clave.SetValue("80000001",                        CalaisKey.DnieMiniDriver, RegistryValueKind.String);
                        clave.SetValue("Crypto Provider",                 CalaisKey.DnieCsp,        RegistryValueKind.String);
                        clave.SetValue("Smart Card Key Storage Provider", CalaisKey.DnieKsp,        RegistryValueKind.String);
                    }
                    else
                    {
                        clave.SetValue("80000001",                        CalaisKey.TokenMiniDriver, RegistryValueKind.String);
                        clave.SetValue("Crypto Provider",                 CalaisKey.TokenCsp,        RegistryValueKind.String);
                        clave.SetValue("Smart Card Key Storage Provider", CalaisKey.TokenKsp,        RegistryValueKind.String);
                    }
                    Log($"HKLM [{view}]: Escrito modo {(dnie ? "DNIe" : "Token")} correctamente.");
                }

                // 3. Reiniciar SCardSvr con espera activa hasta Running
                ReiniciarSCardSvr(detenerSolo: false);

                // 4. Warm-up: pequeña pausa para que Calais lea el registro actualizado
                Thread.Sleep(300);
            }
            catch (Exception ex)
            {
                Log($"HKLM ERROR: {ex.Message}");
            }
        }

        // ── Reinicio robusto de SCardSvr con espera activa ──────────────────
        // Claude señaló que usar un sleep fijo (500ms) es una race condition.
        // Usamos ServiceController.WaitForStatus con timeout real en su lugar.
        private static void ReiniciarSCardSvr(bool detenerSolo)
        {
            const int timeoutMs = 10000;
            try
            {
                using var sc = new ServiceController("SCardSvr");

                if (sc.Status == ServiceControllerStatus.Running)
                {
                    Log("SCardSvr: Deteniendo...");
                    sc.Stop();
                    sc.WaitForStatus(ServiceControllerStatus.Stopped,
                                     TimeSpan.FromMilliseconds(timeoutMs));
                    Log("SCardSvr: Detenido.");
                }

                if (detenerSolo) return;

                Log("SCardSvr: Iniciando...");
                sc.Start();
                sc.WaitForStatus(ServiceControllerStatus.Running,
                                 TimeSpan.FromMilliseconds(timeoutMs));
                Log("SCardSvr: En ejecución.");
            }
            catch (Exception ex)
            {
                Log($"SCardSvr ERROR: {ex.Message}");
            }
        }

        // ── Módulos de Limpieza y Backup de Registro Calais ─────────────────
        private class RegistryValueBackup
        {
            public string Name { get; set; }
            public object Value { get; set; }
            public RegistryValueKind Kind { get; set; }
        }

        private class RegistryKeyBackup
        {
            public RegistryHive Hive { get; set; }
            public RegistryView View { get; set; }
            public string BasePath { get; set; }
            public string SubKeyName { get; set; }
            public System.Collections.Generic.List<RegistryValueBackup> Values { get; set; } = new System.Collections.Generic.List<RegistryValueBackup>();
        }

        private static readonly System.Collections.Generic.List<RegistryKeyBackup> _backups = new System.Collections.Generic.List<RegistryKeyBackup>();

        private static readonly byte[] DnieAtrBytes = new byte[] 
        { 
            0x3B, 0xDC, 0x18, 0xFF, 0x81, 0x91, 0xFE, 0x1F, 0xC3, 0x80, 0x73, 0xC8, 0x21, 0x13, 0x66, 0x05, 0x03, 0x63, 0x51, 0x00, 0x02, 0x50 
        };

        private static bool CompareBytes(byte[] a, byte[] b)
        {
            if (a == null || b == null) return false;
            if (a.Length != b.Length) return false;
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i]) return false;
            }
            return true;
        }

        private void LimpiarClavesRegistroBit4id()
        {
            Log("Limpiando claves de registro de Bit4id que causan conflicto...");
            
            // Detener CertPropSvc primero para liberar SCardSvr instantáneamente sin timeouts
            try
            {
                using var scCert = new ServiceController("CertPropSvc");
                if (scCert.Status == ServiceControllerStatus.Running)
                {
                    Log("CertPropSvc: Deteniendo...");
                    scCert.Stop();
                    scCert.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(5));
                    Log("CertPropSvc: Detenido.");
                }
            }
            catch (Exception ex) { Log($"CertPropSvc stop error: {ex.Message}"); }

            ReiniciarSCardSvr(detenerSolo: true);

            var targets = new[]
            {
                new { Hive = RegistryHive.LocalMachine, BasePath = @"SOFTWARE\Microsoft\Cryptography\Calais\SmartCards", View = RegistryView.Registry64 },
                new { Hive = RegistryHive.LocalMachine, BasePath = @"SOFTWARE\Microsoft\Cryptography\Calais\SmartCards", View = RegistryView.Registry32 },
                new { Hive = RegistryHive.LocalMachine, BasePath = @"SOFTWARE\WOW6432Node\Microsoft\Cryptography\Calais\SmartCards", View = RegistryView.Registry64 },
                new { Hive = RegistryHive.LocalMachine, BasePath = @"SOFTWARE\WOW6432Node\Microsoft\Cryptography\Calais\SmartCards", View = RegistryView.Registry32 },
                new { Hive = RegistryHive.CurrentUser, BasePath = @"Software\Microsoft\Cryptography\Calais\SmartCards", View = RegistryView.Default }
            };

            foreach (var target in targets)
            {
                try
                {
                    using var baseKey = RegistryKey.OpenBaseKey(target.Hive, target.View);
                    using var smartCardsKey = baseKey.OpenSubKey(target.BasePath, writable: true);
                    if (smartCardsKey == null) continue;

                    string[] subKeys = smartCardsKey.GetSubKeyNames();
                    foreach (string subKeyName in subKeys)
                    {
                        bool matches = false;
                        using (var subKey = smartCardsKey.OpenSubKey(subKeyName, writable: false))
                        {
                            if (subKey == null) continue;

                            // Criterio 1: Nombre es "  NXP IAS C18"
                            if (subKeyName.Trim().Equals("NXP IAS C18", StringComparison.OrdinalIgnoreCase))
                            {
                                matches = true;
                            }

                            // Criterio 2: ATR coincide
                            if (!matches && subKey.GetValue("ATR") is byte[] atrBytes)
                            {
                                if (CompareBytes(atrBytes, DnieAtrBytes))
                                {
                                    matches = true;
                                }
                            }

                            // Criterio 3: Cualquier valor de cadena contiene "bit4"
                            if (!matches)
                            {
                                foreach (string valueName in subKey.GetValueNames())
                                {
                                    if (subKey.GetValueKind(valueName) == RegistryValueKind.String)
                                    {
                                        string valStr = subKey.GetValue(valueName) as string;
                                        if (!string.IsNullOrEmpty(valStr) && valStr.IndexOf("bit4", StringComparison.OrdinalIgnoreCase) >= 0)
                                        {
                                            matches = true;
                                            break;
                                        }
                                    }
                                }
                            }
                        }

                        if (matches)
                        {
                            // Realizar backup antes de eliminar
                            RealizarBackupClave(target.Hive, target.View, target.BasePath, subKeyName, smartCardsKey);
                            
                            // Eliminar la clave conflictiva
                            smartCardsKey.DeleteSubKeyTree(subKeyName, false);
                            Log($"Clave eliminada con exito: {target.Hive}\\{target.BasePath}\\{subKeyName} (Vista: {target.View})");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log($"Error al limpiar Calais en {target.Hive}\\{target.BasePath}: {ex.Message}");
                }
            }

            ReiniciarSCardSvr(detenerSolo: false);
            try
            {
                using var scCert = new ServiceController("CertPropSvc");
                scCert.Start();
                scCert.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(5));
                Log("CertPropSvc: En ejecución.");
            }
            catch (Exception ex) { Log($"CertPropSvc start error: {ex.Message}"); }
        }

        private void RealizarBackupClave(RegistryHive hive, RegistryView view, string basePath, string subKeyName, RegistryKey smartCardsKey)
        {
            try
            {
                // Evitar duplicar backups
                foreach (var b in _backups)
                {
                    if (b.Hive == hive && b.View == view && b.BasePath == basePath && b.SubKeyName.Equals(subKeyName, StringComparison.OrdinalIgnoreCase))
                    {
                        return;
                    }
                }

                using var subKey = smartCardsKey.OpenSubKey(subKeyName, false);
                if (subKey == null) return;

                var backup = new RegistryKeyBackup
                {
                    Hive = hive,
                    View = view,
                    BasePath = basePath,
                    SubKeyName = subKeyName
                };

                foreach (string valName in subKey.GetValueNames())
                {
                    backup.Values.Add(new RegistryValueBackup
                    {
                        Name = valName,
                        Value = subKey.GetValue(valName),
                        Kind = subKey.GetValueKind(valName)
                    });
                }

                _backups.Add(backup);
                Log($"Backup realizado para: {hive}\\{basePath}\\{subKeyName}");
            }
            catch (Exception ex)
            {
                Log($"Error al hacer backup de {hive}\\{basePath}\\{subKeyName}: {ex.Message}");
            }
        }

        private void RestaurarClavesRegistroBit4id()
        {
            if (_backups.Count == 0)
            {
                Log("No hay backups de claves de registro Bit4id para restaurar.");
                return;
            }

            Log($"Restaurando {_backups.Count} claves de registro de Bit4id...");

            try
            {
                using var scCert = new ServiceController("CertPropSvc");
                if (scCert.Status == ServiceControllerStatus.Running)
                {
                    scCert.Stop();
                    scCert.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(5));
                    Log("CertPropSvc: Detenido.");
                }
            }
            catch (Exception ex) { Log($"CertPropSvc stop error: {ex.Message}"); }

            ReiniciarSCardSvr(detenerSolo: true);

            foreach (var backup in _backups)
            {
                try
                {
                    using var baseKey = RegistryKey.OpenBaseKey(backup.Hive, backup.View);
                    using var smartCardsKey = baseKey.CreateSubKey(backup.BasePath, true);
                    if (smartCardsKey == null) continue;

                    using var subKey = smartCardsKey.CreateSubKey(backup.SubKeyName, true);
                    if (subKey == null) continue;

                    foreach (var val in backup.Values)
                    {
                        subKey.SetValue(val.Name, val.Value, val.Kind);
                    }
                    Log($"Clave restaurada con exito: {backup.Hive}\\{backup.BasePath}\\{backup.SubKeyName}");
                }
                catch (Exception ex)
                {
                    Log($"Error al restaurar clave {backup.Hive}\\{backup.BasePath}\\{backup.SubKeyName}: {ex.Message}");
                }
            }

            _backups.Clear();

            ReiniciarSCardSvr(detenerSolo: false);
            try
            {
                using var scCert = new ServiceController("CertPropSvc");
                scCert.Start();
                scCert.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(5));
                Log("CertPropSvc: En ejecución.");
            }
            catch (Exception ex) { Log($"CertPropSvc start error: {ex.Message}"); }
        }

        // ── IDProtect Monitor ────────────────────────────────────────────────
        private static void KillIDProtectMonitor()
        {
            try
            {
                var psi = new System.Diagnostics.ProcessStartInfo("taskkill.exe", "/F /IM \"IDProtect Monitor.exe\"")
                {
                    CreateNoWindow = true, UseShellExecute = false
                };
                using var p = System.Diagnostics.Process.Start(psi);
                p?.WaitForExit(3000);
            }
            catch { }
        }

        private static void StartIDProtectMonitor()
        {
            try
            {
                string path = @"C:\Program Files (x86)\NXP Semiconductors\IDProtect Client\Utils\IDProtect Monitor.exe";
                if (File.Exists(path))
                    System.Diagnostics.Process.Start(path);
            }
            catch { }
        }

        // ── Logging ──────────────────────────────────────────────────────────
        private static void Log(string msg)
        {
            try
            {
                string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "agent_log.txt");
                File.AppendAllText(logPath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {msg}\r\n");
            }
            catch { }
        }

        private static void AbrirLog()
        {
            try
            {
                string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "agent_log.txt");
                if (File.Exists(logPath))
                    System.Diagnostics.Process.Start("notepad.exe", logPath);
            }
            catch { }
        }

        // ── Salida limpia ────────────────────────────────────────────────────
        private void Exit()
        {
            // Restaurar modo Token (estado por defecto) antes de salir
            try { EscribirCalaisHkcu(false); } catch { }
            _tray.Visible = false;
            Application.Exit();
        }
    }
}
