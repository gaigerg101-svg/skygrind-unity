// NIGHT43 builder A1 round 3 (moved from K1; GG G28: "The worm boss arena itself also needs a major visual redesign so it feels like an intentional
// boss location rather than another generic geometric room"; DESIGN-RULES V-1..V-5, A-5, A-6): THE RING (level 3's k5, THE KILN WYRM's room: the
// arena file the level names, wyrm39-arena.json today) dressed as the KILN'S HEART. A view row: design.n43.look43 (F1's row, read here; the reel
// forces it with WyrmRing43View.Force). VIEW ONLY: every piece is built from the room's own data when the worm's room is first shown, so it follows
// K1's geometry: the arena's boxes (the floor, the perches, the ride slabs, the stacks) and the worm's own routes (Wyrm.p: its holes and every
// pattern's waypoints where they cross the floor: where it breaks through). No collider, nothing on the sim path. Each piece tells one thing (V-4):
//   THREAT  the floor is dark iron PLATES; where the worm breaks the floor the plates are charred and split and LAVA glows through their seams
//           (the hot zones, from its routes); its two wall EXITS are glowing kiln mouths; the room's hottest light is the boss's red-orange, low
//           at the molten CORE (V-2: red-orange owns the danger and the boss).
//   WORLD   the molten CORE at the centre under a standable iron GRATE (the worm coils round it), fed by a CRUCIBLE hung on chains from the roof,
//           pouring into it: the landmark, seen from the entrance (A-6); the crucible stacks' hot bands.
//   ROUTE   the player's usable things are the only cyan (V-1): a lip round every boost perch's top, strips along the ride slabs (V-3); a perch
//           that crumbles (wyrm31) gets none (its strip would float after it falls).
// Round 3b: K1's wyrm43-arena.json (the level's arena from tonight) adds `landmark43` (THE KILN MOUTH on the north wall, opposite the spawn: a
// furnace mouth, its fire behind bars, its red-orange light out into the room) and `pieces43`'s slag plate (a raised, cool, riveted steel island
// off every worm route); both read from the arena file the sim loaded (Extras43); an arena without them draws neither.
// Light from below (A-5): the core's red-orange light on the floor and the walls, the crucible's amber, the mouths'. The lava breathes (its
// colour and the pour's width per frame: no allocation). Numbers: UnityTuning.Ring43 (render only).
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace Skygrind.Game
{
    public sealed partial class SurvivalGame
    {
        WyrmRing43View ring43View;

        /// <summary>NIGHT43 A1: THE RING's dressing (the reel reads its counts).</summary>
        internal WyrmRing43View Ring43View => ring43View;
    }

    public sealed class WyrmRing43View
    {
        /// <summary>The reel's switch (null: design.n43.look43 decides).</summary>
        public static bool? Force;

        readonly SurvivalGame game;
        GameObject root;
        WyrmArena builtFor;
        Vec3 builtAt;
        Material lavaMat, coreLavaMat, cyanMat, hotMat, pourMat, iron, ironDark;
        Transform pourT;
        Light coreLight, crucibleLight;
        readonly List<Light> mouthLights = new List<Light>();
        float t;
        int rowCfg = -1;

        /// <summary>For the reel: the pieces built (plates, hot plates, hot zones, perch lips, ride strips) and whether it shows now.</summary>
        public int Plates, HotPlates, HotZones, Lips, RideStrips;
        public bool Shown => root != null && root.activeSelf;

        public WyrmRing43View(SurvivalGame game) => this.game = game;

        bool RowOn()
        {
            if (Force.HasValue) return Force.Value;
            if (rowCfg < 0) rowCfg = N43View.RowOn(game.cfg, "look43") ? 1 : 0;
            return rowCfg == 1;
        }

        static Vector3 U(Vec3 v) => WebSpace.ToUnity(v);

        Material Opaque(string name, uint hex)
        {
            var mat = new Material(game.mats.stone) { name = name };
            Color c = Look.Hex(hex);
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", c);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", c);
            if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", null);
            if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", null);
            return Light54.StoneOrLit(game, mat, "WyrmRing43View", name, hex); // NIGHT54 light54: Stone.shader reads the vertex colour, not _BaseColor (white): the intended hex on URP Lit with the row on
        }

        GameObject Piece(Transform parent, string name, Mesh mesh, Material mat, Vector3 at, Vector3 scale, Quaternion rot)
        {
            var g = new GameObject(name);
            g.transform.SetParent(parent, false);
            g.transform.position = at;
            g.transform.localScale = scale;
            g.transform.rotation = rot;
            g.AddComponent<MeshFilter>().sharedMesh = mesh;
            var r = g.AddComponent<MeshRenderer>();
            r.sharedMaterial = mat;
            r.shadowCastingMode = ShadowCastingMode.Off;
            r.receiveShadows = false;
            return g;
        }

        Light PointLight(Transform parent, string name, Vector3 at, float range, float intensity, uint hex)
        {
            var lg = new GameObject(name);
            lg.transform.SetParent(parent, false);
            lg.transform.position = at;
            Light l = lg.AddComponent<Light>();
            l.type = LightType.Point;
            l.shadows = LightShadows.None;
            l.range = range;
            l.intensity = intensity * UnityTuning.LightScale;
            l.color = Look.Hex(hex);
            return l;
        }

        /// <summary>Where the worm breaks the floor, arena-local (x, z): its holes and every route's floor crossings (a waypoint pair across y 0),
        /// merged within Ring43MergeM; at most Ring43MaxZones.</summary>
        static List<Vec3> BreakPoints(Wyrm w)
        {
            var pts = new List<Vec3>();
            void Add(Vec3 p)
            {
                foreach (Vec3 q in pts)
                    if (JsMath.Hypot(q.x - p.x, q.z - p.z) < UnityTuning.Ring43MergeM) return;
                if (pts.Count < UnityTuning.Ring43MaxZones) pts.Add(new Vec3(p.x, 0, p.z));
            }
            void Route(List<Vec3> wp)
            {
                for (int i = 1; i < wp.Count; i++)
                {
                    Vec3 a = wp[i - 1], b = wp[i];
                    if ((a.y < 0) == (b.y < 0) || a.y == b.y) continue;
                    double k = a.y / (a.y - b.y);
                    Add(new Vec3(a.x + (b.x - a.x) * k, 0, a.z + (b.z - a.z) * k));
                }
            }
            if (w == null) return pts;
            foreach (Vec3 h in w.p.holes) Add(h);
            if (w.p.emerge != null) foreach (WyrmVariant v in w.p.emerge.variants) { Route(v.wp); Route(v.wp3); }
            foreach (WyrmPattern pat in w.p.patterns.Values)
                foreach (WyrmVariant v in pat.variants) { Route(v.wp); Route(v.wp3); }
            return pts;
        }

        void Build(WyrmArena a, Vec3 o, SurvivalSim sim)
        {
            if (root != null) Object.Destroy(root);
            root = new GameObject("NIGHT43 THE RING");
            Transform rt = root.transform;
            builtFor = a;
            builtAt = o;
            GameConfig cfg = game.cfg;
            iron = Opaque("ring43 iron", UnityTuning.Ring43IronHex);
            ironDark = Opaque("ring43 iron dark", UnityTuning.Ring43IronDarkHex);
            lavaMat = FxDraw.Opaque(game.mats.neon, "ring43 lava", Look.Hex(UnityTuning.Ring43LavaHex) * UnityTuning.Ring43LavaGlow);
            coreLavaMat = FxDraw.Opaque(game.mats.neon, "ring43 core", Look.Hex(UnityTuning.Ring43CoreHex) * UnityTuning.Ring43CoreGlow);
            cyanMat = FxDraw.Additive(game.mats.neon, "ring43 cyan");
            cyanMat.SetColor("_BaseColor", Look.C(cfg, "colors.usable") * UnityTuning.Ring43CyanGlow);
            hotMat = FxDraw.Additive(game.mats.neon, "ring43 hot band");
            hotMat.SetColor("_BaseColor", Look.Hex(UnityTuning.Ring43LavaHex) * UnityTuning.Ring43BandGlow);
            pourMat = FxDraw.Additive(game.mats.neon, "ring43 pour");
            pourMat.SetColor("_BaseColor", Look.Hex(UnityTuning.Ring43CoreHex) * UnityTuning.Ring43PourGlow);

            // the floor's top and the room's inside (the floor box and the shell's half)
            double floorY = 0, half = a.half;
            foreach (var (min, max, kind) in a.boxes) if (kind == "floor") floorY = max.y;
            List<Vec3> hot = BreakPoints(sim.wyrm24);
            hot.RemoveAll(h => System.Math.Abs(h.x) > half || System.Math.Abs(h.z) > half); // its wall exits' crossings lie outside the room
            HotZones = hot.Count;
            double coreHalf = UnityTuning.Ring43CoreHalf;
            bool Solid(double x, double z)
            {
                foreach (var (min, max, kind) in a.boxes)
                {
                    if (kind == "floor" || kind == "ceiling") continue;
                    if (min.y > floorY + 0.5) continue; // a perch overhead: the floor runs under it
                    if (x > min.x - 0.2 && x < max.x + 0.2 && z > min.z - 0.2 && z < max.z + 0.2) return true;
                }
                return false;
            }
            double HotAt(double x, double z)
            {
                double best = double.MaxValue;
                foreach (Vec3 h in hot) best = System.Math.Min(best, JsMath.Hypot(x - h.x, z - h.z));
                return best;
            }

            // THREAT + WORLD: the plates (cold: whole, raised iron; hot: charred, split in four, over lava), the lava under the hot zones, the core's grate
            var plates = new MeshKit();
            var lava = new MeshKit();
            var core = new MeshKit();
            Color top = Look.Hex(UnityTuning.Ring43PlateHex), side = Look.Hex(UnityTuning.Ring43PlateSideHex);
            Color hotTop = Look.Hex(UnityTuning.Ring43HotPlateHex), hotSide = Look.Hex(UnityTuning.Ring43HotPlateSideHex);
            double step = UnityTuning.Ring43PlateM, gap = UnityTuning.Ring43GapM, h0 = floorY + 0.005, h1 = floorY + UnityTuning.Ring43PlateH;
            int n = (int)System.Math.Floor(half / step);
            Plates = HotPlates = 0;
            for (int ix = -n; ix < n; ix++)
                for (int iz = -n; iz < n; iz++)
                {
                    double cx = (ix + 0.5) * step, cz = (iz + 0.5) * step;
                    if (System.Math.Abs(cx) < coreHalf && System.Math.Abs(cz) < coreHalf) continue; // the core's grate
                    if (Solid(cx, cz)) continue;
                    double d = HotAt(cx, cz);
                    if (d > UnityTuning.Ring43HotM)
                    {
                        double s = step / 2 - gap / 2;
                        plates.Box(o + new Vec3(cx - s, h0, cz - s), o + new Vec3(cx + s, h1, cz + s), top, side, null, true);
                        Plates++;
                        continue;
                    }
                    // a hot plate: four charred shards with wide seams over the lava
                    double q = step / 4, s2 = q - UnityTuning.Ring43HotGapM / 2;
                    for (int k = 0; k < 4; k++)
                    {
                        double sx = cx + (k % 2 == 0 ? -q : q), sz = cz + (k < 2 ? -q : q);
                        plates.Box(o + new Vec3(sx - s2, h0, sz - s2), o + new Vec3(sx + s2, h1 - 0.02, sz + s2), hotTop, hotSide, null, true);
                    }
                    lava.Box(o + new Vec3(cx - step / 2, floorY + 0.001, cz - step / 2), o + new Vec3(cx + step / 2, floorY + 0.004, cz + step / 2), Color.white, Color.white, null, true);
                    HotPlates++;
                }
            // the core: lava under a grate of bars (standable), a rim of dark iron
            core.Box(o + new Vec3(-coreHalf, floorY + 0.001, -coreHalf), o + new Vec3(coreHalf, floorY + 0.004, coreHalf), Color.white, Color.white, null, true);
            var grate = new MeshKit();
            for (double x = -coreHalf + 0.45; x < coreHalf; x += UnityTuning.Ring43BarPitch)
                grate.Box(o + new Vec3(x - 0.14, h0, -coreHalf), o + new Vec3(x + 0.14, h1 + 0.04, coreHalf), side, side, null, true);
            grate.Box(o + new Vec3(-coreHalf - 0.5, h0, -coreHalf - 0.5), o + new Vec3(coreHalf + 0.5, h1 + 0.1, -coreHalf), top, side, null, true);
            grate.Box(o + new Vec3(-coreHalf - 0.5, h0, coreHalf), o + new Vec3(coreHalf + 0.5, h1 + 0.1, coreHalf + 0.5), top, side, null, true);
            grate.Box(o + new Vec3(-coreHalf - 0.5, h0, -coreHalf), o + new Vec3(-coreHalf, h1 + 0.1, coreHalf), top, side, null, true);
            grate.Box(o + new Vec3(coreHalf, h0, -coreHalf), o + new Vec3(coreHalf + 0.5, h1 + 0.1, coreHalf), top, side, null, true);
            MeshKit.Object("ring43 plates", rt, plates.Build("ring43 plates"), game.mats.stone, shadows: false);
            MeshKit.Object("ring43 grate", rt, grate.Build("ring43 grate"), game.mats.stone, shadows: false);
            MeshKit.Object("ring43 lava", rt, lava.Build("ring43 lava"), lavaMat, shadows: false);
            MeshKit.Object("ring43 core lava", rt, core.Build("ring43 core"), coreLavaMat, shadows: false);

            // ROUTE: cyan lips on the perches' tops, strips along the ride slabs (the inner 12 m walls); none on a crumbling perch
            var cyan = new MeshKit();
            var band = new MeshKit();
            Lips = RideStrips = 0;
            bool crumbles = sim.n31.wyrm31;
            double lip = UnityTuning.Ring43LipM, lh = 0.07;
            for (int i = 0; i < a.boxes.Count; i++)
            {
                var (min, max, kind) = a.boxes[i];
                if (kind == "platform")
                {
                    if (crumbles && sim.wyrm31CrumbleBoxes.Contains(i)) continue;
                    double y = max.y + 0.01;
                    cyan.Box(o + new Vec3(min.x, y, min.z), o + new Vec3(max.x, y + lh, min.z + lip), Color.white, Color.white, null, true);
                    cyan.Box(o + new Vec3(min.x, y, max.z - lip), o + new Vec3(max.x, y + lh, max.z), Color.white, Color.white, null, true);
                    cyan.Box(o + new Vec3(min.x, y, min.z), o + new Vec3(min.x + lip, y + lh, max.z), Color.white, Color.white, null, true);
                    cyan.Box(o + new Vec3(max.x - lip, y, min.z), o + new Vec3(max.x, y + lh, max.z), Color.white, Color.white, null, true);
                    Lips++;
                }
                else if (kind == "wall" && max.y < a.height - 1 && min.y <= floorY + 0.01)
                {
                    // a ride slab: strips along its two long faces, at the top and at the ride height's foot (V-3)
                    bool alongX = max.x - min.x >= max.z - min.z;
                    double eps = 0.04, sw = UnityTuning.Ring43StripW;
                    foreach (double y in new[] { max.y - sw - 0.05, floorY + 1.0 })
                    {
                        if (alongX)
                        {
                            cyan.Box(o + new Vec3(min.x, y, min.z - eps), o + new Vec3(max.x, y + sw, min.z), Color.white, Color.white, null, true);
                            cyan.Box(o + new Vec3(min.x, y, max.z), o + new Vec3(max.x, y + sw, max.z + eps), Color.white, Color.white, null, true);
                        }
                        else
                        {
                            cyan.Box(o + new Vec3(min.x - eps, y, min.z), o + new Vec3(min.x, y + sw, max.z), Color.white, Color.white, null, true);
                            cyan.Box(o + new Vec3(max.x, y, min.z), o + new Vec3(max.x + eps, y + sw, max.z), Color.white, Color.white, null, true);
                        }
                    }
                    RideStrips++;
                }
                else if (kind == "pillar")
                {
                    // WORLD: a crucible stack's hot band near its top (the kiln breathing out)
                    double y = max.y - 2.2, e = 0.05;
                    band.Box(o + new Vec3(min.x - e, y, min.z - e), o + new Vec3(max.x + e, y + 0.6, max.z + e), Color.white, Color.white, null, true);
                }
            }
            MeshKit.Object("ring43 cyan", rt, cyan.Build("ring43 cyan"), cyanMat, shadows: false);
            MeshKit.Object("ring43 bands", rt, band.Build("ring43 bands"), hotMat, shadows: false);
            Extras43(a, o, rt, floorY, sim);

            // WORLD + LANDMARK: the crucible on four chains over the core, pouring into it
            Vector3 c0 = U(o + new Vec3(0, floorY, 0));
            float cy = (float)(floorY + UnityTuning.Ring43CrucibleY), cw = UnityTuning.Ring43CrucibleW, ch = UnityTuning.Ring43CrucibleH;
            Vector3 cAt = c0 + Vector3.up * (cy - (float)floorY);
            Piece(rt, "ring43 crucible", Mesh43.Get(Mesh43.Crucible), ironDark, cAt, new Vector3(cw, ch, cw), Quaternion.identity);
            Piece(rt, "ring43 crucible melt", Mesh43.Get(Mesh43.Ball), coreLavaMat, cAt + Vector3.up * ch * 0.32f, new Vector3(cw * 0.84f, 0.25f, cw * 0.84f), Quaternion.identity);
            Piece(rt, "ring43 crucible tap", Mesh43.Get(Mesh43.Spike), ironDark, cAt + Vector3.down * ch * 0.62f, new Vector3(1.0f, 1.2f, 1.0f), Quaternion.Euler(180, 0, 0));
            float roof = (float)(floorY + a.height);
            for (int k = 0; k < 4; k++)
            {
                float ang = k * 90 + 45;
                Vector3 rim = cAt + Quaternion.Euler(0, ang, 0) * new Vector3(0, ch * 0.45f, cw * 0.5f);
                float len = roof - rim.y;
                Piece(rt, "ring43 chain " + k, Mesh43.Get(Mesh43.Pill), iron, rim + Vector3.up * len * 0.5f, new Vector3(0.22f, len * 0.5f, 0.22f), Quaternion.identity);
            }
            float pourTop = cAt.y - ch * 1.1f, pourLen = pourTop - c0.y;
            pourT = Piece(rt, "ring43 pour", Mesh43.Get(Mesh43.Pill), pourMat, c0 + Vector3.up * pourLen * 0.5f, new Vector3(UnityTuning.Ring43PourW, pourLen * 0.5f, UnityTuning.Ring43PourW), Quaternion.identity).transform;
            crucibleLight = PointLight(rt, "ring43 crucible light", cAt + Vector3.up * ch * 0.6f, UnityTuning.Ring43CrucibleLightRange, UnityTuning.Ring43CrucibleLightIntensity, UnityTuning.Ring43CoreHex);
            coreLight = PointLight(rt, "ring43 core light", c0 + Vector3.up * 1.2f, UnityTuning.Ring43CoreLightRange, UnityTuning.Ring43CoreLightIntensity, UnityTuning.Ring43LavaHex);

            // THREAT: the worm's wall exits, kiln mouths (a dark iron ring round a molten throat) with their own light
            mouthLights.Clear();
            if (sim.wyrm24 != null)
                foreach (Vec3 ex in sim.wyrm24.p.exits)
                {
                    bool xWall = System.Math.Abs(ex.x) >= System.Math.Abs(ex.z);
                    Vec3 face = xWall ? new Vec3(System.Math.Sign(ex.x) * (half - 0.05), ex.y, ex.z) : new Vec3(ex.x, ex.y, System.Math.Sign(ex.z) * (half - 0.05));
                    Vector3 at = U(o + face);
                    Vector3 inward = (c0 - at);
                    inward.y = 0;
                    inward.Normalize();
                    Quaternion facing = Quaternion.FromToRotation(Vector3.up, inward);
                    Piece(rt, "ring43 mouth", Mesh43.Get(Mesh43.Ring), ironDark, at, new Vector3(UnityTuning.Ring43MouthR * 2, 3f, UnityTuning.Ring43MouthR * 2), facing);
                    Piece(rt, "ring43 throat", Mesh43.Get(Mesh43.Ball), lavaMat, at - inward * 0.3f, new Vector3(UnityTuning.Ring43MouthR * 1.4f, 0.4f, UnityTuning.Ring43MouthR * 1.4f), facing);
                    mouthLights.Add(PointLight(rt, "ring43 mouth light", at + inward * 2.5f, UnityTuning.Ring43MouthLightRange, UnityTuning.Ring43MouthLightIntensity, UnityTuning.Ring43LavaHex));
                }
            root.SetActive(false);
        }

        /// <summary>
        /// K1's wyrm43-arena.json extras (round 3b), read from the arena file the sim built the room from (SurvivalSim.Wyrm24Dir + wyrm39ArenaBuilt;
        /// none: nothing): `landmark43` (kind kilnMouth, rect [x0, z0, x1, z1] flush on a wall, top: THE KILN MOUTH, the room's landmark opposite the
        /// spawn) drawn as a furnace mouth: a dark iron frame, a molten throat behind iron bars, its red-orange light out into the room; `pieces43`'s
        /// low platforms (a SLAG PLATE, top <= Ring43SlabMaxTop m: an island off every worm route) capped in cool riveted steel with a dark skirt (raised,
        /// worm-proof, cool against the hot field; its cyan lip is the perches' rule).
        /// </summary>
        void Extras43(WyrmArena a, Vec3 o, Transform rt, double floorY, SurvivalSim sim)
        {
            Landmark = Slabs = 0;
            landmarkLight = null;
            if (string.IsNullOrEmpty(SurvivalSim.Wyrm24Dir) || string.IsNullOrEmpty(sim.wyrm39ArenaBuilt)) return;
            string path = System.IO.Path.Combine(SurvivalSim.Wyrm24Dir, sim.wyrm39ArenaBuilt);
            Dictionary<string, object> j;
            try
            {
                if (!System.IO.File.Exists(path)) return;
                j = PortData.Obj(PortData.LoadJson(path));
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("[ring43] " + path + ": " + e.Message);
                return;
            }
            if (j.TryGetValue("pieces43", out object po) && po is List<object> pieces)
            {
                var cap = new MeshKit();
                Color steel = Look.Hex(UnityTuning.Ring43SlabHex), skirt = Look.Hex(UnityTuning.Ring43SlabSideHex), rivet = Look.Hex(UnityTuning.Ring43SlabRivetHex);
                foreach (object pe in pieces)
                {
                    if (!(pe is Dictionary<string, object> pd) || !pd.TryGetValue("index", out object io)) continue;
                    int i = (int)PortData.Num(io);
                    if (i < 0 || i >= a.boxes.Count) continue;
                    var (min, max, kind) = a.boxes[i];
                    if (kind != "platform" || max.y - floorY > UnityTuning.Ring43SlabMaxTop) continue;
                    double e = 0.08;
                    cap.Box(o + new Vec3(min.x - e, max.y, min.z - e), o + new Vec3(max.x + e, max.y + 0.08, max.z + e), steel, skirt, null, true);
                    // a rivet row a hand in from each edge (the plate reads as bolted metal, not stone)
                    for (double x = min.x + 0.4; x < max.x - 0.3; x += 0.6)
                    {
                        cap.Box(o + new Vec3(x - 0.05, max.y + 0.08, min.z + 0.3), o + new Vec3(x + 0.05, max.y + 0.12, min.z + 0.4), rivet, rivet, null, true);
                        cap.Box(o + new Vec3(x - 0.05, max.y + 0.08, max.z - 0.4), o + new Vec3(x + 0.05, max.y + 0.12, max.z - 0.3), rivet, rivet, null, true);
                    }
                    cap.Box(o + new Vec3(min.x - 0.03, floorY + 0.005, min.z - 0.03), o + new Vec3(max.x + 0.03, max.y - 0.1, max.z + 0.03), skirt, skirt, null, true);
                    Slabs++;
                }
                if (Slabs > 0) MeshKit.Object("ring43 slag plates", rt, cap.Build("ring43 slag plates"), game.mats.stone, shadows: false);
            }
            if (j.TryGetValue("landmark43", out object lo) && lo is Dictionary<string, object> lm && PortData.Arr(lm["rect"]) is List<object> rect && rect.Count >= 4)
            {
                double x0 = PortData.Num(rect[0]), z0 = PortData.Num(rect[1]), x1 = PortData.Num(rect[2]), z1 = PortData.Num(rect[3]);
                double top = floorY + (lm.TryGetValue("top", out object to) ? PortData.Num(to) : 12);
                // which wall it is on: the thin axis; the room side is toward the centre
                bool onZ = System.Math.Abs(z1 - z0) < System.Math.Abs(x1 - x0);
                double face = onZ ? (System.Math.Abs(z0) > System.Math.Abs(z1) ? z1 : z0) : (System.Math.Abs(x0) > System.Math.Abs(x1) ? x1 : x0);
                double inSign = onZ ? -System.Math.Sign(face) : -System.Math.Sign(face);
                double lo0 = onZ ? System.Math.Min(x0, x1) : System.Math.Min(z0, z1), lo1 = onZ ? System.Math.Max(x0, x1) : System.Math.Max(z0, z1);
                Vec3 P(double along, double y, double depth) => onZ ? o + new Vec3(along, y, face + inSign * depth) : o + new Vec3(face + inSign * depth, y, along);
                var frame = new MeshKit();
                var throat = new MeshKit();
                Color fr = Look.Hex(UnityTuning.Ring43IronDarkHex), frSide = Look.Hex(UnityTuning.Ring43IronHex);
                double fw = UnityTuning.Ring43MouthFrameM, fd = 0.6;
                void Slab(double a0, double a1, double y0, double y1, double d0, double d1, MeshKit k, Color c, Color s)
                {
                    Vec3 p0 = P(a0, y0, d0), p1 = P(a1, y1, d1);
                    k.Box(new Vec3(System.Math.Min(p0.x, p1.x), p0.y, System.Math.Min(p0.z, p1.z)), new Vec3(System.Math.Max(p0.x, p1.x), p1.y, System.Math.Max(p0.z, p1.z)), c, s, null, true);
                }
                Slab(lo0, lo1, top - fw, top, 0.02, fd, frame, fr, frSide); // the lintel
                Slab(lo0, lo0 + fw, floorY, top, 0.02, fd, frame, fr, frSide); // the jambs
                Slab(lo1 - fw, lo1, floorY, top, 0.02, fd, frame, fr, frSide);
                Slab(lo0 - 0.6, lo1 + 0.6, floorY, floorY + 0.5, 0.02, fd + 0.4, frame, fr, frSide); // the sill
                for (double b = lo0 + fw + UnityTuning.Ring43MouthBarPitch * 0.5; b < lo1 - fw; b += UnityTuning.Ring43MouthBarPitch)
                    Slab(b - 0.18, b + 0.18, floorY + 0.5, top - fw, 0.15, 0.45, frame, fr, frSide); // the bars
                Slab(lo0 + fw, lo1 - fw, floorY + 0.5, top - fw, 0.03, 0.06, throat, Color.white, Color.white); // the molten throat behind the bars
                MeshKit.Object("ring43 kiln mouth", rt, frame.Build("ring43 kiln mouth"), game.mats.stone, shadows: false);
                MeshKit.Object("ring43 kiln throat", rt, throat.Build("ring43 kiln throat"), coreLavaMat, shadows: false);
                Vector3 lightAt = U(P((lo0 + lo1) * 0.5, floorY + (top - floorY) * 0.4, 4));
                landmarkLight = PointLight(rt, "ring43 kiln mouth light", lightAt, UnityTuning.Ring43LandmarkLightRange, UnityTuning.Ring43LandmarkLightIntensity, UnityTuning.Ring43LavaHex);
                Landmark = 1;
            }
        }

        /// <summary>For the reel: K1's landmark drawn (0 / 1), its slag plates capped.</summary>
        public int Landmark, Slabs;
        Light landmarkLight;

        /// <summary>Every frame: shown while the worm's room is (its boss alive) and the row is on; built on the first such frame; the lava breathes.</summary>
        public void Render()
        {
            SurvivalSim sim = game.sim;
            bool on = false;
            WyrmArena a = null;
            Vec3 o = default;
            if (sim != null && RowOn())
            {
                IBossSystem boss = sim.BossRoomSystem("wyrm");
                a = sim.BossRoomArena("wyrm", out o);
                on = ((boss != null && boss.Alive) || sim.Wyrm46ArenaHere) && a != null && game.wyrmRoomView != null && game.wyrmRoomView.ShownFor("wyrm"); // NIGHT46 V1: walkin46's arena from the door
            }
            if (on && (root == null || !ReferenceEquals(builtFor, a) || !builtAt.Equals(o))) Build(a, o, sim);
            if (root == null) return;
            if (root.activeSelf != on) root.SetActive(on);
            if (!on) return;
            t += Time.deltaTime;
            float breathe = 0.82f + 0.18f * Mathf.Sin(t * UnityTuning.Ring43BreatheHz * 2 * Mathf.PI);
            lavaMat.SetColor("_BaseColor", Look.Hex(UnityTuning.Ring43LavaHex) * (UnityTuning.Ring43LavaGlow * breathe));
            coreLavaMat.SetColor("_BaseColor", Look.Hex(UnityTuning.Ring43CoreHex) * (UnityTuning.Ring43CoreGlow * (0.9f + 0.1f * Mathf.Sin(t * 5.3f))));
            if (pourT != null)
            {
                Vector3 s = pourT.localScale;
                float w = UnityTuning.Ring43PourW * (0.85f + 0.15f * Mathf.Sin(t * 11f) + 0.08f * Mathf.Sin(t * 23f));
                pourT.localScale = new Vector3(w, s.y, w);
            }
            if (coreLight != null) coreLight.intensity = UnityTuning.Ring43CoreLightIntensity * UnityTuning.LightScale * breathe;
            if (landmarkLight != null) landmarkLight.intensity = UnityTuning.Ring43LandmarkLightIntensity * UnityTuning.LightScale * (0.85f + 0.15f * Mathf.Sin(t * 2.3f));
            for (int i = 0; i < mouthLights.Count; i++) mouthLights[i].intensity = UnityTuning.Ring43MouthLightIntensity * UnityTuning.LightScale * (0.8f + 0.2f * Mathf.Sin(t * 3.1f + i * 1.7f));
        }
    }

    public static partial class UnityTuning
    {
        // NIGHT43 A1 round 3: THE RING's dressing (WyrmRing43View; render only, no gameplay number).
        public const float Ring43PlateM = 3f; // m: the floor plates' pitch (a person's stride and a half: reads as metal plates, not tiles)
        public const float Ring43GapM = 0.16f; // m: the seam between two cold plates (dark: the stone below)
        public const float Ring43HotGapM = 0.32f; // m: the seams in a hot plate's four shards (lava shows through)
        public const float Ring43PlateH = 0.07f; // m: a plate's height over the floor (raised metal; the feet sink at most this)
        public const float Ring43HotM = 4.5f; // m: a plate this close to where the worm breaks the floor is hot (charred, split, lava under it)
        public const float Ring43MergeM = 3.5f; // m: break points closer than this are one hot zone
        public const int Ring43MaxZones = 16;
        public const float Ring43CoreHalf = 4.5f; // m: the molten core's grate (a 9 m square at the centre: the worm coils round it at ~19 m)
        public const float Ring43BarPitch = 0.9f; // m between the grate's bars (standable: the bars are 0.28 m wide)
        public const float Ring43LipM = 0.18f; // m: a perch's cyan lip
        public const float Ring43StripW = 0.18f; // m: a ride slab's cyan strip (V-3's one consistent edge strip)
        public const float Ring43CrucibleY = 21f; // m over the floor: the crucible's centre (under the 30 m roof, over the worm's highest arc ~13 m)
        public const float Ring43CrucibleW = 7f; // m: its width (seen from the entrance, 27 m away, over the core)
        public const float Ring43CrucibleH = 5f; // m: its height
        public const float Ring43PourW = 0.7f; // m: the molten pour's width (it breathes 0.6-0.8)
        public const float Ring43MouthR = 3.2f; // m: a kiln mouth's radius at the worm's wall exit
        public const uint Ring43IronHex = 0x2A2430; // the chains: dark violet iron (the environment's dusk value, V-1)
        public const uint Ring43IronDarkHex = 0x1A1620; // the crucible, the tap, the mouths' rings: darker
        public const uint Ring43PlateHex = 0x3A3442; // a cold plate's top: mid-dark violet iron (desaturated: the environment's value, V-1)
        public const uint Ring43PlateSideHex = 0x24202C; // its edge (the raised read)
        public const uint Ring43HotPlateHex = 0x2A1612; // a hot plate's top: charred red-black (the threat's hue, dark: the glow is the seam's)
        public const uint Ring43HotPlateSideHex = 0x160A08;
        public const uint Ring43LavaHex = 0xFF4A14; // the lava under the seams, the bands, the mouths: the boss's red-orange (V-2)
        public const float Ring43LavaGlow = 2.4f; // HDR x (the bloom: the seams read at the room's far side)
        public const uint Ring43CoreHex = 0xFF8A2A; // the core, the melt, the pour: hotter, toward amber (the hottest light in the room)
        public const float Ring43CoreGlow = 3.2f;
        public const float Ring43PourGlow = 2.6f;
        public const float Ring43CyanGlow = 2.0f; // the cyan lips and strips (additive)
        public const float Ring43BandGlow = 1.8f; // a stack's hot band (additive)
        public const float Ring43BreatheHz = 0.35f; // the lava's breath (s^-1)
        public const float Ring43CoreLightRange = 34f, Ring43CoreLightIntensity = 3.2f; // the core's red-orange light on the floor and the walls (from below, A-5)
        public const float Ring43CrucibleLightRange = 16f, Ring43CrucibleLightIntensity = 1.6f; // the melt's amber up onto the chains and the roof
        public const float Ring43MouthLightRange = 12f, Ring43MouthLightIntensity = 1.8f; // each kiln mouth's glow on its wall and the floor below it
        // round 3b (K1's wyrm43-arena.json): THE KILN MOUTH (landmark43) and the slag plates (pieces43)
        public const float Ring43MouthFrameM = 1.2f; // m: the kiln mouth's iron frame (lintel and jambs)
        public const float Ring43MouthBarPitch = 1.5f; // m between its bars (the fire behind them)
        public const float Ring43LandmarkLightRange = 30f, Ring43LandmarkLightIntensity = 2.6f; // its red-orange light 4 m out into the room (the far side lit: A-6)
        public const float Ring43SlabMaxTop = 2f; // m: a pieces43 platform this low is a SLAG PLATE (an island), capped in cool steel
        public const uint Ring43SlabHex = 0x6A6C80; // its cap: cool blue-grey steel (lighter than the iron floor: raised, cool, worm-proof against the hot field)
        public const uint Ring43SlabSideHex = 0x2E2E3A; // its skirt
        public const uint Ring43SlabRivetHex = 0x8C8EA2; // its rivets
    }
}
