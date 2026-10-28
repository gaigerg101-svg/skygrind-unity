// NIGHT38 (the shared commit, the lead): the eight sim-changing NIGHT38 design blocks a run reads, one bit each, so every NIGHT38 row
// can be switched off (NIGHT38.md "every n38 sim row behind N38Blocks"). With every block off the game is NIGHT37's bit for bit
// (`--off38` / `-skygrindOff38`; every older off switch also turns these off). Read once from Design/unity-config.json (each block's
// `enabled`); a run takes them at its start (SurvivalSim.N38), a tape records them (format 21: a VarI after the NIGHT37 value; formats
// 1-20 replay with every block off). A level row's levelFrom -> levelTo swap is a flow choice: on the tape as the level id.
// View rows (doors38, hud38, juice38) need no bit: they read their own `enabled` in design.n38 and are off with --off38 too.
// The blocks (GG 2026-10-13 ~00:10, verbatim in Research/2026-10-13-wave38/GG-REQUEST.md; PLAN.md; DECIDE38.md D-1 / D-2):
// ride38 (builder M: a held wall ride keeps its height; the look pitch drifts it; Core/Mover.cs's wallride35 branch only),
// foundry38 / iceline38 / kiln38 / span38 / spire38 / finale38 (the six levels rebuilt for flight: each a levelFrom -> levelTo swap in
// Curriculum25.N38Levels, builders F / S2 / K3 / O2 / C2 / F), stag38 (builder S2: THE STAG's lane walls and read pass).
namespace Skygrind.Core
{
    public struct N38Blocks
    {
        public bool ride38, foundry38, iceline38, kiln38, span38, spire38, finale38, stag38;

        /// <summary>The number of blocks (the tape's NIGHT38 value, bit i = block i).</summary>
        public const int Count = 8;

        /// <summary>The harnesses' and the Player checks' switch (`--off38`, `-skygrindOff38`; also every older off switch): every block
        /// off whatever the config says. A process-wide switch, set before a sim starts its run.</summary>
        public static bool ForceOff;

        /// <summary>Every block off: NIGHT37's game.</summary>
        public static N38Blocks Off => default;

        /// <summary>Is any block on?</summary>
        public bool Any => Bits != 0;

        /// <summary>The blocks' `enabled` flags in the design config (every block off when absent: NIGHT37's config).</summary>
        public static N38Blocks From(GameConfig cfg) => new N38Blocks
        {
            ride38 = cfg.Design("n38.ride38.enabled"),
            foundry38 = cfg.Design("n38.foundry38.enabled"),
            iceline38 = cfg.Design("n38.iceline38.enabled"),
            kiln38 = cfg.Design("n38.kiln38.enabled"),
            span38 = cfg.Design("n38.span38.enabled"),
            spire38 = cfg.Design("n38.spire38.enabled"),
            finale38 = cfg.Design("n38.finale38.enabled"),
            stag38 = cfg.Design("n38.stag38.enabled"),
        };

        /// <summary>Block i in Names order.</summary>
        public bool Flag(int i)
        {
            switch (i)
            {
                case 0: return ride38;
                case 1: return foundry38;
                case 2: return iceline38;
                case 3: return kiln38;
                case 4: return span38;
                case 5: return spire38;
                case 6: return finale38;
                case 7: return stag38;
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
        public static N38Blocks FromBits(int b) => new N38Blocks
        {
            ride38 = (b & 1) != 0, foundry38 = (b & 2) != 0, iceline38 = (b & 4) != 0, kiln38 = (b & 8) != 0,
            span38 = (b & 16) != 0, spire38 = (b & 32) != 0, finale38 = (b & 64) != 0, stag38 = (b & 128) != 0,
        };

        /// <summary>The names in bit order (logs, the harness's header line).</summary>
        public static readonly string[] Names = { "ride38", "foundry38", "iceline38", "kiln38", "span38", "spire38", "finale38", "stag38" };

        public override string ToString()
        {
            if (!Any) return "every NIGHT38 block off";
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
