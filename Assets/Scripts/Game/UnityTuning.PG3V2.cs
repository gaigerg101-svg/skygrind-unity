// PG3 builder V2 (LIGHT AND MOTION): Unity-only numbers for the two view rows trail74 (Trail74.cs, Trail74Math.cs) and rim74 (Rim74.cs, Rim74Math.cs).
// CLAUDE.md rule 5: each number has a comment; the web never had any of them, and the sim never reads them (view only: the golden vectors,
// the replays and the fingerprints cannot move). Every number is a STARTING value: nothing was run (no Unity, no Player: the lead's pair decides).
using UnityEngine;

namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // ---- trail74: movement that reads at speed (Trail74.cs). Cyan is the player's colour; the dust is the web's neutral slamDust ----
        public const uint N74TrailSeedSalt = 0x74726c31; // "trl1": the cosmetic dice's salt (a render-side Mulberry32 stream, reseeded each run; never a sim stream)
        public const int N74TrailRingPool = 12; // flat / wall / dash rings alive at once (a slam lays 2, a wall jump 1, a dash 1)
        public const int N74TrailPuffPool = 72; // soft dust puffs alive at once (a slam lays up to 18, a wall jump 5)
        public const int N74TrailSparkPool = 200; // (pg3-g: 140 a second now) // sparks alive at once (the slide's 70 a second x 0.5 s is about 35; a slam adds up to 20)
        public const int N74TrailSegPool = 128; // the slide's ground-rail segments alive at once (two rails, one segment per 0.45 m each for 0.75 s: 83 at 25 m/s)
        public const int N74TrailFlashPool = 6; // impact flares alive at once
        public const int N74TrailSpriteCap = 72; // quads one sprite bucket (a colour at a brightness step) can hold in a frame
        public const float N74RingMidFrac = 0.80f; // V2Draw's ring texture (V1's recipe): the bright band is centred at 0.8 of the half size: a wall or dash ring of radius r is drawn at r / 0.8
        public const float N74RingOuterFrac = 0.92f; // the band's outer edge (0.8 + 2 sigma of 0.075, then the 0.93 fade): a floor ring is drawn at radius / 0.92 so its OUTER edge is the radius
        public const float N74FxGain = 1.6f; // x the pale cyan (and the cyan rails): colours are HDR and the post stack blooms over 1; the pg3-c / pg3-d pair frames showed no V2 sprite at the 8-step bucket brightness this replaces
        public const int N74DrawMaxQuads = 400; // quads one view may submit in a frame (one Graphics.RenderMesh each, like V1's Fx74Draw: a slide's rails and sparks peak near 100)
        public const int N74DrawTexSize = 128; // px a side of V2Draw's baked textures (V1's size class: smooth gradients, bilinear)
        public const float N74EchoMaxPartM = 1.2f; // m: the longest diagonal of a viewmodel part the dash echo copies (a gun's parts are well under it; a world-sized effect card is never an echo). The echo also copies only OPAQUE-queue parts (the pg3-c square: effect quads redrawn as solid cyan)
        public const float N74NearEyeM = 0.5f; // m: a puff, spark or flash closer to the eye than this is not drawn (a wall jump is born 0.4 m from the lens; at that range a sprite is a screen-sized card)
        public const int N74EchoMeshCap = 128; // meshes of the viewmodel the dash echoes copy (a gun, its parts and the left arm are well under this)

        // dash: the viewmodel's afterimages (the gun's echo, cyan, in the overlay camera) and one soft ring pulse from the crosshair
        public const float N74DashRiseS = 0.04f; // s for the dash's share to come up (the dash is a hit of speed: no soft entry)
        public const float N74DashFallS = 0.20f; // s for it to go after the dash ends (the echoes trail off, they do not cut)
        public static readonly float[] N74DashGhostAlpha = { 0.34f, 0.20f, 0.10f }; // PG3-G (the lead: no pair showed it): the first echo 0.34 x 1.6 = 0.55 (the web's own melee afterimages' 0.34 / 0.20 / 0.10); // the three echoes' brightness (x the cyan x N74DashGhostGain): the web's melee afterimages are 0.34 / 0.20 / 0.10 and GG called the fist's cyan smear too much (n36-r3), so these are lower
        public const float N74DashGhostGain = 1.6f; // x the cyan: additive, so over 1 only to read against the gun's own light parts (the web's afterimages: viewmodel.afterimages.glow 1.6)
        public const float N74DashGhostScale = 0.07f; // (pg3-g: was 0.045) // extra scale per echo, about the overlay camera's origin: an echo i is (1 + 0.045 i) x the gun, pushed a little outward from the view's centre (a zoom echo)
        public const float N74DashGhostBack = 0.06f; // (pg3-g: was 0.035) // m per echo opposite the motion in the overlay camera's space (a forward dash pulls the echoes toward the eye, a strafe pushes them to the other side)
        public const float N74DashRingDist = 2.2f; // m ahead of the eye the dash ring is drawn (locked to the camera: a pulse from the crosshair, never a line at the screen's border)
        public const float N74DashRingR0 = 0.45f; // m: the ring's radius at its birth
        public const float N74DashRingR1 = 3.4f; // m: its radius at its end (past the frame's edge at 2.2 m: the ring leaves through the border as it fades)
        public const float N74DashRingLife = 0.22f; // s
        public const float N74DashRingPeak = 0.30f; // additive brightness of the ring at its peak (x the pale cyan); a soft pulse, never a flash

        // slide: a spark trail at the feet (sparks thrown back and up from the heels, a glowing ground streak left along the path)
        public const float N74SlideCueFloor = 0.35f; // the least share of the slide's speed the trail draws at once past a walk (the existing cue's rule: it reads from the first frame)
        public const float N74SlideSegMinM = 0.45f; // m of slide between two ground-rail segments (the segments chain end to end: no gaps)
        public const float N74SlideSegLifeS = 0.75f; // s a ground-rail segment glows and fades
        public const float N74SlideSegOverlap = 1.1f; // x a segment's length the strip is drawn (the beam texture is even along its length with soft ends: 1.1 makes neighbours overlap a little into one continuous rail)
        public const float N74SlideSegWidthM = 0.70f; // PG3-G: twice as wide (was 0.35): // m across a ground-rail segment's quad (the beam texture's hot core and soft shoulder: about 10 cm of bright light on the floor)
        public const float N74SlideSegPeak = 0.90f; // additive brightness of a fresh segment (x the cyan; PG3-G: twice, was 0.45; x the gain 1.6 a fresh rail is about (0.14, 1.2, 1.3))
        public const float N74SlideRailSideM = 0.90f; // PG3-G: was 0.50; // m each side of the path the two rails lie (the first-person eye cannot see its own heels: the rails sit where the lower view looks)
        public const float N74SlideRailAheadM = 2.5f; // PG3-G (was 0.3 m, which lies under the camera and behind it a frame later: no still ever held one): the rails are laid 2.5 m AHEAD of the feet, on the floor 0.9 m either side of the path, where the lower third of the view looks; the slider runs past them (lane marks that flash by), clear of the crosshair
        public const float N74SlideSparkRate = 140f; // sparks a second at the full slide speed (x the speed share k; PG3-G: twice, was 70)
        public const float N74SlideSparkAheadMin = 1.0f, N74SlideSparkAheadMax = 3.5f; // m ahead of the feet a spark is born (PG3-G: was 0.2-0.9, under the lens)
        public const float N74SlideSparkSideMin = 0.5f, N74SlideSparkSideMax = 1.2f; // m to a side of the path it is born (either side; PG3-G: was 0.35-0.6)
        public const float N74SlideSparkBackMin = 0.6f, N74SlideSparkBackMax = 2.0f; // m/s back along the slide, in the WORLD (the sparks hang where they are born and the slider runs past them)
        public const float N74SlideSparkUpMin = 1.0f, N74SlideSparkUpMax = 2.8f; // m/s thrown up
        public const float N74SlideSparkSide = 1.1f; // m/s sideways (+-)
        public const float N74SlideSparkLifeMin = 0.30f, N74SlideSparkLifeMax = 0.55f; // s
        public const float N74SparkGravity = 11f; // m/s^2 the sparks fall at (a little under the web's cubes: they stay bright longer)
        public const float N74SparkWidthM = 0.20f; // PG3-G: twice (was 0.10): // m across a spark's streak quad (V2Draw's streak texture: a hot thin core and a soft shoulder, about 3 cm of bright core)
        public const float N74SparkStreakS = 0.06f; // (PG3-G: was 0.035) // s of its velocity a spark's streak spans (the streak's tail)
        public const float N74SparkPeak = 0.9f; // additive brightness of a spark at birth (x the pale cyan)

        // slam: the shockwave ring (its outer edge IS the sim's slam.shockwaveRadius: the ring never claims reach the blast lacks), dust and shards sized by the fall
        public const float N74SlamFallMinM = 2.0f; // m of fall at which the effects start to grow (a hop's slam is the smallest)
        public const float N74SlamFallMaxM = 14.0f; // m of fall at which they are whole
        public const float N74SlamRingR0 = 0.7f; // m: the ring's radius at its birth
        public const float N74SlamRingLife = 0.42f; // s
        public const float N74SlamRingPeakMin = 0.90f, N74SlamRingPeakMax = 1.40f; // PG3-G: a 4 m fall (0.18 of the range) must read: the floor is 0.90 (was 0.45), x the gain 1.6; // additive brightness at a small / a whole fall (x the pale cyan)
        public const float N74SlamPoolShare = 0.9f; // x the shockwave radius: the cyan pool's radius (inside the ring's outer edge: it never claims reach the blast lacks)
        public const float N74SlamPoolLife = 0.5f; // s
        public const float N74SlamPoolPeak = 0.55f; // additive brightness at the pool's centre (x the cyan x the gain 1.6), whatever the fall: it is the floor that makes a 4 m slam read
        public const float N74SlamInnerFall01 = 0.40f; // from this share of the fall a second, inner ring follows
        public const float N74SlamInnerShare = 0.55f; // the inner ring's radius at its end, as a share of the shockwave radius
        public const float N74SlamInnerLife = 0.30f; // s
        public const float N74SlamFlashMin = 2.4f, N74SlamFlashMax = 5.0f; // m across the impact flare at a small / a whole fall
        public const float N74SlamFlashLife = 0.14f; // s
        public const float N74SlamFlashPeak = 0.8f; // additive brightness of the flare
        public const int N74SlamDustMin = 8, N74SlamDustMax = 18; // puffs rolled out along the floor at a small / a whole fall
        public const float N74SlamDustSpeedMin = 3.2f, N74SlamDustSpeedMax = 7.0f; // m/s outward at birth (the drag slows them)
        public const float N74SlamDustSizeMin = 0.8f, N74SlamDustSizeMax = 1.5f; // m across a puff at birth
        public const float N74SlamDustLifeMin = 0.60f, N74SlamDustLifeMax = 0.95f; // s
        public const int N74SlamShardsMin = 8, N74SlamShardsMax = 20; // sparks thrown up and out at a small / a whole fall
        public const float N74SlamShardSpeedMin = 4.0f, N74SlamShardSpeedMax = 10.0f; // m/s

        // wall jump: a kick puff where the feet left the wall
        public const int N74WallPuffs = 8; // dust puffs thrown out from the wall (PG3-G: was 5)
        public const int N74WallSparks = 8; // sparks flicked off the wall (pale cyan)
        public const float N74WallHeightM = 1.0f; // m above the feet where the kick is drawn (the hip: where the feet and hands were)
        public const float N74WallRingR0 = 0.15f, N74WallRingR1 = 1.0f; // m: the kick ring's radius, born to end (a ring lying on the wall's face)
        public const float N74WallRingLife = 0.26f; // s
        public const float N74WallRingPeak = 0.50f; // additive brightness of the kick ring (x the pale cyan)
        public const float N74WallPuffSizeMin = 1.0f, N74WallPuffSizeMax = 1.7f; // m across a puff at birth (PG3-G: was 0.5-0.9)
        public const float N74WallPuffLife = 0.45f; // s
        public const float N74WallSparkSpeedMin = 2.5f, N74WallSparkSpeedMax = 5.5f; // m/s along the wall's normal, away from it

        // puffs (the dust): peak brightness, the drag and the growth
        public const float N74PuffPeak = 0.60f; // (PG3-G: was 0.40) // additive brightness of a dust puff at its fullest (x colors.slamDust)
        public const float N74PuffDrag = 3.2f; // 1/s: a puff's drift slows as v e^-kt (the existing dust's PuffDrag is of the same family)
        public const float N74PuffGrowFrom = 0.55f, N74PuffGrowTo = 1.45f; // a puff's size share at its birth and its end (it blooms and thins)

        // ---- rim74: enemies read against the dusk (Rim74.cs). Red-orange is the enemies' colour (CLAUDE.md rule 8); the key on a boss is a warmer cousin of it ----
        public const int N74GlowTexSize = 64; // px a side of the radial glow texture made at run time (a smooth gradient: 64 is plenty, bilinear)
        // pg3-c's Player pair (charge35 f0129: a husk at 6-8 m) showed NO halo: the first glow was 4.7 m across at a peak of 0.16 x a distance share of 0.26 = 0.04 additive, far under the dusk's own noise. This one is an
        // ellipse that HUGS the pill (0.4 m past its sides, 0.7 m past its ends) and is about ten times brighter at 6-25 m: a warm edge, not a fog.
        public const float N74HaloHalfWidthRadii = 2.0f; // x the body's radius: the glow's HALF width (the husk's 0.4 m radius: 0.8 m, so 0.4 m of glow past each side; at the silhouette's side the texture is at 0.56 of its peak)
        public const float N74HaloHalfHeightRatio = 1.7f; // x half the body's height: the glow's HALF height (the husk's 0.9 m: 1.5 m, 0.6 m past the ends)
        public const float N74HaloMaxSizeM = 7f; // m: the glow's largest full height (a tall body does not paint a wall)
        public const float N74HaloBehindRadii = 0.45f; // x the body's radius the glow sits behind it, away from the eye (the body's own pixels hide its middle: the depth test)
        public const float N74HaloPeak = 1.0f; // additive brightness at the texture's centre (x colors.enemy, red-orange; HDR, the post stack blooms over 1): the lead's target is rgb (0.55, 0.20, 0.05) at the brightest texel at 6-25 m: 1.0 x enemy (1.0, 0.23, 0.11) x share 0.6-1 gives (0.6-1.0, 0.14-0.23, 0.07-0.11); about 0.5 at the silhouette's side
        public const float N74HaloNearM = 6f; // m: closer than this the glow is at N74HaloNearShare of its peak (up close the enemy is already the brightest warm thing)
        public const float N74HaloFarM = 18f; // m: from here out the glow is whole (the Valorant rule: a distance-scaled rim keeps a body readable at range)
        public const float N74HaloNearShare = 0.60f; // the share of the peak at N74HaloNearM (from there inward the close fade below takes it to nothing)
        public const float N74HaloCloseM0 = 1.0f, N74HaloCloseM1 = 3.5f; // m: the glow is gone at 1 m and whole again from 3.5 m (the rim56 header: a whole-body "jelly" glow close up was removed once; an enemy at arm's length is lit by its own rim and ink, never a halo)
        public const float N74HaloMaxM = 75f; // m: no glow past this (the pill is a few pixels there; the rim's job is the mid range)
        public const float N74HaloMaxFadeM = 10f; // m: the glow fades out over the last 10 m before the cap (an enemy crossing it does not pop)
        public const float N74HaloNearScale = 0.70f; // the glow's size share at 0 m: it shrinks a little toward the eye
        public const float N74HaloFullSizeM = 8f; // m from which the glow is its whole size
        public const int N74HaloCrowdFree = 8; // glows drawn at full strength; past this many the glows thin as sqrt(8 / n) (24 husks in a knot would add up to an orange fog)
        public const float N74HaloCrowdEaseS = 0.4f; // s the thinning eases over (a death or a spawn in the crowd never pops the others)
        public const float N74HaloFadeInS = 0.15f; // s for a glow to come up from its start share on a new enemy (the first design's 0.35 s from nothing never finished in the reels: the lead's probe read fade 0.03-0.1)
        public const float N74HaloFadeStart = 0.6f; // the share a glow starts at (a spawn pops to 60 % of a subtle glow, then eases up: a reel's frozen frames can never leave it near 0)
        public const float N74HaloFadeOutS = 0.20f; // s for it to go when the enemy dies or leaves view's list
        public const int N74HaloPool = 64; // glows tracked at once (a bucket's capacity is N74TrailSpriteCap-like: one per enemy)

        // the boss entrance light beat: a warm key swells on the boss for about 2 s, then settles
        public const float N74BossKeyRiseS = 0.45f; // s from the trigger to the peak (a swell, not a flash)
        public const float N74BossKeyHoldS = 0.55f; // s at the peak
        public const float N74BossKeySettleS = 1.00f; // s from the peak down to the settled level (the swell is about 2 s in all)
        public const float N74BossKeySettleLevel = 0.30f; // the settled share of the peak the key keeps while the boss lives
        public const float N74BossKeyFadeOutS = 0.90f; // s for the key to go when the boss dies (or never is found)
        public const float N74BossKeyFindS = 3.0f; // s after the trigger the boss's body is looked for (a boss may rise a moment after its card)
        public const float N74BossKeyDedupeS = 1.0f; // s: the intro card and the rise are one entrance (bossIntro and bossRise both fire)
        public const float N74BossKeyLight = 9f; // the Light's intensity at the peak, x LightScale (a point light; the crown's fire is 3, the kit's glow light is on the same scale)
        public const float N74BossKeyRangeMin = 10f, N74BossKeyRangeMax = 20f, N74BossKeyRangePerHeight = 4f; // m: the light's range = 4 x the boss's height, between these
        public const float N74BossKeyFrontRadii = 1.2f; // x the body's radius the light sits toward the player (a key from the player's side)
        public const float N74BossKeyUpHeights = 0.25f; // x the body's height the light sits above its centre
        public static readonly Color N74BossKeyColor = new Color(1f, 0.416f, 0.227f, 1f); // #FF6A3A (hue 14 degrees): a warm red-orange, the enemies' colour a step warmer; not the parry's yellow, not magenta (the RIVAL's)
        // pg3-c's bosskill72 reel: the light lit 661 frames (peak 2.86) and no frame showed it: the boss's surfaces are unlit shaders. So the beat is two sprites that need no light:
        public const float N74BossKeyGlowSizePerHeight = 3.0f; // x the boss's height: the aura BEHIND the boss (was 2.2 x at 0.34: the body hid nearly all of it)
        public const float N74BossKeyGlowMaxSizeM = 22f; // m: the aura's largest
        public const float N74BossKeyGlowPeak = 0.90f; // additive brightness of the aura at the peak (x the key's colour); it keeps the settled share (0.30) while the boss lives
        public const float N74BossKeyWashSizePerHeight = 1.8f; // x the boss's height: the wash IN FRONT of the boss, over its own surfaces (what a key light on an unlit body looks like)
        public const float N74BossKeyWashPeak = 0.30f; // additive brightness of the wash at the swell's peak (x the key's colour); it follows the swell only (none once settled)
        public const float N74BossKeyWashFrontRadii = 1.3f; // x the body's radius (at least 0.3 x its height) the wash sits toward the eye: in front of the whole body, so the body cannot hide it
        public const float N74BossKeyWashMaxAngleDeg = 50f; // degrees of view the wash may span at most (GG's NIGHT43: a screen-filling flash is a bug)
    }
}
