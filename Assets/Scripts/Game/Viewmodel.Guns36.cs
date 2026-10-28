// NIGHT36 builder F crunch36 (view; NIGHT36.md F1 "gun parts move"): the KESTREL's hammer is its own part (BlockyKit.KestrelHammer36). On each KESTREL
// shot it FALLS forward (Hammer36FallDeg) on the shot's own frame, holds Hammer36HoldS, then re-cocks over Hammer36CockS (an ease-out), on the
// viewmodel's anim time (it holds in a freeze after falling: the fall is drawn on frame time, as the kick's attack). Built at load when
// design.n36.crunch36 is enabled and not -skygrindOff36 (a view of the row: the sim bit only changes the freezes). Already moving before tonight and
// kept: the cylinder's turn per shot, the THUNDERCLAP's pump and its shell, the muzzle star's two frames (fx.flashFrames 2, fx.flashSecondFrame).
using UnityEngine;

namespace Skygrind.Game
{
    public sealed partial class Viewmodel
    {
        Transform hammer36;
        double hammer36Age = double.PositiveInfinity;

        /// <summary>NIGHT36 (the reels): the hammer's drawn angle (deg forward from cocked; -1 without the part).</summary>
        public static float Hammer36Deg = -1f;

        /// <summary>Set before the rigs and the arm are built: the KESTREL's body leaves its hammer out (crunch36); the gauntlet and the quick punch's
        /// fist are hands (fist36, BlockyKit.Fist36Hand).</summary>
        void Hammer36Prepare()
        {
            BlockyKit.Hammer36 = N36Fixes.ViewRowOn(game.cfg, "crunch36");
            BlockyKit.Fist36 = fist36 = N36Fixes.ViewRowOn(game.cfg, "fist36");
            if (fist36)
            {
                game.events.On<Skygrind.Core.HitEvent>("parry36", e => { if (e == null || e.owner == "player") parry36Age = 0; });
                game.events.On("punch", _ => Fist36Punches++);
            }
        }

        // --- NIGHT36 F fist36 (view): the gauntlet's punch lunges further and corkscrews (a follow-through past the target, then back); a parry36 throws
        // the fist up into a guard and turns it palm-out for Fist36ParryS (the gauntlet in hand, and the quick punch's arm while it is out) ---------------
        bool fist36;
        double parry36Age = double.PositiveInfinity;

        /// <summary>NIGHT36 (the reels): punches seen, parry poses started, the largest lunge / parry weight drawn since the reel reset them.</summary>
        public static int Fist36Punches, Fist36Parries;
        public static float Fist36LungeMax, Fist36ParryMax;

        /// <summary>The gauntlet's jab (inside Render's held == "gauntlet" branch, after the web's out / hold / back envelope `e`): x Fist36LungeMult
        /// further, a corkscrew roll and a dip with it, and a follow-through push past the target right after the out.</summary>
        void Fist36Swing(double e, double age, double outS, ref double py, ref double pz, ref double rz)
        {
            if (!fist36) return;
            const double Deg = System.Math.PI / 180;
            double follow = age > outS && age < outS + UnityTuning.Fist36FollowS ? System.Math.Sin(System.Math.PI * (age - outS) / UnityTuning.Fist36FollowS) : 0;
            pz += v.swingZ * (e * (UnityTuning.Fist36LungeMult - 1) + follow * UnityTuning.Fist36FollowShare);
            rz += UnityTuning.Fist36TwistDeg * Deg * e;
            py -= UnityTuning.Fist36Dip * e;
            float lunge = (float)(e * UnityTuning.Fist36LungeMult + follow * UnityTuning.Fist36FollowShare);
            if (lunge > Fist36LungeMax) Fist36LungeMax = lunge;
        }

        /// <summary>The parry pose's weight now (0..1: up fast, held, then back), advanced on frame time (it plays through the parry's freeze).</summary>
        double Fist36ParryW(double frameDt)
        {
            if (!fist36 || double.IsPositiveInfinity(parry36Age)) return 0;
            if (parry36Age == 0) Fist36Parries++;
            parry36Age += frameDt;
            double t = parry36Age, up = UnityTuning.Fist36ParryUpS, hold = UnityTuning.Fist36ParryHoldS, back = UnityTuning.Fist36ParryBackS;
            double w = t < up ? t / up : t < up + hold ? 1 : t < up + hold + back ? 1 - (t - up - hold) / back : 0;
            if (t >= up + hold + back) parry36Age = double.PositiveInfinity;
            w = w * w * (3 - 2 * w);
            if (w > Fist36ParryMax) Fist36ParryMax = (float)w;
            return w;
        }

