// NIGHT26 builder V: THE KILN WYRM drawn with GG's Tripo models (Assets/Art/WormBoss, baked by WormBossSetup into Resources/WormBossParts) and the glow, aura
// and animation of GG's task. VISUAL ONLY: it reads the sim's public state (Wyrm.boxes / buried / prevCenters, phase, PatternName, PathS / PathLength, markers,
// Lunging, HpFrac, the bossTell and bossHit events) and changes nothing in it. WyrmView builds it, calls Render each frame, and hides its own blocky boxes
// while it is Active (the boxes stay the sim's hit boxes; a shot finds what the old view drew, the new meshes are fitted onto the same boxes).
//   - 26 pieces = the head, the repeating body mesh (one instanced draw for all of them), the tail: 3 draw calls, 3 materials; each piece sits on its box's
//     centre, turned to the box's frame, scaled so the mesh is as long as its box (a lunge stretches the boxes: no gap) and as wide as WyrmFxWidthBlend says.
//     The chain is the sim's: the pieces already follow the head's exact trail, spaced by design.n24.wyrm.body.spacingSteps (13 steps).
//   - the glow (WyrmGlow26: pure functions) goes per piece into MaterialPropertyBlock arrays; Shaders/Wyrm26.shader makes the seams / eyes / orbs emissive HDR.
//   - the aura: an additive shell (Shaders/Wyrm26Aura.shader, the same meshes pushed out), a halo sphere round the coil, an ember ring (instanced cubes
//     placed by a render-side integer hash: no UnityEngine.Random, nothing the sim sees).
//   - the finisher: the pieces flare white-hot, then burn out head to tail; they stay drawn (frozen) after the sim's death until the last one is out.
// Fallback: with design.n26.wormVisual off, or the parts asset / shaders missing, Create returns null and WyrmView draws its blocky boxes as before.
// Not compiled in the Editor by its author (dn-check only).
using System.Diagnostics;
using Skygrind.Core;
using UnityEngine;
using UnityEngine.Rendering;
using Debug = UnityEngine.Debug;

namespace Skygrind.Game
{
    public sealed class WyrmVisual26
    {
        static readonly int BaseMap = Shader.PropertyToID("_BaseMap"), GlowColorId = Shader.PropertyToID("_GlowColor"), AmberColorId = Shader.PropertyToID("_AmberColor"),
            HotColorId = Shader.PropertyToID("_HotColor"), AshColorId = Shader.PropertyToID("_AshColor"), EmissionId = Shader.PropertyToID("_Emission"),
            MaskLoId = Shader.PropertyToID("_MaskLo"), MaskHiId = Shader.PropertyToID("_MaskHi"), WashId = Shader.PropertyToID("_Wash"), AmbientId = Shader.PropertyToID("_Ambient"), RimId = Shader.PropertyToID("_Rim"), RimPowerId = Shader.PropertyToID("_RimPower"),
            GlowId = Shader.PropertyToID("_Glow"), JawId = Shader.PropertyToID("_Jaw"), HueId = Shader.PropertyToID("_Hue"), HotId = Shader.PropertyToID("_Hot"),
            BurnId = Shader.PropertyToID("_Burn"), ModeId = Shader.PropertyToID("_Mode"), ShellId = Shader.PropertyToID("_Shell"), PowerId = Shader.PropertyToID("_Power"),
            GainId = Shader.PropertyToID("_Gain"), CullId = Shader.PropertyToID("_Cull");

        /// <summary>A hit ray the reel sets (sim space) so a scripted hit lands on a chosen piece; null: the camera's aim.</summary>
        public static (Vec3 o, Vec3 d)? HitRayProbe;

