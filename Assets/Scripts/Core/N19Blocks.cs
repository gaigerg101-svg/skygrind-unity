// NIGHT19 (the shared commit, the lead): the seven NIGHT19 design blocks a run reads, one bit each, so every NIGHT19
// gameplay change can be switched off (NIGHT19.md rule 0.2). With every block off the game is NIGHT18's bit for bit: the
// A side for every judge, and the fingerprint proves it (SimHarness `fingerprint --off19` against FingerprintBaselineN18).
// Read once from Design/unity-config.json (each block's `enabled`); a run takes them at its start (SurvivalSim.n19), a
// tape records them (format 4; formats 1-3 replay with every block off) and the harnesses force them off with ForceOff.
// The blocks: design.arsenal (W: niches, ports, combos), design.gust (W: the KESTREL's LT), design.forge (W: the
// sidegrade forge), design.freshness (W: points only), design.stamina.n19 (E: the drain, the refill, the range rewards;
// the lead: the sprint key), design.enemies.n19 (E: CHARGER, DRONE, SHIELDED), design.pad.n19 (P: the v2 layout).
namespace Skygrind.Core
{
    public struct N19Blocks
    {
        public bool arsenal, gust, forge, freshness, stamina, enemies, pad;

        /// <summary>The number of blocks (the tape's NIGHT19 byte, bit i = block i).</summary>
        public const int Count = 7;

        /// <summary>The harnesses' and the Player checks' switch (`--off19`, `-skygrindOff19`): every block off whatever the
        /// config says. A process-wide switch, set before a sim starts its run.</summary>
        public static bool ForceOff;

        /// <summary>Every block off: NIGHT18's game.</summary>
        public static N19Blocks Off => default;

        /// <summary>Is any block on?</summary>
        public bool Any => arsenal || gust || forge || freshness || stamina || enemies || pad;

        /// <summary>The blocks' `enabled` flags in the design config (every block off when it's absent: NIGHT18's config).</summary>
        public static N19Blocks From(GameConfig cfg) => new N19Blocks
        {
            arsenal = cfg.Design("arsenal.enabled"),
            gust = cfg.Design("gust.enabled"),
            forge = cfg.Design("forge.enabled"),
            freshness = cfg.Design("freshness.enabled"),
            stamina = cfg.Design("stamina.n19.enabled"),
            enemies = cfg.Design("enemies.n19.enabled"),
            pad = cfg.Design("pad.n19.enabled"),
        };

        /// <summary>Block i (0 arsenal, 1 gust, 2 forge, 3 freshness, 4 stamina, 5 enemies, 6 pad).</summary>
        public bool Flag(int i) => i == 0 ? arsenal : i == 1 ? gust : i == 2 ? forge : i == 3 ? freshness : i == 4 ? stamina : i == 5 ? enemies : i == 6 && pad;

        /// <summary>The blocks as a byte (bit i = block i): what a tape records.</summary>
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

        /// <summary>The blocks from a tape's byte.</summary>
        public static N19Blocks FromBits(int b) => new N19Blocks
        {
            arsenal = (b & 1) != 0, gust = (b & 2) != 0, forge = (b & 4) != 0, freshness = (b & 8) != 0,
            stamina = (b & 16) != 0, enemies = (b & 32) != 0, pad = (b & 64) != 0,
        };

        /// <summary>The names in bit order (logs, the harness's header line).</summary>
        public static readonly string[] Names = { "arsenal", "gust", "forge", "freshness", "stamina.n19", "enemies.n19", "pad.n19" };

        public override string ToString()
        {
            if (!Any) return "every NIGHT19 block off";
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
