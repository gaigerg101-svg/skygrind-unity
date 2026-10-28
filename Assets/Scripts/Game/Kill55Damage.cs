// NIGHT55 K kill55 (3): a hit husk shows its damage (design.n55.kill55; the veteran critic's #2: "the husk under the crosshair at point-blank
// range is an intact glossy pill after being shot: no crack, no missing piece"; DOOM Eternal's destructible demons, web.md section 3.2). A view
// on the EXISTING model (GG owns enemy design: CLAUDE.md rule 7): where a hit of the player's lands on a husk that lives, a mark is laid on its
// shell: above UnityTuning.Kill55HoleAt of its health a CRACK (a burnt scorch and jagged strips, a hot core cooling), at or under it a HOLE (a
// piece knocked off: a near-black recess with a hot rim, and one shard thrown by Kill55Debris). Up to Kill55MarksPerHusk a husk; the kill
// breaks the body (the chunks) and its marks go with it. The marks are flat decal meshes curved to the shell, drawn in the husk's own root
// space (they follow its lean, kick, tumble and squash), on EnemyLook's SKYGRIND/Enemy body material (unlit, the proven draw path), colours in
// the enemy red-orange family (rule 8). The husk's root is found as HuskView lays them out: its pool's n-th body is the n-th live husk of
// game.enemies.all (a child of "Husks"), checked against the husk's drawn position. Pictures only: no Core state.
using System;
using Skygrind.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace Skygrind.Game
{
    public sealed class Kill55Damage
    {
        struct Mark
        {
            public int enemy;
            public Vector3 dir; // root space, level, unit: the side of the shell it sits on
            public float y; // root space height (m)
            public float age, spin;
            public int stage; // 1 crack, 2 hole
            public bool alive;
            public Vector3 world; // NIGHT58 G (G1): where it was last drawn (world space; the a258 probe)
        }

        /// <summary>NIGHT58 G (G1, the a258 probe): the victim's newest live mark, where it was last drawn, its stage; false if none.</summary>
        public bool MarkAt(int enemy, out Vector3 world, out int stage)
        {
            world = default;
            stage = 0;
            float best = float.MaxValue;
            for (int i = 0; i < marks.Length; i++)
            {
                if (!marks[i].alive || marks[i].enemy != enemy || marks[i].age >= best) continue;
                best = marks[i].age;
                world = marks[i].world;
                stage = marks[i].stage;
            }
            return stage > 0;
        }

        readonly SurvivalGame game;
        readonly Mark[] marks = new Mark[UnityTuning.Kill55MarkPool];
        // NIGHT57 G (G3): the marks' colours on their OWN material copies (EnemyLook's body material, copied once), not property blocks: n56-r5's
        // a355 hit-after frame shows the hole drawn in a dark red near the body's own hue, never the near-black #070303 / #160807 its block set
        // (my read of the before / after crops: the jagged hole and its strips are there, in red), and 0.04 % of the body's pixels went dark. A
        // copy carries its colour whatever the draw path does with a block; its ink pass (UniversalForward, the inverted hull) is off: a decal is
        // not a silhouette. The hot core / rim cools over Kill55MarkHotSteps copies (hot -> ember), picked by the mark's age.
        readonly Mesh crackMesh, coreMesh, holeMesh, rimMesh;
        readonly float height, radius;
        Material mat;
        RenderParams rpDark, rpHole;
        RenderParams[] rpHot;
        Transform husksRoot;
        int next;
        uint hash = 0x2C1B3C6Du;
        static readonly int BaseColor = Shader.PropertyToID("_BaseColor"), FlashId = Shader.PropertyToID("_Flash"), RimOnId = Shader.PropertyToID("_RimOn"), DissolveId = Shader.PropertyToID("_Dissolve");

        /// <summary>The reel's probe: marks laid, holes among them, marks drawn last frame.</summary>
        public int Laid, Holes, DrawnLastFrame;

        public Kill55Damage(SurvivalGame game)
        {
            this.game = game;
            height = (float)game.cfg.Num("husk.height");
            radius = (float)game.cfg.Num("husk.radius");
            crackMesh = BuildCrack(UnityTuning.Kill55ScorchR, UnityTuning.Kill55CrackLen, UnityTuning.Kill55CrackWidth, UnityTuning.Kill55CrackStrips, 0x1234u);
            coreMesh = BuildCrack(UnityTuning.Kill55ScorchR * 0.35f, UnityTuning.Kill55CrackLen * 0.55f, UnityTuning.Kill55CrackWidth * 0.4f, UnityTuning.Kill55CrackStrips, 0x1234u);
            holeMesh = BuildDisc(UnityTuning.Kill55HoleR, 0f, 9, 0x77u);
            rimMesh = BuildDisc(UnityTuning.Kill55HoleR + UnityTuning.Kill55RimWidth, UnityTuning.Kill55HoleR - 0.005f, 9, 0x77u);
        }

        float H()
        {
            hash ^= hash << 13;
            hash ^= hash >> 17;
            hash ^= hash << 5;
            return (hash & 0xffffff) / (float)0x1000000;
        }

        bool Ready()
        {
            if (mat != null) return true;
            EnemyLook look = EnemyLook.For(game);
            if (look == null || !look.ok || look.body == null) return false;
            mat = look.body;
            // NIGHT59 G (G1): the marks' own shader (SKYGRIND/Kill59Mark: the depth pull, no fog), unless -skygrindOldMarks59 or the shader is
            // missing from the build (then NIGHT58's copies of the body material, as before)
            Shader s59 = OldMarks59 ? null : Shader.Find("SKYGRIND/Kill59Mark");
            Func<Color, RenderParams> make = s59 != null ? c => Params59(s59, c) : c => Params57(look.body, c);
            Mark59 = s59 != null;
            rpDark = make(Look.Hex(UnityTuning.Kill55CrackHex).linear);
            rpHole = make(Look.Hex(UnityTuning.Kill55HoleHex).linear);
            rpHot = new RenderParams[Mathf.Max(2, UnityTuning.Kill55MarkHotSteps)];
            Color hc = Look.Hex(UnityTuning.Kill55MarkHotHex).linear, ec = Look.Hex(UnityTuning.Kill55MarkEmberHex).linear;
            for (int k = 0; k < rpHot.Length; k++) rpHot[k] = make(Color.Lerp(hc, ec, k / (float)(rpHot.Length - 1)));
            return true;
        }

        /// <summary>NIGHT59 G (G1): `-skygrindOldMarks59` keeps NIGHT58's draw (copies of the body material) with kill55 on: the same build's A / B.</summary>
        static bool OldMarks59 => oldMarks59 ??= Array.IndexOf(Environment.GetCommandLineArgs(), "-skygrindOldMarks59") >= 0;
        static bool? oldMarks59;

        /// <summary>NIGHT59 G (G1, the reel's input line): the marks are drawn by SKYGRIND/Kill59Mark (false: NIGHT58's draw or not ready yet).</summary>
        public bool Mark59;

        /// <summary>NIGHT60 V (B2, the reel's input line): the render queue the hole draws at (-1 before the first mark is drawn).</summary>
        public int MarkQueue => rpHole.material != null ? rpHole.material.renderQueue : -1;
        static readonly int Pull59Id = Shader.PropertyToID("_Pull59");

        /// <summary>NIGHT59 G (G1): one mark colour on SKYGRIND/Kill59Mark (unlit, no fog, pulled toward the camera along its view ray). Public for
        /// NIGHT60 V's N60MarkTests (the draw order against the enemy x-ray).</summary>
        public static RenderParams Params59(Shader shader, Color linear)
        {
            // NIGHT60 V (B2): after the enemy x-ray (its ZTest Greater pass tints whatever stands in front of the body, the marks included)
            var m = new Material(shader) { name = "kill59 mark", renderQueue = (int)RenderQueue.Transparent + UnityTuning.EnemyXrayQueueOffset + UnityTuning.Kill60MarkQueueAfterXray };
            linear.a = 1;
            m.SetVector(BaseColor, linear);
            m.SetVector(Pull59Id, new Vector4(UnityTuning.Kill59MarkPull, UnityTuning.Kill59MarkNearest, UnityTuning.Kill59MarkFacing, 0));
            return new RenderParams(m) { layer = 0, shadowCastingMode = ShadowCastingMode.Off, receiveShadows = false };
        }

        /// <summary>NIGHT57 G (G3): one mark colour's own copy of the body material (unlit colour, no flash, no rim, whole, no ink pass).</summary>
        static RenderParams Params57(Material body, Color linear)
        {
            var m = new Material(body) { name = "kill55 mark" };
            linear.a = 1;
            m.SetVector(BaseColor, linear);
            m.SetFloat(FlashId, 0);
            m.SetFloat(RimOnId, 0);
            m.SetFloat(DissolveId, 1);
            m.SetShaderPassEnabled("UniversalForward", false); // the ink hull (NIGHT51): a decal is not a silhouette
            return new RenderParams(m) { layer = 0, shadowCastingMode = ShadowCastingMode.Off, receiveShadows = false };
        }

        public void Clear()
        {
            for (int i = 0; i < marks.Length; i++) marks[i].alive = false;
        }

        /// <summary>A hit of the player's (the bus's `hit`, inside the step): a mark where it landed on a husk that lives.</summary>
        public void OnHit(HitEvent e, Kill55Debris debris)
        {
            if (e == null || e.owner != "player" || e.killed || e.shock) return;
            Husk h = FindHusk(e.enemy);
            if (h == null || !h.alive || h.maxHp <= 0) return;
            float share = (float)(h.hp / h.maxHp);
            int stage = share <= UnityTuning.Kill55HoleAt ? 2 : 1;
            Quaternion yaw = Quaternion.Euler(0, -(float)(h.yaw * Mathf.Rad2Deg), 0);
            Vector3 feet = WebSpace.ToUnity(h.position);
            Vector3 local = Quaternion.Inverse(yaw) * (WebSpace.ToUnity(e.position) - feet);
            var lv = new Vector3(local.x, 0, local.z);
            if (lv.magnitude < radius * 0.5f)
            {
                // the hit point sits on the body's axis (an aim point): the mark goes on the side facing the player
                Vector3 me = Quaternion.Inverse(yaw) * (WebSpace.ToUnity(game.Eye) - feet);
                lv = new Vector3(me.x, 0, me.z);
            }
            if (lv.sqrMagnitude < 1e-6f) lv = Vector3.forward;
            lv.Normalize();
            float y = Mathf.Clamp(local.y, height * UnityTuning.Kill55MarkLow, height * UnityTuning.Kill55MarkHigh);
            // a hit near an old mark grows it; else a new one (a husk's oldest replaced past its count)
            int count = 0, oldest = -1, near = -1;
            float oldestAge = -1;
            for (int i = 0; i < marks.Length; i++)
            {
                if (!marks[i].alive || marks[i].enemy != e.enemy) continue;
                count++;
                if (marks[i].age > oldestAge) { oldestAge = marks[i].age; oldest = i; }
                Vector3 at = marks[i].dir * radius + Vector3.up * marks[i].y;
                if ((at - (lv * radius + Vector3.up * y)).magnitude <= UnityTuning.Kill55MarkMerge) near = i;
            }
            int slot;
            if (near >= 0) slot = near;
            else if (count >= UnityTuning.Kill55MarksPerHusk) slot = oldest;
            else
            {
                slot = next;
                for (int k = 0; k < marks.Length; k++)
                {
                    int i = (next + k) % marks.Length;
                    if (!marks[i].alive) { slot = i; break; }
                }
                next = (slot + 1) % marks.Length;
            }
            ref Mark m = ref marks[slot];
            bool grew = m.alive && m.enemy == e.enemy && stage > m.stage;
            if (!(m.alive && m.enemy == e.enemy) || near < 0) { m.dir = lv; m.y = y; m.spin = H() * 360; }
            m.stage = Math.Max(stage, m.alive && m.enemy == e.enemy ? m.stage : 0);
            m.enemy = e.enemy;
            m.age = 0;
            m.alive = true;
            Laid++;
            if (m.stage == 2 && (grew || near < 0 || stage == 2))
            {
                Holes++;
                // the piece knocked off: one shard out of the hole, away from the gun
                Vector3 world = feet + yaw * (m.dir * radius + Vector3.up * m.y);
                Vector3 shot = (world - WebSpace.ToUnity(game.Eye)).normalized;
                debris?.Burst(world, shot, feet.y, Look.C(game.cfg, "colors.enemy"), 1, 1, -1);
            }
        }

        /// <summary>The kill: the body breaks (the chunks), its marks go.</summary>
        public void OnKill(int enemy)
        {
            for (int i = 0; i < marks.Length; i++) if (marks[i].alive && marks[i].enemy == enemy) marks[i].alive = false;
        }

        Husk FindHusk(int id)
        {
            if (game.enemies == null) return null;
            foreach (Enemy en in game.enemies.all) if (en.id == id) return en as Husk;
            return null;
        }

        /// <summary>The marks' radius at root-space height `y` (m): models43's pill (Mesh43.PillMesh_Profile) or the capsule primitive.</summary>
        float ShellR(float y)
        {
            float yb = Mathf.Clamp((y - height * 0.5f) / (height * 0.5f), -1, 1), a = Mathf.Abs(yb);
            float cap = a > 0.5f ? Mathf.Sqrt(Mathf.Max(0, 0.25f - (a - 0.5f) * (a - 0.5f))) : 0.5f;
            if (!Models43.On(game.cfg)) return cap * radius * 2;
            float w = (yb + 0.05f) / 0.26f;
            float waist = 1 - 0.14f * Mathf.Exp(-w * w);
            float hips = yb < -0.2f ? 0.95f + 0.05f * Mathf.Clamp01((-0.2f - yb) / 0.4f) : 1;
            return cap * waist * hips * radius * 2;
        }

        /// <summary>Once a rendered frame (after HuskView drew): the marks on the husks' roots.</summary>
        public void Render(float wdt)
        {
            DrawnLastFrame = 0;
            bool any = false;
            for (int i = 0; i < marks.Length && !any; i++) any = marks[i].alive;
            if (!any || !Ready()) return;
            if (husksRoot == null)
            {
                GameObject go = GameObject.Find("Husks");
                if (go == null) return;
                husksRoot = go.transform;
            }
            int n = 0;
            foreach (Enemy en in game.enemies.all)
            {
                if (!(en is Husk husk) || husk.kind != "husk" || !husk.alive) continue;
                int child = n++;
                if (child >= husksRoot.childCount) break;
                Transform root = husksRoot.GetChild(child);
                if (!root.gameObject.activeInHierarchy) continue;
                bool has = false;
                for (int i = 0; i < marks.Length && !has; i++) has = marks[i].alive && marks[i].enemy == husk.id;
                if (!has) continue;
                Vector3 drawnFeet = WebSpace.ToUnity(husk.previous + (husk.position - husk.previous) * game.Alpha);
                if ((root.position - drawnFeet).sqrMagnitude > 2.25f) continue; // not this husk's body (the layout moved): no mark this frame
                float scale = root.lossyScale.y;
                for (int i = 0; i < marks.Length; i++)
                {
                    ref Mark m = ref marks[i];
                    if (!m.alive || m.enemy != husk.id) continue;
                    m.age += wdt;
                    float r = ShellR(m.y);
                    Vector3 nrm = root.TransformDirection(m.dir).normalized;
                    Quaternion rot = Quaternion.LookRotation(nrm, root.up) * Quaternion.Euler(0, 0, m.spin);
                    Vector3 basePos = root.TransformPoint(m.dir * (r + UnityTuning.Kill55MarkLift) + Vector3.up * m.y);
                    Vector3 s = Vector3.one * scale;
                    float cool = Mathf.Clamp01(m.age / UnityTuning.Kill55MarkCoolS);
                    int hk = Mathf.Clamp(Mathf.RoundToInt(cool * (rpHot.Length - 1)), 0, rpHot.Length - 1); // NIGHT57 G (G3): the cooling step's copy
                    m.world = basePos; // NIGHT58 G (G1): the a258 probe
                    var bounds = new Bounds(basePos, Vector3.one * 0.8f);
                    Draw(m.stage == 2 ? rpHole : rpDark, m.stage == 2 ? holeMesh : crackMesh, basePos, rot, s, bounds);
                    if (m.stage == 2) Draw(rpDark, crackMesh, basePos - nrm * 0.002f, rot * Quaternion.Euler(0, 0, 30), s, bounds);
                    Draw(rpHot[hk], m.stage == 2 ? rimMesh : coreMesh, basePos + nrm * UnityTuning.Kill55MarkHotLift, rot, s, bounds);
                    DrawnLastFrame++;
                }
            }
        }

        static void Draw(RenderParams rp, Mesh mesh, Vector3 p, Quaternion r, Vector3 s, Bounds bounds)
        {
            rp.worldBounds = bounds; // (a copy: the struct is passed by value)
            Graphics.RenderMesh(rp, mesh, 0, Matrix4x4.TRS(p, r, s));
        }

        // --- the decal meshes: flat shapes in the XY plane, bent onto a cylinder of Kill55MarkCurve (z back from the tangent plane), both faces ---

        static Vector3 Bend(float x, float y)
        {
            float R = UnityTuning.Kill55MarkCurve, a = x / R;
            return new Vector3(R * Mathf.Sin(a), y, R * Mathf.Cos(a) - R);
        }

        /// <summary>A scorch (a jagged disc of radius `r`) and `strips` jagged cracks out to `len`, `w` wide at the root.</summary>
        static Mesh BuildCrack(float r, float len, float w, int strips, uint seed)
        {
            var v = new System.Collections.Generic.List<Vector3>();
            var t = new System.Collections.Generic.List<int>();
            uint h = seed;
            float Rn() { h ^= h << 13; h ^= h >> 17; h ^= h << 5; return (h & 0xffff) / 65536f; }
            const int sides = 8;
            int c0 = v.Count;
            v.Add(Bend(0, 0));
            for (int k = 0; k < sides; k++)
            {
                float a = 2 * Mathf.PI * k / sides, rr = r * (0.75f + 0.5f * Rn());
                v.Add(Bend(Mathf.Cos(a) * rr, Mathf.Sin(a) * rr));
            }
            for (int k = 0; k < sides; k++) Tri(t, c0, c0 + 1 + k, c0 + 1 + (k + 1) % sides);
            for (int s = 0; s < strips; s++)
            {
                float a = 2 * Mathf.PI * (s + 0.3f * Rn()) / strips;
                var d = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
                var nrm = new Vector2(-d.y, d.x);
                float L = len * (0.7f + 0.5f * Rn());
                // three segments, a kink at each joint, narrowing to a point
                Vector2 p = d * r * 0.6f;
                float wid = w;
                for (int seg = 0; seg < 3; seg++)
                {
                    Vector2 dir = (d + nrm * ((Rn() - 0.5f) * 0.9f)).normalized;
                    Vector2 q = p + dir * (L / 3);
                    float w2 = seg == 2 ? 0 : wid * 0.6f;
                    int b = v.Count;
                    Vector2 n1 = new Vector2(-dir.y, dir.x);
                    v.Add(Bend((p + n1 * wid * 0.5f).x, (p + n1 * wid * 0.5f).y));
                    v.Add(Bend((p - n1 * wid * 0.5f).x, (p - n1 * wid * 0.5f).y));
                    v.Add(Bend((q + n1 * w2 * 0.5f).x, (q + n1 * w2 * 0.5f).y));
                    v.Add(Bend((q - n1 * w2 * 0.5f).x, (q - n1 * w2 * 0.5f).y));
                    Tri(t, b, b + 1, b + 2);
                    Tri(t, b + 1, b + 3, b + 2);
                    p = q;
                    wid = w2;
                }
            }
            return Make(v, t, "kill55 crack");
        }

        /// <summary>A jagged disc (inner 0) or ring (inner &gt; 0) of `sides` sides.</summary>
        static Mesh BuildDisc(float outer, float inner, int sides, uint seed)
        {
            var v = new System.Collections.Generic.List<Vector3>();
            var t = new System.Collections.Generic.List<int>();
            uint h = seed;
            float Rn() { h ^= h << 13; h ^= h >> 17; h ^= h << 5; return (h & 0xffff) / 65536f; }
            var jag = new float[sides];
            for (int k = 0; k < sides; k++) jag[k] = 0.8f + 0.4f * Rn();
            if (inner <= 0)
            {
                v.Add(Bend(0, 0));
                for (int k = 0; k < sides; k++)
                {
                    float a = 2 * Mathf.PI * k / sides;
                    v.Add(Bend(Mathf.Cos(a) * outer * jag[k], Mathf.Sin(a) * outer * jag[k]));
                }
                for (int k = 0; k < sides; k++) Tri(t, 0, 1 + k, 1 + (k + 1) % sides);
            }
            else
            {
                for (int k = 0; k < sides; k++)
                {
                    float a = 2 * Mathf.PI * k / sides;
                    v.Add(Bend(Mathf.Cos(a) * outer * jag[k], Mathf.Sin(a) * outer * jag[k]));
                    v.Add(Bend(Mathf.Cos(a) * inner * jag[k], Mathf.Sin(a) * inner * jag[k]));
                }
                for (int k = 0; k < sides; k++)
                {
                    int a0 = 2 * k, a1 = 2 * ((k + 1) % sides);
                    Tri(t, a0, a1, a0 + 1);
                    Tri(t, a1, a1 + 1, a0 + 1);
                }
            }
            return Make(v, t, "kill55 hole");
        }

        static void Tri(System.Collections.Generic.List<int> t, int a, int b, int c)
        {
            t.Add(a); t.Add(b); t.Add(c);
            t.Add(a); t.Add(c); t.Add(b); // both faces (the shader's culling whatever it is)
        }

        static Mesh Make(System.Collections.Generic.List<Vector3> v, System.Collections.Generic.List<int> t, string name)
        {
            var m = new Mesh { name = name };
            m.SetVertices(v);
            var nrm = new Vector3[v.Count];
            for (int i = 0; i < nrm.Length; i++) nrm[i] = Vector3.forward;
            m.normals = nrm;
            m.uv = new Vector2[v.Count];
            m.SetTriangles(t, 0);
            m.RecalculateBounds();
            return m;
        }
    }
}
