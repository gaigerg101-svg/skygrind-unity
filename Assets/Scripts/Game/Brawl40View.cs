// NIGHT40 builder B2 (brawl40; NIGHT40.md B2): what the player SEES of THE ICEBREAKER and its arena. The lead adds two lines in
// SurvivalGame.Boss26.cs:   Boss26InitViews: `Brawl40View.Current = new Brawl40View(this);`
//                           Boss26RenderViews: `Brawl40View.Current?.Render(alpha);`
//  - THE ARENA (whenever the sim has built it: a level with the brawl40 row, or the reel's placement): the SAME boxes the sim collides with
//    (design.n40.brawl40.arena) in dusk stone and dark ice, a cyan trim at the foot and the top of every ride face (the rim walls' inner faces
//    and every pillar face: cyan = player-usable), the entry slab red-orange while locked.
//  - THE BODY (enemies-web.md section 4: big fists, a small head, hunched; squares for a tank): a jointed brute of blocks, dark (a silhouette)
//    with red-orange rim strips; two short thick legs in a walk cycle, a hunched torso, a small head, two arms each a box from the shoulder to
//    its FIST, a big cube placed where the sim's fist is (so the tell, the swing and the stuck fist read exactly where the hit test is).
//  - A POSE PER STATE: walk; haymaker wind-up (the right fist cocked high and back, glowing red-orange brighter through the wind-up); swing
//    (the arm out); STUCK (bent over, the fist in the ice, CYAN: hit it now); slam wind-up (both fists overhead, both glowing, a red-orange
//    ring decal on the ice at the shockwave's reach); the shockwave (a red-orange ring running out along the floor); block wind-up (a YELLOW
//    ice block raised in the left fist), the block in flight (yellow; white on its way back); rush wind-up (head down, a red-orange ground line
//    along its locked path); reeling in the window (down on one knee, CYAN); the phase change (arms spread, the rims flare); dying (it falls).
// Rule 8 colours (UnityTuning.Brawl40).
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace Skygrind.Game
{
    public sealed partial class Brawl40View
    {
        public static Brawl40View Current;

        partial void Kill74Brawl(); // NIGHT74 B kill74 (Brawl40View.Kill74.cs)

        readonly SurvivalGame game;
        readonly Transform root, arenaRoot, fxRoot;
        readonly Mesh boxMesh;
        readonly Material add;
        readonly MaterialPropertyBlock block = new MaterialPropertyBlock();
        static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        Material bodyMat, bodyDark;
        Vec3 builtAt;
        WyrmArena builtArena;
        GameObject slabGo;
        Transform bodyRoot, lean, torso, head, legL, legR, armR, armL, fistR, fistL, blockHeld, blockFly, lineT;
        Renderer fistRR, fistLR, blockHeldR, blockFlyR, lineR, eyesR;
        readonly List<Renderer> rimRs = new List<Renderer>();
        readonly List<Transform> decalTs = new List<Transform>(), waveTs = new List<Transform>();
        readonly List<Renderer> decalRs = new List<Renderer>(), waveRs = new List<Renderer>();
        readonly List<Renderer> floeTellRs = new List<Renderer>();
        readonly List<GameObject> floeWaterGos = new List<GameObject>();
        int builtFloes = -1;
        Vec3 floeAt;
        float gait, hitFlash, fall;
        Vec3 lastFeet;

        /// <summary>Frames the reel reads: the arena drawn, the slab shown, the pose name, the tell glow now (0..1), the ring / block / line shown.</summary>
        public bool ArenaShown => arenaRoot != null && arenaRoot.gameObject.activeSelf;
        public bool SlabShown => slabGo != null && slabGo.activeSelf;
        public string Pose = "";
        public float TellGlow;
        public bool RingShown, DecalShown, BlockShown, LineShown, CyanShown;
        public int FloesTold, FloesOpen;

        public Brawl40View(SurvivalGame game)
        {
            this.game = game;
            Current = this;
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            boxMesh = cube.GetComponent<MeshFilter>().sharedMesh;
            Object.Destroy(cube);
            add = FxDraw.Additive(game.mats.neon, "Brawl40 glow");
            root = new GameObject("NIGHT40 THE ICEBREAKER").transform;
            arenaRoot = new GameObject("arena").transform;
            arenaRoot.SetParent(root, false);
            arenaRoot.gameObject.SetActive(false);
            fxRoot = new GameObject("fight").transform;
            fxRoot.SetParent(root, false);
            fxRoot.gameObject.SetActive(false);
            bodyMat = Opaque("brawl40 body", UnityTuning.Brawl40BodyHex);
            bodyDark = Opaque("brawl40 body dark", UnityTuning.Brawl40BodyDarkHex);
            BuildBody();
            Init41(); // NIGHT41 B8 (brawlLook41): the ice armour's meshes, frost, fist shells (Brawl40View.N41.cs; off: nothing drawn)
            for (int i = 0; i < UnityTuning.Brawl40RingSegments; i++)
            {
                decalRs.Add(Glow(fxRoot, "slam decal", out Transform d));
                decalTs.Add(d);
                waveRs.Add(Glow(fxRoot, "slam wave", out Transform w));
                waveTs.Add(w);
            }
            blockFlyR = Glow(fxRoot, "thrown block", out blockFly);
            lineR = Glow(fxRoot, "rush line", out lineT);
            game.events.On<BossEvent>("bossHit", e => { if (e.boss == "brawl40") hitFlash = UnityTuning.Brawl40HitFlash; });
        }

        static Vector3 U(Vec3 v) => new Vector3((float)v.x, (float)v.y, (float)-v.z); // web z is Unity -z

        void Tint(Renderer r, Color c)
        {
            block.SetColor(BaseColor, c);
            r.SetPropertyBlock(block);
        }

        Material Opaque(string name, uint hex)
        {
            var mat = new Material(game.mats.stone) { name = name };
            Color c = Look.Hex(hex);
            if (mat.HasProperty(BaseColor)) mat.SetColor(BaseColor, c);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", c);
            if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", null);
            if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", null);
            return Light54.StoneOrLit(game, mat, "Brawl40View", name, hex); // NIGHT54 light54: Stone.shader reads the vertex colour, not _BaseColor (white): the intended hex on URP Lit with the row on
        }

        Renderer Glow(Transform parent, string name, out Transform tr)
        {
            var g = new GameObject(name);
            g.transform.SetParent(parent, false);
            g.AddComponent<MeshFilter>().sharedMesh = boxMesh;
            var r = g.AddComponent<MeshRenderer>();
            r.sharedMaterial = add;
            r.shadowCastingMode = ShadowCastingMode.Off;
            tr = g.transform;
            g.SetActive(false);
            return r;
        }

        Transform Pivot(Transform parent, string name, Vector3 at)
        {
            var t = new GameObject(name).transform;
            t.SetParent(parent, false);
            t.localPosition = at;
            return t;
        }

        Transform Part(Transform parent, string name, Vector3 at, Vector3 size, Material mat)
        {
            var g = new GameObject(name);
            g.transform.SetParent(parent, false);
            g.transform.localPosition = at;
            g.transform.localScale = size;
            g.AddComponent<MeshFilter>().sharedMesh = boxMesh;
            g.AddComponent<MeshRenderer>().sharedMaterial = mat;
            return g.transform;
        }

        Renderer GlowPart(Transform parent, string name, Vector3 at, Vector3 size)
        {
            Renderer r = Glow(parent, name, out Transform t);
            t.localPosition = at;
            t.localScale = size;
            t.gameObject.SetActive(true);
            return r;
        }

        // --- the arena ---------------------------------------------------------------------------------------------------------------------------

        void BuildArena(WyrmArena a, Vec3 o)
        {
            foreach (Transform ch in arenaRoot) Object.Destroy(ch.gameObject);
            Material ice = Opaque("brawl40 ice", UnityTuning.Brawl40IceHex), wall = Opaque("brawl40 wall", UnityTuning.Brawl40WallHex), pillar = Opaque("brawl40 pillar", UnityTuning.Brawl40PillarHex);
            Color cyan = Look.Hex(UnityTuning.Brawl40CyanHex, UnityTuning.Brawl40TrimGain);
            float th = UnityTuning.Brawl40TrimHeight, so = UnityTuning.Brawl40TrimStandOff;
            foreach (var (min, max, kind) in a.boxes)
            {
                Vector3 c = U((min + max) * 0.5 + o), s = new Vector3((float)(max.x - min.x), (float)(max.y - min.y), (float)(max.z - min.z));
                Transform t = Part(arenaRoot, "arena " + kind, c, s, kind == "floor" ? ice : kind == "pillar" ? pillar : wall);
                if (kind == "floor") { t.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off; continue; }
                // the cyan ride trims: the faces that look into the arena (a rim wall's inner face; all four faces of a pillar), at the foot and the top
                for (int f = 0; f < 4; f++)
                {
                    // f: 0 -x face, 1 +x face, 2 -z face, 3 +z face (web space)
                    double fx = f == 0 ? min.x - so : f == 1 ? max.x + so : (min.x + max.x) * 0.5;
                    double fz = f == 2 ? min.z - so : f == 3 ? max.z + so : (min.z + max.z) * 0.5;
                    bool inward = kind == "pillar" || (f == 0 && min.x > 0) || (f == 1 && max.x < 0) || (f == 2 && min.z > 0) || (f == 3 && max.z < 0);
                    if (!inward) continue;
                    double len = f < 2 ? max.z - min.z : max.x - min.x;
                    foreach (double y in new[] { min.y + th * 0.5 + 0.02, max.y - th * 0.5 })
                    {
                        Renderer r = GlowPart(arenaRoot, "ride trim", U(new Vec3(fx, y, fz) + o), f < 2 ? new Vector3(0.02f, th, (float)len) : new Vector3((float)len, th, 0.02f));
                        Tint(r, cyan);
                    }
                }
            }
            // the slab (the sim's `slab` box: the entry gap at +z): red-orange while locked
            Vector3 sc = U(new Vec3(0, 4, a.half + 0.5) + o);
            Renderer slab = GlowPart(arenaRoot, "slab", sc, new Vector3(4f, 8f, 1f));
            Tint(slab, Look.Hex(UnityTuning.Brawl40DangerHex, UnityTuning.Brawl40SlabGain));
            slabGo = slab.gameObject;
            builtAt = o;
            builtArena = a;
        }

        // --- the body ----------------------------------------------------------------------------------------------------------------------------

        void BuildBody()
        {
            // Unity local axes: +z = the way it faces, +y up. Sizes near the sim's body (design.n40.brawl40.body: height 3.6, radius 1.2).
            bodyRoot = new GameObject("the icebreaker").transform;
            bodyRoot.SetParent(fxRoot, false);
            legL = Pivot(bodyRoot, "leg L", new Vector3(-0.55f, 1.2f, 0));
            Part(legL, "thigh", new Vector3(0, -0.6f, 0), new Vector3(0.75f, 1.2f, 0.8f), bodyDark);
            Part(legL, "foot", new Vector3(0, -1.12f, 0.2f), new Vector3(0.85f, 0.2f, 1.1f), bodyDark);
            legR = Pivot(bodyRoot, "leg R", new Vector3(0.55f, 1.2f, 0));
            Part(legR, "thigh", new Vector3(0, -0.6f, 0), new Vector3(0.75f, 1.2f, 0.8f), bodyDark);
            Part(legR, "foot", new Vector3(0, -1.12f, 0.2f), new Vector3(0.85f, 0.2f, 1.1f), bodyDark);
            lean = Pivot(bodyRoot, "lean", new Vector3(0, 1.2f, 0));
            torso = Part(lean, "torso", new Vector3(0, 0.9f, 0.15f), new Vector3(2.2f, 1.7f, 1.5f), bodyMat);
            Part(lean, "hump", new Vector3(0, 1.75f, -0.15f), new Vector3(2.6f, 0.8f, 1.4f), bodyMat);
            rimRs.Add(GlowPart(lean, "rim L", new Vector3(-1.32f, 1.75f, 0), new Vector3(0.05f, 0.1f, 1.3f)));
            rimRs.Add(GlowPart(lean, "rim R", new Vector3(1.32f, 1.75f, 0), new Vector3(0.05f, 0.1f, 1.3f)));
            rimRs.Add(GlowPart(lean, "belt", new Vector3(0, 0.15f, 0.92f), new Vector3(1.9f, 0.08f, 0.05f)));
            head = Pivot(lean, "head", new Vector3(0, 1.85f, 0.75f));
            Part(head, "skull", new Vector3(0, 0.1f, 0.1f), new Vector3(0.6f, 0.5f, 0.6f), bodyDark);
            eyesR = GlowPart(head, "eyes", new Vector3(0, 0.15f, 0.42f), new Vector3(0.42f, 0.07f, 0.04f));
            rimRs.Add(eyesR);
            armR = Part(fxRoot, "arm R", Vector3.zero, Vector3.one, bodyMat);
            armL = Part(fxRoot, "arm L", Vector3.zero, Vector3.one, bodyMat);
            fistR = Part(fxRoot, "fist R", Vector3.zero, Vector3.one * 1.3f, bodyDark);
            fistL = Part(fxRoot, "fist L", Vector3.zero, Vector3.one * 1.3f, bodyDark);
            fistRR = GlowPart(fistR, "knuckles R", new Vector3(0, 0, 0.5f), new Vector3(1.04f, 0.5f, 0.06f));
            fistLR = GlowPart(fistL, "knuckles L", new Vector3(0, 0, 0.5f), new Vector3(1.04f, 0.5f, 0.06f));
            blockHeldR = GlowPart(fistL, "held block", new Vector3(0, 0.9f, 0), new Vector3(0.9f, 0.9f, 0.9f));
            blockHeld = blockHeldR.transform;
        }

        /// <summary>An arm: a box from the shoulder to the fist, the fist cube at the sim's fist (world, interpolated).</summary>
        void Arm(Transform arm, Transform fist, Vector3 shoulder, Vector3 fistAt, Quaternion facing)
        {
            Vector3 d = fistAt - shoulder;
            float len = Mathf.Max(0.2f, d.magnitude);
            arm.position = shoulder + d * 0.5f;
            arm.rotation = Quaternion.LookRotation(d / len, Vector3.up);
            arm.localScale = new Vector3(0.7f, 0.7f, len);
            fist.position = fistAt;
            fist.rotation = len > 0.3f ? Quaternion.LookRotation(d / len, Vector3.up) : facing;
        }

        public void Render(double alpha)
        {
            BossTell63.Draw(game, root, BossAttackClock63.Brawl); // NIGHT63 G bossTell63 (BossTell63.cs): the danger column through a non-parry wind-up (nothing with the row off)
            SurvivalSim sim = game.sim;
            float dt = Time.deltaTime;
            if (sim == null || sim.brawl40Centre == null || sim.brawl40Arena == null)
            {
                arenaRoot.gameObject.SetActive(false);
                fxRoot.gameObject.SetActive(false);
                Pose = "";
                return;
            }
            Vec3 o = sim.brawl40Centre.Value;
            if (!ReferenceEquals(builtArena, sim.brawl40Arena) || !builtAt.Equals(o)) BuildArena(sim.brawl40Arena, o);
            arenaRoot.gameObject.SetActive(true);
            slabGo.SetActive(sim.Brawl40SlabClosed);
            Brawl40 g = sim.brawl40;
            bool live = g != null && sim.n40.brawl40 && (g.Alive || g.state == Brawl40State.Dying);
            fxRoot.gameObject.SetActive(live);
            RingShown = DecalShown = BlockShown = LineShown = CyanShown = false;
            TellGlow = 0;
            if (!live) { Pose = ""; fall = 0; return; }
            float a = (float)alpha;
            Vec3 feet = g.body.previous + (g.body.position - g.body.previous) * alpha;
            Vector3 fu = U(feet);
            Vector3 fwd = U(feet + g.facing) - fu;
            Quaternion face = Quaternion.LookRotation(fwd.sqrMagnitude > 1e-6f ? fwd.normalized : Vector3.forward, Vector3.up);
            bodyRoot.position = fu;
            bodyRoot.rotation = face;
            // the walk cycle from the distance moved
            float moved = (float)JsMath.Hypot(feet.x - lastFeet.x, feet.z - lastFeet.z);
            lastFeet = feet;
            if (moved < 2) gait += moved / UnityTuning.Brawl40Stride * Mathf.PI * 2;
            float swing = g.state == Brawl40State.Idle ? Mathf.Sin(gait) * UnityTuning.Brawl40LegSwing : 0;
            legL.localRotation = Quaternion.Euler(swing, 0, 0);
            legR.localRotation = Quaternion.Euler(-swing, 0, 0);
            Brawl40State st = g.state;
            string atk = g.attack;
            float prog = (float)g.Progress;
            // the pose: the torso's lean (deg forward), the head's pitch, the knee drop (m)
            float leanDeg = 18, headDeg = 0, drop = 0;
            Color dangerC = Look.Hex(UnityTuning.Brawl40DangerHex, Mathf.Lerp(UnityTuning.Brawl40TellMin, UnityTuning.Brawl40TellMax, prog));
            Color cyanC = Look.Hex(UnityTuning.Brawl40CyanHex, UnityTuning.Brawl40CyanGain);
            Color pale = Look.Hex(UnityTuning.Brawl40PaleHex, UnityTuning.Brawl40KnuckleGain);
            Color rimC = Look.Hex(UnityTuning.Brawl40DangerHex, UnityTuning.Brawl40RimGain);
            Color fistRC = pale, fistLC = pale;
            blockHeldR.gameObject.SetActive(false);
            switch (st)
            {
                case Brawl40State.Reveal: Pose = "reveal"; leanDeg = 10; break;
                case Brawl40State.Idle: Pose = "walk"; break;
                case Brawl40State.Tell:
                    TellGlow = prog;
                    if (atk == Brawl40.Haymaker) { Pose = "haymaker wind-up"; leanDeg = 8; fistRC = dangerC; }
                    else if (atk == Brawl40.Slam) { Pose = "slam wind-up"; leanDeg = -6; fistRC = fistLC = dangerC; }
                    else if (atk == Brawl40.Block)
                    {
                        Pose = "block wind-up";
                        leanDeg = 4;
                        blockHeldR.gameObject.SetActive(true);
                        Tint(blockHeldR, Look.Hex(UnityTuning.Brawl40YellowHex, Mathf.Lerp(UnityTuning.Brawl40TellMin, UnityTuning.Brawl40BlockGain, prog)));
                        BlockShown = true;
                    }
                    else { Pose = "rush wind-up"; leanDeg = 40; headDeg = 20; Tint(eyesR, dangerC); }
                    break;
                case Brawl40State.Active:
                    if (atk == Brawl40.Rush) { Pose = "rush"; leanDeg = 45; headDeg = 20; }
                    else if (atk == Brawl40.Haymaker) { Pose = "haymaker swing"; leanDeg = 30; fistRC = Look.Hex(UnityTuning.Brawl40DangerHex, UnityTuning.Brawl40TellMax); }
                    else { Pose = atk == Brawl40.Slam ? "slam" : "throw"; leanDeg = 40; }
                    break;
                case Brawl40State.Stuck:
                    Pose = atk == Brawl40.Rush ? "dazed" : "stuck";
                    leanDeg = atk == Brawl40.Rush ? -12 : 50;
                    headDeg = atk == Brawl40.Rush ? -25 : 10;
                    if (atk == Brawl40.Rush) { foreach (Renderer r in rimRs) Tint(r, cyanC); }
                    else fistRC = cyanC;
                    CyanShown = true;
                    break;
                case Brawl40State.Recover: Pose = "recover"; leanDeg = atk == Brawl40.Slam ? 45 : 25; break;
                case Brawl40State.Window: Pose = "reeling"; leanDeg = 35; headDeg = 30; drop = 0.6f; CyanShown = true; break;
                case Brawl40State.PhaseChange: Pose = "phase"; leanDeg = -15; headDeg = -20; rimC = Look.Hex(UnityTuning.Brawl40DangerHex, UnityTuning.Brawl40TellMax); break;
                case Brawl40State.Dying: Pose = "dying"; fall = Mathf.Min(1, fall + dt / 1.0f); leanDeg = 18 + 70 * fall; drop = 1.0f * fall; break;
            }
            Render41(sim, g, st, atk, prog, ref leanDeg, ref drop, dangerC, cyanC); // NIGHT41 B8 (brawlLook41; off: NIGHT40's look)
            Render42(g, st, atk, prog, ref leanDeg, ref headDeg, ref drop, ref fistRC, ref fistLC, dangerC, cyanC, fu); // NIGHT42 B1 (brawl42's attacks; never drawn with the row off)
            Render47(g, st, ref leanDeg, ref headDeg, ref drop, ref cyanC, ref rimC, dt); // NIGHT47 B brawlLook47 (Brawl40View.N47.cs; off: NIGHT46's look)
            DrawMotion63(g, ref leanDeg, ref headDeg, ref drop, fu, face); // NIGHT63 V bossMotion63 (Brawl40View.N63V.cs): the pose on springs, the wind-up, the flinch, the slump, the breath (off: the rigid pose)
            Kill74Brawl(); // NIGHT74 B kill74 (Brawl40View.Kill74.cs): the body root for the death flash
            lean.localPosition = new Vector3(0, 1.2f - drop, 0);
            lean.localRotation = Quaternion.Euler(leanDeg, 0, 0);
            head.localRotation = Quaternion.Euler(headDeg, 0, 0);
            if (hitFlash > 0) { hitFlash -= dt; rimC = Color.white; }
            if (!(st == Brawl40State.Stuck && atk == Brawl40.Rush))
                foreach (Renderer r in rimRs) Tint(r, st == Brawl40State.Window ? cyanC : rimC);
            if (st != Brawl40State.Tell || atk != Brawl40.Rush) { if (st != Brawl40State.Window && !(st == Brawl40State.Stuck && atk == Brawl40.Rush)) Tint(eyesR, rimC); }
            // the arms: from the shoulders (on the leaning torso) to the sim's fists
            Vector3 shR = lean.TransformPoint(new Vector3(1.25f, 1.55f, 0.2f)), shL = lean.TransformPoint(new Vector3(-1.25f, 1.55f, 0.2f));
            Vector3 fR = U(g.Fist), fL = U(g.FistL);
            if (st == Brawl40State.Window || st == Brawl40State.Dying) { fR = fu + face * new Vector3(1.4f, 0.5f, 1.2f); fL = fu + face * new Vector3(-1.4f, 0.5f, 1.2f); }
            Arm(armR, fistR, shR, fR, face);
            Arm(armL, fistL, shL, fL, face);
            Tint(fistRR, fistRC);
            Tint(fistLR, fistLC);
            // the slam: the decal ring at its reach during the wind-up, the shockwave ring on the floor
            bool decal = st == Brawl40State.Tell && atk == Brawl40.Slam;
            DecalShown = decal;
            Ring(decalTs, decalRs, decal, fu, Mathf.Min((float)g.p.slamRadius, (float)g.half), 0.03f, UnityTuning.Brawl40RingWidth, 0.04f,
                Look.Hex(UnityTuning.Brawl40DangerHex, UnityTuning.Brawl40DecalGain * (0.4f + 0.6f * prog)));
            RingShown = g.ringOn;
            Ring(waveTs, waveRs, g.ringOn, U(g.ringAt), (float)g.ringR, UnityTuning.Brawl40WaveHeight * 0.5f, 0.35f, UnityTuning.Brawl40WaveHeight,
                Look.Hex(UnityTuning.Brawl40DangerHex, UnityTuning.Brawl40WaveGain));
            // phase 2's floes: a RED crack plate (brighter through its tell) and the dark open water, one per floe (built once per arena / grid)
            int nf = g.floeTellLeft.Length;
            if (builtFloes != nf || floeTellRs.Count != nf || !floeAt.Equals(g.origin))
            {
                foreach (Renderer r in floeTellRs) Object.Destroy(r.gameObject);
                foreach (GameObject w in floeWaterGos) Object.Destroy(w);
                floeTellRs.Clear();
                floeWaterGos.Clear();
                Material water = Opaque("brawl40 open water", UnityTuning.Brawl40WaterHex);
                for (int i = 0; i < nf; i++)
                {
                    g.FloeRect(i, out double x0, out double z0, out double x1, out double z1);
                    Vector3 cc = U(new Vec3((x0 + x1) * 0.5, 0, (z0 + z1) * 0.5));
                    var sz = new Vector3((float)(x1 - x0) - UnityTuning.Brawl40FloeGap, 0.02f, (float)(z1 - z0) - UnityTuning.Brawl40FloeGap);
                    Renderer tr = Glow(fxRoot, "floe crack", out Transform tt);
                    tt.position = cc + Vector3.up * UnityTuning.Brawl40FloeLift;
                    tt.localScale = sz;
                    floeTellRs.Add(tr);
                    Transform wt = Part(fxRoot, "floe water", cc + Vector3.up * UnityTuning.Brawl40FloeLift, sz, water);
                    wt.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
                    wt.gameObject.SetActive(false);
                    floeWaterGos.Add(wt.gameObject);
                }
                builtFloes = nf;
                floeAt = g.origin;
            }
            FloesTold = FloesOpen = 0;
            int tellFull = Mathf.Max(1, (int)System.Math.Round(g.p.floeTell / sim.step));
            for (int i = 0; i < nf; i++)
            {
                bool told = g.floeTellLeft[i] > 0, open = g.floeOpenLeft[i] > 0;
                floeTellRs[i].gameObject.SetActive(told);
                floeWaterGos[i].SetActive(open);
                if (told)
                {
                    FloesTold++;
                    float k = 1f - g.floeTellLeft[i] / (float)tellFull;
                    Tint(floeTellRs[i], Look.Hex(UnityTuning.Brawl40DangerHex, Mathf.Lerp(UnityTuning.Brawl40TellMin, UnityTuning.Brawl40FloeGain, Mathf.Clamp01(k))));
                }
                if (open) FloesOpen++;
            }
            // the block in flight
            bool fly = g.block.alive;
            blockFly.gameObject.SetActive(fly);
            if (fly)
            {
                BlockShown = true;
                blockFly.position = U(g.block.prev + (g.block.pos - g.block.prev) * alpha);
                blockFly.localScale = Vector3.one * (float)(g.p.blockRadius * 2);
                blockFly.rotation = Quaternion.Euler(Time.time * 200f, Time.time * 140f, 0);
                Tint(blockFlyR, g.block.back ? Color.white * UnityTuning.Brawl40BlockBackGain : Look.Hex(UnityTuning.Brawl40YellowHex, UnityTuning.Brawl40BlockGain));
            }
            // the rush's ground line along its locked path
            bool line = st == Brawl40State.Tell && atk == Brawl40.Rush;
            lineT.gameObject.SetActive(line);
            if (line)
            {
                LineShown = true;
                float len = UnityTuning.Brawl40LineLength;
                Vector3 from = U(g.rushFrom), dir = (U(g.rushFrom + g.rushDir) - from).normalized;
                lineT.position = from + dir * (len * 0.5f) + Vector3.up * 0.03f;
                lineT.rotation = Quaternion.LookRotation(dir, Vector3.up);
                lineT.localScale = new Vector3(UnityTuning.Brawl40LineWidth, 0.02f, len);
                Tint(lineR, Look.Hex(UnityTuning.Brawl40DangerHex, UnityTuning.Brawl40LineGain * (0.4f + 0.6f * prog)));
            }
            NearHide63(); // NIGHT63 V nearFade63 (Brawl40View.N63V.cs): a body part at the eye stops drawing, so a player at gauntlet range sees the boss and the room (off: every part drawn)
            TripoBrawl(); // PG3 tripo74: THE ICEBREAKER's Tripo model over the brute, the fists' and rims' glows kept (Brawl40View.Tripo.cs; nothing when its file is missing or the row is off)
        }

        /// <summary>A ring of segments on the floor (centre `c`, radius r), each `h` tall at height y.</summary>
        void Ring(List<Transform> ts, List<Renderer> rs, bool on, Vector3 c, float r, float y, float w, float h, Color col)
        {
            int n = ts.Count;
            float seg = 2 * Mathf.PI * Mathf.Max(0.5f, r) / n * 1.05f;
            for (int i = 0; i < n; i++)
            {
                ts[i].gameObject.SetActive(on);
                if (!on) continue;
                float ang = i * Mathf.PI * 2 / n;
                Vector3 p = c + new Vector3(Mathf.Cos(ang) * r, y, Mathf.Sin(ang) * r);
                ts[i].position = p;
                ts[i].rotation = Quaternion.LookRotation(new Vector3(-Mathf.Sin(ang), 0, Mathf.Cos(ang)), Vector3.up);
                ts[i].localScale = new Vector3(w, h, seg);
                Tint(rs[i], col);
            }
        }
    }
}
