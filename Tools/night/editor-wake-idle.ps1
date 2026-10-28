# NIGHT24: wake an unfocused, stalled Unity Editor (memory: unity-editor-unfocused-autotick). Needs the machine idle ~45 s.
# AttachThreadInput to the foreground window's thread, BringWindowToTop + SetForegroundWindow on the Editor, then the
# caller sends `unity command set_autotick --enable true --interval_ms 16` and `editor_focus`.
Add-Type @'
using System; using System.Runtime.InteropServices;
public static class WakeN24 {
  [DllImport("user32.dll")] public static extern IntPtr GetForegroundWindow();
  [DllImport("user32.dll")] public static extern uint GetWindowThreadProcessId(IntPtr h, out uint pid);
  [DllImport("user32.dll")] public static extern bool AttachThreadInput(uint a, uint b, bool attach);
  [DllImport("user32.dll")] public static extern bool BringWindowToTop(IntPtr h);
  [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr h);
  [DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr h, int cmd);
  [DllImport("kernel32.dll")] public static extern uint GetCurrentThreadId();
  [StructLayout(LayoutKind.Sequential)] public struct L { public uint cb; public uint t; }
  [DllImport("user32.dll")] static extern bool GetLastInputInfo(ref L l);
  public static uint IdleSec() { var l = new L(); l.cb = (uint)Marshal.SizeOf(l); GetLastInputInfo(ref l); return ((uint)Environment.TickCount - l.t) / 1000; }
}
'@
$p = Get-Process Unity -ErrorAction SilentlyContinue | Where-Object { $_.MainWindowTitle -like 'SKYGRIND-unity*' } | Select-Object -First 1
if (-not $p) { Write-Output 'no Unity window'; exit 1 }
Write-Output ("idle " + [WakeN24]::IdleSec() + " s")
$fg = [WakeN24]::GetForegroundWindow()
$fgPid = 0
$fgThread = [WakeN24]::GetWindowThreadProcessId($fg, [ref]$fgPid)
$me = [WakeN24]::GetCurrentThreadId()
[WakeN24]::AttachThreadInput($me, $fgThread, $true) | Out-Null
[WakeN24]::ShowWindow($p.MainWindowHandle, 9) | Out-Null
[WakeN24]::BringWindowToTop($p.MainWindowHandle) | Out-Null
$ok = [WakeN24]::SetForegroundWindow($p.MainWindowHandle)
Write-Output "SetForegroundWindow: $ok"
Start-Sleep -Milliseconds 800
[WakeN24]::AttachThreadInput($me, $fgThread, $false) | Out-Null