        readonly SurvivalGame game;
        readonly WormBossParts parts;
        readonly Material headMat, bodyMat, tailMat, shellMat, haloMat, emberMat;
        readonly Mesh sphere, cube;
        readonly int pieces;
        readonly Matrix4x4[] bodyM, shellBodyM;
        readonly Matrix4x4[] headM = new Matrix4x4[1], tailM = new Matrix4x4[1], emberM;
        readonly float[] bGlow, bJaw, bHue, bHot, bBurn, bAura, hGlow = new float[1], hJaw = new float[1], hHue = new float[1], hHot = new float[1], hBurn = new float[1],
            hAura = new float[1], tGlow = new float[1], tJaw = new float[1], tHue = new float[1], tHot = new float[1], tBurn = new float[1], tAura = new float[1],
            haloGlow = new float[1], haloHue = new float[1], eGlow, eHue;
        readonly MaterialPropertyBlock mpbHead = new MaterialPropertyBlock(), mpbBody = new MaterialPropertyBlock(), mpbTail = new MaterialPropertyBlock(),
            mpbShellHead = new MaterialPropertyBlock(), mpbShellBody = new MaterialPropertyBlock(), mpbShellTail = new MaterialPropertyBlock(),
            mpbHalo = new MaterialPropertyBlock(), mpbEmber = new MaterialPropertyBlock();
        readonly float[] hotAge;
        readonly bool[] latchBuried;
        readonly Matrix4x4[] haloM = new Matrix4x4[1];
        readonly Stopwatch watch = new Stopwatch();

        Wyrm cur;
        bool wasCoiled;
        float clock, idlePhase, coilPhase, coilWeight, approach, emberAngle, deathAge = -1f;
        float releaseClock = -1e9f, impactClock = -1e9f, trailClock = -1e9f, tellStart, tellDur, tellPhase;
        WyrmKind26 tellKind, impactKind;
        float lastHp = 1f, lastPathS, pathSpeed = 22f;
        int lastStep = -1;

        /// <summary>The effects (glow, aura, flash, death) are on; false draws the models with a fixed idle glow only (the cost measure's "off").</summary>
        public bool FxOn = true;
        /// <summary>Measure the CPU time of Render (Stopwatch ticks): the reel's frame-cost helper reads and clears it.</summary>
        public bool Measure;
        public long RenderTicks;
        public int RenderCalls;

        // --- what the reel reads (the latest frame) ------------------------------------------------------------------------------
        public WyrmFxState26 State;
        public readonly WyrmPieceFx26[] Fx;
        public float CoilRateHz, SecondsToRelease;
        public bool HaloOn;
        public int EmbersDrawn, DrawCallsLast, HitPieceLast = -1;
        public float TellRampLast;
        public WyrmKind26 TellKindLast;
        public bool Drawn;

        public bool Active => parts != null;

        WyrmVisual26(SurvivalGame game, WormBossParts parts, int pieces)
        {
            this.game = game;
            this.parts = parts;
            this.pieces = pieces;
            Color glow = Look.Hex(UnityTuning.WyrmFxGlowHex).linear, amber = Look.Hex(UnityTuning.WyrmFxAmberHex).linear, hot = Look.Hex(UnityTuning.WyrmFxHotHex).linear,
                ash = Look.Hex(UnityTuning.WyrmFxAshHex).linear;
            headMat = Armour("Wyrm head", parts.headTex, glow, amber, hot, ash);
            bodyMat = Armour("Wyrm body", parts.bodyTex, glow, amber, hot, ash);
            tailMat = Armour("Wyrm tail", parts.tailTex, glow, amber, hot, ash);
            // the procedural emission (it does not read the texture's colours): the head's eyes and a few seams, the body's seams and top orbs, the tail's seams
            headMat.SetFloat(Shader.PropertyToID("_Len"), parts.headSize.z);
            headMat.SetFloat(Shader.PropertyToID("_EyeOn"), 1f);
            headMat.SetVector(Shader.PropertyToID("_EyeA"), UnityTuning.WyrmFxEyeA);
            headMat.SetFloat(Shader.PropertyToID("_SeamOn"), 0.6f);
            bodyMat.SetFloat(Shader.PropertyToID("_Len"), parts.bodySize.z);
            bodyMat.SetFloat(Shader.PropertyToID("_SeamOn"), 1f);
            bodyMat.SetFloat(Shader.PropertyToID("_OrbOn"), 1f);
            bodyMat.SetFloat(Shader.PropertyToID("_OrbY"), UnityTuning.WyrmFxOrbY);
            tailMat.SetFloat(Shader.PropertyToID("_Len"), parts.tailSize.z);
            tailMat.SetFloat(Shader.PropertyToID("_SeamOn"), 1f);
            shellMat = Aura("Wyrm shell", 0, glow, amber);
            shellMat.SetFloat(ShellId, UnityTuning.WyrmFxShell);
            shellMat.SetFloat(PowerId, UnityTuning.WyrmFxShellPower);
            haloMat = Aura("Wyrm halo", 1, glow, amber);
            haloMat.SetFloat(CullId, (float)CullMode.Front); // back faces only: one layer of overdraw, and it still shows with the camera inside the coil
            haloMat.SetFloat(PowerId, 1.6f);
            emberMat = Aura("Wyrm embers", 2, glow, amber);
            var ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere = ball.GetComponent<MeshFilter>().sharedMesh; // radius 0.5
            Object.Destroy(ball);
            var box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube = box.GetComponent<MeshFilter>().sharedMesh;
            Object.Destroy(box);

            int bodies = Mathf.Max(1, pieces - 2);
            bodyM = new Matrix4x4[bodies];
            shellBodyM = bodyM;
            bGlow = new float[bodies];
            bJaw = new float[bodies];
            bHue = new float[bodies];
            bHot = new float[bodies];
            bBurn = new float[bodies];
            bAura = new float[bodies];
            emberM = new Matrix4x4[UnityTuning.WyrmFxEmbers];
            eGlow = new float[UnityTuning.WyrmFxEmbers];
            eHue = new float[UnityTuning.WyrmFxEmbers];
            hotAge = new float[pieces];
            latchBuried = new bool[pieces];
            Fx = new WyrmPieceFx26[pieces];
            for (int i = 0; i < pieces; i++) hotAge[i] = WyrmGlow26.None;

            game.events.On<BossEvent>("bossTell", OnTell);
            game.events.On<BossEvent>("bossHit", OnHit);
        }

