// NIGHT18 (NIGHT18.md section 1.1, the lead's): the reel's per-frame telemetry. Static and allocation-free: every call is a
// no-op outside a reel (Recording false), so builders call it from their own files from minute one.
// - Effects: a builder reports each effect group it draws this frame (Effect / EffectWorld / EffectBounds / EffectUi), and
//   each chunk's centre (ChunkPoint); the probe keeps the union screen rect, the count and the strongest alpha per group.
// - Enemies: the husk view reports each drawn husk's bounds and flash (Enemy); the probe keeps its screen rect.
// - The camera, the sim step, HELD, and every UI Graphic under the game's canvases (the scanner: nobody registers
//   anything) are read by the probe itself at the frame's end (FeelProbe.EndFrame, called by SurvivalGame.RenderFrame).
// - Output: one JSONL file per scenario, written between scenarios (Tools/feel-schema.md is the format).
// Screen rects passed in are Unity screen pixels (origin bottom-left, as Camera.WorldToScreenPoint gives); the file has
// image pixels (origin top-left) at the capture's size.
using UnityEngine;

namespace Skygrind.Game
{
    /// <summary>The effect groups the probe measures (M2, M3, M8, the markers).</summary>
    public enum FeelFx
    {
        Chunks = 0, // the kill's thrown blocks (X)
        Burst = 1, // the kill's frame-1 burst / white-hot silhouette (X)
        Ring = 2, // the kill ring (X)
        KillMarker = 3, // the HUD kill confirm (H)
        HitMarker = 4, // the HUD hit marker (H)
        SpeedLines = 5, // the edge speed lines (M); alpha is the lines' strongest
        HitFlash = 6, // a hit's world flash / spark (R)
        Sparks = 7, // movement sparks and dust (M)
        Shed = 8, // a shed block (X)
        SpeedLinesClear = 9, // the clear box the speed lines leave in the middle (M; M8: it must hold the central 60 %)
        Gun = 10, // the gun in hand's screen rect through the viewmodel camera (M; backlog 6's recoil)
        Gust = 11, // NIGHT19: the KESTREL's gust (its air and wake: V; the reel's `gust` scenario)
        Climb = 12, // NIGHT20: a climber's tell and climb (N; the reel's `climb` scenario)
        Shockwave = 13, // NIGHT20: the THUNDERCLAP's shockwave (its air, rings and wake: V; the reel's `shockwave` scenario)
        Count = 14,
    }

    public static partial class FeelProbe
    {
        /// <summary>A reel scenario is recording (FeelStrip sets it). Every report is a no-op while false.</summary>
        public static bool Recording { get; private set; }

        // --- this frame's effect accumulators (reset at EndFrame) -----------------------------------------------------------

        static readonly float[] fxMinX = new float[(int)FeelFx.Count], fxMinY = new float[(int)FeelFx.Count];
        static readonly float[] fxMaxX = new float[(int)FeelFx.Count], fxMaxY = new float[(int)FeelFx.Count];
        static readonly int[] fxCount = new int[(int)FeelFx.Count];
        static readonly float[] fxAlpha = new float[(int)FeelFx.Count];
        static readonly Vector2[] chunkPts = new Vector2[UnityTuning.ProbeMaxChunkPoints];
        static int chunkPtCount, chunkPtTotal;

        /// <summary>An effect group drew `count` item(s) covering `screen` (Unity screen px, origin bottom-left) at `alpha` this frame.</summary>
        public static void Effect(FeelFx fx, Rect screen, float alpha = 1f, int count = 1)
        {
            if (!Recording || count <= 0 || alpha <= 0) return;
            int i = (int)fx;
            if (fxCount[i] == 0)
            {
                fxMinX[i] = screen.xMin;
                fxMinY[i] = screen.yMin;
                fxMaxX[i] = screen.xMax;
                fxMaxY[i] = screen.yMax;
            }
            else
            {
                fxMinX[i] = Mathf.Min(fxMinX[i], screen.xMin);
                fxMinY[i] = Mathf.Min(fxMinY[i], screen.yMin);
                fxMaxX[i] = Mathf.Max(fxMaxX[i], screen.xMax);
                fxMaxY[i] = Mathf.Max(fxMaxY[i], screen.yMax);
            }
            fxCount[i] += count;
            if (alpha > fxAlpha[i]) fxAlpha[i] = alpha;
        }

