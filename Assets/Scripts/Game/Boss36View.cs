// NIGHT36 builder B (boss36 + bossmodel36; DECIDE36 D-3): what the player SEES of THE LANTERN's hall and of the boss on the Boss36 shape.
// The lead adds two lines in SurvivalGame.Boss26.cs:  Boss26InitViews: `Boss36View.Current = new Boss36View(this);`
//                                                      Boss26RenderViews: `Boss36View.Current?.Render(alpha);`
//  - THE HALL (whenever the level has the lantern36 row): the SAME triangles the sim collides with (Lantern36Arena), dark dusk stone; a cyan trim
//    strip at the bottom and the top of the ride band on every facet but the gap's (cyan = player-usable); the entry slab red-orange while the
//    doors are locked; the well's grate; one pale point light.
//  - THE FIGHT (while Lantern36 runs): the reveal's beam out of the well; the floor's lava growing out from the well through its tell, then live;
//    the sweep's bar on the wall in its tell and its beam(s) while it burns; the camp decal; the orbs (YELLOW: parry them; a returned one white).
//  - THE BODY (bossmodel36 on: N36Fixes.ViewRowOn): a jointed lantern built from parts: a cage of four bowed ribs between two caps, four shield
//    plates on hinges, a core, four two-segment arms on shoulder joints and a hanging chain. Every joint follows its pose's target through a
//    critically damped spring (spring bones); the body breathes; each attack has its own pose (fan: arms up, plates open; floor: arms down into
//    the well; sweep: one arm out at the wall; camp: one arm raised over the camper; punish: limp, plates open; stun: tilted, splayed). The tell's
//    colour (P's yellow / red) runs through the core, the claws and the rim. Bossmodel36 off: a plain block and the core (the same fx).
// Rule 8: the body dark so the rim and core read as a silhouette with no light; red-orange danger; cyan the open core and the ride band.
using System.Collections.Generic;
using Skygrind.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace Skygrind.Game
{
    public sealed class Boss36View
    {
        public static Boss36View Current;

        readonly SurvivalGame game;
        readonly bool model;
        readonly Transform root, hallRoot, fxRoot;
        readonly Mesh boxMesh, ballMesh;
        readonly Material add, iron, ironDark;
        readonly MaterialPropertyBlock block = new MaterialPropertyBlock();
        static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        Vec3 builtAt;
        bool built;
        GameObject slabGo;
        Renderer slabR, wellR, beamR, lavaR, campR, sweepBarR;
        Transform wellT, beamT, lavaT, campT, sweepBarT;
        readonly List<Transform> beamTs = new List<Transform>();
        readonly List<Renderer> beamRs = new List<Renderer>();
        readonly List<Transform> orbTs = new List<Transform>();
        readonly List<Renderer> orbRs = new List<Renderer>();
        Light light;
        float campFlash;

        // the body
        Transform bodyRoot, breathe, coreT, coreInnerT, plainT;
        Renderer coreR, coreInnerR;
        readonly Transform[] plateHinge = new Transform[4];
        readonly Quaternion[] plateBase = new Quaternion[4];
        float hitFlash, stunFlash;
        Light coreLight;
        /// <summary>Stun bursts drawn (the reel reads it).</summary>
        public int stunBursts;
        /// <summary>The core's colour this frame (the reel prints it).</summary>
        public Color CoreColour;
        readonly Transform[] shoulder = new Transform[4], elbow = new Transform[4], claw = new Transform[4];
        readonly Renderer[] clawR = new Renderer[4];
        readonly List<Renderer> rimRs = new List<Renderer>();
        readonly Transform[] chain = new Transform[3];
        // spring bones: per arm (shoulder pitch, shoulder yaw offset, elbow), plates, tilt; value + velocity
        readonly float[] sp = new float[4], spV = new float[4], sy = new float[4], syV = new float[4], el = new float[4], elV = new float[4];
        float plate, plateV, tilt, tiltV;
        Vector3 chainLag, chainLagV, lastPos;
        float t;

        /// <summary>Frames the reel reads: the slab drawn, the lava shown, a sweep beam shown, orbs drawn, the body's pose name.</summary>
        public bool SlabShown => slabGo != null && slabGo.activeSelf;
        public bool LavaShown => lavaT != null && lavaT.gameObject.activeSelf;
        public bool SweepShown => sweepBarT != null && sweepBarT.gameObject.activeSelf;
        public int OrbsShown;
        public string Pose = "";

        public Boss36View(SurvivalGame game)
        {
            this.game = game;
            Current = this;
            model = N36Fixes.ViewRowOn(game.cfg, "bossmodel36");
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            boxMesh = cube.GetComponent<MeshFilter>().sharedMesh;
            Object.Destroy(cube);
            var ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ballMesh = ball.GetComponent<MeshFilter>().sharedMesh;
            Object.Destroy(ball);
            add = FxDraw.Additive(game.mats.neon, "Boss36 glow");
            iron = Opaque("lantern36 iron", UnityTuning.Body36Hex);
            ironDark = Opaque("lantern36 iron dark", UnityTuning.Body36DarkHex);
            root = new GameObject("NIGHT36 THE LANTERN").transform;
            hallRoot = new GameObject("hall").transform;
            hallRoot.SetParent(root, false);
            hallRoot.gameObject.SetActive(false);
            fxRoot = new GameObject("fight fx").transform;
            fxRoot.SetParent(root, false);
            fxRoot.gameObject.SetActive(false);
            BuildBody();
            game.events.On<Lantern36Event>("boss36Camp", e => { if (e.stage == "slam") campFlash = UnityTuning.Camp36SlamFlash; });
            game.events.On<BossEvent>("bossHit", e => { if (e.boss == "lantern" && game.sim?.lantern36 != null && game.sim.lantern36.Alive) hitFlash = 0.06f; });
            game.events.On<Boss36Event>("boss36Stun", e =>
            {
                // the stun reads as a hit, not a slump: the core flashes cyan, the plates are kicked open, sparks fly
                if (e.boss != "lantern" || game.sim?.lantern36 == null) return;
                stunFlash = UnityTuning.Body36StunFlashS;
                plateV += UnityTuning.Body36StunPlateKick;
                stunBursts++;
                Vec3 at = game.sim.lantern36.Centre;
                game.ParticleBurst(at, Look.Hex(UnityTuning.Body36OpenHex, UnityTuning.Body36StunSparkGain), UnityTuning.Body36StunSparks, UnityTuning.Body36StunSparkSpeed, UnityTuning.Body36StunSparkLife, UnityTuning.Body36StunSparkSize);
                game.ParticleBurst(at, Look.Hex(UnityTuning.Body36RimHex, UnityTuning.Body36StunSparkGain), UnityTuning.Body36StunSparks / 2, UnityTuning.Body36StunSparkSpeed * 0.7f, UnityTuning.Body36StunSparkLife, UnityTuning.Body36StunSparkSize);
            });
        }

        Material Opaque(string name, uint hex)
        {
            var mat = new Material(game.mats.stone) { name = name };
            Color c = Look.Hex(hex);
            if (mat.HasProperty(BaseColor)) mat.SetColor(BaseColor, c);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", c);
            if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", null);
            if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", null);
            return Light54.StoneOrLit(game, mat, "Boss36View", name, hex); // NIGHT54 light54: Stone.shader reads the vertex colour, not _BaseColor (white): the intended hex on URP Lit with the row on
        }

        static Vector3 U(Vec3 v) => new Vector3((float)v.x, (float)v.y, (float)-v.z); // web z is Unity -z
        static Vector3 N(Vec3 n) => new Vector3((float)n.x, (float)n.y, (float)-n.z);
        /// <summary>A web angle (atan2(z, x), radians) as a Unity direction on the ground.</summary>
        static Vector3 Dir(double a) => new Vector3((float)System.Math.Cos(a), 0, -(float)System.Math.Sin(a));

        void Tint(Renderer r, Color c)
        {
            block.SetColor(BaseColor, c);
            r.SetPropertyBlock(block);
        }

        Renderer Glow(Transform parent, string name, Mesh mesh, out Transform t)
        {
            var g = new GameObject(name);
            g.transform.SetParent(parent, false);
            g.AddComponent<MeshFilter>().sharedMesh = mesh;
            var r = g.AddComponent<MeshRenderer>();
            r.sharedMaterial = add;
            r.shadowCastingMode = ShadowCastingMode.Off;
            t = g.transform;
            return r;
        }

        Transform Solid(Transform parent, string name, Vector3 pos, Vector3 size, Material mat, Mesh mesh = null)
        {
            var g = new GameObject(name);
            g.transform.SetParent(parent, false);
            g.transform.localPosition = pos;
            g.transform.localScale = size;
            g.AddComponent<MeshFilter>().sharedMesh = mesh ?? boxMesh;
            var r = g.AddComponent<MeshRenderer>();
            r.sharedMaterial = mat;
            r.shadowCastingMode = ShadowCastingMode.On;
            return g.transform;
        }

        Transform Pivot(Transform parent, string name, Vector3 pos)
        {
            var g = new GameObject(name);
            g.transform.SetParent(parent, false);
            g.transform.localPosition = pos;
            return g.transform;
        }

        void Rim(Transform parent, Vector3 pos, Vector3 size)
        {
            Renderer r = Glow(parent, "rim", boxMesh, out Transform rt);
            rt.localPosition = pos;
            rt.localScale = size;
            rimRs.Add(r);
        }

        // --- the body -------------------------------------------------------------------------------------------------------------------------

        void BuildBody()
        {
            bodyRoot = new GameObject("THE LANTERN body").transform;
            bodyRoot.SetParent(root, false);
            bodyRoot.gameObject.SetActive(false);
            breathe = Pivot(bodyRoot, "breathe", Vector3.zero);
            float h = UnityTuning.Body36Height * 0.5f, w = UnityTuning.Body36Half;
            coreR = Glow(breathe, "core", ballMesh, out coreT);
            coreT.localScale = Vector3.one * UnityTuning.Body36CoreSize;
            var lightGo = new GameObject("core light");
            lightGo.transform.SetParent(breathe, false);
            coreLight = lightGo.AddComponent<Light>();
            coreLight.type = LightType.Point;
            coreLight.shadows = LightShadows.None;
            coreLight.range = UnityTuning.Body36CoreLightRange;
            coreInnerR = Glow(breathe, "core heart", ballMesh, out coreInnerT);
            coreInnerT.localScale = Vector3.one * 0.7f;
            if (!model)
            {
                // bossmodel36 off: a plain dark block around the core (the fx still draw)
                plainT = Solid(breathe, "plain block", Vector3.zero, new Vector3(2.2f, 2.2f, 2.2f), ironDark);
                plainT.localScale = new Vector3(2.4f, 2.4f, 2.4f);
                plainT.GetComponent<Renderer>().enabled = false; // the core reads through: the block is its outline only
                Rim(breathe, new Vector3(0, 1.2f, 0), new Vector3(2.4f, 0.08f, 2.4f));
                Rim(breathe, new Vector3(0, -1.2f, 0), new Vector3(2.4f, 0.08f, 2.4f));
                return;
            }
            // the caps (a top with a finial, a bottom with a hook) and their rims
            Solid(breathe, "cap top", new Vector3(0, h, 0), new Vector3(2 * w + 0.5f, 0.35f, 2 * w + 0.5f), iron);
            Solid(breathe, "finial", new Vector3(0, h + 0.55f, 0), new Vector3(0.5f, 0.8f, 0.5f), ironDark);
            Solid(breathe, "finial ring", new Vector3(0, h + 1.05f, 0), new Vector3(0.9f, 0.18f, 0.9f), iron);
            Solid(breathe, "cap bottom", new Vector3(0, -h, 0), new Vector3(2 * w + 0.2f, 0.3f, 2 * w + 0.2f), iron);
            Rim(breathe, new Vector3(0, h - 0.2f, 0), new Vector3(2 * w + 0.55f, 0.06f, 2 * w + 0.55f));
            Rim(breathe, new Vector3(0, -h + 0.17f, 0), new Vector3(2 * w + 0.25f, 0.06f, 2 * w + 0.25f));
            // four bowed ribs at the corners (two segments each, bent outward)
            for (int i = 0; i < 4; i++)
            {
                float ax = (i == 0 || i == 3) ? 1 : -1, az = i < 2 ? 1 : -1;
                Vector3 c = new Vector3(ax * w, 0, az * w);
                Transform up = Solid(breathe, "rib up " + i, c * 1.08f + new Vector3(0, h * 0.5f, 0), new Vector3(0.22f, h * 1.05f, 0.22f), ironDark);
                up.localRotation = Quaternion.FromToRotation(Vector3.up, (new Vector3(0, h, 0) - c * 0.16f).normalized);
                Transform dn = Solid(breathe, "rib down " + i, c * 1.08f - new Vector3(0, h * 0.5f, 0), new Vector3(0.22f, h * 1.05f, 0.22f), ironDark);
                dn.localRotation = Quaternion.FromToRotation(Vector3.up, (new Vector3(0, h, 0) + c * 0.16f).normalized);
            }
            // four shield plates, one per side, hinged at the top cap's edge
            for (int i = 0; i < 4; i++)
            {
                Vector3 n = i == 0 ? Vector3.forward : i == 1 ? Vector3.right : i == 2 ? Vector3.back : Vector3.left;
                Transform hinge = Pivot(breathe, "plate hinge " + i, n * (w + 0.05f) + new Vector3(0, h - 0.2f, 0));
                hinge.localRotation = Quaternion.LookRotation(n, Vector3.up);
                Solid(hinge, "plate", new Vector3(0, -(h - 0.25f), 0), new Vector3(2 * w - 0.35f, 2 * h - 0.6f, 0.12f), iron);
                Rim(hinge, new Vector3(0, -(2 * h - 0.65f), 0.07f), new Vector3(2 * w - 0.4f, 0.06f, 0.04f));
                plateHinge[i] = hinge;
                plateBase[i] = hinge.localRotation;
            }
            // four arms on shoulders at the top cap's corners: upper arm, elbow, forearm, a glowing claw
            for (int i = 0; i < 4; i++)
            {
                float ang = 45 + 90 * i;
                Transform yaw = Pivot(breathe, "arm yaw " + i, Quaternion.Euler(0, ang, 0) * new Vector3(0, 0, w + 0.25f) + new Vector3(0, h - 0.1f, 0));
                yaw.localRotation = Quaternion.Euler(0, ang, 0);
                Transform sh = Pivot(yaw, "shoulder " + i, Vector3.zero);
                Solid(sh, "shoulder ball", Vector3.zero, Vector3.one * 0.45f, iron, ballMesh);
                Solid(sh, "upper arm", new Vector3(0, 0, UnityTuning.Body36UpperArm * 0.5f), new Vector3(UnityTuning.Body36ArmThick, UnityTuning.Body36ArmThick, UnityTuning.Body36UpperArm), ironDark);
                Transform elb = Pivot(sh, "elbow " + i, new Vector3(0, 0, UnityTuning.Body36UpperArm));
                Solid(elb, "elbow ball", Vector3.zero, Vector3.one * 0.36f, iron, ballMesh);
                Solid(elb, "forearm", new Vector3(0, 0, UnityTuning.Body36Forearm * 0.5f), new Vector3(UnityTuning.Body36ArmThick * 0.8f, UnityTuning.Body36ArmThick * 0.8f, UnityTuning.Body36Forearm), ironDark);
                clawR[i] = Glow(elb, "claw", boxMesh, out Transform cl);
                cl.localPosition = new Vector3(0, 0, UnityTuning.Body36Forearm + 0.15f);
                cl.localScale = new Vector3(0.34f, 0.34f, 0.3f);
                claw[i] = cl;
                shoulder[i] = sh;
                elbow[i] = elb;
            }
            // the chain under the bottom cap
            Transform prev = breathe;
            Vector3 at = new Vector3(0, -h - 0.15f, 0);
            for (int k = 0; k < 3; k++)
            {
                Transform link = Pivot(prev, "chain " + k, at);
                Solid(link, "link", new Vector3(0, -0.45f, 0), new Vector3(0.18f, 0.9f, 0.18f), ironDark);
                chain[k] = link;
                prev = link;
                at = new Vector3(0, -0.9f, 0);
            }
        }

        static void Spring(ref float x, ref float v, float target, float w, float dt)
        {
            // critically damped spring (the spring bone): x'' = w^2 (target - x) - 2 w x'
            float a = w * w * (target - x) - 2 * w * v;
            v += a * dt;
            x += v * dt;
        }

        // --- the hall ---------------------------------------------------------------------------------------------------------------------------

        void BuildHall(Lantern36Arena a, Vec3 o)
        {
            foreach (Transform ch in hallRoot) Object.Destroy(ch.gameObject);
            foreach (Transform ch in fxRoot) Object.Destroy(ch.gameObject); // a new level: the fx are rebuilt at its hall
            beamTs.Clear(); beamRs.Clear(); orbTs.Clear(); orbRs.Clear();
            var kits = new[] { new MeshKit(), new MeshKit(), new MeshKit() }; // walls (+ lintel), floor, roof
            var slab = new MeshKit();
            foreach (Lantern36Tri tr in a.tris)
            {
                Vec3 n = TriangleWorld.TriangleNormal(tr.a, tr.b, tr.c);
                if (tr.part == Lantern36Part.Slab) { slab.Triangle(U(tr.a + o), U(tr.b + o), U(tr.c + o), N(n), Color.white); continue; }
                int k = tr.part == Lantern36Part.Floor ? 1 : tr.part == Lantern36Part.Roof ? 2 : 0;
                kits[k].Triangle(U(tr.a + o), U(tr.b + o), U(tr.c + o), N(n), Color.white);
            }
            uint[] hex = { UnityTuning.Hall36WallHex, UnityTuning.Hall36FloorHex, UnityTuning.Hall36RoofHex };
            string[] names = { "hall walls", "hall floor", "hall roof" };
            for (int k = 0; k < 3; k++) MeshKit.Object(names[k], hallRoot, kits[k].Build("lantern36 " + names[k]), Opaque(names[k], hex[k]), shadows: false);
            // the cyan trim at the ride band's bottom and top, on every facet but the gap's
            var trim = new MeshKit();
            double fa = a.FacetAngle, apo = a.p.apothem - UnityTuning.Hall36TrimStandOff;
            for (int j = 0; j < a.p.facets; j++)
            {
                if (a.InGap(j)) continue;
                Vec3 nrm = a.FacetNormal(j);
                Vec3 p0 = a.OnRing(j * fa + 1e-6, apo, 0) + o, p1 = a.OnRing((j + 1) * fa - 1e-6, apo, 0) + o;
                foreach (double y in new[] { a.p.trimLow, a.p.trimHigh - UnityTuning.Hall36TrimHeight })
                {
                    Vec3 lo = new Vec3(0, y, 0), hi = new Vec3(0, y + UnityTuning.Hall36TrimHeight, 0);
                    trim.Quad(U(p0 + lo), U(p1 + lo), U(p1 + hi), U(p0 + hi), N(nrm), Color.white);
                }
            }
            Renderer trimR = MeshKit.Object("hall ride trim", hallRoot, trim.Build("lantern36 trim"), add, shadows: false).GetComponent<Renderer>();
            Tint(trimR, Look.Hex(UnityTuning.Hall36TrimHex, UnityTuning.Hall36TrimGain));
            slabGo = MeshKit.Object("hall slab (locked)", hallRoot, slab.Build("lantern36 slab"), add, shadows: false);
            slabR = slabGo.GetComponent<Renderer>();
            Tint(slabR, Look.Hex(UnityTuning.Hall36DangerHex, UnityTuning.Hall36SlabGain));
            slabGo.SetActive(false);
            // the well's grate (a flat disc), the lava disc, the reveal beam
            Vector3 c = U(o);
            wellR = Glow(hallRoot, "well grate", DiscMesh(), out wellT);
            wellT.position = c + new Vector3(0, 0.02f, 0);
            wellT.localScale = new Vector3(UnityTuning.Hall36WellRadius, 1, UnityTuning.Hall36WellRadius);
            lavaR = Glow(fxRoot, "lava floor", DiscMesh(), out lavaT);
            lavaT.position = c + new Vector3(0, UnityTuning.Lava36Lift, 0);
            lavaT.gameObject.SetActive(false);
            beamR = Glow(fxRoot, "reveal beam", boxMesh, out beamT);
            beamT.position = c + new Vector3(0, (float)a.p.height * 0.5f, 0);
            beamT.localScale = new Vector3(UnityTuning.Hall36BeamWidth, (float)a.p.height, UnityTuning.Hall36BeamWidth);
            campR = Glow(fxRoot, "camp decal", DiscMesh(), out campT);
            campT.gameObject.SetActive(false);
            sweepBarR = Glow(fxRoot, "sweep bar", boxMesh, out sweepBarT);
            sweepBarT.gameObject.SetActive(false);
            var lg = new GameObject("hall light");
            lg.transform.SetParent(hallRoot, false);
            lg.transform.position = c + new Vector3(0, UnityTuning.Hall36LightY, 0);
            light = lg.AddComponent<Light>();
            light.type = LightType.Point;
            light.shadows = LightShadows.None;
            light.range = UnityTuning.Hall36LightRange;
            light.intensity = UnityTuning.Hall36LightIntensity * UnityTuning.LightScale;
            light.color = Look.Hex(UnityTuning.Hall36LightHex);
            builtAt = o;
            built = true;
        }

        Mesh disc;
        Mesh DiscMesh()
        {
            if (disc != null) return disc;
            var k = new MeshKit();
            int n = UnityTuning.Lava36Segments;
            for (int i = 0; i < n; i++)
            {
                float a0 = i * Mathf.PI * 2 / n, a1 = (i + 1) * Mathf.PI * 2 / n;
                k.Triangle(Vector3.zero, new Vector3(Mathf.Cos(a1), 0, Mathf.Sin(a1)), new Vector3(Mathf.Cos(a0), 0, Mathf.Sin(a0)), Vector3.up, Color.white);
            }
            disc = k.Build("lantern36 disc");
            return disc;
        }

        static float FlashGain(double progress)
        {
            // builder P's tell flash: from FlashStart to FlashPeak over the wind-up, pulsing in its last 30 %
            float g = Mathf.Lerp(UnityTuning.Tell36FlashStart, UnityTuning.Tell36FlashPeak, (float)progress);
            if (progress > 0.7) g *= Light54.TellOn ? Light54.TellRamp(progress) : 0.75f + 0.25f * Mathf.Sin((float)(Time.time * UnityTuning.Tell36PulseHz * Mathf.PI * 2)); // NIGHT54 light54: one brighten, not the 7 Hz pulse
            return g;
        }

        // --- the frame --------------------------------------------------------------------------------------------------------------------------

        public void Render(double alpha)
        {
            SurvivalSim sim = game.sim;
            float dt = Time.deltaTime;
            t += dt;
            if (sim == null || sim.lantern36Centre == null || sim.lantern36Arena == null)
            {
                hallRoot.gameObject.SetActive(false);
                fxRoot.gameObject.SetActive(false);
                bodyRoot.gameObject.SetActive(false);
                return;
            }
            Vec3 o = sim.lantern36Centre.Value;
            if (!built || builtAt != o) BuildHall(sim.lantern36Arena, o);
            hallRoot.gameObject.SetActive(true);
            slabGo.SetActive(sim.Lantern36SlabClosed);
            Lantern36 l = sim.lantern36;
            bool fight = l != null && l.Alive;
            fxRoot.gameObject.SetActive(fight);
            bodyRoot.gameObject.SetActive(fight);
            Tint(wellR, Look.Hex(UnityTuning.Hall36WellHex, fight && l.brain.state == Boss36State.Reveal ? UnityTuning.Hall36WellGainReveal : UnityTuning.Hall36WellGainRest));
            if (!fight) { OrbsShown = 0; Pose = ""; return; }
            Boss36Brain b = l.brain;
            Boss36Attack atk = b.Current;
            double prog = b.Progress;
            Vector3 c = U(o);
            // the reveal beam
            beamT.gameObject.SetActive(b.state == Boss36State.Reveal);
            if (b.state == Boss36State.Reveal) Tint(beamR, Look.Hex(UnityTuning.Hall36BeamHex, UnityTuning.Hall36BeamGain * Mathf.Sin((float)prog * Mathf.PI)));
            // the lava floor: grows from the well out through the tell, full while live
            bool floorTell = atk != null && b.attack == Lantern36.Floor && b.state == Boss36State.Tell;
            lavaT.gameObject.SetActive(floorTell || l.floorLive);
            if (floorTell)
            {
                float r = Mathf.Lerp(UnityTuning.Hall36WellRadius, (float)l.p.floorR, (float)prog);
                lavaT.localScale = new Vector3(r, 1, r);
                Tint(lavaR, Look.Hex(UnityTuning.Tell36RedHex, Mathf.Lerp(UnityTuning.Lava36TellMin, UnityTuning.Lava36TellMax, (float)prog)));
            }
            else if (l.floorLive)
            {
                lavaT.localScale = new Vector3((float)l.p.floorR, 1, (float)l.p.floorR);
                Tint(lavaR, Look.Hex(UnityTuning.Hall36DangerHex, UnityTuning.Lava36LiveGain * (0.85f + 0.15f * Mathf.Sin(t * 6))));
            }
            // the sweep: the bar on the wall where it starts (tell), the beams while it burns
            bool sweepTell = b.attack == Lantern36.Sweep && b.state == Boss36State.Tell, sweepBurn = b.attack == Lantern36.Sweep && b.state == Boss36State.Active;
            sweepBarT.gameObject.SetActive(sweepTell || sweepBurn);
            int beams = sweepBurn ? l.Beams : 0;
            while (beamTs.Count < 2) { beamRs.Add(Glow(fxRoot, "sweep beam", boxMesh, out Transform bt)); beamTs.Add(bt); }
            float apo = (float)sim.lantern36Arena.p.apothem, lowY = (float)l.p.bandLow, highY = (float)l.p.bandHigh;
            if (sweepTell || sweepBurn)
            {
                Vector3 d = Dir(l.sweepAngle);
                sweepBarT.position = c + d * (apo - 0.1f) + new Vector3(0, (lowY + highY) * 0.5f, 0);
                sweepBarT.rotation = Quaternion.LookRotation(d, Vector3.up);
                sweepBarT.localScale = new Vector3(UnityTuning.Sweep36BarWidth, highY - lowY, 0.1f);
                Tint(sweepBarR, Look.Hex(UnityTuning.Tell36RedHex, sweepBurn ? UnityTuning.Sweep36BurnGain : UnityTuning.Sweep36TellGain * FlashGain(prog)));
            }
            Vector3 coreW = Vector3.Lerp(U(l.body.previous), U(l.body.position), (float)alpha);
            for (int k = 0; k < beamTs.Count; k++)
            {
                bool on = k < beams;
                beamTs[k].gameObject.SetActive(on);
                if (!on) continue;
                Vector3 d = Dir(l.sweepAngle + k * 2 * System.Math.PI / beams);
                Vector3 end = c + d * apo + new Vector3(0, (lowY + highY) * 0.5f, 0);
                Vector3 mid = (coreW + end) * 0.5f;
                beamTs[k].position = mid;
                beamTs[k].rotation = Quaternion.LookRotation(end - coreW, Vector3.up);
                beamTs[k].localScale = new Vector3(UnityTuning.Sweep36BeamThick, highY - lowY, (end - coreW).magnitude);
                Tint(beamRs[k], Look.Hex(UnityTuning.Tell36RedHex, UnityTuning.Sweep36BurnGain));
            }
            // the camp decal
            bool campTell = atk != null && atk.camp && b.state == Boss36State.Tell;
            campFlash = Mathf.Max(0, campFlash - dt);
            campT.gameObject.SetActive(campTell || campFlash > 0);
            if (campTell || campFlash > 0)
            {
                campT.position = U(b.campAt) + new Vector3(0, UnityTuning.Camp36Lift, 0);
                float r = (float)l.p.campRadius;
                campT.localScale = new Vector3(r, 1, r);
                Tint(campR, Look.Hex(UnityTuning.Tell36RedHex, campTell ? Mathf.Lerp(UnityTuning.Camp36TellMin, UnityTuning.Camp36TellMax, (float)prog) : UnityTuning.Camp36TellMax * 1.5f));
            }
            // the orbs
            int shown = 0;
            foreach (Lantern36Orb ob in l.orbs)
            {
                if (!ob.alive) continue;
                if (shown >= orbTs.Count && orbTs.Count < UnityTuning.Orb36Pool) { orbRs.Add(Glow(fxRoot, "orb", ballMesh, out Transform ot)); orbTs.Add(ot); }
                if (shown >= orbTs.Count) break;
                Transform tr = orbTs[shown];
                tr.gameObject.SetActive(true);
                tr.position = Vector3.Lerp(U(ob.prev), U(ob.pos), (float)alpha);
                tr.localScale = Vector3.one * (float)(l.p.orbRadius * 2);
                Tint(orbRs[shown], ob.back ? Look.Hex(0xffffff, UnityTuning.Orb36BackGain) : Look.Hex(UnityTuning.Tell36YellowHex, UnityTuning.Orb36Gain));
                shown++;
            }
            for (int k = shown; k < orbTs.Count; k++) orbTs[k].gameObject.SetActive(false);
            OrbsShown = shown;
            RenderBody(sim, l, coreW, dt);
        }

        void RenderBody(SurvivalSim sim, Lantern36 l, Vector3 coreW, float dt)
        {
            Boss36Brain b = l.brain;
            Boss36Attack atk = b.Current;
            double prog = b.Progress;
            // face the player on the ground plane
            Vector3 me = U(sim.mover.position);
            Vector3 face = me - coreW;
            face.y = 0;
            if (face.sqrMagnitude > 1e-4f) bodyRoot.rotation = Quaternion.Slerp(bodyRoot.rotation, Quaternion.LookRotation(face.normalized, Vector3.up), Mathf.Clamp01(dt * 3));
            Vector3 shake = b.state == Boss36State.Dying ? Random.insideUnitSphere * UnityTuning.Body36DieShake : Vector3.zero; // view-only jitter (never the sim's randomness)
            bodyRoot.position = coreW + shake;
            bodyRoot.localScale = Vector3.one * UnityTuning.Body36Scale;
            float br = 1 + UnityTuning.Body36BreatheAmp * Mathf.Sin(t * UnityTuning.Body36BreatheHz * Mathf.PI * 2);
            breathe.localScale = new Vector3(br, 1 / Mathf.Sqrt(br), br);
            // the pose: arm targets (shoulder pitch deg: + up; elbow deg), plate opening, tilt
            string pose = b.state == Boss36State.Reveal ? "reveal" : b.state == Boss36State.Stun ? "stun" : b.state == Boss36State.Punish ? "punish" : b.state == Boss36State.Dying ? "dying"
                : (b.state == Boss36State.Tell || b.state == Boss36State.Active) && atk != null ? (atk.camp ? "camp" : b.attack == Lantern36.Fan ? "fan" : b.attack == Lantern36.Floor ? "floor" : "sweep") : "idle";
            Pose = pose;
            float sway = UnityTuning.Body36SwayDeg * Mathf.Sin(t * UnityTuning.Body36SwayHz * Mathf.PI * 2);
            float plateT = pose == "fan" ? UnityTuning.Body36PlateFanDeg * (float)(b.state == Boss36State.Tell ? prog : 1) : pose == "punish" || pose == "stun" || l.CoreOpen ? UnityTuning.Body36PlateOpenDeg : UnityTuning.Body36PlateIdleDeg;
            float tiltT = pose == "stun" ? UnityTuning.Body36StunTiltDeg : 0;
            Vector3 sweepDir = Dir(l.sweepAngle), campDir = U(b.campAt) - coreW;
            int sweepArm = -1, campArm = -1;
            float best = -2, bestC = -2;
            for (int i = 0; i < 4 && model; i++)
            {
                Vector3 armDir = shoulder[i].parent.forward;
                float d = Vector3.Dot(armDir, sweepDir);
                if (d > best) { best = d; sweepArm = i; }
                Vector3 cd = campDir; cd.y = 0;
                float dc = cd.sqrMagnitude > 1e-4f ? Vector3.Dot(armDir, cd.normalized) : -1;
                if (dc > bestC) { bestC = dc; campArm = i; }
            }
            for (int i = 0; i < 4 && model; i++)
            {
                float pitch, bend, yawOff = 0;
                switch (pose)
                {
                    case "reveal":
                        float u = Mathf.SmoothStep(0, 1, (float)prog);
                        pitch = Mathf.Lerp(-85, -45, u); bend = Mathf.Lerp(130, 30, u); break;
                    case "fan": pitch = 35 + sway; bend = 10; break;
                    case "floor": pitch = -80; bend = 5 + 10 * Mathf.Sin(t * 8 + i); break;
                    case "sweep":
                        if (i == sweepArm) { pitch = 0; bend = 0; yawOff = Vector3.SignedAngle(shoulder[i].parent.forward, sweepDir, Vector3.up); }
                        else { pitch = -70; bend = 90; }
                        break;
                    case "camp": if (i == campArm) { pitch = 70; bend = 50; } else { pitch = -55; bend = 60; } break;
                    case "punish": pitch = -78 + sway * 0.3f; bend = 45; break;
                    case "stun": pitch = -30 + (i % 2) * 20; bend = 70; break;
                    case "dying": pitch = -90; bend = 140; break;
                    default: pitch = -45 + sway; bend = 30 + sway; break;
                }
                Spring(ref sp[i], ref spV[i], pitch, UnityTuning.Body36SpringW, dt);
                Spring(ref el[i], ref elV[i], bend, UnityTuning.Body36SpringW * 0.8f, dt); // the forearm lags the upper arm: a spring bone
                Spring(ref sy[i], ref syV[i], yawOff, UnityTuning.Body36SpringW, dt);
                shoulder[i].localRotation = Quaternion.Euler(0, sy[i], 0) * Quaternion.Euler(-sp[i], 0, 0);
                elbow[i].localRotation = Quaternion.Euler(el[i], 0, 0);
            }
            Spring(ref plate, ref plateV, plateT, UnityTuning.Body36SpringW, dt);
            Spring(ref tilt, ref tiltV, tiltT, UnityTuning.Body36SpringW * 0.6f, dt);
            breathe.localRotation = Quaternion.Euler(tilt, 0, tilt * 0.4f);
            for (int i = 0; i < 4 && model; i++) plateHinge[i].localRotation = plateBase[i] * Quaternion.Euler(-plate, 0, 0);
            // the chain swings behind the body's motion
            Vector3 vel = dt > 0 ? (coreW - lastPos) / dt : Vector3.zero;
            lastPos = coreW;
            Vector3 lagT = -vel * UnityTuning.Body36ChainLag;
            chainLag = Vector3.SmoothDamp(chainLag, lagT, ref chainLagV, 0.25f);
            for (int k = 0; k < 3 && model; k++) chain[k].localRotation = Quaternion.Euler(chainLag.z * 30 + Mathf.Sin(t * 1.3f + k) * 4, 0, -chainLag.x * 30);
            // colour: the tell's colour through the core, the claws and the rim; else the shield's red-orange or the open core's cyan
            Color tellC = default;
            bool telling = b.state == Boss36State.Tell && atk != null;
            if (telling) tellC = Look.Hex(atk.kind == Tell36Kind.Yellow ? UnityTuning.Tell36YellowHex : UnityTuning.Tell36RedHex, FlashGain(prog));
            bool open = l.CoreOpen;
            // the core is the weak point, lit in one frame: the tell's colour while telling, CYAN while stunned or open (hit it now), warm white at rest
            bool stunned = b.state == Boss36State.Stun;
            float pulse = 0.7f + 0.3f * Mathf.Sin(t * UnityTuning.Body36OpenPulseHz * Mathf.PI * 2);
            Color coreC = telling ? tellC * UnityTuning.Body36CoreTellBoost : stunned || open ? Look.Hex(UnityTuning.Body36OpenHex, UnityTuning.Body36OpenGain * pulse) : Look.Hex(UnityTuning.Body36IdleCoreHex, UnityTuning.Body36IdleCoreGain);
            stunFlash = Mathf.Max(0, stunFlash - dt);
            if (stunFlash > 0) coreC = Color.Lerp(coreC, Look.Hex(UnityTuning.Body36OpenHex, UnityTuning.Body36StunFlashGain), stunFlash / UnityTuning.Body36StunFlashS);
            hitFlash = Mathf.Max(0, hitFlash - dt);
            if (hitFlash > 0) coreC = Color.Lerp(coreC, Color.white * 2, hitFlash / 0.06f);
            Tint(coreR, coreC * 0.6f);
            Tint(coreInnerR, coreC);
            CoreColour = coreC;
            float lum = Mathf.Max(coreC.r, Mathf.Max(coreC.g, coreC.b));
            coreLight.color = lum > 0 ? coreC / lum : Color.white;
            coreLight.intensity = UnityTuning.Body36CoreLightIntensity * UnityTuning.LightScale * Mathf.Min(3, lum);
            Color rimC = telling ? tellC * 0.8f : Look.Hex(UnityTuning.Body36RimHex, UnityTuning.Body36RimGain);
            foreach (Renderer r in rimRs) Tint(r, rimC);
            for (int i = 0; i < 4 && model; i++) Tint(clawR[i], telling ? tellC : rimC);
        }
    }
}
