// NIGHT55 K (builder K): Unity-only numbers of kill55 / finish55 / speed55 / brief55 (pictures, sizes, UI layout, the a355 reel's line). The
// rows' pacing numbers (the holds, the windows, the slow-mo) are in design.n55.<row>.numbers (Design/unity-config.json).
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // --- kill55 (2): the debris that stays (Kill55Debris.cs) ---
        /// <summary>Shell shards a kill throws (the chunks' blocks still break the body and shrink out within ChunkLife 1.45 s; these land and
        /// stay). 10: the critic's ">= 8 pieces lasting >= 1.5 s" with two to spare when a shard falls out of view.</summary>
        public const int Kill55ShardsPerKill = 10;
        /// <summary>Extra shards per chain step past the first (the 3rd kill of a chain throws 2 x this more), at most Kill55ShardsChainMax extra.</summary>
        public const int Kill55ShardsPerChain = 2, Kill55ShardsChainMax = 6;
        /// <summary>The resting ring's size: the oldest shard recycled first (the decal pool's rule, github.md item 5). One draw per shard:
        /// 128 keeps a 24-husk fight's extra draws under the chunks' own 256-block pool.</summary>
        public const int Kill55ShardCap = 128;
        /// <summary>A shard's size (m): the plate's long side between min and max, its thickness x ThickShare of it.</summary>
        public const float Kill55ShardMin = 0.07f, Kill55ShardMax = 0.17f, Kill55ShardThickShare = 0.35f;
        /// <summary>The throw (m/s): out from the kill point and up, between min and max, x the overkill-free speed mult (speed55 raises it).</summary>
        public const float Kill55ShardSpeedMin = 2.5f, Kill55ShardSpeedMax = 5.5f, Kill55ShardUpMin = 2.0f, Kill55ShardUpMax = 4.5f;
        /// <summary>Along the shot's level direction, as a share of the throw (the pieces fly away from the gun).</summary>
        public const float Kill55ShardAlong = 0.6f;
        /// <summary>The floor: restitution, ground friction (x per bounce), the speed under which a shard lies still (m/s).</summary>
        public const float Kill55ShardBounce = 0.3f, Kill55ShardFriction = 0.55f, Kill55ShardRest = 0.7f;
        /// <summary>The spin while flying (deg/s) between min and max.</summary>
        public const float Kill55ShardSpinMin = 240f, Kill55ShardSpinMax = 720f;
        /// <summary>A shard rests this long (s of world time) before it shrinks out over FadeS; one that never lands ends at FlightMaxS.</summary>
        public const float Kill55ShardRestS = 9f, Kill55ShardFadeS = 0.6f, Kill55ShardFlightMaxS = 5f;
        /// <summary>The ray's start above the kill's feet when a falling shard looks for its floor (m), and the drop that sends it looking again.</summary>
        public const float Kill55ShardProbeUp = 0.6f, Kill55ShardProbeSlack = 0.4f;
        /// <summary>The shards' colours (shares of the kill's enemy colour): the shell x ShellValue, a charred inside, a hot edge cooling over HotS.
        /// All in the enemy red-orange family (CLAUDE.md rule 8: never cyan).</summary>
        public const float Kill55ShellValue = 0.8f, Kill55CharShare = 0.3f, Kill55HotShare = 0.2f, Kill55HotS = 1.6f;
        public const uint Kill55CharHex = 0x2A0F0Bu, Kill55HotHex = 0xFF7A2Eu;
        /// <summary>The rim (SKYGRIND/Enemy's _RimOn) on a shard: a touch, so a resting shard reads as a piece of the enemy, not a hole.</summary>
        public const float Kill55ShardRim = 0.6f;

        // --- kill55 (3): the damage shown on a hit husk (Kill55Damage.cs) ---
        /// <summary>Marks kept per husk (a 4th hit replaces its oldest) and in all (the pool).</summary>
        public const int Kill55MarksPerHusk = 3, Kill55MarkPool = 96;
        /// <summary>A hit within this distance (m, on the body) of an old mark grows that mark instead of adding one.</summary>
        public const float Kill55MarkMerge = 0.22f;
        /// <summary>The health share at or under which a mark is a hole (stage 2: a piece knocked off), above it a crack (stage 1).</summary>
        public const float Kill55HoleAt = 0.5f;
        /// <summary>The crack star: the scorch's radius, the strips' count, length and width (m, on the shell). NIGHT56 G (G2): x1.5 (0.11 / 0.24 /
        /// 0.04): on n56-r1's hit frames the marks read as 0.33 % dark changed body pixels ON vs 0.02 % OFF (a355 hit-before / hit-after, my read).</summary>
        public const float Kill55ScorchR = 0.165f, Kill55CrackLen = 0.36f, Kill55CrackWidth = 0.06f;
        public const int Kill55CrackStrips = 6;
        /// <summary>The hole: its radius and its hot rim's width (m). NIGHT56 G (G2): x1.5 (0.14 / 0.035), as the crack.</summary>
        public const float Kill55HoleR = 0.21f, Kill55RimWidth = 0.05f;
        /// <summary>The marks stand this far off the shell (m) against z-fighting (the hot parts HotLift more). NIGHT56 G (G2): 0.016 -> 0.045: r1's
        /// hit-after frame showed almost none of the dark crack (0.33 % of the body); a part of the model standing over ShellR is a guess (not
        /// checked), and 4.5 cm off the shell is still on the body at the reel's 5 m.</summary>
        public const float Kill55MarkLift = 0.045f, Kill55MarkHotLift = 0.005f;
        /// <summary>The marks' colours: the crack and the hole near black (a burnt shell), the hot core and rim red-orange cooling to a dark ember over
        /// CoolS (the enemy family: rule 8).</summary>
        // NIGHT56 G (G2 attempt 2): the ember 0x5A160A (V 0.35) -> 0x1A0604 (V 0.10) and CoolS 1.2 -> 0.35: n56-r2's a355 read the mark pixels at
        // +0.5 s as 0.12 % with the changed pixels' median H 3 S 0.99 V 0.55 (the hot core still glowing over the crack, not a dark mark): the
        // core now flashes hot on the hit and has cooled to a burnt, near-black ember by +0.35 s (still the enemy family's hue).
        public const uint Kill55CrackHex = 0x160807u, Kill55HoleHex = 0x070303u, Kill55MarkHotHex = 0xFF5A1Eu, Kill55MarkEmberHex = 0x1A0604u;
        public const float Kill55MarkCoolS = 0.35f;
        /// <summary>NIGHT57 G (G3): the hot core's cooling in this many material copies (hot .. ember; Kill55Damage's own copies of the body
        /// material, made once: the colour no longer rides a property block). 6 steps over CoolS 0.35 s: one step every ~4 frames at 60 fps.</summary>
        public const int Kill55MarkHotSteps = 6;
        /// <summary>The torso band a mark is kept in (shares of the body's height): never on the head, never on the feet.</summary>
        public const float Kill55MarkLow = 0.3f, Kill55MarkHigh = 0.72f;
        /// <summary>The decal mesh's curvature radius (m): the husk's shell (husk.radius 0.4).</summary>
        public const float Kill55MarkCurve = 0.4f;

        // --- brief55 (P1-teachText: UiBrief55.cs) ---
        /// <summary>The gap (px at the 1920 x 1080 reference) between the room counter's plate and the top-left column under it (the lesson card,
        /// the hint, the toasts): UnityTuning.LeftColumnGap's role between two cards.</summary>
        public const float Brief55CounterGap = 12f;

        // --- the a355 reel (FeelReel.N55.Kill.cs) ---
        /// <summary>a355: the chain's husks (1 hp, held in place), their distance ahead (m) and spacing across (m).</summary>
        public const int Kill55ReelChain = 6;
        public const double Kill55ReelAhead = 7, Kill55ReelAcross = 1.6;
        /// <summary>a355: frames waited after the chain before counting the debris (1.5 s at 60 fps) and the frames after a hit the damage is read at
        /// (0.5 s: the white hit flash and the kick are over).</summary>
        public const int Kill55ReelDebrisWait = 90, Kill55ReelDamageWait = 30;
        /// <summary>a355: a body pixel changed when a channel moved by this much (0-1), and the pass share (5 %).</summary>
        public const float Kill55ReelPixelDelta = 0.12f, Kill55ReelPixelShare = 0.05f;
        /// <summary>a355: the spaced pass (off a chain): frames between shots (0.67 s at 60 fps: over the chain gap, 0.55 s since NIGHT56 G2).</summary>
        public const int Kill55ReelSpacedEvery = 40;
        /// <summary>a355 (NIGHT56 G2): frames recorded after the chain's last kill, so its hold is counted (n56-r1: kill 6 read 0 frames held on
        /// both sides because the loop ended on its frame); 20 frames = 0.33 s at 60 fps, over move54's 0.12 s kill cap.</summary>
        public const int Kill55ReelChainTail = 20;
        /// <summary>a355 (NIGHT56 G2): the damage line's mark measure: a body pixel (warm on the before frame) that changed and is this dark (HSV V)
        /// at +0.5 s is a mark pixel (the crack / hole: Kill55CrackHex V 0.09, Kill55HoleHex V 0.03); GREEN wants this share of the body pixels
        /// (n56-r1: ON 0.33 %, OFF 0.02 %, my read of hit-before / hit-after).</summary>
        public const float Kill55ReelMarkV = 0.12f, Kill55ReelMarkShare = 0.01f;
        /// <summary>a355 (NIGHT57 G, G3): the MAIN's share: the next frame's mark pixels (changed and dark, V under Kill55ReelMarkV) over the body's
        /// warm pixels (NIGHT57.md section 0 rule 5: "hit marks >= 5 % of the body the next frame"). A hole (Kill55HoleR 0.21 m, jagged) on a
        /// husk 5 m ahead covers ~12 % of its body by area if it shows in full.</summary>
        public const float Kill55ReelMarkNextShare = 0.05f;
    }
}
