// NIGHT33 (the shared commit, the lead): the seven sim-changing NIGHT33 design blocks a run reads, one bit each, so every sim-changing
// NIGHT33 feature can be switched off (NIGHT33.md "every sim change behind design.n33"). With every block off the game is NIGHT32's bit
// for bit (`--off33` / `-skygrindOff33`; every older off switch also turns these off). Read once from Design/unity-config.json (each
// block's `enabled`); a run takes them at its start (SurvivalSim.N33), a tape records them (format 16: one byte after the NIGHT32 byte,
// then the shop's owned upgrades and the points banked at the run's start; formats 1-15 replay with every block off). View and flow rows
// (hud33, look33, kit33, arenas33, view33) need no bit: a level swap is on the tape as the level id.
// The blocks (GG 2026-10-11: enemies "run into lava", ULTRAKILL-style trigger spawns, bosses "still way too tanky", the grapple should
// "grapple you towards the enemy ... you don't go through it ... a little damage", a stronger piercing pistol charge, "more elements",
// campaign points that buy something):
// ai33 (the lava mask and the edge guard), spawns33 (trigger spawns and the spawn tell), bosses33 (the 15-22 s pilot band), whip33 (the
// whip), pierce33 (the charged KESTREL pierces), devices33 (updraft fan, hazard vent, moving platform), shop33 (the carried bank and the
// shop's upgrades applied at the run's start).
namespace Skygrind.Core
{
    public struct N33Blocks
    {
        public bool ai33, spawns33, bosses33, whip33, pierce33, devices33, shop33;

        /// <summary>The number of blocks (the tape's NIGHT33 byte, bit i = block i).</summary>
        public const int Count = 7;

        /// <summary>The harnesses' and the Player checks' switch (`--off33`, `-skygrindOff33`; also every older off switch): every block
        /// off whatever the config says. A process-wide switch, set before a sim starts its run.</summary>
        public static bool ForceOff;

        /// <summary>Every block off: NIGHT32's game.</summary>
        public static N33Blocks Off => default;

        /// <summary>Is any block on?</summary>
        public bool Any => ai33 || spawns33 || bosses33 || whip33 || pierce33 || devices33 || shop33;

        /// <summary>Does any block that changes the composed SURVIVAL run (and so the fingerprint) read as on? (ai33 changes every enemy's
        /// nav; whip33 changes the grapple wherever it is owned; pierce33 changes the KESTREL's charged shot, which the fingerprint's script
        /// fires; the others act only in campaign levels.)</summary>
        public bool SimChanging => ai33 || whip33 || pierce33;

        /// <summary>The blocks' `enabled` flags in the design config (every block off when absent: NIGHT32's config).</summary>
        public static N33Blocks From(GameConfig cfg) => new N33Blocks
        {
            ai33 = cfg.Design("n33.ai33.enabled"),
            spawns33 = cfg.Design("n33.spawns33.enabled"),
            bosses33 = cfg.Design("n33.bosses33.enabled"),
            whip33 = cfg.Design("n33.whip33.enabled"),
            pierce33 = cfg.Design("n33.pierce33.enabled"),
            devices33 = cfg.Design("n33.devices33.enabled"),
            shop33 = cfg.Design("n33.shop33.enabled"),
        };

        /// <summary>Block i (0 ai33, 1 spawns33, 2 bosses33, 3 whip33, 4 pierce33, 5 devices33, 6 shop33).</summary>
        public bool Flag(int i) => i == 0 ? ai33 : i == 1 ? spawns33 : i == 2 ? bosses33 : i == 3 ? whip33 : i == 4 ? pierce33 : i == 5 ? devices33 : i == 6 && shop33;

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
        public static N33Blocks FromBits(int b) => new N33Blocks
        {
            ai33 = (b & 1) != 0, spawns33 = (b & 2) != 0, bosses33 = (b & 4) != 0, whip33 = (b & 8) != 0, pierce33 = (b & 16) != 0, devices33 = (b & 32) != 0, shop33 = (b & 64) != 0,
        };

        /// <summary>The names in bit order (logs, the harness's header line).</summary>
        public static readonly string[] Names = { "ai33", "spawns33", "bosses33", "whip33", "pierce33", "devices33", "shop33" };

        public override string ToString()
        {
            if (!Any) return "every NIGHT33 block off";
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