        /// <summary>An effect group drew a sphere of `radius` m at `world` (projected by the main camera; behind it: ignored).</summary>
        public static void EffectWorld(FeelFx fx, Vector3 world, float radius, float alpha = 1f, int count = 1)
        {
            if (!Recording) return;
            Camera cam = Cam;
            if (cam == null) return;
            Vector3 c = cam.WorldToScreenPoint(world);
            if (c.z <= cam.nearClipPlane) return;
            // the sphere's screen radius: the projected offset of a point `radius` m along the camera's up
            Vector3 e = cam.WorldToScreenPoint(world + cam.transform.up * radius);
            float r = Mathf.Max(1f, Vector2.Distance(new Vector2(c.x, c.y), new Vector2(e.x, e.y)));
            Effect(fx, new Rect(c.x - r, c.y - r, 2 * r, 2 * r), alpha, count);
        }

        /// <summary>An effect group drew something with world bounds `b` (its 8 corners projected).</summary>
        public static void EffectBounds(FeelFx fx, Bounds b, float alpha = 1f, int count = 1)
        {
            if (!Recording) return;
            if (ScreenRect(b, out Rect r)) Effect(fx, r, alpha, count);
        }

        /// <summary>A HUD element counts as effect group `fx` this frame (the kill / hit markers), at `alpha`.</summary>
        public static void EffectUi(FeelFx fx, RectTransform rt, float alpha = 1f)
        {
            if (!Recording || rt == null) return;
            if (UiScreenRect(rt, out Rect r)) Effect(fx, r, alpha, 1);
        }

        /// <summary>One chunk's centre this frame (M3: the cloud's screen centroid and its share outside the victim's rect).</summary>
        public static void ChunkPoint(Vector3 world)
        {
            if (!Recording) return;
            chunkPtTotal++;
            if (chunkPtCount >= chunkPts.Length) return;
            Camera cam = Cam;
            if (cam == null) return;
            Vector3 c = cam.WorldToScreenPoint(world);
            if (c.z <= cam.nearClipPlane) return;
            chunkPts[chunkPtCount++] = new Vector2(c.x, c.y);
        }

        // --- enemies (the husk view reports each drawn husk) ------------------------------------------------------------------

        static readonly int[] enemyId = new int[UnityTuning.ProbeMaxEnemies];
        static readonly Rect[] enemyRect = new Rect[UnityTuning.ProbeMaxEnemies];
        static readonly float[] enemyFlash = new float[UnityTuning.ProbeMaxEnemies];
        static readonly Vector2[] enemyAnchor = new Vector2[UnityTuning.ProbeMaxEnemies];
        static int enemyCount;

        /// <summary>A drawn enemy this frame: its parts' world bounds (body, head) and its flash share (0-1).</summary>
        public static void Enemy(int id, Bounds body, Bounds head, float flash)
        {
            if (!Recording || enemyCount >= enemyId.Length) return;
            body.Encapsulate(head);
            if (!ScreenRect(body, out Rect r)) return;
            enemyId[enemyCount] = id;
            enemyRect[enemyCount] = r;
            enemyFlash[enemyCount] = flash;
            enemyAnchor[enemyCount] = Anchor(id);
            enemyCount++;
        }

        /// <summary>The enemy's sim position (interpolated) + ProbeAnchorHeight m, on screen (Unity px): it moves only with the camera.</summary>
        static Vector2 Anchor(int id)
        {
            SurvivalGame g = SurvivalGame.Instance;
            Camera cam = Cam;
            if (g == null || cam == null || g.enemies == null) return new Vector2(float.NaN, float.NaN);
            System.Collections.Generic.List<Skygrind.Core.Enemy> all = g.enemies.all;
            for (int i = 0; i < all.Count; i++)
            {
                Skygrind.Core.Enemy en = all[i];
                if (en.id != id) continue;
                Skygrind.Core.Vec3 p = en.previous + (en.position - en.previous) * g.Alpha;
                Vector3 s = cam.WorldToScreenPoint(WebSpace.ToUnity(p) + Vector3.up * UnityTuning.ProbeAnchorHeight);
                return new Vector2(s.x, s.y);
            }
            return new Vector2(float.NaN, float.NaN);
        }

