// NIGHT20 row 6 beyond (builder V): the long slide's wind wisps (Research/2026-10-06-speed-lines.md 1.2 #6 and U3-34.7's
// beyond: "the wind-trail wiggle for variety on long slides"). Ported from dip000/my-godotshaders
// WindTrailWithLoops/wind_trail_mesh.gdshader (MIT, Copyright (c) 2023 Adrián): its vertex() line for line on a wisp's
// centre line (VERTEX.x = VERTEX.z = 0): the straight-travel wave (sin(y * wave_frequency + t) * wave_amplitude) and the
// back-forth twist (sin(y * twist_frequency + t) * twist_amplitude) along a unit length x wave_length; every wave_count
// periods it blends toward a loop (sin / cos of y * TAU * loop_count + t, x loop_radius) over one period and back over the
// next ("most satisfying line ever :D"). Changes: no travel along y (VERTEX.y += t: here the wisp trails the body), the
// fade_in_out as the slide's own in and out, drawn as soft trace_06 segments (FxSprites) instead of a tube mesh.
// After WindAfter s of a slide faster than a walk, two cyan-pale wisps (the player's) trail from the body's sides at waist
// height, laid back along the slide; they fade out over WindFade s when it ends. None with REDUCED MOTION or EFFECTS
// INTENSITY 0 (a motion flourish), none on the A side (-skygrindOff20). World time; no allocation per frame.
using System;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    /// <summary>The wind wisp's shape, pure (Wind20Tests).</summary>
    public static class WindTrailMath
    {
        const double Tau = Math.PI * 2;

        /// <summary>The shader's blend factor at `scaledTime` (TIME x time_scale + extra_offset_time): 0 the wave, to 0.5 the
        /// loop-wave mix and back.</summary>
        public static double Factor(double scaledTime)
        {
            double period = Tau; // the animation period is given by the loop (1 m at 1 m/s: 2 pi s)
            double offsetTime = period * 0.5; // so the transitions don't bend awkwardly at the wrong moment
            double holdTime = UnityTuning.WindWaveCount * period;
            double totalTime = holdTime + 2.0 * period;
            double x = scaledTime + offsetTime;
            double t = x - totalTime * Math.Floor(x / totalTime); // GLSL mod
            if (t < holdTime) return 0.0; // wave
            if (t < holdTime + period) return (t - holdTime) / period * 0.5; // wave to loop-wave
            return (1.0 - (t - holdTime - period) / period) * 0.5; // loop-wave to wave
        }

        /// <summary>A wisp's centre-line point at `y01` of its length: x the side (the twist), y along it (m), z up (the wave
        /// and the loop).</summary>
        public static Vector3 Point(double y01, double scaledTime)
        {
            double t = scaledTime;
            double wx = Math.Sin(y01 * UnityTuning.WindTwistFreq + t) * UnityTuning.WindTwistAmp;
            double wy = y01 * UnityTuning.WindLength;
            double wz = Math.Sin(y01 * UnityTuning.WindWaveFreq + t) * UnityTuning.WindWaveAmp;
            double loopAngle = y01 * Tau * UnityTuning.WindLoopCount + t;
            double ly = Math.Sin(loopAngle) * UnityTuning.WindLoopRadius;
            double lz = Math.Cos(loopAngle) * UnityTuning.WindLoopRadius;
            double f = Factor(scaledTime);
            return new Vector3((float)(wx * (1 - f)), (float)(wy + (ly - wy) * f), (float)(wz + (lz - wz) * f)); // mix(wave, loop, factor)
        }

        /// <summary>The wisps' brightness share: in over WindFade s of `age`, out as `left` (s of the fade-out left) runs down.</summary>
        public static double Fade(double age, double left)
        {
            double fin = UnityTuning.WindFade > 0 ? Math.Min(1, Math.Max(0, age) / UnityTuning.WindFade) : 1;
            double fout = UnityTuning.WindFade > 0 ? Math.Min(1, Math.Max(0, left) / UnityTuning.WindFade) : 1;
            return fin * fout;
        }
    }

    public sealed partial class MovementFx
    {
        double windSlide, windAge, windLeft, windClock;
        bool windOn;

        /// <summary>The wisps this frame (inside Textured20Render's trace batch); true when something was added.</summary>
        bool Wind20Frame(double dt, Vector3 eye, float intensity)
        {
            Mover mover = game.mover;
            SettingsConfig st = game.cfg.movement.settings;
            double speed = Math.Sqrt(mover.velocity.x * mover.velocity.x + mover.velocity.z * mover.velocity.z);
            bool sliding = mover.state == MoveState.Slide && mover.grounded && speed > walkSpeed && game.Active && FxGates.MotionOn(st.reducedMotion, st.effectsIntensity);
            if (sliding)
            {
                windSlide += dt;
                if (windSlide >= UnityTuning.WindAfter)
                {
                    if (!windOn)
                    {
                        windOn = true;
                        windAge = 0;
                    }
                    windLeft = UnityTuning.WindFade;
                }
            }
            else
            {
                windSlide = 0;
                if (windOn)
                {
                    windLeft -= dt;
                    if (windLeft <= 0 || !FxGates.MotionOn(st.reducedMotion, st.effectsIntensity)) windOn = false;
                }
            }
            if (!windOn || speed < 1e-3) return false;
            windAge += dt;
            windClock += dt;
            float fade = (float)WindTrailMath.Fade(windAge, windLeft) * UnityTuning.WindAlpha * intensity;
            if (fade <= 0) return false;
            Vector3 back = WebSpace.ToUnity(new Vec3(-mover.velocity.x / speed, 0, -mover.velocity.z / speed));
            Vector3 side = Vector3.Cross(Vector3.up, back);
            Vector3 feet = WebSpace.ToUnity(mover.position);
            Color pale = UiTheme.Ready ? UiTheme.CyanPale : Color.Lerp(usable, Color.white, 0.5f);
            for (int w = 0; w < UnityTuning.WindWisps; w++)
            {
                float sign = (w & 1) == 0 ? 1 : -1;
                Vector3 anchor = feet + Vector3.up * (float)UnityTuning.WindHeight + side * (sign * (float)UnityTuning.WindSide);
                double scaled = windClock * UnityTuning.WindTimeScale + w * 1.7; // extra_offset_time: the two wisps out of step
                Vector3 prev = default;
                for (int i = 0; i < UnityTuning.WindPoints; i++)
                {
                    double y = i / (double)(UnityTuning.WindPoints - 1);
                    Vector3 l = WindTrailMath.Point(y, scaled);
                    Vector3 p = anchor + back * l.y + side * (sign * l.x) + Vector3.up * l.z;
                    if (i > 0) trace20.Streak(prev, p, (float)UnityTuning.WindWidth, eye, pale, fade * (float)(1 - y * 0.85)); // thinning to its tail
                    prev = p;
                }
            }
            return true;
        }
    }
}
