// NIGHT36 builder L (NIGHT36.md L1; ULTRAKILL's arena flow, rea-ultrakill-combat.md section 4): the arena flow's picture, view only. It installs itself
// (no edit in the lead's SurvivalGame files; Tell36View's pattern) and draws from the sim's events (Game/SurvivalSim.Level36.cs), so it draws nothing
// on a level without arena36 rooms:
// - arena36Lock: the room's doors (the one behind and the one ahead) get a red-orange slab across the doorway and a red light (Door.cs: a locked door
//   turns its lights red);
// - arena36SpawnIn: a red-orange bubble round the enemy that shrinks to nothing over the event's seconds, and a point light whose range shrinks with it
//   (SpawnEffect.cs: the bubble shrinks at 2 per second, the light's range at 50 per second);
// - arena36Unlock: the door slabs go out, the door ahead first, then the one behind the event's seconds later (ActivateNextWave.cs: one every 0.1 s).
// Unity objects are made on first use (never in field initialisers: L-035).
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed class Arena36View : MonoBehaviour
    {
        /// <summary>For the reels: bubbles and door slabs drawn this frame; counts of what was seen this session.</summary>
        public static int BubblesShown, DoorsShown, LocksSeen, SpawnInsSeen, UnlocksSeen;

        sealed class Bubble
        {
            public Vector3 at;
            public float age, life;
            public Transform t;
            public Renderer r;
            public Light light;
        }

        sealed class Door
        {
            public Transform t;
            public Renderer r;
            public Light light;
            public float offIn = -1; // seconds until it goes out (-1: lit)
            public bool lit;
        }

        SurvivalGame game;
        Transform root;
        Material mat;
        Mesh sphere, cube;
        MaterialPropertyBlock mpb;
        readonly List<System.Action> offs = new List<System.Action>();
        readonly List<Bubble> bubbles = new List<Bubble>();
        readonly List<Bubble> bubblePool = new List<Bubble>();
        readonly List<Door> doors = new List<Door>();
        static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Install()
        {
            if (!Application.isPlaying) return;
            var go = new GameObject("Arena36View") { hideFlags = HideFlags.HideAndDontSave };
            DontDestroyOnLoad(go);
            go.AddComponent<Arena36View>();
        }

        bool Attach()
        {
            game = FindFirstObjectByType<SurvivalGame>();
            if (game == null || game.mats == null || game.events == null || game.sim == null) { game = null; return false; }
            foreach (System.Action o in offs) o();
            offs.Clear();
            if (root != null) Destroy(root.gameObject);
            bubbles.Clear();
            bubblePool.Clear();
            doors.Clear();
            root = new GameObject("Arena 36").transform;
            DontDestroyOnLoad(root.gameObject);
            if (mat != null) Destroy(mat);
            mat = FxDraw.Additive(game.mats.neon, "Arena 36");
            mpb ??= new MaterialPropertyBlock();
            if (sphere == null || cube == null)
            {
                var ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere = ball.GetComponent<MeshFilter>().sharedMesh;
                Destroy(ball);
                var box = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube = box.GetComponent<MeshFilter>().sharedMesh;
                Destroy(box);
            }
            offs.Add(game.events.On<Arena36Event>("arena36Lock", OnLock));
            offs.Add(game.events.On<Arena36Event>("arena36SpawnIn", OnSpawnIn));
            offs.Add(game.events.On<Arena36Event>("arena36Unlock", OnUnlock));
            return true;
        }

        Renderer NewPart(string name, Mesh mesh)
        {
            var go = new GameObject(name);
            go.transform.SetParent(root, false);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var r = go.AddComponent<MeshRenderer>();
            r.sharedMaterial = mat;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            r.receiveShadows = false;
            r.enabled = false;
            return r;
        }

        Light NewLight(Transform parent)
        {
            var go = new GameObject("light");
            go.transform.SetParent(parent, false);
            var l = go.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = Look.Hex(UnityTuning.Arena36RedHex);
            l.shadows = LightShadows.None;
            l.enabled = false;
            return l;
        }

        void Paint(Renderer r, Color c)
        {
            mpb.SetColor(BaseColor, c);
            r.SetPropertyBlock(mpb);
        }

        /// <summary>The map JSON's campaign door rows (rect, height) by index.</summary>
        List<Dictionary<string, object>> DoorRows()
        {
            var res = new List<Dictionary<string, object>>();
            if (game.map != null && game.map.json != null && game.map.json.TryGetValue("campaign", out object co) && co is Dictionary<string, object> camp && camp.TryGetValue("doors", out object d) && d is List<object> dl)
                foreach (object o in dl) res.Add(o as Dictionary<string, object>);
            return res;
        }

        void OnLock(Arena36Event e)
        {
            if (game == null || e == null) return;
            LocksSeen++;
            foreach (Door d in doors) { d.lit = false; d.offIn = -1; }
            List<Dictionary<string, object>> rows = DoorRows();
            var rooms = game.map != null && game.map.json != null && game.map.json.TryGetValue("campaign", out object cmo) ? Doors38Math.Rooms(cmo as Dictionary<string, object>) : null; // NIGHT38 M: the slab's plane
            int k = 0;
            foreach (int di in new[] { e.room - 1, e.room }) // the door behind (doors[i - 1] joins rooms i - 1 and i) and the door ahead
            {
                if (di < 0 || di >= rows.Count || rows[di] == null || !rows[di].TryGetValue("rect", out object ro) || !(ro is List<object> rc) || rc.Count < 4) continue;
                double x0 = PortData.Num(rc[0]), z0 = PortData.Num(rc[1]), x1 = PortData.Num(rc[2]), z1 = PortData.Num(rc[3]);
                double h = rows[di].TryGetValue("height", out object ho) ? PortData.Num(ho) : 4;
                if (k >= doors.Count)
                {
                    var nd = new Door { r = NewPart("door", cube) };
                    nd.t = nd.r.transform;
                    nd.light = NewLight(nd.t.parent);
                    doors.Add(nd);
                }
                Door door = doors[k++];
                // a slab across the doorway at the gap's middle, the full opening, 0.1 m thick. NIGHT38 M (the orientation bug: the slab was always
                // x across, so a door in an x wall, its passage along x, got a slab along the passage): across the passage, which runs from the
                // door's first room to its second (Doors38Math.SpansX; door rects are often square, so their sides cannot tell)
                double[] rr = { x0, z0, x1, z1 };
                double[] ra = null, rb = null;
                if (rooms != null && rows[di].TryGetValue("between", out object bo) && bo is List<object> bl && bl.Count >= 2)
                {
                    if (bl[0] is string a && rooms.TryGetValue(a, out var rowA)) ra = rowA.rect;
                    if (bl[1] is string b && rooms.TryGetValue(b, out var rowB)) rb = rowB.rect;
                }
                bool spansX = Doors38Math.SpansX(rr, ra, rb);
                Vector3 c = WebSpace.ToUnity(new Vec3((x0 + x1) / 2, h / 2, (z0 + z1) / 2));
                door.t.position = c;
                door.t.localScale = spansX ? new Vector3((float)System.Math.Abs(x1 - x0), (float)h, 0.1f) : new Vector3(0.1f, (float)h, (float)System.Math.Abs(z1 - z0));
                door.light.transform.position = c;
                door.lit = true;
                door.offIn = -1;
            }
        }

        void OnUnlock(Arena36Event e)
        {
            if (game == null || e == null) return;
            UnlocksSeen++;
            // the door ahead (the second lit) goes out first, the one behind e.seconds later
            float t = 0;
            for (int i = doors.Count - 1; i >= 0; i--)
                if (doors[i].lit) { doors[i].offIn = t; t += (float)e.seconds; }
        }

        void OnSpawnIn(Arena36Event e)
        {
            if (game == null || e == null) return;
            SpawnInsSeen++;
            Bubble b;
            if (bubblePool.Count > 0)
            {
                b = bubblePool[bubblePool.Count - 1];
                bubblePool.RemoveAt(bubblePool.Count - 1);
            }
            else
            {
                b = new Bubble { r = NewPart("spawn bubble", sphere) };
                b.t = b.r.transform;
                b.light = NewLight(root);
            }
            b.at = WebSpace.ToUnity(new Vec3(e.x, e.y + 0.9, e.z));
            b.age = 0;
            b.life = Mathf.Max(0.05f, (float)e.seconds);
            bubbles.Add(b);
        }

        void LateUpdate()
        {
            if (game == null && !Attach()) return;
            float dt = Time.deltaTime;
            Color red = Look.Hex(UnityTuning.Arena36RedHex) * UnityTuning.Arena36Glow;
            BubblesShown = 0;
            for (int i = bubbles.Count - 1; i >= 0; i--)
            {
                Bubble b = bubbles[i];
                b.age += dt;
                float u = b.age / b.life;
                if (u >= 1f)
                {
                    b.r.enabled = false;
                    b.light.enabled = false;
                    bubblePool.Add(b);
                    bubbles.RemoveAt(i);
                    continue;
                }
                float k = 1f - u; // shrinks linearly, as ULTRAKILL's (a constant rate)
                b.t.position = b.at;
                b.t.localScale = Vector3.one * (UnityTuning.Arena36BubbleR * 2f * k);
                Paint(b.r, red * (UnityTuning.Arena36BubbleAlpha * k));
                b.r.enabled = true;
                b.light.transform.position = b.at;
                b.light.range = UnityTuning.Arena36LightRange * k;
                b.light.intensity = UnityTuning.Arena36LightIntensity;
                b.light.enabled = true;
                BubblesShown++;
            }
            DoorsShown = 0;
            foreach (Door d in doors)
            {
                if (d.lit && d.offIn >= 0)
                {
                    d.offIn -= dt;
                    if (d.offIn <= 0) { d.lit = false; d.offIn = -1; }
                }
                d.r.enabled = d.lit && !Doors38View.On; // NIGHT38 M: with doors38 on, its red-orange curtain is the locked door's sheet (one sheet, not two)
                d.light.enabled = d.lit;
                if (!d.lit) continue;
                Paint(d.r, red * UnityTuning.Arena36DoorAlpha);
                d.light.range = UnityTuning.Arena36DoorLightRange;
                d.light.intensity = UnityTuning.Arena36DoorLightIntensity;
                DoorsShown++;
            }
        }
    }
}
