// PG3 V1 (WEAPON FEEL, row 2; view row design.n74.charge74, read EVERY frame with NightRows.ViewOn; off: -skygrindRows72 charge74:0 and the game draws exactly as before).
// GG 2026-10-28: guns "very satisfying", visuals "much higher quality". The CHARGED shots, in layers added on top of the hands' own tip, barrel steps and sleeve
// (Viewmodel.Charge35, gunglow50) and of Pistol39View's cannon: nothing of theirs is replaced or touched; the row off draws none of this.
//   - the CHARGE GLOW at the muzzle of the KESTREL, the THUNDERCLAP and the PULSE gauntlet: a halo and a hot core that swell with the charge (level 0..1, the
//     same read as gunglow50: the hold for the guns, the pips for the glove), a ring that closes in on the muzzle, and motes that fall into it faster as it fills;
//   - the READY flash at full (the edge, once per fill): a sharp ring blown out and a star, through the game's flash gate (WCAG: three a second; refused, the
//     ring comes dim and the white pop is dropped), and a slow 2 Hz swell while it is held;
//   - the RELEASE: the KESTREL's charged pierce is a thick beam (a wide soft halo, a cyan sleeve, a white-hot core, shock rings marching down it so the line
//     reads as a tunnel from behind the gun) with a burst where it ends; the THUNDERCLAP's slug is a fat comet with a blast and two rings at the muzzle;
//   - the PULSE blast: a ring across the floor that runs out from the player, a ring standing across the aim, a star at the glove;
//   - the PARRY: a bright cyan-white flash (a star, a soft core, a ring) at the parried body or glob, through the same gate, and a 1-2 rendered-frame screen
//     kick (a short FOV punch-in and a pitch dip; x the FOV KICK / CAMERA SHAKE settings, none with REDUCED MOTION).
// Colours: cyan (colors.usable) and white-hot only (CLAUDE.md rule 8: the player's things); never the enemy's red-orange. Sprites age on the world's time (the
// parry's and the cannon's freeze holds the picture), the glow and the kick on frame time. Pooled (Fx74.cs); nothing is allocated per frame. Self-installing
// like Shot74View. The proof: Charge74Probe.Line(), the `[charge74]` line in the Player log at quit. Numbers: UnityTuning.PG3V1.cs.
using Skygrind.Core;
using UnityEngine;

namespace Skygrind.Game
{
    /// <summary>PG3 V1: what charge74 drew (the Player log's `[charge74]` line reads it; zero with the row off).</summary>
    public static class Charge74Probe
    {
        public static bool On;
        public static int GlowFrames, Readies, ReadiesDimmed, Beams, Slugs, ReleasesDimmed, Pulses, Parries, ParriesDimmed, KickFrames, Lights;
        public static float PeakLevel;
        public static int PeakQuads;

        public static void Reset() { GlowFrames = Readies = ReadiesDimmed = Beams = Slugs = ReleasesDimmed = Pulses = Parries = ParriesDimmed = KickFrames = Lights = 0; PeakLevel = 0f; PeakQuads = 0; }

        public static string Line() =>
            $"[charge74] on {On}: glow frames {GlowFrames} (peak level {PeakLevel:0.00}), READY flashes {Readies} (dimmed by the gate {ReadiesDimmed}), pierce beams {Beams}, slugs {Slugs} (blasts dimmed {ReleasesDimmed}), pulse blasts {Pulses}, parries {Parries} (dimmed {ParriesDimmed}, kick frames {KickFrames}), lights {Lights}, peak quads in a frame {PeakQuads}";
    }

    public sealed class Charge74View : MonoBehaviour
    {
        public const string Row = "charge74";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            if (FindAnyObjectByType<Charge74View>() != null) return;
            var go = new GameObject("Charge74");
            DontDestroyOnLoad(go);
            go.AddComponent<Charge74View>();
        }

        SurvivalGame game;
        Fx74Sprites sprites; // pictures on real frame time (the READY flash, the pulse, the parry's star): they snap and go
        Fx74Sprites world; // pictures on the world's time (the release's blast, rings and burst, the parry's rings): a hit freeze holds them
        float starForward;
        Color cyan, hot;

        // the glow
        float level, prevLevel, clock;
        string glowGun;

        // what the bus told us (drained at the frame's render, after the viewmodel's kick moved the muzzle)
        Vector3 lastEnd;
        bool lastEndSet;
        string releaseGun; // "revolver" / "shotgun": a charged shot went this frame
        int releaseHits;
        bool pulsePending, parryPending;
        Vector3 parryAt;
        int releaseFrame = -1, releaseFrameGun = -1, parryFrame = -1;

