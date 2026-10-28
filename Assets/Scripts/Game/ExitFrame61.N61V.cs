// NIGHT61 builder V (slate S1; view row design.n61.exitFrame61, OFF in the repo; Tools/night/n61-v-rows.cjs): THE EXIT FRAME, lamp60 redesigned.
// GG 2026-10-13: doors "not visible now and you just run through it and it's like dark"; critics N55 "Doors read as a dark screen", N56 "the spire
// door is still near-black"; NIGHT60's lamp60 was SUBTLE (its own gain on the door 1.02-1.14x, Logs/n60/b8a2-lamp.txt). Not a third brightness
// tweak: a new view path. Exit58.N58V.cs (lamp60's path) and its numbers are unchanged. View only: nothing here reaches the sim.
// On n57-kiln each door a room's clear opens (a campaign door whose zone is gated, not into a boss room: Exit58's choice) gets, from the moment
// it opens (the room cleared):
// - THE FRAME: emissive cyan edge bars, two posts and a lintel UnityTuning.ExitFrame61BarW wide round the opening, just in front of room A's face
//   of the passage (n57-kiln's doors are 4 m deep), filling the band a157's clear shot counts as the door;
// - THE PASSAGE LIGHT: a point light (no shadow, never a flash) hung from a lit gate just inside the passage's far mouth (its fixture, PRINCIPLES 7),
//   so the passage glows from beyond: you see where to go next;
// - THE FLOOR LINE: dashes on room A's floor along the door's axis, through the doorway to the far mouth, brightening toward the door, clipped
//   UnityTuning.ExitFrame61LineClearM short of the first thing on its path (a step, a ramp, a pit, lava: the level's own data).
// Dark while the door is locked; one rise over UnityTuning.ExitFrame61RiseS when it opens, then it holds until the door locks again (Campaign24
// locks the door behind the player as the next room is entered). A door open since the map was read (a resumed run) stays dark (Exit58's rule).
// Cyan = player-usable (CLAUDE.md rule 8): "go here". Doors into a boss room are left alone (Doors38View's pale violet-white boundary keeps them).
// Off: the row off, -skygrindOff61 (or older), -skygrindNoExitFrame61 (this row alone) or -skygrindNoExit58 (no exit lamp at all: the slate's off
// side, so "on vs -skygrindNoExit58" measures this row in a build where lamp60 is off; the lead's call, NIGHT61). Both rows on draw both.
// The proof (L-089): one Player log line each time a door lights, "[exit61] door lit: room <id> (door <door>, frame <n>)", and one "[exit61] exit
// frames ON on n57-kiln: doors <n>, lit 0" when a map builds them; ExitFrame61.Line / LitNow / LastLit* for a reel. The pure parts and their
// tests: ExitFrame61Math.N61V.cs, N61ExitFrameTests. Numbers: UnityTuning.N61V.cs (Unity-only). No audio, no gameplay number, no sim row.
using System;
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;
using M = Skygrind.Game.ExitFrame61Math;

namespace Skygrind.Game
{
    public sealed class ExitFrame61 : MonoBehaviour
    {
        /// <summary>The level whose doors are framed (n57-kiln: the map kiln57 loads; with kiln57 off nothing is built).</summary>
        public const string Level = "n57-kiln";
        /// <summary>The floor's height at a campaign door (web y): n57-kiln's doors stand on y 0 (Exit58 and a157's clear-shot box assume it).</summary>
        const double FloorY = 0;
        static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

        /// <summary>For a reel: the frames' state on the current map.</summary>
        public static string Line = "exit frames: not attached";
        /// <summary>For a reel: doors framed on this map, lit now, the last door lit, its room and the frame it lit on (-1: none yet).</summary>
        public static int Framed, LitNow, LastLitFrame = -1;
        public static string LastLitDoor = "", LastLitRoom = "";

        static string[] args;

        /// <summary>This process's command line, read once (none readable: no switch).</summary>
        static string[] Args()
        {
            if (args == null)
            {
                try { args = Environment.GetCommandLineArgs() ?? new string[0]; }
                catch (Exception) { args = new string[0]; }
            }
            return args;
        }

        /// <summary>Off for this config and this process's command line (ExitFrame61Math.OffFor).</summary>
        public static bool Off(GameConfig cfg) => M.OffFor(cfg, Args());

        sealed class Lamp
        {
            public string door, room;
            public Renderer[] bars; // the frame's and the gate's
            public Renderer[] dashes;
            public float[] dashGlow; // each dash's share of the line's glow (ExitFrame61Math.LineGlow)
            public Light light;
            public bool seenLocked, lit, risen;
            public float t; // seconds since it lit
        }

