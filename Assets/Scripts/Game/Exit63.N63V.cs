// NIGHT63 builder V (exit63; view row design.n63.exit63, OFF in the repo; Tools/night/n63-v-exit-rows.cjs; the doneWhen in
// Research/2026-10-22-wave63/V-EXIT.md, written before any Player run): THE DOORWAY ITSELF IS THE WAY OUT. GG 2026-10-13: doors "not visible now
// and you just run through it and it's like dark"; tonight's P1 (exitFrame61: a frame, a passage light, a floor line) was SHORT, its own gain
// on the door 1.07-1.56x, and its brighter frame lit the gun (the GUARD). Not a light this time: the door's own geometry.
// On n57-kiln each door a room's clear opens (a campaign door whose zone is gated, not into a boss room: Exit58's and exitFrame61's choice) gets,
// from the moment it opens, inside its passage only (Exit63Math: every part clear of the passage's walls, floor and ceiling, never in room A):
// - THE VEIL: a thin additive cyan sheet over the opening just inside room A's face; it steps aside as the eye reaches it (numbers.veilNearM ..
//   veilFarM), so running through the door never washes the screen (Doors38View's shimmer marks the crossing);
// - THE LINERS: both jambs and the lintel lined in cyan for the passage's first numbers.linerDepthM: the doorway's own surfaces;
// - THE THRESHOLD and THE DASHES: a lit strip across the floor just inside the face, dashes along the passage floor toward room B.
// Cyan = player-usable (CLAUDE.md rule 8): "go here". Additive on FxDraw's copy of the neon material, every gain 1 or under (no bloom past the
// door's box), no Light (nothing lights the room or the gun). Dark while the door is locked; ONE RISE (smoothstep over numbers.riseS on
// FeelClock.UnscaledDelta: 0 in D1's held frame) when it opens, then held until the door locks again (Campaign24 locks it behind the player);
// a door open since the map was read stays dark (exitFrame61's rule). Doors into a boss room keep Doors38View's boundary tone.
// THE ROW, READ EVERY FRAME (NightRows.ViewOn in LateUpdate: its config, the process's off switch, D1's reel override): off hides every part
// and keeps each door's state (lit, its rise's age), so the next frame on is the frame it would have drawn; a map built while the row was off
// is built when it turns on. Off: the row off, -skygrindOff63 (or older), -skygrindNoExit63: nothing is made.
// The proof (L-089): one Player log line each time a door lights, "[exit63] door lit: room <id> (door <door>, frame <n>)", and one "[exit63]
// exit63 ON on n57-kiln: doors <n>, lit 0" when a map builds them; Exit63.Line / LitNow / LastLit* for a reel. No audio, no gameplay number,
// no sim row.
using System;
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;
using F = Skygrind.Game.ExitFrame61Math;
using X = Skygrind.Game.Exit63Math;

namespace Skygrind.Game
{
    public sealed class Exit63 : MonoBehaviour
    {
        /// <summary>The level whose doors are lit (n57-kiln: campaign doors carry no floor height; kiln's stand on y 0, as a157's box assumes).</summary>
        public const string Level = "n57-kiln";
        static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

        /// <summary>For a reel: the doors' state on the current map.</summary>
        public static string Line = "exit63: not attached";
        /// <summary>For a reel: doors built on this map, lit now, the last door lit, its room and the frame it lit on (-1: none yet).</summary>
        public static int Doors, LitNow, LastLitFrame = -1;
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

        sealed class Lamp
        {
            public string door, room;
            public Renderer veil;
            public Bounds veilBox; // the veil's world box (Unity space), for the eye's distance
            public Renderer[] parts; // the liners, the strip, the dashes
            public float[] gains;
            public bool seenLocked, lit, risen;
            public float t; // seconds since it lit
            public float shownK = -1f, shownVeil = -1f; // what is drawn now (-1: hidden)
        }

        SurvivalGame game;
        SurvivalMap builtFor;
        bool idleByRow; // built while the row itself was off: built again when it turns on
        bool hidden; // the row read off this frame: every part hidden, every door's state kept
        Transform root;
        Material mat;
        Mesh cube;
        MaterialPropertyBlock mpb;
        Color cyan;
        X.Params p;
        readonly List<Lamp> lamps = new List<Lamp>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Install()
        {
            if (!Application.isPlaying) return;
            var go = new GameObject("Exit63") { hideFlags = HideFlags.HideAndDontSave };
            DontDestroyOnLoad(go);
            go.AddComponent<Exit63>();
        }

        bool Attach()
        {
            game = SurvivalGame.Instance;
            if (game == null || game.mats == null || game.mats.neon == null || game.cfg == null || game.sim == null) { game = null; return false; }
            mpb ??= new MaterialPropertyBlock();
            builtFor = null;
            return true;
        }

        /// <summary>The one-time cube mesh and additive material, made only when a map is built with the row on (never while off).</summary>
        void EnsureAssets()
        {
            if (cube == null)
            {
                var box = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube = box.GetComponent<MeshFilter>().sharedMesh;
                Destroy(box);
            }
            if (mat == null) mat = FxDraw.Additive(game.mats.neon, "Exit 63");
        }