        // the beams (a charged KESTREL's pierce / a slug): a few live at once
        struct Beam { public bool alive; public Vector3 from, to; public float age, life; public bool slug; }
        readonly Beam[] beams = new Beam[3];
        int beamNext;

        // the parry's screen kick: the rendered frame it is on (-1: none), what was written (undone when nothing re-posed the camera)
        int kickFrame = -1;
        Quaternion kickBaseRot, kickWrittenRot;
        float kickBaseFov, kickWrittenFov;
        bool kickWritten;

        void Update()
        {
            SurvivalGame g = SurvivalGame.Instance;
            if (g == game || g == null) return;
            try { Hook(g); }
            catch (System.Exception ex)
            {
                game = g; // no retry every frame
                faulted = true;
                Debug.LogError("[charge74] could not hook the game, stopped for this session: " + ex);
            }
        }

        void Hook(SurvivalGame g)
        {
            game = g;
            sprites ??= new Fx74Sprites(UnityTuning.PG3V1ChargeSpritePool);
            world ??= new Fx74Sprites(UnityTuning.PG3V1ChargeSpritePool);
            sprites.Clear();
            world.Clear();
            for (int i = 0; i < beams.Length; i++) beams[i].alive = false;
            level = prevLevel = 0f;
            releaseGun = null; pulsePending = parryPending = lastEndSet = false;
            kickFrame = -1; kickWritten = false;
            GameConfig cfg = g.cfg;
            starForward = (float)cfg.Num("fx.flashForward");
            cyan = Look.C(cfg, "colors.usable");
            hot = Color.Lerp(cyan, Color.white, UnityTuning.PG3V1HotWhite);
            EventBus ev = g.events;
            ev.On<ShotEvent>("shotResult", e =>
            {
                if (e == null || e.owner != "player" || (e.weapon != "revolver" && e.weapon != "shotgun")) return;
                lastEnd = WebSpace.ToUnity(e.end);
                lastEndSet = true;
            });
            ev.On<RailFireEvent>(Loadout.KestrelPierce33, e =>
            {
                if (e == null || e.owner != "player") return;
                lastEnd = WebSpace.ToUnity(e.to);
                lastEndSet = true;
            });
            ev.On<ChargeShot27Event>("charge35Fire", e => OnRelease(e));
            ev.On<ChargeShot27Event>("chargeShot27Fire", e => OnRelease(e));
            ev.On<Pulse27Event>("pulse27Blast", e => pulsePending = true);
            ev.On<HitEvent>("parry", OnParry);
            ev.On<HitEvent>(Parry36.Event, OnParry);
            g.OnRender += Render;
            g.OnNewRun += () =>
            {
                sprites.Clear();
                world.Clear();
                for (int i = 0; i < beams.Length; i++) beams[i].alive = false;
                level = prevLevel = 0f;
                releaseGun = null; pulsePending = parryPending = lastEndSet = false;
                kickFrame = -1;
                if (Charge74Probe.GlowFrames + Charge74Probe.Parries + Charge74Probe.Beams + Charge74Probe.Slugs > 0) Debug.Log(Charge74Probe.Line());
            };
        }

        // the proof: what was drawn, printed to the Player log at quit
        void OnApplicationQuit() => Debug.Log(Charge74Probe.Line());

        void OnRelease(ChargeShot27Event e)
        {
            if (e == null || (e.gun != "revolver" && e.gun != "shotgun")) return;
            int gi = e.gun == "revolver" ? 0 : 1;
            int f = FeelClock.FrameCount;
            if (releaseFrame == f && releaseFrameGun == gi) return; // "charge35Fire" and "chargeShot27Fire" for one release: one picture
            releaseFrame = f;
            releaseFrameGun = gi;
            releaseGun = e.gun;
            releaseHits = e.hits;
        }

        void OnParry(HitEvent e)
        {
            if (e == null || e.owner != "player") return;
            int f = FeelClock.FrameCount;
            if (parryFrame == f) return; // "parry" and "parry36" on one press: one flash
            parryFrame = f;
            parryPending = true;
            parryAt = WebSpace.ToUnity(e.position);
        }

        /// <summary>A sprite's size kept under `deg` of the view from the eye (Fx74Math.CapDeg at the sprite's own distance).</summary>
        static float Cap(Camera cam, Vector3 at, float size, float deg) => Fx74Math.CapDeg(size, (at - cam.transform.position).magnitude, deg);

