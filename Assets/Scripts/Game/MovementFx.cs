// NIGHT12 U3-25.4: the movement FX, after skygrind/src/main.js :1522-1529 (the land puffs), combat/effects.js :151-160,
// :192-228, :286-300 (the slam dust rings, the wall-ride streaks) and player.js :55-73, :431 (the dash edge tint), plus a
// port-only touch (the wall-ride contact sparks: Research/2026-10-03-polish.md D1). Visual only: it reads the mover (its
// events and state) and never writes it; it animates on SurvivalGame.WorldDt (frozen in a hitstop and the pause), the dash
// tint on frame time as player.js does; its randomness is its own Mulberry32, seeded per run (never a sim stream).
// - land puff: a landing at fx.landPuffSpeed or faster bursts fx.landPuffCount cubes of colors.slamDust at the feet,
//   at fx.landPuffSpeed x landPuffSpread, life fx.landPuffLife, size x fx.landPuffSize; NIGHT16 (port-only, polish #4):
//   more cubes, a wider spread and a faint dust ring at the feet the harder the landing (UnityTuning.Land*; no ring under
//   a slam's own);
// - NIGHT16 (port-only, polish #5): dust from the heels while sliding faster than a walk, more the faster (UnityTuning.SlideDust*);
// - slam rings: each slam lays a flat ring (inner 1 - fx.slamRingWidth / slam.shockwaveRadius) at the landing,
//   fx.slamRingLift up, growing from fx.slamRingStart to slam.shockwaveRadius as 1 - (1 - t)^2 over fx.slamRingTime, in
//   colors.slamDust at fx.slamRingOpacity x (1 - t); fx.slamRingCount of them, the oldest reused;
// - wall-ride streaks: while riding, fx.streakRate a second, faint cyan lines (colors.usable x fx.streakBrightness)
//   fx.streakLength long just off the wall (capsuleRadius - fx.surfaceOffset), at a height in fx.streakHeightMin..Max,
//   fx.streakAheadMin..Max ahead, streaming past along the ride; they fade over fx.streakLife;
// - wall-ride sparks (port-only): UnityTuning.WallSparkRate a second, one fx.spark* cube (fx.sparkSpeed, sparkLife,
//   x fx.sparkSize) of colors.usable where a streak would start;
// - the dash edge tint: colors.enemy and colors.usable fringes on opposite edges (the sides at 0.55, the top and bottom at
//   0.25, inset feel.dashTintOffset px blurred feel.dashTintWidth px), at min(1, dashKick x feel.dashTintOpacity x effects
//   intensity), none with reduced motion; dashKick eases toward the dash as the camera's FOV kick does (camera.dashFovIn /
//   OutTime). A screen-space canvas under the HUD.
// The shared draws are here too: FxDraw (materials, RenderParams, the ring mesh), FxCubes (fx/particles.js's cubes with
// their `scale`, which the port's Particles doesn't draw) and FxLines (effects.js's additive lines).
// NIGHT19 U3-33.5 (builder V): the KESTREL's gust, its air and wake (MovementFx.Gust.cs, textured: FxSprites.cs).
using System;
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Skygrind.Game
{
    public sealed partial class MovementFx
    {
        readonly SurvivalGame game;
        readonly FxCubes cubes;
        readonly FxLines lines;
        Mulberry32 rng;
        readonly Color dust, usable, enemy;
        // land puffs
        readonly double puffSpeed, puffSpread, puffLife, puffSize;
        readonly int puffCount;
        // slam rings
        readonly Mesh ringMesh;
        readonly RenderParams[] ringParams;
        readonly MaterialPropertyBlock[] ringBlocks;
        readonly Vector3[] ringAt;
        readonly double[] ringAge;
        readonly bool[] ringLive;
        readonly double[] ringRadius, ringAlpha; // NIGHT16 polish #4: a ring grows to its own radius (a slam's: the shockwave's)
        int nextRing;
        readonly double ringTime, ringStart, shockRadius, ringOpacity, ringLift;
        bool landAfterSlam; // NIGHT16: the mover emits Slam then Land on a slam's landing: no landing ring under the slam's
        // streaks and sparks
        double streakBudget, sparkBudget;
        double slideDustBudget; // NIGHT16 polish #5: the slide's heel dust
        readonly double walkSpeed;
        readonly double streakRate, streakLife, streakLength, hMin, hMax, aheadMin, aheadMax, wallGap;
        readonly Color streakColor;
        readonly float sparkSpeed, sparkLife, sparkSize;
        // the dash tint
        readonly Canvas tintCanvas;
        readonly RawImage[] edges = new RawImage[4];
        readonly Color[] edgeColor = new Color[4];
        readonly double tintOpacity, dashFovInTime, dashFovOutTime;
        double dashKick;
        float tintShown = -1;
        static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

        public MovementFx(SurvivalGame game)
        {
            this.game = game;
            GameConfig cfg = game.cfg;
            MovementConfig mc = cfg.movement;
            cubes = new FxCubes(cfg, game.mats.neon, UnityTuning.FxCubePool);
            lines = new FxLines(game.mats.neon, cfg.Int("fx.lineCapacity"));
            rng = new Mulberry32(game.runSeed ^ UnityTuning.MovementFxSeedSalt);
            dust = Look.C(cfg, "colors.slamDust");
            usable = Look.C(cfg, "colors.usable");
            enemy = Look.C(cfg, "colors.enemy");
            puffSpeed = cfg.Num("fx.landPuffSpeed");
            puffSpread = cfg.Num("fx.landPuffSpread");
            puffCount = cfg.Int("fx.landPuffCount");
            puffLife = cfg.Num("fx.landPuffLife");
            puffSize = cfg.Num("fx.landPuffSize");

            ringTime = cfg.Num("fx.slamRingTime");
            ringStart = cfg.Num("fx.slamRingStart");
            shockRadius = cfg.Num("slam.shockwaveRadius");
            ringOpacity = cfg.Num("fx.slamRingOpacity");
            ringLift = cfg.Num("fx.slamRingLift");
            ringMesh = FxDraw.Ring((float)(1 - cfg.Num("fx.slamRingWidth") / shockRadius), 1, cfg.Int("fx.slamRingSegments"), "slam ring");
            int rings = cfg.Int("fx.slamRingCount");
            var ringMat = new Material(game.mats.acid) { name = "Slam ring" }; // alpha-blended _BaseColor (ImpactFx's ring material)
            ringParams = new RenderParams[rings];
            ringBlocks = new MaterialPropertyBlock[rings];
            ringAt = new Vector3[rings];
            ringAge = new double[rings];
            ringLive = new bool[rings];
            ringRadius = new double[rings];
            ringAlpha = new double[rings];
            walkSpeed = cfg.Num("ground.walkSpeed");
            for (int i = 0; i < rings; i++)
            {
                ringBlocks[i] = new MaterialPropertyBlock();
                ringParams[i] = FxDraw.Params(ringMat);
                ringParams[i].matProps = ringBlocks[i];
            }

            streakRate = cfg.Num("fx.streakRate");
            streakLife = cfg.Num("fx.streakLife");
            streakLength = cfg.Num("fx.streakLength");
            hMin = cfg.Num("fx.streakHeightMin");
            hMax = cfg.Num("fx.streakHeightMax");
            aheadMin = cfg.Num("fx.streakAheadMin");
            aheadMax = cfg.Num("fx.streakAheadMax");
            wallGap = mc.body.capsuleRadius - cfg.Num("fx.surfaceOffset"); // just off the wall surface
            streakColor = usable * (float)cfg.Num("fx.streakBrightness");
            sparkSpeed = (float)cfg.Num("fx.sparkSpeed");
            sparkLife = (float)cfg.Num("fx.sparkLife");
            sparkSize = (float)cfg.Num("fx.sparkSize");

            tintOpacity = cfg.Num("feel.dashTintOpacity");
            dashFovInTime = mc.camera.dashFovInTime;
            dashFovOutTime = mc.camera.dashFovOutTime;
            tintCanvas = BuildTint(cfg);

            game.mover.Events += OnMover;
            game.OnNewRun += NewRun;
            GustBuild(); // NIGHT19 U3-33.5 (builder V): the gust's air and wake (MovementFx.Gust.cs)
            Textured20Build(); // NIGHT20 row 6 (builder V): the textured dust and streaks, the climb's dust (MovementFx.Textured20.cs)
        }

        /// <summary>A new run: no FX left over, the cosmetic dice reseeded from the run's seed.</summary>
        void NewRun()
        {
            slideLineBudget = slideSparkBudget = 0; // NIGHT17 round 3
            rng = new Mulberry32(game.runSeed ^ UnityTuning.MovementFxSeedSalt);
            cubes.Clear();
            lines.Clear();
            for (int i = 0; i < ringLive.Length; i++) ringLive[i] = false;
            streakBudget = sparkBudget = 0;
            slideDustBudget = 0;
            landAfterSlam = false;
            dashKick = 0;
            GustClear(); // NIGHT19
            Textured20Clear(); // NIGHT20
        }

        void OnMover(MoverEvent e)
        {
            switch (e.type)
            {
                case MoverEventType.SlideStart:
                    // NIGHT17 round 4 (the reviewer: the cue from the slide's first frame): a first handful at once
                    slideLineBudget = Math.Max(slideLineBudget, UnityTuning.SlideBurstLines);
                    slideSparkBudget = Math.Max(slideSparkBudget, UnityTuning.SlideBurstSparks);
                    break;
                case MoverEventType.Land:
                {
                    bool slammed = landAfterSlam;
                    landAfterSlam = false;
                    // main.js :1522-1529: a hard landing puffs dust at the feet
                    if (e.fallSpeed < puffSpeed) return;
                    // NIGHT16 polish #4 (port-only): the harder the landing, the bigger the puff, and a faint dust ring at the
                    // feet (none under a slam's own ring).
                    double t = Math.Max(0, Math.Min(1, (e.fallSpeed - puffSpeed) / (UnityTuning.LandFullFallSpeed - puffSpeed)));
                    int count = puffCount + (int)Math.Round(puffCount * UnityTuning.LandPuffExtra * t);
                    Vec3 feet = game.mover.position;
                    cubes.Burst(WebSpace.ToUnity(feet), dust, count, (float)(puffSpeed * puffSpread * (1 + UnityTuning.LandPuffSpreadExtra * t)), (float)puffLife, (float)puffSize, rng);
                    if (!slammed) LayRing(feet, UnityTuning.LandRingRadiusMin + (UnityTuning.LandRingRadiusMax - UnityTuning.LandRingRadiusMin) * t, UnityTuning.LandRingAlpha);
                    LandPuff20(feet, slammed ? 1 : t); // NIGHT20 row 6: the soft dust (MovementFx.Textured20.cs; none on the A side)
                    break;
                }
                case MoverEventType.Slam:
                    // effects.js :192-199: the next ring at the slam's landing
                    LayRing(e.position, shockRadius, 1);
                    landAfterSlam = true;
                    break;
            }
        }

        /// <summary>The next ring (the oldest reused) at `at` (web space, the feet), growing to `radius` at `alpha` x the slam's opacity.</summary>
        void LayRing(Vec3 at, double radius, double alpha)
        {
            int i = nextRing;
            nextRing = (nextRing + 1) % ringLive.Length;
            ringAge[i] = 0;
            ringAt[i] = WebSpace.ToUnity(new Vec3(at.x, at.y + ringLift, at.z));
            ringRadius[i] = radius;
            ringAlpha[i] = alpha;
            ringLive[i] = true;
        }

        /// <summary>
        /// NIGHT16 polish #5 (port-only): dust from the heels while sliding on the ground faster than a walk, more the faster
        /// (UnityTuning.SlideDust*): one colors.slamDust cube at a time, just behind the feet along the slide.
        /// </summary>
        void SpawnSlideDust(double dt)
        {
            Mover mover = game.mover;
            double speed = Math.Sqrt(mover.velocity.x * mover.velocity.x + mover.velocity.z * mover.velocity.z);
            if (mover.state != MoveState.Slide || !mover.grounded || speed <= walkSpeed)
            {
                slideDustBudget = 0;
                return;
            }
            double k = Math.Min(1, (speed - walkSpeed) / (UnityTuning.SlideDustFullSpeed - walkSpeed));
            slideDustBudget += UnityTuning.SlideDustRate * k * dt;
            double dx = mover.velocity.x / speed, dz = mover.velocity.z / speed;
            // NIGHT20 row 6: the soft heel puffs (MovementFx.Textured20.cs; none on the A side)
            Vec3 pf = mover.position;
            SlidePuffs20(dt, k, WebSpace.ToUnity(new Vec3(pf.x - dx * UnityTuning.SlideDustBehind, pf.y, pf.z - dz * UnityTuning.SlideDustBehind)), WebSpace.ToUnity(new Vec3(-dx, 0, -dz)));
            while (slideDustBudget >= 1)
            {
                slideDustBudget--;
                Vec3 p = mover.position;
                var heel = new Vec3(p.x - dx * UnityTuning.SlideDustBehind, p.y + 0.05, p.z - dz * UnityTuning.SlideDustBehind);
                cubes.Burst(WebSpace.ToUnity(heel), dust, 1, UnityTuning.SlideDustSpeed, (float)puffLife, UnityTuning.SlideDustSize, rng);
            }
        }

        /// <summary>Frame: the world's FX on SurvivalGame.WorldDt, the dash tint on SurvivalGame.FrameDt.</summary>
        public void Render()
        {
            double dt = game.WorldDt;
            SpawnStreaks(dt);
            SpawnSprintLines(dt); // NIGHT17 U3-30.5 (builder X)
            SpawnSlideCue(dt); // NIGHT17 round 3 (builder X): the slide's own cue
            SpawnSlideDust(dt);
            cubes.Render((float)dt);
            // NIGHT18 U3-31.5: the reel hears the movement sparks and dust (the slide's sparks, the land puffs, the wall sparks)
            if (FeelProbe.Recording)
            {
                int live = cubes.LiveBounds(out Bounds sparks);
                if (live > 0) FeelProbe.EffectBounds(FeelFx.Sparks, sparks, 1f, live);
            }
            lines.Render((float)dt, game.cam);
            // effects.js :291-300: the slam rings grow to the shockwave radius and fade
            for (int i = 0; i < ringLive.Length; i++)
            {
                if (!ringLive[i]) continue;
                ringAge[i] += dt;
                double t = Math.Min(1, ringAge[i] / ringTime);
                if (t >= 1)
                {
                    ringLive[i] = false;
                    continue;
                }
                float radius = (float)(ringStart + (ringRadius[i] - ringStart) * (1 - (1 - t) * (1 - t)));
                Color c = dust;
                c.a = (float)(ringOpacity * ringAlpha[i] * (1 - t));
                ringBlocks[i].SetColor(BaseColor, c);
                Graphics.RenderMesh(ringParams[i], ringMesh, 0, Matrix4x4.TRS(ringAt[i], Quaternion.identity, new Vector3(radius, 1, radius)));
            }
            RenderTint(game.FrameDt);
            GustRender(dt); // NIGHT19 U3-33.5: the gust's air and wake (world time)
            Textured20Render(dt); // NIGHT20 row 6: the textured dust and streaks, the climb's dust (world time)
        }

        // NIGHT17 U3-30.5 (builder X, UnityTuning.Crunch.cs): the sprint's speed lines (port-only: the web has no sprint).
        // While StaminaHud says sprinting (not boosting), UnityTuning.SprintLineRate faint cyan lines a second (colors.usable x
        // SprintLineBrightness) appear in a ring around the view SprintLineAheadMin..Max m ahead, laid along the run, still in
        // the world, so you run past them; they fade over SprintLineLife. Weaker and steadier than the boost's edge tint and
        // 10 deg kick. None with REDUCED MOTION or at EFFECTS INTENSITY 0. World time (frozen in a hitstop).
        double sprintBudget;
        StaminaHud.Info stamina;

        void SpawnSprintLines(double dt)
        {
            StaminaHud.Read(game, ref stamina);
            SettingsConfig st = game.cfg.movement.settings;
            Mover mover = game.mover;
            double speed = Math.Sqrt(mover.velocity.x * mover.velocity.x + mover.velocity.z * mover.velocity.z);
            // NIGHT20 U3-34.7 (builder V): the edge-band lines retire (the AIR streaks subsume them); -skygrindOff20: NIGHT19's
            if (SpeedLines20.Live || !stamina.valid || !stamina.sprinting || stamina.boosting || !game.Active || speed < 1 || !FxGates.MotionOn(st.reducedMotion, st.effectsIntensity))
            {
                sprintBudget = 0;
                return;
            }
            Vector3 fwd = WebSpace.ToUnity(new Vec3(mover.velocity.x / speed, 0, mover.velocity.z / speed));
            Vector3 side = Vector3.Cross(Vector3.up, fwd);
            Vector3 eye = game.cam != null ? game.cam.transform.position : WebSpace.ToUnity(mover.position);
            Color c = Color.Lerp(usable, Color.white, UnityTuning.SprintLinePale) * UnityTuning.SprintLineBrightness; // round 5: pale cyan
            sprintBudget += UnityTuning.SprintLineRate * dt;
            while (sprintBudget >= 1)
            {
                sprintBudget--;
                double angle = rng.Next() * Math.PI * 2;
                float ahead = UnityTuning.SprintLineAheadMin + (float)rng.Next() * (UnityTuning.SprintLineAheadMax - UnityTuning.SprintLineAheadMin);
                Vector3 a;
                Camera view = game.cam;
                if (view != null)
                {
                    // NIGHT17 round 4 (the reviewer: the sprint read as nothing): only in the screen's outer band
                    // (SprintLineEdgeMin..Max of the way from the middle to the edge, on the view's ellipse), ahead along the view
                    float tanV = Mathf.Tan(view.fieldOfView * 0.5f * Mathf.Deg2Rad), tanH = tanV * view.aspect;
                    float r = UnityTuning.SprintLineEdgeMin + (float)rng.Next() * (UnityTuning.SprintLineEdgeMax - UnityTuning.SprintLineEdgeMin);
                    a = view.transform.TransformPoint(new Vector3((float)Math.Cos(angle) * tanH * r * ahead, (float)Math.Sin(angle) * tanV * r * ahead, ahead));
                }
                else a = eye + fwd * ahead + side * (float)Math.Cos(angle) * 2 + Vector3.up * (float)Math.Sin(angle) * 2;
                lines.Add(a, a - fwd * UnityTuning.SprintLineLength, c, UnityTuning.SprintLineLife, UnityTuning.SprintLineWidth);
            }
        }

        // NIGHT17 round 3 (builder X; the reviewer: the slide had no cue of its own: no lines, no FOV change, no particles in
        // view). While sliding on the ground faster than a walk (the slide dust's own test), x how far past a walk
        // (SlideDustFullSpeed): low streaks at the bottom of the view (SlideLineRate a second, faint cyan, SlideLineHeight..
        // above the floor, SlideLineAhead.. ahead, beside the path, laid along the slide, still in the world: you slide past
        // them) and sparks thrown up from the floor ahead, in view (SlideSparkRate a second, colors.usable, fx.spark*);
        // CameraFeel adds its widen. Distinct from the boost (the HUD's radial lines, the 10 deg kick) and the sprint (a ring
        // of shafts around the view). None with REDUCED MOTION or at EFFECTS INTENSITY 0. World time (frozen in a hitstop).
        double slideLineBudget, slideSparkBudget;

        void SpawnSlideCue(double dt)
        {
            Mover mover = game.mover;
            SettingsConfig st = game.cfg.movement.settings;
            double speed = Math.Sqrt(mover.velocity.x * mover.velocity.x + mover.velocity.z * mover.velocity.z);
            if (mover.state != MoveState.Slide || !mover.grounded || speed <= walkSpeed || !game.Active || !FxGates.MotionOn(st.reducedMotion, st.effectsIntensity))
            {
                slideLineBudget = slideSparkBudget = 0;
                return;
            }
            double k = Math.Max(UnityTuning.SlideCueFloor, Math.Min(1, (speed - walkSpeed) / (UnityTuning.SlideDustFullSpeed - walkSpeed)));
            Vector3 fwd = WebSpace.ToUnity(new Vec3(mover.velocity.x / speed, 0, mover.velocity.z / speed));
            Vector3 side = Vector3.Cross(Vector3.up, fwd);
            Vector3 feet = WebSpace.ToUnity(mover.position);
            Color lc = usable * UnityTuning.SlideLineBrightness;
            slideLineBudget += UnityTuning.SlideLineRate * k * dt;
            while (slideLineBudget >= 1)
            {
                slideLineBudget--;
                float s = (rng.Next() < 0.5 ? -1 : 1) * (UnityTuning.SlideLineSideMin + (float)rng.Next() * (UnityTuning.SlideLineSideMax - UnityTuning.SlideLineSideMin));
                float ahead = UnityTuning.SlideLineAheadMin + (float)rng.Next() * (UnityTuning.SlideLineAheadMax - UnityTuning.SlideLineAheadMin);
                float h = UnityTuning.SlideLineHeightMin + (float)rng.Next() * (UnityTuning.SlideLineHeightMax - UnityTuning.SlideLineHeightMin);
                Vector3 a = feet + fwd * ahead + side * s + Vector3.up * h;
                if (!Trail20Add(a, a - fwd * UnityTuning.SlideLineLength, lc, UnityTuning.SlideLineLife, UnityTuning.SlideLineWidth)) // NIGHT20: textured
                    lines.Add(a, a - fwd * UnityTuning.SlideLineLength, lc, UnityTuning.SlideLineLife, UnityTuning.SlideLineWidth);
            }
            slideSparkBudget += UnityTuning.SlideSparkRate * k * dt;
            while (slideSparkBudget >= 1)
            {
                slideSparkBudget--;
                float s = ((float)rng.Next() * 2 - 1) * UnityTuning.SlideSparkSide;
                Vector3 at = feet + fwd * (UnityTuning.SlideSparkAheadMin + (float)rng.Next() * (UnityTuning.SlideSparkAheadMax - UnityTuning.SlideSparkAheadMin)) + side * s + Vector3.up * 0.05f;
                cubes.Burst(at, usable, 1, UnityTuning.SlideSparkSpeed, UnityTuning.SlideSparkLife, UnityTuning.SlideSparkScale, rng); // round 5: bigger, longer
            }
        }

        /// <summary>effects.js spawnStreaks (:205-228), and the port-only contact sparks where a streak would start.</summary>
        void SpawnStreaks(double dt)
        {
            Mover mover = game.mover;
            if (mover.state != MoveState.WallRide)
            {
                streakBudget = sparkBudget = 0;
                return;
            }
            double speed = Math.Sqrt(mover.velocity.x * mover.velocity.x + mover.velocity.z * mover.velocity.z);
            if (speed == 0) return;
            double dx = mover.velocity.x / speed, dz = mover.velocity.z / speed;
            Vec3 n = mover.wallNormal, p = mover.position;
            streakBudget += streakRate * dt;
            while (streakBudget >= 1)
            {
                streakBudget--;
                double height = hMin + rng.Next() * (hMax - hMin);
                double back = -(aheadMin + rng.Next() * (aheadMax - aheadMin)); // + = behind
                var a = new Vec3(p.x - n.x * wallGap - dx * back, p.y + height, p.z - n.z * wallGap - dz * back);
                var b = new Vec3(a.x - dx * streakLength, a.y, a.z - dz * streakLength);
                if (!Trail20Add(WebSpace.ToUnity(a), WebSpace.ToUnity(b), streakColor, (float)streakLife, UnityTuning.StreakWidth)) // NIGHT20: textured
                    lines.Add(WebSpace.ToUnity(a), WebSpace.ToUnity(b), streakColor, (float)streakLife, UnityTuning.StreakWidth);
            }
            sparkBudget += UnityTuning.WallSparkRate * dt;
            while (sparkBudget >= 1)
            {
                sparkBudget--;
                double height = hMin + rng.Next() * (hMax - hMin);
                var at = new Vec3(p.x - n.x * wallGap, p.y + height, p.z - n.z * wallGap);
                cubes.Burst(WebSpace.ToUnity(at), usable, 1, sparkSpeed, sparkLife, sparkSize, rng);
            }
        }

        /// <summary>player.js createDashTint: four edge fringes on a canvas of their own, under the HUD.</summary>
        Canvas BuildTint(GameConfig cfg)
        {
            var go = new GameObject("Dash tint", typeof(Canvas), typeof(CanvasScaler));
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = UnityTuning.DashTintSortingOrder;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, (float)cfg.Num("ui.theme.scaleBaseHeight")); // as Ui.cs's canvas
            // CSS `inset <offset>px 0 <width>px`: the shadow's edge `offset` px in, blurred over `width` px. Baked as one
            // alpha ramp, a texel per px at 1080p: about 0.7 at the screen edge, 0 by offset + width / 2.
            double offset = cfg.Num("feel.dashTintOffset"), width = cfg.Num("feel.dashTintWidth");
            int band = Mathf.CeilToInt((float)(offset + width));
            Texture2D across = Ramp(band, offset, width, true), down = Ramp(band, offset, width, false);
            edgeColor[0] = enemy; // left (player.js: `inset ${o}px 0 ${w}px enemy 0.55`)
            edgeColor[1] = usable; // right (usable 0.55)
            edgeColor[2] = usable; // top (usable 0.25)
            edgeColor[3] = enemy; // bottom (enemy 0.25)
            edgeColor[0].a = edgeColor[1].a = UnityTuning.DashTintSideAlpha;
            edgeColor[2].a = edgeColor[3].a = UnityTuning.DashTintEdgeAlpha;
            edges[0] = Edge(go.transform, "left", across, new Vector2(0, 0), new Vector2(0, 1), new Vector2(0, 0.5f), new Vector2(band, 0), new Rect(0, 0, 1, 1));
            edges[1] = Edge(go.transform, "right", across, new Vector2(1, 0), new Vector2(1, 1), new Vector2(1, 0.5f), new Vector2(band, 0), new Rect(1, 0, -1, 1));
            edges[2] = Edge(go.transform, "top", down, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0, band), new Rect(0, 1, 1, -1));
            edges[3] = Edge(go.transform, "bottom", down, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0.5f, 0), new Vector2(0, band), new Rect(0, 0, 1, 1));
            canvas.enabled = false;
            return canvas;
        }

        static RawImage Edge(Transform parent, string name, Texture2D tex, Vector2 min, Vector2 max, Vector2 pivot, Vector2 size, Rect uv)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(RawImage));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = min;
            rt.anchorMax = max;
            rt.pivot = pivot;
            rt.sizeDelta = size;
            rt.anchoredPosition = Vector2.zero;
            var img = go.GetComponent<RawImage>();
            img.texture = tex;
            img.uvRect = uv;
            img.raycastTarget = false;
            return img;
        }

        /// <summary>The fringe's alpha ramp: texel 0 at the screen edge, 1 - smoothstep across the blurred shadow edge.</summary>
        static Texture2D Ramp(int band, double offset, double width, bool horizontal)
        {
            var tex = new Texture2D(horizontal ? band : 1, horizontal ? 1 : band, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear, name = "dash tint ramp" };
            for (int i = 0; i < band; i++)
            {
                double u = Math.Max(0, Math.Min(1, (i + 0.5 - (offset - width / 2)) / width));
                var c = new Color(1, 1, 1, (float)(1 - u * u * (3 - 2 * u)));
                if (horizontal) tex.SetPixel(i, 0, c);
                else tex.SetPixel(0, i, c);
            }
            tex.Apply(false, true);
            return tex;
        }

        /// <summary>player.js :224, :431: the dash kick eases in and out; the tint follows it (frame time).</summary>
        void RenderTint(double frameDt)
        {
            MovementConfig mc = game.cfg.movement;
            bool dashing = game.mover.state == MoveState.Dash && game.Active;
            double kickTime = dashing ? dashFovInTime : dashFovOutTime;
            double target = dashing ? 1 : 0;
            dashKick += Math.Sign(target - dashKick) * Math.Min(Math.Abs(target - dashKick), frameDt / kickTime);
            double intensity = Math.Max(0, Math.Min(mc.feel.effectsIntensityMax, mc.settings.effectsIntensity));
            // NIGHT20 U3-34.7 (builder V): the edge tint retires behind the AIR streaks (GG's "weird border"); -skygrindOff20: NIGHT19's
            double calm = mc.settings.reducedMotion || SpeedLines20.Live ? 0 : 1;
            float tint = (float)(Math.Min(1, dashKick * tintOpacity * intensity) * calm);
            tint = Mathf.Round(tint * 255) / 255; // set the colours only when the drawn opacity changes
            if (tint == tintShown && tintAb == FeelAB.Version) return;
            tintShown = tint;
            tintAb = FeelAB.Version;
            tintCanvas.enabled = tint > 0;
            if (tint <= 0) return;
            for (int i = 0; i < edges.Length; i++)
            {
                Color c = FeelAB.On ? DashTint18(i) : edgeColor[i];
                c.a *= tint;
                edges[i].color = c;
            }
        }

        int tintAb = -1;

        /// <summary>
        /// NIGHT18 (the lead's list: M11 flagged "Dash tint/left" and "/bottom" in DANGER on the player's own boost; CLAUDE.md rule
        /// 8: red-orange is enemies and danger only): the fringes on the cyan ladder, two-tone as the web's: CYAN-PALE where the
        /// web has colors.enemy (left, bottom), CYAN where it has colors.usable (right, top), at the web's alphas. FeelAB off:
        /// the web's.
        /// </summary>
        Color DashTint18(int i)
        {
            Color c = !UiTheme.Ready ? usable : i == 0 || i == 3 ? UiTheme.CyanPale : UiTheme.Cyan;
            c.a = edgeColor[i].a;
            return c;
        }
    }

    public static class FxDraw
    {
        /// <summary>Bounds that never cull: the FX move every frame and are few (no per-frame bounds upkeep).</summary>
        static readonly Bounds Everywhere = new Bounds(Vector3.zero, Vector3.one * 100000f);

        /// <summary>
        /// A copy of the neon (URP Unlit) material drawn additively: Blend One One, no depth write, the transparent queue
        /// (WeaponFx.Additive's recipe: three's AdditiveBlending; the opacity goes in the colour), instancing on.
        /// </summary>
        public static Material Additive(Material neon, string name)
        {
            var m = new Material(neon) { name = name, renderQueue = (int)RenderQueue.Transparent, enableInstancing = true };
            m.SetFloat("_SrcBlend", (float)BlendMode.One);
            m.SetFloat("_DstBlend", (float)BlendMode.One);
            m.SetFloat("_ZWrite", 0);
            return m;
        }

        /// <summary>An opaque unlit copy of the neon material in one colour (three's MeshBasicMaterial), instancing on.</summary>
        public static Material Opaque(Material neon, string name, Color c)
        {
            var m = new Material(neon) { name = name, enableInstancing = true };
            m.SetColor("_BaseColor", c);
            return m;
        }

        /// <summary>RenderParams for a material on `layer` (0: the world camera; Viewmodel.Layer: the overlay), no shadows.</summary>
        public static RenderParams Params(Material m, int layer = 0) => new RenderParams(m)
        {
            layer = layer,
            shadowCastingMode = ShadowCastingMode.Off,
            receiveShadows = false,
            worldBounds = Everywhere,
        };

        /// <summary>three's RingGeometry(inner, outer, segments) laid flat (y up), both faces.</summary>
        public static Mesh Ring(float inner, float outer, int segments, string name)
        {
            var v = new Vector3[segments * 2];
            var tri = new int[segments * 12];
            for (int i = 0; i < segments; i++)
            {
                float a = i * Mathf.PI * 2 / segments;
                float cs = Mathf.Cos(a), sn = Mathf.Sin(a);
                v[i * 2] = new Vector3(cs * inner, 0, sn * inner);
                v[i * 2 + 1] = new Vector3(cs * outer, 0, sn * outer);
                int j = (i + 1) % segments, k = i * 12;
                int a0 = i * 2, a1 = i * 2 + 1, b0 = j * 2, b1 = j * 2 + 1;
                tri[k] = a0; tri[k + 1] = b0; tri[k + 2] = a1;
                tri[k + 3] = a1; tri[k + 4] = b0; tri[k + 5] = b1;
                tri[k + 6] = a0; tri[k + 7] = a1; tri[k + 8] = b0;
                tri[k + 9] = a1; tri[k + 10] = b1; tri[k + 11] = b0;
            }
            var m = new Mesh { name = name, vertices = v, triangles = tri };
            m.RecalculateBounds();
            return m;
        }
    }

    /// <summary>
    /// fx/particles.js's cubes for the feel views that need its `scale` (a land puff's 1.4, an impact flare's 3.2, a spark's
    /// 0.4; the port's Particles has no scale): cubes of fx.size x scale flying out on a uniform sphere, falling at
    /// fx.gravity, shrinking as they die, speed and life jittered by fx.speedJitter / lifeJitter (a cosmetic Mulberry32 the
    /// caller owns). The oldest is reused. Drawn instanced, one RenderMeshInstanced per colour (URP Unlit has no
    /// per-instance colour); nothing allocated per frame once a colour has been seen. NIGHT17 round 1 (builder X): build 2's
    /// Player drew nothing through RenderMeshInstanced (no puff, spark or flare in any strip): each colour is now one
    /// CubeBatch (Chunks.cs: the cubes written into one dynamic mesh, one Graphics.RenderMesh), same material, same colours.
    /// </summary>
    public sealed class FxCubes
    {
        struct Part
        {
            public Vector3 pos, vel;
            public float life, maxLife, scale;
            public int bucket;
            public bool gravity, alive;
        }

        readonly Part[] parts;
        int next;
        readonly float size, gravity, speedJitter, lifeJitter;
        readonly Mesh cube;
        readonly Material neon;
        readonly int layer;
        readonly Color32[] keys = new Color32[UnityTuning.FxColourBuckets];
        readonly RenderParams[] rps = new RenderParams[UnityTuning.FxColourBuckets];
        readonly CubeBatch[] batches = new CubeBatch[UnityTuning.FxColourBuckets]; // NIGHT17 round 1: RenderMesh, no instancing
        int buckets;

        public FxCubes(GameConfig cfg, Material neon, int capacity, int layer = 0)
        {
            parts = new Part[Math.Min(capacity, 1023)];
            size = (float)cfg.Num("fx.size");
            gravity = (float)cfg.Num("fx.gravity");
            speedJitter = (float)cfg.Num("fx.speedJitter");
            lifeJitter = (float)cfg.Num("fx.lifeJitter");
            this.neon = neon;
            this.layer = layer;
            var kit = new MeshKit();
            kit.Cube(Vector3.zero, Vector3.one, Color.white);
            cube = kit.Build("fx cube");
        }

        /// <summary>NIGHT17 round 2: a colour's material and batch made now (at load), not on its first cube.</summary>
        public void Warm(Color c) => BucketOf(c);

        int BucketOf(Color c)
        {
            Color32 k = c;
            for (int i = 0; i < buckets; i++)
            {
                if (keys[i].r == k.r && keys[i].g == k.g && keys[i].b == k.b) return i;
            }
            if (buckets == keys.Length) return buckets - 1; // full: the last colour (never seen with the handful the FX use)
            keys[buckets] = k;
            Material mat = FxDraw.Opaque(neon, "Fx cubes", c);
            mat.SetFloat("_Cull", 0); // both faces (CubeBatch's winding can't hide one)
            rps[buckets] = FxDraw.Params(mat, layer);
            batches[buckets] = new CubeBatch(parts.Length, "Fx cubes");
            return buckets++;
        }

        /// <summary>particles.js burst(): `count` cubes at `at` (Unity space), `speed` m/s on a uniform sphere, `life` s.</summary>
        public void Burst(Vector3 at, Color c, int count, float speed, float life, float scale, Mulberry32 rng, bool falls = true)
        {
            if (count <= 0) return;
            int b = BucketOf(c);
            for (int i = 0; i < count; i++)
            {
                ref Part p = ref parts[next];
                next = (next + 1) % parts.Length; // oldest particles are recycled first
                p.alive = true;
                p.pos = at;
                // randomDir: uniform on the sphere
                float u = (float)(rng.Next() * 2 - 1);
                float phi = (float)(rng.Next() * Math.PI * 2);
                float s = Mathf.Sqrt(1 - u * u);
                p.vel = new Vector3(s * Mathf.Cos(phi), u, s * Mathf.Sin(phi)) * (speed * (1 - speedJitter * (float)rng.Next()));
                p.life = p.maxLife = life * (1 - lifeJitter * (float)rng.Next());
                p.gravity = falls;
                p.scale = scale;
                p.bucket = b;
            }
        }

        public void Clear()
        {
            for (int i = 0; i < parts.Length; i++) parts[i].alive = false;
        }

        /// <summary>NIGHT18: the live cubes' world bounds (their centres) and how many (the reel's probe; a loop over the pool).</summary>
        public int LiveBounds(out Bounds b)
        {
            b = default;
            int n = 0;
            for (int i = 0; i < parts.Length; i++)
            {
                if (!parts[i].alive) continue;
                if (n++ == 0) b = new Bounds(parts[i].pos, Vector3.zero);
                else b.Encapsulate(parts[i].pos);
            }
            return n;
        }

        /// <summary>particles.js update(): age, fall, move, shrink; then one instanced draw per colour.</summary>
        public void Render(float dt)
        {
            for (int b = 0; b < buckets; b++) batches[b].Begin();
            for (int i = 0; i < parts.Length; i++)
            {
                ref Part p = ref parts[i];
                if (!p.alive) continue;
                p.life -= dt;
                if (p.life <= 0)
                {
                    p.alive = false;
                    continue;
                }
                if (p.gravity) p.vel.y -= gravity * dt;
                p.pos += p.vel * dt;
                float k = p.life / p.maxLife * p.scale * size; // shrink as it dies
                batches[p.bucket].Add(p.pos, k);
            }
            for (int b = 0; b < buckets; b++) batches[b].Draw(rps[b]);
        }
    }

    /// <summary>
    /// effects.js's lines (:98-130 addLine, :277-288 the fade): additive, each fading as colour x life / maxLife over its
    /// life, the oldest reused (fx.lineCapacity). The web's are 1 px GL lines; here thin quads facing the camera, tapering
    /// from `width` at the start to UnityTuning.FxLineTaper x it at the end, drawn instanced in UnityTuning.FxLineLevels
    /// brightness steps per colour (a step is one additive material: URP Unlit has no per-instance colour).
    /// </summary>
    public sealed class FxLines
    {
        struct Line
        {
            public Vector3 a, b;
            public float life, maxLife, width;
            public int colour;
        }

        readonly Line[] lines;
        int nextLine;
        readonly Mesh quad;
        readonly Material neon;
        readonly int layer;
        const int Levels = UnityTuning.FxLineLevels;
        readonly Color32[] keys = new Color32[UnityTuning.FxColourBuckets];
        readonly RenderParams[] rps = new RenderParams[UnityTuning.FxColourBuckets * Levels];
        readonly CubeBatch[] batches = new CubeBatch[UnityTuning.FxColourBuckets * Levels]; // NIGHT17 round 1: RenderMesh, no instancing
        int colours;

        public FxLines(Material neon, int capacity, int layer = 0)
        {
            lines = new Line[Math.Min(capacity, 1023)];
            this.neon = neon;
            this.layer = layer;
            // A unit quad from y 0 (the start, full width) to y 1 (the end, FxLineTaper x the width), both faces.
            float tip = UnityTuning.FxLineTaper * 0.5f;
            var kit = new MeshKit();
            Vector3 a = new Vector3(-0.5f, 0, 0), b = new Vector3(0.5f, 0, 0), c = new Vector3(tip, 1, 0), d = new Vector3(-tip, 1, 0);
            kit.Quad(a, b, c, d, Vector3.back, Color.white);
            kit.Quad(d, c, b, a, Vector3.forward, Color.white);
            quad = kit.Build("fx line");
        }

        /// <summary>NIGHT17 round 2: a colour's materials and batches made now (at load), not on its first line.</summary>
        public void Warm(Color c) => ColourOf(c);

        /// <summary>NIGHT17 round 2: one tiny line of `c` at `at` for the pipeline state (sub-pixel, gone with the next run).</summary>
        public void WarmDraw(Vector3 at, Color c) => Add(at, at + Vector3.up * UnityTuning.CrunchWarmScale, c, UnityTuning.CrunchWarmLineLife, UnityTuning.CrunchWarmScale);

        int ColourOf(Color c)
        {
            Color32 k = new Color(Mathf.Clamp01(c.r / 4), Mathf.Clamp01(c.g / 4), Mathf.Clamp01(c.b / 4)); // HDR colours keyed / 4
            for (int i = 0; i < colours; i++)
            {
                if (keys[i].r == k.r && keys[i].g == k.g && keys[i].b == k.b) return i;
            }
            if (colours == keys.Length) return colours - 1;
            keys[colours] = k;
            for (int l = 0; l < Levels; l++)
            {
                Material m = FxDraw.Additive(neon, "Fx lines");
                m.SetColor("_BaseColor", c * ((l + 1) / (float)Levels));
                rps[colours * Levels + l] = FxDraw.Params(m, layer);
                batches[colours * Levels + l] = new CubeBatch(lines.Length, "Fx lines", quad); // the line's quad, both faces
            }
            return colours++;
        }

        /// <summary>addLine(from, to, colour x brightness, life): Unity space; `width` m at `from`.</summary>
        public void Add(Vector3 from, Vector3 to, Color c, float life, float width)
        {
            ref Line line = ref lines[nextLine];
            nextLine = (nextLine + 1) % lines.Length; // oldest reused
            line.a = from;
            line.b = to;
            line.life = line.maxLife = life;
            line.width = width;
            line.colour = ColourOf(c);
        }

        public void Clear()
        {
            for (int i = 0; i < lines.Length; i++) lines[i].life = 0;
        }

        /// <summary>The lines fade (additive: a dark line is invisible); each faces `cam`.</summary>
        public void Render(float dt, Camera cam)
        {
            for (int i = 0; i < colours * Levels; i++) batches[i].Begin();
            Vector3 eye = cam.transform.position;
            for (int i = 0; i < lines.Length; i++)
            {
                ref Line line = ref lines[i];
                if (line.life <= 0) continue;
                line.life = Math.Max(0, line.life - dt);
                float k = line.life / line.maxLife;
                if (k <= 0) continue;
                Vector3 d = line.b - line.a;
                if (d.sqrMagnitude < 1e-8f) continue;
                Vector3 across = Vector3.Cross(d, eye - line.a);
                if (across.sqrMagnitude < 1e-12f) continue;
                across = across.normalized * line.width;
                Vector3 normal = Vector3.Cross(across, d).normalized;
                var m = new Matrix4x4(
                    new Vector4(across.x, across.y, across.z, 0),
                    new Vector4(d.x, d.y, d.z, 0),
                    new Vector4(normal.x, normal.y, normal.z, 0),
                    new Vector4(line.a.x, line.a.y, line.a.z, 1));
                int level = Math.Min(Levels, Math.Max(1, Mathf.CeilToInt(k * Levels)));
                int bucket = line.colour * Levels + level - 1;
                batches[bucket].Add(m);
            }
            for (int b = 0; b < colours * Levels; b++) batches[b].Draw(rps[b]);
        }
    }
}
