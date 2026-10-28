# NIGHT60 builder L (B9, T-138): bring ONE process's main window (the Player started by n60-focus-probe.sh, by its Windows PID) to the
# foreground, as n56-foreground.ps1 forces its own window (a background process cannot take the focus with SetForegroundWindow alone:
# Windows' foreground lock; the ALT tap and AttachThreadInput lift it). The probe's ready line needs the Player focused, so the planted
# steal that follows is a real focus loss. Prints the foreground process before and after.
#   powershell -NoProfile -ExecutionPolicy Bypass -File Tools/night/n60-focus-player.ps1 -WinPid <pid>
param([int]$WinPid)
Add-Type @"
using System;
using System.Runtime.InteropServices;
public static class N60Fp {
  [DllImport("user32.dll")] public static extern IntPtr GetForegroundWindow();
  [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr h);
  [DllImport("user32.dll")] public static extern bool BringWindowToTop(IntPtr h);
  [DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr h, int cmd);
  [DllImport("user32.dll")] public static extern uint GetWindowThreadProcessId(IntPtr h, out uint pid);
  [DllImport("user32.dll")] public static extern bool AttachThreadInput(uint a, uint b, bool attach);
  [DllImport("kernel32.dll")] public static extern uint GetCurrentThreadId();
  [DllImport("user32.dll")] public static extern void keybd_event(byte vk, byte scan, uint flags, UIntPtr extra);
  public static uint FgPid() { uint p; GetWindowThreadProcessId(GetForegroundWindow(), out p); return p; }
  public static bool Force(IntPtr h) {
    uint fgPid; uint fgThread = GetWindowThreadProcessId(GetForegroundWindow(), out fgPid), me = GetCurrentThreadId();
    keybd_event(0x12, 0, 0, UIntPtr.Zero); keybd_event(0x12, 0, 2, UIntPtr.Zero);
    AttachThreadInput(me, fgThread, true);
    ShowWindow(h, 9); BringWindowToTop(h); bool ok = SetForegroundWindow(h);
    AttachThreadInput(me, fgThread, false);
    return ok;
  }
}
"@
$p = Get-Process -Id $WinPid -ErrorAction SilentlyContinue
if (-not $p) { Write-Output "focus player: no process $WinPid"; exit 1 }
$t = [DateTime]::Now
while ($p.MainWindowHandle -eq [IntPtr]::Zero -and ([DateTime]::Now - $t).TotalSeconds -lt 20) { Start-Sleep -Milliseconds 250; $p.Refresh() }
if ($p.MainWindowHandle -eq [IntPtr]::Zero) { Write-Output "focus player: $($p.ProcessName) $WinPid has no main window after 20 s"; exit 1 }
$before = [N60Fp]::FgPid()
$ok = [N60Fp]::Force($p.MainWindowHandle)
Start-Sleep -Milliseconds 300
$after = [N60Fp]::FgPid()
Write-Output ("focus player: {0} pid {1}, foreground pid before {2} after {3}, SetForegroundWindow {4}, took {5}" -f $p.ProcessName, $WinPid, $before, $after, $ok, ($after -eq $WinPid))