        /// <summary>The visual, or null (row off, or the parts asset missing / not built / without its shaders): the caller keeps the blocky boxes.</summary>
        public static WyrmVisual26 Create(SurvivalGame game, int pieces)
        {
            GameConfig cfg = game.cfg;
            if (!cfg.Has("design.n26.wormVisual.enabled") || !cfg.Design("n26.wormVisual.enabled")) return null;
            var parts = Resources.Load<WormBossParts>(WormBossParts.Resource);
            if (parts == null || !parts.Ready)
            {
                Debug.LogWarning("[wyrm] design.n26.wormVisual is on but Resources/" + WormBossParts.Resource + " is " + (parts == null ? "missing" : "not ready")
                    + ": run SKYGRIND > Worm Boss > Build Prefab. The blocky worm is drawn.");
                return null;
            }
            return new WyrmVisual26(game, parts, pieces);
        }

        Material Armour(string name, Texture2D tex, Color glow, Color amber, Color hot, Color ash)
        {
            var m = new Material(parts.armour) { name = name, enableInstancing = true };
            if (tex != null) m.SetTexture(BaseMap, tex);
            m.SetVector(GlowColorId, glow);
            m.SetVector(AmberColorId, amber);
            m.SetVector(HotColorId, hot);
            m.SetVector(AshColorId, ash);
            m.SetFloat(EmissionId, UnityTuning.WyrmFxEmission);
            m.SetFloat(MaskLoId, UnityTuning.WyrmFxMaskLo);
            m.SetFloat(MaskHiId, UnityTuning.WyrmFxMaskHi);
            m.SetFloat(WashId, UnityTuning.WyrmFxWash);
            m.SetFloat(AmbientId, UnityTuning.WyrmFxAmbient);
            m.SetFloat(RimId, UnityTuning.WyrmFxRim);
            m.SetFloat(RimPowerId, UnityTuning.WyrmFxRimPower);
            return m;
        }

        Material Aura(string name, int mode, Color glow, Color amber)
        {
            var m = new Material(parts.aura) { name = name, enableInstancing = true, renderQueue = (int)RenderQueue.Transparent };
            m.SetVector(GlowColorId, glow);
            m.SetVector(AmberColorId, amber);
            m.SetFloat(ModeId, mode);
            m.SetFloat(GainId, UnityTuning.WyrmFxEmission);
            return m;
        }

        // --- the sim's events ---------------------------------------------------------------------------------------------------------

        /// <summary>The sim's tell names (design.n24.wyrm.tells) onto the glow's kinds.</summary>
        public static WyrmKind26 KindOfTell(string name)
        {
            switch (name)
            {
                case "dive":
                case "crack": return WyrmKind26.Dive;
                case "lunge": return WyrmKind26.Lunge;
                case "spit": return WyrmKind26.Spit;
                case "roar": return WyrmKind26.Roar;
                case "squeeze": return WyrmKind26.Squeeze;
                case "shed": return WyrmKind26.Shed;
                default: return WyrmKind26.None;
            }
        }