        /// <summary>The charge level 0..1 for the gun in hand: the hold for the KESTREL / THUNDERCLAP, the pips for the glove (gunglow50's read).</summary>
        static float LevelOf(SurvivalGame g, string held)
        {
            SurvivalSim sim = g.sim;
            Loadout lo = g.loadout;
            if (sim == null || lo == null || held == null) return 0f;
            if (held == "gauntlet")
            {
                PulseCharge27 pc = sim.pulse27;
                if (!sim.n27.pulse27 || pc == null || !pc.active || pc.p == null || pc.p.pips <= 0 || pc.cooldownSteps > 0) return 0f;
                return Fx74Math.Clamp01(pc.pips / (float)pc.p.pips);
            }
            int gi = ChargeShots27.Index(held);
            ChargeShots27 cs = (sim.n27.chargeShots27 || sim.n31.charge) && lo.charge27 != null && lo.charge27.active ? lo.charge27 : null;
            if (gi < 0 || cs == null) return 0f;
            Charge35 q = cs.c35;
            if (q != null && q.holding && q.gun == gi) return Fx74Math.Clamp01((float)lo.Charge35Level);
            if (cs.charging) return Fx74Math.Clamp01((float)cs.Level(gi));
            return 0f;
        }

        bool faulted;

        /// <summary>The frame hook: any exception in this additive, view-only layer logs once and stops the layer for the session (it must never break the game's frame).</summary>
        void Render(double frameDt, double alpha)
        {
            if (faulted) return;
            try { RenderCore(frameDt); }
            catch (System.Exception ex)
            {
                faulted = true;
                UndoKick(game != null ? game.cam : null); // a kick written before the fault does not stay on the camera
                Debug.LogError("[charge74] stopped for this session after an exception: " + ex);
            }
        }

        void RenderCore(double frameDt)
        {
            SurvivalGame g = game;
            if (g == null || g.cam == null || g.sim == null || g.cfg == null || g.mats == null) return;
            bool on = NightRows.ViewOn(g.cfg, Row); // read every frame (D1's contract)
            Charge74Probe.On = on;
            Camera cam = g.cam;
            UndoKick(cam); // the kick written last frame goes if nothing re-posed the camera since (a menu or recap frame)
            if (!on || g.flow != Flow.Playing || g.attractOn)
            {
                releaseGun = null; pulsePending = parryPending = false; lastEndSet = false;
                if (!on) { sprites.Clear(); world.Clear(); for (int i = 0; i < beams.Length; i++) beams[i].alive = false; kickFrame = -1; level = prevLevel = 0f; }
                return;
            }
            Fx74Draw.Ensure(g.mats.neon);
            Fx74Draw.Begin();
            float dt = (float)frameDt, wdt = (float)g.WorldDt;
            bool still = g.cfg.movement.settings.reducedMotion || g.cfg.movement.settings.effectsIntensity <= 0;
            float fi = Mathf.Clamp01((float)g.settings.View.flashIntensity);
            if (dt > 0f) clock += dt;

            Vector3 muzzle = default;
            bool hasMuzzle = g.viewmodel != null && g.viewmodel.MuzzleWorld(out muzzle);
            if (!hasMuzzle) muzzle = cam.transform.position + cam.transform.forward * UnityTuning.FxMuzzleDepth;
            muzzle += cam.transform.forward * starForward;
            LightPools lights = g.lightAndLens?.lights;

            // ---- the glow, and the READY edge ----
            string held = g.loadout != null ? g.loadout.Weapon : null;
            float raw = held == "revolver" || held == "shotgun" || held == "gauntlet" ? LevelOf(g, held) : 0f;
            if (held != glowGun) { glowGun = held; level = prevLevel = raw; } // a swap: no edge
            else
            {
                prevLevel = level;
                level = held == "gauntlet" ? Mathf.MoveTowards(level, raw, dt * UnityTuning.PG3V1GloveRise) : raw;
            }
            if (level > Charge74Probe.PeakLevel) Charge74Probe.PeakLevel = level;
            if (hasMuzzle && level > 0.001f && fi > 0f) { DrawGlow(cam, muzzle, level, held, still, fi); Charge74Probe.GlowFrames++; }
            if (hasMuzzle && Fx74Math.ReadyEdge(prevLevel, level >= UnityTuning.PG3V1FullAt ? 1f : level)) Ready(g, cam, muzzle, lights, fi);

            // ---- the release ----
            if (releaseGun != null)
            {
                if (hasMuzzle) Release(g, cam, muzzle, lights, releaseGun == "shotgun", fi);
                releaseGun = null;
            }
            lastEndSet = false;
            if (pulsePending)
            {
                pulsePending = false;
                Pulse(g, cam, muzzle, lights, fi);
            }
            if (parryPending)
            {
                parryPending = false;
                Parry(g, cam, lights, fi);
            }

            // ---- the beams, the sprites, the kick ----
            DrawBeams(cam, wdt);
            sprites.StepAndDraw(dt, cam);
            world.StepAndDraw(wdt, dt, cam);
            Kick(g, cam, dt);
            Fx74Draw.End();
            if (Fx74Draw.PeakQuads > Charge74Probe.PeakQuads) Charge74Probe.PeakQuads = Fx74Draw.PeakQuads;
        }

