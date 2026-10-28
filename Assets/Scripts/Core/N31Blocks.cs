// NIGHT31 (the shared commit, the lead): the eight sim-changing NIGHT31 design blocks a run reads, one bit each, so every sim-changing
// NIGHT31 feature can be switched off (NIGHT31.md "Every sim-visible change ships behind a design.n31.<row> switch"). With every block
// off the game is NIGHT30's bit for bit (`--off31` / `-skygrindOff31`; every older off switch also turns these off). Read once from
// Design/unity-config.json (each block's `enabled`); a run takes them at its start (SurvivalSim.N31), a tape records them (format 14:
// one byte after the NIGHT30 byte; formats 1-13 replay with every block off). View rows (the grapple mark, the low-health edge) need no bit.
// The blocks: grapplePull (a tap pulls you in: Arcline21's zip on a node / surface), charge (the charge shot back, re-tuned), ltAlways
// (every weapon's LT does something: the gauntlet's parry), unlocks (level 1 the KESTREL only, one weapon per level), lantern31, stag31
// (level 2's new boss), wyrm31, loom31 (DESIGN31 C1-C4).
namespace Skygrind.Core
{
    public struct N31Blocks
    {
        public bool grapplePull, charge, ltAlways, unlocks, lantern31, stag31, wyrm31, loom31;

        /// <summary>The number of blocks (the tape's NIGHT31 byte, bit i = block i).</summary>
        public const int Count = 8;

        /// <summary>The harnesses' and the Player checks' switch (`--off31`, `-skygrindOff31`; also every older off switch): every block
        /// off whatever the config says. A process-wide switch, set before a sim starts its run.</summary>
        public static bool ForceOff;

        /// <summary>Every block off: NIGHT30's game.</summary>
        public static N31Blocks Off => default;

        /// <summary>Is any block on?</summary>
        public bool Any => grapplePull || charge || ltAlways || unlocks || lantern31 || stag31 || wyrm31 || loom31;

        /// <summary>Does any block that changes the composed SURVIVAL run (and so the fingerprint) read as on? (charge and ltAlways change
        /// a survival run's combat; the others act only in campaign levels or with the grapple owned.)</summary>
        public bool SimChanging => charge || ltAlways || grapplePull;

        /// <summary>The blocks' `enabled` flags in the design config (every block off when absent: NIGHT30's config).</summary>
        public static N31Blocks From(GameConfig cfg) => new N31Blocks
        {
            grapplePull = cfg.Design("n31.grapplePull.enabled"),
            charge = cfg.Design("n31.charge.enabled"),
            ltAlways = cfg.Design("n31.ltAlways.enabled"),
            unlocks = cfg.Design("n31.unlocks.enabled"),
            lantern31 = cfg.Design("n31.lantern31.enabled"),
            stag31 = cfg.Design("n31.stag31.enabled"),
            wyrm31 = cfg.Design("n31.wyrm31.enabled"),
            loom31 = cfg.Design("n31.loom31.enabled"),
        };

        /// <summary>Block i (0 grapplePull, 1 charge, 2 ltAlways, 3 unlocks, 4 lantern31, 5 stag31, 6 wyrm31, 7 loom31).</summary>
        public bool Flag(int i) => i == 0 ? grapplePull : i == 1 ? charge : i == 2 ? ltAlways : i == 3 ? unlocks : i == 4 ? lantern31 : i == 5 ? stag31 : i == 6 ? wyrm31 : i == 7 && loom31;

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
        public static N31Blocks FromBits(int b) => new N31Blocks
        {
            grapplePull = (b & 1) != 0, charge = (b & 2) != 0, ltAlways = (b & 4) != 0, unlocks = (b & 8) != 0, lantern31 = (b & 16) != 0, stag31 = (b & 32) != 0, wyrm31 = (b & 64) != 0, loom31 = (b & 128) != 0,
        };

        /// <summary>The names in bit order (logs, the harness's header line).</summary>
        public static readonly string[] Names = { "grapplePull", "charge", "ltAlways", "unlocks", "lantern31", "stag31", "wyrm31", "loom31" };

        public override string ToString()
        {
            if (!Any) return "every NIGHT31 block off";
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
