// NIGHT34 builder B (leftover 1, design.n34.air34): THE AIR MULTIPLIER, the pure rule. ULTRAKILL's movement multiplier (Research/2026-10-11-wave34/
// research/arenas.md section 6, wiki.gg "Style": "rises 2.0x per second up to 3x while sliding or airborne, falls back to 1x in about 0.2 s on
// landing"). It multiplies the style meter's awards (StyleCore.mult34: the meter and the score) and the combat points (Economy boost "air34"), never
// damage. While on it replaces the web's flat style.stateMult (x1.2 in the air states): one movement multiplier, not two.
// The value moves per fixed step: +risePerSecond x dt while airborne or sliding (capped at max), else -(max - 1) / fallSeconds x dt (floored at 1),
// so a full 3x falls to 1x in fallSeconds. The web has no such number (its style state multiplier is a flat 1.2).
namespace Skygrind.Core
{
    /// <summary>design.n34.air34's numbers.</summary>
    public sealed class Air34Rules
    {
        public double max, risePerSecond, fallSeconds;

        public static Air34Rules From(GameConfig cfg)
        {
            const string p = "design.n34.air34.";
            return new Air34Rules { max = cfg.Num(p + "max"), risePerSecond = cfg.Num(p + "risePerSecond"), fallSeconds = cfg.Num(p + "fallSeconds") };
        }
    }

    /// <summary>The multiplier's state (one per run).</summary>
    public sealed class Air34
    {
        public Air34Rules rules;

        /// <summary>The multiplier now, 1 .. max.</summary>
        public double mult = 1;

        /// <summary>The top value this run (the harness).</summary>
        public double peak = 1;

        public void Reset(Air34Rules r)
        {
            rules = r;
            mult = peak = 1;
        }

        /// <summary>One fixed step: `moving` = airborne or sliding.</summary>
        public void Step(bool moving, double dt)
        {
            if (rules == null) return;
            if (moving) mult = JsMath.Min(rules.max, mult + rules.risePerSecond * dt);
            else mult = JsMath.Max(1, mult - (rules.fallSeconds > 0 ? (rules.max - 1) / rules.fallSeconds * dt : rules.max));
            peak = JsMath.Max(peak, mult);
        }
    }
}