        // ================================================= the glow =================================================

        void DrawGlow(Camera cam, Vector3 at, float lv, string held, bool still, float fi)
        {
            bool full = lv >= UnityTuning.PG3V1FullAt;
            float swell = full && !still ? 1f + UnityTuning.PG3V1HoldSwell * Mathf.Sin(clock * UnityTuning.PG3V1HoldSwellHz * 2f * Mathf.PI) : 1f;
            float gk = held == "gauntlet" ? UnityTuning.PG3V1GloveK : 1f; // the glove's pips are a standing charge: a quieter glow than a hold that is about to go
            float size = Cap(cam, at, Fx74Math.ChargeSize(lv, UnityTuning.PG3V1GlowFromM, UnityTuning.PG3V1GlowToM) * swell * gk, UnityTuning.PG3V1CapSoftDeg); // never more than a soft core's share of the view
            float gain = Fx74Math.ChargeGain(lv, UnityTuning.PG3V1GlowFloor, UnityTuning.PG3V1GlowPeak) * fi * gk;
            Color body = held == "shotgun" ? Color.Lerp(cyan, Color.white, 0.35f) : cyan;
            Fx74Draw.Billboard(Fx74Tex.Soft, cam, at, size, 0f, body * (gain * UnityTuning.PG3V1GlowHaloK));
            Fx74Draw.Billboard(Fx74Tex.Soft, cam, at, size * UnityTuning.PG3V1GlowCoreK, 0f, Color.Lerp(cyan, Color.white, lv) * (gain * (0.5f + 1.5f * lv)));
            if (lv > UnityTuning.PG3V1RingFrom)
            {
                float rl = (lv - UnityTuning.PG3V1RingFrom) / (1f - UnityTuning.PG3V1RingFrom);
                Fx74Draw.Billboard(Fx74Tex.Ring, cam, at, Mathf.Lerp(UnityTuning.PG3V1RingOpenM, UnityTuning.PG3V1RingShutM, rl * rl * (3f - 2f * rl)), clock * 40f, cyan * (UnityTuning.PG3V1RingGain * rl * rl * fi));
            }
            if (lv > UnityTuning.PG3V1MotesFrom && !still)
            {
                // motes fall into the muzzle on a turning shell, faster as it fills
                Transform ct = cam.transform;
                int n = UnityTuning.PG3V1Motes;
                float hz = Mathf.Lerp(UnityTuning.PG3V1MoteHzLow, UnityTuning.PG3V1MoteHzFull, lv);
                for (int i = 0; i < n; i++)
                {
                    float r = Fx74Math.MoteRadius(clock, hz, i, n);
                    float a = i * (Mathf.PI * 2f / n) + clock * UnityTuning.PG3V1MoteTurn;
                    Vector3 p = at + (ct.right * Mathf.Cos(a) + ct.up * Mathf.Sin(a)) * (r * UnityTuning.PG3V1MoteShellM);
                    Fx74Draw.Billboard(Fx74Tex.Soft, cam, p, UnityTuning.PG3V1MoteM * (0.6f + 0.6f * (1f - r)), 0f, hot * (UnityTuning.PG3V1MoteGain * lv * (1f - r * 0.6f) * fi));
                }
            }
        }

        /// <summary>The READY flash: once per fill, a sharp ring blown out and a star, through the flash gate (refused: the ring dim, no white pop).</summary>
        void Ready(SurvivalGame g, Camera cam, Vector3 at, LightPools lights, float fi)
        {
            bool ok = g.flashGate.TryFlash(SurvivalGame.RealNow);
            float k = (ok ? 1f : UnityTuning.PG3V1DimK) * fi;
            sprites.Billboard(Fx74Tex.Ring, at, Cap(cam, at, UnityTuning.PG3V1ReadyRingFromM, UnityTuning.PG3V1CapRingDeg), Cap(cam, at, UnityTuning.PG3V1ReadyRingToM, UnityTuning.PG3V1CapRingDeg), UnityTuning.PG3V1ReadyRingS, 0f, Color.white * (UnityTuning.PG3V1ReadyRingGain * k), cyan * UnityTuning.PG3V1ReadyRingEdge);
            if (ok)
            {
                sprites.Billboard(Fx74Tex.Star, at, Cap(cam, at, UnityTuning.PG3V1ReadyStarM, UnityTuning.PG3V1CapStarDeg), Cap(cam, at, UnityTuning.PG3V1ReadyStarM * UnityTuning.PG3V1StarGrowK, UnityTuning.PG3V1CapStarDeg), UnityTuning.PG3V1ReadyStarS, 0.3f, Color.white * (UnityTuning.PG3V1ReadyStarGain * fi), cyan * UnityTuning.PG3V1ReadyStarEdge, clock * UnityTuning.PG3V1StarSpinDegS);
                if (lights != null && fi > 0f) { lights.Flash(at, cyan, UnityTuning.PG3V1ReadyLight, UnityTuning.PG3V1LightRangeM, UnityTuning.PG3V1ReadyLightS); Charge74Probe.Lights++; }
            }
            else Charge74Probe.ReadiesDimmed++;
            Charge74Probe.Readies++;
        }

