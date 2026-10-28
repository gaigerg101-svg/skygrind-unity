// NIGHT34 (the shared commit, the lead): the four sim-changing NIGHT34 design blocks a run reads, one bit each, so every sim-changing
// NIGHT34 feature can be switched off (NIGHT34.md "every sim change behind design.n34"). With every block off the game is NIGHT33's bit
// for bit (`--off34` / `-skygrindOff34`; every older off switch also turns these off). Read once from Design/unity-config.json (each
// block's `enabled`); a run takes them at its start (SurvivalSim.N34), a tape records them (format 17: one byte after the NIGHT33 bytes;
// formats 1-16 replay with every block off). View and flow rows (bendLook, rail34, nomark34, calm34, lines34) need no bit: a level swap
// is on the tape as the level id.
// The blocks (GG 2026-10-11: the grapple triggers "a dramatic slow-motion effect" so players "switch weapons in real time"; "the world
// bends around you"; movement, responsiveness and flow "dramatically better"; the Pulse Gauntlet "a slightly larger active hitbox when
// striking bosses like the worm"):
// combo34 (NIGHT34 B leftover 3: a named combo for kills with two or more guns inside one bend, a style bonus), air34 (NIGHT34 B leftover 1: the air / slide style multiplier on points and style only, never damage), bend34 (the grapple BEND: the short and full bends, the meter, instant swaps inside a bend), bendSplit34 (the split path: the world
// steps 1 in N while the player steps every step), flow34 (the movement patterns, Mover.cs branches), reach34 (the gauntlet's boss margin).
namespace Skygrind.Core
{
    public struct N34Blocks
    {
        public bool bend34, bendSplit34, flow34, reach34, air34, combo34;

        /// <summary>The number of blocks (the tape's NIGHT34 byte, bit i = block i).</summary>
        public const int Count = 6;

        /// <summary>The harnesses' and the Player checks' switch (`--off34`, `-skygrindOff34`; also every older off switch): every block
        /// off whatever the config says. A process-wide switch, set before a sim starts its run.</summary>
        public static bool ForceOff;

        /// <summary>Every block off: NIGHT33's game.</summary>
        public static N34Blocks Off => default;

        /// <summary>Is any block on?</summary>
        public bool Any => bend34 || bendSplit34 || flow34 || reach34 || air34 || combo34;

        /// <summary>The blocks' `enabled` flags in the design config (every block off when absent: NIGHT33's config). bendSplit34 is
        /// design.n34.bend34.split and only counts while bend34 is on.</summary>
        public static N34Blocks From(GameConfig cfg)
        {
            bool bend = cfg.Design("n34.bend34.enabled");
            return new N34Blocks
            {
                bend34 = bend,
                bendSplit34 = bend && cfg.Design("n34.bend34.split"),
                flow34 = cfg.Design("n34.flow34.enabled"),
                reach34 = cfg.Design("n34.reach34.enabled"),
                air34 = cfg.Design("n34.air34.enabled"),
                combo34 = bend && cfg.Design("n34.combo34.enabled"), // the named combo lives inside a bend: only with bend34 on
            };
        }

        /// <summary>Block i (0 bend34, 1 bendSplit34, 2 flow34, 3 reach34, 4 air34, 5 combo34).</summary>
        public bool Flag(int i) => i == 0 ? bend34 : i == 1 ? bendSplit34 : i == 2 ? flow34 : i == 3 ? reach34 : i == 4 ? air34 : i == 5 && combo34;

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
        public static N34Blocks FromBits(int b) => new N34Blocks
        {
            bend34 = (b & 1) != 0, bendSplit34 = (b & 2) != 0, flow34 = (b & 4) != 0, reach34 = (b & 8) != 0, air34 = (b & 16) != 0, combo34 = (b & 32) != 0,
        };

        /// <summary>The names in bit order (logs, the harness's header line).</summary>
        public static readonly string[] Names = { "bend34", "bendSplit34", "flow34", "reach34", "air34", "combo34" };

        public override string ToString()
        {
            if (!Any) return "every NIGHT34 block off";
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