        void OnTell(BossEvent e)
        {
            Wyrm w = game.sim?.wyrm24;
            if (e == null || e.boss != "wyrm" || w == null) return;
            WyrmKind26 k = KindOfTell(e.name);
            if (k == WyrmKind26.None) return;
            // a tell's clock is the sim's: the step it was announced on (the event fires inside the step, stepNo is already counted)
            tellKind = k;
            tellStart = w.stepNo / UnityTuning.WyrmFxStepHz;
            tellDur = Mathf.Max(0.05f, (float)e.seconds);
        }

        void OnHit(BossEvent e)
        {
            Wyrm w = game.sim?.wyrm24;
            if (e == null || e.boss != "wyrm" || w == null || cur != w) return;
            int piece = FindHitPiece(w, e.part);
            HitPieceLast = piece;
            if (piece >= 0 && piece < pieces) hotAge[piece] = 0f;
        }

        /// <summary>Which piece a hit landed on: the nearest box along the aim ray (the camera's, or the reel's probe ray), among those of the part the sim named;
        /// else by the part's name (the head and the core: piece 0, the tail: the last).</summary>
        int FindHitPiece(Wyrm w, string part)
        {
            int n = w.PieceCount;
            Vec3 o, d;
            if (HitRayProbe.HasValue)
            {
                o = HitRayProbe.Value.o;
                d = HitRayProbe.Value.d;
            }
            else
            {
                Transform t = game.cam != null ? game.cam.transform : null;
                if (t == null) return PieceByName(n, part);
                o = WebSpace.FromUnity(t.position);
                d = WebSpace.FromUnity(t.forward);
            }
            int best = -1;
            double bt = double.MaxValue;
            for (int i = 0; i < n; i++)
            {
                if (w.buried[i]) continue;
                bool head = i == 0, tail = i == n - 1;
                if (part == "head" || part == "core") { if (!head) continue; }
                else if (part == "tail") { if (!tail && !(w.ShedActive && i >= w.ShedFirst && i < w.ShedEnd)) continue; }
                else if (head || tail) continue;
                double t = w.boxes[i].Ray(o, d);
                if (t >= 0 && t < bt)
                {
                    bt = t;
                    best = i;
                }
            }
            return best >= 0 ? best : PieceByName(n, part);
        }

        static int PieceByName(int n, string part) => part == "head" || part == "core" ? 0 : part == "tail" ? n - 1 : n / 2;

        // --- the frame ------------------------------------------------------------------------------------------------------------------

        static Vector3 U(Vec3 v) => new Vector3((float)v.x, (float)v.y, (float)-v.z);

        void Reset(Wyrm w)
        {
            cur = w;
            wasCoiled = false;
            idlePhase = coilPhase = coilWeight = approach = 0f;
            deathAge = -1f;
            releaseClock = impactClock = trailClock = -1e9f;
            tellKind = impactKind = WyrmKind26.None;
            for (int i = 0; i < pieces; i++) hotAge[i] = WyrmGlow26.None;
            lastHp = 1f;
            lastStep = -1;
        }

        /// <summary>Draw the worm for this frame (called by WyrmView.Render before it decides whether the boxes are drawn): the sim's worm, interpolated on the sim's
        /// render alpha; `dt` is the world's frame time (0 in a hitstop, slow in the finisher's slow-mo).</summary>
        public void Render(Wyrm w, double alpha, float dt)
        {
            Drawn = false;
            if (parts == null) return;
            if (Measure) watch.Restart();
            RenderBody(w, alpha, dt);
            if (Measure)
            {
                watch.Stop();
                RenderTicks += watch.ElapsedTicks;
                RenderCalls++;
            }
        }

