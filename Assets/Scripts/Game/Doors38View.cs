// NIGHT38 builder M (design.n38.doors38; NIGHT38.md M1, PLAN.md section 3, DECIDE38 D-2; GG 2026-10-13 ~00:10: doors "not visible now and you
// just run through it and it's like dark", a holographic barrier instead): DOORS YOU CAN SEE. View only: installs itself (no edit in the lead's
// SurvivalGame files; Arena36View's pattern), reads the sim's door state, draws nothing on a level without campaign doors or with the row off
// (or -skygrindOff38). Unity objects are made on first use (never in field initialisers: L-035).
// - Every campaign doorway (json campaign.doors[]: rect [x0, z0, x1, z1], height) gets a holographic curtain across its opening: a thin additive
//   sheet that breathes, scanlines climbing through it, a bright post down each side and a bar along the top (the frame lit), and a point light
//   in the doorway in the curtain's colour.
// - OPEN (the door's zone open in Zones: the gate field is gone, nothing to collide with): CYAN (colors.usable), a thin veil, calm lines, and a
//   light just inside the next room so the room ahead is lit and visible through it. Passing through it washes the view with a short cyan
//   shimmer (Doors38View.Shimmers counts them).
// - LOCKED (the zone locked: Zones' gate field, a collider, is what the sim blocks with): RED-ORANGE, a dense sheet, agitated lines.
// - The BOSS door (the door into a room of kind "boss"): PALE VIOLET-WHITE (the boundary tone), with its sign: the boss room's name over the
//   door, turned to the camera (yaw only) so it reads from either side.
// The curtain's plane: across the passage, which runs from room `between[0]`'s middle to room `between[1]`'s (Doors38Math.SpansX; a door's rect
// is often square, so its sides cannot tell). Arena36View's locked slab uses the same rule (its x-only slab was the orientation bug) and hides
// its slab while this row is on (one red sheet, not two).
// Events (names only, for GG's sounds; CLAUDE.md rule 7): none are emitted by the sim for a view row; the view counts doors38Pass (Shimmers).
// Numbers: UnityTuning.Doors38.cs.
using System.Collections.Generic;
using Skygrind.Core;
using TMPro;
using UnityEngine;

namespace Skygrind.Game
{
    /// <summary>The doorway geometry shared by Doors38View and Arena36View (pure: no Unity objects).</summary>
    public static class Doors38Math
    {
        /// <summary>Does the curtain across door `rect` (web [x0, z0, x1, z1]) span x (the passage runs along z)? From the two rooms' middles
        /// (web rects, either may be null); without both rooms, the rect's longer side (a square rect: x).</summary>
        public static bool SpansX(double[] rect, double[] roomA, double[] roomB)
        {
            if (roomA != null && roomB != null)
            {
                double dx = (roomB[0] + roomB[2]) / 2 - (roomA[0] + roomA[2]) / 2;
                double dz = (roomB[1] + roomB[3]) / 2 - (roomA[1] + roomA[3]) / 2;
                if (System.Math.Abs(dx) > 1e-6 || System.Math.Abs(dz) > 1e-6) return System.Math.Abs(dz) >= System.Math.Abs(dx);
            }
            return rect[2] - rect[0] >= rect[3] - rect[1];
        }

        /// <summary>The unit direction (web x, z) from room A toward room B along the passage axis (+1 or -1 on one axis), for the light beyond.</summary>
        public static (double x, double z) Ahead(double[] rect, double[] roomB, bool spansX)
        {
            double cx = (rect[0] + rect[2]) / 2, cz = (rect[1] + rect[3]) / 2;
            if (roomB == null) return spansX ? (0, -1) : (1, 0);
            double bx = (roomB[0] + roomB[2]) / 2, bz = (roomB[1] + roomB[3]) / 2;
            return spansX ? (0, bz >= cz ? 1 : -1) : (bx >= cx ? 1 : -1, 0);
        }

        /// <summary>A [x0, z0, x1, z1] rect from a JSON list (null when malformed).</summary>
        public static double[] Rect(object o)
        {
            if (!(o is List<object> l) || l.Count < 4) return null;
            return new[] { PortData.Num(l[0]), PortData.Num(l[1]), PortData.Num(l[2]), PortData.Num(l[3]) };
        }

