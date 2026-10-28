// NIGHT58 builder V, slate V3 (A-6, for builder L's L0: n57-kiln's exits): THE EXIT LAMP. View only (nothing reaches the sim): on n57-kiln
// (the map kiln57 loads; with kiln57 off the map is n52-kiln and nothing is built), each door that a room's clear opens (a campaign door whose
// zone is gated: Zones locks it, the clear opens it) gets a lamp: a cyan frame (two posts and a lintel just outside the opening, additive, HDR)
// and a cyan point light on the side of the room being left. It is dark while the door is locked and lights when the door opens (the room
// cleared), with a short fade in (one rise, never a flash; no blinking after), then holds. Cyan = player-usable (CLAUDE.md rule 8): "go here".
// Doors into a boss room are left alone (Doors38View's pale violet-white boundary keeps them). Doors38View's curtain is untouched.
// The data: the level's own campaign.doors[] (rect, height, between) and campaign.rooms[] (rect, kind): no lamp entries are needed in L's plan.
// Off: kiln57 off (no n57-kiln), -skygrindOff58 (NIGHT57's game), or `-skygrindNoExit58` (this lamp alone). Numbers: UnityTuning.N58V.cs.
// NIGHT60 V (B8): ON only with the view row design.n60.lamp60 (OFF in the repo; Tools/night/n60-v-rows.cjs), off under -skygrindOff60 too.
// NIGHT60 V (B8 attempt 2): the frame and the light stand at room A's face of the door's passage (FaceAnchor), not at its middle: n57-kiln's doors
// are 4 m deep, and at the rect's centre the posts and the lintel sat inside the walls (the lamp's own gain 1.00-1.06x on n60-b6).
// The proof: a358 prints Exit58.Line (the doors gated, the lamps lit, the last door lit and its frame); the luma ratio (the exit's luma vs the
// room's median at the clear frame >= 1.5x) is read from L's a157 / the a452 frames on kiln, on vs -skygrindNoExit58.
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed class Exit58 : MonoBehaviour
    {
        public const string Level = "n57-kiln";
        static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

        /// <summary>For the a358 line: the lamps' state on the current map.</summary>
        public static string Line = "exit lamps: not attached";
        /// <summary>For the reel: doors gated (lamps built), lamps lit now, the last door lit and the frame it lit on (-1: none yet).</summary>
        public static int Gated, LitNow, LastLitFrame = -1;
        public static string LastLitDoor = "";

        /// <summary>NIGHT60 V (B8): the lamp's switch is the view row design.n60.lamp60 (was the compile-time UnityTuning.Exit58On): off unless the row
        /// is on (a build copy: n46-build-rows.cjs <tag> on n60.lamp60), and off under -skygrindOff60 (or older) or -skygrindNoExit58.</summary>
        public static bool Off(GameConfig cfg) => !NightRows.ViewOn(cfg, "lamp60") || Rim60.Off60() || Rim58.Off58() || Rim58.No("-skygrindNoExit58");

        /// <summary>NIGHT60 V (B8 attempt 2): where the lamp's frame stands (web x, z) for a door `rect` whose room B lies `ahead` (Doors38Math.Ahead):
        /// just in front of room A's face of the door's passage (n57-kiln's doors are 4 m deep: at the rect's centre the frame was inside the walls).</summary>
        public static (double x, double z) FaceAnchor(double[] rect, double ax, double az, bool spansX)
        {
            double cx = (rect[0] + rect[2]) / 2, cz = (rect[1] + rect[3]) / 2;
            double depth = spansX ? rect[3] - rect[1] : rect[2] - rect[0]; // the passage's length along `ahead`
            double back = depth / 2 + UnityTuning.Exit58FrameW / 2 + UnityTuning.Exit60FaceGap; // from the centre to the frame's centre, toward room A
            return (cx - ax * back, cz - az * back);
        }

        sealed class Lamp
        {
            public string id;
            public Renderer[] parts;
            public Light light;
            public bool seenLocked, lit;
            public float t; // seconds since it lit
        }

        SurvivalGame game;
        SurvivalMap builtFor;
        Transform root;
        Material mat;
        Mesh cube;
        MaterialPropertyBlock mpb;
        readonly List<Lamp> lamps = new List<Lamp>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Install()
        {
            if (!Application.isPlaying) return;
            var go = new GameObject("Exit58") { hideFlags = HideFlags.HideAndDontSave };
            DontDestroyOnLoad(go);
            go.AddComponent<Exit58>();
        }

        bool Attach()
        {
            game = SurvivalGame.Instance;
            if (game == null || game.mats == null || game.cfg == null || game.sim == null) { game = null; return false; }
            mpb ??= new MaterialPropertyBlock();
            if (cube == null)
            {
                var box = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube = box.GetComponent<MeshFilter>().sharedMesh;
                Destroy(box);
            }
            if (mat == null) mat = FxDraw.Additive(game.mats.neon, "Exit 58");
            builtFor = null;
            return true;
        }

        Renderer Part(Transform parent, Vector3 at, Vector3 size)
        {
            var go = new GameObject("exit lamp part");
            go.transform.SetParent(parent, false);
            go.transform.SetPositionAndRotation(at, Quaternion.identity);
            go.transform.localScale = size;
            go.AddComponent<MeshFilter>().sharedMesh = cube;
            var r = go.AddComponent<MeshRenderer>();
            r.sharedMaterial = mat;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            r.receiveShadows = false;
            r.enabled = false;
            return r;
        }

        void Build()
        {
            builtFor = game.map;
            if (root != null) Destroy(root.gameObject);
            lamps.Clear();
            Gated = LitNow = 0;
            LastLitFrame = -1;
            LastLitDoor = "";
            string name = game.map != null && game.map.json != null && game.map.json.TryGetValue("name", out object no) ? no as string : null;
            if (Off(game.cfg)) { Line = $"exit lamps OFF ({(!NightRows.ViewOn(game.cfg, "lamp60") ? "design.n60.lamp60 off" : Rim60.Off60() ? "-skygrindOff60" : Rim58.Off58() ? "-skygrindOff58" : "-skygrindNoExit58")})"; return; }
            if (name != Level) { Line = $"exit lamps: not this level ({name ?? "no map"}; they are n57-kiln's)"; return; }
            if (!game.map.json.TryGetValue("campaign", out object co) || !(co is Dictionary<string, object> camp)
                || !camp.TryGetValue("doors", out object dobj) || !(dobj is List<object> doors))
            { Line = "exit lamps: n57-kiln has no campaign doors"; return; }
            root = new GameObject("Exit lamps 58").transform;
            DontDestroyOnLoad(root.gameObject);
            var rooms = Doors38Math.Rooms(camp);
            float w = UnityTuning.Exit58FrameW, gap = UnityTuning.Exit58FrameOut;
            foreach (object o in doors)
            {
                if (!(o is Dictionary<string, object> d) || !d.TryGetValue("id", out object io) || !(io is string id)) continue;
                if (game.map.ZoneIndex(id) < 0) continue; // never gated: no clear opens it
                double[] rect = d.TryGetValue("rect", out object ro) ? Doors38Math.Rect(ro) : null;
                if (rect == null || rect[2] <= rect[0] || rect[3] <= rect[1]) continue;
                double h = d.TryGetValue("height", out object ho) ? PortData.Num(ho) : 4;
                string a = null, b = null;
                if (d.TryGetValue("between", out object bo) && bo is List<object> bl && bl.Count >= 2) { a = bl[0] as string; b = bl[1] as string; }
                if (b != null && rooms.TryGetValue(b, out var rb0) && rb0.kind == "boss") continue; // the boss door keeps its boundary tone
                double[] ra = a != null && rooms.TryGetValue(a, out var rowA) ? rowA.rect : null;
                double[] rb = b != null && rooms.TryGetValue(b, out var rowB) ? rowB.rect : null;
                bool spansX = Doors38Math.SpansX(rect, ra, rb);
                var (ax, az) = Doors38Math.Ahead(rect, rb, spansX);
                var (cx, cz) = FaceAnchor(rect, ax, az, spansX); // NIGHT60 V (B8 attempt 2): room A's face of the passage, not its middle (the frame was in the walls)
                double half = (spansX ? rect[2] - rect[0] : rect[3] - rect[1]) / 2 + gap + w / 2;
                var t = new GameObject("exit lamp " + id).transform;
                t.SetParent(root, false);
                Vector3 p0 = WebSpace.ToUnity(new Vec3(cx - (spansX ? half : 0), h / 2, cz - (spansX ? 0 : half)));
                Vector3 p1 = WebSpace.ToUnity(new Vec3(cx + (spansX ? half : 0), h / 2, cz + (spansX ? 0 : half)));
                Vector3 top = WebSpace.ToUnity(new Vec3(cx, h + gap + w / 2, cz));
                float span = (float)(half * 2 + w);
                var lamp = new Lamp
                {
                    id = id,
                    parts = new[]
                    {
                        Part(t, p0, new Vector3(w, (float)h + gap, w)),
                        Part(t, p1, new Vector3(w, (float)h + gap, w)),
                        Part(t, top, spansX ? new Vector3(span, w, w) : new Vector3(w, w, span)),
                    },
                };
                var lg = new GameObject("exit lamp light");
                lg.transform.SetParent(t, false);
                // on the side of the room being left (room A): the floor and the walls round the opening take the cyan, the far room is not lit
                lg.transform.position = WebSpace.ToUnity(new Vec3(cx - ax * UnityTuning.Exit58LightBack, h * 0.6, cz - az * UnityTuning.Exit58LightBack));
                lamp.light = lg.AddComponent<Light>();
                lamp.light.type = LightType.Point;
                lamp.light.shadows = LightShadows.None;
                lamp.light.range = UnityTuning.Exit58LightRange;
                lamp.light.enabled = false;
                lamps.Add(lamp);
            }
            Gated = lamps.Count;
            Line = $"exit lamps ON on {Level}: gated doors {Gated}, lit 0";
        }

        void LateUpdate()
        {
            if (game == null && !Attach()) return;
            if (SurvivalGame.Instance != game) { game = null; return; }
            if (builtFor != game.map) Build();
            if (lamps.Count == 0) return;
            Zones zones = game.sim.zones;
            Color cyan = Look.C(game.cfg, "colors.usable");
            float dt = Time.deltaTime;
            int lit = 0;
            foreach (Lamp l in lamps)
            {
                bool open = zones == null || zones.IsOpen(l.id);
                if (!open)
                {
                    l.seenLocked = true;
                    if (l.lit) { l.lit = false; Show(l, false, 0f, cyan); }
                    continue;
                }
                if (!l.seenLocked) continue; // open since the map read (a run resumed past it): not this clear's exit
                if (!l.lit)
                {
                    l.lit = true;
                    l.t = 0f;
                    LastLitFrame = Time.frameCount;
                    LastLitDoor = l.id;
                }
                l.t += dt;
                float k = UnityTuning.Exit58FadeS > 0f ? Mathf.Clamp01(l.t / UnityTuning.Exit58FadeS) : 1f;
                Show(l, true, k, cyan);
                lit++;
            }
            LitNow = lit;
            Line = $"exit lamps ON on {Level}: gated doors {Gated}, lit {lit}; last lit {(LastLitFrame >= 0 ? LastLitDoor + " at frame " + LastLitFrame : "none yet")}";
        }

        void Show(Lamp l, bool on, float k, Color cyan)
        {
            foreach (Renderer r in l.parts)
            {
                r.enabled = on;
                if (!on) continue;
                mpb.SetColor(BaseColor, cyan * (UnityTuning.Exit58Glow * k));
                r.SetPropertyBlock(mpb);
            }
            l.light.enabled = on;
            if (on)
            {
                l.light.color = cyan;
                l.light.intensity = UnityTuning.Exit58LightIntensity * k;
            }
        }

        void OnDestroy()
        {
            if (root != null) Destroy(root.gameObject);
            if (mat != null) Destroy(mat);
        }
    }
}
