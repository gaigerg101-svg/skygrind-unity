// NIGHT59 builder V (slate V2 yellow56's halo). View code only: nothing here reaches the sim.
// THE CAUSE (Research/2026-10-21-wave58/gate-x.md, the critic's FAIL): with NIGHT58's dimYellow58 bounds (whole at V <= 0.84, none from 0.89) the
// foundry a5 husk's parry halo (S 0.99, V 0.84 on the a358 OFF line) sits on vFull, so the dim-yellow cap takes it whole: 20 % of its yellow
// pixels kept (the same-instant -nocap twin 4558 -> 918 px). Value cannot split it from what must stay capped (the lava trench's edge V 0.81,
// the column top plate V 0.82); saturation can (the halo S 0.99, the lava edge 0.64, the plate 0.56).
// THE FIX: design.n56.yellow56.numbers.dimYellow58.sKeep (Tools/night/n59-v-rows.cjs): a yellow at or over that saturation is untouched by the
// dim-yellow rule when its value is at least dimYellow58.sKeepVMin. Handed to Cap57.shader as _Cap57YH.w and _Cap57.w (YellowArc.w, VMin;
// Cap57Math.Apply's y[6], y[7]); 0 = no keep bound, which is NIGHT58's rule exactly. Off: -skygrindOff59 or lower (or the process switch), or no
// sKeep in the config. vFull / vNone unchanged. The floor: simulated on the n58-freeze a358 -nocap twins, the keep bound alone (S 0.90) let
// spire's near-black olive through (S 1.0 at V 0.18-0.19; spire v6-mid non-parry 0.05 -> 0.72 %); a floor of V 0.30 holds it (0.10 %).
// The planted case: python -I Tools/night/n59-v-yellow.py planted. The halo's meter: python -I Tools/night/n59-v-halo.py.
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public static class Yellow59
    {
        static int off59 = -1;

        /// <summary>-skygrindOff59 or lower (or the process switch): NIGHT58's yellow56 exactly (no keep bound).</summary>
        public static bool Off59()
        {
            if (NightRows.ForceOffFrom <= 59) return true;
            if (off59 < 0)
            {
                try { off59 = NightRows.OffFrom(System.Environment.GetCommandLineArgs(), "-skygrindOff") <= 59 ? 1 : 0; }
                catch (System.Exception) { off59 = 0; }
            }
            return off59 == 1;
        }

        /// <summary>Cap57.ForMap's hook: the yellow arc (lo, hi, feather, w) with w = dimYellow58.sKeep (the keep bound), or 0 (none) when
        /// -skygrindOff59 / -skygrindOff58 or lower, or the config has no sKeep.</summary>
        public static Vector4 Keep(GameConfig cfg, Vector4 arc)
        {
            const string K = "design.n56.yellow56.numbers.dimYellow58.sKeep";
            const string KV = "design.n56.yellow56.numbers.dimYellow58.sKeepVMin";
            arc.w = 0f;
            VMin = 0f;
            if (Off59() || Rim58.Off58() || cfg == null || !cfg.Has(K)) return arc;
            arc.w = (float)cfg.Num(K);
            if (cfg.Has(KV)) VMin = (float)cfg.Num(KV);
            return arc;
        }

        /// <summary>The keep bound's value floor this map (dimYellow58.sKeepVMin; Cap57.shader's _Cap57.w; 0 = none): a saturated yellow is kept
        /// only at or over this value (spire's near-black olive reads S 1.0 at V 0.18-0.19: a dim yellow, capped).</summary>
        public static float VMin;

        /// <summary>For the reel's yellow line (Cap57.YellowLine): the keep bound when there is one.</summary>
        public static string LineOf(Vector4 arc) => arc.w > 0f ? $"; a yellow at S {arc.w:0.00} or over and V {VMin:0.00} or over kept (NIGHT59 sKeep)" : "; no keep bound (NIGHT58's rule)";
    }
}