        void RenderBody(Wyrm w, double alpha, float dt)
        {
            bool alive = w != null && w.Alive;
            if (alive && w != cur) Reset(w);
            if (cur == null) return;
            Wyrm s = cur;
            if (!alive)
            {
                // the sim's worm is gone: it stays drawn, frozen, while the finisher's burn-out finishes
                if (!FxOn || deathAge < 0f || deathAge > WyrmGlow26.DeathTotal(pieces) + 0.25f)
                {
                    cur = null;
                    deathAge = -1f;
                    return;
                }
            }
            int n = s.PieceCount;
            if (n != pieces) return;
            float a = alive ? Mathf.Clamp01((float)alpha) : 1f;
            if (alive) clock = (s.stepNo - 1 + a) / UnityTuning.WyrmFxStepHz;
            WyrmParams p = s.p;

            // --- the pose of the finisher: the sim sinks the pieces under the floor as it dies; the view holds them up so the burn is seen ------------------
            bool dying = s.phase == "dying" || !alive;
            if (dying && deathAge < 0f)
            {
                deathAge = 0f;
                for (int i = 0; i < n; i++) latchBuried[i] = s.buried[i];
            }
            if (deathAge >= 0f) deathAge += dt;
            float sinkY = 0f;
            if (dying)
            {
                float k = Mathf.Clamp01(s.phaseSteps / Mathf.Max(1f, (float)p.dieTime * UnityTuning.WyrmFxStepHz));
                sinkY = k * k * (float)p.dieSink;
            }

            // --- the state the glow reads ----------------------------------------------------------------------------------------------
            string pattern = s.PatternName;
            bool coiled = alive && (pattern == "coil" || pattern == "loop") && s.phase != "dying";
            // the head's speed along the path, measured from the sim's own arc-length parameter over the steps since the last frame (exact; the Euclidean velocity
            // is not the path speed on a curve, and the lunge / phase multipliers are in it already)
            if (alive && s.stepNo != lastStep)
            {
                float ds = (float)s.PathS - lastPathS;
                if (lastStep >= 0 && ds >= 0f && s.stepNo > lastStep) pathSpeed = Mathf.Lerp(pathSpeed, ds * UnityTuning.WyrmFxStepHz / (s.stepNo - lastStep), 0.25f);
                lastPathS = (float)s.PathS;
                lastStep = s.stepNo;
            }
            float speed = Mathf.Max(1f, pathSpeed);
            float secLeft = coiled ? Mathf.Max(0f, (float)(s.PathLength - s.PathS)) / speed : 1e9f;
            if (wasCoiled && !coiled && alive) releaseClock = clock;
            wasCoiled = coiled;
            float releaseAge = clock - releaseClock;
            float targetApproach = coiled ? WyrmGlow26.Approach(secLeft) : (releaseAge < UnityTuning.WyrmFxReleaseSec ? 1f : 0f);
            approach = Mathf.MoveTowards(approach, targetApproach, dt * 6f);
            coilWeight = Mathf.MoveTowards(coilWeight, coiled ? 1f : 0f, dt / Mathf.Max(0.01f, UnityTuning.WyrmFxCoilBlendSec));
            float rate = WyrmGlow26.CoilRate(approach);
            idlePhase += dt * UnityTuning.WyrmFxIdleRate;
            coilPhase += dt * rate;
            CoilRateHz = coilWeight > 0.5f ? rate : 0f;
            SecondsToRelease = coiled ? secLeft : -1f;

            // the tell's progress on the sim's clock; at its end it resolves: the impact (and the trail of a dive / lunge)
            float ramp = 0f;
            WyrmKind26 shownTell = WyrmKind26.None;
            if (tellKind != WyrmKind26.None && alive)
            {
                ramp = (clock - tellStart) / tellDur;
                if (ramp >= 1f)
                {
                    impactKind = tellKind;
                    impactClock = tellStart + tellDur;
                    if (tellKind == WyrmKind26.Dive || tellKind == WyrmKind26.Lunge) trailClock = impactClock;
                    tellKind = WyrmKind26.None;
                    ramp = 0f;
                }
                else shownTell = tellKind;
            }
            else if (!alive) tellKind = WyrmKind26.None;
            ramp = Mathf.Clamp01(ramp);
            WyrmKind26 sk = shownTell;
            float rateTell = sk == WyrmKind26.None ? 0f : UnityTuning.WyrmFxTellRate[(int)sk];
            WyrmGlow26.StrobeDepth54 = 1f;
            if (rateTell > 0f && Light54.On(game.cfg))
            {
                // NIGHT54 light54 (fix 10): a strobe at or over numbers.strobeHz becomes a swell (its hue by kind unchanged)
                double hz = rateTell, d = 1;
                Light54.Calm(game.cfg, ref hz, ref d);
                rateTell = (float)hz;
                WyrmGlow26.StrobeDepth54 = (float)d;
            }
            tellPhase += dt * rateTell;
            for (int i = 0; i < pieces; i++) if (hotAge[i] < WyrmGlow26.None) hotAge[i] += dt;

            var st = new WyrmFxState26
            {
                pieces = n,
                idlePhase = idlePhase,
                coilPhase = coilPhase,
                coilWeight = FxOn ? coilWeight : 0f,
                approach = approach,
                releaseAge = FxOn ? releaseAge : WyrmGlow26.None,
                tellKind = FxOn ? shownTell : WyrmKind26.None,
                tellRamp = ramp,
                tellPhase = tellPhase,
                impactKind = impactKind,
                impactAge = FxOn ? clock - impactClock : WyrmGlow26.None,
                trailAge = FxOn ? clock - trailClock : WyrmGlow26.None,
                lunging = FxOn && alive && s.Lunging,
                hpFrac = FxOn ? (float)s.HpFrac : 1f,
                deathAge = FxOn ? deathAge : -1f,
                tailFrom = Mathf.Max(1, n - 1 - p.shedPieces),
            };
            if (!FxOn)
            {
                st.idlePhase = 0.25f; // a fixed mid glow
                st.coilWeight = 0f;
            }
            lastHp = st.hpFrac;
            State = st;
            TellKindLast = shownTell;
            TellRampLast = ramp;

            // --- the poses and the per-piece look ----------------------------------------------------------------------------------------
            Vector3 centroid = Vector3.zero;
            int visible = 0;
            int bodyCount = 0;
            bool headOn = false, tailOn = false;
            float auraMaxHead = 0f;
            Vector3[] centers = centerScratch ??= new Vector3[n];
            for (int i = 0; i < n; i++)
            {
                bool hidden = dying ? latchBuried[i] : s.buried[i];
                float hot = FxOn ? Mathf.Clamp01(WyrmGlow26.Hot(hotAge[i]) + UnityTuning.WyrmFxHotNeighbour
                    * (WyrmGlow26.Hot(i > 0 ? hotAge[i - 1] : WyrmGlow26.None) + WyrmGlow26.Hot(i + 1 < n ? hotAge[i + 1] : WyrmGlow26.None))) : 0f;
                WyrmPieceFx26 fx = WyrmGlow26.Evaluate(st, i, hot);
                if (!FxOn)
                {
                    fx.aura = 0f;
                    fx.hue = 0f;
                    fx.jaw = 0f;
                    fx.hot = 0f;
                    fx.burn = 0f;
                    fx.glow = UnityTuning.WyrmFxIdleMin; // "effects off" is the dimmest idle glow, constant
                }
                Fx[i] = fx;
                if (hidden) continue;
                WyrmBox b = s.boxes[i];
                float off = i == 0 ? sinkY : sinkY * (1f + i * (float)p.sinkTaper);
                Vector3 cur3 = U(b.c), prev3 = U(s.prevCenters[i]);
                Vector3 c = Vector3.Lerp(prev3, cur3, a) + Vector3.up * off;
                Vector3 f = U(b.f), up = U(b.u);
                Quaternion q = Quaternion.LookRotation(f, up);
                Vector3 size = i == 0 ? parts.headSize : i == n - 1 ? parts.tailSize : parts.bodySize;
                float uLen = (float)b.hf * 2f * UnityTuning.WyrmFxLengthFit / size.z;
                float uWid = (float)b.hr * 2f / size.x;
                float uXY = Mathf.Lerp(uLen, uWid, UnityTuning.WyrmFxWidthBlend);
                Matrix4x4 m = Matrix4x4.TRS(c, q, new Vector3(uXY, uXY, uLen));
                centers[i] = c;
                centroid += c;
                visible++;
                if (i == 0)
                {
                    headM[0] = m;
                    hGlow[0] = fx.glow; hJaw[0] = fx.jaw; hHue[0] = fx.hue; hHot[0] = fx.hot; hBurn[0] = fx.burn; hAura[0] = fx.aura;
                    headOn = true;
                    auraMaxHead = fx.aura;
                }
                else if (i == n - 1)
                {
                    tailM[0] = m;
                    tGlow[0] = fx.glow; tJaw[0] = 0f; tHue[0] = fx.hue; tHot[0] = fx.hot; tBurn[0] = fx.burn; tAura[0] = fx.aura;
                    tailOn = true;
                }
                else
                {
                    bodyM[bodyCount] = m;
                    bGlow[bodyCount] = fx.glow; bJaw[bodyCount] = 0f; bHue[bodyCount] = fx.hue; bHot[bodyCount] = fx.hot; bBurn[bodyCount] = fx.burn; bAura[bodyCount] = fx.aura;
                    bodyCount++;
                }
            }

            // --- the draws: three armour calls, up to three shell calls, the halo, the embers ----------------------------------------------
            int calls = 0;
            if (headOn)
            {
                DrawPiece(parts.head, headMat, headM[0], hGlow[0], hJaw[0], hHue[0], hHot[0], hBurn[0]);
                calls++;
            }
            if (bodyCount > 0)
            {
                for (int b = 0; b < bodyCount; b++) DrawPiece(parts.body, bodyMat, bodyM[b], bGlow[b], bJaw[b], bHue[b], bHot[b], bBurn[b]);
                calls++;
            }
            if (tailOn)
            {
                DrawPiece(parts.tail, tailMat, tailM[0], tGlow[0], tJaw[0], tHue[0], tHot[0], tBurn[0]);
                calls++;
            }
            const float AuraMin = 0.02f;
            if (headOn && auraMaxHead > AuraMin)
            {
                DrawAura(parts.head, shellMat, headM[0], hAura[0], hHue[0]);
                calls++;
            }
            // (NIGHT26 V, cost pass: no shell on the 24 body pieces and the tail: it re-drew their whole mesh additively; the coil's aura is the halo and the embers)

            // the coil's halo and ember ring: only while the worm is coiled (or the release burst is flying)
            HaloOn = false;
            EmbersDrawn = 0;
            float burstAge = releaseAge;
            bool burst = FxOn && burstAge >= 0f && burstAge < UnityTuning.WyrmFxEmberBurstSec;
            float coilShow = FxOn ? coilWeight : 0f;
            bool inFront = true;
            if (visible > 0 && game.cam != null)
            {
                Transform ct = game.cam.transform;
                inFront = Vector3.Dot(ct.forward, centroid / visible - ct.position) > -UnityTuning.WyrmFxCullBehind; // the coil's centre is not behind the camera
            }
            if (visible > 0 && alive && inFront && (coilShow > 0.04f || burst))
            {
                centroid /= visible;
                float radius = 0f;
                for (int i = 0; i < n; i++)
                {
                    if (dying ? latchBuried[i] : s.buried[i]) continue;
                    Vector3 dv = centers[i] - centroid;
                    dv.y = 0f;
                    radius += dv.magnitude;
                }
                radius = Mathf.Max(1.5f, radius / visible);
                float level = coilShow * (0.25f + 0.75f * approach) + (burst ? 1f - burstAge / UnityTuning.WyrmFxEmberBurstSec : 0f);
                haloGlow[0] = UnityTuning.WyrmFxHaloIntensity * level;
                haloM[0] = Matrix4x4.TRS(centroid, Quaternion.identity, Vector3.one * (radius * 2f * UnityTuning.WyrmFxHaloScale));
                if (haloGlow[0] > 0.01f)
                {
                    DrawAura(sphere, haloMat, haloM[0], haloGlow[0], 0f);
                    HaloOn = true;
                    calls++;
                }
                DrawEmbers(centroid, radius, approach, coilShow, burst ? burstAge : -1f, dt);
                if (EmbersDrawn > 0) calls++;
            }
            DrawCallsLast = calls;
            Drawn = true;
        }

