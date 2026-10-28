// NIGHT30 (the shared commit, the lead): the eight sim-changing NIGHT30 design blocks a run reads, one bit each, so every sim-changing
// NIGHT30 feature can be switched off (NIGHT30.md "Every sim-visible change ships behind a design.n30.<row> switch"). With every block
// off the game is NIGHT29's bit for bit (`--off30` / `-skygrindOff30`; every older off switch also turns these off). Read once from
// Design/unity-config.json (each block's `enabled`); a run takes them at its start (SurvivalSim.N30), a tape records them (format 13:
// one byte after the NIGHT29 byte; formats 1-12 replay with every block off). The view rows (hud30, look30, lava view) need no bit.
// The blocks: rules (one action at a time, the switch waits for the action only, cooldowns run holstered: Core/Guns/Rules30.cs),
// glide (ice is a floor tag: Core/Glide30.cs), lava (lava rows burn and knock, no teleport; enemies burn), lantern30 (the ring Lantern),
// glacier30 (the Glacier rebuilt), wyrm30 (the head-punch stagger), span (LEVEL 4's ARCLINE / Loom rules), teach (boss and room lines).
namespace Skygrind.Core
{
    public struct N30Blocks
    {
        public bool rules, glide, lava, lantern30, glacier30, wyrm30, span, teach;

        /// <summary>The number of blocks (the tape's NIGHT30 byte, bit i = block i).</summary>
        public const int Count = 8;

        /// <summary>The harnesses' and the Player checks' switch (`--off30`, `-skygrindOff30`; also every older off switch): every block
        /// off whatever the config says. A process-wide switch, set before a sim starts its run.</summary>
        public static bool ForceOff;

        /// <summary>Every block off: NIGHT29's game.</summary>
        public static N30Blocks Off => default;

        /// <summary>Is any block on?</summary>
        public bool Any => rules || glide || lava || lantern30 || glacier30 || wyrm30 || span || teach;

        /// <summary>Does any block that changes the composed SURVIVAL run (and so the fingerprint) read as on? (rules changes a survival
        /// run's combat; the others act only in campaign levels.)</summary>
        public bool SimChanging => rules;

        /// <summary>The blocks' `enabled` flags in the design config (every block off when absent: NIGHT29's config).</summary>
        public static N30Blocks From(GameConfig cfg) => new N30Blocks
        {
            rules = cfg.Design("n30.rules.enabled"),
            glide = cfg.Design("n30.glide.enabled"),
            lava = cfg.Design("n30.lava.enabled"),
            lantern30 = cfg.Design("n30.lantern30.enabled"),
            glacier30 = cfg.Design("n30.glacier30.enabled"),
            wyrm30 = cfg.Design("n30.wyrm30.enabled"),
            span = cfg.Design("n30.span.enabled"),
            teach = cfg.Design("n30.teach.enabled"),
        };

        /// <summary>Block i (0 rules, 1 glide, 2 lava, 3 lantern30, 4 glacier30, 5 wyrm30, 6 span, 7 teach).</summary>
        public bool Flag(int i) => i == 0 ? rules : i == 1 ? glide : i == 2 ? lava : i == 3 ? lantern30 : i == 4 ? glacier30 : i == 5 ? wyrm30 : i == 6 ? span : i == 7 && teach;

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
        public static N30Blocks FromBits(int b) => new N30Blocks
        {
            rules = (b & 1) != 0, glide = (b & 2) != 0, lava = (b & 4) != 0, lantern30 = (b & 8) != 0, glacier30 = (b & 16) != 0, wyrm30 = (b & 32) != 0, span = (b & 64) != 0, teach = (b & 128) != 0,
        };

        /// <summary>The names in bit order (logs, the harness's header line).</summary>
        public static readonly string[] Names = { "rules", "glide", "lava", "lantern30", "glacier30", "wyrm30", "span", "teach" };

        public override string ToString()
        {
            if (!Any) return "every NIGHT30 block off";
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