        // ================================================= the release =================================================

        void Release(SurvivalGame g, Camera cam, Vector3 muzzle, LightPools lights, bool slug, float fi)
        {
            Vector3 aim = cam.transform.forward;
            Vector3 to = lastEndSet ? lastEnd : muzzle + aim * UnityTuning.PG3V1BeamFallbackM;
            bool ok = g.flashGate.TryFlash(SurvivalGame.RealNow);
            float k = (ok ? 1f : UnityTuning.PG3V1DimK) * fi;
            Vector3 d = to - muzzle;
            float len = d.magnitude;
            Vector3 dir = len > 1e-3f ? d / len : aim;
            Beam b = new Beam { alive = true, from = muzzle, to = to, age = 0f, life = slug ? UnityTuning.PG3V1SlugS : UnityTuning.PG3V1BeamS, slug = slug };
            beams[beamNext] = b;
            beamNext = (beamNext + 1) % beams.Length;
            // the blast at the muzzle
            float s = slug ? UnityTuning.PG3V1SlugK : 1f;
            float sr = Mathf.Min(s, UnityTuning.PG3V1SlugRingMaxK); // the slug's rings grow less than its streak (they would pass the screen's edge)
            // the release's pictures age on the world's time: a charged shot that hit freezes the game a moment, and the beam, its rings and the burst hold on that frame;
            // each is capped in the view's degrees (the NIGHT43 lesson: a release's flash at the muzzle must not fill the screen)
            world.Billboard(Fx74Tex.Soft, muzzle, Cap(cam, muzzle, UnityTuning.PG3V1RelCoreFromM * s, UnityTuning.PG3V1CapSoftDeg), Cap(cam, muzzle, UnityTuning.PG3V1RelCoreToM * s, UnityTuning.PG3V1CapSoftDeg), UnityTuning.PG3V1RelCoreS, 0.2f, Color.white * (UnityTuning.PG3V1RelCoreGain * k), cyan * UnityTuning.PG3V1RelCoreEdge);
            if (ok) world.Billboard(Fx74Tex.Star, muzzle, Cap(cam, muzzle, UnityTuning.PG3V1RelStarM * s, UnityTuning.PG3V1CapStarDeg), Cap(cam, muzzle, UnityTuning.PG3V1RelStarM * s * UnityTuning.PG3V1StarGrowK, UnityTuning.PG3V1CapStarDeg), UnityTuning.PG3V1RelStarS, 0.25f, Color.Lerp(cyan, Color.white, 0.6f) * (UnityTuning.PG3V1RelStarGain * fi), cyan * UnityTuning.PG3V1RelStarEdge, Mathf.Repeat(clock * UnityTuning.PG3V1StarSpinDegS, 360f));
            Vector3 r1 = muzzle + aim * UnityTuning.PG3V1RelRing1AheadM, r2 = muzzle + aim * UnityTuning.PG3V1RelRing2AheadM;
            world.Facing(Fx74Tex.Ring, r1, aim, Cap(cam, r1, UnityTuning.PG3V1RelRingFromM * sr, UnityTuning.PG3V1CapRingDeg), Cap(cam, r1, UnityTuning.PG3V1RelRingToM * sr, UnityTuning.PG3V1CapRingDeg), UnityTuning.PG3V1RelRingS, 0f, Color.white * (UnityTuning.PG3V1RelRingGain * k), cyan * UnityTuning.PG3V1RelRingEdge);
            world.Facing(Fx74Tex.Ring, r2, aim, Cap(cam, r2, UnityTuning.PG3V1RelRingFromM * sr * UnityTuning.PG3V1RelRing2K, UnityTuning.PG3V1CapRingDeg), Cap(cam, r2, UnityTuning.PG3V1RelRingToM * sr * UnityTuning.PG3V1RelRing2K, UnityTuning.PG3V1CapRingDeg), UnityTuning.PG3V1RelRingS * 1.2f, 0f, hot * (UnityTuning.PG3V1RelRingGain * UnityTuning.PG3V1RelRing2Gain * k), cyan * UnityTuning.PG3V1RelRingEdge * 0.6f, 0f, UnityTuning.PG3V1RelRing2DelayS);
            // the rings marching down the beam: from behind the gun the line reads as a tunnel
            if (!slug && len > UnityTuning.PG3V1MarchFirstM)
            {
                int n = Mathf.Min(UnityTuning.PG3V1MarchRings, Mathf.FloorToInt((len - UnityTuning.PG3V1MarchFirstM) / UnityTuning.PG3V1MarchGapM) + 1);
                for (int i = 0; i < n; i++)
                {
                    float at = UnityTuning.PG3V1MarchFirstM + i * UnityTuning.PG3V1MarchGapM;
                    world.Facing(Fx74Tex.Ring, muzzle + dir * at, dir, UnityTuning.PG3V1MarchFromM, UnityTuning.PG3V1MarchToM, UnityTuning.PG3V1MarchS, 0.1f, Color.Lerp(cyan, Color.white, 0.6f) * (UnityTuning.PG3V1MarchGain * k), cyan * 0.4f, 0f, i * UnityTuning.PG3V1MarchStaggerS);
                }
            }
            // the end of the line: a burst
            if (len > 0.5f)
            {
                Vector3 e = to - dir * UnityTuning.PG3V1EndBackM;
                world.Billboard(Fx74Tex.Star, e, Cap(cam, e, UnityTuning.PG3V1EndStarM * s, UnityTuning.PG3V1CapStarDeg), Cap(cam, e, UnityTuning.PG3V1EndStarM * s * UnityTuning.PG3V1StarGrowK, UnityTuning.PG3V1CapStarDeg), UnityTuning.PG3V1EndS, 0.25f, Color.white * (UnityTuning.PG3V1EndStarGain * k), cyan * UnityTuning.PG3V1EndStarEdge, Mathf.Repeat(clock * UnityTuning.PG3V1StarSpinDegS, 360f));
                world.Billboard(Fx74Tex.Ring, e, Cap(cam, e, UnityTuning.PG3V1EndRingFromM * s, UnityTuning.PG3V1CapRingDeg), Cap(cam, e, UnityTuning.PG3V1EndRingToM * s, UnityTuning.PG3V1CapRingDeg), UnityTuning.PG3V1EndRingS, 0f, hot * (UnityTuning.PG3V1EndRingGain * k), cyan * UnityTuning.PG3V1EndRingEdge);
            }
            if (lights != null && fi > 0f) { lights.Flash(muzzle, cyan, slug ? UnityTuning.PG3V1SlugLight : UnityTuning.PG3V1BeamLight, UnityTuning.PG3V1LightRangeM * 1.4f, UnityTuning.PG3V1ReadyLightS * 1.6f); Charge74Probe.Lights++; }
            if (slug) Charge74Probe.Slugs++; else Charge74Probe.Beams++;
            if (!ok) Charge74Probe.ReleasesDimmed++;
        }

