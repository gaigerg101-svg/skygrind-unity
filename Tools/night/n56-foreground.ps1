# NIGHT56 (the lead; P0-smoke's planted steal, gate-r1: "make the planted steal really take the foreground"): shows a small window FROM
# THIS SCRIPT'S OWN PROCESS (no Notepad: Windows 11's Notepad hands its window to a shared process that may hold the owner's own tabs, so
# its PID cannot be closed safely) and FORCES it to the foreground (a background process cannot take focus with SetForegroundWindow alone:
# Windows' foreground lock; the ALT key tap and AttachThreadInput lift it), holds it -HoldS seconds, then closes it (the window dies with
# this process). Prints the foreground window's process before and after, so the log shows whether the steal took.
#   powershell -NoProfile -ExecutionPolicy Bypass -File Tools/night/n56-foreground.ps1 [-HoldS 3]
param([double]$HoldS = 3)
Add-Type -AssemblyName System.Windows.Forms
Add-Type @"
using System;
using System.Runtime.InteropServices;
public static class N56Fg {
  [DllImport("user32.dll")] public static extern IntPtr GetForegroundWindow();
  [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr h);
  [DllImport("user32.dll")] public static extern bool BringWindowToTop(IntPtr h);
  [DllImport("user32.dll")] public static extern uint GetWindowThreadProcessId(IntPtr h, out uint pid);
  [DllImport("user32.dll")] public static extern bool AttachThreadInput(uint a, uint b, bool attach);
  [DllImport("kernel32.dll")] public static extern uint GetCurrentThreadId();
  [DllImport("user32.dll")] public static extern void keybd_event(byte vk, byte scan, uint flags, UIntPtr extra);
  public static uint FgPid() { uint p; GetWindowThreadProcessId(GetForegroundWindow(), out p); return p; }
  public static bool Force(IntPtr h) {
    uint fgPid; uint fgThread = GetWindowThreadProcessId(GetForegroundWindow(), out fgPid), me = GetCurrentThreadId();
    keybd_event(0x12, 0, 0, UIntPtr.Zero); keybd_event(0x12, 0, 2, UIntPtr.Zero); // ALT down / up: lifts the foreground lock
    AttachThreadInput(me, fgThread, true);
    BringWindowToTop(h); bool ok = SetForegroundWindow(h);
    AttachThreadInput(me, fgThread, false);
    return ok;
  }
}
"@
$before = [N56Fg]::FgPid()
$f = New-Object System.Windows.Forms.Form
$f.Text = "n56 focus steal"; $f.Width = 320; $f.Height = 120; $f.TopMost = $true; $f.StartPosition = "CenterScreen"
$f.Show(); [System.Windows.Forms.Application]::DoEvents()
$ok = [N56Fg]::Force($f.Handle)
for ($i = 0; $i -lt 3; $i++) { Start-Sleep -Milliseconds 100; [System.Windows.Forms.Application]::DoEvents() }
$after = [N56Fg]::FgPid()
Write-Output ("steal: own pid {0}, foreground pid before {1} after {2}, SetForegroundWindow {3}, took {4}" -f $PID, $before, $after, $ok, ($after -eq $PID))
$t = [DateTime]::Now
while (([DateTime]::Now - $t).TotalSeconds -lt $HoldS) { Start-Sleep -Milliseconds 100; [System.Windows.Forms.Application]::DoEvents() }
$f.Close(); $f.Dispose()
Write-Output ("steal: window closed; foreground pid now {0}" -f [N56Fg]::FgPid())
