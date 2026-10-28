// NIGHT47 builder F (design.n47.zoom47, a view row; NIGHT47.md G22; GG 2026-10-18: "pressing left trigger with the shotgun looks weird and doesn't
// feel like a proper zoom-in, and the angle is awkward"). Today (F-cards 1d, read in code): with ltAlways the THUNDERCLAP zooms x1.25 (74 -> ~62 deg),
// a linear 0.12 s ease, and view33's pose KEEPS the hip pose (yaw 5 deg, pitch 1 deg) and only lowers the gun 0.05 m: the view narrows a little while
// the gun drops at its hip angle, nothing comes to the eye. With the row on, the THUNDERCLAP only (w33's rule kept: world FOV only, the gun never
// slides to the centre of the screen):
//   - the world FOV eases (ease-out cubic) to Zoom47FovShare of the base in Zoom47InS, back in Zoom47OutS (the zoom's factor from the base FOV);
//   - the gun SHOULDERS: from the hip pose it rises Zoom47Rise m (and does not take view33's 0.05 m drop) and comes in Zoom47In m, its yaw turned so
//     its barrel's line meets the reticle at Zoom47MeetM m, its pitch to 0, no roll; a Zoom47DipM m dip in the first Zoom47DipShare of the ease
//     (the anticipation before the lift);
//   - the boost on LT in the air is the sim's and is unchanged (VERBS 29 / 31: a view row cannot touch the sim).
// Off (the row off, -skygrindOff47): NIGHT46's zoom exactly. Numbers: UnityTuning.F47.cs (Zoom47*).
using System;
using Skygrind.Core;

namespace Skygrind.Game
{
    /// <summary>zoom47's switch and the reel's reads.</summary>
    public static class Zoom47
    {
        public static bool? Force;
        static GameConfig readFor;
        static bool on;

        public static bool On(GameConfig cfg)
        {
            if (Force.HasValue) return Force.Value;
            if (cfg == null) return false;
            if (!ReferenceEquals(cfg, readFor)) { readFor = cfg; on = NightRows.ViewOn(cfg, "zoom47"); }
            return on;
        }

        /// <summary>For the a747 reel: the eased zoom (0..1), the world FOV asked (deg), the gun's yaw and its lift now.</summary>
        public static float Ease, Fov, YawDeg, Lift;

        /// <summary>Pure: the zoom factor that gives `share` of the base vertical FOV (CombatMath.ZoomedFov's inverse: tan(b/2) / tan(share b / 2)).</summary>
        public static double FactorFor(double baseFovDeg, double share)
        {
            double b = baseFovDeg * Math.PI / 180;
            return Math.Tan(b / 2) / Math.Tan(share * b / 2);
        }

        /// <summary>Pure: ease-out cubic.</summary>
        public static double EaseOut(double a) { a = Math.Max(0, Math.Min(1, a)); double u = 1 - a; return 1 - u * u * u; }
    }

    public sealed partial class Viewmodel
    {
        bool Zoom47For(string held) => held == "shotgun" && Zoom47.On(cfg);

        /// <summary>The shouldered pose at eased zoom `ak` (raw ease `a` for the dip).</summary>
        static ZoomPose33.Pose Zoom47Pose(ZoomPose33.Pose zp, double ak, double a, double holdX, double holdYaw)
        {
            double dip = a < UnityTuning.Zoom47DipShare ? UnityTuning.Zoom47DipM * Math.Sin(Math.PI * a / UnityTuning.Zoom47DipShare) : 0;
            zp.y += (UnityTuning.ZoomLower33 + UnityTuning.Zoom47Rise) * ak - dip; // view33's drop undone, then the rise to the shoulder
            zp.x -= UnityTuning.Zoom47In * ak;
            double x = holdX - UnityTuning.Zoom47In * ak;
            double meetYaw = Math.Atan2(x, UnityTuning.Zoom47MeetM) * 180 / Math.PI * Math.Sign(holdYaw == 0 ? 1 : holdYaw); // the barrel's line through the reticle
            zp.yawDeg = holdYaw + (meetYaw - holdYaw) * ak;
            zp.pitchDeg *= 1 - ak;
            zp.rollDeg *= 1 - ak;
            Zoom47.YawDeg = (float)zp.yawDeg;
            Zoom47.Lift = (float)((UnityTuning.ZoomLower33 + UnityTuning.Zoom47Rise) * ak - dip);
            return zp;
        }
    }
}
