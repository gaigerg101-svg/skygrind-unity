# NIGHT48 builder F, round 4: an enemy's drawn size on a saved Player frame, by colour: inside a search box round a screen point, the pixels whose
# colour is the enemy palette's (red-orange body / amber visor: R well over B, R over G; the level's floor, walls and cyan trims fail it; the
# white crosshair fails it), their bounding box and count. Prints w x h px and the count; writes a 1:1 crop with the counted pixels marked
# magenta beside it (for the lead's look).
# Run: powershell -ExecutionPolicy Bypass -File Tools/night/n48-f-bodypx.ps1 -Frame <png> -Cx 960 -Cy 540 -Half 80 -Out <crop png>
param([string]$Frame, [int]$Cx = 960, [int]$Cy = 540, [int]$Half = 80, [string]$Out)
Add-Type -AssemblyName System.Drawing
$bmp = [System.Drawing.Bitmap]::FromFile($Frame)
$x0 = [Math]::Max(0, $Cx - $Half); $x1 = [Math]::Min($bmp.Width - 1, $Cx + $Half)
$y0 = [Math]::Max(0, $Cy - $Half); $y1 = [Math]::Min($bmp.Height - 1, $Cy + $Half)
$n = 0; $bx0 = 99999; $by0 = 99999; $bx1 = -1; $by1 = -1
$side = 2 * $Half + 1
$crop = New-Object System.Drawing.Bitmap ($side * 2 + 8), $side
for ($y = $y0; $y -le $y1; $y++) {
  for ($x = $x0; $x -le $x1; $x++) {
    $c = $bmp.GetPixel($x, $y)
    $crop.SetPixel($x - $x0, $y - $y0, $c)
    $hit = ($c.R -gt 90) -and ($c.R -gt $c.B + 45) -and ($c.R -gt $c.G + 15)
    if ($hit) {
      $n++
      if ($x -lt $bx0) { $bx0 = $x }; if ($x -gt $bx1) { $bx1 = $x }
      if ($y -lt $by0) { $by0 = $y }; if ($y -gt $by1) { $by1 = $y }
      $crop.SetPixel($x - $x0 + $side + 8, $y - $y0, [System.Drawing.Color]::Magenta)
    } else { $crop.SetPixel($x - $x0 + $side + 8, $y - $y0, $c) }
  }
}
if ($Out) { $crop.Save($Out, [System.Drawing.Imaging.ImageFormat]::Png) }
$bmp.Dispose(); $crop.Dispose()
if ($n -eq 0) { Write-Output "no enemy-coloured pixel in the box" } else {
  Write-Output ("enemy-coloured pixels " + $n + ", box " + ($bx1 - $bx0 + 1) + " x " + ($by1 - $by0 + 1) + " px (w x h) at x " + $bx0 + ".." + $bx1 + ", y " + $by0 + ".." + $by1 + " (frame " + $Frame + ")")
}
