# NIGHT48 builder F, round 3 (DECIDE48 D-29): the lineup blind pairs for three kinds (the WARDEN's shield, the SWARMER, the DRONE), old (OFF) vs
# new (ON), from the a948 reel's crops (shot-N30-L48-pair-<kind>-<close|10m>-<P|Q>.png). One sheet per kind for the judges: two rows (the
# moments), P on the left, Q on the right, labelled only "P" / "Q" and the moment; no row, build or OFF / ON name on the sheet. The KEY goes
# to its own folder (never beside the sheets).
# Run: powershell -ExecutionPolicy Bypass -File Tools/night/n48-f-pairs.ps1 -Src <the a948 dir> -Out <sheets dir> -KeyDir <key dir> -Keys "swarmer=ON,warden=OFF,drone=ON"
#   -Keys: what P is per kind, read from the a948 line ("pairs ...: <kind>: P = ON (new) / OFF (old)").
param([string]$Src, [string]$Out, [string]$KeyDir, [string]$Keys)
Add-Type -AssemblyName System.Drawing
New-Item -ItemType Directory -Force $Out | Out-Null
New-Item -ItemType Directory -Force $KeyDir | Out-Null
$moments = @(
  @{ tag = "close"; text = "3 m, 45 deg" },
  @{ tag = "10m"; text = "10 m, 45 deg" }
)
$questions = @{
  warden = "Which shield looks held, centred on the chest and clear of the body?";
  swarmer = "Which small robot reads better as a pill on four pointed legs?";
  drone = "Which drone looks sharper and more of one design?"
}
$keyLines = @("NIGHT48 F lineup blind pairs (DECIDE48 D-29): the key. Sheets: $Out", "Source frames: $Src (a948, n48-r2)", "")
$keyMap = @{}
foreach ($kv in $Keys.Split(",")) { $p = $kv.Split("="); $keyMap[$p[0].Trim()] = $p[1].Trim() }
foreach ($kind in @("warden", "swarmer", "drone")) {
  $cell = 640; $gut = 16; $head = 56; $label = 40
  $w = 2 * $cell + 3 * $gut
  $h = $head + 2 * ($label + $cell) + 3 * $gut
  $bmp = New-Object System.Drawing.Bitmap $w, $h
  $gfx = [System.Drawing.Graphics]::FromImage($bmp)
  $gfx.Clear([System.Drawing.Color]::FromArgb(18, 18, 22))
  $gfx.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::NearestNeighbor
  $font = New-Object System.Drawing.Font "Arial", 20, ([System.Drawing.FontStyle]::Bold)
  $small = New-Object System.Drawing.Font "Arial", 16
  $white = [System.Drawing.Brushes]::White
  $gfx.DrawString($questions[$kind], $font, $white, $gut, 14)
  $y = $head + $gut
  foreach ($m in $moments) {
    $x = $gut
    foreach ($side in @("P", "Q")) {
      $file = Join-Path $Src ("shot-N30-L48-pair-" + $kind + "-" + $m.tag + "-" + $side + ".png")
      $mt = if ($kind -eq "warden" -and $m.tag -eq "10m") { "10 m, front" } else { $m.text } # a948: the warden's 10 m pair is its front (the shield)
      $gfx.DrawString($side + "   (" + $mt + ")", $small, $white, $x, $y + 8)
      if (Test-Path $file) {
        $img = [System.Drawing.Image]::FromFile($file)
        $gfx.DrawImage($img, $x, $y + $label, $cell, $cell)
        $img.Dispose()
      } else { $gfx.DrawString("missing: " + $file, $small, [System.Drawing.Brushes]::Red, $x, $y + $label + 20) }
      $x += $cell + $gut
    }
    $y += $label + $cell + $gut
  }
  $path = Join-Path $Out ("pair-" + $kind + ".png")
  $bmp.Save($path, [System.Drawing.Imaging.ImageFormat]::Png)
  $gfx.Dispose(); $bmp.Dispose()
  $pIs = $keyMap[$kind]
  $qIs = if ($pIs -eq "ON") { "OFF" } else { "ON" }
  $keyLines += ("pair-" + $kind + ".png: P = " + $pIs + ", Q = " + $qIs + "   (ON = new: foes47 + visor47 + foes48; OFF = old: NIGHT46's models)")
  $m2 = if ($kind -eq "warden") { "10 m front" } else { "10 m at 45 deg" }
  $keyLines += ("  question: " + $questions[$kind] + "   moments: 3 m at 45 deg (row 1), " + $m2 + " (row 2), frozen, colour")
  Write-Output ("wrote " + $path)
}
$keyFile = Join-Path $KeyDir "KEY-F-lineup-pairs-n48-r2.txt"
Set-Content -Path $keyFile -Value $keyLines -Encoding utf8
Write-Output ("key " + $keyFile)