        void DrawBeams(Camera cam, float wdt)
        {
            for (int i = 0; i < beams.Length; i++)
            {
                if (!beams[i].alive) continue;
                ref Beam b = ref beams[i];
                float u = b.age / b.life;
                float env = Fx74Math.Env(b.age, b.life, UnityTuning.PG3V1BeamHold);
                b.age += wdt;
                if (u >= 1f || env <= 0.002f) { if (u >= 1f) b.alive = false; continue; }
                float w = Mathf.Lerp(1f, UnityTuning.PG3V1BeamNarrow, Fx74Math.EaseOut(u));
                float t = b.slug ? UnityTuning.PG3V1SlugK : 1f;
                // the halo, the sleeve and the core, the same brightness along the whole line (the beam texture), narrowing as it goes
                Fx74Draw.Ribbon(cam, b.from, b.to, UnityTuning.PG3V1BeamHaloM * w * t, cyan * (UnityTuning.PG3V1BeamHaloGain * env), Fx74Tex.Beam);
                Fx74Draw.Ribbon(cam, b.from, b.to, UnityTuning.PG3V1BeamSleeveM * w * t, cyan * (UnityTuning.PG3V1BeamSleeveGain * env), Fx74Tex.Beam);
                Fx74Draw.Ribbon(cam, b.from, b.to, UnityTuning.PG3V1BeamCoreM * w * t, Color.white * (UnityTuning.PG3V1BeamCoreGain * env), Fx74Tex.Beam);
            }
        }

        // ================================================= the pulse and the parry =================================================

