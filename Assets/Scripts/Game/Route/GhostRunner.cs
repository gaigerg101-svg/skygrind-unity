// NIGHT14 U3-27.11 (agent V2): the GHOST runner (design.ghost; D-53): your best run on this seed as a faint figure in the
// player's cyan (colors.usable at design.ghost.alpha, the SKYGRIND/Acid alpha blend: never magenta, RIVAL's, never
// red-orange, danger's), with a pale trail of design.ghost.trailSeconds that fades to nothing. The body is the player's
// capsule (movement.body: capsuleRadius, capsuleHeight) as a cylinder standing on the best's feet; the trail is
// UnityTuning.GhostTrailSegments thin tubes at the body's middle height between the best's positions over the last
// trailSeconds of run time, each a little fainter. Everything is placed from GhostTrack.Sample by the run's clock: it stops
// when the run stops (a pause, hitstop) and the trail is the best's own path, not the frames'. Built once; a frame only
// moves transforms (nothing allocated); hidden off a run, in DOWN, past the best's end and with no best.
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed class GhostRunner
    {
        readonly GameObject root;
        readonly Transform body;
        readonly Transform[] segments;
        readonly Vector3[] trail;
        readonly double trailSteps;
        readonly float middle, tube;
        bool shown;

        /// <summary>The runner is drawn this frame (automation reads it).</summary>
        public bool Visible => shown;

        public GhostRunner(SurvivalGame game)
        {
            GameConfig cfg = game.cfg;
            Color cyan = Look.C(cfg, "colors.usable");
            float alpha = (float)cfg.Num("design.ghost.alpha");
            trailSteps = cfg.Num("design.ghost.trailSeconds") * cfg.movement.loop.simHz;
            BodyConfig b = cfg.movement.body;
            middle = (float)(b.capsuleHeight * 0.5);
            tube = UnityTuning.GhostTrailRadius;
            root = new GameObject("Ghost runner");
            var bodyMat = new Material(game.mats.acid) { name = "Ghost" };
            bodyMat.SetColor("_BaseColor", new Color(cyan.r, cyan.g, cyan.b, alpha));
            Mesh cylinder = RunShapeKit.Cylinder((float)b.capsuleRadius, (float)b.capsuleRadius, (float)b.capsuleHeight, UnityTuning.GhostBodySides, false, "Ghost body");
            body = MeshKit.Object("Body", root.transform, cylinder, bodyMat, shadows: false).transform;
            int n = UnityTuning.GhostTrailSegments;
            Mesh unitTube = RunShapeKit.Cylinder(1, 1, 1, 8, true, "Ghost trail");
            Color pale = Color.Lerp(cyan, Color.white, UnityTuning.GhostTrailPale);
            segments = new Transform[n];
            trail = new Vector3[n + 1];
            for (int i = 0; i < n; i++)
            {
                var m = new Material(game.mats.acid) { name = "Ghost trail " + i };
                m.SetColor("_BaseColor", new Color(pale.r, pale.g, pale.b, alpha * (1 - (i + 0.5f) / n)));
                segments[i] = MeshKit.Object("Trail " + i, root.transform, unitTube, m, shadows: false).transform;
            }
            root.SetActive(false);
        }

        /// <summary>Hide it now (the player's death: WHAT GOT YOU may draw the next frames without the props pass).</summary>
        public void Hide()
        {
            if (!shown) return;
            shown = false;
            root.SetActive(false);
        }

        /// <summary>Place the runner at the best's run step `runStep` (show: a run is on and races this best).</summary>
        public void Render(GhostTrack best, double runStep, bool show)
        {
            bool on = show && best != null && best.Sample(runStep, out double x, out double y, out double z);
            if (on != shown)
            {
                shown = on;
                root.SetActive(on);
            }
            if (!on) return;
            best.Sample(runStep, out x, out y, out z);
            Vector3 feet = WebSpace.ToUnity(new Vec3(x, y, z));
            body.localPosition = feet;
            Vector3 up = new Vector3(0, middle, 0);
            int n = segments.Length;
            double dt = trailSteps / n, start = best.startStep;
            for (int i = 0; i <= n; i++)
            {
                double at = runStep - i * dt;
                if (at < start) at = start;
                best.Sample(at, out double tx, out double ty, out double tz);
                trail[i] = WebSpace.ToUnity(new Vec3(tx, ty, tz)) + up;
            }
            for (int i = 0; i < n; i++) Stretch(segments[i], trail[i + 1], trail[i]);
        }

        /// <summary>The unit tube (base at 0, top at +y 1) from a to b at the trail's radius; a zero-length one shrinks away.</summary>
        void Stretch(Transform t, Vector3 a, Vector3 b)
        {
            Vector3 d = b - a;
            float length = d.magnitude;
            if (length < 1e-4f)
            {
                t.localScale = Vector3.zero;
                return;
            }
            t.SetPositionAndRotation(a, Quaternion.FromToRotation(Vector3.up, d / length));
            t.localScale = new Vector3(tube, length, tube);
        }
    }
}
