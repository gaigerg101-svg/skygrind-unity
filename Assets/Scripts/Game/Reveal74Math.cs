// NIGHT74 builder L, slice L2 (view row design.n74.reveal74; the view is Reveal74.cs). gg-proxy-close #1: foundry-b1 reads "11 LEFT" with no enemy anywhere in the frame; GG 2026-10-09 "I'd like a lot of
// the rooms to be a lot smaller and more action packed", 2026-10-12 "hard to understand what to do". The rule: while the room's counter reads N LEFT (N >= 1) and no live enemy is on screen for
// UnityTuning.N74RevealDelayS (3 s), a red-orange chevron rides the screen's edge toward the nearest live enemy; the moment one is on screen it goes (UnityTuning.N74RevealFadeOutS, under 0.2 s).
// This file is the pure half (no UnityEngine call: SimHarness and the EditMode test compile it alone): the timer, the screen direction of a target off the picture, and where that direction meets
// the screen's edge without landing on the bottom band (the gun and the HP plate sit there).
using System;

namespace Skygrind.Game
{
    public static class Reveal74Math
    {
        public const string Row = "reveal74";

        /// <summary>NIGHT74 review S1 (the enemy scan, a viewport test and up to two wall rays per live enemy, ran every frame of every room fight with the row off): the scan runs when the row is on, and
        /// when the reel's same-run toggle is flipping this very row (it holds one frame with the row off and the state must run through it as before: D1). Otherwise it is skipped.</summary>
        public static bool ShouldScan(bool on, bool reelToggled) => on || reelToggled;

        /// <summary>The timer. Fed every rendered frame (the row keeps running while it reads off, so the next frame on is the frame it would have drawn).</summary>
        public sealed class State
        {
            /// <summary>Seconds in a row with the counter showing N LEFT, a live enemy somewhere, and none on screen.</summary>
            public double NoneS;
            /// <summary>The chevron is wanted now (NoneS reached the delay and nothing has come on screen since).</summary>
            public bool Want;

            public void Reset() { NoneS = 0; Want = false; }

            /// <summary>NIGHT74 review S1: one frame of the view's loop. A frame the counter is up but the scan was skipped (the row is off: ShouldScan) has no enemy facts to feed Step, so the timer clears
            /// (the first frame back on waits the delay again) and nothing is wanted; every other frame is Step as before.</summary>
            public bool StepOrClear(double dt, bool counting, bool scanned, bool hasLive, bool onScreen, double delayS)
            {
                if (counting && !scanned) { Reset(); return false; }
                return Step(dt, counting, hasLive, onScreen, delayS);
            }

            /// <summary>One frame. counting: the counter reads N LEFT with N >= 1 (and the game is in play); hasLive: a live enemy exists to point at; onScreen: at least one live enemy is in the picture
            /// and in line of sight. Any of the three failing clears the wait at once.</summary>
            public bool Step(double dt, bool counting, bool hasLive, bool onScreen, double delayS)
            {
                if (!counting || !hasLive || onScreen)
                {
                    NoneS = 0;
                    Want = false;
                    return false;
                }
                NoneS += dt;
                if (NoneS + 1e-9 >= delayS) Want = true; // (1e-9: 180 steps of 1/60 sum to 2.9999999999999996 s, and the 3 s is meant to be the 180th frame, not the 181st)
                return Want;
            }
        }

        /// <summary>The chevron's screen angle (radians; 0 = to the right, pi / 2 = up) toward a target at (x right, y up, z forward) metres from the camera, in camera space.
        /// In front of the camera the angle is the target's direction in the picture's plane (straight above reads up). Behind it the player has to turn, so the angle is level, to the side
        /// the turn is shorter (the right when the target is dead behind). A target dead ahead but hidden (behind a wall in the middle of the picture) reads up.</summary>
        public static double Angle(double x, double y, double z)
        {
            if (z < 0)
                return x >= 0 ? 0 : Math.PI;
            if (x * x + y * y < 0.01) return Math.PI / 2;
            return Math.Atan2(y, x);
        }

        /// <summary>The point of the screen's edge rectangle (half-width hx, half-height hy, centred on 0, y up) that the ray from the centre at `angle` meets; a point that would fall in the bottom band
        /// (below -hy + lowest) moves to the side edge at that height instead, on the side of cos(angle) (the right when it is 0). Writes the point and returns whether it was moved off the bottom.</summary>
        public static bool EdgePoint(double angle, double hx, double hy, double lowest, out double px, out double py)
        {
            double c = Math.Cos(angle), s = Math.Sin(angle);
            double t = Math.Min(Math.Abs(c) > 1e-9 ? hx / Math.Abs(c) : double.MaxValue, Math.Abs(s) > 1e-9 ? hy / Math.Abs(s) : double.MaxValue);
            px = c * t;
            py = s * t;
            double floor = -hy + lowest;
            if (py >= floor) return false;
            double side = Math.Abs(c) > 1e-4 ? Math.Sign(c) : 1;
            px = side * hx;
            py = floor;
            if (Math.Abs(c) > 1e-4) py = Math.Max(floor, s * hx / Math.Abs(c)); // along the ray when the ray reaches the side edge above the floor
            return true;
        }

        /// <summary>The bearing of a target from the camera's heading in degrees: 0 ahead, positive to the right, -180 .. 180 (for the log; the sim's yaw 0 is north, -z, and a positive yaw turns left).</summary>
        public static double BearingDeg(double fromX, double fromZ, double yawRad, double toX, double toZ)
        {
            double dx = toX - fromX, dz = toZ - fromZ;
            double fx = -Math.Sin(yawRad), fz = -Math.Cos(yawRad); // the heading
            double rx = -fz, rz = fx;                              // its right-hand side in the x-z plane (the right of -z is +x)
            double ahead = dx * fx + dz * fz, right = dx * rx + dz * rz;
            return Math.Atan2(right, ahead) * 180 / Math.PI;
        }
    }
}
