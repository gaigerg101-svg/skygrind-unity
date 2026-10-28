// NIGHT49 builder X (design.n49.fx49, a view row: no sim bit; NIGHT49.md G5; GG 2026-10-19: "The effects are inconsistent it all needs to be
// unified"). THE IMPACT KIT's numbers: one tiers x owners table (a size, a brightness and a shape per cell) and one timing table (rise / hold /
// fade in whole sim steps). Sizes and timings only: the colours are UnityTuning.Look49.cs's role tokens (the one palette; Fx49.cs reads them).
// The rule (Research/2026-10-19-wave49/UNIFY.md section 4, after Riot's VFX guide): more damage = bigger, brighter, sharper; "if everything is
// important, nothing is important": a light hit stays small and short so a heavy one and a critical one read over it. The hitstop is the
// sim's (ImpactRules / SurvivalSim.Hitstop): nothing here touches it. Render numbers (CLAUDE.md rule 5): each with its comment.
namespace Skygrind.Game
{
    /// <summary>NIGHT49 X: the kit's tiers (more damage = bigger, brighter, sharper).</summary>
    public enum Fx49Tier { Light = 0, Heavy = 1, Critical = 2 }

    /// <summary>NIGHT49 X: whose effect it is (its colour from Look49's tokens, its shape from the owner's shape language).</summary>
    public enum Fx49Owner { Player = 0, Gauntlet = 1, Enemy = 2, Parry = 3 }

    /// <summary>NIGHT49 X: the owner's shape language (UNIFY.md section 4): the player's clean round flares and rings; the gauntlet's ring;
    /// the enemy's sharp shards (the pill language's hard edges); the parry's four-point star (ULTRAKILL's parry spark).</summary>
    public enum Fx49Shape { Round = 0, Ring = 1, Shard = 2, Star = 3 }

    /// <summary>NIGHT49 X: one cell of the tiers x owners table.</summary>
    public readonly struct Fx49Cell
    {
        public readonly float size, glow, core, sparkLen, sparkWidth;
        public readonly int sparks;

        public Fx49Cell(float size, float glow, float core, int sparks, float sparkLen, float sparkWidth)
        {
            this.size = size;
            this.glow = glow;
            this.core = core;
            this.sparks = sparks;
            this.sparkLen = sparkLen;
            this.sparkWidth = sparkWidth;
        }
    }

