// NIGHT32 (the shared commit, the lead): the seven sim-changing NIGHT32 design blocks a run reads, one bit each, so every sim-changing
// NIGHT32 feature can be switched off (NIGHT32.md "every sim-visible change ships behind a design.n32.<row> switch"). With every block
// off the game is NIGHT31's bit for bit (`--off32` / `-skygrindOff32`; every older off switch also turns these off). Read once from
// Design/unity-config.json (each block's `enabled`); a run takes them at its start (SurvivalSim.N32), a tape records them (format 15:
// one byte after the NIGHT31 byte; formats 1-14 replay with every block off). View rows (charge pips, boss weak points, the HUD fixes)
// need no bit.
// The blocks (GG 2026-10-10: bosses "way too much health", "you can barely hit them", "you can just spam the grapple", "the arenas are so
// boring", "the 5th level", "the 4th level completely overhauled again", "the first boss fight arena ... needs to be a circle"):
// bosses32 (every boss dies in about a minute; no zero-damage gates), grapple32 (the grapple has charges), balance32 (the combat balance
// pass outside the bosses), arenas32 (new arenas on levels 1-3 and the round boss-1 ring), span32 (level 4 rebuilt), level5 (level 5),
// boss5 (level 5's boss).
namespace Skygrind.Core
{
    public struct N32Blocks
    {
        public bool bosses32, grapple32, balance32, arenas32, span32, level5, boss5;

        /// <summary>The number of blocks (the tape's NIGHT32 byte, bit i = block i).</summary>
        public const int Count = 7;

        /// <summary>The harnesses' and the Player checks' switch (`--off32`, `-skygrindOff32`; also every older off switch): every block
        /// off whatever the config says. A process-wide switch, set before a sim starts its run.</summary>
        public static bool ForceOff;

        /// <summary>Every block off: NIGHT31's game.</summary>
        public static N32Blocks Off => default;

        /// <summary>Is any block on?</summary>
        public bool Any => bosses32 || grapple32 || balance32 || arenas32 || span32 || level5 || boss5;

        /// <summary>Does any block that changes the composed SURVIVAL run (and so the fingerprint) read as on? (balance32 changes a survival
        /// run's combat; grapple32 changes the grapple wherever it is owned; the others act only in campaign levels.)</summary>
        public bool SimChanging => balance32 || grapple32;

        /// <summary>The blocks' `enabled` flags in the design config (every block off when absent: NIGHT31's config).</summary>
        public static N32Blocks From(GameConfig cfg) => new N32Blocks
        {
            bosses32 = cfg.Design("n32.bosses32.enabled"),
            grapple32 = cfg.Design("n32.grapple32.enabled"),
            balance32 = cfg.Design("n32.balance32.enabled"),
            arenas32 = cfg.Design("n32.arenas32.enabled"),
            span32 = cfg.Design("n32.span32.enabled"),
            level5 = cfg.Design("n32.level5.enabled"),
            boss5 = cfg.Design("n32.boss5.enabled"),
        };

        /// <summary>Block i (0 bosses32, 1 grapple32, 2 balance32, 3 arenas32, 4 span32, 5 level5, 6 boss5).</summary>
        public bool Flag(int i) => i == 0 ? bosses32 : i == 1 ? grapple32 : i == 2 ? balance32 : i == 3 ? arenas32 : i == 4 ? span32 : i == 5 ? level5 : i == 6 && boss5;

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
        public static N32Blocks FromBits(int b) => new N32Blocks
        {
            bosses32 = (b & 1) != 0, grapple32 = (b & 2) != 0, balance32 = (b & 4) != 0, arenas32 = (b & 8) != 0, span32 = (b & 16) != 0, level5 = (b & 32) != 0, boss5 = (b & 64) != 0,
        };

        /// <summary>The names in bit order (logs, the harness's header line).</summary>
        public static readonly string[] Names = { "bosses32", "grapple32", "balance32", "arenas32", "span32", "level5", "boss5" };

        public override string ToString()
        {
            if (!Any) return "every NIGHT32 block off";
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