        /// <summary>The campaign's rooms by id: (rect, kind, name).</summary>
        public static Dictionary<string, (double[] rect, string kind, string name)> Rooms(Dictionary<string, object> campaign)
        {
            var res = new Dictionary<string, (double[], string, string)>();
            if (campaign == null || !campaign.TryGetValue("rooms", out object ro) || !(ro is List<object> rooms)) return res;
            foreach (object o in rooms)
            {
                if (!(o is Dictionary<string, object> r) || !r.TryGetValue("id", out object io) || !(io is string id)) continue;
                double[] rect = r.TryGetValue("rect", out object rc) ? Rect(rc) : null;
                string kind = r.TryGetValue("kind", out object k) && k is string ks ? ks : "";
                string name = r.TryGetValue("name", out object n) && n is string ns ? ns : id;
                res[id] = (rect, kind, name);
            }
            return res;
        }

        /// <summary>A room's name for a sign: its number prefix ("1.9 THE STACK" -> "THE STACK") dropped.</summary>
        public static string SignName(string name)
        {
            if (string.IsNullOrEmpty(name)) return "";
            int sp = name.IndexOf(' ');
            if (sp > 0 && char.IsDigit(name[0])) return name.Substring(sp + 1);
            return name;
        }
    }

    public sealed class Doors38View : MonoBehaviour
    {
        /// <summary>For the reels: curtains drawn this frame (open / locked / boss), and the passes through an open curtain this session.</summary>
        public static int OpenShown, LockedShown, BossShown, Shimmers, NoRideBoxes;

        /// <summary>Is the row on in this session (the config's design.n38.doors38.enabled and not -skygrindOff38)? Arena36View reads it.</summary>
        public static bool On;

        /// <summary>NIGHT47 F (door47): frames the shimmer veil was due to draw with a material that is not the live one (the pink-door bug: a
        /// destroyed material after a re-attach). The reel / Player log reads it: RED > 0 with the row off after a level change, GREEN 0 with it on.</summary>
        public static int StaleVeilFrames;

        /// <summary>NIGHT47 F: design.n47.door47 (a view row; -skygrindOff47 = NIGHT46's). Force: the reel's off / on passes (null: the row).</summary>
        public static bool? Door47Force;
        bool Door47On => Door47Force ?? (game != null && game.cfg != null && NightRows.ViewOn(game.cfg, "door47"));

        /// <summary>NIGHT47 F (the a847 door reel): the open, non-boss curtain nearest the camera last frame (its centre, its plane), and the
        /// re-attach a scene load causes (Attach runs again next frame: the material made anew, as on a level change).</summary>
        public static Vector3 ReelOpenCenter;
        public static bool ReelOpenSpansX, ReelOpenFound;
        float reelBest;
        static Doors38View instance;
        public static void ReelReattach() { if (instance != null) instance.game = null; }

        sealed class Curtain
        {
            public string id;
            public bool boss;
            public bool spansX;
            public Vector3 center; // Unity space, the opening's middle at half height
            public float width, height;
            public Vector3 beyond; // the light just inside the next room
            public Renderer sheet;
            public Renderer[] lines;
            public Renderer[] edges; // two posts + the top bar
            public Light light, beyondLight;
            public TextMeshPro sign;
            public float side; // the camera's side of the plane last frame (sign of the offset along the passage)
        }

        SurvivalGame game;
        bool iter41; // NIGHT41 J2: on through design.n41.doors41On: UnityTuning.Doors41's scales apply
        SurvivalMap builtFor;
        Transform root;
        Material mat;
        Mesh cube, quad;
        MaterialPropertyBlock mpb;
        readonly List<Curtain> curtains = new List<Curtain>();
        Renderer veil;
        float veilLeft;
        static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Install()
        {
            if (!Application.isPlaying) return;
            var go = new GameObject("Doors38View") { hideFlags = HideFlags.HideAndDontSave };
            DontDestroyOnLoad(go);
            instance = go.AddComponent<Doors38View>();
        }

