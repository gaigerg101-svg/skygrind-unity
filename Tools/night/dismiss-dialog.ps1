# Chooses Skip on a window titled EXACTLY "Recovering Scene Backups"; any other title: no keystroke.
$target = 'Recovering Scene Backups'
$procs = Get-Process Unity -ErrorAction SilentlyContinue
$hit = $null
foreach ($p in $procs) {
    if ($p.MainWindowTitle -ceq $target) { $hit = $p }
}
if (-not $hit) {
    $titles = ($procs | ForEach-Object { $_.MainWindowTitle }) -join ' | '
    Write-Output "no exact title match (titles: $titles); no keystroke"
    exit 0
}
Write-Output "matched title: '$($hit.MainWindowTitle)'"
Add-Type -AssemblyName System.Windows.Forms
$sh = New-Object -ComObject WScript.Shell
if ($sh.AppActivate($hit.Id)) {
    Start-Sleep -Milliseconds 400
    # Skip is the second button in the dialog; Tab then Enter
    [System.Windows.Forms.SendKeys]::SendWait('{TAB}{ENTER}')
    Write-Output 'sent Tab+Enter (Skip)'
} else {
    Write-Output 'AppActivate failed; no keystroke'
}
