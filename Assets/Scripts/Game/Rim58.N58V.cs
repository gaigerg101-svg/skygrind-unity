// NIGHT58 builder V (slate V1 rim56 on foundry; V2 yellow56; their off switches). View code only: nothing here reaches the sim.
// THE CAUSE (Tools/night/n58-v-rimcause.py on NIGHT57's kept a357 stills, Logs/feel/n57-r3/v1on-foundry): on foundry the environment's p90 V
// (0.68-0.71) is set by colour56's ACCENT, not by lit walls: the env pixels at or over that p90 are hue 260-270 (the foundry accent, 263) on
// 43-94 % of them, 55-95 % in the upper half of the frame (the decor54 rims and trims Colour56.Clamp recolours), S median 0.64; with colour56
// off (v1none) the same rooms' env p90 V is 0.48-0.67. The bodies sit at V 0.59-0.70 (S 0.80-0.95, over the env's 0.70 already): short by
// V 0.007-0.21. rim56's lit gain lifts a body only +0.03-0.11 (on vs off, the same shots): the frame's top is the accent, so the lever is the
// accent's value. Simulated on the same stills (n58-v-rimsim.py, the meter's own measure): the accent's value x 0.75 -> body over env p90
// 58 / 77 (75 %) -> 70 / 77 (91 %); foundry 4 / 14 -> 13 / 14, kiln 11 -> 14 / 15; iceline / span / spire unchanged.
// THE FIX (behind rim56): while rim56 is on, Colour56.Clamp's largest channel x UnityTuning.Rim58AccentScale (hue and saturation kept; the
// world's luminance only lower, so the greyscale guard cannot get worse). -skygrindOff58 (or older): NIGHT57's rim56 exactly.
// THE OFF SIDE without -skygrindOff56 (which also takes colour56 and tokens56): `-skygrindNoRim56` / `-skygrindNoYellow56` turn off only
// that row (its whole effect: rim56's gains and this accent value; yellow56's guns, floors, accent and dim yellow). For the r1 reels.
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public static class Rim58
    {
        static int off58 = -1;

        /// <summary>-skygrindOff58 or lower (or the process switch): NIGHT57's rim56 / yellow56 exactly.</summary>
        public static bool Off58()
        {
            if (NightRows.ForceOffFrom <= 58) return true;
            if (off58 < 0)
            {
                try { off58 = NightRows.OffFrom(System.Environment.GetCommandLineArgs(), "-skygrindOff") <= 58 ? 1 : 0; }
                catch (System.Exception) { off58 = 0; }
            }
            return off58 == 1;
        }

        static string[] args;

        /// <summary>A one-row off switch on the command line (`-skygrindNoRim56`, `-skygrindNoYellow56`): the row off, every other row as built.</summary>
        public static bool No(string flag)
        {
            if (args == null)
            {
                try { args = System.Environment.GetCommandLineArgs() ?? new string[0]; }
                catch (System.Exception) { args = new string[0]; }
            }
            return NightRows.HasSwitch(args, flag); // NIGHT63 R1 N5: the one rule (case ignored, as before)
        }

        /// <summary>For the a358 line: the accent's largest channel was scaled by rim56 (count of pieces since the map read).</summary>
        public static int AccentPieces;

        /// <summary>Cap57Pass's hook (slate V2 yellow56): the dim-yellow rule's value bounds from design.n56.yellow56.numbers.dimYellow58 (vFull,
        /// vNone) unless -skygrindOff58 (NIGHT57's dimYellow): the lava edge's and the plate's V ~0.81 under the cap, the parry tells (V >= 0.90) not.</summary>
        public static Vector4 Yellow58(GameConfig cfg, Vector4 y)
        {
            const string Y = "design.n56.yellow56.numbers.dimYellow58.";
            if (Off58() || cfg == null || !cfg.Has(Y + "vFull") || !cfg.Has(Y + "vNone")) return y;
            return new Vector4(y.x, (float)cfg.Num(Y + "vFull"), (float)cfg.Num(Y + "vNone"), y.w);
        }

        /// <summary>Colour56.Clamp's hook: the accent's largest channel while rim56 is on (NIGHT58), else mc as it came.</summary>
        public static float AccentMax(GameConfig cfg, float mc)
        {
            if (Off58() || !Rim56.On(cfg)) return mc;
            AccentPieces++;
            return mc * UnityTuning.Rim58AccentScale;
        }
    }
}
