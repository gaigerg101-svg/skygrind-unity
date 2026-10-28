// NIGHT63 builder G (leftover 6 = NIGHT61's S7: kiln's presses read as machines; design.n63.press63, a VIEW row): the drawing half
// (Press63Math.cs holds the pure geometry, the colours by part and the head's clock; N63PressTests). Self-installed like Fixture61 (no hook in
// another file): on a map whose campaign boxes name drop presses (job "hero prop: a drop press": n57-kiln's THE PRESS HALL, four), the first
// frame the row is on it builds, under its own root "Press machines 63": the static parts (bed, throat, guide columns, crown, stack, the lamp's
// housing) as one mesh per colour; per press a ram head (head + die) moved every frame to Press63Math.HeadY on the FeelClock, its rods stretched
// from the head's top to the crown, and a lens lit while the head waits up top. S7b: every part is drawn UNLIT (an opaque copy of the neon
// material per colour, FxDraw.Opaque), so what shows is the steel value set in UnityTuning.N63GS7.cs: S7's lit stone parts took the hall's
// warm light (the side faces tan like the walls, the chrome strip a dull lavender, Logs/n63/verdict-S7.md). No shadow, no collider, no sim
// box, no nav: the presses' own boxes stay the level's. THE SWITCH is read every frame (D1's same-run toggle): off (the row off,
// -skygrindOff63 or older, -skygrindNoPress63) hides the root, and nothing is built until the row is first on. Proof (L-089): one Player.log
// line per map built, "[press63] presses on <level>: ..."; Press63.Line for a reel. No audio, no gameplay number, no sim row.
using System;
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed class Press63 : MonoBehaviour
    {
        /// <summary>For a reel: the presses' state on the current map.</summary>
        public static string Line = "presses: not attached";
        /// <summary>For a reel: presses dressed on the current map, heads moved this frame, lamps lit this frame.</summary>
        public static int Dressed, HeadsMoved, LampsLit;

        static string[] args;

        static string[] Args()
        {
            if (args == null)
            {
                try { args = Environment.GetCommandLineArgs() ?? new string[0]; }
                catch (Exception) { args = new string[0]; }
            }
            return args;
        }

        sealed class One
        {
            public Transform head, rods;
            public Renderer lens;
            public double low, high, phase, crownBottom;
            public bool lit, litSet;
        }

        SurvivalGame game;
        SurvivalMap builtFor;
        bool tried;
        Transform root;
        readonly List<One> presses = new List<One>();
        readonly Dictionary<uint, Material> mats = new Dictionary<uint, Material>();
        Material lensMat;
        readonly MaterialPropertyBlock block = new MaterialPropertyBlock();
        static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Install()
        {
            if (!Application.isPlaying) return;
            var go = new GameObject("Press63") { hideFlags = HideFlags.HideAndDontSave };
            DontDestroyOnLoad(go);
            go.AddComponent<Press63>();
        }

        bool Attach()
        {
            game = SurvivalGame.Instance;
            if (game == null || game.mats == null || game.mats.neon == null || game.cfg == null) { game = null; return false; }
            builtFor = null;
            tried = false;
            return true;
        }

        void Drop()
        {
            if (root != null) Destroy(root.gameObject);
            root = null;
            presses.Clear();
            foreach (Material m in mats.Values) if (m != null) Destroy(m);
            mats.Clear();
            if (lensMat != null) Destroy(lensMat);
            lensMat = null;
            Dressed = 0;
        }

        void LateUpdate()
        {
            if (game == null && !Attach()) return;
            if (SurvivalGame.Instance != game) { game = null; Drop(); return; }
            bool on = Press63Math.OffReason(game.cfg, Args()) == null; // every frame: never cached (D1's same-run toggle flips ViewOn)
            if (builtFor != game.map) { Drop(); builtFor = game.map; tried = false; }
            if (on && !tried) { tried = true; Build(); }
            if (root != null && root.gameObject.activeSelf != on) root.gameObject.SetActive(on);
            HeadsMoved = LampsLit = 0;
            if (!on || root == null) return;
            double t = FeelClock.Now;
            foreach (One p in presses)
            {
                double y = Press63Math.HeadY(t, p.phase, p.low, p.high);
                var at = new Vector3(0f, (float)y, 0f);
                if (p.head.localPosition != at) p.head.localPosition = at;
                // the rods: built 1 m tall from y 0, stretched from the head's top to the crown's bottom
                double top = y + UnityTuning.Press63HeadH;
                var rodAt = new Vector3(0f, (float)top, 0f);
                var rodScale = new Vector3(1f, (float)Math.Max(1e-3, p.crownBottom - top), 1f);
                if (p.rods.localPosition != rodAt) p.rods.localPosition = rodAt;
                if (p.rods.localScale != rodScale) p.rods.localScale = rodScale;
                HeadsMoved++;
                bool lit = Press63Math.LampLit(t, p.phase);
                if (lit) LampsLit++;
                if (!p.litSet || lit != p.lit)
                {
                    p.lit = lit;
                    p.litSet = true;
                    block.SetColor(BaseColor, Look.Hex(lit ? UnityTuning.Press63LensOnHex : UnityTuning.Press63LensOffHex));
                    p.lens.SetPropertyBlock(block);
                }
            }
        }

        Material Mat(uint hex)
        {
            if (!mats.TryGetValue(hex, out Material m) || m == null)
            {
                m = FxDraw.Opaque(game.mats.neon, "Press63 " + hex.ToString("X6"), Look.Hex(hex));
                mats[hex] = m;
            }
            return m;
        }

        /// <summary>A box's six faces, each into the kit of its colour (the faces along z take the tone's front, along x its side).</summary>
        static void Faces(Dictionary<uint, MeshKit> kits, Vec3 min, Vec3 max, Press63Math.Tone tone, bool skipBottom)
        {
            Vector3 a = WebSpace.ToUnity(min), b = WebSpace.ToUnity(max);
            Vector3 lo = Vector3.Min(a, b), hi = Vector3.Max(a, b);
            Vector3 P(int x, int y, int z) => new Vector3(x == 0 ? lo.x : hi.x, y == 0 ? lo.y : hi.y, z == 0 ? lo.z : hi.z);
            MeshKit K(uint hex)
            {
                if (!kits.TryGetValue(hex, out MeshKit k)) kits[hex] = k = new MeshKit();
                return k;
            }
            K(tone.top).Quad(P(0, 1, 0), P(1, 1, 0), P(1, 1, 1), P(0, 1, 1), Vector3.up, Color.white);
            if (!skipBottom) K(tone.bottom).Quad(P(0, 0, 0), P(0, 0, 1), P(1, 0, 1), P(1, 0, 0), Vector3.down, Color.white);
            K(tone.front).Quad(P(0, 0, 0), P(1, 0, 0), P(1, 1, 0), P(0, 1, 0), Vector3.back, Color.white);
            K(tone.front).Quad(P(0, 0, 1), P(0, 1, 1), P(1, 1, 1), P(1, 0, 1), Vector3.forward, Color.white);
            K(tone.side).Quad(P(0, 0, 0), P(0, 1, 0), P(0, 1, 1), P(0, 0, 1), Vector3.left, Color.white);
            K(tone.side).Quad(P(1, 0, 0), P(1, 0, 1), P(1, 1, 1), P(1, 1, 0), Vector3.right, Color.white);
        }

        /// <summary>One child object per colour under `parent`, each on its own unlit material.</summary>
        void Objects(string name, Transform parent, Dictionary<uint, MeshKit> kits)
        {
            foreach (KeyValuePair<uint, MeshKit> kv in kits)
                MeshKit.Object(name + " " + kv.Key.ToString("X6"), parent, kv.Value.Build(name + " " + kv.Key.ToString("X6")), Mat(kv.Key), shadows: false);
        }

        void Build()
        {
            Dictionary<string, object> json = game.map != null ? game.map.json : null;
            string name = json != null && json.TryGetValue("name", out object no) ? no as string : null;
            var boxes = new List<(Vec3 min, Vec3 max)>();
            if (json != null && json.TryGetValue("campaign", out object co) && co is Dictionary<string, object> camp && camp.TryGetValue("boxes", out object bo) && bo is List<object> list)
                foreach (object o in list)
                {
                    if (!(o is Dictionary<string, object> b) || !b.TryGetValue("job", out object jo) || !Press63Math.IsPress(jo as string) || !b.ContainsKey("min") || !b.ContainsKey("max")) continue;
                    List<object> mn = PortData.Arr(b["min"]), mx = PortData.Arr(b["max"]);
                    var lo = new Vec3(PortData.Num(mn[0]), PortData.Num(mn[1]), PortData.Num(mn[2]));
                    var hi = new Vec3(PortData.Num(mx[0]), PortData.Num(mx[1]), PortData.Num(mx[2]));
                    if (hi.x - lo.x <= 0 || hi.y - lo.y <= 0 || hi.z - lo.z <= 0) continue;
                    boxes.Add((lo, hi));
                }
            if (boxes.Count == 0) { Line = $"[press63] presses on {name ?? "no map"}: none (no campaign box with job '{UnityTuning.Press63Job}')"; return; }
            double midX = 0;
            foreach (var b in boxes) midX += (b.min.x + b.max.x) / 2;
            midX /= boxes.Count; // the hall's middle between its presses: each lamp faces it
            root = new GameObject("Press machines 63").transform;
            DontDestroyOnLoad(root.gameObject);
            var statics = new Dictionary<uint, MeshKit>();
            int parts = 0;
            lensMat = FxDraw.Opaque(game.mats.neon, "Press63 lens", Look.Hex(UnityTuning.Press63LensOffHex));
            for (int i = 0; i < boxes.Count; i++)
            {
                (Vec3 lo, Vec3 hi) = boxes[i];
                bool lampOnMinX = (lo.x + hi.x) / 2 > midX;
                var lensKit = new MeshKit();
                foreach (Press63Math.Piece pc in Press63Math.Pieces(lo, hi, lampOnMinX))
                {
                    if (pc.part == Press63Math.Part.Lens) { lensKit.Box(pc.min, pc.max, Color.white, Color.white); continue; } // its own object: lit on its clock
                    Faces(statics, pc.min, pc.max, Press63Math.ToneOf(pc.part), skipBottom: pc.part == Press63Math.Part.Bed);
                    parts++;
                }
                // the head: head + die built at bottom 0, moved up and down by its transform
                var head = new GameObject("press63 head " + i).transform;
                head.SetParent(root, false);
                var headKits = new Dictionary<uint, MeshKit>();
                Faces(headKits, Press63Math.Head(lo, hi, 0).min, Press63Math.Head(lo, hi, 0).max, Press63Math.ToneOf(Press63Math.Part.Head), false);
                Faces(headKits, Press63Math.Die(lo, hi, 0).min, Press63Math.Die(lo, hi, 0).max, Press63Math.ToneOf(Press63Math.Part.Die), false);
                Objects("press63 head " + i, head, headKits);
                // the rods: built from y 0 to 1, placed at the head's top and stretched to the crown each frame
                var rods = new GameObject("press63 rods " + i).transform;
                rods.SetParent(root, false);
                var rodKits = new Dictionary<uint, MeshKit>();
                foreach (Press63Math.Piece r in Press63Math.Rods(lo, hi, 0, 1)) Faces(rodKits, r.min, r.max, Press63Math.ToneOf(Press63Math.Part.Rod), false);
                Objects("press63 rods " + i, rods, rodKits);
                GameObject lg = MeshKit.Object("press63 lens " + i, root, lensKit.Build("press63 lens " + i), lensMat, shadows: false);
                presses.Add(new One
                {
                    head = head, rods = rods, lens = lg.GetComponent<Renderer>(),
                    low = Press63Math.Low(lo), high = Press63Math.High(lo, hi), phase = Press63Math.Phase(i), crownBottom = Press63Math.CrownBottom(lo, hi),
                });
            }
            Objects("press63 frames", root, statics);
            Dressed = presses.Count;
            Line = $"[press63] presses on {name ?? "?"}: {presses.Count} dressed (campaign boxes with job '{UnityTuning.Press63Job}'), parts {parts}, colours {statics.Count} (unlit), heads {presses.Count} on the FeelClock | their boxes, colliders and nav untouched";
            Debug.Log(Line);
        }

        void OnDestroy() => Drop();
    }
}
