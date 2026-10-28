// NIGHT43 builder F1 (design.n43.feel43, a view row): one small sprite pool for the night's gun feel (GunFx.Hits43.cs: hit stars and sparks,
// the THUNDERCLAP's slug, the OPEN boss read; GunFx.Torch43.cs: the TORCH's flame). Kenney sprites (FxSprites: additive, textured, one
// RenderMesh a colour bucket), five shapes: a streak along the velocity (trace_06), a star (star_06), a soft flare (flare_01), a ring
// (circle_02) and a puff (smoke_04); each part grows from size0 to size1 and fades (1 - u)^fadePow over its life; optional gravity and drag.
// Frame time (the parts move through a freeze, as KillFeel42's rings do). Pooled (the oldest gives way); nothing allocated per frame.
using System;
using UnityEngine;

namespace Skygrind.Game
{
    public sealed class Fx43Pool
    {
        public enum Shape { Streak = 0, Star = 1, Flare = 2, Ring = 3, Puff = 4, Shade = 5 } // Shade: a dark multiply puff (Fx43Shade): value contrast behind a bright read

        struct P
        {
            public Vector3 pos, vel;
            public float life, maxLife, size0, size1, width, gravity, drag, roll, fadePow;
            public Color color;
            public Shape shape;
            public bool live;
            public bool clear; // down the aim line: kept off the crosshair (banned 16, SprayMath.CrossClear on its edge's distance)
        }

        readonly SurvivalGame game;
        readonly P[] parts;
        int next;
        readonly FxSprites[] sprites = new FxSprites[5];
        Fx43Shade shade;
        bool built;
        readonly int cap;

        /// <summary>Parts drawn last frame (the probe).</summary>
        public int Drawn { get; private set; }

        public Fx43Pool(SurvivalGame game, int capacity)
        {
            this.game = game;
            cap = Math.Max(8, capacity);
            parts = new P[cap];
        }

        void Build()
        {
            built = true;
            Material neon = game.mats.neon;
            sprites[(int)Shape.Streak] = new FxSprites(neon, UnityTuning.KenneyTrace, cap);
            sprites[(int)Shape.Star] = new FxSprites(neon, UnityTuning.KenneyStar, cap / 4);
            sprites[(int)Shape.Flare] = new FxSprites(neon, UnityTuning.KenneyFlare, cap / 2);
            sprites[(int)Shape.Ring] = new FxSprites(neon, UnityTuning.KenneyRing, cap / 8);
            sprites[(int)Shape.Puff] = new FxSprites(neon, UnityTuning.KenneySmoke, cap / 2);
            shade = new Fx43Shade(neon, "Fx43 ink", cap / 4, Look.Hex(UnityTuning.Torch43SmokeHex), UnityTuning.Gun43InkDark);
        }

        /// <summary>A colour's materials made now (at load), for every shape.</summary>
        public void Warm(Color c)
        {
            if (!built) Build();
            for (int i = 0; i < sprites.Length; i++) sprites[i].Warm(c);
        }

        public void Clear()
        {
            for (int i = 0; i < parts.Length; i++) parts[i].live = false;
        }

        /// <summary>A part: a shape at `at` moving at `vel` (m/s), growing size0 -> size1 (m), `width` for a streak (m), living `life` s.</summary>
        public void Add(Shape shape, Vector3 at, Vector3 vel, Color color, float life, float size0, float size1, float width = 0, float gravity = 0, float drag = 0, float roll = 0, float fadePow = 2, bool clear = false)
        {
            if (!(life > 0)) return;
            ref P p = ref parts[next];
            next = (next + 1) % parts.Length;
            p.clear = clear;
            p.pos = at;
            p.vel = vel;
            p.life = p.maxLife = life;
            p.size0 = size0;
            p.size1 = size1;
            p.width = width;
            p.gravity = gravity;
            p.drag = drag;
            p.roll = roll;
            p.fadePow = fadePow;
            p.color = color;
            p.shape = shape;
            p.live = true;
        }

