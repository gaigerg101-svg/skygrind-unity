// NIGHT21 (the shared commit, the lead): the six NIGHT21 design blocks a run reads, one bit each, so every NIGHT21 feature
// can be switched off (NIGHT21.md "Switches"). With every block off the game is NIGHT20's bit for bit: the A side for every
// judge (`--off21` / `-skygrindOff21`); `--off20` and `--off19` also turn these off (NIGHT19's / NIGHT18's game). Read once
// from Design/unity-config.json (each block's `enabled`); a run takes them at its start (SurvivalSim.n21), a tape records
// them (format 6; formats 1-5 replay with every block off) and the harnesses force them off with ForceOff.
// The blocks: design.n21.arcline (M: THE ARCLINE in Survival), design.n21.slideBites (M: the slide hits what it passes
// through), design.n21.stamp (S: round grades, the inked rank stamp, the RESULTS card off POINTS), design.n21.deadline (S:
// the kill-or-die twist round), design.n21.grind (W: the web's endless mode on SURVIVAL's core).
// NIGHT24: bit 5 was the INSTANT REPLAY block; GG removed that feature. The bit stays in the tape byte (format 6 / 7 tapes
// recorded with it on still parse and round-trip) but nothing reads it and the config no longer has the row.
namespace Skygrind.Core
{
    public struct N21Blocks
    {
        public bool arcline, slideBites, stamp, deadline, grind, retired5;

        /// <summary>The number of blocks (the tape's NIGHT21 byte, bit i = block i).</summary>
        public const int Count = 6;

        /// <summary>The harnesses' and the Player checks' switch (`--off21`, `-skygrindOff21`; also `--off20`, `--off19`): every
        /// block off whatever the config says. A process-wide switch, set before a sim starts its run.</summary>
        public static bool ForceOff;

        /// <summary>Every block off: NIGHT20's game.</summary>
        public static N21Blocks Off => default;

        /// <summary>Is any block on?</summary>
        public bool Any => arcline || slideBites || stamp || deadline || grind || retired5;

        /// <summary>The blocks' `enabled` flags in the design config (every block off when absent: NIGHT20's config).</summary>
        public static N21Blocks From(GameConfig cfg) => new N21Blocks
        {
            arcline = cfg.Design("n21.arcline.enabled"),
            slideBites = cfg.Design("n21.slideBites.enabled"),
            stamp = cfg.Design("n21.stamp.enabled"),
            deadline = cfg.Design("n21.deadline.enabled"),
            grind = cfg.Design("n21.grind.enabled"),
        };

        /// <summary>Block i (0 arcline, 1 slideBites, 2 stamp, 3 deadline, 4 grind, 5 retired: the removed INSTANT REPLAY).</summary>
        public bool Flag(int i) => i == 0 ? arcline : i == 1 ? slideBites : i == 2 ? stamp : i == 3 ? deadline : i == 4 ? grind : i == 5 && retired5;

        /// <summary>The blocks as a byte (bit i = block i): what a tape records.</summary>
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

        /// <summary>The blocks from a tape's byte.</summary>
        public static N21Blocks FromBits(int b) => new N21Blocks
        {
            arcline = (b & 1) != 0, slideBites = (b & 2) != 0, stamp = (b & 4) != 0,
            deadline = (b & 8) != 0, grind = (b & 16) != 0, retired5 = (b & 32) != 0,
        };

        /// <summary>The names in bit order (logs, the harness's header line).</summary>
        public static readonly string[] Names = { "arcline", "slideBites", "stamp", "deadline", "grind", "retired5" };

        public override string ToString()
        {
            if (!Any) return "every NIGHT21 block off";
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < Count; i++)
            {
                if (!Flag(i)) continue;
                if (sb.Length > 0) sb.Append(", ");
                sb.Append(Names[i]);
            }
            return sb.Append(" on").ToString();
        }
    }
}