        bool Attach()
        {
            game = FindFirstObjectByType<SurvivalGame>();
            if (game == null || game.mats == null || game.cfg == null || game.sim == null) { game = null; return false; }
            On = (!N38Blocks.ForceOff && game.cfg.Design("n38.doors38.enabled")) || N41View.RowOn(game.cfg, "doors41On"); // NIGHT41 (the lead's hook): or design.n41.doors41On (the n38 row untouched; off with --off41)
            // NIGHT41 J2: the iteration's scales (UnityTuning.Doors41.cs) apply only when the curtains are on through doors41On (the n38 row alone: NIGHT38's)
            iter41 = On && N41View.RowOn(game.cfg, "doors41On") && !(!N38Blocks.ForceOff && game.cfg.Design("n38.doors38.enabled"));
            builtFor = null;
            mpb ??= new MaterialPropertyBlock();
            if (cube == null || quad == null)
            {
                var box = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube = box.GetComponent<MeshFilter>().sharedMesh;
                Destroy(box);
                var q = GameObject.CreatePrimitive(PrimitiveType.Quad);
                quad = q.GetComponent<MeshFilter>().sharedMesh;
                Destroy(q);
            }
            if (mat != null) Destroy(mat);
            mat = FxDraw.Additive(game.mats.neon, "Doors 38");
            return true;
        }

        Renderer NewPart(string name, Mesh mesh, Transform parent)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var r = go.AddComponent<MeshRenderer>();
            r.sharedMaterial = mat;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            r.receiveShadows = false;
            r.enabled = false;
            return r;
        }

        Light NewLight(Transform parent, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var l = go.AddComponent<Light>();
            l.type = LightType.Point;
            l.shadows = LightShadows.None;
            l.enabled = false;
            return l;
        }

        TextMeshPro NewSign(Transform parent, string text, Color c, Vector3 at, Quaternion facing)
        {
            var go = new GameObject("boss sign");
            go.transform.SetParent(parent, false);
            go.transform.SetPositionAndRotation(at, facing);
            var t = go.AddComponent<TextMeshPro>();
            t.text = text;
            t.fontSize = UnityTuning.Doors38SignSize;
            t.alignment = TextAlignmentOptions.Bottom;
            t.fontStyle = FontStyles.Bold;
            t.color = c;
            t.rectTransform.sizeDelta = new Vector2(UnityTuning.Doors38SignWidth, 2f);
            go.SetActive(false);
            return t;
        }

        /// <summary>NIGHT38 noride's look (the card: "a dull, ridged material; no cyan trim"): every campaign box tagged `noride: true` gets dull
        /// dark ribs wrapped round it (horizontal bands proud of its faces), so a wall the ride cannot use reads as rough, not as a ride panel.
        /// Shown whenever the level has such boxes and NIGHT38 is not forced off (the Mover refuses them under the same switch). Returns the boxes.</summary>
        int BuildNoRide()
        {
            if (N38Blocks.ForceOff || game.map == null) return 0;
            Ride38.Box[] boxes = Ride38.NoRideFrom(game.map.json);
            if (boxes == null) return 0;
            var kit = new MeshKit();
            Color col = Look.C(game.cfg, "colors.pillarSide") * UnityTuning.NoRide38RibShade;
            float pitch = UnityTuning.NoRide38RibPitch, rh = UnityTuning.NoRide38RibH, proud = UnityTuning.NoRide38RibProud;
            foreach (Ride38.Box b in boxes)
            {
                Vector3 lo = WebSpace.ToUnity(b.min), hi = WebSpace.ToUnity(b.max);
                float x0 = Mathf.Min(lo.x, hi.x), x1 = Mathf.Max(lo.x, hi.x), z0 = Mathf.Min(lo.z, hi.z), z1 = Mathf.Max(lo.z, hi.z);
                float y0 = Mathf.Max(lo.y, 0f), y1 = hi.y;
                for (float y = y0 + pitch * 0.5f; y + rh * 0.5f < y1; y += pitch)
                    kit.Cube(new Vector3((x0 + x1) / 2, y, (z0 + z1) / 2), new Vector3(x1 - x0 + proud * 2, rh, z1 - z0 + proud * 2), col);
            }
            MeshKit.Object("No-ride ribs 38", root, kit.Build("noride38 ribs"), game.mats.stone, shadows: false);
            NoRideBoxes = boxes.Length;
            return boxes.Length;
        }

