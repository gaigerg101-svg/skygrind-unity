// NIGHT54 builder L: design.n54.light54 (a VIEW row; NIGHT54.md, GG 2026-10-20: "the lighting and flickering bugs that happen constantly break
// immersion and absolutely need to be fixed ... The first boss fight in particular has far too many lighting issues"). Its switch:
// NightRows.ViewOn(cfg, "light54") and -skygrindOff54 (or any older off switch) read from the command line too (the map's lights are built
// before PlayerChecks applies the switch, as polish53 does). Every number is design.n54.light54.numbers.<key> (read once per config). Off:
// NIGHT53's lights exactly. The fixes (each file names its own):
//   1. LightPools.Light54.cs: the pops (kills, blasts, world impacts) get their own light and never take the muzzle flash's; an impact adds to
//      a glow that decays smoothly (no full-range re-trigger per shot), the muzzle flash ends on a short tail instead of a cut.
//   2. LightKit29.Light54.cs: the room's borrowed fixture lights move only when a fixture is clearly closer (hysteresis), cross-fade through a
//      twin light (no dark dip), and the danger fixtures' perlin flicker is slower and shallower (Vfx33).
//   3. LightKit29.Light54.cs: the landmark light eases to its 0.25 s target and ignores the transient lights (Light54.Transient).
//   4+. THE WICK's arena, the juice flashes, the z-fighting lifts, the shadow bias, the one ambient owner: see their files' "light54" lines.
using System;
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public static class Light54
    {
        const string Row = "design.n54.light54.numbers.";

        /// <summary>The a154 reel's off / on passes (null: the config and the command line).</summary>
        public static bool? Force;

        static GameConfig readFor;
        static bool on;

        /// <summary>light54 draws: its row on and no -skygrindOffNN at or under 54.</summary>
        public static bool On(GameConfig cfg)
        {
            if (Force.HasValue) return Force.Value;
            if (cfg == null) return false;
            if (!ReferenceEquals(cfg, readFor))
            {
                readFor = cfg;
                bool offSwitch = NightRows.OffFrom(Environment.GetCommandLineArgs(), "-skygrindOff") <= 54;
                on = NightRows.ViewOn(cfg, "light54") && !offSwitch;
            }
            return on;
        }

        static GameConfig numsFor;
        static readonly Dictionary<string, double> nums = new Dictionary<string, double>();

        /// <summary>A number (design.n54.light54.numbers.&lt;key&gt;), `missing` when the key is not there. Read once per config and kept (the
        /// per-frame readers allocate nothing after the first read).</summary>
        public static double Num(GameConfig cfg, string key, double missing)
        {
            if (cfg == null) return missing;
            if (!ReferenceEquals(cfg, numsFor)) { nums.Clear(); numsFor = cfg; }
            if (nums.TryGetValue(key, out double v)) return double.IsNaN(v) ? missing : v;
            string k = Row + key;
            v = double.NaN;
            if (cfg.Has(k))
            {
                try { v = cfg.Num(k); }
                catch (Exception) { v = double.NaN; }
            }
            nums[key] = v;
            return double.IsNaN(v) ? missing : v;
        }

        public static float F(GameConfig cfg, string key, double missing) => (float)Num(cfg, key, missing);

        /// <summary>The lights that live a fraction of a second (the muzzle flash, the pool's flashes, the pops): the landmark's 0.25 s read
        /// skips them with the row on (it no longer chases a shot).</summary>
        public static readonly HashSet<Light> Transient = new HashSet<Light>();

        /// <summary>Fix 9 (z-fighting): a decal / panel lift at the level's build, raised to numbers.zLiftM with the row on (the camera's near clip
        /// 0.05 m and far 450 m leave a 24-bit depth step of several mm at 30 m: lifts of 5-12 mm fought their faces at range). Off: `old`.</summary>
        public static float Lift(float old)
        {
            GameConfig cfg = SurvivalGame.Instance != null ? SurvivalGame.Instance.cfg : null;
            return On(cfg) ? Mathf.Max(old, F(cfg, "zLiftM", 0.03)) : old;
        }

        /// <summary>Fix 4 / 10: the tells' "now" cue (Tell36PulseHz 7 Hz +-25 % through a tell's last 30 %, in Tell36View, Boss36View, Stag37View,
        /// Wick37View) is calmed with the row on (the game's config: SurvivalGame.Instance).</summary>
        public static bool TellOn => SurvivalGame.Instance != null && On(SurvivalGame.Instance.cfg);

        /// <summary>The calmed cue: one smoothstep brighten over the last 30 % to x (1 + numbers.tellRamp); the tell's colour and timing unchanged.</summary>
        public static float TellRamp(double progress)
        {
            float u = Mathf.Clamp01((float)((progress - 0.7) / 0.3));
            return 1f + F(SurvivalGame.Instance != null ? SurvivalGame.Instance.cfg : null, "tellRamp", 0.25) * u * u * (3f - 2f * u);
        }

        /// <summary>Lift for a double (the drawn ceiling): `old` exactly with the row off.</summary>
        public static double Lift(double old)
        {
            GameConfig cfg = SurvivalGame.Instance != null ? SurvivalGame.Instance.cfg : null;
            return On(cfg) ? System.Math.Max(old, Num(cfg, "zLiftM", 0.03)) : old;
        }

        static readonly HashSet<string> stoneLogged = new HashSet<string>();

        /// <summary>The stone-colour fix (the lead, 15:22, after THE STACK's): Assets/Shaders/Stone.shader takes its albedo from the VERTEX colour
        /// (Stone.shader:197/199) and never reads _BaseColor, so a boss view's "Opaque(name, hex)" (a copy of game.mats.stone with _BaseColor = hex)
        /// on MeshKit's white vertices or a mesh with no vertex colour drew WHITE. With the row on (read at the view's build) the part takes URP Lit
        /// with _BaseColor = the intended hex (smoothness numbers.stoneLitSmooth); off: the stone copy as it was. One log line per view and part:
        /// "[light54] stone colour: &lt;view&gt; &lt;part&gt; intended #xxxxxx drawn lit|white (row off)".</summary>
        public static Material StoneOrLit(SurvivalGame game, Material stoneMat, string view, string part, uint hex)
        {
            bool on = game != null && On(game.cfg) && game.mats != null && game.mats.lit != null;
            string key = view + "|" + part + "|" + on;
            if (stoneLogged.Add(key)) Debug.Log($"[light54] stone colour: {view} {part} intended #{hex & 0xffffff:x6} drawn {(on ? "lit" : "white (row off: the stone shader's white vertex colour)")}");
            if (!on) return stoneMat;
            var m = new Material(game.mats.lit) { name = part + " (light54 lit)" };
            Color c = Bright71.Albedo(Look.Hex(hex)); // GG 2026-10-26: the boss arenas' near-black stone (THE ICEBREAKER's 0x241f2e pillars) gets an albedo floor
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
            if (m.HasProperty("_Color")) m.SetColor("_Color", c);
            if (m.HasProperty("_BaseMap")) m.SetTexture("_BaseMap", null);
            if (m.HasProperty("_MainTex")) m.SetTexture("_MainTex", null);
            if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", F(game.cfg, "stoneLitSmooth", 0.2));
            return m;
        }

        /// <summary>A frame-rate-free ease: the share of the way to a target covered in dt with time constant tau (s).</summary>
        public static float Ease(float dt, float tau) => tau <= 1e-5f ? 1f : 1f - Mathf.Exp(-dt / tau);

        /// <summary>The strobe rule (fix 4 and 10): a pulse at `hz` with depth `depth` (0..1 of its range); above the row's strobeHz it becomes a
        /// slow swell at swellHz with its depth x swellDepth. Returns the (hz, depth) to draw with the row on.</summary>
        public static void Calm(GameConfig cfg, ref double hz, ref double depth)
        {
            double max = Num(cfg, "strobeHz", 3);
            if (hz < max) return; // at the line counts (WCAG 2.3.1: three flashes a second)
            hz = Num(cfg, "swellHz", 1.2);
            depth *= Num(cfg, "swellDepth", 0.35);
        }
    }
}
