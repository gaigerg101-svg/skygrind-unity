// NIGHT24 builder B: THE KILN WYRM as the view draws it (GG's "crazy worm boss", rebuilt as blocky boxes: CLAUDE.md rule 7), from the sim's
// Wyrm (Core/Enemies/Wyrm.cs), interpolated on the sim's render alpha:
// - 26 boxes (the head, 24 body pieces, the tail): the sim's oriented boxes as they are (a shot finds what you see), the body in the enemy
//   hull's dark steel, a pale-violet-white trim plate on each piece's back (the boundary colour, rule 8); pieces under the floor are hidden;
// - the core: a sphere on the head's back, red-orange and pulsing ONLY while it is exposed (rule 8: red-orange is danger); a dim pale-violet glow while
//   the roar tells it is about to open;
// - the spit and the shards: small additive boxes (red-orange spit, red-orange shards that fly out of the wall);
// - the marked spots: the floor crack where the worm dives and the plate on the wall it will erupt from, pulsing red-orange for the tell;
//   the sim's kit.telegraph calls (the circles and the lunge line) are drawn by Telegraph20 and handed to the kit here as the BULWARK does;
// - the finisher: bossDying slows the world (BossDyingSeen), bossDown bursts at the head. Numbers: UnityTuning.Wyrm24.cs.
// Not compiled by its author (the lead compiles Unity-side code): the patterns are BulwarkView's (EnemyViews.Bulwark.cs).
using Skygrind.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace Skygrind.Game
{
    public sealed partial class WyrmView // NIGHT63 V: partial (bossMotion63's hook in WyrmView.N63V.cs)
    {
        readonly SurvivalGame game;
        readonly Transform root, coreT;
        readonly Transform[] baseT, trimT, shotT, markT;
        readonly Renderer[] baseR, trimR, shotR, markR;
        readonly Renderer coreR;
        readonly MaterialPropertyBlock block = new MaterialPropertyBlock();
        readonly Color body, head, trim, core, spit, shard, mark, white;
        readonly float spitSize;
        static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        bool telegraphWired, wasOpen;
        /// <summary>NIGHT26 V: the Tripo worm with its glow (WyrmVisual26), or null: the blocky boxes below are then what is drawn (the row design.n26.wormVisual off, or no parts asset).</summary>
        public readonly WyrmVisual26 visual;

        /// <summary>The worm's pieces are drawn now (the reel's wyrmdies checks they are gone after the finisher).</summary>
        public bool Shown => root != null && root.gameObject.activeSelf;

        public WyrmView(SurvivalGame game)
        {
            this.game = game;
            GameConfig cfg = game.cfg;
            body = Look.Hex(UnityTuning.WyrmBodyHex);
            head = Look.Hex(UnityTuning.WyrmHeadHex);
            trim = Look.C(cfg, "barrier.boundaryColor", UnityTuning.WyrmTrimGain);
            core = Look.Hex(UnityTuning.WyrmCoreHex);
            spit = Look.Hex(UnityTuning.WyrmSpitHex, UnityTuning.WyrmShotGain);
            shard = spit; // a shard is danger: red-orange like the spit (the pale violet-white is the boundary, rule 8)
            mark = Look.Hex(UnityTuning.WyrmMarkHex, UnityTuning.WyrmMarkGain);
            white = Look.Hex(0xffffff);
            spitSize = (float)cfg.Num("design.n24.wyrm.spit.radius") * 2;
            int pieces = (int)cfg.Num("design.n24.wyrm.body.pieces") + 2;

            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Mesh box = cube.GetComponent<MeshFilter>().sharedMesh;
            Object.Destroy(cube);
            var ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Mesh sphere = ball.GetComponent<MeshFilter>().sharedMesh; // radius 0.5
            Object.Destroy(ball);

            Material solid = FxDraw.Opaque(game.mats.neon, "Wyrm body", Color.white);
            solid72 = solid; // NIGHT72 B1 body72 (WyrmView.Body72.cs): put back when the row reads false
            Material add = FxDraw.Additive(game.mats.neon, "Wyrm glow");
            add72 = add; // NIGHT72 B1 body72: the seams are drawn opaque with the row on (additive plates stack into the parry tell's yellow), additive again when it reads false
            root = new GameObject("KILN WYRM").transform;
            baseT = new Transform[pieces];
            baseR = new Renderer[pieces];
            trimT = new Transform[pieces];
            trimR = new Renderer[pieces];
            for (int i = 0; i < pieces; i++)
            {
                baseR[i] = Part(root, "piece " + i, box, solid, out baseT[i]);
                trimR[i] = Part(root, "trim " + i, box, add, out trimT[i]);
            }
            coreR = Part(root, "core", sphere, add, out coreT);
            int nShots = UnityTuning.WyrmShotPool, nMarks = UnityTuning.WyrmMarkerPool;
            shotT = new Transform[nShots];
            shotR = new Renderer[nShots];
            for (int i = 0; i < nShots; i++) shotR[i] = Part(root, "shot " + i, box, add, out shotT[i]);
            markT = new Transform[nMarks];
            markR = new Renderer[nMarks];
            for (int i = 0; i < nMarks; i++) markR[i] = Part(root, "mark " + i, box, add, out markT[i]);
            root.gameObject.SetActive(false);

            visual = WyrmVisual26.Create(game, pieces); // NIGHT26 V: the models; null keeps the boxes
            game.events.On<BossEvent>("bossDying", _ => game.BossDyingSeen()); // the finisher's slow-mo (the BULWARK view does the same: it only sets a timer)
            game.events.On<BossEvent>("bossDown", e => game.ParticleBurst(e.position, white, UnityTuning.WyrmDeathBits, UnityTuning.WyrmDeathSpeed, UnityTuning.WyrmDeathLife, UnityTuning.WyrmDeathSize));
        }

        static Renderer Part(Transform parent, string name, Mesh mesh, Material mat, out Transform t)
        {
            var g = new GameObject(name);
            g.transform.SetParent(parent, false);
            g.AddComponent<MeshFilter>().sharedMesh = mesh;
            var r = g.AddComponent<MeshRenderer>();
            r.sharedMaterial = mat;
            r.shadowCastingMode = ShadowCastingMode.Off;
            t = g.transform;
            return r;
        }

        void Color_(Renderer r, Color c)
        {
            block.SetColor(BaseColor, c);
            r.SetPropertyBlock(block);
        }

        static Vector3 U(Vec3 v) => new Vector3((float)v.x, (float)v.y, (float)-v.z);

        public void Render(double alpha, double frameDt)
        {
            BossTell63.Draw(game, root, BossAttackClock63.Wyrm); // NIGHT63 G bossTell63 (BossTell63.cs): the danger column through a non-parry wind-up (nothing with the row off)
            SurvivalSim sim = game.sim;
            Wyrm w = sim?.wyrm24;
            if (!telegraphWired && sim?.bossKit != null && (object)game.Telegraphs is ITelegraph tg)
            {
                sim.bossKit.telegraph = tg;
                telegraphWired = true;
            }
            visual?.Render(w, alpha, (float)frameDt); // NIGHT26 V: the Tripo pieces (also the finisher's burn-out after the sim's death)
            bool on = w != null && w.Alive;
            if (root.gameObject.activeSelf != on) root.gameObject.SetActive(on);
            if (!on)
            {
                wasOpen = false;
                return;
            }
            float a = Mathf.Clamp01((float)alpha);
            int n = w.PieceCount;
            // NIGHT47 B wyrmLook47 (view row, DECIDE47 D-9: the 10-09 blocky shape in NIGHT26's molten colours): ash blocks whose back plate
            // glows molten in a wave down the body, the head yellow in the punch window, and the burrow's trail: a glowing crack on the floor
            // over every buried piece (G34's "travel under the floor"). Numbers: UnityTuning.Brawl47.cs (Wyrm47*).
            bool look47 = visual == null && NightRows.ViewOn(game.cfg, "wyrmLook47");
            float tSec = (float)w.stepNo / 120f;
            float floorY = (float)(w.arenaCenter.HasValue ? w.arenaCenter.Value.y : 0);
            bool open63 = w.CoreOpen || (sim.wyrm31 != null && sim.wyrm31.Staggered); // NIGHT63 B3b: OPEN (the HUD's "OPEN"): the head lit with bossColours63
            // NIGHT73 core73 (Core73.cs, the row read every frame): the head stays lit cyan while OPEN, as NIGHT63 B3b made it (round 1 took the cyan off the head: the lead's look found the cue weakened); with the row
            // on BossMarks32 draws a soft cyan core on the head's front face instead of its 1.45 x translucent shell, and the exposed-core sphere below breathes at the core's rate
            bool core73 = Core73.On(game.cfg);
            bool body72 = Body72Begin(sim); // NIGHT72 B1 body72 (WyrmView.Body72.cs): the pieces drawn with the enemies' shader, the tell glow (nothing with the row off)
            for (int i = 0; i < n; i++)
            {
                WyrmBox b = w.boxes[i];
                bool trail = look47 && w.buried[i];
                bool hidden = w.buried[i] || visual != null; // NIGHT26 V: with the models drawn the boxes are not (they stay the hit boxes)
                if (baseT[i].gameObject.activeSelf == hidden) baseT[i].gameObject.SetActive(!hidden);
                if (trimT[i].gameObject.activeSelf != (!hidden || trail)) trimT[i].gameObject.SetActive(!hidden || trail);
                if (trail)
                {
                    Vector3 tc = U(b.c);
                    trimT[i].SetPositionAndRotation(new Vector3(tc.x, floorY + UnityTuning.Wyrm47TrailLift, tc.z), Quaternion.LookRotation(U(b.f).sqrMagnitude > 1e-6f ? new Vector3(U(b.f).x, 0, U(b.f).z).normalized + new Vector3(0, 0, 1e-4f) : Vector3.forward, Vector3.up));
                    trimT[i].localScale = new Vector3((float)b.hr * 2 * UnityTuning.Wyrm47TrailWiden, UnityTuning.Wyrm47TrailThick, (float)b.hf * 2 * UnityTuning.Wyrm47TrailWiden);
                    float trailK = UnityTuning.Wyrm47TrailGain * (0.75f + 0.25f * Mathf.Sin(tSec * UnityTuning.Wyrm47TrailPulse + i));
                    Color_(trimR[i], body72 ? BossColours63.Hot(game.cfg, "wyrm.seams", Look.Hex(UnityTuning.Wyrm47GlowHex), trailK) : Look.Hex(UnityTuning.Wyrm47GlowHex, trailK)); // NIGHT72 B1: opaque with the row on: clip-safe
                    continue;
                }
                if (hidden) continue;
                Vector3 cur = U(b.c), prev = U(w.prevCenters[i]);
                Vector3 c = Vector3.Lerp(prev, cur, a);
                Vector3 f = U(b.f), up = U(b.u);
                Quaternion q = Quaternion.LookRotation(f, up);
                baseT[i].SetPositionAndRotation(c, q);
                baseT[i].localScale = new Vector3((float)b.hr * 2, (float)b.hu * 2, (float)b.hf * 2);
                Color bodyC = i == 0 ? head : body, trimC = trim;
                float heat72 = 0f;
                if (look47)
                {
                    // the molten wave: each piece's heat rides a sine down the body (the seams glow, the ash stays dark)
                    float heat = 0.5f + 0.5f * Mathf.Sin(tSec * UnityTuning.Wyrm47WaveRate - i * UnityTuning.Wyrm47WaveStep);
                    heat72 = heat;
                    bodyC = Color.Lerp(Look.Hex(UnityTuning.Wyrm47AshHex), Look.Hex(UnityTuning.Wyrm47GlowHex), UnityTuning.Wyrm47BodyHeat * heat);
                    trimC = body72 // NIGHT72 B1 body72: clip-safe (Hot: the gain first, then the danger route), because a HDR seam x 2.4 over a lifted body clipped R and G to the parry tell's yellow (t1: hue 46-49 deg)
                        ? BossColours63.Hot(game.cfg, "wyrm.seams", Look.Hex(UnityTuning.Wyrm47AmberHex), UnityTuning.Wyrm47SeamGain * (0.6f + 0.4f * heat))
                        : BossColours63.Part(game.cfg, "wyrm.seams", Look.Hex(UnityTuning.Wyrm47AmberHex, UnityTuning.Wyrm47SeamGain * (0.6f + 0.4f * heat))); // NIGHT63 B3b: fire on an enemy: danger
                    if (i == 0) bodyC = sim.Wyrm30Punchable ? BossColours63.PartRim(game.cfg, "wyrm.punch", Look.Hex(UnityTuning.Wyrm47PunchHex, UnityTuning.Wyrm47PunchGain)) :Color.Lerp(Look.Hex(UnityTuning.Wyrm47AshHex), Look.Hex(UnityTuning.Wyrm47HotHex), UnityTuning.Wyrm47JawHeat * heat); // NIGHT63 B3c: through the fight's grade rim
                    // NIGHT63 B3b: the head while OPEN lit as the punch window's, in the weak role (the row off: today's dark head)
                    if (i == 0 && open63 && !sim.Wyrm30Punchable) bodyC = BossColours63.LitRim(game.cfg, "wyrm.open", bodyC, Look.Hex(UnityTuning.Wyrm47PunchHex, UnityTuning.Wyrm47PunchGain)); // NIGHT63 B3c: through the fight's grade rim (it read lime)
                }
                if (body72)
                {
                    // NIGHT72 B1 body72: SKYGRIND/Enemy (rim, form, ink) in the lifted ash, the tell's danger over it; the punch window's and the open head's colours are their own
                    bool special72 = i == 0 && (sim.Wyrm30Punchable || open63); // the punch window's yellow and the open head's weak role stay as NIGHT47 / NIGHT63 draw them
                    Tint72(baseR[i], Body72Colour(i, n, heat72, bodyC, special72));
                    if (!special72) trimC = Body72Trim(trimC);
                }
                else Color_(baseR[i], bodyC);
                trimT[i].SetPositionAndRotation(c + up * ((float)b.hu + UnityTuning.WyrmTrimThick * 0.5f), q);
                trimT[i].localScale = new Vector3((float)b.hr * 2 * UnityTuning.WyrmTrimWidth, UnityTuning.WyrmTrimThick, (float)b.hf * 2 * UnityTuning.WyrmTrimLength);
                Color_(trimR[i], trimC);
            }
            // the core: only while exposed (rule 8); dim and pulsing while the roar tells it
            bool open = w.CoreOpen, telling = w.CoreTelling;
            WyrmBox h = w.boxes[0];
            if (coreT.gameObject.activeSelf != (open || telling)) coreT.gameObject.SetActive(open || telling);
            if (open || telling)
            {
                WyrmParams p = w.p;
                Vec3 cc = h.c + h.u * p.coreUp - h.f * p.coreBack;
                coreT.position = U(cc);
                coreT.localScale = Vector3.one * (float)p.coreR * 2;
                float pulse = Mathf.Sin((float)w.stepNo / 120f * (core73 ? UnityTuning.Core73PulseHz * 2f * Mathf.PI : UnityTuning.WyrmCorePulseRate)); // NIGHT73 core73: the exposed core sphere breathes at the glow's 1.2 Hz (today 14 rad/s = 2.2 Hz)
                Color_(coreR, open ? BossColours63.PartRim(game.cfg, "wyrm.core", core * (UnityTuning.WyrmCoreOpen + pulse * UnityTuning.WyrmCorePulse)) : trim * (UnityTuning.WyrmCoreTell * (0.5f + 0.5f * pulse))); // the roar: the boundary colour; red-orange only once it is exposed (rule 8); NIGHT63 B3b: the exposed core weak with bossColours63 (B3c: through the fight's grade rim)
            }
            if (open && !wasOpen) game.ParticleBurst(h.c + h.u * w.p.coreUp, core, UnityTuning.WyrmCoreBits, UnityTuning.WyrmCoreSpeed, UnityTuning.WyrmCoreLife, UnityTuning.WyrmCoreSize);
            wasOpen = open;
            DrawMotion63(sim, w); // NIGHT63 V bossMotion63 (WyrmView.N63V.cs): the head's wind-up, flinch and slump, the body's ripple, sag and breath; the sim's boxes untouched (off: the rigid pose)
            // the spit and the shards
            int count = Mathf.Min(w.shots.Count, shotT.Length);
            for (int i = 0; i < shotT.Length; i++)
            {
                bool live = i < count;
                if (shotT[i].gameObject.activeSelf != live) shotT[i].gameObject.SetActive(live);
                if (!live) continue;
                WyrmShot s = w.shots[i];
                Vector3 pos = U(s.pos);
                shotT[i].position = pos;
                if (s.shard)
                {
                    Vector3 v = U(s.vel);
                    shotT[i].rotation = v.sqrMagnitude > 1e-6f ? Quaternion.LookRotation(v.normalized) : Quaternion.identity;
                    shotT[i].localScale = new Vector3(UnityTuning.WyrmShardWidth, UnityTuning.WyrmShardWidth, UnityTuning.WyrmShardLength);
                    Color_(shotR[i], BossColours63.Part(game.cfg, "wyrm.spit", shard)); // NIGHT63 B3b: an attack: danger
                }
                else
                {
                    shotT[i].rotation = Quaternion.identity;
                    shotT[i].localScale = Vector3.one * spitSize;
                    Color_(shotR[i], BossColours63.Part(game.cfg, "wyrm.spit", spit)); // NIGHT63 B3b: an attack: danger
                }
            }
            // the marked spots: the floor crack, the wall plate (the wall it will erupt from faces the room)
            for (int i = 0; i < markT.Length; i++)
            {
                bool live = i < w.markers.Count && w.markers[i].kind != "lunge";
                if (markT[i].gameObject.activeSelf != live) markT[i].gameObject.SetActive(live);
                if (!live) continue;
                WyrmMarker m = w.markers[i];
                float k = 1 - Mathf.Clamp01((float)m.left / Mathf.Max(1, m.total + 1)); // 0 at the tell's start .. 1 at the event
                float pulse = 0.5f + 0.5f * Mathf.Sin((float)w.stepNo / 120f * UnityTuning.WyrmMarkPulseRate);
                if (m.kind == "diveCrack")
                {
                    markT[i].position = U(new Vec3(m.pos.x, w.Center.y + UnityTuning.WyrmCrackLift, m.pos.z));
                    markT[i].rotation = Quaternion.identity;
                    markT[i].localScale = new Vector3(UnityTuning.WyrmCrackSize, UnityTuning.WyrmMarkThick, UnityTuning.WyrmCrackSize);
                }
                else
                {
                    float inward = m.pos.x < w.Center.x ? 1f : -1f;
                    markT[i].position = U(new Vec3(m.pos.x + inward * UnityTuning.WyrmWallStand, m.pos.y, m.pos.z));
                    markT[i].rotation = Quaternion.identity;
                    markT[i].localScale = new Vector3(UnityTuning.WyrmMarkThick, UnityTuning.WyrmWallPlate, UnityTuning.WyrmWallPlate);
                }
                Color_(markR[i], mark * (UnityTuning.WyrmMarkMin + UnityTuning.WyrmMarkRamp * k) * (UnityTuning.WyrmMarkPulseMin + UnityTuning.WyrmMarkPulseDepth * pulse));
            }
        }
    }
}