    public static partial class UnityTuning
    {
        // ---- the tiers x owners table: [owner * 3 + tier] ----
        // size: the main flare's span at its peak (m, at the hit; the ring's diameter for a ring shape). glow: the HDR x on the owner's colour
        // (the bloom threshold picks up heavy and critical; a light hit sits near it). core: the white share of the hot core (0 = the pure role
        // colour, 1 = white): a critical hit is white-hot at its heart, a light one keeps its hue. sparks: the streaks thrown (more = sharper).
        // sparkLen / sparkWidth: a streak's length and width (m): longer and THINNER up the tiers (sharper).
        public static readonly Fx49Cell[] Fx49Cells =
        {
            // Player (cyan-white, Look49UsableHex): KESTREL, THUNDERCLAP, the fist, the TORCH, a kill
            new Fx49Cell(0.30f, 1.6f, 0.45f, 4, 0.16f, 0.030f), // light: a KESTREL tap, a pellet, the fist (under fx47's 0.5 m kill flare: a hit never outshines a kill)
            new Fx49Cell(0.55f, 2.2f, 0.60f, 6, 0.28f, 0.026f), // heavy: a KESTREL headshot / charged shot, the slug, the cone, the boost, a plain kill (fx47's kill flare 0.5 m, glow 2.4)
            new Fx49Cell(1.10f, 2.6f, 0.80f, 10, 0.60f, 0.016f), // critical: a headshot / melee / boss kill, a boss weak point (round 2, D-17: was 0.90 m / 3.0 / 9 / 0.45 / 0.020: bigger and sharper, not brighter)
            // Gauntlet (the PULSE's pink, Fx47BodyHex until Look49 has a token: see the report): the PULSE punch and the LT blast
            new Fx49Cell(0.34f, 1.7f, 0.40f, 4, 0.16f, 0.034f), // light: a glancing gauntlet hit (a wave's share)
            new Fx49Cell(0.62f, 2.4f, 0.55f, 7, 0.30f, 0.028f), // heavy: the PULSE punch (fx47's aura at 0.32 m x 1.25 x 1.4 = 0.56 m body)
            new Fx49Cell(1.20f, 2.8f, 0.75f, 10, 0.65f, 0.018f), // critical: the LT blast's core (round 2: was 1.00 / 3.2 / 0.50 / 0.022)
            // Enemy (red-orange, Look49DangerHex): an enemy's hit on the player (shards, never a round flare)
            new Fx49Cell(0.26f, 1.5f, 0.20f, 5, 0.20f, 0.028f), // light: a chip of damage (a little hue kept: the danger reads red, not white)
            new Fx49Cell(0.46f, 2.0f, 0.35f, 7, 0.32f, 0.022f), // heavy: a normal blow
            new Fx49Cell(0.80f, 2.8f, 0.50f, 10, 0.50f, 0.018f), // critical: a big blow (a boss, a heavy kind)
            // Parry (yellow, Look49ParryHex): the parry's star (4 points: ULTRAKILL's melee parry spark; 8 sparks on the critical: its ranged one)
            new Fx49Cell(0.30f, 1.8f, 0.50f, 4, 0.18f, 0.030f), // light: (not used in round 1: every parry is critical)
            new Fx49Cell(0.60f, 2.6f, 0.65f, 4, 0.30f, 0.026f), // heavy: a returned glob's burst
            new Fx49Cell(1.30f, 3.0f, 0.85f, 8, 0.70f, 0.016f), // critical: the parry itself, the biggest and sharpest thing a player makes (round 2: was 1.10 / 3.4 / 0.55 / 0.020)
        };

        /// <summary>The darker rim tone: the owner's colour x this (value only: the hue stays in its role).</summary>
        public const float Fx49RimShare = 0.55f;
        /// <summary>The soft halo behind a heavy / critical flare: its span x the cell's size, its brightness x the glow (light: no halo).</summary>
        public const float Fx49HaloSpan = 1.8f, Fx49HaloShare = 0.35f;
        /// <summary>The ring a round / ring / star shape draws: its end radius x the cell's size (the ring grows 0 -> it on the timing's rise + hold).</summary>
        public const float Fx49RingSpan = 1.2f;

        // ---- the timing table: [tier] in whole sim steps (120 Hz: 1 step = 8.3 ms) ----
        // rise: snap-in to the peak (1-2 steps: "tell me loud and tell me fast", DOOM); hold: the peak held (more for a bigger hit: higher-impact
        // effects get more attention-grabbing timing, UNIFY.md); fade: a quadratic ease-out to 0. Every effect's flash reads these; the motion
        // of a burst (debris, embers) keeps its own physics but takes the kit's colours.
        public static readonly int[] Fx49RiseSteps = { 1, 1, 2 }; // light / heavy / critical
        public static readonly int[] Fx49HoldSteps = { 2, 4, 6 }; // 17 / 33 / 50 ms at the peak
        public static readonly int[] Fx49FadeSteps = { 12, 20, 32 }; // 100 / 167 / 267 ms out: whole lives 125 / 208 / 333 ms (ULTRAKILL's 0.25 s major hitstop sits inside the critical)

