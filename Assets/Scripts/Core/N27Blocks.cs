// NIGHT27 (the shared commit, the lead): the six sim-changing NIGHT27 design blocks a run reads, one bit each, so every sim-changing
// NIGHT27 feature can be switched off (NIGHT27.md "Switches"). With every block off the game is NIGHT26's bit for bit
// (`--off27` / `-skygrindOff27`; `--off26` and older also turn these off). Read once from Design/unity-config.json (each block's
// `enabled`); a run takes them at its start (SurvivalSim.n27), a tape records them (format 11: one byte after the NIGHT26 byte;
// formats 1-10 replay with every block off). The view rows (slowAim27, skate27, roof27) and the level data rows (tutorial27)
// are read directly from design.n27.<row>.enabled and need no bit.
// The blocks: arenas27 (the tight rooms, the cost budget, wall lanes as level data and a validator rule), hazardFloor27 (a floor strip
// that hurts for a limited time), pulse27 (the 2.5 s heavy swing, five kill pips, the LT blast gate), chargeShots27 (RT hold on the
// pistol and the shotgun), boss1 (THE LANTERN), boss2 (THE GLACIER, pulse-only chase).
namespace Skygrind.Core
{
    public struct N27Blocks
    {
        public bool arenas27, hazardFloor27, pulse27, chargeShots27, boss1, boss2;

        /// <summary>The number of blocks (the tape's NIGHT27 byte, bit i = block i).</summary>
        public const int Count = 6;

        /// <summary>The harnesses' and the Player checks' switch (`--off27`, `-skygrindOff27`; also `--off26` and older): every block
        /// off whatever the config says. A process-wide switch, set before a sim starts its run.</summary>
        public static bool ForceOff;

        /// <summary>Every block off: NIGHT26's game.</summary>
        public static N27Blocks Off => default;

        /// <summary>Is any block on?</summary>
        public bool Any => arenas27 || hazardFloor27 || pulse27 || chargeShots27 || boss1 || boss2;

        /// <summary>Does any block that changes the composed SURVIVAL run (and so the fingerprint) read as on? (pulse27 and chargeShots27
        /// change a survival run's combat; the others act only in campaign levels.)</summary>
        public bool SimChanging => pulse27 || chargeShots27;

        /// <summary>The blocks' `enabled` flags in the design config (every block off when absent: NIGHT26's config).</summary>
        public static N27Blocks From(GameConfig cfg) => new N27Blocks
        {
            arenas27 = cfg.Design("n27.arenas27.enabled"),
            hazardFloor27 = cfg.Design("n27.hazardFloor27.enabled"),
            pulse27 = cfg.Design("n27.pulse27.enabled"),
            chargeShots27 = cfg.Design("n27.chargeShots27.enabled"),
            boss1 = cfg.Design("n27.boss1.enabled"),
            boss2 = cfg.Design("n27.boss2.enabled"),
        };

        /// <summary>Block i (0 arenas27, 1 hazardFloor27, 2 pulse27, 3 chargeShots27, 4 boss1, 5 boss2).</summary>
        public bool Flag(int i) => i == 0 ? arenas27 : i == 1 ? hazardFloor27 : i == 2 ? pulse27 : i == 3 ? chargeShots27 : i == 4 ? boss1 : i == 5 && boss2;

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
        public static N27Blocks FromBits(int b) => new N27Blocks
        {
            arenas27 = (b & 1) != 0, hazardFloor27 = (b & 2) != 0, pulse27 = (b & 4) != 0, chargeShots27 = (b & 8) != 0,
            boss1 = (b & 16) != 0, boss2 = (b & 32) != 0,
        };

        /// <summary>The names in bit order (logs, the harness's header line).</summary>
        public static readonly string[] Names = { "arenas27", "hazardFloor27", "pulse27", "chargeShots27", "boss1", "boss2" };

        public override string ToString()
        {
            if (!Any) return "every NIGHT27 block off";
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
