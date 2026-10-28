// NIGHT17 U3-30.5 (builder X): the crunch's pure rules (CrunchTests) and the chunks (GG's CHUNKS switch, ON). A killed
// enemy breaks into its own blocks: each drawn part (EnemyLook.Die's snapshot: the matrix, the mesh's box, the colour)
// splits into cells of about UnityTuning.ChunkCell m, at most ChunkMaxPerAxis a side (the husk: 2 x 4 x 2 body blocks and
// its head; the swarmer 4; the spitter 25), each a cube in the part's colour. On the kill's step (the freeze's first frame) the blocks
// already stand ChunkPopGap apart, white for 2 drawn frames (round 3; then their own colours with a hot rim), and vibrate across the screen (Sakurai: decaying over the freeze); while
// the freeze holds they drift at CrunchFreezeDrift of their speed on frame time (a frozen world still renders); after it
// they fly along the shot (x the overkill), out from the middle and up, spinning, under fx.gravity, bounce off the floor
// (simple maths: the floor under the enemy, checked once by the world collider's ray when a block reaches it; no PhysX),
// fade back from white to their colour, and shrink out within ChunkLife (1.45 s): nothing stays (D-29). A punch kill
// throws them harder with less spread; a headshot pops the head block high. Pooled (ChunkRing; each block a RenderMesh on
// SKYGRIND/Enemy, as the corpse drew: round 1), capped at ChunkCapShare of the quality preset's particleCap, the oldest recycled first. REDUCED
// MOTION halves the spin and stills the vibration. Pictures only: no Core state, no sim randomness (a hash of its own).
// The `kill` event (the web's name) is GG's sound hook for the break: the blocks appear on the frame it fires.
using System;
using Skygrind.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace Skygrind.Game
{
    /// <summary>NIGHT17 U3-30.10: a gun's death signature (how its kill breaks the body).</summary>
    public enum DeathSig { Clean, Shove, Nails, Blast, Split, Punch }

    /// <summary>NIGHT17 U3-30.10: a signature's numbers (UnityTuning.Crunch.cs Sig*).</summary>
    public struct SigParams
    {
        public DeathSig sig;
        public float coneDeg; // the throw's cone (full angle); 0 with radial
        public float speedMult; // x the throw's speed (0: the blocks only drop)
        public float upMult; // x the upward kick
        public float blockScale; // x a block's size (the nails' chips are smaller)
        public float keepShare; // the share of the cells that become blocks (the nails' fewer chips)
        public float slide; // m the held body (or each half) slides before the burst
        public float holdTime; // s of world time the hold lasts after the freeze (the slide, the pin)
        public float ringScale; // x the floor ring (0: none)
        public bool radial; // thrown out from the blast's point
        public bool split; // two halves along the shot, a seam
        public bool pinned; // the chips laid along the shot's line, then dropping
        public bool leadHero; // the block on the shot's line flies furthest
    }

    /// <summary>The crunch's pure rules (no Unity call): caps, curves, the bounce, the gates.</summary>
    public static class CrunchMath
    {
        /// <summary>NIGHT17 U3-30.10: the death signature of a kill by `weapon` (a melee kill: the punch's).</summary>
        public static DeathSig SignatureOf(string weapon, bool melee)
        {
            if (melee) return DeathSig.Punch;
            switch (weapon)
            {
                case "shotgun": return DeathSig.Shove;
                case "nailgun": return DeathSig.Nails;
                case "rocket": return DeathSig.Blast;
                case "rail": return DeathSig.Split;
                case "gauntlet":
                case "punch": return DeathSig.Punch;
                default: return DeathSig.Clean; // the KESTREL (revolver, ricochet) and every gun without its own
            }
        }

        /// <summary>NIGHT17 U3-30.10: a signature's numbers.</summary>
        public static SigParams ParamsOf(DeathSig sig)
        {
            var p = new SigParams
            {
                sig = sig, coneDeg = UnityTuning.ChunkConeDeg, speedMult = 1, upMult = 1, blockScale = 1, keepShare = 1, ringScale = 1,
            };
            switch (sig)
            {
                case DeathSig.Clean:
                    p.leadHero = true;
                    break;
                case DeathSig.Shove:
                    p.coneDeg = UnityTuning.SigShoveConeDeg;
                    p.speedMult = UnityTuning.SigShoveSpeed;
                    p.slide = UnityTuning.CrunchHeavyKnock;
                    p.holdTime = UnityTuning.CrunchHeavySlideTime;
                    break;
                case DeathSig.Nails:
                    p.pinned = true;
                    p.speedMult = 0;
                    p.upMult = 0;
                    p.blockScale = UnityTuning.SigNailChip;
                    p.keepShare = UnityTuning.SigNailKeep;
                    p.holdTime = UnityTuning.SigNailPin;
                    p.ringScale = UnityTuning.SigNailRing;
                    break;
                case DeathSig.Blast:
                    p.radial = true;
                    p.coneDeg = 0;
                    p.speedMult = UnityTuning.SigBlastSpeed;
                    p.upMult = UnityTuning.SigBlastUp;
                    p.ringScale = UnityTuning.SigBlastRing;
                    break;
                case DeathSig.Split:
                    p.split = true;
                    p.coneDeg = UnityTuning.SigSplitConeDeg;
                    p.slide = UnityTuning.SigSplitGap;
                    p.holdTime = UnityTuning.SigSplitTime;
                    break;
                case DeathSig.Punch:
                    break;
            }
            return p;
        }

        /// <summary>The chunk cap: floor(the preset's particleCap x share), within [1, pool].</summary>
        public static int ChunkCap(int pool, double particleCap, double share) => Math.Max(1, Math.Min(pool, (int)Math.Floor(particleCap * share)));

        /// <summary>Blocks along one side of a part: about `cell` m each, at least 1, at most `max`.</summary>
        public static int Cells(float size, float cell, int max) => cell > 0 ? Math.Max(1, Math.Min(max, (int)Math.Round(size / cell))) : 1;

        /// <summary>The hit flash: 1 (white) for `hold` s, then eased back (ease-out) to 0 over `fade` s.</summary>
        public static float HitFlash(float age, float hold, float fade)
        {
            if (age <= hold) return 1;
            if (fade <= 0 || age >= hold + fade) return 0;
            float u = 1 - (age - hold) / fade;
            return u * u;
        }

        /// <summary>The tilt / pop's share: up over `tin` (ease-out), back over `tout` (ease-in), 0 outside.</summary>
        public static float Tilt(float age, float tin, float tout)
        {
            if (age < 0) return 0;
            if (age < tin)
            {
                float u = 1 - age / tin;
                return 1 - u * u;
            }
            age -= tin;
            if (age >= tout) return 0;
            float v = 1 - age / tout;
            return v * v;
        }

        /// <summary>Sakurai's vibration at `t` s (m on one axis): `amp` at once, decaying (squared) to 0 at `time`.</summary>
        public static float Vib(float t, float time, float amp, float hz)
        {
            if (t < 0 || t >= time) return 0;
            float u = 1 - t / time;
            return (float)(amp * u * u * Math.Cos(t * hz * 2 * Math.PI));
        }

        /// <summary>The shake's share: trauma^exponent x scale (exactly 0 at no trauma or scale 0: REDUCED MOTION, effects 0).</summary>
        public static double ShakeShare(double trauma, double exponent, double scale) =>
            trauma > 0 && scale > 0 ? Math.Pow(Math.Min(1, trauma), exponent) * scale : 0;

        /// <summary>One axis of the shake (deg): max x share x (noise + onset), never past the max either way.</summary>
        public static double ShakeDeg(double maxDeg, double share, double noise, double onset)
        {
            if (share <= 0) return 0;
            double d = maxDeg * share * (noise + onset);
            return d > maxDeg ? maxDeg : d < -maxDeg ? -maxDeg : d;
        }

        /// <summary>The onset jolt's share at `age` s: 1 at once, (1 - age / time)^2, 0 from `time`.</summary>
        public static double Onset(double age, double time)
        {
            if (age >= time || time <= 0) return 0;
            double u = 1 - Math.Max(0, age) / time;
            return u * u;
        }

        /// <summary>
        /// A block of half-height `half` against the floor at `floor`: falling into it, it stands on it, its fall turned
        /// back at `restitution` and its ground speed x `friction`; slower than `rest` m/s it lies still. True on a touch.
        /// </summary>
        public static bool Bounce(ref float y, ref float vy, ref float vx, ref float vz, float floor, float half, float restitution, float friction, float rest)
        {
            if (y - half > floor || vy > 0) return false;
            y = floor + half;
            vy = -vy * restitution;
            if (vy < rest) vy = 0;
            vx *= friction;
            vz *= friction;
            return true;
        }

        /// <summary>The muzzle star's strength on its `frame`-th drawn frame: 1, then `secondShare`, then nothing (fx.flashFrames), x FLASH INTENSITY.</summary>
        public static float StarStrength(int frame, int frames, double secondShare, double flashIntensity)
        {
            double fi = flashIntensity < 0 ? 0 : flashIntensity > 1 ? 1 : flashIntensity;
            if (frame < 0 || frame >= frames || fi <= 0) return 0;
            return (float)((frame == 0 ? 1 : secondShare) * fi);
        }

        /// <summary>The impact frame: on every `step`-th kill of a multikill (3, 6, 9 ...), never on fewer.</summary>
        public static bool ImpactFrameDue(int killsInWindow, int step) => step > 0 && killsInWindow >= step && killsInWindow % step == 0;

        /// <summary>
        /// NIGHT19 (builder V; NIGHT18's final pair: horde5's "6-7 spark confetti" after the slabs, "too much"; NIGHT19 banned
        /// pattern 6): the cubes a kill's death burst throws (main.js: fx.deathCount 26 at the body). Where the chunks break the
        /// body into its own blocks (GG's CHUNKS switch) and tonight's look is on (FeelAB), the blocks ARE the death: the burst
        /// throws UnityTuning.DeathBurstWithChunks (none). Otherwise the web's count.
        /// </summary>
        public static int DeathBurstCount(int webCount, bool chunksOn, bool feelOn) => feelOn && chunksOn ? UnityTuning.DeathBurstWithChunks : webCount;

        /// <summary>NIGHT19 (builder V): the cubes a rocket's / grenade's blast bursts (projectiles.js blast(): 30 + 12):
        /// BlastBurstShareN19Pct of them with tonight's look (the rest was the confetti over the slabs); the web's otherwise.</summary>
        public static int BlastBurstCount(int webCount, bool feelOn) => feelOn ? (webCount * UnityTuning.BlastBurstShareN19Pct + 50) / 100 : webCount;

        /// <summary>
        /// NIGHT17 round 3: the crush frame's exposure (EV, negative) at `age` real s: `depthEv` x FLASH INTENSITY (clamped
        /// 0..1) held for `hold` s, then eased back ((1 - u)^2) over `fade` s; 0 before, after, and at FLASH INTENSITY 0.
        /// </summary>
        public static double CrushEv(double age, double hold, double fade, double depthEv, double flashIntensity)
        {
            double fi = flashIntensity < 0 ? 0 : flashIntensity > 1 ? 1 : flashIntensity;
            if (age < 0 || fi <= 0) return 0;
            if (age <= hold) return depthEv * fi;
            if (fade <= 0 || age >= hold + fade) return 0;
            double u = (age - hold) / fade;
            return depthEv * fi * (1 - u) * (1 - u);
        }

        /// <summary>
        /// NIGHT17 round 3: a block's white share: 1 for its first `whiteFrames` drawn frames, then eased back ((1 - t)^2)
        /// over `fade` s of frame time counted from the end of the white frames (`after`).
        /// </summary>
        public static float BlockWhite(int framesDrawn, float after, int whiteFrames, float fade)
        {
            if (framesDrawn < whiteFrames) return 1;
            if (fade <= 0 || after >= fade) return 0;
            float u = 1 - Math.Max(0, after) / fade;
            return u * u;
        }

        /// <summary>How hard a kill throws: damage / max HP within [min, max] (1 with no max HP).</summary>
        public static float Overkill(double damage, double maxHp, float min, float max)
        {
            if (!(maxHp > 0)) return 1;
            double k = damage / maxHp;
            return (float)(k < min ? min : k > max ? max : k);
        }

        /// <summary>
        /// NIGHT18 U3-31.3 (M3): a gun kill's throw axis (unit): `along` the shot's level line, `across` the screen toward
        /// `side` (+: the camera's right), `lift` up.
        /// </summary>
        public static Vector3 KillThrowAxis(Vector3 shot, Vector3 camRight, float side, float along, float across, float lift)
        {
            var level = new Vector3(shot.x, 0, shot.z);
            level = level.sqrMagnitude > 1e-6f ? level.normalized : Vector3.zero;
            var right = new Vector3(camRight.x, 0, camRight.z);
            right = right.sqrMagnitude > 1e-6f ? right.normalized : Vector3.right;
            Vector3 v = level * along + right * (side >= 0 ? across : -across) + Vector3.up * lift;
            return v.sqrMagnitude > 1e-8f ? v.normalized : Vector3.up;
        }

        /// <summary>
        /// NIGHT18 round 3b (M3, horde5's COMET): a blast block's eruption axis (unit): `across` out along `outward`'s level
        /// part (`fallback` when it has none), `lift` up.
        /// </summary>
        public static Vector3 EruptAxis(Vector3 outward, Vector3 fallback, float across, float lift)
        {
            var level = new Vector3(outward.x, 0, outward.z);
            if (level.sqrMagnitude < 1e-6f) level = new Vector3(fallback.x, 0, fallback.z);
            level = level.sqrMagnitude > 1e-6f ? level.normalized : Vector3.zero;
            Vector3 v = level * across + Vector3.up * lift;
            return v.sqrMagnitude > 1e-8f ? v.normalized : Vector3.up;
        }

        /// <summary>NIGHT18 round 4: a near kill's share of the throw's speed: distance / `fullFrom`, within [`minShare`, 1].</summary>
        public static float NearThrowShare(float distance, float fullFrom, float minShare) =>
            fullFrom > 0 ? Math.Max(minShare, Math.Min(1f, distance / fullFrom)) : 1f;

        /// <summary>NIGHT18 (M3): a thrown block's offset `t` s after its release: straight for `gravityDelay` s, then falling.</summary>
        public static Vector3 ThrowOffset(Vector3 vel, float t, float gravity, float gravityDelay)
        {
            float fall = t > gravityDelay ? t - gravityDelay : 0;
            return vel * t + Vector3.down * (0.5f * gravity * fall * fall);
        }

        /// <summary>
        /// NIGHT18 round 1 (M3): the offset with tonight's drag: the velocity x exp(-k dt), k = `drag` x exp(-flight / `dragTime`)
        /// (a pop, a hang, then the fall), then gravity, then the move, stepped as Chunks.Render steps a block.
        /// </summary>
        public static Vector3 ThrowOffset(Vector3 vel, float t, float gravity, float gravityDelay, float drag, float dragTime)
        {
            const float dt = 1f / 600; // finer than a frame: the curve, not one frame rate's stepping
            Vector3 v = vel, p = Vector3.zero;
            for (float flight = 0; flight < t; flight += dt)
            {
                float step = Math.Min(dt, t - flight);
                v = Drag(v, flight, step, drag, dragTime);
                if (flight >= gravityDelay) v.y -= gravity * step;
                p += v * step;
            }
            return p;
        }

        /// <summary>NIGHT18 round 1: one step of tonight's drag: `v` x exp(-drag x exp(-flight / dragTime) x dt) (0 drag: as is).</summary>
        public static Vector3 Drag(Vector3 v, float flight, float dt, float drag, float dragTime)
        {
            if (!(drag > 0) || !(dt > 0)) return v;
            float k = dragTime > 0 ? drag * (float)Math.Exp(-flight / dragTime) : drag;
            return v * (float)Math.Exp(-k * dt);
        }

        /// <summary>
        /// NIGHT18 round 1: the throw's side (+1 the camera's right): the open side when a wall stands within `UnityTuning.KillSideRoom`
        /// on one side only (`roomRight` / `roomLeft`: m to the first wall along each), the roomier when both are close, else `preferred` (UnityTuning.KillSidePreferred: away from the points pop).
        /// </summary>
        public static float ThrowSide(float roomRight, float roomLeft, float preferred)
        {
            float need = UnityTuning.KillSideRoom;
            bool rightOpen = roomRight >= need, leftOpen = roomLeft >= need;
            if (rightOpen != leftOpen) return rightOpen ? 1 : -1;
            if (!rightOpen && roomRight != roomLeft) return roomRight > roomLeft ? 1 : -1;
            return preferred >= 0 ? 1 : -1;
        }

        /// <summary>
        /// NIGHT18 (M2): the kill burst's size (m across): `bodies` x the body's height, never wider than `maxAngle` x the
        /// distance, never under `minScreen` of the screen's height there (`viewTan`: 2 tan(fov / 2)). NIGHT17's rule, its
        /// numbers as arguments (the floor wins over the cap).
        /// </summary>
        public static float KillBurstSize(float bodyH, float distance, float viewTan, float bodies, float maxAngle, float minScreen) =>
            Math.Max(Math.Min(bodyH * bodies, maxAngle * Math.Max(0.5f, distance)), distance * viewTan * minScreen);

        /// <summary>NIGHT18 (M1): the burst's strength on its `frame`-th drawn frame: `full` frames at 1, then `fade` frames fading, x FLASH INTENSITY.</summary>
        public static float BurstStrength(int frame, int full, int fade, double flashIntensity)
        {
            double fi = flashIntensity < 0 ? 0 : flashIntensity > 1 ? 1 : flashIntensity;
            if (frame < 0 || fi <= 0) return 0;
            if (frame < full) return (float)fi;
            if (frame >= full + fade) return 0;
            return (float)(fi * (1 - (frame - full + 1) / (double)(fade + 1)));
        }

        /// <summary>
        /// NIGHT18 U3-31.9 backlog 3: an additive colour's brightest linear channel (`r`, `g`, `b` sRGB 0-1) x `hdr`: at or
        /// under the bloom threshold, no channel clips and no white halo grows, so the tone map keeps the hue.
        /// </summary>
        public static double AdditivePeak(double r, double g, double b, double hdr) => Math.Max(SrgbToLinear(r), Math.Max(SrgbToLinear(g), SrgbToLinear(b))) * hdr;

        /// <summary>The sRGB transfer function's inverse (a 0-1 channel to linear), as Color.linear does.</summary>
        static double SrgbToLinear(double c) => c <= 0.04045 ? c / 12.92 : Math.Pow((c + 0.055) / 1.055, 2.4);

        /// <summary>NIGHT18 backlog 4: the low-HP beat's rate (beats / s): `restHz` at the low-HP line (hurt 0), `lowHz` at 0 HP (hurt 1).</summary>
        public static double HeartRate(double hurt, double restHz, double lowHz)
        {
            double k = hurt < 0 ? 0 : hurt > 1 ? 1 : hurt;
            return restHz + (lowHz - restHz) * k;
        }

        /// <summary>NIGHT18 backlog 4: the beat's shape over one beat (`phase` 0-1): a beat, a smaller echo, then quiet (0-1).</summary>
        public static double HeartPulse(double phase)
        {
            double u = phase - Math.Floor(phase);
            double a = (u - UnityTuning.LowHpBeatAt) / UnityTuning.LowHpBeatWidth, e = (u - UnityTuning.LowHpEchoAt) / UnityTuning.LowHpEchoWidth;
            double v = Math.Exp(-0.5 * a * a) + UnityTuning.LowHpEchoShare * Math.Exp(-0.5 * e * e);
            return v > 1 ? 1 : v;
        }

        /// <summary>NIGHT18: a frame's burst scale with `kills` kills: 1 + perKill x (kills - 1), at most `max`.</summary>
        public static float HordeScale(int kills, float perKill, float max) => kills <= 1 ? 1 : Math.Min(max, 1 + perKill * (kills - 1));
    }

    /// <summary>The chunk pool's slots: a ring over [0, cap), the oldest recycled first; a lower cap kills the slots past it.</summary>
    public sealed class ChunkRing
    {
        readonly bool[] alive;
        int next, cap, live;

        public ChunkRing(int pool)
        {
            alive = new bool[Math.Max(1, pool)];
            cap = alive.Length;
        }

        public int Pool => alive.Length;

        public int Cap => cap;

        /// <summary>Blocks alive now.</summary>
        public int Live => live;

        public bool Alive(int slot) => slot >= 0 && slot < alive.Length && alive[slot];

        public void Kill(int slot)
        {
            if (!Alive(slot)) return;
            alive[slot] = false;
            live--;
        }

        /// <summary>Use at most `n` slots: the ones past it die at once.</summary>
        public void SetCap(int n)
        {
            cap = Math.Max(1, Math.Min(alive.Length, n));
            for (int i = cap; i < alive.Length; i++) Kill(i);
        }

        /// <summary>A slot for a new block: the ring's next (the oldest when full), alive.</summary>
        public int Take()
        {
            int slot = FxGates.TakeSlot(ref next, cap);
            if (!alive[slot])
            {
                alive[slot] = true;
                live++;
            }
            return slot;
        }

        public void Clear()
        {
            for (int i = 0; i < alive.Length; i++) alive[i] = false;
            live = 0;
            next = 0;
        }
    }

    /// <summary>The kill's blocks (CHUNKS ON): pooled, a RenderMesh per block on SKYGRIND/Enemy.</summary>
    public sealed class Chunks
    {
        struct C
        {
            public Vector3 pos, vel, axis, size;
            public Quaternion rot;
            public float spin, age, life, floorY, whiteAfter, vibAge, vibTime, phase;
            public int framesDrawn; // NIGHT17 round 3: white for the first CrunchWhiteFrames drawn frames only
            // NIGHT17 round 4: the hold (the body together, 1.1x, through the freeze; a heavy kill's slide back after it), the
            // far kill's white through the hold, the wall behind the body (probed on the first frame)
            public bool held, farWhite, walled, wallProbed;
            public int heldFrames; // drawn frames while held (the silhouette shows at least one)
            public float flight; // round 5: s since the release (gravity from ChunkGravityDelay)
            public int whiteFrames; // round 5: white-hot drawn frames (a gun kill 2, a punch kill 1)
            public bool free; // round 5: a headshot's head: never held, it flies on frame time inside the freeze
            public float holdScale; // U3-30.10: the hold's scale (1 for the nails' pinned chips)
            public int seamFrames; // U3-30.10: the LONGWAVE's seam: white for this many drawn frames, then gone
            public bool radialPending; // U3-30.10: a COMET block waiting for its blast's point (ApplyBlast)
            public Vector3 center, slide, wallPoint, wallNormal, probeDir;
            public float holdLeft, holdTime;
            public Vector4 color; // linear
            public Color tint; // as drawn on the enemy (SKYGRIND/Enemy's _BaseColor)
            public bool probed;
            // NIGHT18 U3-31.3: what the block is (the reel's probe: a kill's chunk, a shed block, the LONGWAVE's seam), its white
            // (HDR, FeelAB's side), and tonight's in-freeze slide (the THUNDERCLAP's shove on frame time: the freeze's length,
            // the frame time held so far)
            public byte kind;
            public float whiteHdr;
            public bool slideInFreeze;
            public float freezeLen, heldAge;
            // NIGHT18 round 1: tonight's drag (/s at release; 0 none) and the white-hot silhouette drawn over the crush disc
            public float drag;
            public bool silhouette;
            public bool erupt; // NIGHT18 round 3b: a COMET blast block of tonight's: it erupts up from the blast (ApplyBlast)
        }

        const byte KindKill = 0, KindShed = 1, KindSeam = 2; // NIGHT18: C.kind

        // NIGHT17 round 1 fix: the blocks draw as the corpse did (Graphics.RenderMesh with EnemyLook's SKYGRIND/Enemy body
        // material: its colour, _Flash for the white, the rim), one draw per block with its own property block. Build 2's
        // Player drew nothing through RenderMeshInstanced on SKYGRIND/Particle (no block, and no death-burst cube or kill
        // ring either), while the corpse's RenderMesh on SKYGRIND/Enemy drew in build 1. The instanced path stays only for
        // a Player without the Enemy shader.
        Material enemyMat;
        RenderParams erp;
        MaterialPropertyBlock[] blocks;
        static readonly int FlashId = Shader.PropertyToID("_Flash"), RimOnId = Shader.PropertyToID("_RimOn"), DissolveId = Shader.PropertyToID("_Dissolve");
        int reports; // the first breaks' draw is logged once (the reel's log proves the blocks drew)
        bool reportPending;
        // NIGHT17 round 2: the log lines' numbers, kept on the kill frame and formatted / written CrunchLogDelayFrames later
        // (a Debug.Log on the kill frame is part of its cost)
        string logKind;
        bool logHead, logMelee, logEnemyShader, logDrawn, logFrozen;
        int logBreak, logMade, logLive, logCap, logDrawFrame, logBlocks, logAt = int.MaxValue;
        Vector3 logMid;
        float logDistance;

        /// <summary>How the blocks draw (the log line).</summary>
        public string DrawPath => enemyMat != null ? "RenderMesh " + enemyMat.shader.name : (perInstance ? "RenderMeshInstanced SKYGRIND/Particle" : "RenderMeshInstanced neon");

        /// <summary>The cap in use (the log line).</summary>
        public int Cap => ring.Cap;

        /// <summary>EnemyLook's body material (SKYGRIND/Enemy): the blocks draw with it (the corpse's proven path).</summary>
        public void UseEnemyMaterial(Material body)
        {
            enemyMat = body;
            erp = new RenderParams(body) { layer = 0, shadowCastingMode = ShadowCastingMode.Off, receiveShadows = false };
            blocks = new MaterialPropertyBlock[UnityTuning.ChunkPool];
            for (int i = 0; i < blocks.Length; i++) blocks[i] = new MaterialPropertyBlock();
        }

        /// <summary>The next frame's draw is logged (the first breaks only).</summary>
        public void ReportNextDraw()
        {
            if (reports >= 3) return;
            reports++;
            reportPending = true;
        }

        /// <summary>NIGHT17 round 2: a break's log line, kept (no string made on the kill frame) and written later with its draw.</summary>
        public void QueueLog(string kind, bool headshot, bool melee, int made, bool enemyShader)
        {
            if (Breaks > 3 || logAt != int.MaxValue) return; // the first breaks only, one at a time
            logBreak = Breaks;
            logKind = kind;
            logHead = headshot;
            logMelee = melee;
            logMade = made;
            logLive = ring.Live;
            logCap = ring.Cap;
            logEnemyShader = enemyShader;
            logDrawn = false;
            logAt = Time.frameCount + UnityTuning.CrunchLogDelayFrames;
            ReportNextDraw();
        }

        void FlushLog()
        {
            if (Time.frameCount < logAt) return;
            logAt = int.MaxValue;
            Debug.Log($"[crunch] break #{logBreak}: {logKind}{(logHead ? " headshot" : "")}{(logMelee ? " melee" : "")}, blocks {logMade}, pooled {logLive}, cap {logCap}, draw {DrawPath}, enemy shader {(logEnemyShader ? "yes" : "no")}");
            if (logDrawn)
                Debug.Log($"[crunch] frame {logDrawFrame}: drew {logBlocks} blocks ({DrawPath}), middle {logMid.x:0.0} {logMid.y:0.0} {logMid.z:0.0}, {logDistance:0.0} m from the camera, frozen {(logFrozen ? "yes" : "no")}");
            else Debug.Log("[crunch] that break drew no block on its frame");
        }

        /// <summary>
        /// NIGHT17 round 2 (the first kill's hitch): one tiny draw of the blocks' real path (the body material with its
        /// property block, white and not, the cube) in front of `cam`, so the pipeline state exists before the first kill.
        /// </summary>
        public void Warm(Camera cam)
        {
            if (enemyMat == null || cam == null) return;
            Matrix4x4 m = CrunchWarm.Matrix(cam);
            for (int i = 0; i < 2; i++)
            {
                MaterialPropertyBlock b = blocks[i];
                b.SetColor(BaseColor, Color.black);
                b.SetFloat(FlashId, i);
                b.SetFloat(RimOnId, 1);
                b.SetFloat(DissolveId, 1);
                erp.matProps = b;
                erp.worldBounds = CrunchWarm.Bounds(cam);
                Graphics.RenderMesh(erp, cube, 0, m);
            }
            silBlocks[0].SetVector(BaseColor, Vector4.zero); // NIGHT18 round 1: the silhouette's own draw (black: nothing added)
            silRp.matProps = silBlocks[0];
            silRp.worldBounds = CrunchWarm.Bounds(cam);
            Graphics.RenderMesh(silRp, cube, 0, m);
        }

        /// <summary>
        /// NIGHT18 round 1: the room (m) from `from` along `dir`'s level part to the first wall (the world collider's ray), at
        /// most ChunkWallProbe; the kill throw's side picks the open one (CrunchMath.ThrowSide).
        /// </summary>
        public float RoomAlong(Vector3 from, Vector3 dir)
        {
            var d = new Vector3(dir.x, 0, dir.z);
            if (d.sqrMagnitude < 1e-6f) return UnityTuning.ChunkWallProbe;
            d.Normalize();
            ICollisionWorld world = game.mover != null ? game.mover.World : null;
            if (world != null && world.RayIntersect(WebSpace.FromUnity(from), WebSpace.FromUnity(d), out RayHit rh) && rh.distance < UnityTuning.ChunkWallProbe)
                return (float)rh.distance;
            return UnityTuning.ChunkWallProbe;
        }

        readonly SurvivalGame game;
        readonly ChunkRing ring = new ChunkRing(UnityTuning.ChunkPool);
        readonly C[] parts = new C[UnityTuning.ChunkPool];
        readonly Matrix4x4[] matrices = new Matrix4x4[UnityTuning.ChunkPool];
        readonly Vector4[] colors = new Vector4[UnityTuning.ChunkPool];
        readonly MaterialPropertyBlock block = new MaterialPropertyBlock();
        readonly Mesh cube;
        RenderParams rp;
        RenderParams silRp; // NIGHT18 round 1: the silhouette over the crush disc
        readonly MaterialPropertyBlock[] silBlocks = new MaterialPropertyBlock[UnityTuning.ChunkPool];
        readonly bool perInstance;
        readonly float gravity;
        readonly double capLow, capMedium, capHigh;
        string capLevel = "";
        uint hash = 0x9e3779b9u;
        int freezeFrame = -1;
        float freezeSeconds;
        static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

        /// <summary>Blocks alive now (a Play check reads it).</summary>
        public int Live => ring.Live;

        /// <summary>Kills broken into blocks since boot (a Play check reads it).</summary>
        public int Breaks { get; internal set; }

        /// <summary>The unit cube the blocks draw with (the notches use it too).</summary>
        public Mesh Cube => cube;

        public Chunks(SurvivalGame game)
        {
            this.game = game;
            GameConfig cfg = game.cfg;
            gravity = (float)cfg.Num("fx.gravity"); // the web's particles' fall (particles.js), so blocks and cubes fall alike
            capLow = cfg.Num("quality.presets.low.particleCap");
            capMedium = cfg.Num("quality.presets.medium.particleCap");
            capHigh = cfg.Num("quality.presets.high.particleCap");
            var probe = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube = probe.GetComponent<MeshFilter>().sharedMesh;
            UnityEngine.Object.Destroy(probe);
            Shader shader = Shader.Find("SKYGRIND/Particle");
            perInstance = shader != null;
            Material m = perInstance ? new Material(shader) { name = "Chunks" } : new Material(game.mats.neon);
            m.enableInstancing = true;
            rp = new RenderParams(m) { matProps = block, layer = 0, shadowCastingMode = ShadowCastingMode.Off, receiveShadows = false };
            // NIGHT18 round 1: the white-hot silhouette over the crush disc (additive, both faces, after the disc), one block each
            Material sil = FxDraw.Additive(game.mats.neon, "Kill silhouette");
            sil.SetFloat("_Cull", 0);
            sil.renderQueue = (int)RenderQueue.Transparent + 1;
            silRp = new RenderParams(sil) { layer = 0, shadowCastingMode = ShadowCastingMode.Off, receiveShadows = false };
            for (int i = 0; i < silBlocks.Length; i++) silBlocks[i] = new MaterialPropertyBlock();
            // The freeze's length: the impact layer's `impactFreeze` fires after the kill's step, before its frame renders.
            game.events.On<ImpactFreezeEvent>(FeelKeys.ImpactFreeze, e =>
            {
                if (e.skipped) return;
                freezeFrame = Time.frameCount;
                freezeSeconds = (float)e.seconds;
            });
        }

        /// <summary>
        /// NIGHT52 K (T-111): a kill block's white-hot drawn frames when the cap applies, else -1 (today's chunks exactly: a kill from
        /// KillFarWhiteFrom m white through its hold, then KillWhiteFramesGun frames). The cap: fx49 (the impact kit, ON in the shipping config
        /// since NIGHT50) AND skin52 (the NIGHT52 enemy surface row: -skygrindOff52 turns it off, so the off switch gives NIGHT51's chunks),
        /// a gun kill (a punch kill keeps CrunchWhiteFrames), FeelAB on (NIGHT18's chunks), and UnityTuning.Fx49KillWhiteFrames >= 0.
        /// </summary>
        public static int KillWhiteCap(bool n18, bool melee, bool fx49, bool skin52) =>
            n18 && !melee && fx49 && skin52 && UnityTuning.Fx49KillWhiteFrames >= 0 ? UnityTuning.Fx49KillWhiteFrames : -1;

        float H()
        {
            hash ^= hash << 13;
            hash ^= hash >> 17;
            hash ^= hash << 5;
            return (hash & 0xffffff) / (float)0x1000000;
        }

        /// <summary>
        /// Break one part into blocks. `m` is the part's local-to-world (as last drawn), `box` its mesh's local box,
        /// `color` its colour; `shot` the shot's unit direction (Unity space), `center` the body's middle, `throwK` the
        /// overkill share, `sp` the gun's death signature (U3-30.10), `head`: this part is the head of a headshot kill (popped
        /// high), `feetY` the floor guess, `distance` the kill's (a far kill holds its white). NIGHT18 U3-31.3: `throwSide` is
        /// FeelAB's: 0 = NIGHT17's throw (along the shot); +1 / -1 = tonight's (along, across the screen to that side of the
        /// camera's right, and up: CrunchMath.KillThrowAxis), with tonight's silhouette (bigger, whiter, white from nearer).
        /// Returns the blocks made.
        /// </summary>
        public int Break(Matrix4x4 m, Bounds box, Color color, Vector3 shot, Vector3 center, float throwK, SigParams sp, bool head, float feetY, float distance, float throwSide = 0)
        {
            bool melee = sp.sig == DeathSig.Punch;
            bool n18 = throwSide != 0; // NIGHT18: FeelAB on
            Vector3 level = new Vector3(shot.x, 0, shot.z);
            level = level.sqrMagnitude > 1e-6f ? level.normalized : Vector3.zero;
            Vector3 camRight = game.cam != null ? game.cam.transform.right : Vector3.Cross(Vector3.up, shot);
            // U3-30.10 the split's plane: through the body's middle, holding the beam and the vertical (its normal: level, across)
            Vector3 splitN = level != Vector3.zero ? Vector3.Cross(Vector3.up, level).normalized : camRight;
            Vector3 shove = sp.sig == DeathSig.Shove ? level * sp.slide : Vector3.zero; // round 4: the THUNDERCLAP's shove back
            bool farWhite = distance >= UnityTuning.Pick(n18, UnityTuning.KillFarWhiteFrom, UnityTuning.KillFarWhiteFromN17);
            // NIGHT52 K (T-111): with fx49 AND skin52 on, a gun kill's body is white for Fx49KillWhiteFrames drawn frames at most (the kit's burst
            // carries the kill), not through the whole freeze and 3 frames of flight after it (the a249 kill's wash on f6-f8); else unchanged
            // (Skin52.Live, read per kill: the shader's own switch, so -skygrindOff52 and the a252 reel's ForceOff side give NIGHT51's chunks)
            int whiteCap = KillWhiteCap(n18, melee, Fx49.On(game.cfg), Skin52.Live);
            whiteCap = KillWash53.WhiteCap(game.cfg, whiteCap, n18, melee); // NIGHT53 U polish53 (T-111): the f3-f4 peak's white blocks (Polish53.cs)
            if (whiteCap >= 0) farWhite = false;
            Vector3 sx = m.GetColumn(0), sy = m.GetColumn(1), sz = m.GetColumn(2);
            var world = new Vector3(box.size.x * sx.magnitude, box.size.y * sy.magnitude, box.size.z * sz.magnitude);
            // NIGHT18 round 4 (r3b: the COMET's horde broke into "small-cube confetti"; NIGHT17's big slabs read): a blast kill's
            // body breaks into big slabs (KillBlastCellsXZ across, KillBlastCellsY up)
            bool slabs = n18 && sp.radial;
            int maxXZ = slabs ? UnityTuning.KillBlastCellsXZ : UnityTuning.ChunkMaxPerAxis, maxY = slabs ? UnityTuning.KillBlastCellsY : UnityTuning.ChunkMaxPerAxis;
            int nx = head ? 1 : CrunchMath.Cells(world.x, UnityTuning.ChunkCell, maxXZ);
            int ny = head ? 1 : CrunchMath.Cells(world.y, UnityTuning.ChunkCell, maxY);
            int nz = head ? 1 : CrunchMath.Cells(world.z, UnityTuning.ChunkCell, maxXZ);
            Quaternion rot = m.rotation;
            Vector4 lin = (color * UnityTuning.CrunchEnemyValue).linear; // as EnemyLook.Tint draws the enemy (round 3: linear)
            lin.w = 1;
            var size = new Vector3(world.x / nx, world.y / ny, world.z / nz) * (UnityTuning.ChunkFill * sp.blockScale);
            // NIGHT18 round 4 (r3b: a punch kill's pieces washed the crop pale orange): a point-blank kill's pieces smaller and darker
            bool pointBlank = n18 && distance < UnityTuning.KillNearFrom;
            if (pointBlank)
            {
                size *= UnityTuning.KillNearBlockScale;
                lin = new Vector4(lin.x * UnityTuning.KillNearValue, lin.y * UnityTuning.KillNearValue, lin.z * UnityTuning.KillNearValue, 1);
            }
            MovementConfig mc = game.cfg.movement;
            float spinScale = mc.settings.reducedMotion ? UnityTuning.ChunkReducedSpin : 1;
            // NIGHT17 round 2: each block flies in a cone around the throw's axis, out from the body's middle (ChunkOutBias), up
            // (ChunkUpMin..Max m/s); a punch kill's axis tips up and across the screen away from the fist. U3-30.10: the
            // signature sets the cone, the speed, the lift; the blast throws out from its point, the split along its plane.
            // NIGHT18 U3-31.3 (M3): a gun kill's axis crosses the screen and rises (NIGHT17's ran along the shot: depth, a swell
            // in place on screen); the split, the blast and the nails keep their own throws below
            Vector3 axis = melee ? (shot + camRight * UnityTuning.ChunkPunchSide + Vector3.up * UnityTuning.ChunkPunchLift).normalized
                : n18 ? CrunchMath.KillThrowAxis(shot, camRight, throwSide, UnityTuning.KillThrowAlong,
                    sp.sig == DeathSig.Shove ? UnityTuning.KillShoveAcross : UnityTuning.KillThrowAcross, sp.sig == DeathSig.Shove ? UnityTuning.KillShoveLift : UnityTuning.KillThrowLift) : shot; // round 3: the THUNDERCLAP erupts up
            Vector3 p1 = Vector3.Cross(axis, Vector3.up);
            if (p1.sqrMagnitude < 1e-4f) p1 = camRight;
            p1.Normalize();
            Vector3 p2 = Vector3.Cross(axis, p1).normalized;
            float coneRad = sp.coneDeg * 0.5f * Mathf.Deg2Rad; // the half angle
            // round 5: 10-16 m/s at any gun kill (ChunkThrowSpeed 13 x 0.8..1.2), the overkill adding at most ChunkOverkillSpeedMax;
            // NIGHT18: a gun kill at KillThrowSpeed (the punch keeps ChunkThrowSpeed x ChunkPunchMult)
            // round 1: the axis throw (a clean or a shove kill, FeelAB on) pops at KillThrowSpeed against a fading drag; the
            // split, the blast, the nails and the punch keep NIGHT17's speed and no drag
            bool axisThrow = n18 && !melee && !sp.radial && !sp.split && !sp.pinned;
            // round 3b (the reel's horde5 dies to the COMET): a blast's blocks erupt up from it at the axis throw's speed and drag
            bool erupt = n18 && sp.radial;
            float baseSpeed = axisThrow || erupt ? UnityTuning.KillThrowSpeed : UnityTuning.KillThrowSpeedN17;
            float speed = baseSpeed * (melee ? Mathf.Max(1, throwK) * UnityTuning.ChunkPunchMult : Mathf.Clamp(Mathf.Sqrt(throwK), 1f, Night51.On ? UnityTuning.N51ChunkOverkillSpeedMax : UnityTuning.ChunkOverkillSpeedMax)) * sp.speedMult; // NIGHT51 crunch51
            // NIGHT18 round 4 (r3b: at 6 m the chunks left the crop): a near kill throws slower (the blast's eruption keeps its own)
            if (n18 && !erupt) speed *= CrunchMath.NearThrowShare(distance, UnityTuning.KillThrowFullFrom, Night51.On ? UnityTuning.N51KillThrowNearShare : UnityTuning.KillThrowNearShare); // NIGHT51 crunch51
            float outBias = UnityTuning.Pick(n18, UnityTuning.KillOutBias, UnityTuning.KillOutBiasN17);
            float whiteHdr = UnityTuning.Pick(n18, UnityTuning.KillWhiteHdr, UnityTuning.KillWhiteHdrN17);
            float upMult = (melee ? UnityTuning.ChunkPunchUpMult : 1) * sp.upMult;
            // U3-30.10 the KESTREL's lead hero: the block nearest the shot's line through the body flies furthest, along the shot
            int lead = -1;
            if (sp.leadHero && !head)
            {
                float best = float.MaxValue;
                for (int k = 0; k < nx * ny * nz; k++)
                {
                    int ix = k / (ny * nz), iy = k / nz % ny, iz = k % nz;
                    Vector3 at = m.MultiplyPoint3x4(new Vector3(box.min.x + (ix + 0.5f) * box.size.x / nx, box.min.y + (iy + 0.5f) * box.size.y / ny, box.min.z + (iz + 0.5f) * box.size.z / nz));
                    Vector3 off = at - center;
                    float d = (off - shot * Vector3.Dot(off, shot)).sqrMagnitude;
                    if (d < best)
                    {
                        best = d;
                        lead = k;
                    }
                }
            }
            // NIGHT17 round 3: ChunkHeroes of a part's blocks are heroes (bigger, faster, more lift), picked evenly at random
            int cells = nx * ny * nz;
            int keep = Mathf.Max(1, Mathf.RoundToInt(cells * sp.keepShare)); // U3-30.10: the nails' fewer chips
            int cellsLeft = cells, keepLeft = keep, heroesLeft = keep > UnityTuning.ChunkHeroes && !sp.pinned ? UnityTuning.ChunkHeroes : 0;
            int made = 0;
            for (int k = 0; k < cells; k++)
            {
                bool take = H() * cellsLeft < keepLeft; // keep `keep` of the cells, evenly at random
                cellsLeft--;
                if (!take) continue;
                int ix = k / (ny * nz), iy = k / nz % ny, iz = k % nz;
                var local = new Vector3(
                    box.min.x + (ix + 0.5f) * box.size.x / nx,
                    box.min.y + (iy + 0.5f) * box.size.y / ny,
                    box.min.z + (iz + 0.5f) * box.size.z / nz);
                Vector3 at = m.MultiplyPoint3x4(local);
                Vector3 out_ = at - center;
                float outLen = out_.magnitude;
                out_ = outLen > 1e-4f ? out_ / outLen : Vector3.up;
                int slot = ring.Take();
                ref C c = ref parts[slot];
                c.pos = at + out_ * UnityTuning.ChunkPopGap; // round 4: ChunkPopGap 0: the body together through the hold
                bool hero = heroesLeft > 0 && H() * (keepLeft) < heroesLeft;
                if (hero) heroesLeft--;
                keepLeft--;
                float side = Vector3.Dot(at - center, splitN) >= 0 ? 1 : -1; // the split's half
                Vector3 dir;
                if (sp.radial) dir = erupt ? CrunchMath.EruptAxis(out_, camRight, UnityTuning.KillShoveAcross, UnityTuning.KillShoveLift) // round 3b
                    : (out_ + Vector3.up * 0.2f).normalized; // provisional; ApplyBlast aims it from the blast's point
                else
                {
                    Vector3 ax = sp.split ? (splitN * side + level * UnityTuning.SigSplitAlong).normalized : axis;
                    Vector3 q1 = Vector3.Cross(ax, Vector3.up);
                    if (q1.sqrMagnitude < 1e-4f) q1 = camRight;
                    q1.Normalize();
                    Vector3 q2 = Vector3.Cross(ax, q1).normalized;
                    float th = coneRad * Mathf.Sqrt(H()), ph = H() * 2 * Mathf.PI; // uniform over the cone's disc
                    dir = ax * Mathf.Cos(th) + (q1 * Mathf.Cos(ph) + q2 * Mathf.Sin(ph)) * Mathf.Sin(th);
                    dir = (dir + out_ * outBias).normalized;
                }
                bool isLead = k == lead;
                if (isLead) dir = n18 ? axis : shot; // NIGHT18: the lead hero flies the axis (along the shot it stayed inside the body's rect)
                c.vel = dir * speed * (0.8f + 0.4f * H()) * (hero ? UnityTuning.ChunkHeroSpeed : 1) * (isLead ? UnityTuning.SigLeadSpeed : 1)
                        + Vector3.up * (UnityTuning.ChunkUpMin + (UnityTuning.ChunkUpMax - UnityTuning.ChunkUpMin) * H()) * upMult * (hero ? UnityTuning.ChunkHeroUp : 1);
                if (sp.pinned)
                {
                    // U3-30.10 the STITCHER: the chips laid along the nail's line through the body for a beat, then dropping
                    float t = keep > 1 ? (made / (float)(keep - 1) - 0.5f) : 0;
                    c.pos = center + shot * (t * UnityTuning.SigNailLine) + new Vector3(H() - 0.5f, H() - 0.5f, H() - 0.5f) * UnityTuning.SigNailJitter;
                    c.vel = new Vector3(H() - 0.5f, 0, H() - 0.5f) * UnityTuning.SigNailDrift;
                }
                // round 5 (the r4 reviewer: the head only separated at ~200 ms): a headshot's head pops on the impact frame itself
                if (head) c.vel = Vector3.up * UnityTuning.HeadPopUp + shot * UnityTuning.HeadPopAlong;
                c.axis = new Vector3(H() * 2 - 1, H() * 2 - 1, H() * 2 - 1);
                if (c.axis.sqrMagnitude < 1e-4f) c.axis = Vector3.right;
                c.axis.Normalize();
                c.spin = (UnityTuning.ChunkSpinMinDeg + (UnityTuning.ChunkSpinMaxDeg - UnityTuning.ChunkSpinMinDeg) * H()) * spinScale; // deg/s
                c.rot = rot;
                c.size = hero || isLead ? size * UnityTuning.ChunkHeroSize : size;
                c.age = 0;
                c.life = UnityTuning.ChunkLife * (0.75f + 0.25f * H());
                c.floorY = feetY;
                c.probed = false;
                c.whiteAfter = 0;
                c.framesDrawn = 0;
                c.vibAge = 0;
                c.vibTime = -1; // set on the first frame (the freeze's length is known then)
                c.phase = 0; // every block of a kill shakes as one body
                c.color = lin;
                c.tint = color;
                c.held = true;
                c.farWhite = farWhite;
                c.center = center;
                c.slide = sp.split ? splitN * (side * sp.slide) : shove;
                c.holdTime = sp.holdTime;
                c.holdLeft = c.holdTime;
                c.holdScale = sp.pinned ? 1 : UnityTuning.Pick(n18, UnityTuning.KillHoldScale, UnityTuning.KillHoldScaleN17);
                c.walled = false;
                c.wallProbed = false;
                c.heldFrames = 0;
                c.flight = 0;
                c.whiteFrames = melee ? UnityTuning.CrunchWhiteFrames : whiteCap >= 0 ? whiteCap : UnityTuning.Pick(n18, UnityTuning.KillWhiteFramesGun, UnityTuning.KillWhiteFramesGunN17);
                c.seamFrames = 0;
                c.radialPending = sp.radial;
                c.free = head;
                if (head) c.held = false;
                // the wall probe: the kill's level throw (NIGHT18: the axis' level part: it crosses the screen)
                c.probeDir = melee || n18 ? new Vector3(axis.x, 0, axis.z) : level;
                c.kind = KindKill;
                c.whiteHdr = whiteHdr;
                c.slideInFreeze = n18 && c.holdTime <= 0 && c.slide.sqrMagnitude > 0; // NIGHT18: the shove plays inside the freeze
                c.freezeLen = -1; // set on the first frame (the freeze's length is known then)
                c.heldAge = 0;
                c.drag = (axisThrow || erupt) && !head ? UnityTuning.KillThrowDrag : 0; // round 1: the pop and the hang
                c.erupt = erupt && !head;
                c.silhouette = n18 && !pointBlank; // round 1: white-hot over the crush disc on the burst's frames (round 4: not at point blank)
                made++;
            }
            return made;
        }

        /// <summary>
        /// U3-30.10 the LONGWAVE: a thin white-hot seam on the beam's plane through the body (`size`: thin across, the body's
        /// height, its depth along the beam) for SigSeamFrames drawn frames.
        /// </summary>
        public void Seam(Vector3 center, Vector3 shot, Vector3 size, float feetY)
        {
            Vector3 level = new Vector3(shot.x, 0, shot.z);
            if (level.sqrMagnitude < 1e-6f) return;
            int slot = ring.Take();
            ref C c = ref parts[slot];
            c.pos = center;
            c.vel = Vector3.zero;
            c.axis = Vector3.up;
            c.spin = 0;
            c.rot = Quaternion.LookRotation(level.normalized, Vector3.up); // its thin x across the beam
            c.size = size;
            c.age = 0;
            c.life = UnityTuning.ChunkLife;
            c.floorY = feetY;
            c.probed = true;
            c.whiteAfter = 0;
            c.framesDrawn = 0;
            c.vibAge = 0;
            c.vibTime = 0;
            c.phase = 0;
            c.color = Vector4.one;
            c.tint = Color.white;
            c.held = true;
            c.farWhite = true;
            c.center = center;
            c.slide = Vector3.zero;
            c.holdTime = c.holdLeft = 0;
            c.holdScale = 1;
            c.walled = false;
            c.wallProbed = true;
            c.heldFrames = 0;
            c.seamFrames = UnityTuning.SigSeamFrames;
            c.radialPending = false;
            c.free = false;
            c.flight = 0;
            c.kind = KindSeam; // NIGHT18
            c.whiteHdr = UnityTuning.CrunchWhiteHdr;
            c.slideInFreeze = false;
            c.freezeLen = c.heldAge = 0;
            c.drag = 0;
            c.silhouette = false;
            c.erupt = false;
        }

        /// <summary>
        /// U3-30.10 the COMET: the bus's rocketBlast (after its kills, in the same step) aims this frame's blast blocks out
        /// from its point, in every direction, at their speed, with their lift.
        /// </summary>
        public void ApplyBlast(Vector3 point)
        {
            for (int i = 0; i < parts.Length; i++)
            {
                if (!ring.Alive(i) || !parts[i].radialPending) continue;
                ref C c = ref parts[i];
                c.radialPending = false;
                Vector3 flat = new Vector3(c.vel.x, 0, c.vel.z);
                float speed = flat.magnitude;
                Vector3 outward = c.pos - point;
                if (c.erupt)
                {
                    // NIGHT18 round 3b: tonight's eruption, out from the blast's point and mostly up, at the block's own speed
                    Vector3 up = CrunchMath.EruptAxis(outward, c.probeDir, UnityTuning.KillShoveAcross, UnityTuning.KillShoveLift);
                    c.vel = up * c.vel.magnitude;
                    c.probeDir = new Vector3(up.x, 0, up.z);
                    continue;
                }
                outward = outward.sqrMagnitude > 1e-4f ? outward.normalized : Vector3.up;
                c.vel = outward * Mathf.Max(speed, 1f) + Vector3.up * Mathf.Max(0, c.vel.y);
                c.probeDir = new Vector3(outward.x, 0, outward.z);
            }
        }

        /// <summary>The shed block before death (one cube of `size` m at `at`, thrown along `dir` and up).</summary>
        public void Shed(Vector3 at, Vector3 dir, Color color, float feetY)
        {
            int slot = ring.Take();
            ref C c = ref parts[slot];
            Vector4 lin = (color * UnityTuning.CrunchEnemyValue).linear; // as EnemyLook.Tint draws the enemy (round 3: linear)
            lin.w = 1;
            c.pos = at;
            c.vel = dir * UnityTuning.ChunkShotSpeed * (0.7f + 0.5f * H()) + Vector3.up * UnityTuning.ChunkUpSpeed * (0.8f + 0.6f * H());
            c.axis = new Vector3(H() * 2 - 1, H() * 2 - 1, H() * 2 - 1);
            if (c.axis.sqrMagnitude < 1e-4f) c.axis = Vector3.right;
            c.axis.Normalize();
            float spinScale = game.cfg.movement.settings.reducedMotion ? UnityTuning.ChunkReducedSpin : 1;
            c.spin = (UnityTuning.ChunkSpinMinDeg + (UnityTuning.ChunkSpinMaxDeg - UnityTuning.ChunkSpinMinDeg) * H()) * spinScale;
            c.rot = Quaternion.identity;
            c.size = Vector3.one * UnityTuning.CrunchShedSize;
            c.age = 0;
            c.life = UnityTuning.ChunkLife * (0.75f + 0.25f * H());
            c.floorY = feetY;
            c.probed = false;
            c.whiteAfter = 0;
            c.framesDrawn = 0;
            c.vibAge = 0;
            c.vibTime = 0; // a shed block doesn't vibrate
            c.phase = 0;
            c.color = lin;
            c.tint = color;
            c.held = false; // round 4: a shed block flies at once
            c.farWhite = false;
            c.slide = Vector3.zero;
            c.holdTime = c.holdLeft = 0;
            c.walled = false;
            c.wallProbed = true;
            c.heldFrames = 0;
            c.holdScale = 1;
            c.seamFrames = 0;
            c.radialPending = false;
            c.free = false;
            c.flight = 1; // gravity at once
            c.kind = KindShed; // NIGHT18
            c.whiteHdr = UnityTuning.CrunchWhiteHdr;
            c.slideInFreeze = false;
            c.freezeLen = c.heldAge = 0;
            c.drag = 0;
            c.silhouette = false;
            c.erupt = false;
        }

        public void Clear() => ring.Clear();

        // NIGHT17 round 4: the last wall probe (a kill's blocks share one ray: the same middle and the same level throw)
        int wallFrame = -1;
        Vector3 wallFrom, wallDir, wallPointCached, wallNormalCached;
        bool wallHitCached;
        float wallDistCached;

        /// <summary>The wall along the level throw from the body's middle within ChunkWallProbe m (the world collider's ray).</summary>
        void ProbeWall(ICollisionWorld world, Vector3 from, Vector3 along, out bool hit, out Vector3 point, out Vector3 normal, out float dist)
        {
            Vector3 dir = new Vector3(along.x, 0, along.z);
            dir = dir.sqrMagnitude > 1e-6f ? dir.normalized : Vector3.zero;
            // the blocks of one kill fly in a cone: one ray along the kill's level throw serves them all
            if (wallFrame == Time.frameCount && (wallFrom - from).sqrMagnitude < 1e-6f)
            {
                hit = wallHitCached;
                point = wallPointCached;
                normal = wallNormalCached;
                dist = wallDistCached;
                return;
            }
            wallFrame = Time.frameCount;
            wallFrom = from;
            wallDir = dir;
            hit = false;
            point = normal = Vector3.zero;
            dist = 0;
            if (world != null && dir != Vector3.zero && world.RayIntersect(WebSpace.FromUnity(from), WebSpace.FromUnity(dir), out RayHit rh) && rh.distance <= UnityTuning.ChunkWallProbe)
            {
                hit = true;
                dist = (float)rh.distance;
                point = from + dir * dist;
                normal = WebSpace.ToUnity(rh.normal);
                if (Vector3.Dot(normal, dir) > 0) normal = -normal;
                normal.y = 0;
                normal = normal.sqrMagnitude > 1e-6f ? normal.normalized : -dir;
            }
            wallHitCached = hit;
            wallPointCached = point;
            wallNormalCached = normal;
            wallDistCached = dist;
        }

        /// <summary>Once a frame (EnemyLook.Frame): the cap, the motion (frame time inside a freeze), the draw.</summary>
        public void Render(bool simulating)
        {
            Fx47Probe.ChunksLive = Fx47Probe.ChunksCulled = 0; // NIGHT47 F (n47-r1d): the a747 probe, this frame's
            Fx47Probe.ChunkNearest = float.PositiveInfinity;
            if (logAt != int.MaxValue) FlushLog();
            string level = game.lightAndLens != null ? game.lightAndLens.Level : null;
            if (!ReferenceEquals(level, capLevel))
            {
                capLevel = level;
                ring.SetCap(CrunchMath.ChunkCap(UnityTuning.ChunkPool, FxGates.PresetValue(level, capLow, capMedium, capHigh), UnityTuning.ChunkCapShare));
            }
            if (ring.Live == 0) return;
            ImpactFrames director = game.ImpactDirector;
            bool frozen = simulating && director != null && director.FreezeLeft > 0;
            float fdt = simulating ? (float)game.FrameDt : 0, wdt = (float)game.WorldDt;
            MovementConfig mc = game.cfg.movement;
            bool shakeOn = FxGates.MotionOn(mc.settings.reducedMotion, mc.settings.effectsIntensity);
            Transform cam = game.cam != null ? game.cam.transform : null;
            Vector3 right = cam != null ? cam.right : Vector3.right, up = cam != null ? cam.up : Vector3.up;
            float vibNew = freezeFrame == Time.frameCount ? freezeSeconds + UnityTuning.CrunchVibTail : UnityTuning.CrunchVibMin;
            // NIGHT17 round 3: while the crush frame drops the world's exposure, the white blocks rise by as much (white-hot)
            PostFx post = game.lightAndLens != null ? game.lightAndLens.post : null;
            // NIGHT17 round 6 (far kills read as pixel specks): the screen's height in m per m of distance (2 tan(fov / 2))
            float viewTan = game.cam != null ? 2 * Mathf.Tan(game.cam.fieldOfView * 0.5f * Mathf.Deg2Rad) : 1.5f;
            Vector3 eyeAt = cam != null ? cam.position : Vector3.zero;
            float crushComp = post != null ? Mathf.Pow(2, -(float)post.CrushEvNow()) : 1;
            ICollisionWorld world = game.mover != null ? game.mover.World : null;
            int n = 0;
            Vector3 min = Vector3.positiveInfinity, max = Vector3.negativeInfinity;
            bool probing = FeelProbe.Recording; // NIGHT18: the reel's probe (no work outside a reel)
            double flashI = game.settings.View.flashIntensity; // NIGHT18 round 1: the silhouette's white x FLASH INTENSITY
            int killN = 0;
            Vector3 killMin = Vector3.positiveInfinity, killMax = Vector3.negativeInfinity;
            bool fx47Near = cam != null && Fx47View.RowOn(game.cfg); // NIGHT47 F fx47: the near-eye cap below
            for (int i = 0; i < parts.Length; i++)
            {
                if (!ring.Alive(i)) continue;
                ref C c = ref parts[i];
                c.age += wdt;
                if (c.age >= c.life)
                {
                    ring.Kill(i);
                    continue;
                }
                c.radialPending = false; // U3-30.10: no rocketBlast in its step: the provisional outward throw stays
                if (c.vibTime < 0) c.vibTime = Mathf.Max(UnityTuning.CrunchVibMin, vibNew);
                if (c.freezeLen < 0) c.freezeLen = freezeFrame == Time.frameCount ? freezeSeconds : 0; // NIGHT18: the in-freeze slide's length
                c.vibAge += fdt;
                if (!c.wallProbed)
                {
                    // NIGHT17 round 4: the wall behind the body along the throw (one ray a kill, cached for its blocks): the
                    // blocks turn back off it instead of passing through (the r3 kill strip lost them into a doorway); a heavy
                    // kill's slide stops short of it
                    c.wallProbed = true;
                    ProbeWall(world, c.center, c.probeDir, out c.walled, out c.wallPoint, out c.wallNormal, out float wallDist);
                    if (c.walled && c.slide.sqrMagnitude > 0)
                    {
                        float room = Mathf.Max(0, wallDist - UnityTuning.CrunchWallKeep);
                        if (c.slide.magnitude > room) c.slide = c.slide.normalized * room;
                    }
                }
                // NIGHT17 round 4: the hold: the body together (x CrunchHoldScale) while the freeze holds, then a heavy kill's
                // slide back over CrunchHeavySlideTime s of world time, then the throw (the scale and the slide made real)
                float slideShare = 0;
                if (c.held && !frozen && simulating && c.heldFrames > 0)
                {
                    if (c.holdLeft > 0) c.holdLeft -= wdt;
                    if (c.holdLeft <= 0)
                    {
                        c.held = false;
                        c.pos = c.center + c.slide + (c.pos - c.center) * c.holdScale;
                        c.size *= c.holdScale;
                    }
                }
                if (c.held && c.holdTime > 0)
                {
                    float u = 1 - Mathf.Clamp01(c.holdLeft / c.holdTime);
                    slideShare = 1 - (1 - u) * (1 - u);
                }
                else if (c.held && c.slideInFreeze)
                {
                    // NIGHT18 U3-31.3: the THUNDERCLAP's shove inside the freeze (frame time, ease-out), so its blocks fly at the
                    // freeze's end (NIGHT17 slid it after the freeze: the blocks flew 120 ms late)
                    if (frozen) c.heldAge += fdt;
                    float u = c.freezeLen > 0 ? Mathf.Clamp01(c.heldAge / c.freezeLen) : 0;
                    slideShare = 1 - (1 - u) * (1 - u);
                }

                // the motion (round 4: none while held), the world's time
                float md = c.held ? 0 : frozen && c.free ? fdt : wdt; // round 5: the head flies through the freeze
                if (md > 0)
                {
                    float mdt2 = md;
                    if (c.drag > 0) c.vel = CrunchMath.Drag(c.vel, c.flight, mdt2, c.drag, UnityTuning.KillThrowDragTime); // NIGHT18 round 1: the pop, the hang
                    c.flight += mdt2;
                    if (c.flight >= UnityTuning.ChunkGravityDelay) c.vel.y -= gravity * mdt2; // round 5: a straight throw first
                    c.pos += c.vel * mdt2;
                    c.rot = Quaternion.AngleAxis(c.spin * mdt2, c.axis) * c.rot;
                    float half = c.size.y * 0.5f;
                    if (c.walled)
                    {
                        float d = Vector3.Dot(c.pos - c.wallPoint, c.wallNormal);
                        float into = Vector3.Dot(c.vel, c.wallNormal);
                        if (d < half && into < 0)
                        {
                            c.pos += c.wallNormal * (half - d);
                            c.vel -= c.wallNormal * (into * (1 + UnityTuning.ChunkWallBounce)); // round 5: they ricochet into view
                            c.spin *= UnityTuning.ChunkFriction;
                        }
                    }
                    if (c.pos.y - half <= c.floorY && c.vel.y <= 0)
                    {
                        if (!c.probed)
                        {
                            c.probed = true;
                            float guess = c.floorY;
                            c.floorY = -1e4f; // nothing under it: it falls until its life ends
                            if (world != null)
                            {
                                var from = new Vec3(c.pos.x, guess + UnityTuning.ChunkProbeUp, -c.pos.z);
                                if (world.RayIntersect(from, new Vec3(0, -1, 0), out RayHit hit)) c.floorY = (float)(from.y - hit.distance);
                            }
                            if (c.floorY < guess - UnityTuning.ChunkProbeSlack) c.probed = false; // a drop: it falls on and checks again there
                        }
                        if (CrunchMath.Bounce(ref c.pos.y, ref c.vel.y, ref c.vel.x, ref c.vel.z, c.floorY, half, UnityTuning.ChunkBounce, UnityTuning.ChunkFriction, UnityTuning.ChunkRestSpeed))
                            c.spin *= UnityTuning.ChunkFriction;
                    }
                }
                float left = 1 - c.age / c.life;
                float s = Mathf.Min(1, left / UnityTuning.ChunkShrinkShare);
                Vector3 p = c.pos;
                if (c.held)
                {
                    p = c.center + c.slide * slideShare + (c.pos - c.center) * c.holdScale;
                    s *= c.holdScale;
                    c.heldFrames++;
                }
                if (shakeOn && c.vibTime > 0)
                {
                    float vr = CrunchMath.Vib(c.vibAge, c.vibTime, UnityTuning.CrunchVibAmp, UnityTuning.CrunchVibHz);
                    float vu = CrunchMath.Vib(c.vibAge + 0.25f / (UnityTuning.CrunchVibHz * UnityTuning.CrunchVibUpRatio), c.vibTime, UnityTuning.CrunchVibAmp * UnityTuning.CrunchVibUpShare, UnityTuning.CrunchVibHz * UnityTuning.CrunchVibUpRatio);
                    p += right * vr + up * vu;
                }
                // NIGHT17 round 3 (the reviewer: a close kill was a flat white wall through its whole freeze): white for the
                // first CrunchWhiteFrames drawn frames, then back to its own colour over CrunchWhiteFade s of frame time (inside
                // the freeze too), with a hot rim (x CrunchHotRim) through the freeze, cooling over CrunchHotRimTime s of world time
                // round 4: a far kill (CrunchFarWhiteFrom m or more) stays a white silhouette through its hold, then eases
                float white;
                if (c.seamFrames > 0)
                {
                    // U3-30.10 the LONGWAVE's seam: white-hot for its frames, then gone
                    if (c.framesDrawn >= c.seamFrames)
                    {
                        ring.Kill(i);
                        continue;
                    }
                    c.framesDrawn++;
                    white = 1;
                }
                else if (c.held && c.farWhite) white = 1;
                else
                {
                    int wf = c.whiteFrames > 0 ? c.whiteFrames : UnityTuning.CrunchWhiteFrames; // round 5: a gun kill's 2
                    white = CrunchMath.BlockWhite(c.framesDrawn, c.whiteAfter, wf, UnityTuning.CrunchWhiteFade);
                    if (c.framesDrawn >= wf) c.whiteAfter += fdt;
                    c.framesDrawn++;
                }
                float hot = 1 + (UnityTuning.CrunchHotRim - 1) * Mathf.Clamp01(1 - c.age / UnityTuning.CrunchHotRimTime);
                // round 6: a block keeps at least ChunkMinScreen of the screen's height (x ChunkMinScreenMax at most); near: as is
                float dist = Vector3.Distance(p, eyeAt);
                float shown = Mathf.Max(c.size.x, Mathf.Max(c.size.y, c.size.z)) * s;
                float want = dist * viewTan * UnityTuning.ChunkMinScreen;
                if (shown > 1e-5f && shown < want) s *= Mathf.Min(UnityTuning.ChunkMinScreenMax, want / shown);
                if (fx47Near)
                {
                    // NIGHT47 F fx47 (n47-r1d: a close kill's blocks were "large flat angular slabs, white then peach, 1-3 m in front of the
                    // camera, filling the lower-left half"): with the row on no block is drawn within Fx47NearEye of the eye, and none
                    // stands taller than Fx47ChunkMaxScreen of the screen's height (its white silhouette with it)
                    float sz = Mathf.Max(c.size.x, Mathf.Max(c.size.y, c.size.z)) * s;
                    Fx47Probe.ChunksLive++;
                    Fx47Probe.ChunkNearest = Mathf.Min(Fx47Probe.ChunkNearest, dist);
                    if (dist - sz * 0.5f < UnityTuning.Fx47NearEye) { Fx47Probe.ChunksCulled++; continue; }
                    float cap = dist * viewTan * UnityTuning.Fx47ChunkMaxScreen;
                    if (sz > cap && sz > 1e-5f) s *= cap / sz;
                }
                Matrix4x4 tm = Matrix4x4.TRS(p, c.rot, c.size * s);
                var whiteV = new Vector4(c.whiteHdr, c.whiteHdr, c.whiteHdr, 1); // NIGHT18: the block's white (FeelAB's side)
                if (c.silhouette && c.held && c.heldFrames <= KillWash53.SilFrames(game.cfg, UnityTuning.KillBurstFullFrames + UnityTuning.KillBurstFadeFrames)) // NIGHT53 U polish53 (T-111): the burst's full frames only
                {
                    // NIGHT18 round 1 (M1): the held body white-hot over the crush disc (drawn after it: additive, a touch bigger),
                    // on the burst's frames
                    float sk = CrunchMath.BurstStrength(c.heldFrames - 1, UnityTuning.KillBurstFullFrames, UnityTuning.KillBurstFadeFrames, flashI);
                    if (sk > 0)
                    {
                        float sv = UnityTuning.KillSilhouetteHdr * sk;
                        silBlocks[i].SetVector(BaseColor, new Vector4(sv, sv, sv, 1));
                        silRp.matProps = silBlocks[i];
                        silRp.worldBounds = new Bounds(p, Vector3.one * UnityTuning.ChunkBoundsPad * 2);
                        Graphics.RenderMesh(silRp, cube, 0, Matrix4x4.TRS(p, c.rot, c.size * (s * UnityTuning.KillSilhouetteGrow)));
                    }
                }
                if (probing)
                {
                    // NIGHT18 U3-31.3: the reel's probe (M2 / M3): each kill chunk's centre and the cloud's box; a shed block apart
                    float half = Mathf.Max(c.size.x, Mathf.Max(c.size.y, c.size.z)) * s * 0.5f;
                    if (c.kind == KindKill)
                    {
                        FeelProbe.ChunkPoint(p);
                        killMin = Vector3.Min(killMin, p - Vector3.one * half);
                        killMax = Vector3.Max(killMax, p + Vector3.one * half);
                        killN++;
                    }
                    else if (c.kind == KindShed) FeelProbe.EffectWorld(FeelFx.Shed, p, half);
                }
                if (enemyMat != null)
                {
                    // the corpse's path: SKYGRIND/Enemy, its colour toward an HDR white (round 3: x the crush's 2^-EV, so the
                    // blocks stay white-hot while the world drops), the rim on, whole (no dissolve)
                    MaterialPropertyBlock b = blocks[i];
                    Vector4 v = Vector4.Lerp(c.color, whiteV * crushComp, white);
                    v.w = 1;
                    b.SetVector(BaseColor, v);
                    b.SetFloat(FlashId, 0);
                    b.SetFloat(RimOnId, hot);
                    b.SetFloat(DissolveId, 1);
                    erp.matProps = b;
                    erp.worldBounds = new Bounds(p, Vector3.one * UnityTuning.ChunkBoundsPad * 2);
                    Graphics.RenderMesh(erp, cube, 0, tm);
                }
                else
                {
                    matrices[n] = tm;
                    colors[n] = Vector4.Lerp(c.color, whiteV, white);
                }
                n++;
                min = Vector3.Min(min, p);
                max = Vector3.Max(max, p);
            }
            if (killN > 0) FeelProbe.EffectBounds(FeelFx.Chunks, new Bounds((killMin + killMax) * 0.5f, killMax - killMin), 1, killN); // NIGHT18
            if (reportPending && n > 0)
            {
                // NIGHT17 round 2: the numbers only; FlushLog writes the line later
                reportPending = false;
                Vector3 eye = cam != null ? cam.position : Vector3.zero;
                logMid = (min + max) * 0.5f;
                logDistance = Vector3.Distance(eye, logMid);
                logDrawFrame = Time.frameCount;
                logBlocks = n;
                logFrozen = frozen;
                logDrawn = true;
            }
            if (n == 0 || enemyMat != null) return;
            if (perInstance) block.SetVectorArray(BaseColor, colors);
            rp.worldBounds = new Bounds((min + max) * 0.5f, max - min + Vector3.one * UnityTuning.ChunkBoundsPad);
            Graphics.RenderMeshInstanced(rp, cube, 0, matrices, n);
        }
    }

    /// <summary>
    /// NIGHT17 U3-30.10 (beyond the ask; builder X): an enemy's arrival reads as an arrival. On the web's `enemySpawn` (the
    /// spawn beam lands: the handler only queues the point), a DANGER ground ring at its feet grows from SpawnRingStart to
    /// SpawnRingEnd m over SpawnRingTime s (ease-out) and fades ((1 - t)^2), and a bright DANGER core (a camera-facing soft
    /// disc, SpawnCoreSize m at SpawnCoreHeight) shows for 2 frames (full, then half) x FLASH INTENSITY. A local flash, not
    /// a screen-wide one: no flash gate. Additive copies of the neon material, one Graphics.RenderMesh each (no instancing),
    /// frame time; pooled (SpawnFlashPool, the oldest reused). GG's sound hook is `enemySpawn` itself, on the same frame.
    /// </summary>
    /// <summary>NIGHT20 F (builder V): where a kill's floor ring lies (Kill20Tests): at the feet when the floor is there, on the
    /// floor below a flying kill within KillRingReach, nowhere over a drop.</summary>
    public static class KillRing20
    {
        /// <summary>`feetY` the victim's feet (Unity y), `hitDistance` the down ray's from KillRingProbeLift above them (-1:
        /// none): false when there's no floor within reach; else the ring's y.</summary>
        public static bool Place(float feetY, double hitDistance, out float y)
        {
            y = feetY;
            if (!(hitDistance >= 0) || hitDistance > UnityTuning.KillRingProbeLift + UnityTuning.KillRingReach) return false;
            float floor = (float)(feetY + UnityTuning.KillRingProbeLift - hitDistance);
            if (Mathf.Abs(floor - feetY) > UnityTuning.KillRingSnap) y = floor; // airborne (or a hair off): down to the floor
            return true;
        }
    }

    public sealed class SpawnFlashes
    {
        struct Flash
        {
            public Vector3 at; // feet, Unity space
            public double age;
            public int frame0;
            public bool core; // NIGHT17 round 4: a kill's ring has no core
            public float scale; // U3-30.10: x the ring's size (the COMET's dust ring is bigger)
            public bool head; // round 5: a headshot's white ring at the head, facing the camera
        }

        readonly Vector3[] headAsks = new Vector3[4];
        int headAskCount;

        /// <summary>NIGHT17 round 5: a headshot kill's white ring at the head (`at`; only queued).</summary>
        public void HeadRing(Vector3 at)
        {
            if (headAskCount < headAsks.Length) headAsks[headAskCount++] = at;
        }

        // NIGHT17 round 5 (the r4 reviewer: a kill's frame 1 looked like a hit's): a kill's radial burst: flat light streaks
        // (a camera-facing quad with a streak texture made once) about 2x the body's height at its middle, white-hot, for 2
        // drawn frames (full, then half) x FLASH INTENSITY; additive (no flash gate: local, not the screen). Queued from the kill.
        struct Burst
        {
            public Vector3 at;
            public float size, roll;
            public int frame0;
            public bool live;
            // NIGHT18 U3-31.3: tonight's burst (its texture, its frames and brightness) or NIGHT17's
            public bool n18;
            public int full, fade;
            public float hdr, visible;
        }

        readonly Burst[] bursts = new Burst[UnityTuning.SpawnFlashPool];
        readonly MaterialPropertyBlock[] burstBlocks = new MaterialPropertyBlock[UnityTuning.SpawnFlashPool];
        readonly Vector3[] burstAskAt = new Vector3[UnityTuning.SpawnFlashPool];
        readonly float[] burstAskSize = new float[UnityTuning.SpawnFlashPool];
        readonly bool[] burstAskOn = new bool[UnityTuning.SpawnFlashPool]; // NIGHT18: FeelAB's side at the kill
        readonly float[] burstAskBody = new float[UnityTuning.SpawnFlashPool]; // NIGHT18 round 1: the body's height (m: the crush disc's size)
        int burstAskCount, nextBurst;
        uint burstHash = 0x85ebca6bu;
        RenderParams burstRp, burst18Rp; // NIGHT18: tonight's burst texture on its own copy

        /// <summary>
        /// NIGHT17 round 5: a kill's radial burst at `at`, `size` m across (only queued). NIGHT18: `n18` (FeelAB on): tonight's;
        /// round 1: `bodyH`, the victim's height (the crush disc's size on screen).
        /// </summary>
        public void KillBurst(Vector3 at, float size, bool n18 = false, float bodyH = 0)
        {
            if (burstAskCount >= burstAskAt.Length) return;
            burstAskAt[burstAskCount] = at;
            burstAskOn[burstAskCount] = n18;
            burstAskBody[burstAskCount] = bodyH;
            burstAskSize[burstAskCount++] = size;
        }

        // NIGHT18 U3-31.3: the crush disc: one dark disc over a frame's kills (round 1: in front, hard-edged; a multiply), on
        // the bursts' frames, through the flash gate (UnityTuning.KillDisc*)
        struct Disc
        {
            public Vector3 at;
            public float radius;
            public int frame0;
            public bool live;
        }

        Disc disc;
        RenderParams discRp;
        readonly MaterialPropertyBlock discBlock = new MaterialPropertyBlock();
        /// <summary>NIGHT18: the reel's marker of the kill effects' first drawn frame (FeelProbe.Event; the bus's `kill` stays GG's sound hook).</summary>
        public const string KillFxProbeEvent = "killFx";

        /// <summary>
        /// NIGHT18: tonight's burst (round 2: the b3 reviewers read the filled core as a blur over the body): thin, hard
        /// streaks outside the silhouette (from KillBurstInner of the radius), falling to black at the edge (made once).
        /// </summary>
        static Texture2D KillFlashTexture()
        {
            int n = UnityTuning.KillBurstTexture;
            var tex = new Texture2D(n, n, TextureFormat.RGBA32, false, true) { name = "Kill flash", wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
            var px = new Color32[n * n];
            for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                double dx = (x + 0.5) / n * 2 - 1, dy = (y + 0.5) / n * 2 - 1;
                double r = Math.Sqrt(dx * dx + dy * dy);
                double rays = Math.Pow(Math.Abs(Math.Cos(Math.Atan2(dy, dx) * UnityTuning.KillBurstSpikes * 0.5)), UnityTuning.KillBurstSharpness);
                double fall = Math.Max(0, 1 - r);
                double k = Math.Max(0, Math.Min(1, (r - UnityTuning.KillBurstInner) / UnityTuning.KillBurstInnerSoft));
                double inner = k * k * (3 - 2 * k); // 0 over the body, 1 from KillBurstInner + KillBurstInnerSoft out
                byte v = (byte)Math.Round(Math.Min(1, rays * inner * fall * UnityTuning.KillBurstRayGain) * 255);
                px[y * n + x] = new Color32(v, v, v, 255);
            }
            tex.SetPixels32(px);
            tex.Apply(false, true);
            return tex;
        }

        /// <summary>NIGHT18: the crush disc's mask: 1 out to KillDiscEdge of the radius (round 1: a hard edge), smooth to 0 at the rim (made once).</summary>
        static Texture2D DiscTexture()
        {
            int n = UnityTuning.KillDiscTexture;
            var tex = new Texture2D(n, n, TextureFormat.RGBA32, false, true) { name = "Kill disc", wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
            var px = new Color32[n * n];
            for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                double dx = (x + 0.5) / n * 2 - 1, dy = (y + 0.5) / n * 2 - 1;
                double r = Math.Sqrt(dx * dx + dy * dy);
                double k = Math.Max(0, Math.Min(1, (1 - r) / (1 - UnityTuning.KillDiscEdge)));
                byte v = (byte)Math.Round(k * k * (3 - 2 * k) * 255);
                px[y * n + x] = new Color32(v, v, v, 255);
            }
            tex.SetPixels32(px);
            tex.Apply(false, true);
            return tex;
        }

        /// <summary>Flat light streaks (made once): thin radial spikes, bright at the middle, black at the edge (additive).</summary>
        static Texture2D StreakTexture()
        {
            int n = UnityTuning.BurstTexture;
            var tex = new Texture2D(n, n, TextureFormat.RGBA32, false, true) { name = "Kill burst", wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
            var px = new Color32[n * n];
            for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                double dx = (x + 0.5) / n * 2 - 1, dy = (y + 0.5) / n * 2 - 1;
                double r = Math.Sqrt(dx * dx + dy * dy);
                double spikes = Math.Pow(Math.Abs(Math.Cos(Math.Atan2(dy, dx) * UnityTuning.BurstSpikes * 0.5)), UnityTuning.BurstSharpness);
                double fall = Math.Max(0, 1 - r);
                double core = Math.Max(0, 1 - r * 4);
                byte v = (byte)Math.Round(Math.Min(1, spikes * fall * fall + core) * 255);
                px[y * n + x] = new Color32(v, v, v, 255);
            }
            tex.SetPixels32(px);
            tex.Apply(false, true);
            return tex;
        }

        // NIGHT17 round 4: a kill's DANGER floor ring at the victim's feet (queued from the kill: EnemyLook.Die)
        readonly Vector3[] ringAsks = new Vector3[UnityTuning.SpawnFlashPool];
        readonly float[] ringScales = new float[UnityTuning.SpawnFlashPool];
        int ringAskCount;

        /// <summary>NIGHT17 round 4: a kill's floor ring at `feet` x `scale` (only queued; drawn from the next Render).</summary>
        public void KillRing(Vector3 feet, float scale = 1)
        {
            if (scale <= 0 || ringAskCount >= ringAsks.Length) return;
            // NIGHT20 F (builder V): a flying kind's kill (the DRONE's, a launched body's) laid its ring in mid-air at its feet:
            // the ring goes down to the floor under it (the world collider's ray), or nowhere over a drop (KillRing20)
            if (SpeedLines20.Live && game.world != null)
            {
                Vec3 from = WebSpace.FromUnity(feet + Vector3.up * UnityTuning.KillRingProbeLift);
                double d = game.world.RayIntersect(from, new Vec3(0, -1, 0), out RayHit hit) ? hit.distance : -1;
                if (!KillRing20.Place(feet.y, d, out float y)) return;
                feet.y = y;
            }
            ringScales[ringAskCount] = scale;
            ringAsks[ringAskCount++] = feet;
        }

        readonly SurvivalGame game;
        readonly Flash[] flashes = new Flash[UnityTuning.SpawnFlashPool];
        readonly MaterialPropertyBlock[] ringBlocks = new MaterialPropertyBlock[UnityTuning.SpawnFlashPool];
        readonly MaterialPropertyBlock[] coreBlocks = new MaterialPropertyBlock[UnityTuning.SpawnFlashPool];
        readonly Vec3[] asks = new Vec3[UnityTuning.SpawnFlashPool];
        int askCount, next;
        readonly Mesh ring, quad;
        readonly Mesh ring18; // NIGHT18 backlog 3: the DANGER ring's wider band (UnityTuning.DangerRingWidth)
        RenderParams ringRp, coreRp;
        readonly Vector4 danger; // linear
        static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

        /// <summary>Arrivals flashed since boot (a Play check reads it).</summary>
        public int Shown { get; private set; }

        public SpawnFlashes(SurvivalGame game)
        {
            this.game = game;
            danger = Look.C(game.cfg, "colors.enemy").linear;
            for (int i = 0; i < flashes.Length; i++)
            {
                flashes[i].age = double.MaxValue;
                ringBlocks[i] = new MaterialPropertyBlock();
                coreBlocks[i] = new MaterialPropertyBlock();
            }
            ring = FxDraw.Ring(1 - UnityTuning.SpawnRingWidth, 1, UnityTuning.SpawnRingSegments, "Spawn ring");
            ring18 = FxDraw.Ring(1 - UnityTuning.DangerRingWidth, 1, UnityTuning.SpawnRingSegments, "Danger ring");
            var probe = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad = probe.GetComponent<MeshFilter>().sharedMesh;
            UnityEngine.Object.Destroy(probe);
            Material ringMat = FxDraw.Additive(game.mats.neon, "Spawn ring");
            ringMat.SetFloat("_Cull", 0);
            ringRp = new RenderParams(ringMat) { layer = 0, shadowCastingMode = ShadowCastingMode.Off, receiveShadows = false };
            Material coreMat = FxDraw.Additive(game.mats.neon, "Spawn core");
            coreMat.SetFloat("_Cull", 0);
            coreMat.SetTexture("_BaseMap", GlowTexture());
            coreRp = new RenderParams(coreMat) { layer = 0, shadowCastingMode = ShadowCastingMode.Off, receiveShadows = false };
            // round 5: the kill burst's material (the core's recipe: additive, textured, both faces) and its blocks, made at load
            Material burstMat = FxDraw.Additive(game.mats.neon, "Kill burst");
            burstMat.SetFloat("_Cull", 0);
            burstMat.SetTexture("_BaseMap", StreakTexture());
            burstRp = new RenderParams(burstMat) { layer = 0, shadowCastingMode = ShadowCastingMode.Off, receiveShadows = false };
            for (int i = 0; i < burstBlocks.Length; i++) burstBlocks[i] = new MaterialPropertyBlock();
            // NIGHT18 U3-31.3: tonight's burst (the same recipe, the filled-core texture) and the crush disc (a copy of the neon
            // material blending Zero / OneMinusSrcColor: the world behind x (1 - colour); no depth write, both faces, drawn
            // before the additive bursts), made at load
            Material burst18Mat = FxDraw.Additive(game.mats.neon, "Kill flash");
            burst18Mat.SetFloat("_Cull", 0);
            burst18Mat.SetTexture("_BaseMap", KillFlashTexture());
            burst18Rp = new RenderParams(burst18Mat) { layer = 0, shadowCastingMode = ShadowCastingMode.Off, receiveShadows = false };
            var discMat = new Material(game.mats.neon) { name = "Kill disc", renderQueue = (int)RenderQueue.Transparent - 1 };
            discMat.SetFloat("_SrcBlend", (float)BlendMode.Zero);
            discMat.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcColor);
            discMat.SetFloat("_ZWrite", 0);
            discMat.SetFloat("_Cull", 0);
            discMat.SetTexture("_BaseMap", DiscTexture());
            discRp = new RenderParams(discMat) { layer = 0, shadowCastingMode = ShadowCastingMode.Off, receiveShadows = false };
            game.events.On<SpawnEvent>("enemySpawn", e =>
            {
                if (askCount < asks.Length) asks[askCount++] = e.position; // only queued (inside the sim step)
            });
        }

        /// <summary>
        /// NIGHT17 round 5: the kill bursts' 2 frames (full, then half) x FLASH INTENSITY, white-hot, facing the camera.
        /// NIGHT18 U3-31.3: each burst's own frames, brightness and texture (tonight's: 2 full + 2 fading, the filled core);
        /// the crush disc behind a frame's kills on the same frames; both reported to the reel's probe (FeelFx.Burst, M2).
        /// </summary>
        void RenderBursts()
        {
            Transform cam = game.cam != null ? game.cam.transform : null;
            if (cam == null) return;
            double fi = game.settings.View.flashIntensity;
            for (int i = 0; i < bursts.Length; i++)
            {
                ref Burst b = ref bursts[i];
                if (!b.live) continue;
                int f = FeelClock.FrameCount - b.frame0; // NIGHT63 D1: frame ages on FeelClock.FrameCount (a same-run held frame ages nothing)
                float k = CrunchMath.BurstStrength(f, b.full, b.fade, fi); // NIGHT17's (1, 1): StarStrength(f, 2, 0.5, fi)
                if (f >= b.full + b.fade) b.live = false;
                if (k <= 0) continue;
                float v = b.hdr * k;
                RenderParams rp = b.n18 ? burst18Rp : burstRp;
                burstBlocks[i].SetVector(BaseColor, new Vector4(v, v, v, 1));
                rp.matProps = burstBlocks[i];
                rp.worldBounds = new Bounds(b.at, Vector3.one * (b.size + 0.1f));
                Graphics.RenderMesh(rp, quad, 0, Matrix4x4.TRS(b.at, cam.rotation * Quaternion.Euler(0, 0, b.roll), Vector3.one * b.size));
                FeelProbe.EffectWorld(FeelFx.Burst, b.at, b.size * 0.5f * b.visible, k); // NIGHT18: the reel's probe (M2)
            }
            if (!disc.live) return;
            int df = FeelClock.FrameCount - disc.frame0;
            float ds = UnityTuning.KillDiscStrength * CrunchMath.BurstStrength(df, UnityTuning.KillBurstFullFrames, UnityTuning.KillBurstFadeFrames, fi);
            if (df >= UnityTuning.KillBurstFullFrames + UnityTuning.KillBurstFadeFrames) disc.live = false;
            if (ds <= 0) return;
            // the world inside the disc x (1 - ds x the mask): Blend Zero OneMinusSrcColor (no hue, never lighter)
            discBlock.SetVector(BaseColor, new Vector4(ds, ds, ds, 1));
            discRp.matProps = discBlock;
            discRp.worldBounds = new Bounds(disc.at, Vector3.one * (2 * disc.radius + 0.1f));
            Graphics.RenderMesh(discRp, quad, 0, Matrix4x4.TRS(disc.at, cam.rotation, Vector3.one * (2 * disc.radius)));
            FeelProbe.EffectWorld(FeelFx.Burst, disc.at, disc.radius * UnityTuning.KillDiscVisibleShare, ds);
        }

        /// <summary>
        /// NIGHT18 U3-31.3: this frame's new bursts (queued by the kills of the step just run): the horde's scale on tonight's,
        /// the flash gate asked once for the frame (granted: tonight's frames and the crush disc; refused: NIGHT17's frames, no
        /// disc), the reel's `killFx` marker.
        /// </summary>
        void SpawnBursts(Transform cam)
        {
            int on = 0;
            for (int i = 0; i < burstAskCount; i++) if (burstAskOn[i]) on++;
            bool granted = false;
            if (on > 0)
            {
                double fi = game.settings.View.flashIntensity;
                granted = fi > 0 && game.flashGate.TryFlash(SurvivalGame.RealNow);
            }
            float horde = CrunchMath.HordeScale(on, UnityTuning.HordeBurstPerKill, UnityTuning.HordeBurstMax);
            float viewTan = game.cam != null ? 2 * Mathf.Tan(game.cam.fieldOfView * 0.5f * Mathf.Deg2Rad) : 1.5f;
            Vector3 eye = cam != null ? cam.position : Vector3.zero;
            Vector3 mid = Vector3.zero;
            float near = float.MaxValue;
            for (int i = 0; i < burstAskCount; i++)
            {
                ref Burst b = ref bursts[nextBurst];
                nextBurst = (nextBurst + 1) % bursts.Length;
                b.at = burstAskAt[i];
                b.size = burstAskSize[i];
                b.frame0 = FeelClock.FrameCount;
                b.live = true;
                burstHash ^= burstHash << 13;
                burstHash ^= burstHash >> 17;
                burstHash ^= burstHash << 5;
                b.roll = (burstHash & 0xffff) / 65536f * 360f;
                bool n18 = burstAskOn[i] && granted;
                b.n18 = n18;
                b.full = n18 ? UnityTuning.KillBurstFullFrames : UnityTuning.KillBurstFullFramesN17;
                b.fade = n18 ? UnityTuning.KillBurstFadeFrames : UnityTuning.KillBurstFadeFramesN17;
                b.hdr = n18 ? UnityTuning.KillBurstHdr : UnityTuning.KillBurstHdrN17;
                b.visible = n18 ? UnityTuning.KillBurstVisibleShare : UnityTuning.KillBurstVisibleShareN17;
                if (!n18) continue;
                // the horde grows a far burst; a near one stays at its cap (KillBurstMaxAngle x the distance)
                float dist = Vector3.Distance(eye, b.at);
                b.size = Mathf.Min(b.size * horde, Mathf.Max(b.size, UnityTuning.KillBurstMaxAngle * dist));
                mid += b.at;
                near = Mathf.Min(near, dist);
            }
            PostFx post = game.lightAndLens != null ? game.lightAndLens.post : null;
            bool crushing = post != null && post.CrushEvNow() < 0; // a headshot / punch / multikill crush darkens the whole frame already
            if (granted && on > 0 && cam != null && !crushing)
            {
                // round 1 (b3 M1 2.60: the soft disc behind the body hid under the burst's bloom): one hard-edged crush disc IN
                // FRONT of the frame's kills (at the nearest burst's depth, half way to the eye: the world, the floor and the
                // body behind it all drop; the silhouette and the bursts draw over it after), around them all: KillDiscBodies x
                // each body's height on screen past its offset from the middle, KillDiscMinScreen..MaxScreen of the screen's height
                mid /= on;
                Vector3 toMid = mid - eye;
                float midDist = Mathf.Max(0.1f, toMid.magnitude);
                toMid = toMid.sqrMagnitude > 1e-6f ? toMid / midDist : cam.forward;
                float share = 0; // the disc's radius as a share of the screen's height
                for (int k = 0, j = nextBurst - burstAskCount; k < burstAskCount; k++, j++)
                {
                    int slot = (j % bursts.Length + bursts.Length) % bursts.Length;
                    ref Burst b = ref bursts[slot];
                    if (!b.n18) continue;
                    float bd = Mathf.Max(0.1f, Vector3.Distance(eye, b.at));
                    float off = Vector3.Distance(b.at, mid) / (midDist * viewTan);
                    float bodyShare = burstAskBody[k] * UnityTuning.KillBurstDepthShare / (bd * viewTan);
                    share = Mathf.Max(share, off + 0.5f * UnityTuning.KillDiscBodies * bodyShare);
                }
                share = Mathf.Clamp(share, 0.5f * UnityTuning.KillDiscMinScreen, 0.5f * UnityTuning.KillDiscMaxScreen);
                float depth = Mathf.Min(near, midDist);
                disc.at = eye + toMid * depth;
                disc.radius = share * depth * viewTan;
                disc.frame0 = FeelClock.FrameCount;
                disc.live = true;
            }
            if (burstAskCount > 0) FeelProbe.Event(KillFxProbeEvent);
            burstAskCount = 0;
        }

        /// <summary>A soft disc (made once): a bright core and a glow falling to black at the edge (additive: invisible).</summary>
        static Texture2D GlowTexture()
        {
            int n = UnityTuning.SpawnCoreTexture;
            var tex = new Texture2D(n, n, TextureFormat.RGBA32, false, true) { name = "Spawn core", wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
            var px = new Color32[n * n];
            for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                double dx = (x + 0.5) / n * 2 - 1, dy = (y + 0.5) / n * 2 - 1;
                double r = Math.Sqrt(dx * dx + dy * dy);
                double glow = Math.Max(0, 1 - r);
                double core = Math.Max(0, 1 - r * 3);
                byte v = (byte)Math.Round(Math.Min(1, (glow * glow + core) / 2) * 255);
                px[y * n + x] = new Color32(v, v, v, 255);
            }
            tex.SetPixels32(px);
            tex.Apply(false, true);
            return tex;
        }

        public void Clear()
        {
            askCount = 0;
            ringAskCount = 0;
            burstAskCount = 0;
            headAskCount = 0;
            for (int i = 0; i < bursts.Length; i++) bursts[i].live = false;
            for (int i = 0; i < flashes.Length; i++) flashes[i].age = double.MaxValue;
            disc.live = false; // NIGHT18
        }

        /// <summary>NIGHT17 round 2: one tiny draw of the ring and the core (their real materials and meshes) in front of `cam`.</summary>
        public void Warm(Camera cam)
        {
            if (cam == null) return;
            Matrix4x4 m = CrunchWarm.Matrix(cam);
            ringBlocks[0].SetVector(BaseColor, Vector4.zero);
            ringRp.matProps = ringBlocks[0];
            ringRp.worldBounds = CrunchWarm.Bounds(cam);
            Graphics.RenderMesh(ringRp, ring, 0, m);
            coreBlocks[0].SetVector(BaseColor, Vector4.zero);
            coreRp.matProps = coreBlocks[0];
            coreRp.worldBounds = CrunchWarm.Bounds(cam);
            Graphics.RenderMesh(coreRp, quad, 0, m);
            burstBlocks[0].SetVector(BaseColor, Vector4.zero); // round 5: the kill burst's own draw
            burstRp.matProps = burstBlocks[0];
            burstRp.worldBounds = CrunchWarm.Bounds(cam);
            Graphics.RenderMesh(burstRp, quad, 0, m);
            burst18Rp.matProps = burstBlocks[0]; // NIGHT18: tonight's burst and the crush disc (a zero colour: no change)
            burst18Rp.worldBounds = CrunchWarm.Bounds(cam);
            Graphics.RenderMesh(burst18Rp, quad, 0, m);
            discBlock.SetVector(BaseColor, new Vector4(0, 0, 0, 1));
            discRp.matProps = discBlock;
            discRp.worldBounds = CrunchWarm.Bounds(cam);
            Graphics.RenderMesh(discRp, quad, 0, m);
        }

        /// <summary>Once a frame (EnemyLook.Frame): start the queued arrivals, grow and fade the rings, the cores' 2 frames.</summary>
        public void Render(bool simulating)
        {
            bool playing = game.flow == Flow.Playing || game.flow == Flow.Down;
            for (int i = 0; i < askCount; i++)
            {
                if (!playing) break;
                ref Flash f = ref flashes[next];
                next = (next + 1) % flashes.Length;
                f.at = WebSpace.ToUnity(asks[i]);
                f.age = 0;
                f.frame0 = FeelClock.FrameCount;
                f.core = true;
                f.scale = 1;
                f.head = false;
                Shown++;
            }
            askCount = 0;
            for (int i = 0; i < ringAskCount; i++)
            {
                if (!playing) break;
                ref Flash f = ref flashes[next];
                next = (next + 1) % flashes.Length;
                f.at = ringAsks[i];
                f.age = 0;
                f.frame0 = FeelClock.FrameCount;
                f.core = false;
                f.scale = ringScales[i];
                f.head = false;
            }
            ringAskCount = 0;
            for (int i = 0; i < headAskCount; i++)
            {
                if (!playing) break;
                ref Flash f = ref flashes[next];
                next = (next + 1) % flashes.Length;
                f.at = headAsks[i];
                f.age = 0;
                f.frame0 = FeelClock.FrameCount;
                f.core = false;
                f.scale = 1;
                f.head = true;
            }
            headAskCount = 0;
            Transform cam = game.cam != null ? game.cam.transform : null;
            if (playing) SpawnBursts(cam); // NIGHT18 U3-31.3: tonight's or NIGHT17's, the horde's scale, the gate, the crush disc
            burstAskCount = 0;
            if (!playing) return;
            RenderBursts();
            float dt = simulating ? (float)game.FrameDt : 0;
            double fi = game.settings.View.flashIntensity;
            // NIGHT18 U3-31.9 backlog 3 (FeelAB on): the DANGER ring and core under the bloom threshold (their hue holds), the band wider
            bool dangerOn = FeelAB.On;
            float ringHdr = UnityTuning.Pick(dangerOn, UnityTuning.DangerRingHdr, UnityTuning.DangerRingHdrN17);
            float coreHdr = UnityTuning.Pick(dangerOn, UnityTuning.DangerCoreHdr, UnityTuning.DangerCoreHdrN17);
            Mesh dangerRing = dangerOn ? ring18 : ring;
            for (int i = 0; i < flashes.Length; i++)
            {
                ref Flash f = ref flashes[i];
                if (f.head)
                {
                    // round 5: the headshot's white ring at the head, facing the camera, frame time (it plays in the freeze)
                    if (f.age >= UnityTuning.HeadRingTime || cam == null) continue;
                    double th = f.age / UnityTuning.HeadRingTime;
                    f.age += dt;
                    double wh = 1 - th;
                    float rh = (float)(UnityTuning.HeadRingStart + (UnityTuning.HeadRingEnd - UnityTuning.HeadRingStart) * (1 - wh * wh * wh));
                    // round 6: at range the ring keeps HeadRingMinScreen of the screen's height (as its end size)
                    float ringWant = Vector3.Distance(cam.position, f.at) * 2 * Mathf.Tan(game.cam.fieldOfView * 0.5f * Mathf.Deg2Rad) * UnityTuning.HeadRingMinScreen;
                    if (ringWant > UnityTuning.HeadRingEnd * 2) rh *= ringWant / (float)(UnityTuning.HeadRingEnd * 2);
                    float vh = (float)(UnityTuning.HeadRingHdr * wh * wh);
                    ringBlocks[i].SetVector(BaseColor, new Vector4(vh, vh, vh, 1));
                    ringRp.matProps = ringBlocks[i];
                    ringRp.worldBounds = new Bounds(f.at, Vector3.one * (2 * rh + 0.1f));
                    Graphics.RenderMesh(ringRp, ring, 0, Matrix4x4.TRS(f.at, Quaternion.FromToRotation(Vector3.up, -cam.forward), new Vector3(rh, 1, rh)));
                    FeelProbe.EffectWorld(FeelFx.Ring, f.at, rh, (float)(wh * wh)); // NIGHT18: the reel's probe (M2)
                    continue;
                }
                if (f.age >= UnityTuning.SpawnRingTime) continue;
                double t = f.age / UnityTuning.SpawnRingTime;
                f.age += dt;
                double w = 1 - t;
                float r = (float)(UnityTuning.SpawnRingStart + (UnityTuning.SpawnRingEnd - UnityTuning.SpawnRingStart) * (1 - w * w * w)) * f.scale;
                ringBlocks[i].SetVector(BaseColor, danger * (float)(ringHdr * w * w));
                ringRp.matProps = ringBlocks[i];
                Vector3 ground = f.at + Vector3.up * UnityTuning.SpawnRingLift;
                ringRp.worldBounds = new Bounds(ground, new Vector3(2 * r + 0.1f, 0.2f, 2 * r + 0.1f));
                Graphics.RenderMesh(ringRp, dangerRing, 0, Matrix4x4.TRS(ground, Quaternion.identity, new Vector3(r, 1, r)));
                if (!f.core) FeelProbe.EffectBounds(FeelFx.Ring, new Bounds(ground, new Vector3(2 * r, 0.02f, 2 * r)), (float)(w * w)); // NIGHT18: a kill's floor ring (M2)
                float core = f.core ? CrunchMath.StarStrength(FeelClock.FrameCount - f.frame0, 2, 0.5, fi) : 0;
                if (core <= 0 || cam == null) continue;
                Vector3 at = f.at + Vector3.up * UnityTuning.SpawnCoreHeight;
                coreBlocks[i].SetVector(BaseColor, danger * (coreHdr * core) + new Vector4(0, 0, 0, 1));
                coreRp.matProps = coreBlocks[i];
                coreRp.worldBounds = new Bounds(at, Vector3.one * (UnityTuning.SpawnCoreSize + 0.1f));
                Graphics.RenderMesh(coreRp, quad, 0, Matrix4x4.TRS(at, cam.rotation, Vector3.one * UnityTuning.SpawnCoreSize));
            }
        }
    }

    /// <summary>
    /// NIGHT17 round 2 (builder X): the warm-up's tiny draw (the first kill's 122 ms hitch: a pipeline state is made on the
    /// first draw of a shader variant x render state x vertex layout): a point UnityTuning.CrunchWarmDistance m in front of
    /// the camera at UnityTuning.CrunchWarmScale (sub-pixel), drawn for one frame by each path's own material and mesh.
    /// </summary>
    public static class CrunchWarm
    {
        public static Vector3 Point(Camera cam) => cam.transform.position + cam.transform.forward * UnityTuning.CrunchWarmDistance;

        public static Matrix4x4 Matrix(Camera cam) => Matrix4x4.TRS(Point(cam), cam.transform.rotation, Vector3.one * UnityTuning.CrunchWarmScale);

        public static Bounds Bounds(Camera cam) => new Bounds(Point(cam), Vector3.one * 0.1f);
    }

    /// <summary>
    /// NIGHT17 round 1 (builder X): one colour's axis-aligned cubes as ONE dynamic mesh, drawn with Graphics.RenderMesh (no
    /// GPU instancing: build 2's Player drew nothing through Graphics.RenderMeshInstanced: no block, no death-burst cube, no
    /// kill ring; RenderMesh drew the corpse and the muzzle star). The cubes' corners are written on the CPU each frame
    /// into preallocated arrays (8 corners, 36 indices a cube), so a frame allocates nothing. Both faces draw (the
    /// material's _Cull Off), so the winding can't hide a face. Round 1 (2): any template mesh (a line's quad, a shell) with
    /// a matrix per item (Add(Matrix4x4)): its vertices transformed on the CPU, its triangles repeated.
    /// </summary>
    public sealed class CubeBatch
    {
        readonly Vector3[] verts;
        readonly int[] tris;
        readonly Mesh mesh;
        readonly int cap;
        readonly Vector3[] shape; // the template's vertices (the unit cube's corners by default)
        readonly int per, trisPer;
        int count;
        Vector3 min, max;
        static readonly Vector3[] Corners =
        {
            new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(0.5f, -0.5f, -0.5f), new Vector3(0.5f, 0.5f, -0.5f), new Vector3(-0.5f, 0.5f, -0.5f),
            new Vector3(-0.5f, -0.5f, 0.5f), new Vector3(0.5f, -0.5f, 0.5f), new Vector3(0.5f, 0.5f, 0.5f), new Vector3(-0.5f, 0.5f, 0.5f),
        };
        // Unity's front faces wind clockwise: -z, +z, -x, +x, -y, +y.
        static readonly int[] CubeTris = { 0, 3, 2, 0, 2, 1, 5, 6, 7, 5, 7, 4, 4, 7, 3, 4, 3, 0, 1, 2, 6, 1, 6, 5, 1, 5, 4, 1, 4, 0, 3, 7, 6, 3, 6, 2 };

        public CubeBatch(int capacity, string name, Mesh template = null)
        {
            cap = Math.Max(1, capacity);
            shape = template != null ? template.vertices : Corners;
            int[] shapeTris = template != null ? template.triangles : CubeTris;
            per = shape.Length;
            trisPer = shapeTris.Length;
            verts = new Vector3[cap * per];
            tris = new int[cap * trisPer];
            for (int i = 0; i < cap; i++)
                for (int k = 0; k < trisPer; k++) tris[i * trisPer + k] = shapeTris[k] + i * per;
            mesh = new Mesh { name = name };
            if (cap * per > 65535) mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.MarkDynamic();
        }

        /// <summary>Cubes this frame.</summary>
        public int Count => count;

        public void Begin()
        {
            count = 0;
            min = Vector3.positiveInfinity;
            max = Vector3.negativeInfinity;
        }

        /// <summary>An axis-aligned cube of side `size` at `at` (past the capacity: dropped).</summary>
        public void Add(Vector3 at, float size)
        {
            if (count >= cap || !(size > 0)) return;
            int b = count * per;
            for (int k = 0; k < per; k++)
            {
                Vector3 v = at + shape[k] * size;
                verts[b + k] = v;
                min = Vector3.Min(min, v);
                max = Vector3.Max(max, v);
            }
            count++;
        }

        /// <summary>The template under `m` (past the capacity: dropped).</summary>
        public void Add(Matrix4x4 m)
        {
            if (count >= cap) return;
            int b = count * per;
            for (int k = 0; k < per; k++)
            {
                Vector3 v = m.MultiplyPoint3x4(shape[k]);
                verts[b + k] = v;
                min = Vector3.Min(min, v);
                max = Vector3.Max(max, v);
            }
            count++;
        }

        /// <summary>Draw this frame's cubes (one Graphics.RenderMesh).</summary>
        public void Draw(RenderParams rp)
        {
            if (count == 0) return;
            mesh.Clear();
            mesh.SetVertices(verts, 0, count * per);
            mesh.SetTriangles(tris, 0, count * trisPer, 0, false);
            var bounds = new Bounds((min + max) * 0.5f, max - min);
            mesh.bounds = bounds;
            rp.worldBounds = bounds;
            Graphics.RenderMesh(rp, mesh, 0, Matrix4x4.identity);
        }
    }
}