        Renderer Box(Transform parent, string name, F.Part q)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.SetPositionAndRotation(WebSpace.ToUnity(new Vec3(q.x, q.y, q.z)), Quaternion.identity);
            go.transform.localScale = new Vector3((float)q.sx, (float)q.sy, (float)q.sz); // the z flip moves the box, never its size
            go.AddComponent<MeshFilter>().sharedMesh = cube; // no collider: a view, never a solid
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
            hidden = false;
            idleByRow = false;
            Doors = LitNow = 0;
            LastLitFrame = -1;
            LastLitDoor = LastLitRoom = "";
            string why = X.OffReason(game.cfg, Args());
            if (why != null)
            {
                idleByRow = why == X.RowOff;
                Line = "exit63 OFF (" + why + ")";
                return;
            }
            string name = game.map != null && game.map.json != null && game.map.json.TryGetValue("name", out object no) ? no as string : null;
            if (name != Level) { Line = $"exit63: not this level ({name ?? "no map"}; it is {Level}'s)"; return; }
            p = X.Params.From(game.cfg);
            if (!p.Valid) { Line = "exit63: design.n63.exit63.numbers invalid (nothing drawn)"; Debug.Log("[exit63] " + Line); return; }
            if (!game.map.json.TryGetValue("campaign", out object co) || !(co is Dictionary<string, object> camp)
                || !camp.TryGetValue("doors", out object dobj) || !(dobj is List<object> doors))
            { Line = "exit63: " + Level + " has no campaign doors"; return; }
            EnsureAssets();
            cyan = Look.C(game.cfg, "colors.usable");
            root = new GameObject("Exit 63").transform;
            DontDestroyOnLoad(root.gameObject);
            var rooms = Doors38Math.Rooms(camp);
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
                lamps.Add(MakeLamp(id, a ?? "?", F.DoorOf(rect, h, ax, az, spansX)));
            }
            Doors = lamps.Count;
            Line = $"exit63 ON on {Level}: doors {Doors}, lit 0";
            Debug.Log("[exit63] " + Line);
        }

        Lamp MakeLamp(string id, string room, F.Door door)
        {
            var t = new GameObject("exit63 " + id).transform;
            t.SetParent(root, false);
            F.Part v = X.Veil(door, p);
            Renderer veil = Box(t, "veil", v);
            var parts = new List<Renderer>();
            var gains = new List<float>();
            foreach (F.Part q in X.Liners(door, p)) { parts.Add(Box(t, "liner", q)); gains.Add(p.linerGain); }
            parts.Add(Box(t, "threshold", X.Strip(door, p)));
            gains.Add(p.stripGain);
            foreach (F.Part q in X.Dashes(door, p)) { parts.Add(Box(t, "dash", q)); gains.Add(p.dashGain); }
            var box = new Bounds(WebSpace.ToUnity(new Vec3(v.x, v.y, v.z)), new Vector3((float)v.sx, (float)v.sy, (float)v.sz));
            return new Lamp { door = id, room = room, veil = veil, veilBox = box, parts = parts.ToArray(), gains = gains.ToArray() };
        }

        void LateUpdate()
        {
            if (game == null && !Attach()) return;
            if (SurvivalGame.Instance != game) { game = null; return; }
            // the row, read EVERY frame (its config, the process's off switch, D1's reel override first: a same-run toggle flips it)
            bool rowOn = NightRows.ViewOn(game.cfg, X.Row);
            if (builtFor != game.map || (idleByRow && rowOn)) Build();
            if (lamps.Count == 0) return;
            if (!rowOn)
            {
                if (!hidden)
                {
                    hidden = true;
                    foreach (Lamp l in lamps) Hide(l);
                }
                return; // every door's state kept as it was (D1's contract): the next frame on draws what this one would have
            }
            hidden = false;
            Zones zones = game.sim.zones;
            float dt = FeelClock.UnscaledDelta; // a view's rise, in real time (a157's clear shot waits in real time); 0 in D1's held frame
            bool eyeKnown = game.cam != null;
            Vector3 eye = eyeKnown ? game.cam.transform.position : Vector3.zero;
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
                        Hide(l);
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
                    Debug.Log(X.LitLine(l.room, l.door, Time.frameCount));
                }
                lit++;
                if (!l.risen) l.t += dt;
                float k = X.Rise(l.t, p.riseS);
                if (k >= 1f) l.risen = true;
                float veilK = eyeKnown ? X.VeilK(Mathf.Sqrt(l.veilBox.SqrDistance(eye)), p) : 1f;
                Paint(l, k, veilK);
            }
            LitNow = lit;
            if (changed) Line = $"exit63 ON on {Level}: doors {Doors}, lit {lit}; last lit {(LastLitFrame >= 0 ? LastLitDoor + " (room " + LastLitRoom + ") at frame " + LastLitFrame : "none yet")}";
        }

        /// <summary>The lamp at rise level `k` with the veil at `veilK` of it; sets a renderer only when its value changed.</summary>
        void Paint(Lamp l, float k, float veilK)
        {
            if (k != l.shownK)
            {
                l.shownK = k;
                for (int i = 0; i < l.parts.Length; i++)
                {
                    Renderer r = l.parts[i];
                    bool on = k > 0f && l.gains[i] > 0f;
                    r.enabled = on;
                    if (!on) continue;
                    mpb.SetColor(BaseColor, cyan * (l.gains[i] * k));
                    r.SetPropertyBlock(mpb);
                }
            }
            float v = k * veilK;
            if (v != l.shownVeil)
            {
                l.shownVeil = v;
                bool on = v > 0f && p.veilGain > 0f;
                l.veil.enabled = on;
                if (on)
                {
                    mpb.SetColor(BaseColor, cyan * (p.veilGain * v));
                    l.veil.SetPropertyBlock(mpb);
                }
            }
        }

        static void Hide(Lamp l)
        {
            l.veil.enabled = false;
            foreach (Renderer r in l.parts) r.enabled = false;
            l.shownK = l.shownVeil = -1f;
        }

        void OnDestroy()
        {
            if (root != null) Destroy(root.gameObject);
            if (mat != null) Destroy(mat);
        }
    }
}