        /// <summary>A fixed streak from `a` to `b` (a tracer), fading over `life` (no motion).</summary>
        public void Line(Vector3 a, Vector3 b, Color color, float life, float width)
        {
            // stored as a streak whose velocity is the line itself and whose size1 marks it static (the draw keeps a -> b)
            Add(Shape.Streak, b, b - a, color, life, -1, -1, width, 0, 0, 0, 2);
        }

        /// <summary>Once a frame: move, fade, draw.</summary>
        public void Render(double frameDt)
        {
            Camera cam = game.cam;
            Drawn = 0;
            if (cam == null) return;
            if (!built) Build();
            if (!sprites[0].Ready) return;
            float dt = (float)frameDt;
            Transform ct = cam.transform;
            Vector3 eye = ct.position;
            for (int i = 0; i < sprites.Length; i++) sprites[i].Begin();
            shade.Begin();
            for (int i = 0; i < parts.Length; i++)
            {
                ref P p = ref parts[i];
                if (!p.live) continue;
                p.life -= dt;
                if (p.life <= 0)
                {
                    p.live = false;
                    continue;
                }
                float u = 1 - p.life / p.maxLife; // 0 born .. 1 dead
                float k = Mathf.Pow(1 - u, p.fadePow);
                if (!(k > 0.002f)) continue;
                bool line = p.size0 < 0;
                if (!line)
                {
                    p.vel.y -= p.gravity * dt;
                    if (p.drag > 0) p.vel *= Mathf.Max(0, 1 - p.drag * dt);
                    p.pos += p.vel * dt;
                }
                float grow = 1 - (1 - u) * (1 - u) * (1 - u); // ease-out cubic
                float size = p.size0 + (p.size1 - p.size0) * grow;
                if (p.clear)
                {
                    k *= ClearAt(cam, p.pos, p.shape == Shape.Streak ? p.width : size);
                    if (!(k > 0.002f)) continue;
                }
                switch (p.shape)
                {
                    case Shape.Streak:
                        if (line) sprites[0].Streak(p.pos - p.vel, p.pos, p.width, eye, p.color, k);
                        else sprites[0].Streak(p.pos - p.vel * UnityTuning.Spray20StreakTime, p.pos, Mathf.Max(0.01f, p.width * (1 - 0.5f * u)), eye, p.color, k);
                        break;
                    case Shape.Ring:
                        sprites[(int)Shape.Ring].Billboard(p.pos, 2 * size / UnityTuning.KenneyRingMid, p.roll, ct, p.color, k);
                        break;
                    case Shape.Shade:
                        shade.Billboard(p.pos, size, p.roll, ct, k);
                        break;
                    default:
                        sprites[(int)p.shape].Billboard(p.pos, size, p.roll + u * 0.6f, ct, p.color, k);
                        break;
                }
                Drawn++;
            }
            shade.Draw();
            if (Drawn > 0)
                for (int i = 0; i < sprites.Length; i++) sprites[i].Draw();
        }

        /// <summary>banned 16: a sprite `size` m across at `at`: its brightness share by its nearest edge's distance (px) from the crosshair.</summary>
        static float ClearAt(Camera cam, Vector3 at, float size)
        {
            Vector3 sp = cam.WorldToScreenPoint(at);
            if (sp.z <= 0.05f) return 0f; // at or behind the eye: it would fill the view
            float dx = sp.x - cam.pixelWidth * 0.5f, dy = sp.y - cam.pixelHeight * 0.5f;
            float tanV = Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
            float radiusPx = size * 0.5f / (sp.z * tanV) * cam.pixelHeight * 0.5f;
            float edge = Mathf.Max(0, Mathf.Sqrt(dx * dx + dy * dy) - radiusPx * 0.35f); // the soft sprite's bright core is ~35 % of its quad (its glow may cross the reticle, its core may not)
            return SprayMath.CrossClear(edge, cam.pixelHeight);
        }
    }
}