        /// <summary>The curtains for the current map (none with the row off, or no campaign doors).</summary>
        void Build()
        {
            builtFor = game.map;
            if (root != null) Destroy(root.gameObject);
            curtains.Clear();
            root = new GameObject("Doors 38").transform;
            DontDestroyOnLoad(root.gameObject);
            NoRideBoxes = 0;
            BuildNoRide();
            if (!On || game.map == null || game.map.json == null || !game.map.json.TryGetValue("campaign", out object co) || !(co is Dictionary<string, object> camp)) return;
            if (!camp.TryGetValue("doors", out object dobj) || !(dobj is List<object> doors)) return;
            var rooms = Doors38Math.Rooms(camp);
            Color violet = Look.Hex(UnityTuning.Doors38BossHex);
            foreach (object o in doors)
            {
                if (!(o is Dictionary<string, object> d) || !d.TryGetValue("id", out object io) || !(io is string id)) continue;
                double[] rect = d.TryGetValue("rect", out object ro) ? Doors38Math.Rect(ro) : null;
                if (rect == null || rect[2] <= rect[0] || rect[3] <= rect[1]) continue;
                double h = d.TryGetValue("height", out object ho) ? PortData.Num(ho) : 4;
                string a = null, b = null;
                if (d.TryGetValue("between", out object bo) && bo is List<object> bl && bl.Count >= 2)
                {
                    a = bl[0] as string;
                    b = bl[1] as string;
                }
                double[] ra = a != null && rooms.TryGetValue(a, out var rowA) ? rowA.rect : null;
                double[] rb = b != null && rooms.TryGetValue(b, out var rowB) ? rowB.rect : null;
                bool boss = b != null && rooms.TryGetValue(b, out var rowBoss) && rowBoss.kind == "boss";
                bool spansX = Doors38Math.SpansX(rect, ra, rb);
                var (ax, az) = Doors38Math.Ahead(rect, rb, spansX);
                double cx = (rect[0] + rect[2]) / 2, cz = (rect[1] + rect[3]) / 2;
                var c = new Curtain
                {
                    id = id,
                    boss = boss,
                    spansX = spansX,
                    center = WebSpace.ToUnity(new Vec3(cx, h / 2, cz)),
                    width = (float)(spansX ? rect[2] - rect[0] : rect[3] - rect[1]),
                    height = (float)h,
                    beyond = WebSpace.ToUnity(new Vec3(cx + ax * UnityTuning.Doors38BeyondDepth, h * 0.6, cz + az * UnityTuning.Doors38BeyondDepth)),
                    side = float.NaN,
                };
                var t = new GameObject("curtain " + id).transform;
                t.SetParent(root, false);
                c.sheet = NewPart("sheet", cube, t);
                c.lines = new Renderer[UnityTuning.Doors38Lines];
                for (int i = 0; i < c.lines.Length; i++) c.lines[i] = NewPart("scanline", cube, t);
                c.edges = new[] { NewPart("post", cube, t), NewPart("post", cube, t), NewPart("top bar", cube, t) };
                c.light = NewLight(t, "doorway light");
                c.light.transform.position = c.center;
                c.beyondLight = NewLight(t, "next room light");
                c.beyondLight.transform.position = c.beyond;
                Place(c);
                if (boss)
                {
                    string text = Doors38Math.SignName(rooms[b].name);
                    Vector3 at = c.center + Vector3.up * (c.height / 2 + UnityTuning.Doors38SignLift);
                    c.sign = NewSign(t, text, violet, at, Quaternion.identity); // turned to the camera (yaw only) each frame
                }
                curtains.Add(c);
            }
        }