        SurvivalGame game;
        SurvivalMap builtFor;
        Transform root;
        Material mat;
        Mesh cube;
        MaterialPropertyBlock mpb;
        Color cyan;
        readonly List<Lamp> lamps = new List<Lamp>();
        bool reelHidden; // NIGHT63 D1: the reel's held frame turned the row off (every lamp hidden, its state kept)

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Install()
        {
            if (!Application.isPlaying) return;
            var go = new GameObject("ExitFrame61") { hideFlags = HideFlags.HideAndDontSave };
            DontDestroyOnLoad(go);
            go.AddComponent<ExitFrame61>();
        }

        bool Attach()
        {
            game = SurvivalGame.Instance;
            if (game == null || game.mats == null || game.mats.neon == null || game.cfg == null || game.sim == null) { game = null; return false; }
            mpb ??= new MaterialPropertyBlock();
            builtFor = null;
            return true;
        }

        /// <summary>NIGHT63 R1 N1: the one-time cube mesh and additive material, made by Build only when it frames doors (ExitFrame61Math.MakesAssets).</summary>
        void EnsureAssets()
        {
            if (cube == null)
            {
                var box = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube = box.GetComponent<MeshFilter>().sharedMesh;
                Destroy(box);
            }
            if (mat == null) mat = FxDraw.Additive(game.mats.neon, "Exit frame 61");
        }

