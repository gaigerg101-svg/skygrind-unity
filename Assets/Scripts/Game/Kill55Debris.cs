// NIGHT55 K kill55 (2): the debris that stays (design.n55.kill55; the veteran critic's #2: "at f0250, 0.5 s after the last kill, I see nothing of
// the four bodies"; "a campaign kill leaves at least 8 debris pieces that stay at least 1.5 s"). The chunks (Game/Chunks.cs) still break the
// body into its own blocks and shrink them out within ChunkLife (D-29: "nothing stays" was NIGHT17's call; this row is the reversal, logged for
// the lead's DECIDE55). On every kill of the player's this throws UnityTuning.Kill55ShardsPerKill shell shards (more inside a chain, more at
// speed: speed55) out from the kill point and away from the gun; they fall under fx.gravity, find their floor once with the world collider's
// ray (no PhysX), bounce, lie still, and REST for Kill55ShardRestS of world time before they shrink out; a full ring recycles its oldest
// (github.md item 5: the decal pool's rule). Drawn as the chunks draw (Graphics.RenderMesh on EnemyLook's SKYGRIND/Enemy body material, a
// property block per shard: the proven path in a Player). Colours: the enemy's own red-orange, a charred inside, a hot edge cooling (rule 8:
// enemy parts, never cyan). World time: frozen in a hold, slowed in a slow-mo. Pictures only: no Core state, a hash of its own (no sim randomness).
using System;
using Skygrind.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace Skygrind.Game
{
    public sealed class Kill55Debris
    {
        struct P
        {
            public Vector3 pos, vel, axis, size;
            public Quaternion rot;
            public float spin, age, rest, floorY, hot;
            public bool probed, resting, alive;
            public Vector4 color, hotColor;
            public int tag;
        }

        readonly SurvivalGame game;
        readonly P[] parts = new P[UnityTuning.Kill55ShardCap];
        readonly MaterialPropertyBlock[] blocks = new MaterialPropertyBlock[UnityTuning.Kill55ShardCap];
        readonly Mesh cube;
        readonly float gravity;
        Material mat;
        RenderParams rp;
        int next, live;
        uint hash = 0x51ED270Bu;
        static readonly int BaseColor = Shader.PropertyToID("_BaseColor"), FlashId = Shader.PropertyToID("_Flash"), RimOnId = Shader.PropertyToID("_RimOn"), DissolveId = Shader.PropertyToID("_Dissolve");

        /// <summary>Shards alive now; thrown since the run began.</summary>
        public int Live => live;
        public int Thrown { get; private set; }

        public Kill55Debris(SurvivalGame game, Mesh cube)
        {
            this.game = game;
            this.cube = cube;
            gravity = (float)game.cfg.Num("fx.gravity"); // the web's particles' fall: shards, chunks and cubes fall alike
            for (int i = 0; i < blocks.Length; i++) blocks[i] = new MaterialPropertyBlock();
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
            rp = new RenderParams(mat) { layer = 0, shadowCastingMode = ShadowCastingMode.Off, receiveShadows = false };
            return true;
        }

        /// <summary>
        /// A kill's shards: `count` of them from `origin` (Unity space), thrown away from the gun along `shot` (unit; level part used) and up,
        /// x `speedMul`; `feetY` the kill's floor guess; `enemyColor` the body's colour (sRGB); `tag` the kill's number (the reel counts by it).
        /// </summary>
        public void Burst(Vector3 origin, Vector3 shot, float feetY, Color enemyColor, int count, float speedMul, int tag)
        {
            if (count <= 0) return;
            var level = new Vector3(shot.x, 0, shot.z);
            level = level.sqrMagnitude > 1e-6f ? level.normalized : Vector3.zero;
            Vector4 shell = (enemyColor * UnityTuning.Kill55ShellValue).linear;
            shell.w = 1;
            Vector4 charC = Look.Hex(UnityTuning.Kill55CharHex).linear, hotC = Look.Hex(UnityTuning.Kill55HotHex).linear;
            charC.w = hotC.w = 1;
            float spinScale = game.cfg.movement.settings.reducedMotion ? UnityTuning.ChunkReducedSpin : 1;
            for (int n = 0; n < count; n++)
            {
                int slot = next;
                next = (next + 1) % parts.Length;
                ref P p = ref parts[slot];
                if (!p.alive) live++;
                p.alive = true;
                float a = H() * 2 * Mathf.PI;
                var outward = new Vector3(Mathf.Cos(a), 0, Mathf.Sin(a));
                Vector3 dir = (outward + level * UnityTuning.Kill55ShardAlong).normalized;
                float sp = Mathf.Lerp(UnityTuning.Kill55ShardSpeedMin, UnityTuning.Kill55ShardSpeedMax, H()) * speedMul;
                float up = Mathf.Lerp(UnityTuning.Kill55ShardUpMin, UnityTuning.Kill55ShardUpMax, H()) * Mathf.Sqrt(Mathf.Max(1, speedMul));
                p.pos = origin + outward * 0.12f + Vector3.up * (H() - 0.5f) * 0.3f;
                p.vel = dir * sp + Vector3.up * up;
                p.axis = new Vector3(H() * 2 - 1, H() * 2 - 1, H() * 2 - 1);
                if (p.axis.sqrMagnitude < 1e-4f) p.axis = Vector3.right;
                p.axis.Normalize();
                p.spin = Mathf.Lerp(UnityTuning.Kill55ShardSpinMin, UnityTuning.Kill55ShardSpinMax, H()) * spinScale;
                p.rot = Quaternion.Euler(H() * 360, H() * 360, H() * 360);
                float s = Mathf.Lerp(UnityTuning.Kill55ShardMin, UnityTuning.Kill55ShardMax, H());
                p.size = new Vector3(s, s * UnityTuning.Kill55ShardThickShare, s * (0.6f + 0.4f * H()));
                p.age = p.rest = 0;
                p.floorY = feetY;
                p.probed = p.resting = false;
                float pick = H();
                p.color = pick < UnityTuning.Kill55CharShare ? charC : shell;
                p.hot = pick > 1 - UnityTuning.Kill55HotShare ? 1 : 0.35f;
                p.hotColor = hotC;
                p.tag = tag;
            }
            Thrown += count;
        }

        /// <summary>A new run, a level's load: nothing on the floor.</summary>
        public void Clear()
        {
            for (int i = 0; i < parts.Length; i++) parts[i].alive = false;
            live = 0;
            next = 0;
        }

        /// <summary>The reel's count: shards of kill `tag` alive now (and inside `planes` when given: in view).</summary>
        public int CountOf(int tag, Plane[] planes = null)
        {
            int n = 0;
            for (int i = 0; i < parts.Length; i++)
            {
                if (!parts[i].alive || parts[i].tag != tag) continue;
                if (planes != null && !GeometryUtility.TestPlanesAABB(planes, new Bounds(parts[i].pos, parts[i].size))) continue;
                n++;
            }
            return n;
        }

        /// <summary>The reel's count: resting shards of kill `tag`.</summary>
        public int RestingOf(int tag)
        {
            int n = 0;
            for (int i = 0; i < parts.Length; i++) if (parts[i].alive && parts[i].tag == tag && parts[i].resting) n++;
            return n;
        }

        /// <summary>Once a rendered frame: the motion on world time (`wdt`: 0 in a hold), the draw.</summary>
        public void Render(float wdt)
        {
            if (live == 0 || !Ready()) return;
            ICollisionWorld world = game.mover != null ? game.mover.World : null;
            for (int i = 0; i < parts.Length; i++)
            {
                ref P p = ref parts[i];
                if (!p.alive) continue;
                p.age += wdt;
                float half = p.size.y * 0.5f;
                if (!p.resting)
                {
                    if (p.age >= UnityTuning.Kill55ShardFlightMaxS) { Kill(ref p); continue; }
                    if (wdt > 0)
                    {
                        p.vel.y -= gravity * wdt;
                        p.pos += p.vel * wdt;
                        p.rot = Quaternion.AngleAxis(p.spin * wdt, p.axis) * p.rot;
                        if (p.pos.y - half <= p.floorY && p.vel.y <= 0)
                        {
                            if (!p.probed)
                            {
                                p.probed = true;
                                float guess = p.floorY;
                                p.floorY = -1e4f; // nothing under it: it falls until its flight ends
                                if (world != null && world.RayIntersect(new Vec3(p.pos.x, guess + UnityTuning.Kill55ShardProbeUp, -p.pos.z), new Vec3(0, -1, 0), out RayHit hit))
                                    p.floorY = (float)(guess + UnityTuning.Kill55ShardProbeUp - hit.distance);
                                if (p.floorY < guess - UnityTuning.Kill55ShardProbeSlack) p.probed = false; // a drop: it falls on and looks again there
                            }
                            if (CrunchMath.Bounce(ref p.pos.y, ref p.vel.y, ref p.vel.x, ref p.vel.z, p.floorY, half, UnityTuning.Kill55ShardBounce, UnityTuning.Kill55ShardFriction, UnityTuning.Kill55ShardRest))
                            {
                                p.spin *= UnityTuning.Kill55ShardFriction;
                                if (p.vel.y == 0 && p.vel.x * p.vel.x + p.vel.z * p.vel.z < UnityTuning.Kill55ShardRest * UnityTuning.Kill55ShardRest)
                                {
                                    // it lies still: flat on the floor, its own heading kept
                                    p.resting = true;
                                    p.vel = Vector3.zero;
                                    Vector3 f = p.rot * Vector3.forward;
                                    f.y = 0;
                                    p.rot = Quaternion.LookRotation(f.sqrMagnitude > 1e-4f ? f.normalized : Vector3.forward, Vector3.up);
                                    p.pos.y = p.floorY + half;
                                }
                            }
                        }
                    }
                }
                else
                {
                    p.rest += wdt;
                    if (p.rest >= UnityTuning.Kill55ShardRestS + UnityTuning.Kill55ShardFadeS) { Kill(ref p); continue; }
                }
                float scale = p.resting && p.rest > UnityTuning.Kill55ShardRestS ? 1 - (p.rest - UnityTuning.Kill55ShardRestS) / UnityTuning.Kill55ShardFadeS : 1;
                float hot = p.hot * Mathf.Clamp01(1 - p.age / UnityTuning.Kill55HotS);
                Vector4 c = Vector4.Lerp(p.color, p.hotColor, hot);
                c.w = 1;
                MaterialPropertyBlock b = blocks[i];
                b.SetVector(BaseColor, c);
                b.SetFloat(FlashId, 0);
                b.SetFloat(RimOnId, UnityTuning.Kill55ShardRim);
                b.SetFloat(DissolveId, 1);
                rp.matProps = b;
                rp.worldBounds = new Bounds(p.pos, Vector3.one * (UnityTuning.Kill55ShardMax * 2));
                Graphics.RenderMesh(rp, cube, 0, Matrix4x4.TRS(p.pos, p.rot, p.size * Mathf.Max(0, scale)));
            }
        }

        void Kill(ref P p)
        {
            p.alive = false;
            live--;
        }
    }
}