        void Pulse(SurvivalGame g, Camera cam, Vector3 glove, LightPools lights, float fi)
        {
            Transform ct = cam.transform;
            Vector3 feet = WebSpace.ToUnity(g.Feet) + Vector3.up * UnityTuning.PG3V1PulseFloorLiftM;
            bool ok = g.flashGate.TryFlash(SurvivalGame.RealNow);
            float k = (ok ? 1f : UnityTuning.PG3V1DimK) * fi;
            // a ring across the floor running out from the player, a second behind it, and a ring standing across the aim
            sprites.Facing(Fx74Tex.Ring, feet, Vector3.up, UnityTuning.PG3V1PulseFloorFromM, UnityTuning.PG3V1PulseFloorToM, UnityTuning.PG3V1PulseFloorS, 0f, Color.white * (UnityTuning.PG3V1PulseRingGain * k), cyan * UnityTuning.PG3V1PulseRingEdge);
            sprites.Facing(Fx74Tex.Ring, feet, Vector3.up, UnityTuning.PG3V1PulseFloorFromM, UnityTuning.PG3V1PulseFloorToM * UnityTuning.PG3V1PulseRing2K, UnityTuning.PG3V1PulseFloorS * 1.1f, 0f, hot * (UnityTuning.PG3V1PulseRingGain * UnityTuning.PG3V1PulseRing2Gain * k), cyan * UnityTuning.PG3V1PulseRingEdge * 0.75f, 0f, UnityTuning.PG3V1PulseRing2DelayS);
            Vector3 ahead = ct.position + ct.forward * UnityTuning.PG3V1PulseAheadM;
            sprites.Facing(Fx74Tex.Ring, ahead, ct.forward, Cap(cam, ahead, UnityTuning.PG3V1PulseAcrossFromM, UnityTuning.PG3V1CapRingDeg), Cap(cam, ahead, UnityTuning.PG3V1PulseAcrossToM, UnityTuning.PG3V1CapRingDeg), UnityTuning.PG3V1PulseAcrossS, 0f, Color.white * (UnityTuning.PG3V1PulseAcrossGain * k), cyan * UnityTuning.PG3V1PulseRingEdge);
            if (ok)
            {
                sprites.Billboard(Fx74Tex.Star, glove, Cap(cam, glove, UnityTuning.PG3V1PulseStarM, UnityTuning.PG3V1CapStarDeg), Cap(cam, glove, UnityTuning.PG3V1PulseStarM * UnityTuning.PG3V1StarGrowK, UnityTuning.PG3V1CapStarDeg), UnityTuning.PG3V1PulseStarS, 0.25f, Color.white * (UnityTuning.PG3V1PulseStarGain * fi), cyan * UnityTuning.PG3V1PulseStarEdge, Mathf.Repeat(clock * UnityTuning.PG3V1StarSpinDegS, 360f));
                if (lights != null && fi > 0f) { lights.Flash(glove, cyan, UnityTuning.PG3V1PulseLight, UnityTuning.PG3V1LightRangeM * 1.6f, UnityTuning.PG3V1ReadyLightS * 1.6f); Charge74Probe.Lights++; }
            }
            Charge74Probe.Pulses++;
        }

