// NIGHT22 (the shared commit, the lead): the six NIGHT22 design blocks a run reads, one bit each, so every sim-visible NIGHT22
// feature can be switched off (NIGHT22.md "Switches"). With every block off the game is NIGHT21's bit for bit: the A side for
// every judge (`--off22` / `-skygrindOff22`); `--off21`, `--off20` and `--off19` also turn these off. Read once from
// Design/unity-config.json (each block's `enabled`); a run takes them at its start (SurvivalSim.n22), a tape records them
// (format 7; formats 1-6 replay with every block off) and the harnesses force them off with ForceOff.
// The blocks: bit 0 was the DAILY ghost (removed in NIGHT24: the bit stays in the tape byte, retired0, nothing reads it), design.n22.duskClock (W2: the sky sinks per
// round), design.n22.haptics (M: distinct pad patterns), design.n22.directed (leftover 1: directed steps for THE HUNT / THE
// BEACON), design.n22.trickChain (leftover 4), design.n22.overdrive (leftover 5). The NIGHT21 rows keep their N21Blocks.
namespace Skygrind.Core
{
    public struct N22Blocks
    {
        public bool retired0, duskClock, haptics, directed, trickChain, overdrive;

        /// <summary>The number of blocks (the tape's NIGHT22 byte, bit i = block i).</summary>
        public const int Count = 6;

        /// <summary>The harnesses' and the Player checks' switch (`--off22`, `-skygrindOff22`; also `--off21`, `--off20`,
        /// `--off19`): every block off whatever the config says. A process-wide switch, set before a sim starts its run.</summary>
        public static bool ForceOff;

        /// <summary>Every block off: NIGHT21's game.</summary>
        public static N22Blocks Off => default;

        /// <summary>Is any block on?</summary>
        public bool Any => retired0 || duskClock || haptics || directed || trickChain || overdrive;

        /// <summary>Does any block that changes the sim (and so the fingerprint) read as on? (directed, trickChain, overdrive.)</summary>
        public bool SimChanging => directed || trickChain || overdrive;

        /// <summary>The blocks' `enabled` flags in the design config (every block off when absent: NIGHT21's config).</summary>
        public static N22Blocks From(GameConfig cfg) => new N22Blocks
        {
            duskClock = cfg.Design("n22.duskClock.enabled"),
            haptics = cfg.Design("n22.haptics.enabled"),
            directed = cfg.Design("n22.directed.enabled"),
            trickChain = cfg.Design("n22.trickChain.enabled"),
            overdrive = cfg.Design("n22.overdrive.enabled"),
        };

        /// <summary>Block i (0 retired: the removed DAILY ghost, 1 duskClock, 2 haptics, 3 directed, 4 trickChain, 5 overdrive).</summary>
        public bool Flag(int i) => i == 0 ? retired0 : i == 1 ? duskClock : i == 2 ? haptics : i == 3 ? directed : i == 4 ? trickChain : i == 5 && overdrive;

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
        public static N22Blocks FromBits(int b) => new N22Blocks
        {
            retired0 = (b & 1) != 0, duskClock = (b & 2) != 0, haptics = (b & 4) != 0,
            directed = (b & 8) != 0, trickChain = (b & 16) != 0, overdrive = (b & 32) != 0,
        };

        /// <summary>The names in bit order (logs, the harness's header line).</summary>
        public static readonly string[] Names = { "retired0", "duskClock", "haptics", "directed", "trickChain", "overdrive" };

        public override string ToString()
        {
            if (!Any) return "every NIGHT22 block off";
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
