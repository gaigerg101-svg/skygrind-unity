// NIGHT12 U3-25.6 (design.ceremony.livingTitle; Research/2026-10-03-polish.md F5 and section 3 #14: the first five seconds,
// a living title): display only, nothing here touches the sim.
// - TitleView: while TITLE shows, the camera flies a slow closed loop above the rooftops (a Catmull-Rom spline through
//   UnityTuning.TitlePathPoints points round the map's centre, from the map boxes' bounds: radius a share of the half
//   extent, height over the tallest box), looking across the map past its centre; the neon strips (game.mats.neon's
//   _BaseColor) pulse gently by a slow sine, restored as soon as TITLE is left. The lead calls Apply after CameraFeel
//   while the flow is Title; the restore runs on game.OnRender (every frame), so leaving by any path restores it.
// - Ui's half (this file's partial): TITLE's see-through backdrop (a dark band behind the centred column and a shade
//   under the controls line, generated gradients) and the wordmark's breath (menus.js .logo: brightness 1 -> 1.18 -> 1
//   over 4 s). The buttons are live from the first frame (nothing waits on the flight).
// Unity-only numbers (the path, the pulse, the band): UnityTuning.Ui.cs. No allocation per frame.
using System;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed class TitleView
    {
        readonly SurvivalGame game;
        readonly Vector3[] path; // Unity space, closed loop
        readonly Vector3 centre; // the look's anchor (Unity space)
        readonly bool enabled;
        static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        Color neonBase;
        bool pulsing;
        double clock;

        public TitleView(SurvivalGame g)
        {
            game = g;
            enabled = g.cfg.Design("ceremony.livingTitle");
            // The map's bounds from its boxes (web space): x / z extent and the tallest top.
            double minX = double.MaxValue, maxX = double.MinValue, minZ = double.MaxValue, maxZ = double.MinValue, top = 0;
            SurvivalMap map = g.map;
            if (map != null)
            {
                foreach (MapBox b in map.boxes)
                {
                    minX = Math.Min(minX, b.min.x);
                    maxX = Math.Max(maxX, b.max.x);
                    minZ = Math.Min(minZ, b.min.z);
                    maxZ = Math.Max(maxZ, b.max.z);
                    top = Math.Max(top, b.max.y);
                }
            }
            if (minX > maxX)
            {
                double half = map != null && map.floorSize > 0 ? map.floorSize * 0.5 : UnityTuning.TitleFallbackHalfExtent;
                minX = minZ = -half;
                maxX = maxZ = half;
            }
            double cx = (minX + maxX) * 0.5, cz = (minZ + maxZ) * 0.5;
            double rx = (maxX - minX) * 0.5 * UnityTuning.TitleOrbitShare, rz = (maxZ - minZ) * 0.5 * UnityTuning.TitleOrbitShare;
            centre = WebSpace.ToUnity(new Vec3(cx, top, cz)); // the look's height is set by a pitch (Apply), not by this
            int n = UnityTuning.TitlePathPoints;
            path = new Vector3[n];
            for (int i = 0; i < n; i++)
            {
                double a = 2 * Math.PI * i / n + UnityTuning.TitlePathPhase;
                double wide = i % 2 == 0 ? 1 : UnityTuning.TitlePathInner; // in and out, so the loop isn't a plain circle
                double y = top + (i % 2 == 0 ? UnityTuning.TitleHeightHigh : UnityTuning.TitleHeightLow);
                path[i] = WebSpace.ToUnity(new Vec3(cx + Math.Cos(a) * rx * wide, y, cz + Math.Sin(a) * rz * wide));
            }
            g.OnRender += AfterFrame;
        }

        /// <summary>The flow is Title: the camera on the loop, the strips pulsing. Call after CameraFeel.Apply.</summary>
        public void Apply(Camera cam, double frameDt)
        {
            if (!enabled || cam == null || game.flow != Flow.Title) return;
            clock += frameDt;
            double u = clock / UnityTuning.TitleLoopTime % 1.0;
            Vector3 at = Sample(u);
            // Look across the map: past the centre, away from the camera, a few deg below level (NIGHT17 round 4: the horizon
            // near mid-frame, the dusk sky above it).
            Vector3 toward = centre - at;
            toward.y = 0;
            Vector3 look = centre + toward * UnityTuning.TitleLookBeyond;
            float run = new Vector2(look.x - at.x, look.z - at.z).magnitude;
            look.y = at.y + Mathf.Tan(UnityTuning.TitleLookPitchDeg * Mathf.Deg2Rad) * run;
            cam.transform.position = at;
            cam.transform.rotation = Quaternion.LookRotation(look - at, Vector3.up);

            Material neon = game.mats != null ? game.mats.neon : null;
            if (neon == null) return;
            if (!pulsing)
            {
                neonBase = neon.GetColor(BaseColorId);
                pulsing = true;
            }
            float k = 1 + UnityTuning.TitleNeonPulse * Mathf.Sin((float)(clock * 2 * Math.PI / UnityTuning.TitleNeonPeriod));
            Color c = neonBase * k;
            c.a = neonBase.a;
            neon.SetColor(BaseColorId, c);
        }

        /// <summary>Every frame: once TITLE is left, the strips get their colour back.</summary>
        void AfterFrame(double frameDt, double alpha)
        {
            if (!pulsing || game.flow == Flow.Title) return;
            pulsing = false;
            if (game.mats != null && game.mats.neon != null) game.mats.neon.SetColor(BaseColorId, neonBase);
        }

        /// <summary>The closed Catmull-Rom loop at u in [0, 1).</summary>
        Vector3 Sample(double u)
        {
            int n = path.Length;
            double f = u * n;
            int i = (int)Math.Floor(f);
            float t = (float)(f - i);
            Vector3 p0 = path[(i - 1 + n) % n], p1 = path[i % n], p2 = path[(i + 1) % n], p3 = path[(i + 2) % n];
            float t2 = t * t, t3 = t2 * t;
            return 0.5f * (2 * p1 + (p2 - p0) * t + (2 * p0 - 5 * p1 + 4 * p2 - p3) * t2 + (3 * p1 - p0 - 3 * p2 + p3) * t3);
        }
    }
}
