// NIGHT35 builder W (charge35's read in the hands; audit.md item 1: "the viewmodel and WeaponFx never read charge27"). While a charge35 hold builds on
// the KESTREL or THE THUNDERCLAP (Loadout.Charge35Level, the sim's state, read only), the gun in hand:
//   - grows a cyan tip at its muzzle (0 -> Charge35TipR with the level; at FULL it pulses at Charge35PulseHz),
//   - lights a glow along its barrel (an additive shell over the front Charge35SleeveShare of the gun, brightening with the level),
//   - its gem goes toward white,
//   - draws back and tips up a little (the wind-up pose) and shakes (two sines, none with reduced motion), harder at full,
//   - and the tip flashes white for Charge35FireFlash s when the charged shot fires (charge35Fire).
// Off (-skygrindOff35 or the charge35 row off: the level is always 0): nothing is shown, the pose layers add 0 (NIGHT34's hands).
// Numbers: UnityTuning.Weapons35.cs. Objects are made in Charge35Build (from Build), never in a field initialiser (L-035).
using System;
using Skygrind.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace Skygrind.Game
{
    public sealed partial class Viewmodel
    {
        sealed class Charge35Look
        {
            public Transform tip, sleeve;
            public Material tipMat, sleeveMat;
            public float sleeveLen;
            public Vector3 sleeveAt;
        }

        readonly System.Collections.Generic.Dictionary<string, Charge35Look> c35Looks = new System.Collections.Generic.Dictionary<string, Charge35Look>();
        bool c35On;
        Color c35Cyan;
        double c35Clock, c35Flash;

        /// <summary>For the reels (charge35): the hold's level the hands drew this frame (0..1), and the tip's drawn radius (m, rig space).</summary>
        public static double Charge35DrawnLevel;
        public static float Charge35TipNow;

        void Charge35Build()
        {
            Glow50Build(); // NIGHT50 H gunglow50 (below): the four charge steps on the barrel; its own row, not charge35's
            c35On = N35Fixes.On;
            if (!c35On) return;
            c35Cyan = Look.Hex((uint)cfg.Num("colors.usable"));
            foreach (string id in new[] { "revolver", "shotgun" })
            {
                if (!rigs.TryGetValue(id, out Rig r) || r.meshes.Count == 0) continue;
                var look = new Charge35Look();
                // the tip: a small sphere at the muzzle, additive cyan (blooms)
                var tip = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                UnityEngine.Object.Destroy(tip.GetComponent<Collider>());
                tip.name = "charge35 tip";
                tip.layer = Layer;
                tip.transform.SetParent(r.t, false);
                tip.transform.localPosition = r.muzzle;
                tip.transform.localScale = Vector3.zero;
                look.tipMat = FxDraw.Additive(game.mats.neon, "charge35 tip " + id);
                look.tipMat.SetColor(BaseColor, Color.black);
                var tr = tip.GetComponent<Renderer>();
                tr.sharedMaterial = look.tipMat;
                tr.shadowCastingMode = ShadowCastingMode.Off;
                tr.receiveShadows = false;
                tip.SetActive(false);
                look.tip = tip.transform;
                // the barrel glow: a box shell from the muzzle back over the front of the gun (the body mesh's bounds give the length)
                Bounds bb = r.meshes[0].bounds;
                float back = bb.center.z - r.muzzle.z; // from the muzzle toward the body's middle (sign follows the kit's axis)
                float len = Mathf.Abs(back) * 2f * UnityTuning.Charge35SleeveShare;
                var sleeve = GameObject.CreatePrimitive(PrimitiveType.Cube);
                UnityEngine.Object.Destroy(sleeve.GetComponent<Collider>());
                sleeve.name = "charge35 barrel glow";
                sleeve.layer = Layer;
                sleeve.transform.SetParent(r.t, false);
                look.sleeveAt = new Vector3(r.muzzle.x, r.muzzle.y, r.muzzle.z + Mathf.Sign(back) * len * 0.5f);
                look.sleeveLen = len;
                sleeve.transform.localPosition = look.sleeveAt;
                sleeve.transform.localScale = new Vector3(UnityTuning.Charge35SleeveW, UnityTuning.Charge35SleeveW, len);
                look.sleeveMat = FxDraw.Additive(game.mats.neon, "charge35 barrel " + id);
                look.sleeveMat.SetColor(BaseColor, Color.black);
                var sr = sleeve.GetComponent<Renderer>();
                sr.sharedMaterial = look.sleeveMat;
                sr.shadowCastingMode = ShadowCastingMode.Off;
                sr.receiveShadows = false;
                sleeve.SetActive(false);
                look.sleeve = sleeve.transform;
                c35Looks[id] = look;
            }
            game.events.On<ChargeShot27Event>("charge35Fire", e => c35Flash = UnityTuning.Charge35FireFlash);
        }

        /// <summary>NIGHT43 F1 (design.n43.feel43; the lead's n43-r1 frames: at the THUNDERCLAP's charged release the recoil swings the muzzle
        /// at the eye and this tip's white-cyan flash sphere (additive, on the viewmodel layer) filled most of the screen for ~0.1 s, washing
        /// out the slug's own read, f0528 / shot slug-000 / -050 in both passes). With feel43 on, the tip never subtends more than
        /// Charge35TipMaxDeg43 from the viewmodel camera (a muzzle-area flash), and is hidden when the muzzle is inside the near distance.
        /// Off: the radius as it was.</summary>
        float Feel43TipCap(Transform tip, float r)
        {
            Feel43 f = Feel43.Instance;
            if (f == null || !f.On || vmCam == null || tip == null) return r;
            float d = Vector3.Distance(tip.parent != null ? tip.parent.TransformPoint(tip.localPosition) : tip.position, vmCam.transform.position);
            if (d < UnityTuning.Charge35TipNear43) return 0;
            float parentScale = tip.parent != null ? Mathf.Max(1e-4f, tip.parent.lossyScale.x) : 1f;
            float maxLocal = d * Mathf.Tan(UnityTuning.Charge35TipMaxDeg43 * Mathf.Deg2Rad) / parentScale;
            return Mathf.Min(r, maxLocal);
        }

        /// <summary>The hold's level for the gun in hand (0 when the hold is another gun's, or none).</summary>
        static double Charge35LevelFor(Loadout lo, string held)
        {
            if (lo == null) return 0;
            Charge35 q = lo.charge27 != null && lo.charge27.active ? lo.charge27.c35 : null;
            if (q == null || !q.holding || q.gun != ChargeShots27.Index(held)) return 0;
            return lo.Charge35Level;
        }

        /// <summary>The wind-up pose and the shake (from the pose stage, next to AltFrame): adds to the gun's pose sums.</summary>
        void Charge35Frame(Loadout lo, string held, double frameDt, ref double px, ref double py, ref double pz, ref double rx, ref double ry, ref double rz)
        {
            if (!c35On) return;
            c35Clock += frameDt;
            double level = Charge35LevelFor(lo, held);
            Charge35DrawnLevel = level;
            if (level <= 0) return;
            if (g50On) return; // NIGHT50 H gunglow50: the gun never moves (the never-again list); the charge reads on the barrel's steps instead of the wind-up and the shake
            double ease = level * level * (3 - 2 * level); // smoothstep: the wind-up settles as it fills
            pz -= UnityTuning.Charge35BackM * ease; // toward the eye
            rx += UnityTuning.Charge35PitchDeg * Deg * ease;
            if (cfg.movement.settings.reducedMotion) return;
            double amp = UnityTuning.Charge35ShakeM * (level >= 1 ? 1.0 : 0.45 * level);
            double w = 2 * Math.PI * UnityTuning.Charge35ShakeHz * c35Clock;
            px += amp * Math.Sin(w) * Math.Cos(w * 0.61);
            py += amp * Math.Sin(w * 1.37 + 1.1);
        }

        /// <summary>The tip, the barrel glow and the gem (after the gem's own colour is set each frame).</summary>
        void Charge35Glow(Rig rig, Loadout lo, string held, double frameDt)
        {
            Glow50Frame(rig, held); // NIGHT50 H gunglow50 (below)
            if (!c35On) return;
            if (c35Flash > 0) c35Flash = Math.Max(0, c35Flash - frameDt);
            double level = g50On ? 0 : Charge35LevelFor(lo, held); // NIGHT50 H gunglow50: the steps replace the continuous tip / sleeve / gem read (the release's tip flash stays)
            foreach (var kv in c35Looks)
            {
                Charge35Look look = kv.Value;
                bool mine = rig != null && kv.Key == rig.name;
                double flash = mine ? c35Flash / UnityTuning.Charge35FireFlash : 0;
                double lv = mine ? level : 0;
                bool show = lv > 0 || flash > 0;
                if (look.tip.gameObject.activeSelf != show) look.tip.gameObject.SetActive(show);
                if (look.sleeve.gameObject.activeSelf != (mine && lv > 0)) look.sleeve.gameObject.SetActive(mine && lv > 0);
                if (!show) continue;
                double pulse = lv >= 1 ? 0.5 + 0.5 * Math.Sin(2 * Math.PI * UnityTuning.Charge35PulseHz * c35Clock) : 0;
                float r = (float)(UnityTuning.Charge35TipR * Math.Max(lv * (1 + UnityTuning.Charge35TipFullPulse * pulse), flash * 1.4));
                r = Feel43TipCap(look.tip, r); // NIGHT43 F1 (feel43): the fire flash a muzzle-area flash, never a full-screen wash
                look.tip.localScale = Vector3.one * (2 * r);
                Charge35TipNow = r;
                Color tc = Color.Lerp(c35Cyan, Color.white, (float)Math.Max(flash, 0.35 * pulse)) * (float)(UnityTuning.Charge35TipGain * Math.Max(lv * lv, flash));
                look.tipMat.SetColor(BaseColor, tc);
                if (lv > 0)
                {
                    float g = (float)(UnityTuning.Charge35SleeveGain * lv * lv * (lv >= 1 ? 0.8 + 0.4 * pulse : 1));
                    look.sleeveMat.SetColor(BaseColor, c35Cyan * g);
                }
            }
            if (rig != null && level > 0 && c35Looks.ContainsKey(rig.name))
            {
                // the gem burns toward white as it fills (over the frame's own gem colour: its family x the gem's gain)
                Color baseC = Color.Lerp(rig.gemColor, Color.white, (float)(UnityTuning.Charge35GemWhite * level));
                rig.gem.SetColor(BaseColor, baseC * (float)(2.4 * UnityTuning.HandsGemGain * (1 + level)));
            }
        }

        // =================================================================================================================================
        // NIGHT50 builder H (design.n50.gunglow50, a view row: no sim bit; NIGHT50.md G2, DECIDE50 D-8). The charge on the gun, the second read
        // (the bar under the reticle, charge50, stays the first): ULTRAKILL's Railcannon lights its capacitors at quarter steps; here
        // UnityTuning.G50Steps segments along the barrel of the KESTREL, the THUNDERCLAP and the PULSE gauntlet light one per 25 % of the charge,
        // back to front, in the weapon's Look49 token (cyan: usable); 0 at rest (all dark, hidden); one G50FlashFrames white flash when it
        // reaches 100 %; the gun never moves (the never-again list: with the row on, charge35's wind-up and shake add nothing, and its
        // continuous tip / sleeve / gem read gives way to the steps; the release's tip flash stays). The charge: the KESTREL / THUNDERCLAP hold
        // (Charge35 or ChargeShots27, hud38's read); the PULSE's kill pips over its pip count while the BLAST is off cooldown.
        // Off (the row off, -skygrindOff50 / --off50 or any older switch): nothing is built and charge35 draws as NIGHT49 drew it.
        // View only: reads the sim and the loadout, writes nothing back. Numbers: UnityTuning.H50.cs.
        // =================================================================================================================================

        sealed class Glow50Look
        {
            public string gun;
            public Transform[] segs;
            public Material mat;
            public Color shown = new Color(-1, -1, -1, -1);
            public bool row71; // NIGHT71 L6 glow71 (Glow71.cs): the PULSE's steps, drawn only on a frame the row reads on
        }

        readonly System.Collections.Generic.List<Glow50Look> g50Looks = new System.Collections.Generic.List<Glow50Look>();
        bool g50On;
        string g50PrevGun;
        int g50PrevStep = -1, g50FlashLeft;

        /// <summary>Pure: the lit segments for a charge level (0..1): one per full quarter (0.25 -> 1 ... 1.0 -> G50Steps).</summary>
        public static int Glow50Step(double level)
        {
            if (!(level > 0)) return 0;
            int n = (int)Math.Floor(Math.Min(1.0, level) * UnityTuning.G50Steps + 1e-9);
            return n < 0 ? 0 : n > UnityTuning.G50Steps ? UnityTuning.G50Steps : n;
        }

        void Glow50Build()
        {
            g50On = NightRows.ViewOn(cfg, "gunglow50");
            Gunglow50Probe.Built = false;
            Gunglow50Probe.Guns = "";
            if (!g50On) return;
            // NIGHT50 G2 fallback (DECIDE50 D-8): PULSE is OFF. Its steps sat inside the gauntlet's dark block and the 0 % and 75 % frames read alike
            // (n50-r1 a150, glow-gauntlet-0 / -75). KESTREL and THUNDERCLAP only.
            foreach (string id in new[] { "revolver", "shotgun" })
            {
                if (!rigs.TryGetValue(id, out Rig r) || r.meshes.Count == 0) continue;
                // the barrel: from the muzzle back toward the body's middle (charge35's sleeve axis), G50Span of that length, G50Steps slots
                Bounds bb = r.meshes[0].bounds;
                float back = bb.center.z - r.muzzle.z, sign = Mathf.Sign(back);
                float span = Mathf.Abs(back) * 2f * UnityTuning.G50Span, slot = span / UnityTuning.G50Steps, len = slot * (1f - UnityTuning.G50GapShare);
                var look = new Glow50Look { gun = id, segs = new Transform[UnityTuning.G50Steps] };
                look.mat = FxDraw.Additive(game.mats.neon, "gunglow50 " + id);
                look.mat.SetColor(BaseColor, Color.black);
                for (int i = 0; i < UnityTuning.G50Steps; i++)
                {
                    var seg = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    UnityEngine.Object.Destroy(seg.GetComponent<Collider>());
                    seg.name = "gunglow50 step " + (i + 1);
                    seg.layer = Layer;
                    seg.transform.SetParent(r.t, false);
                    // segment 0 is the farthest back (the first quarter), the last sits at the muzzle (full)
                    float z = r.muzzle.z + sign * (span - (i + 0.5f) * slot);
                    seg.transform.localPosition = new Vector3(r.muzzle.x, r.muzzle.y, z);
                    seg.transform.localScale = new Vector3(UnityTuning.G50SegW, UnityTuning.G50SegW, len);
                    var sr = seg.GetComponent<Renderer>();
                    sr.sharedMaterial = look.mat;
                    sr.shadowCastingMode = ShadowCastingMode.Off;
                    sr.receiveShadows = false;
                    seg.SetActive(false);
                    look.segs[i] = seg.transform;
                }
                g50Looks.Add(look);
                Gunglow50Probe.Guns += (Gunglow50Probe.Guns.Length > 0 ? " " : "") + id;
            }
            Glow71Build(); // NIGHT71 L6 glow71 (Glow71.cs): the PULSE gauntlet's own steps on its top, hidden until the row reads on
            Gunglow50Probe.Built = g50Looks.Count > 0;
            game.OnNewRun += () => { g50PrevGun = null; g50PrevStep = -1; g50FlashLeft = 0; Gunglow50Probe.Flashes = 0; };
        }

        /// <summary>The charge (0..1) the gun in hand's steps show: the reel's ForceLevel, the PULSE's pips (BLAST off cooldown), or the hold.</summary>
        double Glow50Level(string held)
        {
            if (Gunglow50Probe.ForceLevel.HasValue) return Math.Max(0, Math.Min(1, Gunglow50Probe.ForceLevel.Value));
            SurvivalSim sim = game.sim;
            Loadout lo = game.loadout;
            if (sim == null || lo == null) return 0;
            if (held == "gauntlet")
            {
                PulseCharge27 pc = sim.pulse27;
                if (!sim.n27.pulse27 || pc == null || !pc.active || pc.p == null || pc.p.pips <= 0 || pc.cooldownSteps > 0) return 0;
                return Math.Max(0, Math.Min(1, pc.pips / (double)pc.p.pips));
            }
            int gi = ChargeShots27.Index(held);
            ChargeShots27 cs = (sim.n27.chargeShots27 || sim.n31.charge) && lo.charge27 != null && lo.charge27.active ? lo.charge27 : null;
            if (gi < 0 || cs == null) return 0;
            Charge35 q = cs.c35;
            if (q != null && q.holding && q.gun == gi) return Math.Max(0, Math.Min(1, lo.Charge35Level));
            if (cs.charging) return Math.Max(0, Math.Min(1, cs.Level(gi)));
            return 0;
        }

        /// <summary>Each frame (from Charge35Glow): the gun in hand's lit steps, the flash at full, the reel's probe.</summary>
        void Glow50Frame(Rig rig, string held)
        {
            if (!g50On) return;
            string gun = rig != null ? rig.name : null;
            Glow50Look mine = null;
            bool glow71 = Glow71.On(cfg); // NIGHT71 L6: read every frame (the same-run toggle); off: the PULSE has no steps, as NIGHT50 shipped
            for (int i = 0; i < g50Looks.Count; i++) if (g50Looks[i].gun == gun && (!g50Looks[i].row71 || glow71)) mine = g50Looks[i];
            double level = mine != null && gun == held ? Glow50Level(held) : 0;
            int step = Glow50Step(level);
            if (gun != g50PrevGun) { g50PrevGun = gun; g50FlashLeft = 0; } // a switch: the new gun as it is, no flash
            else if (step >= UnityTuning.G50Steps && g50PrevStep >= 0 && g50PrevStep < UnityTuning.G50Steps) { g50FlashLeft = UnityTuning.G50FlashFrames; Gunglow50Probe.Flashes++; }
            g50PrevStep = step;
            bool flash = g50FlashLeft > 0 && step > 0;
            if (g50FlashLeft > 0) g50FlashLeft--;
            Color tok = Look.Hex(UnityTuning.Look49UsableHex); // the weapon's token: every one of the three is a player gun (cyan = usable)
            Color want = flash ? Color.white * UnityTuning.G50FlashGain : tok * UnityTuning.G50Gain;
            for (int k = 0; k < g50Looks.Count; k++)
            {
                Glow50Look look = g50Looks[k];
                int lit = look == mine ? step : 0;
                for (int i = 0; i < look.segs.Length; i++)
                {
                    bool on = i < lit;
                    if (look.segs[i].gameObject.activeSelf != on) look.segs[i].gameObject.SetActive(on);
                }
                if (lit > 0 && look.shown != want) { look.shown = want; look.mat.SetColor(BaseColor, want); }
            }
            Gunglow50Probe.Gun = gun ?? "";
            Gunglow50Probe.Level = level;
            Gunglow50Probe.Step = step;
            Gunglow50Probe.FlashNow = flash;
            Gunglow50Probe.HasSteps = mine != null;
            if (Gunglow50Probe.WantBox) Gunglow50Probe.Box = Glow50Box(rig);
        }

        /// <summary>The gun in hand's screen box (px, origin bottom-left as GetPixels32 rows run): its active renderers' bounds projected by the
        /// viewmodel camera. For the reel's luma per step (Gunglow50Probe.WantBox); an empty rect when there is nothing to project.</summary>
        Rect Glow50Box(Rig rig)
        {
            if (rig == null || vmCam == null) return Rect.zero;
            float x0 = float.MaxValue, y0 = float.MaxValue, x1 = float.MinValue, y1 = float.MinValue;
            foreach (Renderer rd in rig.t.GetComponentsInChildren<Renderer>())
            {
                Bounds b = rd.bounds;
                for (int c = 0; c < 8; c++)
                {
                    Vector3 p = new Vector3((c & 1) == 0 ? b.min.x : b.max.x, (c & 2) == 0 ? b.min.y : b.max.y, (c & 4) == 0 ? b.min.z : b.max.z);
                    Vector3 s = vmCam.WorldToScreenPoint(p);
                    if (s.z <= 0) continue;
                    x0 = Mathf.Min(x0, s.x); y0 = Mathf.Min(y0, s.y); x1 = Mathf.Max(x1, s.x); y1 = Mathf.Max(y1, s.y);
                }
            }
            if (x1 <= x0 || y1 <= y0) return Rect.zero;
            x0 = Mathf.Max(0, x0); y0 = Mathf.Max(0, y0); x1 = Mathf.Min(Screen.width, x1); y1 = Mathf.Min(Screen.height, y1);
            return x1 > x0 && y1 > y0 ? Rect.MinMaxRect(x0, y0, x1, y1) : Rect.zero;
        }
    }

    /// <summary>NIGHT50 gunglow50's statics: the reel (a150) reads them and can force a level (the Viewmodel instance is SurvivalGame's).</summary>
    public static class Gunglow50Probe
    {
        /// <summary>The steps are built (gunglow50 on) and on which rigs ("revolver shotgun gauntlet").</summary>
        public static bool Built;
        public static string Guns = "";
        /// <summary>The reel's forced level for the gun in hand (null: the sim's).</summary>
        public static double? ForceLevel;
        /// <summary>The reel asks for the gun's screen box each frame.</summary>
        public static bool WantBox;
        /// <summary>This frame: the drawn gun, its level, its lit steps, the flash on this frame, whether the gun has steps, its screen box.</summary>
        public static string Gun = "";
        public static double Level;
        public static int Step, Flashes;
        public static bool FlashNow, HasSteps;
        public static Rect Box;
    }
}
