// NIGHT36 builder F (gun feel): the Unity-only numbers of tap36's HUD words, muzzle36 (shots leave the gun), crunch36's view (stagger, knockback,
// moving gun parts) and fist36 (the hand). Render and UI numbers only (CLAUDE.md rule 5); gameplay numbers are in design.n36.tap36 / crunch36.
namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        /// <summary>tap36: the gauntlet BLAST slot's word while it only refills its pips (it read CHARGING: NIGHT36.md F1, HudWeapons30.cs Blast).</summary>
        public const string Hud36WordRefill = "REFILL";

        /// <summary>muzzle36: s of world time a new player nail / rocket takes to blend from the muzzle onto its sim path (NIGHT36.md F1: ~0.1 s; a nail
        /// at 60-90 m/s is 6-9 m out by then, so the blend ends before it reaches most targets).</summary>
        public const float Muzzle36BlendS = 0.1f;

        /// <summary>muzzle36: a muzzle further than this from a new shot's drawn position (m) is not its gun (a rebound, a scripted shot): no blend.</summary>
        public const float Muzzle36MaxOffset = 3f;

        /// <summary>muzzle36: hitscan shots held for one frame's render (the frame's shots; more are drawn at once, from the pre-kick muzzle).</summary>
        public const int Muzzle36Pending = 16;

        /// <summary>muzzle36: the charged THUNDERCLAP slug's single line, x the tracer's width (one heavy line, not 8 thin ones).</summary>
        public const float Slug36Width = 2.5f;

        /// <summary>muzzle36 (n36-r1: the volley frame showed no readable nails): a flying nail drawn x Nail36Thick across and x Nail36Long along (a streak
        /// behind its tip: 0.34 m -> 1.0 m, about one frame's travel); Nail36Length is the nail mesh's own length (projectiles.js :65, 0.34 m).</summary>
        public const float Nail36Thick = 2.4f, Nail36Long = 3f, Nail36Length = 0.34f;

        // --- crunch36 (view): the stagger and the knockback (EnemyViews.Hit.cs, HitSpring.cs HitReactBody.Stagger36) ---------------------------------------
        /// <summary>The stagger spring (x'' = -k x - c x'): k 300, c 14 peaks ~75 ms after the hit and settles in ~0.5 s: slower than the 32 ms lateral
        /// kick (HitReactSpring 1500 / 36), so the body reads a blow and a recovery, not a twitch.</summary>
        public const float Stagger36Spring = 300f, Stagger36Damping = 14f;

        /// <summary>The stagger's peak tip back (deg) for a gun's body hit and for a melee hit; a stream never tips past the cap.</summary>
        public const float Stagger36GunDeg = 12f, Stagger36MeleeDeg = 22f, Stagger36CapDeg = 26f;

        /// <summary>The knockback (m back along the shot, CrunchKnockIn / Out's ease) of a body hit with crunch36 on: a light gun (it had none: only the
        /// sideways kick), a heavy gun (NIGHT17's CrunchKnockHeavy 0.38 kept, a touch more), melee (CrunchKnockMelee 0.22 -> 0.35: the punch must shove).</summary>
        public const float Crunch36KnockGun = 0.2f, Crunch36KnockHeavy = 0.45f, Crunch36KnockMelee = 0.35f;

        /// <summary>crunch36 (view): the KESTREL hammer's fall (deg forward from cocked), how long it lies down (s), and its re-cock (s, an ease-out;
        /// inside the 0.4 s fire interval, so it is cocked again before the next shot).</summary>
        public const float Hammer36FallDeg = 38f, Hammer36HoldS = 0.05f, Hammer36CockS = 0.2f;

        /// <summary>crunch36 head-on read (the lead, n36-r2): the squash at its peak (height x 0.85; the width x 1 + this x Crunch36Bulge = 1.1), on the
        /// fast hit spring (peak ~32 ms, one stretch overshoot, back by ~0.2 s), not range-scaled.</summary>
        public const float Crunch36Squash = 0.15f, Crunch36Bulge = 0.67f;

        /// <summary>crunch36: the twist about the vertical toward the struck side (deg; the lead: 6-10), a gun hit and a melee hit.</summary>
        public const float Crunch36TwistDeg = 8f, Crunch36TwistMeleeDeg = 10f;

        /// <summary>crunch36: the body's white-hot hold on a hit (s; NIGHT17's CrunchHitFlashHold 0.03 -> 0.04: two whole frames at 60 fps, then its fade).</summary>
        public const float Crunch36FlashHoldS = 0.04f;

        // --- fist36 (view): the gauntlet's punch and the parry guard (Viewmodel.Guns36.cs) ------------------------------------------------------------------
        /// <summary>The gauntlet's jab reaches this many times the web's viewmodel.swingZ (a lunge, not a nudge).</summary>
        public const double Fist36LungeMult = 1.6;

        /// <summary>The follow-through: a push past the target for this long right after the out (s), this share of swingZ at its peak.</summary>
        public const double Fist36FollowS = 0.09, Fist36FollowShare = 0.35;

        /// <summary>The corkscrew roll with the jab (deg at the full envelope) and the dip (m: the shoulder drives down into it).</summary>
        public const double Fist36TwistDeg = 24, Fist36Dip = 0.02;

        /// <summary>The parry guard: up over Fist36ParryUpS, held Fist36ParryHoldS (about ULTRAKILL's 0.25 s parry stop: PLAN.md), back over Fist36ParryBackS
        /// (frame time, so it plays through the freeze); raised (m), pushed out (x swingZ), pitched back and rolled palm-out (deg).</summary>
        public const double Fist36ParryUpS = 0.04, Fist36ParryHoldS = 0.22, Fist36ParryBackS = 0.16;
        public const double Fist36ParryUp = 0.045, Fist36ParryOut = 0.5, Fist36ParryPitchDeg = 28, Fist36ParryRollDeg = -35;

        // n36-r3 (the lead: "a washed-out cyan smear low in the frame"): the fist in hand 22 % bigger, raised 4 cm and drawn 2 cm in (viewmodel camera
        // space, Unity), its nose pitched 12 deg down and turned 10 deg in so the knuckle row faces the eye; the gem shrunk into the knuckle strip.
        public const float Fist36KitScale = 1.22f, Fist36KitPitchDeg = 12f, Fist36KitYawDeg = -10f;
        public static readonly UnityEngine.Vector3 Fist36KitOffset = new UnityEngine.Vector3(-0.02f, 0.04f, 0f);
        public static readonly UnityEngine.Vector3 Fist36GemAt = new UnityEngine.Vector3(0f, 0.034f, 0.112f);
        public const float Fist36GemSize = 0.005f;
    }
}
