// NIGHT20 U3-34.9 A1 (builder V): the web's telegraph language, ported (src/fx/telegraph.js, OVERNIGHT2 section 8, "shared
// by every boss"; unported until tonight: the BULWARK's RAM line, its stomp rings and hatch circles, the WARDEN's). There is
// no audio, so every warning is a shape, a colour and a wind-up pose (the web's header, kept):
//   red-orange ground decal filling over its wind-up time   avoid this area, it hits when full   (circle / line)
//   cyan glow on the attack                                  parryable, punch it back             (TELEGRAPH.parry)
//   white flash for 0.15 s                                   the hit lands next frame             (flash)
//   blue outline lane                                        a safe lane                          (lane)
//   green-white decal                                        acid or a hazard that will turn hot  (hazard)
//   screen-edge arrow in the attack colour                   an off-screen attack from that side  (threats)
// Frame time for the look, but every shape has an id so the fixed-step owner can move, refill or cancel it. The quality
// preset scales the extra pulse rings (config.quality.presets[*].telegraph). Numbers: config.telegraph (max, maxFlashes,
// maxThreats, hold, lift, glow, fillGlow, flashTime, arrowTime, wideAt, wideFill, laneFill); the module's own colours and
// literals in UnityTuning.Fx20.cs.
// The split: TelegraphModel is the module's state and clock (pure, Telegraph20Tests); Telegraph20 draws it (additive,
// no depth write, just off the ground, one draw per piece with its own colour). The API is the web's, in web space (Core's
// coordinates), so builder E's BULWARK port calls it as bulwark.js calls `kit.telegraph`: both the view and its model are
// Core's ITelegraph (builder EN's Core/Enemies/ITelegraph.cs; the sim's hex 0 is TELEGRAPH.danger: TelegraphMath.Hex), handed
// to the sim at the views' init (SurvivalGame.Fx20.cs: SurvivalSim.TelegraphView). The threat arrows are listed for H's
// screen-edge arrows (ThreatAt).
using System;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    /// <summary>telegraph.js's shapes and their clock, pure (no UnityEngine draw; Telegraph20Tests).</summary>
    public sealed class TelegraphModel : ITelegraph
    {
        public struct Settings
        {
            public int max, maxFlashes, maxThreats;
            public double hold, arrowTime, flashTime;
        }

        public struct Shape
        {
            public int id;
            public bool line, lane, hazard, fill;
            public double x, y, z, r, dx, dz, length, width, time, hold, warn, age;
            public uint hex;
        }

        public struct Point
        {
            public double x, y, z, r, age, left;
            public uint hex;
        }

        readonly Settings s;
        readonly Shape[] shapes;
        readonly Point[] flashes, threats;
        int flashCount, threatCount, nextId = 1;

        /// <summary>The look's clock (s; the pulse reads it).</summary>
        public double Time { get; private set; }

        public TelegraphModel(Settings settings)
        {
            s = settings;
            shapes = new Shape[Math.Max(1, s.max)];
            flashes = new Point[Math.Max(1, s.maxFlashes)];
            threats = new Point[Math.Max(1, s.maxThreats)];
        }

        int Add(Shape shape)
        {
            int slot = -1, oldest = -1;
            for (int i = 0; i < shapes.Length; i++)
            {
                if (shapes[i].id == 0)
                {
                    slot = i;
                    break;
                }
                if (oldest < 0 || shapes[i].id < shapes[oldest].id) oldest = i;
            }
            if (slot < 0) slot = oldest; // full: the oldest gives way (the web's pools stop drawing past max)
            shape.id = nextId++;
            shape.age = 0;
            shapes[slot] = shape;
            return shape.id;
        }

        int Find(int id)
        {
            if (id <= 0) return -1;
            for (int i = 0; i < shapes.Length; i++)
                if (shapes[i].id == id) return i;
            return -1;
        }

        /// <summary>A red-orange circle filling over `time` s (hits when full); hold: s it stays full (-1: config's).</summary>
        public int Circle(double x, double y, double z, double r, double time, uint hex = UnityTuning.TelegraphDanger, double hold = -1) =>
            Add(new Shape { x = x, y = y, z = z, r = r, time = Math.Max(0.01, time), hold = hold >= 0 ? hold : s.hold, hex = TelegraphMath.Hex(hex), fill = true });

        /// <summary>A red-orange line (a charge path, a sweep) from (x, z) along (dx, dz) for `length` m, `width` wide, filling.</summary>
        public int Line(double x, double y, double z, double dx, double dz, double length, double width, double time, uint hex = UnityTuning.TelegraphDanger, double hold = -1)
        {
            double len = JsMath.Hypot(dx, dz);
            if (len == 0) len = 1;
            return Add(new Shape { line = true, x = x, y = y, z = z, dx = dx / len, dz = dz / len, length = length, width = width, time = Math.Max(0.01, time), hold = hold >= 0 ? hold : s.hold, hex = TelegraphMath.Hex(hex), fill = true });
        }

        /// <summary>A blue outlined safe lane (a faint even fill), for `life` s.</summary>
        public int Lane(double x, double y, double z, double dx, double dz, double length, double width, double life)
        {
            double len = JsMath.Hypot(dx, dz);
            if (len == 0) len = 1;
            return Add(new Shape { line = true, lane = true, x = x, y = y, z = z, dx = dx / len, dz = dz / len, length = length, width = width, time = life, hold = 0, hex = UnityTuning.TelegraphSafe, fill = true });
        }

        /// <summary>A green-white hazard circle for `warn` s, then hot (bright green) for `life` s.</summary>
        public int Hazard(double x, double y, double z, double r, double warn, double life) =>
            Add(new Shape { hazard = true, x = x, y = y, z = z, r = r, time = warn + life, warn = warn, hold = 0, hex = UnityTuning.TelegraphHazard, fill = true });

        static void Push(Point[] list, ref int count, Point p)
        {
            if (count >= list.Length)
            {
                Array.Copy(list, 1, list, 0, list.Length - 1); // shift(): the oldest gives way
                count = list.Length - 1;
            }
            list[count++] = p;
        }

        static void RemoveAt(Point[] list, ref int count, int i)
        {
            if (i < count - 1) Array.Copy(list, i + 1, list, i, count - 1 - i);
            count--;
        }

        /// <summary>A white flash for flashTime s at a point (the hit lands next frame).</summary>
        public void Flash(double x, double y, double z, double radius = 1) => Push(flashes, ref flashCount, new Point { x = x, y = y, z = z, r = radius });

        /// <summary>An off-screen attack coming from a world point: an arrow at the screen edge in its colour for `time` s.</summary>
        public void Threat(double x, double y, double z, uint hex = UnityTuning.TelegraphDanger, double time = -1) =>
            Push(threats, ref threatCount, new Point { x = x, y = y, z = z, hex = TelegraphMath.Hex(hex), left = time >= 0 ? time : s.arrowTime });

        /// <summary>Move a shape (a charge line that follows its boss, a circle that tracks you).</summary>
        public void Move(int id, double x, double y, double z)
        {
            int i = Find(id);
            if (i < 0) return;
            shapes[i].x = x;
            shapes[i].y = y;
            shapes[i].z = z;
        }

        /// <summary>Move a line and turn it along (dx, dz).</summary>
        public void Move(int id, double x, double y, double z, double dx, double dz)
        {
            int i = Find(id);
            if (i < 0) return;
            Move(id, x, y, z);
            double len = JsMath.Hypot(dx, dz);
            if (len == 0) len = 1;
            shapes[i].dx = dx / len;
            shapes[i].dz = dz / len;
        }

        /// <summary>Is a hazard hot yet (its warn time over)?</summary>
        public bool Hot(int id)
        {
            int i = Find(id);
            return i >= 0 && shapes[i].hazard && shapes[i].age >= shapes[i].warn;
        }

        public void Cancel(int id)
        {
            int i = Find(id);
            if (i >= 0) shapes[i].id = 0;
        }

        /// <summary>How full a shape is, 0..1 (-1: no such shape).</summary>
        public double Progress(int id)
        {
            int i = Find(id);
            return i < 0 ? -1 : Math.Min(1, shapes[i].age / shapes[i].time);
        }

        public int Count
        {
            get
            {
                int n = 0;
                for (int i = 0; i < shapes.Length; i++)
                    if (shapes[i].id != 0) n++;
                return n;
            }
        }

        public int FlashCount => flashCount;
        public int ThreatCount => threatCount;

        /// <summary>Shape slot i (id 0: empty) for the view.</summary>
        public ref readonly Shape ShapeAt(int i) => ref shapes[i];
        public int Slots => shapes.Length;
        public ref readonly Point FlashAt(int i) => ref flashes[i];
        public ref readonly Point ThreatAt(int i) => ref threats[i];

        public void Clear()
        {
            for (int i = 0; i < shapes.Length; i++) shapes[i].id = 0;
            flashCount = threatCount = 0;
        }

        /// <summary>Frame time: fills grow, hazards turn hot, flashes fade, arrows count down; shapes past their time go.</summary>
        public void Update(double frameDt)
        {
            Time += frameDt;
            for (int i = 0; i < shapes.Length; i++)
            {
                if (shapes[i].id == 0) continue;
                shapes[i].age += frameDt;
                if (shapes[i].age > shapes[i].time + shapes[i].hold) shapes[i].id = 0;
            }
            for (int i = flashCount - 1; i >= 0; i--)
            {
                flashes[i].age += frameDt;
                if (flashes[i].age > s.flashTime) RemoveAt(flashes, ref flashCount, i);
            }
            for (int i = threatCount - 1; i >= 0; i--)
            {
                threats[i].left -= frameDt;
                if (threats[i].left <= 0) RemoveAt(threats, ref threatCount, i);
            }
        }
    }

    /// <summary>telegraph.js's look rules, pure.</summary>
    public static class TelegraphMath
    {
        /// <summary>NIGHT20 A1 (builder HV): the sim's colour (Core/Enemies/ITelegraph.cs): 0 is TELEGRAPH.danger, any other
        /// hex as it is.</summary>
        public static uint Hex(uint hex) => hex == 0 ? UnityTuning.TelegraphDanger : hex;

        /// <summary>The rim pulses faster as the moment nears; full = the hit moment (steady).</summary>
        public static double Pulse(double time, double k, bool full) => 0.75 + 0.25 * Math.Sin(time * (6 + 18 * k)) * (full ? 0 : 1);

        /// <summary>A line's fill: a lane evenly full, a danger line along its length by k.</summary>
        public static double FillLength(double length, double k, bool lane) => length * (lane ? 1 : k);

        /// <summary>The extra closing ring (better presets): tightening from the rim toward 0.65 r, 1.5 times a second.</summary>
        public static double ClosingRing(double r, double time) => r * (1 - 0.35 * ((time * 1.5) % 1));
    }

    /// <summary>The telegraphs' view (SurvivalGame.Telegraphs): the model drawn on frame time.</summary>
    public sealed class Telegraph20 : ITelegraph
    {
        public readonly TelegraphModel model;
        readonly SurvivalGame game;
        readonly double lift, glowBase, fillGlow, flashTime, wideAt, wideFill, laneFill;
        readonly Mesh ring, disc, rectLine, rectFill, sphere;
        readonly RenderParams rp;
        readonly MaterialPropertyBlock[] blocks = new MaterialPropertyBlock[UnityTuning.TelegraphDraws];
        int used;
        string levelSeen;
        double detail = 1;
        static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

        public Telegraph20(SurvivalGame game)
        {
            this.game = game;
            GameConfig cfg = game.cfg;
            model = new TelegraphModel(new TelegraphModel.Settings
            {
                max = cfg.Int("telegraph.max"), maxFlashes = cfg.Int("telegraph.maxFlashes"), maxThreats = cfg.Int("telegraph.maxThreats"),
                hold = cfg.Num("telegraph.hold"), arrowTime = cfg.Num("telegraph.arrowTime"), flashTime = cfg.Num("telegraph.flashTime"),
            });
            lift = cfg.Num("telegraph.lift");
            glowBase = cfg.Num("telegraph.glow");
            fillGlow = cfg.Num("telegraph.fillGlow");
            flashTime = cfg.Num("telegraph.flashTime");
            wideAt = cfg.Num("telegraph.wideAt");
            wideFill = cfg.Num("telegraph.wideFill");
            laneFill = cfg.Num("telegraph.laneFill");
            Material m = FxDraw.Additive(game.mats.neon, "Telegraphs");
            m.SetFloat("_Cull", 0);
            rp = FxDraw.Params(m);
            for (int i = 0; i < blocks.Length; i++) blocks[i] = new MaterialPropertyBlock();
            ring = FxDraw.Ring(UnityTuning.TelegraphRingInner, 1, UnityTuning.TelegraphSegments, "telegraph ring");
            disc = Disc(UnityTuning.TelegraphSegments);
            rectLine = RectOutline(UnityTuning.TelegraphBar);
            rectFill = RectFill();
            var probe = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere = probe.GetComponent<MeshFilter>().sharedMesh; // radius 0.5: scaled x 2 r
            UnityEngine.Object.Destroy(probe);
            game.OnNewRun += model.Clear;
        }

        // --- the web's API (web space) ----------------------------------------------------------------------------------
        public int Circle(double x, double y, double z, double r, double time, uint hex = UnityTuning.TelegraphDanger, double hold = -1) => model.Circle(x, y, z, r, time, hex, hold);
        public int Line(double x, double y, double z, double dx, double dz, double length, double width, double time, uint hex = UnityTuning.TelegraphDanger, double hold = -1) => model.Line(x, y, z, dx, dz, length, width, time, hex, hold);
        public int Lane(double x, double y, double z, double dx, double dz, double length, double width, double life) => model.Lane(x, y, z, dx, dz, length, width, life);
        public int Hazard(double x, double y, double z, double r, double warn, double life) => model.Hazard(x, y, z, r, warn, life);
        public void Flash(double x, double y, double z, double radius = 1) => model.Flash(x, y, z, radius);
        public void Threat(double x, double y, double z, uint hex = UnityTuning.TelegraphDanger, double time = -1) => model.Threat(x, y, z, hex, time);
        public void Move(int id, double x, double y, double z) => model.Move(id, x, y, z);
        public void Move(int id, double x, double y, double z, double dx, double dz) => model.Move(id, x, y, z, dx, dz);
        public bool Hot(int id) => model.Hot(id);
        public void Cancel(int id) => model.Cancel(id);
        public double Progress(int id) => model.Progress(id);
        public void Clear() => model.Clear();

        /// <summary>H's screen-edge arrows: threat i's world point (Unity space) and colour.</summary>
        public bool ThreatAt(int i, out Vector3 at, out Color color)
        {
            if (i < 0 || i >= model.ThreatCount)
            {
                at = default;
                color = default;
                return false;
            }
            ref readonly TelegraphModel.Point p = ref model.ThreatAt(i);
            at = WebSpace.ToUnity(new Vec3(p.x, p.y, p.z));
            color = Look.Hex(p.hex);
            return true;
        }

        /// <summary>Frame time: the model's clock, then every shape, flash drawn.</summary>
        public void Render(double frameDt)
        {
            model.Update(frameDt);
            string level = game.lightAndLens != null ? game.lightAndLens.Level : null;
            if (!ReferenceEquals(level, levelSeen))
            {
                levelSeen = level;
                string key = "quality.presets." + (level ?? "high") + ".telegraph"; // on a preset change only
                detail = game.cfg.Has(key) ? game.cfg.Num(key) : 1;
            }
            used = 0;
            double time = model.Time;
            for (int i = 0; i < model.Slots; i++)
            {
                ref readonly TelegraphModel.Shape s = ref model.ShapeAt(i);
                if (s.id == 0) continue;
                double k = Math.Min(1, s.age / s.time);
                bool full = s.age >= s.time;
                uint hex = s.hazard && s.age >= s.warn ? UnityTuning.TelegraphHot : s.hex;
                double glow = glowBase * TelegraphMath.Pulse(time, k, full) * (full && !s.lane ? 1.6 : 1);
                Color c = Look.Hex(hex);
                Vector3 p = WebSpace.ToUnity(new Vec3(s.x, s.y + lift, s.z));
                if (!s.line)
                {
                    float r = (float)s.r;
                    Draw(ring, Flat(p, r), c * (float)glow);
                    double f = s.hazard ? 1 : k;
                    if (s.fill && f > 0.001)
                        Draw(disc, Flat(p, (float)(s.r * f)), c * (float)(fillGlow * (s.hazard && s.age < s.warn ? 0.6 : 1)) * UnityTuning.TelegraphDiscOpacity);
                    if (!s.hazard && detail >= 1)
                        Draw(ring, Flat(p, (float)TelegraphMath.ClosingRing(s.r, time)), c * (float)(glow * 0.35));
                }
                else
                {
                    Vector3 dir = new Vector3((float)s.dx, 0, (float)-s.dz); // web (dx, dz) -> Unity (x, -z)
                    Draw(rectLine, Strip(p, dir, (float)s.width, (float)s.length), c * (float)glow);
                    if (s.fill)
                    {
                        double wide = s.width > wideAt ? wideFill : 1;
                        float fl = (float)TelegraphMath.FillLength(s.length, k, s.lane);
                        if (fl > 0) Draw(rectFill, Strip(p, dir, (float)s.width, fl), c * (float)(fillGlow * (s.lane ? laneFill : wide)) * UnityTuning.TelegraphRectOpacity);
                    }
                }
            }
            Color white = Look.Hex(UnityTuning.TelegraphFlashHex);
            for (int i = 0; i < model.FlashCount; i++)
            {
                ref readonly TelegraphModel.Point f = ref model.FlashAt(i);
                double kf = 1 - f.age / flashTime;
                float r = (float)(f.r * (1 + 0.3 * (1 - kf))) * 2; // the sphere mesh's radius is 0.5
                Vector3 at = WebSpace.ToUnity(new Vec3(f.x, f.y, f.z));
                Draw(sphere, new Matrix4x4(new Vector4(r, 0, 0, 0), new Vector4(0, r, 0, 0), new Vector4(0, 0, r, 0), new Vector4(at.x, at.y, at.z, 1)), white * (float)(3 * kf) * UnityTuning.TelegraphFlashOpacity);
            }
        }

        void Draw(Mesh mesh, Matrix4x4 m, Color c)
        {
            if (used >= blocks.Length) return;
            MaterialPropertyBlock b = blocks[used++];
            b.SetColor(BaseColor, c);
            RenderParams p = rp;
            p.matProps = b;
            Graphics.RenderMesh(p, mesh, 0, m);
        }

        /// <summary>A flat piece (y up) at p, scaled r on x and z.</summary>
        static Matrix4x4 Flat(Vector3 p, float r) => new Matrix4x4(new Vector4(r, 0, 0, 0), new Vector4(0, 1, 0, 0), new Vector4(0, 0, r, 0), new Vector4(p.x, p.y, p.z, 1));

        /// <summary>A unit rectangle (x -0.5..0.5 across, z 0..1 along) laid from p along dir (unit, flat), width x length.</summary>
        static Matrix4x4 Strip(Vector3 p, Vector3 dir, float width, float length)
        {
            Vector3 across = new Vector3(dir.z, 0, -dir.x); // right of the direction, on the floor
            return new Matrix4x4(
                new Vector4(across.x * width, 0, across.z * width, 0),
                new Vector4(0, 1, 0, 0),
                new Vector4(dir.x * length, 0, dir.z * length, 0),
                new Vector4(p.x, p.y, p.z, 1));
        }

        static Mesh Disc(int segments)
        {
            var v = new Vector3[segments + 1];
            var t = new int[segments * 3];
            v[0] = Vector3.zero;
            for (int i = 0; i < segments; i++)
            {
                float a = i * Mathf.PI * 2 / segments;
                v[i + 1] = new Vector3(Mathf.Cos(a), 0, Mathf.Sin(a));
                t[i * 3] = 0;
                t[i * 3 + 1] = (i + 1) % segments + 1;
                t[i * 3 + 2] = i + 1;
            }
            var m = new Mesh { name = "telegraph disc", vertices = v, triangles = t };
            m.RecalculateBounds();
            return m;
        }

        static Mesh RectFill()
        {
            var m = new Mesh
            {
                name = "telegraph rect",
                vertices = new[] { new Vector3(-0.5f, 0, 0), new Vector3(0.5f, 0, 0), new Vector3(0.5f, 0, 1), new Vector3(-0.5f, 0, 1) },
                triangles = new[] { 0, 2, 1, 0, 3, 2 },
            };
            m.RecalculateBounds();
            return m;
        }

        /// <summary>The unit rectangle's outline: four flat bars `bar` wide (telegraph.js's four thin boxes).</summary>
        static Mesh RectOutline(float bar)
        {
            float h = bar * 0.5f;
            var v = new Vector3[16];
            var t = new int[24];
            void Bar(int n, float x0, float z0, float x1, float z1)
            {
                v[n * 4] = new Vector3(x0, 0, z0);
                v[n * 4 + 1] = new Vector3(x1, 0, z0);
                v[n * 4 + 2] = new Vector3(x1, 0, z1);
                v[n * 4 + 3] = new Vector3(x0, 0, z1);
                int o = n * 6, b = n * 4;
                t[o] = b; t[o + 1] = b + 2; t[o + 2] = b + 1;
                t[o + 3] = b; t[o + 4] = b + 3; t[o + 5] = b + 2;
            }
            Bar(0, -0.5f, -h, 0.5f, h); // the near end
            Bar(1, -0.5f, 1 - h, 0.5f, 1 + h); // the far end
            Bar(2, -0.5f - h, 0, -0.5f + h, 1); // the left side
            Bar(3, 0.5f - h, 0, 0.5f + h, 1); // the right side
            var m = new Mesh { name = "telegraph outline", vertices = v, triangles = t };
            m.RecalculateBounds();
            return m;
        }
    }
}
