// NIGHT39 (the shared commit, the lead): the three sim-changing NIGHT39 design blocks a run reads, one bit each, so every NIGHT39 row
// can be switched off (NIGHT39.md "every n39 sim row behind N39Blocks"). With every block off the game is NIGHT38's bit for bit
// (`--off39` / `-skygrindOff39`; every older off switch also turns these off). Read once from Design/unity-config.json (each block's
// `enabled`); a run takes them at its start (SurvivalSim.N39), a tape records them (format 22: a VarI after the NIGHT38 value; formats
// 1-21 replay with every block off). slow39 (the ride / latch / parry slow motion) is loop pacing the sim never sees: no bit.
// The blocks (GG 2026-10-13 ~14:05, verbatim in Research/2026-10-14-wave39/GG-REQUEST.md; DECIDE39):
// p39 (builder P1: the KESTREL's hold prioritises the charge; a quick release fires the tap), ride39 (builder R1: the ride's speed gain
// and jump-off numbers, overriding design.n35.wallride35 while on), boss39 (builder B1: per-boss overrides so each fight lasts 1-2 min).
namespace Skygrind.Core
{
    public struct N39Blocks
    {
        public bool p39, ride39, boss39;

        /// <summary>The number of blocks (the tape's NIGHT39 value, bit i = block i).</summary>
        public const int Count = 3;

        /// <summary>The harnesses' and the Player checks' switch (`--off39`, `-skygrindOff39`; also every older off switch): every block
        /// off whatever the config says. A process-wide switch, set before a sim starts its run.</summary>
        public static bool ForceOff;

        /// <summary>Every block off: NIGHT38's game.</summary>
        public static N39Blocks Off => default;

        /// <summary>Is any block on?</summary>
        public bool Any => Bits != 0;

        /// <summary>The blocks' `enabled` flags in the design config (every block off when absent: NIGHT38's config).</summary>
        public static N39Blocks From(GameConfig cfg) => new N39Blocks
        {
            p39 = cfg.Design("n39.p39.enabled"),
            ride39 = cfg.Design("n39.ride39.enabled"),
            boss39 = cfg.Design("n39.boss39.enabled"),
        };

        /// <summary>Block i in Names order.</summary>
        public bool Flag(int i)
        {
            switch (i)
            {
                case 0: return p39;
                case 1: return ride39;
                case 2: return boss39;
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
        public static N39Blocks FromBits(int b) => new N39Blocks { p39 = (b & 1) != 0, ride39 = (b & 2) != 0, boss39 = (b & 4) != 0 };

        /// <summary>The names in bit order (logs, the harness's header line).</summary>
        public static readonly string[] Names = { "p39", "ride39", "boss39" };

        public override string ToString()
        {
            if (!Any) return "every NIGHT39 block off";
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
