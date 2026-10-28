// NIGHT20 E2 (builder P; Research/2026-10-06-additions-backlog.md E2): ASSIST, design.adds.assist (Unity-only: the web has
// no assist mode). Hades' God Mode (Supergiant, 2020): damage resistance that starts at 20 % and grows 2 % with each death,
// capped at 80 %, for players who would otherwise quit; Hades keeps it a choice in the settings and marks nothing else.
// Here a run with it on is stamped (no personal best, no contracts: as the LAB) and RESULTS says so. The rule only
// (Game/Assist20Run.cs applies it to a run, SETTINGS > ACCESS > ASSIST turns it on):
//   Resistance(downs) = min(cap, start + perDown x downs)   (0 with the block off)
//   the damage taken x (1 - Resistance)                       (Health.takenMult: HEAT's GLASS CANNON composes)
using System;
using System.Globalization;

namespace Skygrind.Core
{
    public static class Assist20
    {
        /// <summary>design.adds.assist (every number 0 and off when the block is absent).</summary>
        public struct P
        {
            public bool enabled;
            public double start, perDown, cap;

            public static P From(GameConfig cfg)
            {
                if (!cfg.Has("design.adds.assist.start")) return default;
                return new P
                {
                    enabled = cfg.Design("adds.assist.enabled"),
                    start = cfg.Num("design.adds.assist.start"),
                    perDown = cfg.Num("design.adds.assist.perDown"),
                    cap = cfg.Num("design.adds.assist.cap"),
                };
            }
        }

        /// <summary>The resistance after `downs` downs with ASSIST on: min(cap, start + perDown x downs); 0 when off.</summary>
        public static double Resistance(in P p, int downs)
        {
            if (!p.enabled) return 0;
            return Math.Min(p.cap, p.start + p.perDown * Math.Max(0, downs));
        }

        /// <summary>What the damage taken is multiplied by: 1 - Resistance.</summary>
        public static double TakenMult(in P p, int downs) => 1 - Resistance(p, downs);

        /// <summary>The run's stamp ("ASSIST 24%"): RESULTS' line, the run log.</summary>
        public static string Stamp(double resistance) => "ASSIST " + Math.Round(resistance * 100).ToString("0", CultureInfo.InvariantCulture) + "%";
    }
}
