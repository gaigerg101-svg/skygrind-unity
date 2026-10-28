// NIGHT72 builder L (L2; design.n72.fix72, a view row: NightRows.ViewOn(cfg, "fix72") every frame, never `enabled` written here): the temporal-flicker
// sources the L1 meter can rank, switchable ONE AT A TIME for the ranking, and the fixes behind the row.
// GG 2026-10-26: "there's flickers"; 2026-10-20: "flickering bugs that happen constantly". What a STILL camera can show (the meter's still72 phase), by
// reading the pipeline (Research/2026-10-27-wave72/L/flicker-rank.md has the ranking once the Player has run; until then these are hypotheses):
//   grain      URP FilmGrain (PostFx; quality.presets.*.grain 0.035 / 0.045): a new random offset EVERY frame (URP's FilmGrain pass), BEFORE the CRT pass; the
//              CRT pass then quantises to 32 levels (Look35Levels): one step is 8.2 levels, so a pixel within the grain's +-1.5 levels of a step edge swaps to the
//              next step and back by 8 levels, over 6: about 3.2 / 8.2 = 39 % of a flat gradient flips by a whole colour step. fix72: the film grain is off while
//              the quantise is live (the quantiser has its own fixed Bayer dither).
//   crtgrain   Crt29.shader's own grain: re-seeded at design.n29.crt.grainHz (24 Hz), ADDED IN LINEAR LIGHT: +-0.006 at the default 0.012, which the sRGB curve
//              turns into +-6 to +-10 levels in a dark pixel (slope 6 at linear 0.01) and +-1.5 on a bright one. Dark rooms are where GG sees it. fix72: the same
//              grain added in display (sRGB) space: a constant +-1.5 levels, under the meter's 6-level swing, dark or bright.
//   bloom      Bloom without High Quality Filtering ("reduces flicker", URP manual): thin bright strips lose and gain energy through the downscale chain as the
//              view moves. fix72: High Quality Filtering on.
//   ssao       URP's SSAO blue noise moves every frame (Ssao55.cs's finding); the harvest row ssao55 freezes it. Here only the diagnostic (the AO off).
//   quant      the 32-level quantise (the PS2 colour steps) itself: switched off to show how much of the rest it turns into whole-step flips.
//   scan, chroma  the CRT's scanlines and red / blue split.
// THE DIAGNOSTIC SWITCH (a reel's, never the game's): -skygrindFlickerOff72=<source>[+<source>...] turns each named source OFF for the run, so one build
// ranks them: a still72 side per source, the meter's flip share against the plain side. It reads the command line once. fix72 does NOT read it.
// What fix72 changes is only the three rows above (grain, crtgrain, bloom); each is its own line, so the lead can drop one with -skygrindNoFix72 (all).
using System;

namespace Skygrind.Game
{
    public static class Fix72
    {
        public const string Row = "fix72";
        public const string DiagSwitch = "-skygrindFlickerOff72";
        /// <summary>The names -skygrindFlickerOff72 knows.</summary>
        public static readonly string[] Sources = { "grain", "crtgrain", "bloom", "ssao", "quant", "scan", "chroma" };

        static string[] off;

        /// <summary>The sources named on the command line (read once; none: empty).</summary>
        public static string[] OffList
        {
            get
            {
                if (off == null) off = Parse(Environment.GetCommandLineArgs());
                return off;
            }
        }

        /// <summary>The sources of `args`' -skygrindFlickerOff72=a+b (or the list as the next argument): only the names in Sources, lower-cased.</summary>
        public static string[] Parse(string[] args)
        {
            var list = new System.Collections.Generic.List<string>();
            if (args != null)
                for (int i = 0; i < args.Length; i++)
                {
                    string v = null;
                    if (args[i] != null && args[i].StartsWith(DiagSwitch + "=", StringComparison.OrdinalIgnoreCase)) v = args[i].Substring(DiagSwitch.Length + 1);
                    else if (string.Equals(args[i], DiagSwitch, StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length) v = args[i + 1];
                    if (v == null) continue;
                    foreach (string s in v.Split('+', ','))
                    {
                        string n = s.Trim().ToLowerInvariant();
                        if (Array.IndexOf(Sources, n) >= 0 && !list.Contains(n)) list.Add(n);
                    }
                }
            return list.ToArray();
        }

        /// <summary>Is this source switched off by the diagnostic?</summary>
        public static bool Off(string source) => Array.IndexOf(OffList, source) >= 0;

        /// <summary>For tests: forget the parsed list (the next Off / OffList reads the command line again, or `args`).</summary>
        public static void Reset(string[] args = null) => off = args != null ? Parse(args) : null;
    }
}
