// NIGHT24 leftover 8 (builder B): MONOLITH as boss.js draws it, rebuilt as it is in blocky boxes (GG's design; CLAUDE.md rule 7), from the sim's Monolith
// (Core/Enemies/Monolith.cs): the five stacked stone boxes (config boss.body, each with its own yaw) and two arms in the stone colour, kneeling by
// kneelDrop (the upper boxes dip more); the glowing core sphere (shut, then white-hot and pulsing while it is vulnerable); the shock rings (open
// additive cylinders running out along the floor); the laser (a bar from the boss, dim while it charges); the orbs; the surge pillars. The telegraphs
// (the slam circle, the surge circles, the sweep line) are the sim's kit.telegraph calls drawn by Telegraph20. The finisher: bossDying slows the
// world (BossDyingSeen), bossDown bursts at the core. Numbers: UnityTuning.Monolith24.cs. Not compiled by its author before dn-check.
using Skygrind.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace Skygrind.Game
{
    public sealed class MonolithView
    {
        readonly SurvivalGame game;
        readonly Transform root, coreT, laserPivot;
        readonly Transform[] partT, ringT, orbT, surgeT;
        readonly Renderer[] partR, ringR, orbR, surgeR;
        readonly Renderer coreR, laserR;
        readonly double[][] parts;
        readonly MaterialPropertyBlock block = new MaterialPropertyBlock();
        readonly Color stone, core, ring, laser, orb, white;
        static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        bool telegraphWired;

        public MonolithView(SurvivalGame game)
        {
            this.game = game;
            GameConfig cfg = game.cfg;
            stone = Look.Hex(UnityTuning.MonolithStoneHex);
            core = Look.Hex(UnityTuning.MonolithCoreHex);
            ring = Look.Hex(UnityTuning.MonolithRingHex, UnityTuning.MonolithRingGain);
            laser = Look.Hex(UnityTuning.MonolithLaserHex);
            orb = Look.Hex(UnityTuning.MonolithOrbHex, UnityTuning.MonolithOrbGain);
            white = Look.Hex(0xffffff);

            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Mesh box = cube.GetComponent<MeshFilter>().sharedMesh;
            Object.Destroy(cube);
            var ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Mesh sphere = ball.GetComponent<MeshFilter>().sharedMesh;
            Object.Destroy(ball);
            Material solid = FxDraw.Opaque(game.mats.neon, "Monolith stone", Color.white);
            Material add = FxDraw.Additive(game.mats.neon, "Monolith glow");

            var list = new System.Collections.Generic.List<double[]>();
            foreach (object o in cfg.Arr("boss.body"))
            {
                var a = PortData.Arr(o);
                var row = new double[7];
                for (int i = 0; i < 7; i++) row[i] = PortData.Num(a[i]);
                list.Add(row);
            }
            for (int side = -1; side <= 1; side += 2) list.Add(new double[] { UnityTuning.MonolithArmW, UnityTuning.MonolithArmH, UnityTuning.MonolithArmW, side * UnityTuning.MonolithArmX, UnityTuning.MonolithArmY, 0, 0 });
            parts = list.ToArray();
            root = new GameObject("MONOLITH").transform;
            partT = new Transform[parts.Length];
            partR = new Renderer[parts.Length];
            for (int i = 0; i < parts.Length; i++) partR[i] = Part(root, "stone " + i, box, solid, out partT[i]);
            coreR = Part(root, "core", sphere, add, out coreT);
            ringT = new Transform[UnityTuning.MonolithRingPool];
            ringR = new Renderer[ringT.Length];
            Mesh open = OpenCylinder(UnityTuning.MonolithRingSides);
            for (int i = 0; i < ringT.Length; i++) ringR[i] = Part(null, "ring " + i, open, add, out ringT[i]);
            laserPivot = new GameObject("MONOLITH laser").transform;
            laserR = Part(laserPivot, "bar", box, add, out Transform bar);
            bar.localPosition = new Vector3(0.5f, 0, 0);
            orbT = new Transform[UnityTuning.MonolithOrbPool];
            orbR = new Renderer[orbT.Length];
            for (int i = 0; i < orbT.Length; i++) orbR[i] = Part(null, "orb " + i, box, add, out orbT[i]);
            surgeT = new Transform[UnityTuning.MonolithSurgePool];
            surgeR = new Renderer[surgeT.Length];
            for (int i = 0; i < surgeT.Length; i++) surgeR[i] = Part(null, "surge " + i, box, solid, out surgeT[i]);
            root.gameObject.SetActive(false);
            laserPivot.gameObject.SetActive(false);
            foreach (Transform t in ringT) t.gameObject.SetActive(false);
            foreach (Transform t in orbT) t.gameObject.SetActive(false);
            foreach (Transform t in surgeT) t.gameObject.SetActive(false);

            game.events.On<BossEvent>("bossDying", _ => game.BossDyingSeen());
            game.events.On<BossEvent>("bossDown", e => game.ParticleBurst(e.position, white, UnityTuning.MonolithDeathBits, UnityTuning.MonolithDeathSpeed, UnityTuning.MonolithDeathLife, UnityTuning.MonolithDeathSize));
        }

        static Renderer Part(Transform parent, string name, Mesh mesh, Material mat, out Transform t)
        {
            var g = new GameObject(name);
            if (parent != null) g.transform.SetParent(parent, false);
            g.AddComponent<MeshFilter>().sharedMesh = mesh;
            var r = g.AddComponent<MeshRenderer>();
            r.sharedMaterial = mat;
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
            var m = new Mesh { name = "monolith ring", vertices = verts, triangles = tris };
            m.RecalculateNormals();
            m.RecalculateBounds();
            return m;
        }

        void Glow(Renderer r, Color c)
        {
            block.SetColor(BaseColor, c);
            r.SetPropertyBlock(block);
        }

        static Vector3 U(Vec3 v) => new Vector3((float)v.x, (float)v.y, (float)-v.z);

        public void Render(double alpha, double frameDt)
        {
            SurvivalSim sim = game.sim;
            Monolith m = sim?.monolith24;
            if (!telegraphWired && sim?.bossKit != null && (object)game.Telegraphs is ITelegraph tg)
            {
                sim.bossKit.telegraph = tg;
                telegraphWired = true;
            }
            bool on = m != null && m.Alive;
            if (root.gameObject.activeSelf != on) root.gameObject.SetActive(on);
            if (!on)
            {
                laserPivot.gameObject.SetActive(false);
                foreach (Transform t in ringT) t.gameObject.SetActive(false);
                foreach (Transform t in orbT) t.gameObject.SetActive(false);
                foreach (Transform t in surgeT) t.gameObject.SetActive(false);
                return;
            }
            MonolithParams b = m.b;
            Vec3 p = m.body.previous + (m.body.position - m.body.previous) * alpha;
            root.position = U(p);
            root.rotation = Quaternion.Euler(0, -(float)(m.body.yaw * Mathf.Rad2Deg), 0);
            float dip = m.Kneeling ? (float)b.kneelDrop : 0;
            bool dying = m.phase == "dying";
            float k = dying ? Mathf.Clamp01(1f - m.killSteps / Mathf.Max(1f, (float)(b.killTime / game.cfg.Dt))) : 0;
            int nBody = parts.Length - 2;
            for (int i = 0; i < parts.Length; i++)
            {
                double[] q = parts[i];
                float y = (float)q[4] - (i < nBody ? dip * (i / (float)nBody) : dip * 0.5f);
                float x = (float)q[3], z = (float)q[5];
                if (dying)
                {
                    x *= 1 + k * 1.5f;
                    z *= 1 + k * 1.5f;
                }
                partT[i].localPosition = new Vector3(x, y, -z);
                partT[i].localRotation = Quaternion.Euler(0, -(float)(q[6] * Mathf.Rad2Deg), dying ? k * 0.8f * Mathf.Rad2Deg * (i % 2 == 0 ? -1 : 1) : 0);
                partT[i].localScale = new Vector3((float)q[0], (float)q[1], (float)q[2]);
                Glow(partR[i], stone);
            }
            // the core: the web's sphere, white-hot and pulsing while it can be hurt
            Vec3 cw = m.CoreWorld();
            coreT.position = U(cw);
            coreT.rotation = Quaternion.identity;
            coreT.localScale = Vector3.one * (float)b.coreRadius * 2;
            float pulse = Mathf.Sin((float)m.age * UnityTuning.MonolithCorePulseRate);
            Glow(coreR, m.Vulnerable ? white * (UnityTuning.MonolithCoreOpen + pulse * UnityTuning.MonolithCorePulse) : core * UnityTuning.MonolithCoreShut);
            // the shock rings along the floor
            for (int i = 0; i < ringT.Length; i++)
            {
                bool live = i < m.RingCount && m.RingLive(i);
                if (ringT[i].gameObject.activeSelf != live) ringT[i].gameObject.SetActive(live);
                if (!live) continue;
                float r = (float)m.RingRadius(i);
                ringT[i].position = U(new Vec3(p.x, m.RingY(i), p.z));
                ringT[i].localScale = new Vector3(r, (float)b.ringHeight, r);
                Glow(ringR[i], ring);
            }
            // the laser
            bool beam = m.LaserLive;
            if (laserPivot.gameObject.activeSelf != beam) laserPivot.gameObject.SetActive(beam);
            if (beam)
            {
                bool charging = m.LaserCharging;
                float thick = UnityTuning.MonolithLaserThick * (charging ? UnityTuning.MonolithLaserCharge : 1);
                laserPivot.position = U(new Vec3(p.x, m.Center.y + b.laserHeight, p.z));
                laserPivot.rotation = Quaternion.Euler(0, (float)(m.LaserAngle * Mathf.Rad2Deg), 0);
                laserPivot.localScale = new Vector3((float)b.laserRange, thick, thick);
                Glow(laserR, laser * (charging ? UnityTuning.MonolithLaserChargeGain : UnityTuning.MonolithLaserGain));
            }
            // the surge pillars (the eruption: up and back down over surgeErupt)
            for (int i = 0; i < surgeT.Length; i++)
            {
                double up = 0, sx = 0, sy = 0, sz = 0;
                if (i < m.SurgeCount) up = m.Surge(i, out sx, out sy, out sz);
                bool live = up > 0;
                if (surgeT[i].gameObject.activeSelf != live) surgeT[i].gameObject.SetActive(live);
                if (!live) continue;
                float h = (float)(b.surgeHeight * up);
                surgeT[i].position = U(new Vec3(sx, sy + h * 0.5, sz));
                surgeT[i].localScale = new Vector3(UnityTuning.MonolithSurgeWidth, h, UnityTuning.MonolithSurgeWidth);
                Glow(surgeR[i], stone);
            }
            // the orbs
            int nOrb = Mathf.Min(m.orbs.Count, orbT.Length);
            for (int i = 0; i < orbT.Length; i++)
            {
                bool live = i < nOrb;
                if (orbT[i].gameObject.activeSelf != live) orbT[i].gameObject.SetActive(live);
                if (!live) continue;
                orbT[i].position = U(m.orbs[i].pos);
                orbT[i].localScale = Vector3.one * (float)b.orbRadius * 2;
                Glow(orbR[i], orb);
            }
        }
    }
}
