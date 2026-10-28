// NIGHT49 builder X (design.n49.fx49, a view row: no sim bit; NIGHT49.md G5; GG 2026-10-19: "The effects are inconsistent it all needs to be
// unified"). THE IMPACT KIT: before it every effect set its own colours and timings (Research/2026-10-19-wave49/CODE-FACTS.md section 5) and a
// player's gun hit drew FOUR pictures on top of each other (HitSparks' star + chips, fx33's cubes, feel43's star + sparks, fx47's sparks). With
// fx49 on, every hit, kill, parry, enemy blow and boss weak point is ONE burst from this kit, and the effects with a shape of their own (the LT
// blast, the THUNDERCLAP's cone and boost, the TORCH, the gauntlet rings, the shock ring) read the kit's colours, tiers and timing:
//   - the tier (light / heavy / critical) from the damage (more damage = bigger, brighter, sharper: UNIFY.md section 4);
//   - the owner (player cyan-white, gauntlet pink, enemy red-orange, parry yellow) from Look49's role tokens (the one palette);
//   - the shape from the owner (player round flare + ring, gauntlet three-tone ring, enemy shards, parry four-point star);
//   - one timing table (UnityTuning.Fx49.cs: rise / hold / fade in whole sim steps, played on frame time so a burst plays through the sim's
//     hitstop; the hitstop itself stays the sim's);
//   - the guards: nothing reaches the reticle's centre box (18 px at 1080p); a sprite inside the wash box (the middle 20 %) draws dimmer and smaller.
// Off (the row off, -skygrindOff49 or an earlier off switch): nothing hooks, nothing draws, every effect keeps its own look. View randomness
// from its own Mulberry32 (salted; never the sim's stream). New gameplay events: none (the kit only listens).
using System;
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    /// <summary>NIGHT49 X: the kit's rules (the row, the cells, the colours, the envelope, the guards) and its probe.</summary>
    public static class Fx49
    {
        /// <summary>The reel's off / on passes (null: design.n49.fx49).</summary>
        public static bool? Force;

        static GameConfig onFor;
        static bool onRow;
        static double hz = 120, hitHeavy = UnityTuning.Fx49HitHeavyAt, hitCrit = UnityTuning.Fx49HitCriticalAt, hurtHeavy = UnityTuning.Fx49HurtHeavyAt, hurtCrit = UnityTuning.Fx49HurtCriticalAt;

        /// <summary>design.n49.fx49 (read once per config: no string built per frame) or the reel's Force.</summary>
        public static bool On(GameConfig cfg)
        {
            if (!ReferenceEquals(cfg, onFor))
            {
                onFor = cfg;
                onRow = cfg != null && NightRows.ViewOn(cfg, "fx49");
                if (cfg != null)
                {
                    hz = cfg.movement.loop.simHz > 0 ? cfg.movement.loop.simHz : 120;
                    hitHeavy = Num(cfg, "hitHeavyAt", UnityTuning.Fx49HitHeavyAt);
                    hitCrit = Num(cfg, "hitCriticalAt", UnityTuning.Fx49HitCriticalAt);
                    hurtHeavy = Num(cfg, "hurtHeavyAt", UnityTuning.Fx49HurtHeavyAt);
                    hurtCrit = Num(cfg, "hurtCriticalAt", UnityTuning.Fx49HurtCriticalAt);
                }
            }
            if (Force.HasValue) return Force.Value;
            return onRow;
        }

        static double Num(GameConfig cfg, string key, double fallback)
        {
            string p = "design.n49.fx49.numbers." + key;
            try { return cfg.Has(p) ? cfg.Num(p) : fallback; }
            catch (Exception) { return fallback; }
        }

        public static Fx49Cell Cell(Fx49Owner o, Fx49Tier t) => UnityTuning.Fx49Cells[(int)o * 3 + (int)t];

        public static Fx49Shape ShapeOf(Fx49Owner o) => o == Fx49Owner.Player ? Fx49Shape.Round : o == Fx49Owner.Gauntlet ? Fx49Shape.Ring : o == Fx49Owner.Enemy ? Fx49Shape.Shard : Fx49Shape.Star;

        /// <summary>The owner's role colour (LDR, from Look49's tokens; the gauntlet's pink is fx47's body tone: no Look49 token yet, see the report).</summary>
        public static Color Hue(Fx49Owner o) => Look.Hex(o == Fx49Owner.Player ? UnityTuning.Look49UsableHex : o == Fx49Owner.Gauntlet ? UnityTuning.Fx47BodyHex : o == Fx49Owner.Enemy ? UnityTuning.Look49DangerHex : UnityTuning.Look49ParryHex);

        /// <summary>The owner's three tones at the critical glow (the tiers dim them by k: one colour bucket each, 12 for the four owners).</summary>
        public static void Tones(Fx49Owner o, out Color core, out Color body, out Color rim)
        {
            Color h = Hue(o);
            float g = Cell(o, Fx49Tier.Critical).glow;
            core = Color.Lerp(h, Color.white, Cell(o, Fx49Tier.Critical).core) * g;
            body = h * g;
            rim = h * (g * UnityTuning.Fx49RimShare);
        }

        /// <summary>A tier's brightness share of the owner's critical glow (the k a sprite of it draws at).</summary>
        public static float GlowK(Fx49Owner o, Fx49Tier t) => Cell(o, t).glow / Cell(o, Fx49Tier.Critical).glow;

        /// <summary>The sim's step rate the timing table counts in (cfg.movement.loop.simHz).</summary>
        public static double Hz => hz;

        public static float RiseS(Fx49Tier t) => (float)(UnityTuning.Fx49RiseSteps[(int)t] / hz);
        public static float PeakEndS(Fx49Tier t) => (float)((UnityTuning.Fx49RiseSteps[(int)t] + UnityTuning.Fx49HoldSteps[(int)t]) / hz);
        public static float LifeS(Fx49Tier t) => (float)((UnityTuning.Fx49RiseSteps[(int)t] + UnityTuning.Fx49HoldSteps[(int)t] + UnityTuning.Fx49FadeSteps[(int)t]) / hz);

        /// <summary>The one envelope at `age` s: a snap-in over rise, 1 through hold, a quadratic ease-out over fade; 0 after.</summary>
        public static float Env(Fx49Tier t, float age)
        {
            if (age < 0) return 0;
            double s = age * hz;
            int r = UnityTuning.Fx49RiseSteps[(int)t], h = UnityTuning.Fx49HoldSteps[(int)t], f = UnityTuning.Fx49FadeSteps[(int)t];
            if (s < r) return (float)Math.Max(0.35, s / r); // the snap: a third lit on its first frame (never a fade-in)
            if (s < r + h) return 1;
            double u = (s - r - h) / f;
            if (u >= 1) return 0;
            return (float)((1 - u) * (1 - u));
        }

        /// <summary>The growth share (0 -> 1, ease-out) a ring or a spark has travelled at `age` s (reaches 1 half way through the fade).</summary>
        public static float Grow(Fx49Tier t, float age)
        {
            float end = PeakEndS(t) + 0.5f * (float)(UnityTuning.Fx49FadeSteps[(int)t] / hz);
            float k = Mathf.Clamp01(age / Mathf.Max(1e-4f, end));
            return 1 - (1 - k) * (1 - k);
        }

        public static Fx49Tier HitTier(double damage) => damage >= hitCrit ? Fx49Tier.Critical : damage >= hitHeavy ? Fx49Tier.Heavy : Fx49Tier.Light;
        public static Fx49Tier HurtTier(double amount) => amount >= hurtCrit ? Fx49Tier.Critical : amount >= hurtHeavy ? Fx49Tier.Heavy : Fx49Tier.Light;
        public static Fx49Owner OwnerOf(string weapon) => weapon == "gauntlet" ? Fx49Owner.Gauntlet : Fx49Owner.Player;
        public static Fx49Tier Up(Fx49Tier t) => t == Fx49Tier.Light ? Fx49Tier.Heavy : Fx49Tier.Critical;

        /// <summary>
        /// The reticle guard for a sprite of `size` m at `at`: false when it must not draw (behind the eye, or its centre inside the reticle
        /// box); `size` shrinks so its edge stays outside the reticle box (18 px at 1080p). (The wash box's dimming is the caller's: Place.)
        /// </summary>
        public static bool Guard(Camera cam, Vector3 at, ref float size, ref float k)
        {
            if (cam == null) return false;
            Vector3 v = cam.WorldToViewportPoint(at);
            if (v.z <= 0.05f) return false;
            float h = Screen.height > 0 ? Screen.height : 1080, w = Screen.width > 0 ? Screen.width : 1920;
            float px = h / 1080f; // 1080p px -> this screen's px
            float dx = (v.x - 0.5f) * w, dy = (v.y - 0.5f) * h;
            float d = Mathf.Sqrt(dx * dx + dy * dy);
            float clear = UnityTuning.Fx49ReticlePx * px;
            if (d <= clear) return false;
            // the sprite's half span in px at its depth
            float half = size * 0.5f / (v.z * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad)) * (h * 0.5f);
            if (d - half < clear && half > 1e-4f) size *= Mathf.Max(0, (d - clear) / half);
            return size > 1e-4f && k > 0;
        }

        /// <summary>
        /// Where `at` sits on screen: false behind the eye. `clearM` = the reticle box's radius (18 px at 1080p) in m at `at`'s depth (a ring
        /// round a hitscan hit, which is always ON the aim, keeps its band outside it; a spark starts beyond it); `inWash` = inside the wash box.
        /// </summary>
        public static bool Place(Camera cam, Vector3 at, out float clearM, out bool inWash)
        {
            clearM = 0;
            inWash = false;
            if (cam == null) return false;
            Vector3 v = cam.WorldToViewportPoint(at);
            if (v.z <= 0.05f) return false;
            float h = Screen.height > 0 ? Screen.height : 1080;
            clearM = UnityTuning.Fx49ReticlePx * (h / 1080f) / (h * 0.5f) * v.z * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
            inWash = Mathf.Abs(v.x - 0.5f) < UnityTuning.Fx49WashHalf && Mathf.Abs(v.y - 0.5f) < UnityTuning.Fx49WashHalf;
            return true;
        }

        /// <summary>
        /// Round 2 (D-17; the lead's a249 on n49-r1: kestrel-charge's WASH box +0.51 OFF / +0.52 ON, the KESTREL cannon's muzzle blast and shock
        /// ring at the eye): the largest span (m) a muzzle sprite at `at` may have and keep its edge outside the wash box (the middle 20 %): its
        /// distance off the view axis less the box's half width at its depth (the box's wider, horizontal half: conservative). 0 = none fits.
        /// </summary>
        public static float WashCap(Camera cam, Vector3 at)
        {
            if (cam == null) return 0;
            Transform ct = cam.transform;
            Vector3 rel = at - ct.position;
            float z = Vector3.Dot(rel, ct.forward);
            if (z <= 0.01f) return 0;
            float off = Vector3.ProjectOnPlane(rel, ct.forward).magnitude;
            float halfY = 2f * UnityTuning.Fx49WashHalf * z * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
            float halfX = halfY * Mathf.Max(1f, cam.aspect);
            return 2f * Mathf.Max(0f, off - halfX);
        }

        /// <summary>A tier's wash-box share of brightness and size (round 2: the critical's least: its read is OUTSIDE the box, bigger and sharper).</summary>
        public static float WashK(Fx49Tier t) => UnityTuning.Fx49WashKTier[(int)t];
        public static float WashSize(Fx49Tier t) => UnityTuning.Fx49WashSizeTier[(int)t];

        // ---- the probe (the a249 reel): per effect its tier, owner and the frame it peaked ----
        public struct Seen
        {
            public Fx49Tier tier;
            public Fx49Owner owner;
            public int frame, peakFrame, count;
        }

        public static readonly Dictionary<string, Seen> Probe = new Dictionary<string, Seen>();
        public static int Bursts, Sprites;

        /// <summary>An effect of the kit spawned now (frame = Time.frameCount); its peak frame is the first frame past its rise.</summary>
        public static void Note(string effect, Fx49Tier t, Fx49Owner o)
        {
            int f = Time.frameCount;
            float dt = Mathf.Max(1e-3f, Time.unscaledDeltaTime);
            int rise = Mathf.Max(0, Mathf.CeilToInt(RiseS(t) / dt) - 1);
            Probe.TryGetValue(effect, out Seen s);
            Probe[effect] = new Seen { tier = t, owner = o, frame = f, peakFrame = f + rise, count = s.count + 1 };
        }

        public static string OwnerName(Fx49Owner o) => o == Fx49Owner.Player ? "player cyan-white" : o == Fx49Owner.Gauntlet ? "gauntlet pink" : o == Fx49Owner.Enemy ? "enemy red-orange" : "parry yellow";
    }

    /// <summary>NIGHT49 X: the kit's own bursts (one per hit / kill / parry / enemy blow / boss weak point), pooled, drawn on frame time.</summary>
    public sealed class Fx49Kit
    {
        static Fx49Kit current;
        static SurvivalGame currentGame;

        /// <summary>The game's kit (made on the first frame the row is on; a new game builds a new one).</summary>
        public static Fx49Kit Of(SurvivalGame g)
        {
            if (g == null) return null;
            if (!ReferenceEquals(currentGame, g) || current == null) { currentGame = g; current = new Fx49Kit(g); }
            return current;
        }

        /// <summary>Once a frame after the camera (Fx47View.Render calls it whatever fx47's row): hooks once, draws the live bursts.</summary>
        public static void Frame(SurvivalGame g, double frameDt)
        {
            if (g == null || g.cfg == null || !Fx49.On(g.cfg)) return;
            Of(g)?.Render((float)frameDt);
        }

        struct Burst
        {
            public float age; // s (-1: free)
            public Vector3 at;
            public Fx49Owner owner;
            public Fx49Tier tier;
            public float roll, scale;
            public int enemy, frame;
            public byte kind; // 0 a burst, 1 a muzzle flare (no sparks, no ring), 2 a floor ring
            public Vector3 normal; // a floor ring's
            public bool kill; // NIGHT53 U polish53 (T-111): a kill's burst (its in-box brightness x killWash.burstK)
        }

        readonly SurvivalGame game;
        readonly Burst[] bursts = new Burst[UnityTuning.Fx49Pool];
        readonly Vector3[] dirs = new Vector3[UnityTuning.Fx49Pool * 10];
        int next;
        bool hooked, built;
        FxSprites flare, star, ring, trace;
        readonly Color[] core = new Color[4], body = new Color[4], rim = new Color[4];
        Mulberry32 rng = new Mulberry32(UnityTuning.Fx49SeedSalt);
        int parryFrame = -1;

        Fx49Kit(SurvivalGame g)
        {
            game = g;
            for (int i = 0; i < bursts.Length; i++) bursts[i].age = -1;
        }

        bool On => Fx49.On(game.cfg);

        /// <summary>NIGHT53 U polish53 (T-122, FirstShot53): the kit's sprites built at boot (4 Resources.Load, their materials and batches for
        /// every owner's tones), not on the first Spawn (the THUNDERCLAP's first shot's cone / boost: its hitch). Nothing hooked, nothing drawn.</summary>
        public void Prewarm53()
        {
            if (game != null && game.mats != null) Build();
        }

        public bool Built53 => built;

        void Build()
        {
            if (built) return;
            built = true;
            Material neon = game.mats.neon;
            flare = new FxSprites(neon, UnityTuning.KenneyFlare, UnityTuning.Fx49Pool * 4);
            star = new FxSprites(neon, UnityTuning.KenneyStar, UnityTuning.Fx49Pool);
            ring = new FxSprites(neon, UnityTuning.KenneyRing, UnityTuning.Fx49Pool * 3);
            trace = new FxSprites(neon, UnityTuning.KenneyTrace, UnityTuning.Fx49Pool * 10);
            for (int o = 0; o < 4; o++)
            {
                Fx49.Tones((Fx49Owner)o, out core[o], out body[o], out rim[o]);
                foreach (FxSprites s in new[] { flare, star, ring, trace })
                {
                    s.Warm(core[o]);
                    s.Warm(body[o]);
                    s.Warm(rim[o]);
                }
            }
        }

        public void Hook()
        {
            if (hooked || game.events == null) return;
            hooked = true;
            game.events.On<HitEvent>("hit", e => { if (On) OnHit(e); });
            game.events.On<KillEvent>("kill", e => { if (On) OnKill(e); });
            game.events.On<HitEvent>("parry", e => { if (On) OnParry(e); });
            game.events.On<HitEvent>(Parry36.Event, e => { if (On) OnParry(e); });
            game.events.On<HurtEvent>("hurt", e => { if (On) OnHurt(e); });
            // the LT blast's own hits (they come before its event in the step) carry no punch burst: the blast is one critical picture
            // (blast48's lesson: n48-r1 a748's f4 wash was the auras its hits spawned)
            game.events.On<Pulse27Event>("pulse27Blast", e =>
            {
                if (!On) return;
                int f = Time.frameCount;
                for (int i = 0; i < bursts.Length; i++)
                    if (bursts[i].age >= 0 && bursts[i].frame == f && bursts[i].owner == Fx49Owner.Gauntlet && bursts[i].kind == 0) bursts[i].age = -1;
            });
            game.OnNewRun += () => { for (int i = 0; i < bursts.Length; i++) bursts[i].age = -1; rng = new Mulberry32(game.runSeed ^ UnityTuning.Fx49SeedSalt); };
        }

        /// <summary>A burst of `owner` / `tier` at `at` (Unity space) for enemy `enemy` (-1: none): one per enemy a frame (a volley's pellets
        /// raise the burst's tier to the biggest, never stack a sheet of flares).</summary>
        public void Spawn(string effect, Vector3 at, Fx49Owner owner, Fx49Tier tier, int enemy = -1, byte kind = 0, Vector3 normal = default, float scale = 1)
        {
            Build();
            int f = Time.frameCount;
            if (enemy >= 0)
                for (int i = 0; i < bursts.Length; i++)
                    if (bursts[i].age >= 0 && bursts[i].enemy == enemy && bursts[i].frame == f && bursts[i].kind == 0)
                    {
                        if (tier > bursts[i].tier) bursts[i].tier = tier;
                        return;
                    }
            if (enemy < 0 && kind == 0 && tier == Fx49Tier.Critical)
                for (int i = 0; i < bursts.Length; i++)
                    if (bursts[i].age >= 0 && bursts[i].frame == f && bursts[i].kind == 0 && bursts[i].tier < tier && (bursts[i].at - at).sqrMagnitude < UnityTuning.Fx49MergeM * UnityTuning.Fx49MergeM)
                        bursts[i].age = -1; // a boss weak point over its own hit's burst: one picture
            Camera cam = game.cam;
            if (kind == 0 && cam != null)
            {
                // on the camera's side of the skin (Fusion43View's lesson), a short way
                Vector3 to = cam.transform.position - at;
                float d = to.magnitude;
                if (d > 1e-3f) at += to / d * Mathf.Min(0.3f, d * 0.3f);
            }
            int b = next;
            next = (next + 1) % bursts.Length;
            bursts[b] = new Burst { age = 0, at = at, owner = owner, tier = tier, roll = (float)(rng.Next() * Math.PI * 2), scale = scale, enemy = enemy, frame = f, kind = kind, normal = normal, kill = effect == "kill" };
            for (int i = 0; i < 10; i++)
            {
                Vector3 v = SprayMath.UnitSphere(rng.Next(), rng.Next());
                if (cam != null) v = (v + (cam.transform.position - at).normalized * 0.35f).normalized; // a lean toward the shooter (the sparks read on screen)
                dirs[b * 10 + i] = v;
            }
            Fx49.Bursts++;
            if (effect != null) Fx49.Note(effect, tier, owner);
            // round 3 (D-17; n49-r2 a249: the kill +0.32, the punch +0.31 ON): no light from a burst in the wash box (it lit the body and the
            // floor round the aim: the box's mean, not the burst), and none within Fx49LightNearM of the eye
            bool washLit = cam != null && Fx49.Place(cam, at, out float _, out bool inBox) && inBox;
            bool nearEye = cam != null && (at - cam.transform.position).sqrMagnitude < UnityTuning.Fx49LightNearM * UnityTuning.Fx49LightNearM;
            if (kind == 0 && tier != Fx49Tier.Light && owner != Fx49Owner.Enemy && !washLit && !nearEye) // (an enemy's blow lands at the eye: a light there would light the whole view)
            {
                LightPools lights = game.lightAndLens?.lights;
                lights?.Flash(at, Fx49.Hue(owner), tier == Fx49Tier.Critical ? UnityTuning.Fx49LightCritical : UnityTuning.Fx49LightHeavy, UnityTuning.Fx49LightRange[(int)tier], Fx49.PeakEndS(tier));
            }
        }

        // ---- the events ----

        void OnHit(HitEvent e)
        {
            if (e == null || e.owner != "player") return;
            if (e.weapon == "torch") return; // the TORCH's hits are its flame's (GunFx.Torch43.cs reads the kit's tones)
            if (e.killed) return; // the killing hit's picture is the kill's burst (one picture a moment)
            Fx49Owner o = Fx49.OwnerOf(e.weapon);
            Fx49Tier t = Fx49.HitTier(e.damage);
            if (e.weapon == "gauntlet" && !e.shock && t == Fx49Tier.Light) t = Fx49Tier.Heavy; // the PULSE punch: heavy (its wave's share stays light)
            if (e.headshot) t = Fx49.Up(t);
            string name = e.weapon == "revolver" ? (t == Fx49Tier.Light ? "kestrel-tap" : "kestrel-charge")
                : e.weapon == "shotgun" ? (t == Fx49Tier.Light ? "thunder-hit" : "thunder-slug")
                : e.weapon == "gauntlet" ? "pulse-punch" : e.weapon == "punch" ? "fist" : e.weapon;
            Spawn(name, WebSpace.ToUnity(e.position), o, t, e.enemy);
        }

        void OnKill(KillEvent e)
        {
            if (e == null || e.owner != "player") return;
            Fx49Tier t = e.kind == "boss" || e.headshot || e.melee ? Fx49Tier.Critical : Fx49Tier.Heavy;
            Spawn("kill", WebSpace.ToUnity(e.position) + Vector3.up * 0.2f, Fx49.OwnerOf(e.weapon), t, -1);
        }

        void OnParry(HitEvent e)
        {
            if (e == null || e.owner != "player") return;
            if (parryFrame == Time.frameCount) return; // "parry" and "parry36" on one press: one star
            parryFrame = Time.frameCount;
            Spawn("parry", WebSpace.ToUnity(e.position), Fx49Owner.Parry, Fx49Tier.Critical, -1);
        }

        void OnHurt(HurtEvent e)
        {
            if (e == null || e.owner != "player" || !(e.amount > 0) || game.cam == null) return;
            Transform ct = game.cam.transform;
            Vector3 dir = e.hasFrom ? WebSpace.ToUnity(e.from) - ct.position : ct.forward - ct.up * 0.5f;
            dir = dir.sqrMagnitude > 1e-6f ? dir.normalized : ct.forward;
            // never on the reticle: at least Fx49HurtMinDeg off the aim (pushed down-and-out when the blow came from straight ahead)
            float off = Vector3.Angle(ct.forward, dir);
            if (off < UnityTuning.Fx49HurtMinDeg)
            {
                Vector3 side = Vector3.ProjectOnPlane(dir, ct.forward);
                if (side.sqrMagnitude < 1e-6f) side = -ct.up;
                dir = Vector3.RotateTowards(ct.forward, side.normalized, UnityTuning.Fx49HurtMinDeg * Mathf.Deg2Rad, 0f);
            }
            Spawn("hurt", ct.position + dir * UnityTuning.Fx49HurtAhead, Fx49Owner.Enemy, Fx49.HurtTier(e.amount), -1, 0, default, UnityTuning.Fx49HurtScale);
        }

        // ---- the draw ----

        void Render(float dt)
        {
            Hook();
            Camera cam = game.cam;
            int live = 0;
            for (int i = 0; i < bursts.Length; i++) if (bursts[i].age >= 0) { live++; break; }
            if (live == 0 || cam == null || !built) { Fx49.Sprites = 0; return; }
            Transform ct = cam.transform;
            Vector3 eye = ct.position;
            flare.Begin();
            star.Begin();
            ring.Begin();
            trace.Begin();
            int n = 0;
            for (int i = 0; i < bursts.Length; i++)
            {
                if (bursts[i].age < 0) continue;
                Burst b = bursts[i];
                float a = b.age;
                bursts[i].age += dt;
                if (a > Fx49.LifeS(b.tier)) { bursts[i].age = -1; continue; }
                int o = (int)b.owner;
                Fx49Cell c = Fx49.Cell(b.owner, b.tier);
                float env = Fx49.Env(b.tier, a), grow = Fx49.Grow(b.tier, a);
                float k = env * Fx49.GlowK(b.owner, b.tier);
                float size = c.size * b.scale;
                if (!Fx49.Place(cam, b.at, out float clearM, out bool inWash)) continue;
                if (inWash) { size *= Fx49.WashSize(b.tier); k *= Fx49.WashK(b.tier); if (b.kill) k *= KillWash53.BurstK(game.cfg); } // NIGHT53 U polish53 (T-111; 1 off)
                // the flares: only where their edge stays outside the reticle box (a hitscan hit sits ON the aim: its read is the ring and
                // the sparks round the box, never a flare over it)
                float sz = size, kk = k;
                bool flares = Fx49.Guard(cam, b.at, ref sz, ref kk);
                if (!flares) { sz = size; kk = k; }
                Fx49Shape shape = Fx49.ShapeOf(b.owner);
                if (b.kind == 2)
                {
                    // a floor ring (the THUNDERCLAP's boost): the owner's two tones, out to the cell's size x Fx49FloorRingSpan
                    float r = size * UnityTuning.Fx49FloorRingSpan * (0.3f + 0.7f * grow);
                    ring.Disc(b.at, b.normal, r * 0.94f / UnityTuning.KenneyRingMid, 0f, core[o], k);
                    ring.Disc(b.at, b.normal, r / UnityTuning.KenneyRingMid, 0.6f, body[o], k);
                    n += 2;
                    continue;
                }
                // the core: white-hot by the cell's core share (drawn as the core tone at core share + the body at the rest: additive)
                if (flares)
                {
                    float hz = sz;
                    if (shape != Fx49Shape.Shard)
                    {
                        // the halo (heavy / critical) shrinks to keep ITS edge off the box too
                        float hs = sz * UnityTuning.Fx49HaloSpan, hk = kk * UnityTuning.Fx49HaloShare;
                        if (b.tier != Fx49Tier.Light && !inWash && Fx49.Guard(cam, b.at, ref hs, ref hk)) { flare.Billboard(b.at, hs, 0f, ct, body[o], hk); n++; } // round 3: no soft halo in the wash box (the biggest soft area; the read is the sparks)
                        flare.Billboard(b.at, sz, b.roll, ct, body[o], kk * (1 - c.core * 0.5f));
                        flare.Billboard(b.at, sz * 0.55f, b.roll, ct, core[o], kk * c.core);
                        n += 2;
                    }
                    else
                    {
                        flare.Billboard(b.at, sz * 0.45f, b.roll, ct, core[o], kk * Mathf.Max(0.4f, c.core)); // the shard's hot point
                        n++;
                    }
                    if (shape == Fx49Shape.Star) { float ss = hz * 1.4f, sk = kk; if (Fx49.Guard(cam, b.at, ref ss, ref sk)) { star.Billboard(b.at, ss, b.roll, ct, core[o], sk); n++; } }
                }
                if (b.kind == 1) continue; // a muzzle flare: no ring, no sparks
                // the ring: round / star one band, the gauntlet three tones; out to Fx49RingSpan of the size over the grow, its inner band
                // always outside the reticle box (a ring round the box, never over it: fx47's aim ring rule)
                if (shape != Fx49Shape.Shard && grow > 0.05f && !(inWash && b.kill && !KillWash53.RingInWash(game.cfg))) // EXT53 B (T-111): killWash.ringInWash 0: no kill ring in the wash box
                {
                    // round 3: the ring's end radius capped on screen (Fx49RingMaxDeg from the eye: a punch's ring 1.3 m away was a 0.7 m band round the whole view)
                    float ringMax = Vector3.Distance(eye, b.at) * Mathf.Tan(UnityTuning.Fx49RingMaxDeg * Mathf.Deg2Rad);
                    float r = Mathf.Max(Mathf.Min(0.5f * size * UnityTuning.Fx49RingSpan, ringMax) * (0.25f + 0.75f * grow), clearM * UnityTuning.Fx49RingClear);
                    Vector3 nrm = eye - b.at;
                    float rk = k * (1f - 0.5f * grow);
                    if (shape == Fx49Shape.Ring)
                    {
                        ring.Disc(b.at, nrm, r * 0.9f / UnityTuning.KenneyRingMid, 0f, core[o], rk);
                        ring.Disc(b.at, nrm, r / UnityTuning.KenneyRingMid, 0.5f, body[o], rk);
                        ring.Disc(b.at, nrm, r * 1.12f / UnityTuning.KenneyRingMid, 1f, rim[o], rk * 0.8f);
                        n += 3;
                    }
                    else
                    {
                        ring.Disc(b.at, nrm, r / UnityTuning.KenneyRingMid, 0f, BossGrade54.Rim(body[o]), rk); // NIGHT54 G grade54: the player's ring in the boss's rim (cyan inputs only; unchanged off or out of a fight)
                        n++;
                    }
                }
                // the sparks: more, longer and thinner up the tiers; the core tone through the peak, the body after; the enemy's thicker (shards)
                float len = c.sparkLen * b.scale;
                float wid = c.sparkWidth * b.scale * (shape == Fx49Shape.Shard ? 1.8f : 1f);
                bool hot = a <= Fx49.PeakEndS(b.tier);
                Vector3 view = (b.at - eye).normalized;
                for (int s = 0; s < c.sparks && s < 10; s++)
                {
                    // a spark flies out across the view (its along-the-view part dropped: a streak pointing at the eye is a dot over the hit)
                    Vector3 d = Vector3.ProjectOnPlane(dirs[i * 10 + s], view);
                    if (d.sqrMagnitude < 1e-4f) continue;
                    d.Normalize();
                    float from = Mathf.Max(len * (0.15f + 0.85f * grow), clearM * UnityTuning.Fx49RingClear), to = from + len * (1f - 0.4f * grow);
                    Vector3 p0 = b.at + d * from, p1 = b.at + d * to;
                    float pk = k, pw = wid;
                    if (!Fx49.Guard(cam, p0, ref pw, ref pk)) continue;
                    trace.Streak(p0, p1, wid, eye, hot ? core[o] : BossGrade54.Rim(body[o]), pk); // NIGHT54 G grade54: the sparks' tail in the boss's rim
                    n++;
                }
            }
            flare.Draw();
            star.Draw();
            ring.Draw();
            trace.Draw();
            Fx49.Sprites = n;
        }
    }
}
