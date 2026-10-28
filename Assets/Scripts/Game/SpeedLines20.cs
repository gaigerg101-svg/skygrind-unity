// NIGHT20 U3-34.7 (builder V): AIR, the world-space speed streaks (Research/2026-10-06-speed-lines.md section 3; GG: "copy
// an open-source project that has way better dash lines that mimic speed, not just that weird border").
// Ported from two GitHub projects, rewritten in C# (no file vendored):
// - keijiro/KvantStream (MIT, Copyright (C) 2015 Keijiro Takahashi; the licence text is in its README):
//   Assets/Kvant/Stream/Shaders/Line.shader: a line's two ends are the particle's position now and an exposure ago,
//   `p = lerp(p2, p1, (1 - sw) * _Tail)`, alpha x sw (whole at one end, 0 at the other). Here: each streak is a world-fixed
//   air mote (its head, where the eye sees it now) and a tail recomputed every frame as head + v x T (where it was, relative
//   to the eye, T s ago), alpha 1 at the head and 0 at the tail (the shader's `along`). So the length IS the travel over an
//   exposure: it grows with speed, points away from the true focus of expansion (a strafe draws sideways, a fall upward)
//   and shrinks to a dot when you stop: the boost's end collapses the lines physically instead of fading a frame.
// - MikeFrom/Godot-Particles-Tunnel-Speed-Lines (CC0-1.0): godot220222_tunnel_speed_lines/tunnel_speed_lines.tscn: each
//   particle is born on a ring (`angle_rand`, `(sin a, cos a) * tunnel_radius`) and its quad lies flat along the tunnel wall
//   (the ring's tangent as its width axis, CUSTOM.yz). Here: births on a screen ring about the crosshair (rho 0.40-1.10,
//   area-uniform), unprojected at a depth of 3-14 m; the quad's width axis is cross(streak, view ray): the same tangent for
//   a streak along the view axis, and right for any direction.
// The rules (UnityTuning.SpeedLines.cs, every number a starting value):
// - the speed signal v_eff = sqrt(vx^2 + vz^2 + (0.6 vy)^2) from the mover; k = clamp((v_eff - 9) / 15, 0, 1)^0.6 (sprint
//   0.25, slide 0.35, boost 1), rising at once, falling with a 0.08 s half-life;
// - births 120 x k a second (x2 for the boost's 0.25 s), a ray from the eye to each (the sim's GameWorld.RayIntersect)
//   rejecting one behind the world, and none within 40 px of a live enemy's screen rect; life 0.38 s +- 25 %;
// - bursts: 32 x the boost's share on its onset, 20 on a boost-jump, 18 on the THUNDERCLAP's shockwave (N20Events);
// - at most 64 drawn (the newest), one dynamic mesh, one draw (SKYGRIND/SpeedLines20: additive, depth-tested, the central
//   clear ellipse per pixel); x0.3 zoomed; none with REDUCED MOTION or at EFFECTS INTENSITY 0; a streak that comes to
//   cross a live enemy's rect is dropped (L5);
// - world time (frozen in a hitstop and the pause); its dice are its own Mulberry32 (a render-side stream, seeded per run).
// The switch: live unless N20Blocks.ForceOff (-skygrindOff20, the A side: NIGHT19's look; the old layers draw as before).
// The reel: FeelFx.SpeedLines (the drawn streaks' rect, max alpha, count) and FeelFx.SpeedLinesClear every frame, and the
// statics below (M8 v2: L1-L5).
using System;
using Skygrind.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace Skygrind.Game
{
    /// <summary>The AIR streaks' pure rules (SpeedLines20Tests).</summary>
    public static class SpeedLines20Math
    {
        static double Clamp01(double x) => x < 0 ? 0 : x > 1 ? 1 : x;

        /// <summary>GLSL smoothstep(e0, e1, x).</summary>
        public static double SmoothStep(double e0, double e1, double x)
        {
            double t = Clamp01((x - e0) / (e1 - e0));
            return t * t * (3 - 2 * t);
        }

        /// <summary>The speed signal: horizontal in full, the vertical at SpeedLinesVerticalShare.</summary>
        public static double VEff(double vx, double vy, double vz)
        {
            double y = UnityTuning.SpeedLinesVerticalShare * vy;
            return Math.Sqrt(vx * vx + vz * vz + y * y);
        }

        /// <summary>k = clamp((v - gate) / (full - gate), 0, 1)^curve; 0 for no speed.</summary>
        public static double Intensity(double vEff)
        {
            if (!(vEff > UnityTuning.SpeedLinesGate)) return 0;
            double u = Clamp01((vEff - UnityTuning.SpeedLinesGate) / (UnityTuning.SpeedLinesFull - UnityTuning.SpeedLinesGate));
            return Math.Pow(u, UnityTuning.SpeedLinesCurve);
        }

        /// <summary>k one frame on: up at once, down with SpeedLinesReleaseHalfLife; held at dt 0 (a hitstop).</summary>
        public static double SmoothK(double k, double target, double dt) => SmoothKWith(k, target, dt, UnityTuning.SpeedLinesReleaseHalfLife);

        /// <summary>The same with its own release half-life.</summary>
        public static double SmoothKWith(double k, double target, double dt, double halfLife)
        {
            if (target >= k) return target;
            if (!(dt > 0)) return k;
            return target + (k - target) * Math.Pow(0.5, dt / halfLife);
        }

        /// <summary>Build 3 (V5): every streak's share by the fade's k: whole from SpeedLinesFadeK, 0 at 0 (under the gate).</summary>
        public static double FadeShare(double kFade) => Clamp01(kFade / UnityTuning.SpeedLinesFadeK);

        /// <summary>Build 3 (V3): the longest a streak may draw on a screen `heightPx` tall at k: SpeedLinesCapPxLow to
        /// SpeedLinesCapPxHigh (1080p px) from SpeedLinesCapKLow to 1.</summary>
        public static float CapPx(double k, float heightPx)
        {
            double u = Clamp01((k - UnityTuning.SpeedLinesCapKLow) / (1 - UnityTuning.SpeedLinesCapKLow));
            return (float)((UnityTuning.SpeedLinesCapPxLow + (UnityTuning.SpeedLinesCapPxHigh - UnityTuning.SpeedLinesCapPxLow) * u) * heightPx / UnityTuning.SpeedLinesRefHeight);
        }

        /// <summary>Births a second: SpeedLinesRate x k, x SpeedLinesBoostRateMult while a boost runs; 0 with the motion gate shut.</summary>
        public static double SpawnRate(double k, bool boosting, bool motionOn) =>
            !motionOn || !(k > 0) ? 0 : UnityTuning.SpeedLinesRate * k * (boosting ? UnityTuning.SpeedLinesBoostRateMult : 1);

        /// <summary>The exposure (s of travel a flow streak shows) at k: Min + (Max - Min) k^Curve.</summary>
        public static double Exposure(double k) =>
            UnityTuning.SpeedLinesExposureMin + (UnityTuning.SpeedLinesExposureMax - UnityTuning.SpeedLinesExposureMin) * Math.Pow(Clamp01(k), UnityTuning.SpeedLinesExposureCurve);

        /// <summary>
        /// A birth whose ray from the eye met the world at `hitAlong` m: a place along the same ray in front of it, between
        /// SpeedLinesFreeMin and the hit less SpeedLinesFreeMargin (u in 0..1); false when there's no such room (redraw).
        /// </summary>
        public static bool FreeAlong(double hitAlong, double u01, out double along)
        {
            double far = hitAlong - UnityTuning.SpeedLinesFreeMargin;
            along = 0;
            if (!(far > UnityTuning.SpeedLinesFreeMin)) return false;
            along = UnityTuning.SpeedLinesFreeMin + (far - UnityTuning.SpeedLinesFreeMin) * Clamp01(u01);
            return true;
        }

        /// <summary>
        /// KvantStream's rule: the tail is head + v x exposure (where the mote was, relative to the eye, an exposure ago), its
        /// length clamped to SpeedLinesLengthMin..Max; with no velocity, the minimum along `lastDir` (unit).
        /// </summary>
        public static Vector3 Tail(Vector3 head, Vector3 v, double exposure, Vector3 lastDir)
        {
            double speed = v.magnitude;
            Vector3 dir = speed > 1e-6 ? v / (float)speed : lastDir;
            double len = Math.Max(UnityTuning.SpeedLinesLengthMin, Math.Min(UnityTuning.SpeedLinesLengthMax, speed * exposure));
            return head + dir * (float)len;
        }

        /// <summary>The elliptical screen radius about the crosshair from normalised coordinates (-1..1 each axis).</summary>
        public static double Rho(double nx, double ny) => Math.Sqrt(nx * nx + ny * ny);

        /// <summary>rho of screen pixel (x, y) on a w x h screen (0 the middle, 1 at the edge midpoints).</summary>
        public static double RhoPx(float x, float y, float w, float h) => Rho(2.0 * x / w - 1, 2.0 * y / h - 1);

        /// <summary>The clear zone's share: 0 inside SpeedLinesClearIn, whole from SpeedLinesClearFull (a smoothstep).</summary>
        public static double Clear(double rho) => rho <= UnityTuning.SpeedLinesClearIn ? 0 : SmoothStep(UnityTuning.SpeedLinesClearIn, UnityTuning.SpeedLinesClearFull, rho);

        /// <summary>The most alpha a pixel at rho may draw: SpeedLinesCapAlpha to SpeedLinesClearFull, rising to 1 by SpeedLinesCapFull.</summary>
        public static double Cap(double rho) =>
            UnityTuning.SpeedLinesCapAlpha + (1 - UnityTuning.SpeedLinesCapAlpha) * SmoothStep(UnityTuning.SpeedLinesClearFull, UnityTuning.SpeedLinesCapFull, rho);

        /// <summary>A pixel's alpha at rho for a streak of `streakAlpha` (the shader's rule, per pixel).</summary>
        public static double PixelAlpha(double streakAlpha, double rho) => Math.Min(streakAlpha * Clear(rho), Cap(rho));

        /// <summary>Over a streak's life (0..1): in over SpeedLinesLifeFadeIn, out from SpeedLinesLifeFadeOut to 1.</summary>
        public static double LifeFade(double t01) =>
            SmoothStep(0, UnityTuning.SpeedLinesLifeFadeIn, t01) * (1 - SmoothStep(UnityTuning.SpeedLinesLifeFadeOut, 1, t01));

        /// <summary>By its distance from the eye (m): 0 near, whole between, 0 far.</summary>
        public static double DistFade(double d) =>
            SmoothStep(UnityTuning.SpeedLinesNearFade0, UnityTuning.SpeedLinesNearFade1, d) * (1 - SmoothStep(UnityTuning.SpeedLinesFarFade0, UnityTuning.SpeedLinesFarFade1, d));

        /// <summary>x (1 - SpeedLinesAdsCut x the ADS amount): 0.3 fully zoomed.</summary>
        public static double AdsScale(double adsAmount) => 1 - UnityTuning.SpeedLinesAdsCut * Clamp01(adsAmount);

        /// <summary>A streak's alpha: peak x life fade x distance fade x ADS x effects intensity; 0 with REDUCED MOTION.</summary>
        public static double StreakAlpha(double peak, double age, double life, double dist, double ads, double intensity, bool reducedMotion)
        {
            if (reducedMotion || !(intensity > 0) || !(life > 0)) return 0;
            return peak * LifeFade(age / life) * DistFade(dist) * ads * intensity;
        }

        /// <summary>A birth's rho in the ring r0..r1, area-uniform (u in 0..1).</summary>
        public static double RingRho(double u, double r0, double r1) => Math.Sqrt(r0 * r0 + (r1 * r1 - r0 * r0) * Clamp01(u));

        /// <summary>Does the segment a-b touch rect r (Liang-Barsky)?</summary>
        public static bool SegmentHitsRect(float ax, float ay, float bx, float by, Rect r)
        {
            double t0 = 0, t1 = 1, dx = bx - ax, dy = by - ay;
            return Clip(-dx, ax - r.xMin, ref t0, ref t1) && Clip(dx, r.xMax - ax, ref t0, ref t1)
                && Clip(-dy, ay - r.yMin, ref t0, ref t1) && Clip(dy, r.yMax - ay, ref t0, ref t1);
        }

        static bool Clip(double p, double q, ref double t0, ref double t1)
        {
            if (p == 0) return q >= 0;
            double t = q / p;
            if (p < 0)
            {
                if (t > t1) return false;
                if (t > t0) t0 = t;
            }
            else
            {
                if (t < t0) return false;
                if (t < t1) t1 = t;
            }
            return true;
        }

        /// <summary>The share (0..1) of segment a-b inside rect r (Liang-Barsky): the on-screen share of a streak.</summary>
        public static double ShareInRect(float ax, float ay, float bx, float by, Rect r)
        {
            double t0 = 0, t1 = 1, dx = bx - ax, dy = by - ay;
            if (Clip(-dx, ax - r.xMin, ref t0, ref t1) && Clip(dx, r.xMax - ax, ref t0, ref t1)
                && Clip(-dy, ay - r.yMin, ref t0, ref t1) && Clip(dy, r.yMax - ay, ref t0, ref t1))
                return Math.Max(0, t1 - t0);
            return 0;
        }

        /// <summary>The part of segment a-b (normalised screen coordinates) inside the circle of radius r about the
        /// crosshair: true and its span t0..t1 (0..1 along a-b) when there is one.</summary>
        public static bool InsideSpan(double ax, double ay, double bx, double by, double r, out double t0, out double t1)
        {
            t0 = t1 = 0;
            double dx = bx - ax, dy = by - ay;
            double a = dx * dx + dy * dy, b = 2 * (ax * dx + ay * dy), c = ax * ax + ay * ay - r * r;
            if (a < 1e-18) // a point
            {
                if (c > 0) return false;
                t1 = 1;
                return true;
            }
            double disc = b * b - 4 * a * c;
            if (disc <= 0) return false;
            double s = Math.Sqrt(disc);
            double r0 = (-b - s) / (2 * a), r1 = (-b + s) / (2 * a);
            if (r1 <= 0 || r0 >= 1) return false;
            t0 = Math.Max(0, r0);
            t1 = Math.Min(1, r1);
            return t1 > t0;
        }

        /// <summary>A boost's share of the burst: SpeedLinesBoostPartialMin .. 1 by the boost's fraction (CameraFeel's BoostPeak).</summary>
        public static double BoostShare(double fraction) =>
            UnityTuning.SpeedLinesBoostPartialMin + (1 - UnityTuning.SpeedLinesBoostPartialMin) * Clamp01(fraction);

        /// <summary>A flow streak's life: SpeedLinesLife +- SpeedLinesLifeJitter (u in 0..1).</summary>
        public static double Life(double u01) => UnityTuning.SpeedLinesLife * (1 + UnityTuning.SpeedLinesLifeJitter * (2 * Clamp01(u01) - 1));

        /// <summary>A view-space point (x right, y up, z ahead, m) to screen px (origin bottom-left) on a w x h screen with the
        /// view's half-angle tangents; false at or behind `near`.</summary>
        public static bool ProjectView(float x, float y, float z, float tanH, float tanV, float w, float h, float near, out Vector2 px)
        {
            if (!(z > near))
            {
                px = default;
                return false;
            }
            px = new Vector2((x / (z * tanH) + 1) * 0.5f * w, (y / (z * tanV) + 1) * 0.5f * h);
            return true;
        }

        /// <summary>A screen fraction t (0..1 along a projected segment) as the world fraction of the segment whose ends sit at
        /// view depths zA and zB (perspective: s = t zA / ((1 - t) zB + t zA)).</summary>
        public static double WorldParam(double t, double zA, double zB)
        {
            double den = (1 - t) * zB + t * zA;
            return den > 1e-9 ? Math.Max(0, Math.Min(1, t * zA / den)) : t;
        }

        /// <summary>The head of a birth at ring (nx, ny) and view depth d (m), in view space.</summary>
        public static Vector3 BirthView(float nx, float ny, float d, float tanH, float tanV) => new Vector3(nx * tanH * d, ny * tanV * d, d);

        /// <summary>A screen segment's acute angle to the horizontal (deg): 0 horizontal, 90 vertical.</summary>
        public static double AcuteAngleDeg(float dx, float dy) => Math.Atan2(Math.Abs(dy), Math.Abs(dx)) * 180 / Math.PI;

        /// <summary>The median of the first `count` values (sorted in place); 0 for none.</summary>
        public static float Median(float[] values, int count)
        {
            if (count <= 0) return 0;
            Array.Sort(values, 0, count);
            return (count & 1) == 1 ? values[count / 2] : 0.5f * (values[count / 2 - 1] + values[count / 2]);
        }
    }

    /// <summary>The AIR streaks' view (one per game; SurvivalGame.Fx20.cs drives it).</summary>
    public sealed class SpeedLines20
    {
        // --- the switch and the lead's telemetry (M8 v2) -----------------------------------------------------------------

        /// <summary>Tonight's look: on unless every NIGHT20 block is forced off (-skygrindOff20 / -skygrindOff19: the A side).
        /// The old layers (UiFeel's two, MovementFx's dash tint and sprint lines) draw only while this is false.</summary>
        public static bool Live => !N20Blocks.ForceOff;

        /// <summary>Streaks drawn this frame (L2).</summary>
        public static int LiveCount { get; private set; }

        /// <summary>The drawn streaks' median screen length this frame, in px at the capture's size (the part on screen and
        /// outside the clear ellipse) (L3; x 1080 / the height for 1080p).</summary>
        public static float MedianScreenLengthPx { get; private set; }

        /// <summary>The drawn streaks' median acute angle to the screen's horizontal this frame (deg: 0 a strafe's, 90 a
        /// slam's) (L4).</summary>
        public static float MedianScreenAngleDeg { get; private set; }

        /// <summary>The smallest rho any drawn streak pixel can have this frame (>= SpeedLinesClearIn by the clip; -1: none
        /// drawn) (L1).</summary>
        public static float MinDrawnRho { get; private set; } = -1;

        /// <summary>The most alpha a drawn pixel inside rho SpeedLinesClearFull can have this frame (L1: < 0.30).</summary>
        public static float MaxAlphaInsideFull { get; private set; }

        /// <summary>Drawn streaks crossing a live enemy's screen rect this frame (L5: 0 by construction).</summary>
        public static int EnemyCrossings { get; private set; }

        /// <summary>The intensity k this frame (0..1).</summary>
        public static float Intensity { get; private set; }

        // --- the pool ---------------------------------------------------------------------------------------------------

        struct Streak
        {
            public Vector3 head; // world-fixed (Unity space)
            public float age, life, peak;
            public bool burst, alive;
        }

        readonly SurvivalGame game;
        readonly Streak[] pool = new Streak[UnityTuning.SpeedLinesPool];
        int next;
        Mulberry32 rng;
        double k, budget, boostLeft;
        double kFade; // build 3 (V5): a faster copy of k the streaks' alpha follows
        int pendingBurst; // streaks owed by the bursts (born at the next frame, which has the camera)
        Vector3 lastDir = Vector3.forward;
        bool wasStill; // NIGHT20 F2: SKYGRIND TIME held the streaks last frame
        Vector3 stillV; // ... with the velocity at its onset

        // --- the draw: one dynamic mesh, at most SpeedLinesMaxDrawn quads -------------------------------------------------
        const int Max = UnityTuning.SpeedLinesMaxDrawn;
        readonly Vector3[] verts = new Vector3[Max * 4];
        readonly Color[] cols = new Color[Max * 4];
        readonly Vector2[] uvs = new Vector2[Max * 4];
        readonly int[] tris = new int[Max * 6];
        readonly Mesh mesh;
        readonly Material mat;
        RenderParams rp;
        readonly bool shaded; // the SKYGRIND/SpeedLines20 shader was found (else the neon additive fallback, one colour)
        bool anyAlive; // a streak was born since the last Kill
        readonly float[] lens = new float[Max], angs = new float[Max];
        readonly Rect[] enemyRects = new Rect[UnityTuning.SpeedLinesEnemyRects];
        int enemyCount;
        int raysLeft;
        // this frame's camera
        Vector3 eye, fwd, right, up;
        float tanV, tanH, pxW, pxH, near;

        static readonly int FlowColorId = Shader.PropertyToID("_FlowColor"), BurstColorId = Shader.PropertyToID("_BurstColor");

        public SpeedLines20(SurvivalGame game)
        {
            this.game = game;
            rng = new Mulberry32(game.runSeed ^ UnityTuning.SpeedLinesSeedSalt);
            Color cyan = UiTheme.Ready ? UiTheme.Cyan : Look.C(game.cfg, "colors.usable");
            Color flow = Color.Lerp(cyan, Color.white, UnityTuning.SpeedLinesFlowPale);
            Color burst = Color.Lerp(cyan, Color.white, UnityTuning.SpeedLinesBurstPale);
            flow.a = burst.a = 1;
            Shader sh = Shader.Find("SKYGRIND/SpeedLines20");
            shaded = sh != null;
            if (shaded)
            {
                mat = new Material(sh) { name = "AIR streaks" };
                mat.SetColor(FlowColorId, flow);
                mat.SetColor(BurstColorId, burst);
                mat.SetFloat("_ClearIn", (float)UnityTuning.SpeedLinesClearIn);
                mat.SetFloat("_ClearFull", (float)UnityTuning.SpeedLinesClearFull);
                mat.SetFloat("_CapAlpha", (float)UnityTuning.SpeedLinesCapAlpha);
                mat.SetFloat("_CapFull", (float)UnityTuning.SpeedLinesCapFull);
                mat.SetFloat("_EdgeSharp", (UnityTuning.SpeedLinesWidthPx * 0.5f + UnityTuning.SpeedLinesFeatherPx) / UnityTuning.SpeedLinesFeatherPx);
            }
            else
            {
                // not in the build: the neon material, additive, one colour (no clear mask per pixel: the CPU clip still holds)
                Debug.LogWarning("[speedlines20] shader SKYGRIND/SpeedLines20 is not in the build: the AIR streaks draw flat");
                mat = FxDraw.Additive(game.mats.neon, "AIR streaks (flat)");
                mat.SetFloat("_Cull", 0);
                mat.SetColor("_BaseColor", flow * (float)UnityTuning.SpeedLinesAlphaFlow);
            }
            rp = FxDraw.Params(mat);
            for (int i = 0; i < Max; i++)
            {
                int v = i * 4, t = i * 6;
                tris[t] = v; tris[t + 1] = v + 1; tris[t + 2] = v + 2;
                tris[t + 3] = v; tris[t + 4] = v + 2; tris[t + 5] = v + 3;
                uvs[v] = new Vector2(-1, 0); uvs[v + 1] = new Vector2(1, 0); uvs[v + 2] = new Vector2(1, 1); uvs[v + 3] = new Vector2(-1, 1);
            }
            mesh = new Mesh { name = "AIR streaks" };
            mesh.MarkDynamic();
            game.mover.Events += OnMover;
            game.OnNewRun += NewRun;
            game.events.On(N20Events.Shockwave, OnShockwave);
        }

        /// <summary>A new run: no streak left over, the dice reseeded from the run's seed.</summary>
        void NewRun()
        {
            rng = new Mulberry32(game.runSeed ^ UnityTuning.SpeedLinesSeedSalt);
            for (int i = 0; i < pool.Length; i++) pool[i].alive = false;
            k = kFade = budget = boostLeft = 0;
            pendingBurst = 0;
        }

        void OnMover(MoverEvent e)
        {
            if (!Live) return;
            switch (e.type)
            {
                case MoverEventType.Dash:
                {
                    // a boost's onset: a burst by its share (a partial boost bursts less, as CameraFeel's kick), the rate x2
                    Mover m = game.mover;
                    double share = SpeedLines20Math.BoostShare(m.stamina.rules.Any ? m.stamina.lastBoost : 1);
                    pendingBurst += (int)Math.Round(UnityTuning.SpeedLinesBoostBurst * share);
                    boostLeft = UnityTuning.SpeedLinesBoostRateTime;
                    break;
                }
                case MoverEventType.BoostJump:
                    pendingBurst += UnityTuning.SpeedLinesBoostJumpBurst;
                    break;
                case MoverEventType.Slam:
                    // build 3 (V4): the slam's impact clears the fall's air (its streaks would turn flat along the slide after it)
                    for (int i = 0; i < pool.Length; i++)
                        if (pool[i].alive) pool[i].life = Math.Min(pool[i].life, pool[i].age + (float)UnityTuning.SpeedLinesSlamCut);
                    break;
            }
        }

        void OnShockwave(object payload)
        {
            if (!Live) return;
            if (payload is ShockwaveEvent e && e.owner != "player") return;
            pendingBurst += UnityTuning.SpeedLinesShockwaveBurst; // the launch's punctuation (its own air is MovementFx's)
        }

        /// <summary>Frame: births, ages and the draw on the world's time (game.WorldDt), the probes.</summary>
        public void Render()
        {
            LiveCount = 0;
            MedianScreenLengthPx = MedianScreenAngleDeg = 0;
            MinDrawnRho = -1;
            MaxAlphaInsideFull = 0;
            EnemyCrossings = 0;
            Camera cam = game.cam;
            if (!Live || cam == null || !game.Active)
            {
                if (anyAlive) Kill(); // nothing carries over a pause, a screen or the A side
                Intensity = 0;
                if (Live) ReportStreaks(); // 0 streaks
                return;
            }
            double wdt = game.WorldDt;
            MovementConfig mc = game.cfg.movement;
            bool reduced = mc.settings.reducedMotion;
            double intensity = Math.Max(0, Math.Min(mc.feel.effectsIntensityMax, mc.settings.effectsIntensity));
            bool motion = FxGates.MotionOn(reduced, intensity);
            Vec3 wv = game.mover.velocity;
            Vector3 v = WebSpace.ToUnity(wv);
            // NIGHT20 F2: SKYGRIND TIME stills the air: no births, no ageing, each tail held as it was at the onset
            bool still = game.SkygrindTimeLook > 0;
            if (still)
            {
                if (!wasStill) stillV = v;
                v = stillV;
                wv = WebSpace.FromUnity(v);
                wdt = 0;
                pendingBurst = 0;
            }
            wasStill = still;
            double kNow = SpeedLines20Math.Intensity(SpeedLines20Math.VEff(wv.x, wv.y, wv.z));
            k = SpeedLines20Math.SmoothK(k, kNow, wdt);
            kFade = SpeedLines20Math.SmoothKWith(kFade, kNow, wdt, UnityTuning.SpeedLinesFadeHalfLife); // build 3 (V5)
            Intensity = (float)k;
            if (v.sqrMagnitude > 1e-4f) lastDir = v.normalized;
            double ads = SpeedLines20Math.AdsScale(game.viewmodel != null ? game.viewmodel.AdsAmount : 0);

            Transform ct = cam.transform;
            eye = ct.position;
            fwd = ct.forward;
            right = ct.right;
            up = ct.up;
            tanV = Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
            tanH = tanV * cam.aspect;
            pxW = cam.pixelWidth;
            pxH = cam.pixelHeight;
            near = cam.nearClipPlane;
            EnemyRects();

            // births
            if (!motion)
            {
                budget = boostLeft = 0;
                pendingBurst = 0;
            }
            else
            {
                raysLeft = UnityTuning.SpeedLinesRaysPerFrame;
                if (wdt > 0)
                {
                    boostLeft = Math.Max(0, boostLeft - wdt);
                    budget += SpeedLines20Math.SpawnRate(k, boostLeft > 0, true) * wdt;
                    budget = Math.Min(budget, UnityTuning.SpeedLinesBoostBurst); // a long frame (or a wait) never floods the pool
                }
                // a burst's births first (the boost's onset), then the flow's; a birth the frame's rays can't test waits
                pendingBurst = Math.Min(pendingBurst, UnityTuning.SpeedLinesPool / 2);
                while (pendingBurst > 0 && Spawn(true, v)) pendingBurst--;
                while (budget >= 1 && Spawn(false, v)) budget--;
            }

            // ages
            float fdt = (float)wdt;
            for (int i = 0; i < pool.Length; i++)
            {
                if (!pool[i].alive) continue;
                pool[i].age += fdt;
                if (pool[i].age >= pool[i].life) pool[i].alive = false;
            }

            Draw(v, ads, intensity, reduced);
        }

        /// <summary>One streak born on the ring (a flow's or a burst's): moved in front of the world its ray meets, redrawn when
        /// there's no room or an enemy is near; false when the frame's rays are spent (it waits for the next frame).</summary>
        bool Spawn(bool burst, Vector3 v)
        {
            double r0 = burst ? UnityTuning.SpeedLinesBurstRingIn : UnityTuning.SpeedLinesRingIn;
            double r1 = burst ? UnityTuning.SpeedLinesBurstRingOut : UnityTuning.SpeedLinesRingOut;
            double d0 = burst ? UnityTuning.SpeedLinesBurstDepthMin : UnityTuning.SpeedLinesDepthMin;
            double d1 = burst ? UnityTuning.SpeedLinesBurstDepthMax : UnityTuning.SpeedLinesDepthMax;
            double exposure = burst ? UnityTuning.SpeedLinesBurstExposure : SpeedLines20Math.Exposure(k);
            for (int attempt = 0; attempt < UnityTuning.SpeedLinesSpawnTries; attempt++)
            {
                if (game.world != null && raysLeft <= 0) return false; // the frame's rays are spent: this birth waits for the next
                double theta = rng.Next() * Math.PI * 2;
                double rho = SpeedLines20Math.RingRho(rng.Next(), r0, r1);
                double d = d0 + rng.Next() * (d1 - d0);
                float nx = (float)(rho * Math.Cos(theta)), ny = (float)(rho * Math.Sin(theta));
                Vector3 hv = SpeedLines20Math.BirthView(nx, ny, (float)d, tanH, tanV); // MikeFrom's ring, unprojected
                Vector3 head = eye + right * hv.x + up * hv.y + fwd * hv.z;
                if (game.world != null)
                {
                    raysLeft--;
                    Vec3 o = WebSpace.FromUnity(eye), to = WebSpace.FromUnity(head) - o;
                    double len = to.Length;
                    if (len > 1e-6 && game.world.RayIntersect(o, to / len, out RayHit hit) && hit.distance < len)
                    {
                        // build 2's reel (V1): the floor and the walls took half the births; the birth moves in front of what it
                        // hit, along the same ray (the floor rushing past at the screen's foot), or redraws when there's no room
                        if (!SpeedLines20Math.FreeAlong(hit.distance, rng.Next(), out double along)) continue;
                        head = eye + (head - eye) * (float)(along / len);
                    }
                }
                Vector3 tail = SpeedLines20Math.Tail(head, v, exposure, lastDir);
                if (!Project(head, out Vector2 hp)) continue;
                Vector2 tp = Project(tail, out Vector2 t2) ? t2 : hp;
                if (NearEnemy(hp, tp)) continue;
                ref Streak s = ref pool[next];
                next = (next + 1) % pool.Length; // the oldest reused
                s.head = head;
                s.age = 0;
                s.burst = burst;
                s.peak = (float)(burst ? UnityTuning.SpeedLinesAlphaBurst : UnityTuning.SpeedLinesAlphaFlow);
                s.life = (float)(burst ? UnityTuning.SpeedLinesBurstLife : SpeedLines20Math.Life(rng.Next()));
                s.alive = true;
                anyAlive = true;
                return true;
            }
            return true; // every try failed: this birth is spent (no room in view)
        }

        /// <summary>A world point to Unity screen px (origin bottom-left) by this frame's camera; false behind the eye.</summary>
        bool Project(Vector3 p, out Vector2 px)
        {
            Vector3 r = p - eye;
            return SpeedLines20Math.ProjectView(Vector3.Dot(r, right), Vector3.Dot(r, up), Vector3.Dot(r, fwd), tanH, tanV, pxW, pxH, near, out px);
        }

        /// <summary>Every live enemy's screen rect this frame (its body from the feet to its height, its radius wide), grown by
        /// SpeedLinesEnemyMarginPx at 1080p.</summary>
        void EnemyRects()
        {
            enemyCount = 0;
            EnemyRegistry reg = game.enemies;
            if (reg == null) return;
            float margin = UnityTuning.SpeedLinesEnemyMarginPx * pxH / UnityTuning.SpeedLinesRefHeight;
            float f = pxH * 0.5f / tanV; // px per m at 1 m
            double a = game.Alpha;
            var all = reg.all;
            for (int i = 0; i < all.Count && enemyCount < enemyRects.Length; i++)
            {
                Enemy e = all[i];
                if (!e.alive) continue;
                Vec3 p = e.previous + (e.position - e.previous) * a;
                Vector3 feet = WebSpace.ToUnity(p);
                Vector3 top = feet + Vector3.up * (float)e.Height;
                bool fa = Project(feet, out Vector2 pf), ta = Project(top, out Vector2 pt);
                if (!fa && !ta) continue;
                if (!fa) pf = pt;
                if (!ta) pt = pf;
                float z = Mathf.Max(near, Vector3.Dot((feet + top) * 0.5f - eye, fwd));
                float rad = (float)e.Radius * f / z + margin;
                enemyRects[enemyCount++] = Rect.MinMaxRect(Mathf.Min(pf.x, pt.x) - rad, Mathf.Min(pf.y, pt.y) - rad, Mathf.Max(pf.x, pt.x) + rad, Mathf.Max(pf.y, pt.y) + rad);
            }
        }

        bool NearEnemy(Vector2 a, Vector2 b)
        {
            for (int i = 0; i < enemyCount; i++)
                if (SpeedLines20Math.SegmentHitsRect(a.x, a.y, b.x, b.y, enemyRects[i])) return true;
            return false;
        }

        /// <summary>The newest SpeedLinesMaxDrawn live streaks as camera-facing tapered quads in one mesh, one draw; the probes.</summary>
        void Draw(Vector3 v, double ads, double intensity, bool reduced)
        {
            int n = 0;
            Vector3 min = Vector3.positiveInfinity, max = Vector3.negativeInfinity;
            float x0 = float.MaxValue, y0 = float.MaxValue, x1 = float.MinValue, y1 = float.MinValue;
            float maxAlpha = 0, minRho = float.MaxValue, maxInside = 0;
            double flowExposure = SpeedLines20Math.Exposure(k);
            double fadeShare = SpeedLines20Math.FadeShare(kFade); // build 3 (V5)
            float capPx = SpeedLines20Math.CapPx(k, pxH); // build 3 (V3)
            float mPerPx = 2 * tanV / UnityTuning.SpeedLinesRefHeight; // m per 1080p px at 1 m depth
            float halfPx = UnityTuning.SpeedLinesWidthPx * 0.5f + UnityTuning.SpeedLinesFeatherPx;
            double clearIn = UnityTuning.SpeedLinesClearIn, clearFull = UnityTuning.SpeedLinesClearFull;
            for (int j = 0; j < pool.Length && n < Max; j++)
            {
                int idx = ((next - 1 - j) % pool.Length + pool.Length) % pool.Length; // the newest first
                ref Streak s = ref pool[idx];
                if (!s.alive) continue;
                Vector3 head = s.head;
                Vector3 tail = SpeedLines20Math.Tail(head, v, s.burst ? UnityTuning.SpeedLinesBurstExposure : flowExposure, lastDir);
                float zh = Vector3.Dot(head - eye, fwd);
                if (zh <= near * 2) continue; // behind the eye
                float zt = Vector3.Dot(tail - eye, fwd);
                if (zt <= near * 2) tail = head + (tail - head) * ((zh - near * 2) / Mathf.Max(1e-4f, zh - zt)); // to the near plane
                // build 3 (V5): the alpha follows the speed (the fade's k): a stop fades every streak out within ~0.25 s; one under
                // the report floor is neither drawn nor counted (L2)
                double alpha = SpeedLines20Math.StreakAlpha(s.peak, s.age, s.life, (head - eye).magnitude, ads, intensity, reduced) * fadeShare;
                if (alpha < UnityTuning.SpeedLinesReportAlpha) continue;
                if (!Project(head, out Vector2 hp) || !Project(tail, out Vector2 tp)) continue;
                // build 3 (V3): the screen length capped by the speed, the tail pulled in along its own line (perspective-correct)
                float full = Vector2.Distance(hp, tp);
                if (full > capPx)
                {
                    float zt2 = Mathf.Max(near * 2, Vector3.Dot(tail - eye, fwd));
                    double u = capPx / full;
                    tail = head + (tail - head) * (float)SpeedLines20Math.WorldParam(u, zh, zt2);
                    tp = Vector2.Lerp(hp, tp, (float)u);
                }
                if (NearEnemy(hp, tp))
                {
                    s.alive = false; // an enemy came into it: dropped (L5)
                    continue;
                }
                // the clear ellipse (normalised coordinates): the part inside draws nothing (the shader's mask); the probe's clip
                double ax = 2 * hp.x / pxW - 1, ay = 2 * hp.y / pxH - 1, bx = 2 * tp.x / pxW - 1, by = 2 * tp.y / pxH - 1;
                bool cut = SpeedLines20Math.InsideSpan(ax, ay, bx, by, clearIn, out double c0, out double c1);
                if (cut && c0 <= 0 && c1 >= 1) continue; // wholly inside: nothing drawn
                float seg = Vector2.Distance(hp, tp);
                double onScreen = SpeedLines20Math.ShareInRect(hp.x, hp.y, tp.x, tp.y, new Rect(0, 0, pxW, pxH));
                float visible = seg * (float)Math.Max(0, onScreen - (cut ? c1 - c0 : 0));
                if (visible < UnityTuning.SpeedLinesReportPx) continue; // off the screen: no quad (the slot goes to one that shows)
                if (!shaded && cut)
                {
                    // the flat fallback has no mask per pixel: the quad itself stops at the clear ellipse (the outer piece)
                    float zt2 = Mathf.Max(near * 2, Vector3.Dot(tail - eye, fwd));
                    if (c0 > 0) tail = head + (tail - head) * (float)SpeedLines20Math.WorldParam(c0, zh, zt2);
                    else head = head + (tail - head) * (float)SpeedLines20Math.WorldParam(c1, zh, zt2);
                    zh = Vector3.Dot(head - eye, fwd);
                }
                // the quad: camera-facing (MikeFrom's tangent-flat basis), 1.5 px at the head, tapering to the tail
                Vector3 d = tail - head;
                Vector3 across = Vector3.Cross(d, eye - head);
                if (across.sqrMagnitude < 1e-12f) continue;
                across.Normalize();
                Vector3 wh = across * (halfPx * mPerPx * zh), wt = across * (halfPx * mPerPx * Mathf.Max(near * 2, Vector3.Dot(tail - eye, fwd)) * UnityTuning.SpeedLinesTipTaper);
                int b4 = n * 4;
                verts[b4] = head - wh;
                verts[b4 + 1] = head + wh;
                verts[b4 + 2] = tail + wt;
                verts[b4 + 3] = tail - wt;
                var col = new Color(s.burst ? 1 : 0, 0, 0, (float)alpha);
                cols[b4] = cols[b4 + 1] = cols[b4 + 2] = cols[b4 + 3] = col;
                for (int q = 0; q < 4; q++)
                {
                    min = Vector3.Min(min, verts[b4 + q]);
                    max = Vector3.Max(max, verts[b4 + q]);
                }
                // the probes: the visible pieces' screen rect, length and angle; the clear zone's numbers (L1)
                if (!cut || c0 > 0) Grow(ref x0, ref y0, ref x1, ref y1, hp, cut ? Vector2.Lerp(hp, tp, (float)c0) : tp);
                if (cut && c1 < 1) Grow(ref x0, ref y0, ref x1, ref y1, Vector2.Lerp(hp, tp, (float)c1), tp);
                lens[n] = visible; // px at the capture's size (the lead's channel: Screen.width x height)
                angs[n] = (float)SpeedLines20Math.AcuteAngleDeg(tp.x - hp.x, tp.y - hp.y);
                double near0 = cut ? clearIn : ClosestRho(ax, ay, bx, by);
                if (near0 < minRho) minRho = (float)near0;
                if (near0 < clearFull)
                {
                    double far = Math.Max(SpeedLines20Math.Rho(ax, ay), SpeedLines20Math.Rho(bx, by));
                    float inside = (float)SpeedLines20Math.PixelAlpha(alpha, Math.Min(clearFull - 1e-6, far));
                    if (inside > maxInside) maxInside = inside;
                }
                if (alpha > maxAlpha) maxAlpha = (float)alpha;
                n++;
            }
            LiveCount = n;
            if (n > 0)
            {
                MedianScreenLengthPx = SpeedLines20Math.Median(lens, n);
                MedianScreenAngleDeg = SpeedLines20Math.Median(angs, n);
                MinDrawnRho = minRho;
                MaxAlphaInsideFull = maxInside;
            }
            ReportStreaks();
            if (n == 0) return;
            mesh.Clear();
            mesh.SetVertices(verts, 0, n * 4);
            mesh.SetColors(cols, 0, n * 4);
            mesh.SetUVs(0, uvs, 0, n * 4);
            mesh.SetTriangles(tris, 0, n * 6, 0, false);
            var bounds = new Bounds((min + max) * 0.5f, max - min);
            mesh.bounds = bounds;
            rp.worldBounds = bounds;
            Graphics.RenderMesh(rp, mesh, 0, Matrix4x4.identity);
            if (FeelProbe.Recording)
            {
                if (x1 >= x0) FeelProbe.Effect(FeelFx.SpeedLines, Rect.MinMaxRect(Mathf.Max(0, x0), Mathf.Max(0, y0), Mathf.Min(pxW, x1), Mathf.Min(pxH, y1)), maxAlpha, n);
                float hw = (float)(clearIn * pxW * 0.5), hh = (float)(clearIn * pxH * 0.5);
                FeelProbe.Effect(FeelFx.SpeedLinesClear, new Rect(pxW * 0.5f - hw, pxH * 0.5f - hh, 2 * hw, 2 * hh), maxAlpha); // the clear ellipse's box
            }
        }

        /// <summary>
        /// The lead's M8 v2 channel (FeelProbe.Streaks), every frame the lines are live: the count drawn (0 at a walk), the
        /// median visible length in px at the capture's size, the median ACUTE angle to the horizontal (0 horizontal either
        /// way, 90 vertical either way), the nearest drawn rho (-1: none drawn), the strongest alpha inside rho 0.55 (-1: none
        /// drawn) and the streaks crossing a live enemy's rect (0 by construction: such a streak is dropped).
        /// </summary>
        static void ReportStreaks()
        {
            bool any = LiveCount > 0;
            FeelProbe.Streaks(LiveCount, MedianScreenLengthPx, MedianScreenAngleDeg, any ? MinDrawnRho : -1, any ? MaxAlphaInsideFull : -1, EnemyCrossings);
        }

        static void Grow(ref float x0, ref float y0, ref float x1, ref float y1, Vector2 a, Vector2 b)
        {
            x0 = Mathf.Min(x0, Mathf.Min(a.x, b.x));
            y0 = Mathf.Min(y0, Mathf.Min(a.y, b.y));
            x1 = Mathf.Max(x1, Mathf.Max(a.x, b.x));
            y1 = Mathf.Max(y1, Mathf.Max(a.y, b.y));
        }

        /// <summary>The smallest rho on segment a-b (normalised coordinates).</summary>
        static double ClosestRho(double ax, double ay, double bx, double by)
        {
            double dx = bx - ax, dy = by - ay, l2 = dx * dx + dy * dy;
            double t = l2 > 1e-18 ? Math.Max(0, Math.Min(1, -(ax * dx + ay * dy) / l2)) : 0;
            return SpeedLines20Math.Rho(ax + t * dx, ay + t * dy);
        }

        /// <summary>Every streak gone, nothing owed (Graphics.RenderMesh draws for one frame only: no draw, no quads).</summary>
        void Kill()
        {
            for (int i = 0; i < pool.Length; i++) pool[i].alive = false;
            k = kFade = budget = boostLeft = 0;
            pendingBurst = 0;
            anyAlive = false;
        }
    }
}