        void Parry(SurvivalGame g, Camera cam, LightPools lights, float fi)
        {
            Vector3 at = parryAt;
            Vector3 toCam = cam.transform.position - at;
            float d = toCam.magnitude;
            if (d > 1e-3f) at += toCam / d * Mathf.Min(UnityTuning.PG3V1ParryLiftMaxM, d * 0.3f); // on the camera's side of the body, a short way
            float far = Mathf.Clamp(d / UnityTuning.PG3V1ParryFarRefM, 1f, UnityTuning.PG3V1ParryFarMaxK);
            bool ok = g.flashGate.TryFlash(SurvivalGame.RealNow);
            float k = (ok ? 1f : UnityTuning.PG3V1DimK) * fi;
            float s = UnityTuning.PG3V1ParryM * far;
            if (ok)
            {
                float dc = (at - cam.transform.position).magnitude; // the flash's own distance from the eye: a close parry is capped in the view's degrees
                sprites.Billboard(Fx74Tex.Soft, at, Fx74Math.CapDeg(UnityTuning.PG3V1ParryCoreFromK * s, dc, UnityTuning.PG3V1CapSoftDeg), Fx74Math.CapDeg(UnityTuning.PG3V1ParryCoreToK * s, dc, UnityTuning.PG3V1CapSoftDeg), UnityTuning.PG3V1ParryCoreS, 0.2f, Color.white * (UnityTuning.PG3V1ParryCoreGain * fi), cyan * UnityTuning.PG3V1ParryCoreEdge);
                sprites.Billboard(Fx74Tex.Star, at, Fx74Math.CapDeg(UnityTuning.PG3V1ParryStarFromK * s, dc, UnityTuning.PG3V1CapStarDeg), Fx74Math.CapDeg(UnityTuning.PG3V1ParryStarToK * s, dc, UnityTuning.PG3V1CapStarDeg), UnityTuning.PG3V1ParryStarS, 0.3f, Color.Lerp(cyan, Color.white, 0.7f) * (UnityTuning.PG3V1ParryStarGain * fi), cyan * UnityTuning.PG3V1ParryStarEdge, Mathf.Repeat(clock * UnityTuning.PG3V1StarSpinDegS, 360f));
                if (lights != null && fi > 0f && d > UnityTuning.PG3V1ParryLightNearM) { lights.Flash(at, cyan, UnityTuning.PG3V1ParryLight, UnityTuning.PG3V1LightRangeM * 1.4f, UnityTuning.PG3V1ReadyLightS * 1.6f); Charge74Probe.Lights++; }
            }
            else Charge74Probe.ParriesDimmed++;
            // the star and the core snap on real time (gone before the parry's long freeze ends); the rings age on the world's time and hang in the freeze
            float dr = (at - cam.transform.position).magnitude;
            world.Billboard(Fx74Tex.Ring, at, Fx74Math.CapDeg(UnityTuning.PG3V1ParryRingFromK * s, dr, UnityTuning.PG3V1CapRingDeg), Fx74Math.CapDeg(UnityTuning.PG3V1ParryRingToK * s, dr, UnityTuning.PG3V1CapRingDeg), UnityTuning.PG3V1ParryRingS, 0f, Color.white * (UnityTuning.PG3V1ParryRingGain * k), cyan * UnityTuning.PG3V1ParryRingEdge);
            world.Billboard(Fx74Tex.Ring, at, Fx74Math.CapDeg(UnityTuning.PG3V1ParryRingFromK * s * UnityTuning.PG3V1ParryRing2K, dr, UnityTuning.PG3V1CapRingDeg), Fx74Math.CapDeg(UnityTuning.PG3V1ParryRingToK * s * UnityTuning.PG3V1ParryRing2K, dr, UnityTuning.PG3V1CapRingDeg), UnityTuning.PG3V1ParryRingS * 0.8f, 0f, hot * (UnityTuning.PG3V1ParryRingGain * UnityTuning.PG3V1ParryRing2Gain * k), cyan * UnityTuning.PG3V1ParryRingEdge * 0.7f, 0f, UnityTuning.PG3V1ParryRing2DelayS);
            Charge74Probe.Parries++;
            kickFrame = ok ? 0 : -1; // the screen kick only with the flash (the same gate)
        }

        // ================================================= the screen kick =================================================

        /// <summary>The parry's 1-2 rendered-frame kick: a short FOV punch-in and a pitch dip (x FOV KICK / CAMERA SHAKE; none with REDUCED MOTION), written after the
        /// camera and the viewmodel were posed (so the viewmodel's FOV is the frame's own), taken back next frame if nothing re-posed the camera.</summary>
        void Kick(SurvivalGame g, Camera cam, float dt)
        {
            if (kickFrame < 0) return;
            if (g.cfg.movement.settings.reducedMotion) { kickFrame = -1; return; }
            float w = Fx74Math.KickWeight(kickFrame, UnityTuning.PG3V1KickSecond);
            if (w <= 0f) { kickFrame = -1; return; }
            float fov = -UnityTuning.PG3V1KickFovDeg * w * (float)CameraFeel.FovKickScale(g.settings);
            float pitch = UnityTuning.PG3V1KickPitchDeg * w * (float)CameraFeel.ShakeScale(g.settings);
            kickBaseRot = cam.transform.rotation;
            kickBaseFov = cam.fieldOfView;
            cam.transform.rotation = kickBaseRot * Quaternion.Euler(-pitch, 0f, 0f);
            cam.fieldOfView = kickBaseFov + fov;
            kickWrittenRot = cam.transform.rotation;
            kickWrittenFov = cam.fieldOfView;
            kickWritten = true;
            Charge74Probe.KickFrames++;
            if (dt > 0f) kickFrame++;
        }

        void UndoKick(Camera cam)
        {
            if (kickWritten && cam != null && cam.transform.rotation == kickWrittenRot && Mathf.Approximately(cam.fieldOfView, kickWrittenFov))
            {
                cam.transform.rotation = kickBaseRot;
                cam.fieldOfView = kickBaseFov;
            }
            kickWritten = false;
        }
    }
}
