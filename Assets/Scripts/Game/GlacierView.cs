// NIGHT26 builder B (contract glacier) + NIGHT27 builder B (boss2): THE GLACIER's course and boss as the view draws them. The sim keeps a sliding window of 40 m tiles as a world layer
// (SurvivalSim.Glacier26.cs / Glacier27.cs); this draws the same window: ONE tile mesh (the course tile's boxes: stone walls and ceiling, neutral rails, the floor) instanced on a pool of tile
// objects that RECYCLE (the tiles behind the player move ahead), an ice sheet over each tile's floor (the pale violet-white boundary tone, additive) and its boost strips (cyan:
// player-usable); one point light that follows the player; the lane attacks (red-orange: danger) that ramp up through their tell and burn bright while lethal.
// NIGHT26's boss body (row off, design.n26): the hull and the engine block (the sim's oriented boxes as they are).
// NIGHT27's boss body (design.n27.boss2 on): THE ORIGINAL skater (GG: "just switch back to the original [model] because it doesn't look good, and keep the effects"; DECIDE27 D-3): a large blocky
// skater in the web's blocky enemy style on ONE blade (Design/bosses/glacier27.json, boxes in its own frame), with the effects kept: the pale violet-white AURA, two thin additive BLADE TRAILS, the
// red-orange visor, and a cyan PULSE-READY shell while the hit window is open. The lane attacks of the NIGHT27 chase (low bar, sweep, roller, icicle fall) draw from the same pools.
// THE CURVE (view only, DECIDE27 D-8; design.n27.boss2.curve.viewOn, default off): when on, the tiles, the boss and the attacks are placed along Glacier27Curve's bend; the camera follows it too (BendCamera, called from
// SurvivalGame.Render after CameraFeel.Apply: the camera yaw = the player's yaw + the course heading, so the mouse stays relative to the curve; the sim's yaw and position stay straight).
// Rule 8: cyan player-usable, red-orange danger, pale violet-white boundary. Numbers: UnityTuning.Glacier26.cs, UnityTuning.Boss27.cs. Not compiled in the Editor by its author (dn-check only).
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace Skygrind.Game
{
    public sealed class GlacierView
    {
        readonly SurvivalGame game;
        readonly Transform root;
        readonly GameObject[] tiles = new GameObject[UnityTuning.GlacierTileMax];
        readonly Transform[] iceT = new Transform[UnityTuning.GlacierTileMax];
        readonly Transform[] stripT = new Transform[UnityTuning.GlacierStripMax];
        readonly Renderer[] iceR = new Renderer[UnityTuning.GlacierTileMax], stripR = new Renderer[UnityTuning.GlacierStripMax];
        readonly Transform[] hullT = new Transform[2], barT = new Transform[UnityTuning.GlacierBarMax], spikeT = new Transform[UnityTuning.GlacierSpikeMax];
        readonly Renderer[] hullR = new Renderer[2], barR = new Renderer[UnityTuning.GlacierBarMax], spikeR = new Renderer[UnityTuning.GlacierSpikeMax];
        // NIGHT27: the skater, its aura, two trails, the ready shell, the rollers, the icicles
        readonly Transform[] skT = new Transform[UnityTuning.SkaterPartMax];
        readonly Renderer[] skR = new Renderer[UnityTuning.SkaterPartMax];
        readonly Transform[] trailT = new Transform[2], rollT = new Transform[UnityTuning.GlacierRollerMax], iceclT = new Transform[UnityTuning.GlacierIcicleMax];
        readonly Renderer[] trailR = new Renderer[2], rollR = new Renderer[UnityTuning.GlacierRollerMax], iceclR = new Renderer[UnityTuning.GlacierIcicleMax];
        Transform auraT, readyT;
        Renderer auraR, readyR;
        readonly List<(string part, double a, double b, double c, double hx, double hy, double hz)> model = new List<(string, double, double, double, double, double, double)>();
        readonly Light light;
        readonly Mesh tileMesh;
        readonly MaterialPropertyBlock block = new MaterialPropertyBlock();
        readonly Color hull, engine, ice, boost, attack, white, skBody, skVisor, skBlade, skAura, skReady;
        static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        bool telegraphWired;
        GlacierCourse course;

        public bool Shown => root != null && root.gameObject.activeSelf;

        /// <summary>The course bends in the view now (design.n27.boss2.curve.viewOn, with the NIGHT27 chase running): the camera hook reads it.</summary>
        public bool CurveActive { get; private set; }

        public GlacierView(SurvivalGame game)
        {
            this.game = game;
            GameConfig cfg = game.cfg;
            hull = Look.Hex(UnityTuning.GlacierHullHex);
            engine = Look.Hex(UnityTuning.GlacierEngineHex);
            ice = Look.C(cfg, "barrier.boundaryColor", UnityTuning.GlacierIceGain);
            boost = Look.Hex(UnityTuning.GlacierBoostHex, UnityTuning.GlacierBoostGain);
            attack = Look.Hex(UnityTuning.GlacierAttackHex, UnityTuning.GlacierAttackGain);
            white = Look.Hex(0xffffff);
            skBody = Look.Hex(UnityTuning.SkaterBodyHex);
            skVisor = Look.Hex(UnityTuning.SkaterVisorHex, UnityTuning.SkaterVisorGain);
            skBlade = Look.Hex(UnityTuning.SkaterBladeHex, UnityTuning.SkaterBladeGain);
            skAura = Look.Hex(UnityTuning.SkaterAuraHex, UnityTuning.SkaterAuraGain);
            skReady = Look.Hex(UnityTuning.SkaterReadyHex, UnityTuning.SkaterReadyGain);
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Mesh box = cube.GetComponent<MeshFilter>().sharedMesh;
            Object.Destroy(cube);
            Material solid = FxDraw.Opaque(game.mats.neon, "Glacier body", Color.white);
            Material add = FxDraw.Additive(game.mats.neon, "Glacier glow");
            root = new GameObject("THE GLACIER").transform;
            course = LoadCourse(cfg);
            // one tile's mesh: stone for the walls and ceiling, neutral rails, a dark floor under the ice sheet
            Color top = Look.C(cfg, "colors.pillarTop"), side = Look.C(cfg, "colors.pillarSide"), rail = Look.Hex(UnityTuning.GlacierRailHex), floor = Look.C(cfg, "colors.pillarSide", 0.6f);
            var kit = new MeshKit();
            if (course != null)
                foreach (var (min, max, kind) in course.tile)
                    kit.Box(min, max, kind == "rail" ? rail : kind == "floor" ? floor : top, kind == "rail" ? rail : side);
            tileMesh = kit.Build("glacier tile");
            for (int i = 0; i < tiles.Length; i++)
            {
                tiles[i] = MeshKit.Object("tile " + i, root, tileMesh, game.mats.stone, shadows: false);
                iceR[i] = Part(root, "ice " + i, box, add, out iceT[i]);
            }
            for (int i = 0; i < stripT.Length; i++) stripR[i] = Part(root, "strip " + i, box, add, out stripT[i]);
            for (int i = 0; i < 2; i++) hullR[i] = Part(root, i == 0 ? "hull" : "engine", box, solid, out hullT[i]);
            for (int i = 0; i < barT.Length; i++) barR[i] = Part(root, "bar " + i, box, add, out barT[i]);
            for (int i = 0; i < spikeT.Length; i++) spikeR[i] = Part(root, "spike " + i, box, add, out spikeT[i]);
            LoadModel(cfg);
            for (int i = 0; i < skT.Length; i++) skR[i] = Part(root, "skater " + i, box, solid, out skT[i]);
            for (int i = 0; i < trailT.Length; i++) trailR[i] = Part(root, "blade trail " + i, box, add, out trailT[i]);
            for (int i = 0; i < rollT.Length; i++) rollR[i] = Part(root, "roller " + i, box, add, out rollT[i]);
            for (int i = 0; i < iceclT.Length; i++) iceclR[i] = Part(root, "icicle " + i, box, add, out iceclT[i]);
            auraR = Part(root, "aura", box, add, out auraT);
            readyR = Part(root, "pulse ready", box, add, out readyT);
            var lg = new GameObject("Course light");
            lg.transform.SetParent(root, false);
            light = lg.AddComponent<Light>();
            light.type = LightType.Point;
            light.shadows = LightShadows.None;
            light.range = (float)cfg.Num("design.n26.bosses.glacier.room.lightRange");
            light.intensity = (float)cfg.Num("design.n26.bosses.glacier.room.lightIntensity") * UnityTuning.LightScale;
            light.color = Look.Hex((uint)cfg.Num("design.n26.bosses.glacier.room.lightColor"));
            root.gameObject.SetActive(false);
            game.events.On<BossEvent>("bossDying", e => { if (e.boss == "glacier") game.BossDyingSeen(); });
            game.events.On<BossEvent>("bossDown", e =>
            {
                if (e.boss == "glacier") game.ParticleBurst(e.position, white, UnityTuning.GlacierDeathBits, UnityTuning.GlacierDeathSpeed, UnityTuning.GlacierDeathLife, UnityTuning.GlacierDeathSize);
            });
        }

        static GlacierCourse LoadCourse(GameConfig cfg)
        {
            string dir = SurvivalSim.Wyrm24Dir;
            if (string.IsNullOrEmpty(dir) || !cfg.Has("design.n26.bosses.glacier.course.paths")) return null;
            string path = System.IO.Path.Combine(dir, cfg.Str("design.n26.bosses.glacier.course.paths"));
            return System.IO.File.Exists(path) ? GlacierCourse.Load(path) : null;
        }

        void LoadModel(GameConfig cfg)
        {
            string dir = SurvivalSim.Wyrm24Dir;
            if (string.IsNullOrEmpty(dir) || !cfg.Has("design.n27.boss2.body.model")) return;
            string path = System.IO.Path.Combine(dir, cfg.Str("design.n27.boss2.body.model"));
            if (!System.IO.File.Exists(path)) return;
            foreach (object o in PortData.Arr(PortData.Obj(PortData.LoadJson(path))["parts"]))
            {
                var p = PortData.Obj(o);
                var h = PortData.Arr(p["half"]);
                if (model.Count < skT.Length) model.Add(((string)p["part"], PortData.Num(p["a"]), PortData.Num(p["b"]), PortData.Num(p["c"]), PortData.Num(h[0]), PortData.Num(h[1]), PortData.Num(h[2])));
            }
        }

        static Renderer Part(Transform parent, string name, Mesh mesh, Material mat, out Transform t)
        {
            var g = new GameObject(name);
            g.transform.SetParent(parent, false);
            g.AddComponent<MeshFilter>().sharedMesh = mesh;
            var r = g.AddComponent<MeshRenderer>();
            r.sharedMaterial = mat;
            r.shadowCastingMode = ShadowCastingMode.Off;
            t = g.transform;
            return r;
        }

        void Tint(Renderer r, Color c)
        {
            block.SetColor(BaseColor, c);
            r.SetPropertyBlock(block);
        }

        static Vector3 U(Vec3 v) => new Vector3((float)v.x, (float)v.y, (float)-v.z); // web z is Unity -z

        // --- the curve (view only) ---------------------------------------------------------------------------------------------------------

        Glacier27Curve curve;
        Vec3 curveOrigin;

        /// <summary>The heading (rad; 0 looks down the course, positive turns left) of the bent course at a course distance s: the camera hook's yaw offset (0 when the bend is off).</summary>
        public float CurveYaw(double s)
        {
            if (!CurveActive || curve == null) return 0;
            curve.Pose(s, out _, out _, out double h);
            return (float)h;
        }

        /// <summary>A sim point (the straight course) as a Unity point on the bent course (the same point when the bend is off).</summary>
        public Vector3 CurvedPoint(Vec3 sim)
        {
            if (!CurveActive || curve == null) return U(sim);
            curve.Bend(curveOrigin.z - sim.z, sim.x - curveOrigin.x, out double bx, out double bz, out _);
            return U(new Vec3(curveOrigin.x + bx, sim.y, curveOrigin.z + bz));
        }

        /// <summary>Unity rotation about Y of heading h: a sim yaw h is a Unity Y angle of -h (the sim's z is Unity's -z; forward (-sin h, -cos h) becomes (-sin h, +cos h) = Quaternion.Euler(0, -h)).</summary>
        static Quaternion YawOf(double h) => Quaternion.Euler(0, -(float)(h * Mathf.Rad2Deg), 0);

        Quaternion CurvedRot(double s)
        {
            if (!CurveActive || curve == null) return Quaternion.identity;
            curve.Pose(s, out _, out _, out double h);
            return YawOf(h);
        }

        /// <summary>Decide this frame whether the bend is on (the NIGHT27 chase running, design.n27.boss2.curve.viewOn): the camera and the render both call it.</summary>
        public bool RefreshCurve(SurvivalSim sim)
        {
            Glacier27 g27 = sim?.glacier27;
            bool on = g27 != null && g27.Alive && sim.n27.boss2 && g27.curve != null && sim.rink28 == null && game.cfg.Design("n27.boss2.curve.viewOn"); // NIGHT28: the real rink replaces the view-only bend
            CurveActive = on;
            if (on) { curve = g27.curve; curveOrigin = sim.glacier27Origin; }
            return on;
        }

        /// <summary>The camera hook (SurvivalGame.Render, after CameraFeel.Apply): moves the camera onto the bent course and turns it by the course heading, so the player's yaw stays RELATIVE
        /// to the curve (the sim's yaw and position stay straight; only the render pose changes). `drawn`: the sim position the camera was drawn at.</summary>
        public void BendCamera(Camera cam, SurvivalSim sim)
        {
            if (!RefreshCurve(sim)) return;
            Vector3 u = cam.transform.position; // Unity of a straight sim point
            Vec3 simPos = new Vec3(u.x, u.y, -u.z);
            curve.Pose(curveOrigin.z - simPos.z, out _, out _, out double h);
            cam.transform.position = CurvedPoint(simPos);
            cam.transform.rotation = YawOf(h) * cam.transform.rotation;
        }

        void Place(Transform t, Vec3 centre, double sx, double sy, double sz)
        {
            t.position = CurvedPoint(centre);
            t.rotation = CurvedRot(curveOrigin.z - centre.z);
            t.localScale = new Vector3((float)sx, (float)sy, (float)sz);
        }

        public void Render(double alpha)
        {
            SurvivalSim sim = game.sim;
            Glacier27 g27 = sim?.glacier27;
            bool is27 = g27 != null && g27.Alive && sim.n27.boss2;
            Glacier g = is27 ? null : sim?.glacier26;
            if (!telegraphWired && sim?.bossKit != null && (object)game.Telegraphs is ITelegraph tg)
            {
                sim.bossKit.telegraph = tg;
                telegraphWired = true;
            }
            bool on = (is27 || (g != null && g.Alive)) && course != null;
            if (root.gameObject.activeSelf != on) root.gameObject.SetActive(on);
            if (!on) return;
            float a = Mathf.Clamp01((float)alpha);
            double tileLen = is27 ? g27.p.tileLen : g.p.tileLen, laneHalf = is27 ? g27.p.laneHalf : g.p.laneHalf;
            Vec3 o = is27 ? sim.glacier27Origin : sim.glacier26Origin;
            int first = is27 ? sim.Glacier27First : sim.Glacier26First, last = is27 ? sim.Glacier27Last : sim.Glacier26Last, strip = 0;
            RefreshCurve(sim);
            // the floor decals (TelegraphView) are placed in straight sim space and would sit off the bent road: with the bend on the chase's tells are the attacks' own ramping bars
            if (CurveActive && sim.bossKit != null && !(sim.bossKit.telegraph is NullTelegraph)) sim.bossKit.telegraph = new NullTelegraph();
            for (int i = 0; i < tiles.Length; i++)
            {
                int k = first + i;
                bool live = k >= 0 && k <= last && i < tiles.Length && k >= System.Math.Max(0, first);
                if (tiles[i].activeSelf != live) { tiles[i].SetActive(live); iceT[i].gameObject.SetActive(live); }
                if (!live) continue;
                double dz = -tileLen * k;
                if (CurveActive)
                {
                    curve.Pose(tileLen * k, out double cx, out double cz, out double h);
                    tiles[i].transform.SetPositionAndRotation(U(new Vec3(o.x + cx, o.y, o.z + cz)), YawOf(h));
                }
                else tiles[i].transform.SetPositionAndRotation(new Vector3((float)o.x, (float)o.y, -(float)(o.z + dz)), Quaternion.identity); // the mesh holds web coordinates: z flips with the object
                // the ice sheet over the tile's floor
                Place(iceT[i], new Vec3(o.x, o.y + UnityTuning.GlacierFloorLift, o.z + dz - tileLen * 0.5), laneHalf * 2, UnityTuning.GlacierStripThick, tileLen);
                Tint(iceR[i], ice);
                foreach (double[] b in course.boosts)
                {
                    if (strip >= stripT.Length) break;
                    if (!stripT[strip].gameObject.activeSelf) stripT[strip].gameObject.SetActive(true);
                    Place(stripT[strip], new Vec3(o.x + (b[0] + b[2]) * 0.5, o.y + UnityTuning.GlacierFloorLift * 2, o.z + dz + (b[1] + b[3]) * 0.5), b[2] - b[0], UnityTuning.GlacierStripThick, b[1] - b[3]);
                    Tint(stripR[strip], boost);
                    strip++;
                }
            }
            for (int i = strip; i < stripT.Length; i++) if (stripT[i].gameObject.activeSelf) stripT[i].gameObject.SetActive(false);
            // the light follows the player along the straight
            Vec3 me = sim.mover.position;
            light.transform.position = CurvedPoint(new Vec3(me.x, me.y + UnityTuning.GlacierLightUp, me.z));
            if (is27) RenderChase(g27, sim, o, a);
            else RenderChase26(g, o, a, sim);
            // hide the other body's parts
            bool show26 = !is27;
            for (int i = 0; i < 2; i++) if (hullT[i].gameObject.activeSelf != show26) hullT[i].gameObject.SetActive(show26);
            if (!is27) HideSkater();
        }

        void HideSkater()
        {
            foreach (Transform t in skT) if (t.gameObject.activeSelf) t.gameObject.SetActive(false);
            foreach (Transform t in trailT) if (t.gameObject.activeSelf) t.gameObject.SetActive(false);
            foreach (Transform t in rollT) if (t.gameObject.activeSelf) t.gameObject.SetActive(false);
            foreach (Transform t in iceclT) if (t.gameObject.activeSelf) t.gameObject.SetActive(false);
            if (auraT.gameObject.activeSelf) auraT.gameObject.SetActive(false);
            if (readyT.gameObject.activeSelf) readyT.gameObject.SetActive(false);
        }

        /// <summary>NIGHT26's hull and engine block, and its lane attacks (the row off).</summary>
        void RenderChase26(Glacier g, Vec3 o, float a, SurvivalSim sim)
        {
            GlacierParams p = g.p;
            for (int i = 0; i < 2; i++)
            {
                WyrmBox b = g.boxes[i];
                Vector3 c = Vector3.Lerp(U(g.prevCenters[i]), U(b.c), a);
                hullT[i].SetPositionAndRotation(c, Quaternion.LookRotation(U(b.f), U(b.u)));
                hullT[i].localScale = new Vector3((float)b.hr * 2, (float)b.hu * 2, (float)b.hf * 2);
                Tint(hullR[i], i == 0 ? hull : engine);
            }
            int bar = 0, spike = 0;
            foreach (GlacierObstacle ob in g.obstacles)
            {
                float k = ob.tellLeft > 0 ? UnityTuning.GlacierTellMin + UnityTuning.GlacierTellRamp * (1f - (float)ob.tellLeft / Mathf.Max(1, ob.total)) : 1f;
                Color col = attack * (ob.tellLeft > 0 ? k * 0.6f : 1f);
                if (ob.kind == "spikes")
                {
                    (double mn, double mx) = g.OpenRange;
                    for (int s = 0; s < 2; s++)
                    {
                        double x0 = s == 0 ? ob.x0 : mx, x1 = s == 0 ? mn : ob.x1;
                        if (x1 - x0 < 0.05 || spike >= spikeT.Length) continue;
                        if (!spikeT[spike].gameObject.activeSelf) spikeT[spike].gameObject.SetActive(true);
                        double h = ob.tellLeft > 0 ? 3 * (0.15 + 0.85 * (1 - (double)ob.tellLeft / System.Math.Max(1, ob.total))) : 3;
                        Place(spikeT[spike], new Vec3((x0 + x1) * 0.5, o.y + h * 0.5, ob.z), x1 - x0, h, p.thick);
                        Tint(spikeR[spike], col);
                        spike++;
                    }
                }
                else if (bar < barT.Length)
                {
                    if (!barT[bar].gameObject.activeSelf) barT[bar].gameObject.SetActive(true);
                    Place(barT[bar], new Vec3((ob.x0 + ob.x1) * 0.5, o.y + (ob.y0 + ob.y1) * 0.5, ob.z), ob.x1 - ob.x0, ob.y1 - ob.y0, p.thick);
                    Tint(barR[bar], col);
                    bar++;
                }
            }
            for (int i = bar; i < barT.Length; i++) if (barT[i].gameObject.activeSelf) barT[i].gameObject.SetActive(false);
            for (int i = spike; i < spikeT.Length; i++) if (spikeT[i].gameObject.activeSelf) spikeT[i].gameObject.SetActive(false);
            foreach (Transform t in rollT) if (t.gameObject.activeSelf) t.gameObject.SetActive(false);
            foreach (Transform t in iceclT) if (t.gameObject.activeSelf) t.gameObject.SetActive(false);
        }

        /// <summary>NIGHT27: the skater with its aura, trails and ready shell, and the chase's lane attacks.</summary>
        void RenderChase27Body(Glacier27 g, SurvivalSim sim, Vec3 o, float a)
        {
            Vec3 feet = g.BossWorld; // y = the course floor
            Vec3 prev = new Vec3(g.prevCenter.x, o.y, g.prevCenter.z);
            Vec3 f = new Vec3(Mathf.Lerp((float)prev.x, (float)feet.x, a), o.y + 0.2, Mathf.Lerp((float)prev.z, (float)feet.z, a));
            double s = o.z - f.z;
            Quaternion rot = CurvedRot(s);
            int n = System.Math.Min(model.Count, skT.Length);
            for (int i = 0; i < n; i++)
            {
                var m = model[i];
                if (!skT[i].gameObject.activeSelf) skT[i].gameObject.SetActive(true);
                // the skater faces down the course (-z in the sim, +z in Unity): a = right, c = forward
                Vector3 centre = CurvedPoint(new Vec3(f.x + m.a, f.y + m.b, f.z - m.c));
                skT[i].SetPositionAndRotation(centre, rot);
                skT[i].localScale = new Vector3((float)m.hx * 2, (float)m.hy * 2, (float)m.hz * 2);
                Tint(skR[i], m.part == "blade" ? skBlade : m.part == "visor" ? skVisor : m.part == "plate" ? skAura * 2.2f : skBody);
            }
            for (int i = n; i < skT.Length; i++) if (skT[i].gameObject.activeSelf) skT[i].gameObject.SetActive(false);
            // the aura: a slightly larger pale shell round the body; while the window is open a cyan PULSE-READY shell (player-usable) pulses over it
            Glacier27Params p = g.p;
            if (!auraT.gameObject.activeSelf) auraT.gameObject.SetActive(true);
            Place(auraT, new Vec3(f.x, f.y + p.boxH * 0.5, f.z), p.boxW * UnityTuning.SkaterAuraGrow, p.boxH * UnityTuning.SkaterAuraGrow, p.boxL * UnityTuning.SkaterAuraGrow);
            Tint(auraR, skAura);
            bool ready = g.WindowOpen;
            if (readyT.gameObject.activeSelf != ready) readyT.gameObject.SetActive(ready);
            if (ready)
            {
                float pulse = 0.6f + 0.4f * Mathf.Sin(Time.time * 18f);
                Place(readyT, new Vec3(f.x, f.y + p.boxH * 0.5, f.z), p.boxW * 1.3, p.boxH * 1.15, p.boxL * 1.4);
                Tint(readyR, skReady * pulse);
            }
            // two thin additive blade trails on the ice behind it (length with its speed)
            double len = g.CurrentSpeed * UnityTuning.SkaterTrailPerSpeed;
            for (int i = 0; i < 2; i++)
            {
                if (!trailT[i].gameObject.activeSelf) trailT[i].gameObject.SetActive(true);
                Place(trailT[i], new Vec3(f.x + (i == 0 ? -0.18 : 0.18), o.y + UnityTuning.GlacierFloorLift * 3, f.z + len * 0.5 + 0.6), UnityTuning.SkaterTrailWidth, UnityTuning.GlacierStripThick, len);
                Tint(trailR[i], skBlade * (UnityTuning.SkaterTrailGain / UnityTuning.SkaterBladeGain));
            }
        }

        void RenderChase(Glacier27 g, SurvivalSim sim, Vec3 o, float a)
        {
            RenderChase27Body(g, sim, o, a);
            Glacier27Params p = g.p;
            int bar = 0, roll = 0, icl = 0;
            foreach (Glacier27Obstacle ob in g.obstacles)
            {
                float k = ob.tellLeft > 0 ? UnityTuning.GlacierTellMin + UnityTuning.GlacierTellRamp * (1f - (float)ob.tellLeft / Mathf.Max(1, ob.total)) : 1f;
                Color col = attack * (ob.tellLeft > 0 ? k * 0.6f : 1f);
                if (ob.kind == "roller")
                {
                    if (roll >= rollT.Length) continue;
                    if (!rollT[roll].gameObject.activeSelf) rollT[roll].gameObject.SetActive(true);
                    double x = ob.tellLeft > 0 ? o.x - ob.rollDir * p.laneHalf : ob.rollX;
                    Place(rollT[roll], new Vec3(x, o.y + p.rollerH * 0.5, ob.z), 2.0, p.rollerH, p.thick);
                    Tint(rollR[roll], ob.tellLeft > 0 ? col * 0.5f : attack * (UnityTuning.GlacierRollerGain / UnityTuning.GlacierAttackGain));
                    roll++;
                }
                else if (ob.kind == "icicle")
                {
                    // spikes hanging from the roof over the open width, growing through the tell, dropping when lethal
                    double w = ob.x1 - ob.x0;
                    int cnt = System.Math.Min(UnityTuning.GlacierIcicleMax - icl, (int)System.Math.Max(1, w / UnityTuning.GlacierIcicleEvery));
                    for (int s = 0; s < cnt; s++)
                    {
                        double cx = ob.x0 + (s + 0.5) * w / cnt;
                        double hgt = ob.tellLeft > 0 ? p.icicleTop * (0.25 + 0.2 * (1 - (double)ob.tellLeft / System.Math.Max(1, ob.total))) : p.icicleTop;
                        double cy = ob.tellLeft > 0 ? o.y + 9 - hgt * 0.5 : o.y + p.icicleTop * 0.5;
                        if (!iceclT[icl].gameObject.activeSelf) iceclT[icl].gameObject.SetActive(true);
                        Place(iceclT[icl], new Vec3(cx, cy, ob.z), UnityTuning.GlacierIcicleWidth, hgt, UnityTuning.GlacierIcicleWidth);
                        Tint(iceclR[icl], col);
                        icl++;
                    }
                }
                else if (bar < barT.Length)
                {
                    if (!barT[bar].gameObject.activeSelf) barT[bar].gameObject.SetActive(true);
                    Place(barT[bar], new Vec3((ob.x0 + ob.x1) * 0.5, o.y + (ob.y0 + ob.y1) * 0.5, ob.z), ob.x1 - ob.x0, ob.y1 - ob.y0, p.thick);
                    Tint(barR[bar], col);
                    bar++;
                }
            }
            for (int i = bar; i < barT.Length; i++) if (barT[i].gameObject.activeSelf) barT[i].gameObject.SetActive(false);
            for (int i = roll; i < rollT.Length; i++) if (rollT[i].gameObject.activeSelf) rollT[i].gameObject.SetActive(false);
            for (int i = icl; i < iceclT.Length; i++) if (iceclT[i].gameObject.activeSelf) iceclT[i].gameObject.SetActive(false);
            for (int i = 0; i < spikeT.Length; i++) if (spikeT[i].gameObject.activeSelf) spikeT[i].gameObject.SetActive(false);
        }
    }
}
