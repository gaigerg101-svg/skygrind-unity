// NIGHT72 builder B (slate B2, bossfx72, a VIEW row: NightRows.ViewOn(cfg, "bossfx72"); gg-proxy #3; GG 2026-10-20 "the guns and effects during boss fights
// should follow proper color theory", 2026-10-22 "effect colores everything doenst feel alligned"): THE CROWN in its own fight. View only: no collider, no sim
// read for writing, no timing; the sim's ring, gems and pass are the truth.
// WHAT THE FRAMES SHOWED (bp/boss63, n71-polish8): THE CROWN was in none of its five frames. Its 20 gem halos (NIGHT38: 2.2 x a 1.12 m gem = 2.46 m squares,
// additive, on a ring whose gems are 0.94 m apart) overlapped into one annulus 700-900 px wide, cyan while idle and red-orange in a tell, that hid the ring and
// its blades; in a red tell the ring itself went to the danger colour while the fight line said to dash through it.
// THE ROW ON (read EVERY frame, D1's contract; nothing here keeps a state but the scales it restores):
//  - the alive gems are Fx72GemOfGap of the gap between neighbours and their crosses and halos follow (halo = Fx72HaloOfGem x gem): the gems read one by one;
//    their colours (gem, cross, halo) are NIGHT38's / NIGHT40's / the tell's own, read back, x Fx72GemGainShare / Fx72HaloGainShare (no flat clip) and x the AIM WINDOW
//    (AimFactor: within Fx72AimOuterPx of the screen's centre they ease down to Fx72AimFloor; t1's verdict: "the bursts must come off the crosshair"; the ring and blades are lines and stay);
//    the pass runs after DrawMotion63 so the gems' screen positions are final;
//  - the blade ring, the inner rim and the outer halo ring stay CYAN (usable, rule 8) through a RED tell too, at Fx72RingGain (the lit gain when the flat
//    speed is at the pass speed): the tell is on the gems, the points' edges, the point light and the danger column, never on the ring the text names;
//    a YELLOW (parry) tell, the window and a gem's break flash are untouched;
//  - the column (BossTell63.cs) is Fx72ColumnShare of its size.
// OFF: NIGHT71's frame exactly (the scales restored the frame the row is read false; the colours are NIGHT38 / NIGHT40's every frame).
// Proof (L-014 / L-115; Player.log, printed when the row's state or a red tell changes; they read one way OFF and the other ON):
//   [fx72] crown row bossfx72 on|off: gem <m> m, halo <m> m (the drawn squares), gap <m> m
//   [fx72] crown red tell <attack>: ring hue <deg> (cyan 165-200), gems hue <deg>, halo gain x<g> | row bossfx72 on|off
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class Crown37View
    {
        /// <summary>For the reel: the row's pass drew this frame; the drawn gem and halo sizes (m); the red tells seen and their ring hues (deg of the last).</summary>
        public static bool Fx72Shown;
        public static float Fx72GemM, Fx72HaloM, Fx72GapM, Fx72RingHue = -1f, Fx72GemHue = -1f, Fx72AimMin = 1f;
        public static int Fx72RedTells, Fx72AimDimmed;

        bool fx72Applied, fx72SaidOn, fx72Said;
        Vector3[] fx72Cross, fx72Halo;
        string fx72TellSaid = "";
        float fx72AimClock;

        const string Fx72Row = "bossfx72";

        void Crown72Reset()
        {
            fx72Applied = false;
            fx72Cross = fx72Halo = null; // the look pass's parts were rebuilt: their base scales are read again at the next ON frame
            fx72TellSaid = "";
            fx72Said = false;
        }

        /// <summary>Every frame of a live Crown, after Crown47Draw (so these are the last tints) and before DrawMotion63.</summary>
        void Crown72Draw(Crown37 c, SurvivalSim sim, float now, bool window, bool telling, bool redTell, bool flash)
        {
            bool on = NightRows.ViewOn(game.cfg, Fx72Row);
            Fx72Shown = on;
            int n = gemHalo38.Count;
            if (!look38 || n == 0 || gemCross38.Count != n || gems.Count < n) { if (!on) fx72Applied = false; return; }
            float gap = 2f * Mathf.PI * (float)c.p.ringRadius / Mathf.Max(1, c.p.gems);
            Fx72GapM = gap;
            if (!on)
            {
                if (fx72Applied) Fx72Restore(n);
                Fx72Say(false, n);
                if (redTell) Fx72SayTell(c, false);
                else fx72TellSaid = "";
                return;
            }
            if (!fx72Applied)
            {
                fx72Cross = new Vector3[n];
                fx72Halo = new Vector3[n];
                for (int k = 0; k < n; k++) { fx72Cross[k] = gemCross38[k].transform.localScale; fx72Halo[k] = gemHalo38[k].transform.localScale; }
                fx72Applied = true;
            }
            // THE GEMS: one by one (the alive ones; a broken gem's stub is NIGHT38's)
            float gem0 = gem38Size;
            float gem = Mathf.Min(gem0, gap * UnityTuning.Fx72GemOfGap);
            float share = gem0 > 1e-4f ? gem / gem0 : 1f;
            float halo = gem * UnityTuning.Fx72HaloOfGem;
            int left = c.GemsLeft, dimmed = 0;
            float aimMin = 1f;
            for (int k = 0; k < n; k++)
            {
                if (k >= left) continue;
                gems[k].transform.localScale = Vector3.one * gem;
                gemCross38[k].transform.localScale = fx72Cross[k] * share;
                gemHalo38[k].transform.localScale = new Vector3(halo, halo, fx72Halo[k].z);
                // the gem's colour as NIGHT38 / NIGHT40 / the tell set it, read back: x the gem share (no flat clip), x the aim window (the crosshair keeps a dim core, not a burst)
                float aim = AimFactor(gems[k].transform.position);
                if (aim < 0.999f) dimmed++;
                if (aim < aimMin) aimMin = aim;
                ScaleColour(gems[k], UnityTuning.Fx72GemGainShare * aim);
                ScaleColour(gemCross38[k], UnityTuning.Fx72GemGainShare * aim);
                ScaleColour(gemHalo38[k], UnityTuning.Fx72HaloGainShare * aim);
            }
            Fx72GemM = gem;
            Fx72HaloM = halo;
            Fx72AimDimmed = dimmed;
            Fx72AimMin = aimMin;
            // THE RING: cyan while it may be passed, through a red tell too (crown40's rule: the window's pulse, a pass's flash, a yellow tell, a gem's break flash are not touched)
            float sinceBreak = now - frag38Born;
            bool breakFlash = sinceBreak >= 0 && sinceBreak < (float)C38("shatterFlashSeconds");
            if (On40(sim) && !window && !flash && !breakFlash && (!telling || redTell))
            {
                float g = Crown40Lit ? UnityTuning.Crown40LitGain : UnityTuning.Fx72RingGain;
                if (telling || !Crown40Lit) // idle and lit are crown40's own tint (Crown40Draw), but its idle gain is dimmer than the ring needs on the dark ceiling
                {
                    foreach (Renderer r in blades) Tint(r, cyan, g);
                    foreach (Renderer r in rim38) Tint(r, cyan, g);
                    foreach (Renderer r in halo38) Tint(r, cyan, g * 0.6f);
                }
            }
            Fx72Say(true, n);
            if (redTell) Fx72SayTell(c, true);
            else fx72TellSaid = "";
            fx72AimClock += Time.deltaTime;
            if (fx72AimClock >= 2f)
            {
                fx72AimClock = 0f;
                Debug.Log("[fx72] crown aim window: " + Fx72AimDimmed + " of " + left + " gems inside " + UnityTuning.Fx72AimOuterPx.ToString("0") + " px of the crosshair, the dimmest at x"
                    + Fx72AimMin.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) + " | row bossfx72 on");
            }
        }

        /// <summary>The aim window's factor for a point in the world (1 at Fx72AimOuterPx and beyond from the screen's centre, Fx72AimFloor inside Fx72AimInnerPx, a smoothstep between):
        /// the distance in px at 1080p of the viewport point from the centre (the crosshair); 1 for a point behind the camera or with no camera.</summary>
        float AimFactor(Vector3 world)
        {
            Camera cam = game.cam;
            if (cam == null) return 1f;
            Vector3 vp = cam.WorldToViewportPoint(world);
            if (vp.z <= 0f) return 1f;
            float px = (vp.x - 0.5f) * 1080f * cam.aspect, py = (vp.y - 0.5f) * 1080f;
            float d = Mathf.Sqrt(px * px + py * py);
            float t = Mathf.Clamp01((d - UnityTuning.Fx72AimInnerPx) / Mathf.Max(1f, UnityTuning.Fx72AimOuterPx - UnityTuning.Fx72AimInnerPx));
            t = t * t * (3f - 2f * t);
            return Mathf.Lerp(UnityTuning.Fx72AimFloor, 1f, t);
        }

        void Fx72Restore(int n)
        {
            for (int k = 0; k < n && fx72Cross != null && k < fx72Cross.Length; k++)
            {
                gemCross38[k].transform.localScale = fx72Cross[k];
                gemHalo38[k].transform.localScale = fx72Halo[k];
            }
            fx72Applied = false;
        }

        /// <summary>The renderer's drawn _BaseColor times `k` (what the earlier passes set, read back); the alpha is kept.</summary>
        void ScaleColour(Renderer r, float k)
        {
            block.Clear();
            r.GetPropertyBlock(block);
            Color c = block.GetColor(baseColorId);
            block.SetColor(baseColorId, new Color(c.r * k, c.g * k, c.b * k, c.a));
            r.SetPropertyBlock(block);
        }

        float HueOf(Renderer r)
        {
            block.Clear();
            r.GetPropertyBlock(block);
            Color c = block.GetColor(baseColorId);
            Color.RGBToHSV(new Color(Mathf.Clamp01(c.r), Mathf.Clamp01(c.g), Mathf.Clamp01(c.b)), out float h, out _, out _);
            return h * 360f;
        }

        /// <summary>The row's state, once per change (the reel's one-way-OFF, other-way-ON line).</summary>
        void Fx72Say(bool on, int n)
        {
            if (fx72Said && fx72SaidOn == on) return;
            fx72Said = true;
            fx72SaidOn = on;
            float gem = n > 0 ? gems[0].transform.localScale.x : 0f, halo = n > 0 ? gemHalo38[0].transform.localScale.x : 0f;
            Debug.Log("[fx72] crown row bossfx72 " + (on ? "on" : "off") + ": gem " + gem.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)
                + " m, halo " + halo.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) + " m, gap " + Fx72GapM.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) + " m, aim window "
                + (on ? UnityTuning.Fx72AimInnerPx.ToString("0") + "-" + UnityTuning.Fx72AimOuterPx.ToString("0") + " px floor x" + UnityTuning.Fx72AimFloor.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) : "none"));
        }

        /// <summary>A red tell's colours as drawn (read back from the renderers this frame), once per attack.</summary>
        void Fx72SayTell(Crown37 c, bool on)
        {
            string id = c.brain.Current != null ? c.brain.Current.id : "?";
            if (fx72TellSaid == id) return;
            fx72TellSaid = id;
            Fx72RedTells++;
            Fx72RingHue = blades.Count > 0 ? HueOf(blades[0]) : -1f;
            Fx72GemHue = gems.Count > 0 ? HueOf(gems[0]) : -1f;
            var ci = System.Globalization.CultureInfo.InvariantCulture;
            Debug.Log("[fx72] crown red tell " + id + ": ring hue " + Fx72RingHue.ToString("0", ci) + " (cyan 165-200), gems hue " + Fx72GemHue.ToString("0", ci)
                + ", column radius x" + (on ? UnityTuning.Fx72ColumnShare["crown37"][0] : 1f).ToString("0.00", ci) + " | row bossfx72 " + (on ? "on" : "off"));
        }
    }
}
