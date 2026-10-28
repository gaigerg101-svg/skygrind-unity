// NIGHT38 builder C2 (crown38, a view row: design.n38.crown38; GG 2026-10-13: "polish the final boss", "make it look a little bit better"): THE CROWN's
// look pass in Crown37View. View only (no sim change, no bit). On while design.n38.crown38.enabled and not --off38 / -skygrindOff38 (N38Blocks.ForceOff);
// off, NIGHT37's look is drawn unchanged. Every number is the row's (config-driven: the lead tunes them from the frames without a code change):
//  - THE GEMS: bigger (gemScale), each set in a crossed pair of boxes (a star read from any side), a slow pulse (gemPulseHz, under 3 Hz: rule 8) and a
//    dim additive halo behind it (gemHaloScale / gemHaloGain), so the 8 gems count at 30 m; a broken gem leaves a dark ember stub in its socket
//    (brokenGain) instead of vanishing: the ring shows what is gone.
//  - THE SHATTER: the step a gem breaks (Crown37.lastGemStep), shatterShards red-orange fragments burst out of its socket at shatterSpeed m/s and fall
//    (shatterGravity) for shatterSeconds while they fade; the whole ring flashes white for shatterFlashSeconds.
//  - THE RING: an inner rim (rimInner x the ring's radius, rimThick m) inside the blades, so it reads as a crown, not a wheel; a faint halo of segments
//    round the whole (haloRadius x (ring + blade), haloGain), red-orange (the enemy's), CYAN in the window.
//  - THE SPIRE: light slits up the core's four corners (every slitEvery m from the summit to the plaza, slitGain), white, so the 60 m axis the fight
//    descends reads from every leg.
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class Crown37View
    {
        /// <summary>For the reel: the look pass drawn now, fragments flying, the stubs drawn.</summary>
        public static bool Look38Shown;
        public static int Shatter38Shown, Stubs38Shown;

        bool look38;
        Transform halo38T, spire38T;
        readonly List<Renderer> gemCross38 = new List<Renderer>(), gemHalo38 = new List<Renderer>(), rim38 = new List<Renderer>(), halo38 = new List<Renderer>();
        readonly List<Transform> frag38T = new List<Transform>();
        readonly List<Renderer> frag38R = new List<Renderer>();
        readonly List<Vector3> frag38V = new List<Vector3>();
        float frag38Born = -100f;
        int lastGem38 = -100000;
        Color white38;
        float gem38Size;

        double C38(string k) => game.cfg.Num("design.n38.crown38." + k);

        void Crown38Reset()
        {
            look38 = false;
            halo38T = spire38T = null;
            gemCross38.Clear(); gemHalo38.Clear(); rim38.Clear(); halo38.Clear();
            frag38T.Clear(); frag38R.Clear(); frag38V.Clear();
            frag38Born = -100f;
            lastGem38 = -100000;
        }

        /// <summary>BuildCrown's last line: the look pass's parts (none with the row off).</summary>
        void Crown38Build(Crown37 c, SurvivalSim sim)
        {
            look38 = !N38Blocks.ForceOff && game.cfg.Design("n38.crown38.enabled");
            if (!look38) return;
            white38 = Color.white;
            float r = (float)c.p.ringRadius, gs = UnityTuning.Crown37GemSize * (float)C38("gemScale");
            gem38Size = gs;
            int n = c.p.gems;
            for (int k = 0; k < n; k++)
            {
                float a = k * 2 * Mathf.PI / n;
                var at = new Vector3(Mathf.Cos(a) * r, Mathf.Sin(a) * r, 0);
                // the gem's star: the NIGHT37 cube (scaled up) crossed with a second cube turned 45 deg about its own facing axis
                gems[k].transform.localScale = new Vector3(gs, gs, gs);
                var x = Box(spinT, "gem38 cross", addMat, at, new Vector3(gs * 0.8f, gs * 0.8f, gs * 1.3f), out Renderer xr);
                x.localRotation = Quaternion.Euler(0, 0, a * Mathf.Rad2Deg);
                gemCross38.Add(xr);
                float hs = gs * (float)C38("gemHaloScale");
                Box(spinT, "gem38 halo", addMat, at + new Vector3(0, 0, 0.15f), new Vector3(hs, hs, 0.02f), out Renderer hr);
                gemHalo38.Add(hr);
            }
            // the inner rim: segments of a circle inside the blades
            int segs = UnityTuning.Crown37RingSegments;
            float ri = r * (float)C38("rimInner"), th = (float)C38("rimThick");
            for (int k = 0; k < segs; k++)
            {
                float a = (k + 0.5f) * 2 * Mathf.PI / segs;
                var s = Box(spinT, "rim38", ringMat, new Vector3(Mathf.Cos(a) * ri, Mathf.Sin(a) * ri, 0), new Vector3(2 * Mathf.PI * ri / segs * 1.05f, th, th), out Renderer sr);
                s.localRotation = Quaternion.Euler(0, 0, a * Mathf.Rad2Deg + 90);
                rim38.Add(sr);
            }
            // the halo: a faint ring of segments round the whole crown (not spinning)
            halo38T = new GameObject("crown38 halo").transform;
            halo38T.SetParent(crownT, false);
            float rh = (r + UnityTuning.Crown37BladeLength) * (float)C38("haloRadius");
            int hsegs = 32;
            for (int k = 0; k < hsegs; k++)
            {
                float a = (k + 0.5f) * 2 * Mathf.PI / hsegs;
                var s = Box(halo38T, "halo38", addMat, new Vector3(Mathf.Cos(a) * rh, Mathf.Sin(a) * rh, -0.1f), new Vector3(2 * Mathf.PI * rh / hsegs * 0.7f, 0.12f, 0.02f), out Renderer sr);
                s.localRotation = Quaternion.Euler(0, 0, a * Mathf.Rad2Deg + 90);
                halo38.Add(sr);
            }
            // the shatter's fragments (pooled)
            int nf = (int)C38("shatterShards");
            float fs = (float)C38("shatterSize");
            for (int k = 0; k < nf; k++)
            {
                frag38T.Add(Box(root, "shatter38", addMat, Vector3.zero, new Vector3(fs, fs, fs), out Renderer fr));
                frag38R.Add(fr);
                frag38V.Add(Vector3.zero);
                frag38T[k].gameObject.SetActive(false);
            }
            // the spire's light slits: up the core's four corners from the summit to the plaza (the level's spire37 row: axis, coreHalf, leg 3's checkpoint)
            if (sim.spire37Row != null && sim.spire37Row.TryGetValue("coreHalf", out object ch))
            {
                float half = (float)PortData.Num(ch), top = (float)sim.spire37Axis.y, bottom = (float)sim.spire37Checkpoint[2].y;
                float every = (float)C38("slitEvery"), gain = (float)C38("slitGain");
                spire38T = new GameObject("crown38 spire slits").transform;
                spire38T.SetParent(root, false);
                for (float y = top - every / 2; y > bottom + 1; y -= every)
                    foreach (var (sx, sz) in new[] { (-1, -1), (-1, 1), (1, -1), (1, 1) })
                    {
                        var p = U(sim.spire37Axis.x + sx * (half + 0.03), y, sim.spire37Axis.z + sz * (half + 0.03));
                        Box(spire38T, "slit38", addMat, Vector3.zero, new Vector3(0.12f, every * 0.45f, 0.12f), out Renderer sr2).position = p;
                        Tint(sr2, white38, gain);
                    }
            }
        }

        /// <summary>DrawCrown's last part: the look pass for this frame (nothing with the row off).</summary>
        void Crown38Draw(Crown37 c, float now, bool window, bool telling, Color tellC, bool flash, float dtStep)
        {
            if (!look38) { Look38Shown = false; return; }
            // the shatter: a gem broke since the last frame
            if (c.lastGemStep != lastGem38)
            {
                lastGem38 = c.lastGemStep;
                if (c.lastGemStep > -100000) Shatter38(c, now);
            }
            float since = now - frag38Born, life = (float)C38("shatterSeconds");
            bool ringFlash = since >= 0 && since < (float)C38("shatterFlashSeconds");
            int left = c.GemsLeft, stubs = 0;
            float pulse = 1f + (float)C38("gemPulseDepth") * Mathf.Sin(now * 2 * Mathf.PI * (float)C38("gemPulseHz"));
            Color gemC = window ? cyan : telling ? tellC : BossColours63.Part(game.cfg, "crown.gems", Look.Hex(UnityTuning.Crown37GemHex)); // NIGHT63 B3: the weak point's cyan while closed, with bossColours63
            bool redTell38 = telling && !window && tellC == red; // NIGHT63 B3c: a red (dodge-only) tell: danger after its gain (Hot); never the yellow
            for (int k = 0; k < gemCross38.Count; k++)
            {
                bool alive = k < left;
                if (!alive)
                {
                    // the stub: the NIGHT37 gem cube shown again, small and dark (the socket is empty, its ember remains)
                    gems[k].enabled = true;
                    gems[k].transform.localScale = Vector3.one * UnityTuning.Crown37GemSize * 0.45f;
                    Tint(gems[k], Look.Hex(UnityTuning.Crown37GemHex), (float)C38("brokenGain"));
                    gemCross38[k].enabled = gemHalo38[k].enabled = false;
                    stubs++;
                    continue;
                }
                gemCross38[k].enabled = gemHalo38[k].enabled = true;
                gems[k].transform.localScale = Vector3.one * gem38Size; // a new fight's gems (a retry) after the stubs
                if (redTell38)
                {
                    // NIGHT63 B3c: a red (dodge-only) tell is danger after its gain, clip-safe; the yellow (parryable) tell never routed
                    Tint(gemCross38[k], BossColours63.Hot(game.cfg, "crown.tell", gemC, UnityTuning.Crown37GemGain * pulse), 1f);
                    Tint(gemHalo38[k], BossColours63.Hot(game.cfg, "crown.tell", gemC, (float)C38("gemHaloGain") * pulse), 1f);
                }
                else
                {
                    Tint(gemCross38[k], gemC, UnityTuning.Crown37GemGain * pulse);
                    Tint(gemHalo38[k], gemC, (float)C38("gemHaloGain") * pulse);
                }
                if (!window && !telling) Tint(gems[k], gemC, UnityTuning.Crown37GemGain * pulse);
            }
            foreach (Renderer rr in rim38)
                Tint(rr, ringFlash ? white38 : window ? cyan : ringTint, ringFlash ? (float)C38("shatterFlashGain") : 1f);
            if (ringFlash) foreach (Renderer b in blades) Tint(b, white38, (float)C38("shatterFlashGain"));
            Color haloC = window || flash ? cyan : telling ? tellC : BossColours63.Part(game.cfg, "crown.halo", Look.Hex(UnityTuning.Crown37GemHex)); // NIGHT63 B3
            bool redHalo38 = telling && !window && !flash && tellC == red; // NIGHT63 B3c: the halo ring in a red tell, danger after its gain
            foreach (Renderer h in halo38)
            {
                if (redHalo38) Tint(h, BossColours63.Hot(game.cfg, "crown.tell", haloC, (float)C38("haloGain") * 2f), 1f);
                else Tint(h, haloC, (float)C38("haloGain") * (telling ? 2f : 1f));
            }
            // the fragments
            int flying = 0;
            float grav = (float)C38("shatterGravity");
            for (int k = 0; k < frag38T.Count; k++)
            {
                bool live = since >= 0 && since < life;
                frag38T[k].gameObject.SetActive(live);
                if (!live) continue;
                flying++;
                Vector3 v = frag38V[k];
                frag38T[k].position += (v + Vector3.down * grav * since) * dtStep;
                frag38T[k].rotation = Quaternion.Euler(now * 500f + k * 40f, now * 330f, 0);
                Tint(frag38R[k], danger, 2.2f * (1f - since / life));
            }
            Look38Shown = true;
            Shatter38Shown = flying;
            Stubs38Shown = stubs;
        }

        void Shatter38(Crown37 c, float now)
        {
            // the broken gem's socket: gem index GemsLeft (the gems are drawn 0 .. left-1 alive)
            int k = Mathf.Clamp(c.GemsLeft, 0, gems.Count - 1);
            Vector3 at = gems[k].transform.position;
            float sp = (float)C38("shatterSpeed");
            for (int i = 0; i < frag38T.Count; i++)
            {
                // a fixed spread (no randomness: the same burst every time): a golden-angle spiral of directions
                float t = (i + 0.5f) / frag38T.Count, inc = Mathf.Acos(1 - 2 * t), az = i * 2.39996f;
                var d = new Vector3(Mathf.Sin(inc) * Mathf.Cos(az), Mathf.Cos(inc), Mathf.Sin(inc) * Mathf.Sin(az));
                frag38T[i].position = at;
                frag38V[i] = d * sp;
            }
            frag38Born = now;
        }
    }
}
