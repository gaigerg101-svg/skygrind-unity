// NIGHT18 U3-31.4 (builder R, the hit that lands; M5): the body's reaction to a hit while FeelAB is on (Game/HitSpring.cs,
// EnemyViews.Hit.cs). NIGHT17's flinch (CrunchTiltDeg / CrunchPush / EnemyFlinchSquash, an 11 deg tip away from the shot)
// stays the FeelAB-off path, unchanged. Unity-only, view only: the web has no hit reaction beyond its white flag, so it
// has no such numbers.
// The geometry they were sized on (Tools: the M5 model): at 12 m a 1.8 m husk is about 113 px tall and 50 px wide on a
// 1080 p frame (camera.fovDeg 74 vertical: about 60 px per metre), so M5's 8 px is about 0.13 m in the screen plane.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // --- the spring (HitSpring: x'' = -k x - c x'; a hit adds velocity) ------------------------------------------------
        // Source's view punch (PUNCH_SPRING_CONSTANT 65, PUNCH_DAMPING 9: damping ratio 0.56) gives the shape: a hit adds
        // velocity and the spring overshoots once. Its rate is about 4.8 x Source's so the peak lands 32 ms after the hit
        // and it is back (under 4 % of the peak) by 200 ms, M5's windows; damping ratio 0.46 (a 19 % overshoot: the squash
        // rebounds into a visible stretch, the kick swings a little past rest).
        public const float HitReactSpring = 1500f; // k (1/s^2)
        public const float HitReactDamping = 36f; // c (1/s)

        // --- what a full hit does (a KESTREL body hit, weapons.revolver.damage or more) ---------------------------------------
        // The sideways kick's peak along the shooter's right, away from the side the shot hit (m): about 8 px at 12 m alone.
        public const float HitReactLatPeak = 0.13f;
        // The top leans with the kick, about the feet (deg at the kick's peak; it follows the kick, so it overshoots with it).
        public const float HitReactLeanDeg = 4.5f;
        // The squash's peak (a share of the height: the top drops, 1.8 m x 0.06 = 0.11 m, about 6 px at 12 m), rebounding into
        // a stretch (the spring's overshoot).
        public const float HitReactSquashPeak = 0.06f;
        // The width while squashed: x and z grow by this share of the squash (a near-constant volume read).
        public const float HitReactBulge = 0.5f;
        // Caps on the predicted peak (a shotgun's pellets land in one step and stack up to here, never past it).
        public const float HitReactLatCap = 0.26f;
        public const float HitReactSquashCap = 0.12f;

        // --- how big a hit is --------------------------------------------------------------------------------------------------
        // A hit's share of the full reaction: its damage over the revolver's (weapons.revolver.damage, read from config), never
        // under this (a nailgun's 9 still reads):
        public const float HitReactMinScale = 0.45f;
        // A melee hit, a heavy gun's hit (feel.punch.weapons.<w>.cls == heavy) and a parry's attacker scale it by:
        public const float HitReactMelee = 1.5f;
        public const float HitReactHeavy = 1.5f;
        public const float HitReactParry = 1.6f;
        // The full reaction's damage when the config has no revolver (the revolver's own 25 at NIGHT18):
        public const float HitReactFullDamageFallback = 25f;

        // --- the range (the lead, after pass 1: a fixed 0.13 m is about 60 px of head travel at 3 m and 4.7 px at 20 m) ---------
        // The kick, the lean and the squash above are sized at this distance (m, level, eye to the hit point: hit12's)...
        public const float HitReactRefDistance = 12f;
        // ...and drawn x distance / HitReactRefDistance, so a revolver body hit's screen size holds (about 13 px of centre
        // travel at 1080 p from 3 m to 24 m), within these shares: closer than 3 m the world size holds (the screen size grows
        // as anything's does: about 26 px at 2 m), beyond 24 m it falls (about 10 px at 30 m).
        public const float HitReactRangeMin = 0.25f;
        public const float HitReactRangeMax = 2f;

        // --- the side --------------------------------------------------------------------------------------------------------
        // A hit this close to the body's axis (m, along the shooter's right) is centred: the body keeps the side it was kicked
        // to, or a fresh one takes the enemy id's parity (no randomness).
        public const float HitReactSideDeadZone = 0.06f;

        // --- a stream of hits (a fast gun) holds the body back instead of jerking it ---------------------------------------------
        // A kick within this many s of the previous one on the same enemy (on a later frame) is a stream: its side sticks and it
        // builds the hold (pellets in one step stack the kick, not the hold).
        public const float HitReactStreamWindow = 0.2f;
        // Each stream hit raises the hold by this x its scale (the hold is 0-1):
        public const float HitReactHoldStep = 0.35f;
        // The hold decays at this rate (1/s; about 100 ms after the stream stops it is half gone), and at 1 it is this far out
        // (m along the kick, and a squash share): the springs swing about it, so the body stays pushed while the hits land.
        public const float HitReactHoldDecay = 7f;
        public const float HitReactHoldLat = 0.12f;
        public const float HitReactHoldSquash = 0.035f;

        // --- round 1 fix: the impact point (Game/HitSpark.cs; the reviewers saw the shove but "only the white flash" at the hit) -
        // Sizes and speeds are 12 m's and drawn x RangeScale, like the shove (the same screen size from 3 m to 24 m).
        // The flare: a white-hot diamond facing the camera at the hit point (m: about 19 px at 12 m), full for HitSparkFlareHold
        // s (the hit's own frame), then shrinking to nothing over HitSparkFlareFade s; x FLASH INTENSITY (0: none).
        public const float HitSparkFlareSize = 0.32f;
        public const float HitSparkFlareDepth = 0.3f; // its thickness along the view, a share of its size (a flat diamond)
        public const float HitSparkFlareHold = 0.02f;
        public const float HitSparkFlareFade = 0.06f;
        // The chips: this many x the hit's scale (HitReactBody.Scale), within [Min, Max] (a revolver 4, a nailgun 3, a heavy 6);
        // a second pellet on the same enemy in the same frame adds HitSparkExtraChips and no flare.
        public const float HitSparkChips = 4f;
        public const int HitSparkChipsMin = 3;
        public const int HitSparkChipsMax = 6;
        public const int HitSparkExtraChips = 1;
        // Each chip: a cube of HitSparkChipSize m (about 7 px at 12 m), thrown at HitSparkChipSpeed m/s (+-SpeedJitter of it),
        // falling at HitSparkChipGravity m/s^2, living HitSparkChipLife +- LifeJitter s (0.15-0.25), tumbling HitSparkChipSpin
        // deg/s, full size until HitSparkChipShrinkFrom of its life, then shrinking to nothing; white-hot for its first
        // HitSparkChipWhiteFrames drawn frames, then the enemy's own red-orange (colors.enemy).
        public const float HitSparkChipSize = 0.12f;
        public const float HitSparkChipSpeed = 5.5f;
        public const float HitSparkChipSpeedJitter = 0.3f;
        public const float HitSparkChipGravity = 16f;
        public const float HitSparkChipLife = 0.2f;
        public const float HitSparkChipLifeJitter = 0.05f;
        public const float HitSparkChipSpin = 900f;
        public const float HitSparkChipShrinkFrom = 0.6f;
        public const int HitSparkChipWhiteFrames = 1;
        // The fan: the chips spread over HitSparkFanSpread deg in the screen plane, centred straight up, turned HitSparkFanBias
        // deg toward the side the shot struck (debris comes off the struck side), +-HitSparkFanJitter / 2 deg each, never lower
        // than HitSparkFanMinDeg above the horizon; HitSparkChipAway of each throw runs along the shot (away from the camera:
        // a red-orange cube flying at the camera reads as an enemy shot, NIGHT17 round 5's shed).
        public const float HitSparkFanSpread = 120f;
        public const float HitSparkFanBias = 35f;
        public const float HitSparkFanJitter = 20f;
        public const float HitSparkFanMinDeg = 10f;
        public const float HitSparkChipAway = 0.3f;
        // The neon copies' brightness (particles.js sets its burst colour x 2; the flare and a chip's first frame are white x it).
        public const float HitSparkGlow = 2f;
        // The pools (a stream from three guns on three enemies stays well under them; past them the oldest goes).
        public const int HitSparkFlarePool = 16;
        public const int HitSparkChipPool = 64;

        // A spring this close to rest (m or share, and per s) stops being integrated (no work for idle hit slots).
        public const float HitReactRestEps = 1e-4f;
    }
}