        Renderer Box(Transform parent, string name, M.Part p)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.SetPositionAndRotation(WebSpace.ToUnity(new Vec3(p.x, p.y, p.z)), Quaternion.identity);
            go.transform.localScale = new Vector3((float)p.sx, (float)p.sy, (float)p.sz); // the z flip moves the box, never its size
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
            root = null;
            lamps.Clear();
            reelHidden = false;
            Framed = LitNow = 0;
            LastLitFrame = -1;
            LastLitDoor = LastLitRoom = "";
            string why = M.OffReason(game.cfg, Args());
            if (why != null) { Line = "exit frames OFF (" + why + ")"; return; }
            string name = game.map != null && game.map.json != null && game.map.json.TryGetValue("name", out object no) ? no as string : null;
            if (name != Level) { Line = $"exit frames: not this level ({name ?? "no map"}; they are {Level}'s)"; return; }
            if (M.MakesAssets(why, name)) EnsureAssets(); // NIGHT63 R1 N1: the one-time cube and material only now (never while off or on another level)
            if (!game.map.json.TryGetValue("campaign", out object co) || !(co is Dictionary<string, object> camp)
                || !camp.TryGetValue("doors", out object dobj) || !(dobj is List<object> doors))
            { Line = "exit frames: " + Level + " has no campaign doors"; return; }
            cyan = Look.C(game.cfg, "colors.usable");
            root = new GameObject("Exit frames 61").transform;
            DontDestroyOnLoad(root.gameObject);
            var rooms = Doors38Math.Rooms(camp);
            List<M.Block> blocks = M.Blocks(game.map.json);
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
                lamps.Add(MakeLamp(id, a ?? "?", M.DoorOf(rect, h, ax, az, spansX), blocks));
            }
            Framed = lamps.Count;
            Line = $"exit frames ON on {Level}: doors {Framed}, lit 0";
            Debug.Log("[exit61] " + Line);
        }

        Lamp MakeLamp(string id, string room, M.Door door, List<M.Block> blocks)
        {
            var t = new GameObject("exit frame " + id).transform;
            t.SetParent(root, false);
            var bars = new List<Renderer>();
            foreach (M.Part p in M.Frame(door, UnityTuning.ExitFrame61BarW, UnityTuning.ExitFrame61BarD, UnityTuning.ExitFrame61FaceGap)) bars.Add(Box(t, "frame bar", p));
            foreach (M.Part p in M.Gate(door, UnityTuning.ExitFrame61GateW, UnityTuning.ExitFrame61BarD, UnityTuning.ExitFrame61GateInset)) bars.Add(Box(t, "gate bar", p));
            double run = M.FloorRun(door, blocks, FloorY, UnityTuning.ExitFrame61LineMaxM, UnityTuning.ExitFrame61LineHalfM, UnityTuning.ExitFrame61LineClearM,
                UnityTuning.ExitFrame61LineHeadM, UnityTuning.ExitFrame61LineStepM);
            List<M.Part> line = M.Line(door, run, FloorY, UnityTuning.ExitFrame61LineW, UnityTuning.ExitFrame61LineDashM, UnityTuning.ExitFrame61LineGapM,
                UnityTuning.ExitFrame61LineLift, UnityTuning.ExitFrame61LineThick);
            var dashes = new Renderer[line.Count];
            var glow = new float[line.Count];
            for (int i = 0; i < line.Count; i++)
            {
                dashes[i] = Box(t, "floor dash", line[i]);
                glow[i] = (float)M.LineGlow(M.Along(door, line[i].x, line[i].z), run, UnityTuning.ExitFrame61LineDim);
            }
            var lg = new GameObject("exit frame light");
            lg.transform.SetParent(t, false);
            lg.transform.position = WebSpace.ToUnity(M.LightSpot(door, UnityTuning.ExitFrame61GateW, UnityTuning.ExitFrame61BarD, UnityTuning.ExitFrame61GateInset,
                UnityTuning.ExitFrame61LightBack, UnityTuning.ExitFrame61LightDrop));
            Light light = lg.AddComponent<Light>();
            light.type = LightType.Point;
            light.shadows = LightShadows.None;
            light.range = UnityTuning.ExitFrame61LightRange;
            light.enabled = false;
            return new Lamp { door = id, room = room, bars = bars.ToArray(), dashes = dashes, dashGlow = glow, light = light };
        }

        void LateUpdate()
        {
            if (game == null && !Attach()) return;
            if (SurvivalGame.Instance != game) { game = null; return; }
            if (builtFor != game.map) Build();
            if (lamps.Count == 0) return;
            // NIGHT63 D1 (same-run SPOT pairs): the row re-read EVERY frame through the reel's live override (NightRows.Live: the build decided it
            // on, so only the reel can say off). Off (the reel's held frame): every lamp hidden, its state kept (lit, risen, its rise's age); back
            // on: each lamp shown exactly as it was, then the frame goes on as before.
            if (!NightRows.Live(true, M.Row))
            {
                if (!reelHidden)
                {
                    reelHidden = true;
                    foreach (Lamp l in lamps) Show(l, false, 0f);
                }
                return;
            }
            if (reelHidden)
            {
                reelHidden = false;
                foreach (Lamp l in lamps)
                    if (l.lit) Show(l, true, l.risen || UnityTuning.ExitFrame61RiseS <= 0f ? 1f : Mathf.Clamp01(l.t / UnityTuning.ExitFrame61RiseS));
            }
            Zones zones = game.sim.zones;
            float dt = FeelClock.UnscaledDelta; // a view's rise, in real time (a157's clear shot waits in real time); NIGHT63 D1: 0 in a held frame
            int lit = 0;
            bool changed = false;
            foreach (Lamp l in lamps)
            {
                bool open = zones == null || zones.IsOpen(l.door);
                if (!open)
                {
                    l.seenLocked = true;
                    if (l.lit)
                    {
                        l.lit = false;
                        Show(l, false, 0f);
                        changed = true;
                    }
                    continue;
                }
                if (!l.seenLocked) continue; // open since the map was read (a run resumed past it): not this clear's exit
                if (!l.lit)
                {
                    l.lit = true;
                    l.risen = false;
                    l.t = 0f;
                    LastLitFrame = Time.frameCount;
                    LastLitDoor = l.door;
                    LastLitRoom = l.room;
                    changed = true;
                    Debug.Log(M.LitLine(l.room, l.door, Time.frameCount));
                }
                lit++;
                if (l.risen) continue; // held at full: nothing to set
                l.t += dt;
                float k = UnityTuning.ExitFrame61RiseS > 0f ? Mathf.Clamp01(l.t / UnityTuning.ExitFrame61RiseS) : 1f;
                Show(l, true, k);
                if (k >= 1f) l.risen = true;
            }
            LitNow = lit;
            if (changed) Line = $"exit frames ON on {Level}: doors {Framed}, lit {lit}; last lit {(LastLitFrame >= 0 ? LastLitDoor + " (room " + LastLitRoom + ") at frame " + LastLitFrame : "none yet")}";
        }

        void Show(Lamp l, bool on, float k)
        {
            Color bar = cyan * (UnityTuning.ExitFrame61Glow * k);
            foreach (Renderer r in l.bars)
            {
                r.enabled = on;
                if (!on) continue;
                mpb.SetColor(BaseColor, bar);
                r.SetPropertyBlock(mpb);
            }
            for (int i = 0; i < l.dashes.Length; i++)
            {
                Renderer r = l.dashes[i];
                r.enabled = on;
                if (!on) continue;
                mpb.SetColor(BaseColor, cyan * (UnityTuning.ExitFrame61LineGlow * l.dashGlow[i] * k));
                r.SetPropertyBlock(mpb);
            }
            l.light.enabled = on;
            if (on)
            {
                l.light.color = cyan;
                l.light.intensity = UnityTuning.ExitFrame61LightIntensity * k;
            }
        }

        void OnDestroy()
        {
            if (root != null) Destroy(root.gameObject);
            if (mat != null) Destroy(mat);
        }
    }
}