        Vector3[] centerScratch;

        // One DrawMesh per piece with plain float properties in a MaterialPropertyBlock: the first Player build used instanced property arrays and the glow never
        // reached the screen (ON and OFF frames read the same pixels). 26 small draws cost nothing next to that.
        readonly MaterialPropertyBlock mpbOne = new MaterialPropertyBlock();

        void DrawPiece(Mesh mesh, Material mat, Matrix4x4 m, float glow, float jaw, float hue, float hot, float burn)
        {
            mpbOne.SetFloat(GlowId, glow);
            mpbOne.SetFloat(JawId, jaw);
            mpbOne.SetFloat(HueId, hue);
            mpbOne.SetFloat(HotId, hot);
            mpbOne.SetFloat(BurnId, burn);
            Graphics.DrawMesh(mesh, m, mat, 0, null, 0, mpbOne, ShadowCastingMode.Off, false);
        }

        void DrawAura(Mesh mesh, Material mat, Matrix4x4 m, float glow, float hue)
        {
            mpbOne.SetFloat(GlowId, glow);
            mpbOne.SetFloat(HueId, hue);
            Graphics.DrawMesh(mesh, m, mat, 0, null, 0, mpbOne, ShadowCastingMode.Off, false);
        }

        static void SetArrays(MaterialPropertyBlock b, float[] glow, float[] jaw, float[] hue, float[] hot, float[] burn)
        {
            b.SetFloatArray(GlowId, glow);
            b.SetFloatArray(JawId, jaw);
            b.SetFloatArray(HueId, hue);
            b.SetFloatArray(HotId, hot);
            b.SetFloatArray(BurnId, burn);
        }