        // --- events (markers for the flipbooks' frame choice; the probe also listens on the bus) -------------------------------

        static readonly string[] eventNames = new string[16];
        static int eventCount;

        /// <summary>A named moment on this frame ("input", "shot", "kill", "flourish" ...): a constant string, never built per call.</summary>
        public static void Event(string name)
        {
            if (!Recording || eventCount >= eventNames.Length) return;
            eventNames[eventCount++] = name;
        }

        // --- helpers --------------------------------------------------------------------------------------------------------

        /// <summary>The main camera the probe projects with (SurvivalGame's).</summary>
        static Camera Cam => SurvivalGame.Instance != null ? SurvivalGame.Instance.cam : null;

        static readonly Vector3[] corners = new Vector3[8];
        static readonly Vector3[] uiCorners = new Vector3[4];

        /// <summary>World bounds to a Unity screen rect (false when every corner is behind the camera).</summary>
        public static bool ScreenRect(Bounds b, out Rect r)
        {
            r = default;
            Camera cam = Cam;
            if (cam == null) return false;
            Vector3 mn = b.min, mx = b.max;
            corners[0] = new Vector3(mn.x, mn.y, mn.z);
            corners[1] = new Vector3(mx.x, mn.y, mn.z);
            corners[2] = new Vector3(mn.x, mx.y, mn.z);
            corners[3] = new Vector3(mx.x, mx.y, mn.z);
            corners[4] = new Vector3(mn.x, mn.y, mx.z);
            corners[5] = new Vector3(mx.x, mn.y, mx.z);
            corners[6] = new Vector3(mn.x, mx.y, mx.z);
            corners[7] = new Vector3(mx.x, mx.y, mx.z);
            float x0 = float.MaxValue, y0 = float.MaxValue, x1 = float.MinValue, y1 = float.MinValue;
            bool any = false;
            for (int i = 0; i < 8; i++)
            {
                Vector3 s = cam.WorldToScreenPoint(corners[i]);
                if (s.z <= cam.nearClipPlane) continue;
                any = true;
                if (s.x < x0) x0 = s.x;
                if (s.y < y0) y0 = s.y;
                if (s.x > x1) x1 = s.x;
                if (s.y > y1) y1 = s.y;
            }
            if (!any) return false;
            r = Rect.MinMaxRect(x0, y0, x1, y1);
            return true;
        }

        /// <summary>A UI element's screen rect (overlay canvases: world corners are screen px; camera canvases: projected).</summary>
        public static bool UiScreenRect(RectTransform rt, out Rect r)
        {
            r = default;
            rt.GetWorldCorners(uiCorners);
            Canvas c = rt.GetComponentInParent<Canvas>();
            Camera cam = c != null && c.renderMode != RenderMode.ScreenSpaceOverlay ? c.worldCamera : null;
            float x0 = float.MaxValue, y0 = float.MaxValue, x1 = float.MinValue, y1 = float.MinValue;
            for (int i = 0; i < 4; i++)
            {
                Vector3 s = cam != null ? cam.WorldToScreenPoint(uiCorners[i]) : uiCorners[i];
                if (s.x < x0) x0 = s.x;
                if (s.y < y0) y0 = s.y;
                if (s.x > x1) x1 = s.x;
                if (s.y > y1) y1 = s.y;
            }
            r = Rect.MinMaxRect(x0, y0, x1, y1);
            return true;
        }

        /// <summary>The frame's end (SurvivalGame.RenderFrame, after every view drew): record the frame, reset the accumulators.</summary>
        public static void EndFrame(SurvivalGame g)
        {
            if (!Recording || FeelClock.Holding) return; // NIGHT63 D1: a same-run held frame is no frame of the recording (no row, no f####.jpg; its accumulators carry to the next)
            RecordFrame(g);
            for (int i = 0; i < fxCount.Length; i++)
            {
                fxCount[i] = 0;
                fxAlpha[i] = 0;
            }
            chunkPtCount = chunkPtTotal = 0;
            enemyCount = 0;
            eventCount = 0;
        }

        /// <summary>The recorder (FeelProbe.Record.cs, the lead's): the frame into the ring.</summary>
        static partial void RecordFrame(SurvivalGame g);
    }
}
