// PG3 V3 (KILL POP; Pop74.cs, Pop74Math.cs): Unity-only numbers (CLAUDE.md rule 5: each with a comment; the web never had a kill pop, the sim never reads any of these,
// so the golden vectors, the replays and the fingerprints stay as they were). Every number is a STARTING value: nothing was run (no Unity, no Player: the lead's pair decides).
// "Unit" below is the dying body's height in metres (Pop74Math.BodyM) x the tier's scale x the far growth: a husk's unit is 1.8. Colours are HDR multipliers of the enemy's own
// red-orange (colors.enemy: the config's) or white-hot (that colour lerped toward white); the player's cyan is never used (CLAUDE.md rule 8).
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // ---- pop74: pools and caps ----
        public const int PG3V3FlashPool = 48; // real-clock sprites (core, halo, star: 3 a kill, about 5 frames each) alive at once
        public const int PG3V3SpritePool = 96; // world-clock sprites (the rings and the scorch: up to 4 a kill, the scorch lives 1.5 s) alive at once
        public const int PG3V3ShardPool = 128; // chunky shards alive at once; also the most shard quads one frame draws (a quad each)
        public const int PG3V3EmberPool = 192; // embers alive at once (they share the Fx74 kit's per-frame quad budget with the sprites)
        public const int PG3V3MaxKills = 32; // kills one frame can queue (a horde blast); the rest are dropped
        public const uint PG3V3SeedSalt = 0x70673376; // "pg3v": the cosmetic dice's salt (XOR the run seed; gameplay randomness is untouched)
        public const int PG3V3ShardTexSize = 64; // px: the shard texture's side (a hard-edged polygon with a bright rim: it needs little)

        // ---- pop74: body size by kind (m; the kind's height: the config's husk.height 1.8, swarmer.height 0.75, spitter.height 2, charger.height 2.2, warden.height 2.3, lancer.height 2,
        //      the drone's 2 x radius 0.6, the burrower's 2.2). A kind not listed gets no pop: the bosses (kind "boss" and their bodies' own kinds) and the RIVAL (magenta) are left alone ----
        public const float PG3V3BodyHuskM = 1.8f;
        public const float PG3V3BodySwarmerM = 0.75f;
        public const float PG3V3BodySpitterM = 2.0f;
        public const float PG3V3BodyChargerM = 2.2f;
        public const float PG3V3BodyWardenM = 2.3f;
        public const float PG3V3BodyDroneM = 1.2f;
        public const float PG3V3BodyBurrowerM = 2.2f;
        public const float PG3V3BodyLancerM = 2.0f;

        // ---- pop74: the tier (KillConfirmEvent.tier: 0 plain, 1 head or melee, 2 multikill) ----
        public const float PG3V3Tier1K = 1.3f; // x the unit for a head / melee kill
        public const float PG3V3Tier2K = 1.65f; // x the unit for a multikill (and it throws a second ring)
        public const float PG3V3FarRefM = 8f; // m: from this far the pop grows with distance so it reads (a fixed size would be a speck at 25 m)
        public const float PG3V3FarMaxK = 1.8f; // x: the most it grows
        public const float PG3V3LiftMaxM = 0.4f; // m: the flash stands this much on the camera's side of the body (never buried in the chunks)

        // ---- pop74: the core flash (3-5 frames: real time, so a hit freeze does not hold it) ----
        public const float PG3V3FlashS = 0.09f; // s: 5.4 frames at 60 fps (the retake: held 4-6 frames)
        public const int PG3V3FlashMinFrames = 4; // rendered frames the flash lives for however short the frame time is (a 30 fps frame is 33 ms: four of them is 0.13 s)
        public const float PG3V3FlashMaxS = 0.18f; // s: the longest a slow frame stretches it
        public const float PG3V3FlashHold = 0.50f; // share of the flash's life it stays whole before the squared fall (Fx74Math.Env): about 2.7 frames at full, then the fall
        public const float PG3V3CoreFromK = 0.90f, PG3V3CoreToK = 1.50f; // x the unit: the white-hot core's size at the start and the end (a husk at 5 m: 1.6 -> 2.7 m, the retake's ~1.5x the body's height)
        public const float PG3V3CoreGain = 8f; // HDR: the core (white-hot with the enemy's red in it; blooms hard for those frames)
        public const float PG3V3HaloFromK = 1.30f, PG3V3HaloToK = 2.40f; // x the unit: the red-orange halo round the core (the white centre runs into red-orange)
        public const float PG3V3HaloGain = 2.8f; // HDR: the halo (the enemy's red-orange)
        public const float PG3V3StarFromK = 1.6f, PG3V3StarToK = 2.6f; // x the unit: the lens-flare star over the core
        public const float PG3V3StarGain = 5.5f; // HDR: the star (red-orange toward white)
        public const float PG3V3StarS = 0.10f; // s: the star (a frame longer than the core)
        public const float PG3V3WhiteMix = 0.85f; // 0..1: how far "white-hot" is from the enemy's red-orange toward white (0.85 reads white with an orange edge)
        public const float PG3V3CapSoftDeg = 26f; // deg of view: the most a soft flash may subtend (GG: no full-screen flash; was 18, raised for the retake: at 5-15 m the flash is under it, it bites inside about 5 m; V1's muzzle caps at 20)
        public const float PG3V3CapStarDeg = 34f; // deg: the most the star may subtend (V1's star cap)
        public const float PG3V3CapRingDeg = 56f; // deg: the most a ring may subtend (V1's ring cap: a thin line, it may sweep out toward the screen's edge, never past it much)

        // ---- pop74: the rings (world clock: a hit freeze holds the ring at its start) ----
        public const float PG3V3RingReachK = 1.1f; // x the unit: the ring's final radius (a husk: 1.98 m, the card's ~2 m; a swarmer 0.83)
        public const float PG3V3RingBand = 0.8f; // the V1 ring texture's band sits at 0.8 of the quad's half size (Fx74.cs MakeTextures), so a quad is 2 x radius / 0.8 wide
        public const float PG3V3RingFromK = 0.30f; // x the final radius: the ring's size at its birth
        public const float PG3V3RingS = 0.26f; // s
        public const float PG3V3RingGain = 6.0f; // HDR: the ring at its start (white-hot)
        public const float PG3V3RingEdge = 1.6f; // HDR: the ring's red-orange as it goes out
        public const float PG3V3RingThickK = 0.84f; // x the ring's size: a companion ring this much smaller drawn with it, so the band is about twice as thick (the V1 ring texture's band is thin)
        public const float PG3V3RingThickGain = 0.8f; // x the ring's brightness for that companion
        public const float PG3V3Ring2K = 1.45f; // x the first ring's radius: a multikill's second ring
        public const float PG3V3Ring2DelayS = 0.05f; // s of real time before the second ring starts
        public const float PG3V3Ring2S = 0.30f; // s
        public const float PG3V3Ring2Gain = 4.0f; // HDR: the second ring (a little under the first)

        // ---- pop74: the scorch (a hot mark on the floor that cools; additive, so it glows, it does not darken: the dark mark is NIGHT51's scorch51) ----
        public const float PG3V3FloorMaxM = 6f; // m: the floor is searched this far below the body; no floor inside it, no scorch
        public const float PG3V3FloorMinNormalY = 0.5f; // the surface found must face up at least this much (a floor, not a wall's face)
        public const float PG3V3ScorchLiftM = 0.015f; // m off the floor (no z-fight with it; V1's decal lift is 0.012)
        public const float PG3V3ScorchFromK = 1.10f, PG3V3ScorchToK = 1.80f; // x the unit: the scorch's size as it forms and as it cools (a husk: 2 -> 3.2 m wide quad, the glow is its middle 60 %)
        public const float PG3V3ScorchS = 2.0f; // s: it cools from hot to nothing over this (the card says short-lived)
        public const float PG3V3ScorchHold = 0.30f; // share of its life it stays whole before the squared fall (the retake: a clear patch, not a flicker)
        public const float PG3V3ScorchGain = 3.5f; // HDR: the scorch at its hottest (the enemy's red-orange)
        public const float PG3V3ScorchCool = 0.5f; // HDR: the scorch's red as it goes out

        // ---- pop74: the shards (chunky, angular, spinning, in the enemy's own red-orange; they fall and bounce on the floor under the body) ----
        public const int PG3V3Shards0 = 12, PG3V3Shards1 = 17, PG3V3Shards2 = 24; // shards of a plain / head-or-melee / multikill pop (the card: 10-24)
        public const int PG3V3MinShards = 4; // the fewest a crowded frame's lightest pop throws
        public const float PG3V3ShardSizeMinK = 0.12f, PG3V3ShardSizeMaxK = 0.26f; // x the unit: a shard's width (a husk: 0.22-0.47 m; the retake: twice the first numbers)
        public const float PG3V3ShardMinM = 0.12f; // m: the narrowest a shard is (a swarmer's would be a speck)
        public const float PG3V3ShardAspect = 1.5f; // a shard is this much taller than it is wide (it reads as a splinter, not a square)
        public const float PG3V3ShardGain = 4.8f; // HDR: a shard (red-orange; the texture's rim carries the brightest part)
        public const float PG3V3ShardSpeedMin = 5.5f, PG3V3ShardSpeedMax = 13.0f; // m/s at a husk's size (x Pop74Math.ShardSpeedK): thrown wider than the first 3.5-9
        public const float PG3V3ShardSpeedRefM = 1.8f; // m: the body size those speeds are for
        public const float PG3V3ShardSpeedKMin = 0.6f, PG3V3ShardSpeedKMax = 1.3f; // the speed's scale limits (a swarmer's shards are not slow, a warden's not wild)
        public const float PG3V3ShardUp = 0.55f; // how far the throw leans up (so they arc, then fall)
        public const float PG3V3ShardLean = 0.30f; // how far the throw leans toward the camera (so they read on the screen)
        public const float PG3V3ShardLifeMin = 0.70f, PG3V3ShardLifeMax = 1.20f; // s
        public const float PG3V3ShardHold = 0.40f; // share of a shard's life it stays whole before the squared fall
        public const float PG3V3ShardSpinMin = 360f, PG3V3ShardSpinMax = 900f; // deg/s of roll (either way)
        public const float PG3V3ShardDrag = 0.5f; // 1/s: air drag (fx.gravity, the config's, pulls them down)
        public const float PG3V3ShardBounce = 0.35f; // x: the share of its fall speed a shard keeps off the floor
        public const float PG3V3ShardFriction = 0.6f; // x: the share of its sideways speed a bounce keeps
        public const float PG3V3ShardBounceMinSpeed = 1.2f; // m/s: a slower landing comes to rest
        public const float PG3V3ShardFloorLiftM = 0.03f; // m: a shard rests this far above the floor (it is a flat quad: it does not sink in)
        public const float PG3V3PreRollS = 0.025f; // s of flight a shard and an ember have already flown when first drawn (a hit freeze holds the very first frame: a burst, not a ball)

        // ---- pop74: the embers (small hot streaks: they drift out and fall with gravity) ----
        public const int PG3V3Embers0 = 10, PG3V3Embers1 = 15, PG3V3Embers2 = 22; // embers of a plain / head-or-melee / multikill pop
        public const int PG3V3MinEmbers = 3; // the fewest a crowded frame's lightest pop throws
        public const float PG3V3EmberSpeedMin = 2.0f, PG3V3EmberSpeedMax = 7.0f; // m/s (wider than the first 1.2-4.5)
        public const float PG3V3EmberLifeMin = 0.60f, PG3V3EmberLifeMax = 1.10f; // s
        public const float PG3V3EmberLenM = 0.24f; // m: an ember's longest streak (a slow one is a dot: Fx74's stretch rule)
        public const float PG3V3EmberWidthM = 0.055f; // m
        public const float PG3V3EmberGain = 6.5f; // HDR: an ember (red-orange to white-hot)
        public const float PG3V3EmberHotMix = 0.55f; // 0..1: the most an ember is lerped toward white (never yellow: gold is points only)
        public const float PG3V3EmberGravityK = 0.5f; // x fx.gravity: embers fall lighter than shards
        public const float PG3V3EmberDrag = 1.4f; // 1/s: air drag on an ember

        // ---- pop74: a crowded frame (a blast's horde) draws fewer shards and embers per pop, never fewer flashes ----
        public const int PG3V3LodFullKills = 3; // the first this-many kills of a frame throw their full counts
        public const int PG3V3LodHalfKills = 6; // kills up to this index throw half; beyond it a quarter
        public const float PG3V3LodHalfK = 0.5f, PG3V3LodFarK = 0.25f;
        public const float PG3V3CrowdFlashHalfK = 0.7f, PG3V3CrowdFlashFarK = 0.5f; // x: the flashes' brightness (core, halo, star, rings) of the pops past the first few of a frame: they overlap on one spot of the screen

        // ---- pop74: the light ----
        public const float PG3V3Light0 = 1.8f, PG3V3Light1 = 2.4f, PG3V3Light2 = 3.2f; // brief point light peak (LightPools.Flash intensity) of a plain / head-or-melee / multikill pop (V1's burst light is 2.6)
        public const float PG3V3LightRangeM = 5f; // m: its reach
        public const float PG3V3LightS = 0.09f; // s: how long it lasts
        public const float PG3V3LightNearM = 1.2f; // m: no light from a pop this close to the eye (it would light the whole view)
    }
}
