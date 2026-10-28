// NIGHT37 (the shared commit, the lead): the five sim-changing NIGHT37 design blocks a run reads, one bit each, so every NIGHT37 boss
// can be switched off (NIGHT37.md "every n37 row behind N37Blocks"). With every block off the game is NIGHT36's bit for bit
// (`--off37` / `-skygrindOff37`; every older off switch also turns these off). Read once from Design/unity-config.json (each block's
// `enabled`); a run takes them at its start (SurvivalSim.N37), a tape records them (format 20: a VarI after the NIGHT36 value;
// formats 1-19 replay with every block off). A boss row's level swap (levelFrom -> levelTo) is a flow choice: on the tape as the level id.
// The blocks (GG 2026-10-12 ~19:25: "I want all the bosses completely overhauled and restart from scratch except the worm boss ... it
// should be a consistently moving ... battle"; Research/2026-10-12-wave37/BOSSES.md): boss37 (builder K: the shared Boss37 brain:
// stagger, budget, pick with pity, habit, anti-hide, legs), wick37 (builder W: THE WICK, level 1's climb), stag37 (builder S: THE STAG,
// level 2's chase), loom37 (builder O: THE LOOM, level 4's crossing), crown37 (builder C: THE CROWN, level 5's descent).
namespace Skygrind.Core
{
    public struct N37Blocks
    {
        public bool boss37, wick37, stag37, loom37, crown37;

        /// <summary>The number of blocks (the tape's NIGHT37 value, bit i = block i).</summary>
        public const int Count = 5;

        /// <summary>The harnesses' and the Player checks' switch (`--off37`, `-skygrindOff37`; also every older off switch): every block
        /// off whatever the config says. A process-wide switch, set before a sim starts its run.</summary>
        public static bool ForceOff;

        /// <summary>Every block off: NIGHT36's game.</summary>
        public static N37Blocks Off => default;

        /// <summary>Is any block on?</summary>
        public bool Any => Bits != 0;

        /// <summary>The blocks' `enabled` flags in the design config (every block off when absent: NIGHT36's config).</summary>
        public static N37Blocks From(GameConfig cfg) => new N37Blocks
        {
            boss37 = cfg.Design("n37.boss37.enabled"),
            wick37 = cfg.Design("n37.wick37.enabled"),
            stag37 = cfg.Design("n37.stag37.enabled"),
            loom37 = cfg.Design("n37.loom37.enabled"),
            crown37 = cfg.Design("n37.crown37.enabled"),
        };

        /// <summary>Block i in Names order.</summary>
        public bool Flag(int i)
        {
            switch (i)
            {
                case 0: return boss37;
                case 1: return wick37;
                case 2: return stag37;
                case 3: return loom37;
                case 4: return crown37;
                default: return false;
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
        public static N37Blocks FromBits(int b) => new N37Blocks
        {
            boss37 = (b & 1) != 0, wick37 = (b & 2) != 0, stag37 = (b & 4) != 0, loom37 = (b & 8) != 0, crown37 = (b & 16) != 0,
        };

        /// <summary>The names in bit order (logs, the harness's header line).</summary>
        public static readonly string[] Names = { "boss37", "wick37", "stag37", "loom37", "crown37" };

        public override string ToString()
        {
            if (!Any) return "every NIGHT37 block off";
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
