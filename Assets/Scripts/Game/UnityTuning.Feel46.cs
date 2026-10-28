// NIGHT46 builder V1 (design.n46.feel46, a view row: no sim bit; Research/2026-10-17-wave46/V1.md gaps G1-G5): the render numbers of Feel46.cs
// and, at the end, the pure rule Feel46Warn (G1). CLAUDE.md rule 5: Unity-only numbers, each with a comment. Unity-free: the SimHarness
// compiles this one file for `n46feel unseen` (-p:Extra=../../Assets/Scripts/Game/UnityTuning.Feel46.cs), so the proof and the Player run
// the same rule on the same numbers. Times are real (frame-time) seconds: the kicks play through a freeze, as KillFeel42's do.
using System;
using Skygrind.Core;

namespace Skygrind.Game
{
    public static partial class UnityTuning
    {
        // --- G1: the off-screen wind-up warning (feel-flow-web F21: a directional cue for a threat you cannot see) -----------------------------
        /// <summary>m: a live wind-up (Tells36) at most this far from the eye is warned. The husk's lunge and the swarmer's pounce start
        /// within ~6 m; a spitter's volley and a drone's shot from farther, so the reach covers a ~30 x 30 m arena's half-diagonal.</summary>
        public const double Warn46RangeM = 22;
        /// <summary>A wind-up counts as seen (no marker) only when it sits inside this share of the view's half-extents: a body at the very
        /// edge of the frame is easy to miss, so the outer 8 % of each side still warns.</summary>
        public const double Warn46ViewShare = 0.92;
        /// <summary>The markers drawn at once at most (the nearest first): a pool, no allocation per frame.</summary>
        public const int Warn46Max = 6;
        /// <summary>px at 1080p: the marker's distance in from the screen edge (an ellipse through these insets: edge only, never the middle).</summary>
        public const float Warn46InsetPx = 64;
        /// <summary>px at 1080p: the chevron's size at the wind-up's start and at its end (it grows as the blow nears).</summary>
        public const float Warn46SizeFromPx = 46, Warn46SizeToPx = 74;
        /// <summary>The chevron's alpha at the wind-up's start and at its end (red-orange, colors.enemy: danger only, CLAUDE.md rule 8).</summary>
        public const float Warn46AlphaFrom = 0.55f, Warn46AlphaTo = 0.95f;
        /// <summary>Hz: the chevron's pulse in the last third of the wind-up (alpha x 0.75-1), so "about to land" reads apart from "started".</summary>
        public const float Warn46PulseHz = 9f;
        /// <summary>The danger colour (PortExport colors.enemy #FF4A1C), as an RGB hex.</summary>
        public const uint Warn46ColorHex = 0xFF4A1C;
        /// <summary>A behind-the-player wind-up (more than 90 deg from the look) sits on the bottom edge, pushed toward its side by this share
        /// of the quarter turn (0: always bottom centre; 1: the bottom corners): the side still reads.</summary>
        public const double Warn46BehindSide = 0.75;
        /// <summary>px at 1080p: a marker in the lower half sits on its side edge at least this far above the bottom inset, never on the bottom
        /// edge (round 4: n46-r2's behind chevron sat partly behind the gun model; the gun, the HP plate and the ammo fill the bottom band).</summary>
        public const float Warn46LowestPx = 240;
        /// <summary>The marker's canvas sorting order: under the HUD's (10), over the world.</summary>
        public const int Warn46SortingOrder = 8;
        /// <summary>The chevron's ink rim (the dusk navy ink, as Fusion43's ink twins): value contrast on level 2's white ice.</summary>
        public const uint Warn46InkHex = 0x141026;
        /// <summary>The chevron's stroke half-width and its rim's, in the sprite's -1..1 units; the rim's alpha.</summary>
        public const float Warn46CoreHalf = 0.13f, Warn46RimHalf = 0.24f, Warn46InkAlpha = 0.7f;

        // --- G2: a gun hit that did not kill: a small view kick (C-1's camera channel; smaller than any kill's) ---------------------------------
        /// <summary>deg of view pitch-up on a KESTREL / other light gun's body hit (the tap's own fire kick is 1.2 + 1.8 deg: this is a
        /// third of it, a confirm, not a recoil).</summary>
        public const double Kick46HitLightDeg = 0.45;
        /// <summary>deg on a THUNDERCLAP body hit (once a shot, however many pellets land).</summary>
        public const double Kick46HitHeavyDeg = 0.65;
        /// <summary>s: every feel46 kick's attack and recover (KickStack's curve): one 60 fps frame up, back over ~7 frames.</summary>
        public const double Kick46Attack = 0.02, Kick46Recover = 0.12;

