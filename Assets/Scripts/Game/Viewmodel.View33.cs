// NIGHT33 builder W (row view33, DESIGN33 W, D-5; A3 2: the ADS pose slid the gun ~330 px to the centre and rolled it 4-5 degrees, GG's "forced angle").
// With view33 on (N33Fixes.ViewRowOn; off with -skygrindOff33), LT zoom changes the world FOV only: the gun keeps its hold pose (x, z, yaw, pitch) and
// takes no ADS roll; it lowers UnityTuning.ZoomLower33 m at full zoom, and the zoom eases in and out over UnityTuning.ZoomEase33 s. The idle breath,
// the speed-scaled bob, the landing dip and the per-class recoil springs are the existing layers (Viewmodel.cs :724-859), unchanged.
// pierce33's look in the hands: the charged KESTREL's kestrelPierce33 beam adds a camera punch (x PierceKickMult33 the KESTREL's) and trauma.
// Off: the web's ADS mix (ZoomPose33.Pose with view33 false is Viewmodel.cs's old arithmetic).
using System;
using Skygrind.Core;

namespace Skygrind.Game
{
    /// <summary>The zoom's gun pose (pure: ZoomLookTests pins both paths).</summary>
    public static class ZoomPose33
    {
        public struct Pose
        {
            public double x, y, z, pitchDeg, yawDeg, rollDeg;
        }

        static double Mix(double a, double b, double k) => a + (b - a) * k;

        /// <summary>The pose at eased zoom `ak` (0 hip .. 1 full) before the sway layers: view33 off = the web's mix toward the ADS pose with its roll;
        /// on = the hold pose lowered by `lower` x ak, no roll.</summary>
        public static Pose Of(bool view33, double ak, double lower,
            double holdX, double holdY, double holdZ, double holdPitch, double holdYaw,
            double adsX, double adsY, double adsZ, double adsPitch, double adsYaw, double adsRoll)
        {
            double k = view33 ? 0 : ak;
            return new Pose
            {
                x = Mix(holdX, adsX, k),
                y = Mix(holdY, adsY, k) - (view33 ? lower * ak : 0),
                z = Mix(holdZ, adsZ, k),
                pitchDeg = Mix(holdPitch, adsPitch, k),
                yawDeg = Mix(holdYaw, adsYaw, k),
                rollDeg = adsRoll * k,
            };
        }
    }

    /// <summary>NIGHT33 round 3: the gun feel's view33 layers (pure: GunFeel33Tests pins them; the feel33 reel prints them).</summary>
    public static class GunFeel33
    {
        /// <summary>The idle sway at time t (s), x `still` (1 = no bob .. 0): x / y (m) and roll (deg), a figure-eight.</summary>
        public static (double x, double y, double rollDeg) IdleSway(double t, double still)
        {
            double w = 2 * Math.PI * UnityTuning.IdleSwayHz33 * t;
            double k = Math.Max(0, Math.Min(1, still));
            return (UnityTuning.IdleSwayX33 * Math.Sin(w) * k, UnityTuning.IdleSwayY33 * Math.Sin(2 * w) * k, UnityTuning.IdleSwayRollDeg33 * Math.Sin(w) * k);
        }

        /// <summary>The bob's scale at `speed` (m/s): the web's clamp(speed / walk, 0, webMax); view33's clamp(speed / walk, 0, BobMaxScale33).</summary>
        public static double BobScale(bool view33, double speed, double walk, double webMax) =>
            Math.Max(0, Math.Min(speed / walk, view33 ? UnityTuning.BobMaxScale33 : webMax));

        /// <summary>The recoil clock's rate for a weight class (1 with view33 off).</summary>
        public static double RecoilRate(bool view33, string cls) =>
            !view33 ? 1 : cls == "light" ? UnityTuning.RecoilRateLight33 : cls == "heavy" ? UnityTuning.RecoilRateHeavy33 : UnityTuning.RecoilRateMedium33;

        /// <summary>A damped spring's time to settle inside 2 % of its kick (the envelope e^(-z w t)), at clock rate `rate`.</summary>
        public static double SettleSeconds(double freqHz, double zeta, double rate) => -Math.Log(0.02) / (zeta * 2 * Math.PI * freqHz) / rate;

        /// <summary>The landing dip's pose for a dip of `dip` m (already x intensity x landDipScale): y down, z toward the eye, pitch (deg, down).</summary>
        public static (double y, double z, double pitchDeg) LandDip(bool view33, double dip) =>
            view33 ? (-dip, dip * UnityTuning.LandDipBack33, -dip * UnityTuning.LandDipPitch33) : (-dip, 0, 0);
    }

    public sealed partial class Viewmodel
    {
        /// <summary>NIGHT33: the view33 row (read once at build; a view row, not on the tape).</summary>
        bool view33;

        /// <summary>NIGHT33 (the feel33 reel): is view33's gun feel on?</summary>
        public bool View33On => view33;

        /// <summary>NIGHT33 (the feel33 reel): this frame's idle sway x (m, 0 off) and the landing dip's depth (m).</summary>
        public double IdleSwayNow33, LandDipNow33;

        /// <summary>NIGHT33 (the feel33 reel): the gun root's pose this frame (local position, Unity space, and euler angles).</summary>
        public UnityEngine.Vector3 RootPos33 => root.localPosition;
        public UnityEngine.Vector3 RootEuler33 => root.localEulerAngles;

        void View33Build()
        {
            view33 = N33Fixes.ViewRowOn(cfg, "view33");
            game.events.On<RailFireEvent>(Loadout.KestrelPierce33, OnKestrelPierce33);
            if (view33 && vmCam != null) vmCam.fieldOfView = UnityTuning.ViewmodelFov33; // D-5: 54-60
        }

        /// <summary>The held gun's recoil clock rate (view33: the class's faster settle; 1 off).</summary>
        double RecoilRate33(string held) => view33 ? GunFeel33.RecoilRate(true, GunOf(held).cls) : 1;

        /// <summary>The zoom's ease time: view33's 120 ms, else the gun's class ease (ads.classEase).</summary>
        double ZoomEase(double gunEase) => view33 ? UnityTuning.ZoomEase33 : gunEase;

        void OnKestrelPierce33(RailFireEvent e)
        {
            if (e == null || e.owner != "player") return;
            Gun g = GunOf("revolver");
            if (g.hasPunch)
            {
                peaks2[0] = g.punchPitch * UnityTuning.PierceKickMult33;
                peaks2[1] = 0;
                punch.Add(g.punchShape, peaks2);
                punchCap = System.Math.Max(punchCap, g.punchCapDeg * UnityTuning.PierceKickMult33);
            }
            game.trauma.Add(UnityTuning.PierceTrauma33);
        }
    }
}
