// NIGHT48 builder W (G5, NIGHT48.md section 1: the fight text at 1280x720 and 1920x1080): a read-only probe of hud47's fight line (F's
// UiHud47.Boss.cs builds and draws it; nothing here changes it) for the a148 reel: the line shown now, its em size and its drawn glyphs' height in
// SCREEN pixels at the current resolution (the HUD canvas is screen-space overlay, scaled with the height from 1080: Ui.cs), how many text objects
// draw it and whether a raw tag is left in what TMP drew. No Unity API in field initializers (L-035).
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class Ui
    {
        /// <summary>a148 (G5): the fight line now. False: hud47's fight line is not built (hud47 off, or no boss bar yet). emPx: the font's em in
        /// screen px; inkPx: the drawn glyphs' bounds height in screen px (cap + descender); scale: the canvas scale (1 at 1080 p).</summary>
        public bool W48FightLine(out string shown, out float emPx, out float inkPx, out float scale, out float alpha)
        {
            shown = Hud47Probe.FightLine;
            emPx = inkPx = scale = alpha = 0f;
            if (fight47 == null || fight47.main == null) return false;
            fight47.main.ForceMeshUpdate();
            float k = fight47.main.rectTransform.lossyScale.y;
            scale = k;
            emPx = fight47.main.fontSize * k;
            inkPx = fight47.main.textBounds.size.y * k;
            alpha = fight47.Alpha;
            return true;
        }
    }
}