        // --- G3: the kill ladder on camera (body < head < punch; NIGHT16's FOV punch-ins: headshot kill 1.5 deg, punch 1.6) ----------------------
        /// <summary>deg of FOV punch-in on a plain gun body kill (below the headshot kill's 1.5 and the punch's 1.6).</summary>
        public const double Fov46BodyKillDeg = 0.7;
        /// <summary>s: the punch-in's attack and recover (as feel.fovPunch: 0.02 / 0.08, a little longer back so it reads at 60 fps).</summary>
        public const double Fov46Attack = 0.02, Fov46Recover = 0.12;
        /// <summary>deg of view pitch-up on a plain gun body kill (above the hit's 0.45-0.65, below the headshot kill's).</summary>
        public const double Kick46BodyKillDeg = 0.9;
        /// <summary>deg of view pitch-up on a headshot gun kill (it had the FOV punch only: the ladder's middle).</summary>
        public const double Kick46HeadKillDeg = 1.3;
        /// <summary>deg of view pitch-up on a punch kill (the ladder's top; the gauntlet's own kick 1.6-3.4 deg stays KillFeel42's).</summary>
        public const double Kick46PunchKillDeg = 1.7;
        /// <summary>Kick slots alive at once (KickStack's capacity: a volley killing three adds three).</summary>
        public const int Kick46Slots = 6;
        /// <summary>Guns that hit in a stream (a tick every ~0.1 s): no hit / kill kick from them, or the view would buzz (C-3: the TORCH's
        /// feel is its held rumble and flame).</summary>
        public static readonly string[] Kick46SustainedWeapons = { "torch", "nailgun" };

        // --- G4: the THUNDERCLAP's shot shake under its kill's (C-2: each tier visibly bigger; F13: shake "like salt") --------------------------
        /// <summary>The share of a heavy gun's shot trauma kept with feel46 on, for both shakes: the crunch's ShakeFireHeavy (0.42 -> 0.25) and
        /// the web's feel.trauma.fire.heavy (0.3 -> 0.18). The kill keeps its 0.42 + 0.15 + 0.12.</summary>
        public const double Shake46HeavyFireKeep = 0.6;

        // --- G5: no full-screen effect (GG w30; DESIGN-RULES C-6) -----------------------------------------------------------------------------
        /// <summary>The edge band's inner edge (Chebyshev share of the half-screen from the centre) where the parry flash and the hurt static
        /// start, and where they are full (UiEdge31's hurt edge is clear inside 86 %: the same family, a little wider).</summary>
        public const float Edge46From = 0.72f, Edge46Full = 0.97f;
        /// <summary>The parry flash's centre ring: radius and half-width as shares of the screen's half-height (around the crosshair: the
        /// parry's "you caught it" lands where you look, the middle stays clear).</summary>
        public const float Ring46Radius = 0.16f, Ring46HalfWidth = 0.018f;
        /// <summary>The edge sprite's size in texels (one-off texture; the band's soft edge needs no more).</summary>
        public const int Edge46TexW = 384, Edge46TexH = 216;
        /// <summary>The hurt static's edge layer: its alpha per unit of juice38's static level (the full-screen grain was 0.22 per level; in a
        /// band it can be stronger and still never cover the middle).</summary>
        public const float Static46Alpha = 0.55f;
        /// <summary>Hz: the static's offset jumps this many times a second (TV static, not a smooth scroll).</summary>
        public const float Static46Hz = 30f;
        /// <summary>Static frames baked once (cycled at Static46Hz): four read as noise, not a loop, at 30 Hz.</summary>
        public const int Static46Frames = 4;
        /// <summary>How far the static's grey-white leans toward the danger colour (a hurt is danger: CLAUDE.md rule 8).</summary>
        public const float Static46TintMix = 0.45f;
    }
}