        double parry36W; // this frame's weight (Fist36ParryW once a frame, read by the gun's and the arm's pose)

        /// <summary>Once a frame, before the gun's pose: the parry pose's weight.</summary>
        void Fist36Frame(double frameDt) => parry36W = Fist36ParryW(frameDt);

        /// <summary>The gauntlet in hand: the guard (up, pitched back, palm out).</summary>
        void Fist36GuardGun(ref double py, ref double pz, ref double rx, ref double rz)
        {
            if (parry36W <= 0) return;
            const double Deg = System.Math.PI / 180;
            py += UnityTuning.Fist36ParryUp * parry36W;
            pz += v.swingZ * UnityTuning.Fist36ParryOut * parry36W;
            rx += UnityTuning.Fist36ParryPitchDeg * Deg * parry36W;
            rz += UnityTuning.Fist36ParryRollDeg * Deg * parry36W;
        }

        /// <summary>n36-r3 (the lead: the fist sat low and small, knuckles hidden): the gauntlet's rig x Fist36KitScale, raised and drawn in by
        /// Fist36KitOffset (the viewmodel camera's space, m), its nose pitched down and turned in (Fist36KitPitchDeg / YawDeg) so the knuckle row faces the eye.</summary>
        void Fist36Kit(Rig rig, string held)
        {
            if (!fist36 || held != "gauntlet") return;
            rig.t.localScale *= UnityTuning.Fist36KitScale;
            rig.t.localPosition += UnityTuning.Fist36KitOffset;
            rig.t.localRotation = rig.t.localRotation * Quaternion.Euler(UnityTuning.Fist36KitPitchDeg, UnityTuning.Fist36KitYawDeg, 0);
        }

        /// <summary>The quick punch's arm while it is out: the same guard on its pose (web camera space: m, deg).</summary>
        void Fist36GuardArm(ref HandsArmPose a)
        {
            if (parry36W <= 0 || !a.visible) return;
            a.y += UnityTuning.Fist36ParryUp * parry36W;
            a.pitchDeg += UnityTuning.Fist36ParryPitchDeg * parry36W;
            a.rollDeg += UnityTuning.Fist36ParryRollDeg * parry36W;
        }

        void Hammer36Build(Rig rig, Material kitMat)
        {
            if (!BlockyKit.Hammer36) return;
            var kit = new MeshKit();
            BlockyKit.KestrelHammer36(kit);
            GameObject go = MeshKit.Object("hammer", rig.t, kit.Build("kestrel hammer"), kitMat, shadows: false);
            go.transform.localPosition = BlockyKit.KestrelHammerPivot36;
            SetLayer(go);
            hammer36 = go.transform;
            Hammer36Deg = 0f;
        }

        void Hammer36Fire(string weapon)
        {
            if (hammer36 != null && weapon == "revolver") hammer36Age = 0;
        }

        void Hammer36Frame(double animDt, double frameDt)
        {
            if (hammer36 == null) return;
            hammer36Age += hammer36Age < UnityTuning.Hammer36HoldS ? System.Math.Max(animDt, frameDt) : animDt; // falls and holds even in a freeze
            float a;
            if (hammer36Age < UnityTuning.Hammer36HoldS) a = UnityTuning.Hammer36FallDeg;
            else if (hammer36Age < UnityTuning.Hammer36HoldS + UnityTuning.Hammer36CockS)
            {
                float u = (float)((hammer36Age - UnityTuning.Hammer36HoldS) / UnityTuning.Hammer36CockS);
                a = UnityTuning.Hammer36FallDeg * (1 - u) * (1 - u); // eases back to cocked
            }
            else a = 0f;
            hammer36.localRotation = Quaternion.Euler(a, 0, 0); // + about x: the top goes forward (toward the muzzle)
            Hammer36Deg = a;
        }
    }
}
