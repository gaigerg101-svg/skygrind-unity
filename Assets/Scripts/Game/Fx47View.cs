// NIGHT47 builder F (design.n47.fx47, a view row: no sim bit; NIGHT47.md G20's look, G24 to come; GG 2026-10-18: "The left-trigger special attack
// needs to be significantly more powerful, with a heavier feel, better particle effects, and a more dramatic impact"). The PULSE gauntlet's LT
// blast had NO picture of its own (Research/2026-10-18-wave47/F-cards.md 1b: only HUD code reacted); with the row on it plays, on frame time (so it
// runs through the sim's hitstop), variant A "ring" (F-cards 1b; the blind pair's B "lance" is round 2):
//   t 0-0.10   the GATHER: pink motes pulled from a shell into the glove (never a delay: the sim's blast already went on the press);
//   t 0-0.03   a white-pink CORE flash at the glove (beside the reticle's centre box, never over it);
//   t 0-0.20   the SHOCKWAVE: a three-tone pink ring on the floor from the feet out to the blast's reach, and a ring along the aim;
//   t 0-0.50   DEBRIS streaks thrown up along the line and EMBERS drifting up;
//   the camera: a 3.5 deg pitch kick springing back over 0.45 s (x EFFECTS INTENSITY) and trauma +0.5 into the shake; both off under REDUCED MOTION.
// Colours: the gauntlet's pink family (F-cards 1c: core #ffe8f4, body #ff6fb0, rim #d8487e: OKLab h 345-1, C <= 0.20, apart from RIVAL's h 338 /
// C 0.28). Fires on "pulse27Blast" (the blast with or without pulse47's sim row). Pooled: sprites in three FxSprites, nothing allocated per frame.
// Off (the row off, -skygrindOff47): nothing hooks, nothing draws. Numbers: UnityTuning.F47.cs (Fx47*).
using System;
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    /// <summary>fx47's statics for the a747 reel.</summary>
    public static class Fx47Probe
    {
        public static int Blasts, Sprites, Auras, CoreHits;
        public static float HitKickDeg, WeaponKickDeg, FovPunch;
        /// <summary>The blast's look: 0 variant A "ring", 1 variant B "lance" (the blind pair; the a747 reel flips it). UnityTuning.Fx47BlastVariant at boot.</summary>
        public static int Variant = UnityTuning.Fx47BlastVariant;
        public static float RingRadius, KickDeg, CoreK;
        /// <summary>n47-r1d: a kill's blocks (Chunks.cs) this frame under fx47: drawn or not, culled near the eye, the nearest's distance (m).</summary>
        public static int ChunksLive, ChunksCulled;
        public static float ChunkNearest = float.PositiveInfinity;
        /// <summary>The reel's off / on passes (null: design.n47.fx47).</summary>
        public static bool? Force;
    }

    public sealed partial class Fx47View // NIGHT47 F: partial (the per-weapon layers: Fx47View.Weapons.cs)
    {
        readonly SurvivalGame game;
        bool hooked, built;
        FxSprites flare, ring, trace;
        Color core, body, rim;
        Mulberry32 rng;

        double age = -1; // s since the blast (-1: none)
        Vector3 feet0, eye0, aim0;
        readonly Vector3[] moteFrom = new Vector3[UnityTuning.Fx47Motes];
        readonly Vector3[] debrisAt = new Vector3[UnityTuning.Fx47Debris], debrisVel = new Vector3[UnityTuning.Fx47Debris];
        readonly float[] debrisSize = new float[UnityTuning.Fx47Debris];
        readonly Vector3[] emberAt = new Vector3[UnityTuning.Fx47Embers], emberVel = new Vector3[UnityTuning.Fx47Embers];

        public Fx47View(SurvivalGame game)
        {
            this.game = game;
            rng = new Mulberry32(UnityTuning.Fx47SeedSalt);
        }

        static GameConfig onFor;
        static bool onRow;

        /// <summary>design.n47.fx47 (read once per config: no string built per frame) or the reel's Force. The TORCH and the burn read it too.</summary>
        public static bool RowOn(GameConfig cfg)
        {
            if (Fx47Probe.Force.HasValue) return Fx47Probe.Force.Value;
            if (!ReferenceEquals(cfg, onFor)) { onFor = cfg; onRow = cfg != null && NightRows.ViewOn(cfg, "fx47"); }
            return onRow;
        }

        bool On => RowOn(game.cfg);

        bool Calm() => game.cfg.movement.settings.reducedMotion;

        /// <summary>Hooks the bus once (the first frame the row is on).</summary>
        void Hook()
        {
            if (hooked || game.events == null) return;
            hooked = true;
            game.events.On<Pulse27Event>("pulse27Blast", e => { if (On) OnBlast(e); });
            game.events.On<HitEvent>("hit", e => { if (On && e.owner == "player" && (e.weapon == "punch" || e.weapon == "gauntlet")) OnMeleeHit(e); });
            game.events.On<MeleeEvent>("punch", e => { if (On && e.owner == "player") OnSwing(false); });
            game.events.On<MeleeEvent>("meleeSwing", e => { if (On && e.owner == "player") OnSwing(true); });
            game.events.On("wick47CorePunch", o => { if (On) { hitAge = 0; if (!Calm()) game.trauma.Add(UnityTuning.Fx47CoreTrauma); Fx47Probe.CoreHits++; } }); // G7 (W's hand-off): the hard camera hit
            HookWeapons47();
            HookGuns48(); // NIGHT48 F guns48 (Fx48View.Guns.cs): the fist's nod, the plain shots' FOV punch
            game.OnNewRun += () => { for (int i = 0; i < puffs.Length; i++) puffs[i].age = -1; for (int i = 0; i < kicks.Length; i++) kicks[i].age = -1; };
            game.OnNewRun += () => { age = -1; swingAge = -1; for (int i = 0; i < auras.Length; i++) auras[i].age = -1; rng = new Mulberry32(game.runSeed ^ UnityTuning.Fx47SeedSalt); };
            if (Polish53.Item(game.cfg, "firstShot")) Prewarm53(); // EXT53 B (T-122): the sheets now (this game's first frame with the row on), not on the THUNDERCLAP's first shot
        }

        // ---- the punch aura (G21, the player's side; F-cards 1c): three tones of one hue + a halo, at the hit, and a ring in the same three tones;
        // fist = the cyan-blue family, gauntlet = the pink family. The swing's start lights the glove in its tones (the hit's "start"). ----
        struct Aura
        {
            public float age; // s (-1: free)
            public Vector3 at;
            public bool pink;
        }

        readonly Aura[] auras = new Aura[UnityTuning.Fx47Auras];
        int nextAura;
        float swingAge = -1;
        bool swingPink;
        Color fCore, fBody, fRim, fHalo, gHalo;

        void OnMeleeHit(HitEvent e)
        {
            Camera cam = game.cam;
            if (cam == null) return;
            if (!built) Build();
            if (Blast48NoAura(e)) return; // NIGHT48 F blast48 (Fx48View.Blast.cs): the blast's own hits carry no punch aura (n48-r1 a748's f4 wash)
            if (Fx49.On(game.cfg)) return; // NIGHT49 X fx49: the punch's hit is the kit's burst (Fx49Kit.OnHit: the gauntlet's ring, the fist's round flare)
            Vector3 at = WebSpace.ToUnity(e.position);
            Vector3 toCam = cam.transform.position - at;
            float d = toCam.magnitude;
            if (d > 1e-3f) at += toCam / d * Mathf.Min(UnityTuning.Fx47AuraFront, d * 0.35f); // on the camera's side of the skin (Fusion43View's lesson)
            auras[nextAura] = new Aura { age = 0, at = at, pink = e.weapon == "gauntlet" };
            nextAura = (nextAura + 1) % auras.Length;
            Fx47Probe.Auras++;
        }

        void OnSwing(bool pink)
        {
            if (!built) Build();
            if (!pink && Fx49.On(game.cfg)) return; // NIGHT49 X fx49: the fist's blue glove glow was a fourth blue (not the player's cyan): the kit's hit is the fist's read
            swingAge = 0;
            swingPink = pink;
            if (pink) AddKick(UnityTuning.Fx47SwingKickDeg, 0, UnityTuning.Fx47SwingKickRiseS, UnityTuning.Fx47SwingKickSettleS); // G24: the PULSE swing to the medium tier
        }

        void Build()
        {
            built = true;
            MakeSheets53();
            BuildWeapons47();
        }

        /// <summary>
        /// EXT53 B (T-122, polish53's firstShot): the three sheets made at the run's first frame (Hook), not on the THUNDERCLAP's first shot. Under
        /// fx49 nothing else builds this view before it: OnGunHit and OnKill47 return before Build, OnFire47 builds only for "shotgun" (the
        /// KESTREL's shot never reaches Build), so the run's first THUNDERCLAP shot (or first punch, OnSwing) made 3 FxSprites x about 10 colours x
        /// UnityTuning.FxSpriteLevels (8) additive materials, dynamic meshes and QuadBatches (flare and trace at Fx47Pool 160 quads: ~2.7 MB of
        /// arrays) inside that frame. Only the sheets: `built` (the draw pass, the kicks' and puffs' reset in BuildWeapons47) still flips where
        /// it did, so what the view draws and when is unchanged. Off (the item's on 0, polish53 off, -skygrindOff53): never called.
        /// </summary>
        void Prewarm53()
        {
            if (flare != null || game.mats == null) return;
            var w = System.Diagnostics.Stopwatch.StartNew();
            MakeSheets53();
            FirstShot53.Fx47Sheets = flare != null;
            FirstShot53.Fx47Ms = w.Elapsed.TotalMilliseconds;
            Debug.Log($"[polish53] first shot (T-122): fx47's sprite sheets built at the run's first frame in {FirstShot53.Fx47Ms:0.0} ms (were the THUNDERCLAP's first shot's)");
        }

        /// <summary>The sheets and their colours' materials (Build's heavy part, once: Prewarm53 or the first Build).</summary>
        void MakeSheets53()
        {
            if (flare != null) return;
            Material neon = game.mats.neon;
            flare = new FxSprites(neon, UnityTuning.KenneyFlare, UnityTuning.Fx47Pool);
            ring = new FxSprites(neon, UnityTuning.KenneyRing, 16);
            trace = new FxSprites(neon, UnityTuning.KenneyTrace, UnityTuning.Fx47Pool);
            core = Look.Hex(UnityTuning.Fx47CoreHex) * UnityTuning.Fx47Glow;
            body = Look.Hex(UnityTuning.Fx47BodyHex) * UnityTuning.Fx47Glow;
            rim = Look.Hex(UnityTuning.Fx47RimHex) * UnityTuning.Fx47Glow;
            fCore = Look.Hex(UnityTuning.Fx47FistCoreHex) * UnityTuning.Fx47Glow;
            fBody = Look.Hex(UnityTuning.Fx47FistBodyHex) * UnityTuning.Fx47Glow;
            fRim = Look.Hex(UnityTuning.Fx47FistRimHex) * UnityTuning.Fx47Glow;
            fHalo = Look.Hex(UnityTuning.Fx47FistHaloHex) * (UnityTuning.Fx47Glow * 0.5f);
            gHalo = Look.Hex(UnityTuning.Fx47HaloHex) * (UnityTuning.Fx47Glow * 0.5f);
            foreach (FxSprites s in new[] { flare, ring, trace })
            {
                foreach (Color c in new[] { core, body, rim, fCore, fBody, fRim, fHalo, gHalo }) s.Warm(c);
            }
            WarmWeapons47(); // the weapons' colours after these 8, as Build always made them (the same buckets in the same order)
        }

        void OnBlast(Pulse27Event e)
        {
            Camera cam = game.cam;
            if (cam == null) return;
            if (!built) Build();
            Transform ct = cam.transform;
            eye0 = ct.position;
            aim0 = ct.forward;
            Vec3 f = game.Feet;
            feet0 = WebSpace.ToUnity(f) + Vector3.up * UnityTuning.Fx47FloorLift;
            age = 0;
            Vector3 glove = Glove(ct);
            for (int i = 0; i < moteFrom.Length; i++)
                moteFrom[i] = glove + SprayMath.UnitSphere(rng.Next(), rng.Next()) * UnityTuning.Fx47GatherShell;
            Vector3 flat = new Vector3(aim0.x, 0, aim0.z);
            flat = flat.sqrMagnitude > 1e-6f ? flat.normalized : ct.forward;
            for (int i = 0; i < debrisAt.Length; i++)
            {
                // n47-r1b: small chunks off the floor ahead (from Fx47ChunkFrom m), thrown up and away along the line, never toward the eye
                float along = UnityTuning.Fx47ChunkFrom + (UnityTuning.Fx47Reach - UnityTuning.Fx47ChunkFrom) * (float)rng.Next();
                debrisAt[i] = feet0 + flat * along + Vector3.Cross(Vector3.up, flat) * ((float)rng.Next() - 0.5f) * 2f;
                debrisVel[i] = Vector3.up * (3f + 3f * (float)rng.Next()) + flat * (2f + 2f * (float)rng.Next());
                debrisSize[i] = Mathf.Lerp(UnityTuning.Fx47ChunkMin, UnityTuning.Fx47ChunkMax, (float)rng.Next());
            }
            for (int i = 0; i < emberAt.Length; i++)
            {
                emberAt[i] = feet0 + flat * (UnityTuning.Fx47ChunkFrom + (float)rng.Next() * (UnityTuning.Fx47Reach - UnityTuning.Fx47ChunkFrom)) +SprayMath.UnitSphere(rng.Next(), rng.Next()) * 1.2f + Vector3.up;
                emberVel[i] = Vector3.up * (0.8f + 1.6f * (float)rng.Next()) + SprayMath.UnitSphere(rng.Next(), rng.Next()) * 0.6f;
            }
            if (!Calm()) game.trauma.Add(UnityTuning.Fx47Trauma);
            Blast48DropAuras(); // NIGHT48 F blast48: the auras its hits spawned this step (the hits come before the blast's event) go
            Fx47Probe.Blasts++;
            if (Fx49.On(game.cfg)) Fx49.Note("pulse-blast", Fx49Tier.Critical, Fx49Owner.Gauntlet); // NIGHT49 X fx49: the kit's probe (the a249 line)
        }

        float hitAge = -1;

        /// <summary>THE WICK's core punch (wick47CorePunch, W's G7): a hard pitch-and-roll kick (the heavy tier's top: Fx47CoreKickDeg), springing back
        /// over Fx47CoreSettleS, x EFFECTS INTENSITY, off under REDUCED MOTION; the slow motion is ImpactRules' CorePunch (ImpactFrames).</summary>
        void HitKick(Camera cam, float dt)
        {
            Fx47Probe.HitKickDeg = 0;
            if (hitAge < 0 || cam == null) return;
            float t = hitAge;
            hitAge += dt;
            if (t > UnityTuning.Fx47KickRiseS + UnityTuning.Fx47CoreSettleS) { hitAge = -1; return; }
            if (Calm()) return;
            float up = Mathf.Clamp01(t / UnityTuning.Fx47KickRiseS), down = Mathf.Clamp01((t - UnityTuning.Fx47KickRiseS) / UnityTuning.Fx47CoreSettleS);
            float ei = (float)System.Math.Max(0, System.Math.Min(1, game.cfg.movement.settings.effectsIntensity));
            float k = up * (1f - down) * (1f - down) * ei;
            cam.transform.rotation = cam.transform.rotation * Quaternion.Euler(-UnityTuning.Fx47CoreKickDeg * k, 0, UnityTuning.Fx47CoreRollDeg * k);
            Fx47Probe.HitKickDeg = UnityTuning.Fx47CoreKickDeg * k;
        }

        /// <summary>The punch auras and the swing's glove glow this frame (sprites added).</summary>
        int Auras(Transform ct, Vector3 glove, float dt)
        {
            int n = 0;
            if (swingAge >= 0)
            {
                // the start: the glove lit in its family's three tones, growing over the swing's way out, then gone
                float k = swingAge / UnityTuning.Fx47SwingS;
                if (k >= 1f) swingAge = -1;
                else
                {
                    float grow = Mathf.Sin(k * Mathf.PI);
                    Color c0 = swingPink ? core : fCore, c1 = swingPink ? body : fBody, c2 = swingPink ? rim : fRim;
                    flare.Billboard(glove, 0.10f + 0.12f * grow, 0f, ct, c0, grow);
                    flare.Billboard(glove, 0.18f + 0.2f * grow, 0.6f, ct, c1, grow * 0.7f);
                    flare.Billboard(glove, 0.28f + 0.3f * grow, 1.2f, ct, c2, grow * 0.45f);
                    n += 3;
                    swingAge += dt;
                }
            }
            for (int i = 0; i < auras.Length; i++)
            {
                if (auras[i].age < 0) continue;
                float a = auras[i].age;
                auras[i].age += dt;
                if (a > UnityTuning.Fx47AuraLife) { auras[i].age = -1; continue; }
                bool p = auras[i].pink;
                Color c0 = p ? core : fCore, c1 = p ? body : fBody, c2 = p ? rim : fRim, hl = p ? gHalo : fHalo;
                float fade = 1f - a / UnityTuning.Fx47AuraLife;
                float s = (p ? 1.25f : 1f) * UnityTuning.Fx47AuraSize;
                Vector3 at = auras[i].at;
                flare.Billboard(at, s * 2.5f, 0f, ct, hl, fade * 0.5f); // the soft halo
                flare.Billboard(at, s * 1.8f, 1.1f, ct, c2, fade * 0.7f); // the darker rim
                flare.Billboard(at, s * 1.4f, 0.5f, ct, c1, fade); // the body
                flare.Billboard(at, s, 0f, ct, c0, fade); // the light core
                // the hit's ring: 0 -> Fx47AuraRing m in Fx47AuraRingS, three bands
                float rk = Mathf.Clamp01(a / UnityTuning.Fx47AuraRingS), r = UnityTuning.Fx47AuraRing * (1f - (1f - rk) * (1f - rk));
                if (r > 0.02f)
                {
                    float mid = UnityTuning.KenneyRingMid;
                    Vector3 nrm = ct.position - at;
                    ring.Disc(at, nrm, r * 0.9f / mid, 0f, c0, fade);
                    ring.Disc(at, nrm, r / mid, 0.5f, c1, fade);
                    ring.Disc(at, nrm, r * 1.12f / mid, 1f, c2, fade * 0.8f);
                    n += 3;
                }
                n += 4;
            }
            return n;
        }

        /// <summary>The glove's point in the view: low and right of the reticle (never in its centre box).</summary>
        static Vector3 Glove(Transform ct) => ct.position + ct.forward * UnityTuning.Fx47GloveFwd + ct.right * UnityTuning.Fx47GloveRight - ct.up * UnityTuning.Fx47GloveDown;

        /// <summary>Once a frame after the camera (Feel43.Render calls it whatever feel43's own row).</summary>
        public void Render(double frameDt)
        {
            Fx49Kit.Frame(game, frameDt); // NIGHT49 X fx49 (Fx49.cs): the impact kit's bursts (its own row, whatever fx47's)
            if (!On) return;
            Hook();
            Camera cam = game.cam;
            HitKick(cam, (float)frameDt);
            Fx47Probe.WeaponKickDeg = built ? Kicks47(cam, (float)frameDt) : 0; // G24: the boost's and the swap's kicks // G7: the core punch's hard camera hit (independent of the blast's sprites)
            Guns48.FovNow = Fov48Render(cam, (float)frameDt); // NIGHT48 F guns48: the plain shots' FOV punch (0 with the row off: nothing queued)
            if (cam == null || !built) { Fx47Probe.Sprites = 0; Fx47Probe.RingRadius = Fx47Probe.KickDeg = Fx47Probe.CoreK = 0; return; }
            Transform ct = cam.transform;
            float t = (float)age;
            if (age >= 0) { age += frameDt; if (t > UnityTuning.Fx47Life) { age = -1; t = -1; } }
            bool blast = t >= 0;
            bool anyAura = swingAge >= 0 || livePuffs > 0 || PuffQueued47();
            for (int i = 0; i < auras.Length && !anyAura; i++) if (auras[i].age >= 0) anyAura = true;
            if (!blast && !anyAura) { Fx47Probe.Sprites = 0; Fx47Probe.RingRadius = Fx47Probe.KickDeg = Fx47Probe.CoreK = 0; return; }
            flare.Begin();
            ring.Begin();
            trace.Begin();
            int n = 0;
            Vector3 eye = ct.position, glove = Glove(ct);
            n += Auras(ct, glove, (float)frameDt);
            n += Puffs47(ct, (float)frameDt); // G24: impact sparks, kill flares, the THUNDERCLAP's cone and boost ring
            if (!blast)
            {
                flare.Draw();
                ring.Draw();
                trace.Draw();
                Fx47Probe.Sprites = n;
                Fx47Probe.RingRadius = Fx47Probe.KickDeg = Fx47Probe.CoreK = 0;
                return;
            }
            // the gather: motes into the glove
            if (t < UnityTuning.Fx47GatherS)
            {
                float k = t / UnityTuning.Fx47GatherS, ek = k * k;
                for (int i = 0; i < moteFrom.Length; i++)
                {
                    Vector3 from = Vector3.Lerp(moteFrom[i], glove, ek), to = Vector3.Lerp(moteFrom[i], glove, Mathf.Min(1f, ek + 0.25f));
                    trace.Streak(from, to, 0.025f, eye, i % 3 == 0 ? core : body, 1f - 0.5f * k);
                    n++;
                }
            }
            // the core flash
            float coreK = t < UnityTuning.Fx47CoreS ? 1f - t / UnityTuning.Fx47CoreS : 0f;
            if (Fx49.On(game.cfg)) coreK = Fx49.Env(Fx49Tier.Critical, t); // NIGHT49 X fx49: the blast's core on the kit's critical timing (2 steps in, 6 held, 32 out; was 33 ms linear)
            if (coreK > 0f)
            {
                flare.Billboard(glove, UnityTuning.Fx47CoreSize, 0f, ct, core, coreK);
                flare.Billboard(glove, UnityTuning.Fx47CoreSize * UnityTuning.Fx47CoreBodyX, 0.7f, ct, body, coreK * 0.6f);
                n += 2;
            }
            // the shockwave: three tones on the floor, one ring along the aim
            float wk = Mathf.Clamp01(t / UnityTuning.Fx47WaveS);
            float r = UnityTuning.Fx47Reach * (1f - (1f - wk) * (1f - wk));
            float fade = t < UnityTuning.Fx47WaveS ? 1f : Mathf.Clamp01(1f - (t - UnityTuning.Fx47WaveS) / UnityTuning.Fx47WaveFadeS);
            bool b48 = Blast48.On(game.cfg); // NIGHT48 F blast48 (Fx48View.Blast.cs): the picked variant, the ring and the streaks round the view as it kicks
            bool lance = (b48 ? Blast48.Variant(game.cfg) : Fx47Probe.Variant) == 1; // the blind pair's B "lance": a cone of pink streaks along the aim leads, the floor ring small
            Blast48Axis(ct, b48, b48 ? BlastKick48(t) : 0f, out Vector3 axO, out Vector3 axD, out float axClear);
            if (fade > 0f && r > 0.05f)
            {
                float mid = UnityTuning.KenneyRingMid;
                float rr = lance ? r * UnityTuning.Fx47LanceRingShare : r;
                ring.Disc(feet0, Vector3.up, rr * 0.94f / mid, 0f, core, fade);
                ring.Disc(feet0, Vector3.up, rr / mid, 0.6f, body, fade);
                ring.Disc(feet0, Vector3.up, rr * 1.07f / mid, 1.2f, rim, fade * 0.8f);
                float ar = Mathf.Lerp(0.3f, UnityTuning.Fx47AimRing, wk);
                // round 3 (n47-r1d: "the vertical shockwave ring along the aim ... near the camera?"): the aim ring starts Fx47NearEye ahead (was
                // 0.8 m), and its radius keeps at least Fx47AimRingClear of its distance (a ring round the reticle's box, never over it)
                float ad = UnityTuning.Fx47NearEye + UnityTuning.Fx47Reach * 0.5f * wk;
                ar = Mathf.Max(ar, ad * axClear);
                ring.Disc(axO + axD * ad, axD, ar / mid, 0f, body, fade);
                n += 4;
                if (lance)
                {
                    Vector3 side = Vector3.Cross(axD, Vector3.up);
                    side = side.sqrMagnitude > 1e-6f ? side.normalized : ct.right;
                    Vector3 upv = Vector3.Cross(side, axD);
                    for (int i = 0; i < UnityTuning.Fx47LanceStreaks; i++)
                    {
                        float a = i * 2.399963f; // the golden angle round the aim
                        float rad = UnityTuning.Fx47AimRing * Mathf.Sqrt((i + 0.5f) / UnityTuning.Fx47LanceStreaks);
                        Vector3 offDir = side * Mathf.Cos(a) + upv * Mathf.Sin(a);
                        // round 3 (the rules: nothing within Fx47NearEye, never over the reticle's centre box): the streaks start beyond
                        // Fx47NearEye and stand off the aim by at least Fx47AimRingClear of their distance (was from 0.6 m, on the axis for i 0)
                        float d0 = UnityTuning.Fx47NearEye + UnityTuning.Fx47Reach * wk * 0.6f, d1 = Mathf.Max(d0 + 0.3f, UnityTuning.Fx47NearEye + UnityTuning.Fx47Reach * wk);
                        Vector3 s0 = axO + axD * d0 + offDir * Mathf.Max(rad * wk * 0.6f, d0 * axClear);
                        Vector3 s1 = axO + axD * d1 + offDir * Mathf.Max(rad * wk, d1 * axClear);
                        trace.Streak(s0, s1, 0.06f, eye, i % 3 == 0 ? core : i % 3 == 1 ? body : rim, fade);
                    }
                    n += UnityTuning.Fx47LanceStreaks;
                }
            }
            // debris and embers
            float dt = (float)frameDt;
            for (int i = 0; i < debrisAt.Length; i++)
            {
                if (t < UnityTuning.Fx47DebrisFrom) break;
                debrisVel[i] += Vector3.down * 18f * dt;
                debrisAt[i] += debrisVel[i] * dt;
                float dk = Mathf.Clamp01(1f - t / UnityTuning.Fx47DebrisLife);
                if (dk <= 0f) break;
                if (Chunk47(debrisAt[i], debrisVel[i], debrisSize[i], eye, i % 3 == 0 ? rim : wDust, dk)) n++;
            }
            for (int i = 0; i < emberAt.Length; i++)
            {
                emberAt[i] += emberVel[i] * dt;
                float ek = Mathf.Clamp01(1f - t / UnityTuning.Fx47EmberLife);
                if (ek <= 0f) break;
                if ((emberAt[i] - eye).sqrMagnitude < UnityTuning.Fx47NearEye * UnityTuning.Fx47NearEye) continue; // n47-r1b: nothing in the face
                flare.Billboard(emberAt[i], 0.08f, 0f, ct, i % 2 == 0 ? body : core, ek);
                n++;
            }
            flare.Draw();
            ring.Draw();
            trace.Draw();
            // the camera kick (after CameraFeel: as Fusion43View's)
            float kick = 0f;
            if (!Calm())
            {
                float up = Mathf.Clamp01(t / UnityTuning.Fx47KickRiseS);
                float down = Mathf.Clamp01((t - UnityTuning.Fx47KickRiseS) / UnityTuning.Fx47KickSettleS);
                float ei = (float)Math.Max(0, Math.Min(1, game.cfg.movement.settings.effectsIntensity));
                kick = UnityTuning.Fx47KickDeg * up * (1f - down) * (1f - down) * ei;
                if (kick > 0f) ct.rotation = ct.rotation * Quaternion.Euler(-kick, 0, 0);
                // the FOV punch: out Fx47FovPunchDeg in 60 ms, back over 0.3 s (after CameraFeel / the viewmodel set this frame's FOV: it does not accumulate)
                float fo = t < UnityTuning.Fx47FovOutS ? t / UnityTuning.Fx47FovOutS : Mathf.Clamp01(1f - (t - UnityTuning.Fx47FovOutS) / UnityTuning.Fx47FovBackS);
                float fov = UnityTuning.Fx47FovPunchDeg * fo * ei;
                if (fov > 0f) cam.fieldOfView += fov;
                Fx47Probe.FovPunch = fov;
            }
            Fx47Probe.Sprites = n;
            Fx47Probe.RingRadius = r;
            Fx47Probe.KickDeg = kick;
            Fx47Probe.CoreK = coreK;
        }
    }
}
