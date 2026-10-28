// NIGHT26 (the shared commit, the lead): the seven NIGHT26 design blocks a run reads, one bit each, so every NIGHT26 feature can be
// switched off (NIGHT26.md "Switches"). With every block off the game is NIGHT25's bit for bit: the A side for every judge
// (`--off26` / `-skygrindOff26`); `--off25`, `--off24`, `--off22`, `--off21`, `--off20` and `--off19` also turn these off. Read once from
// Design/unity-config.json (each block's `enabled`); a run takes them at its start (SurvivalSim.n26), a tape records them
// (format 10: one byte after the NIGHT25 byte; formats 1-9 replay with every block off) and the harnesses force them off with ForceOff.
// The blocks: design.n26.pulseLT (W: the PULSE GAUNTLET's blast is the LEFT TRIGGER), weaponIdentity (W: every weapon's LT, costs,
// enemy vulnerabilities, the rocket and BOUNCER nerfs, the unlock order), surfaces (M: ice and boost strips through Mover.groundFrictionMult),
// levelSelect (L: the ULTRAKILL-style level select, ranks, CAMPAIGN as the default mode), level1 / level2 / level3 (L, B: the first three
// levels and their bosses).
namespace Skygrind.Core
{
    public struct N26Blocks
    {
        public bool pulseLT, weaponIdentity, surfaces, levelSelect, level1, level2, level3;

        /// <summary>The number of blocks (the tape's NIGHT26 byte, bit i = block i).</summary>
        public const int Count = 7;

        /// <summary>The harnesses' and the Player checks' switch (`--off26`, `-skygrindOff26`; also `--off25` and older): every block
        /// off whatever the config says. A process-wide switch, set before a sim starts its run.</summary>
        public static bool ForceOff;

        /// <summary>Every block off: NIGHT25's game.</summary>
        public static N26Blocks Off => default;

        /// <summary>Is any block on?</summary>
        public bool Any => pulseLT || weaponIdentity || surfaces || levelSelect || level1 || level2 || level3;

        /// <summary>Does any block that changes the composed SURVIVAL run (and so the fingerprint) read as on? (pulseLT and weaponIdentity
        /// change a survival run's combat; surfaces only act where a level plan places them; the level blocks change only campaign runs.)</summary>
        public bool SimChanging => pulseLT || weaponIdentity;

        /// <summary>The blocks' `enabled` flags in the design config (every block off when absent: NIGHT25's config).</summary>
        public static N26Blocks From(GameConfig cfg) => new N26Blocks
        {
            pulseLT = cfg.Design("n26.pulseLT.enabled"),
            weaponIdentity = cfg.Design("n26.weaponIdentity.enabled"),
            surfaces = cfg.Design("n26.surfaces.enabled"),
            levelSelect = cfg.Design("n26.levelSelect.enabled"),
            level1 = cfg.Design("n26.level1.enabled"),
            level2 = cfg.Design("n26.level2.enabled"),
            level3 = cfg.Design("n26.level3.enabled"),
        };

        /// <summary>Block i (0 pulseLT, 1 weaponIdentity, 2 surfaces, 3 levelSelect, 4 level1, 5 level2, 6 level3).</summary>
        public bool Flag(int i) => i == 0 ? pulseLT : i == 1 ? weaponIdentity : i == 2 ? surfaces : i == 3 ? levelSelect : i == 4 ? level1 : i == 5 ? level2 : i == 6 && level3;

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
        public static N26Blocks FromBits(int b) => new N26Blocks
        {
            pulseLT = (b & 1) != 0, weaponIdentity = (b & 2) != 0, surfaces = (b & 4) != 0, levelSelect = (b & 8) != 0,
            level1 = (b & 16) != 0, level2 = (b & 32) != 0, level3 = (b & 64) != 0,
        };

        /// <summary>The names in bit order (logs, the harness's header line).</summary>
        public static readonly string[] Names = { "pulseLT", "weaponIdentity", "surfaces", "levelSelect", "level1", "level2", "level3" };

        public override string ToString()
        {
            if (!Any) return "every NIGHT26 block off";
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
