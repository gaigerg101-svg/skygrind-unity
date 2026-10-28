// NIGHT20 U3-34.9 A1 block 2 (builder EN owns this file): the BULWARK as bulwark.js's render() draws it (GG's design,
// rebuilt as it is; CLAUDE.md rule 7), from the sim's Bulwark (Core/Enemies/Bulwark.cs), interpolated on frame time:
// - the hull: six boxes (the legs, the torso, the head, the arms: config.bulwark.parts) in 0x3a3f4a with the enemy's rim;
//   the legs swing with the distance it walked, the ram's wind-up drops and leans the upper body in, the stun drops it (the
//   sim poses the boxes: Bulwark.Pose, so a shot finds what you see);
// - the plates: five boxes (two front, two shoulders, the back) in 0x9aa3b0, white for 0.08 s when hit, gone once broken;
//   the back plate swings up 1.4 m while the vent is open (the stun);
// - the vent core: a sphere at config.bulwark.core, 0xff7a20 x1.2, white and pulsing while the vent is open (a hit flashes
//   it white); the eye: a strip in TELEGRAPH.danger x2.5, white while it winds an attack up (the tells' DANGER face);
// - the attacks (not telegraphs): the stomp's shock ring (an open cylinder, 0xff7a30 x2, additive) running out along the
//   floor; the sweep beam (a bar out to beam.range, 0xff3a10: dim and thin while it charges, x4 while it sweeps);
// - the bursts (particles.burst): a broken plate's bits, the slam dust at a wall stun and a stomp's landing, the white
//   burst at the core when it dies; the telegraphs (the RAM line, the stomp circle, the beam's line, the hatch circles, the
//   flashes, the threat arrows) are the sim's kit.telegraph calls drawn by Telegraph20 (V's), handed to the kit here;
// - the finisher: bossDying slows the world to bossKit.killSlowMo for bossKit.killTime s (main.js's slowMoLeft /
//   timeScale), through BossSlowMoFrame (the impact layer's frame scale asks it: the lead's one line).
// Made once (one boss), x-ray as an elite, FeelProbe.Enemy for the reel. Numbers: UnityTuning.Bulwark20.cs, config.bulwark.
using Skygrind.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace Skygrind.Game
{
    public sealed partial class SurvivalGame
    {
        double bossSlowLeft;

        /// <summary>NIGHT20 A1: main.js's boss finisher (bossDying: timeScale = killSlowMo for killTime s): the world's scale
        /// this frame, at most `share` (the impact layer's). The lead's line in ImpactBeginFrame calls it after SKYGRIND TIME.</summary>
        public double BossSlowMoFrame(double frameDt, double share)
        {
            if (bossSlowLeft <= 0) return share;
            bossSlowLeft -= frameDt;
            return System.Math.Min(share, cfg.Num("bossKit.killSlowMo"));
        }

        /// <summary>The finisher starts (bossDying), once per boss.</summary>
        public void BossDyingSeen() => bossSlowLeft = cfg.Num("bossKit.killTime");
    }

    public sealed class BulwarkView
    {
        readonly SurvivalGame game;
        readonly EnemyLook look;
        readonly MaterialPropertyBlock block = new MaterialPropertyBlock(), glowBlock = new MaterialPropertyBlock();
        readonly Transform root, ringT, beamPivot;
        readonly Transform[] partT, plateT;
        readonly Renderer[] partR, plateR;
        readonly Transform coreT, eyeT;
        readonly Renderer coreR, eyeR, ringR, beamR;
        readonly Color hull, plate, plateFlash, coreShut, white, danger, ringColor, beamColor, slamDust, plateBits;
        readonly float coreR0;
        static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        bool ringWas;
        bool telegraphWired;

        public BulwarkView(SurvivalGame game)
        {
            this.game = game;
            GameConfig cfg = game.cfg;
            look = EnemyLook.For(game);
            hull = Look.Hex(UnityTuning.BulwarkHullHex);
            plate = Look.Hex(UnityTuning.BulwarkPlateHex);
            plateFlash = Look.Hex(UnityTuning.BulwarkPlateHex, UnityTuning.BulwarkPlateFlash); // the instance colour (3, 3, 3) x the plate's
            coreShut = Look.Hex(UnityTuning.BulwarkCoreHex, UnityTuning.BulwarkCoreShut);
            white = Look.Hex(0xffffff);
            danger = Look.Hex(UnityTuning.TelegraphDanger);
            ringColor = Look.Hex(UnityTuning.BulwarkRingHex, UnityTuning.BulwarkRingGain);
            beamColor = Look.Hex(UnityTuning.BulwarkBeamHex);
            slamDust = Look.C(cfg, "colors.slamDust");
            plateBits = Look.Hex(UnityTuning.BulwarkPlateHex);
            coreR0 = (float)cfg.Num("bulwark.core.r");

            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Mesh box = cube.GetComponent<MeshFilter>().sharedMesh;
            Object.Destroy(cube);
            var ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Mesh sphere = ball.GetComponent<MeshFilter>().sharedMesh; // radius 0.5
            Object.Destroy(ball);

            root = new GameObject("BULWARK").transform;
            int nParts = cfg.Arr("bulwark.parts").Count, nPlates = cfg.Arr("bulwark.plates").Count;
            partT = new Transform[nParts];
            partR = new Renderer[nParts];
            for (int i = 0; i < nParts; i++)
            {
                partR[i] = Part(root, "hull " + i, box, out partT[i]);
                look.Attach(partR[i], i == 2, true); // the x-ray on the torso; the boss is elite (main.js's xray list)
            }
            plateT = new Transform[nPlates];
            plateR = new Renderer[nPlates];
            for (int i = 0; i < nPlates; i++)
            {
                plateR[i] = Part(root, "plate " + i, box, out plateT[i]);
                look.Attach(plateR[i], false, false);
            }
            Material glow = FxDraw.Opaque(game.mats.neon, "Bulwark glow", white);
            coreR = Part(root, "vent core", sphere, out coreT);
            coreR.sharedMaterial = glow;
            coreT.localScale = Vector3.one * coreR0 * 2;
            eyeR = Part(root, "eye", box, out eyeT);
            eyeR.sharedMaterial = glow;
            eyeT.localScale = new Vector3(UnityTuning.BulwarkEyeW, UnityTuning.BulwarkEyeH, UnityTuning.BulwarkEyeD);

            Material add = FxDraw.Additive(game.mats.neon, "Bulwark attacks");
            ringR = Part(null, "BULWARK stomp ring", OpenCylinder(UnityTuning.BulwarkRingSides), out ringT);
            ringR.sharedMaterial = add;
            beamPivot = new GameObject("BULWARK beam").transform;
            beamR = Part(beamPivot, "bar", box, out Transform bar);
            beamR.sharedMaterial = add;
            bar.localPosition = new Vector3(0.5f, 0, 0); // BoxGeometry(1, 0.3, 0.3).translate(0.5, 0, 0): from the pivot out
            root.gameObject.SetActive(false);
            ringT.gameObject.SetActive(false);
            beamPivot.gameObject.SetActive(false);

            game.events.On<BossEvent>("bossPart", e => game.ParticleBurst(e.position, plateBits, UnityTuning.BulwarkPlateBits, UnityTuning.BulwarkPlateBitSpeed, UnityTuning.BulwarkPlateBitLife, UnityTuning.BulwarkPlateBitSize));
            game.events.On<BossEvent>("bossStun", e => game.ParticleBurst(e.position, slamDust, UnityTuning.BulwarkWallDust, UnityTuning.BulwarkWallDustSpeed, UnityTuning.BulwarkDustLife, UnityTuning.BulwarkDustSize));
            game.events.On<BossEvent>("bossDown", e => game.ParticleBurst(e.position, white, UnityTuning.BulwarkDeathBits, UnityTuning.BulwarkDeathSpeed, UnityTuning.BulwarkDeathLife, UnityTuning.BulwarkDeathSize));
            game.events.On<BossEvent>("bossDying", _ => game.BossDyingSeen()); // the finisher's slow-mo
        }

        static Renderer Part(Transform parent, string name, Mesh mesh, out Transform t)
        {
            var g = new GameObject(name);
            if (parent != null) g.transform.SetParent(parent, false);
            g.AddComponent<MeshFilter>().sharedMesh = mesh;
            var r = g.AddComponent<MeshRenderer>();
            r.shadowCastingMode = ShadowCastingMode.Off;
            t = g.transform;
            return r;
        }

        /// <summary>three.js CylinderGeometry(1, 1, 1, sides, 1, true) from y 0 to 1 (both faces: a thin additive wall).</summary>
        static Mesh OpenCylinder(int sides)
        {
            var verts = new Vector3[(sides + 1) * 2];
            var tris = new int[sides * 12];
            for (int i = 0; i <= sides; i++)
            {
                float a = i / (float)sides * Mathf.PI * 2;
                verts[i * 2] = new Vector3(Mathf.Cos(a), 0, Mathf.Sin(a));
                verts[i * 2 + 1] = new Vector3(Mathf.Cos(a), 1, Mathf.Sin(a));
            }
            for (int i = 0, t = 0; i < sides; i++)
            {
                int a = i * 2, b = a + 1, c = a + 2, d = a + 3;
                tris[t++] = a; tris[t++] = b; tris[t++] = c;
                tris[t++] = c; tris[t++] = b; tris[t++] = d;
                tris[t++] = a; tris[t++] = c; tris[t++] = b;
                tris[t++] = c; tris[t++] = d; tris[t++] = b;
            }
            var m = new Mesh { name = "bulwark ring", vertices = verts, triangles = tris };
            m.RecalculateNormals();
            m.RecalculateBounds();
            return m;
        }

        /// <summary>A web-local box centre in the boss's Unity frame (the web's -z ahead is Unity's +z).</summary>
        static Vector3 Local(Vec3 v) => new Vector3((float)v.x, (float)v.y, (float)-v.z);

        void Glow(Renderer r, Color c)
        {
            glowBlock.SetColor(BaseColor, c);
            r.SetPropertyBlock(glowBlock);
        }

        public void Render(double alpha, double frameDt)
        {
            SurvivalSim sim = game.sim;
            Bulwark b = sim?.bulwark;
            // The sim's kit draws its telegraphs through V's Telegraph20 (ITelegraph) once both exist.
            if (!telegraphWired && sim?.bossKit != null && (object)game.Telegraphs is ITelegraph tg)
            {
                sim.bossKit.telegraph = tg;
                telegraphWired = true;
            }
            bool on = b != null && b.Alive;
            if (root.gameObject.activeSelf != on) root.gameObject.SetActive(on);
            if (!on)
            {
                if (ringT.gameObject.activeSelf) ringT.gameObject.SetActive(false);
                if (beamPivot.gameObject.activeSelf) beamPivot.gameObject.SetActive(false);
                ringWas = false;
                return;
            }
            look.Frame();
            BulwarkBody body = b.body;
            Vec3 p = body.previous + (body.position - body.previous) * alpha;
            Quaternion yawQ = Quaternion.Euler(0, -(float)(body.yaw * Mathf.Rad2Deg), 0);
            root.position = WebSpace.ToUnity(p);
            root.rotation = yawQ;
            body.flash = System.Math.Max(0, body.flash - frameDt);
            // The hull and the plates: the sim's posed boxes (Bulwark.Pose: render()'s legs, lean and stun).
            for (int i = 0; i < partT.Length; i++)
            {
                BulwarkPart part = b.parts[i];
                partT[i].localPosition = Local(part.pos);
                partT[i].localScale = new Vector3((float)part.size.x, (float)part.size.y, (float)part.size.z);
                look.Tint(block, partR[i], hull, 0, 1);
            }
            bool vent = b.stun > 0;
            for (int i = 0; i < plateT.Length; i++)
            {
                BulwarkPart pl = b.plates[i];
                pl.flash = System.Math.Max(0, pl.flash - frameDt);
                bool open = pl.name == "back" && vent && !pl.broken;
                Vector3 at = Local(pl.pos);
                if (open) at.y += UnityTuning.BulwarkVentLift;
                plateT[i].localPosition = at;
                plateT[i].localScale = pl.broken ? Vector3.zero : new Vector3((float)pl.size.x, (float)pl.size.y, (float)pl.size.z);
                look.Tint(block, plateR[i], pl.flash > 0 ? plateFlash : plate, 0, 0);
            }
            float lean = (float)b.lean, stun = vent ? 1 : 0;
            float drop = lean * UnityTuning.BulwarkLeanDrop + stun * UnityTuning.BulwarkStunDrop;
            float ahead = lean * UnityTuning.BulwarkLeanAhead - stun * UnityTuning.BulwarkStunBack;
            BulwarkParams bp = b.b;
            coreT.localPosition = new Vector3((float)bp.coreX, (float)bp.coreY - drop, -((float)bp.coreZ - ahead));
            bool ventOpen = b.VentOpen;
            float age = (float)b.age;
            Color core = body.flash > 0 || ventOpen ? white * (ventOpen ? UnityTuning.BulwarkCoreOpen + Mathf.Sin(age * UnityTuning.BulwarkCorePulseRate) * UnityTuning.BulwarkCorePulse : UnityTuning.BulwarkCoreShut) : coreShut;
            Glow(coreR, core);
            eyeT.localPosition = new Vector3(0, UnityTuning.BulwarkEyeY - drop, -(UnityTuning.BulwarkEyeZ - ahead));
            bool winding = b.action == "ramTell" || b.action == "stompTell" || b.BeamCharging;
            Glow(eyeR, (winding ? white : danger) * UnityTuning.BulwarkEyeGain);
            // The stomp's shock ring (its landing's slam dust once, as bulwark.js bursts it).
            Vec3 c = b.Center;
            bool ring = b.RingLive;
            if (ringT.gameObject.activeSelf != ring) ringT.gameObject.SetActive(ring);
            if (ring)
            {
                if (!ringWas) game.ParticleBurst(new Vec3(b.RingX, c.y + 0.3, b.RingZ), slamDust, UnityTuning.BulwarkStompDust, UnityTuning.BulwarkStompDustSpeed, UnityTuning.BulwarkDustLife, UnityTuning.BulwarkDustSize);
                float r = (float)b.RingRadius, h = (float)bp.stompHeight;
                ringT.position = WebSpace.ToUnity(new Vec3(b.RingX, c.y, b.RingZ));
                ringT.localScale = new Vector3(r, h, r);
                Glow(ringR, ringColor);
            }
            ringWas = ring;
            // The sweep beam: dim while it charges, blazing while it sweeps (web angle a: direction (cos a, sin a) in x / z).
            bool beam = b.BeamLive;
            if (beamPivot.gameObject.activeSelf != beam) beamPivot.gameObject.SetActive(beam);
            if (beam)
            {
                bool charging = b.BeamCharging;
                float thick = UnityTuning.BulwarkBeamThick * (charging ? UnityTuning.BulwarkBeamCharge : 1);
                beamPivot.position = WebSpace.ToUnity(new Vec3(p.x, c.y + bp.beamHeight, p.z));
                beamPivot.rotation = Quaternion.Euler(0, (float)(b.BeamAngle * Mathf.Rad2Deg), 0); // z flipped: +x turns to (cos a, -sin a)
                beamPivot.localScale = new Vector3((float)bp.beamRange, thick, thick);
                Glow(beamR, beamColor * (charging ? UnityTuning.BulwarkBeamChargeGain : UnityTuning.BulwarkBeamGain));
            }
            look.Notch(body.id, root);
            FeelProbe.Enemy(body.id, partR[2].bounds, partR[3].bounds, body.flash > 0 ? 1 : 0);
        }
    }
}
