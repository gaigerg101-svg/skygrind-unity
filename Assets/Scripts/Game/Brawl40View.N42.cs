// NIGHT42 builder B1 (brawl42): what the player SEES of THE ICEBREAKER's two new attacks (the sim: Core/Enemies/Brawl40.N42.cs). They only
// happen with brawl42 on, so with the row off nothing here is ever drawn (NIGHT41's look). Called from Brawl40View.Render after Render41.
//  - THE SWEEP: the wind-up crouches low with the right arm cocked back near the ice (its fist and shell red-orange, brighter through the
//    tell) and a red-orange ring on the ice at the arm's reach (where it will pass: jump); the spin draws a bright red-orange blade at shin
//    height from its body to the reach, turning with the sim's arm; then it is dizzy (the head wobbles).
//  - THE LEAP: the wind-up crouches; a red-orange landing circle on the ice follows the player, then LOCKS (it flares brighter: leave it);
//    in the air the body rides the sim's height (tucked); on landing its fists are in the ice, both CYAN (the punish).
// Numbers: UnityTuning.Brawl42.cs. Frame flags for the brawl42 reel: SweepRingShown, SweepBladeShown, LeapCircleShown, LeapLockedShown, AirShown.
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class Brawl40View
    {
        readonly List<Transform> reachTs42 = new List<Transform>(), circleTs42 = new List<Transform>(), innerTs42 = new List<Transform>();
        readonly List<Renderer> reachRs42 = new List<Renderer>(), circleRs42 = new List<Renderer>(), innerRs42 = new List<Renderer>();
        Transform blade42, disc42;
        Renderer blade42R, disc42R;
        readonly List<Transform> shinTs42 = new List<Transform>();
        readonly List<Renderer> shinRs42 = new List<Renderer>();
        bool built42;

        /// <summary>For the reels / frames: the sweep's reach ring, its blade, the leap's circle (locked), the body in the air.</summary>
        public bool SweepRingShown, SweepBladeShown, LeapCircleShown, LeapLockedShown, AirShown;

        void Build42()
        {
            built42 = true;
            for (int i = 0; i < UnityTuning.Brawl40RingSegments; i++)
            {
                reachRs42.Add(Glow(fxRoot, "sweep reach", out Transform a));
                reachTs42.Add(a);
                circleRs42.Add(Glow(fxRoot, "leap circle", out Transform b));
                circleTs42.Add(b);
                innerRs42.Add(Glow(fxRoot, "leap circle inner", out Transform c));
                innerTs42.Add(c);
                shinRs42.Add(Glow(fxRoot, "sweep shin band", out Transform s));
                shinTs42.Add(s);
            }
            blade42R = Glow(fxRoot, "sweep blade", out blade42);
            // the landing disc: opaque UNLIT (game.mats.neon is URP/Unlit), a cylinder mesh flattened: the warm room light cannot tint it amber
            var cyl = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            Mesh discMesh = cyl.GetComponent<MeshFilter>().sharedMesh;
            Object.Destroy(cyl);
            var dg = new GameObject("leap disc");
            dg.transform.SetParent(fxRoot, false);
            dg.AddComponent<MeshFilter>().sharedMesh = discMesh;
            disc42R = dg.AddComponent<MeshRenderer>();
            disc42R.sharedMaterial = new Material(game.mats.neon) { name = "brawl42 leap disc" };
            disc42R.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            disc42 = dg.transform;
            dg.SetActive(false);
        }

        void Render42(Brawl40 g, Brawl40State st, string atk, float prog, ref float leanDeg, ref float headDeg, ref float drop,
            ref Color fistRC, ref Color fistLC, Color dangerC, Color cyanC, Vector3 fu)
        {
            SweepRingShown = SweepBladeShown = LeapCircleShown = LeapLockedShown = AirShown = false;
            bool sweep = atk == Brawl40.Sweep, leap = atk == Brawl40.Leap;
            Hook42();
            if (!built42)
            {
                if (!sweep && !leap && !pending42) return; // never built with the row off
                Build42();
            }
            Punch42(g, ref leanDeg, fu); // F1's gauntletHit42 on THE ICEBREAKER: chips, the crack flash, the recoil (Brawl40View.N42.Hit.cs)
            if (sweep && g.p42 != null)
            {
                if (st == Brawl40State.Tell)
                {
                    Pose = "sweep wind-up";
                    drop += UnityTuning.Brawl42SweepCrouch * prog;
                    leanDeg = UnityTuning.Brawl42SweepLean;
                    headDeg = UnityTuning.Brawl42SweepHead;
                    fistRC = dangerC;
                    TellGlow = prog;
                    SweepRingShown = true;
                }
                else if (st == Brawl40State.Active)
                {
                    Pose = "sweep";
                    drop += UnityTuning.Brawl42SweepCrouch;
                    leanDeg = UnityTuning.Brawl42SweepLean;
                    fistRC = Look.Hex(UnityTuning.Brawl40DangerHex, UnityTuning.Brawl40TellMax);
                    SweepBladeShown = true;
                }
                else if (st == Brawl40State.Recover)
                {
                    Pose = "dizzy";
                    headDeg = Mathf.Sin(Time.time * UnityTuning.Brawl42DizzyRate) * UnityTuning.Brawl42DizzyRoll;
                    leanDeg = 10;
                }
                if (shellR41 != null && Look41On && (st == Brawl40State.Tell || st == Brawl40State.Active))
                {
                    if (!shellR41.gameObject.activeSelf) shellR41.gameObject.SetActive(true);
                    Tint(shellR41, st == Brawl40State.Active ? Look.Hex(UnityTuning.Brawl40DangerHex, UnityTuning.Brawl41ShellGain)
                        : Look.Hex(UnityTuning.Brawl40DangerHex, Mathf.Lerp(UnityTuning.Brawl40TellMin, UnityTuning.Brawl41ShellGain, prog)));
                    Shell41Lit = true;
                }
            }
            if (leap && g.p42 != null)
            {
                if (st == Brawl40State.Tell)
                {
                    Pose = g.leapLocked ? "leap wind-up (locked)" : "leap wind-up";
                    drop += UnityTuning.Brawl42LeapCrouch * prog;
                    leanDeg = UnityTuning.Brawl42LeapLean;
                    TellGlow = prog;
                    LeapCircleShown = true;
                    LeapLockedShown = g.leapLocked;
                }
                else if (st == Brawl40State.Active)
                {
                    Pose = "leap";
                    leanDeg = UnityTuning.Brawl42AirLean;
                    drop += UnityTuning.Brawl42AirTuck;
                    LeapCircleShown = LeapLockedShown = true;
                    AirShown = g.air42 > 0.05;
                }
                else if (st == Brawl40State.Stuck)
                {
                    Pose = "stuck (landed)";
                    leanDeg = 50;
                    headDeg = 10;
                    fistRC = fistLC = cyanC;
                    CyanShown = true;
                    if (shellL41 != null && Look41On)
                    {
                        if (!shellL41.gameObject.activeSelf) shellL41.gameObject.SetActive(true);
                        if (!shellR41.gameObject.activeSelf) shellR41.gameObject.SetActive(true);
                        Color c = Look.Hex(UnityTuning.Brawl40CyanHex, UnityTuning.Brawl41ShellStuckGain);
                        Tint(shellR41, c);
                        Tint(shellL41, c);
                    }
                }
            }
            // the body rides the leap's height (the sim's hit spheres do: Brawl40.Base)
            if (g.air42 != 0) bodyRoot.position = fu + Vector3.up * (float)g.air42;
            // the sweep's reach ring
            float reach = g.p42 != null ? (float)(g.p.radius + g.p42.sweepReach) : 1f;
            Ring(reachTs42, reachRs42, SweepRingShown, fu, reach, UnityTuning.Brawl42ReachY, UnityTuning.Brawl42ReachWidth, UnityTuning.Brawl42ReachThick,
                Look.Hex(UnityTuning.Brawl42RedHex, Mathf.Lerp(UnityTuning.Brawl40TellMin, UnityTuning.Brawl42ReachGain, prog)));
            // n42-r1: the shin band (the arm's height, all round its reach): what it will hit, and that a jump clears it
            Ring(shinTs42, shinRs42, SweepRingShown, fu, reach, UnityTuning.Brawl42ShinY, UnityTuning.Brawl42ShinThick, UnityTuning.Brawl42ShinHeight,
                Look.Hex(UnityTuning.Brawl42RedHex, Mathf.Lerp(UnityTuning.Brawl40TellMin, UnityTuning.Brawl42ShinGain, prog)));
            // the blade
            blade42.gameObject.SetActive(SweepBladeShown);
            if (SweepBladeShown)
            {
                Vec3 dir = new Vec3(JsMath.Sin(g.sweepArm), 0, JsMath.Cos(g.sweepArm));
                double r0 = g.p.radius * 0.5;
                Vector3 a = U(g.Feet + dir * r0), b = U(g.Feet + dir * (g.p.radius + g.p42.sweepReach));
                Vector3 mid = (a + b) * 0.5f + Vector3.up * UnityTuning.Brawl42BladeY;
                blade42.position = mid;
                blade42.rotation = Quaternion.LookRotation((b - a).normalized, Vector3.up);
                blade42.localScale = new Vector3(UnityTuning.Brawl42BladeThick, UnityTuning.Brawl42BladeThick, (b - a).magnitude);
                Tint(blade42R, Look.Hex(UnityTuning.Brawl42RedHex, UnityTuning.Brawl42BladeGain));
            }
            // the landing circle (tracking, then locked: brighter): a saturated red ring + an opaque unlit red disc (n42-r1: the additive ring
            // alone read amber over the warm ice and thin)
            float cr = g.p42 != null ? (float)g.p42.leapRadius : 1f;
            Color cc = Look.Hex(UnityTuning.Brawl42RedHex, LeapLockedShown ? UnityTuning.Brawl42CircleLockGain : Mathf.Lerp(UnityTuning.Brawl40TellMin, UnityTuning.Brawl42CircleGain, prog));
            Vector3 at = U(g.leapTo);
            Ring(circleTs42, circleRs42, LeapCircleShown, at, cr, UnityTuning.Brawl42CircleY, UnityTuning.Brawl42CircleWidthR1, UnityTuning.Brawl42CircleThick, cc);
            Ring(innerTs42, innerRs42, LeapLockedShown, at, cr * UnityTuning.Brawl42CircleInner, UnityTuning.Brawl42CircleY, UnityTuning.Brawl42CircleWidth * 0.6f, UnityTuning.Brawl42CircleThick, cc);
            disc42.gameObject.SetActive(LeapCircleShown);
            if (LeapCircleShown)
            {
                // tracking: a dark red disc growing from the centre with the wind-up; locked: full radius, bright, pulsing
                float k = LeapLockedShown ? 1f : Mathf.Clamp01(prog / Mathf.Max(0.05f, (float)g.p42.leapTrack));
                float rr = LeapLockedShown ? cr : cr * (0.35f + 0.6f * k);
                float gain = LeapLockedShown ? UnityTuning.Brawl42DiscLockGain + UnityTuning.Brawl42DiscPulse * Mathf.Sin(Time.time * UnityTuning.Brawl42DiscRate) : UnityTuning.Brawl42DiscTrackGain;
                disc42.position = at + Vector3.up * UnityTuning.Brawl42DiscY;
                disc42.localScale = new Vector3(rr * 2f, 0.005f, rr * 2f); // the cylinder primitive is 1 wide, 2 tall
                Tint(disc42R, Look.Hex(UnityTuning.Brawl42RedHex, gain));
            }
        }
    }
}
