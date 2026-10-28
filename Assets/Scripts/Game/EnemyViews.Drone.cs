// NIGHT19 U3-33.7 (builder E owns this file): the DRONE and its orbs as drone.js draws them (GG's design, rebuilt as it
// is; CLAUDE.md rule 7), a new partial of SurvivalGame through the shared commit's hooks (N19DroneViewInit /
// N19DroneViewRender):
// - the shell: an octahedron of drone.radius stretched x drone.shellStretch in y, spinning about y at drone.spinRate rad/s
//   (spinChargeRate while charging), in colors.enemy x drone.bodyShade, tinted toward husk.windupColor by drone.chargeTint
//   while it charges (the tell: DANGER's warm end), white on a hit (EnemyLook); the rim (rimGlow.js) on the shell only;
// - the core: an octahedron of radius x drone.coreScale, swelling 1 + coreSwell x wave (coreChargeSwell while charging),
//   the wave 0.5 + 0.5 sin(pulse) with the pulse at corePulseRate (corePulseChargeRate charging), coloured husk.windupColor
//   toward white by coreSwell x wave (by the wave itself while charging);
// - the orbs: orb.radius spheres in orb.hostileColor (orb.reflectedColor once parried), and the parry cue around every
//   hostile orb (settings.shapeCues): a torus of radius orb.radius x ring.scale, tube ring.tube, tumbling ring.spin rad a
//   step about x and 0.62 of it about y. Pooled: orb.poolSize spheres and rings made once.
// Interpolated on frame time; FeelProbe.Enemy reports each drone as the husk view does.
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace Skygrind.Game
{
    public sealed partial class SurvivalGame
    {
        DroneView droneView;

        partial void N19DroneViewInit() => droneView = new DroneView(this);

        partial void N19DroneViewRender(double alpha, double frameDt) => droneView?.Render(alpha, frameDt);
    }

    public sealed class DroneView
    {
        sealed class Body
        {
            public Transform root, shell, core;
            public readonly Renderer[] rends = new Renderer[2];
            public readonly Color[] colors = new Color[2];
            public Enemy40Parts m40; // NIGHT40 E3 models40: the silhouette parts (null: the row off)
            public Look43 l43; // NIGHT43 A1 models43: the rebuilt model (Enemy43View.cs)
            public Enemy drawn;
            public bool band47Read; // NIGHT47 F foes47: the rim ring's own scale read (the first models43 frame)
            public Vector3 band47Base;
        }

        /// <summary>NIGHT47 F foes47: the rim ring (models43's first drone part, TRIM) as the lenses' band frame: Foes47DroneBandY x taller with
        /// the row on, its own scale with it off (set each frame from the scale read once: never compounded).</summary>
        static void Band47(Body b, Transform ring, bool on)
        {
            if (ring == null) return;
            if (!b.band47Read) { b.band47Read = true; b.band47Base = ring.localScale; }
            ring.localScale = on ? new Vector3(b.band47Base.x, b.band47Base.y * UnityTuning.Foes47DroneBandY, b.band47Base.z) : b.band47Base;
        }

        static readonly float[] Rims = { 1, 0 }; // drone.js: addRimGlow on the shells only
        readonly SurvivalGame game;
        readonly EnemyLook look;
        readonly bool models40; // NIGHT40 E3: design.n40.models40.enabled when the view was made
        readonly Color danger40, parry40; // NIGHT40 E3: the parts' red-orange and yellow
        readonly List<Body> pool = new List<Body>();
        int drawnCount;
        readonly Mesh shellMesh, coreMesh;
        readonly Mesh[] meshes;
        readonly Mesh[] meshes43; // NIGHT43 A1 models43: the saucer and the core ball (metres)
        readonly float radius43;
        readonly MaterialPropertyBlock block = new MaterialPropertyBlock();
        readonly Transform parent;
        readonly Color baseColor, tell;
        readonly float shellStretch, coreSwell, coreChargeSwell, chargeTint, spinRate, spinChargeRate, pulseRate, pulseChargeRate, ringSpin;
        readonly bool shapeCues;
        readonly Transform[] orbs, rings;
        readonly Renderer[] orbRends;
        readonly bool[] orbReflected;
        readonly Material hostileMat, reflectedMat;
        // NIGHT40 E3 foes40: the campaign drone's orb and its glow are YELLOW (parryable: Tell36's yellow; CLAUDE.md rule 8: cyan is the player's,
        // and the web's hostile orb was cyan). Swapped when the run's bit changes; off: the web's colours.
        readonly Color hostileWeb, yellow40;
        bool yellowOn40;

        public DroneView(SurvivalGame game)
        {
            this.game = game;
            GameConfig cfg = game.cfg;
            look = EnemyLook.For(game);
            models40 = Enemy40Parts.On(game.cfg);
            danger40 = Look.Hex(UnityTuning.Tell36RedHex);
            parry40 = Look.Hex(UnityTuning.Tell36YellowHex);
            parent = new GameObject("Drones").transform;
            float r = (float)cfg.Num("drone.radius");
            shellMesh = Octahedron(r);
            coreMesh = Octahedron(r * (float)cfg.Num("drone.coreScale"));
            meshes = new[] { shellMesh, coreMesh };
            meshes43 = new[] { Mesh43.Scaled(Mesh43.Saucer, r), Mesh43.Scaled(Mesh43.Ball, r * UnityTuning.Models43DroneCore) }; // NIGHT43 A1 models43: made once
            radius43 = r;
            baseColor = Look.C(cfg, "colors.enemy", (float)cfg.Num("drone.bodyShade"));
            tell = Look.C(cfg, "husk.windupColor");
            shellStretch = (float)cfg.Num("drone.shellStretch");
            coreSwell = (float)cfg.Num("drone.coreSwell");
            coreChargeSwell = (float)cfg.Num("drone.coreChargeSwell");
            chargeTint = (float)cfg.Num("drone.chargeTint");
            spinRate = (float)cfg.Num("drone.spinRate");
            spinChargeRate = (float)cfg.Num("drone.spinChargeRate");
            pulseRate = (float)cfg.Num("drone.corePulseRate");
            pulseChargeRate = (float)cfg.Num("drone.corePulseChargeRate");
            ringSpin = (float)cfg.Num("orb.ring.spin");
            shapeCues = cfg.Bool("settings.shapeCues");
            game.events.On<KillEvent>("kill", OnKill);

            // The orbs and their parry rings (pooled once).
            DroneOrbs o = game.husks.orbs;
            int n = o.pool.Length;
            orbs = new Transform[n];
            rings = new Transform[n];
            orbRends = new Renderer[n];
            orbReflected = new bool[n];
            float orbR = (float)cfg.Num("orb.radius");
            hostileMat = new Material(game.mats.neon) { name = "Orb hostile" };
            hostileWeb = Look.C(cfg, "orb.hostileColor");
            yellow40 = Look.Hex(UnityTuning.Tell36YellowHex);
            hostileMat.SetColor(Shader.PropertyToID("_BaseColor"), hostileWeb);
            reflectedMat = new Material(game.mats.neon) { name = "Orb reflected" };
            reflectedMat.SetColor(Shader.PropertyToID("_BaseColor"), Look.C(cfg, "orb.reflectedColor"));
            Mesh torus = Torus(orbR * (float)cfg.Num("orb.ring.scale"), (float)cfg.Num("orb.ring.tube"), 24, 6);
            for (int i = 0; i < n; i++)
            {
                var g = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                Object.Destroy(g.GetComponent<Collider>());
                g.transform.SetParent(parent, false);
                g.transform.localScale = Vector3.one * orbR * 2;
                Renderer rr = g.GetComponent<Renderer>();
                rr.sharedMaterial = hostileMat;
                rr.shadowCastingMode = ShadowCastingMode.Off;
                g.SetActive(false);
                orbs[i] = g.transform;
                orbRends[i] = rr;
                var ring = new GameObject("orb ring");
                ring.transform.SetParent(parent, false);
                ring.AddComponent<MeshFilter>().sharedMesh = torus;
                var ringR = ring.AddComponent<MeshRenderer>();
                ringR.sharedMaterial = hostileMat;
                ringR.shadowCastingMode = ShadowCastingMode.Off;
                ring.SetActive(false);
                rings[i] = ring.transform;
            }
        }

        /// <summary>three.js OctahedronGeometry(radius, 0): six points, eight flat faces.</summary>
        static Mesh Octahedron(float r)
        {
            Vector3[] p = { new Vector3(r, 0, 0), new Vector3(-r, 0, 0), new Vector3(0, r, 0), new Vector3(0, -r, 0), new Vector3(0, 0, r), new Vector3(0, 0, -r) };
            int[] f = { 0, 2, 4, 0, 4, 3, 0, 3, 5, 0, 5, 2, 1, 2, 5, 1, 5, 3, 1, 3, 4, 1, 4, 2 };
            var verts = new Vector3[f.Length];
            var tris = new int[f.Length];
            for (int i = 0; i < f.Length; i += 3)
            {
                // wound so the face looks outward in Unity's left-handed space
                verts[i] = p[f[i]];
                verts[i + 1] = p[f[i + 2]];
                verts[i + 2] = p[f[i + 1]];
                tris[i] = i;
                tris[i + 1] = i + 1;
                tris[i + 2] = i + 2;
            }
            var m = new Mesh { name = "octahedron", vertices = verts, triangles = tris };
            m.RecalculateNormals();
            m.RecalculateBounds();
            return m;
        }

        /// <summary>three.js TorusGeometry(radius, tube, radialSegments, tubularSegments) in the xy plane.</summary>
        static Mesh Torus(float radius, float tube, int around, int sides)
        {
            var verts = new List<Vector3>();
            var tris = new List<int>();
            for (int j = 0; j <= sides; j++)
            {
                for (int i = 0; i <= around; i++)
                {
                    float u = i / (float)around * Mathf.PI * 2, v = j / (float)sides * Mathf.PI * 2;
                    verts.Add(new Vector3((radius + tube * Mathf.Cos(v)) * Mathf.Cos(u), (radius + tube * Mathf.Cos(v)) * Mathf.Sin(u), tube * Mathf.Sin(v)));
                }
            }
            for (int j = 1; j <= sides; j++)
            {
                for (int i = 1; i <= around; i++)
                {
                    int a = (around + 1) * j + i - 1, b = (around + 1) * (j - 1) + i - 1, c = (around + 1) * (j - 1) + i, d = (around + 1) * j + i;
                    tris.AddRange(new[] { a, d, b, b, d, c, a, b, d, b, c, d }); // both faces (thin tube)
                }
            }
            var m = new Mesh { name = "orb ring" };
            m.SetVertices(verts);
            m.SetTriangles(tris, 0);
            m.RecalculateNormals();
            m.RecalculateBounds();
            return m;
        }

        void OnKill(KillEvent e)
        {
            for (int i = 0; i < drawnCount; i++)
            {
                Body b = pool[i];
                if (b.drawn == null || b.drawn.id != e.enemy) continue;
                look.Die(b.drawn, e, b.rends, meshes, b.colors, Rims, 2, -1, b.l43 != null ? b.l43.DyingParts(b.m40) : b.m40); // NIGHT41 E6 models41: + the parts break (NIGHT43 A1: models43's while it draws)
                return;
            }
        }

        Body Make()
        {
            var b = new Body();
            b.root = new GameObject("drone").transform;
            b.root.SetParent(parent, false);
            var shell = new GameObject("shell");
            shell.transform.SetParent(b.root, false);
            shell.AddComponent<MeshFilter>().sharedMesh = shellMesh;
            b.rends[0] = shell.AddComponent<MeshRenderer>();
            var core = new GameObject("core");
            core.transform.SetParent(b.root, false);
            core.AddComponent<MeshFilter>().sharedMesh = coreMesh;
            b.rends[1] = core.AddComponent<MeshRenderer>();
            b.shell = shell.transform;
            b.core = core.transform;
            look.Attach(b.rends[0], true, false); // drone.js xrayMesh: the shells
            look.Attach(b.rends[1], false, false);
            if (models40) b.m40 = Enemy40Parts.Make("drone", b.root, look, 1f); // NIGHT40 E3 models40
            b.l43 = new Look43("drone", b.root, b.rends, null, meshes43, look, 1f); // NIGHT43 A1 models43 (y in m)
            return b;
        }

        public void Render(double alpha, double frameDt)
        {
            look.Frame();
            int n = 0;
            float fdt = (float)frameDt;
            bool y40 = game.sim != null && game.sim.n40.foes40; // NIGHT40 E3 foes40: yellow orbs and glow
            if (y40 != yellowOn40)
            {
                yellowOn40 = y40;
                hostileMat.SetColor(Shader.PropertyToID("_BaseColor"), y40 ? yellow40 : hostileWeb);
            }
            Color tell = y40 ? yellow40 : this.tell;
            foreach (Enemy e in game.enemies.all)
            {
                if (!(e is Drone d) || !d.alive) continue;
                if (n >= pool.Count) pool.Add(Make());
                Body b = pool[n++];
                b.drawn = d;
                b.root.gameObject.SetActive(true);
                d.flash = System.Math.Max(0, d.flash - frameDt);
                bool charging = d.chargeSteps > 0;
                d.spin += fdt * (charging ? spinChargeRate : spinRate);
                d.pulse += fdt * (charging ? pulseChargeRate : pulseRate);
                float wave = 0.5f + 0.5f * Mathf.Sin((float)d.pulse);
                Vec3 p = d.previous + (d.position - d.previous) * alpha;
                Quaternion spinQ = Quaternion.Euler(0, -(float)(d.spin * Mathf.Rad2Deg), 0);
                float flash = look.React(d.id, spinQ, d.flash > 0, out Vector3 off, out Vector3 squash, out Quaternion tilt); // NIGHT17
                b.root.position = WebSpace.ToUnity(p) + off;
                b.root.rotation = tilt * spinQ;
                b.root.localScale = squash;
                b.shell.localScale = new Vector3(1, shellStretch, 1);
                float core = 1 + (charging ? coreChargeSwell : coreSwell) * wave;
                b.core.localScale = new Vector3(core, core, core);
                Color sc = baseColor;
                if (charging) sc = Color.Lerp(sc, tell, chargeTint);
                Color cc = Color.Lerp(tell, Color.white, charging ? wave : coreSwell * wave);
                b.l43.Apply(Models43.On(game.cfg), b.m40); // NIGHT43 A1 models43: the saucer darkened, the core hung under it (its charge-up seen)
                b.core.localPosition = b.l43.on ? new Vector3(0, UnityTuning.Models43DroneCoreY * radius43, 0) : Vector3.zero;
                if (b.l43.on) sc = Models43.Body(sc);
                b.colors[0] = sc;
                b.colors[1] = cc;
                look.Tint(block, b.rends[0], sc, flash, Rims[0]);
                look.Tint(block, b.rends[1], cc, flash, Rims[1]);
                b.m40?.Pose(look, block, sc, flash, charging ? 1 : 0, false, danger40, parry40, (float)d.pulse); // NIGHT40 E3 models40
                bool f47 = Foes47.On(game.cfg);
                // NIGHT47 F foes47 (G14; F-cards 5c "the drone: keep the saucer + ring of lenses and add the faction's triangle (a downward spike,
                // today's emitter, made the yellow-free red-orange danger point when it fires) and the band frame round its lenses"): the spike's
                // PARRY role is drawn DANGER (an ember at rest, lit through the charge: the body is not parryable, its orbs are and keep their
                // yellow); the TRIM ring at the rim stands Foes47DroneBandY taller, a band framing the lenses above and below
                if (b.l43.on) b.l43.extras.Pose(look, block, sc, flash, charging ? 1 : 0, false, danger40, f47 ? danger40 * UnityTuning.Foes47DroneSpikeGlow : parry40, (float)d.pulse); // NIGHT43 A1 models43
                // NIGHT48 F foes48 round 6 (the blind pair, DECIDE48: the old drone won 3-0 twice, "mismatched pods ... the rim ring off-centre"): the
                // taller band is the cause: the rim ring's tube (radius 0.40-0.66 m) holds the three lenses (radius 0.63 m, 0.25 m balls, 5 cm over the
                // ring's middle), and x1.7 taller it swallows each lens to a different depth by its place round the rim (the near one pokes out as a
                // big pod, the far one as a small one) and shows its 5 cm offset. With foes48 on the ring keeps its own scale (the old, symmetric
                // band; the spike's red-orange danger point stays)
                // NIGHT49 P0 (DECIDE49 D-2): the drone keeps its NIGHT47 look with foes48 on (NIGHT48's own-scale band was an OFF row, deleted at NIGHT71 D1)
                if (b.l43.on && b.l43.extras != null && b.l43.extras.parts.Length > 0) Band47(b, b.l43.extras.parts[0], f47);
                look.Notch(d.id, b.root);
                FeelProbe.Enemy(d.id, b.rends[0].bounds, b.rends[1].bounds, flash); // NIGHT18: the reel's victim rect (M5)
            }
            drawnCount = n;
            for (int i = n; i < pool.Count; i++)
            {
                pool[i].root.gameObject.SetActive(false);
                pool[i].drawn = null;
            }

            // The orbs (drone.js orbs.render): hostile cyan with its ring, reflected warm white.
            DroneOrbs.Orb[] live = game.husks.orbs.pool;
            for (int i = 0; i < orbs.Length; i++)
            {
                DroneOrbs.Orb o = live[i];
                if (o.alive != orbs[i].gameObject.activeSelf) orbs[i].gameObject.SetActive(o.alive);
                bool ring = o.alive && !o.reflected && shapeCues;
                if (ring != rings[i].gameObject.activeSelf) rings[i].gameObject.SetActive(ring);
                if (!o.alive) continue;
                if (o.reflected != orbReflected[i])
                {
                    orbReflected[i] = o.reflected;
                    orbRends[i].sharedMaterial = o.reflected ? reflectedMat : hostileMat;
                }
                Vector3 at = WebSpace.ToUnity(o.previous + (o.position - o.previous) * alpha);
                orbs[i].position = at;
                if (ring)
                {
                    float a = (o.ageSteps + (float)alpha) * ringSpin;
                    rings[i].position = at;
                    rings[i].rotation = Quaternion.Euler(-a * Mathf.Rad2Deg, -a * 0.62f * Mathf.Rad2Deg, 0);
                }
            }
        }
    }
}
