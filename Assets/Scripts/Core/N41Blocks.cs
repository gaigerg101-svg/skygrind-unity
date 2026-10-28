// NIGHT41 (the shared commit, the lead): the eight sim-changing NIGHT41 design blocks a run reads, one bit each, so every NIGHT41 row can
// be switched off (NIGHT41.md method 3). With every block off the game is NIGHT40's bit for bit (`--off41` / `-skygrindOff41`; every older
// off switch also turns these off). Read once from Design/unity-config.json (each block's `enabled`); a run takes them at its start
// (SurvivalSim.N41), a tape records them (format 24: a VarI after the NIGHT40 value; formats 1-23 replay with every block off). The view
// rows (juice41, juice41On, doors41On, pump41On, bars41, entrance41, select41, look41, brawlLook41, models41) change only what is drawn: no
// bit (Game/N41View.cs).
// The blocks (GG 2026-10-14 ~08:40, verbatim in Research/2026-10-14-wave41/GG-REQUEST.md; INVENTORY.md; DECIDE41):
// sweep41 (B8: THE LOOM sweeps its canyon floor), tokens41 (E6: at most two enemies winding up at once in the campaign), heavy41 (E6: the
// gauntlet answers harder on a big enemy), aim41 (P3: aim assist finds a boss's weak point), foundry41 (R5: level 1 as n41-foundry, a no-ride
// band on THE GANTRY's b2-g1 wall), kiln41 (E6: level 3 as n41-kiln, a CHARGER room), spire41 (R5: level 5 as n41-spire, grapple nodes and a
// DRONE room), finale41 (L6: the level 6 greybox as n41-finale; OFF tonight, GG reacts first: DECIDE41 D-3).
namespace Skygrind.Core
{
    public struct N41Blocks
    {
        public bool sweep41, tokens41, heavy41, aim41, foundry41, kiln41, spire41, finale41;

        /// <summary>The number of blocks (the tape's NIGHT41 value, bit i = block i).</summary>
        public const int Count = 8;

        /// <summary>The harnesses' and the Player checks' switch (`--off41`, `-skygrindOff41`; also every older off switch): every block
        /// off whatever the config says. A process-wide switch, set before a sim starts its run.</summary>
        public static bool ForceOff;

        /// <summary>Every block off: NIGHT40's game.</summary>
        public static N41Blocks Off => default;

        /// <summary>Is any block on?</summary>
        public bool Any => Bits != 0;

        /// <summary>The names in bit order (logs, the harness's header line, the design rows `design.n41.<name>`).</summary>
        public static readonly string[] Names = { "sweep41", "tokens41", "heavy41", "aim41", "foundry41", "kiln41", "spire41", "finale41" };

        /// <summary>The blocks' `enabled` flags in the design config (every block off when absent: NIGHT40's config).</summary>
        public static N41Blocks From(GameConfig cfg)
        {
            var b = new N41Blocks();
            for (int i = 0; i < Count; i++) b.Set(i, cfg.Design("n41." + Names[i] + ".enabled"));
            return b;
        }

        /// <summary>Block i in Names order.</summary>
        public bool Flag(int i)
        {
            switch (i)
            {
                case 0: return sweep41;
                case 1: return tokens41;
                case 2: return heavy41;
                case 3: return aim41;
                case 4: return foundry41;
                case 5: return kiln41;
                case 6: return spire41;
                case 7: return finale41;
                default: return false;
            }
        }

        void Set(int i, bool v)
        {
            switch (i)
            {
                case 0: sweep41 = v; break;
                case 1: tokens41 = v; break;
                case 2: heavy41 = v; break;
                case 3: aim41 = v; break;
                case 4: foundry41 = v; break;
                case 5: kiln41 = v; break;
                case 6: spire41 = v; break;
                case 7: finale41 = v; break;
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
        public static N41Blocks FromBits(int b)
        {
            var r = new N41Blocks();
            for (int i = 0; i < Count; i++) r.Set(i, (b & (1 << i)) != 0);
            return r;
        }

        public override string ToString()
        {
            if (!Any) return "every NIGHT41 block off";
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