// NIGHT46 builder V1 (design.n46.feel46, G1): the off-screen wind-up warning's rule, pure (no UnityEngine): which live wind-ups (Core's
// Tells36, one per attacker, raised from the wind-up's first step) the player cannot see, and where on the screen's edge each one's marker
// sits. Feel46View.cs draws it every frame from the camera's look; the SimHarness's `n46feel unseen` runs the same rule on the sim's look for
// the proof ("warned before the hurt"). Read only: it never writes sim state. No allocation (the caller's arrays).

namespace Skygrind.Game
{
    public static class Feel46Warn
    {
        /// <summary>
        /// Fill `angle` (rad on the screen: 0 = right edge, +pi/2 = top) and `progress` (the wind-up's 0..1) and `source` for each live tell
        /// within UnityTuning.Warn46RangeM of `eye` that is outside Warn46ViewShare of the view cone (vertical half-angle tangent `tanV`, width /
        /// height `aspect`), nearest first, at most the arrays' length. `yaw` 0 looks toward -z, + `pitch` looks up (SurvivalSim.Aim's frame).
        /// Returns how many. `dist` (m) is filled too (the caller may order or fade by it).
        /// </summary>
        public static int Scan(Tells36 tells, Vec3 eye, double yaw, double pitch, double tanV, double aspect,
            double[] angle, double[] progress, int[] source, double[] dist)
        {
            if (tells == null || !tells.enabled) return 0;
            int max = Math.Min(Math.Min(angle.Length, progress.Length), Math.Min(source.Length, dist.Length));
            int n = 0;
            double cy = Math.Cos(yaw), sy = Math.Sin(yaw), cp = Math.Cos(pitch), sp = Math.Sin(pitch);
            double rx = cy, rz = -sy; // right (CameraFeel.UpdateRoll's)
            double fx = -sy * cp, fy = sp, fz = -cy * cp; // forward (SurvivalSim.Aim's)
            double ux = sy * sp, uy = cp, uz = cy * sp; // up
            double tanH = tanV * aspect, share = UnityTuning.Warn46ViewShare, range = UnityTuning.Warn46RangeM;
            foreach (Tell36 t in tells.Live)
            {
                if (t == null || !t.live) continue;
                double dx = t.at.x - eye.x, dy = t.at.y - eye.y, dz = t.at.z - eye.z;
                double d = Math.Sqrt(dx * dx + dy * dy + dz * dz);
                if (d > range || d < 1e-6) continue;
                double lx = dx * rx + dz * rz, lf = dx * fx + dy * fy + dz * fz, lu = dx * ux + dy * uy + dz * uz;
                if (lf > 0 && Math.Abs(lx / lf) <= tanH * share && Math.Abs(lu / lf) <= tanV * share) continue; // seen
                double a = EdgeAngle(lx, lf, lu, tanH, tanV);
                // insert by distance (nearest first); drop the farthest when full
                int at = n;
                while (at > 0 && dist[at - 1] > d) at--;
                if (at >= max) continue;
                int last = Math.Min(n, max - 1);
                for (int k = last; k > at; k--)
                {
                    angle[k] = angle[k - 1];
                    progress[k] = progress[k - 1];
                    source[k] = source[k - 1];
                    dist[k] = dist[k - 1];
                }
                angle[at] = a;
                progress[at] = t.Progress;
                source[at] = t.source;
                dist[at] = d;
                if (n < max) n++;
            }
            return n;
        }

        /// <summary>The marker's direction on the screen for a point at camera-frame (right `lx`, forward `lf`, up `lu`): in front, the
        /// direction of its projection (normalised by the half-extents, so a point past the right edge sits on the right edge); behind, the
        /// bottom edge pushed toward its side (UnityTuning.Warn46BehindSide).</summary>
        public static double EdgeAngle(double lx, double lf, double lu, double tanH, double tanV)
        {
            if (lf > 1e-6)
            {
                double sx = lx / lf / tanH, su = lu / lf / tanV;
                if (Math.Abs(sx) < 1e-9 && Math.Abs(su) < 1e-9) return -Math.PI / 2;
                return Math.Atan2(su, sx);
            }
            double h = Math.Sqrt(lx * lx + lf * lf);
            double side = h > 1e-9 ? lx / h : 0; // -1 left .. 1 right
            return -Math.PI / 2 + side * UnityTuning.Warn46BehindSide * (Math.PI / 2);
        }
    }
}
