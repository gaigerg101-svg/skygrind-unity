// NIGHT35 (the shared commit, the lead): the four sim-changing NIGHT35 design blocks a run reads, one bit each, so every sim-changing
// NIGHT35 feature can be switched off (NIGHT35.md "every sim change behind design.n35"). With every block off the game is NIGHT34's bit
// for bit (`--off35` / `-skygrindOff35`; every older off switch also turns these off). Read once from Design/unity-config.json (each
// block's `enabled`); a run takes them at its start (SurvivalSim.N35), a tape records them (format 18: one byte after the NIGHT34 byte;
// formats 1-17 replay with every block off). View and flow rows (pickup35, look35, lights35, tv35, ridelook35, campaign6) need no bit:
// a level swap is on the tape as the level id.
// The blocks (GG 2026-10-11 after n34: "the pistol lost the ability to charge and fire same with the shotgun"; ULTRAKILL-style text
// that teaches each mechanic; the grapple "shouldn't go limp ... it needs to take you to wherever you fire" and unlocks "like the 4th or
// 5th level"; wall riding "a lot more like Lucio from Overwatch"):
// charge35 (builder W: hold to charge with no shots first, release fires), wallride35 (builder M: the Lucio wall ride, Mover.cs branches),
// grapple35 (builder M: a straight constant pull to the hit point, no hold-swing, owned from level 4), teach35 (builder T: the teaching
// text's once-per-profile lesson triggers).
namespace Skygrind.Core
{
    public struct N35Blocks
    {
        public bool charge35, wallride35, grapple35, teach35;

        /// <summary>The number of blocks (the tape's NIGHT35 byte, bit i = block i).</summary>
        public const int Count = 4;

        /// <summary>The harnesses' and the Player checks' switch (`--off35`, `-skygrindOff35`; also every older off switch): every block
        /// off whatever the config says. A process-wide switch, set before a sim starts its run.</summary>
        public static bool ForceOff;

        /// <summary>Every block off: NIGHT34's game.</summary>
        public static N35Blocks Off => default;

        /// <summary>Is any block on?</summary>
        public bool Any => charge35 || wallride35 || grapple35 || teach35;

        /// <summary>The blocks' `enabled` flags in the design config (every block off when absent: NIGHT34's config).</summary>
        public static N35Blocks From(GameConfig cfg) => new N35Blocks
        {
            charge35 = cfg.Design("n35.charge35.enabled"),
            wallride35 = cfg.Design("n35.wallride35.enabled"),
            grapple35 = cfg.Design("n35.grapple35.enabled"),
            teach35 = cfg.Design("n35.teach35.enabled"),
        };

        /// <summary>Block i (0 charge35, 1 wallride35, 2 grapple35, 3 teach35).</summary>
        public bool Flag(int i) => i == 0 ? charge35 : i == 1 ? wallride35 : i == 2 ? grapple35 : i == 3 && teach35;

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
        public static N35Blocks FromBits(int b) => new N35Blocks
        {
            charge35 = (b & 1) != 0, wallride35 = (b & 2) != 0, grapple35 = (b & 4) != 0, teach35 = (b & 8) != 0,
        };

        /// <summary>The names in bit order (logs, the harness's header line).</summary>
        public static readonly string[] Names = { "charge35", "wallride35", "grapple35", "teach35" };

        public override string ToString()
        {
            if (!Any) return "every NIGHT35 block off";
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