        // ---- the guards (NIGHT49.md section 1's two boxes, at 1080p; scaled by the screen's height) ----
        /// <summary>The reticle box: no effect sprite reaches within this many px (1080p) of the screen centre (the ticks' end, the Hud38BarY comment).</summary>
        public const float Fx49ReticlePx = 18f;
        /// <summary>NIGHT50 G4 (DECIDE50 D-7): the web's enemy death burst (SurvivalGame.Init.cs, the kill listener) is drawn at this share of its size when fx49 is on, so the kill's wash stays inside the wash-box rule (+0.25 or less). A pick: measured on n50-r1's kill frames (NOT YET CHECKED); 1 = the web's size (fx49 off).</summary>
        public const float Fx49KillBurstScale = 0.5f;
        /// <summary>NIGHT52 K (T-111, NIGHT50 G4 SHORT: the enemy kill's wash +0.27 on f8 and +0.25 on f6 with fx49 on, +0.28 / +0.26 off): with fx49
        /// AND skin52 on (Chunks.KillWhiteCap: fx49 is ON in the shipping config and -skygrindOff52 does not cover it, so the NIGHT52 enemy surface
        /// row gates it), a gun kill's blocks (Chunks.Break) are white for at most this many drawn frames from the kill, then ease to the enemy's colour over
        /// CrunchWhiteFade s. Without it a kill from KillFarWhiteFrom (3 m) on holds the body white (x KillWhiteHdr, x KillHoldScale 1.25 in size)
        /// through the whole freeze (impact42's killS 0.085 s: ~5 frames at 60 fps), then KillWhiteFramesGun (3) frames more as the blocks fly
        /// apart: f5-f9, after the crush disc (frames 1-4) has gone, in the middle of the wash box. 4 = the kill burst's frames (KillBurstFullFrames
        /// + KillBurstFadeFrames): the white body under the disc kept, none after it. -1 = NIGHT18's look (white through the hold). Not read with
        /// fx49 off. A pick from the code's timeline: the a249 kill's wash with it is NOT CHECKED (the lead's reel).</summary>
        public static readonly int Fx49KillWhiteFrames = 4;
        /// <summary>The wash box's half extents in screen shares (the middle 20 %: 384 x 216 px at 1080p, FeelReel.N31.Grapple.cs:301).</summary>
        public const float Fx49WashHalf = 0.10f;
        /// <summary>A ring's least radius and a spark's least start, x the reticle box's radius at the burst's depth (a hitscan hit is always on the
        /// aim: its ring's band, at 0.54 of the sprite's radius / KenneyRingMid, sits clear of the 18 px box).</summary>
        public const float Fx49RingClear = 1.4f;
        /// <summary>A sprite whose centre is inside the wash box draws at this share of its brightness and size, per tier light / heavy / critical
        /// (the wash meter: +0.25 over f-2 at most). Round 2 (D-17): the critical's least (0.55 / 0.75 for every tier in round 1): a critical burst
        /// reads by its size and its sparks OUTSIDE the box, not by a brighter middle.</summary>
        // NIGHT50 (reverted to the NIGHT49 values): the heavy tier at 0.24 share and 0.40 size left the kill's wash at +0.27 (n50-r2, n50-r3 a249)
        public static readonly float[] Fx49WashKTier = { 0.55f, 0.45f, 0.30f };
        public static readonly float[] Fx49WashSizeTier = { 0.75f, 0.65f, 0.50f };
        /// <summary>Round 3 (D-17; n49-r2 a249 ON: the kill +0.32, the TORCH +0.30, the PULSE punch +0.31 over the +0.25 bar):
        /// a kit ring's end radius never subtends more than this from the eye (deg; KillFeel42's own cap was Kills42RingMaxDeg);
        /// no burst light within Fx49LightNearM of the eye (m); KillFeel42's extra kill debris x Fx49KillDebrisShare (its colour, the enemy's,
        /// unchanged: fewer bits, the kit's sparks carry the kill); the TORCH's puffs inside the wash box at the light tier's wash share and its
        /// wall light x Fx49TorchLightShare (the flame outside the box unchanged: it reads by its length).</summary>
        public const float Fx49RingMaxDeg = 12f, Fx49LightNearM = 2.5f, Fx49KillDebrisShare = 0.5f, Fx49TorchLightShare = 0.5f;
        /// <summary>Round 2 (D-17): the KESTREL cannon (Pistol39View, the charged shot) on the kit with fx49 on: its beam's sleeve and core widths x
        /// these (0.32 / 0.09 m: a 60 px sleeve at 5 m down the middle of the wash box), its glows the critical cell's (sleeve x Fx49BeamSleeveShare);
        /// sharper (a thin hot line), not brighter. Its muzzle blast and shock ring are capped by Fx49.WashCap (their edge outside the box).</summary>
        public const float Fx49BeamSleeveWidth = 0.35f, Fx49BeamCoreWidth = 0.55f, Fx49BeamSleeveShare = 0.35f;

