// NIGHT25 (the shared commit, the lead): the six NIGHT25 design blocks a run reads, one bit each, so every NIGHT25 feature can be
// switched off (NIGHT25.md "Switches"). With every block off the game is NIGHT24's bit for bit: the A side for every judge
// (`--off25` / `-skygrindOff25`); `--off24`, `--off22`, `--off21`, `--off20` and `--off19` also turn these off. Read once from
// Design/unity-config.json (each block's `enabled`); a run takes them at its start (SurvivalSim.n25), a tape records them
// (format 9: one byte after the campaign state; formats 1-8 replay with every block off) and the harnesses force them off with ForceOff.
// The blocks: design.n25.meleeFire (C: the quick melee and the gun's fire in the same frame), pulseCharge (C: the PULSE GAUNTLET's
// left click builds by its own hits and launches), shockwaveOff (C: no shockwave on the shotgun), wyrmAim (W: the worm aims at the
// player and is harder), tutorial (L: layer 1's six teaching stages), curriculum (L: levels 2-10).
namespace Skygrind.Core
{
    public struct N25Blocks
    {
        public bool meleeFire, pulseCharge, shockwaveOff, wyrmAim, tutorial, curriculum;

        /// <summary>The number of blocks (the tape's NIGHT25 byte, bit i = block i).</summary>
        public const int Count = 6;

        /// <summary>The harnesses' and the Player checks' switch (`--off25`, `-skygrindOff25`; also `--off24` and older): every block
        /// off whatever the config says. A process-wide switch, set before a sim starts its run.</summary>
        public static bool ForceOff;

        /// <summary>Every block off: NIGHT24's game.</summary>
        public static N25Blocks Off => default;

        /// <summary>Is any block on?</summary>
        public bool Any => meleeFire || pulseCharge || shockwaveOff || wyrmAim || tutorial || curriculum;

        /// <summary>Does any block that changes the composed SURVIVAL run (and so the fingerprint) read as on? (meleeFire, pulseCharge,
        /// shockwaveOff change a survival run's combat; the worm, the tutorial and the levels change only their own runs.)</summary>
        public bool SimChanging => meleeFire || pulseCharge || shockwaveOff;

        /// <summary>The blocks' `enabled` flags in the design config (every block off when absent: NIGHT24's config).</summary>
        public static N25Blocks From(GameConfig cfg) => new N25Blocks
        {
            meleeFire = cfg.Design("n25.meleeFire.enabled"),
            pulseCharge = cfg.Design("n25.pulseCharge.enabled"),
            shockwaveOff = cfg.Design("n25.shockwaveOff.enabled"),
            wyrmAim = cfg.Design("n25.wyrmAim.enabled"),
            tutorial = cfg.Design("n25.tutorial.enabled"),
            curriculum = cfg.Design("n25.curriculum.enabled"),
        };

        /// <summary>Block i (0 meleeFire, 1 pulseCharge, 2 shockwaveOff, 3 wyrmAim, 4 tutorial, 5 curriculum).</summary>
        public bool Flag(int i) => i == 0 ? meleeFire : i == 1 ? pulseCharge : i == 2 ? shockwaveOff : i == 3 ? wyrmAim : i == 4 ? tutorial : i == 5 && curriculum;

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
        public static N25Blocks FromBits(int b) => new N25Blocks
        {
            meleeFire = (b & 1) != 0, pulseCharge = (b & 2) != 0, shockwaveOff = (b & 4) != 0,
            wyrmAim = (b & 8) != 0, tutorial = (b & 16) != 0, curriculum = (b & 32) != 0,
        };

        /// <summary>The names in bit order (logs, the harness's header line).</summary>
        public static readonly string[] Names = { "meleeFire", "pulseCharge", "shockwaveOff", "wyrmAim", "tutorial", "curriculum" };

        public override string ToString()
        {
            if (!Any) return "every NIGHT25 block off";
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
