// NIGHT72 builder L (L3; design.n72.lit72, a view row: NightRows.ViewOn(cfg, "lit72") every frame, never `enabled` written here): the boss arenas and
// level 5 LIFTED INSIDE EACH BOSS'S OWN HUE. GG 2026-10-26: "a lot of the room especially the boss rooms are very dark" (asked three times); n71-forge72's
// Player frames: THE KILN WYRM's arena 47 % of its pixels under luma 30 at idle, THE CROWN's 35-43 %, the spire's rooms 23-30 % (Bright71's ambient
// and albedo floors are already in those frames: the walls and ceilings are red-brown or green-black, R 50 / G 11 / B 6 on the wyrm's, which no ambient
// floor can lift past luma 30 without a 2x gain).
// The lift is a display-space shadow lift in Crt29.shader (its Lit72 block, after the scanlines, before the quantise so the 32 levels stay even steps,
// and before the grain so the grain is not amplified): per pixel
//   w = 1 - smoothstep(l0, l1, l);  k = min(kmax, (l + A * w) / l);  rgb' = rgb * k
// with l the pixel's display luma (0.299 / 0.587 / 0.114, the weights of the "dark < 30" measure). The colour is SCALED, never replaced, so each
// arena keeps its own palette (the wyrm's ember red, THE CROWN's green and violet, the spire's olive); a pixel at l1 or over is untouched, so the
// lights, the tells, the boss and the player's cyan keep their value. Tools/night/n72-lit-sim.py is the same maths run on the existing frames; this
// file's Lit72Math is its C# twin (Lit72Tests pins the two together).
// WHERE: a campaign boss room (Campaign24.InBossRoom), every room of a spire level (LightPlan33.Theme == 4), and any live boss grade (grade54 /
// grade55's weight, the survival boss fights). The weight fades over UnityTuning.L72LitFadeInS / FadeOutS (no pop). It is computed every frame from
// the room whether the row is on or not, so the frame the row turns ON is the frame it would have drawn (D1's contract).
// Off: -skygrindNoLit72, or design.n72.lit72.enabled false (the lead flips it ON after the pair, the SPOT and the guard run).
// Tuning without a rebuild (the lead's retake): -skygrindLit72Tune=A,l0,l1,kmax (any prefix of the list; the rest stay UnityTuning's).
using System;
using System.Globalization;
using Skygrind.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace Skygrind.Game
{
    /// <summary>lit72's pure maths (no scene read): the gain, the weight's target and its fade.</summary>
    public static class Lit72Math
    {
        public static float SmoothStep(float e0, float e1, float x)
        {
            float t = Math.Max(0f, Math.Min(1f, (x - e0) / (e1 - e0)));
            return t * t * (3f - 2f * t);
        }

        /// <summary>The display luma of a colour (the "dark &lt; 30" measure's weights).</summary>
        public static float Luma(float r, float g, float b) => 0.299f * r + 0.587f * g + 0.114f * b;

        /// <summary>The gain a pixel of display luma `luma` is scaled by: its own luma plus A x w, over itself, capped at kmax (1 = left alone).</summary>
        public static float Gain(float luma, float a, float l0, float l1, float kmax)
        {
            float w = 1f - SmoothStep(l0, l1, luma);
            return Math.Min(kmax, (luma + a * w) / Math.Max(luma, 1e-4f));
        }

        /// <summary>The lifted channel: the channel scaled by the gain and clamped to 1 (a gain of 1 leaves a value over 1 alone).</summary>
        public static float Lift(float channel, float gain) => gain > 1.0001f ? Math.Min(1f, channel * gain) : channel;

        /// <summary>The weight the lift is heading to: 1 in a boss room, in a spire level's rooms, or under a live boss grade; else 0.</summary>
        public static float Target(bool bossRoom, int theme, int spireTheme, float gradeWeight)
            => Math.Max(Math.Max(bossRoom ? 1f : 0f, theme == spireTheme ? 1f : 0f), Math.Max(0f, Math.Min(1f, gradeWeight)));

        /// <summary>One frame's step of the weight toward its target (linear, `fadeInS` up and `fadeOutS` down; a fade of 0 jumps).</summary>
        public static float Step(float weight, float target, float dt, float fadeInS, float fadeOutS)
        {
            if (target > weight) return fadeInS <= 0f ? target : Math.Min(target, weight + dt / fadeInS);
            return fadeOutS <= 0f ? target : Math.Max(target, weight - dt / fadeOutS);
        }
    }

    /// <summary>lit72's live state: ticked once a frame by Crt29Driver.Update, read by Crt29Pass for the shader's _Lit72.</summary>
    public static class Lit72
    {
        public const string Row = "lit72";
        public const string TuneSwitch = "-skygrindLit72Tune";
        public const string ProbeSwitch = "-skygrindLit72Probe";

        /// <summary>The weight (0-1) the room asks for, faded; independent of the row so the first ON frame is the frame it would have drawn.</summary>
        public static float Weight { get; private set; }

        /// <summary>The row is ON and the weight is above 0: the pass draws the lift.</summary>
        public static bool Live { get; private set; }

        static float a = UnityTuning.L72LitA, l0 = UnityTuning.L72LitL0, l1 = UnityTuning.L72LitL1, kmax = UnityTuning.L72LitKMax, probeK = UnityTuning.L72LitProbeK;
        static bool probeFloor = UnityTuning.L72LitProbeFloor;
        static float probeFloorMul = 1f;
        static bool tuneRead;
        static SurvivalMap themeMap;
        static int themeOf = -1;

        /// <summary>The numbers in use: A, l0, l1, kmax (UnityTuning's, or -skygrindLit72Tune's).</summary>
        public static float[] Tuned => new[] { a, l0, l1, kmax };

        /// <summary>The shader's _Lit72: x the luma added at black (A x the weight; 0 = off), y l0, z l1, w kmax.</summary>
        public static Vector4 Vector => Live ? new Vector4(a * Weight, l0, l1, kmax) : Vector4.zero;

        /// <summary>This frame: the weight's target from where the player is, one step toward it, the row read now.</summary>
        public static void Tick(SurvivalGame g)
        {
            if (g == null || g.cfg == null) { Weight = 0; Live = false; return; }
            if (!tuneRead) { tuneRead = true; ReadTune(Environment.GetCommandLineArgs()); }
            bool campaign = g.sim != null && g.sim.campaignOn && g.sim.campaign != null;
            bool bossRoom = campaign && g.sim.campaign.InBossRoom;
            if (!ReferenceEquals(g.map, themeMap)) { themeMap = g.map; themeOf = LightPlan33.Theme(g.map?.json); } // once per map: Theme lower-cases the name (no per-frame garbage)
            int theme = campaign ? themeOf : -1;
            float grade = Math.Max(BossGrade54.WeightNow, BossGrade55.WeightNow);
            float target = Lit72Math.Target(bossRoom, theme, UnityTuning.L72LitSpireTheme, grade);
            Weight = Lit72Math.Step(Weight, target, (float)FeelClock.FrameDelta, UnityTuning.L72LitFadeInS, UnityTuning.L72LitFadeOutS);
            Live = NightRows.ViewOn(g.cfg, Row) && Weight > 0f && a * Weight >= UnityTuning.L72LitMinA;
            Probe(Live ? Weight : 0f);
        }

        // ---- the ambient probe (an experiment, default off) ----
        // Stone.shader lights with SampleSH, the AMBIENT PROBE, and PostFx.Probe33 writes that probe once per theme from the theme's trilight; nothing recomputes
        // it at runtime (PostFx.cs's own note), so Bright71's lifts of RenderSettings.ambientSky / Equator / GroundColor do NOT reach the stone: its boss-arena
        // floors are inert there. Scaling the probe is the physical lift (the walls lit by ambient x albedo, the relief kept), but its result on the frame is not
        // something the existing frames can size: -skygrindLit72Probe=<k> (k > 1) scales every coefficient by 1 + (k - 1) x the weight, held over whatever
        // wrote the probe (the written one is the base again the moment it differs from ours). Default UnityTuning.L72LitProbeK = 1: untouched.
        static SphericalHarmonicsL2 probeBase, probeApplied;
        static bool probeHeld;

        // -skygrindLit72Probe=floor: the probe is built from Bright71's own floors (UnityTuning.Bright71Ambient*: the room floors, the boss floors at full weight), keeping the trilight's hue,
        // with PostFx.Probe33's construction (the middle everywhere, the top and bottom as one up / down lobe), and blended in by the weight. That is what Bright71 meant its floors to do for the
        // stone and never did: THE LOOM's and the iceline boss's walls are (3, 3, 11) because the probe is the theme's 0.03-0.12 and their albedo is 0.1; a display lift cannot take luma 1 over 30
        // (lit72-sim-other-bosses.txt), the ambient at the boss floor (linear 0.28 at the equator) can.
        static SphericalHarmonicsL2 FloorProbe(float weight)
        {
            float skyF = Mathf.Lerp(UnityTuning.Bright71AmbientSky, UnityTuning.Bright71BossAmbientSky, weight);
            float eqF = Mathf.Lerp(UnityTuning.Bright71AmbientEquator, UnityTuning.Bright71BossAmbientEquator, weight);
            float grF = Mathf.Lerp(UnityTuning.Bright71AmbientGround, UnityTuning.Bright71BossAmbientGround, weight);
            Color sky, eq, gr;
            if (RenderSettings.ambientMode == AmbientMode.Trilight) { sky = RenderSettings.ambientSkyColor; eq = RenderSettings.ambientEquatorColor; gr = RenderSettings.ambientGroundColor; }
            else { sky = eq = gr = RenderSettings.ambientLight; }
            var sh = new SphericalHarmonicsL2();
            Color e = Bright71.Lift(eq, eqF).linear;
            sh.AddAmbientLight(e);
            sh.AddDirectionalLight(Vector3.up, (Bright71.Lift(sky, skyF).linear - Bright71.Lift(gr, grF).linear) * 0.5f, 1f);
            return sh;
        }

        static void Probe(float weight)
        {
            float k = 1f + (probeK - 1f) * weight;
            bool lifting = (probeFloor && weight > 0.0005f) || k > 1.0001f;
            SphericalHarmonicsL2 now = RenderSettings.ambientProbe;
            bool ours = probeHeld && now == probeApplied;
            if (!lifting)
            {
                if (ours) RenderSettings.ambientProbe = probeBase;
                probeHeld = false;
                return;
            }
            if (!ours) probeBase = now; // someone else wrote it (a theme was applied): that is the base
            float fw = weight * probeFloorMul;
            SphericalHarmonicsL2 want = probeFloor ? probeBase * (1f - fw) + FloorProbe(weight) * fw : probeBase * k;
            if (!(now == want)) RenderSettings.ambientProbe = want;
            probeApplied = want;
            probeHeld = true;
        }

        /// <summary>-skygrindLit72Tune=A,l0,l1,kmax (or the list as the next argument): a prefix of the four numbers replaces UnityTuning's, the rest stay.</summary>
        public static void ReadTune(string[] args)
        {
            a = UnityTuning.L72LitA; l0 = UnityTuning.L72LitL0; l1 = UnityTuning.L72LitL1; kmax = UnityTuning.L72LitKMax; probeK = UnityTuning.L72LitProbeK; probeFloor = UnityTuning.L72LitProbeFloor; probeFloorMul = 1f;
            if (args == null) return;
            for (int i = 0; i < args.Length; i++)
            {
                string v = null;
                if (args[i] != null && args[i].StartsWith(ProbeSwitch + "=", StringComparison.OrdinalIgnoreCase)
                    && args[i].Substring(ProbeSwitch.Length + 1).StartsWith("floor", StringComparison.OrdinalIgnoreCase))
                {
                    // floor, or floor:<m>: m (0..1, default 1) scales the weight the floor probe is blended in by (a quarter-strength try: floor:0.25)
                    string rest = args[i].Substring(ProbeSwitch.Length + 1 + 5);
                    probeFloor = true;
                    probeFloorMul = 1f;
                    if (rest.StartsWith(":") && float.TryParse(rest.Substring(1), NumberStyles.Float, CultureInfo.InvariantCulture, out float fm)) probeFloorMul = Mathf.Clamp01(fm);
                    Debug.Log("[lit72] ambient probe from Bright71's floors in boss rooms and level 5, blended at x" + probeFloorMul.ToString("0.##", CultureInfo.InvariantCulture) + " of the weight (" + ProbeSwitch + "=floor[:m])");
                    continue;
                }
                if (args[i] != null && args[i].StartsWith(ProbeSwitch + "=", StringComparison.OrdinalIgnoreCase)
                    && float.TryParse(args[i].Substring(ProbeSwitch.Length + 1), NumberStyles.Float, CultureInfo.InvariantCulture, out float pk))
                {
                    probeK = Math.Max(1f, pk);
                    Debug.Log("[lit72] ambient probe x" + probeK.ToString("0.##", CultureInfo.InvariantCulture) + " in boss rooms and level 5 (" + ProbeSwitch + ")");
                    continue;
                }
                if (args[i] != null && args[i].StartsWith(TuneSwitch + "=", StringComparison.OrdinalIgnoreCase)) v = args[i].Substring(TuneSwitch.Length + 1);
                else if (string.Equals(args[i], TuneSwitch, StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length) v = args[i + 1];
                if (v == null) continue;
                string[] p = v.Split(',');
                float[] t = { a, l0, l1, kmax };
                for (int k = 0; k < p.Length && k < 4; k++)
                    if (float.TryParse(p[k], NumberStyles.Float, CultureInfo.InvariantCulture, out float f)) t[k] = f;
                a = t[0]; l0 = t[1]; l1 = Math.Max(t[2], t[1] + 1e-3f); kmax = Math.Max(1f, t[3]);
                Debug.Log("[lit72] tune A " + a.ToString("0.###", CultureInfo.InvariantCulture) + " l0 " + l0.ToString("0.###", CultureInfo.InvariantCulture) + " l1 " + l1.ToString("0.###", CultureInfo.InvariantCulture) + " kmax " + kmax.ToString("0.##", CultureInfo.InvariantCulture) + " (" + TuneSwitch + ")");
            }
        }
    }
}