        // ---- the tier rules (which tier an event is) ----
        /// <summary>A player hit's tier by its damage (HitEvent.damage): under Heavy = light (a KESTREL tap 25, a pellet 8, a punch 10), from it heavy
        /// (a headshot 50, a charged shot, the slug), from Critical critical. The defaults when design.n49.fx49.numbers has none.</summary>
        public const double Fx49HitHeavyAt = 40, Fx49HitCriticalAt = 150;
        /// <summary>An enemy's hit on the player (HurtEvent.amount): the same ladder on the player's side.</summary>
        public const double Fx49HurtHeavyAt = 15, Fx49HurtCriticalAt = 35;
        /// <summary>The enemy-hit shards' distance ahead of the eye toward the blow (m) and their least angle off the aim (deg: outside the reticle box).</summary>
        public const float Fx49HurtAhead = 1.6f, Fx49HurtMinDeg = 9f;
        /// <summary>The enemy-hit shards' size x the cell's (they sit 1.6 m from the eye, not at a body 5-20 m away: a third keeps them a burst, not a wall).</summary>
        public const float Fx49HurtScale = 0.35f;
        /// <summary>The THUNDERCLAP's muzzle cone: its three flares x the heavy cell's size (0.55 m), x 0.6 / 1.0 / 1.4 along the aim (fx47's
        /// Fx47ConeSize 0.6 m at the same steps; a touch smaller since it now holds the heavy tier's 4 steps instead of 33 ms).</summary>
        public const float Fx49ConeScale = 0.8f;
        /// <summary>A boss weak point's burst x the critical cell (1.1 m): a boss is hit from 10-25 m, so its read is the critical drawn 2x
        /// (2.2 m, feel43's 2.2 m white star it replaces): the biggest burst in the game, as the boss's open window is its biggest moment.</summary>
        public const float Fx49BossWeakScale = 2f;
        /// <summary>A critical burst with no enemy (a boss weak point) drops the lesser bursts of its frame within this many m (one picture a moment).</summary>
        public const float Fx49MergeM = 1.5f;
        /// <summary>The TORCH's flame tips: the player's rim tone x this (the jet fades into the dusk, not into a second hue).</summary>
        public const float Fx49TorchTipShare = 0.55f;
        /// <summary>A floor ring's (the THUNDERCLAP's boost) end radius x the cell's size (fx47's boost ring was 1.6 m: heavy 0.55 x 3 = 1.65 m).</summary>
        public const float Fx49FloorRingSpan = 3f;
        /// <summary>The short light a heavy / critical burst throws (LightPools' flash slot: never a new Light): intensity (fx47's hit light 2.5),
        /// range (m) per tier; its life is the tier's rise + hold. A light burst throws none (if everything lights the room, nothing does).</summary>
        public const double Fx49LightHeavy = 2.5, Fx49LightCritical = 4.0;
        public static readonly double[] Fx49LightRange = { 0, 4, 6 };
        /// <summary>The cosmetic generator's salt (view randomness: never the sim's Mulberry32 stream).</summary>
        public const uint Fx49SeedSalt = 0x0F49A11u;
        /// <summary>The kit's own pool: sprites live at once (the hurt shards, the parry star, the kit's flares).</summary>
        public const int Fx49Pool = 48;
    }
}
