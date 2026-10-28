// NIGHT20 (the shared commit, the lead): the eight NIGHT20 design blocks a run reads, one bit each, so every NIGHT20
// gameplay change can be switched off (NIGHT20.md rule 0.2). With every block off the game is NIGHT19's bit for bit: the
// A side for every judge (`--off20` / `-skygrindOff20`); `--off19` turns off NIGHT19's and NIGHT20's blocks (NIGHT18's
// game, the fingerprint's path). Read once from Design/unity-config.json (each block's `enabled`); a run takes them at its
// start (SurvivalSim.n20), a tape records them (format 5; formats 1-4 replay with every block off) and the harnesses force
// them off with ForceOff.
// The blocks: design.shockwave (G: the THUNDERCLAP's LT, 18 s; replaces design.gust on the KESTREL), design.zoom (G: zoom
// on most guns), design.regen.n20 (E: the round-end refill, the segments, the earned heals), design.routes (N: the extra
// nav links, the climb, no dead perches), design.director.n20 (E: special rounds, pacing, the camp score), design.arsenal.n20
// (G: tonight's gun retune and ports), design.pad.n20 (P: RB scrolls, layout v3), design.adds (every builder: the
// additions wave, one sub-block each: `design.adds.<row>.enabled` AND this bit).
namespace Skygrind.Core
{
    public struct N20Blocks
    {
        public bool shockwave, zoom, regen, routes, director, arsenal, pad, adds;

        /// <summary>The number of blocks (the tape's NIGHT20 byte, bit i = block i).</summary>
        public const int Count = 8;

        /// <summary>The harnesses' and the Player checks' switch (`--off20`, `-skygrindOff20`; also `--off19`): every block off
        /// whatever the config says. A process-wide switch, set before a sim starts its run.</summary>
        public static bool ForceOff;

        /// <summary>Every block off: NIGHT19's game.</summary>
        public static N20Blocks Off => default;

        /// <summary>Is any block on?</summary>
        public bool Any => shockwave || zoom || regen || routes || director || arsenal || pad || adds;

        /// <summary>The blocks' `enabled` flags in the design config (every block off when it's absent: NIGHT19's config).</summary>
        public static N20Blocks From(GameConfig cfg) => new N20Blocks
        {
            shockwave = cfg.Design("shockwave.enabled"),
            zoom = cfg.Design("zoom.enabled"),
            regen = cfg.Design("regen.n20.enabled"),
            routes = cfg.Design("routes.enabled"),
            director = cfg.Design("director.n20.enabled"),
            arsenal = cfg.Design("arsenal.n20.enabled"),
            pad = cfg.Design("pad.n20.enabled"),
            adds = cfg.Design("adds.enabled"),
        };

        /// <summary>An additions-wave row: the adds bit and its own sub-block (`design.adds.<row>.enabled`).</summary>
        public bool Add(GameConfig cfg, string row) => adds && cfg.Design("adds." + row + ".enabled");

        /// <summary>Block i (0 shockwave, 1 zoom, 2 regen, 3 routes, 4 director, 5 arsenal, 6 pad, 7 adds).</summary>
        public bool Flag(int i) => i == 0 ? shockwave : i == 1 ? zoom : i == 2 ? regen : i == 3 ? routes : i == 4 ? director : i == 5 ? arsenal : i == 6 ? pad : i == 7 && adds;

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
        public static N20Blocks FromBits(int b) => new N20Blocks
        {
            shockwave = (b & 1) != 0, zoom = (b & 2) != 0, regen = (b & 4) != 0, routes = (b & 8) != 0,
            director = (b & 16) != 0, arsenal = (b & 32) != 0, pad = (b & 64) != 0, adds = (b & 128) != 0,
        };

        /// <summary>The names in bit order (logs, the harness's header line).</summary>
        public static readonly string[] Names = { "shockwave", "zoom", "regen.n20", "routes", "director.n20", "arsenal.n20", "pad.n20", "adds" };

        public override string ToString()
        {
            if (!Any) return "every NIGHT20 block off";
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
