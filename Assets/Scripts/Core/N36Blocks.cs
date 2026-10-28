// NIGHT36 (the shared commit, the lead): the nine sim-changing NIGHT36 design blocks a run reads, one bit each, so every sim-changing
// NIGHT36 feature can be switched off (NIGHT36.md "every n36 row behind N36Blocks"). With every block off the game is NIGHT35's bit for
// bit (`--off36` / `-skygrindOff36`; every older off switch also turns these off). Read once from Design/unity-config.json (each block's
// `enabled`); a run takes them at its start (SurvivalSim.N36), a tape records them (format 19: a VarI after the NIGHT35 byte, since nine
// bits do not fit a byte; formats 1-18 replay with every block off). View and flow rows (muzzle36, fist36, text36, bossmodel36,
// level36) need no bit: a level swap is on the tape as the level id.
// The blocks (GG 2026-10-12 after n35-r4: "the pistol is not satisfying anymore", "the crosshair fires normally but then the gun doesn't
// fire out the gun", "an understanding of parry mechanics", "you just teleport into these random rooms", the wall ride "you kind of
// hook onto a wall and you kind of just start sliding up it"; DECIDE36 D-1 movement is the weapon):
// tap36 + crunch36 (builder F: the tap fires on the press, a hold charges a second shot; body-hit freezes), tells36 + parry36 +
// launch36 + heal36 (builder P: yellow / red attack tells, the parry's payoff, a moving punch launches enemies into bursts, close kills
// heal), wallgrip36 (builder M: a wall grips and never lifts you; Mover.cs's wallride35 branch), walkin36 + boss36 (builder B: the boss
// fights in the level's last room with no Respawn cut; the shared boss shape and THE LANTERN on it).
namespace Skygrind.Core
{
    public struct N36Blocks
    {
        public bool tap36, crunch36, tells36, parry36, launch36, heal36, wallgrip36, walkin36, boss36;

        /// <summary>The number of blocks (the tape's NIGHT36 value, bit i = block i).</summary>
        public const int Count = 9;

        /// <summary>The harnesses' and the Player checks' switch (`--off36`, `-skygrindOff36`; also every older off switch): every block
        /// off whatever the config says. A process-wide switch, set before a sim starts its run.</summary>
        public static bool ForceOff;

        /// <summary>Every block off: NIGHT35's game.</summary>
        public static N36Blocks Off => default;

        /// <summary>Is any block on?</summary>
        public bool Any => Bits != 0;

        /// <summary>The blocks' `enabled` flags in the design config (every block off when absent: NIGHT35's config).</summary>
        public static N36Blocks From(GameConfig cfg) => new N36Blocks
        {
            tap36 = cfg.Design("n36.tap36.enabled"),
            crunch36 = cfg.Design("n36.crunch36.enabled"),
            tells36 = cfg.Design("n36.tells36.enabled"),
            parry36 = cfg.Design("n36.parry36.enabled"),
            launch36 = cfg.Design("n36.launch36.enabled"),
            heal36 = cfg.Design("n36.heal36.enabled"),
            wallgrip36 = cfg.Design("n36.wallgrip36.enabled"),
            walkin36 = cfg.Design("n36.walkin36.enabled"),
            boss36 = cfg.Design("n36.boss36.enabled"),
        };

        /// <summary>Block i in Names order.</summary>
        public bool Flag(int i)
        {
            switch (i)
            {
                case 0: return tap36;
                case 1: return crunch36;
                case 2: return tells36;
                case 3: return parry36;
                case 4: return launch36;
                case 5: return heal36;
                case 6: return wallgrip36;
                case 7: return walkin36;
                case 8: return boss36;
                default: return false;
            }
        }

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
        public static N36Blocks FromBits(int b) => new N36Blocks
        {
            tap36 = (b & 1) != 0, crunch36 = (b & 2) != 0, tells36 = (b & 4) != 0, parry36 = (b & 8) != 0, launch36 = (b & 16) != 0,
            heal36 = (b & 32) != 0, wallgrip36 = (b & 64) != 0, walkin36 = (b & 128) != 0, boss36 = (b & 256) != 0,
        };

        /// <summary>The names in bit order (logs, the harness's header line).</summary>
        public static readonly string[] Names = { "tap36", "crunch36", "tells36", "parry36", "launch36", "heal36", "wallgrip36", "walkin36", "boss36" };

        public override string ToString()
        {
            if (!Any) return "every NIGHT36 block off";
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