        static void SetAura(MaterialPropertyBlock b, float[] aura, float[] hue)
        {
            b.SetFloatArray(GlowId, aura);
            b.SetFloatArray(HueId, hue);
        }

        // --- the embers: a render-side integer hash places them (no UnityEngine.Random: the sim never sees it, a reel never depends on it) ---------

        /// <summary>A hash of (i, salt) in 0..1 (an integer mix: the same every run, not gameplay randomness).</summary>
        public static float Hash01(int i, int salt)
        {
            uint x = (uint)(i * 747796405 + salt * 2891336453u + 12345);
            x ^= x >> 16;
            x *= 2246822519u;
            x ^= x >> 13;
            x *= 3266489917u;
            x ^= x >> 16;
            return (x & 0xFFFFFF) / 16777216f;
        }

        void DrawEmbers(Vector3 centroid, float radius, float appr, float coilShow, float burstAge, float dt)
        {
            int count = emberM.Length;
            float turn = Mathf.Lerp(UnityTuning.WyrmFxEmberTurnFar, UnityTuning.WyrmFxEmberTurnNear, appr);
            emberAngle += turn * dt;
            float level = coilShow * (0.3f + 0.7f * appr);
            float burstK = burstAge >= 0f ? burstAge / UnityTuning.WyrmFxEmberBurstSec : 0f;
            for (int i = 0; i < count; i++)
            {
                float h1 = Hash01(i, 1), h2 = Hash01(i, 2), h3 = Hash01(i, 3), h4 = Hash01(i, 4);
                float ang = h1 * 6.2831853f + emberAngle * (0.6f + 0.8f * h2);
                float life = Mathf.Repeat(h4 + clock * UnityTuning.WyrmFxEmberRise / UnityTuning.WyrmFxEmberSpread, 1f); // 0 low .. 1 high, wraps
                float r = radius * UnityTuning.WyrmFxEmberRadius * (0.85f + 0.3f * h2);
                float y = (life - 0.5f) * UnityTuning.WyrmFxEmberSpread * (0.6f + 0.8f * h3);
                float fade = Mathf.Sin(life * Mathf.PI);
                float glow = level * fade;
                if (burstAge >= 0f)
                {
                    r += UnityTuning.WyrmFxEmberBurst * burstAge * (0.5f + h3);
                    y += UnityTuning.WyrmFxEmberBurst * 0.35f * burstAge * (h2 - 0.3f);
                    glow = Mathf.Max(glow, (1f - burstK) * (0.5f + 0.5f * h1));
                }
                var pos = new Vector3(centroid.x + Mathf.Cos(ang) * r, centroid.y + y, centroid.z + Mathf.Sin(ang) * r);
                float sz = UnityTuning.WyrmFxEmberSize * (0.6f + 0.8f * h4);
                emberM[i] = Matrix4x4.TRS(pos, Quaternion.Euler(h1 * 360f, ang * 57.29578f, h3 * 360f), Vector3.one * sz);
                eGlow[i] = glow * UnityTuning.WyrmFxEmberGain;
                eHue[i] = h2 * 0.5f;
                if (eGlow[i] > 0.02f) EmbersDrawn++;
            }
            if (EmbersDrawn == 0) return;
            for (int i = 0; i < count; i++)
                if (eGlow[i] > 0.02f) DrawAura(cube, emberMat, emberM[i], eGlow[i], eHue[i]);
        }
    }
}
