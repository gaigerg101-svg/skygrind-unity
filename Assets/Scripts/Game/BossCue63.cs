// NIGHT63 builder G (slate B2's measurement, first: "the boss63 reel lists every attack of the five bosses with its cue lead and cue hue"; the
// lead's order: measurement only, bossTell63 stays OFF, no sim change). The pure half of the cue measure (EditMode: N63BossCueTests); the Unity
// half is FeelReel.N63G.cs (V's boss63 partials Boss63EachG / Boss63EndG), which hands it, every frame, each renderer and light near the boss:
// whether it is drawn and the colour the view draws it with.
// THE METHOD (per attack, from BossAttackClock63's wind-up start W and hit H, ms on the reel's fixed clock):
//   - each object's REFERENCE: its highest drawn level (the drawn colour's max channel) in each hue class (danger, parry, pale, other) over the
//     frames in [W - Boss63CueRefMs, W - Boss63CueEarlyMs): what it already showed before the wind-up (a lamp that pulses, a hit flash);
//   - THE CUE'S START: the first frame in [W - Boss63CueEarlyMs, H) where some object is drawn in a hue class at a level above its reference in
//     that class by max(Boss63CueRiseAbs, Boss63CueRiseShare x the reference): it turned on, brightened or changed colour for this attack;
//   - THE CUE'S HUE: the strongest riser (the largest rise) at the frame where the summed rise peaks before the hit: its hue and saturation;
//     danger = that hue in the danger band (the colour table's: design.n63.colours63.numbers.bands, review S3; saturation >= Boss63CueMinSat);
//     parry = any riser in the table's parry yellow band;
//   - lead = H - start; no riser at all: not found, lead 0;
//   - THE TELL: the first frame with a riser in the danger or the parry class (B2's target: "a danger-hue cue >= 300 ms before its damage
//     frame"; the parry yellow for a parryable attack); tell lead = H - that frame. Short = no tell, or a tell lead < Boss63CueLeadMinMs (300 ms),
//     whatever an earlier off-palette riser did.
// What it does not see (said in the reel's line): screen pixels (the post stack, the shader's own use of the colour), particle systems, vertex
// colours. Pure: no UnityEngine.
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Skygrind.Game
{
    /// <summary>One drawn thing (a renderer or a light near the boss) in one frame: its id, whether it is drawn, the colour it is drawn with.</summary>
    public struct Lit63
    {
        public int id;
        public bool drawn;
        public float r, g, b;
    }

    /// <summary>One attack's cue as measured (BossCue63.Analyse).</summary>
    public struct Cue63
    {
        public bool found;
        public double startMs, hitMs, leadMs, hue, sat;
        public bool danger, parry;
        /// <summary>The tell: the first riser in the danger or the parry class (B2's target), its frame and its lead before the hit.</summary>
        public bool tellFound;
        public double tellStartMs, tellLeadMs;
        /// <summary>Objects that rose during the wind-up; the strongest riser's id.</summary>
        public int objects, strongest;
        /// <summary>NIGHT63 G B2e: the EARLIEST danger-hue cue, read on its own (the cue's hue above is the strongest riser's, which can be a
        /// pale glow brighter than the danger cue: n63-b9's crownFire clapper): the first frame from the wind-up with a riser in the danger class,
        /// its lead before the hit, the strongest danger riser's hue and id on that frame. It never changes the verdict.</summary>
        public bool dangerCueFound;
        public double dangerCueStartMs, dangerCueLeadMs, dangerCueHue;
        public int dangerCueId;
    }

    public static class BossCue63
    {
        public enum Hue63 : byte { None = 0, Danger, Parry, Pale, Other }

        /// <summary>The danger and parry hue bands (deg on the sRGB colour; lo > hi wraps through 0). Review S3 (Logs/n63/review-n63.md): ONE band
        /// set, the colour table's (design.n63.colours63.numbers.bands, read through Colours63Math.From), so B2's verdict, C1's role and B3's
        /// table call a pixel the same thing. `ok` false: no table (nothing is in a band).</summary>
        public struct Bands63
        {
            public bool ok;
            public double dangerLo, dangerHi, parryLo, parryHi;
        }

        static Bands63 bands;
        static Skygrind.Core.GameConfig bandsFor;

        /// <summary>The bands in use: the last config handed to Use (the reel and bossTell63 hand the game's every frame they measure or draw).</summary>
        public static Bands63 Bands => bands;

        /// <summary>Read the bands from this config's colour table (cached per config object); null leaves the bands as they are.</summary>
        public static Bands63 Use(Skygrind.Core.GameConfig cfg)
        {
            if (cfg == null || ReferenceEquals(cfg, bandsFor)) return bands;
            bandsFor = cfg;
            Colours63Math.Table t = Colours63Math.From(cfg);
            bands = t == null ? default : new Bands63 { ok = true, dangerLo = t.dangerLo, dangerHi = t.dangerHi, parryLo = t.parryLo, parryHi = t.parryHi };
            return bands;
        }

        /// <summary>Is `h` on the arc lo..hi (inclusive; lo > hi wraps through 0)? Colours63Math.RoleOf's rule.</summary>
        public static bool InArc(double h, double lo, double hi) => lo <= hi ? h >= lo && h <= hi : h >= lo || h <= hi;

        static readonly CultureInfo Ci = CultureInfo.InvariantCulture;

        /// <summary>HSV of an RGB colour (channels may pass 1: HDR gains): hue in degrees [0, 360), saturation 0..1, value the max channel.</summary>
        public static void Hsv(double r, double g, double b, out double h, out double s, out double v)
        {
            double max = Math.Max(r, Math.Max(g, b)), min = Math.Min(r, Math.Min(g, b)), d = max - min;
            v = max;
            s = max > 0 ? d / max : 0;
            if (d <= 1e-12) { h = 0; return; }
            if (max == r) h = 60 * (((g - b) / d) % 6);
            else if (max == g) h = 60 * ((b - r) / d + 2);
            else h = 60 * ((r - g) / d + 4);
            if (h < 0) h += 360;
            if (h >= 360) h -= 360;
        }

        /// <summary>In the danger band of the bands in use (the colour table's), saturated enough to read as a hue.</summary>
        public static bool DangerHue(double h, double s) => bands.ok && s >= UnityTuning.Boss63CueMinSat && InArc(h, bands.dangerLo, bands.dangerHi);

        /// <summary>In the parry band of the bands in use (the colour table's), saturated enough to read as a hue.</summary>
        public static bool ParryHue(double h, double s) => bands.ok && s >= UnityTuning.Boss63CueMinSat && InArc(h, bands.parryLo, bands.parryHi);

        /// <summary>A drawn colour's class: None (too dark), Danger, Parry, Pale (low saturation) or Other.</summary>
        public static Hue63 Class(double r, double g, double b)
        {
            Hsv(r, g, b, out double h, out double s, out double v);
            if (v < UnityTuning.Boss63CueMinLevel) return Hue63.None;
            if (s < UnityTuning.Boss63CueMinSat) return Hue63.Pale;
            if (DangerHue(h, s)) return Hue63.Danger;
            if (ParryHue(h, s)) return Hue63.Parry;
            return Hue63.Other;
        }

        static double Level(Lit63 x) => x.drawn ? Math.Max(x.r, Math.Max(x.g, x.b)) : 0;

        /// <summary>The attack's cue (see the file's head) with the short reference (Boss63CueRefMs before the wind-up). `ms` and `frames` in time
        /// order, the same length.</summary>
        public static Cue63 Analyse(IList<double> ms, IList<Lit63[]> frames, double windupMs, double hitMs) =>
            Analyse(ms, frames, windupMs, hitMs, windupMs - UnityTuning.Boss63CueRefMs);

        /// <summary>NIGHT63 G B2b: the attack's cue with the reference reaching back to `refFromMs` (the previous attack's hit; -infinity: none), at
        /// most Boss63CueRefLongMs and never less than Boss63CueRefMs before the wind-up: one cycle of a boss's ambient glow (the wyrm's 2.1 s molten
        /// seams, n63-b4's spit) is inside the reference, so its peak in a wind-up is no cue; a cue re-lit after the previous hit still rises.</summary>
        public static Cue63 Analyse(IList<double> ms, IList<Lit63[]> frames, double windupMs, double hitMs, double refFromMs)
        {
            double from = Math.Max(refFromMs, windupMs - UnityTuning.Boss63CueRefLongMs);
            from = Math.Min(from, windupMs - UnityTuning.Boss63CueRefMs);
            return AnalyseFrom(ms, frames, windupMs, hitMs, from);
        }

        /// <summary>NIGHT63 G B2b: what B2's fix list does with an attack's measured cue (the reel's cue line and Tools/night/n63-b2-fixlist.cjs).</summary>
        public enum Verdict63 { Ok, Short, OffHue }

        /// <summary>Short: no danger- or parry-hue tell >= Boss63CueLeadMinMs before the hit. OffHue: a cue found whose strongest riser is not in
        /// the danger band, on an attack the SIM does not mark parryable (`kind` not yellow): a parry-yellow cue on a dodge-only attack is off the
        /// danger hue (DECIDE63 D-1; n63-b4's crown beamCut / lowFire). Else Ok.</summary>
        public static Verdict63 Verdict(Cue63 c, string kind) => Short(c) ? Verdict63.Short : c.found && !c.danger && kind != "yellow" ? Verdict63.OffHue : Verdict63.Ok;

        static Cue63 AnalyseFrom(IList<double> ms, IList<Lit63[]> frames, double windupMs, double hitMs, double refFrom)
        {
            var c = new Cue63 { hitMs = hitMs }; // strongest: an instance id, negative for run-time objects: read only when found
            if (ms == null || frames == null) return c;
            int n = Math.Min(ms.Count, frames.Count);
            double refTo = windupMs - UnityTuning.Boss63CueEarlyMs;
            // each object's reference: its highest level per hue class before the wind-up
            var refMax = new Dictionary<int, double[]>();
            for (int i = 0; i < n; i++)
            {
                if (ms[i] < refFrom || ms[i] >= refTo || frames[i] == null) continue;
                foreach (Lit63 x in frames[i])
                {
                    Hue63 k = x.drawn ? Class(x.r, x.g, x.b) : Hue63.None;
                    if (k == Hue63.None) continue;
                    if (!refMax.TryGetValue(x.id, out double[] m)) refMax[x.id] = m = new double[5];
                    m[(int)k] = Math.Max(m[(int)k], Level(x));
                }
            }
            var risers = new HashSet<int>();
            double bestSum = 0;
            for (int i = 0; i < n; i++)
            {
                if (ms[i] < refTo || ms[i] >= hitMs || frames[i] == null) continue;
                double sum = 0, top = 0;
                int topId = 0;
                bool haveTop = false; // n63-b2's red: run-time instance ids are negative, so "no riser" is this flag, never a negative id
                Lit63 topX = default;
                bool haveDanger = false; // NIGHT63 G B2e: this frame's strongest danger-class riser (the earliest danger cue's frame)
                double dangerTop = 0;
                Lit63 dangerX = default;
                foreach (Lit63 x in frames[i])
                {
                    Hue63 k = x.drawn ? Class(x.r, x.g, x.b) : Hue63.None;
                    if (k == Hue63.None) continue;
                    double lv = Level(x), was = refMax.TryGetValue(x.id, out double[] m) ? m[(int)k] : 0;
                    double rise = lv - was;
                    if (rise < Math.Max(UnityTuning.Boss63CueRiseAbs, UnityTuning.Boss63CueRiseShare * was)) continue;
                    risers.Add(x.id);
                    sum += rise;
                    if (!haveTop || rise > top) { haveTop = true; top = rise; topId = x.id; topX = x; }
                    if (k == Hue63.Parry) c.parry = true;
                    if ((k == Hue63.Danger || k == Hue63.Parry) && !c.tellFound) { c.tellFound = true; c.tellStartMs = ms[i]; }
                    if (k == Hue63.Danger && (!haveDanger || rise > dangerTop)) { haveDanger = true; dangerTop = rise; dangerX = x; }
                }
                if (haveDanger && !c.dangerCueFound)
                {
                    c.dangerCueFound = true;
                    c.dangerCueStartMs = ms[i];
                    c.dangerCueLeadMs = hitMs - ms[i];
                    c.dangerCueId = dangerX.id;
                    Hsv(dangerX.r, dangerX.g, dangerX.b, out double dh, out double _, out double _);
                    c.dangerCueHue = dh;
                }
                if (!haveTop) continue;
                if (!c.found) { c.found = true; c.startMs = ms[i]; }
                if (sum > bestSum)
                {
                    bestSum = sum;
                    c.strongest = topId;
                    Hsv(topX.r, topX.g, topX.b, out double h, out double s, out double _);
                    c.hue = h;
                    c.sat = s;
                }
            }
            c.objects = risers.Count;
            if (c.found)
            {
                c.leadMs = hitMs - c.startMs;
                c.danger = DangerHue(c.hue, c.sat);
            }
            if (c.tellFound) c.tellLeadMs = hitMs - c.tellStartMs;
            return c;
        }

        /// <summary>No danger- or parry-hue tell, or one that starts under Boss63CueLeadMinMs before the hit (the slate's B2 floor).</summary>
        public static bool Short(Cue63 c) => !c.tellFound || c.tellLeadMs < UnityTuning.Boss63CueLeadMinMs;

        static string Ms(double v) => Math.Round(v, MidpointRounding.AwayFromZero).ToString("0", Ci);

        /// <summary>"[boss63] cue &lt;boss&gt; &lt;attack&gt; start &lt;ms|none&gt; hit &lt;ms&gt; lead &lt;ms&gt; hue &lt;deg|-&gt; sat &lt;s|-&gt; parry &lt;yes|no&gt; danger &lt;yes|no&gt;
        /// kind &lt;the sim's tell kind&gt; objects &lt;n&gt; obj &lt;the strongest riser&gt;" (ms from the rise on the reel's fixed clock).</summary>
        public static string CueLine(string boss, string attack, Cue63 c, string kind, string what, string dangerWhat = null) =>
            "[boss63] cue " + (boss ?? "?") + " " + (attack ?? "?") + " start " + (c.found ? Ms(c.startMs) : "none") + " hit " + Ms(c.hitMs)
            + " lead " + (c.found ? Ms(c.leadMs) : "0") + " hue " + (c.found ? Ms(c.hue) : "-") + " sat " + (c.found ? c.sat.ToString("0.00", Ci) : "-")
            + " parry " + (c.parry ? "yes" : "no") + " danger " + (c.found && c.danger ? "yes" : "no")
            + " tell " + (c.tellFound ? Ms(c.tellLeadMs) : "none") + " kind " + (kind ?? "?")
            + " verdict " + (Verdict(c, kind) == Verdict63.Short ? "short" : Verdict(c, kind) == Verdict63.OffHue ? "off-hue" : "ok")
            + " objects " + c.objects + " obj " + (string.IsNullOrEmpty(what) ? "-" : what)
            // NIGHT63 G B2e: the earliest danger-hue cue on its own, appended (the fields above, the verdict and the fix-list tool's parse unchanged)
            + (c.dangerCueFound
                ? " | danger cue start " + Ms(c.dangerCueStartMs) + " lead " + Ms(c.dangerCueLeadMs) + " hue " + Ms(c.dangerCueHue) + " obj " + (string.IsNullOrEmpty(dangerWhat) ? "-" : dangerWhat)
                : " | danger cue none");

        /// <summary>"[boss63] cues &lt;boss&gt; attacks &lt;n&gt; found &lt;f&gt; | under 300 ms &lt;k&gt; (ids) | off the danger hue &lt;m&gt; (ids) | parry drawn &lt;p&gt; of
        /// &lt;y&gt; yellow | method ...": the fix list for B2 (a parryable attack's yellow cue is not counted off the danger hue).</summary>
        public static string EndLine(string boss, int attacks, int found, List<string> shortIds, List<string> offHueIds, int parryDrawn, int parryKind)
        {
            shortIds = shortIds ?? new List<string>();
            offHueIds = offHueIds ?? new List<string>();
            return "[boss63] cues " + (boss ?? "?") + " attacks " + attacks + " found " + found
                + " | under " + Ms(UnityTuning.Boss63CueLeadMinMs) + " ms " + shortIds.Count + " (" + string.Join(", ", shortIds) + ")"
                + " | off the danger hue " + offHueIds.Count + " (" + string.Join(", ", offHueIds) + ")"
                + " | parry drawn " + parryDrawn + " of " + parryKind + " yellow"
                + " | bands " + (bands.ok ? "danger " + Ms(bands.dangerLo) + "-" + Ms(bands.dangerHi) + " parry " + Ms(bands.parryLo) + "-" + Ms(bands.parryHi) + " (colours63's table)" : "none (no colour table)")
                + " | method: each renderer's and light's drawn colour as the view set it (renderer: its MaterialPropertyBlock _BaseColor / _EmissionColor / _Color, else its material's; light: colour x intensity) within "
                + UnityTuning.Boss63CueRadiusM.ToString("0", Ci) + " m of the boss, not screen pixels; particles and vertex colours not read";
        }
    }
}
