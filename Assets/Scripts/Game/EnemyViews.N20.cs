// NIGHT20 (builder E owns this file): the new kinds' views through the shared commit's hooks (SurvivalGame.N20.cs:
// N20EnemyViewInit / N20EnemyViewRender), as partials of SurvivalGame.
// U3-34.9 C2: the LANCER as lancer.js draws it (GG's design, rebuilt as it is; CLAUDE.md rule 7):
// - the body: a capsule of lancer.radius x lancer.height in colors.enemy x lancer.shade (0.7 toward white and stretched
//   (0.4, 1.3, 0.4) while it blinks), a gunmetal barrel box (0.12 x 0.14 x 1.3 at (0.3, 0.78 h, 0.55 ahead)) and the visor
//   box (0.36 x 0.14 x 0.12 at (0, 0.88 h, 0.28 ahead)) in colors.enemy x2.5 while it aims (x1.2 otherwise): the tell;
// - the laser (the DANGER tell, >= 1.2 s before every shot): a thin open cylinder from the muzzle to the lagging aim point
//   (Lancer.AimTo), colors.enemy x2.5, its radius growing over the aim (Lancer.AimProgress); the fired beam: 0.2 s from the
//   muzzle to Lancer.BeamTo, white-hot 0xffe0c0 x4, thinning out (a new Lancer.Shots starts it).
// V draws the beam's polished look from the same fields (State, AimTo, AimProgress, Muzzle, BeamTo, Shots, LastHit) or the
// bus's lancerAim / lancerFire / lancerBlink. Interpolated on frame time; pooled (lancer.maxCount bodies and lasers, made
// once); FeelProbe.Enemy reports each lancer as the husk view does. Numbers: UnityTuning.Lancer20.cs and config.lancer.
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace Skygrind.Game
{
    public sealed partial class SurvivalGame
    {
        LancerView lancerView;
        WardenView wardenView;

        BulwarkView bulwarkView;

        partial void N20EnemyViewInit()
        {
            lancerView = new LancerView(this);
            wardenView = new WardenView(this);
            bulwarkView = new BulwarkView(this); // NIGHT20 A1 block 2 (EnemyViews.Bulwark.cs)
        }

        partial void N20EnemyViewRender(double alpha, double frameDt)
        {
            lancerView?.Render(alpha, frameDt);
            wardenView?.Render(alpha, frameDt);
            bulwarkView?.Render(alpha, frameDt);
        }
    }

    /// <summary>
    /// U3-34.9 C4: the WARDEN as warden.js draws it: the body (a capsule of warden.radius x warden.height, x (1.25, 1, 1.1),
    /// colors.enemy x warden.shade; the tell pulses toward husk.windupColor: the DANGER swing tell; shield down: 0.25 toward
    /// white), the shield (1.9 x 0.85 h x 0.12, 0.85 m ahead at 0.52 h, amber shimmering, flaring on a block; down: dark,
    /// pulled aside and tilted), the arm (raised in the tell, swung in the swing). Numbers: UnityTuning.Warden20.cs.
    /// </summary>
    public sealed partial class WardenView // NIGHT47 F: partial (foes47's shield: EnemyViews.Shield47.cs)
    {
        sealed class Body
        {
            public Transform root, body, shield, arm;
            public readonly Renderer[] rends = new Renderer[3];
            public readonly Color[] colors = new Color[3];
            public Enemy40Parts m40; // NIGHT40 E3 models40: the silhouette parts (null: the row off)
            public Look43 l43; // NIGHT43 A1 models43: the rebuilt model (Enemy43View.cs)
            public Enemy drawn;
            public float downK47, downV47, kick47; // NIGHT47 F foes47: the shield's eased down (0 up .. 1 down) and its block kick
            public bool shieldMoved47;
            public Mesh mesh47; // the shield's own mesh while foes47 shows the arc (put back with the row off)
        }

        static readonly float[] Rims = { 1, 0, 1 }; // warden.js: the shield has rim false
        readonly SurvivalGame game;
        readonly EnemyLook look;
        readonly bool models40; // NIGHT40 E3: design.n40.models40.enabled when the view was made
        readonly Color danger40, parry40; // NIGHT40 E3: the parts' red-orange and yellow
        readonly List<Body> pool = new List<Body>();
        readonly Mesh capsuleMesh, boxMesh;
        readonly Mesh[] meshes;
        readonly MaterialPropertyBlock block = new MaterialPropertyBlock();
        readonly Transform parent;
        readonly Color bodyColor, armColor, tell, shieldUp, shieldDown, white;
        readonly float radius, height;
        int drawnCount;

        public WardenView(SurvivalGame game)
        {
            this.game = game;
            GameConfig cfg = game.cfg;
            look = EnemyLook.For(game);
            models40 = Enemy40Parts.On(game.cfg);
            danger40 = Look.Hex(UnityTuning.Tell36RedHex);
            parry40 = Look.Hex(UnityTuning.Tell36YellowHex);
            parent = new GameObject("Wardens").transform;
            radius = (float)cfg.Num("warden.radius");
            height = (float)cfg.Num("warden.height");
            bodyColor = Look.C(cfg, "colors.enemy", (float)cfg.Num("warden.shade"));
            armColor = Look.C(cfg, "colors.enemy", 0.75f);
            tell = Look.C(cfg, "husk.windupColor");
            shieldUp = Look.Hex(UnityTuning.WardenShieldHex);
            shieldDown = Look.Hex(UnityTuning.WardenShieldDownHex);
            white = Look.Hex(0xffffff);
            var probe = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            capsuleMesh = probe.GetComponent<MeshFilter>().sharedMesh;
            Object.Destroy(probe);
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            boxMesh = cube.GetComponent<MeshFilter>().sharedMesh;
            Object.Destroy(cube);
            meshes = new[] { capsuleMesh, boxMesh, boxMesh };
            game.events.On<KillEvent>("kill", OnKill);
        }

        void OnKill(KillEvent e)
        {
            for (int i = 0; i < drawnCount; i++)
            {
                Body b = pool[i];
                if (b.drawn == null || b.drawn.id != e.enemy) continue;
                look.Die(b.drawn, e, b.rends, meshes, b.colors, Rims, 3, -1, b.l43 != null ? b.l43.DyingParts(b.m40) : b.m40); // NIGHT41 E6 models41: + the parts break (NIGHT43 A1: models43's while it draws)
                return;
            }
        }

        Renderer Part(Transform root, string name, Mesh mesh, out Transform t)
        {
            var g = new GameObject(name);
            g.transform.SetParent(root, false);
            g.AddComponent<MeshFilter>().sharedMesh = mesh;
            var r = g.AddComponent<MeshRenderer>();
            r.shadowCastingMode = ShadowCastingMode.Off;
            t = g.transform;
            return r;
        }

        Body Make()
        {
            var b = new Body();
            b.root = new GameObject("warden").transform;
            b.root.SetParent(parent, false);
            b.rends[0] = Part(b.root, "body", capsuleMesh, out b.body);
            b.rends[1] = Part(b.root, "shield", boxMesh, out b.shield);
            b.rends[2] = Part(b.root, "arm", boxMesh, out b.arm);
            b.body.localPosition = new Vector3(0, height * 0.5f, 0);
            b.body.localScale = new Vector3(radius * 2 * UnityTuning.WardenBodyX, height / 2, radius * 2 * UnityTuning.WardenBodyZ);
            b.shield.localScale = new Vector3(1.9f, height * 0.85f, 0.12f);
            b.arm.localPosition = new Vector3(0.75f, height * 0.6f, 0.2f); // the web's -z ahead is Unity's +z
            b.arm.localScale = new Vector3(0.35f, 1.3f, 0.35f);
            look.Attach(b.rends[0], true, true); // main.js's xray list: the warden is elite
            look.Attach(b.rends[1], false, false);
            look.Attach(b.rends[2], false, false);
            if (models40) b.m40 = Enemy40Parts.Make("warden", b.root, look, height); // NIGHT40 E3 models40
            b.l43 = new Look43("warden", b.root, b.rends, UnityTuning.Models43WardenBase, null, look, height); // NIGHT43 A1 models43
            return b;
        }

        public void Render(double alpha, double frameDt)
        {
            look.Frame();
            int n = 0;
            foreach (Enemy e in game.enemies.all)
            {
                if (!(e is Warden w) || !w.alive) continue;
                if (n >= pool.Count) pool.Add(Make());
                Body b = pool[n++];
                b.drawn = w;
                b.root.gameObject.SetActive(true);
                w.flash = System.Math.Max(0, w.flash - frameDt);
                Vec3 p = w.previous + (w.position - w.previous) * alpha;
                Quaternion yawQ = Quaternion.Euler(0, -(float)(w.yaw * Mathf.Rad2Deg), 0);
                float flash = look.React(w.id, yawQ, w.flash > 0, out Vector3 off, out Vector3 squash, out Quaternion tilt);
                b.root.position = WebSpace.ToUnity(p) + off;
                b.root.rotation = tilt * yawQ;
                b.root.localScale = squash;
                float age = (float)w.drawAge;
                bool down = w.ShieldDown;
                Color body = bodyColor;
                if (w.state == "tell") body = Color.Lerp(body, tell, 0.5f + 0.5f * Mathf.Sin(age * UnityTuning.WardenTellPulse));
                if (down) body = Color.Lerp(body, white, UnityTuning.WardenDownWhite);
                Color shield = down ? shieldDown : shieldUp * (UnityTuning.WardenShieldBase + (w.blockFlash > 0 ? UnityTuning.WardenShieldBlock : UnityTuning.WardenShieldShimmer * Mathf.Sin(age * 3)));
                if (Foes48.On(game.cfg)) shield = Shield48Color(shield, down, w, age); // NIGHT48 F foes48 round 5: the shield orange at a body's value (it drew pale)
                float arm = w.state == "tell" ? UnityTuning.WardenArmTell : w.state == "swing" ? UnityTuning.WardenArmSwing : 0;
                Shield48Queue(b, Foes48.On(game.cfg)); // NIGHT48 F foes48 round 3: the shield drawn after the body's x-ray (it painted over the shield)
                if (Foes48.On(game.cfg)) Shield48(b, w, down, arm, (float)frameDt); // NIGHT48 F foes48 (G12; EnemyViews.Foes48.cs): the arm behind the shield, down swung to the side
                else if (Foes47.On(game.cfg)) Shield47(b, w, down, arm, (float)frameDt); // NIGHT47 F foes47 (G12; EnemyViews.Shield47.cs): held, centred, clear when down
                else
                {
                    if (b.shieldMoved47)
                    {
                        b.shieldMoved47 = false;
                        b.shield.localScale = new Vector3(1.9f, height * 0.85f, 0.12f);
                        b.arm.localPosition = new Vector3(0.75f, height * 0.6f, 0.2f);
                        b.arm.localScale = new Vector3(0.35f, 1.3f, 0.35f); // NIGHT48 F foes48 sizes the arm (foes47 never did: a no-op after it)
                        MeshFilter mf = b.shield.GetComponent<MeshFilter>(); // NIGHT47 F foes47 off again: the shield's own mesh and its frame back
                        if (mf != null && b.mesh47 != null) mf.sharedMesh = b.mesh47;
                        for (int c = 0; c < b.shield.childCount; c++) b.shield.GetChild(c).gameObject.SetActive(true);
                    }
                    b.shield.localPosition = down ? new Vector3(0.7f, height * 0.35f, 0.3f) : new Vector3(0, height * 0.52f, 0.85f);
                    b.shield.localRotation = Quaternion.Euler(down ? -UnityTuning.WardenShieldDownTilt * Mathf.Rad2Deg : 0, 0, 0);
                    b.arm.localRotation = Quaternion.Euler(-arm * Mathf.Rad2Deg, 0, 0);
                }
                b.l43.Apply(Models43.On(game.cfg), b.m40); // NIGHT43 A1 models43: the body and the arm darkened, the shield keeps its colours
                Color body43 = b.l43.on ? Models43.Body(body) : body, arm43 = b.l43.on ? Models43.Body(armColor) : armColor;
                if (w.state == "tell" && Foes47.On(game.cfg)) // NIGHT47 F foes47 (G21, the enemies' side): the swing's aura: the arm burns red-orange through the wind-up (it had no tell halo)
                    arm43 = Color.Lerp(arm43, danger40 * UnityTuning.Foes47SwingGlow, 0.55f + 0.45f * Mathf.Sin(age * UnityTuning.WardenTellPulse));
                b.colors[0] = body43;
                b.colors[1] = shield;
                b.colors[2] = arm43;
                look.Tint(block, b.rends[0], body43, flash, Rims[0]);
                look.Tint(block, b.rends[1], shield, 0, Rims[1]);
                look.Tint(block, b.rends[2], arm43, flash, Rims[2]);
                b.m40?.Pose(look, block, body, flash, w.state == "tell" ? 1 : 0, down, danger40, parry40, age); // NIGHT40 E3 models40
                if (b.l43.on) b.l43.extras.Pose(look, block, body43, flash, w.state == "tell" ? 1 : 0, down, danger40, parry40, age); // NIGHT43 A1 models43
                look.Notch(w.id, b.root);
                if (Foes48.Probe) Shield48Measure(b); // NIGHT48 F a948's shield probe (off in play)
                FeelProbe.Enemy(w.id, b.rends[0].bounds, b.rends[1].bounds, flash);
            }
            drawnCount = n;
            for (int i = n; i < pool.Count; i++)
            {
                if (pool[i].root.gameObject.activeSelf) pool[i].root.gameObject.SetActive(false);
                pool[i].drawn = null;
            }
        }
    }

    public sealed class LancerView
    {
        sealed class Body
        {
            public Transform root, capsule, barrel, visor;
            public readonly Renderer[] rends = new Renderer[3];
            public readonly Color[] colors = new Color[3];
            public Enemy drawn;
            public Transform laser;
            public Renderer laserRend;
            public int shots;
            public float beamAge = float.PositiveInfinity;
        }

        static readonly float[] Rims = { 1, 0, 0 }; // lancer.js: the rim on the body only (parts 2-3: rim false)
        readonly SurvivalGame game;
        readonly EnemyLook look;
        readonly List<Body> pool = new List<Body>();
        readonly Mesh capsuleMesh, boxMesh, laserMesh;
        readonly Mesh[] meshes;
        readonly MaterialPropertyBlock block = new MaterialPropertyBlock(), laserBlock = new MaterialPropertyBlock();
        readonly Transform parent;
        readonly Color bodyColor, enemy, barrelColor, beamColor, white;
        readonly float radius, height;
        readonly Material laserMat;
        static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        int drawnCount;

        public LancerView(SurvivalGame game)
        {
            this.game = game;
            GameConfig cfg = game.cfg;
            look = EnemyLook.For(game);
            parent = new GameObject("Lancers").transform;
            radius = (float)cfg.Num("lancer.radius");
            height = (float)cfg.Num("lancer.height");
            bodyColor = Look.C(cfg, "colors.enemy", (float)cfg.Num("lancer.shade"));
            enemy = Look.C(cfg, "colors.enemy");
            barrelColor = Look.Hex(UnityTuning.LancerBarrelHex);
            beamColor = Look.Hex(UnityTuning.LancerViewBeamHex, UnityTuning.LancerBeamGain);
            white = Look.Hex(0xffffff);
            var probe = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            capsuleMesh = probe.GetComponent<MeshFilter>().sharedMesh;
            Object.Destroy(probe);
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            boxMesh = cube.GetComponent<MeshFilter>().sharedMesh;
            Object.Destroy(cube);
            meshes = new[] { capsuleMesh, boxMesh, boxMesh };
            laserMesh = OpenCylinder(6);
            laserMat = new Material(game.mats.neon) { name = "Lancer laser" };
            game.events.On<KillEvent>("kill", OnKill);
        }

        /// <summary>three.js CylinderGeometry(1, 1, 1, sides, 1, true).translate(0, 0.5, 0): an open tube from y 0 to 1.</summary>
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
                tris[t++] = a; tris[t++] = b; tris[t++] = c; // both faces (a thin, additive tube)
                tris[t++] = c; tris[t++] = b; tris[t++] = d;
                tris[t++] = a; tris[t++] = c; tris[t++] = b;
                tris[t++] = c; tris[t++] = d; tris[t++] = b;
            }
            var m = new Mesh { name = "lancer laser", vertices = verts, triangles = tris };
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
                look.Die(b.drawn, e, b.rends, meshes, b.colors, Rims, 3, -1);
                return;
            }
        }

        Renderer Part(Transform root, string name, Mesh mesh, out Transform t)
        {
            var g = new GameObject(name);
            g.transform.SetParent(root, false);
            g.AddComponent<MeshFilter>().sharedMesh = mesh;
            var r = g.AddComponent<MeshRenderer>();
            r.shadowCastingMode = ShadowCastingMode.Off;
            t = g.transform;
            return r;
        }

        Body Make()
        {
            var b = new Body();
            b.root = new GameObject("lancer").transform;
            b.root.SetParent(parent, false);
            b.rends[0] = Part(b.root, "body", capsuleMesh, out b.capsule);
            b.rends[1] = Part(b.root, "barrel", boxMesh, out b.barrel);
            b.rends[2] = Part(b.root, "visor", boxMesh, out b.visor);
            // Unity's capsule primitive: 2 tall, radius 0.5 (three.js CapsuleGeometry(r, h - 2r): h tall in all)
            b.capsule.localScale = new Vector3(radius * 2, height / 2, radius * 2);
            b.barrel.localPosition = new Vector3(0.3f, height * 0.78f, 0.55f); // the web's -z ahead is Unity's +z
            b.barrel.localScale = new Vector3(0.12f, 0.14f, 1.3f);
            b.visor.localPosition = new Vector3(0, height * 0.88f, 0.28f);
            b.visor.localScale = new Vector3(0.36f, 0.14f, 0.12f);
            look.Attach(b.rends[0], true, true); // lancer.js: xray elite (main.js's xray list)
            look.Attach(b.rends[1], false, false);
            look.Attach(b.rends[2], false, false);
            var laser = new GameObject("lancer laser");
            laser.transform.SetParent(parent, false);
            laser.AddComponent<MeshFilter>().sharedMesh = laserMesh;
            var lr = laser.AddComponent<MeshRenderer>();
            lr.sharedMaterial = laserMat;
            lr.shadowCastingMode = ShadowCastingMode.Off;
            laser.SetActive(false);
            b.laser = laser.transform;
            b.laserRend = lr;
            return b;
        }

        public void Render(double alpha, double frameDt)
        {
            look.Frame();
            int n = 0;
            float fdt = (float)frameDt;
            foreach (Enemy e in game.enemies.all)
            {
                if (!(e is Lancer l) || !l.alive) continue;
                if (n >= pool.Count) pool.Add(Make());
                Body b = pool[n++];
                if (b.drawn != l)
                {
                    b.drawn = l;
                    b.shots = l.Shots;
                    b.beamAge = float.PositiveInfinity;
                }
                b.root.gameObject.SetActive(true);
                l.flash = System.Math.Max(0, l.flash - frameDt);
                bool blinking = l.state == Lancer.State.Blink, aiming = l.state == Lancer.State.Aim;
                Vec3 p = l.previous + (l.position - l.previous) * alpha;
                Quaternion yawQ = Quaternion.Euler(0, -(float)(l.yaw * Mathf.Rad2Deg), 0);
                float flash = look.React(l.id, yawQ, l.flash > 0, out Vector3 off, out Vector3 squash, out Quaternion tilt);
                b.root.position = WebSpace.ToUnity(p) + off;
                b.root.rotation = tilt * yawQ;
                b.root.localScale = blinking ? Vector3.Scale(squash, new Vector3(UnityTuning.LancerBlinkXZ, UnityTuning.LancerBlinkY, UnityTuning.LancerBlinkXZ)) : squash;
                b.capsule.localPosition = new Vector3(0, height * 0.5f, 0);
                Color body = blinking ? Color.Lerp(bodyColor, white, UnityTuning.LancerBlinkWhite) : bodyColor;
                Color visor = enemy * (aiming ? UnityTuning.LancerVisorAim : UnityTuning.LancerVisorIdle);
                b.colors[0] = body;
                b.colors[1] = barrelColor;
                b.colors[2] = visor;
                look.Tint(block, b.rends[0], body, flash, Rims[0]);
                look.Tint(block, b.rends[1], barrelColor, flash, Rims[1]);
                look.Tint(block, b.rends[2], visor, flash, Rims[2]);
                look.Notch(l.id, b.root);
                FeelProbe.Enemy(l.id, b.rends[0].bounds, b.rends[2].bounds, flash);

                // The laser while it aims (the tell), the beam for 0.2 s after a shot.
                if (l.Shots != b.shots)
                {
                    b.shots = l.Shots;
                    b.beamAge = 0;
                }
                else b.beamAge += fdt;
                bool beam = b.beamAge < UnityTuning.LancerBeamTime;
                bool show = false; // NIGHT20 (the lead, builder V): V's LancerBeam20 draws the laser and the beam (one, not two)
                if (show != b.laser.gameObject.activeSelf) b.laser.gameObject.SetActive(show);
                if (!show) continue;
                Vector3 from = WebSpace.ToUnity(l.Muzzle);
                Vector3 to = WebSpace.ToUnity(beam && !aiming ? l.BeamTo : l.AimTo);
                Vector3 dir = to - from;
                float len = dir.magnitude;
                float r = beam && !aiming
                    ? UnityTuning.LancerViewBeamRadius * (1 - b.beamAge / UnityTuning.LancerBeamTime)
                    : UnityTuning.LancerLaserRadius + UnityTuning.LancerLaserGrow * (float)l.AimProgress;
                b.laser.position = from;
                b.laser.rotation = Quaternion.FromToRotation(Vector3.up, len > 0 ? dir / len : Vector3.up);
                b.laser.localScale = new Vector3(r, len, r);
                laserBlock.SetColor(BaseColor, beam && !aiming ? beamColor : enemy * UnityTuning.LancerLaserGain);
                b.laserRend.SetPropertyBlock(laserBlock);
            }
            drawnCount = n;
            for (int i = n; i < pool.Count; i++)
            {
                if (pool[i].root.gameObject.activeSelf) pool[i].root.gameObject.SetActive(false);
                if (pool[i].laser.gameObject.activeSelf) pool[i].laser.gameObject.SetActive(false);
                pool[i].drawn = null;
            }
        }
    }
}
