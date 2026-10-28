// NIGHT40 (the shared commit, the lead): the ten sim-changing NIGHT40 design blocks a run reads, one bit each, so every NIGHT40 row can
// be switched off (NIGHT40.md method 2). With every block off the game is NIGHT39's bit for bit (`--off40` / `-skygrindOff40`; every older
// off switch also turns these off). Read once from Design/unity-config.json (each block's `enabled`); a run takes them at its start
// (SurvivalSim.N40), a tape records them (format 23: a VarI after the NIGHT39 value; formats 1-22 replay with every block off). The view
// rows (arc40, pump40, rods40, models40) change only what is drawn: no bit.
// The blocks (GG 2026-10-13 ~22:00, verbatim in Research/2026-10-14-wave40/GG-REQUEST.md; DECIDE40):
// start40 (S1: the first wave waits for movement or a timer), kiln40 (S1: level 3 as n40-kiln, the shotgun first, the boost taught),
// cut40 (S1: level 6 out of the campaign), charge40 (W1: longer charges, the THUNDERCLAP's hold rule), ride40 (R2: the ride holds, the
// attach asks for intent), iceline40 (L2: level 2 as n40-iceline), brawl40 (B2: the brawler boss), loom40 (B4: THE LOOM by the grapple,
// level 4 as n40-span, the STITCHER out), crown40 (B5: THE CROWN taught, level 5 as n40-spire), foes40 (E3: the DRONE, WARDEN and CHARGER
// in the campaign).
namespace Skygrind.Core
{
    public struct N40Blocks
    {
        public bool start40, kiln40, cut40, charge40, ride40, iceline40, brawl40, loom40, crown40, foes40;

        /// <summary>The number of blocks (the tape's NIGHT40 value, bit i = block i).</summary>
        public const int Count = 10;

        /// <summary>The harnesses' and the Player checks' switch (`--off40`, `-skygrindOff40`; also every older off switch): every block
        /// off whatever the config says. A process-wide switch, set before a sim starts its run.</summary>
        public static bool ForceOff;

        /// <summary>Every block off: NIGHT39's game.</summary>
        public static N40Blocks Off => default;

        /// <summary>Is any block on?</summary>
        public bool Any => Bits != 0;

        /// <summary>The names in bit order (logs, the harness's header line, the design rows `design.n40.<name>`).</summary>
        public static readonly string[] Names = { "start40", "kiln40", "cut40", "charge40", "ride40", "iceline40", "brawl40", "loom40", "crown40", "foes40" };

        /// <summary>The blocks' `enabled` flags in the design config (every block off when absent: NIGHT39's config).</summary>
        public static N40Blocks From(GameConfig cfg)
        {
            var b = new N40Blocks();
            for (int i = 0; i < Count; i++) b.Set(i, cfg.Design("n40." + Names[i] + ".enabled"));
            return b;
        }

        /// <summary>Block i in Names order.</summary>
        public bool Flag(int i)
        {
            switch (i)
            {
                case 0: return start40;
                case 1: return kiln40;
                case 2: return cut40;
                case 3: return charge40;
                case 4: return ride40;
                case 5: return iceline40;
                case 6: return brawl40;
                case 7: return loom40;
                case 8: return crown40;
                case 9: return foes40;
                default: return false;
            }
        }

        void Set(int i, bool v)
        {
            switch (i)
            {
                case 0: start40 = v; break;
                case 1: kiln40 = v; break;
                case 2: cut40 = v; break;
                case 3: charge40 = v; break;
                case 4: ride40 = v; break;
                case 5: iceline40 = v; break;
                case 6: brawl40 = v; break;
                case 7: loom40 = v; break;
                case 8: crown40 = v; break;
                case 9: foes40 = v; break;
            }
        }

        /// <summary>The blocks as a value (bit i = block i): what a tape records.</summary>
        public int Bits
        {
            get
            {
                int b = 0;
                for (int i = 0; i < Count; i++)
                    if (Flag(i)) b |= 1 << i;
                return b;
            }
        }

        /// <summary>The blocks from a tape's value.</summary>
        public static N40Blocks FromBits(int b)
        {
            var r = new N40Blocks();
            for (int i = 0; i < Count; i++) r.Set(i, (b & (1 << i)) != 0);
            return r;
        }

        public override string ToString()
        {
            if (!Any) return "every NIGHT40 block off";
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < Count; i++)
            {
                if (!Flag(i)) continue;
                if (sb.Length > 0) sb.Append(", ");
                sb.Append(Names[i]);
            }
            return sb.ToString();
        }
    }
}
