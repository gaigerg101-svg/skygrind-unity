// NIGHT29 (the shared commit, the lead): the six sim-changing NIGHT29 design blocks a run reads, one bit each, so every sim-changing
// NIGHT29 feature can be switched off (NIGHT29.md "Switches"). With every block off the game is NIGHT28's bit for bit
// (`--off29` / `-skygrindOff29`; every older off switch also turns these off). Read once from Design/unity-config.json (each block's
// `enabled`); a run takes them at its start (SurvivalSim.n29), a tape records them (format 12: one byte after the NIGHT27 byte;
// formats 1-11 replay with every block off). The view rows (crt, lights, props, doors) need no bit.
// The blocks: lockout (fire and melee cannot overlap, both ways), shotgun (the slower pump), tank (the level-2 tank's vulnerability row),
// boss1 (THE LANTERN reworked: light damage, the floor cycle), boss2 (THE GLACIER reworked: the other-gun fraction, the skate lane, the
// frost / patience rules), doorFree (the hold-RB dev toggle).
namespace Skygrind.Core
{
    public struct N29Blocks
    {
        public bool lockout, shotgun, tank, boss1, boss2, doorFree;
        /// <summary>Round 2 (builder S; tape bits 6 and 7, the byte had room: older tapes read them 0 = off): pickups = a plan's weaponPickup / healthPickup props are collectable; roomRules = the wall-ride room's stand-ins and the 25 % fall cost.</summary>
        public bool pickups, roomRules;

        /// <summary>The number of blocks (the tape's NIGHT29 byte, bit i = block i).</summary>
        public const int Count = 8;

        /// <summary>The harnesses' and the Player checks' switch (`--off29`, `-skygrindOff29`; also every older off switch): every block
        /// off whatever the config says. A process-wide switch, set before a sim starts its run.</summary>
        public static bool ForceOff;

        /// <summary>Every block off: NIGHT28's game.</summary>
        public static N29Blocks Off => default;

        /// <summary>Is any block on?</summary>
        public bool Any => lockout || shotgun || tank || boss1 || boss2 || doorFree || pickups || roomRules;

        /// <summary>Does any block that changes the composed SURVIVAL run (and so the fingerprint) read as on? (lockout and shotgun change
        /// a survival run's combat; the others act only in campaign levels.)</summary>
        public bool SimChanging => lockout || shotgun;

        /// <summary>The blocks' `enabled` flags in the design config (every block off when absent: NIGHT28's config).</summary>
        public static N29Blocks From(GameConfig cfg) => new N29Blocks
        {
            lockout = cfg.Design("n29.lockout.enabled"),
            shotgun = cfg.Design("n29.shotgun.enabled"),
            tank = cfg.Design("n29.tank.enabled"),
            boss1 = cfg.Design("n29.boss1.enabled"),
            boss2 = cfg.Design("n29.boss2.enabled"),
            doorFree = cfg.Design("n29.doorFree.enabled"),
            pickups = cfg.Design("n29.pickups.enabled"),
            roomRules = cfg.Design("n29.roomRules.enabled"),
        };

        /// <summary>Block i (0 lockout, 1 shotgun, 2 tank, 3 boss1, 4 boss2, 5 doorFree).</summary>
        public bool Flag(int i) => i == 0 ? lockout : i == 1 ? shotgun : i == 2 ? tank : i == 3 ? boss1 : i == 4 ? boss2 : i == 5 ? doorFree : i == 6 ? pickups : i == 7 && roomRules;

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
        public static N29Blocks FromBits(int b) => new N29Blocks
        {
            lockout = (b & 1) != 0, shotgun = (b & 2) != 0, tank = (b & 4) != 0, boss1 = (b & 8) != 0, boss2 = (b & 16) != 0, doorFree = (b & 32) != 0, pickups = (b & 64) != 0, roomRules = (b & 128) != 0,
        };

        /// <summary>The names in bit order (logs, the harness's header line).</summary>
        public static readonly string[] Names = { "lockout", "shotgun", "tank", "boss1", "boss2", "doorFree", "pickups", "roomRules" };

        public override string ToString()
        {
            if (!Any) return "every NIGHT29 block off";
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