        /// <summary>The sheet, the posts and the top bar of a curtain (the lines are placed each frame).</summary>
        void Place(Curtain c)
        {
            c.sheet.transform.position = c.center;
            c.sheet.transform.localScale = Dims(c, c.width, c.height, UnityTuning.Doors38Thick);
            PlaceEdges(c, UnityTuning.Doors38EdgeW);
        }

        /// <summary>NIGHT39 V: the frame's width this frame: Doors38EdgeW, or wider at distance so it keeps Doors38EdgeMinPx on screen (a 0.09 m post
        /// is ~1 px at 40 m), never more than Doors38EdgeMaxShare of the opening.</summary>
        static float EdgeWidth(Curtain c, Camera cam, float dist)
        {
            float ew = UnityTuning.Doors38EdgeW;
            if (cam == null || dist <= 0f || Screen.height <= 0) return ew;
            float metresPerPx = 2f * dist * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad) / Screen.height;
            return Mathf.Min(Mathf.Max(ew, UnityTuning.Doors38EdgeMinPx * metresPerPx), Mathf.Min(c.width, c.height) * UnityTuning.Doors38EdgeMaxShare);
        }

        /// <summary>The two posts and the top bar at frame width `ew`.</summary>
        void PlaceEdges(Curtain c, float ew)
        {
            float th = UnityTuning.Doors38Thick;
            float half = c.width / 2 - ew / 2;
            Vector3 along = c.spansX ? Vector3.right : Vector3.forward;
            c.edges[0].transform.position = c.center - along * half;
            c.edges[0].transform.localScale = Dims(c, ew, c.height, th * 2);
            c.edges[1].transform.position = c.center + along * half;
            c.edges[1].transform.localScale = Dims(c, ew, c.height, th * 2);
            c.edges[2].transform.position = c.center + Vector3.up * (c.height / 2 - ew / 2);
            c.edges[2].transform.localScale = Dims(c, c.width, ew, th * 2);
        }

        /// <summary>A box's scale: `across` along the opening's width, `up`, `deep` along the passage.</summary>
        static Vector3 Dims(Curtain c, float across, float up, float deep) => c.spansX ? new Vector3(across, up, deep) : new Vector3(deep, up, across);

        void Paint(Renderer r, Color col)
        {
            mpb.SetColor(BaseColor, col);
            r.SetPropertyBlock(mpb);
        }

        void LateUpdate()
        {
            if (game == null && !Attach()) return;
            if (builtFor != game.map) Build();
            OpenShown = LockedShown = BossShown = 0;
            if (!On || curtains.Count == 0)
            {
                if (veil != null) veil.enabled = false;
                return;
            }
            float time = Time.time, dt = Time.deltaTime;
            Color cyan = Look.C(game.cfg, "colors.usable") * UnityTuning.Doors38Glow;
            Color red = Look.Hex(UnityTuning.Arena36RedHex) * UnityTuning.Doors38Glow;
            Color violet = Look.Hex(UnityTuning.Doors38BossHex) * UnityTuning.Doors38Glow;
            Zones zones = game.sim.zones;
            Camera cam = game.cam != null ? game.cam : Camera.main;
            float breathe = 1f - UnityTuning.Doors38PulseDepth * 0.5f * (1f + Mathf.Sin(time * UnityTuning.Doors38PulseHz * 2f * Mathf.PI));
            bool wick43Doors = Wick37View.On43(game); // NIGHT43 B1 (wick43): THE STACK's door drawn the wick43 way
            reelBest = float.MaxValue;
            ReelOpenFound = false;
            foreach (Curtain c in curtains)
            {
                bool open = zones == null || game.map.ZoneIndex(c.id) < 0 || zones.IsOpen(c.id);
                // NIGHT39 V (the lead's n38-r1 frame f1200, 1.10 PIERCE, enemies alive: door d10 into a9 THE STACK, kind "boss", drew its violet
                // sheet while LOCKED and read pale white / pink): the lock wins the sheet and its lines (red-orange = solid, D-2), the boss door
                // keeps its violet frame (posts, top bar) and its sign, so it still reads as the boss door; open, the whole curtain is violet.
                Color col = c.boss && open ? violet : open ? cyan : red;
                Color frame = c.boss ? violet : col;
                // NIGHT43 B1 (wick43, G10: the boss door's violet-white and the entrance's red / blue split "a strange pink effect"): with the sim's
                // wick43 on, THE STACK's door is a door like the others (cyan open, red-orange locked), set apart by a heavier frame, its sign in
                // warm white and the furnace's warm light through it (UnityTuning.Wick43.cs)
                bool w43 = c.boss && wick43Doors;
                if (w43) { col = open ? cyan : red; frame = col; }
                float sheetA = c.boss ? UnityTuning.Doors38BossAlpha : open ? UnityTuning.Doors38OpenAlpha : UnityTuning.Doors38LockedAlpha;
                if (c.boss && !open) sheetA = UnityTuning.Doors38LockedAlpha;
                if (w43) sheetA = open ? UnityTuning.Doors38OpenAlpha : UnityTuning.Doors38LockedAlpha; // NIGHT43 B1: a door like the others
                if (iter41) sheetA *= !open ? UnityTuning.Doors41SheetLocked : c.boss ? UnityTuning.Doors41SheetBoss : UnityTuning.Doors41SheetOpen; // NIGHT41 J2
                float lineK = iter41 ? UnityTuning.Doors41Lines : 1f, edgeK = iter41 ? UnityTuning.Doors41Edges : 1f, lightK = iter41 ? UnityTuning.Doors41Light : 1f;
                // NIGHT39 V (frame f0800, THE YARD: the open cyan curtain a thin sliver at ~40 m): past Doors38FarStart the frame keeps at least
                // Doors38EdgeMinPx on screen and an open sheet brightens toward Doors38FarSheetBoost at Doors38FarEnd; nearer, nothing changes.
                float dist = cam != null ? Vector3.Distance(cam.transform.position, c.center) : 0f;
                float far = Mathf.Clamp01((dist - UnityTuning.Doors38FarStart) / (UnityTuning.Doors38FarEnd - UnityTuning.Doors38FarStart));
                if (open) sheetA *= Mathf.Lerp(1f, UnityTuning.Doors38FarSheetBoost, far);
                float ew = EdgeWidth(c, cam, dist);
                if (w43) ew = Mathf.Min(ew * UnityTuning.Wick43DoorFrameMul, Mathf.Min(c.width, c.height) * UnityTuning.Doors38EdgeMaxShare * UnityTuning.Wick43DoorFrameMul); // NIGHT43 B1: the heavier frame
                PlaceEdges(c, ew);
                Paint(c.sheet, col * (sheetA * breathe));
                c.sheet.enabled = true;
                foreach (Renderer e in c.edges)
                {
                    Paint(e, frame * (UnityTuning.Doors38EdgeAlpha * edgeK));
                    e.enabled = true;
                }
                float speed = open ? UnityTuning.Doors38LineSpeedOpen : UnityTuning.Doors38LineSpeedLocked;
                float lh = UnityTuning.Doors38LineH, inner = c.height - ew - lh;
                for (int i = 0; i < c.lines.Length; i++)
                {
                    float u = Mathf.Repeat(time * speed + (float)i / c.lines.Length, 1f);
                    Renderer l = c.lines[i];
                    l.transform.position = c.center + Vector3.up * (-c.height / 2 + lh / 2 + u * inner);
                    l.transform.localScale = Dims(c, c.width - ew * 2, lh, UnityTuning.Doors38Thick * 1.5f);
                    float fade = Mathf.Sin(u * Mathf.PI); // in at the bottom, out at the top
                    Paint(l, col * (UnityTuning.Doors38LineAlpha * lineK * fade));
                    l.enabled = true;
                }
                c.light.color = col / UnityTuning.Doors38Glow;
                c.light.intensity = UnityTuning.Doors38LightIntensity * lightK;
                c.light.range = UnityTuning.Doors38LightRange;
                c.light.enabled = true;
                c.beyondLight.enabled = open;
                if (open)
                {
                    c.beyondLight.color = w43 ? Look.Hex(UnityTuning.Wick43DoorBeyondHex) : (c.boss ? violet : cyan) / UnityTuning.Doors38Glow; // NIGHT43 B1: the furnace beyond
                    c.beyondLight.intensity = UnityTuning.Doors38BeyondIntensity;
                    c.beyondLight.range = UnityTuning.Doors38BeyondRange;
                }
                if (c.sign != null)
                {
                    if (!c.sign.gameObject.activeSelf) c.sign.gameObject.SetActive(true);
                    c.sign.color = w43 ? Look.Hex(UnityTuning.Wick43SignHex) : Look.Hex(UnityTuning.Doors38BossHex); // NIGHT43 B1: warm white with wick43 (the violet-white otherwise, as built)
                    if (cam != null)
                    {
                        Vector3 look = c.sign.transform.position - cam.transform.position;
                        look.y = 0;
                        if (look.sqrMagnitude > 1e-4f) c.sign.transform.rotation = Quaternion.LookRotation(look); // readable from either side
                    }
                }
                if (open && !c.boss && cam != null) // NIGHT47 F (the a847 door reel): the nearest open curtain this frame
                {
                    float dd = (cam.transform.position - c.center).sqrMagnitude;
                    if (dd < reelBest) { reelBest = dd; ReelOpenCenter = c.center; ReelOpenSpansX = c.spansX; ReelOpenFound = true; }
                }
                if (c.boss) BossShown++;
                else if (open) OpenShown++;
                else LockedShown++;
                // the camera through an open curtain: a short shimmer
                if (cam != null)
                {
                    Vector3 p = cam.transform.position - c.center;
                    float off = c.spansX ? p.z : p.x, lateral = c.spansX ? p.x : p.z;
                    bool inside = Mathf.Abs(lateral) <= c.width / 2 && p.y >= -c.height / 2 - 0.5f && p.y <= c.height / 2;
                    float side = Mathf.Sign(off);
                    if (open && inside && !float.IsNaN(c.side) && side != c.side && Mathf.Abs(off) < 2f)
                    {
                        veilLeft = UnityTuning.Doors38ShimmerS;
                        Shimmers++;
                    }
                    c.side = side;
                }
            }
            Veil(cam, cyan, dt);
        }

        /// <summary>The camera's shimmer: an additive cyan quad just in front of the view, fading over Doors38ShimmerS with a scanline flicker.</summary>
        void Veil(Camera cam, Color cyan, float dt)
        {
            if (cam == null) return;
            if (veil == null)
            {
                veil = NewPart("door shimmer", quad, root);
                veil.transform.SetParent(null, false);
                DontDestroyOnLoad(veil.gameObject);
            }
            // NIGHT47 F (door47, G10 "the door randomly turns pink"): the veil outlives a scene load (DontDestroyOnLoad) but Attach destroys `mat`
            // and makes a new one on every re-attach (each level change / continue / level select), so the veil kept the destroyed material and
            // drew Unity's magenta error shader over the whole view for Doors38ShimmerS on the next pass through an open door. Row on: the veil
            // takes the live material. Row off: NIGHT46's (the stale veil) bit for bit. StaleVeilFrames counts the frames it would draw stale.
            if (veil.sharedMaterial != mat)
            {
                if (Door47On) veil.sharedMaterial = mat; // fixed before it draws: never counted
                else if (veilLeft > 0) StaleVeilFrames++; // it draws stale this frame (the magenta veil)
            }
            if (veilLeft <= 0)
            {
                veil.enabled = false;
                return;
            }
            veilLeft -= dt;
            float k = Mathf.Clamp01(veilLeft / UnityTuning.Doors38ShimmerS);
            float d = UnityTuning.Doors38ShimmerDist;
            float hh = 2f * d * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
            Transform vt = veil.transform;
            vt.SetPositionAndRotation(cam.transform.position + cam.transform.forward * d, cam.transform.rotation);
            vt.localScale = new Vector3(hh * cam.aspect * 1.05f, hh * 1.05f, 1f);
            float flicker = 0.6f + 0.4f * Mathf.Abs(Mathf.Sin(Time.time * 90f));
            Paint(veil, cyan * (UnityTuning.Doors38ShimmerAlpha * k * flicker));
            veil.enabled = true;
        }
    }
}
