// NIGHT43 builder F1 round 2 (design.n43.look43, a view row: no sim bit; G8 GG: "stronger lighting, effects, environmental composition ... neon strips
// attached to simple geometry are not enough ... decorations should support the environment, gameplay, theme, navigation, or worldbuilding"). The
// look kit: from each campaign room's own data (LevelLook43: rooms, doors, lava, ride panels) it lights the world by rule, DESIGN-RULES A-5 / A-6 / V:
//   (1) the doors (A-6, V-5): a lit cyan frame inside the opening is the fixture: the current room's exit dim while it fights, bright and breathing
//       once it clears; then the NEXT room's exit frame lights too, with one moved point light at it (seen through the open door: the way on);
//   (2) lava lights the geometry from below (A-5): an additive red-orange gradient climbs each pool's walls from the surface, pulsing slowly, and one
//       moved point light stands over the pool nearest the player in the current room;
//   (3) ridable walls look ridable (V-3): one cyan edge strip under each ride panel's top and a rhythm of dashes along the ride band, on both long
//       faces; nothing on a wall that cannot be ridden. NIGHT26's strips (every long "wall" box) and NIGHT36's ride trim are hidden while this is on:
//       both place their pieces at the web z without the z flip the map builder applies (MeshKit.Box: WebSpace.ToUnity), so they stand mirrored
//       across z = 0, away from their walls (Look43Probe.OldOnPanels counts how many of their pieces touch a ride panel; the reel prints it);
//   (4) one landmark per arena (A-6): two tall pylons flanking its exit door against the far wall, in the level's cool hue (the lights35 fill's),
//       floor to under the roof: seen from the entrance, they say where forward is;
//   (5) the saturation budget (V-1): the map's stone (its vertex colours) drained 35 % toward grey and 8 % darker; enemies, lava, pickups, the HUD and
//       the cyan fixtures keep theirs.
// Built by CampaignLighting24 (one line) for a campaign map when the row is on, or with -skygrindLook43 on the command line (the reel's off / on pair
// toggles SetOn). Two URP point lights (the next door, the lava), no shadows; everything else emissive meshes. Nothing per frame allocated.
using System;
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    /// <summary>NIGHT43 F1 look43: what the kit built and shows (the reel's line).</summary>
    public static class Look43Probe
    {
        public static int RidePanels, RideDashes, Doors, Landmarks, LavaPools, StoneMeshes, OldPieces, OldOnPanels;
        /// <summary>NIGHT45 A3 (look45): arena rooms that kept their own landmark instead of the generic pylons (0 with the row off).</summary>
        public static int OwnLandmarks45;
        public static string Room = "", ExitDoor = "", NextDoor = "";
        public static bool On, Cleared, LavaLit;

        public static string Line() =>
            $"look43 {(On ? "ON" : "off")} | room {Room} cleared {Cleared}, exit {ExitDoor}, next {NextDoor}, lava light {LavaLit} | built: ride panels {RidePanels} (dashes {RideDashes}), door frames {Doors}, landmarks {Landmarks}, lava pools {LavaPools}, stone meshes drained {StoneMeshes} | NIGHT26 / 36 strip pieces {OldPieces}, touching a ride panel {OldOnPanels}";
    }

    public sealed class Look43View : MonoBehaviour
    {
        SurvivalGame game;
        LevelLook43 L;
        LevelDef def;
        bool on;
        readonly List<GameObject> mine = new List<GameObject>();
        Renderer[] doorR;
        Material[] doorM;
        float[] doorShown;
        Material lavaMat;
        Color cyan, lavaColour;
        Light nextLight, lavaLight;
        Transform oldStrips, oldTrim;
        readonly List<Mesh> stoneMeshes = new List<Mesh>();
        readonly List<Color[]> stoneOriginal = new List<Color[]>(), stoneDrained = new List<Color[]>();
        bool stoneDone;
        readonly HashSet<string> clearedIds = new HashSet<string>();
        Action unhook;

        /// <summary>The live kit (the reel; null on a map without one).</summary>
        public static Look43View Instance;

        /// <summary>The reel: draw the room as cleared whatever the campaign says (null = the campaign's state).</summary>
        public static bool? ForceCleared;

        static int cli = -1;

        /// <summary>-skygrindLook43 on the command line (the reel builds the kit even with the row off, to toggle it).</summary>
        public static bool ReelFlag
        {
            get
            {
                if (cli < 0)
                {
                    cli = 0;
                    try
                    {
                        foreach (string a in Environment.GetCommandLineArgs())
                            if (a == "-skygrindLook43") cli = 1;
                    }
                    catch (Exception) { cli = 0; }
                }
                return cli == 1;
            }
        }

        public bool On => on;

        /// <summary>The kit for a campaign map (CampaignLighting24.Build, after NIGHT26's strips and NIGHT36's trim): null with the row off and no reel flag.</summary>
        public static Look43View Create(SurvivalGame g, Transform parent, SurvivalMaterials mats)
        {
            bool row = N43View.RowOn(g.cfg, "look43");
            if (!row && !(ReelFlag && !N43View.Off)) return null;
            if (g.map == null || g.map.json == null || !g.map.json.ContainsKey("campaign")) return null;
            var go = new GameObject("Look 43");
            go.transform.SetParent(parent, false);
            var k = go.AddComponent<Look43View>();
            k.Build(g, parent, mats);
            k.SetOn(row);
            return k;
        }

        void Build(SurvivalGame g, Transform parent, SurvivalMaterials mats)
        {
            Instance = this;
            game = g;
            Look43Probe.RidePanels = Look43Probe.RideDashes = Look43Probe.Doors = Look43Probe.Landmarks = Look43Probe.LavaPools = Look43Probe.StoneMeshes = Look43Probe.OwnLandmarks45 = 0;
            GameConfig cfg = g.cfg;
            L = LevelLook43.From(g.map.json);
            def = LevelDef.From(g.map.json);
            cyan = Look.C(cfg, "colors.usable");
            oldStrips = parent.Find("Wall ride strips");
            oldTrim = parent.Find("Ride trim 36");
            BuildRide(mats);
            BuildDoors(mats);
            BuildLandmarks(mats, cfg);
            BuildLava(mats);
            nextLight = MakeLight("Look43 next door", (float)UnityTuning.Look43NextLightRange, cyan);
            lavaLight = MakeLight("Look43 lava", 8, Look.Hex(UnityTuning.Look43LavaHex));
            CountOld();
            unhook = g.events.On<string>(N43Events.RoomClear, id => { if (id != null) clearedIds.Add(id); });
            g.OnNewRun += clearedIds.Clear;
        }

        void OnDestroy()
        {
            unhook?.Invoke();
            if (game != null) game.OnNewRun -= clearedIds.Clear;
            if (Instance == this) Instance = null;
            if (!on) return;
            RestoreStone();
        }

        Light MakeLight(string name, float range, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            var l = go.AddComponent<Light>();
            l.type = LightType.Point;
            l.shadows = LightShadows.None;
            l.range = range;
            l.color = color;
            l.intensity = 0;
            return l;
        }

        GameObject Add(string name, MeshKit kit, Material m)
        {
            GameObject o = MeshKit.Object(name, transform, kit.Build(name), m, shadows: false);
            mine.Add(o);
            return o;
        }

        static Vec3 V(double x, double y, double z) => new Vec3(x, y, z);

        // --- (3) ride panels -------------------------------------------------------------------------------------------------------------------
        void BuildRide(SurvivalMaterials mats)
        {
            if (L.ride.Count == 0) return;
            var m = new Material(mats.neon) { name = "Look43 ride" };
            m.SetColor("_BaseColor", cyan * UnityTuning.Look43RideGain);
            var kit = new MeshKit();
            var starts = new List<double>();
            float th = UnityTuning.Look43RideStripH, off = UnityTuning.Look43RideOut, t = 0.04f;
            foreach (LevelLook43.Box b in L.ride)
            {
                bool alongX = b.x1 - b.x0 >= b.z1 - b.z0;
                double len = alongX ? b.x1 - b.x0 : b.z1 - b.z0;
                double base0 = Math.Max(b.y0, 0), top = b.y1 - UnityTuning.Look43RideTopDrop, band = Math.Min(base0 + UnityTuning.Look43RideBandY, top - th * 3);
                int n = LevelLook43.Dashes(len, UnityTuning.Look43RideDash, UnityTuning.Look43RideGap, starts);
                for (int side = 0; side < 2; side++)
                {
                    if (alongX)
                    {
                        double z = side == 0 ? b.z0 - off - t : b.z1 + off;
                        kit.Box(V(b.x0, top - th, z), V(b.x1, top, z + t), Color.white, Color.white);
                        foreach (double s in starts) kit.Box(V(b.x0 + s, band, z), V(b.x0 + s + UnityTuning.Look43RideDash, band + UnityTuning.Look43RideDashH, z + t), Color.white, Color.white);
                    }
                    else
                    {
                        double x = side == 0 ? b.x0 - off - t : b.x1 + off;
                        kit.Box(V(x, top - th, b.z0), V(x + t, top, b.z1), Color.white, Color.white);
                        foreach (double s in starts) kit.Box(V(x, band, b.z0 + s), V(x + t, band + UnityTuning.Look43RideDashH, b.z0 + s + UnityTuning.Look43RideDash), Color.white, Color.white);
                    }
                    Look43Probe.RideDashes += n;
                }
                Look43Probe.RidePanels++;
            }
            Add("Look43 ride strips", kit, m);
        }

        // --- (1) door frames ---------------------------------------------------------------------------------------------------------------------
        void BuildDoors(SurvivalMaterials mats)
        {
            int n = L.doors.Count;
            doorR = new Renderer[n];
            doorM = new Material[n];
            doorShown = new float[n];
            float p = UnityTuning.Look43DoorPost, ins = UnityTuning.Look43DoorInset;
            for (int i = 0; i < n; i++)
            {
                LevelLook43.Door d = L.doors[i];
                int ra = L.RoomIndex(d.a);
                double y0 = ra >= 0 ? L.rooms[ra].floorY : 0, h = d.height;
                var kit = new MeshKit();
                if (L.PassAlongZ(i))
                {
                    // the opening spans x: posts at x0 / x1 through the door's depth, the lintel across
                    kit.Box(V(d.x0 + ins, y0, d.z0), V(d.x0 + ins + p, y0 + h, d.z1), Color.white, Color.white);
                    kit.Box(V(d.x1 - ins - p, y0, d.z0), V(d.x1 - ins, y0 + h, d.z1), Color.white, Color.white);
                    kit.Box(V(d.x0 + ins, y0 + h - p, d.z0), V(d.x1 - ins, y0 + h, d.z1), Color.white, Color.white);
                }
                else
                {
                    kit.Box(V(d.x0, y0, d.z0 + ins), V(d.x1, y0 + h, d.z0 + ins + p), Color.white, Color.white);
                    kit.Box(V(d.x0, y0, d.z1 - ins - p), V(d.x1, y0 + h, d.z1 - ins), Color.white, Color.white);
                    kit.Box(V(d.x0, y0 + h - p, d.z0 + ins), V(d.x1, y0 + h, d.z1 - ins), Color.white, Color.white);
                }
                var m = new Material(mats.neon) { name = "Look43 door " + d.id };
                m.SetColor("_BaseColor", Color.black);
                doorM[i] = m;
                GameObject o = Add("Look43 door " + d.id, kit, m);
                doorR[i] = o.GetComponent<Renderer>();
                doorR[i].enabled = false;
                doorShown[i] = -1;
                Look43Probe.Doors++;
            }
        }

        // --- (4) landmarks -----------------------------------------------------------------------------------------------------------------------
        void BuildLandmarks(SurvivalMaterials mats, GameConfig cfg)
        {
            int theme = Look41.ThemeFor(cfg, game.map.json);
            var m = new Material(mats.neon) { name = "Look43 landmark" };
            m.SetColor("_BaseColor", Look.Hex(Look41.FillHex(theme)) * UnityTuning.Look43MarkGain);
            var kit = new MeshKit();
            float w = UnityTuning.Look43MarkW, side = UnityTuning.Look43MarkSide, inn = UnityTuning.Look43MarkIn;
            HashSet<string> own45 = Look45.Skip(cfg, game.map.json); // NIGHT45 A3 (look45): null with the row off (nothing skipped)
            foreach (LevelLook43.Room r in L.rooms)
            {
                if (!LevelLook43.IsArena(r)) continue;
                if (own45 != null && own45.Contains(r.id)) { Look43Probe.OwnLandmarks45++; continue; } // its own landmark, not the generic pylons
                int di = L.ExitDoor(r.id);
                double y0 = r.floorY, y1 = r.floorY + Math.Max(3, r.height - UnityTuning.Look43MarkTopDrop);
                double cx = (r.x0 + r.x1) / 2, cz = (r.z0 + r.z1) / 2;
                if (di >= 0 ? L.PassAlongZ(di) : Math.Abs(r.exitZ - cz) >= Math.Abs(r.exitX - cx))
                {
                    double dx0 = di >= 0 ? L.doors[di].x0 : r.exitX - 1.5, dx1 = di >= 0 ? L.doors[di].x1 : r.exitX + 1.5;
                    double dz = di >= 0 ? (L.doors[di].z0 + L.doors[di].z1) / 2 : r.exitZ;
                    double z = dz < cz ? r.z0 + inn : r.z1 - inn - w;
                    kit.Box(V(dx0 - side - w, y0, z), V(dx0 - side, y1, z + w), Color.white, Color.white);
                    kit.Box(V(dx1 + side, y0, z), V(dx1 + side + w, y1, z + w), Color.white, Color.white);
                }
                else
                {
                    double dz0 = di >= 0 ? L.doors[di].z0 : r.exitZ - 1.5, dz1 = di >= 0 ? L.doors[di].z1 : r.exitZ + 1.5;
                    double dx = di >= 0 ? (L.doors[di].x0 + L.doors[di].x1) / 2 : r.exitX;
                    double x = dx < cx ? r.x0 + inn : r.x1 - inn - w;
                    kit.Box(V(x, y0, dz0 - side - w), V(x + w, y1, dz0 - side), Color.white, Color.white);
                    kit.Box(V(x, y0, dz1 + side), V(x + w, y1, dz1 + side + w), Color.white, Color.white);
                }
                Look43Probe.Landmarks++;
            }
            if (Look43Probe.Landmarks > 0) Add("Look43 landmarks", kit, m);
        }

        // --- (2) lava underglow ------------------------------------------------------------------------------------------------------------------
        static Texture2D Gradient()
        {
            const int n = 64;
            var tex = new Texture2D(4, n, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear, name = "Look43 underglow" };
            var px = new Color32[4 * n];
            for (int y = 0; y < n; y++)
            {
                float u = y / (n - 1f), k = (1 - u) * (1 - u); // bright at the surface, gone by the top
                byte v = (byte)Mathf.RoundToInt(k * 255);
                for (int x = 0; x < 4; x++) px[y * 4 + x] = new Color32(v, v, v, 255);
            }
            tex.SetPixels32(px);
            tex.Apply(false, true);
            return tex;
        }

        void BuildLava(SurvivalMaterials mats)
        {
            if (L.lava.Count == 0) return;
            lavaColour = Look.Hex(UnityTuning.Look43LavaHex) * UnityTuning.Look43LavaGlowGain;
            lavaMat = FxDraw.Additive(mats.neon, "Look43 underglow");
            lavaMat.SetTexture("_BaseMap", Gradient());
            lavaMat.SetColor("_BaseColor", lavaColour);
            lavaMat.SetFloat("_Cull", 0);
            var v = new List<Vector3>();
            var uv = new List<Vector2>();
            var tri = new List<int>();
            float h = UnityTuning.Look43LavaGlowH;
            // a quad from the edge a -> b (Unity space) along `d` (up a wall, or out across the floor); v 0 at the edge (bright), 1 at the far side
            void Q(Vector3 a, Vector3 b, Vector3 d)
            {
                int i0 = v.Count;
                v.Add(a); v.Add(b); v.Add(b + d); v.Add(a + d);
                uv.Add(new Vector2(0, 0)); uv.Add(new Vector2(1, 0)); uv.Add(new Vector2(1, 1)); uv.Add(new Vector2(0, 1));
                tri.Add(i0); tri.Add(i0 + 2); tri.Add(i0 + 1);
                tri.Add(i0); tri.Add(i0 + 3); tri.Add(i0 + 2);
            }
            foreach (LevelLook43.Box b in L.lava)
            {
                double y = b.y0, cx = (b.x0 + b.x1) / 2, cz = (b.z0 + b.z1) / 2;
                LevelLook43.Room room = L.RoomAt(cx, cz);
                double floor = room != null ? Math.Max(room.floorY, y) : Math.Max(0, y);
                // each edge: the pit wall up to the floor; where the edge is the room's own wall, the glow climbs it (Look43LavaGlowH); else it
                // spills across the floor round the pool (Look43LavaFloorBand), so nothing glows in mid-air
                for (int e = 0; e < 4; e++)
                {
                    double ax, az, bx, bz; // the edges in order round the rect: z0 side, x1 side, z1 side, x0 side
                    switch (e)
                    {
                        case 0: ax = b.x0; az = b.z0; bx = b.x1; bz = b.z0; break;
                        case 1: ax = b.x1; az = b.z0; bx = b.x1; bz = b.z1; break;
                        case 2: ax = b.x1; az = b.z1; bx = b.x0; bz = b.z1; break;
                        default: ax = b.x0; az = b.z1; bx = b.x0; bz = b.z0; break;
                    }
                    Vector3 a = WebSpace.ToUnity(V(ax, y, az)), bb = WebSpace.ToUnity(V(bx, y, bz));
                    bool onWall = room != null && LevelLook43.OnRoomWall(room, ax, az, bx, bz);
                    float rise = onWall ? h : (float)(floor - y);
                    if (rise > 0.05f) Q(a, bb, Vector3.up * rise);
                    if (!onWall)
                    {
                        // outward across the floor (web: away from the pool's centre)
                        double ox = (ax + bx) / 2 - cx, oz = (az + bz) / 2 - cz;
                        Vector3 outward = Math.Abs(ox) > Math.Abs(oz) ? new Vector3(Math.Sign(ox), 0, 0) : new Vector3(0, 0, -Math.Sign(oz)); // web z -> Unity -z
                        Vector3 lift = Vector3.up * (float)(floor - y + 0.02);
                        Q(a + lift, bb + lift, outward * UnityTuning.Look43LavaFloorBand);
                    }
                }
                // the ride panels standing in the pool: their faces lit from the surface up
                foreach (LevelLook43.Box p in L.ride)
                {
                    if (p.x1 < b.x0 || p.x0 > b.x1 || p.z1 < b.z0 || p.z0 > b.z1 || p.y0 > y + 0.5) continue;
                    bool alongX = p.x1 - p.x0 >= p.z1 - p.z0;
                    float po = 0.02f;
                    if (alongX)
                    {
                        Q(WebSpace.ToUnity(V(p.x0, y, p.z0 - po)), WebSpace.ToUnity(V(p.x1, y, p.z0 - po)), Vector3.up * h);
                        Q(WebSpace.ToUnity(V(p.x0, y, p.z1 + po)), WebSpace.ToUnity(V(p.x1, y, p.z1 + po)), Vector3.up * h);
                    }
                    else
                    {
                        Q(WebSpace.ToUnity(V(p.x0 - po, y, p.z0)), WebSpace.ToUnity(V(p.x0 - po, y, p.z1)), Vector3.up * h);
                        Q(WebSpace.ToUnity(V(p.x1 + po, y, p.z0)), WebSpace.ToUnity(V(p.x1 + po, y, p.z1)), Vector3.up * h);
                    }
                }
                Look43Probe.LavaPools++;
            }
            var mesh = new Mesh { name = "Look43 underglow" };
            mesh.SetVertices(v);
            mesh.SetUVs(0, uv);
            mesh.SetTriangles(tri, 0);
            mesh.RecalculateBounds();
            GameObject o = MeshKit.Object("Look43 underglow", transform, mesh, lavaMat, shadows: false);
            mine.Add(o);
        }

        // --- (5) the stone's saturation -----------------------------------------------------------------------------------------------------------
        void DrainStone()
        {
            stoneDone = true;
            Transform root = transform.parent != null && transform.parent.parent != null ? transform.parent.parent : transform.parent;
            if (root == null || game.mats == null) return;
            foreach (MeshRenderer r in root.GetComponentsInChildren<MeshRenderer>(true))
            {
                if (r.sharedMaterial != game.mats.stone) continue;
                MeshFilter f = r.GetComponent<MeshFilter>();
                Mesh mesh = f != null ? f.sharedMesh : null;
                if (mesh == null || !mesh.isReadable) continue;
                Color[] c = mesh.colors;
                if (c == null || c.Length == 0) continue;
                var d = new Color[c.Length];
                for (int i = 0; i < c.Length; i++) d[i] = LevelLook43.Drain(c[i], UnityTuning.Look43StoneDesat, UnityTuning.Look43StoneValue);
                stoneMeshes.Add(mesh);
                stoneOriginal.Add(c);
                stoneDrained.Add(d);
            }
            Look43Probe.StoneMeshes = stoneMeshes.Count;
        }

        void ApplyStone(bool drained)
        {
            for (int i = 0; i < stoneMeshes.Count; i++)
                if (stoneMeshes[i] != null) stoneMeshes[i].colors = drained ? stoneDrained[i] : stoneOriginal[i];
        }

        void RestoreStone() => ApplyStone(false);

        // --- NIGHT26 / NIGHT36 pieces: do they touch a ride panel? (the reel's line; their placement is theirs) ------------------------------------
        void CountOld()
        {
            int pieces = 0, onPanel = 0;
            foreach (Transform t in new[] { oldStrips, oldTrim })
            {
                if (t == null) continue;
                foreach (Renderer r in t.GetComponentsInChildren<Renderer>(true))
                {
                    pieces++;
                    Bounds pb = r.bounds;
                    foreach (LevelLook43.Box b in L.ride)
                    {
                        Vector3 a = WebSpace.ToUnity(V(b.x0, b.y0, b.z0)), c = WebSpace.ToUnity(V(b.x1, b.y1, b.z1));
                        var bb = new Bounds((a + c) * 0.5f, new Vector3(Mathf.Abs(c.x - a.x), Mathf.Abs(c.y - a.y), Mathf.Abs(c.z - a.z)) + Vector3.one * 1.0f);
                        if (bb.Intersects(pb)) { onPanel++; break; }
                    }
                }
            }
            Look43Probe.OldPieces = pieces;
            Look43Probe.OldOnPanels = onPanel;
        }

        /// <summary>The kit on / off now (the design row at build; the reel's pair). Off: nothing of it drawn, the old strips back, the stone's colours back.</summary>
        public void SetOn(bool v)
        {
            on = v;
            Look43Probe.On = v;
            foreach (GameObject o in mine) if (o != null) o.SetActive(v);
            if (oldStrips != null) oldStrips.gameObject.SetActive(!v);
            if (oldTrim != null) oldTrim.gameObject.SetActive(!v);
            if (!v)
            {
                if (nextLight != null) nextLight.intensity = 0;
                if (lavaLight != null) lavaLight.intensity = 0;
                ApplyStone(false);
            }
            else if (stoneDone) ApplyStone(true);
        }

        void Update()
        {
            if (game == null || game.sim == null || L == null) return;
            if (!on) return;
            if (!stoneDone)
            {
                DrainStone(); // the first frame: every stone mesh of the map exists by now (Kit33's props are built after this view)
                ApplyStone(true);
            }
            Campaign24 c = game.sim.campaign;
            int ri = -1;
            if (c != null && c.room >= 0 && def != null && c.room < def.rooms.Count) ri = L.RoomIndex(def.rooms[c.room].id);
            if (ri < 0 && L.rooms.Count > 0) ri = 0;
            if (ri < 0) return;
            LevelLook43.Room room = L.rooms[ri];
            bool cleared = ForceCleared ?? (c != null && c.phase == Campaign24.Phase.Cleared) || clearedIds.Contains(room.id);
            bool reduced = game.cfg.movement.settings.reducedMotion;
            float t = Time.time;
            int exit = L.ExitDoor(room.id), next = cleared ? L.NextDoor(room.id) : -1;
            float open = UnityTuning.Look43DoorOpen * LevelLook43.Pulse(t, UnityTuning.Look43DoorPulseHz, UnityTuning.Look43DoorPulseDepth, reduced);
            for (int i = 0; i < doorR.Length; i++)
            {
                float k = i == exit ? (cleared ? open : UnityTuning.Look43DoorLocked) : i == next ? UnityTuning.Look43DoorNext : 0;
                if (k == doorShown[i]) continue;
                doorShown[i] = k;
                doorR[i].enabled = k > 0;
                if (k > 0) doorM[i].SetColor("_BaseColor", cyan * (UnityTuning.Look43DoorGain * k));
            }
            Look43Probe.Room = room.id;
            Look43Probe.Cleared = cleared;
            Look43Probe.ExitDoor = exit >= 0 ? L.doors[exit].id : "none";
            Look43Probe.NextDoor = next >= 0 ? L.doors[next].id : "none";
            // the next door's light
            if (next >= 0)
            {
                LevelLook43.Door d = L.doors[next];
                int rb = L.RoomIndex(d.a);
                double y = (rb >= 0 ? L.rooms[rb].floorY : 0) + UnityTuning.Look43NextLightY;
                nextLight.transform.position = WebSpace.ToUnity(V((d.x0 + d.x1) / 2, y, (d.z0 + d.z1) / 2));
                nextLight.intensity = (float)UnityTuning.Look43NextLightIntensity * UnityTuning.LightScale;
            }
            else nextLight.intensity = 0;
            // the lava: the underglow's pulse and the nearest pool's light
            bool lava47 = Lava47.On(game.cfg); // NIGHT47 F lava47 (LightKit29.Lava47.cs): the one breath; the roaming lava light stays dark
            float lp = lava47 ? Lava47.Breath(Time.time, reduced) : LevelLook43.Pulse(t, UnityTuning.Look43LavaPulseHz, UnityTuning.Look43LavaPulseDepth, reduced);
            if (lavaMat != null) lavaMat.SetColor("_BaseColor", lavaColour * lp);
            Vec3 feet = game.Feet;
            int li = L.LavaIn(room, feet.x, feet.z);
            Look43Probe.LavaLit = li >= 0;
            if (li >= 0 && !lava47)
            {
                LevelLook43.Box b = L.lava[li];
                lavaLight.transform.position = WebSpace.ToUnity(V((b.x0 + b.x1) / 2, b.y0 + UnityTuning.Look43LavaLightY, (b.z0 + b.z1) / 2));
                lavaLight.range = (float)Math.Max(UnityTuning.Look43LavaLightRangeMin, Math.Max(b.x1 - b.x0, b.z1 - b.z0) * UnityTuning.Look43LavaLightRangeShare);
                lavaLight.intensity = (float)UnityTuning.Look43LavaLightIntensity * UnityTuning.LightScale * lp;
            }
            else lavaLight.intensity = 0;
        }
    }
}
